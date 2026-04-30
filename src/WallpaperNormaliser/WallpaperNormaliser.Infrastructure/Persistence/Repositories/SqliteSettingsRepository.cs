using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteSettingsRepository : ISettingsRepository
{
    public Task<AppSettings> GetAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    
    public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<string> ExportJsonAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task ImportJsonAsync(string json, CancellationToken cancellationToken = default) => throw new NotImplementedException();

}
