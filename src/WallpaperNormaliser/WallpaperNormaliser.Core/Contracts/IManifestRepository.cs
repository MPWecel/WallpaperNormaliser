using WallpaperNormaliser.Core.Models.Manifest;

namespace WallpaperNormaliser.Core.Contracts;
public interface IManifestRepository
{
    Task<ManifestDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ManifestDocument?> GetByNameAsync(string fileName, CancellationToken cancellationToken = default);
    Task<ManifestDocument?> GetByHashAsync(string hash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManifestDocument>> GetManyAsync(ManifestQuery query, CancellationToken cancellationToken = default);
    Task SaveAsync(ManifestDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
}
