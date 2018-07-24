namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public interface IScaleStrategy
    {
        SimpleScaler Scaler { get; set; }
        void AddRequestCharge(double requestCharge);
    }
}