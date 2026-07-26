using Spectre.Console;
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
    ISettingsValidator settingsValidator,
    ISettingsRepository settingsRepository,
    IWorkingDirectoryResolver workingDirectoryResolver)
{
    private readonly IProcessingOrchestrator _orchestrator = orchestrator;
    private readonly ISettingsValidator _settingsValidator = settingsValidator;
    private readonly ISettingsRepository _settingsRepository = settingsRepository;
    private readonly IWorkingDirectoryResolver _workingDirectoryResolver = workingDirectoryResolver;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();

        AppSettings settings = await _settingsRepository.GetAsync();
        SettingsValidationResult validation = _settingsValidator.Validate(settings);

        if (!validation.IsValid)
        {
            AnsiConsole.MarkupLine(ProcessingConstants.SettingsInvalidWarning);
            foreach (string err in validation.Errors)
                AnsiConsole.MarkupLine($"[yellow]  - {err}[/]");
        }
        else
        {
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
    internal const string SettingsInvalidWarning      = "[yellow]Settings invalid:[/]";
    internal const string ProcessingWaitText          = "Processing...";
    internal const string ProcessingImagesTaskName    = "Processing images";
    internal const string ProcessingSuccessfulInfo    = "[green]Processing completed.[/]";
    internal const string ProcessingSuccessfulSummary = "Run Summary: processed batch finished.";
}
