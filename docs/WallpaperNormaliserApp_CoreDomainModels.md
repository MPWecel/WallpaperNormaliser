# CoreDomainModels.md

# WallpaperNormaliserApp - Core Domain Models

## 1. Enums
	```csharp
		public enum EFileFormat			{ Unknown=0,	Jpeg=1,		Png=2,			Bmp=3,			Gif=4, 					Tiff=5,					Webp=6 }
		public enum LogSeverity			{ Trace=0,		Debug=1,	Information=2,	Warning=3,		Error=4,				Critical=5 }
		public enum OverwriteMode		{ Skip=0,		SkipAll=1,	Replace=2,		ReplaceAll=3,	SaveWithTimestamp=4,	SaveAllWithTimestamp=5 }
		public enum ScanMode			{ Manual=0,		Watch=1 }
		public enum ProcessingStatus	{ Pending=0,	Running=1,	Completed=2,	Skipped=3,		Failed=4,				Cached=5 }
	```
	
## 2. Value Objects
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
	
## 3. File Context
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
	
## 4. Processing Models
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
	```
	
## 5. Logging Models
	```csharp
		public sealed record LogEntry(
										Guid Id,
										DateTimeOffset CreatedUtc,
										LogSeverity Severity,
										string Category,
										string Message,
										string? CorrelationId
									);
	```
	
## 6. Notes
	>	Immutable records preferred
	>	Use UTC timestamps in persistence
	>	DateTimeOffset in contracts
	>	Validate values at boundaries
