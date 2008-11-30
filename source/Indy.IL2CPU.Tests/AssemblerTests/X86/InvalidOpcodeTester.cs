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
            public bool TestSegments = true;
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
                    if (type.IsAbstract) {
                        continue;
                    }
                    if(!type.IsSubclassOf(typeof(Instruction))) {
                        continue;
                    }
                    if(type == typeof(Jump)) {
                        System.Diagnostics.Debugger.Break();
                    }
                    Console.WriteLine("Testing " + type.Name);
                    if (type.IsSubclassOf(typeof(Assembler.X86.InstructionWithDestinationAndSourceAndSize))) {
                        TestInstructionWithDestinationAndSourceAndSize(type);
                        continue;
                    }
                    if (type.IsSubclassOf(typeof(Assembler.X86.InstructionWithDestinationAndSource))) {
                        TestInstructionWithDestinationAndSource(type);
                        continue;
                    }
                    if (type.IsSubclassOf(typeof(Assembler.X86.InstructionWithDestinationAndSize))) {
                        TestInstructionWithDestinationAndSize(type);
                        continue;
                    }
                    if (type.IsSubclassOf(typeof(Assembler.X86.InstructionWithDestination))) {
                        TestInstructionWithDestination(type);
                        continue;
                    }
                    if (type.IsSubclassOf(typeof(Assembler.X86.Instruction))) {
                        TestSimpleInstruction(type);
                        continue;
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
                xFile = Path.Combine(Path.GetDirectoryName(typeof(InvalidOpcodeTester).Assembly.Location), "TestResults.xml");
                GenerateXml(xFile);
                Console.WriteLine("Tests finished. Results have been written to '{0}'", xFile);
                Console.ReadLine();
            }
        }

        public class AbortException: Exception {
         //   
        }

        private static void GenerateXml(string aFile) {
            using(var xWriter = XmlWriter.Create(aFile)) {
                xWriter.WriteStartDocument();
                xWriter.WriteStartElement("TestResults");
                {
                    foreach (var xItem in mTestStates) {
                        xWriter.WriteStartElement("TestCase");
                        {
                            xWriter.WriteAttributeString("Instruction", xItem.Instruction);
                            xWriter.WriteAttributeString("Size", xItem.Size.ToString());
                            if (xItem.HasDest) {
                                if (xItem.DestReg == Guid.Empty) {
                                    xWriter.WriteAttributeString("DestinationReg", "");
                                } else {
                                    xWriter.WriteAttributeString("DestinationReg", Registers.GetRegisterName(xItem.DestReg));
                                }
                                xWriter.WriteAttributeString("DestinationValue", xItem.DestValue.ToString());
                                xWriter.WriteAttributeString("DestinationIsIndirect", xItem.DestIsIndirect.ToString());
                                xWriter.WriteAttributeString("DestinationDisplacement", xItem.DestDisplacement.ToString());
                            } else {
                                xWriter.WriteAttributeString("DestinationReg", "");
                                xWriter.WriteAttributeString("DestinationValue", "");
                                xWriter.WriteAttributeString("DestinationIsIndirect", "");
                                xWriter.WriteAttributeString("DestinationDisplacement", "");
                            }
                            if (xItem.HasSource) {
                                if (xItem.SourceReg == Guid.Empty) {
                                    xWriter.WriteAttributeString("SourceReg", "");
                                } else {
                                    xWriter.WriteAttributeString("SourceReg", Registers.GetRegisterName(xItem.SourceReg));
                                }
                                xWriter.WriteAttributeString("SourceValue", xItem.SourceValue.ToString());
                                xWriter.WriteAttributeString("SourceIsIndirect", xItem.SourceIsIndirect.ToString());
                                xWriter.WriteAttributeString("SourceDisplacement", xItem.SourceDisplacement.ToString());
                            } else {
                                xWriter.WriteAttributeString("SourceReg", "");
                                xWriter.WriteAttributeString("SourceValue", "");
                                xWriter.WriteAttributeString("SourceIsIndirect", "");
                                xWriter.WriteAttributeString("SourceDisplacement", "");
                            }
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
            if(t.GetConstructor(new Type[0])==null) {
                throw new Exception("Type '" + t.FullName + "' doesnt have a parameterless constructor!");
            }
            var xResult = (T)Activator.CreateInstance(t);
            if(aSize!=0) {
                ((IInstructionWithSize)xResult).Size = aSize;
            }
            return xResult;
        }

        private static void TestInstructionWithDestination(Type type, byte size, Action<IInstructionWithDestination> aInitInstruction)
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
                computeResult();
            }
            
            //Test Immediate 16
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestImmediate16))
            {
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 300;
                computeResult();
            }
            //Test Immediate 32
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestImmediate32))
            {
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 300000;
                computeResult();
            }
            //memory 8 bits
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem8)) {
                //no offset
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect = true;
                computeResult();
                //offset 8
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 203;
                computeResult();
                //offset 16
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 2030;
                computeResult();
                //offset 32
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 65;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement=70000;
                computeResult();
            }
            //memory 16 bits
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem16))
            {   
                //no offset
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect=true;
                computeResult();
                //offset 8
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 203;
                computeResult();
                //offset 16
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650;
                xInstruction.DestinationIsIndirect=true;
                xInstruction.DestinationDisplacement = 2003;
                computeResult();
                //offset 32
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
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                computeResult();
                //offset 8
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 203;
                computeResult();
                //offset 16
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 2003;
                computeResult();
                //offset 32
                xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                aInitInstruction(xInstruction);
                xInstruction.DestinationValue = 650000;
                xInstruction.DestinationIsIndirect = true;
                xInstruction.DestinationDisplacement = 70000;
                computeResult();
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
                        computeResult();
                        //offset 8
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        computeResult();
                        //offset 16
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2030;
                        computeResult();
                        //offset 32
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 70000;
                        computeResult();
                    }
                    //memory 16 bits
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].DestInfo.TestMem16)) {
                        //no offset
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        computeResult();
                        //offset 8
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        computeResult();
                        //offset 16
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2003;
                        computeResult();
                        //offset 32
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
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        computeResult();
                        //offset 8
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 203;
                        computeResult();
                        //offset 16
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 2003;
                        computeResult();
                        //offset 32
                        xInstruction = CreateInstruction<IInstructionWithDestination>(type, size);
                        aInitInstruction(xInstruction);
                        xInstruction.DestinationReg = xReg;
                        xInstruction.DestinationIsIndirect = true;
                        xInstruction.DestinationDisplacement = 70000;
                        computeResult();
                    }
                }
                foreach (Guid register in Registers.GetRegisters()) {
                    if (!type.Namespace.Contains("SSE") && (Registers.getXMMs().Contains(register)))
                        continue;
                    if (Registers.getCRs().Contains(register) && (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && (!opcodesException[type].DestInfo.TestCR))))
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
                        computeResult();
                    }
                }
            }
        }

        private static void TestInstructionWithSource(Type type, byte size) {
            //Test Immediate 8
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate8)) {
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 30;
                });
            }

            //Test Immediate 16
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate16)) {
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 300;
                });
            }
            //Test Immediate 32
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestImmediate32)) {
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 300000;
                });
            }
            //memory 8 bits
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem8)) {
                //no offset
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 65;
                    xInstruction.SourceIsIndirect = true;
                });
                //offset 8
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 65;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 203;
                });
                //offset 16
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 65;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 2030;
                });
                //offset 32
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 65;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 70000;
                });
            }
            //memory 16 bits
            if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem16)) {
                //no offset
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650;
                    xInstruction.SourceIsIndirect = true;
                });
                //offset 8
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 203;
                });
                //offset 16
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 2003;
                });
                //offset 32
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
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650000;
                    xInstruction.SourceIsIndirect = true;
                });
                //offset 8
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650000;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 203;
                });
                //offset 16
                TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                    var xInstruction = (IInstructionWithSource)aInstruction;
                    xInstruction.SourceValue = 650000;
                    xInstruction.SourceIsIndirect = true;
                    xInstruction.SourceDisplacement = 2003;
                });
                //offset 32
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
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem8)) {
                        //no offset
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                        });
                        //offset 8
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 203;
                        });
                        //offset 16
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 2030;
                        });
                        //offset 32
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 70000;
                        });
                    }
                    //memory 16 bits
                    if (!opcodesException.ContainsKey(type) || (opcodesException.ContainsKey(type) && opcodesException[type].SourceInfo.TestMem16)) {
                        //no offset
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                        });
                        //offset 8
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 203;
                        });
                        //offset 16
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 2003;
                        });
                        //offset 32
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
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                        });
                        //offset 8
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 203;
                        });
                        //offset 16
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 2003;
                        });
                        //offset 32
                        TestInstructionWithDestination(type, size, delegate(IInstructionWithDestination aInstruction) {
                            var xInstruction = (IInstructionWithSource)aInstruction;
                            xInstruction.SourceReg = xReg;
                            xInstruction.SourceIsIndirect = true;
                            xInstruction.SourceDisplacement = 70000;
                        });
                    }
                }
            }

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
            if (!opcodesException.ContainsKey(type) || ((opcodesException[type].InvalidSizes & Instruction.InstructionSizes.Byte) == 0)){
                TestInstructionWithDestination(type, 8, null);
            }
            if (!opcodesException.ContainsKey(type) || ((opcodesException[type].InvalidSizes & Instruction.InstructionSizes.Word) == 0)) {
                TestInstructionWithDestination(type, 16, null);
            }
            if (!opcodesException.ContainsKey(type) || ((opcodesException[type].InvalidSizes & Instruction.InstructionSizes.DWord) == 0)) {
                TestInstructionWithDestination(type, 32, null);
            }
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
         
        }

        private static void TestInstructionWithDestinationAndSourceAndSize(Type type)
        {
            TestInstructionWithDestinationAndSource(type, 8);
            TestInstructionWithDestinationAndSource(type, 16);
            TestInstructionWithDestinationAndSource(type, 32);
        }   

        private static void TestSimpleInstruction(Type type)
        {
            var test = CreateInstruction<object>(type, 0);
            Verify();
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
                xNasmWriter.Flush();
                if(xNasmWriter.BaseStream.Length==0) {
                    throw new Exception("No content found");
                }
                xNasmWriter.Close();
                ProcessStartInfo pinfo = new ProcessStartInfo();
                pinfo.Arguments = "TheOutput.asm" + " -o " + "TheOutput.bin";
                pinfo.WorkingDirectory = tempPath;
                pinfo.FileName = nasmPath;
                pinfo.UseShellExecute ^= true;
                pinfo.CreateNoWindow=true;
                pinfo.RedirectStandardOutput = true;
                pinfo.RedirectStandardError=true;
                Process xProc = Process.Start(pinfo);
                xProc.WaitForExit();
                if (xProc.ExitCode != 0) {
                    xResult=false;
                    xMessage="Error while invoking nasm.exe: \r\n" + xProc.StandardOutput.ReadToEnd() + "\r\n" + xProc.StandardError.ReadToEnd();
                    goto WriteResult;
                } else {
                    xNasmReader = File.OpenRead(Path.Combine(tempPath, "TheOutput.bin"));
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
                WriteResult:
                return xResult;
            } catch (Exception e) {
                if (e.Message != "No valid EncodingOption found!") {
                    xMessage = e.ToString();
                }else{ xMessage = e.Message;}
            } finally {
                var xInstrWithSize = x86Assembler.Instructions[0] as IInstructionWithSize;
                var xInstrWithDest = x86Assembler.Instructions[0] as IInstructionWithDestination;
                var xInstrWithSource = x86Assembler.Instructions[0] as IInstructionWithSource;
                WriteStatusEntry((String.IsNullOrEmpty(x86Assembler.Instructions[0].Mnemonic) ? x86Assembler.Instructions[0].GetType().FullName : x86Assembler.Instructions[0].Mnemonic),
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
        private static void computeResult()
        {
            Verify();
        }
    }
}
