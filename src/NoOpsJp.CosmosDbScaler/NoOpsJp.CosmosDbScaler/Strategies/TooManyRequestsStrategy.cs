using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoOpsJp.CosmosDbScaler.Scalers;

namespace NoOpsJp.CosmosDbScaler.Strategies
{
    class TooManyRequestsStrategy : IScaleStrategy<double>
    {
        private List<long> _tooManyRequestsHistory = new List<long>();
        // TODO make configurable
        private int _monitorDuration = -5;
        private int _tooManyRequestsThreshold = 4;
        private int _scaleFactor = 2;

        public SimpleScaler Scaler { get ; set ; }

        public void AddRequestCharge(double context)
        { 
            var eventTime = DateTime.Now;
            long tooManyRequestsEvent = eventTime.Ticks;

            _tooManyRequestsHistory.Add(tooManyRequestsEvent);

            _tooManyRequestsHistory = _tooManyRequestsHistory
                .Where(record => record >= eventTime.AddSeconds(_monitorDuration).Ticks)
                .ToList();

            if (_tooManyRequestsHistory.Count >= _tooManyRequestsThreshold)
            {
                var currentThroughput = Scaler.GetCurrentThroughputAsync().Result;
                // TODO make dynamic 
                var scaleRequest = new ScaleRequest("ToDoList", "Items", currentThroughput * _scaleFactor );
                var result = Scaler.AdjustThroughputAsync(scaleRequest).Result;
            }
        }
    }
}
