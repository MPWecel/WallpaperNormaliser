using Microsoft.Extensions.DependencyInjection;
using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Infrastructure.Background;
using WallpaperNormaliser.Infrastructure.FileSystem;
using WallpaperNormaliser.Infrastructure.Imaging;
using WallpaperNormaliser.Infrastructure.Logging;
using WallpaperNormaliser.Infrastructure.Persistence;
using WallpaperNormaliser.Infrastructure.Persistence.Repositories;
using WallpaperNormaliser.Infrastructure.Persistence.TypeHandlers;
using WallpaperNormaliser.Infrastructure.Processing;
using WallpaperNormaliser.Infrastructure.Security;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbPaths(this IServiceCollection services) => services.AddSingleton<DbPaths>();

    public static IServiceCollection AddConnectionFactory(this IServiceCollection services)
        => services.AddSingleton<SqliteConnectionFactory>(
                                                             sp =>
                                                             {
                                                                 DbPaths paths = sp.GetRequiredService<DbPaths>();
                                                                 paths.EnsureDatabaseDirectoryExists();

                                                                 string pragmaFilePath = Path.Combine(
                                                                                                        AppContext.BaseDirectory,
                                                                                                        "Persistence",
                                                                                                        "Sql",
                                                                                                        "SchemaCreation",
                                                                                                        "000_Pragma.pragmasqlite"
                                                                                                     );

                                                                 if (!File.Exists(pragmaFilePath))
                                                                     throw new FileNotFoundException($"Required SQLite pragma file not found at '{pragmaFilePath}'.", pragmaFilePath);

                                                                 string pragmaSql = File.ReadAllText(pragmaFilePath);
                                                                 if (String.IsNullOrWhiteSpace(pragmaSql))
                                                                     throw new InvalidOperationException($"SQLite pragma file at '{pragmaFilePath}' is empty.");

                                                                 return new SqliteConnectionFactory(paths.ConnectionString, pragmaSql);
                                                             }
                                                         );

    public static IServiceCollection AddMigrationRunner(this IServiceCollection services)
        => services.AddSingleton<MigrationRunner>();

    public static IServiceCollection AddSettingsChangeNotifier(this IServiceCollection services)
        => services.AddSingleton<ISettingsChangeNotifier, SettingsChangeNotifier>();

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IWorkingDirectoryResolver, WorkingDirectoryResolver>();

        services.AddScoped<IHashService, Sha256HashService>();

        services.AddScoped<IImageProcessor, ImageSharpProcessor>();

        services.AddScoped<IManifestRepository>(sp =>
            new JsonManifestRepository(
                sp.GetRequiredService<IWorkingDirectoryResolver>().GetManifestDirectory()));

        services.AddScoped<IInputScanner, InputScanner>()
                .AddScoped<IOutputWriter, OutputWriter>();
        
        services.AddScoped<ISettingsRepository, SqliteSettingsRepository>();
        
        services.AddScoped<SqliteLogRepository>();
        services.AddScoped<FileLogSink>();
        
        services.AddScoped<ILogRepository, CompositeLogger>()
                .AddScoped<IFileIndexRepository, SqliteFileIndexRepository>()
                .AddScoped<IRunRepository, SqliteRunRepository>()
                .AddScoped<IPreprocessCacheRepository, SqlitePreprocessCacheRepository>();

        services.AddScoped<IProcessingOrchestrator, ProcessingOrchestrator>();
        
        services.AddScoped<PreprocessWorker>();
        services.AddScoped<WatcherService>();
        
        return services;
    }

    public static IServiceCollection AddTypeHandlers(this IServiceCollection services)
    {
        SqlMapper.AddTypeHandler<Resolution>(new ResolutionTypeHandler());
        return services;
    } 
}
