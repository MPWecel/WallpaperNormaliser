using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Models.Processing;

namespace WallpaperNormaliser.Core.Contracts;
public interface IRunRepository
{
    Task<ProcessingRun?> GetRunAsync(string runId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProcessingRunItem>> GetRunItemsAsync(string runId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProcessingRun>> GetRecentRunsAsync(int take, CancellationToken cancellationToken = default);
    Task CreateRunAsync(ProcessingRun run, CancellationToken cancellationToken = default);
    Task UpsertRunAsync(ProcessingRun run, CancellationToken cancellationToken = default);
    Task AddRunItemAsync(ProcessingRunItem item, CancellationToken cancellationToken = default);
    Task FinaliseRunAsync(ProcessingRun run, CancellationToken cancellationToken = default);
}
