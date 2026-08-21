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

## Tweak engine (v0.3.0.0)

The tweak catalog is declarative JSON (`Assets/Catalog/tweaks-en.json`), embedded as
`SophiaWin11.Core.Catalog.tweaks.en.json` and loaded by `TweakCatalogLoader` into `RegistryTweak`
instances at runtime. Three concrete tweak kinds implement `ITweak`:

| Kind | Backing mechanism | Status |
|---|---|---|
| `RegistryTweak` | `Microsoft.Win32.Registry` via `IRegistryService` | Functional |
| `PowerShellNativeTweak` | In-process PowerShell SDK via `IPowerShellHost` | Interface ready, SDK hosting lands in v0.4.0.0 |
| `Win32ApiTweak` | P/Invoke via `IWin32InteropHost` | Interface ready, P/Invoke lands in v0.4.0.0 |

Tweaks with `RiskLevel` `Medium` or `High` automatically trigger a pre-apply registry value
snapshot (`ITweakSnapshotService`), written to `%LOCALAPPDATA%\SophiaWin11\snapshots\`.

### Sophia function to ITweak mapping (full inventory)

Source: `src/Sophia_Script_for_Windows_11/Module/Sophia.psm1`, vendored from Sophia Script for
Windows 11 Enterprise LTSC 2024, v7.2.0 (2026-07-31), MIT-licensed, (c) Team Sophia. Full inventory:
**113 functions across 9 regions**, 9 implemented as of v0.3.0.0,
the remaining 104 scheduled for v0.4.0.0 (full port, category by category).

#### Protection (0/2)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `Logging` | `PowerShellNativeTweak` | Pending |
| `CreateRestorePoint` | `PowerShellNativeTweak` (via `IBackupService`) | Pending |

#### Privacy & Telemetry (9/15)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `DiagTrackService` | `PowerShellNativeTweak` | Implemented |
| `DiagnosticDataLevel` | `RegistryTweak` | Implemented |
| `ErrorReporting` | `PowerShellNativeTweak` | Implemented |
| `FeedbackFrequency` | `RegistryTweak` | Implemented |
| `ScheduledTasks` | `Win32ApiTweak` | Pending |
| `SigninInfo` | `RegistryTweak` | Pending |
| `LanguageListAccess` | `RegistryTweak` | Implemented |
| `AdvertisingID` | `RegistryTweak` | Implemented |
| `WindowsWelcomeExperience` | `RegistryTweak` | Implemented |
| `WindowsTips` | `RegistryTweak` | Implemented |
| `SettingsSuggestedContent` | `RegistryTweak` | Implemented |
| `AppsSilentInstalling` | `RegistryTweak` | Pending |
| `WhatsNewInWindows` | `RegistryTweak` | Pending |
| `TailoredExperiences` | `RegistryTweak` | Pending |
| `BingSearch` | `RegistryTweak` | Pending |

#### UI & Personalization (0/40)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `ThisPC` | `RegistryTweak` | Pending |
| `CheckBoxes` | `RegistryTweak` | Pending |
| `HiddenItems` | `RegistryTweak` | Pending |
| `FileExtensions` | `RegistryTweak` | Pending |
| `MergeConflicts` | `RegistryTweak` | Pending |
| `OpenFileExplorerTo` | `RegistryTweak` | Pending |
| `FileExplorerCompactMode` | `RegistryTweak` | Pending |
| `OneDriveFileExplorerAd` | `RegistryTweak` | Pending |
| `SnapAssist` | `RegistryTweak` | Pending |
| `FileTransferDialog` | `RegistryTweak` | Pending |
| `RecycleBinDeleteConfirmation` | `RegistryTweak` | Pending |
| `QuickAccessRecentFiles` | `RegistryTweak` | Pending |
| `QuickAccessFrequentFolders` | `RegistryTweak` | Pending |
| `TaskbarAlignment` | `RegistryTweak` | Pending |
| `TaskbarSearch` | `RegistryTweak` | Pending |
| `SearchHighlights` | `RegistryTweak` | Pending |
| `TaskViewButton` | `RegistryTweak` | Pending |
| `SecondsInSystemClock` | `RegistryTweak` | Pending |
| `ClockInNotificationCenter` | `RegistryTweak` | Pending |
| `TaskbarCombine` | `RegistryTweak` | Pending |
| `TaskbarEndTask` | `RegistryTweak` | Pending |
| `ControlPanelView` | `RegistryTweak` | Pending |
| `WindowsColorMode` | `RegistryTweak` | Pending |
| `AppColorMode` | `RegistryTweak` | Pending |
| `FirstLogonAnimation` | `RegistryTweak` | Pending |
| `JPEGWallpapersQuality` | `RegistryTweak` | Pending |
| `ShortcutsSuffix` | `RegistryTweak` | Pending |
| `PrtScnSnippingTool` | `RegistryTweak` | Pending |
| `AppsLanguageSwitch` | `PowerShellNativeTweak` | Pending |
| `AeroShaking` | `RegistryTweak` | Pending |
| `Install-Cursors` | `Win32ApiTweak` | Pending |
| `FolderGroupBy` | `RegistryTweak` | Pending |
| `NavigationPaneExpand` | `RegistryTweak` | Pending |
| `RecentlyAddedStartApps` | `RegistryTweak` | Pending |
| `UnpinAllStartTiles` | `RegistryTweak` | Pending |
| `MostUsedStartApps` | `RegistryTweak` | Pending |
| `StartRecommendedSection` | `RegistryTweak` | Pending |
| `StartRecommendationsTips` | `RegistryTweak` | Pending |
| `StartAccountNotifications` | `RegistryTweak` | Pending |
| `StartLayout` | `RegistryTweak` | Pending |

#### System (0/36)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `StorageSense` | `RegistryTweak` | Pending |
| `Hibernation` | `PowerShellNativeTweak` | Pending |
| `Win32LongPathsSupport` | `RegistryTweak` | Pending |
| `BSoDStopError` | `RegistryTweak` | Pending |
| `AdminApprovalMode` | `RegistryTweak` | Pending |
| `DeliveryOptimization` | `RegistryTweak` | Pending |
| `WindowsManageDefaultPrinter` | `RegistryTweak` | Pending |
| `WindowsFeatures` | `Win32ApiTweak` | Pending |
| `WindowsCapabilities` | `Win32ApiTweak` | Pending |
| `UpdateMicrosoftProducts` | `RegistryTweak` | Pending |
| `RestartNotification` | `RegistryTweak` | Pending |
| `RestartDeviceAfterUpdate` | `RegistryTweak` | Pending |
| `ActiveHours` | `RegistryTweak` | Pending |
| `WindowsLatestUpdate` | `RegistryTweak` | Pending |
| `PowerPlan` | `RegistryTweak` | Pending |
| `NetworkAdaptersSavePower` | `Win32ApiTweak` | Pending |
| `InputMethod` | `RegistryTweak` | Pending |
| `Set-UserShellFolderLocation` | `Win32ApiTweak` | Pending |
| `WinPrtScrFolder` | `RegistryTweak` | Pending |
| `RecommendedTroubleshooting` | `PowerShellNativeTweak` | Pending |
| `ReservedStorage` | `PowerShellNativeTweak` | Pending |
| `F1HelpPage` | `RegistryTweak` | Pending |
| `NumLock` | `RegistryTweak` | Pending |
| `CapsLock` | `RegistryTweak` | Pending |
| `StickyShift` | `RegistryTweak` | Pending |
| `Autoplay` | `RegistryTweak` | Pending |
| `ThumbnailCacheRemoval` | `RegistryTweak` | Pending |
| `SaveRestartableApps` | `RegistryTweak` | Pending |
| `RestorePreviousFolders` | `Win32ApiTweak` | Pending |
| `Set-Association` | `Win32ApiTweak` | Pending |
| `Export-Associations` | `PowerShellNativeTweak` | Pending |
| `Import-Associations` | `RegistryTweak` | Pending |
| `Install-VCRedist` | `RegistryTweak` | Pending |
| `Install-DotNetRuntimes` | `RegistryTweak` | Pending |
| `PreventEdgeShortcutCreation` | `RegistryTweak` | Pending |
| `RegistryBackup` | `PowerShellNativeTweak` | Pending |

#### WSL (0/1)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `Install-WSL` | `PowerShellNativeTweak` | Pending |

#### Gaming (0/1)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `GPUScheduling` | `RegistryTweak` | Pending |

#### Scheduled tasks (0/3)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `CleanupTask` | `Win32ApiTweak` | Pending |
| `SoftwareDistributionTask` | `Win32ApiTweak` | Pending |
| `TempTask` | `Win32ApiTweak` | Pending |

#### Microsoft Defender & Security (0/9)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `NetworkProtection` | `PowerShellNativeTweak` | Pending |
| `PUAppsDetection` | `PowerShellNativeTweak` | Pending |
| `DefenderSandbox` | `PowerShellNativeTweak` | Pending |
| `EventViewerCustomView` | `RegistryTweak` | Pending |
| `AppsSmartScreen` | `RegistryTweak` | Pending |
| `SaveZoneInformation` | `RegistryTweak` | Pending |
| `WindowsSandbox` | `PowerShellNativeTweak` | Pending |
| `DNSoverHTTPS` | `RegistryTweak` | Pending |
| `LocalSecurityAuthority` | `RegistryTweak` | Pending |

#### Context menu (0/6)

| Sophia function | Target ITweak kind | Status |
|---|---|---|
| `MSIExtractContext` | `Win32ApiTweak` | Pending |
| `CABInstallContext` | `Win32ApiTweak` | Pending |
| `PrintCMDContext` | `RegistryTweak` | Pending |
| `CompressedFolderNewContext` | `RegistryTweak` | Pending |
| `MultipleInvokeContext` | `RegistryTweak` | Pending |
| `ScanRegistryPolicies` | `Win32ApiTweak` | Pending |

**Total: 9/113 functions implemented.**

