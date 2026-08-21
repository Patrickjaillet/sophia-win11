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
