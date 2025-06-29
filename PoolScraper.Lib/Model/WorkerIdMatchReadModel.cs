using MongoDB.Bson.Serialization.Attributes;
using PoolScraper.Domain;
using PoolScraper.Model.PowerPool;
using PoolScraper.View;

namespace PoolScraper.Model
{
    public class WorkerIdMatchReadModel(IExternalId externalId,IWorkerId workerId)
    {
        [BsonId]
        public string Id => $"{externalId.Id}.{WorkerId.PoolId}.{WorkerId.Id}";
        public ExternalIdReadModel ExternalId { get; set; } = externalId.AsReadModel();
        public WorkerIdReadModel WorkerId { get; set; } = workerId.AsReadModel();
    }

    public static class WorkerIdMatchReadModelExtensions
    {
        public static IWorkerIdMatch AsWorkerIdMatch(this WorkerIdMatchReadModel workerIdReadModel)
        {
            return WorkerIdMatch.Create(workerIdReadModel.WorkerId.AsWorkerId(), workerIdReadModel.ExternalId.AsExternalId());
        }
    }
}