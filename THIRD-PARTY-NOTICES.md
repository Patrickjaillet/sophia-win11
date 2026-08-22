# Third-Party Notices

## Libraries compiled into the application

| Library | License | Usage |
|---|---|---|
| WPF-UI | MIT | UI controls, Fluent/Mica shell |
| CommunityToolkit.Mvvm | MIT | MVVM source generators |
| Serilog | Apache-2.0 | Local file logging |
| Serilog.Sinks.File | Apache-2.0 | Local log file sink (no network sink) |
| Serilog.Extensions.Hosting | Apache-2.0 | Wires Serilog into the `Microsoft.Extensions.Hosting` generic host |
| Microsoft.Extensions.Hosting | MIT | DI container / host bootstrap |
| Microsoft.Extensions.DependencyInjection | MIT | Dependency injection container |
| Microsoft.Extensions.DependencyInjection.Abstractions | MIT | DI abstractions consumed by `SophiaWin11.Core` (no UI dependency) |
| Microsoft.Extensions.Logging.Abstractions | MIT | Logging abstractions consumed by `SophiaWin11.Core` |
| Microsoft.PowerShell.SDK | MIT | In-process PowerShell SDK hosting for `PowerShellNativeTweak` (never shells out to `powershell.exe`) |
| Sophia Script for Windows (© Team Sophia) | MIT | Vendored reference source (`src/Sophia_Script_for_Windows_11/Module/Sophia.psm1`) for tweak inventory/porting — not compiled into the application |
| Poiret One (© 2011 The Poiret One Project Authors) | SIL Open Font License 1.1 | Embedded display font, `src/SophiaWin11.UI/Assets/Fonts/PoiretOne-Regular.ttf`, used for the Art Déco accent typography (title bar) |
| Cinzel Decorative (© 2012 Natanael Gama) | SIL Open Font License 1.1 | Embedded display font, `src/SophiaWin11.UI/Assets/Fonts/CinzelDecorative-Regular.ttf` and `CinzelDecorative-Bold.ttf`, used for page/section headings |
| Inter (© 2016 The Inter Project Authors) | SIL Open Font License 1.1 | Embedded body font, `src/SophiaWin11.UI/Assets/Fonts/Inter-Regular.ttf`, used for all body/UI text |
| SkiaSharp.Skottie | MIT | Lottie/Bodymovin animation playback engine, used by `SophiaWin11.UI/Controls/LottieAnimationPresenter.cs` |
| SkiaSharp.Views.WPF | MIT | WPF `SKElement` host surface for SkiaSharp/Skottie rendering |

## Test-only tooling (not compiled into the shipped application)

Used exclusively by `SophiaWin11.Tests` to build and run the unit test suite; none of these
assemblies are referenced by, or packaged with, `SophiaWin11.App`/`SophiaWin11.Core`/`SophiaWin11.UI`.
Listed for transparency even though they fall outside the "embedded third-party library" acceptance
criterion.

| Library | License | Usage |
|---|---|---|
| xUnit | Apache-2.0 | Unit testing framework |
| xunit.runner.visualstudio | Apache-2.0 | Test discovery/execution adapter |
| Microsoft.NET.Test.Sdk | MIT | .NET test host/SDK |
| coverlet.collector | MIT | Code coverage collection |
