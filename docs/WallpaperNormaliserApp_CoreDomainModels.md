# CoreDomainModels.md

# WallpaperNormaliserApp - Core Domain Models

## 1. Enums::
```csharp
public enum EFileFormat        { Unknown=0, Jpeg=1,    Png=2,           Bmp=3,          Gif=4,                 Tiff=5,                 Webp=6 }
public enum ELogSeverity       { Trace=0,   Debug=1,   Information=2,   Warning=3,      Error=4,               Critical=5 }
public enum EOverwriteMode     { Skip=0,    SkipAll=1, Replace=2,       ReplaceAll=3,   SaveWithTimestamp=4,   SaveAllWithTimestamp=5 }
public enum EScanMode          { Manual=0,  Watch=1 }
public enum EProcessingStatus  { Pending=0, Running=1, Completed=2,     Skipped=3,      Failed=4,              Cached=5 }
```

## 2. Value Objects::
```csharp
public sealed record FileFormatInfo(EFileFormat Format)
{
	public string Extension =>	Format switch
								{
									EFileFormat.Jpeg => "jpg",
									EFileFormat.Png  => "png",
									EFileFormat.Bmp  => "bmp",
									EFileFormat.Gif  => "gif",
									EFileFormat.Tiff => "tiff",
									EFileFormat.Webp => "webp",
									_                => "bin"
								};

	public string MimeType =>	Format switch
								{
									EFileFormat.Jpeg => "image/jpeg",
									EFileFormat.Png  => "image/png",
									EFileFormat.Bmp  => "image/bmp",
									EFileFormat.Gif  => "image/gif",
									EFileFormat.Tiff => "image/tiff",
									EFileFormat.Webp => "image/webp",
									_                => "application/octet-stream"
								};

	public static FileFormatInfo FromExtension(string ext) { ... }
	public static FileFormatInfo FromMimeType(string mime) { ... }
}

public sealed record Resolution(uint Width, uint Height)
{
	public static Resolution FromString(string s) { ... }   // parses "1920x1080"
	public override string ToString() => $"{Width}x{Height}";
}
```

## 3. File Context::
```csharp
public sealed record FileContext(
									string                              FileName,
									string                              RelativePath,
									string?                             FullPath,
									byte[]                              Bytes,
									FileFormatInfo                      Format,
									string?                             Hash     = null,
									IReadOnlyDictionary<string,string>? Metadata = null
								);
```

## 4. Processing Models::
```csharp
public sealed record ProcessingOptions(
										Resolution TargetResolution,
										int        JpegQuality,
										bool       ApplyExifOrientation,
										bool       WarnOnSmallImages,
										int        MinimumWidth,
										int        MinimumHeight,
										bool       DryRun
									);

public sealed record ImageProcessingResult(
											EProcessingStatus Status,
											byte[]?           OutputBytes,
											FileFormatInfo    OutputFormat,
											Resolution        OutputResolution,
											string?           WarningMessage,
											string?           ErrorMessage,
											TimeSpan          Duration
										);
```

## 5. Processing Run Models::
```csharp
public sealed record ProcessingRun(
									string            Id,
									DateTimeOffset    StartedUtc,
									DateTimeOffset?   FinishedUtc,
									EProcessingStatus Status,
									int               TotalFiles,
									int               SuccessCount,
									int               FailedCount,
									int               SkippedCount
								)
{
	public override string ToString() { ... }
	public static ProcessingRun FromString(string s) { ... }
}

public sealed record ProcessingRunItem(
										string            Id,
										string            RunId,
										string?           SourceHash,
										string            FileName,
										EProcessingStatus Status,
										string?           Message,
										long?             DurationMs,
										DateTimeOffset    CreatedUtc
									)
{
	public override string ToString() { ... }
}
```

## 6. Logging Models::
```csharp
public sealed record LogEntry(
								Guid           Id,
								DateTimeOffset CreationDateUtc,
								ELogSeverity   Severity,
								string         Category,
								string         Message,
								string?        CorrelationId,
								string?        SourceHash,
								string?        ExceptionMessage
							);

public sealed record LogQuery(
								DateTimeOffset? DateRangeFromUtc,
								DateTimeOffset? DateRangeToUtc,
								ELogSeverity?   MinimumSeverity,
								string?         CorrelationId,
								string?         SourceHash,
								int             Skip,
								int             Limit
							);

public sealed record LogRetentionPolicy(
										int MaxDays,
										int MaxRows,
										int KeepDays,
										int KeepRows
									);
```

