using NoOpsJp.CosmosDbScaler.Scalers;
using System.Threading.Tasks;

namespace NoOpsJp.CosmosDbScaler.Strategies
{
    public interface IScaleStrategy<in TContext>
    {
        SimpleScaler Scaler { get; set; }

        void AddRequestCharge(TContext context);
    }
}