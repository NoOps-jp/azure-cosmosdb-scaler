using NoOpsJp.CosmosDbScaler.Scalers;

namespace NoOpsJp.CosmosDbScaler.Strategies
{
    public interface IScaleStrategy<in TContext>
    {
        SimpleScaler Scaler { get; set; }
        void AddRequestCharge(TContext context);
    }
}