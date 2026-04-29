using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Manifest;

namespace WallpaperNormaliser.Infrastructure.FileSystem;
public class JsonManifestRepository : IManifestRepository
{
    public Task<ManifestDocument?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ManifestDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ManifestDocument?> GetByNameAsync(string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ManifestDocument>> GetManyAsync(ManifestQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(ManifestDocument document, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
