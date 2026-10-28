---
source_hash: 6032e53537c9e5361c2d34a5d4d69910ee4ab1fd
---
<a id="file-system"></a>

# 文件系统

本文将介绍如何使用 Cosmos Gen3 的 VFS（虚拟文件系统）。
在 Gen2 中，你要通过 `CosmosVFS` 和一个经 plug 实现的 `System.IO` 子集来操作文件；Gen3 则为你提供**标准的 .NET `System.IO` API**（`File`、`Directory`、`FileStream`、`StreamReader`/`StreamWriter`、`FileInfo`/`DirectoryInfo`），它不加修改地运行在内核的 VFS 之上。你把文件系统挂载到 Unix 风格的挂载点上，然后使用普通的绝对路径。

如果你是从 Gen2 迁移过来的，主要区别如下：

| | Gen2 | Gen3 |
|---|---|---|
| 路径 | DOS 驱动器号（`0:\file.txt`） | Unix 路径（`/mnt/file.txt`） |
| 初始化 | `CosmosVFS` + `VFSManager.RegisterVFS` | `VfsManager.RegisterFileSystem` + `VfsManager.TryMount` |
| API 面 | 经 plug 实现的 `System.IO` 子集 | 完整的 `System.IO`（流、枚举模式、`FileInfo` 等） |
| `File.Move` | 没有 plug（只能先复制再删除） | 可用，包括覆盖已有目标的语义 |
| 文件系统 | FAT32（FAT12/16 部分支持） | FAT12/16/32 |

**注意**：如果你打算在 Cosmos 中使用某个驱动器，请**始终**用 Cosmos 格式化它，并且**只**用 Cosmos。Parted 或 FDisk 等工具要高级得多，它们对磁盘的布局可能与 Cosmos 预期的不同。

**警告**：请**不要**在真实硬件上尝试！这可能对你的数据造成**无法挽回的损害**。请使用虚拟机（通过 `cosmos run` 运行的 QEMU、VMware、VirtualBox 等）。

<a id="enable-storage-in-your-kernel"></a>

## 在内核中启用存储

存储支持由一个功能开关控制。请确保内核的 `.csproj` 没有将其关闭（默认值为 `true`）：

```xml
<PropertyGroup>
  <CosmosEnableStorage>true</CosmosEnableStorage>
</PropertyGroup>
```

启动时，内核会初始化 `StorageManager`，后者会安装自己的消费者，用来接收[驱动工具包](drivers.md)的块设备。随后，工具包中的 AHCI、NVMe、virtio-blk 和 USB 大容量存储驱动程序会在驱动阶段发布它们找到的每个 SATA 磁盘（`sata0`、`sata1`……）、每个 NVMe 命名空间（`nvme0n1`……）、每个 virtio-blk 磁盘（`vblk0`……）和每个 U 盘（`usb0`、`usb1`……）；管理器在每个设备发布时将其注册，并在发布返回之前扫描它的 MBR/GPT 分区表，把分区加入 `StorageManager.Partitions`。U 盘需要 `CosmosEnableUsb`，只要 `CosmosEnableStorage` 开启，它默认也是开启的；将其设为 `false` 会保留 AHCI 和 NVMe 磁盘，而 USB 磁盘会连同主机控制器驱动程序一起被去掉。管理器的各个表采用同一种顺序，这个顺序决定了主设备：在 `Devices` 中、在 `Partitions` 中以及对 `PrimaryDevice` 而言，首先按节点路径排序，`pci:` 在 `usb:` 之前，`usb:` 在 `virtio:` 之前；内核自己注册的磁盘排在所有工具包磁盘之后；最后按注册顺序。因此，无论谁先注册，PCI 控制器上的内部磁盘相对于启动时已插入的 U 盘都是主设备；而在 U 盘之后到达的工具包磁盘会排到它前面，并使 `Partitions` 重新编号，这就是为什么按 `Partition` 挂载（见下文）优于按索引字符串挂载。

