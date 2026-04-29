using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Infrastructure.Security;
public class Sha256HashService : IHashService
{
    public Task<string> ComputeAsync(FileContext file, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
