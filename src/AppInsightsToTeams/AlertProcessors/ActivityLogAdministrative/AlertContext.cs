using System;
namespace AzureMonitorAlertToTeams.AlertProcessors.ActivityLogAdministrative
{
    public class AlertContext
    {
        public Authorization Authorization { get; set; }
        public string Channels { get; set; }
        public string Claims { get; set; }
        public Guid? Caller { get; set; }
        public Guid? CorrelationId { get; set; }
        public string EventSource { get; set; }
        public DateTimeOffset? EventTimestamp { get; set; }
        public Guid? EventDataId { get; set; }
        public string Level { get; set; }
        public string OperationName { get; set; }
        public Guid? OperationId { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }
        public DateTimeOffset? SubmissionTimestamp { get; set; }
        public string FormattedEventTimestamp => EventTimestamp?.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedSubmissionTimestamp => SubmissionTimestamp?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class Authorization
    {
        public string Action { get; set; }
        public string Scope { get; set; }
    }
}
