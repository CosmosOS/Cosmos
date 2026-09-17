// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Filesystems.Ext2;
using Cosmos.Kernel.System.Vfs;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Ext2;

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
    {
        if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo));
        }

        int byteOffset = (int)(blockNo * BlockSize);
        int byteLen = (int)(blockCount * BlockSize);
        _storage.AsSpan(byteOffset, byteLen).CopyTo(data);
    }

    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo));
        }

        int byteOffset = (int)(blockNo * BlockSize);
        int byteLen = (int)(blockCount * BlockSize);
        data.Slice(0, byteLen).CopyTo(_storage.AsSpan(byteOffset, byteLen));
    }

    public void Flush()
    {
    }
}

[TestFixture]
public class Ext2SmokeTest
{
    private const ulong BlockSize = 512;
    private const ulong BlockCount = 16384; // 8 MiB

    [Test]
    public void FormatAndMount()
    {
        MemoryBlockDevice device = new("ram0", BlockSize, BlockCount);
        Ext2FilesystemType fs = new(device);
        Assert.That(fs.TryFormat(ReadOnlySpan<char>.Empty, new Ext2FormatOptions { BlockSize = 1024 }), Is.True);
        Assert.That(fs.TryMount(ReadOnlySpan<char>.Empty, MountFlags.None, out IVfsSuperblock? sb), Is.True);
        Assert.That(sb, Is.Not.Null);
        Assert.That(sb!.Root, Is.Not.Null);
        Assert.That(sb.SuperOperations.StatFs(sb, out VfsStatFs stat), Is.True);
        Assert.That(stat.Type, Is.EqualTo(0xEF53u));
        Assert.That(stat.BlockSize, Is.EqualTo(1024u));
    }

    [Test]
    public void CreateWriteRead()
    {
        MemoryBlockDevice device = new("ram0", BlockSize, BlockCount);
        Ext2FilesystemType fs = new(device);
        Assert.That(fs.TryFormat(ReadOnlySpan<char>.Empty, null), Is.True);
        Assert.That(fs.TryMount(ReadOnlySpan<char>.Empty, MountFlags.None, out IVfsSuperblock? sb), Is.True);
        Assert.That(VfsManager.RegisterFilesystem("ext2test1", fs), Is.True);
        try
        {
            Assert.That(VfsManager.TryMount("ext2test1", ReadOnlySpan<char>.Empty, MountFlags.None, "/ext2a", out _), Is.True);
            Assert.That(VfsManager.TryCreateFile("/ext2a/hello.txt", VfsMode.OwnerRead | VfsMode.OwnerWrite), Is.True);
            Assert.That(VfsManager.TryOpenFile("/ext2a/hello.txt", out IVfsFileHandle? fh), Is.True);
            byte[] payload = global::System.Text.Encoding.ASCII.GetBytes("hello ext2");
            long written = fh!.Write(payload);
            Assert.That(written, Is.EqualTo(payload.Length));
            fh.TrySeek(0, SeekWhence.Set);
            byte[] buf = new byte[32];
            long read = fh.Read(buf);
            Assert.That(read, Is.EqualTo(payload.Length));
            Assert.That(global::System.Text.Encoding.ASCII.GetString(buf, 0, (int)read), Is.EqualTo("hello ext2"));
            fh.Dispose();
            Assert.That(VfsManager.TryUnmount("/ext2a"), Is.True);
        }
        finally
        {
            VfsManager.TryUnmount("/ext2a");
        }
    }

