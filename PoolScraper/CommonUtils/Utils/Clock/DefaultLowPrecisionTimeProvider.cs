using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommonUtils.Utils.Clock
{
    public  class DefaultLowPrecisionTimeProvider : TimeProvider
    {
        readonly static Stopwatch _stopWatch = new Stopwatch();
        readonly static TimeProvider instance = new DefaultLowPrecisionTimeProvider();

        private DefaultLowPrecisionTimeProvider() { _stopWatch.Start(); }

        public static TimeProvider Instance { get { return instance; } }

        public override TimeSpan StopWatchElapsed => _stopWatch.Elapsed;

        public override long UtcTimeNow => DateTime.Now.Ticks;

        public override DateTime UtcNow => DateTime.UtcNow;

        public override IClock NewClock => new DefaultClock();

        protected override void Dispose(bool disposing) => base.Dispose(disposing);
    }
}
