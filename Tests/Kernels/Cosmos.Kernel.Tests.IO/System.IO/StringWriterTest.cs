using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using System;
using System.Text;

namespace Cosmos.Kernel.Tests.IO.System.IO
{
    class StringWriterTest
    {
        /// <summary>
        /// Tests System.IO.StringReaderTest plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            StringWriter strWriter = new StringWriter();

            strWriter.Write("This");

            Assert.IsTrue(strWriter.ToString() == "This", "StringWriter.Write(String) does not work");

            strWriter.Write(' ');

            Assert.IsTrue(strWriter.ToString() == "This ", "StringWriter.Write(Char) does not work");

            // This is a little weird in C#
            char[] arr = { 'i', 's', ' ', 'a', ' ', 't', 'e', 's', 't' };

            strWriter.Write(arr);

            Assert.IsTrue(strWriter.ToString() == "This is a test", "StringWriter.Write(Char[]) does not work");

            StringBuilder sb = strWriter.GetStringBuilder();

            Assert.IsTrue(strWriter.ToString() == sb.ToString(), "StringWriter.GetStringBuilder() does not work");

            // These need again NumberBuffer working. It should be all managed on Net Core 2.1 so let's wait for it
#if false
            strWriter = new StringWriter();
            strWriter.Write(1);

            Assert.IsTrue(strWriter.ToString() == "1", "StringWriter.Write(Int[]) does not work");

            strWriter.Write(' ');
            strWriter.Write(42.42);

            Assert.IsTrue(strWriter.ToString() == "1 42.42", "StringWriter.Write(Double[]) does not work");
#endif
        }
    }
}
