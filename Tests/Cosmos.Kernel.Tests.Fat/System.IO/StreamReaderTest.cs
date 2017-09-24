using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class StreamReaderTest
    {
        /// <summary>
        /// Tests System.IO.StreamReader plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            mDebugger.Send("START TEST: StreamReader:");
            mDebugger.Send("Create StreamReader");

            using (var xSR = new StreamReader(@"0:\test.txt"))
            {
                if (xSR != null)
                {
                    mDebugger.Send("Start reading");

                    var content = xSR.ReadToEnd();
                    Assert.IsTrue(content == "A line of text for testing\nSecond line", "Content: " + content);
                }
                else
                {
                    Assert.IsTrue(false, @"Failed to create StreamReader for file 0:\test.txt");
                }
            }

            mDebugger.Send("END TEST");
        }
    }
}
