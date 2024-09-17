using chatgptbot.Controllers.Base;
using chatgptbot.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using chatgptbot.Entities;

namespace chatgptbot.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MessageController : BaseController
    {
        private readonly IChatGPTService _chatGPTService;
        private const string LoggerScope = nameof(MessageController);
        public MessageController(IChatGPTService chatGPTService, ILogger<MessageController> logger) : base(logger)
        {
            _chatGPTService = chatGPTService;
        }

        [HttpPost("chat/send")]
        public async Task<IActionResult> SendMessage([FromBody] UserMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                return BadRequest("Message text is required.");
            }

            var result = await _chatGPTService.SendMessage(message);
            
            return Ok(result);
        }
        
    }
}
