# Sophia Script for Win11

**Langues :** [English](README.md) | [**Français**](README.fr.md) | [Deutsch](README.de.md) | [Русский](README.ru.md) | [Українська](README.uk.md)

[![Version](https://img.shields.io/badge/version-1.1.0.0-D4AF37?style=flat-square)](https://github.com/Patrickjaillet/sophia-win11/releases/tag/v1.1.0.0)
[![License](https://img.shields.io/badge/license-MIT-D4AF37?style=flat-square)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2011-D4AF37?style=flat-square)](#requirements)

Une application native d'optimisation pour Windows 11, écrite en C# / WPF, qui réimplémente l'intégralité du catalogue de réglages de [Sophia Script for Windows](https://github.com/farag2/Sophia-Script-for-Windows) avec une interface graphique — sans nécessiter de console PowerShell.

Windows 11 uniquement (25H2+ / Enterprise LTSC 2024). Offline-first : aucune dépendance réseau au runtime, chaque ressource et chaque runtime est intégré localement.

![Tableau de bord](docs/screenshots/dashboard.png)

## Fonctionnalités

- **97 réglages** répartis sur 6 catégories — Confidentialité et télémétrie, Interface et personnalisation, Système, Jeu, Microsoft Defender et sécurité, Menu contextuel — chacun avec un niveau de risque, une application/annulation en un clic, et un bouton Aperçu qui montre exactement ce qui va changer avant de valider.
- **Filet de sécurité à chaque application** : les réglages en conflit sont détectés et bloqués avant toute exécution, un point de restauration système Windows est créé automatiquement pour les sessions à risque moyen/élevé, et une vérification de l'état du système est effectuée avant les changements à risque élevé.
- **Profils** : enregistrez l'ensemble de vos réglages actuellement appliqués dans un fichier `.sophiaprofile` portable, et rechargez-le sur une autre machine.
- **Recherche floue instantanée** sur le nom et la description de chaque réglage.
- **Thème Art Déco** — une identité visuelle unique et fixe (or sur noir profond, iconographie personnalisée, transitions animées) plutôt qu'un aspect système générique.

![Catégorie Confidentialité et télémétrie](docs/screenshots/category-privacy-telemetry.png)

D'autres captures d'écran — l'onglet À propos et la vue de session guidée — seront ajoutées une fois ces pages livrées (voir [État du projet](#état-du-projet)).

## Installation

1. Téléchargez `SophiaWin11-Setup.exe` depuis la [dernière version](https://github.com/Patrickjaillet/sophia-win11/releases/latest).
2. Exécutez-le et suivez l'assistant d'installation. Les droits administrateur sont requis — l'application applique des modifications au niveau du registre et des stratégies système.
3. Lancez **Sophia Script for Win11** depuis le menu Démarrer.

Aucune installation séparée du runtime .NET n'est nécessaire — l'installateur intègre une version autonome (self-contained).

## Configuration requise

- Windows 11, build 25H2 ou ultérieure, ou Windows 11 Enterprise LTSC 2024
- Droits administrateur (l'élévation est demandée une seule fois au lancement, pas à chaque réglage)

## État du projet

Ce projet est en développement actif. `v1.0.0.0` est une version candidate en gel des fonctionnalités, couvrant le moteur complet de réglages, le filet de sécurité (détection de conflits, points de restauration, diagnostics de santé), l'ensemble de l'interface utilisateur, ainsi que le thème et le système d'animation Art Déco. La localisation, l'onglet À propos, les sessions guidées et l'installateur signé figurent au programme des prochaines versions.

## Licence

MIT — voir [LICENSE](LICENSE).

Sophia Script — Copyright © 2026 Dmitry Nefedov ([projet original](https://github.com/farag2/Sophia-Script-for-Windows))
Interface pour Windows 11 — Copyright © 2026 Patrick JAILLET
