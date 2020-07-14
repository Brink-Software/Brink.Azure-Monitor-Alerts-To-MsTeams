namespace AzureMonitorAlertToTeams.Models
{
    public class ResultSet
    {
        public ResultSetTable[] Tables { get; set; }
    }

    public class ResultSetTable
    {
        public string Name { get; set; }
        public ResultSetColumn[] Columns { get; set; }
        public string[][] Rows { get; set; }
    }

    public class ResultSetColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
