using PoolScraper.Domain;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Model
{
    public class ExternalIdReadModel(string poolId, long id)
    {
        public string PoolId { get; set; } = poolId;
        public long Id { get; set; } = id;
    }

    public static class ExternalIdReadModelExtension
    {
        public static IExternalId AsExternalId(this ExternalIdReadModel externalIdRM)
        {
            return ExternalId.Create(externalIdRM.PoolId, externalIdRM.Id);
        }
    }
}