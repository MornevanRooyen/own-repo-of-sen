using Microsoft.AspNetCore.Mvc;
using Sen381Backend.Models;
using Supabase;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase

    {
        [HttpPost("uploadfile")]
        public async Task<IActionResult> UploadFile([FromForm] FileInput input, [FromServices] Supabase.Client client)

        {
            if (input.File == null)
                return BadRequest("No file provided.");

            using var ms = new MemoryStream();
            await input.File.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var bucket = client.Storage.From("User_Uploads");
            var fileName = $"{Guid.NewGuid()}_{input.File.FileName}";

            // Upload to Supabase storage
            await bucket.Upload(fileBytes, fileName);

            // Generate a signed URL valid for 5 minutes
            var signedUrl = await bucket.CreateSignedUrl(fileName, 300);

            // Get current user ID (if available)
            var userId = User.FindFirst("sub")?.Value ?? "anonymous";

            var uploadedFile = new UploadedFile
            {
                FileName = fileName,
                StorageUrl = signedUrl,
                UploaderId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Save file metadata in Supabase DB
            await client.From<UploadedFile>().Insert(uploadedFile);


            return Ok(new { SignedUrl = signedUrl });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFiles([FromServices] Supabase.Client client)
        {
            var response = await client.From<UploadedFile>().Get();
            return Ok(response.Models);
        }
    }
}
