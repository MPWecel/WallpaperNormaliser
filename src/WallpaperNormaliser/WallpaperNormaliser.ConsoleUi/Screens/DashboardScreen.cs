using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;

using WallpaperNormaliser.ConsoleUi.Services;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class DashboardScreen
{
    private readonly WorkingDirectoryResolver _paths;

    public DashboardScreen(WorkingDirectoryResolver resolver)
    {
        _paths = resolver;
    }

    public Task ShowAsync()
    {
        AnsiConsole.Clear();
        var root = _paths.GetRoot();
        Table table = new();

        AnsiConsole.Write(table);
        Console.ReadKey(true);

        return Task.CompletedTask;
    }
}
