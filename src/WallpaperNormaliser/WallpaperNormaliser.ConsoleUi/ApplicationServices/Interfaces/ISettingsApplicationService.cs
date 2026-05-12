using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.ConsoleUi.Models;
using WallpaperNormaliser.ConsoleUi.Models.ViewModels;

namespace WallpaperNormaliser.ConsoleUi.ApplicationServices.Interfaces;
public interface ISettingsApplicationService
{
    Task<SettingsViewModel> GetSettingsAsync();
    Task SaveSettingsAsync(SettingsViewModel toSave);
}
