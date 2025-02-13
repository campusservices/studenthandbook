using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Entities
{
    public class UserMessage
    {
        public string Text { get; set; }
        public string ThreadId { get; set; }
        public string AssistantId { get; set; }
    }
}
