using Microsoft.Azure.Documents;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public static class OfferExtensions
    {
        public static int GetThroughput(this Offer offer) => ((OfferV2)offer).Content.OfferThroughput;
    }
}