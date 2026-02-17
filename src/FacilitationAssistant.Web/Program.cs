using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Web.Components;
using FacilitationAssistant.Web.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR
builder.Services.AddSignalR();

// Add Rate Limiting (60 requests per minute per IP)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            }));
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
    };
});

// Configure database based on environment
// CosmosDB credentials present? → Use CosmosDB (Azure production)
// Otherwise → Use InMemory (Local development)
var cosmosEndpoint = builder.Configuration.GetValue<string>("Database:ConnectionStrings:CosmosDB:AccountEndpoint");
var cosmosKey = builder.Configuration.GetValue<string>("Database:ConnectionStrings:CosmosDB:AccountKey");

if (!string.IsNullOrEmpty(cosmosEndpoint) && !string.IsNullOrEmpty(cosmosKey))
{
    // CosmosDB - Azure production environment
    var databaseName = builder.Configuration.GetValue<string>("Database:ConnectionStrings:CosmosDB:DatabaseName") ?? "FacilitationAssistant";
    
    builder.Services.AddDbContext<FacilitationDbContext>(options =>
        options.UseCosmos(cosmosEndpoint, cosmosKey, databaseName), ServiceLifetime.Scoped);
    
    Console.WriteLine($"Using CosmosDB: {databaseName}");
}
else
{
    // InMemory - Local development (default)
    var databaseName = builder.Configuration.GetConnectionString("InMemory") 
        ?? builder.Configuration.GetValue<string>("Database:ConnectionStrings:InMemory") 
        ?? "FacilitationDb";
    
    builder.Services.AddDbContext<FacilitationDbContext>(options =>
        options.UseInMemoryDatabase(databaseName), ServiceLifetime.Singleton);
    
    Console.WriteLine($"Using InMemory Database: {databaseName}");
}

// Add Mediator
builder.Services.AddMediator();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hub
app.MapHub<MeetingHub>("/meetinghub");

// Ensure database is created (important for CosmosDB)
if (!string.IsNullOrEmpty(cosmosEndpoint) && !string.IsNullOrEmpty(cosmosKey))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FacilitationDbContext>();
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Database initialization failed: {ex.Message}");
    }
}

app.Run();
