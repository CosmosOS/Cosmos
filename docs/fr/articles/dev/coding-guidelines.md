---
source_hash: 23e46dacd2fdf78d032c70cf67a36a77947285ca
---
<a id="coding-guidelines"></a>

# Règles de codage

Ce document établit le style de code et les modèles d'architecture de Cosmos Gen3. Ces règles garantissent une base de code propre, moderne et cohérente entre tous les contributeurs.

<a id="table-of-contents"></a>

## Table des matières

- [1. Architecture du projet](#1-project-architecture)
- [2. Conventions de nommage](#2-naming-conventions)
- [3. Organisation des fichiers](#3-file-organization)
- [4. Conception des classes et des types](#4-class--type-design)
- [5. Cycle de vie du noyau](#5-kernel-lifecycle)
- [6. Architecture de la HAL](#6-hal-architecture)
- [7. Système de plugs](#7-plug-system)
- [8. Interopérabilité native](#8-native-interop)
- [9. Code spécifique à une architecture](#9-architecture-specific-code)
- [10. Mémoire et sûreté](#10-memory--safety)
- [11. Gestion des erreurs](#11-error-handling)
- [12. Types référence nullables](#12-nullable-reference-types)
- [13. Idiomes modernes de C# / .NET 10](#13-modern-c--net-10-idioms)
- [14. Contraintes AOT](#14-aot-constraints)
- [15. Documentation](#15-documentation)
- [16. Tests](#16-testing)
- [17. Surface d'API publique](#17-public-api-surface)

---

<a id="1-project-architecture"></a>

## 1. Architecture du projet

<a id="layer-dependency-rules"></a>

### Règles de dépendance entre couches

Le projet est découpé en couches strictes. Les dépendances vont **uniquement vers le bas**. Ces règles sont **appliquées à la compilation** par l'analyseur Roslyn `LayerAnalyzer` de `Cosmos.Build.Analyzer.Patcher`, qui juge un projet sur les types et les membres que son code nomme, et non sur la liste de références que construit la restauration, et qui signale une seule fois chaque assembly utilisé par-delà une frontière, à sa première utilisation. Un noyau utilisateur, ainsi qu'un assembly de pilotes (`<CosmosDriverAssembly>true</CosmosDriverAssembly>`, voir [Suivi de l'API publique](public-api.md)), peuvent aussi nommer ce qu'offre `Cosmos.Kernel.HAL` : le point d'extension du kit de pilotes et les contrats de périphériques (`IBlockDevice`, `MacAddress`).

```
User Kernel (DevKernel, test kernels)
Cosmos.Kernel.Drivers (the shipped drivers, a driver assembly held to the User layer)
    └── Cosmos.Kernel.System        ← high-level OS APIs (Console, Graphics, Network)
         └── Cosmos.Kernel.HAL      ← hardware abstraction (shared logic, boot and device contracts)
              ├── Cosmos.Kernel.HAL.X64        ← x64 platform code (machine description, line routing, PIT, RTC)
              ├── Cosmos.Kernel.HAL.ARM64      ← ARM64 platform code (machine description, line routing, generic timer, RTC)
              └── Cosmos.Kernel.Core   ← low-level runtime (memory, scheduler, serial)
                   ├── Cosmos.Kernel.Native.X64       ← x64 assembly (.s)
                   ├── Cosmos.Kernel.Native.ARM64     ← ARM64 assembly (.s)
                   └── Cosmos.Kernel.Native.MultiArch ← cross-platform native C code (ACPI, libc stubs)
```

Pour le graphe de dépendances complet, la description des projets et les règles, voir [Organisation d'un projet de noyau](kernel-project-layout.md).

<a id="when-to-create-a-new-project"></a>

### Quand créer un nouveau projet

- Un nouveau matériel que les bus du kit atteignent (une fonction PCI, un périphérique virtio, une interface USB, un port PS/2, un nœud de plateforme) → une classe `[Driver]` dans `Cosmos.Kernel.Drivers`, écrite sur le point d'extension public ([Écrire un pilote](../user/drivers.md)) ; l'agrégateur embarque le paquet, si bien que chaque noyau le reçoit. Un nouveau type de bus → une identité, une correspondance, un objet d'accès et un chemin dans le kit. Le code HAL multiplateforme va dans `Cosmos.Kernel.HAL`, le code de plateforme dans `Cosmos.Kernel.HAL.X64`/`Cosmos.Kernel.HAL.ARM64`.
- Nouvelle fonctionnalité de niveau OS, API utilisateur exposée → dans `Cosmos.Kernel.System`.
- Nouvelle préoccupation du runtime bas niveau → dans `Cosmos.Kernel.Core`.

---

<a id="2-naming-conventions"></a>

## 2. Conventions de nommage

Certaines règles de nommage essentielles sont appliquées par `.editorconfig` ; le tableau ci-dessous documente l'ensemble des règles de nommage :

| Élément | Convention | Exemple |
|---------|-----------|---------|
| Espace de noms | `Cosmos.Kernel.{Layer}.{Category}` (le projet, puis ses dossiers) | `Cosmos.Kernel.Core.Scheduler` |
| Classe/struct publique | PascalCase | `GarbageCollector`, `Thread` |
| Interface | `I` + PascalCase | `IScheduler`, `IPlatformInitializer` |
| Méthode/propriété publique | PascalCase | `InitializeStack()`, `StackPointer` |
| Champ privé/interne | `_camelCase` | `_currentScheduler`, `_kernel` |
| Champ statique privé/interne | `s_camelCase` | `s_cursorPattern`, `s_nextThreadId` |
| Constante | PascalCase | `DefaultStackSize`, `MaxThreadCount` |
| Constante privée | PascalCase | `COM1_BASE` (les registres matériels utilisent UPPER_SNAKE pour la lisibilité) |
| Variable locale | camelCase | `stackTop`, `entryPoint` |
| Paramètre | camelCase | `cpuState`, `threadId` |
| Paramètre de type | `T` + PascalCase | `TValue`, `TKey` |
| Membre d'énumération | PascalCase | `ThreadState.Running` |

Dans `Cosmos.Kernel.System.Diagnostics`, un suffixe indique ce qu'est un type : un type `*Diagnostics` est la vue de diagnostic statique d'un sous-système (`DriverDiagnostics`, `SchedulerDiagnostics`, `MemoryDiagnostics`), qui sert surtout à lire mais peut aussi agir (`SchedulerDiagnostics.RequestKill`, `MemoryDiagnostics.Collect`), et un type `*Info` est une struct d'instantané en lecture seule que remet une telle vue (`DriverInfo`, `DeviceNodeInfo`, `KernelThreadInfo`). Voir [Suivi de l'API publique](public-api.md#naming).

<a id="avoid"></a>

### À éviter

- La notation hongroise (`m_`, `p_`, `g_`) : utilisez le préfixe `_` pour les champs privés uniquement.
- L'ancien style `mStopped` : utilisez `_stopped`.
- Les abréviations, sauf si elles sont universellement comprises (`GC`, `CPU`, `HAL`, `IO`, `IP`, `MAC`).

---

<a id="3-file-organization"></a>

## 3. Organisation des fichiers

<a id="one-type-per-file"></a>

### Un type par fichier

Chaque type public a son propre fichier, nommé d'après le type :

```
Cosmos.Kernel.Core/
  Scheduler/
    SchedulerThread.cs
    SchedulerThreadState.cs
    IScheduler.cs
    SchedulerManager.cs
    PerCpuState.cs
```

Deux sortes de compagnons partagent le fichier du type principal, qui garde le nom de ce dernier : le compagnon étroitement couplé propre à un type, comme `GptPartitionEntry` pour `Gpt`, ou `TcpFlags` et `TcpOption` pour `TcpPacket`, et une famille de sous-classes, comme `IcmpEchoRequest` et `IcmpEchoReply` pour `IcmpPacket`. Une implémentation n'est pas un compagnon de son interface : 24 fichiers déclarent une interface de premier niveau, et chacun d'eux déclare cette interface et rien d'autre.

<a id="partial-classes-for-large-types"></a>

### Classes partielles pour les gros types

Répartissez les grosses classes sur plusieurs fichiers avec `partial`. Nommez les fichiers `{ClassName}.{Aspect}.cs` :

```
Memory/
  GarbageCollector.cs              ← core structure, fields, initialization
  GarbageCollector.Alloc.cs        ← allocation methods
  GarbageCollector.Mark.cs         ← mark phase
  GarbageCollector.Sweep.cs        ← sweep phase
  GarbageCollector.GCHandler.cs    ← GC handle table
  GarbageCollector.Frozen.cs       ← frozen segment support
```

<a id="architecture-specific-files"></a>

### Fichiers spécifiques à une architecture

Pour les types qui ont besoin d'une implémentation par architecture au sein du même projet, utilisez un suffixe :

```
Scheduler/
  ThreadContext.X64.cs             ← x64-specific context layout
  ThreadContext.ARM64.cs           ← ARM64-specific context layout
```

Ils sont compilés de manière conditionnelle via le `.csproj` :

```xml
<ItemGroup Condition="'$(CosmosArch)' != 'arm64'">
    <Compile Remove="Scheduler\ThreadContext.ARM64.cs" />
</ItemGroup>
```

Ce n'est autorisé pour l'instant que dans `Cosmos.Kernel.Core`, mais cela pourrait changer à l'avenir.

<a id="namespaces-follow-folders"></a>

### Les espaces de noms suivent les dossiers

L'espace de noms d'un fichier est le nom de son projet suivi des dossiers situés entre le projet et le fichier : `Cosmos.Kernel.Core/Scheduler/SchedulerThread.cs` déclare `Cosmos.Kernel.Core.Scheduler`. Le build l'impose avec IDE0130 au niveau erreur pour chaque projet `src/Cosmos.Kernel*`, pour les noyaux de test, pour les tests unitaires côté hôte de `tests/Cosmos.Kernel.Tests.System` et pour `examples/DevKernel`, si bien que `dotnet build` échoue sur un fichier qui s'en écarte (la CI de formatage vérifie les projets de `nativeaot-patcher.slnx`, qui laisse de côté les noyaux de test). Déplacez le fichier ou corrigez l'espace de noms plutôt que d'ajouter une exemption. Les exemptions, chacune avec sa raison, terminent `.editorconfig` : les noms qu'ILC reconnaît par leur nom complet (chaque `LibraryInitializer`, `EagerStaticClassConstructionAttribute`, `Core/Runtime/Stdllib.cs`), le `IsExternalInit` des générateurs de source (le marqueur d'accesseur init du compilateur C#, qui doit se trouver dans `System.Runtime.CompilerServices`), les dossiers `Bridge/Import` et `Bridge/Export`, dont les fichiers partagent l'espace de noms `Bridge` du projet, le code tiers embarqué sous `Cosmos.Kernel.System/ThirdParty/` et les copies de dotnet/runtime sous `Cosmos.Kernel.Core/Runtime/Internal/`. IDE0130 ne vérifie ni un espace de noms qui contient un type partiel réparti sur plusieurs fichiers (chaque classe `[LibraryImport]`) ni un espace de noms imbriqué ; ceux-là suivent la règle sans vérification.

<a id="driver-folders"></a>

### Dossiers des pilotes

`Cosmos.Kernel.Drivers` range chaque pilote à trois niveaux de profondeur, le type de bus, puis la catégorie, puis le pilote, et l'espace de noms de chaque fichier suit son dossier :

```
Cosmos.Kernel.Drivers/
  Pci/                       ← the bus kind the driver matches on (PciMatch)
    Bus/                     ← a bus driver: its binding publishes child nodes
      Xhci/
        XhciDriver.cs        ← Cosmos.Kernel.Drivers.Pci.Bus.Xhci
    Storage/                 ← otherwise, what it publishes (a block device)
      Nvme/
        NvmeDriver.cs
  Virtio/
    Display/
      VirtioGpu/
        VirtioGpuDriver.cs   ← Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu
```

Le type de bus est celui sur lequel le pilote établit sa correspondance, nommé d'après son type de correspondance : `Pci`, `Platform`, `Ps2`, `Usb` ou `Virtio`. La catégorie est `Bus` pour un pilote de bus et, sinon, nomme ce que le pilote publie : `Audio`, `Display`, `Input` (un clavier ou un pointeur), `Network` ou `Storage` (un périphérique bloc). Le dossier du pilote est le nom de la classe sans `Driver`, sauf si un type du dossier porte déjà ce nom, car un espace de noms ne doit pas partager son nom avec l'un de ses types : les pilotes de transport se trouvent dans `VirtioPci` et `VirtioMmio`, à côté de leurs classes `VirtioPciTransport` et `VirtioMmioTransport`. Les types d'état, de protocole et de vocabulaire d'un pilote restent dans son dossier, dans des sous-dossiers quand ils sont nombreux (`VmwareSvga/Enums`, `VmwareSvga/Structs`). L'espace de noms fait partie du nom de type complet que désigne `CosmosDriverExclude` et selon lequel le manifeste trie ([Manifeste des pilotes](build/driver-manifest.md)), si bien que déplacer un pilote change les deux.

<a id="hal-folders"></a>

### Dossiers de la HAL

`Cosmos.Kernel.HAL` donne à chaque concept un dossier de premier niveau, et aucun nom de dossier n'apparaît deux fois dans l'arborescence :

```
Cosmos.Kernel.HAL/
  Experimentals.cs           ← the seam's diagnostic ID (COSMOS0003)
  Boot/                      ← internal: IPlatformInitializer, PlatformHAL
  Timers/                    ← internal: TickSource, TimerEntry
  Internal/                  ← the LibraryInitializer ILC finds by full name
  Devices/                   ← what a device is, a folder per category
    Audio/                   ← IAudioOutput, AudioFormat, AudioSink, AudioConsumer
    Display/                 ← IDisplay, its facets, DisplayMode, DisplaySink, DisplayConsumer, FirmwareDisplay
    Input/
    Network/                 ← INetworkInterface, NetworkSink, NetworkConsumer, MacAddress
    Storage/                 ← IBlockDevice, BlockConsumer
  DriverKit/                 ← how a driver finds and binds a device
    Driver.cs                ← the driver and its binding: matching, probing, the node tree
    DeviceBinding.cs
    Resources/               ← what a binding hands out: RegisterWindow, DeviceRegion, DmaBuffer
    Interrupts/              ← InterruptSource, InterruptHandle, InterruptHandler, InterruptContext
    Threading/               ← DeviceLock, DeviceEvent, DriverThread, WorkItem
    Buses/
      Pci/                   ← PciIdentity, PciMatch, PciAccess, PciLineInterruptSource, …
      Usb/
    Engine/                  ← internal: DeviceRegistry, DriverEngine, Arbitration, DriverLog
```

Une catégorie de périphériques sous `Devices/` contient tout ce qui concerne ce type de périphérique : le contrat qu'un pilote implémente, ses facettes et son vocabulaire, le récepteur par lequel le pilote rend compte (les périphériques bloc n'en ont pas) et le consommateur interne de l'anneau supporté, qui apprend l'arrivée et le départ de chaque périphérique et reçoit ses comptes rendus. Les catégories sont celles sous lesquelles `Cosmos.Kernel.Drivers` range ses pilotes ([Dossiers des pilotes](#driver-folders)), moins `Bus`. `Display/` contient aussi `FirmwareDisplay`, l'écran que le moteur publie sur le framebuffer de démarrage qu'a lu le `BootFirmware` du Core. La HAL n'a pas de dossier pour le firmware : lire ce que transmettent le chargeur d'amorçage et le firmware demande des pointeurs, cela revient donc au Core ([Code unsafe](#unsafe-code)).

La racine `DriverKit/` contient le pilote et sa liaison : `Driver` et son attribut, le registre, `DeviceBinding`, l'arbre des nœuds avec ses classes de base d'identité et de correspondance, et le vocabulaire du sondage et du détachement. Ce qu'une liaison remet à un pilote se trouve un dossier plus bas, rangé par préoccupation : `Resources/` pour les moyens d'atteindre le matériel (ressources, fenêtres de registres, régions mappées, tampons DMA), `Interrupts/` pour les sources et le contrat du gestionnaire d'interruption, et `Threading/` pour les verrous, les événements, les threads et les éléments de travail avec lesquels un pilote se synchronise. Un pilote nomme la racine, puis les dossiers qu'il utilise.

Un type de bus sous `DriverKit/Buses/` donne à chaque rôle le même nom dans chaque dossier : `<Bus>Identity`, `<Bus>Match`, `<Bus>Access` quand un pilote atteint le périphérique à travers le bus (le bus de plateforme n'en a pas : un pilote de plateforme mappe les ressources de son nœud, et seul le nœud hôte PCI porte un objet d'accès, le `PciHostAccess` du dossier PCI), `<Bus>InterruptSource` quand le bus délivre ses propres interruptions (sur PCI et sur le bus de plateforme, le nom dit aussi comment : `PciLineInterruptSource`, `PciMessageInterruptSource`, `PlatformLineInterruptSource`), et le vocabulaire propre au bus à côté d'eux. La classe de base que l'hôte d'un bus implémente pour le kit garde le nom qu'utilise sa spécification (`Ps2Controller`, `UsbHostController`, `VirtioTransport`) plutôt qu'un suffixe commun à tous les bus ; celle du bus de plateforme est `PlatformLineRouting`, que la description de la machine implémente.

La stabilité est marquée sur chaque type, et non par dossier. Chaque type public de la HAL porte `[Experimental(Experimentals.DriverKitSeamDiagId)]`, sauf les deux contrats stables, `IBlockDevice` et `MacAddress`, et la cible `CosmosGuardHalPromotion` du projet HAL fait échouer le build dès qu'un autre type public perd l'attribut ([Suivi de l'API publique](public-api.md#the-policy)).

<a id="member-order"></a>

### Ordre des membres

Il n'y a pas de convention de séparateurs à suivre. Quatre fichiers du ramasse-miettes portent des en-têtes `// --- Section Name ---`, écrits trois semaines avant cette page, et les fichiers des pilotes d'affichage de `Cosmos.Kernel.Drivers` ont ensuite été écrits pour s'y conformer. Les 353 autres fichiers des quatre assemblies suivis, hors code tiers embarqué, ne le font pas, aucun fichier n'a jamais été converti et aucun analyseur ne le vérifie. Il n'y a pas non plus d'ordre des membres à décrire : sur 131 types qui ont trois sortes de membres ou plus, on compte 93 ordres distincts, et l'ordre unique qui s'en approche le mieux en couvre 43.

Deux ordres valent tout de même d'être suivis, parce qu'un lecteur les vérifie sans quitter l'écran :

- Les champs et les constantes viennent avant les membres qui les lisent, et non après la première méthode.
- Les constructeurs viennent après les champs et les propriétés et avant les méthodes, et une fabrique statique se place avec les constructeurs dont elle tient lieu.

Si vous voulez un modèle de la forme complète avec séparateurs pour un nouveau fichier, `Cosmos.Kernel.Drivers/Virtio/Display/VirtioGpu/VirtioGpuDriver.cs` est le fichier qui la montre. Ne convertissez pas un fichier existant à cette forme.

<a id="using-directives"></a>

### Directives using

- Placez les directives `using` **en dehors** de l'espace de noms.
- Triez les espaces de noms `System` en premier.
- Utilisez des espaces de noms de portée fichier (par ex. `namespace Cosmos.Kernel.Core.Scheduler;`). Les arborescences de code tiers embarqué sous `ThirdParty/` (BigGustave, SharpZipLib, LunarFonts) et les copies de dotnet/runtime gardent la forme en bloc afin de rester comparables par diff avec l'amont, et `Core/Runtime/Stdllib.cs` ne peut pas du tout prendre la forme de portée fichier : il déclare quatre espaces de noms, dont un imbriqué, ce qui provoque l'erreur CS8955.

---

<a id="4-class--type-design"></a>

## 4. Conception des classes et des types

<a id="static-utility-classes"></a>

### Classes utilitaires statiques

Utilisez une `static class` pour les utilitaires du noyau sans état, qui n'ont aucun état par instance.

<a id="managers"></a>

### Gestionnaires

Utilisez une `static class`. Les gestionnaires coordonnent l'état d'un sous-système sans exiger d'instance. La mise en place sur le chemin de démarrage est une méthode `internal static void Initialize()` appelée depuis `LibraryInitializer`, jamais depuis un noyau :

```csharp
// Simple manager: no underlying instance to expose
public static class TimerManager
{
    private static TickSource? s_tickSource;

    public static bool IsInitialized => s_tickSource is not null;

    internal static void RegisterTickSource(TickSource tickSource) { ... }
    public static void Wait(uint ms) { ... }
}

// Manager wrapping a pluggable implementation: expose via Current
public static class SchedulerManager
{
    private static IScheduler? s_currentScheduler;

    public static IScheduler? Current => s_currentScheduler;

    internal static void Initialize(uint cpuCount) { ... }
}
```

N'ajoutez une propriété `Current` que lorsque le gestionnaire enveloppe une implémentation interchangeable (par ex. `IScheduler` pour plusieurs algorithmes d'ordonnancement). La plupart des gestionnaires n'en ont pas besoin.

Ne gardez pas d'indicateur `_initialized` séparé à côté de l'état qu'il représente. Dérivez `IsInitialized` de cet état, et faites en sorte qu'`Initialize` se protège de la réentrance sur ce même champ, en l'assignant en dernier pour qu'aucun lecteur ne voie `IsInitialized` passer à vrai avant que l'état qui le sous-tend n'existe.

Quand un gestionnaire se trouve derrière un commutateur de fonctionnalité, ses membres suivent la règle de [Suivi de l'API publique](public-api.md#behaviour-when-a-feature-is-compiled-out) : les lectures répondent honnêtement quand la fonctionnalité est désactivée, les actions lèvent une exception qui nomme le commutateur.

<a id="structs-for-low-level-data"></a>

### Structs pour les données bas niveau

Utilisez `struct` avec `[StructLayout(LayoutKind.Sequential)]` pour les données qui correspondent à une disposition matérielle ou mémoire :

```csharp
[StructLayout(LayoutKind.Sequential)]
internal struct FreeBlock
{
    public unsafe MethodTable* MethodTable;
    public nuint Size;
    public unsafe FreeBlock* Next;
}
```

<a id="enums"></a>

### Énumérations

Utilisez `enum` pour les machines à états et les indicateurs. Appliquez `[Flags]` lorsque c'est approprié :

```csharp
public enum ThreadState
{
    Ready,
    Running,
    Blocked,
    Sleeping,
    Dead
}

[Flags]
public enum ThreadFlags
{
    None = 0,
    IsIdle = 1 << 0,
    IsKernel = 1 << 1
}
```

<a id="types-compared-by-value"></a>

### Types comparés par valeur

Un type dont l'identité est son contenu (`Address`, `MacAddress`, `Mode`) est `sealed`, garde son état dans des membres en lecture seule définis par le constructeur, et implémente `IEquatable<T>` à côté de `Equals(object?)` et `GetHashCode()`. Les trois concordent : deux instances égales au sens de `Equals` ont le même hachage, et `GetHashCode` ne lève jamais d'exception. L'ordre, quand il existe, passe par `IComparable<T>`, jamais par l'`IComparable` non générique. Un appelant qui a besoin des octets reçoit un `ReadOnlySpan<byte>` ; le tableau sous-jacent n'est jamais remis, parce qu'un appelant qui peut y écrire change l'identité derrière chaque dictionnaire dont la valeur est la clé.

```csharp
public sealed class Address : IComparable<Address>, IEquatable<Address>
{
    public ImmutableArray<byte> Parts { get; }

    public Address(ReadOnlySpan<byte> buffer) { ... }

    public ReadOnlySpan<byte> ToSpan()
    {
        return Parts.AsSpan();
    }

    public bool Equals(Address? other)
    {
        return other is not null && Parts.SequenceEqual(other.Parts);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Address);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public int CompareTo(Address? other)
    {
        return other is null ? 1 : Id.CompareTo(other.Id);
    }
}
```

<a id="registered-instances"></a>

### Instances enregistrées

Quand le sous-système doit voir chaque instance vivante (`TcpConnection.Connections`), le constructeur est privé et une fabrique statique crée et enregistre l'instance (`TcpConnection.CreateConnection`). Le registre reste privé, et le retrait libère l'instance afin que ce qu'elle a emprunté retourne à son pool.

---

<a id="5-kernel-lifecycle"></a>

## 5. Cycle de vie du noyau

<a id="user-kernel-pattern"></a>

### Modèle du noyau utilisateur

Les noyaux utilisateur héritent de `Cosmos.Kernel.System.Kernel` et redéfinissent les méthodes du cycle de vie :

```csharp
using Cosmos.Kernel.System;

namespace MyKernel;

public class Kernel : Cosmos.Kernel.System.Kernel
{
    protected override void BeforeRun()
    {
        // One-time setup (after boot, before main loop)
        Console.WriteLine("Booted!");
    }

    protected override void Run()
    {
        // Called in a loop until Stop() is called
        string? input = Console.ReadLine();
        ProcessCommand(input);
    }

    protected override void AfterRun()
    {
        // Cleanup (after main loop exits)
    }
}
```

**Règles :**
- Redéfinissez `OnBoot()` uniquement pour personnaliser le démarrage (l'implémentation par défaut démarre `KernelConsole`).
- Redéfinissez `BeforeRun()` pour la mise en place unique, une fois le système prêt.
- `Run()` est le corps de la boucle principale : gardez-le ciblé.
- Appelez `Stop()` pour sortir proprement de la boucle principale.
- Il faut éviter de revenir du noyau : la classe de base arrête le CPU après `AfterRun()`.

---

<a id="6-hal-architecture"></a>

## 6. Architecture de la HAL


<a id="interface-driven-hal"></a>

### HAL pilotée par des interfaces

Les services de plateforme passent par des interfaces. Les implémentations sont enregistrées au démarrage :

```csharp
// Interface (Cosmos.Kernel.Core, internal: no kernel registers or obtains one)
internal interface ICpuOps
{
    void Halt();
    void DisableInterrupts();
    void EnableInterrupts();
}

// Implementation (Cosmos.Kernel.HAL.X64)
public class X64CpuOps : ICpuOps
{
    public void Halt()
    {
        Native.Cpu.Halt();
    }

    // ...
}
```

<a id="platform-initializer-pattern"></a>

### Modèle de l'initialiseur de plateforme

Chaque architecture fournit une fabrique qui crée tous les composants propres à la plateforme. La fabrique, le contrat et tout ce qu'elle renvoie sont internes à la HAL : un noyau n'en installe jamais. L'initialiseur est aussi la description de la machine : il publie les nœuds de plateforme racines auxquels un pilote de bus se lie, à partir des sources que connaît la plateforme : les tables ACPI, le device tree que le chargeur d'amorçage a transmis, ou une table propre à chaque machine ; et il ne contient aucun pilote de périphérique.

```csharp
internal class X64PlatformInitializer : IPlatformInitializer
{
    public string PlatformName => "x86-64";
    public PlatformArchitecture Architecture => PlatformArchitecture.X64;

    public IPortIO CreatePortIO()
    {
        return new X64PortIO();
    }

    public ICpuOps CreateCpuOps()
    {
        return new X64CpuOps();
    }

    public IInterruptController CreateInterruptController()
    {
        return new X64InterruptController();
    }

    public TickSource CreateTickSource()
    {
        return new PIT();
    }

    public void PublishPlatformNodes()
    {
        // the 8042 and the PCI host, with their port ranges and lines
    }

    public uint GetCpuCount()
    {
        return /* ACPI/MADT */ 1;
    }

    public void InitializeHardware()
    {
        // ACPI, APIC, timers
    }

    public void StartSchedulerTimer(uint quantumMs)
    {
        // Configure PIT/APIC timer for preemptive scheduling
    }
}
```

<a id="adding-a-new-device"></a>

### Ajouter un nouveau périphérique

Écrivez une classe `[Driver]` dans `Cosmos.Kernel.Drivers` sur le kit et publiez le périphérique par sa liaison ([Écrire un pilote](../user/drivers.md)). Quand le périphérique se trouve sur un bus que le kit ne connaît pas :

1. Ajoutez le type de bus au kit, dans son propre dossier sous `DriverKit/Buses/` : une identité, une correspondance, un objet d'accès quand un pilote atteint le périphérique à travers le bus, une source d'interruptions quand le bus délivre ses propres interruptions, et un format de chemin ([Dossiers de la HAL](#hal-folders)).
2. Publiez ses nœuds depuis la description de la machine (`PublishPlatformNodes`) ou depuis un pilote de bus (`PublishChild`).
3. Écrivez le pilote feuille sur l'objet d'accès.

Rien d'autre que du code de plateforme ne va dans `Cosmos.Kernel.HAL.X64` ou `Cosmos.Kernel.HAL.ARM64`. Le parseur du device tree est du code Core, sous `Cosmos.Kernel.Core/Firmware`, à côté des autres lecteurs de ce que transmet le firmware ; la description ARM64 ne fait que le lire.

<a id="hal-registration"></a>

### Enregistrement de la HAL

```csharp
// At boot (from the library initializer, or from OnBoot):
PlatformHAL.Initialize(new X64PlatformInitializer());
```

---

<a id="7-plug-system"></a>

## 7. Système de plugs

Les plugs remplacent des méthodes de la BCL au niveau IL. Le patcher recâble les appels au moment du build. Pour la documentation complète sur les attributs de plug (`[Plug]`, `[PlugMember]`, `[PlatformSpecific]`) et le modèle de plug, voir [Plugs](plugs.md).

<a id="when-to-use-plugs-vs-other-approaches"></a>

### Quand utiliser les plugs plutôt que d'autres approches

| Scénario | Approche |
|----------|----------|
| Remplacer une méthode de la BCL | Plug |
| Ajouter une API au niveau du noyau | Nouvelle classe dans `Cosmos.Kernel.System` |
| Fournir un stub du runtime | `[RuntimeExport]` dans Core |
| Appeler du code natif | `[LibraryImport]` dans Core |

---

<a id="8-native-interop"></a>

## 8. Interopérabilité native

<a id="c-callable-from-native"></a>

### C# appelable depuis le code natif

Il existe deux modèles selon l'appelant :

**`[RuntimeExport]`** : pour les stubs du runtime NativeAOT (appelés par le runtime lui-même). Ils correspondent aux déclarations `[RuntimeImport]` de [`System.Private.CoreLib/RuntimeImports.cs`](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/System.Private.CoreLib/src/System/Runtime/RuntimeImports.cs) :

```csharp
[RuntimeExport("RhNewArray")]
internal static unsafe void* RhNewArray(MethodTable* pEEType, int length)
{
    return GarbageCollector.AllocArray(pEEType, length);
}
```

**`[UnmanagedCallersOnly]`** : pour les méthodes C# appelables depuis du code C natif (ponts) :

```csharp
[UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_write")]
public static void CosmosSerialWrite(byte* str)
{
    // C code calls this via the symbol __cosmos_serial_write
}
```

Utilisez `[RuntimeExport]` pour les exports qu'exigent le runtime ou l'ABI (par exemple les stubs `Rh*` et les symboles libc/math ou mémoire comme `ceil`, `sqrt`, `memmove`, `memset`). Préférez `[UnmanagedCallersOnly]` pour les autres callbacks du code natif vers le code managé (gestionnaires d'interruption, ponts vers des bibliothèques C, etc.).

<a id="native-callable-from-c"></a>

### Code natif appelable depuis C#

Le modèle P/Invoke moderne pour appeler des routines en assembleur :

```csharp
[LibraryImport("*", EntryPoint = "_native_cpu_rdtsc")]
[SuppressGCTransition]
private static partial ulong NativeReadTSC();
```

**Règles :**
- Utilisez `LibraryImport` (et non `DllImport`) : il est généré par source et compatible AOT.
- Utilisez `"*"` comme nom de bibliothèque (lien vers le binaire du noyau lui-même).
- Ajoutez `[SuppressGCTransition]` pour les appels sur des chemins critiques qui ne déclenchent pas de GC.
- Gardez les méthodes d'interopérabilité native `private` et enveloppez-les dans une API publique propre.
- Les noms des points d'entrée en assembleur utilisent `_snake_case` avec un préfixe de catégorie : `_native_cpu_rdtsc`, `_native_io_write_byte`.

---

<a id="9-architecture-specific-code"></a>

## 9. Code spécifique à une architecture

<a id="conditional-compilation"></a>

### Compilation conditionnelle

Utilisez `#if ARCH_X64` / `#if ARCH_ARM64` pour le code qui diffère selon l'architecture. Pour l'instant, ce n'est toléré que dans `Cosmos.Kernel.Core`, et c'est voué à disparaître.

```csharp
public static void ComWrite(byte value)
{
#if ARCH_ARM64
    while ((Native.MMIO.Read32(PL011_BASE + PL011_FR) & FR_TXFF) != 0) ;
    Native.MMIO.Write8(PL011_BASE + PL011_DR, value);
#else
    while ((Native.IO.Read8(COM1_BASE + REG_LSR) & LSR_TX_EMPTY) == 0) ;
    Native.IO.Write8(COM1_BASE + REG_DATA, value);
#endif
}
```

<a id="when-to-use-what"></a>

### Quand utiliser quoi

| Modèle | Quand |
|---------|------|
| `#if ARCH_X64` | Même classe, petites différences de code (par ex. le pilote série) |
| Fichiers séparés (`*.X64.cs`) | Même classe, gros blocs par architecture (par ex. ThreadContext) |
| Projets HAL séparés | Implémentations différentes derrière une interface commune |
| Projets Native séparés | Code assembleur (`.s`) |

<a id="guard-rules"></a>

### Règles de garde

- Le chemin `#else` par défaut doit être x64 (la cible de développement la plus courante).
- Ayez toujours les deux chemins : pas de `#if` isolé sans l'autre architecture.
- Demandez-vous si le code n'a pas plutôt sa place dans une implémentation de la HAL que dans un `#if`.

---

<a id="10-memory--safety"></a>

## 10. Mémoire et sûreté

<a id="unsafe-code"></a>

### Code unsafe

Le code unsafe se trouve dans `Cosmos.Kernel.Core` et ses projets d'architecture (`Cosmos.Kernel.Core.X64`, `Cosmos.Kernel.Core.ARM64`), ainsi que dans les éléments situés hors des couches (`Cosmos.Kernel.Boot.Limine`, `Cosmos.Kernel.Plugs`, `Cosmos.Kernel.Debug` et l'agrégateur `Cosmos.Kernel`). `Cosmos.Kernel.HAL`, ses projets de plateforme, `Cosmos.Kernel.System` et `Cosmos.Kernel.Drivers` sont compilés avec `<AllowUnsafeBlocks>false</AllowUnsafeBlocks>`, si bien que le compilateur y refuse tout pointeur. Ce dont ces couches ont besoin en mémoire, le Core le leur fournit par adresse :

- `MemoryBlock` (`Core/Memory/`, le type de gen2) est une vue sur la mémoire située à une adresse : `Span` et `AsSpan<T>` la lisent et l'écrivent. `PageAllocator.AllocBlock` fournit des pages sous forme de bloc et `PageAllocator.Free(MemoryBlock)` les reprend. Une ressource du kit (`DeviceRegion`, `DmaBuffer`) détient un bloc et refuse de donner son span une fois sa liaison démantelée.
- `AddressSpace.HhdmOffset` est le décalage de la moitié haute de Limine, lu à un seul endroit.
- `Core/Firmware/` lit ce que transmettent le chargeur d'amorçage et le firmware : le framebuffer, le device tree, l'heure de démarrage, l'entrée MCFG de l'ACPI et l'horloge EFI (`BootFirmware`, `DeviceTree`, `AcpiMcfg`, `EfiRtc`).

Une structure que le périphérique lit sur place, comme un anneau de virtqueue, est atteinte par `MemoryMarshal.Cast` et `MemoryMarshal.AsRef` sur un tel span, avec `Volatile.Read` pour un champ que le périphérique écrit. Quand une couche au-dessus du Core a besoin de ce que seul un pointeur donne, la solution est un point d'entrée dans le Core, pas un bloc `unsafe`. Au sein du Core, limitez `unsafe` aux endroits où il est nécessaire :

```csharp
// Good: unsafe scoped to where needed
public static unsafe void WriteString(string str)
{
    fixed (char* ptr = str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            ComWrite((byte)ptr[i]);
        }
    }
}

// Good: unsafe class for types that inherently work with pointers
public unsafe class Thread : SchedulerExtensible
{
    public ThreadContext* GetContext()
    {
        return (ThreadContext*)StackPointer;
    }
}
```

<a id="memory-allocation"></a>

### Allocation mémoire

```csharp
// Kernel heap allocation (manual, non-GC)
void* ptr = MemoryOp.Alloc(size);
MemoryOp.Free(ptr);

// GC-managed allocation (via RuntimeExport stubs)
// Happens automatically through normal C# object creation
List<int> list = new();  // uses RhAllocateNewArray under the hood
```

<a id="pointer-safety"></a>

### Sûreté des pointeurs

- Vérifiez toujours les pointeurs avant de les déréférencer dans le code du GC ou de parcours.
- Utilisez `nuint` pour les adresses (et non `uint`, ce qui évite la troncature en 64 bits).
- Utilisez `nint`/`nuint` pour l'arithmétique de pointeurs, et non `int`/`uint`.
- Utilisez `stackalloc` pour les petits tampons à courte durée de vie plutôt qu'une allocation sur le tas.

<a id="buffers"></a>

### Tampons

| Tampon | Utilisation |
|--------|-----|
| Petit, de taille fixe, limité à un appel | `stackalloc` |
| De taille variable, conservé d'un appel à l'autre (un tampon de réception) | `ArrayPool<T>.Shared.Rent`, rendu depuis `Dispose()` |
| Entrée en lecture seule | Un paramètre `ReadOnlySpan<T>`, pas `byte[]` |
| Copie | `source.CopyTo(destination)` sur des spans, pas `Buffer.BlockCopy` |

Un tableau emprunté est au moins aussi long que demandé, et pas exactement aussi long : le propriétaire suit donc la longueur et le décalage qu'il utilise et expose la partie vivante sous forme de `ReadOnlySpan<T>`, jamais le tableau. `TcpConnection` sert de modèle : `_data`, `_dataOffset` et `_dataLength` derrière `Data => _data.AsSpan().Slice(_dataOffset, _dataLength)`, avec `AppendToData` qui écrit sur place tant que le tableau emprunté a de la place et `Dispose` qui le rend.

<a id="critical-sections"></a>

### Sections critiques

```csharp
// Preferred: scoped interrupt disable
using (InternalCpu.DisableInterruptsScope())
{
    // Interrupts disabled here
    // Re-enabled automatically at scope exit
}
```

---

<a id="11-error-handling"></a>

## 11. Gestion des erreurs

<a id="kernel-panic"></a>

### Panique du noyau

Pour les erreurs irrécupérables, utilisez `Panic.Halt()`. Elle désactive les interruptions, affiche le message ainsi que la méthode, le fichier et la ligne de l'appelant, puis arrête le CPU :

```csharp
if (ptr == null)
    Panic.Halt("Memory allocation failed");
```

Les informations sur l'appelant sont remplies par le compilateur : passez donc le message et rien d'autre. `Panic.Halt` est `[DoesNotReturn]`, ce qui permet au code situé après une vérification de nullité de déréférencer la valeur que cette vérification vient d'écarter.

### Exceptions

Les exceptions fonctionnent, mais utilisez-les avec discernement :

```csharp
// Good: validate at API boundaries
public static void RescanPartitions(IBlockDevice device)
{
    ArgumentNullException.ThrowIfNull(device);
    // ...
}

// Good: feature guards
private static void ThrowIfKeyboardDisabled()
{
    if (!CosmosFeatures.KeyboardEnabled)
        throw new InvalidOperationException("Console input requires keyboard feature.");
}

// Bad: exceptions in interrupt handlers, GC, or scheduler hot paths
// These paths cannot allocate (exception objects are heap-allocated)
```

<a id="argument-checks"></a>

### Vérification des arguments

Quand la vérification tient en une seule comparaison, le helper de levée d'exception remplace le `if` et le `throw` écrits à la main. Le paramètre nommé est celui dont la valeur dérive, afin qu'un appelant qui lit l'exception trouve l'argument à corriger :

```csharp
ArgumentNullException.ThrowIfNull(device);
ArgumentOutOfRangeException.ThrowIfNegative(width);
ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, MaxPartitions);
ArgumentOutOfRangeException.ThrowIfNotEqual(buffer.Length, 4, nameof(buffer));
ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / sector, nameof(blockCount));
ObjectDisposedException.ThrowIf(_disposed, this);
```

Un intervalle se vérifie avec deux helpers, un par borne (`ThrowIfLessThan` puis `ThrowIfGreaterThan`), et non avec un seul `||` écrit à la main. Une condition qu'aucun helper n'exprime (`sectorCount > host.BlockCount - startSector`) garde le `throw` explicite, tout comme une vérification qui doit journaliser avant de lever l'exception. Le nom du paramètre est toujours `nameof(x)`, jamais un littéral de chaîne, et jamais le message : `new ArgumentOutOfRangeException("Invalid offset or size")` nomme un paramètre appelé `Invalid offset or size`.

<a id="when-to-panic-vs-throw"></a>

### Quand paniquer, quand lever une exception

| Situation | Action |
|-----------|--------|
| Défaillance matérielle, état corrompu | `Panic.Halt()` |
| Erreur interne du GC ou de l'allocateur | `Panic.Halt()` |
| Utilisation invalide de l'API | `throw` de l'exception appropriée |
| Fonctionnalité absente à l'exécution, dans un membre qui agit sur elle | `throw InvalidOperationException` en nommant le commutateur |
| Fonctionnalité absente à l'exécution, dans un membre qui répond à une question | Renvoyer `0`/`null`/`false`/vide |
| Erreur visible par l'utilisateur dans le shell du noyau | `try/catch` + affichage d'un message d'erreur |

Le type levé est un type qu'un appelant peut intercepter par son nom : `InvalidOperationException`, `ArgumentException` et ses sous-classes, `NotSupportedException`. On n'écrit pas de `throw new Exception(...)` nu (CA2201). Le canal qu'utilise un membre de l'anneau supporté pour signaler un échec, et les cas où un `Try` renvoie plutôt `false`, sont décrits dans [Suivi de l'API publique](public-api.md#how-a-member-reports-failure).

---

<a id="12-nullable-reference-types"></a>

## 12. Types référence nullables

Les types référence nullables sont activés dans toute la solution (`<Nullable>enable</Nullable>` dans `Directory.Build.props`). Les annotations font partie du contrat de l'API : un type référence sans `?` promet qu'il n'est jamais null. Le code nouveau ou modifié ne doit pas introduire d'avertissements de nullabilité : annotez honnêtement au lieu de faire taire les avertissements.

<a id="rules"></a>

### Règles

| Modèle | Utilisation |
|---------|---------|
| `T?` sur les champs, les paramètres, les retours | Toute valeur qui peut légitimement être null |
| `?? throw` | Convertir une valeur nullable en valeur obligatoire |
| `ArgumentNullException.ThrowIfNull(arg)` | Validation des arguments aux frontières de l'API |
| Helpers de garde `[MemberNotNull(...)]` | Vérifications d'initialisation que le compilateur peut suivre |
| `is null` / `is not null` | Tous les tests de null (pas `== null` / `!= null`). Un pointeur garde `== null` : les patterns ne sont pas autorisés sur les types pointeur |
| Initialisation par sentinelle (`= []`) | Les champs pour lesquels null et vide veulent dire la même chose |
| `[NotNullWhen(true)]` sur le `out` d'une méthode `Try` | La valeur que remplit un retour `true` ; voir [Comment un membre signale un échec](public-api.md#how-a-member-reports-failure) |
| `[MemberNotNullWhen(true, ...)]` sur un `bool Initialize()` | Un retour `true` qui garantit un membre statique à l'appelant (`KernelConsole.Initialize` et `Default`) |
| `T?[]` | Un tableau dont les emplacements peuvent être vides (`IrqDelegate?[]`) |

<a id="honest-annotations"></a>

### Annotations honnêtes

Un membre qui peut renvoyer `null` est déclaré avec `?` : ne le cachez jamais derrière `!` pour préserver une ancienne signature. À l'inverse, n'ajoutez pas de vérifications de null à l'exécution pour des valeurs que les annotations garantissent déjà : pas de `if (buffer == null) throw` sur un paramètre non nullable, pas de `?? "fallback"` sur un retour non nullable.

```csharp
// Declare what can actually be null: callers are forced to handle it
public static IScheduler? Current => _currentScheduler;

public static PerCpuState? GetCpuState(uint cpuId)
{
    return _cpuStates?[cpuId];
}

// Prefer a sentinel over a nullable field when null and empty mean the same thing
private byte[] _window = [];                 // not: private byte[]? _window;
```

<a id="guard-helpers"></a>

### Helpers de garde

Les gestionnaires à initialisation différée exposent des gardes `ThrowIf*NotInitialized()` annotées avec `[MemberNotNull(...)]` et les appellent au début des points d'entrée publics (voir [Gestionnaires](#4-class--type-design)). Un seul appel prouve que le champ est non null pour le reste de la méthode, sans `!` ni vérification à chaque ligne :

```csharp
private static PerCpuState[]? s_cpuStates;

[MemberNotNull(nameof(s_cpuStates))]
private static void ThrowIfCpuStateNotInitialized()
{
    if (s_cpuStates is null)
    {
        throw new InvalidOperationException($"{nameof(SchedulerManager)} not initialized");
    }
}

public static void CreateThread(uint cpuId, Thread thread)
{
    ThrowIfCpuStateNotInitialized();

    PerCpuState state = s_cpuStates[cpuId];   // no warning, no !
    // ...
}
```

La même famille d'attributs couvre un initialiseur `bool` : `[MemberNotNullWhen(true, nameof(Default))]` sur `KernelConsole.Initialize()` permet à l'appelant de lire `KernelConsole.Default` dans `if (KernelConsole.Initialize())` sans garde de sa part.

<a id="callers-of-a-try"></a>

### Appelants d'un `Try`

Une fois qu'un `Try` annoté a renvoyé `true`, son `out` est non null et aucun site d'appel ne le teste à nouveau : `if (!TryOpen(path, out handle) || handle == null)` s'écrit `if (!TryOpen(path, out handle))`. `Dictionary<TKey, TValue>.TryGetValue` est annoté de la même façon par la BCL, si bien qu'un `&& value != null` placé après lui ne protège de rien quand `TValue` est non nullable. Un test redondant n'est pas inoffensif : il masque le seul endroit où une garde est réelle.

<a id="required-values"></a>

### Valeurs obligatoires

Utilisez `?? throw` là où un résultat null est une erreur de programmation :

```csharp
Address source = IPConfig.FindNetwork(destination)
    ?? throw new InvalidOperationException("No network route to destination");

public MacAddress MacAddress
{
    get
    {
        return _macAddress ?? throw new InvalidOperationException($"{nameof(_macAddress)} is null");
    }
}
```

<a id="constructors"></a>

### Constructeurs

Chaque constructeur doit laisser initialisés tous les membres non nullables. Supprimez les constructeurs sans paramètre inutilisés au lieu de les laisser empoisonner le type :

```csharp
// Bad: leaves RawData null and forces nullable noise on every user of the class
internal IcmpPacket()
{
}
```

<a id="the--operator"></a>

### L'opérateur `!`

L'opérateur de tolérance null `!` est un dernier recours pour les invariants que l'analyse de flux ne peut pas voir (par ex. un champ garanti non null tant qu'un indicateur de garde vaut `true`). Si l'invariant peut s'exprimer avec `[MemberNotNull]` ou `[NotNullWhen]`, utilisez plutôt l'attribut.

<a id="enforcement"></a>

### Contrôle

La CI de formatage (`dotnet format --severity error`) impose les règles de style de traitement du null définies dans `.editorconfig` : `csharp_style_throw_expression`, `dotnet_style_coalesce_expression`, `dotnet_style_null_propagation`, `csharp_style_prefer_null_check_over_type_check`, `csharp_style_pattern_matching_over_as_with_null_check`, `dotnet_style_prefer_is_null_check_over_reference_equality_method` et `csharp_style_conditional_delegate_call`.

Les avertissements de nullabilité (`CS86xx`) ne sont pas encore des erreurs : n'en introduisez pas de nouveaux.

---

<a id="13-modern-c--net-10-idioms"></a>

## 13. Idiomes modernes de C# / .NET 10

Le projet cible `<LangVersion>latest</LangVersion>` et .NET 10. Utilisez partout les fonctionnalités modernes du langage.

<a id="modern-patterns"></a>

### Modèles modernes

```csharp
// File-scoped namespaces
namespace Cosmos.Kernel.Core.Scheduler;

// Expression bodies for a property, indexer or accessor that fits on one line
public string PlatformName => "x86-64";

// Pattern matching
switch (args[i])
{
    case null:
        WriteString("null");
        break;
    case string s:
        WriteString(s);
        break;
    case int n:
        WriteNumber(n);
        break;
}

// Null-conditional and coalescing
IBlockDevice? device = StorageManager.PrimaryDevice;
if (device?.BlockSize is not > 0)
{
    return;
}

Address ip = config?.Address ?? defaultAddress;

// Collection expressions
DeviceResource[] resources = [DeviceResource.PortRange(0x60, 1), DeviceResource.PortRange(0x64, 1)];

// Target-typed new
Thread thread = new();
List<int> items = new();

// Numeric separators for readability
public const nuint DefaultStackSize = 64 * 1024;
public static long TscFrequency { get; set; } = 1_000_000_000;

// stackalloc for small buffers (no heap allocation)
Span<byte> buffer = stackalloc byte[64];

// field keyword: auto-property with validation without a manual backing field
public int Priority
{
    get => field;
    set => field = value >= 0 ? value : throw new ArgumentOutOfRangeException();
}

// Interpolation, not concatenation
Serial.WriteString($"[{Table[(int)Status]}] {packet}\n");

// required members for state the constructor cannot take; derive what follows from it
public sealed class DhcpOption
{
    public required byte Type { get; init; }
    public required byte[] Data { get; init; }
    public byte Length => (byte)Data.Length;
}

// One dictionary call carries the answer
if (!s_clients.TryGetValue(port, out UdpClient? client))
{
    return;
}
s_registeredTypes.TryAdd(name, fileSystemType);
cache[id] = mac;                        // upsert: no ContainsKey first

// readonly on every field assigned only at its declaration or in a constructor
private readonly Canvas _canvas;
private static readonly ArrayPool<byte> s_arrayPool = ArrayPool<byte>.Shared;

// Auto-property over a field behind a pass-through accessor
public static KernelConsole? Default { get; private set; }
```

Trois limites s'appliquent aux deux derniers. `readonly` sur un champ dont le type est une struct mutable (`SpinLock`) est une erreur : chaque appel de méthode agirait sur une copie défensive, et le verrou ne serait jamais pris. L'analyseur derrière `dotnet_style_readonly_field` ne sait pas quelles méthodes de struct modifient l'état, si bien que sa suggestion n'est retenue que pour les types référence et pour les structs sans membre qui modifie l'état. Un champ qu'un plug atteint par son nom (`[FieldAccess]`) reste un champ : le champ de stockage d'une propriété automatique porte un nom généré par le compilateur. Enfin, un initialiseur statique qui alloue est un constructeur de classe, qui s'exécute au premier accès en passant par un verrou qui a besoin d'un thread courant : un type accessible depuis la mise en route des périphériques (`MacAddress`, dont n'importe quel pilote peut lire `None` et `Broadcast`) garde ses champs statiques remplis à la demande, avec un commentaire qui explique pourquoi.

<a id="avoid-1"></a>

### À éviter

```csharp
// Don't use var when type isn't obvious
var x = GetValue();           // Bad: what type is x?
int count = GetValue();       // Good: explicit type

// Don't use var for built-in types
var i = 0;                    // Bad
int i = 0;                    // Good

// Don't use this. qualification
this._field = value;          // Bad
_field = value;               // Good

// Don't allocate arrays in hot paths when Span works
byte[] temp = new byte[16];       // Bad in hot path: heap allocation
Span<byte> temp = stackalloc byte[16]; // Good: stack allocation

// Always use braces, no single-line if/for/while without braces
if (ptr == null) return;          // Bad
if (ptr == null)                  // Bad
    return;

if (ptr == null)                  // Good
{
    return;
}

// Don't give a method, constructor, operator or local function an expression body
public void Halt() => Native.Cpu.Halt();     // Bad

public void Halt()                           // Good
{
    Native.Cpu.Halt();
}

// Don't wrap an expression body: past one line, a property takes a get block
public bool IsFixedRoot =>                   // Bad
    Parent is null && Superblock.Boot.Type != FatType.Fat32;

public bool IsFixedRoot                      // Good
{
    get
    {
        return Parent is null && Superblock.Boot.Type != FatType.Fat32;
    }
}

// Don't compare a bool to a literal
if (applied == false)             // Bad
if (!applied)                     // Good

// Don't test a key, then index; the lookup answers once
if (map.ContainsKey(key))         // Bad: two lookups
{
    device = map[key];
}

// Don't loop on a literal; the condition says what ends the loop
while (true)                      // Bad: nothing in the header bounds it
{
    if (pos >= end)
    {
        break;
    }
    pos += Step(pos);
}

while (pos < end)                 // Good: the bound is the condition
{
    pos += Step(pos);
}
```

> Les accolades sont imposées par `.editorconfig` (`csharp_prefer_braces = true:error`) et par la CI (`dotnet format style --severity error`).

Les méthodes, les constructeurs, les opérateurs et les fonctions locales prennent un corps en bloc, même pour une seule instruction : un `=>` renvoyé à la ligne cache l'endroit où commence le corps. Une propriété, un indexeur ou un accesseur n'utilise `=>` que si le membre entier tient sur une ligne, et une lambda garde son `=>`. Les arborescences de code tiers embarqué sous `ThirdParty/` suivent elles aussi cette règle, et le README de chacune liste les membres qu'elle a modifiés. `.editorconfig` consigne cette règle dans les options `csharp_style_expression_bodied_*` au niveau `silent`, si bien que la CI ne l'impose pas.

<a id="bounded-loops"></a>

### Boucles bornées

On n'écrit pas `while (true)`. L'en-tête d'une boucle énonce ce qui la termine : un compteur, une borne sur un indice, une échéance, un indicateur que le corps positionne quand il a trouvé ce qu'il cherchait. Avec la borne dans l'en-tête, un lecteur vérifie en un seul endroit que chaque chemin progresse vers elle ; avec `while (true)`, la sortie se trouve là où il y a un `break` ou un `return`, et la boucle qui a perdu sa sortie lors d'une modification se lit exactement comme celle qui n'en a jamais eu. Les trois sortes de boucles qui semblent non bornées ont chacune une borne à énoncer :

| Boucle | En-tête |
|------|--------|
| Une analyse qui parcourt un tampon | `while (pos < end && !closed)`, avec la fin de l'analyse comme borne et un indicateur pour l'enregistrement qui la clôt |
| Une attente sur le matériel ou sur un autre thread | `while (!ready && Stopwatch.GetTimestamp() < deadline)` ou `while (!ready && attempts++ < MaxAttempts)` ; une scrutation sans budget est le blocage que suit [#365](https://github.com/valentinbreiz/nativeaot-patcher/issues/365) |
| La boucle principale d'un noyau, ou un arrêt qui ne revient jamais | `while (!_stopped)` pour la première ; le second arrête le CPU dans le corps de sa boucle, tourner y est donc voulu et `while (true) { Halt(); }` reste |

La dernière ligne est la seule exemption : une méthode qui ne revient jamais et dont le corps de boucle arrête le CPU, comme celle sur laquelle `Power.Reboot` et `Power.Shutdown` se rabattent quand la plateforme ne sait faire ni l'un ni l'autre, et comme le fait `Kernel.Start` après `AfterRun`. Une boucle au corps vide ne relève pas de cette exemption : elle brûle le CPU pour n'aller nulle part, et doit plutôt arrêter le CPU. Tout le reste énonce sa borne. Les boucles `while (true)` existantes dans `src/` sont antérieures à cette règle et sont converties à mesure que le code qui les entoure est modifié, pas en une seule passe ; aucun analyseur ne le vérifie.

<a id="feature-switches"></a>

### Commutateurs de fonctionnalité

Utilisez `[FeatureSwitchDefinition]` pour activer ou désactiver des fonctionnalités à la compilation (retirées par le trimming de l'éditeur de liens NativeAOT). Tous les indicateurs de fonctionnalité se trouvent dans `Cosmos.Kernel.Core.CosmosFeatures` :

```csharp
// In CosmosFeatures.cs: one property per feature
[FeatureSwitchDefinition("Cosmos.Kernel.HAL.Interrupts.Enabled")]
public static bool InterruptsEnabled
{
    get
    {
        return AppContext.TryGetSwitch("Cosmos.Kernel.HAL.Interrupts.Enabled", out bool enabled) ? enabled : true;
    }
}

// The ILC linker trims the dead branch entirely
if (CosmosFeatures.KeyboardEnabled)
{
    // This code is removed from the binary when keyboard is disabled
}
```

---

<a id="14-aot-constraints"></a>

## 14. Contraintes AOT

NativeAOT impose des limitations strictes. **Tout le code du noyau doit être compatible AOT.**

.NET 10 a amélioré NativeAOT avec des binaires plus petits et une prise en charge plus large des plateformes, mais les contraintes fondamentales demeurent.

<a id="forbidden"></a>

### Interdit

- `System.Reflection.Emit` (pas de génération de code à l'exécution).
- Le mot-clé `dynamic`.
- `Assembly.Load` à l'exécution.
- `Type.GetType("...")` par chaîne à l'exécution (les types doivent être atteignables statiquement).
- `MakeGenericType` / `MakeGenericMethod` à l'exécution (les instanciations génériques doivent être connues à la compilation).

<a id="use-with-caution"></a>

### À utiliser avec prudence

- **Méthodes virtuelles génériques** : NativeAOT doit générer toutes les instanciations possibles à la compilation. Évitez les dispatchs virtuels génériques profondément imbriqués ou ouverts.
- **LINQ** : de nombreux opérateurs LINQ allouent des itérateurs et des fermetures. À éviter sur les chemins critiques (GC, ordonnanceur, gestionnaires d'interruption).

---

## 15. Documentation

<a id="license-header"></a>

### En-tête de licence

Chaque fichier `.cs` doit commencer par l'en-tête de licence (imposé par `.editorconfig`) :

```csharp
// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
```

<a id="xml-documentation"></a>

### Documentation XML

Documentez toutes les API **publiques** avec `<summary>`. Restez concis :

```csharp
/// <summary>
/// Triggers a kernel panic with the specified message.
/// Disables interrupts and halts the CPU.
/// </summary>
/// <param name="message">The panic message describing the error.</param>
public static void Halt(string message) { ... }
```

**Règles :**
- Documentez les classes, les interfaces, les méthodes et les propriétés publiques.
- Ne documentez pas les membres évidents (`get`/`set` pour des noms de propriétés qui parlent d'eux-mêmes).
- Utilisez `<param>` pour les paramètres non évidents.
- Utilisez `<returns>` quand la valeur de retour mérite une explication.
- Utilisez `<see cref="..."/>` pour référencer les types liés.
- Ne documentez pas les détails d'implémentation privés, sauf si la logique est complexe.

<a id="inline-comments"></a>

### Commentaires dans le code

Utilisez-les avec parcimonie, uniquement quand le code ne s'explique pas de lui-même :

```csharp
// Stack layout (growing downward from top):
// [StackBase + stackSize] = Top of usable stack
// [contextAddr] = Start of ThreadContext (where StackPointer points)
// [StackBase] = Bottom of stack

// Align context to 16 bytes (required for XMM operations)
contextAddr = (contextAddr + 0xF) & ~(nuint)0xF;
```

Le code mis en commentaire est supprimé ; l'historique le conserve. Un `TODO` nomme ce qui manque et pourquoi cela peut attendre ; un doute sur le code (`// is this correct?`) est une issue, pas un commentaire. Un membre au corps vide n'est pas un espace réservé : on ne l'écrit pas tant qu'il ne fait rien.

<a id="16-testing"></a>

## 16. Tests

Pour le guide de test complet (tests unitaires, tests d'intégration du noyau, protocole UART, CI, écriture de noyaux de test), voir [Tests](testing.md).

La logique qui n'a pas besoin de matériel (l'arithmétique du tampon de réception de `TcpConnection`, l'analyse des adresses) est testée unitairement dans le processus hôte depuis `tests/Cosmos.Kernel.Tests.System` (NUnit) : une fixture imbriquée par membre testé, nommée d'après lui, des noms de tests de la forme `WhenX_AndY_ResultZ`, et `[TestCase(..., ExpectedResult = ...)]` pour les tables de valeurs. Le projet bénéficie d'une autorisation `InternalsVisibleTo` accordée par `Cosmos.Kernel.System`.

**Couverture de code :** ajoutez le label `run-coverage` à une pull request pour déclencher la CI de couverture. Elle exécute les suites de tests du noyau et indique quels chemins de code sont couverts par les tests d'intégration.

---

<a id="17-public-api-surface"></a>

## 17. Surface d'API publique

Trois règles décident de ce qui est `public` (la politique complète et ses mécanismes sont décrits dans [Suivi de l'API publique](public-api.md)) :

1. **Un seul anneau supporté.** `Cosmos.Kernel.System` est l'API sur laquelle les noyaux sont programmés, plus les types de contrat qu'exposent ses signatures (les contrats de périphériques de la HAL, `IBlockDevice` et `MacAddress`, et les interfaces de plateforme du Core). Seule cette surface est suivie, documentée et couverte par des cycles de dépréciation.
2. **Des points d'extension expérimentaux choisis.** Un point d'extension hors de l'anneau est ouvert délibérément et marqué `[Experimental("COSMOSxxxx")]` : utilisable dès maintenant, sans promesse de compatibilité, promu en retirant l'attribut. N'ouvrez jamais un point d'extension en rendant simplement quelque chose public.
3. **Tout le reste est `internal`.** La visibilité n'est pas le mécanisme d'extension. Les assemblies du projet lui-même et les noyaux de test en boîte blanche utilisent `InternalsVisibleTo` ; le code externe utilise `[UnsafeAccessor]` ([Accéder aux éléments internes](accessing-internals.md)) à ses propres risques.

Les règles pratiques qui en découlent :

```csharp
// Good: new user-facing capability lands as a Cosmos.Kernel.System facade
public static class MemoryDiagnostics { public static ulong FreePages => PageAllocator.FreePageCount; }

// Bad: making the Core type public so a kernel can reach it
public static class PageAllocator { ... }
```

- **Les nouveaux types sont `internal` par défaut.** Rendre un symbole `public` dans un projet suivi est une décision soumise à revue : le build échoue (`RS0016`) tant que `make api` ne l'a pas consigné dans `PublicAPI.Unshipped.txt`, et le diff du txt a sa place dans le même commit.
- **Le test qui fait foi est DevKernel.** `examples/DevKernel` se compile sans aucune autorisation `InternalsVisibleTo` ; tout ce dont il a besoin doit venir de l'anneau supporté.
- **La surface suivie doit être documentée.** Activer `CosmosTrackPublicApi` transforme les documentations XML manquantes (`CS1591`) en erreurs de build pour les symboles publics du projet.
- **Un noyau de test en boîte blanche reçoit une autorisation `InternalsVisibleTo`**, avec, dans le `.csproj` qui l'accorde, un commentaire qui indique ce qu'il observe ; il ne force jamais un symbole à devenir public.

---
