# File System

In this article, we will discuss using the Cosmos Gen3 VFS (virtual file system).
Unlike Gen2, where you talked to `CosmosVFS` and a plugged subset of `System.IO`, Gen3 gives you the **standard .NET `System.IO` API** (`File`, `Directory`, `FileStream`, `StreamReader`/`StreamWriter`, `FileInfo`/`DirectoryInfo`) running unmodified on top of the kernel's VFS. You mount a filesystem at a Unix-style mount point and use ordinary rooted paths.

The main differences if you come from Gen2:

| | Gen2 | Gen3 |
|---|---|---|
| Paths | DOS drive letters (`0:\file.txt`) | Unix paths (`/mnt/file.txt`) |
| Setup | `CosmosVFS` + `VFSManager.RegisterVFS` | `VfsManager.RegisterFilesystem` + `VfsManager.TryMount` |
| API surface | Plugged subset of `System.IO` | Full `System.IO` (streams, enumeration patterns, `FileInfo`, …) |
| `File.Move` | Not plugged (copy + delete) | Works, including onto-existing overwrite semantics |
| Filesystems | FAT32 (FAT12/16 partial) | FAT12/16/32 |

**Attention**: **Always** format your drive with Cosmos and **only** Cosmos if you plan to use it with Cosmos. Tools like Parted or FDisk are much more advanced and may lay the disk out differently than Cosmos expects.

**WARNING!**: Please do **not** try this on actual hardware! It may cause **IRREPARABLE DAMAGE** to your data. Use a virtual machine (QEMU via `cosmos run`, VMware, VirtualBox, …).

## Enable storage in your kernel

Storage support is behind a feature switch. Make sure your kernel's `.csproj` does not turn it off (it defaults to `true`):

```xml
<PropertyGroup>
  <CosmosEnableStorage>true</CosmosEnableStorage>
</PropertyGroup>
```

At boot the kernel initializes `StorageManager`, which registers every AHCI, NVMe and USB mass storage device it finds and scans their MBR/GPT partition tables into `StorageManager.Partitions`. USB sticks and disks come up as `usb0`, `usb1`, ... after the internal disks, so the first internal disk stays the primary device. USB devices are only discovered at boot: plug the stick in before starting the kernel.

To give your kernel a disk in QEMU, attach an image with `cosmos run`:

```console
$ qemu-img create disk.img 64M
$ cosmos run --disk disk.img            # attached as an AHCI disk (default)
$ cosmos run --disk disk.img,nvme       # or as an NVMe namespace
$ cosmos run --disk disk.img,usb        # or as a USB stick on an xHCI controller
```

`--disk` is repeatable if you want several drives.

## Register a filesystem driver and mount it

These are the `using`s the snippets below rely on:

```csharp
using System.IO;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Vfs;
```

Two of those five are HAL namespaces, and that is deliberate. `Cosmos.Kernel.HAL.Vfs` holds the VFS vocabulary that drivers and callers share: the contracts a filesystem driver implements (`IVfsFilesystemType`, `IVfsSuperblock`, `IVfsInode`, `IVfsOpenFile` and their operations interfaces) and the flag, mode and metadata types every mount, create and stat call names (`MountFlags`, `VfsMode`, `VfsStat`, `VfsStatFs`, `SetAttrFlags`, `SeekWhence`, `VfsTimespec`). That is why `MountFlags.None` appears in a kernel's `BeforeRun()`. `Cosmos.Kernel.System.Vfs` holds the manager and the handles, `VfsManager` plus `IVfsNodeHandle`, `IVfsFileHandle` and `IVfsDirectoryHandle`, and the handles are typed in the shared vocabulary, so `IVfsNodeHandle.Inode` gives you a `Cosmos.Kernel.HAL.Vfs.IVfsInode`. The vocabulary sits in the lower assembly because the reference graph runs `Cosmos.Kernel.System` to `Cosmos.Kernel.HAL` to `Cosmos.Kernel.HAL.Interfaces`, never the other way.

