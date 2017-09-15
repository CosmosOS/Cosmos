using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class FileStreamTest
    {
        /// <summary>
        /// Tests System.IO.FileStream plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            mDebugger.Send("START TEST: Filestream:");

            using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
            {
                mDebugger.Send("Start writing");
                var xStr = "Test FAT Write.";
                byte[] xWriteBuff = xStr.GetUtf8Bytes(0, (uint)xStr.Length);
                xFS.Write(xWriteBuff, 0, xWriteBuff.Length);
                mDebugger.Send("---- Data written");
                xFS.Position = 0;
                byte[] xReadBuff = new byte[xWriteBuff.Length];
                xFS.Read(xReadBuff, 0, xWriteBuff.Length);
                mDebugger.Send("xWriteBuff " + xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length) + " xReadBuff " +
                               xReadBuff.GetUtf8String(0, (uint)xWriteBuff.Length));
                string xWriteBuffAsString = xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length);
                string xReadBuffAsString = xReadBuff.GetUtf8String(0, (uint)xReadBuff.Length);
                Assert.IsTrue(xWriteBuffAsString == xReadBuffAsString, "Failed to write and read file");
                mDebugger.Send("END TEST");
            }
        }
    }
}
