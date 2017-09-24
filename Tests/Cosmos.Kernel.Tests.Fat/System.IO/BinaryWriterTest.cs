using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class BinaryWriterTest
    {
        static private byte[] xBytes = new byte[16]
        {
            0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7,
            0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF
        };

        /// <summary>
        /// Tests System.IO.BinaryWriter plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            //TODO: Implement FileStream with FileMode.Create, currently throws a file not found exception

            mDebugger.Send("START TEST: BinaryWriter");
            mDebugger.Send("Creating FileStream: FileMode.Create");

            using (var xFS = new FileStream(@"0:\binary.bin", FileMode.Create))
            {
                mDebugger.Send("Creating BinaryWriter");

                using (var xBW = new BinaryWriter(xFS))
                {
                    if (xFS != null)
                    {
                        mDebugger.Send("Start writing");

                        xBW.Write(xBytes);
                        Assert.IsTrue(xFS.Length == xBytes.Length, "The length of the stream and the length of the bytes are different");
                    }
                    else
                    {
                        Assert.IsTrue(false, @"Failed to create StreamWriter for file 0:\binary.bin");
                    }
                }
            }

            mDebugger.Send("END TEST");
        }
    }
}
