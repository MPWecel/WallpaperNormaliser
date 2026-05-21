# WallpaperNormaliser_ProjectContext.md

# A. Executive Overview
	WallpaperNormaliserApp is a cross-platform C# .NET application for normalising wallpaper images used in slideshow/background rotation scenarios.

	Primary purpose:
	>	Accept source images from INPUT directory
	>	Convert them into standardised JPEG wallpapers at chosen resolution
	>	Preserve aspect ratio
	>	Center image on black canvas/background
	>	Store outputs in organised structure
	>	Track processing history
	>	Be extensible toward Console UI, Web API, Blazor UI, Desktop UI

# B. Tech Stack
	## Language / Runtime
	>	C#
	>	.NET 9

	## Core Libraries
	>	SixLabors.ImageSharp (image processing)
	>	Dapper (data access)
	>	Microsoft.Data.Sqlite (SQLite provider)
	>	Microsoft.Extensions.DependencyInjection
	>	Spectre.Console (Console TUI — current phase)
	>	SQLKata (query builder — planned, not yet integrated)

	## Storage
	>	SQLite local database
	>	JSON manifest files
	>	File system INPUT / OUTPUT / MANIFEST folders

# C. Core Design Principles
	>	Clean / layered architecture
	>	UI replaceable
	>	Infrastructure replaceable
	>	Contracts-first design
	>	Async-first APIs
	>	Cross-platform support
	>	Human-readable manifests
	>	Local-first persistence
	>	Future API / GUI ready

# D. Key Domain Rules
	>	One output resolution per processing run
	>	Default output format = JPEG
	>	Default target resolution = 1920x1080
	>	Other supported resolutions configurable
	>	Output image = centered source + black background
	>	Preserve aspect ratio
	>	Downscale if too large
	>	Warn if image smaller than 640x480
	>	User may continue after warning
	>	Hash is ultimate file identity
	>	Recursive scanning configurable
	>	Output grouped by source-file subdirectory

	Example:
	cat.png -> OUTPUT/cat_png/cat_1920x1080.jpg

# E. Repository Structure

	repo-root/
	├── db/
	│   └── wallpaper-normaliser.db
	├── APPLICATION_WORKING_DIRECTORY/
	│   ├── INPUT/
	│   ├── OUTPUT/
	│   └── MANIFEST/
	└── src/
		└── WallpaperNormaliser/
			├── WallpaperNormaliser.sln
			├── WallpaperNormaliser.Core/
			├── WallpaperNormaliser.Infrastructure/
			└── WallpaperNormaliser.ConsoleUi/

# F. Project Dependencies
	## WallpaperNormaliser.Core
	Depends on: none

	Contains:
	>	contracts
	>	domain models
	>	enums
	>	value objects

	## WallpaperNormaliser.Infrastructure
	Depends on:
	>	WallpaperNormaliser.Core

	Contains:
	>	SQLite repositories
	>	filesystem services
	>	hashing
	>	image processing
	>	orchestrator
	>	dependency injection

	Packages:
	>	Dapper
	>	Microsoft.Data.Sqlite
	>	SixLabors.ImageSharp
	>	Microsoft.Extensions.DependencyInjection

	## WallpaperNormaliser.ConsoleUi
	Depends on:
	>	Core
	>	Infrastructure

	Contains:
	>	Program.cs
	>	Bootstrap (DI setup, startup runner)
	>	Spectre.Console screens and navigation
	>	ViewModels and ApplicationServices

