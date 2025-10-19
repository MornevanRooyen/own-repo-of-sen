namespace Frontend.Models
{
    public class BookingSessionDto
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
    }
}
