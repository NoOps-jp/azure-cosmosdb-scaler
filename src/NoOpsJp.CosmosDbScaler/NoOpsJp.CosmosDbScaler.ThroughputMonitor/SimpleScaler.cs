using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System.Threading.Tasks;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class SimpleScaler
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;
        private readonly string _collectionId;

        public SimpleScaler(DocumentClient client, string databaseId, string collectionId)
        {
            _client = client;
            _databaseId = databaseId;
            _collectionId = collectionId;
        }

        public async Task<ScaleResponse> AdjustThroughputAsync(ScaleRequest scaleRequest)
        {
            DocumentCollection collection =
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));
            
            // get current throuput
            var offersFeed = await _client.ReadOffersFeedAsync();
            var currentOffer = offersFeed.Single(o => o.ResourceLink == collection.SelfLink);

            // TODO: Insert judgment on whether to actually execute Scale

            // change throuput
            var replaced = await _client.ReplaceOfferAsync(new OfferV2(currentOffer, scaleRequest.TargetThroughput));
            var newOffer = (OfferV2)replaced;

            // return response
            var response = new ScaleResponse
            {
                IsAccepted = true,
                AdjustedThroughput = newOffer.Content.OfferThroughput
            };
            return response;
        }
    }
}
