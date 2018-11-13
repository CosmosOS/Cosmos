using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using System;
using System.Text;

namespace Cosmos.Kernel.Tests.IO.System.IO
{
    class StringReaderTest
    {
        /// <summary>
        /// Tests System.IO.StringReaderTest plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            mDebugger.Send("START TEST: StreamWriter:");

            StringBuilder stringToRead = new StringBuilder();
            stringToRead.AppendLine("Characters in 1st line to read");
            stringToRead.AppendLine("and 2nd line");
            stringToRead.AppendLine("and the end");

            mDebugger.Send("Testing ReadLine");
            string Line;
            using (StringReader reader = new StringReader(stringToRead.ToString()))
            {
                Line = reader.ReadLine();
                Assert.IsTrue(Line == "Characters in 1st line to read", "StringReader.ReadLine() #1 does not work");

                Line = reader.ReadLine();
                Assert.IsTrue(Line == "and 2nd line", "StringReader.ReadLine() #2 does not work");

                Line = reader.ReadLine();
                Assert.IsTrue(Line == "and the end", "StringReader.ReadLine() #3 does not work");

                // The text is finished we should read null to indicate EOF
                Line = reader.ReadLine();
                Assert.IsTrue(Line == null, "StringReader.ReadLine() #4 does not work");
            }

            mDebugger.Send("Testing Read");
            using (StringReader reader = new StringReader("123"))
            {
                int val;

                val = reader.Read();
                Assert.IsTrue(val != -1, "StringReader.Read #1 does not work returns -1");
                Assert.IsTrue((char)val == '1', "StringReader.Read #1 does not work: read wrong value");

                val = reader.Read();
                Assert.IsTrue(val != -1, "StringReader.Read #2 does not work returns -1");
                Assert.IsTrue((char)val == '2', "StringReader.Read #2 does not work: read wrong value");

                val = reader.Read();
                Assert.IsTrue(val != -1, "StringReader.Read #3 does not work returns -1");
                Assert.IsTrue((char)val == '3', "StringReader.Read #3 does not work: read wrong value");

                val = reader.Read();
                Assert.IsTrue(val == -1, "StringReader.Read #4 does not work EOF reached but it does not return -1");
            }

            mDebugger.Send("Testing ReadBlock");
            using (StringReader reader = new StringReader("123"))
            {
                int count;
                int charsRead;

                char[] buffer = new char[3];

                count = buffer.Length;
                charsRead = reader.ReadBlock(buffer, 0, count);

                Assert.IsTrue(charsRead == count, "StringReader.ReadBlock does not work the expected number of characters has not been read");
                Assert.IsTrue(new string(buffer, 0, charsRead) == "123", "StringReader.ReadBlock does not work");
            }

            mDebugger.Send("Testing ReadBlock (with bigger buffer)");
            using (StringReader reader = new StringReader("123"))
            {
                int count;
                int charsRead;

                char[] buffer = new char[4];

                count = buffer.Length;
                charsRead = reader.ReadBlock(buffer, 0, count);

                // It should always read 3 chars (not 4 as count is)
                Assert.IsTrue(charsRead == 3, "StringReader.ReadBlock read more characters that in the sting are");
                Assert.IsTrue(charsRead != count, "StringReader.ReadBlock does not work the expected number of characters has not been read");
                Assert.IsTrue(new string(buffer, 0, charsRead) == "123", "StringReader.ReadBlock does not work");
            }

            using (StringReader reader = new StringReader("12345"))
            {
                mDebugger.Send("Testing ReadBlock (with smaller buffer)");

                int count;
                int charsRead;

                char[] buffer = new char[2];

                count = buffer.Length;
                charsRead = reader.ReadBlock(buffer, 0, count);

                Assert.IsTrue(charsRead == count, "StringReader.ReadBlock does not work the expected number of characters has not been read");
                Assert.IsTrue(new string(buffer, 0, charsRead) == "12", "StringReader.ReadBlock does not work");

                mDebugger.Send("Testing ReadToEnd");

                var EndOfReader = reader.ReadToEnd();
                Assert.IsTrue(EndOfReader == "345", "StringReader.ReadToEnd does not work");
            }

            mDebugger.Send("END TEST");
        }
    }
}
