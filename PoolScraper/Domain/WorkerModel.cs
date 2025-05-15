using CommonUtils.Utils;
using PoolScraper.View;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PoolScraper.Domain
{
    public interface IWorkerModel
    {
        int Id { get; }
        string Name { get; }
    }

    public class WorkerModel
    {
        public static readonly IWorkerModel UNKNOWN = new DefaultWorkerModel(0, "UNKNOWN");
        public static readonly IWorkerModel[] DEFAULT_MODELS = [
            UNKNOWN,
            new DefaultWorkerModel(10, "L7"),
            new DefaultWorkerModel(20, "L9"),
            new DefaultWorkerModel(30, "DG1"),
            new DefaultWorkerModel(40, "T21"),
            new DefaultWorkerModel(50, "S21"),
            new DefaultWorkerModel(55, "S21XP"),
            new DefaultWorkerModel(60, "KS5"),
            new DefaultWorkerModel(65, "KS5M"),
            new DefaultWorkerModel(70, "M60"),
        ];
        private static IWorkerModelStore _modelStore = WorkerModelStore.Create(DEFAULT_MODELS);
        public static void UpdateStore(IEnumerable<IWorkerModel> models)
        {
            _modelStore = WorkerModelStore.Create(models);
        }
        public static IEnumerable<IWorkerModel> GetAllModels() => _modelStore.GetAllModels();
        public static bool TryGet(int modelId, [NotNullWhen(true)] out IWorkerModel? model) => _modelStore.TryGetModel(modelId, out model);

        private class DefaultWorkerModel : IWorkerModel
        {
            public DefaultWorkerModel(int id, string name)
            {
                Id = id;
                Name = name;
            }
            public int Id { get; }
            public string Name { get; }
            public override string ToString() => $"{Name}";

        }        
    }

    public static class WorkerModelExtensions
    {
        public static WorkerModelDTO AsModelDTO(this IWorkerModel model) => new WorkerModelDTO(model.Id, model.Name);
        
        public static bool TryGetModel(string workerName, out IWorkerModel model)
        {
            model = WorkerModel.UNKNOWN;
            if (string.IsNullOrWhiteSpace(workerName))
                return false;

            var suffix = Worker.GetWorkerSuffix(workerName);
            if (suffix == null)
                return false;

            // Iterate over all model names (longest first to prevent partial matches)
            var modelNames = WorkerModel.GetAllModels().OrderByDescending(m => m.Name.Length);            
            foreach (var mn in modelNames)
            {
                if (mn.Name.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Use word boundary, case-insensitive matching for model name
                var pattern = Regex.Escape(mn.Name);
                var match = Regex.Match(suffix, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    model = mn;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetNominalHashRate(string workerName, out long nominalHashRate)
        {
            nominalHashRate = 0;
            if (TryGetModel(workerName, out var model))
            {
                var modelNameIndex = workerName.IndexOf(model.Name, StringComparison.OrdinalIgnoreCase);
                var modelNameLenght = model.Name.Length;
                if (workerName.Length > modelNameIndex + modelNameLenght)
                {
                    var hashRateString = workerName.Substring(modelNameIndex + modelNameLenght);
                    return long.TryParse(hashRateString, out nominalHashRate);
                }
            }
            return false;
        }
    }
}