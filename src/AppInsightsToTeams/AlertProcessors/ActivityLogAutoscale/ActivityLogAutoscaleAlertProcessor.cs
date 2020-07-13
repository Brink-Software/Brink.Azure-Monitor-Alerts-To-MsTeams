using System;
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
                .Replace("[[alert.alertContext.OperationName]]", alertContext.OperationName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.OperationId]]", alertContext.OperationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventSource]]", alertContext.EventSource, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Level]]", alertContext.Level, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Status]]", alertContext.Status, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.CorrelationId]]", alertContext.CorrelationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Caller]]", alertContext.Caller, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Claims]]", alertContext.Claims, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Channels]]", alertContext.Channels, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.ActiveAutoscaleProfile]]", alertContext.Properties.ActiveAutoscaleProfile, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.Description]]", alertContext.Properties.Description, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.LastScaleActionTime]]", alertContext.Properties.LastScaleActionTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.ResourceName]]", alertContext.Properties.ResourceName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.NewInstancesCount]]", alertContext.Properties.NewInstancesCount.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.Properties.OldInstancesCount]]", alertContext.Properties.OldInstancesCount.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[alert.alertContext.EventDataId]]", alertContext.EventDataId, StringComparison.InvariantCultureIgnoreCase);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}