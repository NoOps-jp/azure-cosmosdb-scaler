using System;
using System.Collections.Generic;
using System.Text;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class ScaleResponse
    {
        public bool IsAccepted { get; set; }
        public int AdjustedThroughput { get; set; }
    }
}
