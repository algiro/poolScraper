using log4net;
using MarketLib.Utils.Clock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Utils.Clock
{
    public sealed class Sim: IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Sim));
        readonly List<FakeClock> clocks = new List<FakeClock>();

        public Sim() : this(DateTime.UtcNow) { }

        public Sim(DateTime startingTime)
        {
            UTCNow = startingTime;
        }

        public IClock CreateClock()
        {
            FakeClock clock = new FakeClock(this);
            lock (clocks) clocks.Add(clock);
            return clock;
        }

        public IStopWatch CreateStopWatch()
        {
            FakeStopWatch sw = new FakeStopWatch(this);
            return sw;
        }

        public void Step()
        {
            GetClonedClocks().ForEach(clock => clock.NextEvent());
        }

        private List<FakeClock> GetClonedClocks()
        {
            lock (clocks) return new List<FakeClock>(clocks);
        }

        public void Advance(int ms)
        {
            UTCNow = UTCNow.AddMilliseconds(ms);
            GetClonedClocks().ForEach(clock =>
            {
                UTCNow = clock?.StepForward(ms) ?? UTCNow;
                if (log.IsDebugEnabled) log.Debug("<<Advance>> invoked on " +  clock + " for ms:" + ms);
            });
        }

        public void Dispose()
        {
            GetClonedClocks().ForEach(clock => clock?.Dispose());
        }

        public DateTime UTCNow
        {
            get; set;
        }
    }
}
