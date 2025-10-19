using System.Net.Http.Json;

namespace Frontend.Services
{
    public class SubjectTutorService
    {
        private readonly HttpClient _httpClient;

        public SubjectTutorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SubjectTutorVM>> GetTutorsForSubjectAsync(int subjectId, int? currentUserId = null)
        {
            try
            {
                var url = $"api/subjecttutor/subject/{subjectId}";
                if (currentUserId.HasValue)
                {
                    url += $"?currentUserId={currentUserId.Value}";
                }
                
                var response = await _httpClient.GetFromJsonAsync<List<SubjectTutorVM>>(url);
                return response ?? new List<SubjectTutorVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tutors for subject: {ex.Message}");
                return new List<SubjectTutorVM>();
            }
        }

        public async Task<List<SubjectVM>> GetSubjectsForTutorAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<SubjectVM>>($"api/subjecttutor/user/{userId}");
                return response ?? new List<SubjectVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching subjects for tutor: {ex.Message}");
                return new List<SubjectVM>();
            }
        }

        public async Task<bool> CheckTutorStatusAsync(int userId, int subjectId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TutorStatusResponse>($"api/subjecttutor/check/{userId}/{subjectId}");
                return response?.IsTutor ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking tutor status: {ex.Message}");
                return false;
            }
        }
    }

    public class SubjectTutorVM
    {
        public int SubjectTutorId { get; set; }
        public int UserId { get; set; }
        public int SubjectId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int SubjectYear { get; set; }
        public bool Following { get; set; } = false; // Default to not following
        
        public string FullName => $"{FirstName} {LastName}";
        public string ProfileRoute => $"/studentprofile/{UserId}";
    }

    public class TutorStatusResponse
    {
        public bool IsTutor { get; set; }
    }
}
