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

### Sophia Script function → ITweak mapping

Source: `src/Sophia_Script_for_Windows_11/Module/Sophia.psm1`, Sophia Script for Windows v7.1.4
(Windows 11 24H2+, 2026). The full 150+ function inventory is ported incrementally through
v0.4.0.0; the table below lists the functions currently wired into the seed catalog.

| Sophia function | Category | ITweak.Name | Kind |
|---|---|---|---|
| `DiagTrackService -Disable` | Privacy & Telemetry | DiagTrackService | RegistryTweak |
| `DiagnosticDataLevel -Minimal` | Privacy & Telemetry | DiagnosticDataLevel | RegistryTweak |
| `ErrorReporting -Disable` | Privacy & Telemetry | ErrorReporting | RegistryTweak |
| `FeedbackFrequency -Never` | Privacy & Telemetry | FeedbackFrequency | RegistryTweak |
| `LanguageListAccess -Disable` | Privacy & Telemetry | LanguageListAccess | RegistryTweak |
| `AdvertisingID -Disable` | Privacy & Telemetry | AdvertisingID | RegistryTweak |
| `WindowsWelcomeExperience -Hide` | Privacy & Telemetry | WindowsWelcomeExperience | RegistryTweak |
| `WindowsTips -Disable` | Privacy & Telemetry | WindowsTips | RegistryTweak |
| `SettingsSuggestedContent -Hide` | Privacy & Telemetry | SettingsSuggestedContent | RegistryTweak |

Pending for v0.4.0.0: `Logging`, `CreateRestorePoint` (System Protection region, mapped to
`PowerShellNativeTweak`/`IBackupService` once SDK hosting lands), `SigninInfo` (per-user SID
resolution), and the remaining UI & Personalization, System, Scheduled Tasks, Microsoft
Defender, Application Management, and Context Menu regions (~140 functions).
