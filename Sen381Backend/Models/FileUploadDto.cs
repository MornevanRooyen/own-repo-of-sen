using Microsoft.AspNetCore.Http;

namespace Sen381Backend.Models
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; } = null!;
        public string UploaderId { get; set; } = string.Empty;
    }
}
