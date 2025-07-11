using AutoMapper;
using chatgptbot.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace chatgptbot.Services
{
    public class AssistantInitializer : IHostedService
    {
         private readonly IServiceScopeFactory _scopeFactory;
       
        public AssistantInitializer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            
        }
        public async Task Execute(IJobExecutionContext context)

        {
            Log.Logger = new LoggerConfiguration()
               .WriteTo.File("logs/myjob.log")
              .CreateLogger();

            Log.Information("AssistantService is running...");
            Console.WriteLine("AssistantService is starting...");
            using var scope = _scopeFactory.CreateScope();
            var assistantService = scope.ServiceProvider.GetRequiredService<IAssistantService>();

            await assistantService.getAllAssistantInfo(); // Example method
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("AssistantService is starting...");
            using var scope = _scopeFactory.CreateScope();
            var assistantService = scope.ServiceProvider.GetRequiredService<IAssistantService>();

            await assistantService.getAllAssistantInfo(); // Example method
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("AssistantService is starting...");
            await Task.CompletedTask;
        }
        /*
        public override async Task StartAsync()
        {
           Console.WriteLine("AssistantService is starting...");
           using var scope = _scopeFactory.CreateScope();
           var assistantService = scope.ServiceProvider.GetRequiredService<IAssistantService>();

           await Task.FromResult(assistantService.getAllAssistantInfo()); // Example method
        }

        public override Task StopAsync()
        {
            Console.WriteLine("AssistantService is starting...");
            return Task.CompletedTask;
        }
        
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
           Console.WriteLine("AssistantService is starting...");
           using var scope = _scopeFactory.CreateScope();
           var assistantService = scope.ServiceProvider.GetRequiredService<IAssistantService>();

           await assistantService.getAllAssistantInfo(); // Example method
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
           Console.WriteLine("AssistantService is starting...");
           return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           Console.WriteLine("AssistantService is starting...");
           using var scope = _scopeFactory.CreateScope();
           var assistantService = scope.ServiceProvider.GetRequiredService<IAssistantService>();

           await assistantService.getAllAssistantInfo(); // Example method
        }*/
    }
}
