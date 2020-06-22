using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ActivityLogSecurity
{
    public class ActivityLogSecurityAlertProcessor : IAlertProcessor
    {
        public ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId)
                .Replace("[[alert.alertContext.Properties.ThreatStatus]]", alertContext.Properties.ThreatStatus)
                .Replace("[[alert.alertContext.Properties.ActionTaken]]", alertContext.Properties.ActionTaken)
                .Replace("[[alert.alertContext.Properties.AttackedResourceType]]", alertContext.Properties.AttackedResourceType)
                .Replace("[[alert.alertContext.Properties.Category]]", alertContext.Properties.Category)
                .Replace("[[alert.alertContext.Properties.CompromisedEntity]]", alertContext.Properties.CompromisedEntity)
                .Replace("[[alert.alertContext.Properties.FilePath]]", alertContext.Properties.FilePath)
                .Replace("[[alert.alertContext.Properties.ProtectionType]]", alertContext.Properties.ProtectionType)
                .Replace("[[alert.alertContext.Properties.RemediationSteps]]", alertContext.Properties.RemediationSteps)
                .Replace("[[alert.alertContext.Properties.ResourceType]]", alertContext.Properties.ResourceType)
                .Replace("[[alert.alertContext.Properties.Severity]]", alertContext.Properties.Severity)
                .Replace("[[alert.alertContext.Properties.ThreatId]]", alertContext.Properties.ThreatId);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
