using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace chatgptbot.Configuration
{
    public class UserEndpointCorsPolicyConfiguration
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public Action<CorsOptions> SetupAction;

        public UserEndpointCorsPolicyConfiguration(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
            CorsOptions opt = new CorsOptions();
            _logger.Information("Setup cors policy");
            SetupAction = new Action<CorsOptions>(options =>
                                options.AddDefaultPolicy(
                        builder => builder.
                            AllowAnyMethod().AllowAnyHeader().WithOrigins(
                                                                         "http://localhost:3000",
                                                                           "http://localhost:44337",
                                                                           "https://cavehillmaps.cavehill.uwi.edu",
                                                                           "http://owl4:8080"
                                                                           )
                               )             
            );
            
        }
    }
}
