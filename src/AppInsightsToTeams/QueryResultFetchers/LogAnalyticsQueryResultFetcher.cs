using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Configurations;
using AzureMonitorAlertToTeams.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureMonitorAlertToTeams.QueryResultFetchers
{
    public interface ILogAnalyticsQueryResultFetcher : IQueryResultFetcher
    {
    };

    public class LogAnalyticsQueryResultFetcher : ILogAnalyticsQueryResultFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _log;

        public LogAnalyticsQueryResultFetcher(ILogger log, IHttpClientFactory httpClientFactory)
        {
            _log = log;
            _httpClient = httpClientFactory.CreateClient();
        }

        public LogAnalyticsConfiguration Configuration { get; set; }

        public async Task<ResultSet> FetchLogQueryResultsAsync(string url)
        {
            var formData = new Dictionary<string, string>
            {
                {"client_id", Configuration.ClientId},
                {"redirect_uri", Configuration.RedirectUrl},
                {"grant_type", "client_credentials"},
                {"client_secret", Configuration.ClientSecret},
                {"resource", "https://api.loganalytics.io"}
            };

            var postResponse = await _httpClient.PostAsync($"https://login.microsoftonline.com/{Configuration.TenantId}/oauth2/token", new FormUrlEncodedContent(formData));
            var tokenData = await postResponse.Content.ReadAsStringAsync();
            if (!postResponse.IsSuccessStatusCode)
                throw new HttpRequestException(tokenData);

            var token = JsonConvert.DeserializeObject<dynamic>(tokenData);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)token.access_token);

            var rawResult = await _httpClient.GetStringAsync(url);

            _log.LogDebug($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<ResultSet>(rawResult);
            return result;
        }
    }
}