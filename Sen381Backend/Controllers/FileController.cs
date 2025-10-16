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

                // Generate a signed URL valid for 24 hours (86400 seconds)
                var signedUrl = await bucket.CreateSignedUrl(fileName, 86400);

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

        [HttpPost("upload-transcript")]
        public async Task<IActionResult> UploadTranscript([FromForm] FileInput input)
        {
            if (input.File == null)
                return BadRequest("No file provided.");

            // Validate file type
            var allowedTypes = new[] { "application/pdf" };
            var allowedExtensions = new[] { ".pdf" };
            
            if (!allowedTypes.Contains(input.File.ContentType) && 
                !allowedExtensions.Any(ext => input.File.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Only PDF files are allowed for transcripts.");
            }

            // Validate file size (10 MB limit)
            if (input.File.Length > 10 * 1024 * 1024)
            {
                return BadRequest("File size cannot exceed 10 MB.");
            }

            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            using var ms = new MemoryStream();
            await input.File.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            try
            {
                var bucket = client.Storage.From("Transcripts");
                var originalName = string.IsNullOrWhiteSpace(input.Name) ? input.File.FileName : input.Name;
                var fileName = $"transcript_{Guid.NewGuid()}_{originalName}";

                // Upload to Supabase storage
                await bucket.Upload(fileBytes, fileName);

                // Generate a signed URL valid for 1 hour (longer for transcripts)
                var signedUrl = await bucket.CreateSignedUrl(fileName, 3600);

                // Skip file metadata storage for now - just upload the file
                // TODO: Uncomment below if you create the uploaded_files table
                /*
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
                */

                Console.WriteLine($"✅ Uploaded transcript '{fileName}' successfully.");
                // 🔥 lowercase key name to match frontend JSON parser
                return Ok(new { signedUrl = signedUrl, fileName = fileName, filePath = fileName });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FileController] Transcript upload error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("get-transcript-url")]
        public async Task<IActionResult> GetTranscriptUrl([FromQuery] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest("File path is required.");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var bucket = client.Storage.From("Transcripts");
                
                // Generate a signed URL valid for 1 hour
                var signedUrl = await bucket.CreateSignedUrl(filePath, 3600);

                Console.WriteLine($"✅ Generated signed URL for transcript: {filePath}");
                return Ok(new { signedUrl = signedUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FileController] Error generating transcript URL: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("view-transcript")]
        public async Task<IActionResult> ViewTranscript([FromQuery] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest("File path is required.");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var bucket = client.Storage.From("Transcripts");
                
                // Download the file content - explicitly use the simple overload
                var fileBytes = await bucket.Download(filePath, (EventHandler<float>?)null);

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    Console.WriteLine($"❌ [FileController] No file content returned for: {filePath}");
                    return NotFound("File not found or empty");
                }

                Console.WriteLine($"✅ [FileController] Successfully retrieved {fileBytes.Length} bytes for: {filePath}");

                // Create a FileContentResult with inline disposition
                var result = new FileContentResult(fileBytes, "application/pdf")
                {
                    FileDownloadName = null // This prevents download
                };
                
                // Set headers to force inline viewing
                Response.Headers.Add("Content-Disposition", "inline; filename=\"" + Path.GetFileName(filePath) + "\"");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FileController] Error viewing transcript: {ex.Message}");
                Console.WriteLine($"❌ [FileController] Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Error retrieving file: {ex.Message}");
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
