# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/) (4-segment: `MAJOR.MINOR.PATCH.BUILD`).

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
