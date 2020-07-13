using System;
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
                .Replace("[[alert.alertContext.Authorization.Action]]", alertContext.Authorization.Action, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Authorization.Scope]]", alertContext.Authorization.Scope, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SubStatus]]", alertContext.SubStatus, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Caller]]", alertContext.Caller, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Claims]]", alertContext.Claims, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Ancestors]]", alertContext.Properties.Ancestors, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.IsComplianceCheck]]", alertContext.Properties.IsComplianceCheck, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Policies]]", alertContext.Properties.Policies, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.ResourceLocation]]", alertContext.Properties.ResourceLocation, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SubStatus]]", alertContext.SubStatus, StringComparison.InvariantCultureIgnoreCase);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
