using Microsoft.Extensions.DependencyInjection;
using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Infrastructure.FileSystem;
using WallpaperNormaliser.Infrastructure.Imaging;
using WallpaperNormaliser.Infrastructure.Persistence.Repositories;
using WallpaperNormaliser.Infrastructure.Persistence.TypeHandlers;
using WallpaperNormaliser.Infrastructure.Processing;
using WallpaperNormaliser.Infrastructure.Security;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectionFactory(this IServiceCollection services, string connectionString) 
        => services.AddSingleton<SqliteConnectionFactory>(new SqliteConnectionFactory(connectionString));

    public static IServiceCollection AddMigrationRunner(this IServiceCollection services) 
        => services.AddSingleton<MigrationRunner>();

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IHashService, Sha256HashService>();

        services.AddScoped<IImageProcessor, ImageSharpProcessor>()
            ;
        services.AddScoped<IManifestRepository, JsonManifestRepository>()
            ;
        services.AddScoped<IInputScanner, InputScanner>();
        services.AddScoped<IOutputWriter, OutputWriter>();
        
        services.AddScoped<ISettingsRepository, SqliteSettingsRepository>();
        services.AddScoped<ILogRepository, SqliteLogRepository>();
        services.AddScoped<IFileIndexRepository, SqliteFileIndexRepository>();
        services.AddScoped<IRunRepository, SqliteRunRepository>();
        services.AddScoped<IPreprocessCacheRepository, SqlitePreprocessCacheRepository>();

        services.AddScoped<IProcessingOrchestrator, ProcessingOrchestrator>();
        return services;
    }

    public static IServiceCollection AddTypeHandlers(this IServiceCollection services)
    {
        SqlMapper.AddTypeHandler<Resolution>(new ResolutionTypeHandler());
        return services;
    } 
}
