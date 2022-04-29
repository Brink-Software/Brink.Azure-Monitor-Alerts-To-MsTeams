using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace AzureMonitorAlertToTeams.Tests
{
    public class ApplicationInsightsAlertProcessorTests
    {
        private AzureMonitorAlertToTeamFunction functionInstance;

        public ApplicationInsightsAlertProcessorTests()
        {
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(Options.DefaultName)).Returns(() => new HttpClient());

            var alertProcessorRepository = new Mock<IAlertProcessorRepository>();
            var queryResultFetcher = new Mock<IAppInsightsQueryResultFetcher>();

            var services = new ServiceCollection()
                .AddLogging(
                    builder => builder
                        .AddConsole(c => new SimpleConsoleFormatterOptions { ColorBehavior = LoggerColorBehavior.Disabled })
                )
                .BuildServiceProvider();
            var factory = services.GetService<ILoggerFactory>();

            var processor = new ApplicationInsightsAlertProcessor(factory.CreateLogger<ApplicationInsightsAlertProcessor>(), queryResultFetcher.Object);
            alertProcessorRepository.Setup(r => r.GetAlertProcessor("Application Insights")).Returns(processor);

            functionInstance = new AzureMonitorAlertToTeamFunction(
                httpClientFactory.Object,
                factory.CreateLogger<AzureMonitorAlertToTeamFunction>(),
                alertProcessorRepository.Object);
        }

        [Test]
        public async Task ShouldProcessAlert()
        {
            var alertJson = await File.ReadAllTextAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, "assets", "ApplicationInsights", "alert.json"));

            await using var configurationStream = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "assets", "ApplicationInsights", "configuration.json"));

            var (teamsMessage, _) = await functionInstance.ProcessAlertAsync(alertJson, configurationStream);

            System.Console.WriteLine(teamsMessage);

            Assert.IsTrue(teamsMessage.Contains("TEST APP"), "Application Insights processor not called correctly, or template is incorrect");
            Assert.IsTrue(teamsMessage.Contains("Alert fired for rule Exception"), "Generic alert values not parsed correctly, or template is incorrect");
        }

        [Test]
        public async Task ShouldProcessAlertWithSectionTemplate()
        {
            var alertJson = await File.ReadAllTextAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, "assets", "ApplicationInsights", "alert.json"));

            await using var configurationStream = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "assets", "ApplicationInsights", "configuration_with_section_template.json"));

            var (teamsMessage, _) = await functionInstance.ProcessAlertAsync(alertJson, configurationStream);

            System.Console.WriteLine(teamsMessage);

            Assert.IsTrue(teamsMessage.Contains("TEST APP"), "Application Insights processor not called correctly, or template is incorrect");
            Assert.IsTrue(teamsMessage.Contains("SECOND APP"), "Application Insights processor not called correctly, or section template is incorrect");
            Assert.IsTrue(teamsMessage.Contains("Alert fired for rule Exception"), "Generic alert values not parsed correctly, or template is incorrect");
        }
    }
}
