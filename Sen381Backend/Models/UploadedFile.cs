using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381Backend.Models
{
    [Table("uploaded_files")]
    public class UploadedFile : BaseModel
    {
        [PrimaryKey("file_id")]
        public int FileId { get; set; }

        [Column("uploader_user_id")]
        public long UploaderUserId { get; set; }

        [Column("filename")]
        public string FileName { get; set; }

        [Column("file_type")]
        public string FileType { get; set; }

        [Column("size_bytes")]
        public long SizeBytes { get; set; }

        [Column("storage_location")]
        public string StorageLocation { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // ⚠️ Optional legacy column — some rows may still have this
        [Column("file_name")]
        public string LegacyFileName { get; set; }
    }
}
