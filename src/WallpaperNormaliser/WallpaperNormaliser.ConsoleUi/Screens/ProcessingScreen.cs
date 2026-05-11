using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;
using WallpaperNormaliser.ConsoleUi.Services;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Orchestration;
using WallpaperNormaliser.Core.Models.Processing;
using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class ProcessingScreen(IProcessingOrchestrator orchestrator, SettingsValidator settingsValidator)
{
    private readonly IProcessingOrchestrator _orchestrator = orchestrator;
    private readonly SettingsValidator _settingsValidator = settingsValidator;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();

        bool validationResult = _settingsValidator.Validate();

        if(!validationResult)
        {
            AnsiConsole.MarkupLine(ProcessingConstants.SettingsInvalidWarning);
        }
        else
        {
            var processingTask = AnsiConsole.Status()
                                            .Start(
                                                      ProcessingConstants.ProcessingWaitText,
                                                      async x=> await AnsiConsole.Progress()
                                                                           .StartAsync(
                                                                                          async x =>
                                                                                          {
                                                                                              var task = x.AddTask(ProcessingConstants.ProcessingImagesTaskName);
                                                                                              task.Increment(25);
                                                                                              ProcessRequest request = new(
                                                                                                                              OverwriteMode: EOverwriteMode.Skip,
                                                                                                                              ScanOptions: new("", false, false, false),
                                                                                                                              ProcessingOptions: new(TargetResolution: new(1920,1080), 100, true, true, 640,480, false)
                                                                                                                          );
                                                                                              await _orchestrator.RunAsync(request);
                                                                                              task.Value = 100;
                                                                                          }
                                                                                      )                                                  );
            await processingTask!.ConfigureAwait(false);

            AnsiConsole.MarkupLine(ProcessingConstants.ProcessingSuccessfulInfo);
            AnsiConsole.MarkupLine(ProcessingConstants.ProcessingSuccessfulSummary);
        }

        Console.ReadKey(true);
        return;
    }
}

internal static class ProcessingConstants
{
    internal const string SettingsInvalidWarning = "[yellow]Settings invalid. Wizard required in next iteration.[/]";
    internal const string ProcessingWaitText = "Processing...";
    internal const string ProcessingImagesTaskName = "Processing images";
    internal const string ProcessingSuccessfulInfo = "[green]Processing completed.[/]";
    internal const string ProcessingSuccessfulSummary = "Run Summary: processed batch finished.";
}
