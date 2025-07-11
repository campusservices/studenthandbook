using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Services.Interface
{
    public interface IOpenAIService
    {
        public Task StreamAssistantResponseAsync(string assistantId, string threadId, string userMessage, Func<string, Task> onMessageReceived);
        
    }
}
