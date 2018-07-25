using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MathNet.Numerics;
using Microsoft.Azure.Documents;
using NoOpsJp.CosmosDbScaler.Scalers;

namespace NoOpsJp.CosmosDbScaler.Strategies
{
    public class SimpleScaleStrategy : IScaleStrategy<double>
    {
        private readonly Subject<double> _requestChargeSubject = new Subject<double>();
        private double _slopeThreshold;

        // TODO determine if collection ID is in the ctor or not?

        public SimpleScaleStrategy(string databaseId, string collectionId)
        {
            // TODO make configurable
            var options = new SimpleScaleStrategyOptions()
            {
                TrendDuration = TimeSpan.FromSeconds(30),
                TrendInterval = TimeSpan.FromSeconds(1),
            };

            RegisterObserver(databaseId, collectionId, options);
        }

        public SimpleScaler Scaler { get; set; }

        public void AddRequestCharge(double requestCharge)
        {
            _requestChargeSubject.OnNext(requestCharge);
        }
        
        #region factory

        // とりあえずの実装
        public static IScaleStrategy<double> Create(IDocumentClient client, string databaseId, string collectionId)
        {
            return new SimpleScaleStrategy(databaseId, collectionId)
            {
                Scaler = new SimpleScaler(client, databaseId, collectionId)
            };
        }

        #endregion
        
        #region private

        private void RegisterObserver(string databaseId, string collectionId, SimpleScaleStrategyOptions options)
        {
            // Note: this is an overly simplistic approach
            // CosmosDb RU is calculated/charged per second
            _requestChargeSubject.Buffer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
                .Select(x => x.Sum())
                .Buffer(options.TrendDuration, options.TrendInterval)
                .Select(graphItem =>
                {
                    var lineIntercept = Fit.Line(Enumerable.Range(0, graphItem.Count).Select(m => (double)m).ToArray(), graphItem.ToArray());
                    return new TrendModel()
                    {
                        Intercept = lineIntercept.Item1,
                        Slope = lineIntercept.Item2,
                        LastRecordIndex = graphItem.Count
                    };
                })
                .Select(trend => Observable.FromAsync(async cancelToken => await ScaleUnstableTrendAsync(databaseId, collectionId, trend)))
                .Subscribe();
        }

        private async Task ScaleUnstableTrendAsync(string databaseId, string collectionId, TrendModel trend)
        {
            if (_slopeThreshold <= trend.Slope)
            {
                double forecastedThroughput = CalculateForecastedThroughput(trend);
                // TODO add after refactoring
                await Scaler.AdjustThroughputAsync(new ScaleRequest(databaseId, collectionId, (int)forecastedThroughput));
            }
        }

        private double CalculateForecastedThroughput(TrendModel trend)
        {
            return trend.Slope * (trend.LastRecordIndex + 5) + trend.Intercept;
        }
        #endregion
    }
}