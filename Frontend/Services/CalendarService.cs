using System.Net.Http.Json;

namespace Frontend.Services
{
    public class CalendarService
    {
        private readonly HttpClient _httpClient;

        public CalendarService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CalendarEventVM>> GetUserCalendarEventsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<CalendarEventVM>>($"api/calendar/user/{userId}");
                return response ?? new List<CalendarEventVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching calendar events: {ex.Message}");
                return new List<CalendarEventVM>();
            }
        }

        public async Task<bool> DeleteCalendarEventAsync(int eventId, int userId)
        {
            try
            {
                Console.WriteLine($"[CalendarService] Attempting to delete calendar event {eventId} for user {userId}");
                var response = await _httpClient.DeleteAsync($"api/calendar/{eventId}?userId={userId}");
                Console.WriteLine($"[CalendarService] Delete response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[CalendarService] Delete error: {errorContent}");
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CalendarService] Error deleting calendar event: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CalendarEventVM>> GetUpcomingEventsAsync(int userId, int days = 30)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<CalendarEventVM>>($"api/calendar/user/{userId}/upcoming?days={days}");
                return response ?? new List<CalendarEventVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching upcoming events: {ex.Message}");
                return new List<CalendarEventVM>();
            }
        }
    }

    public class CalendarEventVM
    {
        public int EventId { get; set; }
        public int? BookingId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventType { get; set; } = string.Empty;
        public bool IsAllDay { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Additional fields from joined tables
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? SubjectId { get; set; }
        public string? BookingStatus { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        
        public string StartTimeFormatted => StartTime.ToString("MMM dd, yyyy HH:mm");
        public string EndTimeFormatted => EndTime.ToString("MMM dd, yyyy HH:mm");
        public string Duration => $"{EndTime.Subtract(StartTime).TotalMinutes} minutes";
        
        public string EventTypeColor => EventType.ToLower() switch
        {
            "booking" => "#3B82F6",
            "reminder" => "#F59E0B",
            "meeting" => "#10B981",
            _ => "#6B7280"
        };
    }
}
