# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/) (4-segment: `MAJOR.MINOR.PATCH.BUILD`).

## [0.4.0.0] - Unreleased

### Added

- Full functional port: 88 additional Sophia Script functions ported to `ITweak` catalog entries (97/113 total, up from 9/113), across Privacy & Telemetry (15/15), UI & Personalization (38/40), System (29/36), Gaming (1/1), Microsoft Defender & Security (9/9), and Context menu (5/6)
- `PowerShellNativeTweak` catalog support: `TweakDefinition` gained `ApplyScript`/`RevertScript`/`ProbeScript`, `TweakCatalogLoader` now branches on `type: "PowerShellNative"` and threads `IPowerShellHost` through the loader, `TweakService`, and `AddSophiaCore()`
- `Win32ApiTweak` catalog support: `TweakDefinition` gained `Operation`/`ApplyParameters`/`RevertParameters`, `TweakCatalogLoader` now branches on `type: "Win32Api"` and threads `IWin32InteropHost` through the same call chain (no ported function ended up needing a real P/Invoke call once its source was read — every function COMPILATION.md's original heuristic tagged `Win32ApiTweak` turned out to be a plain registry write, a cmdlet call, or an interactive/one-shot action; documented in `COMPILATION.md`)
- `PowerShellHost.InvokeAsync` implemented for real: hosts `System.Management.Automation.PowerShell` in-process via `Microsoft.PowerShell.SDK`, `[SupportedOSPlatform("windows")]`-guarded, propagates pipeline errors as exceptions
- `Win32InteropHost.InvokeAsync` implemented for real as an empty, `[SupportedOSPlatform("windows")]`-guarded dispatch (throws `NotSupportedException` for any operation) — intentionally not a speculative generic wrapper, since no ported tweak currently needs one
- `RegistryValueKind.Binary` conversion support in `TweakCatalogLoader.ConvertValue` (needed for the `CapsLock` scancode-map tweak)
- `RecordingPowerShellHost` / `RecordingWin32InteropHost` test fakes and focused `TweakCatalogLoader` tests covering `PowerShellNativeTweak`/`Win32ApiTweak` construction and Apply/Revert dispatch
- 16 Sophia functions explicitly left Pending with a documented reason each (network downloads, interactive dialogs, one-shot non-toggleable actions, or complexity beyond a registry/cmdlet toggle — e.g. inline `Add-Type` C# helpers) — see the Notes column in `COMPILATION.md`

### Changed

- `Assets/Catalog/tweaks-en.json` grown from 9 to 97 entries (63 `Registry`, 34 `PowerShellNative`)

## [0.3.0.0] - Unreleased

### Added

- Vendored the reference source, `src/Sophia_Script_for_Windows_11/Module/Sophia.psm1` (Sophia Script for Windows 11 Enterprise LTSC 2024, v7.2.0, MIT, © Team Sophia), for local, offline function inventory — see `THIRD-PARTY-NOTICES.md`
- Full inventory of the 113 Sophia Script functions across all 9 regions, mapped to a target `ITweak` kind and implementation status, documented in `COMPILATION.md` (closes the acceptance-criterion gap: 9/113 functions implemented, the remaining 104 explicitly scheduled for v0.4.0.0)

- Tweak engine abstractions: `ITweak`, `RegistryImpact`, `TweakRiskLevel`, `TweakBase`
- Three concrete tweak kinds: `RegistryTweak` (functional), `PowerShellNativeTweak` and `Win32ApiTweak` (interfaces ready, hosting lands in v0.4.0.0)
- Declarative JSON tweak catalog (`Assets/Catalog/tweaks-en.json`), embedded and loaded via `TweakCatalogLoader`
- 9 real, verified tweaks ported from Sophia Script for Windows 11 v7.1.4 (Privacy & Telemetry region)
- `RegistryService` upgraded from stub to functional `Microsoft.Win32.Registry` implementation, `[SupportedOSPlatform("windows")]`-guarded
- `TweakSnapshotService`: automatic pre-apply registry value snapshot for Medium/High risk tweaks, `%LOCALAPPDATA%\SophiaWin11\snapshots\`
- `IPowerShellHost` / `IWin32InteropHost` service interfaces (stubs, real hosting in v0.4.0.0)
- `RegistryHiveVirtualizer` test double; 15 new tests covering apply/revert/risk-triggered snapshot behavior and catalog loading, 100% against the virtualized hive (never touches the real registry)

## [0.2.0.0] - Unreleased

### Added

- Seven separated Core services with dedicated contracts: `ITweakService`, `IRegistryService`, `IElevationService`, `IBackupService`, `ILocalizationService`, `IThemeService`, `IAnimationService`
- Centralized DI bootstrap: `ServiceCollectionExtensions.AddSophiaCore()`
- Strict MVVM wiring: `MainViewModel` (`CommunityToolkit.Mvvm`, `ObservableProperty`/`RelayCommand`), bound to `MainWindow`
- Local-only structured logging via Serilog + `Microsoft.Extensions.Hosting` integration, file sink under `%LOCALAPPDATA%\SophiaWin11\logs`
- DI bootstrap test suite (12 tests, 100% coverage on `ServiceCollectionExtensions`)

## [0.1.0.0] - Unreleased

### Added

- Solution structure: `SophiaWin11.App`, `SophiaWin11.Core`, `SophiaWin11.UI`, `SophiaWin11.Tests`
- `Directory.Build.props`, `.editorconfig`, `.gitignore`
- `LICENSE` (MIT, dual copyright notice)
- Application manifest requiring administrator elevation
- Base `MainWindow` shell using WPF-UI `FluentWindow`
