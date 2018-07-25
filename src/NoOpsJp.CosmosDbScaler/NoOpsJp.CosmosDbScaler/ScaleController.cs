using Microsoft.Azure.Documents;
using NoOpsJp.CosmosDbScaler.Strategies;
using System.Collections.Concurrent;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using NoOpsJp.CosmosDbScaler.Clients;

namespace NoOpsJp.CosmosDbScaler
{
    public class ScaleController<TStrategy> : IScaleController where TStrategy : IScaleStrategy<double>, new()
    {
        private readonly IDocumentClient _client;
        private readonly string _databaseId;
        private readonly ConcurrentDictionary<string, IScaleStrategy<double>> _strategies
            = new ConcurrentDictionary<string, IScaleStrategy<double>>();


        public ScaleController(DocumentClient client, IOptions<StreamlinedDocumentClientOptions> options)
        {
            _client = client;
            _databaseId = options.Value.DatabaseId;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="requestCharge"></param>
        public void TrackRequestCharge(string collectionId, double requestCharge)
        {
            // CollectionId 単位で IScaleStrategy を持つ
            var strategy = _strategies.GetOrAdd(collectionId,SimpleScaleStrategy.Create(_client, _databaseId, collectionId));

            strategy.AddRequestCharge(requestCharge);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collectionId"></param>
        public void TrackTooManyRequest(string collectionId)
        {
            // CollectionId 単位で IScaleStrategy を持つ
            var strategy = _strategies.GetOrAdd(collectionId, x => new TStrategy());
        }

    }
}