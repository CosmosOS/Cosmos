# Visual Basic kernels

In this article, we will discuss writing a Cosmos Gen3 kernel in Visual Basic: creating the project, the kernel class, the members that take a span, and the classes that are written in C#.

A Visual Basic kernel is built like a C# one. Its `.vbproj` uses the same `Cosmos.Sdk` and takes the same feature switches, `cosmos build` turns it into a bootable ISO for x64 or ARM64, and `cosmos run` boots that ISO in QEMU. What differs comes from the language: Visual Basic cannot declare a `Span(Of T)` or a `ReadOnlySpan(Of T)`. A Visual Basic kernel still calls the members that take one, but the few classes whose own members take one are written in C#.

| | C# | Visual Basic |
|---|---|---|
| Created with | `cosmos new MyKernel` | `cosmos new MyKernel -l vb` |
| Project file | `MyKernel.csproj` | `MyKernel.vbproj` |
| Kernel class in | `Kernel.cs` | `Kernel.vb` |
| Calls a member that takes a span | Yes | Yes, with a `String` or an array |
| Declares a member that takes or returns a span | Yes | No, a build error (`BC30668`) |

If you find bugs or something abnormal, please [submit an issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) on our repository.

---

## Creating the kernel

`cosmos new` creates a kernel project in a folder named after it. Its `-l` option picks the language, `csharp` (the default) or `vb`, and the commands that build and run the kernel are the same in both:

```bash
cosmos new MyKernel -l vb
cd MyKernel
cosmos build
cosmos run
```

The Cosmos extensions for VS Code, Visual Studio and Rider ([IDE Extensions](install.md#ide-extensions)) ask for the language when they create a kernel.

The folder holds what a C# kernel's folder holds, with `MyKernel.vbproj` and `Kernel.vb` in place of `MyKernel.csproj` and `Kernel.cs`. A property that another article of this guide sets in the `.csproj`, a feature switch for instance, goes in the `.vbproj` and is written the same way. This one leaves the network stack out of the kernel:

```xml
<PropertyGroup>
  <CosmosEnableNetwork>false</CosmosEnableNetwork>
</PropertyGroup>
```

---

## The kernel class

`Kernel.vb` holds the kernel class. It derives from `Cosmos.Kernel.System.Kernel` and overrides `BeforeRun` and `Run`, as the C# kernel of [Kernel Startup](startup.md#a-minimal-kernel) does:

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

The class is declared outside any `Namespace` block, and that is what lets the SDK find it. The SDK looks for the kernel class under the name `<RootNamespace>.Kernel`, and Visual Basic places a class declared outside any block in the project's root namespace, which is the project's name: here the class is `MyKernel.Kernel`. A `Namespace` block is nested under the root namespace, so a class declared inside `Namespace Boot` is `MyKernel.Boot.Kernel`, and the build stops on `BC30002: Type 'Global.MyKernel.Kernel' is not defined` until the `.vbproj` names the class:

```xml
<PropertyGroup>
  <CosmosKernelClass>MyKernel.Boot.Kernel</CosmosKernelClass>
</PropertyGroup>
```

The SDK also turns `Option Strict` on. A late-bound call, one made on an `Object` whose member Visual Basic looks up when the kernel runs, goes through reflection, which a kernel compiled ahead of time cannot count on, so the compiler refuses it at build time (`BC30574`).

---

## Members that take a span

Some members of the Cosmos API, and of .NET, take a span: one overload of `VfsManager.TryMount` takes the mount source as a `ReadOnlySpan(Of Char)`, and `IBlockDevice.ReadBlock` fills a `Span(Of Byte)`. Call them with a `String` or an array and the compiler converts the argument, as in these two calls of the kernel at the end of this page, where `""` is the mount source and `sector` a `Byte` array:

```vb
VfsManager.TryMount("ramfat", "", MountFlags.None, "/mnt", mount)
ramDisk.ReadBlock(0, 1, sector)
```

What Visual Basic cannot do is name the type. `Span(Of T)` and `ReadOnlySpan(Of T)` are ref structs, a kind of type the language does not support, and a parameter, a return type, a field or a local declared as one is a build error:

```
error BC30668: 'Span(Of Byte)' is obsolete: 'Types with embedded references are not supported in this version of your compiler.'.
```

---

## Classes written in C#

Overriding or implementing a member means declaring its parameters, so a class cannot be written in Visual Basic when a member it must override or implement takes or returns a span. Among the classes this guide has you write, three are in that case:

| To write | The class declares | Described in |
|---|---|---|
| A disk of your own, such as a RAM disk | `IBlockDevice.ReadBlock` and `IBlockDevice.WriteBlock` | [Try it on a RAM disk](filesystem.md#try-it-on-a-ram-disk) |
| A sound computed as it is read | `AudioStream.Read` | [Generating sound](audio.md#generating-sound) |
| A driver | `Driver.Matches` | [Writing a Driver](drivers.md) |

The interfaces a filesystem driver implements are in that case too, with members such as `IFileOperations.Read`.

Write such a class in a C# class library and reference the library from the kernel. For the RAM disk, run these three commands in the folder that holds `MyKernel`:

```bash
dotnet new classlib -lang "C#" -o MyKernel.Storage
dotnet add MyKernel.Storage package Cosmos.Kernel.HAL --version <version>
dotnet add MyKernel reference MyKernel.Storage
```

`<version>` is the version `MyKernel.vbproj` names in its `Sdk="Cosmos.Sdk/..."` attribute, so that the library and the kernel are compiled against the same Cosmos release. `Cosmos.Kernel.HAL` is the package that holds `IBlockDevice`; a class deriving from `AudioStream` needs `Cosmos.Kernel.System` instead, and a driver library is set up as [A driver library](drivers.md#a-driver-library) describes, with the `NoWarn` line that acknowledges the experimental driver kit (`COSMOS0003`). Once a library sits beside the kernel, run `cosmos update` from the folder that holds both, where it finds the version of each project and moves the two together.

Replace the `Class1.cs` the template created with `MemoryBlockDevice.cs`. It is the class of [Try it on a RAM disk](filesystem.md#try-it-on-a-ram-disk), declared `public` so that the kernel sees it:

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

The kernel creates the disk, formats it, mounts it and uses it through `System.IO`, passing a `String` or an array wherever a member takes a span:

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

The kernel prints the content of the file it wrote, then the two bytes that end a FAT boot sector, read straight from the disk:

```
Hello from Visual Basic
Boot sector ends with 55 AA
```

---

## Summary

| Task | Call |
|---|---|
| Create a Visual Basic kernel | `cosmos new MyKernel -l vb` |
| Build and run it | `cosmos build`, `cosmos run` |
| Set a feature switch | `<CosmosEnableNetwork>false</CosmosEnableNetwork>` in the `.vbproj` |
| Declare the kernel class inside a `Namespace` block | `<CosmosKernelClass>MyKernel.Boot.Kernel</CosmosKernelClass>` in the `.vbproj` |
| Pass text to a `ReadOnlySpan(Of Char)` parameter | A `String` |
| Pass a buffer to a `Span(Of Byte)` parameter | A `Byte` array |
| Write a class that declares a span | A C# class library the kernel references |
