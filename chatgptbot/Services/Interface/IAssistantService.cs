using chatgptbot.dto;
using chatgptbot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Services.Interface
{
    public interface IAssistantService
    {
        public Task<List<AssistantDto>> getAllAssistantInfo();
    }
}
