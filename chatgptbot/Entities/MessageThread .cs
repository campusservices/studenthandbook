using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Entities
{
    public class MessageThread
    {
        public int Id { get; set; }                      // Auto-incremented DB ID
        public string ThreadId { get; set; }             // OpenAI thread ID
        public string AssistantId { get; set; }          // OpenAI assistant ID
        public string UserInput { get; set; }            // What the user said
        public string AssistantResponse { get; set; }    // What the assistant replied
        public DateTime CreatedAt { get; set; }          // Timestamp
    }
}
