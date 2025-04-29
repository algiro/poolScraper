namespace CommonUtils.Utils.Clock
{
    public class FakeTimeProvider : TimeProvider
    {
        readonly Sim sim;
        readonly IStopWatch stopWatch;
        readonly IClock clock;
        private bool disposedValue;

        public FakeTimeProvider(Sim sim)
        {
            this.sim = sim;
            stopWatch = sim.CreateStopWatch();
            clock = sim.CreateClock();
        }
        
        public override TimeSpan StopWatchElapsed => stopWatch.Elapsed();

        public override DateTime UtcNow => clock.UTCNow;

        public override IClock NewClock => sim.CreateClock();

        public override long UtcTimeNow => UtcNow.ToFileTimeUtc();

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sim.Dispose();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

    }
}
