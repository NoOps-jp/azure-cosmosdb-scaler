namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public interface IThroughputAnalyzer
    {
        void TrackRequestCharge(string collectionId, double requestCharge);
        void TrackTooManyRequest(string collectionId);
    }
}