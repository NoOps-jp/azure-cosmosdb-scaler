namespace NoOpsJp.CosmosDbScaler
{
    public class ScaleRequest 
    { 
        public string DatabaseId { get; }
        public string CollectionId { get; }
        public int TargetThroughput { get; }

        public ScaleRequest(string databaseId, string collectionId, int targetThroughput)
        {
            DatabaseId = databaseId;
            CollectionId = collectionId;
            TargetThroughput = targetThroughput;
        }
    }
}
