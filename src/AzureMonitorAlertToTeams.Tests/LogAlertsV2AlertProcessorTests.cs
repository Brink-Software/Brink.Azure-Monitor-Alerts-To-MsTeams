using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights;
using AzureMonitorAlertToTeams.AlertProcessors.LogAlertsV2;
using AzureMonitorAlertToTeams.Models;
using AzureMonitorAlertToTeams.QueryResultFetchers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace AzureMonitorAlertToTeams.Tests
{
    public class LogAlertsV2AlertProcessorTests
    {
        [Test]
        public async Task ShouldProcessAlert()
        {
            const string appName = "TEST APP";

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(Options.DefaultName)).Returns(() => new HttpClient());

            var alertProcessorRepository = new Mock<IAlertProcessorRepository>();
            var queryResultFetcher = new Mock<IAppInsightsQueryResultFetcher>();
            queryResultFetcher.Setup((f) => f.FetchLogQueryResultsAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.FromResult(new ResultSet
            {
                Tables = new[]
                {
                    new ResultSetTable
                    {
                        Columns = new[]
                        {
                            new ResultSetColumn
                            {
                                Name = "appType",
                                Type = string.Empty
                            }
                        },
                        Name = "table",
                        Rows = new[]
                        {
                            new []
                            {
                                appName
                            }
                        }
                    }
                }
            }));

            var queryResultFetcherFabric = new Mock<IQueryResultFetcherFabric>();
            queryResultFetcherFabric.Setup(r => r.CreateQueryResultFetcher(It.Is<string>(s => s.Contains("api.applicationinsights.io")))).Returns(queryResultFetcher.Object);

            var processor = new LogAlertsV2AlertProcessor(queryResultFetcherFabric.Object);
            alertProcessorRepository.Setup(r => r.GetAlertProcessor("Log Alerts V2")).Returns(processor);

            var functionInstance = new AzureMonitorAlertToTeamFunction(
                httpClientFactory.Object,
                new NullLogger<AzureMonitorAlertToTeamFunction>(),
                alertProcessorRepository.Object);

            var alertJson = await File.ReadAllTextAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, @"assets\LogAlertsV2\alert.json"));

            await using var configurationStream = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, @"assets\LogAlertsV2\configuration.json"));

            var (teamsMessage, _) = await functionInstance.ProcessAlertAsync(alertJson, configurationStream);

            Assert.IsTrue(teamsMessage.Contains(appName), "Log Alerts V2 processor not called correctly, or template is incorrect");
            Assert.IsTrue(teamsMessage.Contains("Alert fired for rule ExceptionMonitoring"), "Generic alert values not parsed correctly, or template is incorrect");
        }
    }
}