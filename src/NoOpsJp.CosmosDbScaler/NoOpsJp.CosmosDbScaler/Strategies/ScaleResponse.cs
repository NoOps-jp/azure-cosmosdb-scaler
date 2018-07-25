namespace NoOpsJp.CosmosDbScaler
{
    public class ScaleResponse
    {
        public bool IsAccepted { get; set; }
        public int AdjustedThroughput { get; set; }
    }
}
