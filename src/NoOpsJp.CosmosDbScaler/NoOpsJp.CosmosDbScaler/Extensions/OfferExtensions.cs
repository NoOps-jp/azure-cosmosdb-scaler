using Microsoft.Azure.Documents;

namespace NoOpsJp.CosmosDbScaler.Extensions
{
    public static class OfferExtensions
    {
        public static int GetThroughput(this Offer offer) => ((OfferV2)offer).Content.OfferThroughput;
    }
}