using Newtonsoft.Json;

namespace PoolScraper.Model
{
    public class Worker : IWorker
    {
        public Worker(string poolId, string algorithm, long id, string name, WorkerModel model, Farm farm)
        {
            PoolId = poolId;
            Algorithm = algorithm;
            Id = id;
            Name = name;
            _model = model;
            _farm = farm;

            ModelStr = model.ToString();
            FarmStr = farm.ToString();
            WorkerId = PoolScraper.Model.WorkerId.Create(poolId, id);
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
        public WorkerModel Model { 
            get
            {
                if (_model == null)
                {
                    _model = Enum.Parse<WorkerModel>(ModelStr);
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
                    _farm = Enum.Parse<Farm>(FarmStr);
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
    }
}
