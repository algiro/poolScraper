using MongoDB.Driver;
using PoolScraper.Model.PowerPool;
using PoolScraper.Model;
using log4net;
using PoolScraper.View;
using CommonUtils.Utils;
using PoolScraper.Components.Pages;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Config;

namespace PoolScraper.Persistency.Consolidation
{
    public class SnapshotHourConsolidationPersistency : SnapshotConsolidationPersistency
    {
        public SnapshotHourConsolidationPersistency(ILogger<SnapshotHourConsolidationPersistency> log, IPoolScraperConfig poolScraperConfig,ISnapshotDataConsolidationPersistency snapshotDataConsolidationPersistency) : base(log, poolScraperConfig, snapshotDataConsolidationPersistency)
        {
            log.LogInformation("SnapshotHourConsolidationPersistency C.tor Granularity {granularity} with connection string: {connectionString} and database name: {databaseName}", Granularity, poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
        }

        public override Granularity Granularity { get; } = Granularity.Hours;
    }
}
