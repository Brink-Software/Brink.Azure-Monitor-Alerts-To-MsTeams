using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Auth;

namespace AppInsightsToTeams
{
    public class AppInsightsToTeams
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private IConfigurationRoot _config;
        private ILogger _log;

        [FunctionName("AppInsightsToTeams")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log, ExecutionContext context)
        {
            _log = log;

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));

            var _builder = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KeyVaultUrl")))
                _builder.AddAzureKeyVault(Environment.GetEnvironmentVariable("KeyVaultUrl"), keyVaultClient, new DefaultKeyVaultSecretManager());

            _config = _builder
                .AddEnvironmentVariables()
                .Build();

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var alert = JsonConvert.DeserializeObject<dynamic>(requestBody);
            var query = (string)alert.data.alertContext.SearchQuery;
            var queryStart = DateTime.ParseExact((string)alert.data.alertContext.SearchIntervalStartTimeUtc, @"M\/d\/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None);
            var queryEnd = DateTime.ParseExact((string)alert.data.alertContext.SearchIntervalEndtimeUtc, @"M\/d\/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None);

            var formattedStart = queryStart.ToString("yyyy-MM-dd HH:mm:ss");
            var formattedEnd = queryEnd.ToString("yyyy-MM-dd HH:mm:ss");

            var result = await FetchApplicationInsightsQueryResultsAsync(formattedStart, formattedEnd, query.Replace("\n", string.Empty));
            await PostApplicationInsightsQueryResultsToTeamsAsync(result, alert, formattedStart, formattedEnd);

            return new OkResult();
        }

        private async Task<dynamic> FetchApplicationInsightsQueryResultsAsync(string formattedStart, string formattedEnd, dynamic query)
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _config["ApplicationInsightsApiKey"]);

            var getUrl = $"https://api.applicationinsights.io/v1/apps/{_config["ApplicationInsightsAppId"]}/query?timespan={formattedStart}/{formattedEnd}&query={query}";

            _log.LogInformation($"Attempting to get data from {getUrl}");

            var rawResult = await _httpClient.GetStringAsync(getUrl);

            _log.LogInformation($"Data received: {rawResult}");

            var result = JsonConvert.DeserializeObject<dynamic>(rawResult);
            return result;
        }

        private async Task PostApplicationInsightsQueryResultsToTeamsAsync(dynamic result, dynamic alert, string formattedStart, string formattedEnd)
        {
            _httpClient.DefaultRequestHeaders.Remove("x-api-key");

            var templateUri = $"{_config["MessageCardTemplateBaseUrl"].TrimEnd('/')}/{alert.data.essentials.alertRule}.json";

            _log.LogInformation($"Using template '{templateUri}'");

            const string StorageResource = "https://storage.azure.com/";
            var azureServiceTokenProvider = new AzureServiceTokenProvider("RunAs=App;AppId=1250a484-3b89-48a9-bba7-6213ee72f3b2");
            var tokenCredential = new TokenCredential(await azureServiceTokenProvider.GetAccessTokenAsync(StorageResource));

            StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);
            var blob = new CloudBlockBlob(new Uri(templateUri), storageCredentials);
            var cardTemplate = await blob.DownloadTextAsync();

            if ((int)result.tables.Count <= 0)
                return;

            if ((int)result.tables[0].columns.Count <= 0)
                return;

            var columns = new List<string>();

            foreach (var column in result.tables[0].columns)
            {
                columns.Add((string)column.name);
            }

            foreach (var row in result.tables[0].rows)
            {
                var currentMessage = cardTemplate
                    .Replace("[[alert.data.essentials.alertRule]]", (string)alert.data.essentials.alertRule)
                    .Replace("[[alert.data.essentials.description]]", (string)alert.data.essentials.description)
                    .Replace("[[alert.data.essentials.severity]]", (string)alert.data.essentials.severity)
                    .Replace("[[alert.alertContext.LinkToSearchResults]]", (string)alert.data.alertContext.LinkToSearchResults)
                    .Replace("[[alert.alertContext.Threshold]]", (string)alert.data.alertContext.Threshold)
                    .Replace("[[alert.alertContext.Operator]]", (string)alert.data.alertContext.Operator)
                    .Replace("[[alert.alertContext.SearchIntervalDurationMin]]", (string)alert.data.alertContext.SearchIntervalDurationMin)
                    .Replace("[[alert.alertContext.SearchIntervalInMinutes]]", (string)alert.data.alertContext.SearchIntervalInMinutes)
                    .Replace("[[alert.alertContext.SearchIntervalStartTimeUtc]]", formattedStart)
                    .Replace("[[alert.alertContext.SearchIntervalEndtimeUtc]]", formattedEnd);

                foreach (var column in result.tables[0].columns)
                {
                    currentMessage = currentMessage
                        .Replace($"[[searchResult.{(string)column.name}]]", (string)row[columns.IndexOf((string)column.name)]);
                }

                _log.LogInformation($"Sending data: {currentMessage}");

                await _httpClient.PostAsync(_config["TeamsWebhookUrl"], new StringContent(currentMessage, Encoding.UTF8, "application/json"));
            }
        }
    }
}