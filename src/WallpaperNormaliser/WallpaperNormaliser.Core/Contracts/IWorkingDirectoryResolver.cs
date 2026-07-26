namespace WallpaperNormaliser.Core.Contracts;
public interface IWorkingDirectoryResolver
{
    string GetRoot();
    string GetInputDirectory();
    string GetOutputDirectory();
    string GetManifestDirectory();
}
