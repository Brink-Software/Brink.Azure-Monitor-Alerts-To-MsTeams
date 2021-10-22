using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.LogAlertsV2.Models;
using AzureMonitorAlertToTeams.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.LogAlertsV2
{
    public class LogAlertsV2AlertProcessor : IAlertProcessor
    {
        private readonly ILogger _log;
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;

        public LogAlertsV2AlertProcessor(ILogger log, IHttpClientFactory httpClientFactory)
        {
            _log = log;
            _httpClientFactory = httpClientFactory;
        }

        public async ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration, Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());
            
            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[$.data.alertContext.ConditionType]]", alertContext.ConditionType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Condition.WindowEndTime]]", alertContext.Condition.FormattedWindowEndTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Condition.WindowStartTime]]", alertContext.Condition.FormattedWindowStartTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Condition.WindowSize]]", alertContext.Condition.WindowSize, StringComparison.InvariantCultureIgnoreCase);

            var index = 1;
            foreach (var condition in alertContext.Condition.AllOf) 
            {
                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].LinkToFilteredSearchResultsApi]]", condition.LinkToFilteredSearchResultsApi, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].LinkToFilteredSearchResultsUi]]", condition.LinkToFilteredSearchResultsUi, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].LinkToSearchResultsApi]]", condition.LinkToSearchResultsApi, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].LinkToSearchResultsUi]]", condition.LinkToSearchResultsUi, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].SearchQuery]]", condition.SearchQuery)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Threshold]]", condition.Threshold, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Operator]]", condition.Operator, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].TimeAggregation]]", condition.TimeAggregation, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Threshold]]", condition.Threshold, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].TargetResourceTypes]]", condition.TargetResourceTypes, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].MetricValue]]", condition.MetricValue.ToString(), StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].FailingPeriods.MinFailingPeriodsToAlert]]", condition.FailingPeriods.MinFailingPeriodsToAlert.ToString(), StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].FailingPeriods.NumberOfEvaluationPeriods]]", condition.FailingPeriods.NumberOfEvaluationPeriods.ToString(), StringComparison.InvariantCultureIgnoreCase);

                int dimensionsIndex = 1;
                foreach (var dimension in condition.Dimensions)
                {
                    teamsMessageTemplate = teamsMessageTemplate
                        .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Dimensions[{dimensionsIndex}].Name]]", dimension.Name, StringComparison.InvariantCultureIgnoreCase)
                        .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Dimensions[{dimensionsIndex}].Value]]", dimension.Value, StringComparison.InvariantCultureIgnoreCase);
                    ++dimensionsIndex;
                }

                teamsMessageTemplate = await UpdateMessageWithSearchResultsAsync(teamsMessageTemplate, alertConfiguration, condition, index);

                ++index;
            }

            return teamsMessageTemplate;
        }

        private async Task<string> UpdateMessageWithSearchResultsAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration, AllOf condition, int conditionIndex)
        {
            var configuration = JsonConvert.DeserializeObject<Configuration>(alertConfiguration.Context.ToString());

            if (configuration?.ClientId == null)
                return teamsMessageTemplate;

            var result = await FetchLogQueryResultsAsync(configuration, condition);
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
                            .Replace($"[[$.data.alertContext.SearchResults[{conditionIndex}].Tables[{tableIndex}].Rows[{rowIndex}].{column}]]", row[Array.IndexOf(columns, column)], StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }

            return teamsMessageTemplate;
        }

        private async Task<ResultSet> FetchLogQueryResultsAsync(Configuration alertConfiguration, AllOf condition)
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
            if (!postResponse.IsSuccessStatusCode)
                throw new HttpRequestException(tokenData);

            var token = JsonConvert.DeserializeObject<dynamic>(tokenData);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token.access_token);

            var rawResult = await _httpClient.GetStringAsync(condition.LinkToSearchResultsApi);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;
        }
    }
}