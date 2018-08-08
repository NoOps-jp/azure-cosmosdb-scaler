using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NoOpsJp.CosmosDbScaler.Clients
{
    public interface IRequestProcessor
    {
        void Initialize(DocumentClient documentClient, string databaseId);
        void PostDocumentRequest(string collectionId, ResourceResponseBase resourceResponse);
        void ExceptionHandled(string collectionId, DocumentClientException exception);
    }
}
