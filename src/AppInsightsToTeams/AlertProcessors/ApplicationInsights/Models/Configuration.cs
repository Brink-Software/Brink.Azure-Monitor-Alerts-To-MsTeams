using System;

namespace AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights.Models
{
    public class Configuration
    {
        public Guid Id { get; set; }
        public string ApiKey { get; set; }
    }
}