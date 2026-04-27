using WallpaperNormaliser.Core.Enums;

namespace WallpaperNormaliser.Core.Models.Logging;
public sealed record LogEntry(
                                Guid Id,
                                DateTimeOffset CreatedAtUtc,
                                ELogSeverity Severity,
                                string Category,
                                string Message,
                                string? CorrelationId,
                                string? SourceHash,
                                string? ExceptionMessage
                             );
