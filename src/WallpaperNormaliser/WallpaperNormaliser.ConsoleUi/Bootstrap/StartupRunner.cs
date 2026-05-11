using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using WallpaperNormaliser.ConsoleUi.Navigation;

namespace WallpaperNormaliser.ConsoleUi.Bootstrap;
public sealed class StartupRunner
{
    private readonly MainMenu _menu;

    public StartupRunner(MainMenu menu)
    {
        _menu = menu;
    }

    public async Task RunAsync(IServiceProvider provider)
    {
        try
        {
            RenderHeader();
            await _menu.ShowAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"{StartupRunnerConstants.ExceptionMessage} {ex.Message}");
        }
    }

    private static void RenderHeader()
    {
        AnsiConsole.Write(new Rule(StartupRunnerConstants.Title));
    }
}

internal static class StartupRunnerConstants
{
    internal const string Title = "[grey]WALLPAPER NORMALISER";
    internal const string ExceptionMessage = "[red]FatalError:\t[/]";
}