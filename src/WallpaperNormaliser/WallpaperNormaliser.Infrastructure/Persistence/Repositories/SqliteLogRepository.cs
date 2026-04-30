using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Logging;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteLogRepository : ILogRepository
{
    public Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    
    public Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default) => throw new NotImplementedException();

}
