using System.Text;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.System.FileSystem.ISO9660;

public class ISO9660FileSystemFactory : FileSystemFactory
{
    public override string Name => "ISO9660";

    public override FileSystem Create(Partition aDevice, string aRootPath, long aSize) =>
        new ISO9660FileSystem(aDevice, aRootPath, aSize);

    public override bool IsType(Partition aDevice)
    {
        var primarySectory = aDevice.NewBlockArray(1);
        aDevice.ReadBlock(0x10, 1, ref primarySectory);
        if (Encoding.ASCII.GetString(primarySectory, 1, 5) == "CD001")
        {
            return true;
        }

        return false;
    }
}
