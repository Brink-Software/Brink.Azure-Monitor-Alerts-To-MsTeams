using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights.Models;
using AzureMonitorAlertToTeams.Models;
using AzureMonitorAlertToTeams.QueryResultFetchers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights
{
    public class ApplicationInsightsAlertProcessor : IAlertProcessor
    {
        private readonly ILogger _log;
        private readonly IQueryResultFetcher _queryResultFetcher;

        public ApplicationInsightsAlertProcessor(ILogger<ApplicationInsightsAlertProcessor> log, IAppInsightsQueryResultFetcher queryResultFetcher)
        {
            _log = log;
            _queryResultFetcher = queryResultFetcher;
        }

        public ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration, Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());

            this._log.LogDebug("Alert context: {alertContext}", alert.Data.AlertContext.ToString());

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[$.data.alertContext.Threshold]]", alertContext.Threshold?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Operator]]", alertContext.Operator, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchIntervalDurationMin]]", alertContext.SearchIntervalDurationMin?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchIntervalInMinutes]]", alertContext.SearchIntervalInMinutes?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchIntervalStartTimeUtc]]", alertContext.FormattedStartDateTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchIntervalEndtimeUtc]]", alertContext.FormattedEndDateTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.AlertType]]", alertContext.AlertType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Threshold]]", alertContext.Threshold.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.ApplicationId]]", alertContext.ApplicationId?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.ResultCount]]", alertContext.ResultCount?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.LinkToFilteredSearchResultsApi]]", alertContext.LinkToFilteredSearchResultsApi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.LinkToFilteredSearchResultsUi]]", alertContext.LinkToFilteredSearchResultsUi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.LinkToSearchResults]]", alertContext.LinkToSearchResults.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.LinkToSearchResultsApi]]", alertContext.LinkToSearchResultsApi.OriginalString, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SearchQuery]]", alertContext.SearchQuery);

            foreach (var dimension in alertContext.Dimensions)
            {
                var index = Array.IndexOf(alertContext.Dimensions, dimension) + 1;

                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[$.data.alertContext.Dimensions[{index}].Name]]", dimension.Name, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Dimensions[{index}].Value]]", dimension.Value, StringComparison.InvariantCultureIgnoreCase);
            }

            teamsMessageTemplate = UpdateMessageWithSearchResults(teamsMessageTemplate, alertConfiguration, alertContext);

            this._log.LogDebug("Finished message is: {messageTemplate}", teamsMessageTemplate);

            return new ValueTask<string>(teamsMessageTemplate);
        }

        private string UpdateMessageWithSearchResults(string teamsMessageTemplate, AlertConfiguration alertConfiguration, AlertContext alertContext)
        {
            // Not sure why this would be needed, as the search results are already part of the alert
            // var result = await _queryResultFetcher.FetchLogQueryResultsAsync(alertContext.LinkToSearchResultsApi.ToString(), alertConfiguration.Context.ToString());

            List<string> sections = new List<string>();

            var tableIndex = -1;
            foreach (var table in alertContext.SearchResults.Tables)
            {
                tableIndex++;

                var rowIndex = -1;
                foreach (var row in table.Rows)
                {
                    rowIndex++;

                    var sectionTemplate = alertConfiguration.TeamsMessageSectionTemplateAsJson;

                    var columnIndex = -1;
                    foreach (var column in table.Columns)
                    {
                        columnIndex++;

                        try
                        {
                            this._log.LogDebug("Table {tableIndex}, Row {rowIndex}, Column {name}: {value}", tableIndex, rowIndex, column.Name, row[columnIndex]);

                            if (row[columnIndex] is null)
                            {
                                continue;
                            }

                            teamsMessageTemplate = teamsMessageTemplate
                                .Replace($"[[$.data.alertContext.SearchResults.Tables[{tableIndex}].Rows[{rowIndex}].{column.Name}]]", row[columnIndex].Replace("\"", ""), StringComparison.InvariantCultureIgnoreCase);

                            if (sectionTemplate != null)
                            {
                                sectionTemplate = sectionTemplate.Replace($"[[$.{column.Name}]]", row[columnIndex]);
                            }
                        }
                        catch (Exception e)
                        {
                            this._log.LogError(e, "Error replacing column {columnName} with type {columnType}", column.Name, column.Type);
                        }
                    }

                    if (sectionTemplate != null)
                    {
                        sections.Add(sectionTemplate);
                    }
                }
            }

            if (alertConfiguration.TeamsMessageSectionTemplate != null)
            {
                teamsMessageTemplate = teamsMessageTemplate.Replace("\"[[TeamsMessageSectionTemplate]]\"", String.Join(", ", sections));
            }

            return teamsMessageTemplate;
        }
    }
}
