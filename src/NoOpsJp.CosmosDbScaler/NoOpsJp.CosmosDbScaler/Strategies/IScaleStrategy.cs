using NoOpsJp.CosmosDbScaler.Scalers;

namespace NoOpsJp.CosmosDbScaler.Strategies
{
    public interface IScaleStrategy
    {
        SimpleScaler Scaler { get; set; }
        void AddRequestCharge(double requestCharge);
        void AddTooManyRequest();
    }
}