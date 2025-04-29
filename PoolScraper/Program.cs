using PoolScraper.Components;
using PoolScraper.Persistency;
using PoolScraper.Persistency.Consolidation;
using PoolScraper.Service;
using PoolScraper.Service.Consolidation;
using PoolScraper.Utils;

string connectionString = "mongodb://mongodb:27017";
string databaseName = "PowerPoolDB";
string apiKey = "0803cab54344474b915b42c74b5d8d8b";
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("/config/appsettings.Development.json", true, true);


// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();
builder.Services.AddSingleton<IPowerPoolScrapingService, PowePoolScrapingService>();
builder.Services.AddSingleton<IPowerPoolScrapingPersistency>((sp) => new PowerPoolScrapingPersistency(LoggerUtils.CreateLogger<PowerPoolScrapingPersistency>(), connectionString, databaseName));
builder.Services.AddSingleton<IWorkersService>((sp) => new WorkersService(LoggerUtils.CreateLogger<PowePoolScrapingService>(), connectionString, databaseName));
builder.Services.AddSingleton<IUptimeHourConsolidationPersistency>( (sp) => new UptimeHourConsolidationPersistency(LoggerUtils.CreateLogger<UptimeHourConsolidationPersistency>(), connectionString, databaseName));
builder.Services.AddSingleton<IUptimeConsolidateServiceClient, UptimeConsolidateServiceClient>();

builder.Services.AddSingleton<IWorkerPersistency>( (sp) => new WorkerPersistency(LoggerUtils.CreateLogger<WorkerPersistency>(), connectionString, databaseName));

builder.Services.AddSingleton<IUptimeService, UptimeService>();
builder.Services.AddSingleton<IScrapingServiceClient, ScrapingServiceClient>();
builder.Services.AddSingleton<IUptimeServiceClient, UptimeServiceClient>();

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
