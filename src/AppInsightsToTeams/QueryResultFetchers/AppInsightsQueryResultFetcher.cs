using System.Net.Http;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Configurations;
using AzureMonitorAlertToTeams.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.QueryResultFetchers
{
    public interface IAppInsightsQueryResultFetcher : IQueryResultFetcher
    {
    };

    public class AppInsightsQueryResultFetcher : IAppInsightsQueryResultFetcher
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        private readonly ILogger _log;

        public AppInsightsQueryResultFetcher(ILogger log, IHttpClientFactory httpClientFactory)
        {
            _log = log;
            _httpClientFactory = httpClientFactory;
        }

        public ApplicationInsightsConfiguration Configuration { get; set; }

        public async Task<ResultSet> FetchLogQueryResultsAsync(string url)
        {
            var client = _httpClient ?? CreateAndSetClient();

            var rawResult = await client.GetStringAsync(url);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;

            HttpClient CreateAndSetClient()
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", Configuration.ApiKey);

                return _httpClient;
            }
        }
    }
}