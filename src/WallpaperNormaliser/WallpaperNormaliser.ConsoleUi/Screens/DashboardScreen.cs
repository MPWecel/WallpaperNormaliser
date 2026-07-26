using Spectre.Console;

using WallpaperNormaliser.ConsoleUi.Models.ViewModels;
using WallpaperNormaliser.ConsoleUi.Services;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class DashboardScreen(WorkingDirectoryResolver resolver, ISettingsRepository settingsRepository)
{
    private readonly WorkingDirectoryResolver _paths = resolver;
    private readonly ISettingsRepository _settingsRepository = settingsRepository;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(DashboardConstants.Title);

        AppSettings settings = await _settingsRepository.GetAsync();
        string root = _paths.GetRoot();

        SearchOption inputSearch = settings.ScanSettings.IsRecursive
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        DirectoryStatusViewModel[] statuses =
        [
            BuildStatus(root, DashboardConstants.FolderInput,    inputSearch),
            BuildStatus(root, DashboardConstants.FolderOutput,   SearchOption.AllDirectories),
            BuildStatus(root, DashboardConstants.FolderManifest, SearchOption.TopDirectoryOnly),
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

    private static DirectoryStatusViewModel BuildStatus(string rootDirectory, string folderName, SearchOption searchOption)
    {
        string path = Path.Combine(rootDirectory, folderName);
        bool exists = Directory.Exists(path);
        if (!exists)
            return new DirectoryStatusViewModel(folderName, false, 0);

        try
        {
            int count = Directory.EnumerateFiles(path, "*", searchOption).Count();
            return new DirectoryStatusViewModel(folderName, true, count);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return new DirectoryStatusViewModel(folderName, true, -1);
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
