using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using System;
using System.Text;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class StreamWriterStreamReaderTest
    {
        /// <summary>
        /// Tests System.IO.StreamWriter plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            string file = @"0:\test.txt";

            mDebugger.Send("START TEST: StreamWriter:");

            /*
             * To Show that UTF-8 is effectively working you write in the file "Cosmos is wonderful!" in Japanase
             * and read it again
             */
            var text = "Cosmos 素晴らしいです!";

            using (var xSW = new StreamWriter(file))
            {
                if (xSW == null)
                    Assert.IsTrue(false, $"Failed to create StreamWriter for file {file}");

                try
                {
                    mDebugger.Send("Start writing");

                    xSW.Write(text);
                }
                catch
                {
                    Assert.IsTrue(false, $"Couldn't write to file {file} using StreamWriter");
                }
            }
            mDebugger.Send("END TEST");

            /* We use StreamReader() instead of File now it is more "correct" and we test 2 classes in one too! */
            mDebugger.Send("START TEST: StreamReader:");
            using (var xSR = new StreamReader(file))
            {
                if (xSR == null)
                    Assert.IsTrue(false, $"Failed to create StreamReader for file {file}");

                try
                {
                    mDebugger.Send("Start reading");
                    var readText = xSR.ReadToEnd();
                    Assert.IsTrue(text == readText, "Failed to write and read file");
                }
                catch
                {
                    Assert.IsTrue(false, $"Couldn't read from file {file} using StreamReader");
                }
            }

            using (StreamReader xSR2 = new StreamReader(file, Encoding.UTF8, true))
            {
                mDebugger.Send("Reading using different Ctor!!!");
                var readText2 = xSR2.ReadToEnd();
                mDebugger.Send($"Read {readText2}");

               Assert.IsTrue(text == readText2, "Failed to write and read file");
            }

            mDebugger.Send("END TEST");
        }
    }
}
