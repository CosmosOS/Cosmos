using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Xml;
using Instruction=Indy.IL2CPU.Assembler.X86.Instruction;
using System.Reflection.Emit;

namespace Indy.IL2CPU.Tests.AssemblerTests.X86
{
    public partial class InvalidOpcodeTester
    {
        //PS. 16 bit registers function INCLUDES segment registers and pointers
        //PS2. By default, Control registers ARE NOT TESTED with the opcode. To test him, add a constraint with testCR = true on the opcode you wish to test
        //PS3. Do not add control register as invalid register since testCR handles this.
        //PS4. SSE/MMX instruction uses MM/XMM registers. You should not specify any constrains as it is automatically covered by checking the instruction namespace.
        class ConstraintsContainer {
            public Constraints SourceInfo;
            public Constraints DestInfo;
            public Instruction.InstructionSizes InvalidSizes = Instruction.InstructionSizes.None;
            public bool MemToMem = true;
        }

        public class TestState {
            public string Description;
            public bool Success;
            public string Message;
        }

        class Constraints
        {
            public bool TestMem32=true;
            public bool TestMem16=true;
            public bool TestMem8=true;
            public bool TestImmediate8=true;
            public bool TestImmediate16=true;
            public bool TestImmediate32=true;
            public bool TestRegisters = true;
            public bool TestCR = false;
            public bool TestSegments = false;
            public IEnumerable<Guid> InvalidRegisters = new Guid[0];
        }

        private const int VerificationLevel = 1;
        private static string nasmPath;
        private static Dictionary<Type, ConstraintsContainer> opcodesException;

        public static Indy.IL2CPU.Assembler.X86.Assembler x86Assembler
        {
            get;
            private set;
        }
        private static List<TestState> mTestStates;
        private static void WriteStatusEntry(string aDescription, bool aSuccess, string aMessage) {
            mTestStates.Add(new TestState {
                Description=aDescription,
                Success = aSuccess,
                Message = aMessage
            });
        }

        public static void Main() {
            Execute();
        }

