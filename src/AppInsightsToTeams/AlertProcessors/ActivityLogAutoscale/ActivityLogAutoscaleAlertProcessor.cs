using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ActivityLogAutoscale
{
    public class ActivityLogAutoscaleAlertProcessor : IAlertProcessor
    {
        public ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration, Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId)
                .Replace("[[alert.alertContext.Caller]]", alertContext.Caller)
                .Replace("[[alert.alertContext.Claims]]", alertContext.Claims)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp)
                .Replace("[[alert.alertContext.Properties.ActiveAutoscaleProfile]]", alertContext.Properties.ActiveAutoscaleProfile)
                .Replace("[[alert.alertContext.Properties.Description]]", alertContext.Properties.Description)
                .Replace("[[alert.alertContext.Properties.LastScaleActionTime]]", alertContext.Properties.LastScaleActionTime)
                .Replace("[[alert.alertContext.Properties.ResourceName]]", alertContext.Properties.ResourceName)
                .Replace("[[alert.alertContext.Properties.NewInstancesCount]]", alertContext.Properties.NewInstancesCount.ToString())
                .Replace("[[alert.alertContext.Properties.OldInstancesCount]]", alertContext.Properties.OldInstancesCount.ToString())
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}