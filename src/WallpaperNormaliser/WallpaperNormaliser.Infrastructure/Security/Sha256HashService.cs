using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Infrastructure.Security;
public sealed class Sha256HashService : IHashService
{
    public Task<string> ComputeAsync(FileContext file, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var sha = SHA256.Create();

        var hash = sha.ComputeHash(file.Bytes);

        return Task.FromResult(Convert.ToHexString(hash).ToLowerInvariant());
    }
}
