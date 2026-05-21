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
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class ProcessingScreen(
    IProcessingOrchestrator orchestrator,
    SettingsValidator settingsValidator,
    ISettingsRepository settingsRepository,
    WorkingDirectoryResolver workingDirectoryResolver)
{
    private readonly IProcessingOrchestrator _orchestrator = orchestrator;
    private readonly SettingsValidator _settingsValidator = settingsValidator;
    private readonly ISettingsRepository _settingsRepository = settingsRepository;
    private readonly WorkingDirectoryResolver _workingDirectoryResolver = workingDirectoryResolver;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();

        bool validationResult = _settingsValidator.Validate();

        if (!validationResult)
        {
            AnsiConsole.MarkupLine(ProcessingConstants.SettingsInvalidWarning);
        }
        else
        {
            AppSettings settings = await _settingsRepository.GetAsync();
            string inputDirectory = _workingDirectoryResolver.GetInputDirectory();

            ProcessRequest request = new(
                                            ScanOptions: new(
                                                                InputDirectory:    inputDirectory,
                                                                IsRecursive:       settings.ScanSettings.IsRecursive,
                                                                IsRaiseEventsOn:   false,
                                                                IsComputeHashesOn: false
                                                            ),
                                            ProcessingOptions: new(
                                                                      TargetResolution:     settings.Resolution,
                                                                      JpegQuality:          settings.Quality,
                                                                      ApplyExifOrientation: true,
                                                                      WarnOnSmallImages:    true,
                                                                      MinimumWidth:         640,
                                                                      MinimumHeight:        480,
                                                                      DryRun:               false
                                                                  ),
                                            OverwriteMode: EOverwriteMode.Skip
                                        );

            var processingTask = AnsiConsole.Status()
                                            .Start(
                                                      ProcessingConstants.ProcessingWaitText,
                                                      async x => await AnsiConsole.Progress()
                                                                                  .StartAsync(
                                                                                                 async ctx =>
                                                                                                 {
                                                                                                     var task = ctx.AddTask(ProcessingConstants.ProcessingImagesTaskName);
                                                                                                     task.Increment(25);
                                                                                                     await _orchestrator.RunAsync(request);
                                                                                                     task.Value = 100;
                                                                                                 }
                                                                                             )
                                                  );
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
    internal const string SettingsInvalidWarning      = "[yellow]Settings invalid. Wizard required in next iteration.[/]";
    internal const string ProcessingWaitText          = "Processing...";
    internal const string ProcessingImagesTaskName    = "Processing images";
    internal const string ProcessingSuccessfulInfo    = "[green]Processing completed.[/]";
    internal const string ProcessingSuccessfulSummary = "Run Summary: processed batch finished.";
}
