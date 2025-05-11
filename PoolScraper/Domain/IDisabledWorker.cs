using Newtonsoft.Json;

namespace PoolScraper.Domain
{
    public interface IDisabledWorker
    {
        IWorkerId WorkerId { get; }
    }
    public static class DisabledWorker
    {
        public static IDisabledWorker Create(IWorkerId workerId)
        {
            return new DefaultDisabledWorker(workerId);
        }

        private class DefaultDisabledWorker(IWorkerId workerId) : IDisabledWorker
        {
            public IWorkerId WorkerId { get; } = workerId;
        }
    }
}