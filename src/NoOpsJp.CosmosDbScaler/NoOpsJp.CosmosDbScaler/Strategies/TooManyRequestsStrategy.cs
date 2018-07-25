using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
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
        private readonly string _databaseId;
        private readonly string _collectionId;

        public TooManyRequestsStrategy(string databaseId, string collectionId)
        {
            _databaseId = databaseId;
            _collectionId = collectionId;
        }

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
                var scaleRequest = new ScaleRequest(_databaseId, _collectionId, currentThroughput * _scaleFactor );
                var result = Scaler.AdjustThroughputAsync(scaleRequest).Result;
            }
        }


        #region factory

        public static IScaleStrategy<double> Create(IDocumentClient client, string databaseId, string collectionId)
        {
            return new TooManyRequestsStrategy(databaseId, collectionId)
            {
                Scaler = new SimpleScaler(client, databaseId, collectionId)
            };
        }

        #endregion

    }
}