USB 磁盘，以及位于 PCI Express 热插拔插槽后面的 virtio-blk 磁盘，也可以在内核运行时插入和拔出。插入的磁盘由工具包发布，并像启动时找到的磁盘一样被注册和扫描，名称取空闲的编号最小的 `usbN` 或 `vblkN`。拔出的磁盘会被撤下：它离开 `StorageManager.Devices` 和 `StorageManager.Partitions`，通过 `TryMount` 的 `Partition` 重载（见下文）在其分区上建立的挂载会被分离，仍在其上打开的文件会以 `IOException` 失败（USB 大容量存储驱动程序抛出 `IOException("USB device detached")`，virtio-blk 驱动程序抛出 `IOException("virtio-blk device detached")`，正如 NVMe 驱动程序在分离过程中抛出 `IOException`）；如果工具包已经释放了某个磁盘的驱动程序内存，该磁盘则会以工具包的 `InvalidOperationException` 失败，因为受支持层并不包装驱动程序的设备。由源字符串建立的挂载并不指向任何磁盘，因此它会保留下来，其 I/O 也会以同样的方式失败。分离的挂载不会被刷新，因为磁盘已经不在了，所以请在拔出磁盘之前调用 `VfsManager.TryUnmount`。当 USB 或 virtio-blk 磁盘接入或移除时，这两个列表都可能在两次读取之间被工具包的工作线程改变：请只读取一次 `Devices` 或 `Partitions`，然后对这份副本进行索引。

要在 QEMU 中为内核提供磁盘，请用 `cosmos run` 附加一个镜像：

```console
$ qemu-img create disk.img 64M
$ cosmos run --disk disk.img            # attached as an AHCI disk (default)
$ cosmos run --disk disk.img,nvme       # or as an NVMe namespace
$ cosmos run --disk disk.img,usb        # or as a USB stick on an xHCI controller
$ cosmos run --disk disk.img,virtio-blk # or as a virtio-blk-pci function
$ cosmos run --disk disk.img,virtio-blk-mmio # or, on arm64, in the virt machine's virtio-mmio window
```

如果需要多个驱动器，可以重复使用 `--disk`。`usb` 磁盘还会为机器提供 xHCI 控制器 `usbxhci0`；`--usb-controller ehci` 或 `ehci-ohci` 会改为放置一个 EHCI 控制器 `usbehci0`，`ohci` 则放置一个 OHCI 控制器 `usbohci0`，它们都会把每个 USB 设备固定到自己的某个根端口上。借助该控制器，你可以在内核运行时从 QEMU 监视器（在 QEMU 窗口中按 Ctrl+Alt+2）插入另一个 U 盘并将其拔出：

```console
(qemu) drive_add 0 if=none,id=stick,file=stick.img,format=raw
(qemu) device_add usb-storage,drive=stick,id=stick,bus=usbxhci0.0
(qemu) device_del stick
```

<a id="register-a-filesystem-driver-and-mount-it"></a>

## 注册文件系统驱动程序并挂载

下面的代码片段依赖以下 `using`：

```csharp
using System.IO;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.System.FileSystem;
using Cosmos.Kernel.System.FileSystem.Fat;
using Cosmos.Kernel.System.Storage;
```

`Cosmos.Kernel.System.FileSystem` 在一个命名空间中容纳了整个 VFS：管理器及其交出的对象（`VfsManager`、`VfsMount`、`IVfsNodeHandle`、`IVfsFileHandle` 和 `IVfsDirectoryHandle`），文件系统驱动程序要实现的契约（`IVfsFileSystemType`、`IVfsSuperblock`、`IVfsInode`、`IVfsOpenFile` 及其操作接口），以及每次挂载、创建和 stat 调用都会用到的标志、模式和元数据类型（`MountFlags`、`VfsMode`、`VfsStat`、`VfsStatFs`、`SetAttrFlags`、`SeekWhence`、`VfsTimespec`）。这就是为什么内核的 `BeforeRun()` 中出现的 `MountFlags.None` 与 `VfsManager` 用的是同一个 `using`，也是为什么 `IVfsNodeHandle.Inode` 给你的 `IVfsInode` 不需要其他 `using`。文件系统驱动程序位于下一级，每个驱动程序一个命名空间：FAT 驱动程序的 `FatFileSystemType` 和 `FatFormatOptions` 位于 `Cosmos.Kernel.System.FileSystem.Fat` 中。

只有下文中实现了 `IBlockDevice` 的 RAM 磁盘代码片段才需要 `Cosmos.Kernel.HAL.Devices.Storage`。去掉它之后，挂载真实分区只需要 `Cosmos.Kernel.System` 下的那三个命名空间。

