using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("uploaded_files")]
    public class UploadedFile : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Column("storage_url")]
        public string StorageUrl { get; set; } = string.Empty;

        [Column("uploader_id")]
        public string UploaderId { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
