using System;
using System.Collections.Generic;

namespace AzureMonitorAlertToTeams.AlertProcessors.Metric
{
    public class AlertContext
    {
        public object Properties { get; set; }
        public string ConditionType { get; set; }
        public Condition Condition { get; set; }
    }

    public class Condition
    {
        public string WindowSize { get; set; }
        public List<AllOf> AllOf { get; set; }
        public DateTimeOffset? WindowStartTime { get; set; }
        public DateTimeOffset? WindowEndTime { get; set; }
        public string FormattedWindowStartTime => WindowStartTime?.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedWindowEndTime => WindowEndTime?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class AllOf
    {
        public string MetricName { get; set; }
        public string MetricNamespace { get; set; }
        public string Operator { get; set; }
        public long? Threshold { get; set; }
        public string TimeAggregation { get; set; }
        public List<Dimension> Dimensions { get; set; }
        public double? MetricValue { get; set; }
    }

    public class Dimension
    {
        public string Name { get; set; }
        public Guid? Value { get; set; }
    }
}
