using System;

namespace AzureMonitorAlertToTeams.AlertProcessors.ActivityLogPolicy
{
    public class AlertContext
    {
        public Authorization Authorization { get; set; }
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

    public class Properties
    {
        public string IsComplianceCheck { get; set; }
        public string ResourceLocation { get; set; }
        public string Ancestors { get; set; }
        public string Policies { get; set; }
    }
}
