using chatgptbot.Controllers.Base;
using chatgptbot.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ChatGPTController : BaseController
    {
        private readonly IChatGPTService _chatGPTService;
        private const string LoggerScope = nameof(ChatGPTController);

        public ChatGPTController(IChatGPTService chatGPTService, ILogger<ChatGPTController> logger) : base(logger)
        {
            _chatGPTService = chatGPTService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPDF(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Process the file (e.g., extract text)
            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Extract PDF content and send to ChatGPT (or other processing)
            var content = _chatGPTService.ExtractTextFromPDF(filePath);
            var chatGptResponse = await _chatGPTService.SendToChatGPT(content.Result);

            return Ok(chatGptResponse);
        }

    }
}
