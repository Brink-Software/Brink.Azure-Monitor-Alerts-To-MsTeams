using System;
using Newtonsoft.Json.Linq;

namespace AzureMonitorAlertToTeams.Models
{
    public class Alert
    {
        public string SchemaId { get; set; }
        public Data Data { get; set; }
    }

    public class Data
    {
        public Essentials Essentials { get; set; }
        public JObject AlertContext { get; set; }
    }

    public class Essentials
    {
        public string AlertId { get; set; }
        public string AlertRule { get; set; }
        public string Severity { get; set; }
        public string SignalType { get; set; }
        public string MonitorCondition { get; set; }
        public string MonitoringService { get; set; }
        public string[] AlertTargetIDs { get; set; }
        public Guid? OriginAlertId { get; set; }
        public DateTimeOffset? FiredDateTime { get; set; }
        public string Description { get; set; }
        public string EssentialsVersion { get; set; }
        public string AlertContextVersion { get; set; }
        public string FormattedFiredDateTime => FiredDateTime?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    
}
