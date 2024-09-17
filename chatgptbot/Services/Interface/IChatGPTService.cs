using chatgptbot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Services.Interface
{
    public interface IChatGPTService
    {
        public Task<string> SendToChatGPT(string content);
        public Task<String> ExtractTextFromPDF(string filePath);
        public Task<String> SendMessage(UserMessage message);
    }
}
