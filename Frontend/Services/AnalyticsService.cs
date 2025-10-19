using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Services
{
    public class AnalyticsService
    {
        private readonly HttpClient _httpClient;

        public AnalyticsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AnalyticsOverview> GetOverviewAsync(int days = 30, int? subjectId = null)
        {
            try
            {
                var url = $"https://localhost:7228/api/analytics/overview?days={days}";
                if (subjectId.HasValue)
                {
                    url += $"&subjectId={subjectId.Value}";
                }

                var response = await _httpClient.GetFromJsonAsync<AnalyticsOverview>(url);
                return response ?? new AnalyticsOverview();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsService] Error getting overview: {ex.Message}");
                return new AnalyticsOverview();
            }
        }

        public async Task<List<SessionsOverTimeData>> GetSessionsOverTimeAsync(int days = 30, string bucket = "daily", int? subjectId = null)
        {
            try
            {
                var url = $"https://localhost:7228/api/analytics/sessions-over-time?days={days}&bucket={bucket}";
                if (subjectId.HasValue)
                {
                    url += $"&subjectId={subjectId.Value}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<SessionsOverTimeData>>(url);
                return response ?? new List<SessionsOverTimeData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsService] Error getting sessions over time: {ex.Message}");
                return new List<SessionsOverTimeData>();
            }
        }

        public async Task<List<SubjectShareData>> GetSubjectShareAsync(int days = 30, int? subjectId = null)
        {
            try
            {
                var url = $"https://localhost:7228/api/analytics/subject-share?days={days}";
                if (subjectId.HasValue)
                {
                    url += $"&subjectId={subjectId.Value}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<SubjectShareData>>(url);
                return response ?? new List<SubjectShareData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsService] Error getting subject share: {ex.Message}");
                return new List<SubjectShareData>();
            }
        }

        public async Task<List<TutorsBySubjectData>> GetTutorsBySubjectAsync(int days = 30, int? subjectId = null)
        {
            try
            {
                var url = $"https://localhost:7228/api/analytics/tutors-by-subject?days={days}";
                if (subjectId.HasValue)
                {
                    url += $"&subjectId={subjectId.Value}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<TutorsBySubjectData>>(url);
                return response ?? new List<TutorsBySubjectData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsService] Error getting tutors by subject: {ex.Message}");
                return new List<TutorsBySubjectData>();
            }
        }

        public async Task<List<UtilizationData>> GetUtilizationAsync(int days = 30, int? subjectId = null)
        {
            try
            {
                var url = $"https://localhost:7228/api/analytics/utilization?days={days}";
                if (subjectId.HasValue)
                {
                    url += $"&subjectId={subjectId.Value}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<UtilizationData>>(url);
                return response ?? new List<UtilizationData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsService] Error getting utilization: {ex.Message}");
                return new List<UtilizationData>();
            }
        }

        public async Task<List<SubjectData>> GetSubjectsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<SubjectData>>("https://localhost:7228/api/analytics/subjects");
                return response ?? new List<SubjectData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsService] Error getting subjects: {ex.Message}");
                return new List<SubjectData>();
            }
        }
    }

    // Data models for analytics
           public class AnalyticsOverview
           {
               public int TotalSessions { get; set; }
               public int UniqueStudents { get; set; }
               public int ActiveTutors { get; set; }
               public double RepeatStudentsPct { get; set; }
               public int TotalSubscribers { get; set; }
               public double EngagementRate { get; set; }
           }

    public class SessionsOverTimeData
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
    }

    public class SubjectShareData
    {
        public string Label { get; set; } = string.Empty;
        public double Pct { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class TutorsBySubjectData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public double Width { get; set; }
    }

    public class UtilizationData
    {
        public string Label { get; set; } = string.Empty;
        public int Sessions { get; set; }
        public int Tutors { get; set; }
        public double Util { get; set; }
    }

    public class SubjectData
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
    }
}
