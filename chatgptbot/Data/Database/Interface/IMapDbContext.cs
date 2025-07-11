using chatgptbot.dto;
using chatgptbot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Data.Database.Interface
{
    public interface IMapDbContext
    {
        public TData AddEntry<TData>(TData data, Boolean saveType);
        public List<MessageThread> GetThreads();
        public Task<List<AssistantDto>> GetAssistants();
    }
}
