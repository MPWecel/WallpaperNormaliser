namespace WallpaperNormaliser.Core.Models.Settings;
public sealed record SettingsValidationResult(bool IsValid, IReadOnlyList<string> Errors);
