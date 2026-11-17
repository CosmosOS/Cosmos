---
source_hash: e13b28e10d02259f3eddc1aeab6dbb7dd2d1a471
---
<a id="dev-container-setup"></a>

# Configuration du dev container

Pour développer gen3 ou y contribuer, un [dev container](https://containers.dev/) est fourni afin de mettre en place un environnement cohérent avec tous les outils nécessaires. Une fois le conteneur créé, le script `postCreateCommand` compile tous les packages Cosmos et installe les outils globaux nécessaires.

Si vous ne voulez pas utiliser Docker, vous pouvez installer le framework directement sur votre machine hôte en lançant le script manuellement.

## Linux / macOS (`postCreateCommand.sh`)

Lancez le script depuis la racine du dépôt :

```bash
./.devcontainer/postCreateCommand.sh
```

<a id="what-it-does"></a>

### Ce que fait le script

1. **Vide le cache NuGet** de tous les packages `cosmos.*` existants, pour garantir une restauration propre.
2. **Supprime tous les artefacts de build** (`artifacts/`) et les dossiers intermédiaires `obj` sous `src/`.
3. **Enregistre une source NuGet locale** dans `artifacts/package/release`, afin que les packages compilés localement soient préférés lors de la restauration.
4. **Compile et empaquette chaque package Cosmos** dans l'ordre des dépendances :
   - Bibliothèques de base (`Cosmos.Build.API`, `Cosmos.Build.Common`)
   - Outils de build (`Cosmos.Build.Asm`, `Cosmos.Build.CC`, `Cosmos.Build.Ilc`, `Cosmos.Build.Patcher`, `Cosmos.Patcher`, `Cosmos.Tools`)
   - Packages runtime natifs pour x64 et ARM64 (`Cosmos.Kernel.Native.*`)
   - Packages de noyau indépendants de l'architecture (`Cosmos.Kernel.Drivers`, `Cosmos.Kernel.Debug`, `Cosmos.Kernel.Boot.Limine`, `Cosmos.Kernel.SourceGenerators`)
   - Packages HAL propres à chaque architecture (`Cosmos.Kernel.HAL.X64`, `Cosmos.Kernel.HAL.ARM64`)
   - Packages de noyau multi-architecture compilés à la fois pour `linux-x64` et `linux-arm64` (`Cosmos.Kernel.Core`, `Cosmos.Kernel.HAL`, `Cosmos.Kernel.System`, `Cosmos.Kernel.Plugs`, `Cosmos.Kernel`)
   - SDK et modèles (`Cosmos.Sdk`, `Cosmos.Build.Templates`)
5. **Restaure** la solution principale (`nativeaot-patcher.slnx`).
6. **Installe les outils .NET globaux** : `ilc`, `Cosmos.Patcher` et `Cosmos.Tools`.

## Windows (`postCreateCommand.ps1`)

Lancez le script PowerShell depuis la racine du dépôt :

```powershell
.\.devcontainer\postCreateCommand.ps1
```

Le script Windows effectue les mêmes étapes que le script Linux, avec leurs équivalents PowerShell :

1. **Vide le cache NuGet** sous `%USERPROFILE%\.nuget\packages\cosmos.*`.
2. **Supprime tous les artefacts de build** et les dossiers `obj` sous `src/`.
3. **Enregistre la source NuGet locale** dans `artifacts/package/release`.
4. **Compile et empaquette chaque package Cosmos** dans le même ordre de dépendances que sous Linux.
5. **Restaure** la solution principale.
6. **Installe les outils .NET globaux** : `ilc`, `Cosmos.Patcher` et `Cosmos.Tools`.

<a id="verifying-the-setup"></a>

## Vérifier la configuration

Une fois le script terminé, vérifiez que les packages ont bien été créés :

```bash
ls artifacts/package/release/*.nupkg
```

Puis vérifiez que les outils globaux sont disponibles :

```bash
ilc --version
cosmos.patcher --version
cosmos --version
```

Si une étape échoue, relancez le script : il est conçu pour être idempotent et nettoie l'état précédent avant de recompiler.