    [Test]
    public void CaseSensitiveAndSymlink()
    {
        MemoryBlockDevice device = new("ram0", BlockSize, BlockCount);
        Ext2FilesystemType fs = new(device);
        Assert.That(fs.TryFormat(ReadOnlySpan<char>.Empty, null), Is.True);
        Assert.That(fs.TryMount(ReadOnlySpan<char>.Empty, MountFlags.None, out IVfsSuperblock? sb), Is.True);
        Assert.That(VfsManager.RegisterFilesystem("ext2test2", fs), Is.True);
        try
        {
            Assert.That(VfsManager.TryMount("ext2test2", ReadOnlySpan<char>.Empty, MountFlags.None, "/ext2b", out _), Is.True);

            // Case sensitivity: "File" vs "file" are distinct.
            Assert.That(VfsManager.TryCreateFile("/ext2b/File", VfsMode.OwnerRead | VfsMode.OwnerWrite), Is.True);
            Assert.That(VfsManager.TryCreateFile("/ext2b/file", VfsMode.OwnerRead | VfsMode.OwnerWrite), Is.True);
            Assert.That(VfsManager.TryStat("/ext2b/File", out _), Is.True);
            Assert.That(VfsManager.TryStat("/ext2b/file", out _), Is.True);
            Assert.That(VfsManager.TryStat("/ext2b/FILE", out _), Is.False);

            // Symlink: busybox multicall pattern.
            Assert.That(VfsManager.TryCreateFile("/ext2b/busybox", VfsMode.OwnerRead | VfsMode.OwnerWrite | VfsMode.OwnerExecute), Is.True);
            Assert.That(VfsManager.TryOpenFile("/ext2b/busybox", out IVfsFileHandle? bf), Is.True);
            bf!.Write(global::System.Text.Encoding.ASCII.GetBytes("busybox binary"));
            bf.Dispose();

            // Create symlink /ext2b/ls -> busybox (relative).
            Assert.That(VfsManager.TryOpenDirectory("/ext2b", out IVfsDirectoryHandle? dir), Is.True);
            Assert.That(dir!.TrySymlink("ls", "busybox", out _), Is.True);
            dir.Dispose();

            // Opening symlink should follow to target.
            Assert.That(VfsManager.TryOpenFile("/ext2b/ls", out IVfsFileHandle? lf), Is.True);
            lf!.TrySeek(0, SeekWhence.Set);
            byte[] buf = new byte[32];
            long r = lf.Read(buf);
            Assert.That(global::System.Text.Encoding.ASCII.GetString(buf, 0, (int)r), Is.EqualTo("busybox binary"));
            lf.Dispose();

            // lstat-style: symlink itself is a symlink, not regular file.
            Assert.That(VfsManager.TryOpenDirectory("/ext2b", out IVfsDirectoryHandle? dir2), Is.True);
            Assert.That(dir2!.TryLookup("ls", out IVfsNodeHandle? node), Is.True);
            Assert.That(node!.Inode.InodeOperations.GetAttr(node.Inode, out VfsStat st), Is.True);
            Assert.That(st.IsSymbolicLink, Is.True);
            node.Dispose();
            dir2.Dispose();

            Assert.That(VfsManager.TryUnmount("/ext2b"), Is.True);
        }
        finally
        {
            VfsManager.TryUnmount("/ext2b");
        }
    }

