using AutoMapper;
using chatgptbot.Data.Database.Interface;
using chatgptbot.dto;
using chatgptbot.Entities;
using chatgptbot.Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace chatgptbot.Services
{
    public class AssistantService : IAssistantService
    {
        private readonly Task<List<AssistantDto>> _assistantList;
        private readonly IMapper _mapper;
        public AssistantService(IMapper mapper)
        {
            if (_assistantList == null)
            { 
                _mapper = mapper;
            string path = Path.Combine("Json", "Assistants.json");
            string json = File.ReadAllText(path);
            List<Assistants> data = JsonSerializer.Deserialize<List<Assistants>>(json);
            _assistantList = Task.FromResult(_mapper.Map<List<AssistantDto>>(data));
            }
        }

        public Task<List<AssistantDto>> getAllAssistantInfo()
        {
            return _assistantList;
        }
    }
}
