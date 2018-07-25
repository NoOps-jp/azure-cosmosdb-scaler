using System;

using Microsoft.Azure.Documents.Client;

using NoOpsJp.CosmosDbScaler.ThroughputMonitor;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddStreamlinedDocumentClient(this IServiceCollection services, Action<StreamlinedDocumentClientOptions> setupAction)
        {
            var options = new StreamlinedDocumentClientOptions();

            setupAction(options);

            services.AddSingleton(provider => new DocumentClient(new Uri(options.AccountEndpoint), options.AccountKey));
            services.AddSingleton<IThroughputAnalyzer, ThroughputAnalyzer<SimpleScaleStrategy>>();

            services.AddSingleton(provider => new StreamlinedDocumentClient(provider.GetRequiredService<DocumentClient>(), options.DatabaseId, provider.GetRequiredService<IThroughputAnalyzer>()));
        }
    }
}
