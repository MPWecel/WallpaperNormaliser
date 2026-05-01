using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Enums;


namespace WallpaperNormaliser.Core.Models.Processing;
public sealed record ProcessingRun(
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
    public static ProcessingRun? FromString(string input)
    {
        char entrySeparator = ';';
        char keyvalSeparator = ':';
        StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        IEnumerable<string> inputChunks = input.Split(entrySeparator, options);
        Func<string, KeyValuePair<string,string>> parseEntry = 
                                                               (string entry) => 
                                                                               { 
                                                                                   string[] entryChunks = entry.Split(keyvalSeparator, options);
                                                                                   return new KeyValuePair<string, string>(entryChunks[0], entryChunks[1]);
                                                                               };
        Dictionary<string, string> entries = inputChunks.Select(parseEntry)
                                                        .ToDictionary(x=>x.Key, x => x.Value);

        const string runIdKey = "RunId";
        string RunIdValue = entries[runIdKey];
        bool parseResult = entries.TryGetValue(runIdKey, out string runId);


        return new ProcessingRun(RunIdValue)
    }
}
