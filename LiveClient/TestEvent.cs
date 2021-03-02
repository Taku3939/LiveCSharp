using System;
using LiveCoreLibrary;

namespace LiveClient
{
    public class TestEvent : Event<MusicValue>
    {
        protected override void Received(MusicValue t)
        {
            Console.WriteLine($"MusicNumber : {t.MusicNumber}, TimeCode : {t.TimeCode}, Message : {t.State}");
        }
    }
}