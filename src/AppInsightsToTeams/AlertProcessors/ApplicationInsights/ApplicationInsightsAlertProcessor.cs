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
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;

        public ApplicationInsightsAlertProcessor(ILogger log, IHttpClientFactory httpClientFactory)
        {
            _log = log;
            _httpClientFactory = httpClientFactory;
        }

        public async ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration, Alert alert)
        {
            var configuration = JsonConvert.DeserializeObject<Configuration>(alertConfiguration.Context.ToString());
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());
            var result = await FetchLogQueryResultsAsync(configuration, alertContext);

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.Threshold]]", alertContext.Threshold?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Operator]]", alertContext.Operator, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchIntervalDurationMin]]", alertContext.SearchIntervalDurationMin?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchIntervalInMinutes]]", alertContext.SearchIntervalInMinutes?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchIntervalStartTimeUtc]]", alertContext.FormattedStartDateTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchIntervalEndtimeUtc]]", alertContext.FormattedEndDateTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.AlertType]]", alertContext.AlertType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Threshold]]", alertContext.Threshold.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.ApplicationId]]", alertContext.ApplicationId?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.ResultCount]]", alertContext.ResultCount?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.LinkToFilteredSearchResultsApi]]", alertContext.LinkToFilteredSearchResultsApi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.LinkToFilteredSearchResultsUi]]", alertContext.LinkToFilteredSearchResultsUi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.LinkToSearchResults]]", alertContext.LinkToSearchResults.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.LinkToSearchResultsApi]]", alertContext.LinkToSearchResultsApi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
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

                    var columns = table.Columns.Select(c => c.Name).ToArray();
                    foreach (var column in columns)
                    {
                        teamsMessageTemplate = teamsMessageTemplate
                            .Replace($"[[alert.alertContext.SearchResults.Tables[{tableIndex}].Rows[{rowIndex}].{column}]]", row[Array.IndexOf(columns, column)], StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }

            return teamsMessageTemplate;
        }

        private async Task<ResultSet> FetchLogQueryResultsAsync(Configuration alertConfiguration, AlertContext alertContext)
        {
            var client = _httpClient ?? CreateAndSetClient();

            var rawResult = await client.GetStringAsync(alertContext.LinkToSearchResultsApi);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;

            HttpClient CreateAndSetClient()
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", alertConfiguration.ApiKey);

                return _httpClient;
            }
        }
    }
}