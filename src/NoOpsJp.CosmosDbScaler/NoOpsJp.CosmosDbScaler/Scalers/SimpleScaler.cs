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

            if (NeedScale())
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


        internal virtual bool NeedScale()
        {
            // TODO: Insert judgment on whether to actually execute Scale
            return true;

        }



        private async Task<Offer> GetCurrentOfferAsync()
        {
            DocumentCollection collection = await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));

            // get current throuput
            var currentOffer = _client.CreateOfferQuery()
                .AsEnumerable()
                .Single(o => o.ResourceLink == collection.SelfLink);
            return currentOffer;
        }
    }
}
