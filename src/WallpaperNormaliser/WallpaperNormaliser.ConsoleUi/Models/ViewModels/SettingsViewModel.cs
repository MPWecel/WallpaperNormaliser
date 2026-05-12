using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core;
using WallpaperNormaliser.Core.Models;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.ConsoleUi.Models.ViewModels;
public sealed record SettingsViewModel(uint Width, uint Height, int Quality)
{
    public SettingsViewModel CreateUpdatedViewModel(uint? width = null, uint? height = null, int? quality = null)
    {
        uint newWidth = width ?? this.Width;
        uint newHeight = height ?? this.Height;
        int newQuality = quality ?? this.Quality;

        SettingsViewModel result = new(newWidth, newHeight, newQuality);
        return result;
    }

    public static SettingsViewModel FromDomainEntity(AppSettings appSettings)
        => new(appSettings.Resolution.Width, appSettings.Resolution.Height, appSettings.Quality);

    public AppSettings ToDomainEntity(AppSettings original) 
        => original with { Resolution = new(Width, Height), Quality = this.Quality };

    public string ResolutionString => $"{Width}x{Height}";
    public string QualityString => Quality.ToString();
}
