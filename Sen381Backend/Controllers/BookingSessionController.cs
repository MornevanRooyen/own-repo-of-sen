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
    public class BookingSessionController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public BookingSessionController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // Create a new booking session
        [HttpPost("create")]
        public async Task<IActionResult> CreateBookingSession([FromBody] BookingSessionInput input)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Validate that the session date is in the future
                if (input.SessionDate <= DateTime.UtcNow)
                {
                    return BadRequest(new BookingResponse 
                    { 
                        Success = false, 
                        Message = "Session date must be in the future" 
                    });
                }

                // Create the booking session using the insert model (no BookingId property)
                var bookingSessionInsert = new BookingSessionInsert
                {
                    TutorId = input.TutorId,
                    StudentId = input.StudentId, // Use the StudentId from the input
                    SubjectId = input.SubjectId,
                    Title = input.Title,
                    Description = input.Description,
                    SessionDate = input.SessionDate,
                    DurationMinutes = input.DurationMinutes,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"[BookingSessionController] Inserting booking session with StudentId: {bookingSessionInsert.StudentId}, TutorId: {bookingSessionInsert.TutorId}");
                
                // Use the insert model that doesn't include BookingId
                var response = await client.From<BookingSessionInsert>().Insert(bookingSessionInsert);
                var createdBookingInsert = response.Models.FirstOrDefault();

                if (createdBookingInsert == null)
                {
                    Console.WriteLine($"[BookingSessionController] Failed to create booking session - no model returned");
                    return StatusCode(500, new BookingResponse 
                    { 
                        Success = false, 
                        Message = "Failed to create booking session" 
                    });
                }

                // Now fetch the created booking with the auto-generated ID
                var createdBooking = await client
                    .From<BookingSession>()
                    .Select("*")
                    .Filter("tutor_id", Operator.Equals, createdBookingInsert.TutorId)
                    .Filter("student_id", Operator.Equals, createdBookingInsert.StudentId)
                    .Filter("title", Operator.Equals, createdBookingInsert.Title)
                    .Order("created_at", Ordering.Descending)
                    .Limit(1)
                    .Get();

                var finalBooking = createdBooking.Models.FirstOrDefault();
                if (finalBooking == null)
                {
                    Console.WriteLine($"[BookingSessionController] Failed to retrieve created booking");
                    return StatusCode(500, new BookingResponse 
                    { 
                        Success = false, 
                        Message = "Failed to retrieve created booking" 
                    });
                }

                Console.WriteLine($"[BookingSessionController] Successfully created booking session with ID: {finalBooking.BookingId}");

                // Create calendar events for both tutor and student
                await CreateCalendarEvents(finalBooking);

                // Get the booking details with user and subject information
                var bookingDetails = await GetBookingDetails(finalBooking.BookingId);

                return Ok(new BookingResponse 
                { 
                    Success = true, 
                    Message = "Booking session created successfully and calendar events added",
                    Booking = bookingDetails
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error (CreateBookingSession): {ex.Message}");
                return StatusCode(500, new BookingResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        // Get booking details with user and subject information
        private async Task<BookingSessionDto?> GetBookingDetails(int bookingId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[BookingSessionController] GetBookingDetails: Looking for bookingId {bookingId}");

                // Get booking session
                var bookingResponse = await client
                    .From<BookingSession>()
                    .Filter("booking_id", Operator.Equals, bookingId)
                    .Get();

                Console.WriteLine($"[BookingSessionController] GetBookingDetails: Found {bookingResponse.Models.Count} booking sessions");

                var booking = bookingResponse.Models.FirstOrDefault();
                if (booking == null) 
                {
                    Console.WriteLine($"[BookingSessionController] GetBookingDetails: No booking found for ID {bookingId}");
                    return null;
                }

                Console.WriteLine($"[BookingSessionController] GetBookingDetails: Found booking - TutorId: {booking.TutorId}, StudentId: {booking.StudentId}, Title: {booking.Title}");

                // Get tutor details
                var tutorResponse = await client
                    .From<User>()
                    .Select("first_name, last_name, email")
                    .Filter("user_id", Operator.Equals, booking.TutorId)
                    .Get();

                var tutor = tutorResponse.Models.FirstOrDefault();

                // Get student details
                var studentResponse = await client
                    .From<User>()
                    .Select("first_name, last_name, email")
                    .Filter("user_id", Operator.Equals, booking.StudentId)
                    .Get();

                var student = studentResponse.Models.FirstOrDefault();

                // Get subject details
                var subjectResponse = await client
                    .From<Sen381Backend.Models.Subject>()
                    .Select("subject_code, name, year")
                    .Filter("subject_id", Operator.Equals, booking.SubjectId)
                    .Get();

                var subject = subjectResponse.Models.FirstOrDefault();

                return new BookingSessionDto
                {
                    BookingId = booking.BookingId,
                    TutorId = booking.TutorId,
                    StudentId = booking.StudentId,
                    SubjectId = booking.SubjectId,
                    Title = booking.Title,
                    Description = booking.Description,
                    SessionDate = booking.SessionDate,
                    DurationMinutes = booking.DurationMinutes,
                    Status = booking.Status,
                    CreatedAt = booking.CreatedAt,
                    UpdatedAt = booking.UpdatedAt,
                    CompletedAt = booking.CompletedAt,
                    TutorFirstName = tutor?.FirstName ?? "",
                    TutorLastName = tutor?.LastName ?? "",
                    TutorEmail = tutor?.Email ?? "",
                    StudentFirstName = student?.FirstName ?? "",
                    StudentLastName = student?.LastName ?? "",
                    StudentEmail = student?.Email ?? "",
                    SubjectCode = subject?.SubjectCode ?? "",
                    SubjectName = subject?.Name ?? "",
                    SubjectYear = subject?.Year ?? 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error (GetBookingDetails): {ex.Message}");
                return null;
            }
        }

        // Debug endpoint to manually complete a session
        [HttpPut("debug/complete-session/{bookingId}")]
        public async Task<IActionResult> CompleteSessionDebug(int bookingId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[BookingSessionController] DEBUG: Manually completing session {bookingId}");

                // Find the booking session
                var bookingResponse = await client
                    .From<BookingSession>()
                    .Filter("booking_id", Operator.Equals, bookingId)
                    .Get();

                var booking = bookingResponse.Models.FirstOrDefault();
                if (booking == null)
                {
                    return NotFound(new { message = "Booking session not found" });
                }

                Console.WriteLine($"[BookingSessionController] DEBUG: Found booking {bookingId} with status '{booking.Status}'");

                // Update status to completed with completion timestamp
                booking.Status = "completed";
                booking.UpdatedAt = DateTime.UtcNow;
                booking.CompletedAt = DateTime.UtcNow;

                var updateResponse = await client
                    .From<BookingSession>()
                    .Filter("booking_id", Operator.Equals, bookingId)
                    .Update(booking);

                Console.WriteLine($"[BookingSessionController] DEBUG: Session {bookingId} completed successfully");

                return Ok(new { 
                    message = "Session completed successfully",
                    bookingId = bookingId,
                    status = "completed",
                    completedAt = booking.CompletedAt
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] DEBUG Error completing session: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Debug endpoint to check if there are any sessions at all
        [HttpGet("debug/all-sessions")]
        public async Task<IActionResult> GetAllSessionsDebug()
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var allSessions = await client
                    .From<BookingSession>()
                    .Get();

                Console.WriteLine($"[BookingSessionController] DEBUG: Found {allSessions.Models.Count} total sessions in database");
                
                foreach (var session in allSessions.Models)
                {
                    Console.WriteLine($"[BookingSessionController] DEBUG: Session {session.BookingId}: Status='{session.Status}', Title='{session.Title}', StudentId={session.StudentId}, TutorId={session.TutorId}");
                }

                return Ok(new { 
                    totalSessions = allSessions.Models.Count,
                    sessions = allSessions.Models.Select(s => new {
                        bookingId = s.BookingId,
                        status = s.Status,
                        title = s.Title,
                        studentId = s.StudentId,
                        tutorId = s.TutorId,
                        completedAt = s.CompletedAt
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] DEBUG Error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Get bookings for a specific user (as tutor or student)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBookings(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[BookingSessionController] GetUserBookings: Getting bookings for user {userId}");

                // Get bookings where user is either tutor or student
                var tutorBookings = await client
                    .From<BookingSession>()
                    .Filter("tutor_id", Operator.Equals, userId)
                    .Get();

                var studentBookings = await client
                    .From<BookingSession>()
                    .Filter("student_id", Operator.Equals, userId)
                    .Get();

                Console.WriteLine($"[BookingSessionController] GetUserBookings: Found {tutorBookings.Models.Count} tutor bookings, {studentBookings.Models.Count} student bookings");

                var allBookings = tutorBookings.Models.Concat(studentBookings.Models)
                    .OrderByDescending(b => b.SessionDate)
                    .ToList();

                Console.WriteLine($"[BookingSessionController] GetUserBookings: Total bookings: {allBookings.Count}");

                var result = new List<BookingSessionDto>();
                foreach (var booking in allBookings)
                {
                    Console.WriteLine($"[BookingSessionController] GetUserBookings: Processing booking {booking.BookingId} with status '{booking.Status}'");
                    var details = await GetBookingDetails(booking.BookingId);
                    if (details != null)
                    {
                        result.Add(details);
                        Console.WriteLine($"[BookingSessionController] GetUserBookings: Added booking {booking.BookingId} to result");
                    }
                }

                Console.WriteLine($"[BookingSessionController] GetUserBookings: Returning {result.Count} bookings");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error (GetUserBookings): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Update booking status
        [HttpPut("{bookingId}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var validStatuses = new[] { "pending", "confirmed", "cancelled", "completed" };
                if (!validStatuses.Contains(request.Status))
                {
                    return BadRequest(new BookingResponse 
                    { 
                        Success = false, 
                        Message = "Invalid status. Must be one of: pending, confirmed, cancelled, completed" 
                    });
                }

                await client
                    .From<BookingSession>()
                    .Set(x => x.Status, request.Status)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow)
                    .Filter("booking_id", Operator.Equals, bookingId)
                    .Update();

                var updatedBooking = await GetBookingDetails(bookingId);

                return Ok(new BookingResponse 
                { 
                    Success = true, 
                    Message = "Booking status updated successfully",
                    Booking = updatedBooking
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error (UpdateBookingStatus): {ex.Message}");
                return StatusCode(500, new BookingResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        // Create calendar events for both tutor and student
        private async Task CreateCalendarEvents(BookingSession booking)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[BookingSessionController] Creating calendar events for booking {booking.BookingId}");

                var endTime = booking.SessionDate.AddMinutes(booking.DurationMinutes);

                // Create calendar event for tutor (don't set EventId - let database auto-increment)
                var tutorEvent = new CalendarEvent
                {
                    BookingId = booking.BookingId,
                    UserId = booking.TutorId,
                    Title = $"Tutoring Session: {booking.Title}",
                    Description = $"Tutoring session with student. Subject: {booking.SubjectId}\n\n{booking.Description}",
                    StartTime = booking.SessionDate,
                    EndTime = endTime,
                    EventType = "booking",
                    IsAllDay = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"[BookingSessionController] Creating tutor event with BookingId: {tutorEvent.BookingId}");

                // Create calendar event for student (don't set EventId - let database auto-increment)
                var studentEvent = new CalendarEvent
                {
                    BookingId = booking.BookingId,
                    UserId = booking.StudentId,
                    Title = $"Tutoring Session: {booking.Title}",
                    Description = $"Tutoring session with tutor. Subject: {booking.SubjectId}\n\n{booking.Description}",
                    StartTime = booking.SessionDate,
                    EndTime = endTime,
                    EventType = "booking",
                    IsAllDay = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"[BookingSessionController] Creating student event with BookingId: {studentEvent.BookingId}");

                // Insert tutor event
                var tutorResponse = await client.From<CalendarEvent>().Insert(tutorEvent);
                if (tutorResponse.Models?.Any() == true)
                {
                    var tutorEvt = tutorResponse.Models.First();
                    Console.WriteLine($"[BookingSessionController] Created tutor calendar event {tutorEvt.EventId} for user {tutorEvt.UserId}");
                }
                else
                {
                    Console.WriteLine($"[BookingSessionController] Warning: Failed to create tutor calendar event");
                }

                // Insert student event
                var studentResponse = await client.From<CalendarEvent>().Insert(studentEvent);
                if (studentResponse.Models?.Any() == true)
                {
                    var studentEvt = studentResponse.Models.First();
                    Console.WriteLine($"[BookingSessionController] Created student calendar event {studentEvt.EventId} for user {studentEvt.UserId}");
                }
                else
                {
                    Console.WriteLine($"[BookingSessionController] Warning: Failed to create student calendar event");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error (CreateCalendarEvents): {ex.Message}");
                Console.WriteLine($"[BookingSessionController] Stack trace: {ex.StackTrace}");
                // Don't throw - calendar events are not critical for booking creation
            }
        }

        // Debug endpoint to create a test booking session for both users
        [HttpPost("debug/create-test-session")]
        public async Task<IActionResult> CreateTestSessionForBothUsers()
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Create a test booking session that both users can join
                var testBooking = new BookingSessionInsert
                {
                    TutorId = 1, // First user as tutor
                    StudentId = 2, // Second user as student
                    SubjectId = 1,
                    Title = "Test Session - Both Users",
                    Description = "Test session for WebRTC functionality",
                    SessionDate = DateTime.UtcNow.AddMinutes(5), // 5 minutes from now
                    DurationMinutes = 60,
                    Status = "confirmed", // Set as confirmed so both can join
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var response = await client.From<BookingSessionInsert>().Insert(testBooking);
                var createdBookingInsert = response.Models.FirstOrDefault();

                if (createdBookingInsert == null)
                {
                    return StatusCode(500, new { error = "Failed to create test booking" });
                }

                // Now fetch the created booking with the auto-generated ID
                var createdBooking = await client
                    .From<BookingSession>()
                    .Select("*")
                    .Filter("tutor_id", Operator.Equals, testBooking.TutorId)
                    .Filter("student_id", Operator.Equals, testBooking.StudentId)
                    .Filter("title", Operator.Equals, testBooking.Title)
                    .Order("created_at", Ordering.Descending)
                    .Limit(1)
                    .Get();

                var finalBooking = createdBooking.Models.FirstOrDefault();
                if (finalBooking == null)
                {
                    return StatusCode(500, new { error = "Failed to retrieve created booking" });
                }

                Console.WriteLine($"[BookingSessionController] Created test session with ID: {finalBooking.BookingId}");

                return Ok(new { 
                    message = "Test session created successfully!",
                    sessionId = finalBooking.BookingId,
                    sessionUrl = $"/tutoring-session/{finalBooking.BookingId}",
                    instructions = new {
                        user1 = "User 1 (Tutor) - Navigate to the session URL",
                        user2 = "User 2 (Student) - Navigate to the session URL",
                        note = "Both users can join the same session URL"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error creating test session: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Debug endpoint to create a test booking session
        [HttpPost("debug/create-test")]
        public async Task<IActionResult> CreateTestBookingSession()
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Create a test booking session
                var testBooking = new BookingSessionInsert
                {
                    TutorId = 1,
                    StudentId = 2,
                    SubjectId = 1,
                    Title = "Test Tutoring Session",
                    Description = "This is a test session for debugging purposes",
                    SessionDate = DateTime.UtcNow.AddHours(1), // 1 hour from now
                    DurationMinutes = 60,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var response = await client.From<BookingSessionInsert>().Insert(testBooking);
                var createdBookingInsert = response.Models.FirstOrDefault();

                if (createdBookingInsert == null)
                {
                    return StatusCode(500, new { error = "Failed to create test booking" });
                }

                // Now fetch the created booking with the auto-generated ID
                var createdBooking = await client
                    .From<BookingSession>()
                    .Select("*")
                    .Filter("tutor_id", Operator.Equals, testBooking.TutorId)
                    .Filter("student_id", Operator.Equals, testBooking.StudentId)
                    .Filter("title", Operator.Equals, testBooking.Title)
                    .Order("created_at", Ordering.Descending)
                    .Limit(1)
                    .Get();

                var finalBooking = createdBooking.Models.FirstOrDefault();
                if (finalBooking == null)
                {
                    return StatusCode(500, new { error = "Failed to retrieve created booking" });
                }

                Console.WriteLine($"[BookingSessionController] Created test booking session with ID: {finalBooking.BookingId}");

                // Get the full booking details
                var bookingDetails = await GetBookingDetails(finalBooking.BookingId);

                return Ok(new { 
                    message = "Test booking session created successfully",
                    bookingId = finalBooking.BookingId,
                    bookingDetails = bookingDetails
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error creating test booking session: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Debug endpoint to check database connection and table structure
        [HttpGet("debug/check-db")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[BookingSessionController] CheckDatabase: Testing database connection");

                // Try to query the booking_sessions table
                var allBookings = await client
                    .From<BookingSession>()
                    .Get();

                Console.WriteLine($"[BookingSessionController] CheckDatabase: Successfully connected to booking_sessions table");
                Console.WriteLine($"[BookingSessionController] CheckDatabase: Found {allBookings.Models.Count} total booking sessions");
                
                foreach (var booking in allBookings.Models)
                {
                    Console.WriteLine($"[BookingSessionController] Booking ID: {booking.BookingId}, Title: {booking.Title}, TutorId: {booking.TutorId}, StudentId: {booking.StudentId}");
                }

                return Ok(new { 
                    success = true,
                    message = "Database connection successful",
                    tableExists = true,
                    totalBookings = allBookings.Models.Count,
                    bookings = allBookings.Models
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] CheckDatabase: Error - {ex.Message}");
                Console.WriteLine($"[BookingSessionController] CheckDatabase: Stack trace - {ex.StackTrace}");
                return Ok(new { 
                    success = false,
                    message = $"Database error: {ex.Message}",
                    tableExists = false,
                    error = ex.ToString()
                });
            }
        }

        // Debug endpoint to list all booking sessions
        [HttpGet("debug/all")]
        public async Task<IActionResult> GetAllBookingSessions()
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var allBookings = await client
                    .From<BookingSession>()
                    .Get();

                Console.WriteLine($"[BookingSessionController] GetAllBookingSessions: Found {allBookings.Models.Count} total booking sessions");
                
                foreach (var booking in allBookings.Models)
                {
                    Console.WriteLine($"[BookingSessionController] Booking ID: {booking.BookingId}, Title: {booking.Title}, TutorId: {booking.TutorId}, StudentId: {booking.StudentId}");
                }

                return Ok(allBookings.Models);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error getting all booking sessions: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Get session details by ID
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingSession(int bookingId)
        {
            try
            {
                Console.WriteLine($"[BookingSessionController] GetBookingSession: Received request for bookingId {bookingId}");
                
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get the booking session with user and subject details
                var bookingDetails = await GetBookingDetails(bookingId);
                
                if (bookingDetails == null)
                {
                    Console.WriteLine($"[BookingSessionController] GetBookingSession: Booking {bookingId} not found, returning 404");
                    return NotFound(new BookingResponse 
                    { 
                        Success = false, 
                        Message = "Booking session not found" 
                    });
                }

                Console.WriteLine($"[BookingSessionController] GetBookingSession: Successfully retrieved booking {bookingId}");
                return Ok(bookingDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error getting booking session: {ex.Message}");
                Console.WriteLine($"[BookingSessionController] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new BookingResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        // Helper method to get current user ID - this should be implemented based on your auth system
        private int GetCurrentUserId()
        {
            // TODO: Implement based on your authentication system
        // For now, returning a placeholder - this should be extracted from JWT token or session
        return 1; // Replace with actual current user ID
    }

    private async Task UpdateCalendarEventsForCompletedSession(Supabase.Client client, int bookingId)
    {
        try
        {
            Console.WriteLine($"[BookingSessionController] Updating calendar events for completed session {bookingId}");
            
            // Find all calendar events for this booking
            var calendarEventsResponse = await client
                .From<CalendarEvent>()
                .Filter("booking_id", Operator.Equals, bookingId)
                .Get();

            var calendarEvents = calendarEventsResponse.Models.ToList();
            Console.WriteLine($"[BookingSessionController] Found {calendarEvents.Count} calendar events for booking {bookingId}");

            // Update each calendar event to show completed status
            foreach (var calendarEvent in calendarEvents)
            {
                calendarEvent.Title = $"✅ {calendarEvent.Title}"; // Add checkmark to indicate completed
                calendarEvent.UpdatedAt = DateTime.UtcNow;

                await client
                    .From<CalendarEvent>()
                    .Filter("event_id", Operator.Equals, calendarEvent.EventId)
                    .Update(calendarEvent);

                Console.WriteLine($"[BookingSessionController] Updated calendar event {calendarEvent.EventId} for completed session");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BookingSessionController] Error updating calendar events for completed session: {ex.Message}");
        }
    }

        // Complete a tutoring session
        [HttpPut("{bookingId}/complete")]
        public async Task<IActionResult> CompleteSession(int bookingId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Find the booking session
                var bookingResponse = await client
                    .From<BookingSession>()
                    .Select("*")
                    .Filter("booking_id", Operator.Equals, bookingId)
                    .Get();

                var booking = bookingResponse.Models.FirstOrDefault();
                if (booking == null)
                {
                    return NotFound(new BookingResponse { Success = false, Message = "Booking session not found" });
                }

                // Update status to completed with completion timestamp
                booking.Status = "completed";
                booking.UpdatedAt = DateTime.UtcNow;
                booking.CompletedAt = DateTime.UtcNow; // Add completion timestamp

                var updateResponse = await client
                    .From<BookingSession>()
                    .Filter("booking_id", Operator.Equals, bookingId)
                    .Update(booking);

                Console.WriteLine($"[BookingSessionController] Session {bookingId} completed successfully for Student {booking.StudentId} and Tutor {booking.TutorId}");

                // Update calendar events to reflect completed status
                await UpdateCalendarEventsForCompletedSession(client, bookingId);

                return Ok(new BookingResponse 
                { 
                    Success = true, 
                    Message = "Session completed successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BookingSessionController] Error completing session: {ex.Message}");
                return StatusCode(500, new BookingResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