`Cosmos.Kernel.HAL.Interfaces.Devices` is needed only by the RAM-disk snippet further down, which implements `IBlockDevice`. Drop that and mounting a real partition takes four.

First, register a FAT driver under a name of your choice, then mount a partition at a mount point. Add this to your kernel's `BeforeRun()`:

```csharp
FatFilesystemType fat = new();

if (!VfsManager.RegisterFilesystem("fat", fat))
{
    Console.WriteLine("The name \"fat\" is already registered.");
    return;
}

if (StorageManager.Partitions.Count == 0)
{
    Console.WriteLine("No partitions found.");
    return;
}

if (VfsManager.TryMount("fat", StorageManager.Partitions[0], MountFlags.None, "/mnt", out VfsManager.VfsMount? mount))
{
    Console.WriteLine("Mounted " + mount.Name + " at " + mount.MountPoint);
}
```

`Partitions` is empty before storage is scanned, when no disk is attached, and when storage is compiled out, so index it only after checking `Count`. Every call above returns whether it worked: `RegisterFilesystem` refuses a name already in use, and `TryMount` refuses a source the driver does not recognize.

`StorageManager.GetPartitions(device)` lists the partitions of one disk, numbered the way a user numbers them; `StorageManager.Partitions` is the flat list across every disk.

There is a second spelling, `VfsManager.TryMount("fat", "0", ...)`, where `source` is a driver-specific string. Every driver accepts one, and the FAT driver reads it as an index into `StorageManager.Partitions`. Prefer the `Partition` overload: creating or deleting a partition renumbers that list, so an index held across a rescan can come to name a different partition.

<!-- screenshot: kernel console right after boot showing the "Mounted fat partition 0 at /mnt" line -->
![Mount](images/filesystem-mount.png)

From this point on, everything under `/mnt` is served by the FAT driver, and everything in this article is plain `System.IO`.

**Note**: `/` itself is a *virtual root*. It always exists, even with nothing mounted, and enumerating it lists the mount points. You cannot create files directly in it (`IOException`, read-only file system); create them under a mount point like `/mnt`.

### Alternative: a RAM disk

For quick experiments you don't need a disk image at all. A block device is just an `IBlockDevice` (from `Cosmos.Kernel.HAL.Interfaces.Devices`), and a RAM-backed one fits in a few lines; this is exactly what the kernel test suites use:

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

The FAT driver accepts an injected device directly. Register it as usual and leave `source` empty: with a device already in hand the driver has nothing to look up.

```csharp
MemoryBlockDevice ramDisk = new("RAMDISK", 512, 65536);   // 32 MiB
FatFilesystemType fat = new(ramDisk);

if (!VfsManager.RegisterFilesystem("ramfat", fat)
    || !VfsManager.TryFormat("ramfat", "", new FatFormatOptions { Type = FatType.Fat16 })
    || !VfsManager.TryMount("ramfat", "", MountFlags.None, "/mnt", out _))
{
    Console.WriteLine("RAM disk setup failed.");
    return;
}
```

## Partition a disk

Formatting needs a partition to format. `StorageManager.Partitions` lists the ones already on the disks the kernel found at boot; when there are none, or you want a different layout, `Gpt`, `Mbr` and `PartitionManager` write the partition table itself. They all take an `IBlockDevice`, which `StorageManager.PrimaryDevice` hands you.

Start by asking what scheme the disk already carries:

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

`Gpt.Create` and `Mbr.Create` lay down an empty table of that scheme, **destroying whatever was there**. `PartitionManager.Create` then adds a partition, working on whichever scheme the disk carries so you do not have to branch:

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

`RescanPartitions` is what makes the new partition show up in `StorageManager.Partitions`. Until you call it the list still describes the old table.

