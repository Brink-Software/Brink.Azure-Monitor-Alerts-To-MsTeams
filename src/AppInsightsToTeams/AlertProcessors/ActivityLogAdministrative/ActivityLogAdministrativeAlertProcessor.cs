using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ActivityLogAdministrative
{
    public class ActivityLogAdministrativeAlertProcessor : IAlertProcessor
    {
        public ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.Authorization.Action]]", alertContext.Authorization.Action)
                .Replace("[[alert.alertContext.Authorization.Scope]]", alertContext.Authorization.Scope)
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId.ToString())
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status)
                .Replace("[[alert.alertContext.SubStatus]]", alertContext.SubStatus)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId.ToString())
                .Replace("[[alert.alertContext.Caller]]", alertContext.Caller.ToString())
                .Replace("[[alert.alertContext.Claims]]", alertContext.Claims)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId.ToString())
                .Replace("[[alert.alertContext.SubStatus]]", alertContext.SubStatus);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
