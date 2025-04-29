using CommonUtils.Utils.Clock;
using log4net;
using Nito.AsyncEx;

namespace MarketLib.Utils.Clock
{
    public sealed class FakeClock : IClock, IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FakeClock));
        
        readonly List<IEvent> events = new List<IEvent>();
        readonly ManualResetEventSlim itemEvent = new ManualResetEventSlim(false);
        readonly Sim sim;

        int eventIndex = 0;

        public FakeClock(Sim sim)
        {
            this.sim = sim;
        }

        public void Sleep(int durationMs) => SleepAsync(durationMs, default).Wait();

        public async Task SleepAsync(int durationMs, CancellationToken cancellationToken)
        {
            if (log.IsDebugEnabled) { log.Debug("<<Sleep>> creating sleep event for ms:" + durationMs); }
            IEvent newEvent = new Event(time: When(durationMs), index: Interlocked.Increment(ref eventIndex));
            lock (events)
            {
                events.Add(newEvent);
                if (isWaitingStatus)
                {
                    itemEvent.Set();
                }
            }
            await newEvent.WaitAsync(cancellationToken);
        }

        internal DateTime StepForward(int ms)
        {
            DateTime endTime = UTCNow;
            List<IEvent> tmpEvents = GetClonedEvents();
            if (log.IsDebugEnabled) log.Debug("<<StepForward>> invoking on tmpEvents.Count:" + tmpEvents.Count);

            for (int evIndex = 0; evIndex < tmpEvents.Count; evIndex++)
            {
                IEvent currentEvent = tmpEvents[evIndex];
                var timeIsOver = currentEvent.IsBefore(endTime);
                if (log.IsDebugEnabled) log.Debug("<<StepForward>> invoked on " +  currentEvent + " timeIsOver:" + timeIsOver + " currentEvent.time " + currentEvent.Time + " endTime:" + endTime);

                if (timeIsOver)
                {
                    currentEvent.Set();
                    RemoveEvent(currentEvent);
                }
            }

            return endTime;
        }

        private void RemoveEvent(IEvent currentEvent)
        {
            lock (events) events.Remove(currentEvent);
        }

        private List<IEvent> GetClonedEvents()
        {
            lock (events) return new List<IEvent>(events);
        }

        bool isWaitingStatus = false;

        public DateTime UTCNow => sim.UTCNow;

        public long UtcTimeNow => sim.UTCNow.ToFileTimeUtc();

        internal DateTime NextEvent()
        {
            IEvent currentEvent;
            int eventsCount = 0;
            lock (events)
            {
                eventsCount = events.Count;
            }
            if (eventsCount == 0)
            {
                isWaitingStatus = true;
                itemEvent.Wait();
                itemEvent.Reset();
            }

            lock (events) currentEvent = events.Last();

            DateTime lastNow = currentEvent.Time;
            sim.UTCNow = lastNow;
            currentEvent.Set();
            RemoveEvent(currentEvent);
            return lastNow;
        }
 
        private DateTime When(int durationMs)
        {
            return UTCNow.AddMilliseconds(durationMs);
        }

        public void Dispose()
        {
            itemEvent.Dispose();
        }

        private interface IEvent
        {
            DateTime Time { get; }
            bool IsBefore(DateTime endTime);
            void Set();
            Task WaitAsync(CancellationToken cancellationToken = default);
        }

        private class Event: IEvent
        {
            readonly long index;
            readonly AsyncManualResetEvent mre;

            public Event(DateTime time, int index)
            {
                Time = time;
                mre = new AsyncManualResetEvent();
                this.index = index;
            }

            public DateTime Time { get; }

            public bool IsBefore(DateTime endTime) => Time <= endTime;

            public void Set() => mre.Set();

            public Task WaitAsync(CancellationToken cancellationToken = default) => mre.WaitAsync(cancellationToken);
        }
    }
}
