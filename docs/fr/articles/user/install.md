---
source_hash: efa0e487ab8d5e0d0a48f4f88281e5ec483a36ad
---
<a id="installation-guide"></a>

# Guide d'installation

<a id="prerequisites"></a>

## Prérequis

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) ou une version ultérieure
- Un IDE avec son extension Cosmos : [Visual Studio Code](https://code.visualstudio.com/), [Visual Studio](https://visualstudio.microsoft.com/) ou [JetBrains Rider](https://www.jetbrains.com/rider/) (voir [Extensions pour IDE](#ide-extensions))
- [Homebrew](https://brew.sh/) sous macOS

## Windows

Téléchargez et lancez le dernier installateur depuis la page [Releases](https://github.com/CosmosOS/Cosmos/releases) :

```
CosmosSetup-<version>-windows.exe
```

L'installateur recherche VS Code, Visual Studio et Rider, et propose d'installer l'extension Cosmos dans chacun de ceux qu'il trouve (voir [Extensions pour IDE](#ide-extensions)). Visual Studio doit être fermé, et l'extension Visual Studio demande les droits d'administrateur, car elle s'installe pour tous les utilisateurs. Rider n'est détecté qu'après avoir été lancé au moins une fois.

Après l'installation, ouvrez un nouveau terminal et vérifiez :

```powershell
cosmos check
```

Pour désinstaller, utilisez **Ajouter ou supprimer des programmes** dans les Paramètres de Windows.

## Linux / macOS

Installez la CLI Cosmos et toutes ses dépendances :

```bash
dotnet tool install -g Cosmos.Tools
cosmos install
```

Cette commande télécharge les bundles précompilés de la chaîne d'outils (Clang et LLD, xorriso, QEMU, GDB) depuis la release GitHub `tools-latest` vers `~/.cosmos/tools/`, puis installe le patcher Cosmos, les modèles de projet et l'extension VS Code. Un outil déjà présent dans le `PATH` est utilisé à la place du bundle lorsque sa version correspond à celle du bundle.

Vérifiez l'installation :

```bash
cosmos check
```

Pour désinstaller :

```bash
cosmos uninstall
dotnet tool uninstall -g Cosmos.Tools
```

Pour mettre à jour :

```bash
cosmos update
```

Exécutez cette commande dans le dossier d'un projet de noyau pour faire aussi passer les versions de Cosmos épinglées par le projet (l'attribut `Sdk="Cosmos.Sdk/..."` et les références de package `Cosmos.*`) à la dernière version. Options supplémentaires :

| Option | Effet |
|--------|--------|
| `cosmos update --check` | Signale les mises à jour disponibles sans rien installer |
| `cosmos update --no-project` | Met à jour les outils sans toucher aux fichiers du projet |
| `cosmos update --version <VERSION>` | Fait passer la CLI, le patcher, les modèles et les versions épinglées du projet à une version précise (les outils système suivent toujours les bundles `tools-latest`) |

<a id="macos-on-apple-silicon"></a>

## macOS sur Apple Silicon

Les bundles d'outils sont compilés uniquement pour Apple Silicon (`darwin-arm64`) ; il n'existe pas de bundle Intel. Un Mac Apple Silicon nécessite les étapes suivantes en plus de celles de Linux / macOS. `cosmos check` ne signale pas l'absence de Rosetta 2 ou de SDL 3.

| Étape | Pourquoi |
|------|-----|
| Installer Rosetta 2 | Le SDK exécute l'ILCompiler x64 (`runtime.osx-x64.Microsoft.DotNet.ILCompiler`) sur tout hôte macOS. Sans Rosetta 2, `cosmos build` échoue avec `error MSB3073` et le code de sortie 126 d'ILC. |
| Installer SDL 3 avec Homebrew | Le QEMU du bundle charge SDL 3 depuis Homebrew pour sa fenêtre d'affichage et plante au démarrage lorsque la bibliothèque est absente. `cosmos run --headless` n'en a pas besoin. |
| Signer à nouveau le QEMU du bundle | macOS tue le binaire du bundle à son lancement lorsque sa signature de code est absente ou invalide. Il en va de même pour `qemu-system-x86_64.real` lorsque vous exécutez des noyaux x64. |
| Passer `-a arm64` à `cosmos run` | `cosmos build` cible arm64 par défaut sur Apple Silicon, mais `cosmos run` cible x64 par défaut et cherche l'ISO dans `output-x64/`. |

```bash
softwareupdate --install-rosetta
brew install sdl3
codesign --force --deep --sign - ~/.cosmos/tools/qemu/qemu-system-aarch64.real
codesign --force --deep --sign - ~/.cosmos/tools/qemu/qemu-system-x86_64.real
```

Ensuite, compilez et lancez :

```bash
cosmos build
cosmos run -a arm64
```

Si un build a échoué avant l'installation de Rosetta 2, supprimez le dossier `obj/` du projet avant de relancer le build.

<a id="ide-extensions"></a>

## Extensions pour IDE

Chaque extension crée, compile, lance et débogue un noyau depuis l'IDE, et peut vérifier et installer elle-même la chaîne d'outils.

| IDE | Extension | Installation |
|-----|-----------|---------|
| Visual Studio Code | [CosmosVsCodeExtension](https://github.com/CosmosOS/CosmosVsCodeExtension) | L'installateur Windows ou `cosmos install` |
| Visual Studio 2026 (ou 2022 17.14+) | [CosmosVsExtension](https://github.com/CosmosOS/CosmosVsExtension) | L'installateur Windows, ou le `.vsix` de la [page Releases](https://github.com/CosmosOS/CosmosVsExtension/releases) |
| JetBrains Rider 2025.3+ | [CosmosRiderExtension](https://github.com/CosmosOS/CosmosRiderExtension) | L'installateur Windows, ou le plugin de la [page Releases](https://github.com/CosmosOS/CosmosRiderExtension/releases) |

<a id="quick-start"></a>

## Démarrage rapide

Une fois l'installation terminée, consultez [Démarrage du noyau](startup.md) pour créer votre premier noyau et découvrir le déroulement du démarrage.
