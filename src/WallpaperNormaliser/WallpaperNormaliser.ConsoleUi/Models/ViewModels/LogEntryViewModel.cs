using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core;
using WallpaperNormaliser.Core.Models;
using WallpaperNormaliser.Core.Models.Logging;

namespace WallpaperNormaliser.ConsoleUi.Models.ViewModels;
public sealed record LogEntryViewModel(DateTime TimeStampUtc, string Severity, string Message)
{
    public static LogEntryViewModel FromDomainEntity(LogEntry entry)
    {
        LogEntryViewModel result = new(entry.CreationDateUtc.UtcDateTime, entry.Severity.ToString(), entry.Message);
        return result;
    }

    public LogEntry ToDomainEntity(LogEntry original)
    {
        LogEntry updated = original with { CreationDateUtc = this.TimeStampUtc, Severity = original.Severity, Message = this.Message};
        return updated;
    }
}
