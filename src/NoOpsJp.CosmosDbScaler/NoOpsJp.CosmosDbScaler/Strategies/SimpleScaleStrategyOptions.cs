using System;

namespace NoOpsJp.CosmosDbScaler.Strategies
{
    public class SimpleScaleStrategyOptions
    {
        public TimeSpan TrendDuration { get; internal set; }
        public TimeSpan TrendInterval { get; internal set; }
    }
}
