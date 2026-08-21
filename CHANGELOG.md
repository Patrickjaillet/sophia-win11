# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/) (4-segment: `MAJOR.MINOR.PATCH.BUILD`).

## [0.6.0.0] - Unreleased

### Added

- WPF-UI-based application shell: `MainWindow` rebuilt around `ui:NavigationView` (Mica backdrop via `ui:FluentWindow.WindowBackdropType="Mica"` — `WindowBackdropType.MicaAlt` does not exist in WPF-UI 4.0.2's enum, verified by reflecting the installed `Wpf.Ui.dll`; the enum only defines `None`/`Auto`/`Mica`/`Acrylic`/`Tabbed`) with a custom `ui:TitleBar` merged into the extended client area
- Dynamic navigation pane: `ShellViewModel` builds one `NavigationViewItem` per distinct `Category` present in `ITweakService.Tweaks` after `InitializeCatalogAsync()` (plus a Dashboard and a Search entry); categories with zero tweaks never produce a nav node since the list is derived from the loaded catalog, not a hardcoded roadmap category list
- Category pages: `CategoryViewModel` + `CategoryPage`, one `TweakRowViewModel` per tweak (name, description, risk-level badge, requires-restart flag, Apply/Revert buttons wired to `ITweak.ApplyAsync`/`RevertAsync`, state read via `IsAppliedAsync`); per-row apply/revert failures are caught and surfaced as a Snackbar instead of crashing the page
- Global search page: `SearchViewModel` + `SearchPage`, live-filtered as you type over `ITweakService.Tweaks` (captured once after catalog init) using a hand-written `TweakSearchScorer` (exact match > substring > ordered-subsequence fuzzy match on Name/Category, substring-only on Description to avoid subsequence noise on longer text) — no fuzzy-search NuGet dependency added, keeps the app offline-first
- Dashboard page: `DashboardViewModel` + `DashboardPage` showing total tweak count and currently-applied count (computed via `Task.WhenAll` over all 97 `IsAppliedAsync()` calls instead of serially awaiting each one), a "last session" card (no session history is persisted yet, so this honestly shows a static "no session yet" state rather than inventing a persistence layer), and Load/Save profile buttons wired to `IProfileService` via `Microsoft.Win32.OpenFileDialog`/`SaveFileDialog`
- In-app notifications: `Wpf.Ui.ISnackbarService`/`SnackbarService` registered in DI, presenter wired once in `MainWindow`'s constructor, used to confirm/report apply and revert outcomes from the category and search pages and profile load/save from the dashboard
- `SophiaWin11.Tests` now also references `SophiaWin11.App` (retargeted to `net9.0-windows10.0.22621.0` with `UseWPF` enabled) so ViewModel logic (search filtering/scoring, dashboard count aggregation, category apply/revert row state) is unit tested without needing a live window; `TweakRowViewModel`/`DashboardViewModel` avoid constructing WPF-UI `SymbolIcon` elements from command logic specifically so this logic stays testable off the UI thread (constructing a `FrameworkElement` off an STA thread throws)

### Changed

- `MainViewModel` replaced by `ShellViewModel` (navigation-shell state: elevation status, initialization status, the dynamic nav item collection) plus dedicated per-page ViewModels; the old placeholder "Apply Theme" button and status text UI is gone
- `Directory.Build.props` version bumped to `0.6.0.0`

### Notes

- Roadmap acceptance criterion "navigation fluide à 60 FPS mesurée (profiling WPF `PresentationTraceSources`)" cannot be certified from this environment — there is no interactive display to run `PresentationTraceSources` frame-timing profiling against. What was verified statically: the dashboard's 97x `IsAppliedAsync()` aggregation runs via `Task.WhenAll` off the synchronous UI path, category/search page population reads the already-in-memory catalog (no I/O on navigation), and no navigation path does synchronous blocking work on the UI thread. Actual frame-rate measurement needs to be done by running the built app.

## [0.5.0.0] - Unreleased

### Added

- `IRestorePointService` / `RestorePointService`: automatic Windows System Restore point creation before a batch tweak session, via a real `srclient.dll!SRSetRestorePointW` P/Invoke (`[DllImport]`, `[SupportedOSPlatform("windows")]`) using the documented `RESTOREPOINTINFOW`/`STATEMGRSTATUS` struct layout (`BEGIN_SYSTEM_CHANGE`/`END_SYSTEM_CHANGE`/`MODIFY_SETTINGS`, `MAX_DESC_W = 256`); untestable against the live OS, so the service is a thin injectable wrapper and only the calling logic (one restore point per session) is unit tested, against a fake `IRestorePointService`
- Dry-Run preview: `ITweak.PreviewAsync()` added (implemented via a new `TweakBase.PreviewCoreAsync` abstract hook), producing a "current -> target" textual description per registry value for `RegistryTweak` (reads the live current value through `IRegistryService.GetValue`, never calls `ApplyAsync`), the would-run script text for `PowerShellNativeTweak` (without executing it), and the operation/parameters that would be invoked for `Win32ApiTweak`
- Profile system: `IProfileService` / `ProfileService`, `TweakProfile` record (Name, CreatedAt, TweakIds) — pure JSON file round-trip to/from a `.sophiaprofile` path, no catalog coupling
- Conflict detection: `IConflictDetectionService` / `ConflictDetectionService`, `TweakConflict` record — generic, mechanical detection of two selected tweaks that target the exact same `RegistryHive`+`SubKey`+`ValueName` (via a new optional `RegistryImpact.ApplyValue`) and would apply different (or undeterminable) values; covers the DNS-over-HTTPS conflict class described in the roadmap without hand-listing provider pairs
- System health diagnostic: `IHealthDiagnosticService` / `HealthDiagnosticService`, `HealthDiagnosticResult` record — runs DISM `/CheckHealth` and `sfc /verifyonly` (via a new `IPowerShellHost.InvokeAndCaptureAsync` that returns captured script output) and parses both for known-healthy markers
- Session apply/rollback: `ISessionService` / `SessionService`, `TweakSession` record, `TweakConflictException` — `ApplySessionAsync` runs conflict detection first (aborts via `TweakConflictException` on any conflict), runs the health diagnostic and aborts if unhealthy when the session includes a `RiskLevel.High` tweak, creates exactly one System Restore point for the whole session, applies each tweak (existing per-tweak Medium/High `ITweakSnapshotService` capture still fires through `TweakBase.ApplyAsync`), and auto-rolls-back already-applied tweaks if one fails mid-session; `RollbackSessionAsync` reverts every applied tweak in reverse order. Integration-tested end-to-end against `RegistryHiveVirtualizer` with 3 `RegistryTweak` fixtures, proving the registry hive is restored bit-for-bit identical to its seeded initial state after apply + rollback
- `ITweakService.Tweaks` — exposes the loaded catalog as `IReadOnlyList<ITweak>` (previously only `TweakCount` was available)
- UAC single-elevation verified (no code change needed): `app.manifest` already requires `requireAdministrator` at launch, and no tweak path anywhere in the codebase shells out to a new elevated child process — `PowerShellNativeTweak` stays in-process via `IPowerShellHost`

### Changed

- `RegistryImpact` gained an optional `ApplyValue` (defaults to `null`, backward compatible with existing 4-arg construction) so conflict detection can compare target values without threading a new parameter through every call site
- `IPowerShellHost` gained `InvokeAndCaptureAsync`, returning `IReadOnlyList<string>` output lines; existing `InvokeAsync` call sites (catalog-driven `PowerShellNativeTweak` apply/revert/probe) are unchanged
- `RecordingPowerShellHost` test fake extended with canned-output support (`CannedOutput`/`DefaultOutput`) for testing output-parsing logic without real PowerShell/DISM/SFC execution

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
