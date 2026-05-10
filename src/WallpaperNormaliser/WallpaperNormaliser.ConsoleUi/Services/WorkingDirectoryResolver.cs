using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace WallpaperNormaliser.ConsoleUi.Services;
public sealed class WorkingDirectoryResolver
{
    public string GetRoot() => Path.Combine(AppContext.BaseDirectory, "APPLICATION_WORKING_DIRECTORY");
}
