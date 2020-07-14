using System;

namespace AzureMonitorAlertToTeams.AlertProcessors.ActivityLogAutoscale
{
    public class AlertContext
    {
        public string Channels { get; set; }
        public string Claims { get; set; }
        public string Caller { get; set; }
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
        public string Description { get; set; }
        public string ResourceName { get; set; }
        public long? OldInstancesCount { get; set; }
        public long? NewInstancesCount { get; set; }
        public string ActiveAutoscaleProfile { get; set; }
        public string LastScaleActionTime { get; set; }
    }
}
