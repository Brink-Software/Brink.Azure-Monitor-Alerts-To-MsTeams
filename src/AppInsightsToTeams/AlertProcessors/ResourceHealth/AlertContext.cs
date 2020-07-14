using System;

namespace AzureMonitorAlertToTeams.AlertProcessors.ResourceHealth
{
    public class AlertContext
    {
        public string Channels { get; set; }
        public string CorrelationId { get; set; }
        public string EventSource { get; set; }
        public DateTimeOffset? EventTimestamp { get; set; }
        public string EventDataId { get; set; }
        public string Level { get; set; }
        public string OperationName { get; set; }
        public string OperationId { get; set; }
        public Properties Properties { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? SubmissionTimestamp { get; set; }
        public string FormattedEventTimestamp => EventTimestamp?.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedSubmissionTimestamp => SubmissionTimestamp?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class Properties
    {
        public string Title { get; set; }
        public string Details { get; set; }
        public string CurrentHealthStatus { get; set; }
        public string PreviousHealthStatus { get; set; }
        public string Type { get; set; }
        public string Cause { get; set; }
    }
}
