using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Model
{
    public interface IWorkerId
    {
        string PoolId { get; }
        long Id { get; }
    }

    public static class WorkerId
    {
        public static IWorkerId Create(string poolId, long workerId)
        {
            return new WorkerIdImpl(poolId, workerId);
        }
        public static WorkerIdView AsWorkerIdView(this IWorkerId workerId)
        {
            return new WorkerIdView(workerId.PoolId, workerId.Id);
        }

        private readonly struct WorkerIdImpl(string poolId, long workerId) : IWorkerId
        {
            public string PoolId { get; } = poolId;
            public long Id { get; } = workerId;

            public override int GetHashCode()
            {
                return PoolId.GetHashCode() ^ Id.GetHashCode();
            }
            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is WorkerIdImpl other)
                {
                    return PoolId == other.PoolId && Id == other.Id;
                }
                return false;
            }
        }
    }
}