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
                .Replace("[[$.data.alertContext.OperationName]]", alertContext.OperationName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.OperationId]]", alertContext.OperationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventSource]]", alertContext.EventSource.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Level]]", alertContext.Level.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Status]]", alertContext.Status, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.CorrelationId]]", alertContext.CorrelationId.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Channels]]", alertContext.Channels.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Title]]", alertContext.Properties.Title, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Details]]", alertContext.Properties.Service, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Region]]", alertContext.Properties.Region, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Communication]]", alertContext.Properties.Communication, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.IncidentType]]", alertContext.Properties.IncidentType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.TrackingId]]", alertContext.Properties.TrackingId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.ImpactStartTime]]", alertContext.Properties.FormattedImpactStartTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.ImpactMitigationTime]]", alertContext.Properties.FormattedImpactMitigationTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.ImpactedServices]]", alertContext.Properties.ImpactedServices, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.ImpactedServicesTableRows]]", alertContext.Properties.ImpactedServicesTableRows, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.DefaultLanguageTitle]]", alertContext.Properties.DefaultLanguageTitle, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.DefaultLanguageContent]]", alertContext.Properties.DefaultLanguageContent, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Stage]]", alertContext.Properties.Stage, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.CommunicationId]]", alertContext.Properties.CommunicationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.MaintenanceId]]", alertContext.Properties.MaintenanceId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.IsHir]]", alertContext.Properties.IsHir.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Version]]", alertContext.Properties.Version, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventDataId]]", alertContext.EventDataId, StringComparison.InvariantCultureIgnoreCase);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
