using System;
using System.Globalization;

namespace AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights.Models
{
    public class AlertContext
    {
        private readonly CultureInfo _cultureInfo = new CultureInfo("en-US");

        public string SearchQuery { get; set; }
        public string SearchIntervalStartTimeUtc { get; set; }
        public string SearchIntervalEndtimeUtc { get; set; }
        public long? ResultCount { get; set; }
        public Uri LinkToSearchResults { get; set; }
        public Uri LinkToFilteredSearchResultsUi { get; set; }
        public Uri LinkToSearchResultsApi { get; set; }
        public Uri LinkToFilteredSearchResultsApi { get; set; }
        public long? SearchIntervalDurationMin { get; set; }
        public long? SearchIntervalInMinutes { get; set; }
        public long? Threshold { get; set; }
        public string Operator { get; set; }
        public Guid? ApplicationId { get; set; }
        public Dimension[] Dimensions { get; set; }
        public SearchResults SearchResults { get; set; }
        public string IncludeSearchResults { get; set; }
        public string AlertType { get; set; }
        public string FormattedSearchQuery => SearchQuery.Replace("\n", string.Empty);
        public string FormattedStartDateTime => DateTime.Parse(SearchIntervalStartTimeUtc, _cultureInfo).ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedEndDateTime => DateTime.Parse(SearchIntervalEndtimeUtc, _cultureInfo).ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class Dimension
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class SearchResults
    {
        public Table[] Tables { get; set; }
        public DataSource[] DataSources { get; set; }
    }

    public class DataSource
    {
        public string ResourceId { get; set; }
        public string[] Tables { get; set; }
    }

    public class Table
    {
        public string Name { get; set; }
        public Column[] Columns { get; set; }
        public string[][] Rows { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
