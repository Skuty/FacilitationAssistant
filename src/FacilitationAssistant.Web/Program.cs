using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Extensions;
using FacilitationAssistant.Infrastructure.Hubs;
using FacilitationAssistant.Web.Components;
using FacilitationAssistant.Web.Middleware;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();

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

// Configure database provider from settings ("Database:Provider": InMemory | CosmosDb | SqlServer)
builder.Services.AddDatabase(builder.Configuration);

// Add Mediator
builder.Services.AddMediator();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
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

// Initialize database (provider-specific: Cosmos container creation, SQL migrations, or EnsureCreated)
logger.LogInformation("Initializing database...");
var dbInitializer = app.Services.GetRequiredService<IDatabaseInitializer>();
try
{
    await dbInitializer.InitializeAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Database initialization failed. The application cannot start.");
    throw;
}

app.Run();
