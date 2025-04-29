using MarketLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonUtils.Utils.Clock
{
    public class DefaultTimeProvider : TimeProvider
    {
        readonly static Stopwatch _stopWatch = new Stopwatch();
        readonly static TimeProvider instance = new DefaultTimeProvider();


        private DefaultTimeProvider() { _stopWatch.Start(); }

        public static TimeProvider Instance { get { return instance; } }

        public override TimeSpan StopWatchElapsed => _stopWatch.Elapsed;

        public override long UtcTimeNow => DateTime.UtcNow.Ticks;

        public override DateTime UtcNow => DateTime.UtcNow;

        public override IClock NewClock => new DefaultClock();

        protected override void Dispose(bool disposing) => base.Dispose(disposing);
    }

    public class DefaultClock : IClock
    {
        public void Sleep(int ms) => Thread.Sleep(ms);

        public Task SleepAsync(int ms, CancellationToken cancellationToken) => Task.Delay(ms, cancellationToken);

        public DateTime UTCNow => DateTime.UtcNow;

        public long UtcTimeNow => DateTime.UtcNow.Ticks;
    }
}