# G. Core Contracts (Agreed Interfaces)

	```csharp

	public interface IHashService
	{
		Task<string> ComputeAsync(FileContext file, CancellationToken ct = default);
	}

	public interface IImageProcessor
	{
		Task<ImageProcessingResult> ProcessAsync(
			FileContext file,
			ProcessingOptions options,
			CancellationToken ct = default
		);
	}

	public interface IInputScanner
	{
		event EventHandler<FileDiscoveredEventArgs> FileDiscovered;
		event EventHandler<FileChangedEventArgs>   FileChanged;
		event EventHandler<FileRemovedEventArgs>   FileRemoved;

		Task<ScanResult> ScanAsync(ScanOptions options, CancellationToken ct = default);
		Task StartWatchingAsync(ScanOptions options, CancellationToken ct = default);
		Task StopWatchingAsync(CancellationToken ct = default);
	}

	public interface IOutputWriter
	{
		Task<OutputWriteResult> WriteAsync(OutputWriteRequest request, CancellationToken ct = default);
		Task WriteManyAsync(IEnumerable<OutputWriteRequest> requests, CancellationToken ct = default);
	}

	public interface IManifestRepository
	{
		Task<ManifestDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<ManifestDocument?> GetByFileNameAsync(string fileName, CancellationToken ct = default);
		Task<ManifestDocument?> GetByHashAsync(string hash, CancellationToken ct = default);
		Task<IReadOnlyList<ManifestDocument>> GetManyAsync(ManifestQuery query, CancellationToken ct = default);
		Task SaveAsync(ManifestDocument manifest, CancellationToken ct = default);
		Task DeleteAsync(Guid id, CancellationToken ct = default);
	}

	public interface ISettingsRepository
	{
		Task<AppSettings> GetAsync(CancellationToken ct = default);
		Task SaveAsync(AppSettings settings, CancellationToken ct = default);
		Task ResetToDefaultsAsync(CancellationToken ct = default);
		Task<string> ExportJsonAsync(CancellationToken ct = default);
		Task ImportJsonAsync(string json, CancellationToken ct = default);
		Task<bool> ExistsAsync(CancellationToken ct = default);
		Task UpdateAsync(Func<AppSettings, AppSettings> update, CancellationToken ct = default);
	}

	public interface ILogRepository
	{
		Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken ct = default);
		Task WriteAsync(LogEntry entry, CancellationToken ct = default);
		Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken ct = default);
		Task CleanupAsync(LogRetentionPolicy policy, CancellationToken ct = default);
		Task<int> CountAsync(LogQuery query, CancellationToken ct = default);
	}

	public interface IFileIndexRepository
	{
		Task<FileIndexEntry?> GetByHashAsync(string hash, CancellationToken ct = default);
		Task<FileIndexEntry?> GetByRelativePathAsync(string relativePath, CancellationToken ct = default);
		Task<IReadOnlyList<FileIndexEntry>> GetDuplicatesAsync(string hash, CancellationToken ct = default);
		Task<IReadOnlyList<FileIndexEntry>> ListAsync(CancellationToken ct = default);
		Task UpsertAsync(FileIndexEntry entry, CancellationToken ct = default);
		Task UpsertManyAsync(IReadOnlyCollection<FileIndexEntry> entries, CancellationToken ct = default);
		Task RemoveMissingAsync(IReadOnlyCollection<string> presentRelativePaths, CancellationToken ct = default);
	}

	public interface IRunRepository
	{
		Task<ProcessingRun?> GetRunAsync(string runId, CancellationToken ct = default);
		Task<IReadOnlyList<ProcessingRunItem>> GetRunItemsAsync(string runId, CancellationToken ct = default);
		Task<IReadOnlyList<ProcessingRun>> GetRecentRunsAsync(int count, CancellationToken ct = default);
		Task CreateRunAsync(ProcessingRun run, CancellationToken ct = default);
		Task UpsertRunAsync(ProcessingRun run, CancellationToken ct = default);
		Task AddRunItemAsync(ProcessingRunItem item, CancellationToken ct = default);
		Task FinaliseRunAsync(ProcessingRun run, CancellationToken ct = default);
	}

	public interface IPreprocessCacheRepository
	{
		Task<PreprocessCacheEntry?> GetAsync(string sourceHash, CancellationToken ct = default);
		Task UpsertAsync(PreprocessCacheEntry entry, CancellationToken ct = default);
		Task RemoveByHashAsync(string sourceHash, CancellationToken ct = default);
		Task CleanupExpiredAsync(CancellationToken ct = default);
		Task ClearAsync(CancellationToken ct = default);
	}

	public interface IProcessingOrchestrator
	{
		Task<BatchProcessResult> RunAsync(ProcessRequest request, CancellationToken ct = default);
	}
	```