Existing partitions are addressed by a `PartitionLocation`, which is the start sector and length rather than an index, so a partition does not change identity when the table is renumbered:

```csharp
PartitionManager.PartitionLocation where = new(startSector: 2048, sectorCount: 131072);

PartitionManager.Resize(disk, where, newSectorCount: 262144);
PartitionManager.MoveWithData(disk, where, newStartSector: 4096);
PartitionManager.Delete(disk, where);
```

`MoveWithData` copies the contents to the new location before rewriting the entry; `Resize` and `Delete` only touch the table, so shrinking a partition below its filesystem's size loses data.

**Every partition index is positional.** Deleting one renumbers the entries after it, and `StorageManager.Partitions` renumbers with them, so re-read the list after any change rather than holding an index across one.

MBR's four primary slots are extended with a chain of logical partitions. `PartitionManager.TryCreateLogical` adds one inside the extended partition, and `Mbr.TryGetExtendedPartition` finds it:

```csharp
if (Mbr.TryGetExtendedPartition(disk, out ulong extendedStart, out ulong extendedCount)
    && PartitionManager.TryCreateLogical(disk, systemId: 0x0C, sectorCount: 65536, out ulong logicalStart))
{
    Console.WriteLine("logical partition at LBA " + logicalStart);
}
```

`Gpt`, `Mbr` and `Ebr` are the lower layer, one type per scheme, for when you need to write entries the manager does not expose.

## Format a disk

To format (mkfs) a partition through the VFS, use `VfsManager.TryFormat` with the driver name, the partition and the driver's option type. The FAT formatter picks sane geometry from the options you give it:

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

Formatting is refused while the source is mounted: unmount first with `VfsManager.TryUnmount("/mnt")`.

## List mounted volumes

`VfsManager.Mounts` is the mount table. Each entry tells you the driver name, the backing source and the mount point:

```csharp
foreach (VfsManager.VfsMount m in VfsManager.Mounts)
{
    Console.WriteLine(m.MountPoint + " -> " + m.Name + " (source " + m.Source + ")");
}
```

<!-- screenshot: console output of the mount-table loop, e.g. "/mnt -> fat (source 0)" -->
![Mounts](images/filesystem-mounts.png)

`m.Partition` is the partition itself, for mounts made with the `Partition` overload. Prefer it to parsing `m.Source` back into an index: it keeps naming the same range on the same disk after a rescan renumbers `StorageManager.Partitions`.

## Check free space

`VfsManager.TryStatFs` reports a mount's block accounting. Free space is the available block count times the block size:

```csharp
if (VfsManager.TryStatFs("/mnt", out VfsStatFs stats))
{
    ulong freeBytes = stats.Bavail * stats.BlockSize;
    ulong totalBytes = stats.Blocks * stats.BlockSize;
    Console.WriteLine(freeBytes + " of " + totalBytes + " bytes free");
}
```

## Get a list of files

We start by getting a list of files, using:

```csharp
string[] files = Directory.GetFiles("/mnt");

foreach (string file in files)
{
    Console.WriteLine(file);
}
```

Search patterns work too; this is the stock BCL enumeration engine:

```csharp
string[] logs = Directory.GetFiles("/mnt", "*.txt");
```

<!-- screenshot: console listing a few files under /mnt -->
![Files List](images/filesystem-files-list.png)

## Get a directory listing (files and other directories)

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

<!-- screenshot: console listing files and directories under /mnt -->
![Directory Listing](images/filesystem-directory-listing.png)

## Read all the files in a directory

We get the file list and print the content of each file. As in Gen2, keep filesystem code inside `try`/`catch`: the exceptions are the standard `System.IO` ones and they are catchable:

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

<!-- screenshot: console showing a file's name, size and content -->
![Read All Files](images/filesystem-read-all-files.png)

## Create a new file

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

## Create a new directory

`Directory.CreateDirectory` creates the whole chain of missing parents in one call:

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

## Write to a file

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

