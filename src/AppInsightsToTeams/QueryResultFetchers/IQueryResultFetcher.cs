using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;

namespace AzureMonitorAlertToTeams.QueryResultFetchers
{
    public interface IQueryResultFetcher
    {
        Task<ResultSet> FetchLogQueryResultsAsync(string url);
    }
}