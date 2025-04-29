namespace CommonUtils.Utils.Clock
{
    public class FakeStopWatch : IStopWatch
    {
        readonly Sim sim;
        public FakeStopWatch(Sim sim)
        {
            this.sim = sim;
        }
        public TimeSpan Elapsed()
        {
            return new TimeSpan(sim.UTCNow.Ticks);
        }
    }
}
