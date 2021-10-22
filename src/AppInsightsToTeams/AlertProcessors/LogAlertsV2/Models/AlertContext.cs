using System;
using System.Collections.Generic;

namespace AzureMonitorAlertToTeams.AlertProcessors.LogAlertsV2.Models
{
    public class AlertContext
    {
        public object Properties { get; set; }
        public string ConditionType { get; set; }
        public Condition Condition { get; set; }
    }

    public class Dimension
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class FailingPeriods
    {
        public int NumberOfEvaluationPeriods { get; set; }
        public int MinFailingPeriodsToAlert { get; set; }
    }

    public class AllOf
    {
        public string SearchQuery { get; set; }
        public object MetricMeasure { get; set; }
        public string TargetResourceTypes { get; set; }
        public string Operator { get; set; }
        public string Threshold { get; set; }
        public string TimeAggregation { get; set; }
        public List<Dimension> Dimensions { get; set; } = new List<Dimension>();
        public double MetricValue { get; set; }
        public FailingPeriods FailingPeriods { get; set; }
        public string LinkToSearchResultsUi { get; set; }
        public string LinkToFilteredSearchResultsUi { get; set; }
        public string LinkToSearchResultsApi { get; set; }
        public string LinkToFilteredSearchResultsApi { get; set; }
    }

    public class Condition
    {
        public string WindowSize { get; set; }
        public List<AllOf> AllOf { get; set; }
        public DateTime WindowStartTime { get; set; }
        public DateTime WindowEndTime { get; set; }

        public string FormattedWindowStartTime => WindowStartTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedWindowEndTime => WindowEndTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
