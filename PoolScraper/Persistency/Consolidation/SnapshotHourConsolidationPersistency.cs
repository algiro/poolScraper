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
        }

        public override Granularity Granularity { get; } = Granularity.Hours;
    }
}
