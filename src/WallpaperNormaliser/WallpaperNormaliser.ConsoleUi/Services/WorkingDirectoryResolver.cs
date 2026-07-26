namespace WallpaperNormaliser.ConsoleUi.Services;
public sealed class WorkingDirectoryResolver
{
    private readonly string _workingDirectory = "APPLICATION_WORKING_DIRECTORY";
    private readonly string _inputDirectory = "INPUT";

    public string GetRoot() => Path.Combine(AppContext.BaseDirectory, _workingDirectory);

    public string GetInputDirectory() => Path.Combine(GetRoot(), _inputDirectory);
}
