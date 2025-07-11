using chatgptbot.Controllers.Base;
using chatgptbot.dto;
using chatgptbot.Entities;
using chatgptbot.Services.Interface;
using chatgptbot.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace chatgptbot.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ChatGPTController : BaseController
    {
        private readonly IChatGPTService _chatGPTService;
        private readonly IOpenAIService _openAIService;
        private readonly IAssistantService _assistantService;
        private readonly IDocumentService _documentService;
        private const string LoggerScope = nameof(ChatGPTController);
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly CurrentAssistant _currentAssistant;
        public ChatGPTController(IConfiguration configuration, CurrentAssistant currentAssistant, IDocumentService documentService, IAssistantService assistantService, IChatGPTService chatGPTService, IOpenAIService openAIService, IHubContext<ChatHub> chatHub, ILogger<ChatGPTController> logger) : base(logger)
        {
            //_schedulerFactory = schedulerFactory;
            _chatGPTService = chatGPTService;
            _configuration = configuration;
            _openAIService = openAIService;
            _assistantService = assistantService;
            _documentService = documentService;
            _chatHub = chatHub;
            _currentAssistant = currentAssistant;
        }

        [HttpPost("current/assistant")]
        public async Task<IActionResult> currentAssistant([FromForm] CurrentAssistant currentAssistant)
        {
            Results<CurrentAssistant> results = new Results<CurrentAssistant>();
            _currentAssistant.AssistantId = currentAssistant.AssistantId;
            results.Data = _currentAssistant;
            results.status = 200;
            results.message = "Current Assistant adjusted Successfully";
            return Ok(await Task.FromResult(results));
        }

        [HttpGet("assistant/list")]
        public async Task<IActionResult> getAssistants(CancellationToken stoppingToken)
        {
            Results<List<AssistantDto>> results = new Results<List<AssistantDto>>();
            try
            {
                var result = await _assistantService.getAllAssistantInfo();
                results.Data = result;
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

        [HttpGet("conversation/{threadId}")]
        public async Task<IActionResult> getDiscussion(String threadId)
        {
            return Ok(await _chatGPTService.getDiscussion(threadId));
        }

        [HttpGet("upload/generate/pdf/{threadId}")]
        public async Task<IActionResult> generatePdf(string threadId)
        {
            string discussion = await _chatGPTService.getDiscussion(threadId);
            string path = await _documentService.ComposeDocument(discussion);

            // process uploaded files
            if (System.IO.File.Exists(path))
            {

                HttpContext.Response.OnCompleted(() =>
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        var dir = Path.GetDirectoryName(path);
                        if (Directory.Exists(dir))
                        {
                            Directory.Delete(dir, true); // Optional: delete containing temp folder
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                    }
                    return Task.CompletedTask;
                });


                return File(System.IO.File.OpenRead(path), "application/octet-stream", Path.GetFileName(path));
            }

            return NotFound();
        }

        [HttpPost("chat/info")]
        public async Task<IActionResult> emailDiscussion([FromForm] DiscussionDto chatinfo)
        {
            Results<String> results = new Results<String>();
            results.message = await _chatGPTService.sendMail(chatinfo.Discussion, chatinfo.Email);
            results.status = 200;
            
            return Ok(results);
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
            // 👇 Here's where you pass the callback
            await _openAIService.StreamAssistantResponseAsync(userMessage.AssistantId, userMessage.ThreadId, userMessage.Text, async (chunk) =>
            {
                // This is the callback — it's called with every message chunk from the assistant
                await _chatHub.Clients.Client(userMessage.ConnectionId).SendAsync("ReceiveAssistantMessage", chunk);
            });

            return Ok(new { success = true });

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
        /*
        [AllowAnonymous]
        [HttpGet("run")]
        public async Task<IActionResult> RunJob()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey("AssistantJob");
            await scheduler.TriggerJob(jobKey);

            return Ok("Job triggered!");
        }
        */
    }
 }

