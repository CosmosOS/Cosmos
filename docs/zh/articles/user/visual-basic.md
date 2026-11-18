---
source_hash: c72d51aa7dc241de295bff490bee80e7c768e10d
---
<a id="visual-basic-kernels"></a>

# Visual Basic 内核

本文将介绍如何用 Visual Basic 编写 Cosmos Gen3 内核：创建项目、内核类、接受 span 的成员，以及需要用 C# 编写的类。

Visual Basic 内核的构建方式与 C# 内核相同。它的 `.vbproj` 使用同一个 `Cosmos.Sdk`，支持同样的功能开关，`cosmos build` 把它构建成面向 x64 或 ARM64 的可启动 ISO，`cosmos run` 则在 QEMU 中启动这个 ISO。区别来自语言本身：Visual Basic 无法声明 `Span(Of T)` 或 `ReadOnlySpan(Of T)`。Visual Basic 内核仍然可以调用接受 span 的成员，但少数自身必须声明 span 的类需要用 C# 编写。

| | C# | Visual Basic |
|---|---|---|
| 创建命令 | `cosmos new MyKernel` | `cosmos new MyKernel -l vb` |
| 项目文件 | `MyKernel.csproj` | `MyKernel.vbproj` |
| 内核类所在文件 | `Kernel.cs` | `Kernel.vb` |
| 调用接受 span 的成员 | 可以 | 可以，传入 `String` 或数组 |
| 声明接受或返回 span 的成员 | 可以 | 不可以，构建错误（`BC30668`） |

