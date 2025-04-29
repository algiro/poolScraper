namespace CommonUtils.Utils.Clock
{
    public abstract class TimeProvider: IDisposable
    {
        static TimeProvider current = DefaultTimeProvider.Instance;

        public static TimeProvider Current
        {
            get { return TimeProvider.current; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                TimeProvider.current = value;
            }
        }

        public abstract DateTime UtcNow { get; }
        public abstract long UtcTimeNow { get; }
        public abstract TimeSpan StopWatchElapsed { get; }
        public abstract IClock NewClock { get; }

        public static void ResetToDefault()
        {
            TimeProvider.current.Dispose();
            TimeProvider.current = DefaultTimeProvider.Instance;
        }

        protected virtual void Dispose(bool disposing) { } // Nothing to Dispose

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
