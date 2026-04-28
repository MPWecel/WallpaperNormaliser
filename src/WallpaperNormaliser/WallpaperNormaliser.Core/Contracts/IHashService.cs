using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Contracts;
public interface IHashService
{
    Task<string> ComputeAsync(FileContext file, CancellationToken cancellationToken = default);
}
