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
                .Replace("[[$.data.alertContext.Authorization.Action]]", alertContext.Authorization.Action, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Authorization.Scope]]", alertContext.Authorization.Scope, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.OperationName]]", alertContext.OperationName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.OperationId]]", alertContext.OperationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventSource]]", alertContext.EventSource, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Level]]", alertContext.Level, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Status]]", alertContext.Status, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SubStatus]]", alertContext.SubStatus, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.CorrelationId]]", alertContext.CorrelationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Caller]]", alertContext.Caller, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Claims]]", alertContext.Claims, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Channels]]", alertContext.Channels, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventDataId]]", alertContext.EventDataId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Ancestors]]", alertContext.Properties.Ancestors, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.IsComplianceCheck]]", alertContext.Properties.IsComplianceCheck, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Policies]]", alertContext.Properties.Policies, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.ResourceLocation]]", alertContext.Properties.ResourceLocation, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SubStatus]]", alertContext.SubStatus, StringComparison.InvariantCultureIgnoreCase);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
