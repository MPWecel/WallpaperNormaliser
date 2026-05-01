using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Models.Indexing;

namespace WallpaperNormaliser.Core.Contracts;
public interface IFileIndexRepository
{
    Task<FileIndexEntry?> GetByHashAsync(string hash, CancellationToken cancellationToken = default);

    Task<FileIndexEntry?> GetByRelativePathAsync(string relativePath, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileIndexEntry>> GetDuplicatesAsync(string hash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileIndexEntry>> ListAsync(CancellationToken cancellationToken = default);

    Task UpsertAsync(FileIndexEntry entry, CancellationToken cancellationToken = default);

    Task UpsertManyAsync(IReadOnlyCollection<FileIndexEntry> entries, CancellationToken cancellationToken = default);

    Task RemoveMissingAsync(IReadOnlyCollection<string> existingRelativePaths, CancellationToken cancellationToken = default);
}