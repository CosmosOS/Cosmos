using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using static Cosmos.Kernel.Tests.Fat.System.IO.HelperMethods;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class BinaryWriterBinaryReaderTest
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
            mDebugger.Send("START TEST: BinaryWriter");
            mDebugger.Send("Creating FileStream: FileMode.Create");

            using (var xFS = new FileStream(@"0:\binary.bin", FileMode.Create))
            {             
                if (xFS == null)
                {
                    Assert.IsTrue(false, "Failed to create StreamWriter for file 0:\binary.bin");
                    return;
                }

                mDebugger.Send("Creating BinaryWriter");
                using (var xBW = new BinaryWriter(xFS))
                {
                    mDebugger.Send("Start writing");

                    xBW.Write(xBytes);
                    Assert.IsTrue(xFS.Length == xBytes.Length, "The length of the stream and the length of the bytes are different");

                    mDebugger.Send("Binary data written");
                }

                /* Put the FileStream on position 0 again */
                mDebugger.Send("Resetting FileStream");
                //xFS.Seek(0, SeekOrigin.Begin);
                xFS.Position = 0;

                using (var xBR = new BinaryReader(xFS))
                {
                    if (xFS != null)
                    {
                        mDebugger.Send("Start reading");

                        byte[] xBuffer = xBR.ReadBytes(xBytes.Length);
                        Assert.IsTrue(ByteArrayAreEquals(xBytes, xBuffer), "Bytes changed during BinaryWriter and BinaryReader opeartions on FileStream");

                        mDebugger.Send("Binary data read");
                    }
                }
            }

            mDebugger.Send("END TEST");
        }
    }
}
