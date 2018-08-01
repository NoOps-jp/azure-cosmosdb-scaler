namespace NoOpsJp.CosmosDbScaler.Strategies
{
    public class SimpleReducer
    {
        private readonly int _minimumThroughput;

        public SimpleReducer(int minimumThroughput)
        {
            _minimumThroughput = minimumThroughput;
        }

        // TODO: rename better method name
        public ScaleRequest GetReductionThroughput(string databaseId, string collectionId)
        {
            return new ScaleRequest(databaseId, collectionId, _minimumThroughput);
        }
    }
}