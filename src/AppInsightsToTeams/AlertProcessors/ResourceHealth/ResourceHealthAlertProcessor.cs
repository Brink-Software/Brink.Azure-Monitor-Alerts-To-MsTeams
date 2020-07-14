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
                .Replace("[[$.data.alertContext.OperationName]]", alertContext.OperationName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.OperationId]]", alertContext.OperationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventSource]]", alertContext.EventSource, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Level]]", alertContext.Level, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Status]]", alertContext.Status, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.CorrelationId]]", alertContext.CorrelationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Channels]]", alertContext.Channels, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Title]]", alertContext.Properties.Title, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Details]]", alertContext.Properties.Details, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.CurrentHealthStatus]]", alertContext.Properties.CurrentHealthStatus, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.PreviousHealthStatus]]", alertContext.Properties.PreviousHealthStatus, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Type]]", alertContext.Properties.Type, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Cause]]", alertContext.Properties.Cause, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventDataId]]", alertContext.EventDataId, StringComparison.InvariantCultureIgnoreCase);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
