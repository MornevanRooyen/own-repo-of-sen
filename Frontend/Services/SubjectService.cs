using System.Net.Http.Json;

namespace Frontend.Services
{
    public class SubjectService
    {
        private readonly HttpClient _httpClient;

        public SubjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SubjectVM>> GetAllSubjectsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<SubjectDto>>("api/subject");
                if (response == null) return new List<SubjectVM>();

                return response.Select(s => new SubjectVM
                {
                    SubjectId = s.SubjectId,
                    SubjectCode = s.SubjectCode,
                    Name = s.Name,
                    Year = s.Year,
                    IsActive = s.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectService] Error getting subjects: {ex.Message}");
                return new List<SubjectVM>();
            }
        }

        public async Task<SubjectVM?> GetSubjectByIdAsync(int subjectId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<SubjectDto>($"api/subject/{subjectId}");
                if (response == null) return null;

                return new SubjectVM
                {
                    SubjectId = response.SubjectId,
                    SubjectCode = response.SubjectCode,
                    Name = response.Name,
                    Year = response.Year,
                    IsActive = response.IsActive
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectService] Error getting subject {subjectId}: {ex.Message}");
                return null;
            }
        }

        public async Task<SubjectVM?> CreateSubjectAsync(CreateSubjectDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/subject", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SubjectService] Error creating subject: {errorContent}");
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<SubjectDto>();
                if (result == null) return null;

                return new SubjectVM
                {
                    SubjectId = result.SubjectId,
                    SubjectCode = result.SubjectCode,
                    Name = result.Name,
                    Year = result.Year,
                    IsActive = result.IsActive
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectService] Error creating subject: {ex.Message}");
                return null;
            }
        }

        public async Task<SubjectVM?> UpdateSubjectAsync(int subjectId, UpdateSubjectDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/subject/{subjectId}", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SubjectService] Error updating subject: {errorContent}");
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<SubjectDto>();
                if (result == null) return null;

                return new SubjectVM
                {
                    SubjectId = result.SubjectId,
                    SubjectCode = result.SubjectCode,
                    Name = result.Name,
                    Year = result.Year,
                    IsActive = result.IsActive
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectService] Error updating subject: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteSubjectAsync(int subjectId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/subject/{subjectId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectService] Error deleting subject: {ex.Message}");
                return false;
            }
        }

        public async Task<SubjectVM?> ToggleActiveAsync(int subjectId, bool isActive)
        {
            try
            {
                var dto = new ToggleActiveDto { IsActive = isActive };
                var response = await _httpClient.PatchAsJsonAsync($"api/subject/{subjectId}/toggle-active", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SubjectService] Error toggling active status: {errorContent}");
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<SubjectDto>();
                if (result == null) return null;

                return new SubjectVM
                {
                    SubjectId = result.SubjectId,
                    SubjectCode = result.SubjectCode,
                    Name = result.Name,
                    Year = result.Year,
                    IsActive = result.IsActive
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectService] Error toggling active status: {ex.Message}");
                return null;
            }
        }
    }

    public class SubjectVM
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = "";
        public string Name { get; set; } = "";
        public int Year { get; set; }
        public bool IsActive { get; set; }
    }

    public class SubjectDto
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SubjectDb
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSubjectDto
    {
        public string SubjectCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSubjectDto
    {
        public string SubjectCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
    }

    public class ToggleActiveDto
    {
        public bool IsActive { get; set; }
    }
}