        public static void ExecuteSingle(Type type, int level) {
            if (type.IsAbstract) {
                return;
            }
            if (!type.IsSubclassOf(typeof(Instruction))) {
                return;
            }
            Console.WriteLine("Testing " + type.Name);
            var xDoTest = new Action<Type, int, string, Action<Instruction>>(delegate(Type aType, int aLevel, string aDescription, Action<Instruction> aInit) {
                if (aInit == null) {
                    aInit = delegate { };
                }
                if (aType.IsSubclassOf(typeof(Assembler.X86.InstructionWithDestinationAndSourceAndSize))) {
                    TestInstructionWithDestinationAndSourceAndSize(aType,
                                                                   aLevel, aDescription,
                                                                   aInstruction => aInit((Instruction)aInstruction));
                    return;
                }
                if (aType.IsSubclassOf(typeof(Assembler.X86.InstructionWithDestinationAndSource))) {
                    TestInstructionWithDestinationAndSource(aType,
                                                            aLevel, aDescription,
                                                            aInstruction => aInit((Instruction)aInstruction));
                    return;
                }
                if (aType.IsSubclassOf(typeof(Assembler.X86.InstructionWithDestinationAndSize))) {
                    TestInstructionWithDestinationAndSize(aType,
                                                          aLevel, aDescription,
                                                          aInstruction => aInit((Instruction)aInstruction));
                    return;
                }
                if (aType.IsSubclassOf(typeof(Assembler.X86.InstructionWithDestination))) {
                    TestInstructionWithDestination(aType,
                                                   aLevel, aDescription,
                                                   aInstruction => aInit((Instruction)aInstruction));
                    return;
                }
                if (type.IsSubclassOf(typeof(Assembler.X86.InstructionWithSize))) {
                    TestInstructionWithSize(aType,
                                            aLevel,
                                            aDescription,
                                            aInstruction => aInit((Instruction)aInstruction));
                    return;
                }
                if (aType.IsSubclassOf(typeof(Assembler.X86.Instruction))) {
                    TestSimpleInstruction(aType, aDescription, aInit);
                    return;
                }
            });
            xDoTest(type, level, type.Name, null);
            Verify(type.Name);
        }
        public static void Initialize() {
            //Pre-initialization
            if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output"))) {
                Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    "Output"));
            }
            nasmPath = Directory.GetCurrentDirectory().ToLowerInvariant();
            int xPos = nasmPath.LastIndexOf("source");
            nasmPath = nasmPath.Substring(0, xPos) + @"Build\Tools\NAsm\nasm.exe";

            opcodesException = new Dictionary<Type, ConstraintsContainer>();
            AddExceptions();

            x86Assembler = new Assembler.X86.Assembler();
            x86Assembler.Initialize();
            x86Assembler.Instructions.Clear();
            x86Assembler.DataMembers.Clear();
            mTestStates = new List<TestState>();
        }
        public static void Execute()
        {
            Console.TreatControlCAsInput=true;
            Initialize();            
            //Determine opcodes
            Assembly xAsm = Assembly.Load("Indy.IL2CPU");
            try {
                foreach (Type type in xAsm.GetTypes()) {
                    ExecuteSingle(type, 0);
                }
            }catch(AbortException){
                Console.WriteLine("Testing interrupted, still generating test results page.");
            }catch(Exception E){
                throw new Exception("Error while testing", E);
            } finally {
                // output results to html
                var xFile = Path.Combine(Path.GetDirectoryName(typeof(InvalidOpcodeTester).Assembly.Location), "TestResults.html");
                GenerateHtml(xFile);
                xFile = Path.Combine(Path.GetDirectoryName(typeof(InvalidOpcodeTester).Assembly.Location), "TestResults.xml");
                GenerateXml(xFile);
                Console.WriteLine("Tests finished. Results have been written to '{0}'", xFile);
                Console.ReadLine();
            }
        }

        private static void TestInstructionWithSize(Type type, int aTestLevel, string aDescription, Action<IInstructionWithSize> aInitInstruction) {
            ConstraintsContainer xInfo=null;
            if (opcodesException.ContainsKey(type)) {
                xInfo = opcodesException[type];
            }
            if (aInitInstruction == null) {
                aInitInstruction = delegate { };
            }
            if (!(xInfo != null && ((xInfo.InvalidSizes & Instruction.InstructionSizes.DWord) != 0))) {
                var xInstruction = CreateInstruction<IInstructionWithSize>(type, 32);
                aInitInstruction(xInstruction);
                if (aTestLevel <= VerificationLevel) {
                    Verify(aDescription + ", size 32");
                }
            }
            if (!(xInfo != null && ((xInfo.InvalidSizes & Instruction.InstructionSizes.Word) != 0))) {
                var xInstruction = CreateInstruction<IInstructionWithSize>(type, 16);
                aInitInstruction(xInstruction);
                if (aTestLevel <= VerificationLevel) {
                    Verify(aDescription + ", size 32");
                }
            }
            if (!(xInfo != null && ((xInfo.InvalidSizes & Instruction.InstructionSizes.Byte) != 0))) {
                var xInstruction = CreateInstruction<IInstructionWithSize>(type, 8);
                aInitInstruction(xInstruction);
                if (aTestLevel <= VerificationLevel) {
                    Verify(aDescription + ", size 32");
                }
            }
            if (aTestLevel <= VerificationLevel) {
                Verify(aDescription);
            }
        }

        public class AbortException: Exception {
         //   
        }

        public static void GenerateXml(string aFile) {
            using(var xWriter = XmlWriter.Create(aFile)) {
                xWriter.WriteStartDocument();
                xWriter.WriteStartElement("TestResults");
                {
                    foreach (var xItem in mTestStates) {
                        xWriter.WriteStartElement("TestCase");
                        {
                            xWriter.WriteAttributeString("Description", xItem.Description);
                            xWriter.WriteAttributeString("Message", xItem.Message);
                            xWriter.WriteAttributeString("Succeeded", xItem.Success.ToString());
                        }
                        xWriter.WriteEndElement();
                    }
                }
                xWriter.WriteEndElement(); // 
                xWriter.WriteEndDocument();
            }
        }

        public static void GenerateHtml(string aFile) {
            using(var xWriter = new StreamWriter(aFile, false)) {
                xWriter.WriteLine("<html><body>");
                xWriter.WriteLine("<h1>Failing Tests:</h1><br/>");
                Action<IEnumerable<TestState>> WriteList = delegate(IEnumerable<TestState> aList) {
                    foreach (var xItem in aList) {
                        xWriter.WriteLine("<tr>");
                        xWriter.WriteLine("<td>{0}</td>", xItem.Description);
                        xWriter.WriteLine("<td>{0}</td>", xItem.Success);
                        xWriter.WriteLine("<td>{0}</td>", xItem.Message);
                        xWriter.WriteLine("</tr>");
                    }
                };
                xWriter.WriteLine("<table>");
                xWriter.WriteLine("<thead>");
                xWriter.WriteLine("<th>Description</th>");
                xWriter.WriteLine("<th>Success</th>");
                xWriter.WriteLine("<th>Message</th>");
                xWriter.WriteLine("<tbody>");
                WriteList((from item in mTestStates
                           where !item.Success
                           select item));
                xWriter.WriteLine("</tbody></table>");
                xWriter.WriteLine("<h1>Passing tests:</h1></br>");
                xWriter.WriteLine("<table>");
                xWriter.WriteLine("<thead>");
                xWriter.WriteLine("<th>Description</th>");
                xWriter.WriteLine("<th>Success</th>");
                xWriter.WriteLine("<th>Message</th>");
                xWriter.WriteLine("</thead>");
                xWriter.WriteLine("<tbody>");
                WriteList((from item in mTestStates
                           where item.Success
                           select item));
                xWriter.WriteLine("</tbody></table>");
                xWriter.Flush();
            }
        }

        private static void TestInstructionWithDestination(Type type, int aLevel, string aDescription, Action<IInstructionWithDestination> aInitInstruction)
        {
            TestInstructionWithDestination(type, 0, aLevel, aDescription, aInitInstruction);
        }

        private static T CreateInstruction<T>(Type t, byte aSize) {
            var xResult = (T)Activator.CreateInstance(t);
            if(aSize!=0) {
                ((IInstructionWithSize)xResult).Size = aSize;
            }
            return xResult;
        }

        private static void TestInstructionWithDestination(Type type, byte size, int aLevel, string aDescription, Action<IInstructionWithDestination> aInitInstruction)
        {
            if(aInitInstruction==null) {
                aInitInstruction = delegate { };
            }
            IInstructionWithDestination xInstruction=null;
            //Test Immediate 8
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestImmediate8))
            {
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                xInstruction.DestinationValue = 30;
                aInitInstruction(xInstruction);
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 8bit immediate");
                }
            }
            
            //Test Immediate 16
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestImmediate16))
            {
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 300;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 16bit immediate");
                }
            }
            //Test Immediate 32
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestImmediate32))
            {
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 300000;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 32bit immediate");
                }
            }
            //memory 8 bits
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem8)) {
                //no offset
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect = true;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 8bit memory, no offset");
                }
                //offset 8
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 203;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 8bit memory, 8bit offset");
                }
                //offset 16
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 2030;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 8bit memory, 16bit offset");
                }
                //offset 32
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement=70000;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 8bit memory, 32bit offset");
                }
            }
            //memory 16 bits
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem16))
            {   
                //no offset
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect=true;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 16bit memory, no offset");
                }
                //offset 8
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 203;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 16bit memory, 8bit offset");
                }
                //offset 16
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 2003;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 16bit memory, 16bit offset");
                }
                //offset 32
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 70000;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 16bit memory, 32bit offset");
                }
            }
            // mem32
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem32)) {
                //no offset
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 32bit memory, no offset");
                }
                //offset 8
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 203;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 32bit memory, 8bit offset");
                }
                //offset 16
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 2003;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 32bit memory, 16bit offset");
                }
                //offset 32
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 70000;
                if (aLevel <= VerificationLevel) {
                    Verify(aDescription + ", destination 32bit memory, 32bit offset");
                }
            }
            if (!opcodesException.ContainsKey(type) || opcodesException[type].DestInfo.TestRegisters) {
                var xRegistersToSkip = new List<Guid>();
                if (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.InvalidRegisters != null) {
                    xRegistersToSkip.AddRange(opcodesException[type].DestInfo.InvalidRegisters);
                }
                foreach (var xReg in Registers.GetRegisters()) {
                    if (xRegistersToSkip.Contains(xReg)) {
                        continue;
                    }
                    if (!Registers.Is32Bit(xReg)) {
                        continue;
                    }
                    if (Registers.IsCR(xReg)) {
                        continue;
                    }
                    //memory 8 bits
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem8)) {
                        //no offset
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 8bit memory at reg, no offset");
                        }
                        //offset 8
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 8bit memory at reg, 8bit offset");
                        }
                        //offset 16
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2030;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 8bit memory at reg, 16bit offset");
                        }
                        //offset 32
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 70000;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 8bit memory at reg, 32bit offset");
                        }
                    }
                    //memory 16 bits
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem16)) {
                        //no offset
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 16bit memory at reg, no offset");
                        }
                        //offset 8
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 16bit memory at reg, 8bit offset");
                        }
                        //offset 16
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2003;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 16bit memory at reg, 16bit offset");
                        }
                        //offset 32
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 70000;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 16bit memory at reg, 32bit offset");
                        }
                    }
                    // mem32
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem32)) {
                        //no offset
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 32bit memory at reg, no offset");
                        }
                        //offset 8
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 32bit memory at reg, 8bit offset");
                        }
                        //offset 16
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2003;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 32bit memory at reg, 16bit offset");
                        }
                        //offset 32
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 70000;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination 32bit memory at reg, 32bit offset");
                        }
                    }
                }
                foreach (Guid register in Registers.GetRegisters()) {
                    if (!type.Namespace.Contains("SSE") && (Registers.getXMMs().Contains(register)))
                        continue;
                    if (Registers.GetCRs().Contains(register) && (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].DestInfo.TestCR))))
                        continue;
                    if ((!opcodesException.ContainsKey(type) ||
                         (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.InvalidRegisters != null &&
                           (opcodesException[type].DestInfo.InvalidRegisters.Contains(register)))))
                        continue;
                    if (Registers.IsSegment(register) && !(opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestSegments)) {
                        continue;
                    }
                    if (size == 0 || Registers.GetSize(register) == size) {
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = register;
                        if (aLevel <= VerificationLevel) {
                            Verify(aDescription + ", destination reg");
                        }
                    }
                }
            }

        }

        private static void TestInstructionWithSource(Type type, byte size, int aLevel, string aDescription, Action<IInstructionWithDestination> aInitInstruction) {
            //Test Immediate 8
            if (aInitInstruction == null) {
                aInitInstruction = delegate { };
            }
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate8)) {
                TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", 8-bit source value", delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    aInitInstruction(aInstruction);
                    xInstruction.SourceValue = 30;
                });
            }

            //Test Immediate 16
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate16)) {
                TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", 16-bit source value", delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    aInitInstruction(aInstruction);
                    xInstruction.SourceValue = 300;
                });
            }
            //Test Immediate 32
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate32)) {
                TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", 32-bit source value", delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    aInitInstruction(aInstruction);
                    xInstruction.SourceValue = 300000;
                });
            }
            bool xOrigMem8 = false;
            bool xOrigMem16 = false;
            bool xOrigMem32 = false;
            bool xNeedsReset = false;
            if (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo!=null) {
                if (!opcodesException[type].MemToMem) {
                    xNeedsReset = true;
                    xOrigMem8 = opcodesException[type].DestInfo.TestMem8;
                    xOrigMem16 = opcodesException[type].DestInfo.TestMem16;
                    xOrigMem32 = opcodesException[type].DestInfo.TestMem32;
                    opcodesException[type].DestInfo.TestMem8 = false;
                    opcodesException[type].DestInfo.TestMem16 = false;
                    opcodesException[type].DestInfo.TestMem32 = false;
                }
            }
            try {
                //memory 8 bits
                if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem8)) {
                    //no offset
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 8-bit memory location, no offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 65;
                        xInstruction.SourceIsIndirect = true;
                    });
                    //offset 8
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 8-bit memory location, 8-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 65;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 203;
                    });
                    //offset 16
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 8-bit memory location, 16-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 65;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 2030;
                    });
                    //offset 32
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 8-bit memory location, 32-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 65;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 70000;
                    });
                }
                //memory 16 bits
                if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem16)) {
                    //no offset
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 16-bit memory location, no offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 650;
                        xInstruction.SourceIsIndirect = true;
                    });
                    //offset 8
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 16-bit memory location, 8-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 650;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 203;
                    });
                    //offset 16
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 16-bit memory location, 16-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 650;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 2003;
                    });
                    //offset 32
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 16-bit memory location, 32-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 650;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 70000;
                    });
                }
                // mem32
                if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem32)) {
                    //no offset
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 32-bit memory location, no offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 650000;
                        xInstruction.SourceIsIndirect = true;
                    });
                    //offset 8
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 32-bit memory location, 8-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 650000;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 203;
                    });
                    //offset 16
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 32-bit memory location, 16-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 650000;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 2003;
                    });
                    //offset 32
                    TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source 32-bit memory location, 32-bit offset", delegate(IInstructionWithDestination aInstruction) {
                        var xInstruction = (IInstructionWithSource)aInstruction;
                        aInitInstruction(aInstruction);
                        xInstruction.SourceValue = 650000;
                        xInstruction.SourceIsIndirect = true;
                        xInstruction.SourceDisplacement = 70000;
                    });
                }
                if (!opcodesException.ContainsKey(type) || opcodesException[type].SourceInfo.TestRegisters) {
                    var xRegistersToSkip = new List<Guid>();
                    if (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.InvalidRegisters != null) {
                        xRegistersToSkip.AddRange(opcodesException[type].SourceInfo.InvalidRegisters);
                    }
                    foreach (var xReg in Registers.GetRegisters()) {
                        if (xRegistersToSkip.Contains(xReg)) {
                            continue;
                        }
                        if (!Registers.Is32Bit(xReg)) {
                            continue;
                        }
                        if (Registers.IsCR(xReg)) {
                            continue;
                        }
                        //memory 8 bits
                        if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem8)) {
                            //no offset
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 8-bit, no offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                            });
                            //offset 8
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 8-bit, 8-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 203;
                            });
                            //offset 16
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 8-bit, 16-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 2030;
                            });
                            //offset 32
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 8-bit, 32-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 70000;
                            });
                        }
                        //memory 16 bits
                        if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem16)) {
                            //no offset
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 16-bit, no offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                            });
                            //offset 8
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 16-bit, 8-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 203;
                            });
                            //offset 16
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 16-bit, 16-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 2003;
                            });
                            //offset 32
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 16-bit, 32-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 70000;
                            });
                        }
                        // mem32
                        if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem32)) {
                            //no offset
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 32-bit, no offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                            });
                            //offset 8
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 32-bit, 8-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 203;
                            });
                            //offset 16
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 32-bit, 16-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 2003;
                            });
                            //offset 32
                            TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg memory location 32-bit, 32-bit offset", delegate(IInstructionWithDestination aInstruction) {
                                var xInstruction = (IInstructionWithSource)aInstruction;
                                aInitInstruction(aInstruction);
                                xInstruction.SourceReg = xReg;
                                xInstruction.SourceIsIndirect = true;
                                xInstruction.SourceDisplacement = 70000;
                            });
                        }
                    }
                }
            } finally {
                if (xNeedsReset) {
                    opcodesException[type].DestInfo.TestMem8 = xOrigMem8;
                    opcodesException[type].DestInfo.TestMem16 = xOrigMem16;
                    opcodesException[type].DestInfo.TestMem32 = xOrigMem32;
                }
            }
            foreach (Guid register in Registers.GetRegisters()) {
                if (!type.Namespace.Contains("SSE") && (Registers.getXMMs().Contains(register)))
                    continue;
                if (Registers.GetCRs().Contains(register) && 
                    (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].SourceInfo.TestCR))))
                    continue;
                if (Registers.GetRegisters().Contains(register) && 
                    (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!(opcodesException[type].SourceInfo.InvalidRegisters != null || opcodesException[type].SourceInfo.InvalidRegisters.Contains(register))))))
                    continue;
                if (Registers.IsSegment(register) && opcodesException.ContainsKey(type) && (opcodesException[type].SourceInfo == null || !opcodesException[type].SourceInfo.TestSegments)) {
                    continue;
                }
                if (Registers.GetSize(register) != size) {
                    continue;
                }
                TestInstructionWithDestination(type, size, aLevel + 1, aDescription + ", source reg", delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    aInitInstruction(aInstruction);
                    xInstruction.SourceReg = register;
                });
            }
            if (aLevel <= VerificationLevel) {
                Verify(aDescription);
            }
        }

        private static void TestInstructionWithDestinationAndSize(Type type, int aLevel, string aDescription, Action<IInstructionWithDestination> aInitInstruction)
        {
            if (!opcodesException.ContainsKey(type) || ((opcodesException[type].InvalidSizes & Instruction.InstructionSizes.Byte) == 0)){
                TestInstructionWithDestination(type, 8, aLevel + 1, aDescription + ", size 8", aInitInstruction);
            }
            if (!opcodesException.ContainsKey(type) || ((opcodesException[type].InvalidSizes & Instruction.InstructionSizes.Word) == 0)) {
                TestInstructionWithDestination(type, 16, aLevel + 1, aDescription + ", size 16", aInitInstruction);
            }
            if (!opcodesException.ContainsKey(type) || ((opcodesException[type].InvalidSizes & Instruction.InstructionSizes.DWord) == 0)) {
                TestInstructionWithDestination(type, 32, aLevel + 1, aDescription + ", size 32", aInitInstruction);
            }
            if (aLevel <= VerificationLevel) {
                Verify(aDescription);
            }
        }

        private static void TestInstructionWithDestinationAndSource(Type type, int aLevel, string aDescription, Action<IInstructionWithDestination> aInitInstruction)
        {
            TestInstructionWithSource(type, 0, aLevel+1, aDescription, aInitInstruction);
            if (aLevel <= VerificationLevel) {
                Verify(aDescription);
            }
        }

        private static void TestInstructionWithDestinationAndSourceAndSize(Type type, int aLevel, string aDescription, Action<IInstructionWithDestination> aInitInstruction)
        {
            TestInstructionWithSource(type, 8, aLevel + 1, aDescription + ", size 8", aInitInstruction);
            TestInstructionWithSource(type, 16, aLevel + 1, aDescription + ", size 16", aInitInstruction);
            TestInstructionWithSource(type, 32, aLevel + 1, aDescription + ", size 32", aInitInstruction);
            if (aLevel <= VerificationLevel) {
                Verify(aDescription);
            }
        }

        private static void TestSimpleInstruction(Type type, string aDescription, Action<Instruction> aInitInstruction)
        {
            var test = CreateInstruction<Instruction>(type, 0);
            aInitInstruction(test);
            Verify(aDescription);
        }

        private static bool Verify(string aDescription) {
            if (x86Assembler.Instructions.Count == 0) {
                return true;
            }
            String tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output");
            var xNasmWriter = new StreamWriter(Path.Combine(tempPath, "TheOutput.asm"), false);
            FileStream xNasmReader = null;
            bool xResult = false;
            string xMessage=null;
            try {
                using (var xIndy86MS = new FileStream(Path.Combine(tempPath, "TheOutput.bin"), FileMode.Create)) {
                    x86Assembler.FlushText(xNasmWriter);
                    xNasmWriter.Flush();
                    if (xNasmWriter.BaseStream.Length == 0) {
                        throw new Exception("No content found");
                    }
                    xNasmWriter.Close();
                    ProcessStartInfo pinfo = new ProcessStartInfo();
                    pinfo.Arguments = "TheOutput.asm" + " -o " + "TheOutput";
                    var xErrorFile = Path.Combine(tempPath, "TheOutput.err");
                    pinfo.Arguments += " -E\"" + xErrorFile + "\"";
                    if (File.Exists(xErrorFile)) {
                        File.Delete(xErrorFile);
                    }
                    pinfo.WorkingDirectory = tempPath;
                    pinfo.FileName = nasmPath;
                    pinfo.UseShellExecute ^= true;
                    pinfo.CreateNoWindow = true;
                    pinfo.RedirectStandardOutput = true;
                    pinfo.RedirectStandardError = true;
                    Process xProc = Process.Start(pinfo);
                    xProc.WaitForExit();
                    if (xProc.ExitCode != 0) {
                        xResult = false;
                        xMessage = "Error while invoking nasm.exe";
                        goto WriteResult;
                    } else {
                        xNasmReader = File.OpenRead(Path.Combine(tempPath, "TheOutput"));
                        x86Assembler.FlushBinary(xIndy86MS, 0x200000);
                        xIndy86MS.Position = 0;
                        if (xNasmReader.Length != xIndy86MS.Length) {
                            xNasmReader.Close();
                            xMessage = "Binary size mismatch";
                            goto WriteResult;
                        }
                        while (true) {
                            var xVerData = xNasmReader.ReadByte();
                            var xActualData = xIndy86MS.ReadByte();
                            if (xVerData != xActualData) {
                                xNasmReader.Close();
                                xMessage = "Binary data mismatch";
                                goto WriteResult;
                            }
                            if (xVerData == -1) {
                                break;
                            }
                        }
                        xNasmReader.Close();
                        xResult = true;
                    }
                }
                WriteResult:
                return xResult;
            } catch (Exception e) {
                if (e.Message != "No valid EncodingOption found!") {
                    xMessage = e.ToString();
                }else{ xMessage = e.Message;}
            } finally {
                WriteStatusEntry(aDescription, xResult, xMessage);
                if (xNasmReader != null)
                    xNasmReader.Close();
                if (xNasmWriter != null)
                    xNasmWriter.Close();
                x86Assembler.Instructions.Clear();
                x86Assembler.DataMembers.Clear();
                if(Console.KeyAvailable) {
                    Console.Read();
                    throw new AbortException();
                }
            }
            return xResult;
        }
        private static void addSegmentRegisters(List<Guid> list)
        {
            list.Add(Registers.CS);
            list.Add(Registers.DS);
            list.Add(Registers.ES);
            list.Add(Registers.SS);
            list.Add(Registers.FS);
            list.Add(Registers.GS);
        }

        private static void add8BitRegisters(List<Guid> list)
        {
            list.Add(Registers.AL);
            list.Add(Registers.CL);
            list.Add(Registers.DL);
            list.Add(Registers.BL);
            list.Add(Registers.AH);
            list.Add(Registers.CH);
            list.Add(Registers.DH);
            list.Add(Registers.BH);
        }


        private static void add16BitRegisters(List<Guid> list)
        {
            addSegmentRegisters(list);
            list.Add(Registers.AX);
            list.Add(Registers.BP);
            list.Add(Registers.BX);
            list.Add(Registers.CX);
            list.Add(Registers.DI);
            list.Add(Registers.DX);
            list.Add(Registers.SI);
            list.Add(Registers.SP);
        }

        private static void add32BitRegisters(List<Guid> list)
        {
            list.Add(Registers.EAX);
            list.Add(Registers.EBP);
            list.Add(Registers.EBX);
            list.Add(Registers.ECX);
            list.Add(Registers.EDI);
            list.Add(Registers.EDX);
            list.Add(Registers.ESI);
            list.Add(Registers.ESP);
        }
    }
}
