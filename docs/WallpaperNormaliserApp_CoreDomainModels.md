# CoreDomainModels.md

# WallpaperNormaliserApp - Core Domain Models

## 1. Enums::
```csharp
	public enum EFileFormat			{ Unknown=0,	Jpeg=1,		Png=2,			Bmp=3,			Gif=4, 					Tiff=5,					Webp=6 }
	public enum LogSeverity			{ Trace=0,		Debug=1,	Information=2,	Warning=3,		Error=4,				Critical=5 }
	public enum OverwriteMode		{ Skip=0,		SkipAll=1,	Replace=2,		ReplaceAll=3,	SaveWithTimestamp=4,	SaveAllWithTimestamp=5 }
	public enum ScanMode			{ Manual=0,		Watch=1 }
	public enum ProcessingStatus	{ Pending=0,	Running=1,	Completed=2,	Skipped=3,		Failed=4,				Cached=5 }
```
	
## 2. Value Objects::
```csharp
	public sealed record FileFormatInfo(EFileFormat Format)
	{
		public string Extension =>	Format switch
									{
										EFileFormat.Jpeg => "jpg",
										EFileFormat.Png => "png",
										EFileFormat.Bmp => "bmp",
										EFileFormat.Gif => "gif",
										EFileFormat.Tiff => "tiff",
										EFileFormat.Webp => "webp",
										_ => "bin"
									};
	
		public string MimeType =>	Format switch
									{
										EFileFormat.Jpeg => "image/jpeg",
										EFileFormat.Png => "image/png",
										EFileFormat.Bmp => "image/bmp",
										EFileFormat.Gif => "image/gif",
										EFileFormat.Tiff => "image/tiff",
										EFileFormat.Webp => "image/webp",
										_ => "application/octet-stream"
									};
	}
```
	
## 3. File Context::
```csharp
	public sealed record FileContext(
										string FileName,
										string RelativePath,
										string? FullPath,
										byte[] Bytes,
										FileFormatInfo Format,
										string? Hash = null,
										IReadOnlyDictionary<string,string>? Metadata = null
									);
```
	
## 4. Processing Models::
```csharp
	public sealed record Resolution(int Width, int Height);
	
	public sealed record ProcessingOptions(
											Resolution TargetResolution,
											int JpegQuality,
											bool ApplyExifOrientation,
											bool WarnOnSmallImages,
											int MinimumWidth,
											int MinimumHeight,
											bool DryRun
										);
	public sealed record ImageProcessingResult(
												ProcessingStatus Status,
												byte[]? OutputBytes,
												FileFormatInfo OutputFormat,
												int OutputWidth,
												int OutputHeight,
												string? WarningMessage,
												string? ErrorMessage,
												TimeSpan Duration
											);
```
	
## 5. Logging Models::
```csharp
	public sealed record LogEntry(
									Guid Id,
									DateTimeOffset CreatedUtc,
									LogSeverity Severity,
									string Category,
									string Message,
									string? CorrelationId,
									string? SourceHash,
									string? ExceptionMessage
								);
	public sealed record LogQuery(
									DateTimeOffset? FromUtc,
									DateTimeOffset? ToUtc,
									LogSeverity? MinimumSeverity,
									string? CorrelationId,
									string? SourceHash,
									int Limit
								);
```
	
## 6. Manifest Models::
```csharp
	public sealed record ManifestResultEntry(
												string FileName,
												Resolution Resolution,
												int Quality,
												string Hash,
												DateTimeOffset CreatedUtc
											);
	public sealed record ManifestDocument(
											Guid Id,
											string SourceHash,
											string SourceFileName,
											string RelativePath,
											DateTimeOffset CreatedUtc,
											DateTimeOffset LastUpdatedUtc,
											IReadOnlyDictionary<string,string>? SourceMetadata,
											IReadOnlyList<ManifestResultEntry> Results
										);
```
	
## 7. Settings Models::
	
```csharp
	public sealed record ScanSettings(
										bool Recursive,
										bool WatchEnabled,
										int DebounceMilliseconds
									);
	public sealed record CacheSettings(
										bool Enabled,
										int MaxItems,
										int ExpirationMinutes
									);
	public sealed record LoggingSettings(
											bool FileLoggingEnabled,
											bool DatabaseLoggingEnabled,
											int RetentionDays,
											int MaxRows
										);
	public sealed record AppSettings(
										string RootDirectory,
										Resolution DefaultResolution,
										int DefaultJpegQuality,
										ScanSettings Scan,
										CacheSettings Cache,
										LoggingSettings Logging
									);
```
	
## 8. Scan Models::
```csharp
	public sealed record ScanOptions(
										string InputDirectory,
										bool Recursive,
										bool RaiseEvents,
										bool ComputeHashes
									);
	public sealed record ScanItem(
									string FileName,
									string RelativePath,
									string? FullPath,
									FileFormatInfo Format,
									long SizeBytes,
									DateTimeOffset LastWriteTimeUtc
								);
	public sealed record ScanResult(
										IReadOnlyList<ScanItem> Items,
										int FilesFound,
										int FilesSkipped,
										TimeSpan Duration
									);
```
	
## 9. Output Models::
```csharp
	public sealed record OutputWriteRequest(
												string TargetDirectory,
												string FileName,
												byte[] Bytes,
												OverwriteMode Mode
											);
	public sealed record OutputWriteResult(
												bool Success,
												string FullPath,
												string? ErrorMessage
											);
```
	
## 10. Orchestration Models::
```csharp
	public sealed record ProcessRequest(
											ScanOptions ScanOptions,
											ProcessingOptions ProcessingOptions,
											OverwriteMode OverwriteMode
										);
	public sealed record FileProcessResult(
											string FileName,
											ProcessingStatus Status,
											string? Message
										);
	public sealed record BatchProcessResult(
												string CorrelationId,
												IReadOnlyList<FileProcessResult> Items,
												int SuccessCount,
												int FailedCount,
												int SkippedCount,
												TimeSpan Duration
											);
```
	
## 11. Validation Rules::
	>	JpegQuality:				1..100
	>	Resolution width/height:	> 0
	>	RootDirectory:				required
	>	FileName:					required
	>	RelativePath:				required
	>	Output bytes:				required for successful processing
	>	LogRetentionDays:			> 0
	>	LogMaxRows:					> 0
	>	Cache limits:				>= 0
	
## 12. Notes
	>	Immutable records:	preferred
	>	Use UTC timestamps in persistence
	>	DateTimeOffset in contracts
	>	Validate values:	at boundaries
	>	Prefet constructor validation or FluentValidation on every change
	

