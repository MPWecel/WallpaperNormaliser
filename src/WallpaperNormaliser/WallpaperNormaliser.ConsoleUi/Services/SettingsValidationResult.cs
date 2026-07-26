namespace WallpaperNormaliser.ConsoleUi.Services;
public sealed record SettingsValidationResult(bool IsValid, IReadOnlyList<string> Errors);
