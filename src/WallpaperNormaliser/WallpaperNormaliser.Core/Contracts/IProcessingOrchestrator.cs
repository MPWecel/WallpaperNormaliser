using WallpaperNormaliser.Core.Models.Orchestration;

namespace WallpaperNormaliser.Core.Contracts;
public interface IProcessingOrchestrator
{
    Task<BatchProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken = default);
}
