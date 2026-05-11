namespace WallpaperNormaliser.ConsoleUi.Rendering;
public sealed class ThemeMarkupProvider
{
    public string Header(string text)   => $"[{Theme.HeaderStyle}]{text}[/]";
    public string Success(string text)  => $"[{Theme.SuccessStyle}]{text}[/]";
    public string Warning(string text)  => $"[{Theme.WarningStyle}]{text}[/]";
    public string Error(string text)    => $"[{Theme.ErrorStyle}]{text}[/]";
    public string Muted(string text)    => $"[{Theme.MutedStyle}]{text}[/]";
}
