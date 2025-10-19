using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Services
{
    public class SubjectSubscriptionService
    {
        private readonly HttpClient _httpClient;

        public SubjectSubscriptionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SubscriptionResponse> SubscribeToSubjectAsync(int userId, int subjectId)
        {
            try
            {
                var request = new SubscriptionRequest
                {
                    UserId = userId,
                    SubjectId = subjectId
                };

                var response = await _httpClient.PostAsJsonAsync("api/subjectsubscription/subscribe", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SubscriptionResponse>();
                    return result ?? new SubscriptionResponse { Success = false, Message = "Failed to parse response" };
                }
                else
                {
                    return new SubscriptionResponse 
                    { 
                        Success = false, 
                        Message = $"Error: {response.StatusCode}" 
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing to subject: {ex.Message}");
                return new SubscriptionResponse 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<SubscriptionResponse> UnsubscribeFromSubjectAsync(int userId, int subjectId)
        {
            try
            {
                var request = new SubscriptionRequest
                {
                    UserId = userId,
                    SubjectId = subjectId
                };

                var response = await _httpClient.PostAsJsonAsync("api/subjectsubscription/unsubscribe", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SubscriptionResponse>();
                    return result ?? new SubscriptionResponse { Success = false, Message = "Failed to parse response" };
                }
                else
                {
                    return new SubscriptionResponse 
                    { 
                        Success = false, 
                        Message = $"Error: {response.StatusCode}" 
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unsubscribing from subject: {ex.Message}");
                return new SubscriptionResponse 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<bool> GetSubscriptionStatusAsync(int userId, int subjectId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<SubscriptionResponse>($"api/subjectsubscription/status/{userId}/{subjectId}");
                return response?.IsSubscribed ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking subscription status: {ex.Message}");
                return false;
            }
        }

        public async Task<List<SubjectSubscriptionVM>> GetUserSubscriptionsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<SubjectSubscriptionVM>>($"api/subjectsubscription/user/{userId}");
                return response ?? new List<SubjectSubscriptionVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user subscriptions: {ex.Message}");
                return new List<SubjectSubscriptionVM>();
            }
        }

        public async Task<List<SubscriberVM>> GetSubscribersForSubjectAsync(int subjectId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<SubscriberVM>>($"api/subjectsubscription/subject/{subjectId}/subscribers");
                return response ?? new List<SubscriberVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching subscribers for subject: {ex.Message}");
                return new List<SubscriberVM>();
            }
        }
    }

    public class SubscriptionRequest
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }
    }

    public class SubscriptionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSubscribed { get; set; }
    }

    public class SubjectSubscriptionVM
    {
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public int SubjectId { get; set; }
        public DateTime? SubscribedAt { get; set; }
        public bool IsActive { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int SubjectYear { get; set; }
    }

    public class SubscriberVM
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public DateTime? SubscribedAt { get; set; }
        
        public string FullName => $"{FirstName} {LastName}";
    }
}
