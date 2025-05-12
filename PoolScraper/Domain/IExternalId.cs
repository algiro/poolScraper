using PoolScraper.Model;
using PoolScraper.View;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IExternalId : IComparable
    {
        string PoolId { get; }
        long Id { get; }
    }

    public static class ExternalId
    {
        public static IExternalId UNINITIALIZED = new ExternalIdImpl(string.Empty, 0);
        public static IExternalId Create(string poolId, long workerId)
        {
            return new ExternalIdImpl(poolId, workerId);
        }
        public static ExternalIdReadModel AsWorkerIdReadModel(this IExternalId externalId)
        {
            return new ExternalIdReadModel(externalId.PoolId, externalId.Id);
        }
        private class ExternalIdImpl(string poolId, long workerId) : IExternalId
        {
            public string PoolId { get; } = poolId;
            public long Id { get; } = workerId;

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
        }
    }
}