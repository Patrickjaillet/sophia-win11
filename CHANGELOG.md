# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/) (4-segment: `MAJOR.MINOR.PATCH.BUILD`).

## [1.2.0.0] - 2026-08-22

Added an About page, reachable from the navigation menu, showing the app name and current version, an animated Art Déco banner, the Sophia Script and UI Windows 11 copyright notices with clickable links to the original project and the app's website, and a mention of the MIT license. A new button lets you export the full license text and the list of third-party libraries used by the app to a text file of your choice, so you no longer need to open the installation folder to read them. The list of third-party libraries and their licenses was also audited and completed to cover every library actually shipped in the app.

## [1.1.0.0] - 2026-08-22

The app is now available in French, German, Russian, and Ukrainian, alongside English. On first launch it automatically picks up your Windows display language if it's one of these five; otherwise it starts in English. You can change the language at any time from the new Settings page, and the switch applies immediately across the whole app — no restart needed. This includes every button, label, and message in the interface, as well as all 97 tweak names, descriptions, and categories. The project's documentation (README) is now also available in all five languages, with a language switcher at the top of each page.

## [1.0.0.0] - 2026-08-22

Release candidate: feature freeze (v0.3.0.0 → v0.5.0.0 portage, v0.6.0.0 → v0.9.0.0 UI/theme/animation), full codebase review, and bug-fix pass. Zero residual comments, zero hardcoded visual values outside the theme dictionaries, project-wide.

### Fixed

- `SessionService.ApplySessionAsync` now only creates a Windows System Restore point when the session's tweak selection contains at least one `Medium`- or `High`-risk tweak, mirroring `TweakBase.ApplyAsync`'s per-tweak snapshot gating — previously it created a restore point unconditionally for every session, including single `Low`-risk registry toggles, making every Apply click pay the restore-point cost (several seconds, plus Windows' 24h `SystemRestorePointCreationFrequency` throttle)
- `TweakRowViewModel.ApplyAsync`/`RevertAsync` now route through `ISessionService.ApplySessionAsync`/`RollbackSessionAsync` instead of calling `Tweak.ApplyAsync()`/`RevertAsync()` directly, so the v0.5.0.0 safety-net engine (conflict detection, restore points, health diagnostics) actually runs for real UI apply/revert clicks rather than only through the automatic per-tweak snapshot; a `TweakConflictException` now surfaces as a distinct "Conflict detected" snackbar instead of the generic failure message
- `DashboardViewModel.LoadProfileAsync` now resolves a loaded profile's tweak IDs and applies them as a single `ISessionService.ApplySessionAsync` session instead of looping `tweak.ApplyAsync()` per tweak, so a profile's tweaks get one conflict check/restore point instead of none; a `TweakConflictException` shows a distinct "conflicting tweaks" snackbar
- Added a Preview (Dry-Run) button to `TweakRowView`, calling `Tweak.PreviewAsync()` and surfacing the result via a longer-duration `ISnackbarService` toast — the v0.5.0.0 `ITweak.PreviewAsync` path was previously unreachable from the UI
- **Critical**: `ShellViewModel.InitializeAsync()` had no error handling around the tweak catalog load; since it's called from an `async void` event handler (`MainWindow.xaml.cs`'s `OnLoaded`), any exception during catalog parsing (a malformed entry in the 97-tweak hand-authored JSON, a missing embedded resource) would crash the entire application on every launch with no recovery path. Now caught and surfaced as a status message + error snackbar instead
- `RegistryTweak.IsAppliedCoreAsync` and `ConflictDetectionService`'s apply-value comparison used `Equals(object, object)` on registry values, which is reference equality for arrays (`byte[]`/`string[]`) — a `Binary`-kind tweak (e.g. the shipped `CapsLock` scancode-map tweak) would always report `IsAppliedAsync() == false` even immediately after a successful apply, and two tweaks writing byte-for-byte identical binary/multi-string values to the same registry target would be falsely flagged as conflicting. Added `RegistryValueComparer.AreEqual` (structural `SequenceEqual` for array types, falls back to `Equals` otherwise) and used it in both places

### Removed

