using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace NoOpsJp.CosmosDbScaler.ThroughputMonitor
{
    public class ThroughputAnalyzer
    {
        // TDOO:
        private readonly Subject<double> _requestChargeSubject = new Subject<double>();

        private readonly SimpleScaler _scaler = new SimpleScaler(); //TODO DI

        public ThroughputAnalyzer()
        {
            //TODO: とりあえずな処理
            _requestChargeSubject.Buffer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
                .Select(x => x.Sum())
                .Do(x => Console.WriteLine($"Step1: {JsonConvert.SerializeObject(x)}"))
                .Buffer(2)
                .Do(x => Console.WriteLine($"Step2: {JsonConvert.SerializeObject(x)}"))
                .Select(x => x.Zip(x.Skip(1), (a, b)
                    => (prev: a, current: b)).Aggregate(true, (isFire, tappleValue) => IsFire(isFire, tappleValue)))
                .Subscribe(x => _scaler.AdjustThroughput(new ScaleRequest())); //TODO: とりあえず全部なげてる
        }

        private bool IsFire(bool isFire, (double prev, double current) data)
        {
            //TODO: とりあえず適当実装
            return isFire && data.prev < data.current;
        }

        // CosmosDB の RU がくる
        public void TrackRequestCharge(double requestCharge)
        {
            _requestChargeSubject.OnNext(requestCharge);
        }

        private double CalulateDelta(IList<double> val)
        {
            return 0;
        }

        public async Task TrackTooManyRequestAsync()
        {
            //TODO:
        }
    }

    public class SomeObject
    {
        // 1秒に一回の集計データ * N 秒のコレクションを持っている
    }
}