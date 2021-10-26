using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
using AzureMonitorAlertToTeams.Models;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
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

        public AzureMonitorAlertToTeamFunction(IHttpClientFactory httpClientFactory, ILogger<AzureMonitorAlertToTeamFunction> log, IServiceProvider serviceProvider)
        {
            _httpClient = httpClientFactory.CreateClient();
            _log = log;

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

        [FunctionName("AzureMonitorAlertToTeams")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Blob("%ContainerName%/%ConfigurationFilename%", FileAccess.Read, Connection = "ConfigurationStorageConnection")] Stream configuration)
        {
            var operationId = req.HttpContext.Features.Get<RequestTelemetry>().Context.Operation.Id;
            
            _alertConfigurations ??= await ReadConfigurationAsync(configuration);

            string requestBody;
            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            await CaptureAlertToFileIfRequested(operationId, requestBody);

            var alert = JsonConvert.DeserializeObject<Alert>(requestBody);
            if (alert?.Data == null)
            {
                _log.LogError("Invalid request body: \"{RequestBody}\".", requestBody);
                throw new ArgumentException("Invalid request", nameof(req));
            }

            var alertConfiguration = _alertConfigurations.FirstOrDefault(ac => 
                    ac.AlertRule.Equals(alert.Data.Essentials.AlertRule, StringComparison.InvariantCultureIgnoreCase)
                    && alert.Data.Essentials.AlertTargetIDs.Any(id => id.Equals(ac.AlertTargetID, StringComparison.InvariantCultureIgnoreCase)));
            if (alertConfiguration == null)
            {
                _log.LogError("No configuration found for Azure Monitor Alert with rule {AlertRule} and targetId of {AlertTargetIDs}", 
                    alert.Data.Essentials.AlertRule, 
                    string.Join(", ", alert.Data.Essentials.AlertTargetIDs));
                throw new InvalidOperationException();
            }

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
                _log.LogInformation("Processing monitoring service {MonitoringService} using alert processor of type {ProcessorType} for alert rule {AlertTargetID}", 
                    alert.Data.Essentials.MonitoringService,
                    alertProcessor.GetType().FullName,
                    alertConfiguration.AlertTargetID);
                teamsMessageTemplate = await alertProcessor.CreateTeamsMessageTemplateAsync(teamsMessageTemplate, alertConfiguration, alert);
            }
            else
            {
                _log.LogInformation("No specific alert processor found for monitoring service {MonitoringService} for alert rule {AlertTargetID}", 
                    alert.Data.Essentials.MonitoringService,
                    alertConfiguration.AlertTargetID);

                throw new InvalidOperationException();
            }

            _log.LogDebug(teamsMessageTemplate);

            var response = await _httpClient.PostAsync(alertConfiguration.TeamsChannelConnectorWebhookUrl,
                new StringContent(teamsMessageTemplate, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError("Posting to teams failed with status code {StatusCode}: {Reason}", response.StatusCode,  response.ReasonPhrase);
                throw new InvalidOperationException();
            }

            return new OkResult();
        }

        private static async Task CaptureAlertToFileIfRequested(string operationId, string requestBody)
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable("CaptureAlerts"), out var shouldCapture) && shouldCapture)
            {
                var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("ConfigurationStorageConnection"));
                var cloudBlobClient = storageAccount.CreateCloudBlobClient();
                var container = cloudBlobClient.GetContainerReference(Environment.GetEnvironmentVariable("ContainerName"));
                var blob = container.GetBlockBlobReference($"{operationId}.json");
                await blob.UploadTextAsync(requestBody);
            }
        }

        private static async Task<IEnumerable<AlertConfiguration>> ReadConfigurationAsync(Stream configuration)
        {
            using var sr = new StreamReader(configuration);
            
            return JsonConvert.DeserializeObject<IEnumerable<AlertConfiguration>>(await sr.ReadToEndAsync());
        }
    }
}
