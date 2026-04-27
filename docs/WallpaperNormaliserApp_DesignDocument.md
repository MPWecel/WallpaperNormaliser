
# WallpaperNormaliserApp - Design Document

## 1. Purpose

WallpaperNormaliserApp is a cross-platform .NET application for normalising image files into standardized wallpaper outputs.

The initial frontend is a console TUI, but the architecture must support future frontends:

- Console UI
- Web API
- Blazor client
- Desktop UI

Core logic must remain independent from transport and storage mechanisms.

---

## 2. Primary Goals

- Scan INPUT directories recursively or non-recursively
- Detect supported image files
- Read metadata
- Compute content hash
- Warn on small images
- Process images into centered black-background JPG wallpapers
- Cache processing work in background
- Save final output only on explicit user request
- Track manifests per source file
- Persist settings and logs in SQLite
- Support replace / skip / timestamp overwrite policies

---

## 3. Working Directory Structure

```text
ROOT/
├── INPUT/
├── OUTPUT/
└── MANIFEST/
```

Example:

```text
INPUT/cat.png
OUTPUT/cat_png/cat_1920x1080.jpg
MANIFEST/cat.json
```

---

## 4. Identity Model

Ultimate identity of source files is content hash (SHA-256 recommended).

Filename is metadata only.

This prevents duplicate work after renames.

---

## 5. Processing Rules

- Supported formats: png, jpg, jpeg, bmp, webp, gif, tif, tiff
- Apply EXIF orientation automatically
- If image smaller than 640x480, warn user and request confirmation
- Scale proportionally to fit target resolution
- Never crop by default
- Center image on black canvas
- Save as JPG
- Default quality: 100

Current mode: one target resolution per run.

---

## 6. Resolution Presets

Examples:

- 1920x1080 (default)
- 1920x1200
- 3840x2160
- 800x600
- 1024x768
- 1280x1024

---

## 7. Scanner Service

Scanner responsibilities:

- Monitor INPUT directory
- Optional recursive scan
- Detect new/changed files
- Read metadata
- Compute hash
- Detect duplicates
- Generate warnings
- Start background preprocessing using current settings
- Cache intermediate/final bytes for fast user-triggered export

No output files are written automatically.

---

## 8. Persistence

SQLite + Dapper

### Suggested tables

#### Settings

- Key
- Value

#### ProcessingLog

- Id
- DateUtc
- SourceHash
- SourceName
- Action
- Status
- Message
- DurationMs

#### FileIndex (optional)

- SourceHash
- CurrentPath
- LastSeenUtc
- ManifestPath

---

## 9. Manifest Files

One manifest per source input.

Example:

```json
{
  "sourceFile": {
    "name": "cat.png",
    "hash": "sha256...",
    "metadata": {
      "width": 2560,
      "height": 1440,
      "format": "png"
    }
  },
  "lastProcessedUtc": "2026-04-27T17:31:10Z",
  "results": [
    {
      "fileName": "cat_1920x1080.jpg",
      "resolution": "1920x1080",
      "quality": 100
    }
  ]
}
```

---

## 10. Overwrite Policies

- Skip
- Skip All
- Replace
- Replace All
- Save With Timestamp
- Save All With Timestamp

---

## 11. Architecture

```text
Solution
├── WallpaperNormaliser.Core
├── WallpaperNormaliser.Infrastructure
├── WallpaperNormaliser.ConsoleUi
├── WallpaperNormaliser.WebApi (future)
├── WallpaperNormaliser.Blazor (future)
└── WallpaperNormaliser.Desktop (future)
```

### Core

Business logic, contracts, models.

### Infrastructure

SQLite, Dapper, filesystem, ImageSharp, hashing, logging.

### ConsoleUi

Spectre.Console TUI frontend.

---

## 12. Dependency Injection

Use:

- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging

---

## 13. Recommended Roadmap

### Phase 1

Core contracts, models, processing engine.

### Phase 2

Filesystem scanning, manifests, overwrite flow.

### Phase 3

SQLite persistence.

### Phase 4

Spectre.Console TUI.

### Phase 5

Parallel processing.

### Phase 6

Watch mode + preprocessing cache.

### Phase 7

Additional frontends.

---

## 14. Core Contracts Planned

- IImageProcessor
- IManifestRepository
- ISettingsRepository
- ILogRepository
- IInputScanner
- IOutputWriter

---

## 15. Non-Functional Goals

- Cross-platform
- Testable
- Replaceable UI
- Good performance for <1000 files
- Safe file writes
- Clear logs
- Extensible architecture
