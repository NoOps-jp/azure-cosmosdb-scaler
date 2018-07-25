using Microsoft.Azure.Documents;
using System.Collections.Concurrent;

using Microsoft.Azure.Documents.Client;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class ThroughputAnalyzer<TStrategy> : IThroughputAnalyzer where TStrategy : IScaleStrategy, new()
    {
        private readonly IDocumentClient _client;
        private readonly string _databaseId;

        public ThroughputAnalyzer(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        // CosmosDB の RU がくる
        public void TrackRequestCharge(string collectionId, double requestCharge)
        {
            // CollectionId 単位で IScaleStrategy を持つ
            var strategy = _strategies.GetOrAdd(collectionId, x => new TStrategy { Scaler = new SimpleScaler(_client, _databaseId, collectionId) });

            strategy.AddRequestCharge(requestCharge);
        }

        public void TrackTooManyRequest(string collectionId)
        {
            // CollectionId 単位で IScaleStrategy を持つ
            var strategy = _strategies.GetOrAdd(collectionId, x => new TStrategy { Scaler = new SimpleScaler(_client, _databaseId, collectionId) });

            strategy.AddTooManyRequest();
        }

        private readonly ConcurrentDictionary<string, IScaleStrategy> _strategies = new ConcurrentDictionary<string, IScaleStrategy>();
    }
}