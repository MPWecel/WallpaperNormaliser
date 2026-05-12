using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using WallpaperNormaliser.ConsoleUi.Navigation;
using WallpaperNormaliser.ConsoleUi.Rendering;
using WallpaperNormaliser.ConsoleUi.Services;

namespace WallpaperNormaliser.ConsoleUi.Bootstrap;
public sealed class StartupRunner(MainMenu menu, StartupValidator validator, ThemeMarkupProvider themeMarkupProvider)
{
    private readonly MainMenu _menu = menu;
    private readonly StartupValidator _validator = validator;
    private readonly ThemeMarkupProvider _themeMarkupProvider = themeMarkupProvider;

    public async Task RunAsync(IServiceProvider provider)
    {
        try
        {
            RenderHeader();
            StartupValidationResult validationResult = await _validator.ValidateAsync();
            if (!validationResult.IsValid)
            {
                RenderValidationErrors(validationResult.Errors);
                return;
            }

            await _menu.ShowAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(_themeMarkupProvider.Error($"{StartupRunnerConstants.ExceptionMessage}{ex.Message}"));
        }
    }

    private void RenderHeader() => AnsiConsole.Write(new Rule(_themeMarkupProvider.Header(StartupRunnerConstants.Title)));
    private void RenderValidationErrors(IReadOnlyList<string> errors)
    {
        foreach(var error in errors)
            AnsiConsole.MarkupLine(_themeMarkupProvider.Error(error));

        AnsiConsole.Console.Input.ReadKey(false);
    }
}

internal static class StartupRunnerConstants
{
    internal const string Title = "WALLPAPER NORMALISER";
    internal const string ExceptionMessage = "FatalError:\t";
}