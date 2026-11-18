---
source_hash: c72d51aa7dc241de295bff490bee80e7c768e10d
---
<a id="visual-basic-kernels"></a>

# Noyaux en Visual Basic

Dans cet article, nous verrons comment écrire un noyau Cosmos Gen3 en Visual Basic : créer le projet, la classe du noyau, les membres qui prennent un span, et les classes qui s'écrivent en C#.

Un noyau Visual Basic se construit comme un noyau C#. Son `.vbproj` utilise le même `Cosmos.Sdk` et accepte les mêmes commutateurs de fonctionnalité, `cosmos build` en fait une ISO amorçable pour x64 ou ARM64, et `cosmos run` démarre cette ISO dans QEMU. La différence vient du langage : Visual Basic ne peut pas déclarer un `Span(Of T)` ni un `ReadOnlySpan(Of T)`. Un noyau Visual Basic appelle quand même les membres qui en prennent un, mais les quelques classes qui doivent elles-mêmes en déclarer un s'écrivent en C#.

| | C# | Visual Basic |
|---|---|---|
| Création | `cosmos new MyKernel` | `cosmos new MyKernel -l vb` |
| Fichier projet | `MyKernel.csproj` | `MyKernel.vbproj` |
| Classe du noyau dans | `Kernel.cs` | `Kernel.vb` |
| Appelle un membre qui prend un span | Oui | Oui, avec une `String` ou un tableau |
| Déclare un membre qui prend ou renvoie un span | Oui | Non, erreur de build (`BC30668`) |

Si vous trouvez des bugs ou quelque chose d'anormal, merci de [soumettre une issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) sur notre dépôt.

---

<a id="creating-the-kernel"></a>

## Créer le noyau

`cosmos new` crée un projet de noyau dans un dossier qui porte son nom. Son option `-l` choisit le langage, `csharp` (par défaut) ou `vb`, et les commandes qui compilent et lancent le noyau sont les mêmes dans les deux cas :

```bash
cosmos new MyKernel -l vb
cd MyKernel
cosmos build
cosmos run
```