如果你发现 bug 或任何异常，请在我们的仓库中[提交 issue](https://github.com/CosmosOS/Cosmos/issues/new/choose)。

---

<a id="creating-the-kernel"></a>

## 创建内核

`cosmos new` 会在一个以项目名命名的文件夹中创建内核项目。它的 `-l` 选项用来选择语言：`csharp`（默认）或 `vb`，而构建和运行内核的命令在两种语言下完全相同：

```bash
cosmos new MyKernel -l vb
cd MyKernel
cosmos build
cosmos run
```

VS Code、Visual Studio 和 Rider 的 Cosmos 扩展（[IDE 扩展](install.md#ide-extensions)）在创建内核时会询问使用哪种语言。

这个文件夹里的内容与 C# 内核的文件夹相同，只是 `MyKernel.csproj` 和 `Kernel.cs` 换成了 `MyKernel.vbproj` 和 `Kernel.vb`。本指南其他文章要求写进 `.csproj` 的属性（例如功能开关），在这里写进 `.vbproj`，写法不变。下面这个属性会把网络栈排除在内核之外：

```xml
<PropertyGroup>
  <CosmosEnableNetwork>false</CosmosEnableNetwork>
</PropertyGroup>
```

---

<a id="the-kernel-class"></a>

## 内核类

`Kernel.vb` 中存放内核类。它派生自 `Cosmos.Kernel.System.Kernel`，并重写 `BeforeRun` 和 `Run`，与[内核启动](startup.md#a-minimal-kernel)中的 C# 内核一样：

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

这个类声明在所有 `Namespace` 块之外，SDK 正是因此才能找到它。SDK 按 `<RootNamespace>.Kernel` 这个名称查找内核类，而 Visual Basic 会把声明在所有块之外的类放进项目的根命名空间，也就是项目名：这里的类就是 `MyKernel.Kernel`。`Namespace` 块会嵌套在根命名空间之下，因此声明在 `Namespace Boot` 中的类是 `MyKernel.Boot.Kernel`，此时构建会停在 `BC30002: Type 'Global.MyKernel.Kernel' is not defined`，直到 `.vbproj` 指明这个类：

```xml
<PropertyGroup>
  <CosmosKernelClass>MyKernel.Boot.Kernel</CosmosKernelClass>
</PropertyGroup>
```

SDK 还会打开 `Option Strict`。后期绑定调用是指在 `Object` 上发起、由 Visual Basic 等到内核运行时才查找成员的调用，它依赖反射，而预先编译的内核无法指望反射，因此编译器在构建时就拒绝它（`BC30574`）。

---

<a id="members-that-take-a-span"></a>

## 接受 span 的成员

Cosmos API 和 .NET 中都有一些成员接受 span：`VfsManager.TryMount` 的一个重载以 `ReadOnlySpan(Of Char)` 接收挂载源，`IBlockDevice.ReadBlock` 则填充一个 `Span(Of Byte)`。调用它们时传入 `String` 或数组即可，编译器会转换参数，就像本页末尾那个内核中的这两个调用，其中 `""` 是挂载源，`sector` 是一个 `Byte` 数组：

```vb
VfsManager.TryMount("ramfat", "", MountFlags.None, "/mnt", mount)
ramDisk.ReadBlock(0, 1, sector)
```

Visual Basic 做不到的是写出这个类型的名称。`Span(Of T)` 和 `ReadOnlySpan(Of T)` 是 ref struct，这类类型不受该语言支持，把参数、返回类型、字段或局部变量声明为其中之一都会导致构建错误：

```
error BC30668: 'Span(Of Byte)' is obsolete: 'Types with embedded references are not supported in this version of your compiler.'.
```

---

<a id="classes-written-in-c"></a>

## 用 C# 编写的类

重写或实现一个成员就意味着要声明它的参数，因此，只要一个类必须重写或实现的成员接受或返回 span，这个类就无法用 Visual Basic 编写。在本指南让你编写的类当中，有三个属于这种情况：

| 要编写的内容 | 类需要声明 | 说明见 |
|---|---|---|
| 你自己的磁盘，例如 RAM 磁盘 | `IBlockDevice.ReadBlock` 和 `IBlockDevice.WriteBlock` | [在 RAM 磁盘上尝试](filesystem.md#try-it-on-a-ram-disk) |
| 在读取时即时计算的声音 | `AudioStream.Read` | [生成声音](audio.md#generating-sound) |
| 驱动程序 | `Driver.Matches` | [编写驱动程序](drivers.md) |

文件系统驱动程序所实现的接口也属于这种情况，例如其中的 `IFileOperations.Read`。

请把这样的类写在一个 C# 类库中，并让内核引用该类库。以 RAM 磁盘为例，在包含 `MyKernel` 的文件夹中运行这三条命令：

```bash
dotnet new classlib -lang "C#" -o MyKernel.Storage
dotnet add MyKernel.Storage package Cosmos.Kernel.HAL --version <version>
dotnet add MyKernel reference MyKernel.Storage
```

`<version>` 是 `MyKernel.vbproj` 在 `Sdk="Cosmos.Sdk/..."` 属性中写明的版本，这样类库和内核就是针对同一个 Cosmos 版本编译的。`Cosmos.Kernel.HAL` 是包含 `IBlockDevice` 的包；派生自 `AudioStream` 的类则需要改用 `Cosmos.Kernel.System`，而驱动程序库按[驱动程序库](drivers.md#a-driver-library)所述搭建，其中包括项目用来确认实验性驱动工具包（`COSMOS0003`）的 `NoWarn` 行。内核旁边有了类库之后，请在同时包含两者的文件夹中运行 `cosmos update`：它会在那里找到每个项目的版本，并把两者一起更新。

把模板创建的 `Class1.cs` 换成 `MemoryBlockDevice.cs`。它就是[在 RAM 磁盘上尝试](filesystem.md#try-it-on-a-ram-disk)中的那个类，只是声明为 `public`，以便内核能看到它：

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

内核创建这个磁盘，将其格式化并挂载，再通过 `System.IO` 使用它，凡是成员接受 span 的地方都传入 `String` 或数组：

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

内核会打印它所写入文件的内容，然后是直接从磁盘读出的、位于 FAT 引导扇区末尾的两个字节：

```
Hello from Visual Basic
Boot sector ends with 55 AA
```

---

<a id="summary"></a>

## 总结

| 任务 | 调用 |
|---|---|
| 创建 Visual Basic 内核 | `cosmos new MyKernel -l vb` |
| 构建并运行它 | `cosmos build`、`cosmos run` |
| 设置功能开关 | 在 `.vbproj` 中写 `<CosmosEnableNetwork>false</CosmosEnableNetwork>` |
| 在 `Namespace` 块中声明内核类 | 在 `.vbproj` 中写 `<CosmosKernelClass>MyKernel.Boot.Kernel</CosmosKernelClass>` |
| 向 `ReadOnlySpan(Of Char)` 参数传入文本 | 一个 `String` |
| 向 `Span(Of Byte)` 参数传入缓冲区 | 一个 `Byte` 数组 |
| 编写声明了 span 的类 | 内核所引用的 C# 类库 |