首先，以你选择的名称注册一个 FAT 驱动程序，然后将一个分区挂载到挂载点上。把下面的代码加入内核的 `BeforeRun()`：

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
    Console.WriteLine("Mounted " + mount.Name + " at " + mount.MountPoint);
}
```

在存储扫描完成之前、没有附加磁盘时，以及存储被编译排除时，`Partitions` 都是空的，因此请先检查 `Count` 再对其进行索引。上面的每个调用都会返回是否成功：`RegisterFileSystem` 会拒绝已被占用的名称，`TryMount` 会拒绝驱动程序无法识别的源。

`StorageManager.GetPartitions(device)` 列出单个磁盘的分区，编号方式与用户的习惯一致；`StorageManager.Partitions` 则是跨所有磁盘的扁平列表。

还有第二种写法 `VfsManager.TryMount("fat", "0", ...)`，其中 `source` 是驱动程序特定的字符串。每个驱动程序都接受这种字符串，FAT 驱动程序把它解读为 `StorageManager.Partitions` 中的索引。请优先使用 `Partition` 重载：创建或删除分区会使该列表重新编号，因此跨越一次重新扫描而保留的索引可能会指向另一个分区。

<!-- screenshot: 内核刚启动后的控制台，显示 "Mounted fat partition 0 at /mnt" 这一行 -->
![挂载](images/filesystem-mount.png)

从这里开始，`/mnt` 下的一切都由 FAT 驱动程序提供，本文的其余内容都是普通的 `System.IO`。

**说明**：`/` 本身是一个“虚拟根”。它始终存在，即使什么都没有挂载也是如此，枚举它会列出各个挂载点。你不能直接在其中创建文件（`IOException`，只读文件系统）；请在 `/mnt` 这样的挂载点下创建文件。

<a id="alternative-a-ram-disk"></a>

### 另一种选择：RAM 磁盘

做快速实验时，你甚至不需要磁盘镜像。块设备不过是一个 `IBlockDevice`（来自 `Cosmos.Kernel.HAL.Devices.Storage`），一个以 RAM 为后备的块设备只需几行代码；内核测试套件用的正是这种方式：

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

FAT 驱动程序可以直接接受注入的设备。像往常一样注册它，并将 `source` 留空：设备已经在手，驱动程序无需再查找。

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

<a id="partition-a-disk"></a>

## 为磁盘分区

格式化需要一个可供格式化的分区。`StorageManager.Partitions` 列出内核启动时找到的磁盘上已有的分区；如果一个分区都没有，或者你想要不同的布局，可以用 `Gpt`、`Mbr` 和 `PartitionManager` 写入分区表本身。它们都接受一个 `IBlockDevice`，你可以从 `StorageManager.PrimaryDevice` 获得。

先查询磁盘上已有的分区方案：

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

`Gpt.Create` 和 `Mbr.Create` 会写入一个对应方案的空分区表，**并销毁原有的一切内容**。随后 `PartitionManager.Create` 添加一个分区，它适用于磁盘所采用的任何方案，因此你无需分情况处理：

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

正是 `RescanPartitions` 让新分区出现在 `StorageManager.Partitions` 中。在调用它之前，该列表描述的仍然是旧的分区表。

已有分区通过 `PartitionLocation` 来寻址，它记录的是起始扇区和长度，而不是索引，因此分区表重新编号时，分区的身份不会改变：

```csharp
PartitionManager.PartitionLocation where = new(startSector: 2048, sectorCount: 131072);

