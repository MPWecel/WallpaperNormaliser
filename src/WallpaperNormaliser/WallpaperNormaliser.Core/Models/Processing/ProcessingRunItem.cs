using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Enums;


namespace WallpaperNormaliser.Core.Models.Processing;
public sealed record ProcessingRunItem(
                                          string RunId,
                                          DateTime StartedUtc,
                                          DateTime? FinishedUtc,
                                          EProcessingStatus Status,
                                          int TotalFiles,
                                          int SuccessCount,
                                          int FailedCount,
                                          int SkippedCount
                                      )
{
    public override string ToString() => new StringBuilder(8).AppendFormat("RunId: {0};", RunId)
                                                             .AppendFormat("StartedUtc: {0};", StartedUtc.ToString("yyyy-MM-dd HH:mm:ss.ff"))
                                                             .AppendFormat("FinishedUtc: {0}", FinishedUtc?.ToString("yyyy-MM-dd HH:mm:ss.ff") ?? "null")
                                                             .AppendFormat("")
                                                             .ToString();
}
