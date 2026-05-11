namespace WallpaperNormaliser.ConsoleUi.Services;
public sealed record StartupValidationResult(bool IsValid, IReadOnlyList<string> Errors);
