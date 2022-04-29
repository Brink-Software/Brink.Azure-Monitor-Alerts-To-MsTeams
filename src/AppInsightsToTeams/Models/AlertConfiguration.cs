using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureMonitorAlertToTeams.Models
{
    public class AlertConfiguration
    {
        public JObject Context { get; set; }
        public string AlertRule { get; set; }
        public string AlertTargetID { get; set; }
        public string TeamsChannelConnectorWebhookUrl { get; set; }
        public JObject TeamsMessageTemplate { get; set; }
        public string TeamsMessageTemplateAsJson => JsonConvert.SerializeObject(TeamsMessageTemplate);
        public JObject TeamsMessageSectionTemplate { get; set; }
        public string TeamsMessageSectionTemplateAsJson => JsonConvert.SerializeObject(TeamsMessageSectionTemplate);
    }
}
