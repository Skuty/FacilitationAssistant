using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Web.Components;
using FacilitationAssistant.Web.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR
builder.Services.AddSignalR();

// Add DbContext with in-memory database for now (can switch to PostgreSQL later)
// Using singleton for in-memory database to work with Mediator's singleton lifetime
builder.Services.AddDbContext<FacilitationDbContext>(options =>
    options.UseInMemoryDatabase("FacilitationDb"), ServiceLifetime.Singleton);

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


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hub
app.MapHub<MeetingHub>("/meetinghub");

app.Run();
