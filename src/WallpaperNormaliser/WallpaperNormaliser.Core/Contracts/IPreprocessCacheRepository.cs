using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Models.Cache;

namespace WallpaperNormaliser.Core.Contracts;
public interface IPreprocessCacheRepository
{
    Task<PreprocessCacheEntry?> GetAsync(string sourceHash, CancellationToken cancellationToken = default);
    Task UpsertAsync(PreprocessCacheEntry entry, CancellationToken cancellationToken = default);
    Task RemoveByHashAsync(string sourceHash, CancellationToken cancellationToken = default);
    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}