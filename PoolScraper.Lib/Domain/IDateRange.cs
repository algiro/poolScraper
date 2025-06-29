using CommonUtils.Utils;
using PoolScraper.Model;

namespace PoolScraper.Domain
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

        public static IEnumerable<DateOnly> GetDatesWithinRange(this IDateRange dateRange)
        {
            var start = DateOnly.FromDateTime(dateRange.From);
            var end = DateOnly.FromDateTime(dateRange.To);
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                yield return date;
            }
        }

        public static DateRangeReadModel AsDateRangeView(this IDateRange dateRange)
        {
            return new DateRangeReadModel(dateRange.From, dateRange.To);
        }
        public static DateTime MiddleDateTime(this IDateRange dateRange)
            => dateRange.From.AddMinutes((dateRange.To - dateRange.From).TotalMinutes / 2);
        
        public static IDateRange AsDateRange(this DateOnly dateOnly) => Create(dateOnly.GetBeginOfDay(), dateOnly.GetEndOfDay());

        public static bool IsSameDateRange(this IDateRange dateRange1, IDateRange dateRange2)
            => dateRange1.From.Date == dateRange2.From.Date && dateRange1.To.Date == dateRange2.To.Date;


        private readonly struct DateRangeImpl(DateTime from, DateTime to) : IDateRange
        {
            public DateTime From { get; } = from;
            public DateTime To { get; } = to;
            public override string ToString() => $"{From} - {To}";

        }
    }

}
