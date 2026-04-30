using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Infrastructure.FileSystem;
using WallpaperNormaliser.Infrastructure.Imaging;
using WallpaperNormaliser.Infrastructure.Persistence.Repositories;
using WallpaperNormaliser.Infrastructure.Persistence.TypeHandlers;
using WallpaperNormaliser.Infrastructure.Processing;
using WallpaperNormaliser.Infrastructure.Security;

namespace WallpaperNormaliser.Infrastructure.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsRepository, SqliteSettingsRepository>();
        services.AddSingleton<ILogRepository, SqliteLogRepository>();
        services.AddSingleton<IManifestRepository, JsonManifestRepository>();
        services.AddSingleton<IHashService, Sha256HashService>();
        services.AddSingleton<IImageProcessor, ImageSharpProcessor>();
        services.AddSingleton<IInputScanner, InputScanner>();
        services.AddSingleton<IOutputWriter, OutputWriter>();
        services.AddSingleton<IProcessingOrchestrator, ProcessingOrchestrator>();

        SqlMapper.AddTypeHandler<Resolution>(new ResolutionTypeHandler());

        return services;
    }
}
