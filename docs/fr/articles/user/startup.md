---
source_hash: 182026ee40c8fb202dff9cc679d4cedf12c97e13
---
<a id="kernel-startup"></a>

# Démarrage du noyau

Dans cet article, nous verrons ce qui se passe entre la mise sous tension et le premier appel à la méthode `Run()` de votre noyau : la chaîne de démarrage, les phases d'initialisation et le cycle de vie `BeforeRun`/`Run`/`AfterRun` sur lequel repose votre noyau.

Les principales différences si vous venez de Gen2 :

| | Gen2 | Gen3 |
|---|---|---|
| Compilateur | IL2CPU | .NET NativeAOT (ILC) + le patcher IL de Cosmos |
| Firmware | BIOS | UEFI, sur x64 et ARM64 |
| Chargeur d'amorçage | Limine | Limine |
| Mécanique du point d'entrée | IL2CPU émet l'appel à `Kernel.Start()` | Un générateur de source émet `CosmosEntryPoint.Main()` |
| Cycle de vie | `BeforeRun` / `Run` / `AfterRun` | `BeforeRun` / `Run` / `AfterRun` (inchangé) |

<a id="the-boot-chain"></a>

## La chaîne de démarrage

```
UEFI firmware
    │
Limine            loads kernel.elf, maps it in the higher half, sets up the framebuffer
    │
kmain()           native C bootstrap (Cosmos.Kernel/Bootstrap/kmain.c)
    ├─ Phase 1    CPU: enable SIMD, initialize the serial port
    ├─ Phase 2    Platform: RSDP + HHDM from Limine, early ACPI parse (MADT, MCFG)
    ├─ Phase 3    Managed runtime: heap, GC, type system, library initializers
    └─ Phase 4    User kernel: Main(argc, argv) → Kernel.Start()
```

Le chargeur d'amorçage [Limine](https://limine-bootloader.org/) charge l'ELF du noyau produit par le pipeline de build, puis saute vers `kmain()`, un petit code d'amorçage en C compilé dans chaque noyau. À partir de là :

