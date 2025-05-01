using CommonUtils.Utils;

namespace PoolScraper.Model
{
    public interface IDateRange
    {
        DateTime From { get; }
        DateTime To { get; }
    }
    public static class DateRange
    {
        public static IDateRange Create(DateTime from, DateTime to)
        {
            return new DateRangeImpl(from, to);
        }
        public static DateRangeView AsDateRangeView(this IDateRange dateRange)
        {
            return new DateRangeView(dateRange.From, dateRange.To);
        }
        public static DateTime MiddleDateTime(this IDateRange dateRange)
            => dateRange.From.AddMinutes((dateRange.To - dateRange.From).TotalMinutes / 2);
        
        public static IDateRange AsDateRange(this DateOnly dateOnly) => Create(dateOnly.GetBeginOfDay(), dateOnly.GetEndOfDay());
        private readonly struct DateRangeImpl(DateTime from, DateTime to) : IDateRange
        {
            public DateTime From { get; } = from;
            public DateTime To { get; } = to;
            public override string ToString() => $"{From} - {To}";

        }
    }

}
