namespace Sen381Backend.Models
{
    public class SessionResourceResponseDto
    {
        public int FileId { get; set; }
        public int SessionId { get; set; }
        public string UploaderId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
