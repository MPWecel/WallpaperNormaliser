using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Core.Contracts;
public interface ISettingsRepository
{
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
    Task<string> ExportJsonAsync(CancellationToken cancellationToken = default);
    Task ImportJsonAsync(string json, CancellationToken cancellationToken = default);
}
