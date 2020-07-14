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
                .Replace("[[$.data.alertContext.OperationName]]", alertContext.OperationName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.OperationId]]", alertContext.OperationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventSource]]", alertContext.EventSource, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Level]]", alertContext.Level, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Status]]", alertContext.Status, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.CorrelationId]]", alertContext.CorrelationId, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Caller]]", alertContext.Caller, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Claims]]", alertContext.Claims, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Channels]]", alertContext.Channels, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventTimestamp]]", alertContext.FormattedEventTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.ActiveAutoscaleProfile]]", alertContext.Properties.ActiveAutoscaleProfile, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.Description]]", alertContext.Properties.Description, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.LastScaleActionTime]]", alertContext.Properties.LastScaleActionTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.ResourceName]]", alertContext.Properties.ResourceName, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.NewInstancesCount]]", alertContext.Properties.NewInstancesCount.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Properties.OldInstancesCount]]", alertContext.Properties.OldInstancesCount.ToString(), StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.SubmissionTimestamp]]", alertContext.FormattedSubmissionTimestamp, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.EventDataId]]", alertContext.EventDataId, StringComparison.InvariantCultureIgnoreCase);

            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}