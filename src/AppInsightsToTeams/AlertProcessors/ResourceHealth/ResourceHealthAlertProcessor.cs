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
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp)
                .Replace("[[alert.alertContext.Properties.Title]]", alertContext.Properties.Title)
                .Replace("[[alert.alertContext.Properties.Details]]", alertContext.Properties.Details)
                .Replace("[[alert.alertContext.Properties.CurrentHealthStatus]]", alertContext.Properties.CurrentHealthStatus)
                .Replace("[[alert.alertContext.Properties.PreviousHealthStatus]]", alertContext.Properties.PreviousHealthStatus)
                .Replace("[[alert.alertContext.Properties.Type]]", alertContext.Properties.Type)
                .Replace("[[alert.alertContext.Properties.Cause]]", alertContext.Properties.Cause)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
