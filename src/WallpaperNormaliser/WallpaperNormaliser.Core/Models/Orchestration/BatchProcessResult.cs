namespace WallpaperNormaliser.Core.Models.Orchestration;
public sealed record BatchProcessResult(
                                            string CorrelationId,
                                            IReadOnlyList<FileProcessResult> Items,
                                            int SuccessCount,
                                            int FailedCount,
                                            int SkippedCount,
                                            TimeSpan Duration
                                       );
