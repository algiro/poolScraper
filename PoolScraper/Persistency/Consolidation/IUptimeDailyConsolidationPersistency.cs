﻿using PoolScraper.Domain;

namespace PoolScraper.Persistency.Consolidation
{
    public interface IUptimeDailyConsolidationPersistency
    {
        Task<IEnumerable<IUptimePercentage>> GetDailyUptimeAsync(IDateRange dateRange);
        Task<bool> InsertManyAsync(DateOnly date, IEnumerable<IUptimePercentage> hourlyUptime);
        Task<(bool isSuccesfull, long deleteCount)> RemoveDayConsolidationAsync(IDateRange dateRange);
    }
}