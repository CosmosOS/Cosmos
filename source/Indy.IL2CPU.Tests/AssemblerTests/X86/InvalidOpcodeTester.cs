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

        private static void Execute()
        {
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
            //public Constraints(bool testMem32, bool testMem16, bool testImmediate8, bool testImmediate16, bool testImmediate32, bool testCR, List<Guid> invalidDestRegisters)
            opcodesException.Add(typeof(Pop), new ConstraintsContainer { DestInfo = new Constraints { InvalidRegisters = Registers.Get8BitRegisters().Union(Registers.Get16BitRegisters()) } });
            //opcodesException.Add(typeof(Assembler.X86.));
            //
            x86Assembler=new Assembler.X86.Assembler();
            x86Assembler.Initialize();
            x86Assembler.Instructions.Clear();
            x86Assembler.DataMembers.Clear();
            //Determine opcodes
            Assembly xAsm = Assembly.Load("Indy.IL2CPU");
            foreach (Type type in xAsm.GetTypes())
            {
                try
                {
                    if (type.IsAbstract)
                        continue;
                    else
                    {
                        if (type.BaseType == typeof(Assembler.X86.InstructionWithDestination))
                        {
                            Console.WriteLine("Testing " + type.ToString());
                            TestInstructionWithDestination(type);
                        }
                        else if (type.BaseType == typeof(Assembler.X86.InstructionWithDestinationAndSize))
                        {
                            Console.WriteLine("Testing " + type.ToString());
                            TestInstructionWithDestinationAndSize(type);
                        }
                        else if (type.BaseType == typeof(Assembler.X86.InstructionWithDestinationAndSource))
                        {
                            Console.WriteLine("Testing " + type.ToString());
                            TestInstructionWithDestinationAndSource(type);
                        }
                        else if (type.BaseType == typeof(Assembler.X86.InstructionWithDestinationAndSourceAndSize))
                        {
                            Console.WriteLine("Testing " + type.ToString());
                            TestInstructionWithDestinationAndSourceAndSize(type);
                        }
                        else if (type.BaseType == typeof(Assembler.X86.Instruction))
                        {
                            Console.WriteLine("Testing " + type.ToString());
                            TestSimpleInstruction(type);
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.Beep();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(type.ToString() + ": " + e.Message);
                    //Console.ReadLine();
                    
                }
                finally
                {
                    x86Assembler.Instructions.Clear();
                    x86Assembler.DataMembers.Clear();
                    Console.ResetColor();
                }
            }
            //Define constraints
            Console.WriteLine("Tests finished!");
            Console.ReadLine();
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
                }
                );
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

        private static bool Verify()
        {

            String tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output");
            var xNasmWriter = new StreamWriter(Path.Combine(tempPath, "TheOutput.asm"), false);
            FileStream xNasmReader=null;
            try{
            var xIndy86MS = new MemoryStream();
            x86Assembler.FlushText(xNasmWriter);
            xNasmWriter.Close();
            ProcessStartInfo pinfo = new ProcessStartInfo();
            pinfo.Arguments = "TheOutput.asm" + " -o " + "TheOutput.bin";
            pinfo.WorkingDirectory=tempPath;
            pinfo.FileName = nasmPath;
            Process xProc = Process.Start(pinfo);
            xProc.WaitForExit();
            xNasmReader = File.OpenRead(Path.Combine(tempPath, "TheOutput.bin"));
            x86Assembler.FlushBinary(xIndy86MS,0x200000);
            x86Assembler.Instructions.Clear();
            x86Assembler.DataMembers.Clear();
            xIndy86MS.Position = 0;
            if (xNasmReader.Length != xIndy86MS.Length)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                xNasmReader.Close();
                x86Assembler.Instructions.Clear();
                x86Assembler.DataMembers.Clear();
                return false;
            }
            while (true)
            {
                var xVerData = xNasmReader.ReadByte();
                var xActualData = xIndy86MS.ReadByte();
                if (xVerData != xActualData)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    xNasmReader.Close();
                    x86Assembler.Instructions.Clear();
                    x86Assembler.DataMembers.Clear();
                    return false;
                }
                if (xVerData == -1)
                {
                    break;
                }
            }
            xNasmReader.Close();
            x86Assembler.Instructions.Clear();
            x86Assembler.DataMembers.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            return true;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if(xNasmReader!=null)
                    xNasmReader.Close();
                if (xNasmWriter != null)
                    xNasmWriter.Close();
            }
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
                Console.Beep();
                Console.Write("Wrong data emitted");
                Console.ReadLine();
            }
            Console.WriteLine();
        }
    }
}
