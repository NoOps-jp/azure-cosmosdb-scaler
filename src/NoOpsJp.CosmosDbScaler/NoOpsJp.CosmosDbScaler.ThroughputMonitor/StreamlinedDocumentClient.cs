using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class StreamlinedDocumentClient
    {
        public StreamlinedDocumentClient(Uri serviceEndpoint, string authKeyOrResourceToken, string databaseId, IThroughputAnalyzer monitor = null)
        {
            _documentClient = new DocumentClient(serviceEndpoint, authKeyOrResourceToken);
            _databaseId = databaseId;
            _monitor = monitor;
        }

        private readonly DocumentClient _documentClient;
        private readonly string _databaseId;
        private readonly IThroughputAnalyzer _monitor;

        public async Task<T> ReadDocumentAsync<T>(string collectionId, string documentId)
        {
            try
            {
                var response = await _documentClient.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(_databaseId, collectionId, documentId));

                _monitor.TrackRequestCharge(collectionId, response.RequestCharge);

                return response.Document;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    _monitor.TrackRequestCharge(collectionId, ex.RequestCharge);
                    _monitor.TrackTooManyRequest(collectionId);
                }

                throw;
            }
        }

        public async Task CreateDocumentAsync<T>(string collectionId, T document)
        {
            try
            {
                var response = await _documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), document);

                _monitor.TrackRequestCharge(collectionId, response.RequestCharge);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    _monitor.TrackRequestCharge(collectionId, ex.RequestCharge);
                    _monitor.TrackTooManyRequest(collectionId);
                }

                throw;
            }
        }

        public async Task ReplaceDocumentAsync<T>(string collectionId, string documentId, T document)
        {
            try
            {
                var response = await _documentClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, collectionId, documentId), document);

                _monitor.TrackRequestCharge(collectionId, response.RequestCharge);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    _monitor.TrackRequestCharge(collectionId, ex.RequestCharge);
                    _monitor.TrackTooManyRequest(collectionId);
                }

                throw;
            }
        }

        public async Task DeleteDocumentAsync(string collectionId, string documentId)
        {
            try
            {
                var response = await _documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, collectionId, documentId));

                _monitor.TrackRequestCharge(collectionId, response.RequestCharge);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    _monitor.TrackRequestCharge(collectionId, ex.RequestCharge);
                    _monitor.TrackTooManyRequest(collectionId);
                }

                throw;
            }
        }

        public IOrderedQueryable<T> CreateDocumentQuery<T>(string collectionId, FeedOptions feedOptions = null)
        {
            return _documentClient.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), feedOptions);
        }

        public async Task<FeedResponse<T>> ExecuteQueryAsync<T>(IDocumentQuery<T> documentQuery)
        {
            try
            {
                var response = await documentQuery.ExecuteNextAsync<T>();

                // TODO: CollectionId の取り方
                _monitor.TrackRequestCharge("", response.RequestCharge);

                return response;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    _monitor.TrackRequestCharge("", ex.RequestCharge);
                    _monitor.TrackTooManyRequest("");
                }

                throw;
            }
        }
    }
}