- `IThemeService`/`ThemeService` and `IAnimationService`/`AnimationService` (`SophiaWin11.Core`): dead code orphaned since `MainViewModel` was deleted in v0.6.0.0's UI rewrite — the real Art Déco theme is applied statically via `App.xaml` `ResourceDictionary` merging (v0.7.0.0) and the real reduced-motion gate is `SophiaWin11.UI/Animation/MotionPolicy.cs` (v0.8.0.0); `ThemeService.ApplyTheme()` did nothing beyond flipping a bool, and `AnimationService.AnimationsEnabled` was a hardcoded-`true` field nothing real ever read
- `IBackupService`/`BackupService` (`SophiaWin11.Core`): both methods were `NotImplementedException` stubs since v0.2.0.0, never called from anywhere, fully superseded by the working `ITweakSnapshotService` (per-tweak registry snapshot) and `IRestorePointService` (real Windows System Restore points)

### Known gaps at release

- The manual test campaign across Windows 11 25H2 and Windows 11 Enterprise LTSC 2024 (Home/Pro/Enterprise) called for by this milestone was not performed — it requires a dedicated VM fleet not available in this environment.
- The "stable 2h+, no memory leak (dotnet-trace)" acceptance criterion has only partial evidence: a 2-hour trace was started against the running app but the process was closed externally after ~14 minutes (confirmed unrelated to the application — a clean Generic Host shutdown, no exception). Those 14 minutes ran clean with no errors, but this falls short of the full 2h+ requirement.

## [0.9.0.0] - Unreleased

### Added

- 6 real Lottie/Bodymovin animated assets under `SophiaWin11.UI/Assets/Animations/` (44 KB total, well under the 5 MB budget): `splash-loop.json` (app-load ornament), `status-success.json`, `status-failure.json`, `status-progress.json` (fast-operation in-progress), `loading-mascot.json` (long-running `PowerShellNativeTweak` operations), `about-banner.json` (produced now, no host page until v1.2.0.0)
- `tools/animations/generate_lottie_assets.py`: standalone Python 3 stdlib-only script that emits valid Bodymovin JSON directly (no After Effects/GUI exporter available) using only Skottie-confirmed-supported features (shapes, trim paths, transforms — no raster/text/expressions); colors sourced from `DesignTokens.xaml` hex values kept as named constants in the script
- `SophiaWin11.Tests/Animation/LottieAssetTests.cs`: parses every asset through the real `SkiaSharp.Skottie.Animation.Create` parser (not a hand-rolled schema check), asserts sane `Duration`/`Fps`/`InPoint`/`OutPoint`, plus a total-size-under-5MB assertion — 7 new tests
- `COMPILATION.md`: new "Animation asset pipeline" section documenting the generation method, export/embedding path, validation approach, and a full animation style guide table (duration, loop, easing, `DesignTokens.xaml` token references per asset)
- `MainWindow.xaml`'s splash/loading state now plays the real `splash-loop.json` through `LottieAnimationPresenter`, replacing v0.8.0.0's placeholder procedural `LoopingOrnament` rotation; the redundant stock `ui:ProgressRing` shown alongside it was removed as visually redundant with the new animation
- `TweakRowView`/`TweakRowViewModel`: apply/revert now shows `status-progress.json` for fast (`RegistryTweak`) operations vs `loading-mascot.json` for long-running (`PowerShellNativeTweak`) ones, then `status-success.json`/`status-failure.json` on completion (auto-reset after 2.5s) — no default `ProgressBar`/`ProgressRing` anywhere in this flow

### Changed

- `Directory.Build.props` version bumped to `0.9.0.0`

### Notes

- The roadmap's "icônes de statut animées par catégorie" was interpreted as 3 generic states (success/failure/in-progress) reused across every category, not 3×8=24 category-specific animated variants — the 8 static category icons from v0.7.0.0 (`ArtDecoIcons.xaml`) are unrelated and out of scope here; documented in `COMPILATION.md`
- Fixed during review: the delegated work left `MainWindow.xaml.cs` with a missing `using SophiaWin11.UI.Animation;` (broke the build, `PageTransitions.Enter` calls unresolved) — restored

## [0.8.0.0] - Unreleased

### Added