PartitionManager.Resize(disk, where, newSectorCount: 262144);
PartitionManager.MoveWithData(disk, where, newStartSector: 4096);
PartitionManager.Delete(disk, where);
```

`MoveWithData` 会先把内容复制到新位置，再重写分区表项；`Resize` 和 `Delete` 只修改分区表，因此把分区缩小到其文件系统大小以下会丢失数据。

**每个分区索引都由位置决定**。删除一个分区会使其后的表项重新编号，`StorageManager.Partitions` 也会随之重新编号，因此每次修改后都请重新读取列表，而不要跨越修改保留索引。

MBR 的四个主分区槽位可以通过一串逻辑分区来扩展。`PartitionManager.TryCreateLogical` 在扩展分区内添加一个逻辑分区，`Mbr.TryGetExtendedPartition` 则用于找到该扩展分区：

```csharp
if (Mbr.TryGetExtendedPartition(disk, out ulong extendedStart, out ulong extendedCount)
    && PartitionManager.TryCreateLogical(disk, systemId: 0x0C, sectorCount: 65536, out ulong logicalStart))
{
    Console.WriteLine("logical partition at LBA " + logicalStart);
}
```

`Gpt`、`Mbr` 和 `Ebr` 是更底层的一层，每种方案一个类型，用于你需要写入管理器未公开的表项的场合。

<a id="format-a-disk"></a>

## 格式化磁盘

要通过 VFS 格式化（mkfs）一个分区，请使用 `VfsManager.TryFormat`，并传入驱动程序名称、分区和该驱动程序的选项类型。FAT 格式化程序会根据你给出的选项选择合理的几何参数：

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

源处于挂载状态时，格式化会被拒绝：请先用 `VfsManager.TryUnmount("/mnt")` 卸载。

<a id="list-mounted-volumes"></a>

## 列出已挂载的卷

`VfsManager.Mounts` 是挂载表。每个表项会告诉你驱动程序名称、后备源和挂载点：

```csharp
foreach (VfsMount m in VfsManager.Mounts)
{
    Console.WriteLine(m.MountPoint + " -> " + m.Name + " (source " + m.Source + ")");
}
```

<!-- screenshot: 挂载表循环的控制台输出，例如 "/mnt -> fat (source 0)" -->
![挂载列表](images/filesystem-mounts.png)

对于通过 `Partition` 重载建立的挂载，`m.Partition` 就是分区本身。请优先使用它，而不是把 `m.Source` 解析回索引：在重新扫描使 `StorageManager.Partitions` 重新编号之后，它仍然指向同一磁盘上的同一范围。

<a id="check-free-space"></a>

## 检查可用空间

`VfsManager.TryStatFs` 报告一个挂载的块统计信息。可用空间等于可用块数乘以块大小：

```csharp
if (VfsManager.TryStatFs("/mnt", out VfsStatFs stats))
{
    ulong freeBytes = stats.AvailableBlocks * stats.BlockSize;
    ulong totalBytes = stats.Blocks * stats.BlockSize;
    Console.WriteLine(freeBytes + " of " + totalBytes + " bytes free");
}
```

<a id="get-a-list-of-files"></a>

## 获取文件列表

我们先用以下代码获取文件列表：

```csharp
string[] files = Directory.GetFiles("/mnt");

foreach (string file in files)
{
    Console.WriteLine(file);
}
```

搜索模式同样可用；这就是 BCL 原有的枚举引擎：

```csharp
string[] logs = Directory.GetFiles("/mnt", "*.txt");
```

<!-- screenshot: 控制台列出 /mnt 下的几个文件 -->
![文件列表](images/filesystem-files-list.png)

<a id="get-a-directory-listing-files-and-other-directories"></a>

## 获取目录列表（文件和其他目录）

```csharp
string[] files = Directory.GetFiles("/mnt");
string[] directories = Directory.GetDirectories("/mnt");

