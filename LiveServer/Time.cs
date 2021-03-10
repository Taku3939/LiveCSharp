// Created by Takuya Isaki on 2021/03/03

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LiveCoreLibrary;

namespace LiveServer
{
    public static class Time
    {
        public static MusicValue Value = new MusicValue(0, 0, PlayState.Playing);

        public static double TotalTime;
        private static CancellationTokenSource _source;
        public static void Start(int interval)
        {
            TotalTime = 0;
            double buf = DateTime.Now.Second;
            _source = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (true)
                {
                    if (_source.IsCancellationRequested) return;
                    if ((PlayState) Value.State == PlayState.Playing)
                    {
                        var now = DateTime.Now.Second;
                        TotalTime += now - buf;
                        Value.TimeCode = TotalTime;
                        buf = now;
                    }
                    else
                    {
                        buf = DateTime.Now.Second;
                    }
                    await Task.Delay(interval);
                }
            },_source.Token);
        }

        public static void Stop()
        {
            _source?.Cancel();
            _source?.Dispose();
        }
    }
}