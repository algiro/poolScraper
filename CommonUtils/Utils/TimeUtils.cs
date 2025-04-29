using TimeProvider = CommonUtils.Utils.Clock.TimeProvider;

namespace CommonUtils.Utils
{
    public static class TimeUtils
    {
        public static TimeSpan GetTimeSpanUntilNext(TimeOnly executionTime)
        {
            var now = TimeProvider.Current.UtcNow;
            DateTime startTime = now.Date.AddHours(executionTime.Hour).AddMinutes(executionTime.Minute);
            if (now > startTime) startTime = startTime.AddDays(1);
            return startTime - now;
        }

    }
}
