using System;
using System.Net.Http;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Configurations;
using AzureMonitorAlertToTeams.Models;
using AzureMonitorAlertToTeams.QueryResultFetchers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights
{
    public interface IAppInsightsQueryResultFetcher : IQueryResultFetcher
    {
    };

    public class AppInsightsQueryResultFetcher : IAppInsightsQueryResultFetcher
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        private readonly ILogger _log;

        public AppInsightsQueryResultFetcher(ILogger<AppInsightsQueryResultFetcher> log, IHttpClientFactory httpClientFactory)
        {
            _log = log;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResultSet> FetchLogQueryResultsAsync(string url, string jsonConfiguration)
        {
            var configuration = JsonConvert.DeserializeObject<ApplicationInsightsConfiguration>(jsonConfiguration);

            if (configuration?.ApiKey == null)
                throw new InvalidOperationException("Cannot get ApiKey from configuration {jsonConfiguration}");

            var client = _httpClient ?? CreateAndSetClient();

            var rawResult = await client.GetStringAsync(url);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;

            HttpClient CreateAndSetClient()
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", configuration.ApiKey);

                return _httpClient;
            }
        }
    }
}