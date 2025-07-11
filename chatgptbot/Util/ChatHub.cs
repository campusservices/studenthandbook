using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Util
{
    public class ChatHub : Hub
    {
        // You can call this method from backend to send assistant output
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveAssistantMessage", message);
        }
        public Task<string> GetConnectionId()
        {
            return Task.FromResult(Context.ConnectionId);
        }
    }
}
