using WallpaperNormaliser.Core.Models.Cache;
using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Contracts;
public interface IPreprocessCacheRepository
{
    Task<PreprocessCacheEntry?> GetAsync(string sourceHash, Resolution resolution, int quality, CancellationToken cancellationToken = default);
    Task UpsertAsync(PreprocessCacheEntry entry, CancellationToken cancellationToken = default);
    Task RemoveByHashAsync(string sourceHash, CancellationToken cancellationToken = default);
    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
