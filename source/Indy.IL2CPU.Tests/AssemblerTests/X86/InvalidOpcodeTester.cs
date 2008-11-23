using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using System.IO;
using System.Diagnostics;

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
            public int[] InvalidSizes;
        }

        public class TestState {
            public string Instruction;
            public byte Size;
            public bool HasDest;
            public Guid DestReg;
            public uint DestValue;
            public bool DestIsIndirect;
            public int DestDisplacement;
            public bool HasSource;
            public Guid SourceReg;
            public uint SourceValue;
            public bool SourceIsIndirect;
            public int SourceDisplacement;
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
            public bool TestCR=true;
            public IEnumerable<Guid> InvalidRegisters;
        }


        private static string nasmPath;
        private static Dictionary<Type, ConstraintsContainer> opcodesException;

        public static Indy.IL2CPU.Assembler.X86.Assembler x86Assembler
        {
            get;
            private set;
        }
        private static List<TestState> mTestStates;
        private static void WriteStatusEntry(string aInstruction, byte aSize, bool aHasDest, Guid aDestReg, uint aDestValue, bool aDestIsIndirect, int aDestDisplacement,
            bool aHasSource, Guid aSourceReg, uint aSourceValue, bool aSourceIsIndirect, int aSourceDisplacement, bool aSuccess, string aMessage) {
            mTestStates.Add(new TestState {
                Instruction = aInstruction,
                Size=aSize,
                HasDest=aHasDest,
                DestReg=aDestReg,
                DestValue=aDestValue,
                DestIsIndirect=aDestIsIndirect,
                DestDisplacement = aDestDisplacement,
                HasSource=aHasSource,
                SourceReg=aSourceReg,
                SourceValue = aSourceValue,
                SourceIsIndirect = aSourceIsIndirect,
                SourceDisplacement=aSourceDisplacement,
                Success = aSuccess,
                Message = aMessage
            });
        }

        public static void Main() {
            Execute();
        }

        private static void Execute()
        {
            Console.TreatControlCAsInput=true;
            //Pre-initialization
            if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output")))
            {
                Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    "Output"));
            }
            nasmPath = Directory.GetCurrentDirectory().ToLowerInvariant();
            int xPos = nasmPath.LastIndexOf("source");
            nasmPath = nasmPath.Substring(0, xPos) + @"Build\Tools\NAsm\nasm.exe";

            opcodesException = new Dictionary<Type, ConstraintsContainer>();
            AddExceptions();

            x86Assembler=new Assembler.X86.Assembler();
            x86Assembler.Initialize();
            x86Assembler.Instructions.Clear();
            x86Assembler.DataMembers.Clear();
            mTestStates = new List<TestState>();
            //Determine opcodes
            Assembly xAsm = Assembly.Load("Indy.IL2CPU");
            try {
                foreach (Type type in xAsm.GetTypes()) {
                    if (type.IsAbstract)
                        continue;
                    else {
                        if (type.BaseType == typeof(Assembler.X86.InstructionWithDestination)) {
                            TestInstructionWithDestination(type);
                        } else if (type.BaseType == typeof(Assembler.X86.InstructionWithDestinationAndSize)) {
                            TestInstructionWithDestinationAndSize(type);
                        } else if (type.BaseType == typeof(Assembler.X86.InstructionWithDestinationAndSource)) {
                            TestInstructionWithDestinationAndSource(type);
                        } else if (type.BaseType == typeof(Assembler.X86.InstructionWithDestinationAndSourceAndSize)) {
                            TestInstructionWithDestinationAndSourceAndSize(type);
                        } else if (type.BaseType == typeof(Assembler.X86.Instruction)) {
                            TestSimpleInstruction(type);
                        }
                    }
                }
            }catch(AbortException){
                Console.WriteLine("Testing interrupted, still generating test results page.");
            }catch(Exception E){
                throw new Exception("Error while testing", E);
            } finally {
                // output results to html
                var xFile = Path.Combine(Path.GetDirectoryName(typeof(InvalidOpcodeTester).Assembly.Location), "TestResults.html");
                GenerateHtml(xFile);
                Console.WriteLine("Tests finished. Results have been written to '{0}'", xFile);
                Console.ReadLine();
            }
        }

        public class AbortException: Exception {
         //   
        }

        private static void GenerateHtml(string aFile) {
            using(var xWriter = new StreamWriter(aFile, false)) {
                xWriter.WriteLine("<html><body>");
                xWriter.WriteLine("<h1>Failing Tests:</h1><br/>");
                Action<IEnumerable<TestState>> WriteList = delegate(IEnumerable<TestState> aList) {
                    foreach (var xItem in aList) {
                        xWriter.WriteLine("<tr>");
                        xWriter.WriteLine("<td>{0}</td>", xItem.Instruction);
                        xWriter.WriteLine("<td>{0}</td>", xItem.Size);
                        if (xItem.HasDest) {
                            if (xItem.DestReg == Guid.Empty) {
                                xWriter.WriteLine("<td></td>");
                            } else {
                                xWriter.WriteLine("<td>{0}</td>", Registers.GetRegisterName(xItem.DestReg));
                            }
                            xWriter.WriteLine("<td>{0}</td>", xItem.DestValue);
                            xWriter.WriteLine("<td>{0}</td>", xItem.DestIsIndirect);
                            xWriter.WriteLine("<td>{0}</td>", xItem.DestDisplacement);
                        }else {
                            xWriter.WriteLine("<td></td>");
                            xWriter.WriteLine("<td></td>");
                            xWriter.WriteLine("<td></td>");
                            xWriter.WriteLine("<td></td>");
                        }
                        if (xItem.HasSource) {
                            if (xItem.SourceReg == Guid.Empty) {
                                xWriter.WriteLine("<td></td>");
                            } else {
                                xWriter.WriteLine("<td>{0}</td>", Registers.GetRegisterName(xItem.SourceReg));
                            }
                            xWriter.WriteLine("<td>{0}</td>", xItem.SourceValue);
                            xWriter.WriteLine("<td>{0}</td>", xItem.SourceIsIndirect);
                            xWriter.WriteLine("<td>{0}</td>", xItem.SourceDisplacement);
                        } else {
                            xWriter.WriteLine("<td></td>");
                            xWriter.WriteLine("<td></td>");
                            xWriter.WriteLine("<td></td>");
                            xWriter.WriteLine("<td></td>");
                        }
                        xWriter.WriteLine("<td>{0}</td>", xItem.Message);
                        xWriter.WriteLine("</tr>");
                    }
                };
                xWriter.WriteLine("<table>");
                xWriter.WriteLine("<thead>");
                xWriter.WriteLine("<th>Instruction</th>");
                xWriter.WriteLine("<th>Size</th>");
                xWriter.WriteLine("<th>DestReg</th>");
                xWriter.WriteLine("<th>DestValue</th>");
                xWriter.WriteLine("<th>DestIsIndirect</th>");
                xWriter.WriteLine("<th>DestDisplacement</th>");
                xWriter.WriteLine("<th>SourceReg</th>");
                xWriter.WriteLine("<th>SourceValue</th>");
                xWriter.WriteLine("<th>SourceIsIndirect</th>");
                xWriter.WriteLine("<th>SourceDisplacement</th>");
                xWriter.WriteLine("</thead>");
                xWriter.WriteLine("<tbody>");
                WriteList((from item in mTestStates
                           where !item.Success
                           select item));
                xWriter.WriteLine("</tbody></table>");
                xWriter.WriteLine("<h1>Passing tests:</h1></br>");
                xWriter.WriteLine("<table>");
                xWriter.WriteLine("<thead>");
                xWriter.WriteLine("<th>Instruction</th>");
                xWriter.WriteLine("<th>Size</th>");
                xWriter.WriteLine("<th>DestReg</th>");
                xWriter.WriteLine("<th>DestValue</th>");
                xWriter.WriteLine("<th>DestIsIndirect</th>");
                xWriter.WriteLine("<th>DestDisplacement</th>");
                xWriter.WriteLine("<th>SourceReg</th>");
                xWriter.WriteLine("<th>SourceValue</th>");
                xWriter.WriteLine("<th>SourceIsIndirect</th>");
                xWriter.WriteLine("<th>SourceDisplacement</th>");
                xWriter.WriteLine("</thead>");
                xWriter.WriteLine("<tbody>");
                WriteList((from item in mTestStates
                           where item.Success
                           select item));
                xWriter.WriteLine("</tbody></table>");
                xWriter.Flush();
            }
        }

        private static void TestInstructionWithDestination(Type type)
        {
            TestInstructionWithDestination(type, 0, null);
        }

        private static T CreateInstruction<T>(Type t, byte aSize) {
            var xResult = (T)Activator.CreateInstance(t);
            if(aSize!=0) {
                ((IInstructionWithSize)xResult).Size = aSize;
            }
            return xResult;
        }

        private static void TestInstructionWithDestination(Type type, byte size, Action<IInstructionWithDestination> aInitInstruction)
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
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
                Console.Write("\t --> Immediate 8: ");
                computeResult();
            }
            
            //Test Immediate 16
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestImmediate16))
            {
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 300;
                Console.Write("\t --> Immediate 16: ");
                computeResult();
            }
            //Test Immediate 32
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestImmediate32))
            {
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 300000;
                Console.Write("\t --> Immediate 32: ");
                computeResult();
            }
            //memory 8 bits
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem8)) {
                //no offset
                Console.WriteLine("\t --> Mem");
                Console.Write("\t\t --> no offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect = true;
                computeResult();
                //offset 8
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 8bit offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 203;
                computeResult();
                //offset 16
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 16bit offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 2030;
                computeResult();
                //offset 32
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 32bit offset");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement=70000;
                computeResult();
            }
            //memory 16 bits
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem16))
            {   
                //no offset
                Console.WriteLine("\t --> Mem16");
                Console.Write("\t\t --> no offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect=true;
                computeResult();
                //offset 8
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 8bit offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 203;
                computeResult();
                //offset 16
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 16bit offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 2003;
                computeResult();
                //offset 32
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 32bit offset");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 70000;
                computeResult();
            }
            // mem32
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem32)) {
                //no offset
                Console.WriteLine("\t --> Mem32");
                Console.Write("\t\t --> no offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                computeResult();
                //offset 8
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 8bit offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 203;
                computeResult();
                //offset 16
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 16bit offset: ");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 2003;
                computeResult();
                //offset 32
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 32bit offset");
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 70000;
                computeResult();
            }
            if(!opcodesException.ContainsKey(type) || opcodesException[type].DestInfo.TestRegisters) {
                var xRegistersToSkip = new List<Guid>();
                if (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.InvalidRegisters!=null) {
                    xRegistersToSkip.AddRange(opcodesException[type].DestInfo.InvalidRegisters);
                }
                foreach(var xReg in Registers.GetRegisters()) {
                    if (xRegistersToSkip.Contains(xReg)) {
                        continue;
                    }
                    //memory 8 bits
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem8)) {
                        //no offset
                        Console.WriteLine("\t --> Reg-Mem8");
                        Console.Write("\t\t --> no offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        computeResult();
                        //offset 8
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 8bit offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        computeResult();
                        //offset 16
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 16bit offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2030;
                        computeResult();
                        //offset 32
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 32bit offset");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 70000;
                        computeResult();
                    }
                    //memory 16 bits
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem16)) {
                        //no offset
                        Console.WriteLine("\t --> Reg-Mem16");
                        Console.Write("\t\t --> no offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        computeResult();
                        //offset 8
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 8bit offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        computeResult();
                        //offset 16
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 16bit offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2003;
                        computeResult();
                        //offset 32
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 32bit offset");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 70000;
                        computeResult();
                    }
                    // mem32
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem32)) {
                        //no offset
                        Console.WriteLine("\t --> Reg-Mem32");
                        Console.Write("\t\t --> no offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        computeResult();
                        //offset 8
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 8bit offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        computeResult();
                        //offset 16
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 16bit offset: ");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2003;
                        computeResult();
                        //offset 32
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 32bit offset");
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 70000;
                        computeResult();
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\tRegisters");
            foreach (Guid register in Registers.GetRegisters()) {
                if (!type.Namespace.Contains("SSE") && (Registers.getXMMs().Contains(register)))
                    continue;
                if (Registers.getCRs().Contains(register) && (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].DestInfo.TestCR))))
                    continue;
                if (Registers.GetRegisters().Contains(register) && (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].DestInfo.InvalidRegisters.Contains(register)))))
                    continue;
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationReg = register;
                computeResult();
            }
        }

        private static void TestInstructionWithSource(Type type, byte size) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            //Test Immediate 8
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate8)) {
                Console.Write("\t --> Immediate 8: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 30;
                });
            }

            //Test Immediate 16
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate16)) {
                Console.Write("\t --> Immediate 16: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 300;
                });
            }
            //Test Immediate 32
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate32)) {
                Console.Write("\t --> Immediate 32: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 300000;
                });
            }
            //memory 8 bits
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem8)) {
                //no offset
                Console.WriteLine("\t --> Mem");
                Console.Write("\t\t --> no offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 65;
                    xInstruction.SourceIsIndirect = true;
                });
                //offset 8
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 8bit offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 65;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 203;
                });
                //offset 16
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 16bit offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 65;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 2030;
                });
                //offset 32
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 32bit offset");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 65;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 70000;
                });
            }
            //memory 16 bits
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem16)) {
                //no offset
                Console.WriteLine("\t --> Mem16");
                Console.Write("\t\t --> no offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650;
                    xInstruction.SourceIsIndirect = true;
                });
                //offset 8
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 8bit offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 203;
                });
                //offset 16
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 16bit offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 2003;
                });
                //offset 32
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 32bit offset");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 70000;
                });
            }
            // mem32
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem32)) {
                //no offset
                Console.WriteLine("\t --> Mem32");
                Console.Write("\t\t --> no offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650000;
                    xInstruction.SourceIsIndirect = true;
                });
                //offset 8
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 8bit offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650000;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 203;
                });
                //offset 16
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 16bit offset: ");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650000;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 2003;
                });
                //offset 32
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 32bit offset");
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
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
                    //memory 8 bits
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem8)) {
                        //no offset
                        Console.WriteLine("\t --> Reg-Mem8");
                        Console.Write("\t\t --> no offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                        });
                        //offset 8
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 8bit offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 203;
                        });
                        //offset 16
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 16bit offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 2030;
                        });
                        //offset 32
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 32bit offset");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 70000;
                        });
                    }
                    //memory 16 bits
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem16)) {
                        //no offset
                        Console.WriteLine("\t --> Reg-Mem16");
                        Console.Write("\t\t --> no offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                        });
                        //offset 8
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 8bit offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 203;
                        });
                        //offset 16
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 16bit offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 2003;
                        });
                        //offset 32
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 32bit offset");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 70000;
                        });
                    }
                    // mem32
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem32)) {
                        //no offset
                        Console.WriteLine("\t --> Reg-Mem32");
                        Console.Write("\t\t --> no offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                        });
                        //offset 8
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 8bit offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 203;
                        });
                        //offset 16
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 16bit offset: ");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 2003;
                        });
                        //offset 32
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\t\t --> 32bit offset");
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 70000;
                        });
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\tRegisters");
            foreach (Guid register in Registers.GetRegisters()) {
                if (!type.Namespace.Contains("SSE") && (Registers.getXMMs().Contains(register)))
                    continue;
                if (Registers.getCRs().Contains(register) && (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].SourceInfo.TestCR))))
                    continue;
                if (Registers.GetRegisters().Contains(register) && (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].SourceInfo.InvalidRegisters.Contains(register)))))
                    continue;
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceReg = register;
                });
            }
        }

        private static void TestInstructionWithDestinationAndSize(Type type)
        {
            Console.ForegroundColor=ConsoleColor.Cyan;
            Console.WriteLine("-->Size 8");
            TestInstructionWithDestination(type, 8, null);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-->Size 16");
            TestInstructionWithDestination(type, 16, null);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-->Size 32");
            TestInstructionWithDestination(type, 32, null);
        }

        private static void TestInstructionWithDestinationAndSource(Type type)
        {
            TestInstructionWithDestinationAndSource(type, 0);
        }

        private static void TestInstructionWithDestinationAndSource(Type type, byte size)
        {
            PropertyInfo source = type.GetProperty("SourceValue");
            PropertyInfo sindirect = type.GetProperty("SourceIsIndirect");
            PropertyInfo sreg = type.GetProperty("SourceReg");
         /*
            Console.ForegroundColor = ConsoleColor.Yellow;
            //Test Immediate 8-->reg
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].testImmediate8))
            {
                var test = Activator.CreateInstance(type);
                dest.SetValue(test, (UInt32)30, new object[0]);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                Console.Write("\t --> Immediate 8: ");
                if (Verify())
                    Console.Write("OK!");
                else
                    Console.Write("Wrong data emitted");
                Console.WriteLine();
            }

            //Test Immediate 16
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].testImmediate16))
            {
                var test = Activator.CreateInstance(type);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                dest.SetValue(test, (UInt32)300, new object[0]);
                Console.Write("\t --> Immediate 16: ");
                if (Verify())
                    Console.Write("OK!");
                else
                    Console.Write("Wrong data emitted");
                Console.WriteLine();
            }
            //Test Immediate 32
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].testImmediate16))
            {
                var test = Activator.CreateInstance(type);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                dest.SetValue(test, (UInt32)300000, new object[0]);
                Console.Write("\t --> Immediate 32: ");
                if (Verify())
                    Console.Write("OK!");
                else
                    Console.Write("Wrong data emitted");
                Console.WriteLine();
            }
            //memory 16 bits
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].testMem16))
            {
                //no offset
                Console.WriteLine("\t --> Mem16");
                Console.Write("\t\t --> no offset: ");
                var test = Activator.CreateInstance(type);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                dest.SetValue(test, (UInt32)65, new object[0]);
                dindirect.SetValue(test, true, new object[0]);
                if (Verify())
                    Console.Write("OK!");
                else
                    Console.Write("Wrong data emitted");
                Console.WriteLine();
                //offset 16
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 16bit offset: ");
                test = Activator.CreateInstance(type);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                dest.SetValue(test, (UInt32)65, new object[0]);
                dindirect.SetValue(test, true, new object[0]);
                displacement.SetValue(test, 203, new object[0]);
                if (Verify())
                    Console.Write("OK!");
                else
                    Console.Write("Wrong data emitted");
                Console.WriteLine();
                //offset 32
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 32bit offset");
                test = Activator.CreateInstance(type);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                dest.SetValue(test, (UInt32)65, new object[0]);
                dindirect.SetValue(test, true, new object[0]);
                displacement.SetValue(test, 70000, new object[0]);
                if (Verify())
                    Console.Write("OK!");
                else
                    Console.Write("Wrong data emitted");
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].testMem32))
            {
                //no offset
                Console.WriteLine("\t --> Mem32");
                Console.Write("\t\t --> no offset: ");
                var test = Activator.CreateInstance(type);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                dest.SetValue(test, (UInt32)70000, new object[0]);
                dindirect.SetValue(test, true, new object[0]);
                //if (Verify())
                //    Console.Write("OK!");
                //else
                //    Console.Write("Wrong data emitted");
                Console.WriteLine();
                //offset 16
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 16bit offset: ");
                test = Activator.CreateInstance(type);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                dest.SetValue(test, (UInt32)70000, new object[0]);
                dindirect.SetValue(test, true, new object[0]);
                displacement.SetValue(test, (Int16)203, new object[0]);
                if (Verify())
                    Console.Write("OK!");
                else
                    Console.Write("Wrong data emitted");
                Console.WriteLine();
                //offset 32
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\t\t --> 32bit offset");
                test = Activator.CreateInstance(type);
                if (size != 0)
                    psize.SetValue(test, size, new object[0]);
                dest.SetValue(test, (UInt32)70000, new object[0]);
                dindirect.SetValue(test, true, new object[0]);
                displacement.SetValue(test, (Int32)70000, new object[0]);
                if (Verify())
                    Console.Write("OK!");
                else
                    Console.Write("Wrong data emitted");
                Console.WriteLine();
            }
            var testReg = Activator.CreateInstance(type);
            Console.WriteLine("\tRegisters");
            foreach (Guid register in Registers.getRegisters())
            {
                if (!type.Namespace.Contains("SSE") && (Registers.getXMMs().Contains(register)))
                    continue;
                if (Registers.getCRs().Contains(register) && (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].testCR))))
                    continue;
                if (Registers.getRegisters().Contains(register) && (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].invalidDestRegisters.Contains(register)))))
                    continue;
                if (size != 0)
                    psize.SetValue(testReg, size, new object[0]);
                dreg.SetValue(testReg, register, new object[0]);
                Console.Write("\t\t" + Registers.GetRegisterName(register) + ": ");
                dest.SetValue(testReg, (UInt32)8, new object[0]);
                if (Verify())
                    Console.WriteLine("Ok!");
                else
                    Console.WriteLine("Wrong data emitted");
                Console.WriteLine();
            }*/
        }

        private static void TestInstructionWithDestinationAndSourceAndSize(Type type)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-->Size 8");
            TestInstructionWithDestinationAndSource(type, 8);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-->Size 16");
            TestInstructionWithDestinationAndSource(type, 16);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-->Size 32");
            TestInstructionWithDestinationAndSource(type, 32);
        }   

        private static void TestSimpleInstruction(Type type)
        {
            var test = Activator.CreateInstance(type);
            if (Verify())
                Console.WriteLine("Ok!");
            else
                Console.WriteLine("Wrong data emitted");
        }

        private static bool Verify() {

            String tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output");
            var xNasmWriter = new StreamWriter(Path.Combine(tempPath, "TheOutput.asm"), false);
            FileStream xNasmReader = null;
            bool xResult = false;
            string xMessage=null;
            try {
                var xIndy86MS = new MemoryStream();
                x86Assembler.FlushText(xNasmWriter);
                xNasmWriter.Close();
                ProcessStartInfo pinfo = new ProcessStartInfo();
                pinfo.Arguments = "TheOutput.asm" + " -o " + "TheOutput.bin";
                pinfo.WorkingDirectory = tempPath;
                pinfo.FileName = nasmPath;
                pinfo.UseShellExecute^= true;
                pinfo.CreateNoWindow=true;
                Process xProc = Process.Start(pinfo);
                xProc.WaitForExit();
                xNasmReader = File.OpenRead(Path.Combine(tempPath, "TheOutput.bin"));
                x86Assembler.FlushBinary(xIndy86MS, 0x200000);
                xIndy86MS.Position = 0;
                if (xNasmReader.Length != xIndy86MS.Length) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    xNasmReader.Close();
                    goto WriteResult;
                }
                while (true) {
                    var xVerData = xNasmReader.ReadByte();
                    var xActualData = xIndy86MS.ReadByte();
                    if (xVerData != xActualData) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        xNasmReader.Close();
                        goto WriteResult;
                    }
                    if (xVerData == -1) {
                        break;
                    }
                }
                xNasmReader.Close();
                Console.ForegroundColor = ConsoleColor.Green;
                xResult=true;
                WriteResult:
                return xResult;
            } catch (Exception e) {
                xMessage = e.Message; 
            } finally {
                var xInstrWithSize = x86Assembler.Instructions[0] as IInstructionWithSize;
                var xInstrWithDest = x86Assembler.Instructions[0] as IInstructionWithDestination;
                var xInstrWithSource = x86Assembler.Instructions[0] as IInstructionWithSource;
                WriteStatusEntry(x86Assembler.Instructions[0].Mnemonic,
                    xInstrWithSize == null ? (byte)0 : xInstrWithSize.Size,
                    xInstrWithDest != null,
                    xInstrWithDest != null ? xInstrWithDest.DestinationReg : Guid.Empty,
                    xInstrWithDest != null ? xInstrWithDest.DestinationValue.GetValueOrDefault() : 0,
                    xInstrWithDest != null && xInstrWithDest.DestinationIsIndirect,
                    xInstrWithDest != null ? xInstrWithDest.DestinationDisplacement : 0,
                    xInstrWithSource != null,
                    xInstrWithSource != null ? xInstrWithSource.SourceReg : Guid.Empty,
                    xInstrWithSource != null ? xInstrWithSource.SourceValue.GetValueOrDefault() : 0,
                    xInstrWithSource != null ? xInstrWithSource.SourceIsIndirect : false,
                    xInstrWithSource != null ? xInstrWithSource.SourceDisplacement : 0,
                    xResult,
                    xMessage);
                if (xNasmReader != null)
                    xNasmReader.Close();
                if (xNasmWriter != null)
                    xNasmWriter.Close();
                x86Assembler.Instructions.Clear();
                x86Assembler.DataMembers.Clear();
                if(Console.KeyAvailable) {
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
        private static void computeResult()
        {
            if (Verify())
                Console.Write("OK!");
            else
            {
                Console.Write("Wrong data emitted");
            }
            Console.WriteLine();
        }
    }
}
