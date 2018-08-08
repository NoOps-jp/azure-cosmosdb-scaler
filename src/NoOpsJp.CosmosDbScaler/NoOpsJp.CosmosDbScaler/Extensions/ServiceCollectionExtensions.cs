using System;

using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

using NoOpsJp.CosmosDbScaler.Clients;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IStreamlinedDocumentClientBuilder AddStreamlinedDocumentClient(this IServiceCollection services, Action<StreamlinedDocumentClientOptions> setupAction)
        {
            services.Configure(setupAction);

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<StreamlinedDocumentClientOptions>>();

                return new DocumentClient(new Uri(options.Value.AccountEndpoint), options.Value.AccountKey, options.Value.ConnectionPolicy);
            });

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<StreamlinedDocumentClientOptions>>();

                return new StreamlinedDocumentClient(provider.GetRequiredService<DocumentClient>(), options.Value.DatabaseId, options.Value.RequestProcessors);
            });

            return new StreamlinedDocumentClientBuilder(services);
        }

        public static IStreamlinedDocumentClientBuilder SetConnectionPolicy(this IStreamlinedDocumentClientBuilder builder, ConnectionPolicy policy)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.Configure<StreamlinedDocumentClientOptions>(o => o.ConnectionPolicy = policy);

            return builder;
        }

        public static IStreamlinedDocumentClientBuilder SetRequestProcessors(this IStreamlinedDocumentClientBuilder builder, params IRequestProcessor[] requestProcessors)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.Configure<StreamlinedDocumentClientOptions>(o => o.RequestProcessors = requestProcessors);

            return builder;
        }

        public interface IStreamlinedDocumentClientBuilder
        {
            IServiceCollection Services { get; }
        }

        public class StreamlinedDocumentClientBuilder : IStreamlinedDocumentClientBuilder
        {
            public StreamlinedDocumentClientBuilder(IServiceCollection services)
            {
                Services = services;
            }

            public IServiceCollection Services { get; }
        }
    }
}