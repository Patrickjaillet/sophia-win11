# Sophia Script for Win11

**Sprachen:** [English](README.md) | [Français](README.fr.md) | **Deutsch** | [Русский](README.ru.md) | [Українська](README.uk.md)

[![Version](https://img.shields.io/badge/version-1.1.0.0-D4AF37?style=flat-square)](https://github.com/Patrickjaillet/sophia-win11/releases/tag/v1.1.0.0)
[![License](https://img.shields.io/badge/license-MIT-D4AF37?style=flat-square)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2011-D4AF37?style=flat-square)](#requirements)

Eine native Windows-11-Tweaking-Anwendung in C# / WPF, die den vollständigen Tweak-Katalog von [Sophia Script for Windows](https://github.com/farag2/Sophia-Script-for-Windows) mit einer grafischen Oberfläche neu umsetzt — keine PowerShell-Konsole erforderlich.

Nur für Windows 11 (25H2+ / Enterprise LTSC 2024). Offline-first: keinerlei Netzwerkabhängigkeit zur Laufzeit, jede Ressource und Laufzeitumgebung ist lokal eingebettet.

![Dashboard](docs/screenshots/dashboard.png)

## Funktionen

- **97 Tweaks** in 6 Kategorien — Datenschutz & Telemetrie, Benutzeroberfläche & Personalisierung, System, Gaming, Microsoft Defender & Sicherheit, Kontextmenü — jeweils mit Risikostufe, einem Ein-Klick-Anwenden/Zurücksetzen und einer Vorschau-Schaltfläche, die genau zeigt, was sich ändern würde, bevor man sich festlegt.
- **Sicherheitsnetz bei jeder Anwendung**: widersprüchliche Tweaks werden erkannt und blockiert, bevor irgendetwas ausgeführt wird, bei mittleren/hohen Risikositzungen wird automatisch ein Windows-Systemwiederherstellungspunkt erstellt, und vor risikoreichen Änderungen läuft eine Systemzustandsprüfung.
- **Profile**: Speichern Sie Ihre aktuell angewendeten Tweaks in einer portablen `.sophiaprofile`-Datei und wenden Sie sie auf einem anderen Rechner wieder an.
- **Sofortige unscharfe Suche** über Name und Beschreibung jedes Tweaks.
- **Art-Déco-Design** — eine einzige, feste visuelle Identität (Gold auf tiefem Schwarz, eigene Symbolik, animierte Übergänge) statt eines generischen Systemlooks.

![Kategorie Datenschutz & Telemetrie](docs/screenshots/category-privacy-telemetry.png)

Weitere Screenshots — der Info-Tab und die geführte Sitzungsansicht — werden hinzugefügt, sobald diese Seiten fertig sind (siehe [Roadmap-Status](#status)).

## Installation

1. Laden Sie `SophiaWin11-Setup.exe` aus der [neuesten Version](https://github.com/Patrickjaillet/sophia-win11/releases/latest) herunter.
2. Führen Sie die Datei aus und folgen Sie dem Installationsassistenten. Administratorrechte sind erforderlich — die App nimmt Änderungen auf Registrierungs- und Systemrichtlinienebene vor.
3. Starten Sie **Sophia Script for Win11** über das Startmenü.

Eine separate Installation der .NET-Laufzeitumgebung ist nicht nötig — der Installer enthält einen eigenständigen Build.

## Voraussetzungen

- Windows 11, Build 25H2 oder höher, oder Windows 11 Enterprise LTSC 2024
- Administratorrechte (die Rechteerweiterung wird beim Start einmalig angefordert, nicht pro Tweak)

## Status

Dies ist ein aktiv weiterentwickeltes Projekt. `v1.0.0.0` ist ein Release Candidate mit Funktionseinfrierung, der die vollständige Tweak-Engine, das Sicherheitsnetz (Konflikterkennung, Wiederherstellungspunkte, Zustandsdiagnose), die komplette UI-Shell sowie das Art-Déco-Design-/Animationssystem umfasst. Lokalisierung, der Info-Tab, geführte Sitzungen und der signierte Installer stehen für kommende Versionen auf der Roadmap.

## Lizenz

MIT — siehe [LICENSE](LICENSE).

Sophia Script — Copyright © 2026 Dmitry Nefedov ([Originalprojekt](https://github.com/farag2/Sophia-Script-for-Windows))
UI für Windows 11 — Copyright © 2026 Patrick JAILLET
