using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ServiceHealth
{
    public class ServiceHealthAlertProcessor : IAlertProcessor
    {
        public ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource.ToString())
                .Replace("[[alert.alertContext.Level]]", alertContext.Level.ToString())
                .Replace("[[alert.alertContext.Status]]", alertContext.Status)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId.ToString())
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels.ToString())
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp)
                .Replace("[[alert.alertContext.Properties.Title]]", alertContext.Properties.Title)
                .Replace("[[alert.alertContext.Properties.Details]]", alertContext.Properties.Service)
                .Replace("[[alert.alertContext.Properties.Region]]", alertContext.Properties.Region)
                .Replace("[[alert.alertContext.Properties.Communication]]", alertContext.Properties.Communication)
                .Replace("[[alert.alertContext.Properties.IncidentType]]", alertContext.Properties.IncidentType)
                .Replace("[[alert.alertContext.Properties.TrackingId]]", alertContext.Properties.TrackingId)
                .Replace("[[alert.alertContext.Properties.ImpactStartTime]]", alertContext.Properties.FormattedImpactStartTime)
                .Replace("[[alert.alertContext.Properties.ImpactMitigationTime]]", alertContext.Properties.FormattedImpactMitigationTime)
                .Replace("[[alert.alertContext.Properties.ImpactedServices]]", alertContext.Properties.ImpactedServices)
                .Replace("[[alert.alertContext.Properties.ImpactedServicesTableRows]]", alertContext.Properties.ImpactedServicesTableRows)
                .Replace("[[alert.alertContext.Properties.DefaultLanguageTitle]]", alertContext.Properties.DefaultLanguageTitle)
                .Replace("[[alert.alertContext.Properties.DefaultLanguageContent]]", alertContext.Properties.DefaultLanguageContent)
                .Replace("[[alert.alertContext.Properties.Stage]]", alertContext.Properties.Stage)
                .Replace("[[alert.alertContext.Properties.CommunicationId]]", alertContext.Properties.CommunicationId)
                .Replace("[[alert.alertContext.Properties.MaintenanceId]]", alertContext.Properties.MaintenanceId)
                .Replace("[[alert.alertContext.Properties.IsHir]]", alertContext.Properties.IsHir.ToString())
                .Replace("[[alert.alertContext.Properties.Version]]", alertContext.Properties.Version)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
