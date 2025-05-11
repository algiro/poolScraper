using Newtonsoft.Json;
using PoolScraper.Domain;

namespace PoolScraper.Model
{
    public class DisabledWorkerReadModel
    {
        public DisabledWorkerReadModel(string poolId, long id)
        {
            PoolId = poolId;
            Id = id;
        }
        [JsonProperty("poolId")]
        public string PoolId { get; set; }
        [JsonProperty("id")]
        public long Id { get; set; }

        public override int GetHashCode()
        {
            return PoolId.GetHashCode() ^ Id.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is IWorker other)
            {
                return PoolId == other.WorkerId.PoolId &&
                       Id == other.WorkerId.Id;
            }
            return false;
        }
    }

    public static class DisabledWorkerExtensions
    {
        public static DisabledWorkerReadModel AsDisabledWorkerReadModel(this IDisabledWorker disabledWorker)
            => new DisabledWorkerReadModel(disabledWorker.WorkerId.PoolId, disabledWorker.WorkerId.Id);

        public static IEnumerable<DisabledWorkerReadModel> AsDisabledWorkers(this IEnumerable<IDisabledWorker> disabledWorkers)
            => disabledWorkers.Select(w => w.AsDisabledWorkerReadModel());
        public static IDisabledWorker AsDisabledWorker(this DisabledWorkerReadModel disabledWorker)
            => DisabledWorker.Create(WorkerId.Create(disabledWorker.PoolId, disabledWorker.Id));
        public static IEnumerable<IDisabledWorker> AsDisabledWorkers(this IEnumerable<DisabledWorkerReadModel> disabledWorkers)
            => disabledWorkers.Select(w => w.AsDisabledWorker());
        
    }
}