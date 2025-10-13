using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Business.Services;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService = new();

        [HttpPost("send-verification")]
        public IActionResult SendVerification([FromBody] VerificationRequest request)
        {
            try
            {
                _emailService.SendVerificationEmail(request.Email, request.Token);
                return Ok(new { message = $"Email sent successfully to {request.Email}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class VerificationRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
