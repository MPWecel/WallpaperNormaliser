using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Enums;


namespace WallpaperNormaliser.Core.Models.Processing;
public sealed record ProcessingRunItem(
                                          string Id,
                                          string RunId,
                                          string SourceHash,
                                          string FileName,
                                          EProcessingStatus Status,
                                          string? Message,
                                          int? DurationMs,
                                          DateTime CreatedUtc
                                      )
{
    public override string ToString() => new StringBuilder(8).AppendFormat("Id: {0};", Id)
                                                             .AppendFormat("RunId: {0};", RunId)
                                                             .AppendFormat("SourceHash: {0};", SourceHash)
                                                             .AppendFormat("FileName: {0};", FileName)
                                                             .AppendFormat("Status: {0}", (int)(Status))
                                                             .AppendFormat("Message: {0}", Message)
                                                             .AppendFormat("DurationMs: {0}", DurationMs?.ToString() ?? "null")
                                                             .AppendFormat("CreatedUtc: {0}", CreatedUtc.ToString("yyyy-MM-dd HH:mm:ss.ff"))
                                                             .AppendFormat("")
                                                             .ToString();
}
