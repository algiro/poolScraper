using Newtonsoft.Json;
using PoolScraper.Domain;

namespace PoolScraper.Model
{
    public class WorkerReadModel
    {
        public WorkerReadModel(string poolId, string algorithm, long id, string name, WorkerModel model, Farm farm)
        {
            PoolId = poolId;
            Algorithm = algorithm;
            Id = id;
            Name = name;
            _model = model;
            _farm = farm;

            ModelStr = model.ToString();
            FarmStr = farm.ToString();
            WorkerId = Domain.WorkerId.Create(poolId, id);
        }
        [JsonProperty("poolId")]
        public string PoolId { get; set; }
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("model")]
        public string ModelStr { get; set; }
        [JsonProperty("farmId")]
        public string FarmStr { get; set; }
        private WorkerModel? _model;
        public WorkerModel Model
        {
            get
            {
                if (_model == null)
                {
                    _model = (ModelStr != null) ? Enum.Parse<WorkerModel>(ModelStr) : WorkerModel.UNKNOWN;
                }
                return _model ?? WorkerModel.UNKNOWN;
            }
            private set
            {
                _model = value;
            }
        }
        private Farm? _farm;
        public Farm FarmId
        {
            get
            {
                if (_farm == null)
                {
                    _farm = (FarmStr != null) ? Enum.Parse<Farm>(FarmStr) : Farm.UNKNOWN;
                }
                return _farm ?? Farm.UNKNOWN;
            }
            private set
            {
                _farm = value;
            }
        }

        public IWorkerId WorkerId { get; }

        public override int GetHashCode()
        {
            return PoolId.GetHashCode() ^ Algorithm.GetHashCode() ^ Id.GetHashCode() ^ Model.GetHashCode() ^ FarmId.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is IWorker other)
            {
                return WorkerId == other.WorkerId &&
                       Algorithm == other.Algorithm &&
                       Name == other.Name &&
                       Model == other.Model &&
                       FarmId == other.FarmId;
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
        public static IWorker AsWorker(this WorkerReadModel workerReadModel)
        {
            return Worker.Create(workerReadModel.PoolId, workerReadModel.Algorithm, workerReadModel.Id, workerReadModel.Name, workerReadModel.Model, workerReadModel.FarmId);
        }
        public static WorkerReadModel AsWorkerReadModel(this IWorker worker)
        {
            return new WorkerReadModel(worker.WorkerId.PoolId, worker.Algorithm, worker.WorkerId.Id, worker.Name, worker.Model, worker.FarmId);
        }
    }
}
