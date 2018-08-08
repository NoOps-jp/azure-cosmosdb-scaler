using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace NoOpsJp.CosmosDbScaler.Clients
{
    public class StreamlinedDocumentClient
    {
        public StreamlinedDocumentClient(DocumentClient documentClient, string databaseId, IList<IRequestProcessor> requestProcessors)
        {
            _documentClient = documentClient;

            DatabaseId = databaseId;
            RequestProcessors = requestProcessors;

            RaiseInitialize();
        }

        private readonly DocumentClient _documentClient;

        public string DatabaseId { get; }

        public IList<IRequestProcessor> RequestProcessors { get; }

        public async Task<T> ReadDocumentAsync<T>(string collectionId, string documentId)
        {
            try
            {
                var response = await _documentClient.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(DatabaseId, collectionId, documentId));

                RaisePostDocumentRequest(collectionId, response);

                return response.Document;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    RaiseExceptionHandled(collectionId, ex);
                }

                throw;
            }
        }

        public async Task CreateDocumentAsync<T>(string collectionId, T document)
        {
            try
            {
                var response = await _documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collectionId), document);

                RaisePostDocumentRequest(collectionId, response);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    RaiseExceptionHandled(collectionId, ex);
                }

                throw;
            }
        }

        public async Task ReplaceDocumentAsync<T>(string collectionId, string documentId, T document)
        {
            try
            {
                var response = await _documentClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, collectionId, documentId), document);

                RaisePostDocumentRequest(collectionId, response);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    RaiseExceptionHandled(collectionId, ex);
                }

                throw;
            }
        }

        public async Task DeleteDocumentAsync(string collectionId, string documentId)
        {
            try
            {
                var response = await _documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, collectionId, documentId));

                RaisePostDocumentRequest(collectionId, response);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    RaiseExceptionHandled(collectionId, ex);
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

                RaisePostDocumentRequest("Items", null);

                return response;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    // TODO: get CollectionID dynamically
                    RaiseExceptionHandled("Items", ex);
                }

                throw;
            }
        }

        private void RaiseInitialize()
        {
            foreach (var processor in RequestProcessors)
            {
                processor.Initialize(_documentClient, DatabaseId);
            }
        }

        private void RaisePostDocumentRequest(string collectionId, ResourceResponseBase response)
        {
            foreach (var processor in RequestProcessors)
            {
                processor.PostDocumentRequest(collectionId, response);
            }
        }

        private void RaiseExceptionHandled(string collectionId, DocumentClientException exception)
        {
            foreach (var processor in RequestProcessors)
            {
                processor.ExceptionHandled(collectionId, exception);
            }
        }
    }
}
