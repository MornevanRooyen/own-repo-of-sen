using Microsoft.AspNetCore.Mvc;
using Sen381.Data_Access;
using Sen381Backend.Models;
using Sen381.Business.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public CalendarController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // Get calendar events for a specific user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCalendarEvents(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[CalendarController] Fetching calendar events for user {userId}");

                var events = await client
                    .From<CalendarEvent>()
                    .Filter("user_id", Operator.Equals, userId)
                    .Order("start_time", Ordering.Ascending)
                    .Get();

                Console.WriteLine($"[CalendarController] Found {events.Models.Count()} calendar events for user {userId}");

                var result = new List<CalendarEventDto>();
                foreach (var evt in events.Models)
                {
                    Console.WriteLine($"[CalendarController] Processing event {evt.EventId}: {evt.Title} (BookingId: {evt.BookingId})");
                    var details = await GetCalendarEventDetails(evt.EventId);
                    if (details != null)
                    {
                        result.Add(details);
                    }
                }

                Console.WriteLine($"[CalendarController] Returning {result.Count} calendar events for user {userId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CalendarController] Error (GetUserCalendarEvents): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Get upcoming calendar events for a specific user
        [HttpGet("user/{userId}/upcoming")]
        public async Task<IActionResult> GetUpcomingEvents(int userId, [FromQuery] int days = 30)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var now = DateTime.UtcNow;
                var futureDate = now.AddDays(days);

                var events = await client
                    .From<CalendarEvent>()
                    .Filter("user_id", Operator.Equals, userId)
                    .Filter("start_time", Operator.GreaterThanOrEqual, now)
                    .Filter("start_time", Operator.LessThanOrEqual, futureDate)
                    .Order("start_time", Ordering.Ascending)
                    .Get();

                var result = new List<CalendarEventDto>();
                foreach (var evt in events.Models)
                {
                    var details = await GetCalendarEventDetails(evt.EventId);
                    if (details != null)
                    {
                        result.Add(details);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CalendarController] Error (GetUpcomingEvents): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Get calendar event details with user and booking information
        private async Task<CalendarEventDto?> GetCalendarEventDetails(int eventId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get calendar event
                var eventResponse = await client
                    .From<CalendarEvent>()
                    .Filter("event_id", Operator.Equals, eventId)
                    .Get();

                var evt = eventResponse.Models.FirstOrDefault();
                if (evt == null) return null;

                // Get user details
                var userResponse = await client
                    .From<User>()
                    .Select("first_name, last_name, email")
                    .Filter("user_id", Operator.Equals, evt.UserId)
                    .Get();

                var user = userResponse.Models.FirstOrDefault();

                // Get booking details if this is a booking event
                string? subjectCode = null;
                string? subjectName = null;
                string? bookingStatus = null;

                if (evt.BookingId.HasValue)
                {
                    var bookingResponse = await client
                        .From<BookingSession>()
                        .Filter("booking_id", Operator.Equals, evt.BookingId.Value)
                        .Get();

                    var booking = bookingResponse.Models.FirstOrDefault();
                    if (booking != null)
                    {
                        bookingStatus = booking.Status;

                        // Get subject details
                        var subjectResponse = await client
                            .From<Sen381Backend.Models.Subject>()
                            .Select("subject_code, name")
                            .Filter("subject_id", Operator.Equals, booking.SubjectId)
                            .Get();

                        var subject = subjectResponse.Models.FirstOrDefault();
                        if (subject != null)
                        {
                            subjectCode = subject.SubjectCode;
                            subjectName = subject.Name;
                        }
                    }
                }

                return new CalendarEventDto
                {
                    EventId = evt.EventId,
                    BookingId = evt.BookingId,
                    UserId = evt.UserId,
                    Title = evt.Title,
                    Description = evt.Description,
                    StartTime = evt.StartTime,
                    EndTime = evt.EndTime,
                    EventType = evt.EventType,
                    IsAllDay = evt.IsAllDay,
                    CreatedAt = evt.CreatedAt,
                    UpdatedAt = evt.UpdatedAt,
                    FirstName = user?.FirstName ?? "",
                    LastName = user?.LastName ?? "",
                    Email = user?.Email ?? "",
                    SubjectId = evt.BookingId.HasValue ? GetSubjectIdFromBooking(evt.BookingId.Value).Result : null,
                    BookingStatus = bookingStatus,
                    SubjectCode = subjectCode,
                    SubjectName = subjectName
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CalendarController] Error (GetCalendarEventDetails): {ex.Message}");
                return null;
            }
        }

        private async Task<int?> GetSubjectIdFromBooking(int bookingId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var bookingResponse = await client
                    .From<BookingSession>()
                    .Select("subject_id")
                    .Filter("booking_id", Operator.Equals, bookingId)
                    .Get();

                return bookingResponse.Models.FirstOrDefault()?.SubjectId;
            }
            catch
            {
                return null;
            }
        }

        // Delete a calendar event
        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteCalendarEvent(int eventId, [FromQuery] int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // First, get the event to check if it's a booking event
                var eventResponse = await client
                    .From<CalendarEvent>()
                    .Filter("event_id", Operator.Equals, eventId)
                    .Get();

                var evt = eventResponse.Models.FirstOrDefault();
                if (evt == null)
                {
                    return NotFound(new { error = "Calendar event not found" });
                }

                // If this is a booking event, check if the user is the student who created it
                if (evt.BookingId.HasValue)
                {
                    // Get the booking to check who created it
                    var bookingResponse = await client
                        .From<BookingSession>()
                        .Filter("booking_id", Operator.Equals, evt.BookingId.Value)
                        .Get();

                    var booking = bookingResponse.Models.FirstOrDefault();
                    if (booking == null)
                    {
                        return NotFound(new { error = "Booking session not found" });
                    }
                    
                    Console.WriteLine($"[CalendarController] Found booking: ID={booking.BookingId}, StudentId={booking.StudentId}, TutorId={booking.TutorId}, Title={booking.Title}");

                    // Special case: if BookingId is 0, it's an orphaned booking that anyone can delete
                    if (booking.BookingId == 0)
                    {
                        Console.WriteLine($"[CalendarController] Orphaned booking detected (BookingId=0), allowing deletion by any user");
                    }
                    else
                    {
                        // Only allow the student who created the booking to delete it
                        Console.WriteLine($"[CalendarController] Checking permissions: booking.StudentId = {booking.StudentId}, userId = {userId}");
                        if (booking.StudentId != userId)
                        {
                            Console.WriteLine($"[CalendarController] Access denied: StudentId {booking.StudentId} != userId {userId}");
                            return BadRequest(new { error = "Only the student who created the booking can delete it" });
                        }
                        Console.WriteLine($"[CalendarController] Access granted: StudentId {booking.StudentId} == userId {userId}");
                    }
                }

                // If this is a booking event, we need to delete the associated booking and all related calendar events
                if (evt.BookingId.HasValue)
                {
                    // Special case: if BookingId is 0, it's an orphaned event that should be deleted directly
                    if (evt.BookingId.Value == 0)
                    {
                        Console.WriteLine($"[CalendarController] Deleting orphaned calendar event {evt.EventId} with BookingId 0");
                        await client
                            .From<CalendarEvent>()
                            .Filter("event_id", Operator.Equals, eventId)
                            .Delete();
                        
                        return Ok(new { message = "Orphaned calendar event deleted successfully" });
                    }
                    Console.WriteLine($"[CalendarController] Deleting booking {evt.BookingId.Value} and all related calendar events");
                    
                    // Get all calendar events for this booking
                    var bookingEventsResponse = await client
                        .From<CalendarEvent>()
                        .Filter("booking_id", Operator.Equals, evt.BookingId.Value)
                        .Get();

                    Console.WriteLine($"[CalendarController] Found {bookingEventsResponse.Models.Count()} calendar events to delete");

                    // Delete all calendar events for this booking
                    foreach (var bookingEvent in bookingEventsResponse.Models)
                    {
                        try
                        {
                            await client
                                .From<CalendarEvent>()
                                .Filter("event_id", Operator.Equals, bookingEvent.EventId)
                                .Delete();
                            
                            Console.WriteLine($"[CalendarController] Successfully deleted calendar event {bookingEvent.EventId}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[CalendarController] Error deleting calendar event {bookingEvent.EventId}: {ex.Message}");
                        }
                    }

                    // Delete the booking session
                    try
                    {
                        await client
                            .From<BookingSession>()
                            .Filter("booking_id", Operator.Equals, evt.BookingId.Value)
                            .Delete();

                        Console.WriteLine($"[CalendarController] Successfully deleted booking session {evt.BookingId.Value}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CalendarController] Error deleting booking session {evt.BookingId.Value}: {ex.Message}");
                    }

                    // Verify deletion by checking if events still exist
                    var verifyResponse = await client
                        .From<CalendarEvent>()
                        .Filter("booking_id", Operator.Equals, evt.BookingId.Value)
                        .Get();

                    Console.WriteLine($"[CalendarController] After deletion, found {verifyResponse.Models.Count()} remaining calendar events for booking {evt.BookingId.Value}");

                    return Ok(new { message = "Booking session and all related calendar events deleted successfully" });
                }
                else
                {
                    // Delete just the calendar event (non-booking event)
                    await client
                        .From<CalendarEvent>()
                        .Filter("event_id", Operator.Equals, eventId)
                        .Delete();

                    return Ok(new { message = "Calendar event deleted successfully" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CalendarController] Error (DeleteCalendarEvent): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
