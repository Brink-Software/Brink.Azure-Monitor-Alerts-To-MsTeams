using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace AzureMonitorAlertToTeams
{
    public class TelemetryEnrichment : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version =
                typeof(TelemetryEnrichment).Assembly.GetName().Version?.ToString();
        }
    }
}