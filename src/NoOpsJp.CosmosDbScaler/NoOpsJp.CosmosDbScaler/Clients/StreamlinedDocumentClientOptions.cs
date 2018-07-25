namespace NoOpsJp.CosmosDbScaler.Clients
{
    public class StreamlinedDocumentClientOptions
    {
        public string AccountEndpoint { get; set; }
        public string AccountKey { get; set; }
        public string DatabaseId { get; set; }
        public string ConnectionMode { get; set; }
        public string ConnectionProtocol { get; set; }
        public string ConsistencyLevel { get; set; }
        public string[] PreferredLocations { get; set; }

    }
}
