using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Kernel.Tests.Fat
{
    public class Files
    {
        [ManifestResourceStream(ResourceName = "Cosmos.Kernel.Tests.Fat.Resources.smol.bin")] 
        public static byte[] smolBinBytes;

        [ManifestResourceStream(ResourceName = "Cosmos.Kernel.Tests.Fat.Resources.long.bin")]
        public static byte[] longBinBytes;

        [ManifestResourceStream(ResourceName = "Cosmos.Kernel.Tests.Fat.Resources.longer.bin")]
        public static byte[] longerBinBytes;

        [ManifestResourceStream(ResourceName = "Cosmos.Kernel.Tests.Fat.Resources.big.bin")]
        public static byte[] bigBinBytes;
    }
}