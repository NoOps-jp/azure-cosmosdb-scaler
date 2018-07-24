using System.Collections.Concurrent;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class ThroughputAnalyzer<TStrategy> : IThroughputAnalyzer where TStrategy : IScaleStrategy, new()
    {
        // CosmosDB の RU がくる
        public void TrackRequestCharge(string collectionId, double requestCharge)
        {
            // CollectionId 単位で IScaleStrategy を持つ
            var strategy = _strategies.GetOrAdd(collectionId, x => new TStrategy { Scaler = new SimpleScaler(x) });

            strategy.AddRequestCharge(requestCharge);
        }

        public void TrackTooManyRequest(string collectionId)
        {
            //TODO:
        }

        private readonly ConcurrentDictionary<string, IScaleStrategy> _strategies = new ConcurrentDictionary<string, IScaleStrategy>();
    }
}