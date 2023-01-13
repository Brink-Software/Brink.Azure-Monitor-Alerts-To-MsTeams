using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams
{
    public class AzureMonitorAlertToTeamFunction
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IAlertProcessorRepository _alertProcessorRepository;

        public AzureMonitorAlertToTeamFunction(IHttpClientFactory httpClientFactory, ILogger<AzureMonitorAlertToTeamFunction> log, IAlertProcessorRepository alertProcessorRepository)
        {
            _httpClient = httpClientFactory.CreateClient();
            _log = log;
            _alertProcessorRepository = alertProcessorRepository;
        }

        [FunctionName("AzureMonitorAlertToTeams")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            [Blob("%ContainerName%/%ConfigurationFilename%", FileAccess.Read,
                Connection = "ConfigurationStorageConnection")]
            Stream configurationStream)
        {
            var operationId = req.HttpContext.Features.Get<RequestTelemetry>().Context.Operation.Id;

            string requestBody;
            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            await CaptureAlertToFileIfRequestedAsync(operationId, requestBody);
            var (teamsMessage, teamsChannelConnectorWebhookUrl) = await ProcessAlertAsync(requestBody, configurationStream);
            await PostToChannelAsync(teamsChannelConnectorWebhookUrl, teamsMessage);

            return new OkResult();
        }

        public async Task<(string, string)> ProcessAlertAsync(string alertJson, Stream configuration)
        {
            var alertConfigurations = await ReadConfigurationAsync(configuration);

            var alert = JsonConvert.DeserializeObject<Alert>(alertJson);
            if (alert?.Data == null)
            {
                _log.LogError("Invalid alert body: \"{AlertJson}\".", alertJson);
                throw new ArgumentException("Invalid request", nameof(alertJson));
            }

            var alertConfiguration = alertConfigurations.FirstOrDefault(ac =>
                ac.AlertRule.Equals(alert.Data.Essentials.AlertRule, StringComparison.InvariantCultureIgnoreCase)
                && alert.Data.Essentials.AlertTargetIDs.Any(id =>
                    id.Equals(ac.AlertTargetID, StringComparison.InvariantCultureIgnoreCase)));
            if (alertConfiguration == null)
            {
                _log.LogError(
                    "No configuration found for Azure Monitor Alert with rule {AlertRule} and targetId of {AlertTargetIDs}",
                    alert.Data.Essentials.AlertRule,
                    string.Join(", ", alert.Data.Essentials.AlertTargetIDs));
                throw new InvalidOperationException();
            }

            var teamsMessageTemplate = alertConfiguration.TeamsMessageTemplateAsJson
                .Replace("[[$.data.essentials.alertRule]]", alert.Data.Essentials.AlertRule,
                    StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.description]]", alert.Data.Essentials.Description,
                    StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.severity]]", alert.Data.Essentials.Severity,
                    StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.signalType]]", alert.Data.Essentials.SignalType,
                    StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.monitorCondition]]", alert.Data.Essentials.MonitorCondition,
                    StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.monitoringService]]", alert.Data.Essentials.MonitoringService,
                    StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.essentials.firedDateTime]]", alert.Data.Essentials.FormattedFiredDateTime,
                    StringComparison.InvariantCultureIgnoreCase);

            foreach (var essentialsAlertTargetID in alert.Data.Essentials.AlertTargetIDs)
            {
                var index = Array.IndexOf(alert.Data.Essentials.AlertTargetIDs, essentialsAlertTargetID) + 1;

                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[$.data.essentials.alertTargetIDs[{index}]]]", essentialsAlertTargetID,
                        StringComparison.InvariantCultureIgnoreCase);
            }

            var alertProcessor = _alertProcessorRepository.GetAlertProcessor(alert.Data.Essentials.MonitoringService);
            if (alertProcessor != null)
            {
                _log.LogInformation(
                    "Processing monitoring service {MonitoringService} using alert processor of type {ProcessorType} for alert rule {AlertTargetID}",
                    alert.Data.Essentials.MonitoringService,
                    alertProcessor.GetType().FullName,
                    alertConfiguration.AlertTargetID);
                teamsMessageTemplate =
                    await alertProcessor.CreateTeamsMessageTemplateAsync(teamsMessageTemplate, alertConfiguration,
                        alert);
            }
            else
            {
                _log.LogInformation(
                    "No specific alert processor found for monitoring service {MonitoringService} for alert rule {AlertTargetID}",
                    alert.Data.Essentials.MonitoringService,
                    alertConfiguration.AlertTargetID);

                throw new InvalidOperationException();
            }

            _log.LogDebug(teamsMessageTemplate);

            return (teamsMessageTemplate, alertConfiguration.TeamsChannelConnectorWebhookUrl);
        }

        public async Task PostToChannelAsync(string teamsChannelConnectorWebhookUrl, string teamsMessageTemplate)
        {
            var response = await _httpClient.PostAsync(teamsChannelConnectorWebhookUrl, new StringContent(teamsMessageTemplate, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError("Posting to teams failed with status code {StatusCode}: {Reason}", response.StatusCode, response.ReasonPhrase);
                throw new InvalidOperationException();
            }
        }

        private static async Task CaptureAlertToFileIfRequestedAsync(string operationId, string requestBody)
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