## 7. Manifest Models::
```csharp
public sealed record ManifestResultEntry(
											string         FileName,
											Resolution     Resolution,
											int            Quality,
											string         Hash,
											DateTimeOffset CreationDateUtc
										);

public sealed record ManifestDocument(
										Guid                               Id,
										string                             SourceHash,
										string                             SourceFileName,
										string                             RelativePath,
										DateTimeOffset                     CreationDateUtc,
										DateTimeOffset                     LastUpdateDateUtc,
										IReadOnlyList<ManifestResultEntry> Results
									);

public sealed record ManifestQuery(
									string? SourceHash,
									string? FileName,
									int     Limit
								);
```

## 8. Settings Models::
```csharp
public sealed record ScanSettings(
									bool IsRecursive,
									bool IsWatchEnabled,
									int  DebounceMilliseconds
								);

public sealed record CacheSettings(
									bool IsEnabled,
									int  MaxItems,
									int  ExpirationMinutes
								);

public sealed record LoggingSettings(
										bool IsFileLoggingEnabled,
										bool IsDatabaseLoggingEnabled,
										int  RetentionDays,
										int  MaxRows
									);

public record AppSettings(
							string          RootDirectory,
							Resolution      Resolution,
							int             Quality,
							ScanSettings    Scan,
							CacheSettings   Cache,
							LoggingSettings Logging
						)
{
	public static AppSettings Default { get; }
}
```

## 9. Scan Models::
```csharp
public sealed record ScanOptions(
									string InputDirectory,
									bool   IsRecursive,
									bool   IsRaiseEventsOn,
									bool   IsComputeHashesOn
								);

public sealed record ScanItem(
								string         FileName,
								string         RelativePath,
								string?        FullPath,
								FileFormatInfo Format,
								long           SizeBytes,
								DateTimeOffset LastWriteTimeUtc
							);

public sealed record ScanResult(
									IReadOnlyList<ScanItem> Items,
									int                     FilesFound,
									int                     FilesSkipped,
									TimeSpan                Duration
								);
```

## 10. Output Models::
```csharp
public sealed record OutputWriteRequest(
											string         TargetDirectory,
											string         FileName,
											byte[]         Bytes,
											EOverwriteMode OverwriteMode
										);

public sealed record OutputWriteResult(
										bool    IsSuccess,
										string  FullPath,
										string? ErrorMessage
									);
```

## 11. Orchestration Models::
```csharp
public sealed record ProcessRequest(
										ScanOptions       ScanOptions,
										ProcessingOptions ProcessingOptions,
										EOverwriteMode    OverwriteMode
									);

public sealed record FileProcessResult(
										string            FileName,
										EProcessingStatus Status,
										string?           Message
									);

public sealed record BatchProcessResult(
											string                           CorrelationId,
											IReadOnlyList<FileProcessResult> Items,
											int                              SuccessCount,
											int                              FailedCount,
											int                              SkippedCount,
											TimeSpan                         Duration
										);
```

## 12. File Index Models::
```csharp
public sealed record FileIndexEntry(
										string         Id,
										string         Hash,
										string         RelativePath,
										string?        FullPath,
										Resolution     Resolution,
										long           SizeBytes,
										DateTimeOffset LastSeenUtc
									);
```

## 13. Cache Models::
```csharp
public sealed record PreprocessCacheEntry(
											string         SourceHash,
											Resolution     Resolution,
											int            Quality,
											byte[]         OutputBytes,
											DateTimeOffset CreatedUtc,
											DateTimeOffset ExpiresUtc
										);
```

## 14. Validation Rules::
	>	JpegQuality:				1..100
	>	Resolution Width/Height:	> 0
	>	RootDirectory:				required
	>	FileName:					required
	>	RelativePath:				required
	>	Output bytes:				required for EProcessingStatus.Completed result
	>	LogRetentionDays:			> 0
	>	LogMaxRows:					> 0
	>	Cache limits:				>= 0

## 15. Notes::
	>	Immutable records:		preferred (sealed record)
	>	Use UTC timestamps in persistence
	>	DateTimeOffset in contracts (not DateTime)
	>	Validate values at boundaries only
	>	Prefer constructor validation or FluentValidation for complex rules
	>	Boolean properties on records use the Is prefix (IsRecursive, IsEnabled, etc.)
