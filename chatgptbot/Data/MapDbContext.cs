using AutoMapper;
using chatgptbot.Data.Database.Interface;
using chatgptbot.dto;
using chatgptbot.Entities;
using chatgptbot.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Data
{
    public class MapDbContext : DbContext, IMapDbContext
    {
        public DbSet<MessageThread> messageThread { get; set; }
        public DbSet<Assistants> Assistants { get; set; }
        private readonly IMapper _mapper;
        private readonly ILogger<MapDbContext> _logger;
        private readonly IConfiguration _configuration;
        public MapDbContext(ILogger<MapDbContext> logger, DbContextOptions<MapDbContext> opt, IMapper mapper, IConfiguration configuration) : base(opt)
        {
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }
        public TData AddEntry<TData>(TData data, bool saveType)
        {
            this.ChangeTracker.Clear();
            try
            {
                if (data == null) throw new SaveException();
                if (saveType)
                {
                    this.Entry(data).State = EntityState.Added;
                }
                else
                {
                    this.Entry(data).State = EntityState.Modified;
                }

                base.SaveChanges();


            }
            catch (Exception ex)
            {
                throw new SaveException(String.Format("{0}, Error Saving", ex.Message.ToString()));
            }
            return data;
        }

        public async Task<List<AssistantDto>> GetAssistants()
        {
           
            List<Assistants> assistantList = Assistants.Select(r => r).ToList();
            List<AssistantDto> dtos = assistantList.Select(r => {  
                AssistantDto dto = _mapper.Map<AssistantDto>(r);
                return dto;
            }).ToList();

            return await Task.FromResult(dtos);
        }

        public List<MessageThread> GetThreads()
        {
            return messageThread.Select(s => s).ToList();
        }
    }
}
