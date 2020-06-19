using Newtonsoft.Json.Linq;

namespace AzureMonitorAlertToTeams.Models
{
    public class AlertConfiguration
    {
        public JObject Context { get; set; }
        public string AlertId { get; set; }
        public string TeamsChannelConnectorWebhookUrl { get; set; }
        public string TeamsMessageTemplate { get; set; }
    }
}
