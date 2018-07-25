using System;

using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

using NoOpsJp.CosmosDbScaler;
using NoOpsJp.CosmosDbScaler.Clients;
using NoOpsJp.CosmosDbScaler.Strategies;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddStreamlinedDocumentClient(this IServiceCollection services, Action<StreamlinedDocumentClientOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<StreamlinedDocumentClientOptions>>();

                return new DocumentClient(new Uri(options.Value.AccountEndpoint), options.Value.AccountKey);
            });

            services.AddSingleton<IScaleController, ScaleController<SimpleScaleStrategy>>();

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<StreamlinedDocumentClientOptions>>();

                return new StreamlinedDocumentClient(provider.GetRequiredService<DocumentClient>(), options.Value.DatabaseId, provider.GetRequiredService<IScaleController>());
            });
        }
    }
}