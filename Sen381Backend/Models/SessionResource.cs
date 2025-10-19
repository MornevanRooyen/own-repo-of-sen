using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("session_resources")]
    public class SessionResource : BaseModel
    {
        [PrimaryKey("file_id")]
        public int FileId { get; set; }

        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("uploader_id")]
        public int UploaderId { get; set; }

        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Column("file_type")]
        public string FileType { get; set; } = string.Empty;

        [Column("file_size_bytes")]
        public long FileSizeBytes { get; set; }

        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [Column("uploaded_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}