using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Output;

namespace WallpaperNormaliser.Infrastructure.FileSystem;
public class OutputWriter : IOutputWriter
{
    public Task<OutputWriteResult> WriteAsync(OutputWriteRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<OutputWriteResult>> WriteManyAsync(IEnumerable<OutputWriteRequest> requests, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
