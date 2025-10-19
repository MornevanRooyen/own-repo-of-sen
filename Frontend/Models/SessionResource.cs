namespace Frontend.Models
{
    public class SessionResource
    {
        public int FileId { get; set; }
        public int SessionId { get; set; }
        public int UploaderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
