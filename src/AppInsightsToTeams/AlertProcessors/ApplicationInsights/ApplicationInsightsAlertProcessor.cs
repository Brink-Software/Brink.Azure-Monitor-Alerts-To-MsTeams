using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights.Models;
using AzureMonitorAlertToTeams.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights
{
    public class ApplicationInsightsAlertProcessor : IAlertProcessor
    {
        private readonly ILogger _log;
        private readonly HttpClient _httpClient;

        public ApplicationInsightsAlertProcessor(ILogger log, HttpClient httpClient)
        {
            _log = log;
            _httpClient = httpClient;
        }

        public async ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration, Alert alert)
        {
            var appInsightsConfiguration = JsonConvert.DeserializeObject<Configuration>(alertConfiguration.Context.ToString());
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());
            var result = await FetchLogQueryResultsAsync(appInsightsConfiguration, alertContext);

            if (!result.Tables.Any() || !result.Tables[0].Columns.Any())
                return teamsMessageTemplate;

            teamsMessageTemplate = teamsMessageTemplate.Replace("[[alert.alertContext.LinkToSearchResults]]", alertContext.LinkToSearchResults.ToString())
                .Replace("[[alert.alertContext.Threshold]]", alertContext.Threshold.ToString())
                .Replace("[[alert.alertContext.Operator]]", alertContext.Operator)
                .Replace("[[alert.alertContext.SearchIntervalDurationMin]]", alertContext.SearchIntervalDurationMin.ToString())
                .Replace("[[alert.alertContext.SearchIntervalInMinutes]]", alertContext.SearchIntervalInMinutes.ToString())
                .Replace("[[alert.alertContext.SearchIntervalStartTimeUtc]]", alertContext.FormattedStartDateTime)
                .Replace("[[alert.alertContext.SearchIntervalEndtimeUtc]]", alertContext.FormattedEndDateTime);

            foreach (var row in result.Tables.First().Rows)
            {
                var columns = result.Tables.First().Columns;
                foreach (var column in columns)
                {
                    teamsMessageTemplate = teamsMessageTemplate
                        .Replace($"[[alert.alertContext.Result.{column.Name}]]", row[Array.IndexOf(columns, column.Name)]);
                }
            }

            return teamsMessageTemplate;
        }

        private async Task<ResultSet> FetchLogQueryResultsAsync(Configuration alertConfiguration, AlertContext alertContext)
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", alertConfiguration.ApiKey);

            var getUrl = $"https://api.applicationinsights.io/v1/apps/{alertConfiguration.Id}/query?timespan={alertContext.FormattedStartDateTime}/{alertContext.FormattedEndDateTime}&query={alertContext.FormattedSearchQuery}";

            _log.LogInformation($"Attempting to get data from {getUrl}");

            var rawResult = await _httpClient.GetStringAsync(getUrl);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;
        }
    }
}