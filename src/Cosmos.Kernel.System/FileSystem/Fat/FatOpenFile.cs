// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Fat;

internal sealed class FatOpenFile : IVfsOpenFile
{
    public FatOpenFile(FatInode inode, IFileOperations operations)
    {
        Inode = inode;
        Operations = operations;
    }

    public IVfsInode Inode { get; }

    public IFileOperations Operations { get; }

    public long Position { get; set; }
}
