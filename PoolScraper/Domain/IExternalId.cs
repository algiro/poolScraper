using PoolScraper.Model;
using PoolScraper.View;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IExternalId : IComparable
    {
        string PoolId { get; }
        string Id { get; }
    }

    public static class ExternalId
    {
        public static IExternalId UNINITIALIZED = new ExternalIdImpl(string.Empty, string.Empty);
        public static IExternalId Create(string poolId, string id)
        {
            return new ExternalIdImpl(poolId, id);
        }


        public static ExternalIdReadModel AsReadModel(this IExternalId externalId)
        {
            return new ExternalIdReadModel(externalId.PoolId, externalId.Id);
        }
        private class ExternalIdImpl(string poolId, string id) : IExternalId
        {
            public string PoolId { get; } = poolId;
            public string Id { get; } = id;

            public override int GetHashCode()
            {
                return PoolId.GetHashCode() ^ Id.GetHashCode();
            }
            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is IExternalId other)
                {
                    return PoolId == other.PoolId && Id == other.Id;
                }
                return false;
            }

            public int CompareTo(object? obj)
            {
                if (obj is IExternalId other)
                {
                    var poolIdComparison = PoolId.CompareTo(other.PoolId);
                    if (poolIdComparison != 0)
                    {
                        return poolIdComparison;
                    }
                    return Id.CompareTo(other.Id);
                }
                return 0;
            }
            public override string ToString() => $"{PoolId}.{Id}";
        }
    }
}