- `SophiaWin11.UI/Animation/MotionPolicy.cs`: central gate reading `SystemParameters.ClientAreaAnimation` (Windows "Show animations" accessibility setting), live-updated via `SystemParameters.StaticPropertyChanged`; every animation helper below checks it and jumps straight to the end state when animations are disabled
- `PageTransitions.Enter`: `Storyboard`-equivalent `CubicEase`-driven fade + slide-in on navigation content swap (`MainWindow.xaml.cs`'s `ReplaceContent` path), ~200ms
- `ToggleBounceBehavior` (attached property on `ArtDecoTheme.xaml`'s `ToggleSwitch` style): gold overshoot-and-settle scale bounce on check/uncheck via `BackEase`, scaling the control's `RenderTransform` rather than guessing WPF-UI's internal knob `PART_` name
- `CardReveal.Play`: staggered fan-in reveal (opacity + scale-from-center, `CubicEase`) for `TweakRowView` rows, `BeginTime` offset per row index (capped) so a category page's rows cascade in rather than popping at once
- `LoopingOrnament`: seamless 360° rotation loop (`RepeatBehavior.Forever`) on a chevron/fan Art Déco ornament, shown on `MainWindow` while `ShellViewModel.InitializeAsync()` loads the tweak catalog (the app's splash-equivalent moment)
- `LottieAnimationPresenter` (`SophiaWin11.UI/Controls/`): Lottie/Bodymovin playback via `SkiaSharp.Skottie` + `SkiaSharp.Views.WPF` (`SKElement` render surface) — verified as real, published, MIT-licensed packages compatible with net9.0-windows before adding (`Wpf.Ui.Lottie` named in the roadmap does not exist as a published package); used as the tweak-apply-in-progress indicator on `TweakRowView`, replacing the default `ProgressBar`
- `AnimatedGifPresenter` (`SophiaWin11.UI/Controls/`): frame-by-frame GIF playback (`GifBitmapDecoder`, per-frame delay read from `/grctlext/Delay` metadata), decoded frames cached by `Uri` and frozen for cross-thread reuse, playback driven by a shared `CompositionTarget.Rendering`-based `RenderLoop`, `IsPlaying`/`Loop` dependency properties
- `RenderLoop`, `LoopClock`, `GifFrameTiming`: small testable helpers extracted from the two presenter controls (frame-index/elapsed-time resolution, loop-vs-clamp elapsed advancement) — unit tested directly rather than only through the WPF controls
- `SophiaWin11.Tests/Animation/`: `MotionPolicyTests`, `CardRevealTests`, `GifFrameTimingTests`, `LoopClockTests`, `RenderLoopTests` — 20 new tests, including the reduced-motion decision logic (`GetEffectiveDuration` returns `TimeSpan.Zero` when animations are disabled)

### Changed

- `TweakRowView`: apply/revert in-flight state now shows `LottieAnimationPresenter` instead of a default `ProgressBar`; rows play `CardReveal` on load
- `Directory.Build.props` version bumped to `0.8.0.0`
- `THIRD-PARTY-NOTICES.md`: added `SkiaSharp.Skottie` and `SkiaSharp.Views.WPF` (both MIT)

### Notes

- "Pas de latence perçue > 16ms/frame (60 FPS)" cannot be measured in a non-interactive environment; every animation here animates `RenderTransform`/`Opacity` only (never layout-affecting properties like `Width`/`Margin`), which is the GPU-friendly WPF animation pattern, but real frame-rate profiling on the reference hardware is still outstanding

## [0.7.0.0] - Unreleased

### Added

- Single non-switchable Art Déco theme: `SophiaWin11.UI/Theme/DesignTokens.xaml` extended with the full palette (deep black `#0B0B10`, charcoal `#151521`, metallic gold `#D4AF37` plus a `#B8860B`→`#F4E5A1` gradient brush, emerald `#0F5C4C`, bordeaux `#6E1423`, peacock `#0F3D5C`), WCAG-checked text tokens (`ColorTextPrimary`/`ColorTextSecondary`/`ColorTextOnAccent`), font-family tokens, and two procedural `DrawingBrush` resources (`BrushArtDecoMotif` — a tiled chevron/fan watermark, `BrushArtDecoNoise` — a tiled dot-grain texture) — no raster assets, both resolution-independent
- `SophiaWin11.UI/Theme/ArtDecoTheme.xaml`: reassigns every semantic brush key WPF-UI's stock Dark theme controls (`Button`, `CardControl`, `CardExpander`, `ToggleSwitch`, `NavigationView`/`NavigationViewItem`, the accent system) already read via `DynamicResource` (`ButtonBackground`, `CardBackground`, `ToggleSwitchFillOn/Off`, `AccentFillColorDefault/Secondary/Tertiary`, `NavigationViewItem*`, etc. — enumerated by fetching WPF-UI 4.0.2's actual control XAML from source rather than guessing), plus additive `BasedOn` styles per control adding gold `DropShadowEffect`s, decorated border thickness, and the embedded body font; merged into `App.xaml` after `ui:ThemesDictionary`/`ui:ControlsDictionary` so it wins every lookup — the stock Fluent dark palette is structurally present but never visually reaches the user
- `SophiaWin11.UI/Theme/ArtDecoIcons.xaml` + `SophiaWin11.UI/Controls/ArtDecoIcon.cs` (a `Wpf.Ui.Controls.IconElement` subclass wrapping a `Viewbox`/`Path`): 8 hand-authored 24x24 geometric Art Déco icons (stepped ziggurat ascent for Dashboard, ray-fan magnifier for Search, chevron shield for Privacy & Telemetry, symmetric sunburst fan for UI & Personalization, stepped concentric squares for System, stacked chevrons for Gaming, fan-lined shield for Microsoft Defender & Security, tapered stepped bars for Context menu); `ShellViewModel.BuildNavigationItems` now assigns these instead of `SymbolIcon`, resolved by category string against `Assets/Catalog/tweaks-en.json`'s real category names
- Embedded fonts (SIL OFL 1.1, real files fetched from `google/fonts` and `rsms/inter` upstream, ~660 KB total): Poiret One Regular, Cinzel Decorative Regular + Bold (display serif, titles), Inter Regular (body) — added as `Resource` build items in `SophiaWin11.UI.csproj` under `Assets/Fonts/`, referenced via `pack://application:,,,/SophiaWin11.UI;component/Assets/Fonts/#<family>`; embedded family names verified with `fontTools` rather than assumed from filenames
- Tinted Mica background: `MainWindow.xaml` layers a low-opacity charcoal tint, the chevron motif brush, and the noise-grain brush as three stacked `Border` overlays behind `ui:NavigationView`, on top of the existing `WindowBackdropType="Mica"` system backdrop
- `SophiaWin11.Tests/Theme/ContrastTests.cs`: parses `DesignTokens.xaml` directly via `System.Xml.Linq` (no hand-copied hex constants) and computes real WCAG relative-luminance contrast ratios for every foreground/background pair actually used in the app (gold-on-deep-black, gold-on-charcoal, primary/secondary text on both backgrounds, white risk-badge text on emerald/bordeaux/peacock), asserting 4.5:1 for normal text and 3:1 for the large gold display titles

### Changed

- `MainWindow.xaml` and all 4 `Views/*.xaml` pages: every hardcoded `Foreground="White"`, literal `FontSize`, and literal `FontWeight`-only text style replaced with `DynamicResource` tokens (`BrushTextOnAccent`, `BrushTextSecondary`, `FontSizeDisplayLarge/Medium/Body/Caption`, `FontFamilyDisplay/Body`); the "Restart required" indicator in `TweakRowView` is now a peacock-tinted badge instead of a plain dimmed `TextBlock`, giving the peacock accent color a real, contrast-tested use
- `Directory.Build.props` version bumped to `0.7.0.0`

### Notes

- Roadmap acceptance criterion "100% des composants utilisent le ResourceDictionary du thème, zéro style par défaut WPF visible" verified by grepping every `.xaml` under `src/SophiaWin11.App` and `src/SophiaWin11.UI` for hardcoded `Color`/`Brush`/hex-literal/`FontFamily` values outside the theme dictionaries themselves — none remain in the view/window XAML
- "Contraste texte/fond conforme WCAG AA" is covered by the `ContrastTests` pass/fail assertions added this milestone; the full automated accessibility report/tooling (`docs/accessibility-report.md`) remains v1.7.0.0 scope as roadmapped, not built here
- `Wpf.Ui.Controls.NavigationView`/`NavigationViewItem` were deliberately **not** given full `ControlTemplate` overrides — their real templates (`NavigationLeftFluent.xaml`) use internal `PART_`-style template parts wired up in code-behind for pane scrolling/back-button/breadcrumb behavior; retemplating them from a guessed structure risks silently breaking navigation, so they are reskinned entirely through the semantic brush keys they already expose via `DynamicResource`, which is sufficient to remove every trace of the stock Fluent look

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
