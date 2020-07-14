using System;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.Metric
{
    public class MetricAlertProcessor : IAlertProcessor
    {
        public ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration,  Alert alert)
        {
            var alertContext = JsonConvert.DeserializeObject<AlertContext>(alert.Data.AlertContext.ToString());

            teamsMessageTemplate = teamsMessageTemplate
                .Replace("[[$.data.alertContext.ConditionType]]", alertContext.ConditionType, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Condition.WindowSize]]", alertContext.Condition.WindowSize, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Condition.WindowStartTime]]", alertContext.Condition.FormattedWindowStartTime, StringComparison.InvariantCultureIgnoreCase)
                .Replace("[[$.data.alertContext.Condition.WindowEndTime]]", alertContext.Condition.FormattedWindowEndTime, StringComparison.InvariantCultureIgnoreCase);

            foreach (var allOf in alertContext.Condition.AllOf)
            {
                var index = alertContext.Condition.AllOf.IndexOf(allOf) + 1;
                teamsMessageTemplate = teamsMessageTemplate
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].MetricName]]", allOf.MetricName, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].MetricNamespace]]", allOf.MetricNamespace, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Operator]]", allOf.Operator, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Threshold]]", allOf.Threshold.ToString(), StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].TimeAggregation]]", allOf.TimeAggregation, StringComparison.InvariantCultureIgnoreCase)
                    .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].MetricValue]]", allOf.MetricValue.ToString(), StringComparison.InvariantCultureIgnoreCase);

                foreach (var dimension in allOf.Dimensions)
                {
                    var dimensionIndex = alertContext.Condition.AllOf.IndexOf(allOf) + 1;
                    teamsMessageTemplate = teamsMessageTemplate
                        .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Dimension[{dimensionIndex}].Name]]", dimension.Name, StringComparison.InvariantCultureIgnoreCase)
                        .Replace($"[[$.data.alertContext.Condition.AllOf[{index}].Dimension[{dimensionIndex}].Value]]", dimension.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
                }
            }
            
            return new ValueTask<string>(teamsMessageTemplate);
        }
    }
}
