using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class SimpleScaleStrategy : IScaleStrategy
    {
        private readonly Subject<double> _requestChargeSubject = new Subject<double>();

        public SimpleScaleStrategy()
        {
            //TODO: とりあえずな処理
            _requestChargeSubject.Buffer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
                                 .Select(x => x.Sum())
                                 .Do(x => Console.WriteLine($"Step1: {JsonConvert.SerializeObject(x)}"))
                                 .Buffer(2)
                                 .Do(x => Console.WriteLine($"Step2: {JsonConvert.SerializeObject(x)}"))
                                 .Select(x => x.Zip(x.Skip(1), (a, b)
                                     => (prev: a, current: b)).Aggregate(true, (isFire, tappleValue) => IsFire(isFire, tappleValue)))
                                 .Subscribe(x => Scaler.AdjustThroughput(new ScaleRequest())); //TODO: とりあえず全部なげてる
        }

        private bool IsFire(bool isFire, (double prev, double current) data)
        {
            //TODO: とりあえず適当実装
            return isFire && data.prev < data.current;
        }

        public SimpleScaler Scaler { get; set; }

        public void AddRequestCharge(double requestCharge)
        {
            _requestChargeSubject.OnNext(requestCharge);
        }

        //Is DateTime ok?
        private List<DateTime> _tooManyRequestsHistory = new List<DateTime>();

        public void AddTooManyRequest()
        {
            var tooManyRequestEvent = DateTime.Now;
            
            _tooManyRequestsHistory.Add(tooManyRequestEvent); 
        
            _tooManyRequestsHistory = _tooManyRequestsHistory
                .Where(record => record >= tooManyRequestEvent.AddSeconds(-5))
                .ToList();

            if (_tooManyRequestsHistory.Count >= 4)
            {
                // TODO Call scaler
            }
        }
    }
}