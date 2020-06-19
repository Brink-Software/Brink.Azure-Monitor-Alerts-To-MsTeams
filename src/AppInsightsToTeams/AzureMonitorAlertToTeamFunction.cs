using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AzureMonitorAlertToTeams.AlertProcessors;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights;
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

        public AzureMonitorAlertToTeamFunction(HttpClient httpClient, ILogger<AzureMonitorAlertToTeamFunction> log)
        {
            _httpClient = httpClient;
            _log = log;

            _alertProcessors = new Dictionary<string, Func<IAlertProcessor>>
            {
                {"Application Insights", () => new ApplicationInsightsAlertProcessor(_log, _httpClient)}
            };
        }

        [FunctionName("AzureMonitorAlertToTeams")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, 
            ExecutionContext executionContext)
        {
            _alertConfigurations ??= await ReadConfigurationAsync(executionContext);

            string requestBody;
            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var alert = JsonConvert.DeserializeObject<Alert>(requestBody);

            var alertConfiguration = _alertConfigurations.FirstOrDefault(ac => ac.AlertId == alert.Data.Essentials.AlertId);
            if (alertConfiguration == null)
                return new BadRequestErrorMessageResult($"No configuration found for Azure Monitor Alert with id {alert.Data.Essentials.AlertId}");

            var teamsMessageTemplate = alertConfiguration.TeamsMessageTemplate
                .Replace("[[alert.data.essentials.alertRule]]", alert.Data.Essentials.AlertRule)
                .Replace("[[alert.data.essentials.description]]", alert.Data.Essentials.Description)
                .Replace("[[alert.data.essentials.severity]]", alert.Data.Essentials.Severity)
                .Replace("[[alert.data.essentials.signalType]]", alert.Data.Essentials.SignalType)
                .Replace("[[alert.data.essentials.monitorCondition]]", alert.Data.Essentials.MonitorCondition)
                .Replace("[[alert.data.essentials.monitoringService]]", alert.Data.Essentials.MonitoringService)
                .Replace("[[alert.data.essentials.firedDateTime]]", alert.Data.Essentials.FormattedFiredDateTime);

            if (_alertProcessors.ContainsKey(alert.Data.Essentials.MonitoringService))
            {
                var alertProcessor = _alertProcessors[alert.Data.Essentials.MonitoringService].Invoke();
                teamsMessageTemplate = await alertProcessor.CreateTeamsMessageTemplateAsync(teamsMessageTemplate, alertConfiguration, alert);
            }

            await _httpClient.PostAsync(alertConfiguration.TeamsChannelConnectorWebhookUrl,
                new StringContent(teamsMessageTemplate, Encoding.UTF8, "application/json"));

            _log.LogInformation(teamsMessageTemplate);

            return new OkResult();
        }

        private static async Task<IEnumerable<AlertConfiguration>> ReadConfigurationAsync(ExecutionContext executionContext)
        {
            var path = Path.Combine(executionContext.FunctionDirectory, "alert-configurations.json");
            var json = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<IEnumerable<AlertConfiguration>>(json);
        }
    }
}