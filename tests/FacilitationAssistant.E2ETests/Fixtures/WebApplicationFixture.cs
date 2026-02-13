using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FacilitationAssistant.E2ETests.Fixtures;

/// <summary>
/// Hosts the ASP.NET Core application for E2E testing
/// Uses WebApplicationFactory to spin up the app in-process
/// </summary>
public class WebApplicationFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IHost? _host;
    public string BaseUrl { get; private set; } = "http://localhost:5000";

    public WebApplicationFixture()
    {
        // Load base URL from settings
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        BaseUrl = config.GetSection("TestSettings:BaseUrl").Value ?? BaseUrl;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false);
        });

        builder.UseUrls(BaseUrl);
    }

    public new Task InitializeAsync()
    {
        // WebApplicationFactory creates the server automatically
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        await base.DisposeAsync();
    }
}
