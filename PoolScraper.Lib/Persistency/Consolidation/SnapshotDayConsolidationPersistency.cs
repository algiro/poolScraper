using MongoDB.Driver;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model;
using log4net;
using PoolScraper.View;
using CommonUtils.Utils;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Config;
using Microsoft.Extensions.Logging;

namespace PoolScraper.Persistency.Consolidation
{
    public class SnapshotDayConsolidationPersistency : SnapshotConsolidationPersistency
    {
        public SnapshotDayConsolidationPersistency(ILogger<SnapshotDayConsolidationPersistency> log, IPoolScraperConfig poolScraperConfig, ISnapshotDataConsolidationPersistency snapshotDataConsolidationPersistency) : base(log, poolScraperConfig, snapshotDataConsolidationPersistency)
        {
            log.LogInformation("SnapshotDayConsolidationPersistency C.tor Granularity {granularity} with connection string: {connectionString} and database name: {databaseName}", Granularity, poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
        }

        public override Granularity Granularity { get; } = Granularity.Days;
    }
}
