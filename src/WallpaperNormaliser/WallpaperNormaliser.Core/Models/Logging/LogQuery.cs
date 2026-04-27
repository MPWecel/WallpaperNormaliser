using WallpaperNormaliser.Core.Enums;

namespace WallpaperNormaliser.Core.Models.Logging;
public sealed record LogQuery(
                                DateTimeOffset? DateRangeFromUtc,
                                DateTimeOffset? DateRangeToUtc,
                                ELogSeverity? MinimumSeverity,
                                string? CorrelationId,
                                string? SourceHash,
                                int Limit
                             );
