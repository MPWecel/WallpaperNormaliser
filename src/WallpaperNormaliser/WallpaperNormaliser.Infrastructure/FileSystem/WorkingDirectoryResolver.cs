using WallpaperNormaliser.Core.Contracts;

namespace WallpaperNormaliser.Infrastructure.FileSystem;
public sealed class WorkingDirectoryResolver : IWorkingDirectoryResolver
{
    // Root is currently hardcoded. AppSettings.RootDirectory still exists on the settings
    // record but is no longer consulted by this resolver — no UI editor exposes it, and
    // making the resolver settings-driven would require an async signature that ripples
    // to every caller. Deferred as a follow-up.
    private const string WorkingDirectoryName     = "APPLICATION_WORKING_DIRECTORY";
    private const string InputSubDirectoryName    = "INPUT";
    private const string OutputSubDirectoryName   = "OUTPUT";
    private const string ManifestSubDirectoryName = "MANIFEST";

    public string GetRoot()              => Path.Combine(AppContext.BaseDirectory, WorkingDirectoryName);
    public string GetInputDirectory()    => Path.Combine(GetRoot(), InputSubDirectoryName);
    public string GetOutputDirectory()   => Path.Combine(GetRoot(), OutputSubDirectoryName);
    public string GetManifestDirectory() => Path.Combine(GetRoot(), ManifestSubDirectoryName);
}
