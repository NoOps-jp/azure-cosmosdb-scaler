namespace NoOpsJp.CosmosDbScaler
{
    public interface IScaleController
    {
        void TrackRequestCharge(string collectionId, double requestCharge);
        void TrackTooManyRequest(string collectionId);
    }
}