namespace NoOpsJp.CosmosDbScaler.Strategies
{
    public class TrendModel
    {
        public double Intercept { get; set; }
        public double Slope { get; set; }
        public int LastRecordIndex { get; set; }
    }
}
