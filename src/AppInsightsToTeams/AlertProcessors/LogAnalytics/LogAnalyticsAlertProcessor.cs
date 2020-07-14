using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics.Models;
using AzureMonitorAlertToTeams.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics
{
    public class LogAnalyticsAlertProcessor : IAlertProcessor
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _log;
    
        public LogAnalyticsAlertProcessor(ILogger log, IHttpClientFactory httpClientFactory)
        {
            _log = log;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
        {
            var configuration = JsonConvert.DeserializeObject<Configuration>(alertConfiguration.Context.ToString());
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());
            var result = await FetchLogQueryResultsAsync(configuration, alertContext);
           
            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.Threshold]]", alertContext.Threshold.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Operator]]", alertContext.Operator, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchIntervalDurationMin]]", alertContext.SearchIntervalDurationMin.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchIntervalInMinutes]]", alertContext.SearchIntervalInMinutes.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchIntervalStartTimeUtc]]", alertContext.FormattedStartDateTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchIntervalEndtimeUtc]]", alertContext.FormattedEndDateTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.AlertType]]", alertContext.AlertType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Threshold]]", alertContext.Threshold.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.WorkspaceId]]", alertContext.WorkspaceId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.ResultCount]]", alertContext.ResultCount.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.LinkToFilteredSearchResultsApi]]", alertContext.LinkToFilteredSearchResultsApi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.LinkToFilteredSearchResultsUi]]", alertContext.LinkToFilteredSearchResultsUi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.LinkToSearchResults]]", alertContext.LinkToSearchResults.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.LinkToSearchResultsApi]]", alertContext.LinkToSearchResultsApi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SearchQuery]]", alertContext.SearchQuery, StringComparison.InvariantCultureIgnoreCase);

            foreach (var configurationItem in alertContext.AffectedConfigurationItems)
            {
                var index = alertContext.AffectedConfigurationItems.IndexOf(configurationItem) + 1;

                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[alert.alertContext.AffectedConfigurationItems[{index}]]]", configurationItem, StringComparison.InvariantCultureIgnoreCase);
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
            var formData = new Dictionary<string, string>
            {
                {"client_id", alertConfiguration.ClientId},
                {"redirect_uri", alertConfiguration.RedirectUrl},
                {"grant_type", "client_credentials"},
                {"client_secret", alertConfiguration.ClientSecret},
                {"resource", "https://api.loganalytics.io"}
            };
            
            var postResponse = await _httpClient.PostAsync($"https://login.microsoftonline.com/{alertConfiguration.TenantId}/oauth2/token", new FormUrlEncodedContent(formData));
            var tokenData = await postResponse.Content.ReadAsStringAsync();
            if(!postResponse.IsSuccessStatusCode)
                throw new HttpRequestException(tokenData);

            var token = JsonConvert.DeserializeObject<dynamic>(tokenData);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token.access_token);

            var rawResult = await _httpClient.GetStringAsync(alertContext.LinkToSearchResultsApi);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;
        }
    }
}
