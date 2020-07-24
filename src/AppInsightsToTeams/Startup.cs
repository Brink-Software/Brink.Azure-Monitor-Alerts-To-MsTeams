using AzureMonitorAlertToTeams;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureMonitorAlertToTeams
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ITelemetryInitializer, TelemetryEnrichment>();
            builder.Services.AddLogging();
            builder.Services.AddHttpClient();
        }
    }
}