`File.AppendAllText`, `File.WriteAllBytes` and `File.WriteAllLines` work the same way.

## Read a specific file

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

<!-- screenshot: console printing the file content written in the previous step -->
![Read Specific File](images/filesystem-read-specific-file.png)

And for binary data:

```csharp
byte[] data = File.ReadAllBytes("/mnt/testing.txt");
Console.WriteLine("Read " + data.Length + " bytes");
```

## Copy, move, delete

Unlike Gen2, `File.Move` is fully supported, no copy-and-delete workaround needed. A move onto an existing destination throws `IOException` unless you pass `overwrite: true`; the overwrite is crash-safe (the destination is kept as a backup until the rename lands).

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

Deleting a file that is still open does not fail: the delete goes *pending* and the entry disappears when the last handle closes, which is what the BCL's `DeleteOnClose` semantics expect.

## Streams

The full stream stack is available, including seeking, truncation (`SetLength`) and buffered text I/O:

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

<!-- screenshot: console printing the two lines read back through StreamReader -->
![Streams](images/filesystem-streams.png)

## Current directory and relative paths

The kernel keeps a current directory (it starts at `/`), so relative paths and `Path.GetFullPath` behave like on any Unix system:

```csharp
Directory.CreateDirectory("/mnt/work");
Directory.SetCurrentDirectory("/mnt/work");

File.WriteAllText("relative.txt", "resolved against the CWD");
Console.WriteLine(File.Exists("/mnt/work/relative.txt"));      // True
Console.WriteLine(Path.GetFullPath("sub/../file.txt"));        // /mnt/work/file.txt

Directory.SetCurrentDirectory("/");
```

## Error handling

The standard `System.IO` exception contract applies, so you can catch precisely:

- Opening a missing file whose parent exists → `FileNotFoundException`
- Any path under a missing directory (or an unmounted prefix) → `DirectoryNotFoundException`
- Creating a file directly in the virtual root `/` → `IOException` (read-only file system)
- Deleting a non-empty directory without `recursive: true` → `IOException`
- `File.Copy`/`File.Move` onto an existing file without overwrite → `IOException`

With **nothing mounted at all**, `System.IO` still degrades gracefully: `Directory.Exists("/")` is `true`, enumerating `/` returns an empty list, and every access to another path fails with one of the exceptions above, never a kernel fault.

## Current limitations

- Symbolic links and hard links are not supported (`ENOTSUP`/`EPERM` under the hood; the BCL surfaces `IOException`).
- File timestamps are not persisted yet (`File.SetLastWriteTime` is accepted but a FAT timestamp lands later).
- `DriveInfo` is not wired up yet; use `VfsManager.TryStatFs` for free-space queries.
- FAT is the only filesystem driver today; the `IVfsFilesystemType` interface is what a new driver implements.

## How it works

Your code calls the stock BCL, which bottoms out in the Unix PAL (`Interop.Sys.*` P/Invokes). Those ~45 entry points are [plugged](../dev/plugs.md) in `Cosmos.Kernel.Plugs`: a file-descriptor table adapts the PAL contract (fds, dir streams, PAL errnos) and delegates to `VfsManager`, which owns path resolution, the mount table, the current directory and open-handle semantics, and dispatches to the mounted filesystem driver, which reads and writes an `IBlockDevice` (AHCI, NVMe or USB mass storage via `StorageManager`, RAM via `MemoryBlockDevice`).

```
File / Directory / FileStream          (stock BCL)
        │
Interop.Sys.* PAL calls                (stock BCL, plugged)
        │
FileDescriptorTable                    (Cosmos.Kernel.Plugs: fds, dir streams, errno)
        │
VfsManager                             (mounts, paths, CWD, open handles)
        │
IVfsFilesystemType / IVfsSuperblock    (FAT driver)
        │
IBlockDevice                           (AHCI, NVMe, USB, MemoryBlockDevice)
```
