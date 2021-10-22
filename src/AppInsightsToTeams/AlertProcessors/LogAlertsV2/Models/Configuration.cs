namespace AzureMonitorAlertToTeams.AlertProcessors.LogAlertsV2.Models
{
    public class Configuration
    {
        public string ClientSecret { get; set; }
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string RedirectUrl { get; set; }
    }
}