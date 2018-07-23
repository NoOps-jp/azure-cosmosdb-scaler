using System;
using System.Collections.Generic;
using System.Text;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class ScaleRequest
    {
        public int TargetThroughput { get; set; }
        public string CollectionId { get; set; }
        public string DatabaseId { get; set; }

    }
}
