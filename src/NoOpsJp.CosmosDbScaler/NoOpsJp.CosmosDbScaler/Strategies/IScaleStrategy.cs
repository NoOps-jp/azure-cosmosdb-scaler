using NoOpsJp.CosmosDbScaler.Scalers;
using System.Threading.Tasks;

namespace NoOpsJp.CosmosDbScaler.Strategies
{
    public interface IScaleStrategy
    {
        SimpleScaler Scaler { get; set; }
        void AddRequestCharge(double requestCharge);
    }
}