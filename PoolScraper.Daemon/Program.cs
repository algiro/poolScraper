// See https://aka.ms/new-console-template for more information
using CommonUtils.Utils.Logs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Configuration;
using PoolScraper.Config;
using PoolScraper.Persistency;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service;
using PoolScraper.Service.Consolidation;
using PoolScraper.Service.Store;

var logger = LoggerUtils.CreateLogger("PoolScraper.Daemon");
logger.LogInformation("Start PoolScraper.Daemon!");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("/app/Config/psSettings.json", false, true);
PoolScraperConfig.SetConfigurationManager(builder.Configuration);

string connectionString = PoolScraperConfig.Instance.MongoConnectionString;
string databaseName = PoolScraperConfig.Instance.MongoDatabaseName;

builder.Services.AddSingleton<IPoolScraperConfig>(PoolScraperConfig.Instance);
builder.Services.AddSingleton<IInitApp, InitApp>();
builder.Services.AddSingleton<IScrapingServiceClient, ScrapingServiceClient>();
builder.Services.AddSingleton<IPowerPoolScrapingService, PowePoolScrapingService>();
builder.Services.AddSingleton<IPowerPoolScrapingPersistency, PowerPoolScrapingPersistency>();
builder.Services.AddSingleton<IAppEventsPersistency, AppEventsPersistency>();
builder.Services.AddSingleton<ISnapshotDataConsolidationPersistency, SnapshotDataConsolidationPersistency>();
builder.Services.AddSingleton<IUptimeDataConsolidationPersistency, UptimeDataConsolidationPersistency>();
builder.Services.AddSingleton<IUptimeDailyConsolidationPersistency, UptimeDailyConsolidationPersistency>();
builder.Services.AddKeyedSingleton<ISnapshotConsolidationPersistency, SnapshotHourConsolidationPersistency>("hourSnapConsolidation");
builder.Services.AddKeyedSingleton<ISnapshotConsolidationPersistency, SnapshotDayConsolidationPersistency>("daySnapConsolidation");
builder.Services.AddSingleton<IUptimeHourConsolidationPersistency>((sp) => new UptimeHourConsolidationPersistency(LoggerUtils.CreateLogger<UptimeHourConsolidationPersistency>(), connectionString, databaseName));

builder.Services.AddSingleton<ISnapshotConsolidateServiceClient, SnapshotConsolidateServiceClient>();
builder.Services.AddSingleton<IUptimeConsolidateServiceClient, UptimeConsolidateServiceClient>();
builder.Services.AddSingleton<IWorkerPersistency, WorkerPersistency>();
builder.Services.AddSingleton<IWorkerStore, WorkerStore>();

builder.Services.AddHostedService<ScheduledService>();
//builder.Configuration.AddJsonFile("/config/appsettings.Development.json", true, true);

var host = builder.Build();
host.Run();
logger.LogInformation("App build completed");

