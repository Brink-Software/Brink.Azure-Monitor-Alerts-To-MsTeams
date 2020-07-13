using Newtonsoft.Json.Linq;

namespace AzureMonitorAlertToTeams.Models
{
    public class AlertConfiguration
    {
        public JObject Context { get; set; }
        public string AlertRule { get; set; }
        public string SubscriptionId { get; set; }
        public string TeamsChannelConnectorWebhookUrl { get; set; }
        public string TeamsMessageTemplate { get; set; }
        public string Description { get; set; }
    }
}