# H. Key Domain Models (excerpt — see CoreDomainModels.md for full reference)

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

	public enum EFileFormat { Unknown=0, Jpeg=1, Png=2, Bmp=3, Gif=4, Tiff=5, Webp=6 }

	public sealed record Resolution(uint Width, uint Height)
	{
		public static Resolution FromString(string s) { ... }
		public override string ToString() => $"{Width}x{Height}";
	}

	public sealed record ProcessingOptions(
		Resolution TargetResolution,
		int        JpegQuality,
		bool       ApplyExifOrientation,
		bool       WarnOnSmallImages,
		int        MinimumWidth,
		int        MinimumHeight,
		bool       DryRun
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

# I. Dependency Injection

	```csharp
	services.AddSingleton<IHashService,                Sha256HashService>();
	services.AddSingleton<IImageProcessor,             ImageSharpProcessor>();
	services.AddSingleton<ISettingsRepository,         SqliteSettingsRepository>();
	services.AddSingleton<ILogRepository,              SqliteLogRepository>();
	services.AddSingleton<IManifestRepository,         JsonManifestRepository>();
	services.AddSingleton<IInputScanner,               RecursiveInputScanner>();
	services.AddSingleton<IOutputWriter,               AtomicOutputWriter>();
	services.AddSingleton<IProcessingOrchestrator,     ProcessingOrchestrator>();
	services.AddSingleton<IFileIndexRepository,        SqliteFileIndexRepository>();
	services.AddSingleton<IRunRepository,              SqliteRunRepository>();
	services.AddSingleton<IPreprocessCacheRepository,  SqlitePreprocessCacheRepository>();
	```

# J. Console Entry Point

	```csharp
	// Program.cs
	var services = new ServiceCollection();
	ServiceRegistration.Configure(services);
	using var provider = services.BuildServiceProvider();
	await StartupRunner.RunAsync(provider);
	```

	`StartupRunner` renders the application header, validates startup preconditions
	(working directory exists, settings reachable), and launches `MainMenu`.

# K. Infrastructure File Summary
	### Security
		>	Sha256HashService
			Computes SHA256 hashes for source identity and duplicate detection.

	### Imaging
		>	ImageSharpProcessor
			Loads image bytes, auto-rotates EXIF, resizes proportionally, centres on black canvas,
			outputs JPEG bytes. Validates minimum image size before processing.

	### FileSystem
		>	WorkingDirectoryService
			Resolves INPUT, OUTPUT, and MANIFEST paths relative to the application root.

		>	JsonManifestRepository
			Stores per-source manifest JSON files in the MANIFEST directory.

		>	RecursiveInputScanner
			Scans INPUT directory recursively or flat; detects supported image formats;
			raises FileDiscovered / FileChanged / FileRemoved events in watch mode.

		>	AtomicOutputWriter
			Writes output files safely using temp-then-move; enforces EOverwriteMode policies.

	### Persistence
		>	SqliteConnectionFactory
			Creates SQLite connections with PRAGMA settings (WAL, foreign keys, memory cache).

		>	MigrationRunner
			Loads and executes ordered SQL migration scripts; tracks version in SchemaInfo.
			(LoadEmbeddedSqlAsync / EnsureSchemaInfoAsync not yet fully implemented.)

		>	ResolutionTypeHandler
			Dapper TypeHandler<Resolution> — serialises Resolution to/from "WxH" string.

		>	SqliteSettingsRepository
			Stores/retrieves app settings as key/value rows. Export/import JSON supported.

		>	SqliteLogRepository
			Structured log storage; supports severity filtering, time-range queries, retention cleanup.

		>	SqliteFileIndexRepository
			Tracks discovered source files; supports duplicate detection via hash lookup.

		>	SqliteRunRepository
			Records batch processing runs and per-file results.

		>	SqlitePreprocessCacheRepository
			Caches preprocessed JPEG bytes keyed by source hash + resolution; handles expiry cleanup.

	### Background (stubs — not yet implemented)
		>	PreprocessWorker
			Planned background worker for proactive preprocessing.

		>	WatcherService
			Planned background service for watch-mode file monitoring.

		>	FileLogSink
			Planned file-based log output sink.

		>	CompositeLogger
			Planned multi-sink log fan-out (database + file).

	### Processing
		>	ProcessingOrchestrator
			Coordinates the complete workflow: scan → hash → process → write → manifest → log.

# L. Database Schema
	### Tables:
		>	SchemaInfo			— migration version (Version PK, AppliedUtc)
		>	AppSettings			— key/value configuration store (Key PK, Value, UpdatedUtc)
		>	FileIndex			— source files with hash, format, dimensions, size, duplicate flag
		>	ProcessingRuns		— batch run history (Id, StartedUtc, FinishedUtc, Status, TotalFiles, counts)
		>	ProcessingRunItems	— per-file run results (Id, RunId, SourceHash, FileName, Status, DurationMs)
		>	Logs				— structured operational logs (Id, CreatedUtc, Severity, Category, Message, CorrelationId)
		>	PreprocessCache		— cached JPEG bytes (SourceHash PK, Resolution, JpegQuality, OutputBytes, ExpiresUtc)

	### FileIndex columns (full list):
		>	Id, SourceHash, FileName, RelativePath, FullPath
		>	Format, SizeBytes, Width, Height
		>	LastSeenUtc, LastWriteUtc, IsDuplicate

# M. Current Status
	### Completed
		<	Architecture design
		<	Core contracts (all 11 interfaces)
		<	Domain model definitions
		<	SQLite schema design and migration scripts
		<	Infrastructure phase 1 (all repositories, services, orchestrator)
		<	Compilable solution skeleton
		<	ConsoleUi scaffold (screens, navigation, ViewModels, ApplicationServices)

	### In Progress
		>	ConsoleUI functional completion (see next-steps.md)
		>	MigrationRunner full implementation
		>	Settings serialisation fix

# N. Next Phase (Immediate)
	Complete outstanding implementation tasks. Priority order:

	1.	Fix connection string wiring in ServiceRegistration
	2.	Complete MigrationRunner (LoadEmbeddedSqlAsync, EnsureSchemaInfoAsync)
	3.	Fix SqliteSettingsRepository temporary string parsing
	4.	Fix ProcessingScreen to read resolution from ISettingsRepository (not hardcoded)
	5.	Implement DbPaths for SQLite file path resolution
	6.	Implement SettingsValidator.Validate() with real logic
	7.	Fix LogsScreen.SearchLogsAsync() to apply LogQuery filters
	8.	Implement JsonManifestRepository.GetByIdAsync() and DeleteAsync()
	9.	Complete DashboardScreen with file counts
	10.	Implement FileLogSink and CompositeLogger
	11.	Implement WatcherService and InputScanner watch mode
	12.	Implement PreprocessWorker

	See .claude/docs/next-steps.md for full task list with file paths.

# O. Deferred / Future Work
	>	Unit tests
	>	Integration tests
	>	Watcher debounce tuning
	>	Background preprocess worker (PreprocessWorker)
	>	Streams-first contracts
	>	Advanced migrations
	>	Web API
	>	Blazor UI
	>	Desktop UI
	>	Per-user settings
	>	SQLKata query builder integration
