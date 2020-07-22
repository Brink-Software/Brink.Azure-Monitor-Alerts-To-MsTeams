using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AzureMonitorAlertToTeams.AlertProcessors;
using AzureMonitorAlertToTeams.AlertProcessors.ActivityLogAdministrative;
using AzureMonitorAlertToTeams.AlertProcessors.ActivityLogAutoscale;
using AzureMonitorAlertToTeams.AlertProcessors.ActivityLogPolicy;
using AzureMonitorAlertToTeams.AlertProcessors.ActivityLogSecurity;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights;
using AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics;
using AzureMonitorAlertToTeams.AlertProcessors.Metric;
using AzureMonitorAlertToTeams.AlertProcessors.ResourceHealth;
using AzureMonitorAlertToTeams.AlertProcessors.ServiceHealth;
using AzureMonitorAlertToTeams.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams
{
    public class AzureMonitorAlertToTeamFunction
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _log;
        private IEnumerable<AlertConfiguration> _alertConfigurations;
        private readonly Dictionary<string, Func<IAlertProcessor>> _alertProcessors;

        public AzureMonitorAlertToTeamFunction(IHttpClientFactory httpClientFactory, ILogger<AzureMonitorAlertToTeamFunction> log)
        {
            _httpClient = httpClientFactory.CreateClient();
            _log = log;

            _alertProcessors = new Dictionary<string, Func<IAlertProcessor>>
            {
                {"Application Insights", () => new ApplicationInsightsAlertProcessor(_log, httpClientFactory)},
                {"Activity Log - Administrative", () => new ActivityLogAdministrativeAlertProcessor()},
                {"Activity Log - Policy", () => new ActivityLogPolicyAlertProcessor()},
                {"Activity Log - Autoscale", () => new ActivityLogAutoscaleAlertProcessor()},
                {"Activity Log - Security", () => new ActivityLogSecurityAlertProcessor()},
                {"Log Analytics", () => new LogAnalyticsAlertProcessor(_log, httpClientFactory)},
                {"Platform", () => new MetricAlertProcessor()},
                {"Resource Health", () => new ResourceHealthAlertProcessor()},
                {"ServiceHealth", () => new ServiceHealthAlertProcessor()}
            };
        }

        [FunctionName("AzureMonitorAlertToTeams")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Blob("%ContainerName%/%ConfigurationFilename%", FileAccess.Read, Connection = "ConfigurationStorageConnection")] Stream configuration)
        {
            _alertConfigurations ??= await ReadConfigurationAsync(configuration);

            string requestBody;
            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var alert = JsonConvert.DeserializeObject<Alert>(requestBody);

            var alertConfiguration = _alertConfigurations.FirstOrDefault(ac => 
                    ac.AlertRule == alert.Data.Essentials.AlertRule 
                    && alert.Data.Essentials.AlertTargetIDs.Contains(ac.AlertTargetID));
            if (alertConfiguration == null)
                return new BadRequestErrorMessageResult($"No configuration found for Azure Monitor Alert with id {alert.Data.Essentials.AlertId}");

            var teamsMessageTemplate = alertConfiguration.TeamsMessageTemplateAsJson
                .Replace("[[$.data.essentials.alertRule]]", alert.Data.Essentials.AlertRule, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.description]]", alert.Data.Essentials.Description, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.severity]]", alert.Data.Essentials.Severity, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.signalType]]", alert.Data.Essentials.SignalType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.monitorCondition]]", alert.Data.Essentials.MonitorCondition, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.monitoringService]]", alert.Data.Essentials.MonitoringService, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.firedDateTime]]", alert.Data.Essentials.FormattedFiredDateTime, StringComparison.InvariantCultureIgnoreCase);

            foreach (var essentialsAlertTargetID in alert.Data.Essentials.AlertTargetIDs)
            {
                var index = Array.IndexOf(alert.Data.Essentials.AlertTargetIDs, essentialsAlertTargetID) + 1;

                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[$.data.essentials.alertTargetIDs[{index}]]]", essentialsAlertTargetID, StringComparison.InvariantCultureIgnoreCase);
            }

            if (_alertProcessors.ContainsKey(alert.Data.Essentials.MonitoringService))
            {
                var processor = _alertProcessors[alert.Data.Essentials.MonitoringService];
                var alertProcessor = processor.Invoke();
                _log.LogInformation("Processing monitoring service {MonitoringService} using alert processor of type {ProcessorType}", alertProcessor.GetType().FullName, alert.Data.Essentials.MonitoringService);
                teamsMessageTemplate = await alertProcessor.CreateTeamsMessageTemplateAsync(teamsMessageTemplate, alertConfiguration, alert);
            }
            else
            {
                _log.LogInformation("No specific alert processor found for monitoring service {MonitoringService}", alert.Data.Essentials.MonitoringService);
            }

            var response = await _httpClient.PostAsync(alertConfiguration.TeamsChannelConnectorWebhookUrl,
                new StringContent(teamsMessageTemplate, Encoding.UTF8, "application/json"));

            if(!response.IsSuccessStatusCode)
                _log.LogError("Posting to teams failed with status code {StatusCode}: {Reason}", response.StatusCode, response.ReasonPhrase);

            _log.LogDebug(teamsMessageTemplate);

            return new OkResult();
        }

        private static async Task<IEnumerable<AlertConfiguration>> ReadConfigurationAsync(Stream configuration)
        {
            using var sr = new StreamReader(configuration);
            
            return JsonConvert.DeserializeObject<IEnumerable<AlertConfiguration>>(await sr.ReadToEndAsync());
        }
    }
}