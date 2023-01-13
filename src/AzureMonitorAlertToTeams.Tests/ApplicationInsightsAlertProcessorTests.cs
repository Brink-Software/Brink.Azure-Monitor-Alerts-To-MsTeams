using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights;
using AzureMonitorAlertToTeams.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace AzureMonitorAlertToTeams.Tests
{
    public class ApplicationInsightsAlertProcessorTests
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

            var processor = new ApplicationInsightsAlertProcessor(queryResultFetcher.Object);
            alertProcessorRepository.Setup(r => r.GetAlertProcessor("Application Insights")).Returns(processor);

            var functionInstance = new AzureMonitorAlertToTeamFunction(
                httpClientFactory.Object,
                new NullLogger<AzureMonitorAlertToTeamFunction>(),
                alertProcessorRepository.Object);

            var alertJson = await File.ReadAllTextAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, @"assets\ApplicationInsights\alert.json"));

            await using var configurationStream = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, @"assets\ApplicationInsights\configuration.json"));

            var (teamsMessage, _) = await functionInstance.ProcessAlertAsync(alertJson, configurationStream);

            Assert.IsTrue(teamsMessage.Contains(appName), "Application Insights processor not called correctly, or template is incorrect");
            Assert.IsTrue(teamsMessage.Contains("Alert fired for rule Exception"), "Generic alert values not parsed correctly, or template is incorrect");
        }
    }
}