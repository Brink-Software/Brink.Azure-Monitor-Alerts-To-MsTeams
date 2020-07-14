using System;

namespace AzureMonitorAlertToTeams.AlertProcessors.ActivityLogSecurity
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
        public string ThreatStatus { get; set; }
        public string Category { get; set; }
        public string ThreatId { get; set; }
        public string FilePath { get; set; }
        public string ProtectionType { get; set; }
        public string ActionTaken { get; set; }
        public string ResourceType { get; set; }
        public string Severity { get; set; }
        public string CompromisedEntity { get; set; }
        public string RemediationSteps { get; set; }
        public string AttackedResourceType { get; set; }
    }
}
