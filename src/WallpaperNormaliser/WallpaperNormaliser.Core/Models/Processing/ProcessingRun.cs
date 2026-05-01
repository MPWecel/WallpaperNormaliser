using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Enums;


namespace WallpaperNormaliser.Core.Models.Processing;
public sealed record ProcessingRun(
                                      string Id,
                                      DateTime StartedUtc,
                                      DateTime? FinishedUtc,
                                      EProcessingStatus Status,
                                      int TotalFiles,
                                      int SuccessCount,
                                      int FailedCount,
                                      int SkippedCount
                                  )
{
    public override string ToString() => new StringBuilder(8).AppendFormat("RunId: {0};", Id)
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
        bool parseResult = entries.TryGetValue(runIdKey, out string runIdValue);
        const string startedUtcKey = "StartedUtc";
        parseResult &= entries.TryGetValue(startedUtcKey, out string startedTimestampString);
        parseResult &= DateTime.TryParse(startedTimestampString, out DateTime startedUtcValue);
        
        const string finishedUtcKey = "FinishedUtc";
        parseResult &= entries.TryGetValue(finishedUtcKey, out string finishedTimestampString);
        DateTime? finishedUtcNullableValue;
        
        if(String.IsNullOrEmpty(finishedUtcKey) || finishedUtcKey!.Equals("null"))
        {
            finishedUtcNullableValue = (DateTime?)(null); 
        }
        else
        {
            parseResult &= DateTime.TryParse(finishedTimestampString, out DateTime finishedUtcValue);
            finishedUtcNullableValue = (DateTime?)(finishedUtcValue);
        }
        
        const string StatusKey = "Status";
        parseResult &= entries.TryGetValue(StatusKey, out string statusValueString);
        parseResult &= Int32.TryParse(statusValueString, out int statusValueInt);
        EProcessingStatus statueValueEnum = (EProcessingStatus)(statusValueInt);

        const string TotalFilesKey = "TotalFiles";
        parseResult &= entries.TryGetValue(TotalFilesKey, out string TotalFilesValueString);
        parseResult &= Int32.TryParse(TotalFilesValueString, out int TotalFilesValueInt);

        const string successCountKey = "SuccessCount";
        parseResult &= entries.TryGetValue(successCountKey, out string successCountValueString);
        parseResult &= Int32.TryParse(successCountValueString, out int successCountValueInt);

        const string failedCountKey = "FailedCount";
        parseResult &= entries.TryGetValue(failedCountKey, out string failedCountValueString);
        parseResult &= Int32.TryParse(failedCountValueString, out int failedCountValueInt);

        const string skippedCountKey = "SkippedCount";
        parseResult &= entries.TryGetValue(skippedCountKey, out string skippedCountValueString);
        parseResult &= Int32.TryParse(skippedCountValueString, out int skippedCountValueInt);

        if (!parseResult)
            return null;


        return new ProcessingRun(
                                    runIdValue,
                                    startedUtcValue,
                                    finishedUtcNullableValue,
                                    statueValueEnum,
                                    TotalFilesValueInt,
                                    successCountValueInt,
                                    failedCountValueInt,
                                    skippedCountValueInt
                                );
    }
}