    [Test]
    public void LargeFileIndirectRoundtrip()
    {
        MemoryBlockDevice device = new("ram0", BlockSize, BlockCount);
        Ext2FilesystemType fs = new(device);
        Assert.That(fs.TryFormat(ReadOnlySpan<char>.Empty, null), Is.True);
        Assert.That(fs.TryMount(ReadOnlySpan<char>.Empty, MountFlags.None, out IVfsSuperblock? sb), Is.True);
        Assert.That(VfsManager.RegisterFilesystem("ext2test4", fs), Is.True);
        try
        {
            Assert.That(VfsManager.TryMount("ext2test4", ReadOnlySpan<char>.Empty, MountFlags.None, "/ext2d", out _), Is.True);
            // 32 KiB with 1 KiB blocks: 12 direct + 20 single-indirect blocks.
            const int Size = 32 * 1024;
            byte[] payload = new byte[Size];
            for (int i = 0; i < Size; i++)
            {
                payload[i] = (byte)(i * 7 + 13);
            }

            Assert.That(VfsManager.TryCreateFile("/ext2d/big.bin", VfsMode.OwnerRead | VfsMode.OwnerWrite), Is.True);
            Assert.That(VfsManager.TryOpenFile("/ext2d/big.bin", out IVfsFileHandle? fh), Is.True);
            long written = fh!.Write(payload);
            Assert.That(written, Is.EqualTo(Size));
            fh.Dispose();

            // Read back whole file; exercises the indirect-block cache path.
            Assert.That(VfsManager.TryOpenFile("/ext2d/big.bin", out IVfsFileHandle? rh), Is.True);
            byte[] buf = new byte[Size];
            long read = rh!.Read(buf);
            Assert.That(read, Is.EqualTo(Size));
            Assert.That(buf, Is.EqualTo(payload));
            rh.Dispose();

            // Overwrite a span inside the indirect region; the cache must
            // observe the new mapping, not stale entries.
            Assert.That(VfsManager.TryOpenFile("/ext2d/big.bin", out IVfsFileHandle? wh), Is.True);
            wh!.TrySeek(16 * 1024, SeekWhence.Set);
            byte[] patch = new byte[4096];
            for (int i = 0; i < patch.Length; i++)
            {
                patch[i] = (byte)(255 - i);
            }

            Assert.That(wh.Write(patch), Is.EqualTo(patch.Length));
            wh.Dispose();

            Assert.That(VfsManager.TryOpenFile("/ext2d/big.bin", out IVfsFileHandle? vh), Is.True);
            byte[] verify = new byte[Size];
            Assert.That(vh!.Read(verify), Is.EqualTo(Size));
            for (int i = 0; i < Size; i++)
            {
                byte expected = i >= 16 * 1024 && i < 16 * 1024 + 4096
                    ? (byte)(255 - (i - 16 * 1024))
                    : (byte)(i * 7 + 13);
                Assert.That(verify[i], Is.EqualTo(expected));
            }

            vh.Dispose();
            Assert.That(VfsManager.TryUnmount("/ext2d"), Is.True);
        }
        finally
        {
            VfsManager.TryUnmount("/ext2d");
        }
    }

    [Test]
    public void Permissions()
    {
        MemoryBlockDevice device = new("ram0", BlockSize, BlockCount);
        Ext2FilesystemType fs = new(device);
        Assert.That(fs.TryFormat(ReadOnlySpan<char>.Empty, null), Is.True);
        Assert.That(fs.TryMount(ReadOnlySpan<char>.Empty, MountFlags.None, out IVfsSuperblock? sb), Is.True);
        Assert.That(VfsManager.RegisterFilesystem("ext2test3", fs), Is.True);
        try
        {
            Assert.That(VfsManager.TryMount("ext2test3", ReadOnlySpan<char>.Empty, MountFlags.None, "/ext2c", out _), Is.True);
            VfsMode mode = VfsMode.OwnerRead | VfsMode.OwnerWrite | VfsMode.GroupRead | VfsMode.OtherRead;
            Assert.That(VfsManager.TryCreateFile("/ext2c/perm.txt", mode), Is.True);
            Assert.That(VfsManager.TryStat("/ext2c/perm.txt", out VfsStat st), Is.True);
            Assert.That(st.Mode & VfsMode.PermissionMask, Is.EqualTo(mode));
            // chmod via SetAttr
            Assert.That(VfsManager.TryOpenFile("/ext2c/perm.txt", out IVfsFileHandle? fh), Is.True);
            VfsStat chmod = st;
            chmod.Mode = VfsMode.RegularFile | VfsMode.OwnerRead | VfsMode.OwnerWrite;
            Assert.That(fh!.TrySetAttr(SetAttrFlags.Mode, chmod), Is.True);
            Assert.That(fh.TryStat(out VfsStat st2), Is.True);
            Assert.That(st2.Mode & VfsMode.PermissionMask, Is.EqualTo(VfsMode.OwnerRead | VfsMode.OwnerWrite));
            fh.Dispose();
            Assert.That(VfsManager.TryUnmount("/ext2c"), Is.True);
        }
        finally
        {
            VfsManager.TryUnmount("/ext2c");
        }
    }
}
