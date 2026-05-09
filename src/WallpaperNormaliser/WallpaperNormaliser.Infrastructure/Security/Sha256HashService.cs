using System.Security.Cryptography;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Infrastructure.Security;
public sealed class Sha256HashService : IHashService
{
    public Task<string> ComputeAsync(FileContext file, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using SHA256 sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(file.Bytes);
        string hashHexString = Convert.ToHexString(hash).ToLowerInvariant();

        return Task.FromResult(hashHexString);
    }
}
