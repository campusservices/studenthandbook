using chatgptbot.Data;
using chatgptbot.Data.Database.Interface;
using chatgptbot.Entities;
using chatgptbot.Services;
using chatgptbot.Services.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddSingleton<IOpenAIService, OpenAIService>();
                    services.AddSingleton<IChatGPTService, ChatGPTService>();
                    services.AddScoped<CurrentAssistant>();
                    services.AddScoped<IMapDbContext, MapDbContext>();
                    services.AddScoped<IDocumentService, DocumentService>();
                    services.AddSingleton<IAssistantService, AssistantService>();
                    services.AddSingleton<AssistantInitializer>();
                    services.AddHostedService<AssistantInitializer>();
                });
    }
}
