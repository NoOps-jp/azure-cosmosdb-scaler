using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NoOpsJp.CosmosDbScaler.Extensions;

namespace NoOpsJp.CosmosDbScaler.Scalers
{
    public class SimpleScaler
    {
        private readonly IDocumentClient _client;
        private readonly string _databaseId;
        private readonly string _collectionId;

        public SimpleScaler(IDocumentClient client, string databaseId, string collectionId)
        {
            _client = client;
            _databaseId = databaseId;
            _collectionId = collectionId;
        }


        public async Task<int> GetCurrentThroughputAsync()
        {
            var currentOffer = await GetCurrentOfferAsync();
            return currentOffer.GetThroughput();
        }


        public async Task<ScaleResponse> AdjustThroughputAsync(ScaleRequest scaleRequest)
        {
            var currentOffer = await GetCurrentOfferAsync();

            if (NeedScale(scaleRequest, currentOffer))
            {
                Offer replaced = await _client.ReplaceOfferAsync(new OfferV2(currentOffer, scaleRequest.TargetThroughput));
                return new ScaleResponse
                {
                    IsAccepted = true,
                    AdjustedThroughput = replaced.GetThroughput()
                };
            }

            return new ScaleResponse()
            {
                IsAccepted = false,
                AdjustedThroughput = currentOffer.GetThroughput()
            };
        }


        internal virtual bool NeedScale(ScaleRequest scaleRequest, Offer currentOffer)
        {
            // TODO: temporary implementation
            if (currentOffer.GetThroughput() == scaleRequest.TargetThroughput)
            {
                return false;
            }

            return true;
        }


        private async Task<Offer> GetCurrentOfferAsync()
        {
            // this proccess's RU is 1
            DocumentCollection collection = await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));
            var currentOffer = _client.CreateOfferQuery()
                .AsEnumerable()
                .Single(o => o.ResourceLink == collection.SelfLink);
            return currentOffer;
        }
    }
}
