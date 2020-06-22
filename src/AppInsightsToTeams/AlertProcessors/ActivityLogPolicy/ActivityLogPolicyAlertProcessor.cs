using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ActivityLogPolicy
{
    public class ActivityLogPolicyAlertProcessor : IAlertProcessor
    {
        public ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.Authorization.Action]]", alertContext.Authorization.Action)
                .Replace("[[alert.alertContext.Authorization.Scope]]", alertContext.Authorization.Scope)
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status)
                .Replace("[[alert.alertContext.SubStatus]]", alertContext.SubStatus)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId)
                .Replace("[[alert.alertContext.Caller]]", alertContext.Caller)
                .Replace("[[alert.alertContext.Claims]]", alertContext.Claims)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId)
                .Replace("[[alert.alertContext.Properties.Ancestors]]", alertContext.Properties.Ancestors)
                .Replace("[[alert.alertContext.Properties.IsComplianceCheck]]", alertContext.Properties.IsComplianceCheck)
                .Replace("[[alert.alertContext.Properties.Policies]]", alertContext.Properties.Policies)
                .Replace("[[alert.alertContext.Properties.ResourceLocation]]", alertContext.Properties.ResourceLocation)
                .Replace("[[alert.alertContext.SubStatus]]", alertContext.SubStatus);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
