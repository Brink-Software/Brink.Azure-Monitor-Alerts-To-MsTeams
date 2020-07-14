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
                .Replace("[[$.data.alertContext.OperationName]]", alertContext.OperationName)
                .Replace("[[$.data.alertContext.OperationId]]", alertContext.OperationId)
                .Replace("[[$.data.alertContext.EventSource]]", alertContext.EventSource)
                .Replace("[[$.data.alertContext.Level]]", alertContext.Level)
                .Replace("[[$.data.alertContext.Status]]", alertContext.Status)
                .Replace("[[$.data.alertContext.CorrelationId]]", alertContext.CorrelationId)
                .Replace("[[$.data.alertContext.Channels]]", alertContext.Channels)
                .Replace("[[$.data.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp)
                .Replace("[[$.data.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp)
                .Replace("[[$.data.alertContext.EventDataId]]", alertContext.EventDataId)
                .Replace("[[$.data.alertContext.Properties.ThreatStatus]]", alertContext.Properties.ThreatStatus)
                .Replace("[[$.data.alertContext.Properties.ActionTaken]]", alertContext.Properties.ActionTaken)
                .Replace("[[$.data.alertContext.Properties.AttackedResourceType]]", alertContext.Properties.AttackedResourceType)
                .Replace("[[$.data.alertContext.Properties.Category]]", alertContext.Properties.Category)
                .Replace("[[$.data.alertContext.Properties.CompromisedEntity]]", alertContext.Properties.CompromisedEntity)
                .Replace("[[$.data.alertContext.Properties.FilePath]]", alertContext.Properties.FilePath)
                .Replace("[[$.data.alertContext.Properties.ProtectionType]]", alertContext.Properties.ProtectionType)
                .Replace("[[$.data.alertContext.Properties.RemediationSteps]]", alertContext.Properties.RemediationSteps)
                .Replace("[[$.data.alertContext.Properties.ResourceType]]", alertContext.Properties.ResourceType)
                .Replace("[[$.data.alertContext.Properties.Severity]]", alertContext.Properties.Severity)
                .Replace("[[$.data.alertContext.Properties.ThreatId]]", alertContext.Properties.ThreatId);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
