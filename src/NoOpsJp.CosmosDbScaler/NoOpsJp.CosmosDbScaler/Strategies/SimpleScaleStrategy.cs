using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using NoOpsJp.CosmosDbScaler.Scalers;

namespace NoOpsJp.CosmosDbScaler.Strategies
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
                                 .Subscribe(x =>
                                    {
                                        // TODO: Not Implemented
                                        var sampleRequest = new ScaleRequest("ToDoList", "Items", 800);
                                        var result = Scaler.AdjustThroughputAsync(sampleRequest).Result;
                                    }); 
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

        public void AddTooManyRequest()
        {
        }
    }
}