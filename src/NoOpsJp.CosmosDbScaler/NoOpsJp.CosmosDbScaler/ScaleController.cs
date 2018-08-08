using System.Collections.Concurrent;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

using NoOpsJp.CosmosDbScaler.Clients;
using NoOpsJp.CosmosDbScaler.Strategies;

namespace NoOpsJp.CosmosDbScaler
{
    public class ScaleController<TStrategy> : IRequestProcessor where TStrategy : IScaleStrategy<double>
    {
        private IDocumentClient _client;
        private string _databaseId;

        private readonly ConcurrentDictionary<string, IScaleStrategy<double>> _usageStrategy
            = new ConcurrentDictionary<string, IScaleStrategy<double>>();

        // TODO needs discussion
        private readonly ConcurrentDictionary<string, IScaleStrategy<double>> _tooManyRequestsStrategy
            = new ConcurrentDictionary<string, IScaleStrategy<double>>();


        public void Initialize(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="resourceResponse"></param>
        public void PostDocumentRequest(string collectionId, ResourceResponseBase resourceResponse)
        {
            // CollectionId 単位で IScaleStrategy を持つ
            var strategy = _usageStrategy.GetOrAdd(collectionId, SimpleScaleStrategy.Create(_client, _databaseId, collectionId));

            strategy.AddRequestCharge(resourceResponse.RequestCharge);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="exception"></param>
        public void ExceptionHandled(string collectionId, DocumentClientException exception)
        {
            // CollectionId 単位で IScaleStrategy を持つ
            var strategy = _usageStrategy.GetOrAdd(collectionId, SimpleScaleStrategy.Create(_client, _databaseId, collectionId));

            strategy.AddRequestCharge(exception.RequestCharge * 2);
            // has ScaleStrategy in each CollectionId
            //var strategy = _tooManyRequestsStrategy.GetOrAdd(collectionId, TooManyRequestsStrategy.Create(_client, _databaseId, collectionId));
            // TODO implement after API discussion
        }
    }
}