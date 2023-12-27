using GenesysContactsProcessJob;
using GenesysContactsProcessJob.DataLayer.Interfaces;
using GenesysContactsProcessJob.GenesysLayer.Interfaces;
using GenesysContactsProcessJob.GenesysLayer.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GenesysContactsProcessJob
{
    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configures the app configuration.
        /// </summary>
        /// <param name="builder">Builder.</param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="builder">Builder.<see cref="IFunctionsHostBuilder"/></param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Initialize constants
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddHttpClient();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddSingleton<IDataLayer>((s) =>
            {
                return new DataLayer.Services.DataLayer();
            });
            builder.Services.AddTransient<IGenesysClientService, GenesysClientService>((s) =>
            {
                var httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                var configuration = s.GetRequiredService<IConfiguration>();
                return new GenesysClientService(httpClientFactory, configuration);
            });
        }
    }
}

