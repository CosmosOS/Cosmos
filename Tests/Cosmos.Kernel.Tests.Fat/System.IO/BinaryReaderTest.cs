using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using static Cosmos.Kernel.Tests.Fat.System.IO.HelperMethods;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class BinaryReaderTest
    {
        // TODO change this to only a test (BinaryIOTest?) together with BinaryWriter
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
            /*Error Stack Trace:
             *
             *Error: Exception: System.Exception: Error compiling method 'SystemVoidSystemIOMemoryStreamDisposeSystemBoolean': System.NullReferenceException: Object reference not set to an instance of an object.
             *at Cosmos.IL2CPU.X86.IL.Leave.Execute(MethodInfo aMethod, ILOpCode aOpCode) in Cosmos\source\Cosmos.IL2CPU\IL\Leave.cs:line 17
             *at Cosmos.IL2CPU.AppAssembler.EmitInstructions(MethodInfo aMethod, List`1 aCurrentGroup, Boolean & amp; emitINT3) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 667
             *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 533-- - >; System.NullReferenceException: Object reference not set to an instance of an object.
             *at Cosmos.IL2CPU.X86.IL.Leave.Execute(MethodInfo aMethod, ILOpCode aOpCode) in Cosmos\source\Cosmos.IL2CPU\IL\Leave.cs:line 17
             *at Cosmos.IL2CPU.AppAssembler.EmitInstructions(MethodInfo aMethod, List`1 aCurrentGroup, Boolean & amp; emitINT3) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 667
             *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 533
             *--- End of inner exception stack trace ---
             *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 540
             *at Cosmos.IL2CPU.ILScanner.Assemble() in Cosmos\source\Cosmos.IL2CPU\ILScanner.cs:line 946
             *at Cosmos.IL2CPU.ILScanner.Execute(MethodBase aStartMethod) in Cosmos\source\Cosmos.IL2CPU\ILScanner.cs:line 247
             *at Cosmos.IL2CPU.CompilerEngine.Execute() in Cosmos\source\Cosmos.IL2CPU\CompilerEngine.cs:line 252
             *
             */

            mDebugger.Send("START TEST: BinaryReader");
            mDebugger.Send("Creating FileStream: FileMode.Open");

            using (var xFS = new FileStream(@"0:\binary.bin", FileMode.Open))
            {
                mDebugger.Send("Creating BinaryReader");

                using (var xBR = new BinaryReader(xFS))
                {
                    if (xFS != null)
                    {
                        mDebugger.Send("Start reading");

                        byte[] xBuffer = xBR.ReadBytes(xBytes.Length);
                        Assert.IsTrue(ByteArrayAreEquals(xBytes, xBuffer), "Bytes changed during BinaryWriter and BinaryReader opeartions on FileStream");
                    }
                    else
                    {
                        Assert.IsTrue(false, @"Failed to create StreamReader for file 0:\binary.bin");
                    }
                }
            }

            mDebugger.Send("END TEST");
        }
    }
}
