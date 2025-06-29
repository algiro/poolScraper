using PoolScraper.Model;
using PoolScraper.View;
using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IWorkerId : IComparable
    {
        string PoolId { get; }
        long Id { get; }
    }

    public static class WorkerId
    {
        public static IWorkerId UNINITIALIZED = new WorkerIdImpl(string.Empty, 0);
        public static bool IsUninitialized(IWorkerId? workerId) => workerId == null || workerId.Equals(UNINITIALIZED);

        public static IWorkerId Create(string poolId, long workerId)
        {
            return new WorkerIdImpl(poolId, workerId);
        }
        public static WorkerIdReadModel AsReadModel(this IWorkerId workerId)
        {
            return new WorkerIdReadModel(workerId.PoolId, workerId.Id);
        }
        public static WorkerIdDTO AsDTO(this IWorkerId workerId)
        {
            return new WorkerIdDTO(workerId.PoolId, workerId.Id);
        }
        private class WorkerIdImpl(string poolId, long workerId) : IWorkerId
        {
            public string PoolId { get; } = poolId;
            public long Id { get; } = workerId;

            public override int GetHashCode()
            {
                return PoolId.GetHashCode() ^ Id.GetHashCode();
            }
            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is IWorkerId other)
                {
                    return PoolId == other.PoolId && Id == other.Id;
                }
                return false;
            }

            public int CompareTo(object? obj)
            {
                if (obj is IWorkerId other)
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