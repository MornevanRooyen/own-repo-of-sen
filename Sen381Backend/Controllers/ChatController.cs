using Microsoft.AspNetCore.Mvc;
using Sen381.Data_Access;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ChatController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] UserInput input)
        {
            //authenticate user and check sender/receiever roles
            // forbid if invalid, allow if correct direction

            //TODO: save all chat message to DB via (input.senderId, input.ReceiverId, input.MessageText, input.FileUrl)

            //Demo success reply
            return Ok(new { Status = "Message sent", Data = input });
        }
    }
}
