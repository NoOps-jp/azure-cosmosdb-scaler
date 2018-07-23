using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class StreamlinedDocumentClient
    {
        public StreamlinedDocumentClient(Uri serviceEndpoint, string authKeyOrResourceToken, string databaseId)
        {
            _documentClient = new DocumentClient(serviceEndpoint, authKeyOrResourceToken);

            _databaseId = databaseId;
        }

        private readonly DocumentClient _documentClient;
        private readonly string _databaseId;

        public async Task<T> ReadDocumentAsync<T>(string collectionId, string documentId)
        {
            try
            {
                var response = await _documentClient.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(_databaseId, collectionId, documentId));

                // TODO: RU を送る
                //response.RequestCharge;

                return response.Document;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != null && (int)ex.StatusCode == 429)
                {
                    // TODO: RU を送る
                    //ex.RequestCharge;
                }

                throw;
            }
        }

        public async Task CreateDocumentAsync<T>(string collectionId, T document)
        {
            try
            {
                var response = await _documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), document);
            }
            catch (DocumentClientException ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(IDocumentQuery<T> documentQuery)
        {
            var response = await documentQuery.ExecuteNextAsync<T>();

            //response.RequestCharge;

            return response;
        }
    }
}
