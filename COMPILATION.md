# Compilation

## Requirements

- Windows 11 (25H2 or later)
- .NET 9 SDK
- Windows App SDK workload (for WPF-UI / Mica backdrop APIs)

## Build

```
dotnet restore
dotnet build -c Release
```

`SophiaWin11.Core` and `SophiaWin11.Tests` target `net9.0` and can be built on any platform.
`SophiaWin11.UI` and `SophiaWin11.App` target `net9.0-windows10.0.22621.0` with WPF enabled and require Windows to build.

## Tests

```
dotnet test src/SophiaWin11.Tests
```

## Installer

Requires [Inno Setup 7](https://jrsoftware.org/isinfo.php).

```
ISCC.exe Installer/setup.iss
```

## Tweak engine (v0.4.0.0)

The tweak catalog is declarative JSON (`Assets/Catalog/tweaks-en.json`), embedded as
`SophiaWin11.Core.Catalog.tweaks.en.json` and loaded by `TweakCatalogLoader` into `ITweak`
instances at runtime. Three concrete tweak kinds implement `ITweak`:

| Kind | Backing mechanism | Status |
|---|---|---|
| `RegistryTweak` | `Microsoft.Win32.Registry` via `IRegistryService` | Functional |
| `PowerShellNativeTweak` | In-process PowerShell SDK (`Microsoft.PowerShell.SDK`) via `IPowerShellHost` | Functional |
| `Win32ApiTweak` | P/Invoke via `IWin32InteropHost` | Functional dispatch host; no ported function currently needs a genuine Win32 API call (see note below) |

No function in the full inventory below turned out to require a real P/Invoke call once its body was
read: every Sophia function COMPILATION.md's original heuristic tagged `Win32ApiTweak` was, on
inspection, actually a plain registry write, a PowerShell/DISM cmdlet call, or an interactive/one-shot
action outside the `ITweak` model (exactly the miscategorization the heuristic was known to make, e.g.
functions calling `[WinAPI.GetStrings]::GetString` only to fetch a localized progress string). Because
of this, `Win32ApiTweak`'s catalog usage is currently zero and `Win32InteropHost.InvokeAsync` is a real,
empty dispatch (throws `NotSupportedException` for any operation name) rather than a speculative generic
wrapper, per the porting brief's instruction to implement only what ported functions actually need.

Tweaks with `RiskLevel` `Medium` or `High` automatically trigger a pre-apply registry value
snapshot (`ITweakSnapshotService`), written to `%LOCALAPPDATA%\SophiaWin11\snapshots\`.

### Sophia function to ITweak mapping (full inventory)

Source: `src/Sophia_Script_for_Windows_11/Module/Sophia.psm1`, vendored from Sophia Script for
Windows 11 Enterprise LTSC 2024, v7.2.0 (2026-07-31), MIT-licensed, (c) Team Sophia. Full inventory:
**113 functions across 9 regions**. 97 implemented as of v0.4.0.0; 16 remain genuinely Pending, each
with a one-line reason in the Notes column — they were read in full and deliberately left unported
rather than risk an incorrect registry value or script (see the "Hard constraints" note in the v0.4.0.0
porting brief: accuracy over completeness for a system tweaking tool).

Where a Sophia function only exposed auxiliary "remove all policies / clear GPO overrides so the change
is visible in the Settings UI" steps alongside its core value, those auxiliary steps were intentionally
not ported (consistent with the 9 tweaks already shipped in v0.3.0.0) — the core registry value fully
reproduces the function's effect. Where a function changes two or more registry values together, or
calls an external cmdlet/tool (DISM, `Set-MpPreference`, `powercfg.exe`, `auditpol`, `Set-WinLanguageBarOption`,
etc.) instead of a plain `Set-ItemProperty`, it is ported as `PowerShellNativeTweak` with a minimal
faithful in-process script rather than split across multiple `RegistryTweak` rows.

#### Protection (0/2)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `Logging` | — | Pending | Starts a `Start-Transcript` log of the script's own console session; not applicable to a native app, no Apply/Revert state |
| `CreateRestorePoint` | — | Pending | One-shot action (`Enable-ComputerRestore` + `Checkpoint-Computer`); no meaningful "revert" of creating a restore point, doesn't fit the `ITweak` toggle model |

#### Privacy & Telemetry (15/15)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `DiagTrackService` | `PowerShellNativeTweak` | Implemented | |
| `DiagnosticDataLevel` | `RegistryTweak` | Implemented | |
| `ErrorReporting` | `PowerShellNativeTweak` | Implemented | |
| `FeedbackFrequency` | `RegistryTweak` | Implemented | |
| `ScheduledTasks` | `PowerShellNativeTweak` | Implemented | Curated list of `Disable-ScheduledTask`/`Enable-ScheduledTask` calls, not a Win32 API |
| `SigninInfo` | `PowerShellNativeTweak` | Implemented | Needs the current user's SID resolved at runtime (`Get-CimInstance Win32_UserAccount`) |
| `LanguageListAccess` | `RegistryTweak` | Implemented | |
| `AdvertisingID` | `RegistryTweak` | Implemented | |
| `WindowsWelcomeExperience` | `RegistryTweak` | Implemented | |
| `WindowsTips` | `RegistryTweak` | Implemented | |
| `SettingsSuggestedContent` | `RegistryTweak` | Implemented | |
| `AppsSilentInstalling` | `RegistryTweak` | Implemented | |
| `WhatsNewInWindows` | `RegistryTweak` | Implemented | |
| `TailoredExperiences` | `RegistryTweak` | Implemented | |
| `BingSearch` | `RegistryTweak` | Implemented | |

#### UI & Personalization (38/40)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `ThisPC` | `RegistryTweak` | Implemented | |
| `CheckBoxes` | `RegistryTweak` | Implemented | |
| `HiddenItems` | `RegistryTweak` | Implemented | |
| `FileExtensions` | `RegistryTweak` | Implemented | |
| `MergeConflicts` | `RegistryTweak` | Implemented | |
| `OpenFileExplorerTo` | `RegistryTweak` | Implemented | |
| `FileExplorerCompactMode` | `RegistryTweak` | Implemented | |
| `OneDriveFileExplorerAd` | `RegistryTweak` | Implemented | |
| `SnapAssist` | `PowerShellNativeTweak` | Implemented | Two registry values change together (`WindowArrangementActive` + `SnapAssist`) |
| `FileTransferDialog` | `RegistryTweak` | Implemented | |
| `RecycleBinDeleteConfirmation` | `PowerShellNativeTweak` | Implemented | Read-modify-write of one bit in the binary `ShellState` value; a fixed apply/revert byte array would clobber unrelated bits |
| `QuickAccessRecentFiles` | `RegistryTweak` | Implemented | |
| `QuickAccessFrequentFolders` | `RegistryTweak` | Implemented | |
| `TaskbarAlignment` | `RegistryTweak` | Implemented | |
| `TaskbarSearch` | `RegistryTweak` | Implemented | |
| `SearchHighlights` | `RegistryTweak` | Implemented | |
| `TaskViewButton` | `RegistryTweak` | Implemented | |
| `SecondsInSystemClock` | `RegistryTweak` | Implemented | |
| `ClockInNotificationCenter` | `RegistryTweak` | Implemented | |
| `TaskbarCombine` | `RegistryTweak` | Implemented | |
| `TaskbarEndTask` | `RegistryTweak` | Implemented | |
| `ControlPanelView` | `PowerShellNativeTweak` | Implemented | Two registry values change together (`AllItemsIconView` + `StartupPage`) |
| `WindowsColorMode` | `RegistryTweak` | Implemented | |
| `AppColorMode` | `RegistryTweak` | Implemented | |
| `FirstLogonAnimation` | `RegistryTweak` | Implemented | |
| `JPEGWallpapersQuality` | `RegistryTweak` | Implemented | |
| `ShortcutsSuffix` | `RegistryTweak` | Implemented | |
| `PrtScnSnippingTool` | `RegistryTweak` | Implemented | |
| `AppsLanguageSwitch` | `PowerShellNativeTweak` | Implemented | Calls the `Set-WinLanguageBarOption` cmdlet, not raw registry |
| `AeroShaking` | `RegistryTweak` | Implemented | |
| `Install-Cursors` | — | Pending | Downloads a third-party cursor pack ZIP from GitHub at apply time; violates the offline-first constraint |
| `FolderGroupBy` | `PowerShellNativeTweak` | Implemented | Creates/removes a `FolderTypes\...\TopViews` subtree; `Remove-Item -Recurse` has no `IRegistryService` equivalent |
| `NavigationPaneExpand` | `RegistryTweak` | Implemented | |
| `RecentlyAddedStartApps` | `RegistryTweak` | Implemented | |
| `UnpinAllStartTiles` | — | Pending | One-shot action (writes a temp pin-layout JSON, pulses a policy for 3 seconds, then removes both); no persistent state to revert |
| `MostUsedStartApps` | `RegistryTweak` | Implemented | |
| `StartRecommendedSection` | `PowerShellNativeTweak` | Implemented | Four registry values change together |
| `StartRecommendationsTips` | `RegistryTweak` | Implemented | |
| `StartAccountNotifications` | `RegistryTweak` | Implemented | |
| `StartLayout` | `RegistryTweak` | Implemented | |

#### System (29/36)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `StorageSense` | `PowerShellNativeTweak` | Implemented | Three registry values change together |
| `Hibernation` | `PowerShellNativeTweak` | Implemented | Calls `powercfg.exe /HIBERNATE` |
| `Win32LongPathsSupport` | `RegistryTweak` | Implemented | |
| `BSoDStopError` | `RegistryTweak` | Implemented | |
| `AdminApprovalMode` | `PowerShellNativeTweak` | Implemented | Sets 9 UAC policy values together; High risk |
| `DeliveryOptimization` | `PowerShellNativeTweak` | Implemented | Registry write plus `Delete-DeliveryOptimizationCache` cmdlet |
| `WindowsManageDefaultPrinter` | `RegistryTweak` | Implemented | |
| `WindowsFeatures` | `PowerShellNativeTweak` | Implemented | Curated preset list via `Enable/Disable-WindowsOptionalFeature`, not the original interactive checkbox dialog |
| `WindowsCapabilities` | `PowerShellNativeTweak` | Implemented | Curated preset list via `Add/Remove-WindowsCapability`, not the original interactive checkbox dialog |
| `UpdateMicrosoftProducts` | `RegistryTweak` | Implemented | |
| `RestartNotification` | `RegistryTweak` | Implemented | |
| `RestartDeviceAfterUpdate` | `RegistryTweak` | Implemented | |
| `ActiveHours` | `RegistryTweak` | Implemented | |
| `WindowsLatestUpdate` | `RegistryTweak` | Implemented | |
| `PowerPlan` | `PowerShellNativeTweak` | Implemented | Calls `powercfg.exe /SETACTIVE` |
| `NetworkAdaptersSavePower` | — | Pending | Enumerates physical adapters/Wi-Fi profile at runtime and reconnects the active Wi-Fi network as a side effect; not a static, safe apply/revert |
| `InputMethod` | `PowerShellNativeTweak` | Implemented | Calls the `Set-WinDefaultInputMethodOverride` cmdlet |
| `Set-UserShellFolderLocation` | — | Pending | Fully interactive per-folder relocation wizard (console menu / `FolderBrowserDialog`); no fixed target to apply |
| `WinPrtScrFolder` | `PowerShellNativeTweak` | Implemented | Reads the current Desktop folder path at runtime to build the target value |
| `RecommendedTroubleshooting` | `RegistryTweak` | Implemented | Only the core `WindowsMitigation\UserPreference` value is ported; the function's telemetry/error-reporting prerequisite side effects are skipped to avoid fighting the dedicated `DiagnosticDataLevel`/`ErrorReporting` tweaks |
| `ReservedStorage` | `PowerShellNativeTweak` | Implemented | Calls the `Set-WindowsReservedStorageState` cmdlet |
| `F1HelpPage` | `PowerShellNativeTweak` | Implemented | Revert needs `Remove-Item -Recurse` of a GUID Typelib subtree |
| `NumLock` | `RegistryTweak` | Implemented | Targets `HKEY_USERS\.DEFAULT`, modeled as `RegistryHive.Users` + `.DEFAULT\...` subkey |
| `CapsLock` | `RegistryTweak` | Implemented | Binary `Scancode Map` value; added `RegistryValueKind.Binary` conversion support to `TweakCatalogLoader` |
| `StickyShift` | `RegistryTweak` | Implemented | |
| `Autoplay` | `RegistryTweak` | Implemented | |
| `ThumbnailCacheRemoval` | `PowerShellNativeTweak` | Implemented | Same value set in both the 64-bit and WOW6432Node registry views |
| `SaveRestartableApps` | `RegistryTweak` | Implemented | |
| `RestorePreviousFolders` | `RegistryTweak` | Implemented | |
| `Set-Association` | — | Pending | Requires per-invocation user-supplied program path/extension/icon, computes a UserChoice hash to bypass UCPD; not a fixed toggle |
| `Export-Associations` | — | Pending | Dumps the live file-association state to a JSON file via DISM; a one-shot export, not a toggle, and depends on `Set-Association` |
| `Import-Associations` | — | Pending | Interactive `OpenFileDialog` picker that replays `Set-Association` calls; depends on the same Pending function |
| `Install-VCRedist` | — | Pending | Downloads and silently runs an installer executable from the internet; violates the offline-first constraint |
| `Install-DotNetRuntimes` | — | Pending | Downloads and silently runs an installer executable from the internet; violates the offline-first constraint |
| `PreventEdgeShortcutCreation` | `PowerShellNativeTweak` | Implemented | Stable channel only, guarded by `Get-Package` presence check, faithful to the original per-channel conditional |
| `RegistryBackup` | `PowerShellNativeTweak` | Implemented | Registry value plus `Enable-ScheduledTask` on the built-in `RegIdleBackup` task |

#### WSL (0/1)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `Install-WSL` | — | Pending | Fetches the distro list from GitHub and shows an interactive selection dialog before running `wsl.exe --install`; network + interactive, not a toggle |

#### Gaming (1/1)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `GPUScheduling` | `PowerShellNativeTweak` | Implemented | Preserves the original dedicated-GPU/WDDM 2.7+ hardware guard on Apply |

#### Scheduled tasks (0/3)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `CleanupTask` | — | Pending | Writes VBS/PS1 helper scripts under `%SystemRoot%\System32\Tasks`, registers a COM-based scheduled task, and compiles an inline C# WNF-state helper via `Add-Type`; far beyond a registry/cmdlet toggle |
| `SoftwareDistributionTask` | — | Pending | Same pattern as `CleanupTask` (VBS/PS1 helper scripts, COM task registration, inline `Add-Type` C#) |
| `TempTask` | — | Pending | Same pattern as `CleanupTask` (VBS/PS1 helper scripts, COM task registration, inline `Add-Type` C#) |

#### Microsoft Defender & Security (9/9)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `NetworkProtection` | `PowerShellNativeTweak` | Implemented | Calls `Set-MpPreference` |
| `PUAppsDetection` | `PowerShellNativeTweak` | Implemented | Calls `Set-MpPreference` |
| `DefenderSandbox` | `PowerShellNativeTweak` | Implemented | Calls `setx.exe /M` |
| `EventViewerCustomView` | `PowerShellNativeTweak` | Implemented | `auditpol` call plus several registry values and an Event Viewer view XML file |
| `AppsSmartScreen` | `RegistryTweak` | Implemented | |
| `SaveZoneInformation` | `RegistryTweak` | Implemented | |
| `WindowsSandbox` | `PowerShellNativeTweak` | Implemented | Preserves the original virtualization/Hyper-V hardware guard |
| `DNSoverHTTPS` | `PowerShellNativeTweak` | Implemented | Cloudflare preset only, dynamic per-adapter `Set-DnsClientServerAddress` + registry DoH template, faithful to the original adapter-enumeration logic; High risk (reconfigures live DNS) |
| `LocalSecurityAuthority` | `PowerShellNativeTweak` | Implemented | Preserves the original virtualization/Hyper-V hardware guard |

#### Context menu (5/6)

| Sophia function | Target ITweak kind | Status | Notes |
|---|---|---|---|
| `MSIExtractContext` | `PowerShellNativeTweak` | Implemented | Three registry values change together |
| `CABInstallContext` | `PowerShellNativeTweak` | Implemented | Preserves the original third-party-archiver guard |
| `PrintCMDContext` | `PowerShellNativeTweak` | Implemented | Same value set on two file-type keys (`batfile`, `cmdfile`) |
| `CompressedFolderNewContext` | `PowerShellNativeTweak` | Implemented | Binary + ExpandString values change together |
| `MultipleInvokeContext` | `RegistryTweak` | Implemented | |
| `ScanRegistryPolicies` | — | Pending | One-shot registry-to-ADMX policy visibility scan depending on the module-internal `Set-Policy` helper, which isn't vendored in this excerpt; not a toggle |

**Total: 97/113 functions implemented, 16 explicitly Pending with reasons above.**

