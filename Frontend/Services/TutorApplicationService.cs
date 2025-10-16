using System.Net.Http.Json;

namespace Frontend.Services
{
    public class TutorApplicationService
    {
        private readonly HttpClient _httpClient;

        public TutorApplicationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TutorApplicationResponse> SubmitApplicationAsync(TutorApplicationInput input)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/tutorapplication/submit", input);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TutorApplicationResponse>() ?? new TutorApplicationResponse { Success = false, Message = "Failed to parse response" };
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<TutorApplicationResponse>();
                    return errorResponse ?? new TutorApplicationResponse { Success = false, Message = "Failed to submit application" };
                }
            }
            catch (Exception ex)
            {
                return new TutorApplicationResponse { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<List<TutorApplicationVM>> GetPendingApplicationsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TutorApplicationVM>>("api/tutorapplication/pending");
                return response ?? new List<TutorApplicationVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching pending applications: {ex.Message}");
                return new List<TutorApplicationVM>();
            }
        }

        public async Task<bool> ApproveApplicationAsync(int applicationId, int adminUserId, string? notes = null)
        {
            try
            {
                var request = new { AdminUserId = adminUserId, Notes = notes };
                var response = await _httpClient.PostAsJsonAsync($"api/tutorapplication/{applicationId}/approve", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving application: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeclineApplicationAsync(int applicationId, int adminUserId, string? notes = null)
        {
            try
            {
                var request = new { AdminUserId = adminUserId, Notes = notes };
                var response = await _httpClient.PostAsJsonAsync($"api/tutorapplication/{applicationId}/decline", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error declining application: {ex.Message}");
                return false;
            }
        }

        public async Task<ApplicationStatusResponse> GetUserApplicationStatusAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApplicationStatusResponse>($"api/tutorapplication/user/{userId}/status");
                return response ?? new ApplicationStatusResponse { HasApplication = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching application status: {ex.Message}");
                return new ApplicationStatusResponse { HasApplication = false };
            }
        }
    }

    public class TutorApplicationInput
    {
        public int UserId { get; set; }
        public string? PhoneNum { get; set; }
        public string? StudentNo { get; set; }
        public string? Major { get; set; }
        public int? YearOfStudy { get; set; }
        public int? MinRequiredGrade { get; set; }
        public string? TranscriptPath { get; set; }
    }

    public class TutorApplicationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ApplicationId { get; set; }
    }

    public class TutorApplicationVM
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNum { get; set; }
        public string? StudentNo { get; set; }
        public string? Major { get; set; }
        public int? YearOfStudy { get; set; }
        public int CompletedSessions { get; set; }
        public int? MinRequiredGrade { get; set; }
        public string? ProfilePicturePath { get; set; }
        public string? TranscriptPath { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewNotes { get; set; }
    }

    public class ApplicationStatusResponse
    {
        public bool HasApplication { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
    }
}


