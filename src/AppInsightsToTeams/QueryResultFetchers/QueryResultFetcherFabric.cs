using System;
using AzureMonitorAlertToTeams.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace AzureMonitorAlertToTeams.QueryResultFetchers
{
    public interface IQueryResultFetcherFabric
    {
        IQueryResultFetcher CreateQueryResultFetcher(ApplicationInsightsConfiguration configuration);
        IQueryResultFetcher CreateQueryResultFetcher(LogAnalyticsConfiguration configuration);
    }

    public class QueryResultFetcherFabric : IQueryResultFetcherFabric
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryResultFetcherFabric(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IQueryResultFetcher CreateQueryResultFetcher(ApplicationInsightsConfiguration configuration)
        {
            var service = _serviceProvider.GetService<AppInsightsQueryResultFetcher>();
            service.Configuration = configuration;

            return service;
        }

        public IQueryResultFetcher CreateQueryResultFetcher(LogAnalyticsConfiguration configuration)
        {
            var service = _serviceProvider.GetService<LogAnalyticsQueryResultFetcher>();
            service.Configuration = configuration;

            return service;
        }
    }
}