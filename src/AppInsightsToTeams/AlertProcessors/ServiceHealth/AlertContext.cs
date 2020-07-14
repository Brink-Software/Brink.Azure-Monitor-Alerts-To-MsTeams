using System;

namespace AzureMonitorAlertToTeams.AlertProcessors.ServiceHealth
{
    public class AlertContext
    {
        public object Authorization { get; set; }
        public long? Channels { get; set; }
        public object Claims { get; set; }
        public object Caller { get; set; }
        public Guid? CorrelationId { get; set; }
        public long? EventSource { get; set; }
        public DateTimeOffset? EventTimestamp { get; set; }
        public object HttpRequest { get; set; }
        public string EventDataId { get; set; }
        public long? Level { get; set; }
        public string OperationName { get; set; }
        public string OperationId { get; set; }
        public Properties Properties { get; set; }
        public string Status { get; set; }
        public object SubStatus { get; set; }
        public DateTimeOffset? SubmissionTimestamp { get; set; }
        public object ResourceType { get; set; }
        public string FormattedEventTimestamp => EventTimestamp?.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedSubmissionTimestamp => SubmissionTimestamp?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class Properties
    {
        public string Title { get; set; }
        public string Service { get; set; }
        public string Region { get; set; }
        public string Communication { get; set; }
        public string IncidentType { get; set; }
        public string TrackingId { get; set; }
        public DateTimeOffset? ImpactStartTime { get; set; }
        public DateTimeOffset? ImpactMitigationTime { get; set; }
        public string ImpactedServices { get; set; }
        public string ImpactedServicesTableRows { get; set; }
        public string DefaultLanguageTitle { get; set; }
        public string DefaultLanguageContent { get; set; }
        public string Stage { get; set; }
        public string CommunicationId { get; set; }
        public string MaintenanceId { get; set; }
        public bool? IsHir { get; set; }
        public string Version { get; set; }
        public string FormattedImpactStartTime => ImpactStartTime?.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedImpactMitigationTime => ImpactMitigationTime?.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
