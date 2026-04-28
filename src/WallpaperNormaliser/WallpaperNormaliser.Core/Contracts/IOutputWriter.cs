using WallpaperNormaliser.Core.Models.Output;

namespace WallpaperNormaliser.Core.Contracts;
public interface IOutputWriter
{
    Task<OutputWriteResult> WriteAsync(OutputWriteRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutputWriteResult>> WriteManyAsync(IEnumerable<OutputWriteRequest> requests, CancellationToken cancellationToken = default);
}
