using System;
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
            var configuration = JsonConvert.DeserializeObject<Configuration>(alertConfiguration.Context.ToString());
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());
            var result = await FetchLogQueryResultsAsync(configuration, alertContext);

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.Threshold]]", alertContext.Threshold.ToString())
                .Replace("[[alert.alertContext.Operator]]", alertContext.Operator)
                .Replace("[[alert.alertContext.SearchIntervalDurationMin]]", alertContext.SearchIntervalDurationMin.ToString())
                .Replace("[[alert.alertContext.SearchIntervalInMinutes]]", alertContext.SearchIntervalInMinutes.ToString())
                .Replace("[[alert.alertContext.SearchIntervalStartTimeUtc]]", alertContext.FormattedStartDateTime)
                .Replace("[[alert.alertContext.SearchIntervalEndtimeUtc]]", alertContext.FormattedEndDateTime)
                .Replace("[[alert.alertContext.AlertType]]", alertContext.AlertType)
                .Replace("[[alert.alertContext.Threshold]]", alertContext.Threshold.ToString())
                .Replace("[[alert.alertContext.ApplicationId]]", alertContext.ApplicationId.ToString())
                .Replace("[[alert.alertContext.ResultCount]]", alertContext.ResultCount.ToString())
                .Replace("[[alert.alertContext.LinkToFilteredSearchResultsApi]]", alertContext.LinkToFilteredSearchResultsApi.ToString())
                .Replace("[[alert.alertContext.LinkToFilteredSearchResultsUi]]", alertContext.LinkToFilteredSearchResultsUi.ToString())
                .Replace("[[alert.alertContext.LinkToSearchResults]]", alertContext.LinkToSearchResults.ToString())
                .Replace("[[alert.alertContext.LinkToSearchResultsApi]]", alertContext.LinkToSearchResultsApi.ToString())
                .Replace("[[alert.alertContext.SearchQuery]]", alertContext.SearchQuery);

            foreach (var dimension in alertContext.Dimensions)
            {
                var index = Array.IndexOf(alertContext.Dimensions, dimension) + 1;

                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[alert.alertContext.Dimensions[{index}].Name]]", dimension.Name, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[alert.alertContext.Dimensions[{index}].Value]]", dimension.Value, StringComparison.InvariantCultureIgnoreCase);
            }

            foreach (var table in result.Tables)
            {
                var tableIndex = Array.IndexOf(result.Tables, table) + 1;

                foreach (var row in table.Rows)
                {
                    var rowIndex = Array.IndexOf(table.Rows, row) + 1;

                    var columns = table.Columns;
                    foreach (var column in columns)
                    {
                        teamsMessageTemplate = teamsMessageTemplate
                            .Replace($"[[alert.alertContext.SearchResults.Tables[{tableIndex}].Rows[{rowIndex}].{column.Name}]]", row[Array.IndexOf(columns, column.Name)], StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }

            return teamsMessageTemplate;
        }

        private async Task<ResultSet> FetchLogQueryResultsAsync(Configuration alertConfiguration, AlertContext alertContext)
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", alertConfiguration.ApiKey);

            _log.LogInformation($"Attempting to get data from {alertContext.LinkToSearchResultsApi}");

            var rawResult = await _httpClient.GetStringAsync(alertContext.LinkToSearchResultsApi);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;
        }
    }
}