using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Settings;


namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class SettingsScreen
{
    private readonly ISettingsRepository _settingsRepository;

    public SettingsScreen(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task ShowAsync()
    {
        while(true)
        {
            AnsiConsole.Clear();

            AppSettings settings = await _settingsRepository.GetAsync();

            Resolution resolution = settings.Resolution;

            Table table = new();
            table.AddColumn("SETTING")
                 .AddColumn("VALUE");

            table.AddRow("Resolution", settings.Resolution.ToString())
                 .AddRow("Quality", settings.Quality.ToString());

            AnsiConsole.Write(table);

            string choice = AnsiConsole.Prompt(
                                                  new SelectionPrompt<string>().Title("")
                                                                               .AddChoices(
                                                                                              "[1] Change resolution",
                                                                                              "[2] Change quality",
                                                                                              "[3] Back."
                                                                                          )
                                              );

            switch(choice)
            {
                case "Change resolution":
                    uint width = AnsiConsole.Ask<uint>("Target width: ", resolution.Width);
                    uint height = AnsiConsole.Ask<uint>("Target height: ", resolution.Height);

                    settings = settings with { Resolution = new(width, height) };
                    await _settingsRepository.SaveAsync(settings);
                    AnsiConsole.MarkupLine($"[green]Saved resolution {settings.Resolution.ToString()}[/]");
                    break;
                case "Change quality":
                    break;
                case "Back":
                    return;
                default:
                    return;
            }

        }
    }
}
