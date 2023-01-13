using System;
using System.Linq;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics.Models;
using AzureMonitorAlertToTeams.Models;
using AzureMonitorAlertToTeams.QueryResultFetchers;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics
{
    public class LogAnalyticsAlertProcessor : IAlertProcessor
    {
        private readonly IQueryResultFetcher _queryResultFetcher;

        public LogAnalyticsAlertProcessor(ILogAnalyticsQueryResultFetcher queryResultFetcher)
        {
            _queryResultFetcher = queryResultFetcher;
        }

        public async ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());
            
            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[$.data.alertContext.Threshold]]", alertContext.Threshold.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Operator]]", alertContext.Operator, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchIntervalDurationMin]]", alertContext.SearchIntervalDurationMin.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchIntervalInMinutes]]", alertContext.SearchIntervalInMinutes.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchIntervalStartTimeUtc]]", alertContext.FormattedStartDateTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchIntervalEndtimeUtc]]", alertContext.FormattedEndDateTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.AlertType]]", alertContext.AlertType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Threshold]]", alertContext.Threshold.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.WorkspaceId]]", alertContext.WorkspaceId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.ResultCount]]", alertContext.ResultCount.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.LinkToFilteredSearchResultsApi]]", alertContext.LinkToFilteredSearchResultsApi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.LinkToFilteredSearchResultsUi]]", alertContext.LinkToFilteredSearchResultsUi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.LinkToSearchResults]]", alertContext.LinkToSearchResults.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.LinkToSearchResultsApi]]", alertContext.LinkToSearchResultsApi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchQuery]]", alertContext.SearchQuery, StringComparison.InvariantCultureIgnoreCase);

            foreach (var configurationItem in alertContext.AffectedConfigurationItems)
            {
                var index = alertContext.AffectedConfigurationItems.IndexOf(configurationItem) + 1;

                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[$.data.alertContext.AffectedConfigurationItems[{index}]]]", configurationItem, StringComparison.InvariantCultureIgnoreCase);
            }

            teamsMessageTemplate = await UpdateMessageWithSearchResultsAsync(teamsMessageTemplate, alertConfiguration, alertContext);

            return teamsMessageTemplate;
        }

        private async Task<string> UpdateMessageWithSearchResultsAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration, AlertContext alertContext)
        {
            var result = await _queryResultFetcher.FetchLogQueryResultsAsync(alertContext.LinkToSearchResultsApi.ToString(), alertConfiguration.Context.ToString());
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
                            .Replace($"[[$.data.alertContext.SearchResults.Tables[{tableIndex}].Rows[{rowIndex}].{column}]]", row[Array.IndexOf(columns, column)].Replace("\"", ""), StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }

            return teamsMessageTemplate;
        }
    }
}
