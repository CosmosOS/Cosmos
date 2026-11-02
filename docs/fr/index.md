---
_layout: landing
source_hash: 7277f6b5f33ef20c90622f8945755ecf24727d45
---

Bienvenue sur le wiki de Cosmos gen3 !

Consultez la [Feuille de route](roadmap.md) pour suivre notre progression vers la première version 🚀.

La documentation se divise en deux parties, selon ce que vous voulez faire avec gen3 :

- **[Guide utilisateur](articles/user/install.md)** : vous voulez **créer votre propre OS** avec Cosmos : installer la chaîne d'outils, utiliser le système de fichiers, déboguer votre noyau.
- **[Documentation contributeur](articles/dev/install-dev.md)** : vous voulez **contribuer à Cosmos lui-même** ou comprendre son fonctionnement interne : architecture, pipeline de build, sous-systèmes du runtime.

<a id="user-guide"></a>

## Guide utilisateur

Tout ce qu'il vous faut pour créer, compiler et exécuter votre propre noyau Cosmos :

 - [Guide d'installation](articles/user/install.md) : installer la chaîne d'outils et créer votre premier noyau depuis VS Code.
 - [Démarrage du noyau](articles/user/startup.md) : la chaîne de démarrage et le cycle de vie `BeforeRun`/`Run`/`AfterRun`.
 - [Système de fichiers](articles/user/filesystem.md) : monter un disque et utiliser l'API .NET standard `System.IO` (`File`, `Directory`, flux).
 - [Réseau](articles/user/network.md) : DHCP, UDP et TCP via l'API .NET standard `System.Net.Sockets`, ainsi que DNS.
 - [Minuteurs et alarmes](articles/user/timers.md) : bloquer pendant un moment, ou exécuter un callback plus tard depuis l'interruption du minuteur (`TimerManager`) ou depuis un thread du noyau (`AlarmManager`).
 - [Graphismes](articles/user/graphics.md) : dessiner des formes, du texte et des images à l'écran avec l'API Canvas.
 - [Audio](articles/user/audio.md) : jouer des tonalités, des fichiers WAV et des sons que vous générez, via la sortie HD Audio, avec `AudioManager` et `Console.Beep`.
 - [Clavier](articles/user/keyboard.md) : lire des lignes, des événements de touche et des dispositions de clavier via `Console` et `KeyboardManager`.
 - [Sessions de console et Telnet](articles/user/sessions.md) : exécuter plusieurs shells côte à côte sur des consoles virtuelles, et les servir à des terminaux distants via Telnet.
 - [Souris](articles/user/mouse.md) : position du pointeur, boutons et molette de défilement via `MouseManager`.
 - [Écrire un pilote](articles/user/drivers.md) : faire correspondre un pilote à un périphérique, accéder au matériel, remettre un périphérique au noyau, et le tester sans matériel.
 - [Écrire un ordonnanceur](articles/user/scheduler-plugging.md) : comment implémenter et installer une politique d'ordonnancement.
 - [Déboguer avec VSCode et QEMU](articles/user/debugging.md) : poser des points d'arrêt dans votre noyau avec GDB à distance.

<a id="contributor-docs"></a>

## Documentation contributeur

Architecture et fonctionnement interne, pour les contributeurs et les curieux :

 - [Configuration du dev container](articles/dev/install-dev.md) : compiler le framework depuis les sources.
 - [Organisation d'un projet de noyau](articles/dev/kernel-project-layout.md) : le graphe de projets en couches.
 - [Règles de codage](articles/dev/coding-guidelines.md) : style et patrons d'architecture.
 - [Plugs](articles/dev/plugs.md) : le système de remplacement de méthodes au niveau IL.
 - [Tests](articles/dev/testing.md) : tests unitaires et suites de tests de noyau sous QEMU.
 - [Suivi de l'API publique](articles/dev/public-api.md) : fichiers de surface déclarée, validation des packages, documentation versionnée.
 - [Ramasse-miettes](articles/dev/garbage-collector.md) : le GC à marquage-balayage.
 - [Ramasse-miettes - Parcours précis de la pile](articles/dev/garbage-collector-gcinfo.md) : comment GCInfo rend exact le parcours de la pile du thread qui déclenche la collecte.
 - [Ramasse-miettes - Glossaire](articles/dev/garbage-collector-glossary.md) : notes de contexte sur les concepts du GC sur lesquels s'appuient les articles.
 - [Ordonnanceur](articles/dev/scheduler.md) : l'ordonnanceur préemptif et interchangeable.
 - [Ordonnanceur - Glossaire](articles/dev/scheduler-glossary.md) : notes de contexte sur les concepts d'ordonnancement sur lesquels s'appuie l'article.
 - [Étapes de compilation du noyau](articles/dev/build/kernel-compilation-steps.md) : du C# à l'ISO amorçable, de bout en bout.
 - [Cosmos.Build.Asm](articles/dev/build/asm-build.md), [Cosmos.Build.GCC](articles/dev/build/gcc-build.md), [Cosmos.Build.Patcher](articles/dev/build/patcher-build.md), [Cosmos.Build.Ilc](articles/dev/build/ilc-build.md) : les composants du pipeline de build.
 - [Manifeste des pilotes](articles/dev/build/driver-manifest.md) : quelles classes `[Driver]` le build enregistre, dans quel ordre, et comment un noyau en écarte une ou en active une explicitement.

<a id="cross-referencing"></a>

## Références croisées

La référence de l'API publie une carte de références croisées DocFX à l'adresse [`/xrefmap.yml`](https://cosmosos.github.io/xrefmap.yml), qui couvre chaque espace de noms, type et membre documenté. Un autre site DocFX la consomme en ajoutant cette URL à la liste `xref` de son propre `docfx.json` :

```json
{
  "build": {
    "xref": [ "https://cosmosos.github.io/xrefmap.yml" ]
  }
}
```

Les types Cosmos sont alors liés par UID plutôt que par URL, sous la forme `<xref:Cosmos.Kernel.System.Graphics.Canvas>` ou de son raccourci `@Cosmos.Kernel.System.Graphics.Canvas`. Chaque copie figée d'une version publiée porte sa propre carte à côté d'elle, dans `/vX.Y.Z/xrefmap.yml` et `/latest/xrefmap.yml`.

<a id="resources"></a>

## Ressources
- [Cosmos Gen3 : l'ère NativeAOT et la fin d'IL2CPU ?](https://valentin.bzh/posts/3)
- [Workflow de développement NativeAOT](https://github.com/dotnet/runtime/blob/main/docs/workflow/building/coreclr/nativeaot.md)
- [Limitations de Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#limitations-of-native-aot-deployment)
