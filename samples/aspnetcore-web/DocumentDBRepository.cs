using Microsoft.Extensions.Options;

using NoOpsJp.CosmosDbScaler.ThroughputMonitor;

namespace todo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    
    public class DocumentDBRepository<T> : IDocumentDBRepository<T> where T : class
    {
        private StreamlinedDocumentClient client;
        private DocumentDBOptions _options;

        public DocumentDBRepository(IOptions<DocumentDBOptions> options, IThroughputAnalyzer throughputAnalyzer)
        {
            _options = options.Value;
            this.client = new StreamlinedDocumentClient(new Uri(_options.AccountEndpoint), _options.AccountKeys, _options.Database, throughputAnalyzer);
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                return await client.ReadDocumentAsync<T>(_options.Collection, id);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            var query = client.CreateDocumentQuery<T>(_options.Collection, new FeedOptions { MaxItemCount = -1 })
                              .Where(predicate)
                              .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await client.ExecuteQueryAsync(query));
            }

            return results;
        }

        public async Task CreateItemAsync(T item)
        {
            await client.CreateDocumentAsync(_options.Collection, item);
        }

        public async Task UpdateItemAsync(string id, T item)
        {
            await client.ReplaceDocumentAsync(_options.Collection, id, item);
        }

        public async Task DeleteItemAsync(string id)
        {
            await client.DeleteDocumentAsync(_options.Collection, id);
        }
    }

    public class DocumentDBOptions
    {
        public string AccountEndpoint { get; set; }
        public string AccountKeys { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
    }
}