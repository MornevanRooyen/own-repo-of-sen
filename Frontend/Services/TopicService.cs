using System.Net.Http.Json;

namespace Frontend.Services
{
    public class TopicService
    {
        private readonly HttpClient _httpClient;

        public TopicService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TopicVM>> GetTopicsBySubjectAsync(int subjectId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TopicDto>>($"api/topic/by-subject/{subjectId}");
                if (response == null) return new List<TopicVM>();

                return response.Select(t => new TopicVM
                {
                    TopicId = t.TopicId,
                    Title = t.Title,
                    OrderNumber = t.OrderNumber,
                    IsActive = t.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicService] Error getting topics for subject {subjectId}: {ex.Message}");
                return new List<TopicVM>();
            }
        }

        public async Task<TopicVM?> GetTopicByIdAsync(int topicId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TopicDto>($"api/topic/{topicId}");
                if (response == null) return null;

                return new TopicVM
                {
                    TopicId = response.TopicId,
                    Title = response.Title,
                    OrderNumber = response.OrderNumber,
                    IsActive = response.IsActive
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicService] Error getting topic {topicId}: {ex.Message}");
                return null;
            }
        }

        public async Task<TopicVM?> CreateTopicAsync(CreateTopicDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/topic", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[TopicService] Error creating topic: {errorContent}");
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<TopicDto>();
                if (result == null) return null;

                return new TopicVM
                {
                    TopicId = result.TopicId,
                    Title = result.Title,
                    OrderNumber = result.OrderNumber,
                    IsActive = result.IsActive
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicService] Error creating topic: {ex.Message}");
                return null;
            }
        }

        public async Task<TopicVM?> UpdateTopicAsync(int topicId, UpdateTopicDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/topic/{topicId}", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[TopicService] Error updating topic: {errorContent}");
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<TopicDto>();
                if (result == null) return null;

                return new TopicVM
                {
                    TopicId = result.TopicId,
                    Title = result.Title,
                    OrderNumber = result.OrderNumber,
                    IsActive = result.IsActive
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicService] Error updating topic: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteTopicAsync(int topicId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/topic/{topicId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicService] Error deleting topic: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ReorderTopicsAsync(int subjectId, List<int> topicIds)
        {
            try
            {
                var dto = new ReorderTopicsDto { TopicIds = topicIds };
                var response = await _httpClient.PutAsJsonAsync($"api/topic/reorder/{subjectId}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicService] Error reordering topics: {ex.Message}");
                return false;
            }
        }
    }

    public class TopicVM
    {
        public int TopicId { get; set; }
        public string Title { get; set; } = "";
        public int OrderNumber { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class TopicDto
    {
        public int TopicId { get; set; }
        public int SubjectId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int OrderNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TopicDb
    {
        public int TopicId { get; set; }
        public int SubjectId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int OrderNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateTopicDto
    {
        public int SubjectId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTopicDto
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class ReorderTopicsDto
    {
        public List<int> TopicIds { get; set; } = new List<int>();
    }
}
