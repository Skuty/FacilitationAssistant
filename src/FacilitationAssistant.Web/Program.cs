using FacilitationAssistant.Core.Application.Commands;
using FacilitationAssistant.Core.Interfaces;
using FacilitationAssistant.Infrastructure.Persistence;
using FacilitationAssistant.Infrastructure.Persistence.Repositories;
using FacilitationAssistant.Infrastructure.Services;
using FacilitationAssistant.Web.Components;
using FacilitationAssistant.Web.Hubs;
using FacilitationAssistant.Web.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

namespace FacilitationAssistant.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add SignalR
            builder.Services.AddSignalR();

            // Database
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite(
                    builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                    "Data Source=facilitation.db"));

            // MediatR
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateMeetingCommand).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            // Core Services
            builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
            builder.Services.AddSingleton<IDateTimeProvider, SystemClock>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Map SignalR hub
            app.MapHub<MeetingHub>("/meetingHub");

            app.Run();
        }
    }
}
