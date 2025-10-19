using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("session_resources")]
    public class SessionResource : BaseModel
    {
        [PrimaryKey("file_id", false)]
        public int FileId { get; set; }

        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("uploaded_by")]
        public string UploaderId { get; set; } = string.Empty;

        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        // Remove file_type since it doesn't exist in your database
        // [Column("file_type")]
        // public string FileType { get; set; } = string.Empty;

        [Column("file_size")]
        public long FileSizeBytes { get; set; }

        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; }
    }
}