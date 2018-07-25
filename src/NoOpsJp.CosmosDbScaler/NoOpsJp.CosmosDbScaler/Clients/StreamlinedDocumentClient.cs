using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace NoOpsJp.CosmosDbScaler.Clients
{
    public class StreamlinedDocumentClient
    {
        public StreamlinedDocumentClient(DocumentClient documentClient, string databaseId, IScaleController scaleController)
        {
            _documentClient = documentClient;
            _scaleController = scaleController;

            DatabaseId = databaseId;
        }

        private readonly DocumentClient _documentClient;
        private readonly IScaleController _scaleController;

        public string DatabaseId { get; }

        public async Task<T> ReadDocumentAsync<T>(string collectionId, string documentId)
        {
            try
            {
                var response = await _documentClient.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(DatabaseId, collectionId, documentId));

                _scaleController.TrackRequestCharge(collectionId, response.RequestCharge);

                return response.Document;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    // TODO: 429 support
                    var requiredCharge = ex.RequestCharge * 2;
                    _scaleController.TrackRequestCharge(collectionId, requiredCharge);
                }

                throw;
            }
        }

        public async Task CreateDocumentAsync<T>(string collectionId, T document)
        {
            try
            {
                var response = await _documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collectionId), document);

                _scaleController.TrackRequestCharge(collectionId, response.RequestCharge);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    // TODO: 429 support
                    var requiredCharge = ex.RequestCharge * 2;
                    _scaleController.TrackRequestCharge(collectionId, requiredCharge);
                }

                throw;
            }
        }

        public async Task ReplaceDocumentAsync<T>(string collectionId, string documentId, T document)
        {
            try
            {
                var response = await _documentClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, collectionId, documentId), document);

                _scaleController.TrackRequestCharge(collectionId, response.RequestCharge);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    // TODO: 429 support
                    var requiredCharge = ex.RequestCharge * 2;
                    _scaleController.TrackRequestCharge(collectionId, requiredCharge);
                }

                throw;
            }
        }

        public async Task DeleteDocumentAsync(string collectionId, string documentId)
        {
            try
            {
                var response = await _documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, collectionId, documentId));

                _scaleController.TrackRequestCharge(collectionId, response.RequestCharge);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    _scaleController.TrackRequestCharge(collectionId, ex.RequestCharge);
                    _scaleController.TrackTooManyRequest(collectionId);
                }

                throw;
            }
        }

        public IOrderedQueryable<T> CreateDocumentQuery<T>(string collectionId, FeedOptions feedOptions = null)
        {
            return _documentClient.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(DatabaseId, collectionId), feedOptions);
        }

        public async Task<FeedResponse<T>> ExecuteQueryAsync<T>(IDocumentQuery<T> documentQuery)
        {
            try
            {
                var response = await documentQuery.ExecuteNextAsync<T>();

                // TODO: CollectionId の取り方
                _scaleController.TrackRequestCharge("Items", response.RequestCharge);

                return response;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    // TODO: 429 support
                    // TODO: CollectionId の取り方
                    var requiredCharge = ex.RequestCharge * 2;
                    _scaleController.TrackRequestCharge("Items", requiredCharge);
                }

                throw;
            }
        }
    }
}
