namespace WallpaperNormaliser.Core.Events;
public sealed class FileRemovedEventArgs(string fileName, string? fullPath) : EventArgs
{
    public string FileName  { get; } = fileName;
    public string? FullPath { get; } = fullPath;
}
