using chatgptbot.Services;
using chatgptbot.Services.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ILogger = Serilog.ILogger;
using chatgptbot.Configuration;
using chatgptbot.Util;
using Microsoft.EntityFrameworkCore;
using chatgptbot.Data;
using Quartz;
using QuestPDF.Infrastructure;

namespace chatgptbot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using var logger = new LoggerConfiguration()
              .ReadFrom.Configuration(Configuration)
              .CreateLogger();

            ConfigureCorsPolicies(services, Configuration, logger);
            
            services.AddSignalR();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "chatgptbot", Version = "v1" });
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            ILogger<DatabaseConfiguration> mylogger = loggerFactory.CreateLogger<DatabaseConfiguration>();
            DatabaseConfiguration dConfig = new DatabaseConfiguration(Configuration, mylogger);
            var connectionString = dConfig.MySQLConnectString();

            services.AddDbContext<MapDbContext>(options =>
            {
                options.UseMySql(connectionString,
                new MySqlServerVersion(new Version(10, 1, 40)),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure());
            });

            QuestPDF.Settings.License = LicenseType.Community; // Or Commercial, depending on your org

            /*
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("AssistantJob");

                q.AddJob<AssistantInitializer>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("KryptoJob-trigger")
                    .WithCronSchedule("0 0/5 * * * ?"));

            });
            
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);*/
            ExtendConfigurations(services, logger);
        }

        public void ExtendConfigurations(IServiceCollection services, ILogger logger)
        {
            logger.Information("Configuring Services ...");
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "chatgptbot v1"));
            }

            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
        private static void ConfigureCorsPolicies(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            logger.Information("Configuring CORS policies");
            var userEndpointPolicyConfig = new UserEndpointCorsPolicyConfiguration(configuration, logger);
            services.AddCors(userEndpointPolicyConfig.SetupAction);

            logger.Information("CORS policies configured");
        }
    }
}
