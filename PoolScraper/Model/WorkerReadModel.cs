using CommonUtils.Utils.Logs;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;

namespace PoolScraper.Model
{
    public class WorkerReadModel
    {
        public WorkerReadModel(string poolId, string algorithm, long id, string name, long nominalHashRate,string provider, IWorkerModel model, IFarm farm)
        {
            PoolId = poolId;
            Algorithm = algorithm;
            Id = id;
            Name = name;
            ModelId = model.Id;
            FarmId = farm.Id;
            NominalHashRate = nominalHashRate;
            Provider = provider;
            WorkerId = Domain.WorkerId.Create(poolId, id);
        }
        
        [BsonId]
        public long Id { get; set; }
        public string PoolId { get; set; }
        public string Algorithm { get; set; }
        public string Name { get; set; }
        public int ModelId{ get; set; }
        public int FarmId { get; set; }
        public long NominalHashRate{ get; set; }
        public string Provider { get; set; }
        private IWorkerId WorkerId { get; }

        public override int GetHashCode()
        {
            return PoolId.GetHashCode() ^ Algorithm.GetHashCode() ^ Id.GetHashCode() ^ ModelId.GetHashCode() ^ FarmId.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is IWorker other)
            {
                return WorkerId.Equals(other.WorkerId) &&
                       Algorithm == other.Algorithm &&
                       Name == other.Name &&
                       ModelId == other.Model.Id &&
                       FarmId == other.Farm.Id;
            }
            return false;
        }

        public int CompareTo(object? obj)
        {
            if (obj is IWorker other)
            {
                return WorkerId.Id.CompareTo(other.WorkerId.Id);
            }
            throw new ArgumentException("Object is not a Worker");
        }
    }

    public static class WorkerReadModelExtensions
    {
        public static readonly ILogger  logger  = LoggerUtils.CreateLogger(nameof(WorkerReadModelExtensions));
        public static IWorker AsWorker(this WorkerReadModel workerReadModel)
        {
            if (!WorkerModel.TryGet(workerReadModel.ModelId, out var model)) 
            {
                logger.LogOnce(LogLevel.Warning, $"Worker model not found for id: {workerReadModel.ModelId}");
            }
            if (!Farm.TryGet(workerReadModel.FarmId,out var farm))
            {
                logger.LogOnce(LogLevel.Warning, $"Farm id not found for id: {workerReadModel.FarmId}");
            }
            return Worker.Create(workerReadModel.PoolId, workerReadModel.Algorithm, workerReadModel.Id, workerReadModel.Name, workerReadModel.NominalHashRate, workerReadModel.Provider, model!, farm! );
        }
        public static WorkerReadModel AsWorkerReadModel(this IWorker worker)
        {
            return new WorkerReadModel(worker.WorkerId.PoolId, worker.Algorithm, worker.WorkerId.Id, worker.Name, worker.NominalHashRate, worker.Provider, worker.Model, worker.Farm);
        }
    }
}
