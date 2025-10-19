using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Services
{
    public class BookingSessionService
    {
        private readonly HttpClient _httpClient;

        public BookingSessionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BookingResponse> CreateBookingSessionAsync(BookingRequest request)
        {
            try
            {
                var input = new BookingSessionInput
                {
                    TutorId = request.TutorId,
                    StudentId = request.StudentId, // Include the StudentId
                    SubjectId = request.SubjectId,
                    Title = request.Title,
                    Description = request.Description,
                    SessionDate = request.GetSessionDateTime(),
                    DurationMinutes = request.Duration
                };

                var response = await _httpClient.PostAsJsonAsync("api/bookingsession/create", input);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BookingResponse>() ?? 
                           new BookingResponse { Success = false, Message = "Failed to parse response" };
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<BookingResponse>();
                    return errorResponse ?? new BookingResponse { Success = false, Message = "Failed to create booking" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating booking session: {ex.Message}");
                return new BookingResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<List<BookingSessionVM>> GetUserBookingsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<BookingSessionVM>>($"api/bookingsession/user/{userId}");
                return response ?? new List<BookingSessionVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user bookings: {ex.Message}");
                return new List<BookingSessionVM>();
            }
        }

        public async Task<BookingResponse> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var request = new UpdateStatusRequest { Status = status };
                var response = await _httpClient.PutAsJsonAsync($"api/bookingsession/{bookingId}/status", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BookingResponse>() ?? 
                           new BookingResponse { Success = false, Message = "Failed to parse response" };
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<BookingResponse>();
                    return errorResponse ?? new BookingResponse { Success = false, Message = "Failed to update booking status" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating booking status: {ex.Message}");
                return new BookingResponse { Success = false, Message = ex.Message };
            }
        }
    }

    // Frontend View Models
    public class BookingSessionVM
    {
        public int BookingId { get; set; }
        public int TutorId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime SessionDate { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Additional fields from joined tables
        public string TutorFirstName { get; set; } = string.Empty;
        public string TutorLastName { get; set; } = string.Empty;
        public string TutorEmail { get; set; } = string.Empty;
        public string StudentFirstName { get; set; } = string.Empty;
        public string StudentLastName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int SubjectYear { get; set; }

        public string TutorFullName => $"{TutorFirstName} {TutorLastName}";
        public string StudentFullName => $"{StudentFirstName} {StudentLastName}";
        public DateTime EndDate => SessionDate.AddMinutes(DurationMinutes);
        
        public string StatusColor => Status.ToLower() switch
        {
            "pending" => "#F59E0B",
            "confirmed" => "#10B981",
            "cancelled" => "#EF4444",
            "completed" => "#6B7280",
            _ => "#6B7280"
        };
    }

    public class BookingRequest
    {
        public int TutorId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? SessionDate { get; set; }
        public TimeOnly? SessionTime { get; set; }
        public int Duration { get; set; }
        
        public DateTime GetSessionDateTime()
        {
            if (SessionDate == null || SessionTime == null)
                throw new InvalidOperationException("Session date and time must be set");
            
            return SessionDate.Value.Date.Add(SessionTime.Value.ToTimeSpan());
        }
        
        public DateTime GetEndDateTime()
        {
            return GetSessionDateTime().AddMinutes(Duration);
        }
    }

    public class BookingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BookingSessionVM? Booking { get; set; }
    }

    public class BookingSessionInput
    {
        public int TutorId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
