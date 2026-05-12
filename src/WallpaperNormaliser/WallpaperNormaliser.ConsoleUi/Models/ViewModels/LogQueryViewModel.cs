using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core;
using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models;
using WallpaperNormaliser.Core.Models.Logging;

namespace WallpaperNormaliser.ConsoleUi.Models.ViewModels;
public sealed record LogQueryViewModel(string? SearchText = null, ELogSeverity? MinimumSeverity = null)
{
    public static LogQueryViewModel FromDomainEntity(LogQuery entity) => new(entity.CorrelationId, entity.MinimumSeverity);
    public LogQuery ToDomainEntity() => new(null, null, this.MinimumSeverity, SearchText, null);
}
