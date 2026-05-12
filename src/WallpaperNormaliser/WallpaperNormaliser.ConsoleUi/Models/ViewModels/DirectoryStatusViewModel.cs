using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperNormaliser.ConsoleUi.Models.ViewModels;
public sealed record DirectoryStatusViewModel(string Name, bool Exists);
