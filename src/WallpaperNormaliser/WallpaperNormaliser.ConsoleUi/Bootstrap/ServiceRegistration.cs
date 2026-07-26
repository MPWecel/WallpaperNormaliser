using Microsoft.Extensions.DependencyInjection;
using WallpaperNormaliser.ConsoleUi.ApplicationServices;
using WallpaperNormaliser.ConsoleUi.ApplicationServices.Interfaces;
using WallpaperNormaliser.ConsoleUi.Navigation;
using WallpaperNormaliser.ConsoleUi.Rendering;
using WallpaperNormaliser.ConsoleUi.Screens;
using WallpaperNormaliser.ConsoleUi.Services;
using WallpaperNormaliser.Infrastructure;
using WallpaperNormaliser.Infrastructure.DependencyInjection;

namespace WallpaperNormaliser.ConsoleUi.Bootstrap;
public static class ServiceRegistration
{
    public static void Configure(IServiceCollection services)
    {
        services.AddTypeHandlers();
        services.AddDbPaths();
        services.AddConnectionFactory();
        services.AddMigrationRunner();
        services.AddSettingsChangeNotifier();
        services.AddInfrastructure();

        services.AddSingleton<SettingsValidator>()
                .AddSingleton<StartupValidator>()
                .AddSingleton<ThemeMarkupProvider>();

        services.AddScoped<ISettingsApplicationService, SettingsApplicationService>();

        services.AddScoped<StartupRunner>();

        services.AddScoped<MainMenu>();

        services.AddScoped<DashboardScreen>()
                .AddScoped<ProcessingScreen>()
                .AddScoped<LogsScreen>()
                .AddScoped<InventoryScreen>()
                .AddScoped<SettingsScreen>();
    }
}
