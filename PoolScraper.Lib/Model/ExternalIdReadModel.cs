using PoolScraper.Domain;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Model
{
    public class ExternalIdReadModel(string poolId, string id)
    {
        public string PoolId { get; set; } = poolId;
        public string Id { get; set; } = id;
    }

    public static class ExternalIdReadModelExtension
    {
        public static IExternalId AsExternalId(this ExternalIdReadModel externalIdRM)
        {
            return ExternalId.Create(externalIdRM.PoolId, externalIdRM.Id);
        }
    }
}