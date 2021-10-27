using System;
using AzureMonitorAlertToTeams.AlertProcessors.ApplicationInsights;
using AzureMonitorAlertToTeams.AlertProcessors.LogAnalytics;
using Microsoft.Extensions.DependencyInjection;

namespace AzureMonitorAlertToTeams.QueryResultFetchers
{
    public interface IQueryResultFetcherFabric
    {
        IQueryResultFetcher CreateQueryResultFetcher(string apiUrl);
    }

    public class QueryResultFetcherFabric : IQueryResultFetcherFabric
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryResultFetcherFabric(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IQueryResultFetcher CreateQueryResultFetcher(string apiUrl)
        {
            if(apiUrl.Contains("api.applicationinsights.io", StringComparison.InvariantCultureIgnoreCase))
                return _serviceProvider.GetService<IAppInsightsQueryResultFetcher>();
            
            return _serviceProvider.GetService<ILogAnalyticsQueryResultFetcher>();
        }
    }
}