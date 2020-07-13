using System;
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
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Title]]", alertContext.Properties.Title, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Details]]", alertContext.Properties.Service, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Region]]", alertContext.Properties.Region, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Communication]]", alertContext.Properties.Communication, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.IncidentType]]", alertContext.Properties.IncidentType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.TrackingId]]", alertContext.Properties.TrackingId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.ImpactStartTime]]", alertContext.Properties.FormattedImpactStartTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.ImpactMitigationTime]]", alertContext.Properties.FormattedImpactMitigationTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.ImpactedServices]]", alertContext.Properties.ImpactedServices, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.ImpactedServicesTableRows]]", alertContext.Properties.ImpactedServicesTableRows, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.DefaultLanguageTitle]]", alertContext.Properties.DefaultLanguageTitle, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.DefaultLanguageContent]]", alertContext.Properties.DefaultLanguageContent, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Stage]]", alertContext.Properties.Stage, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.CommunicationId]]", alertContext.Properties.CommunicationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.MaintenanceId]]", alertContext.Properties.MaintenanceId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.IsHir]]", alertContext.Properties.IsHir.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Version]]", alertContext.Properties.Version, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId, StringComparison.InvariantCultureIgnoreCase);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
