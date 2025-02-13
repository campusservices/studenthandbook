using chatgptbot.Controllers.Base;
using chatgptbot.dto;
using chatgptbot.Entities;
using chatgptbot.Services.Interface;
using chatgptbot.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        public ChatGPTController(IConfiguration configuration, IChatGPTService chatGPTService, ILogger<ChatGPTController> logger) : base(logger)
        {
            _chatGPTService = chatGPTService;
            _configuration = configuration;
        }

        [HttpGet("thread/id")]
        public async Task<IActionResult> createThread()
        {
            Results<String> results = new Results<String>();
            try
            {
                var response = await _chatGPTService.CreateThreadAsync();

                results.Data = response;
                results.message = "Thread Created successfully";
                results.status = 200;
                return Ok(results);

            }
            catch (Exception e)
            {
                results.Data = null;
                results.message = e.Message;
                results.status = 404;
                return Ok(results);
            }
        }
        [HttpPost("addmessage")]
        public async Task<IActionResult> addMessageToThread([FromForm] UserMessage userMessage)
        {
            Results<String> results = new Results<String>();
            try
            {
                
                await _chatGPTService.AddMessageWithFileSearchAsync(userMessage.ThreadId, userMessage.Text);

                await _chatGPTService.CheckAssistantFilesAsync(userMessage.AssistantId);

                var runid = await _chatGPTService.RunAssistantAsync(userMessage.ThreadId, userMessage.AssistantId);

                bool complete = await _chatGPTService.IsRunCompletedAsync(userMessage.ThreadId, runid);
                if (complete)
                {

                    results.Data = await _chatGPTService.GetThreadMessagesAsync(userMessage.ThreadId);
                    results.message = "Messages received successfully!!";
                    results.status = 200;

                }
                return Ok(results);
            } catch (Exception ex)
            {
                results.Data = "can you rephrase the question  ?";
                results.message = ex.Message;
                results.status = 500;
                return Ok(results);
            }

        }
        [HttpGet("assistant/id")]
        public async Task<IActionResult> getAssistantId()
        {
            var res = await _chatGPTService.GetAssistantIdAsync();
            Results<String> results = new Results<String>();
            results.Data = res;
            results.message = "Assistant ID retrieved successfully";
            results.status = 200;

            return Ok(results);
            
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPDF(IFormFile file)
        {
            Results<String> results = new Results<String>();
            ChatGptHelper helper = new ChatGptHelper(_configuration);
            if (file == null || file.Length == 0 || file.ContentType != "application/pdf")
            {
                return BadRequest("Incorrect file type. Should be pdf.");
            }

            var assistantId = await _chatGPTService.CreateAssistantForFileAsync(); // Only once
            
            var fileId = await _chatGPTService.UploadFileAsync(file);
            
            var threadId = await _chatGPTService.CreateThreadWithFileAsync(fileId);
            var runId = await _chatGPTService.RunAssistantAsync(threadId, assistantId);

            results.message = "Thread Created successfully";
            results.status = 200;

            return Ok(results);


        }

        [HttpPost("list")]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                List<FilePropertiesDto> list = await _chatGPTService.ListFilesAsync();
                Results<List<FilePropertiesDto>> results = new Results<List<FilePropertiesDto>>();
                results.Data = list;
                results.status = 200;
                results.message = "File list successfully returned";
                return Ok(results);
            } catch (Exception ex)
            {
                Results<String> results = new Results<String>();
                results.Data = null;
                results.status = 404;
                results.message = ex.Message;
                return Ok(results);
            }
        }
    }
 }

