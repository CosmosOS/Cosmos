---
source_hash: cd3e319754a359b511df8073aa553e62612229de
---
<a id="file-system"></a>

# Système de fichiers

Dans cet article, nous verrons les disques et les fichiers sur Cosmos Gen3 : donner un disque à votre noyau, y monter un système de fichiers, lire des CD-ROM et des images ISO, partitionner et formater un disque, et manipuler des fichiers une fois un volume monté.

Lire et écrire des fichiers ne demande aucune API propre à Cosmos. Gen3 exécute sans modification le `System.IO` standard de .NET (`File`, `Directory`, `FileStream`, `StreamReader`, `FileInfo` et le reste), et le noyau répond aux appels système qu'il ferait sous Linux. La part de Cosmos est l'étape d'avant : votre noyau enregistre un pilote de système de fichiers et monte un volume sur un chemin de style Unix comme `/mnt`, et dès lors chaque chemin sous `/mnt` est servi par ce pilote.

Les principales différences si vous venez de Gen2 :

| | Gen2 | Gen3 |
|---|---|---|
| Chemins | Lettres de lecteur DOS (`0:\file.txt`) | Chemins Unix (`/mnt/file.txt`) |
| Mise en place | `CosmosVFS` + `VFSManager.RegisterVFS` | `VfsManager.RegisterFileSystem` + `VfsManager.TryMount` |
| Surface d'API | Sous-ensemble de `System.IO` remplacé par des plugs | `System.IO` complet (flux, motifs d'énumération, `FileInfo`, ...) |
| `File.Move` | Sans plug (copie + suppression) | Fonctionne, et peut écraser une destination existante |
| Systèmes de fichiers | FAT32 (FAT12/16 en partie), ISO 9660 | FAT12/16/32, ext2, ISO 9660 avec Rock Ridge (lecture seule) |
| Fichiers sur l'ISO | Un dossier `isoFiles` dans le projet | Le même dossier `isoFiles`, plus des éléments `CosmosIsoFile` ([Livrer des fichiers sur l'ISO de démarrage](#ship-files-on-the-boot-iso)) |

**Essayez d'abord dans une machine virtuelle.** Partitionner et formater écrivent directement sur le disque, sans rien à confirmer ni rien à annuler, si bien que sur du vrai matériel un décalage en secteurs erroné détruit tout ce que contenait le disque. `cosmos run` donne à QEMU une image disque que vous pouvez jeter, et un disque virtuel VMware ou VirtualBox est tout aussi jetable.

**AVERTISSEMENT** : n'essayez **pas** ceci sur du vrai matériel ! Cela peut causer des **DOMMAGES IRRÉPARABLES** à vos données. Utilisez une machine virtuelle (QEMU via `cosmos run`, VMware, VirtualBox, …).

Partitionnez et formatez les disques de votre noyau depuis Cosmos, avec les appels que montre cette page, plutôt qu'avec Parted ou fdisk. Cosmos est testé sur des disques qu'il a organisés lui-même, et un outil de l'hôte peut organiser un disque d'une façon que ces tests ne couvrent jamais.

Si vous trouvez des bugs ou quelque chose d'anormal, merci de [soumettre une issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) sur notre dépôt.

<a id="enable-storage-in-your-kernel"></a>

## Activer le stockage dans votre noyau

La prise en charge du stockage dépend d'un commutateur de fonctionnalité. Vérifiez que le `.csproj` de votre noyau ne la désactive pas (sa valeur par défaut est `true`) :

```xml
<PropertyGroup>
  <CosmosEnableStorage>true</CosmosEnableStorage>
</PropertyGroup>
```

Avec le commutateur activé, le noyau initialise ses pilotes de stockage au démarrage et remet chaque disque qu'ils trouvent à `StorageManager`. Le gestionnaire liste les disques dans `StorageManager.Devices`, lit la table de partitions MBR ou GPT de chacun et liste les partitions qu'il trouve dans `StorageManager.Partitions`, le tout avant que le `BeforeRun()` de votre noyau ne s'exécute. Chaque disque reçoit un nom, celui qu'affichent le journal de démarrage et `IBlockDevice.Name` :

| Disque | Noms | L'attacher dans QEMU avec |
|---|---|---|
| Un disque SATA, derrière un contrôleur AHCI | `sata0`, `sata1`, ... | `cosmos run --disk disk.img` |
| Un namespace NVMe | `nvme0n1`, ... | `cosmos run --disk disk.img,nvme` |
| Un disque virtio-blk | `vblk0`, ... | `cosmos run --disk disk.img,virtio-blk` (ou `virtio-blk-mmio` sur arm64) |
| Une clé USB | `usb0`, `usb1`, ... | `cosmos run --disk disk.img,usb` |
| Un lecteur de CD-ROM, derrière un contrôleur AHCI | `cdrom0`, ... | Rien : sur x64, `cosmos run` démarre depuis un tel lecteur |

Les clés USB demandent aussi `CosmosEnableUsb`, qui vaut `true` par défaut dès que `CosmosEnableStorage` est activé. Le passer à `false` retire les pilotes de contrôleur hôte USB, et les clés avec eux, tandis que les autres disques restent.

Pour donner des disques à votre noyau dans QEMU, créez une image par lecteur et passez chacune à `cosmos run` avec son type, pris dans le tableau ci-dessus (une image sans type est un disque SATA) :

```console
$ qemu-img create disk.img 64M
$ qemu-img create stick.img 64M
$ cosmos run --disk disk.img --disk stick.img,usb
```

Une image tout juste sortie de `qemu-img create` est vierge : elle ne porte ni table de partitions ni système de fichiers, si bien que le noyau liste le disque mais n'y trouve aucune partition. Écrivez une table et formatez une partition depuis votre noyau, comme le montrent [Partitionner un disque](#partition-a-disk) et [Formater une partition](#format-a-partition), avant de la monter.

<a id="which-disk-comes-first"></a>

### Quel disque vient en premier

`StorageManager.PrimaryDevice` est la première entrée de `Devices`, et le gestionnaire tient cette liste dans un ordre fixe plutôt que dans l'ordre d'arrivée des disques, si bien que le périphérique principal ne dépend pas du pilote qui a fini le premier. Les disques trouvés par les pilotes viennent d'abord, classés selon le bus et l'adresse du périphérique sur lequel le [kit de pilotes](drivers.md) les a trouvés, ce qui place un disque SATA ou NVMe (`pci:`) avant une clé USB (`usb:`) et une clé USB avant un disque virtio-blk (`virtio:`). Les disques que votre noyau a enregistrés lui-même avec `StorageManager.RegisterDevice` suivent, dans l'ordre où il les a enregistrés. Les supports en lecture seule comme un CD viennent en dernier, si bien que le périphérique principal est un disque que vous pouvez formater dès que la machine en a un.

Cet ordre a une conséquence à prévoir : un disque qui arrive plus tard prend sa place dans cet ordre, et non la fin de la liste. Une clé USB branchée alors qu'un disque virtio-blk est attaché passe devant lui, et chaque partition du disque virtio-blk recule dans `StorageManager.Partitions`. C'est pourquoi cette page monte une `Partition` plutôt qu'un index dans cette liste.

<a id="disks-that-come-and-go"></a>

### Les disques qui vont et viennent

Les clés USB, ainsi que les disques virtio-blk derrière un emplacement de branchement à chaud PCI Express, peuvent être branchés et retirés pendant que le noyau tourne. Un disque branché est enregistré et analysé comme un disque trouvé au démarrage, sous le plus petit nom `usbN` ou `vblkN` libre.

Un disque retiré quitte `Devices` et `Partitions`, et les montages faits dessus partent avec lui. Un montage fait avec la surcharge `Partition` de `TryMount` (montrée plus bas) est détaché sans être vidé sur le disque, puisque le disque ne peut plus prendre d'écriture : appelez donc `VfsManager.TryUnmount` avant de retirer un disque sur lequel vous avez écrit. Un montage fait à partir d'une chaîne source ne désigne aucun disque, si bien qu'il reste dans la table des montages. Dans les deux cas, un fichier encore ouvert sur le disque échoue à sa lecture ou à son écriture suivante avec l'`IOException` du pilote (`"USB device detached"` pour le pilote de stockage de masse USB, `"virtio-blk device detached"` pour le pilote virtio-blk, `"NVMe device detached"` pour le pilote NVMe), ou avec une `InvalidOperationException` une fois que le kit a libéré la mémoire du pilote.

Quand un disque arrive ou s'en va, un thread du kit de pilotes remplace les deux listes en entier : une liste que vous tenez ne change donc jamais sous vos yeux, mais relire la propriété peut en donner une plus récente. Lisez `Devices` ou `Partitions` une seule fois et indexez cette copie.

Pour essayer dans QEMU, démarrez avec au moins un disque `usb` : le premier donne à la machine un contrôleur xHCI, `usbxhci0`, et le nom du contrôleur est le bus sur lequel vous branchez une autre clé depuis le moniteur QEMU (Ctrl+Alt+2 dans la fenêtre QEMU). `--usb-controller ehci` ou `ehci-ohci` y place plutôt un contrôleur EHCI, `usbehci0`, et `ohci` un contrôleur OHCI, `usbohci0` ; tous deux rattachent chaque périphérique USB à l'un de leurs ports racines :

```console
(qemu) drive_add 0 if=none,id=stick,file=stick.img,format=raw
(qemu) device_add usb-storage,drive=stick,id=stick,bus=usbxhci0.0
(qemu) device_del stick
```

<a id="register-a-filesystem-driver-and-mount-it"></a>

## Enregistrer un pilote de système de fichiers et le monter

Une partition n'est qu'une plage de secteurs tant qu'un pilote de système de fichiers ne la lit pas. Cosmos fournit trois pilotes, chacun dans son espace de noms : FAT (FAT12, FAT16 et FAT32), ext2 et ISO 9660. Votre noyau enregistre ceux qu'il veut sous des noms de son choix, puis monte une partition avec l'un d'eux sur un point de montage.

Voici les `using` sur lesquels s'appuient les extraits ci-dessous :

```csharp
using System.IO;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.System.FileSystem;
using Cosmos.Kernel.System.FileSystem.Fat;
using Cosmos.Kernel.System.Storage;
```

`Cosmos.Kernel.System.FileSystem` est le VFS lui-même : `VfsManager`, les entrées de la table des montages (`VfsMount`), les types d'indicateurs et de métadonnées que nomme un appel de montage ou de stat (`MountFlags`, `VfsMode`, `VfsStat`, `VfsStatFs`), et les interfaces qu'implémente un pilote de système de fichiers. C'est pourquoi `MountFlags.None` ne demande aucun `using` à lui. Les pilotes se trouvent un niveau plus bas : `FatFileSystemType` et `FatFormatOptions` dans `Cosmos.Kernel.System.FileSystem.Fat`, `Ext2FileSystemType` dans `Cosmos.Kernel.System.FileSystem.Ext2`, et `Iso9660FileSystemType` dans `Cosmos.Kernel.System.FileSystem.Iso9660`. `StorageManager` et les types de partition sont dans `Cosmos.Kernel.System.Storage`, et `Cosmos.Kernel.HAL.Devices.Storage` n'est là que pour `IBlockDevice`, le type d'un disque, qu'implémente le disque RAM plus bas et que manipulent les extraits de partitionnement.

Enregistrez le pilote FAT, puis montez la première partition sur `/mnt`. Ceci va dans le `BeforeRun()` de votre noyau :

```csharp
FatFileSystemType fat = new();

if (!VfsManager.RegisterFileSystem("fat", fat))
{
    Console.WriteLine("The name \"fat\" is already registered.");
    return;
}

if (StorageManager.Partitions.Count == 0)
{
    Console.WriteLine("No partitions found.");
    return;
}

if (VfsManager.TryMount("fat", StorageManager.Partitions[0], MountFlags.None, "/mnt", out VfsMount? mount))
{
    Console.WriteLine("Mounted " + mount.Name + " partition " + mount.Source + " at " + mount.MountPoint);
}
else
{
    Console.WriteLine("No FAT volume on the first partition.");
}
```

Chacun de ces appels signale un refus par sa valeur de retour plutôt que par une exception : `RegisterFileSystem` refuse un nom déjà utilisé, et `TryMount` refuse une partition sur laquelle le pilote ne trouve aucun volume. Un disque dont la lecture elle-même échoue lève toujours l'exception de son pilote. `Partitions` est vide quand le stockage est exclu de la compilation, ou quand aucun disque ne porte de table de partitions ni de volume FAT et qu'aucun CD n'est dans un lecteur, c'est pourquoi l'extrait vérifie `Count` avant de l'indexer. Sur x64, `cosmos run` démarre depuis un CD que le noyau liste aussi : quand aucun disque attaché n'a de partition, la première entrée est donc ce CD, et le pilote FAT n'y trouve aucun volume.

`StorageManager.GetPartitions(device)` donne les partitions d'un seul disque, numérotées comme un utilisateur les numérote, alors que `Partitions` est la liste à plat de tous les disques.

`TryMount` accepte aussi la source sous forme de chaîne, comme dans `VfsManager.TryMount("fat", "0", ...)`, que chaque pilote lit à sa façon : le pilote FAT la lit comme un index dans `StorageManager.Partitions`. Préférez la surcharge `Partition`. Un index désigne la partition, quelle qu'elle soit, qui se trouve à cette position, et créer ou supprimer une partition, ou un disque qui arrive ou s'en va, les déplace.

<!-- screenshot: console du noyau juste après le démarrage, avec la ligne "Mounted fat partition 0 at /mnt" -->
![Montage](images/filesystem-mount.png)

À partir de là, tout ce qui se trouve sous `/mnt` est servi par le pilote FAT.

La racine, `/`, n'est pas un volume. Elle existe même quand rien n'est monté, et l'énumérer liste les points de montage, mais y créer un fichier directement échoue avec `IOException` (système de fichiers en lecture seule), puisqu'aucun pilote ne s'y trouve pour stocker le fichier. Créez les fichiers sous un point de montage comme `/mnt`.

<a id="try-it-on-a-ram-disk"></a>

### Essayer sur un disque RAM

Pour des essais rapides, vous n'avez même pas besoin d'image disque. Un périphérique bloc est tout ce qui implémente `IBlockDevice`, et un périphérique adossé à un tableau tient en quelques lignes :

```csharp
internal sealed class MemoryBlockDevice : IBlockDevice
{
    private readonly byte[] _storage;

    public MemoryBlockDevice(string name, ulong blockSize, ulong blockCount)
    {
        Name = name;
        BlockSize = blockSize;
        BlockCount = blockCount;
        _storage = new byte[blockSize * blockCount];
    }

    public string Name { get; }
    public ulong BlockSize { get; }
    public ulong BlockCount { get; }

    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
        => _storage.AsSpan((int)(blockNo * BlockSize), (int)(blockCount * BlockSize)).CopyTo(data);

    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
        => data.Slice(0, (int)(blockCount * BlockSize)).CopyTo(_storage.AsSpan((int)(blockNo * BlockSize)));

    public void Flush() { }
}
```

Le pilote FAT prend un tel périphérique dans son constructeur. Enregistrez-le comme d'habitude et passez une `source` vide, puisqu'un pilote qui tient déjà son périphérique n'a rien à chercher, puis formatez le périphérique et montez-le :

```csharp
MemoryBlockDevice ramDisk = new("RAMDISK", 512, 65536);   // 32 MiB
FatFileSystemType fat = new(ramDisk);

if (!VfsManager.RegisterFileSystem("ramfat", fat)
    || !VfsManager.TryFormat("ramfat", "", new FatFormatOptions { Type = FatType.Fat16 })
    || !VfsManager.TryMount("ramfat", "", MountFlags.None, "/mnt", out _))
{
    Console.WriteLine("RAM disk setup failed.");
    return;
}
```

<a id="read-a-cd-rom-or-an-iso-image"></a>

### Lire un CD-ROM ou une image ISO

Les lecteurs de CD ont presque disparu de nos ordinateurs, mais leur système de fichiers leur a survécu : on télécharge toujours les systèmes d'exploitation sous forme d'images `.iso`, et c'est aussi sous cette forme que `cosmos build` emballe votre propre noyau. `Iso9660FileSystemType`, dans `Cosmos.Kernel.System.FileSystem.Iso9660`, permet à votre noyau de lire un tel volume, qu'il se trouve sur un CD, sur une clé USB sur laquelle une image a été écrite, ou en mémoire.

À lui seul, le standard ne connaît que des noms en majuscules terminés par un numéro de version, comme `HELLO.TXT;1`. C'est pourquoi les images faites sur les systèmes Unix portent en général l'extension Rock Ridge, et celle que fait `cosmos build` la porte : Rock Ridge donne à chaque fichier son vrai nom, `hello.txt`, avec ses permissions Unix, ses horodatages et ses liens symboliques, et le pilote lit tout cela quand le volume l'a. Sur un volume sans Rock Ridge, un fichier apparaît sous son identifiant brut sans la version (`README`, `FILE.TXT`), et vous pouvez taper ce nom dans n'importe quelle casse, car le standard n'y autorise que les majuscules. Le pilote ne lit pas Joliet, les noms longs qu'ajoutent les outils Windows, si bien qu'un volume qui n'a que Joliet présente lui aussi ses identifiants bruts.

Le pilote se monte comme le pilote FAT, depuis une partition ou depuis un périphérique passé à son constructeur, et il accepte n'importe quelle taille de secteur, si bien que la même image se monte depuis un lecteur de CD (secteurs de 2048 octets), depuis une clé USB (secteurs de 512 octets) ou depuis un `MemoryBlockDevice` qui contient ses octets. Ici, `cdrom` est n'importe quel `IBlockDevice` qui contient un volume, et les paragraphes suivants montrent comment trouver le lecteur de CD parmi les disques :

```csharp
using Cosmos.Kernel.System.FileSystem.Iso9660;

Iso9660FileSystemType iso = new(cdrom);

if (!VfsManager.RegisterFileSystem("iso", iso)
    || !VfsManager.TryMount("iso", "", MountFlags.None, "/cd", out VfsMount? cd))
{
    Console.WriteLine("No ISO 9660 volume there.");
    return;
}

Console.WriteLine(cd.IsReadOnly);                       // True
foreach (string path in Directory.GetFiles("/cd/docs"))
{
    Console.WriteLine(path + ": " + new FileInfo(path).Length + " bytes");
}
string text = File.ReadAllText("/cd/hello.txt");
```

Le plus souvent, le volume est dans un lecteur de CD. Quand ce lecteur se trouve derrière un contrôleur SATA en mode AHCI, le noyau le liste dans `StorageManager.Devices` sous le nom `cdrom0`, à condition qu'un disque y ait été présent au démarrage : le pilote n'initialise un lecteur qu'une fois qu'il y trouve un disque, et il ne guette pas un disque inséré plus tard, si bien qu'un lecteur vide au démarrage n'apparaît jamais. Ce qui le distingue d'un disque est une seconde interface sur le même objet, `IRemovableMedia`, avec `IsReadOnly`, `IsRemovable` et `HasMedia`. Le kit de pilotes appelle une telle interface supplémentaire une facette. Celle-ci fait partie de la surface expérimentale du kit, marquée `[Experimental("COSMOS0003")]`, si bien qu'un noyau qui la teste reconnaît l'identifiant dans son `.csproj` avec `<NoWarn>$(NoWarn);COSMOS0003</NoWarn>`. Trouver le lecteur tient alors en une boucle sur la liste :

```csharp
using Cosmos.Kernel.HAL.Devices.Storage;   // IBlockDevice, IRemovableMedia
using Cosmos.Kernel.System.Storage;        // StorageManager

IBlockDevice? cdrom = null;
foreach (IBlockDevice device in StorageManager.Devices)
{
    if (device is IRemovableMedia { IsReadOnly: true })
    {
        cdrom = device;
        break;
    }
}
```

Le gestionnaire de stockage ne cherche pas de table de partitions sur un CD. Un disque en lecture seule ne porte aucune table de partitions qu'un noyau pourrait modifier, et le MBR protecteur au début d'une ISO amorçable compte en unités de 512 octets qu'un lecteur à secteurs de 2048 octets lirait de travers : le gestionnaire liste donc tout le disque comme une seule partition. Il range aussi le lecteur après tous les disques inscriptibles, comme l'explique [Quel disque vient en premier](#which-disk-comes-first), si bien que sur x64, où `cosmos run` démarre depuis un tel lecteur, le CD de démarrage est la dernière entrée de `Partitions`, et la première quand aucun disque n'a de partition à lui.

Le volume est en lecture seule, et tout le VFS le sait. `VfsMount.IsReadOnly` est vrai et `VfsMount.Flags` porte `MountFlags.ReadOnly` quels que soient les indicateurs avec lesquels vous avez monté, `FileInfo.IsReadOnly` est vrai pour chaque fichier, et toute écriture échoue comme sous Linux, avec une simple `IOException` (« Read-only file system ») plutôt qu'avec une exception qui laisserait croire que le fichier est absent.

N'importe quel pilote refuse les écritures de la même façon quand vous le montez avec `MountFlags.ReadOnly`. L'indicateur appartient au montage plutôt qu'au volume : le VFS le vérifie au point de montage, pas dans le pilote.

Quelques éléments d'un disque restent hors de portée. Un fichier réparti sur plusieurs extents, la façon dont ISO 9660 stocke un fichier de plus de 4 Gio, n'est pas listé. Seule la première session d'un disque multisession est lue, et UDF n'est pas lu du tout. Le catalogue de démarrage El Torito, qui rend un CD amorçable, n'est pas interprété non plus : quand le disque le liste, il apparaît comme un fichier de plus.

Le lecteur lui-même doit être un lecteur que le kit publie comme périphérique bloc, ce qui veut dire aujourd'hui un lecteur derrière un contrôleur SATA en mode AHCI ([Les pilotes de stockage](drivers.md#the-storage-drivers)). C'est là que la machine q35 de QEMU place son lecteur de CD, si bien qu'un noyau que `cosmos run` démarre sur x64 peut lire son propre CD de démarrage, et c'est là que VMware le place quand le lecteur virtuel est sur le contrôleur SATA plutôt que sur le contrôleur IDE. Un lecteur derrière un contrôleur USB, virtio-scsi ou IDE n'est pas encore un périphérique bloc, et sur arm64 `cosmos run` attache l'ISO de démarrage par virtio-scsi. Sur cette architecture, un noyau lit plutôt une image : depuis la RAM, depuis une partition de disque dans laquelle l'image a été écrite, ou depuis une clé USB sur laquelle l'image a été écrite, passée entière au constructeur sous la forme `new Iso9660FileSystemType(stick)`.

<a id="ship-files-on-the-boot-iso"></a>

### Livrer des fichiers sur l'ISO de démarrage

Puisqu'un noyau peut lire son CD de démarrage, l'ISO est aussi un endroit pratique pour les fichiers dont il a besoin à l'exécution : un fichier de configuration, une police, une image. `cosmos build` place le noyau et Limine sous `/boot` et `/EFI`, et, comme en Gen2, tout ce qui se trouve dans un dossier `isoFiles` à côté du `.csproj` de votre noyau va à la racine de l'ISO avec ses dossiers, si bien que `isoFiles/data/config.txt` devient `/data/config.txt` sur le disque.

Un fichier qui se trouve ailleurs, ou qui doit arriver sous un autre nom, est un élément `CosmosIsoFile` du `.csproj`. Sa métadonnée `IsoPath` est son chemin sur l'ISO. Un `IsoPath` qui se termine par `/` désigne un dossier et garde le nom propre du fichier, et un fichier qui n'en a pas va à la racine :

```xml
<ItemGroup>
  <CosmosIsoFile Include="../assets/logo.bmp" IsoPath="images/logo.bmp" />
  <CosmosIsoFile Include="../assets/readme.txt" IsoPath="docs/" />
</ItemGroup>
```

La propriété `CosmosIsoFilesDirectory` dirige le build vers un autre dossier. Ce que contient ce dossier reste hors de la compilation, si bien qu'un fichier `.cs` qui s'y trouve est livré comme fichier au lieu d'être compilé dans le noyau.

Le build s'arrête sur une erreur plutôt que d'écrire une ISO cassée. Il refuse un fichier qui atterrirait sous `/boot` ou `/EFI`, puisque ces dossiers contiennent le noyau et le chargeur d'amorçage qu'écrit le SDK. Il refuse un `IsoPath` qui contient un segment `.` ou `..`, puisque `..` pourrait sortir de la racine de l'ISO. Et il refuse deux fichiers au même chemin, comparés sans tenir compte de la casse, puisque sur un hôte Windows la seconde copie écraserait la première.

Le cache de build de l'ISO couvre les fichiers et leurs chemins, si bien qu'ajouter, modifier, renommer ou retirer l'un d'eux reconstruit l'ISO sans recompiler le noyau.

Sur le disque, les fichiers gardent leurs noms, casse et espaces compris, parce que Rock Ridge les porte. Le noyau les relit une fois qu'il a monté le CD de démarrage, trouvé comme dans la section précédente :

```csharp
using Cosmos.Kernel.System.FileSystem;           // VfsManager, MountFlags
using Cosmos.Kernel.System.FileSystem.Iso9660;   // Iso9660FileSystemType

VfsManager.RegisterFileSystem("bootcd", new Iso9660FileSystemType(cdrom));
VfsManager.TryMount("bootcd", "", MountFlags.None, "/cdrom", out _);
string config = File.ReadAllText("/cdrom/data/config.txt");
```

Cette lecture suppose que le lecteur de démarrage soit un périphérique bloc, ce qu'il est derrière un contrôleur AHCI. Là où il ne l'est pas, comme sur une machine arm64 qui démarre depuis virtio-scsi, les fichiers sont sur le disque mais le noyau ne peut pas encore les atteindre.

<a id="partition-a-disk"></a>

## Partitionner un disque

Un disque tout juste sorti de `qemu-img create` ne porte aucune table de partitions, et formater demande une partition à formater. `StorageManager.Partitions` liste celles qui existent déjà sur les disques que le noyau a trouvés au démarrage ; quand il n'y en a aucune, ou que vous voulez une autre organisation, `Gpt`, `Mbr` et `PartitionManager` écrivent la table de partitions elle-même. Tous prennent un `IBlockDevice`, que vous fournit `StorageManager.PrimaryDevice`.

Commencez par demander quel schéma le disque porte déjà :

```csharp
IBlockDevice? disk = StorageManager.PrimaryDevice;
if (disk is null)
{
    Console.WriteLine("No disk");
    return;
}

if (Gpt.IsGpt(disk))
{
    Console.WriteLine("GPT, " + Gpt.Parse(disk).Count + " partition(s)");
}
else if (Mbr.IsMbr(disk))
{
    Console.WriteLine("MBR, " + Mbr.Parse(disk).Count + " partition(s)");
}
else
{
    Console.WriteLine("No partition table");
}
```

`Gpt.Create` et `Mbr.Create` écrivent une table vide de ce schéma, et les partitions que portait le disque disparaissent avec l'ancienne table. `PartitionManager.Create` ajoute ensuite une partition, quel que soit le schéma que porte le disque, si bien que vous n'avez pas à distinguer les cas :

```csharp
Gpt.Create(disk);

/* 64 MiB at LBA 2048, on a 512-byte-sector disk. The MBR system id and the
   GPT type GUID are both given; only the one matching the disk's scheme is
   used. */
if (!PartitionManager.Create(disk, startSector: 2048, sectorCount: 131072,
                             mbrSystemId: 0x0C, gptType: Gpt.BasicDataPartitionType))
{
    Console.WriteLine("Create failed");
    return;
}

StorageManager.RescanPartitions(disk);
```

C'est `RescanPartitions` qui fait apparaître la nouvelle partition dans `StorageManager.Partitions`. Tant que vous ne l'avez pas appelé, la liste décrit encore l'ancienne table.

Les partitions existantes sont désignées par un `PartitionLocation`, le secteur de début et la longueur plutôt qu'un index, si bien qu'une partition garde son identité quand la table est renumérotée :

```csharp
PartitionManager.PartitionLocation where = new(startSector: 2048, sectorCount: 131072);

PartitionManager.Resize(disk, where, newSectorCount: 262144);
PartitionManager.MoveWithData(disk, where, newStartSector: 4096);
PartitionManager.Delete(disk, where);
```

`MoveWithData` copie le contenu vers le nouvel emplacement avant de réécrire l'entrée. `Resize` et `Delete` ne touchent qu'à la table, si bien que réduire une partition sous la taille de son système de fichiers fait perdre des données.

**Tout index de partition est positionnel.** Supprimer une partition renumérote les entrées qui la suivent, et `StorageManager.Partitions` est renumérotée avec elles : relisez donc la liste après chaque modification plutôt que de garder un index d'un côté à l'autre d'une modification.

Le MBR a quatre emplacements primaires, et une chaîne de partitions logiques dans une partition étendue permet d'aller au-delà. `Mbr.TryGetExtendedPartition` trouve la partition étendue, et `PartitionManager.TryCreateLogical` y ajoute une partition logique :

```csharp
if (Mbr.TryGetExtendedPartition(disk, out ulong extendedStart, out ulong extendedCount)
    && PartitionManager.TryCreateLogical(disk, systemId: 0x0C, sectorCount: 65536, out ulong logicalStart))
{
    Console.WriteLine("logical partition at LBA " + logicalStart);
}
```

`Gpt`, `Mbr` et `Ebr` forment la couche inférieure, un type par schéma, pour les entrées que `PartitionManager` n'expose pas.

<a id="format-a-partition"></a>

## Formater une partition

`VfsManager.TryFormat` formate (mkfs) une partition par le VFS. Il prend le nom du pilote, la partition et le type d'options du pilote, et chaque option FAT que vous laissez vide (la taille de cluster, le nombre de copies de la FAT, les secteurs réservés) est choisie par le formateur à partir du type de FAT et de la taille de la partition :

```csharp
FatFormatOptions options = new()
{
    Type = FatType.Fat32,
    VolumeLabel = "COSMOS     ",
};

if (StorageManager.Partitions.Count == 0
    || !VfsManager.TryFormat("fat", StorageManager.Partitions[0], options))
{
    Console.WriteLine("Format failed");
}
```

`TryFormat` refuse une partition montée : démontez-la donc d'abord avec `VfsManager.TryUnmount("/mnt")`.

<a id="list-mounted-volumes"></a>

## Lister les volumes montés

`VfsManager.Mounts` est la table des montages. Chaque entrée donne le nom sous lequel le pilote a été enregistré, la source depuis laquelle il a été monté et le point de montage :

```csharp
foreach (VfsMount m in VfsManager.Mounts)
{
    Console.WriteLine(m.MountPoint + " -> " + m.Name + " (source " + m.Source + ")");
}
```

<!-- screenshot: sortie console de la boucle sur la table des montages, par exemple "/mnt -> fat (source 0)" -->
![Montages](images/filesystem-mounts.png)

`m.Partition` est la partition elle-même, pour un montage fait avec la surcharge `Partition`. Préférez-la à une reconversion de `m.Source` en index : elle continue de désigner la même plage du même disque après qu'une nouvelle analyse a renuméroté `StorageManager.Partitions`.

<a id="check-free-space"></a>

## Vérifier l'espace libre

`VfsManager.TryStatFs` rapporte le décompte des blocs d'un montage. L'espace libre est le nombre de blocs disponibles multiplié par la taille de bloc :

```csharp
if (VfsManager.TryStatFs("/mnt", out VfsStatFs stats))
{
    ulong freeBytes = stats.AvailableBlocks * stats.BlockSize;
    ulong totalBytes = stats.Blocks * stats.BlockSize;
    Console.WriteLine(freeBytes + " of " + totalBytes + " bytes free");
}
```

Un montage ISO 9660 rapporte sa taille sans aucun bloc libre, puisque rien ne peut y être écrit. `DriveInfo` n'est pas encore branché, c'est donc ainsi que l'on lit l'espace libre.

<a id="list-a-directory"></a>

## Lister un répertoire

À partir d'ici, tout est du `System.IO` standard, et les chemins sont ceux qui se trouvent sous votre point de montage. `Directory.GetFiles` liste les fichiers d'un répertoire :

```csharp
foreach (string file in Directory.GetFiles("/mnt"))
{
    Console.WriteLine(file);
}
```

<!-- screenshot: console listant quelques fichiers sous /mnt -->
![Liste des fichiers](images/filesystem-files-list.png)

Les motifs de recherche fonctionnent aussi, puisque c'est le code d'énumération standard de .NET :

```csharp
string[] logs = Directory.GetFiles("/mnt", "*.txt");
```

`Directory.GetDirectories` liste les sous-répertoires de la même façon :

```csharp
foreach (string directory in Directory.GetDirectories("/mnt"))
{
    Console.WriteLine(directory);
}
```

<!-- screenshot: console listant les fichiers et les répertoires sous /mnt -->
![Contenu du répertoire](images/filesystem-directory-listing.png)

<a id="read-a-file"></a>

## Lire un fichier

`File.ReadAllText` lit un fichier entier sous forme de chaîne. Un appel de système de fichiers qui échoue lève l'exception `System.IO` standard, comme en Gen2, et vous l'interceptez comme sur n'importe quelle autre plateforme .NET, si bien que cette boucle affiche chaque fichier sous `/mnt` et signale une lecture qui échoue au lieu d'arrêter le noyau :

```csharp
try
{
    foreach (string file in Directory.GetFiles("/mnt"))
    {
        string content = File.ReadAllText(file);

        Console.WriteLine("File name: " + file);
        Console.WriteLine("File size: " + content.Length);
        Console.WriteLine("Content: " + content);
    }
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

<!-- screenshot: console affichant le nom, la taille et le contenu d'un fichier -->
![Lire tous les fichiers](images/filesystem-read-all-files.png)

`File.ReadAllBytes` lit un fichier sous forme d'octets. Les extraits qui suivent omettent le `try`/`catch` pour rester courts :

```csharp
Console.WriteLine(File.ReadAllText("/mnt/testing.txt"));

byte[] data = File.ReadAllBytes("/mnt/testing.txt");
Console.WriteLine("Read " + data.Length + " bytes");
```

<!-- screenshot: console affichant le contenu du fichier écrit à l'étape précédente -->
![Lire un fichier précis](images/filesystem-read-specific-file.png)

<a id="create-and-write-files"></a>

## Créer et écrire des fichiers

`File.Create` crée un fichier vide, ou vide un fichier existant, et renvoie le flux qu'il a ouvert, que le `using` ferme. `Directory.CreateDirectory` crée un répertoire avec tous les parents manquants, en un seul appel :

```csharp
using (FileStream stream = File.Create("/mnt/testing.txt"))
{
}

Directory.CreateDirectory("/mnt/documents/reports");
```

`File.WriteAllText` remplace le contenu d'un fichier, en créant le fichier s'il n'existe pas encore, et `File.WriteAllBytes` et `File.WriteAllLines` font de même avec des octets et des lignes. `File.AppendAllText` ajoute plutôt à la fin du fichier, et le crée aussi quand il est absent :

```csharp
File.WriteAllText("/mnt/testing.txt", "Learning how to use the Gen3 VFS!");
```

<a id="copy-move-and-delete"></a>

## Copier, déplacer et supprimer

`File.Copy`, `File.Move`, `File.Delete` et `Directory.Delete` fonctionnent comme sur n'importe quelle autre plateforme .NET :

```csharp
File.Copy("/mnt/testing.txt", "/mnt/copy.txt");
File.Move("/mnt/copy.txt", "/mnt/renamed.txt");
File.Delete("/mnt/renamed.txt");

Directory.Delete("/mnt/documents", recursive: true);
```

Contrairement à Gen2, un `File.Move` à l'intérieur d'un même montage est un vrai renommage plutôt qu'une copie suivie d'une suppression. Un déplacement vers un fichier existant lève `IOException` à moins que vous ne passiez `overwrite: true`, et l'écrasement ne perd rien s'il s'arrête à mi-chemin : la destination est mise de côté comme sauvegarde et n'est supprimée qu'une fois le renommage abouti. Un déplacement entre deux montages est une copie suivie d'une suppression, comme sous Linux, puisqu'un renommage ne peut pas passer d'un volume à un autre.

Supprimer un fichier encore ouvert n'échoue pas. Le VFS retient la suppression jusqu'à ce que le dernier handle sur le fichier se ferme, et seulement alors retire l'entrée : le pilote FAT libère les clusters d'un fichier dès que son entrée disparaît, si bien qu'un handle qui les lit encore lirait ce qui y serait écrit ensuite. C'est aussi ce qui fait fonctionner `FileOptions.DeleteOnClose`.

<a id="streams"></a>

## Flux

Toute la pile des flux est disponible, avec le positionnement, la troncature (`SetLength`) et les E/S texte avec tampon :

```csharp
using (FileStream stream = new("/mnt/log.bin", FileMode.Create, FileAccess.ReadWrite))
{
    stream.Write(new byte[] { 1, 2, 3, 4 });
    stream.Seek(0, SeekOrigin.Begin);
    int first = stream.ReadByte();          // 1
}

using (StreamWriter writer = new("/mnt/notes.txt"))
{
    writer.WriteLine("first line");
    writer.WriteLine("second line");
}

using (StreamReader reader = new("/mnt/notes.txt"))
{
    string? line;
    while ((line = reader.ReadLine()) != null)
    {
        Console.WriteLine(line);
    }
}
```

<!-- screenshot: console affichant les deux lignes relues par StreamReader -->
![Flux](images/filesystem-streams.png)

<a id="current-directory-and-relative-paths"></a>

## Répertoire courant et chemins relatifs

Le noyau tient un répertoire courant, qui commence à `/`, si bien que les chemins relatifs et `Path.GetFullPath` se comportent comme sur n'importe quel système Unix :

```csharp
Directory.CreateDirectory("/mnt/work");
Directory.SetCurrentDirectory("/mnt/work");

File.WriteAllText("relative.txt", "resolved against the CWD");
Console.WriteLine(File.Exists("/mnt/work/relative.txt"));      // True
Console.WriteLine(Path.GetFullPath("sub/../file.txt"));        // /mnt/work/file.txt

Directory.SetCurrentDirectory("/");
```

<a id="error-handling"></a>

## Gestion des erreurs

Un échec lève l'exception que la documentation de `System.IO` prévoit pour lui, si bien que vous pouvez intercepter exactement le cas que vous attendez :

| Ce qui a échoué | Exception |
|---|---|
| Ouvrir un fichier qui n'existe pas, dans un répertoire qui existe | `FileNotFoundException` |
| Tout chemin sous un répertoire absent, ou sous un chemin où rien n'est monté | `DirectoryNotFoundException` |
| Créer un fichier directement dans la racine, `/` | `IOException` (système de fichiers en lecture seule) |
| Toute écriture sous un montage en lecture seule : un volume ISO 9660, ou un montage fait avec `MountFlags.ReadOnly` | `IOException` (système de fichiers en lecture seule) |
| Supprimer un répertoire non vide sans `recursive: true` | `IOException` |
| `File.Copy` ou `File.Move` vers un fichier existant sans `overwrite: true` | `IOException` |

Cela vaut même quand rien du tout n'est monté. `Directory.Exists("/")` vaut alors `true` et énumérer `/` donne une liste vide, et tout autre chemin échoue avec l'une des exceptions ci-dessus plutôt que de faire planter le noyau.

<a id="current-limitations"></a>

## Limitations actuelles

Les liens symboliques ne peuvent pas être créés : `File.CreateSymbolicLink` est refusé avec « Operation not permitted », que .NET lève sous forme d'`UnauthorizedAccessException`. Les pilotes ext2 et ISO 9660 suivent bien les liens symboliques que portent déjà leurs volumes, mais la cible d'un lien ne peut pas être relue : `FileSystemInfo.LinkTarget` vaut null.

Les horodatages ne peuvent pas non plus être définis : `File.SetLastWriteTime` et ses semblables sont acceptés et ne changent rien. Leur lecture dépend du volume : les pilotes ext2 et ISO 9660 rapportent ceux que portent leurs volumes, alors que le pilote FAT ne les stocke ni ne les lit, si bien que chaque fichier d'un volume FAT rapporte l'époque Unix, le 1970-01-01.

FAT et ISO 9660 sont les pilotes que Cosmos teste dans un noyau en cours d'exécution. Le pilote ext2, `Ext2FileSystemType`, est livré à côté d'eux mais n'est testé pour l'instant que sur l'hôte. Un lecteur de CD n'est accessible que derrière un contrôleur AHCI, comme le décrit [Lire un CD-ROM ou une image ISO](#read-a-cd-rom-or-an-iso-image). Le gestionnaire de stockage prend au plus huit disques, et ceux qui viennent ensuite ne sont pas listés.

<a id="how-it-works"></a>

## Fonctionnement

Votre code appelle les bibliothèques .NET standard, qui aboutissent à la couche de plateforme Unix, les P/Invoke `Interop.Sys.*` que .NET fait sous Linux. Cosmos remplace ces appels par des [plugs](../dev/plugs.md) dans `Cosmos.Kernel.Plugs`, où une table de descripteurs de fichiers les traduit pour `VfsManager`. `VfsManager` détient les chemins, le répertoire courant, les handles ouverts et la table des montages avec les indicateurs de chaque montage, et c'est là qu'une écriture sous un montage en lecture seule est refusée ; il transmet tout le reste au pilote de système de fichiers monté. Le pilote lit et écrit un `IBlockDevice` : un disque que le kit de pilotes a publié et que `StorageManager` a enregistré, ou un périphérique que votre noyau a construit, comme le disque RAM plus haut. Un pilote de système de fichiers à vous implémente `IVfsFileSystemType` et s'enregistre comme les pilotes fournis. De votre appel jusqu'au disque, les couches s'empilent ainsi :

```
File / Directory / FileStream          (stock .NET)
        │
Interop.Sys.* PAL calls                (stock .NET, plugged)
        │
file descriptor table                  (Cosmos.Kernel.Plugs: fds, dir streams, errno)
        │
VfsManager                             (mounts, paths, CWD, open handles)
        │
IVfsFileSystemType / IVfsSuperblock    (the FAT, ext2 and ISO 9660 drivers)
        │
IBlockDevice                           (the kit's AHCI, NVMe, virtio-blk and USB mass storage drivers, MemoryBlockDevice)
```

<a id="summary"></a>

## Résumé

| Tâche | Appel |
|---|---|
| Lister les disques | `StorageManager.Devices` |
| Lister les partitions | `StorageManager.Partitions`, `StorageManager.GetPartitions(device)` |
| Enregistrer un pilote de système de fichiers | `VfsManager.RegisterFileSystem(name, driver)` |
| Monter une partition | `VfsManager.TryMount(name, partition, flags, mountPoint, out mount)` |
| Monter un périphérique que tient le pilote | `VfsManager.TryMount(name, "", flags, mountPoint, out mount)` |
| Démonter | `VfsManager.TryUnmount(mountPoint)` |
| Écrire une table de partitions | `Gpt.Create(disk)`, `Mbr.Create(disk)` |
| Ajouter, redimensionner, déplacer ou supprimer une partition | `PartitionManager.Create`, `Resize`, `MoveWithData`, `Delete` |
| Prendre en compte une nouvelle table de partitions | `StorageManager.RescanPartitions(disk)` |
| Formater une partition | `VfsManager.TryFormat(name, partition, options)` |
| Lister les montages | `VfsManager.Mounts` |
| Lire l'espace libre | `VfsManager.TryStatFs(mountPoint, out stats)` |
| Manipuler des fichiers | `File`, `Directory`, `FileStream`, `StreamReader`, `StreamWriter` |
