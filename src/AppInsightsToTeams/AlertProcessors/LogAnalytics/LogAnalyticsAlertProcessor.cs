using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics.Models;
using AzureMonitorAlertToTeams.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics
{
    public class LogAnalyticsAlertProcessor : IAlertProcessor
    {
        private readonly ILogger _log;
        private readonly HttpClient _httpClient;

        public LogAnalyticsAlertProcessor(ILogger log, HttpClient httpClient)
        {
            _log = log;
            _httpClient = httpClient;
        }

        public async ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
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
                .Replace("[[alert.alertContext.WorkspaceId]]", alertContext.WorkspaceId)
                .Replace("[[alert.alertContext.ResultCount]]", alertContext.ResultCount.ToString())
                .Replace("[[alert.alertContext.LinkToFilteredSearchResultsApi]]", alertContext.LinkToFilteredSearchResultsApi.ToString())
                .Replace("[[alert.alertContext.LinkToFilteredSearchResultsUi]]", alertContext.LinkToFilteredSearchResultsUi.ToString())
                .Replace("[[alert.alertContext.LinkToSearchResults]]", alertContext.LinkToSearchResults.ToString())
                .Replace("[[alert.alertContext.LinkToSearchResultsApi]]", alertContext.LinkToSearchResultsApi.ToString())
                .Replace("[[alert.alertContext.SearchQuery]]", alertContext.SearchQuery);

            foreach (var configurationItem in alertContext.AffectedConfigurationItems)
            {
                var index = alertContext.AffectedConfigurationItems.IndexOf(configurationItem) + 1;

                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[alert.alertContext.AffectedConfigurationItems[{index}]]]", configurationItem);
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
                            .Replace($"[[alert.alertContext.SearchResults.Tables[{tableIndex}].Rows[{rowIndex}].{column.Name}]]", row[Array.IndexOf(columns, column.Name)]);
                    }
                }
            }

            return teamsMessageTemplate;
        }

        private async Task<ResultSet> FetchLogQueryResultsAsync(Configuration alertConfiguration, AlertContext alertContext)
        {
            var formData = new Dictionary<string, string>
            {
                {"client_id", alertConfiguration.ClientId},
                {"redirect_uri", alertConfiguration.RedirectUrl},
                {"grant_type", "client_credentials"},
                {"client_secret", alertConfiguration.ClientSecret},
                {"resource", "https://api.loganalytics.io"}
            };

            var tokenResponse = await _httpClient.PostAsync($"https://login.microsoftonline.com/{alertConfiguration.TenantId}/oauth2/token", new FormUrlEncodedContent(formData));
            var token = await tokenResponse.Content.ReadAsStringAsync();
            if(!tokenResponse.IsSuccessStatusCode)
                throw new HttpRequestException(token);
            
            var getUrl = $"https://api.loganalytics.io/v1/workspaces/{alertContext.WorkspaceId}/query?timespan={alertContext.FormattedStartDateTime}/{alertContext.FormattedEndDateTime}&query={alertContext.FormattedSearchQuery}";

            _log.LogInformation($"Attempting to get data from {getUrl}");

            var rawResult = await _httpClient.GetStringAsync(getUrl);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;
        }
    }
}
