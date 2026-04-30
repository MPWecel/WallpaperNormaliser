using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class MigrationRunner
{
    public Task RunAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