- **Phase 1 : CPU.** SIMD est activé en premier (le code généré par NativeAOT utilise les registres XMM dès la toute première instruction) et le port série est initialisé, si bien que tout ce qui suit cette ligne est journalisé. Sur ARM64, la vérification d'alignement est également désactivée à ce stade.
- **Phase 2 : plateforme.** Le code d'amorçage demande à Limine le RSDP ACPI et le décalage du mappage direct en moitié haute, puis effectue une analyse ACPI précoce : la MADT (où se trouvent les contrôleurs d'interruptions et les CPU) et la MCFG (où se trouve l'espace de configuration PCIe).
- **Phase 3 : runtime managé.** Le chemin de démarrage de NativeAOT s'exécute. C'est là que le monde C# prend vie, un package à la fois (voir la section suivante).
- **Phase 4 : noyau utilisateur.** Le code d'amorçage construit `argv` à partir de la ligne de commande du noyau et appelle le `Main` managé, qui aboutit au `Start()` de votre noyau.

<a id="phase-3-how-the-managed-kernel-comes-up"></a>

## Phase 3 : comment le noyau managé démarre

Chaque package Cosmos fournit un *initialiseur de bibliothèque* que le runtime exécute avant la moindre ligne de votre code. Le SDK les ordonne en trois temps : le tas d'abord, puis les initialiseurs propres au runtime, puis les bibliothèques du noyau, chacune après les packages qu'elle référence.

1. **Cosmos.Kernel.Core** : découpe le tas dans la carte mémoire de Limine, initialise le ramasse-miettes, puis enregistre le système de types (champs statiques, constructeurs statiques immédiats, initialiseurs de module). Rien n'alloue avant cette étape.
2. **Les initialiseurs propres au runtime** (`System.Private.CoreLib` et ses compagnons) : l'`OutOfMemoryException` préallouée, l'exécuteur de constructeurs de classe, le chargeur de types et les callbacks de réflexion, les métadonnées de trace de pile. L'exécuteur de constructeurs de classe est créé ici : un champ statique dont le type a un constructeur statique paresseux peut donc être lu à partir de cette étape, et pas avant.
3. **Cosmos.Kernel.HAL** : HAL de la plateforme ; le device tree transmis par le chargeur d'amorçage, s'il y en a un, est enregistré (`[Firmware] Device tree at 0x..., N bytes`, ou `[Firmware] No device tree` sur q35 et lors d'un démarrage ACPI de la machine virt) ; le contrôleur d'interruptions, le matériel de la plateforme (APIC/GIC, minuteurs), puis les nœuds de plateforme racines de la machine sont publiés dans le kit de pilotes juste après `InitializeHardware` (sur x64, le contrôleur clavier 8042, `platform:i8042@60`, puis l'hôte PCI, `platform:pci@cf8` ; sur ARM64, l'hôte ECAM issu de la MCFG ou du device tree et un nœud `platform:virtio_mmio@...` par emplacement occupé, issu du device tree ou de la fenêtre de la machine virt, selon ce que disent les commutateurs), où ils attendent l'étape des pilotes. Les contrôleurs 8042, AHCI, NVMe et les contrôleurs hôtes USB (xHCI, EHCI, OHCI) ne sont pas initialisés ici : leurs pilotes vivent dans le kit de pilotes et les lient pendant l'étape des pilotes. Enfin, dès que les graphismes sont activés, avec ou sans interruptions, le framebuffer transmis par Limine est enregistré (`[KERNEL]   - Recording the firmware framebuffer...`) pour que l'étape des pilotes le publie comme écran du firmware.
4. **Cosmos.Kernel** : les gestionnaires d'exceptions du CPU et l'ordonnanceur (un thread inactif par CPU, préemption sur un quantum de 10 ms).
5. **Cosmos.Kernel.System** : les gestionnaires de services `TimerManager`, `KeyboardManager`, `MouseManager`, `NetworkManager`, `DisplayManager`, `StorageManager`. `KeyboardManager`, `MouseManager`, `NetworkManager`, `DisplayManager` et `StorageManager` installent chacun ici leur consommateur du kit de pilotes, avant l'étape des pilotes, pour qu'un clavier, un pointeur, une interface, un écran ou un disque publié par un pilote du kit parvienne à son gestionnaire, tout comme l'écran du firmware que l'étape des pilotes publie en premier ; aucun périphérique réseau de plateforme n'est enregistré ici, et la première interface publiée par un pilote du kit devient l'interface principale. Aucun clavier ni aucune souris de plateforme n'est enregistré ici non plus : les périphériques PS/2 arrivent par les consommateurs pendant l'étape des pilotes, comme tout autre périphérique d'entrée. Les disques du gestionnaire de stockage arrivent tous par son consommateur pendant l'étape des pilotes.

Chacune des étapes 3 à 5 est conditionnée par un commutateur de fonctionnalité (`CosmosEnableInterrupts`, `CosmosEnablePCI`, `CosmosEnableTimer`, `CosmosEnableKeyboard`, `CosmosEnableMouse`, `CosmosEnableNetwork`, `CosmosEnableStorage`, `CosmosEnableGraphics`, `CosmosEnableScheduler`, tous à `true` par défaut, et `CosmosEnableUsb`, activé par défaut dès que le commutateur du clavier, de la souris ou du stockage l'est). Mettez-en un à `false` dans votre `.csproj` : le sous-système correspondant est alors ignoré ici et exclu de la compilation du noyau.

<a id="the-generated-entry-point"></a>

## Le point d'entrée généré

Vous n'écrivez jamais de `Main` pour un noyau Cosmos. Le SDK fournit un générateur de source qui l'émet à partir de la propriété de projet `CosmosKernelClass` :

```csharp
// <auto-generated/> CosmosEntryPoint.g.cs
namespace Cosmos.Kernel.System.Internal;

public static class CosmosEntryPoint
{
    public static void Main()
    {
        global::Cosmos.Kernel.System.Internal.DriverManifest.Register();
        global::Cosmos.Kernel.System.Internal.KernelEntry.Start(new global::MyOS.Kernel());
    }
}
```

`DriverManifest` est le second fichier généré : la liste des classes de pilotes que le noyau embarque, enregistrée avant le démarrage du noyau pour que l'étape des pilotes de `Start()` puisse leur proposer des périphériques. Un noyau sans pilote qui lui soit propre embarque tout de même les vingt-cinq que Cosmos livre dans `Cosmos.Kernel.Drivers` : le pilote de l'hôte PCI, le pilote de port racine PCI Express, le pilote Intel E1000E, les pilotes de transport virtio PCI et MMIO, les pilotes virtio-net, virtio-input et virtio-blk, les quatre pilotes d'affichage, virtio-gpu, VMware SVGA II, AMD DCN 3.1.5 et la partie graphique intégrée d'Intel, les deux pilotes de stockage, AHCI et NVMe, le pilote HD Audio, les sept pilotes USB, les contrôleurs hôtes xHCI, EHCI et OHCI avec les pilotes de classe hub, clavier HID boot, souris HID boot et stockage de masse, et les trois pilotes PS/2, le contrôleur 8042 avec les pilotes de classe clavier et souris, dont vingt-deux dépendent d'un commutateur de fonctionnalité (les sept pilotes USB de `CosmosEnableUsb`, le clavier et la souris PS/2 de `CosmosEnableKeyboard` et `CosmosEnableMouse`, le pilote HD Audio de `CosmosEnableAudio`). La page [Manifeste des pilotes](../dev/build/driver-manifest.md) décrit comment la liste est construite et comment un projet exclut un pilote ou choisit d'en inclure un.

`CosmosKernelClass` vaut `<RootNamespace>.Kernel` par défaut : une classe nommée `Kernel` dans l'espace de noms racine de votre projet est donc prise en compte automatiquement. Pour utiliser un autre type, définissez la propriété explicitement :

```xml
<PropertyGroup>
  <CosmosKernelClass>MyOS.Boot.MyKernel</CosmosKernelClass>
</PropertyGroup>
```

`KernelEntry.Start` enregistre ensuite cette instance comme `Kernel.Current` et appelle son `Start()`. `KernelEntry` est une mécanique de démarrage réservée à ce code généré, publique parce que ce code est compilé dans l'assembly de votre noyau ; votre propre code ne l'appelle jamais.

## Sys.Kernel.Start()

`Cosmos.Kernel.System.Kernel` est la classe de base abstraite de tout noyau utilisateur. Son `Start()` pilote l'ensemble du cycle de vie :

1. Active les interruptions matérielles (tout ce qui précède s'est exécuté avec les interruptions désactivées).
2. Démarre le thread des finaliseurs lorsque `CosmosEnableFinalizerThread` est activé, ce qui est le cas par défaut dès que l'ordonnanceur l'est : à partir de là, les finaliseurs s'exécutent sur leur propre thread plutôt que sur le thread qui a effectué la collecte ou l'allocation ([Finalisation](../dev/garbage-collector.md#finalization)).
3. Exécute l'étape des pilotes : le kit journalise son manifeste, publie le framebuffer du firmware enregistré en phase 3 comme écran du firmware (l'écran principal du gestionnaire d'affichage jusqu'à ce qu'un pilote d'affichage en publie un), puis chaque nœud publié par les bus est proposé aux pilotes listés dans le manifeste du noyau, d'abord les nœuds de plateforme placés en phase 3 (le pilote 8042 publie ses deux nœuds de port PS/2 depuis son sondage, et les pilotes clavier et souris les lient pendant l'étape), puis chaque fonction PCI trouvée par le pilote de l'hôte PCI et chacune de celles qu'un pilote de port racine a trouvées derrière son emplacement de branchement à chaud, puis chaque périphérique virtio qu'un pilote de transport a publié sous une fonction ou un emplacement, et l'étape rend la main une fois que chaque nœud, enfants compris, a été proposé ; les pilotes AHCI, NVMe, virtio-blk et stockage de masse USB publient ici les disques qu'ils trouvent (les pilotes de contrôleurs hôtes énumèrent leurs ports racines pendant leur sondage, si bien qu'une clé présente au démarrage est elle aussi publiée pendant l'étape), et le gestionnaire de stockage enregistre et analyse chacun d'eux au cours de la publication : `StorageManager.Partitions` est donc rempli au moment où l'étape rend la main. Le thread de branchement à chaud de chaque pilote de contrôleur hôte USB, démarré pendant son sondage, suit ensuite les périphériques branchés ou retirés ; il a besoin de l'ordonnanceur. Le thread d'emplacement du pilote de port racine fait de même pour un périphérique branché dans un emplacement de branchement à chaud PCI Express ou retiré de celui-ci, en plaçant les registres de celui qui arrive ; il a lui aussi besoin de l'ordonnanceur.
3. Appelle `OnBoot()`, dont l'implémentation par défaut initialise la `KernelConsole` graphique sur l'écran devenu principal à ce moment-là, ce qui fait fonctionner `Console.WriteLine`.
5. Désactive le moteur de rendu de texte du démarrage précoce : jusqu'ici, le journal de démarrage que vous voyez à l'écran est le journal série recopié par un module minimal d'écriture dans le framebuffer ; désormais, l'écran appartient à `Console` et au [Canvas](graphics.md).
6. Appelle `BeforeRun()` une fois.
7. Appelle `Run()` en boucle jusqu'à ce que `Stop()` soit appelé.
8. Appelle `AfterRun()` une fois.
9. Arrête le CPU. Il n'y a aucun système d'exploitation auquel revenir : un noyau ne se termine jamais.

<a id="stopping-the-machine"></a>

## Arrêter la machine

L'étape 9 ci-dessus est l'endroit où un noyau aboutit de lui-même. `Power` vous permet d'y arriver délibérément, et ses trois membres diffèrent par la portée de leur effet :

```csharp
using Cosmos.Kernel.System;

Power.Halt();      // park this CPU until an interrupt wakes it
Power.Reboot();    // restart the machine; does not return
Power.Shutdown();  // power off; does not return
```

`Halt()` est le seul qui rend la main. Il met le CPU en attente plutôt que de le faire tourner à vide : c'est donc ce qu'une boucle inactive doit appeler au lieu de `while (true) { }`, qui consume un cœur et, sur un noyau à un seul CPU, empêche l'ordonnanceur de progresser.

`Reboot()` et `Shutdown()` ne rendent pas la main en cas de succès. Tous deux passent par les opérations d'alimentation de la plateforme et, lorsque le firmware n'en propose aucune, se rabattent sur la mise en attente définitive du CPU plutôt que de continuer, ce qui explique que le compilateur les considère comme ne retournant jamais.

Pour terminer la boucle principale sans arrêter la machine, appelez `Stop()` sur votre noyau. `Run()` cesse d'être appelé, `AfterRun()` s'exécute une fois, et le CPU s'arrête.

Le code statique, qui n'a pas de `this` sur lequel l'appeler, atteint l'instance en cours d'exécution par `Kernel.Current`, que le point d'entrée généré définit avant le démarrage de votre noyau (il vaut `null` jusque-là, y compris dans le constructeur de votre noyau) :

```csharp
using Cosmos.Kernel.System;

Kernel.Current?.Stop();
```

<a id="a-minimal-kernel"></a>

## Un noyau minimal

```csharp
using System;
using Sys = Cosmos.Kernel.System;

namespace MyOS;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        Console.WriteLine("Cosmos booted successfully!");
    }

    protected override void Run()
    {
        Console.Write("Input: ");
        string? input = Console.ReadLine();
        Console.Write("Text typed: ");
        Console.WriteLine(input);
    }
}
```

- `BeforeRun()` : initialisation unique (monter un [système de fichiers](filesystem.md), configurer le [réseau](network.md), dessiner un écran d'accueil).
- `Run()` : le corps de votre boucle principale. Il est rappelé dès qu'il rend la main, il n'a donc pas besoin de boucler lui-même ; gardez-le réentrant.
- `Stop()` : appelez-le depuis n'importe où pour sortir de la boucle une fois le `Run()` en cours terminé.
- `AfterRun()` : nettoyage facultatif une fois la boucle terminée.

Une exception non interceptée dans `Run()` se propage hors de la boucle : enveloppez donc le corps dans un `try`/`catch` si l'échec d'une commande ne doit pas faire tomber le noyau.

<a id="customizing-startup"></a>

## Personnaliser le démarrage

`OnBoot()` s'exécute avant que la console n'existe : c'est le bon endroit pour vos propres initialisations précoces. Les interruptions sont déjà activées et l'étape des pilotes a eu lieu, si bien qu'un périphérique lié par un pilote est utilisable à partir d'ici ; le code qui doit s'exécuter avec les interruptions désactivées a sa place dans une redéfinition de `Start()` lui-même :

```csharp
protected override void OnBoot()
{
    base.OnBoot();   // keep the KernelConsole setup; drop this line to boot headless

    // your early initialization here
}
```

Un noyau headless qui veut plus tard une sortie `Console` appelle lui-même `KernelConsole.Initialize()` : c'est la seule voie sur l'anneau supporté, `Console.WriteLine` ne fait pas apparaître la console de lui-même. L'appel est idempotent : il peut donc être fait sans risque, que `base.OnBoot()` se soit déjà exécuté ou non, et il renvoie `false` lorsque les graphismes sont exclus de la compilation, qu'aucun écran n'est publié ou que l'écran n'a aucun mode.

Pour un contrôle total, vous pouvez redéfinir `Start()` lui-même et prendre en main le cycle de vie : l'implémentation par défaut dans [`Cosmos.Kernel.System/Kernel.cs`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Kernel.System/Kernel.cs) est courte et constitue un bon point de départ à copier.

<a id="the-kernel-command-line"></a>

## La ligne de commande du noyau

Limine transmet une ligne de commande au noyau : l'entrée `cmdline:` du fichier `Bootloader/limine.conf` de votre projet de noyau. Elle se récupère de la manière standard :

```csharp
foreach (string arg in Environment.GetCommandLineArgs())
{
    Console.WriteLine(arg);
}
```

<a id="watching-a-boot"></a>

## Observer un démarrage

Chacune des phases ci-dessus journalise sur le port série (COM1), que `cosmos run` relie à votre terminal, et c'est la première chose à lire lorsqu'un noyau ne démarre pas :

```
========================================
  CosmosOS v3.0.62 (gen3)
  Architecture: x86-64
========================================
[KMAIN] Phase 1: CPU initialization
[KMAIN] Phase 2: Platform initialization
[KMAIN]   - RSDP found at: 0xFFFF8000000F52D0
[KMAIN]   - Initializing ACPI...
[KMAIN] Phase 3: Managed kernel initialization
[KERNEL]   - Initializing heap...
[KERNEL]   - Initializing garbage collector...
[KERNEL]   - Initializing HAL...
[KERNEL]   - Recording the firmware device tree...
[Firmware] No device tree
[KERNEL]   - Initializing interrupts...
[KERNEL]   - Initializing platform hardware...
[KERNEL]   - Publishing platform nodes...
[KERNEL]   - Recording the firmware framebuffer...
[KERNEL]   - Initializing scheduler...
[KMAIN] Phase 4: User kernel
[KernelEntry] Registering kernel
[Kernel] Enabling interrupts...
[Kernel] Starting drivers...
[Drivers] manifest: HdAudioDriver(prio 0) EhciDriver(prio 0) OhciDriver(prio 0) PcieRootPortDriver(prio 0) VirtioPciTransportDriver(prio 0) XhciDriver(prio 0) AmdDcnDriver(prio 0) IntelGraphicsDriver(prio 0) VmwareSvgaDriver(prio 0) E1000EDriver(prio 0) AhciDriver(prio 0) NvmeDriver(prio 0) I8042Driver(prio 0) PciHostDriver(prio 0) VirtioMmioTransportDriver(prio 0) Ps2KeyboardDriver(prio 0) Ps2MouseDriver(prio 0) UsbHubDriver(prio 0) UsbKeyboardDriver(prio 0) UsbMouseDriver(prio 0) UsbMassStorageDriver(prio 0) VirtioGpuDriver(prio 0) VirtioInputDriver(prio 0) VirtioNetDriver(prio 0) VirtioBlkDriver(prio 0)
[Display] primary: firmware "framebuffer" (the only display)
[Drivers] firmware published display "framebuffer" (consumed)
[Drivers] engine started, worker thread
...
[StorageManager] sata0 registered by AhciDriver (primary)
[Drivers] pci:0000:00:03.0 AhciDriver published block "sata0" (consumed)
...
[Kernel] Calling OnBoot()...
[Kernel] Calling BeforeRun()...
[Kernel] Entering main loop...
[Kernel] Calling Run()...
```

Les deux lignes `sata0` apparaissent quand un disque est attaché (`cosmos run --disk disk.img`) : le pilote AHCI du kit le publie pendant l'étape des pilotes, et le gestionnaire de stockage l'enregistre au cours de la publication en analysant ses partitions, ce qui explique que la ligne du gestionnaire précède celle du kit. Avec une clé USB (`cosmos run --disk disk.img,usb`), ce sont les lignes du pilote xHCI qui apparaissent à leur place : le périphérique trouvé par le parcours des ports racines pendant le sondage, la ligne du contrôleur, puis le nœud d'interface de la clé proposé au pilote de stockage de masse et son disque enregistré pendant ce sondage :

```
[Drivers] pci:0000:00:03.0 XhciDriver: usb 1-1: 46f4:0001 SuperSpeed, 1 interface(s)
[Drivers] pci:0000:00:03.0 XhciDriver: version 0x100, 64 slots, 8 ports, 32-byte contexts, 0 scratchpad buffers, events via message interrupt, 1 device(s)
[Drivers] pci:0000:00:03.0 offer XhciDriver -> bound
[Drivers] usb:1-1:0 UsbMassStorageDriver: usb0 (LUN 0): QEMU QEMU HARDDISK, 524288 blocks of 512 bytes
[StorageManager] usb0 registered by UsbMassStorageDriver (primary)
[Drivers] usb:1-1:0 UsbMassStorageDriver published block "usb0" (consumed)
[Drivers] usb:1-1:0 UsbMassStorageDriver: 1 unit(s), max LUN 0
[Drivers] usb:1-1:0 offer UsbMassStorageDriver -> bound
```

Pour un débogage interactif en plus du journal série, consultez [Déboguer avec VSCode et QEMU](debugging.md).
