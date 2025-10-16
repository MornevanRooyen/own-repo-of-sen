using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Sen381.Business.Models;
using Sen381.Data_Access;

namespace Sen381.Business.Services
{
    public class SubjectService
    {
        private readonly SupaBaseAuthService _supabase;

        public SubjectService(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        /// <summary>
        /// Gets all subjects from the database
        /// </summary>
        public async Task<List<SubjectDb>> GetAllSubjectsAsync()
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client
                .From<SubjectDb>()
                .Order("subject_code", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models.ToList();
        }

        /// <summary>
        /// Gets a subject by ID
        /// </summary>
        public async Task<SubjectDb?> GetSubjectByIdAsync(int subjectId)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client
                .From<SubjectDb>()
                .Filter("subject_id", Supabase.Postgrest.Constants.Operator.Equals, subjectId)
                .Get();

            return response.Models.FirstOrDefault();
        }

        /// <summary>
        /// Creates a new subject
        /// </summary>
        public async Task<SubjectDb> CreateSubjectAsync(SubjectDb subject)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            subject.CreatedAt = DateTime.UtcNow;
            subject.UpdatedAt = DateTime.UtcNow;

            var response = await client.From<SubjectDb>().Insert(subject);
            return response.Models.FirstOrDefault() ?? subject;
        }

        /// <summary>
        /// Updates an existing subject
        /// </summary>
        public async Task<SubjectDb> UpdateSubjectAsync(SubjectDb subject)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            subject.UpdatedAt = DateTime.UtcNow;

            var response = await client
                .From<SubjectDb>()
                .Filter("subject_id", Supabase.Postgrest.Constants.Operator.Equals, subject.SubjectId)
                .Update(subject);

            return response.Models.FirstOrDefault() ?? subject;
        }

        /// <summary>
        /// Deletes a subject by ID
        /// </summary>
        public async Task DeleteSubjectAsync(int subjectId)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            await client
                .From<SubjectDb>()
                .Filter("subject_id", Supabase.Postgrest.Constants.Operator.Equals, subjectId)
                .Delete();
        }

        /// <summary>
        /// Checks if a subject code already exists
        /// </summary>
        public async Task<bool> SubjectCodeExistsAsync(string subjectCode, int? excludeSubjectId = null)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client
                .From<SubjectDb>()
                .Filter("subject_code", Supabase.Postgrest.Constants.Operator.Equals, subjectCode)
                .Get();

            var existing = response.Models.FirstOrDefault();
            
            if (existing == null) return false;
            if (excludeSubjectId.HasValue && existing.SubjectId == excludeSubjectId.Value) return false;
            
            return true;
        }
    }
}
