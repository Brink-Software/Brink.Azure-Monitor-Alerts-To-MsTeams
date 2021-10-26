using AzureMonitorAlertToTeams;
using AzureMonitorAlertToTeams.AlertProcessors.ActivityLogAdministrative;
using AzureMonitorAlertToTeams.AlertProcessors.ActivityLogAutoscale;
using AzureMonitorAlertToTeams.AlertProcessors.ActivityLogPolicy;
using AzureMonitorAlertToTeams.AlertProcessors.ActivityLogSecurity;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights;
using AzureMonitorAlertToTeams.AlertProcessors.LogAlertsV2;
using AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics;
using AzureMonitorAlertToTeams.AlertProcessors.Metric;
using AzureMonitorAlertToTeams.AlertProcessors.ResourceHealth;
using AzureMonitorAlertToTeams.AlertProcessors.ServiceHealth;
using AzureMonitorAlertToTeams.QueryResultFetchers;
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
            builder.Services.AddLogging();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ITelemetryInitializer, TelemetryEnrichment>();
            builder.Services.AddSingleton<ApplicationInsightsAlertProcessor>();
            builder.Services.AddSingleton<LogAlertsV2AlertProcessor>();
            builder.Services.AddSingleton<ActivityLogAdministrativeAlertProcessor>();
            builder.Services.AddSingleton<ActivityLogPolicyAlertProcessor>();
            builder.Services.AddSingleton<ActivityLogAutoscaleAlertProcessor>();
            builder.Services.AddSingleton<ActivityLogSecurityAlertProcessor>();
            builder.Services.AddSingleton<LogAnalyticsAlertProcessor>();
            builder.Services.AddSingleton<MetricAlertProcessor>();
            builder.Services.AddSingleton<ResourceHealthAlertProcessor>();
            builder.Services.AddSingleton<ServiceHealthAlertProcessor>();
            builder.Services.AddSingleton<IQueryResultFetcherFabric, QueryResultFetcherFabric>();
            builder.Services.AddSingleton<IAppInsightsQueryResultFetcher, AppInsightsQueryResultFetcher>();
            builder.Services.AddSingleton<ILogAnalyticsQueryResultFetcher, LogAnalyticsQueryResultFetcher>();
        }
    }
}