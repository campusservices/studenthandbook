using chatgptbot.dto;
using chatgptbot.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Services.Interface
{
    public interface IChatGPTService
    {
        public Task<string> CreateThreadAsync();
        public Task AddMessageToThreadAsync(string threadId, string userMessage);
        public Task<string> UploadFileAsync(IFormFile file);
        public Task<string> CreateAssistantForFileAsync();
        public Task<string> RunAssistantAsync(string threadId, String assistantId);
        public Task<string> GetAssistantIdAsync();
        public Task<List<FilePropertiesDto>> ListFilesAsync();
        public Task<string> CreateThreadWithFileAsync(string fileId);
        public Task AddMessageToAssistantAsync(String assistantId);
        public Task<string> CreateAssistantAsync(string[] fileIds);
        public Task<bool> IsRunCompletedAsync(string threadId, string runId);
        public Task<String> GetThreadMessagesAsync(string threadId);
        public Task CheckAssistantFilesAsync(string assistantId);
        public Task AddMessageWithFileSearchAsync(string threadId, string msg);
        public Task<string> getDiscussion(string threadId);
        public Task<string> sendMail(string body, string toEmail);
    }
}
