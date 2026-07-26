using Spectre.Console;

using WallpaperNormaliser.ConsoleUi.Models.ViewModels;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class DashboardScreen(IWorkingDirectoryResolver resolver, ISettingsRepository settingsRepository)
{
    private readonly IWorkingDirectoryResolver _paths = resolver;
    private readonly ISettingsRepository _settingsRepository = settingsRepository;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(DashboardConstants.Title);

        AppSettings settings = await _settingsRepository.GetAsync();

        SearchOption inputSearch = settings.ScanSettings.IsRecursive
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        DirectoryStatusViewModel[] statuses =
        [
            BuildStatus(DashboardConstants.FolderInput,    _paths.GetInputDirectory(),    inputSearch),
            BuildStatus(DashboardConstants.FolderOutput,   _paths.GetOutputDirectory(),   SearchOption.AllDirectories),
            BuildStatus(DashboardConstants.FolderManifest, _paths.GetManifestDirectory(), SearchOption.TopDirectoryOnly),
        ];

        Table table = new();
        table.AddColumn(DashboardConstants.TableColumnHeader_Folders)
             .AddColumn(DashboardConstants.TableColumnHeader_Exists)
             .AddColumn(DashboardConstants.TableColumnHeader_FileCount);

        foreach (DirectoryStatusViewModel status in statuses)
        {
            string countCell = !status.Exists       ? DashboardConstants.LabelDash
                             : status.FileCount < 0 ? DashboardConstants.LabelUnknown
                                                    : status.FileCount.ToString();

            table.AddRow(
                            status.Name,
                            status.Exists ? DashboardConstants.LabelYes : DashboardConstants.LabelNo,
                            countCell
                        );
        }

        AnsiConsole.Write(table);
        Console.ReadKey(true);
    }

    private static DirectoryStatusViewModel BuildStatus(string label, string path, SearchOption searchOption)
    {
        bool exists = Directory.Exists(path);
        if (!exists)
            return new DirectoryStatusViewModel(label, false, 0);

        try
        {
            int count = Directory.EnumerateFiles(path, "*", searchOption).Count();
            return new DirectoryStatusViewModel(label, true, count);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return new DirectoryStatusViewModel(label, true, -1);
        }
    }
}

internal static class DashboardConstants
{
    internal const string Title = "[grey]Dashboard[/]";

    internal const string TableColumnHeader_Folders   = "FOLDER";
    internal const string TableColumnHeader_Exists    = "EXISTS";
    internal const string TableColumnHeader_FileCount = "FILES";

    internal const string FolderInput    = "INPUT";
    internal const string FolderOutput   = "OUTPUT";
    internal const string FolderManifest = "MANIFEST";

    internal const string LabelYes     = "Yes";
    internal const string LabelNo      = "No";
    internal const string LabelDash    = "-";
    internal const string LabelUnknown = "?";
}
