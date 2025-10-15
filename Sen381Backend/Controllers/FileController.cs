using Microsoft.AspNetCore.Mvc;
using Sen381Backend.Models;
using Sen381.Data_Access;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public FileController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        [HttpPost("uploadfile")]
        public async Task<IActionResult> UploadFile([FromForm] FileInput input)
        {
            if (input.File == null)
                return BadRequest("No file provided.");

            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            using var ms = new MemoryStream();
            await input.File.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            try
            {
                var bucket = client.Storage.From("User_Uploads");
                var originalName = string.IsNullOrWhiteSpace(input.Name) ? input.File.FileName : input.Name;
                var fileName = $"{Guid.NewGuid()}_{originalName}";

                // Upload to Supabase storage
                await bucket.Upload(fileBytes, fileName);

                // Generate a signed URL valid for 5 minutes
                var signedUrl = await bucket.CreateSignedUrl(fileName, 300);

                // Save metadata in DB if desired
                var uploadedFile = new UploadedFile
                {
                    FileName = fileName,
                    StorageUrl = signedUrl,
                    UploaderId = User?.FindFirst("sub")?.Value ?? "anonymous",
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    await client.From<UploadedFile>().Insert(uploadedFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FileController] DB insert warning: {ex.Message}");
                }

                Console.WriteLine($"✅ Uploaded file '{fileName}' successfully.");
                // 🔥 lowercase key name to match frontend JSON parser
                return Ok(new { signedUrl = signedUrl, fileName = fileName });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FileController] Upload error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFiles()
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client.From<UploadedFile>().Get();
            return Ok(response.Models);
        }
    }
}