Les extensions Cosmos pour VS Code, Visual Studio et Rider ([Extensions pour IDE](install.md#ide-extensions)) demandent le langage quand elles créent un noyau.

Le dossier contient ce que contient celui d'un noyau C#, avec `MyKernel.vbproj` et `Kernel.vb` à la place de `MyKernel.csproj` et `Kernel.cs`. Une propriété qu'un autre article de ce guide place dans le `.csproj`, un commutateur de fonctionnalité par exemple, va dans le `.vbproj` et s'écrit de la même façon. Celle-ci retire la pile réseau du noyau :

```xml
<PropertyGroup>
  <CosmosEnableNetwork>false</CosmosEnableNetwork>
</PropertyGroup>
```

---

<a id="the-kernel-class"></a>

## La classe du noyau

`Kernel.vb` contient la classe du noyau. Elle dérive de `Cosmos.Kernel.System.Kernel` et redéfinit `BeforeRun` et `Run`, comme le noyau C# de [Démarrage du noyau](startup.md#a-minimal-kernel) :

```vb
Imports Sys = Cosmos.Kernel.System

Public Class Kernel
    Inherits Sys.Kernel

    Protected Overrides Sub BeforeRun()
        Console.WriteLine("Cosmos booted successfully!")
    End Sub

    Protected Overrides Sub Run()
        Console.Write("Input: ")
        Dim input As String = Console.ReadLine()
        Console.Write("Text typed: ")
        Console.WriteLine(input)
    End Sub
End Class
```

La classe est déclarée hors de tout bloc `Namespace`, et c'est ce qui permet au SDK de la trouver. Le SDK cherche la classe du noyau sous le nom `<RootNamespace>.Kernel`, et Visual Basic range une classe déclarée hors de tout bloc dans l'espace de noms racine du projet, qui est le nom du projet : ici, la classe s'appelle `MyKernel.Kernel`. Un bloc `Namespace` s'imbrique sous l'espace de noms racine : une classe déclarée dans `Namespace Boot` s'appelle donc `MyKernel.Boot.Kernel`, et le build s'arrête sur `BC30002: Type 'Global.MyKernel.Kernel' is not defined` tant que le `.vbproj` ne nomme pas la classe :

```xml
<PropertyGroup>
  <CosmosKernelClass>MyKernel.Boot.Kernel</CosmosKernelClass>
</PropertyGroup>
```

Le SDK active aussi `Option Strict`. Un appel à liaison tardive, c'est-à-dire fait sur un `Object` dont Visual Basic ne cherche le membre qu'une fois le noyau lancé, passe par la réflexion, sur laquelle un noyau compilé à l'avance ne peut pas compter : le compilateur le refuse donc dès le build (`BC30574`).

---

<a id="members-that-take-a-span"></a>

## Les membres qui prennent un span

Certains membres de l'API de Cosmos, et de .NET, prennent un span : une surcharge de `VfsManager.TryMount` prend la source du montage sous forme de `ReadOnlySpan(Of Char)`, et `IBlockDevice.ReadBlock` remplit un `Span(Of Byte)`. Appelez-les avec une `String` ou un tableau et le compilateur convertit l'argument, comme dans ces deux appels du noyau présenté à la fin de cette page, où `""` est la source du montage et `sector` un tableau de `Byte` :

```vb
VfsManager.TryMount("ramfat", "", MountFlags.None, "/mnt", mount)
ramDisk.ReadBlock(0, 1, sector)
```

Ce que Visual Basic ne sait pas faire, c'est nommer le type. `Span(Of T)` et `ReadOnlySpan(Of T)` sont des ref structs, une sorte de type que le langage ne prend pas en charge, et un paramètre, un type de retour, un champ ou une variable locale déclarés avec l'un d'eux provoquent une erreur de build :

```
error BC30668: 'Span(Of Byte)' is obsolete: 'Types with embedded references are not supported in this version of your compiler.'.
```

---

<a id="classes-written-in-c"></a>

## Les classes écrites en C#

Redéfinir ou implémenter un membre, c'est déclarer ses paramètres : une classe ne peut donc pas être écrite en Visual Basic quand un membre qu'elle doit redéfinir ou implémenter prend ou renvoie un span. Parmi les classes que ce guide vous fait écrire, trois sont dans ce cas :

| À écrire | La classe déclare | Décrit dans |
|---|---|---|
| Un disque à vous, un disque RAM par exemple | `IBlockDevice.ReadBlock` et `IBlockDevice.WriteBlock` | [Essayer sur un disque RAM](filesystem.md#try-it-on-a-ram-disk) |
| Un son calculé au fur et à mesure de sa lecture | `AudioStream.Read` | [Générer du son](audio.md#generating-sound) |
| Un pilote | `Driver.Matches` | [Écrire un pilote](drivers.md) |

Les interfaces qu'implémente un pilote de système de fichiers sont dans le même cas, avec des membres comme `IFileOperations.Read`.

Écrivez une telle classe dans une bibliothèque de classes C# et référencez la bibliothèque depuis le noyau. Pour le disque RAM, lancez ces trois commandes dans le dossier qui contient `MyKernel` :

```bash
dotnet new classlib -lang "C#" -o MyKernel.Storage
dotnet add MyKernel.Storage package Cosmos.Kernel.HAL --version <version>
dotnet add MyKernel reference MyKernel.Storage
```

`<version>` est la version que `MyKernel.vbproj` indique dans son attribut `Sdk="Cosmos.Sdk/..."`, pour que la bibliothèque et le noyau soient compilés avec la même version de Cosmos. `Cosmos.Kernel.HAL` est le paquet qui contient `IBlockDevice` ; une classe dérivée de `AudioStream` a besoin de `Cosmos.Kernel.System` à la place, et une bibliothèque de pilotes se met en place comme le décrit [Une bibliothèque de pilotes](drivers.md#a-driver-library), avec la ligne `NoWarn` par laquelle le projet accepte le kit de pilotes expérimental (`COSMOS0003`). Une fois la bibliothèque posée à côté du noyau, lancez `cosmos update` depuis le dossier qui contient les deux : il y trouve la version de chaque projet et fait avancer les deux ensemble.

Remplacez le `Class1.cs` que le modèle a créé par `MemoryBlockDevice.cs`. C'est la classe de la section [Essayer sur un disque RAM](filesystem.md#try-it-on-a-ram-disk), déclarée `public` pour que le noyau la voie :

```csharp
using Cosmos.Kernel.HAL.Devices.Storage;

namespace MyKernel.Storage;

public sealed class MemoryBlockDevice : IBlockDevice
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

Le noyau crée le disque, le formate, le monte et s'en sert à travers `System.IO`, en passant une `String` ou un tableau partout où un membre prend un span :

```vb
Imports System.IO
Imports Cosmos.Kernel.System.FileSystem
Imports Cosmos.Kernel.System.FileSystem.Fat
Imports MyKernel.Storage
Imports Sys = Cosmos.Kernel.System

Public Class Kernel
    Inherits Sys.Kernel

    Protected Overrides Sub BeforeRun()
        Dim ramDisk As New MemoryBlockDevice("RAMDISK", 512, 65536)
        Dim fat As New FatFileSystemType(ramDisk)
        Dim options As New FatFormatOptions With {.Type = FatType.Fat16}
        Dim mount As VfsMount = Nothing

        If Not VfsManager.RegisterFileSystem("ramfat", fat) OrElse
           Not VfsManager.TryFormat("ramfat", "", options) OrElse
           Not VfsManager.TryMount("ramfat", "", MountFlags.None, "/mnt", mount) Then
            Console.WriteLine("RAM disk setup failed.")
            Return
        End If

        File.WriteAllText("/mnt/hello.txt", "Hello from Visual Basic")
        Console.WriteLine(File.ReadAllText("/mnt/hello.txt"))

        Dim sector(511) As Byte
        ramDisk.ReadBlock(0, 1, sector)
        Console.WriteLine("Boot sector ends with " & sector(510).ToString("X2") & " " & sector(511).ToString("X2"))
    End Sub

    Protected Overrides Sub Run()
        Sys.Power.Halt()
    End Sub
End Class
```

Le noyau affiche le contenu du fichier qu'il a écrit, puis les deux octets qui terminent un secteur de démarrage FAT, lus directement sur le disque :

```
Hello from Visual Basic
Boot sector ends with 55 AA
```

---

<a id="summary"></a>

## Résumé

| Tâche | Appel |
|---|---|
| Créer un noyau Visual Basic | `cosmos new MyKernel -l vb` |
| Le compiler et le lancer | `cosmos build`, `cosmos run` |
| Régler un commutateur de fonctionnalité | `<CosmosEnableNetwork>false</CosmosEnableNetwork>` dans le `.vbproj` |
| Déclarer la classe du noyau dans un bloc `Namespace` | `<CosmosKernelClass>MyKernel.Boot.Kernel</CosmosKernelClass>` dans le `.vbproj` |
| Passer du texte à un paramètre `ReadOnlySpan(Of Char)` | Une `String` |
| Passer un tampon à un paramètre `Span(Of Byte)` | Un tableau de `Byte` |
| Écrire une classe qui déclare un span | Une bibliothèque de classes C# que le noyau référence |
