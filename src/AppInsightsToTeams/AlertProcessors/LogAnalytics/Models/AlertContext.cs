using System;
using System.Collections.Generic;
using System.Globalization;

namespace AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics.Models
{
    public class AlertContext
    {
        private readonly CultureInfo _cultureInfo = new ("en-US");

        public string SearchQuery { get; set; }
        public string SearchIntervalStartTimeUtc { get; set; }
        public string SearchIntervalEndtimeUtc { get; set; }
        public long? ResultCount { get; set; }
        public Uri LinkToSearchResults { get; set; }
        public Uri LinkToFilteredSearchResultsUi { get; set; }
        public Uri LinkToSearchResultsApi { get; set; }
        public Uri LinkToFilteredSearchResultsApi { get; set; }
        public string SeverityDescription { get; set; }
        public string WorkspaceId { get; set; }
        public long? SearchIntervalDurationMin { get; set; }
        public List<string> AffectedConfigurationItems { get; set; }
        public long? SearchIntervalInMinutes { get; set; }
        public long? Threshold { get; set; }
        public string Operator { get; set; }
        public SearchResult SearchResults { get; set; }
        public List<DataSource> DataSources { get; set; } = new List<DataSource>();
        public string IncludeSearchResults { get; set; }
        public string AlertType { get; set; }
        public string FormattedSearchQuery => SearchQuery.Replace("\n", string.Empty);
        public string FormattedStartDateTime => DateTime.Parse(SearchIntervalStartTimeUtc, _cultureInfo).ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedEndDateTime => DateTime.Parse(SearchIntervalEndtimeUtc, _cultureInfo).ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class DataSource
    {
        public string ResourceId { get; set; }
        public List<string> Tables { get; set; }
    }

    public class SearchResult
    {
        public List<Table> Tables { get; set; } = new List<Table>();
    }

    public class Table
    {
        public string Name { get; set; }
        public List<Column> Columns { get; set; } = new List<Column>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
    }

    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
