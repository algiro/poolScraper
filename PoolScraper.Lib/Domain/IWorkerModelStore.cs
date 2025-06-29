using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IWorkerModelStore
    {
        void AddModel(IWorkerModel model);
        IEnumerable<IWorkerModel> GetAllModels();
        void RemoveModel(int  modelId);
        bool TryGetModel(int modelId, [NotNullWhen(true)] out IWorkerModel? model);
    }

    public static class WorkerModelStore
    {
        public static IWorkerModelStore Create(IEnumerable<IWorkerModel> models)
        {
            return new DefaultModelStore(models);
        }

        private class DefaultModelStore : IWorkerModelStore
        {
            private readonly IDictionary<int, IWorkerModel> _models;
            public DefaultModelStore(IEnumerable<IWorkerModel> models)
            {
                _models = models.ToDictionary(f => f.Id, f => f);
            }

            public void AddModel(IWorkerModel farm) => _models.Add(farm.Id, farm);

            public IEnumerable<IWorkerModel> GetAllModels() => _models.Values;

            public void RemoveModel(int modelId) => _models.Remove(modelId);

            public bool TryGetModel(int modelId, [NotNullWhen(true)] out IWorkerModel? model)
            {
                var found = _models.TryGetValue(modelId, out model);
                if (!found) model = WorkerModel.UNKNOWN;
                return found;
            }
        }
    }
}
