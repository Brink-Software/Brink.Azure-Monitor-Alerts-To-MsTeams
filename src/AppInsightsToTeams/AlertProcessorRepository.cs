using System;
using System.Collections.Generic;
using AzureMonitorAlertToTeams.AlertProcessors;
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
using Microsoft.Extensions.DependencyInjection;

namespace AzureMonitorAlertToTeams
{
    public interface IAlertProcessorRepository
    {
        IAlertProcessor GetAlertProcessor(string monitoringService);
    }

    public class AlertProcessorRepository : IAlertProcessorRepository
    {
        private readonly Dictionary<string, Func<IAlertProcessor>> _alertProcessors;

        public AlertProcessorRepository(IServiceProvider serviceProvider)
        {
            _alertProcessors = new Dictionary<string, Func<IAlertProcessor>>
            {
                {"Application Insights", serviceProvider.GetService<ApplicationInsightsAlertProcessor>},
                {"Log Alerts V2", serviceProvider.GetService<LogAlertsV2AlertProcessor>},
                {"Activity Log - Administrative", serviceProvider.GetService<ActivityLogAdministrativeAlertProcessor>},
                {"Activity Log - Policy", serviceProvider.GetService<ActivityLogPolicyAlertProcessor>},
                {"Activity Log - Autoscale", serviceProvider.GetService<ActivityLogAutoscaleAlertProcessor>},
                {"Activity Log - Security", serviceProvider.GetService<ActivityLogSecurityAlertProcessor>},
                {"Log Analytics", serviceProvider.GetService<LogAnalyticsAlertProcessor>},
                {"Platform", serviceProvider.GetService<MetricAlertProcessor>},
                {"Resource Health", serviceProvider.GetService<ResourceHealthAlertProcessor>},
                {"ServiceHealth", serviceProvider.GetService<ServiceHealthAlertProcessor>}
            };
        }

        public IAlertProcessor GetAlertProcessor(string monitoringService)
        {
            return _alertProcessors.ContainsKey(monitoringService) ? (IAlertProcessor)_alertProcessors[monitoringService].Invoke() : null;
        }
    }
}