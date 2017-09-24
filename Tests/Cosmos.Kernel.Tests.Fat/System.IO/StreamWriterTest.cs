using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class StreamWriterTest
    {
        /// <summary>
        /// Tests System.IO.StreamWriter plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {           
            mDebugger.Send("START TEST: StreamWriter:");
            mDebugger.Send("Create StreamWriter");

            using (var xSW = new StreamWriter(@"0:\test.txt"))
            {
                if (xSW != null)
                {
                    try
                    {
                        mDebugger.Send("Start writing");

                        xSW.Write("A line of text for testing\nSecond line");
                    }
                    catch
                    {
                        Assert.IsTrue(false, @"Couldn't write to file 0:\test.txt using StreamWriter");
                    }
                }
                else
                {
                    Assert.IsTrue(false, @"Failed to create StreamWriter for file 0:\test.txt");
                }
            }

            mDebugger.Send("END TEST");
        }
    }
}
