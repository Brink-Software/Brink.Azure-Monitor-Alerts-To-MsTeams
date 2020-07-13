using System;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ResourceHealth
{
    public class ResourceHealthAlertProcessor : IAlertProcessor
    {
        public ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Title]]", alertContext.Properties.Title, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Details]]", alertContext.Properties.Details, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.CurrentHealthStatus]]", alertContext.Properties.CurrentHealthStatus, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.PreviousHealthStatus]]", alertContext.Properties.PreviousHealthStatus, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Type]]", alertContext.Properties.Type, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Cause]]", alertContext.Properties.Cause, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId, StringComparison.InvariantCultureIgnoreCase);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
