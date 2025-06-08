using CommonUtils.Utils.Logs;
using log4net;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model.Scheduler;
using PoolScraper.View;

namespace PoolScraper.Service.Consolidation
{
    public interface IUptimeConsolidateServiceClient
    {
        Task ConsolidateHours(DateOnly date);
        Task<IEnumerable<IUptimeDataConsolidationInfo>> GetUptimeDataConsolidationInfoAsync(IDateRange dateRange);
        Task ConsolidateDays(IDateRange dateRange);
        Task<IEnumerable<IUptimePercentageDetailedView>> GetHourlyUptimeAsync(DateOnly dateOnly);
        Task<IEnumerable<IUptimePercentageDetailedView>> GetDailyWorkerUptimeAsync(DateOnly dateOnly);
        Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange);

    }

    public static class UptimeConsolidateServiceClientExtensions
    {
        private static readonly ILogger _log = LoggerUtils.CreateLogger<IUptimeConsolidateServiceClient>();
        public static async Task ConsolidateDays(this IUptimeConsolidateServiceClient uptimeConsolidateServiceClient, IDateRange dateRange)
        {
            _log.LogInformation("ConsolidateDays for dateRange: {dateRange}", dateRange);
            var dataConsolidated = await uptimeConsolidateServiceClient.GetUptimeDataConsolidationInfoAsync(dateRange);
            var dayConsolidated = dataConsolidated.Where(d => d.Granularity == Granularity.Days).ToList();
            _log.LogInformation("ConsolidateDays found {count} days already consolidated", dayConsolidated.Count);
            var currentDate = DateOnly.FromDateTime(dateRange.From);
            while (currentDate < DateOnly.FromDateTime(dateRange.To))
            {
                var currentDateRange = currentDate.AsDateRange();
                var hasBeenAlreadyConsolidated = dayConsolidated.Any(d => d.DateRange.Equals(currentDateRange));
                _log.LogInformation("ConsolidateDays date: {date}, already consolidated: {hasBeenAlreadyConsolidated}", currentDate, hasBeenAlreadyConsolidated);
                if (!hasBeenAlreadyConsolidated)
                {
                    await uptimeConsolidateServiceClient.ConsolidateDays(currentDateRange);
                    _log.LogInformation("ConsolidateDays done dateRange: {currentDateRange}", currentDateRange);
                }
                currentDate = currentDate.AddDays(1);
            }
        }
    }
}