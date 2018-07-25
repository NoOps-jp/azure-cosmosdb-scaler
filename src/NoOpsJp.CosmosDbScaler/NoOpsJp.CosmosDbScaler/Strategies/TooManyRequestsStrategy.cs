using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoOpsJp.CosmosDbScaler.Scalers;

namespace NoOpsJp.CosmosDbScaler.Strategies
{
    class TooManyRequestsStrategy : IScaleStrategy
    {
        private List<long> _tooManyRequestsHistory = new List<long>();

        public SimpleScaler Scaler { get ; set ; }

        public void AddRequestCharge(double requestCharge)
        {
            throw new NotImplementedException();
        }

        public async Task AddTooManyRequests()
        {
            var eventTime = DateTime.Now;
            long tooManyRequestsEvent = eventTime.Ticks;

            _tooManyRequestsHistory.Add(tooManyRequestsEvent);

            _tooManyRequestsHistory = _tooManyRequestsHistory
                .Where(record => record >= eventTime.AddSeconds(-5).Ticks)
                .ToList();

            if (_tooManyRequestsHistory.Count >= 4)
            {
                var currentThroughput = await Scaler.GetCurrentThroughputAsync();
                var doubleRequest = new ScaleRequest("ToDoList", "Items", currentThroughput * 2 );
                var result = Scaler.AdjustThroughputAsync(doubleRequest).Result;
            }
        }
    }
}