foreach (string file in files)
{
    Console.WriteLine(file);
}
foreach (string directory in directories)
{
    Console.WriteLine(directory);
}
```

<!-- screenshot: 控制台列出 /mnt 下的文件和目录 -->
![目录列表](images/filesystem-directory-listing.png)

<a id="read-all-the-files-in-a-directory"></a>

## 读取目录中的所有文件

我们获取文件列表，并打印每个文件的内容。与 Gen2 一样，请把文件系统代码放在 `try`/`catch` 中：这些异常是标准的 `System.IO` 异常，可以被捕获：

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

<!-- screenshot: 控制台显示一个文件的名称、大小和内容 -->
![读取所有文件](images/filesystem-read-all-files.png)

<a id="create-a-new-file"></a>

## 创建新文件

```csharp
try
{
    using FileStream stream = File.Create("/mnt/testing.txt");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

<a id="create-a-new-directory"></a>

## 创建新目录

`Directory.CreateDirectory` 一次调用即可创建整条缺失的父目录链：

```csharp
try
{
    Directory.CreateDirectory("/mnt/documents/reports");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

<a id="write-to-a-file"></a>

## 写入文件

```csharp
try
{
    File.WriteAllText("/mnt/testing.txt", "Learning how to use the Gen3 VFS!");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

`File.AppendAllText`、`File.WriteAllBytes` 和 `File.WriteAllLines` 的用法相同。

<a id="read-a-specific-file"></a>

## 读取指定文件

```csharp
try
{
    Console.WriteLine(File.ReadAllText("/mnt/testing.txt"));
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

<!-- screenshot: 控制台打印上一步写入的文件内容 -->
![读取指定文件](images/filesystem-read-specific-file.png)

对于二进制数据：

```csharp
byte[] data = File.ReadAllBytes("/mnt/testing.txt");
Console.WriteLine("Read " + data.Length + " bytes");
```

<a id="copy-move-delete"></a>

## 复制、移动、删除

与 Gen2 不同，`File.Move` 得到完整支持，无需采用先复制再删除的变通办法。移动到已存在的目标时会抛出 `IOException`，除非你传入 `overwrite: true`；覆盖操作在崩溃时是安全的（在重命名完成之前，目标会作为备份保留）。

```csharp
try
{
    File.Copy("/mnt/testing.txt", "/mnt/copy.txt");
    File.Move("/mnt/copy.txt", "/mnt/renamed.txt");
    File.Delete("/mnt/renamed.txt");

    Directory.Delete("/mnt/documents", recursive: true);
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
```

删除仍处于打开状态的文件不会失败：删除操作会进入“待处理”状态，表项在最后一个句柄关闭时消失，这正是 BCL 的 `DeleteOnClose` 语义所期望的。

<a id="streams"></a>

## 流

完整的流栈均可使用，包括定位、截断（`SetLength`）和带缓冲的文本 I/O：

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

<!-- screenshot: 控制台打印通过 StreamReader 读回的两行 -->
![流](images/filesystem-streams.png)

<a id="current-directory-and-relative-paths"></a>

## 当前目录与相对路径

内核维护一个当前目录（初始为 `/`），因此相对路径和 `Path.GetFullPath` 的行为与在任何 Unix 系统上一样：

```csharp
Directory.CreateDirectory("/mnt/work");
Directory.SetCurrentDirectory("/mnt/work");

File.WriteAllText("relative.txt", "resolved against the CWD");
Console.WriteLine(File.Exists("/mnt/work/relative.txt"));      // True
Console.WriteLine(Path.GetFullPath("sub/../file.txt"));        // /mnt/work/file.txt

Directory.SetCurrentDirectory("/");
```

<a id="error-handling"></a>

## 错误处理

标准的 `System.IO` 异常约定同样适用，因此你可以精确地捕获异常：

- 打开一个父目录存在、自身却不存在的文件 → `FileNotFoundException`
- 位于不存在的目录（或未挂载的前缀）下的任何路径 → `DirectoryNotFoundException`
- 直接在虚拟根 `/` 中创建文件 → `IOException`（只读文件系统）
- 不带 `recursive: true` 删除非空目录 → `IOException`
- 不允许覆盖时，用 `File.Copy`/`File.Move` 写到已存在的文件上 → `IOException`

即使**完全没有挂载任何东西**，`System.IO` 仍能平稳降级：`Directory.Exists("/")` 为 `true`，枚举 `/` 返回空列表，对其他路径的任何访问都会以上述异常之一失败，绝不会引发内核故障。

<a id="current-limitations"></a>

## 当前限制

- 不支持符号链接和硬链接（底层为 `ENOTSUP`/`EPERM`；BCL 将其表现为 `IOException`）。
- 文件时间戳尚未持久化（`File.SetLastWriteTime` 会被接受，但 FAT 时间戳的支持要在以后的版本中才会实现）。
- `DriveInfo` 尚未接入；查询可用空间请使用 `VfsManager.TryStatFs`。
- FAT 是内核测试套件覆盖的唯一文件系统驱动程序；ext2 驱动程序 `Ext2FileSystemType`（位于 `Cosmos.Kernel.System.FileSystem.Ext2`）与它一起提供，但只在主机上测试。新的驱动程序要实现的是 `IVfsFileSystemType` 接口。

<a id="how-it-works"></a>

## 工作原理

你的代码调用未经修改的 BCL，后者最终落到 Unix PAL（`Interop.Sys.*` P/Invoke）。这大约 45 个入口点在 `Cosmos.Kernel.Plugs` 中[通过 plug 替换](../dev/plugs.md)：一个文件描述符表适配 PAL 的契约（文件描述符、目录流、PAL errno），并委托给 `VfsManager`；`VfsManager` 负责路径解析、挂载表、当前目录和打开句柄的语义，并分派给已挂载的文件系统驱动程序；该驱动程序读写一个 `IBlockDevice`（由驱动工具包的 `AhciDriver`、`NvmeDriver`、`VirtioBlkDriver` 或 `UsbMassStorageDriver` 发布的 SATA 磁盘、NVMe 命名空间、virtio-blk 磁盘或 USB 大容量存储单元，全部经由 `StorageManager`；RAM 则经由 `MemoryBlockDevice`）。

```
File / Directory / FileStream          (stock BCL)
        │
Interop.Sys.* PAL calls                (stock BCL, plugged)
        │
FileDescriptorTable                    (Cosmos.Kernel.Plugs: fds, dir streams, errno)
        │
VfsManager                             (mounts, paths, CWD, open handles)
        │
IVfsFileSystemType / IVfsSuperblock    (FAT driver)
        │
IBlockDevice                           (the kit's AHCI, NVMe, virtio-blk and USB mass storage drivers, MemoryBlockDevice)
```
