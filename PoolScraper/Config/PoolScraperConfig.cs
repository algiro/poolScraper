using CommonUtils.Utils.Logs;
using PoolScraper.Domain;

namespace PoolScraper.Config
{
    public interface IPoolScraperConfig
    {
        string MongoConnectionString { get; }
        string MongoDatabaseName { get; }
        string PowerPoolApiKey { get; }
        string PowerPoolURL { get; }
        IEnumerable<IFarm> Farms { get; }
        IEnumerable<string> Providers { get; }
    }
    public class PoolScraperConfig
    {
        public static readonly IPoolScraperConfig Instance = new DefaultPoolScraperConfig();
        private static ConfigurationManager? _configurationManager;
        private static readonly ILogger logger = LoggerUtils.CreateLogger(nameof(PoolScraperConfig));
        public static void SetConfigurationManager(ConfigurationManager configurationManager)
        {
            logger.LogInformation("SetConfigurationManager " + configurationManager);
            _configurationManager = configurationManager;
        }
        public static IEnumerable<IFarm> GetFarms(string farmConfigStr)
        {
            // "Russia1|^\\d*mr|Russia;Dubai1|^\\d*d|Dubai;Ethiopia1|^\\d*eth|Ethiopia",
            var farmTokens = farmConfigStr?.Split(';');
            foreach (var farmToken in farmTokens)
            {
                var farmParts = farmToken.Split('|');
                if (farmParts.Length == 3)
                {
                    var farmId = farmParts[0];
                    var pattern = farmParts[1];
                    var location = farmParts[2];
                    yield return Farm.Create(farmId, pattern, location);
                }
            }
        }
        private class DefaultPoolScraperConfig : IPoolScraperConfig
        {
            public string MongoConnectionString { get; } = _configurationManager?.GetValue<string>("MongoConnectionString") ?? "mongodb://mongodb:27017";
            public string MongoDatabaseName {
                get {
                    logger.LogInformation("_configurationManager: " + _configurationManager);
                    return _configurationManager?.GetValue<string>("MongoDatabaseName") ?? "";
                }
            }
            public string PowerPoolApiKey { get;  } = _configurationManager?.GetValue<string>("PowerPoolApiKey") ?? "";
            public string PowerPoolURL { get; } = _configurationManager?.GetValue<string>("PowerPoolURL") ?? "https://api.powerpool.io/api/user";
            public IEnumerable<IFarm> Farms { 
                get {
                    var farmConfigStr = _configurationManager?.GetValue<string>("Workers.Farms");
                    if (string.IsNullOrEmpty(farmConfigStr))
                    {
                        return [];
                    }
                    var farms = GetFarms(farmConfigStr);
                    Farm.UpdateStore(farms);
                    return farms;
                }
            }

            public IEnumerable<string> Providers
            {
                get
                {
                    var providerStr = _configurationManager?.GetValue<string>("Workers.Providers");
                    if (string.IsNullOrEmpty(providerStr))
                    {
                        return ["Provider1"];
                    }
                    return string.IsNullOrEmpty(providerStr) ? ["Provider1"] : providerStr.Split(';');
                }
            }

        }
    }
}
