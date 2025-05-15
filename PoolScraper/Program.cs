using CommonUtils.Utils;
using Microsoft.Extensions.DependencyInjection;
using PoolScraper.Components;
using PoolScraper.Config;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Persistency;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Persistency.Utils;
using PoolScraper.Service;
using PoolScraper.Service.Consolidation;
using PoolScraper.Service.Store;
using PoolScraper.Utils;

var builder = WebApplication.CreateBuilder(args);
var logger = LoggerUtils.CreateLogger("PoolScraper");
builder.Configuration.AddJsonFile("/app/Config/psSettings.json", false, true);
//builder.Configuration.AddJsonFile("/config/appsettings.Development.json", true, true);

logger.LogInformation("App build completed");
PoolScraperConfig.SetConfigurationManager(builder.Configuration);
logger.LogInformation("PoolScraperConfig.SetConfigurationManager");

string connectionString = PoolScraperConfig.Instance.MongoConnectionString;
string databaseName = PoolScraperConfig.Instance.MongoDatabaseName;


// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();
builder.Services.AddSingleton<IPowerPoolScrapingService, PowePoolScrapingService>();

builder.Services.AddSingleton<IWorkerIdMap>((sp) => WorkerIdMap.Create(sp.GetService<IWorkerPersistency>().CheckNotNull()));
builder.Services.AddSingleton<IPowerPoolScrapingPersistency>((sp) => new PowerPoolScrapingPersistency(LoggerUtils.CreateLogger<PowerPoolScrapingPersistency>(), connectionString, databaseName, sp.GetService<IWorkerIdMap>().CheckNotNull()));
builder.Services.AddSingleton<IWorkersService,WorkersService>();
builder.Services.AddSingleton<IUptimeHourConsolidationPersistency>( (sp) => new UptimeHourConsolidationPersistency(LoggerUtils.CreateLogger<UptimeHourConsolidationPersistency>(), connectionString, databaseName));
builder.Services.AddSingleton<ISequenceGenerator>( (sp) => new SequenceGenerator(LoggerUtils.CreateLogger<SequenceGenerator>(), connectionString, databaseName));

builder.Services.AddKeyedSingleton<ISnapshotConsolidationPersistency>("hourSnapConsolidation", (sp,name) => new SnapshotConsolidationPersistency(LoggerUtils.CreateLogger<SnapshotConsolidationPersistency>(), connectionString, databaseName,Granularity.Hours));
builder.Services.AddKeyedSingleton<ISnapshotConsolidationPersistency>("daySnapConsolidation", (sp, name) => new SnapshotConsolidationPersistency(LoggerUtils.CreateLogger<SnapshotConsolidationPersistency>(), connectionString, databaseName, Granularity.Days));
builder.Services.AddSingleton<ISnapshotDataConsolidationPersistency>((sp) => new SnapshotDataConsolidationPersistency(LoggerUtils.CreateLogger<SnapshotDataConsolidationPersistency>(), connectionString, databaseName, Granularity.Hours));

builder.Services.AddSingleton<IUptimeConsolidateServiceClient, UptimeConsolidateServiceClient>();
builder.Services.AddSingleton<IWorkerPersistency>( (sp) => new WorkerPersistency(LoggerUtils.CreateLogger<WorkerPersistency>(), connectionString, databaseName));

builder.Services.AddSingleton<IUptimeService, UptimeService>();
builder.Services.AddSingleton<IScrapingServiceClient, ScrapingServiceClient>();
builder.Services.AddSingleton<IUptimeServiceClient, UptimeServiceClient>();
builder.Services.AddSingleton<ISnapshotConsolidateServiceClient, SnapshotConsolidateServiceClient>();
builder.Services.AddSingleton<IWorkersReportService, WorkersReportService>();
builder.Services.AddSingleton<IWorkerStore>((sp) => new WorkerStore(LoggerUtils.CreateLogger<WorkerStore>(),sp.GetService<IWorkerPersistency>().CheckNotNull()));

builder.Services.AddHostedService<ScheduledService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
