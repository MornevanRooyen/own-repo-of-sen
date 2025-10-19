using Microsoft.AspNetCore.Mvc;
using Sen381.Data_Access;
using Sen381Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public AnalyticsController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview([FromQuery] int days = 30, [FromQuery] int? subjectId = null)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

                // Get all booking sessions within the date range
                var sessionsQuery = client
                    .From<BookingSession>()
                    .Filter("session_date", Operator.GreaterThanOrEqual, startDate)
                    .Filter("status", Operator.Equals, "completed"); // Only completed sessions

                if (subjectId.HasValue)
                {
                    sessionsQuery = sessionsQuery.Filter("subject_id", Operator.Equals, subjectId.Value);
                }

                var sessions = await sessionsQuery.Get();
                var sessionsList = sessions.Models.ToList();

                // Calculate metrics
                var totalSessions = sessionsList.Count;
                var uniqueStudents = sessionsList.Select(s => s.StudentId).Distinct().Count();
                var activeTutors = sessionsList.Select(s => s.TutorId).Distinct().Count();

                // Get subscription data for context
                int totalSubscribers = 0;
                if (subjectId.HasValue)
                {
                    try
                    {
                        var subscriptions = await client
                            .From<SubjectSubscription>()
                            .Filter("subject_id", Operator.Equals, subjectId.Value)
                            .Filter("is_active", Operator.Equals, true)
                            .Get();
                        totalSubscribers = subscriptions.Models.Count;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AnalyticsController] Error getting subscription data: {ex.Message}");
                        totalSubscribers = 0;
                    }
                }

                // Calculate repeat students percentage
                var tutorStudentPairs = sessionsList
                    .GroupBy(s => new { s.TutorId, s.StudentId })
                    .Select(g => new { g.Key.TutorId, g.Key.StudentId, Count = g.Count() });

                var repeatStudents = tutorStudentPairs
                    .Where(x => x.Count >= 2)
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count();

                var repeatStudentsPct = uniqueStudents > 0 ? (100.0 * repeatStudents / uniqueStudents) : 0;

                var overview = new
                {
                    TotalSessions = totalSessions,
                    UniqueStudents = uniqueStudents,
                    ActiveTutors = activeTutors,
                    RepeatStudentsPct = Math.Round(repeatStudentsPct, 1),
                    TotalSubscribers = totalSubscribers,
                    EngagementRate = totalSubscribers > 0 ? Math.Round(100.0 * uniqueStudents / totalSubscribers, 1) : 0
                };

                return Ok(overview);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsController] Error getting overview: {ex.Message}");
                return StatusCode(500, new { error = "Failed to get analytics overview" });
            }
        }

        [HttpGet("sessions-over-time")]
        public async Task<IActionResult> GetSessionsOverTime([FromQuery] int days = 30, [FromQuery] string bucket = "daily", [FromQuery] int? subjectId = null)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

                var sessionsQuery = client
                    .From<BookingSession>()
                    .Filter("session_date", Operator.GreaterThanOrEqual, startDate)
                    .Filter("status", Operator.Equals, "completed"); // Only completed sessions

                if (subjectId.HasValue)
                {
                    sessionsQuery = sessionsQuery.Filter("subject_id", Operator.Equals, subjectId.Value);
                }

                var sessions = await sessionsQuery.Get();
                var sessionsList = sessions.Models.ToList();

                IEnumerable<dynamic> series;

                if (bucket == "weekly")
                {
                    series = sessionsList
                        .GroupBy(s => StartOfWeek(s.SessionDate))
                        .OrderBy(g => g.Key)
                        .Select(g => new { Date = g.Key, Value = g.Count() });
                }
                else
                {
                    series = sessionsList
                        .GroupBy(s => s.SessionDate.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => new { Date = g.Key, Value = g.Count() });
                }

                return Ok(series);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsController] Error getting sessions over time: {ex.Message}");
                return StatusCode(500, new { error = "Failed to get sessions over time data" });
            }
        }

        [HttpGet("subject-share")]
        public async Task<IActionResult> GetSubjectShare([FromQuery] int days = 30, [FromQuery] int? subjectId = null)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

                var sessionsQuery = client
                    .From<BookingSession>()
                    .Filter("session_date", Operator.GreaterThanOrEqual, startDate)
                    .Filter("status", Operator.Equals, "completed"); // Only completed sessions

                if (subjectId.HasValue)
                {
                    sessionsQuery = sessionsQuery.Filter("subject_id", Operator.Equals, subjectId.Value);
                }

                var sessions = await sessionsQuery.Get();
                var sessionsList = sessions.Models.ToList();

                // Get subjects to get their codes and names
                var subjects = await client.From<Subject>().Get();
                var subjectsDict = subjects.Models.ToDictionary(s => s.SubjectId, s => new { SubjectCode = s.SubjectCode, SubjectName = s.Name });

                var bySubject = sessionsList
                    .GroupBy(s => s.SubjectId)
                    .Select(g => new { SubjectId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var total = Math.Max(1, bySubject.Sum(x => x.Count));
                var palette = new[] { "#1B998B", "#2A6FFF", "#AD1F54", "#F2B705", "#4A4E69" };

                var subjectShare = bySubject.Select((x, i) =>
                {
                    var subject = subjectsDict.ContainsKey(x.SubjectId) ? subjectsDict[x.SubjectId] : new { SubjectCode = "Unknown", SubjectName = "Unknown" };
                    return new
                    {
                        Label = subject.SubjectCode,
                        Pct = Math.Round(100.0 * x.Count / total, 1),
                        Color = palette[i % palette.Length]
                    };
                }).ToList();

                return Ok(subjectShare);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsController] Error getting subject share: {ex.Message}");
                return StatusCode(500, new { error = "Failed to get subject share data" });
            }
        }

        [HttpGet("tutors-by-subject")]
        public async Task<IActionResult> GetTutorsBySubject([FromQuery] int days = 30, [FromQuery] int? subjectId = null)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

                var sessionsQuery = client
                    .From<BookingSession>()
                    .Filter("session_date", Operator.GreaterThanOrEqual, startDate)
                    .Filter("status", Operator.Equals, "completed"); // Only completed sessions

                if (subjectId.HasValue)
                {
                    sessionsQuery = sessionsQuery.Filter("subject_id", Operator.Equals, subjectId.Value);
                }

                var sessions = await sessionsQuery.Get();
                var sessionsList = sessions.Models.ToList();

                // Get subjects to get their codes
                var subjects = await client.From<Subject>().Get();
                var subjectsDict = subjects.Models.ToDictionary(s => s.SubjectId, s => s.SubjectCode);

                var data = sessionsList
                    .GroupBy(s => s.SubjectId)
                    .Select(g => new
                    {
                        SubjectId = g.Key,
                        Tutors = g.Select(x => x.TutorId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.Tutors)
                    .ToList();

                // Handle case where no data exists
                if (data.Count == 0)
                {
                    return Ok(new List<object>());
                }

                var max = Math.Max(1, data.Max(x => x.Tutors));

                var tutorsBySubject = data.Select(x =>
                {
                    var subjectCode = subjectsDict.ContainsKey(x.SubjectId) ? subjectsDict[x.SubjectId] : "Unknown";
                    return new
                    {
                        Label = subjectCode,
                        Value = x.Tutors,
                        Width = 100.0 * x.Tutors / max
                    };
                }).ToList();

                return Ok(tutorsBySubject);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsController] Error getting tutors by subject: {ex.Message}");
                Console.WriteLine($"[AnalyticsController] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Failed to get tutors by subject data" });
            }
        }

        [HttpGet("utilization")]
        public async Task<IActionResult> GetUtilization([FromQuery] int days = 30, [FromQuery] int? subjectId = null)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

                var sessionsQuery = client
                    .From<BookingSession>()
                    .Filter("session_date", Operator.GreaterThanOrEqual, startDate)
                    .Filter("status", Operator.Equals, "completed"); // Only completed sessions

                if (subjectId.HasValue)
                {
                    sessionsQuery = sessionsQuery.Filter("subject_id", Operator.Equals, subjectId.Value);
                }

                var sessions = await sessionsQuery.Get();
                var sessionsList = sessions.Models.ToList();

                // Get subjects to get their codes
                var subjects = await client.From<Subject>().Get();
                var subjectsDict = subjects.Models.ToDictionary(s => s.SubjectId, s => s.SubjectCode);

                var utilizationData = sessionsList
                    .GroupBy(s => s.SubjectId)
                    .Select(g => new { SubjectId = g.Key, Sessions = g.Count(), Tutors = g.Select(x => x.TutorId).Distinct().Count() })
                    .OrderByDescending(x => x.Sessions)
                    .ToList();

                var utilization = utilizationData.Select(x =>
                {
                    var subjectCode = subjectsDict.ContainsKey(x.SubjectId) ? subjectsDict[x.SubjectId] : "Unknown";
                    var util = x.Tutors == 0 ? 0 : (double)x.Sessions / x.Tutors;
                    return new
                    {
                        Label = subjectCode,
                        Sessions = x.Sessions,
                        Tutors = x.Tutors,
                        Util = Math.Round(util, 1)
                    };
                }).ToList();

                return Ok(utilization);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsController] Error getting utilization: {ex.Message}");
                return StatusCode(500, new { error = "Failed to get utilization data" });
            }
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjects()
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var subjects = await client.From<Subject>().Get();
                var subjectsList = subjects.Models.Select(s => new
                {
                    SubjectId = s.SubjectId,
                    SubjectCode = s.SubjectCode,
                    SubjectName = s.Name
                }).ToList();

                return Ok(subjectsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnalyticsController] Error getting subjects: {ex.Message}");
                return StatusCode(500, new { error = "Failed to get subjects" });
            }
        }

        private static DateTime StartOfWeek(DateTime dt)
        {
            var d = dt.Date;
            int diff = (7 + (int)d.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return d.AddDays(-diff);
        }
    }
}
