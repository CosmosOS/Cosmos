using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Build.Common;
using Cosmos.Debug.DebugStub;
using XSharp.Compiler;

namespace Cosmos.IL2CPU
{
    public class CosmosAssembler: Assembler.Assembler
    {
        public CosmosAssembler(int comPort)
        {
            mComPort = comPort;
        }

        protected int mComPort = 0;

        /// <summary>
        /// Setting this field to false means the .xs files for the debug stub are read from the DebugStub assembly.
        /// This allows the automated kernel tester to use the live ones, instead of the installed ones.
        /// </summary>
        public static bool ReadDebugStubFromDisk = true;

        public virtual void WriteDebugVideo(string aText)
        {
            // This method emits a lot of ASM, but thats what we want becuase
            // at this point we need ASM as simple as possible and completely transparent.
            // No stack changes, no register mods, etc.

            // TODO: Add an option on the debug project properties to turn this off.
            // Also see TokenPatterns.cs Checkpoint in X#
            var xPreBootLogging = true;
            if (xPreBootLogging)
            {
                UInt32 xVideo = 0xB8000;
                for (UInt32 i = xVideo; i < xVideo + 80 * 2; i = i + 2)
                {
                    new LiteralAssemblerCode("mov byte [0x" + i.ToString("X") + "], 0");
                    new LiteralAssemblerCode("mov byte [0x" + (i + 1).ToString("X") + "], 0x02");
                }

                foreach (var xChar in aText)
                {
                    new LiteralAssemblerCode("mov byte [0x" + xVideo.ToString("X") + "], " + (byte)xChar);
                    xVideo = xVideo + 2;
                }
            }
        }

        public void CreateGDT()
        {
            new Comment(this, "BEGIN - Create GDT");
            var xGDT = new List<byte>();

            // Null Segment - Selector 0x00
            // Not used, but required by many emulators.
            xGDT.AddRange(new byte[8]
                          {
                              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                          });

            // Code Segment
            mGdCode = (byte)xGDT.Count;
            xGDT.AddRange(GdtDescriptor(0x00000000, 0xFFFFFFFF, true));

            // Data Segment - Selector
            mGdData = (byte)xGDT.Count;
            xGDT.AddRange(GdtDescriptor(0x00000000, 0xFFFFFFFF, false));
            DataMembers.Add(new DataMember("_NATIVE_GDT_Contents", xGDT.ToArray()));

            XS.Comment("Tell CPU about GDT");
            var xGdtPtr = new UInt16[3];

            // Size of GDT Table - 1
            xGdtPtr[0] = (UInt16)(xGDT.Count - 1);
            DataMembers.Add(new DataMember("_NATIVE_GDT_Pointer", xGdtPtr));
            new Mov
            {
                DestinationRef = ElementReference.New("_NATIVE_GDT_Pointer"),
                DestinationIsIndirect = true,
                DestinationDisplacement = 2,
                SourceRef = ElementReference.New("_NATIVE_GDT_Contents")
            };
            XS.Set(XSRegisters.EAX, "_NATIVE_GDT_Pointer");
            XS.LoadGdt(XSRegisters.EAX, isIndirect: true);

            XS.Comment("Set data segments");
            XS.Set(XSRegisters.EAX, mGdData);
            XS.Set(XSRegisters.DS, XSRegisters.AX);
            XS.Set(XSRegisters.ES, XSRegisters.AX);
            XS.Set(XSRegisters.FS, XSRegisters.AX);
            XS.Set(XSRegisters.GS, XSRegisters.AX);
            XS.Set(XSRegisters.SS, XSRegisters.AX);

            XS.Comment("Force reload of code segment");
            new JumpToSegment
            {
                Segment = mGdCode, DestinationLabel = "Boot_FlushCsGDT"
            };
            XS.Label("Boot_FlushCsGDT");
            new Comment(this, "END - Create GDT");
        }

        protected void SetIdtDescriptor(int aNo, string aLabel, bool aDisableInts)
        {
            int xOffset = aNo * 8;
            XS.Set(XSRegisters.EAX, aLabel);
            var xIDT = ElementReference.New("_NATIVE_IDT_Contents");
            new Mov
            {
                DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset, SourceReg = RegistersEnum.AL
            };
            new Mov
            {
                DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 1, SourceReg = RegistersEnum.AH
            };
            XS.ShiftRight(XSRegisters.EAX, 16);
            new Mov
            {
                DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 6, SourceReg = RegistersEnum.AL
            };
            new Mov
            {
                DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 7, SourceReg = RegistersEnum.AH
            };

            // Code Segment
            new Mov
            {
                DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 2, SourceValue = mGdCode, Size = 16
            };

            // Reserved
            new Mov
            {
                DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 4, SourceValue = 0x00, Size = 8
            };

            // Type
            new Mov
            {
                DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 5, SourceValue = (byte)(aDisableInts ? 0x8E : 0x8F), Size = 8
            };
        }

        public void CreateIDT()
        {
            new Comment(this, "BEGIN - Create IDT");

            // Create IDT
            UInt16 xIdtSize = 8 * 256;
            DataMembers.Add(new DataMember("_NATIVE_IDT_Contents", new byte[xIdtSize]));

            //
            if (mComPort > 0)
            {
                SetIdtDescriptor(1, "DebugStub_TracerEntry", false);
                SetIdtDescriptor(3, "DebugStub_TracerEntry", false);

                //for (int i = 0; i < 256; i++)
                //{
                //  if (i == 1 || i == 3)
                //  {
                //    continue;
                //  }

                //  SetIdtDescriptor(i, "DebugStub_Interrupt_" + i.ToString(), true);
                //}
            }

            //SetIdtDescriptor(1, "DebugStub_INT0"); - Change to GPF

            // Set IDT
            DataMembers.Add(new DataMember("_NATIVE_IDT_Pointer", new UInt16[]
                                                                  {
                                                                      xIdtSize, 0, 0
                                                                  }));
            new Mov
            {
                DestinationRef = ElementReference.New("_NATIVE_IDT_Pointer"),
                DestinationIsIndirect = true,
                DestinationDisplacement = 2,
                SourceRef = ElementReference.New("_NATIVE_IDT_Contents")
            };

            XS.Set(XSRegisters.EAX, "_NATIVE_IDT_Pointer");

            if (mComPort > 0)
            {
                XS.Set("static_field__Cosmos_Core_CPU_mInterruptsEnabled", 1, destinationIsIndirect: true);
                XS.LoadIdt(XSRegisters.EAX, isIndirect: true);
            }
            XS.Label("AfterCreateIDT");
            new Comment(this, "END - Create IDT");
        }

        public void Initialize()
        {
            uint xSig = 0x1BADB002;

            DataMembers.Add(new DataIfNotDefined("ELF_COMPILATION"));
            DataMembers.Add(new DataMember("MultibootSignature", new uint[]
                                                                 {
                                                                     xSig
                                                                 }));
            uint xFlags = 0x10003;
            DataMembers.Add(new DataMember("MultibootFlags", xFlags));
            DataMembers.Add(new DataMember("MultibootChecksum", (int)(0 - (xFlags + xSig))));
            DataMembers.Add(new DataMember("MultibootHeaderAddr", ElementReference.New("MultibootSignature")));
            DataMembers.Add(new DataMember("MultibootLoadAddr", ElementReference.New("MultibootSignature")));
            DataMembers.Add(new DataMember("MultibootLoadEndAddr", ElementReference.New("_end_code")));
            DataMembers.Add(new DataMember("MultibootBSSEndAddr", ElementReference.New("_end_code")));
            DataMembers.Add(new DataMember("MultibootEntryAddr", ElementReference.New("Kernel_Start")));
            DataMembers.Add(new DataEndIfDefined());

            DataMembers.Add(new DataIfDefined("ELF_COMPILATION"));
            xFlags = 0x00003;
            DataMembers.Add(new DataMember("MultibootSignature", new uint[]
                                                                 {
                                                                     xSig
                                                                 }));
            DataMembers.Add(new DataMember("MultibootFlags", xFlags));
            DataMembers.Add(new DataMember("MultibootChecksum", (int)(0 - (xFlags + xSig))));
            DataMembers.Add(new DataEndIfDefined());

            // graphics info fields
            DataMembers.Add(new DataMember("MultibootGraphicsRuntime_VbeModeInfoAddr", Int32.MaxValue));
            DataMembers.Add(new DataMember("MultibootGraphicsRuntime_VbeControlInfoAddr", Int32.MaxValue));
            DataMembers.Add(new DataMember("MultibootGraphicsRuntime_VbeMode", Int32.MaxValue));

            // memory
            DataMembers.Add(new DataMember("MultiBootInfo_Memory_High", 0));
            DataMembers.Add(new DataMember("MultiBootInfo_Memory_Low", 0));
            DataMembers.Add(new DataMember("Before_Kernel_Stack", new byte[0x50000]));
            DataMembers.Add(new DataMember("Kernel_Stack", new byte[0]));
            DataMembers.Add(new DataMember("MultiBootInfo_Structure", new uint[1]));

            // constants
            DataMembers.Add(new DataMember(@"__uint2double_const", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xf0, 0x41 }));
            DataMembers.Add(new DataMember(@"__ulong2double_const", 0x5F800000));
            DataMembers.Add(new DataMember(@"__doublesignbit", 0x8000000000000000));
            DataMembers.Add(new DataMember(@"__floatsignbit", 0x80000000));

            if (mComPort > 0)
            {
                new Define("DEBUGSTUB");
            }

            // This is our first entry point. Multiboot uses this as Cosmos entry point.
            new Label("Kernel_Start", isGlobal: true);
            XS.Set(XSRegisters.ESP, "Kernel_Stack");

            // Displays "Cosmos" in top left. Used to make sure Cosmos is booted in case of hang.
            // ie bootloader debugging. This must be the FIRST code, even before setup so we know
            // we are being called properly by the bootloader and that if there are problems its
            // somwhere in our code, not the bootloader.
            WriteDebugVideo("Cosmos pre boot");

            // For when using Bochs, causes a break ASAP on entry after initial Cosmos display.
            new LiteralAssemblerCode("xchg bx, bx");

            // CLI ASAP
            WriteDebugVideo("Clearing interrupts.");
            XS.ClearInterruptFlag();


            WriteDebugVideo("Begin multiboot info.");
            new LiteralAssemblerCode("%ifndef EXCLUDE_MULTIBOOT_MAGIC");
            new Comment(this, "MultiBoot compliant loader provides info in registers: ");
            new Comment(this, "EBX=multiboot_info ");
            new Comment(this, "EAX=0x2BADB002 - check if it's really Multiboot-compliant loader ");
            new Comment(this, "                ;- copy mb info - some stuff for you  ");
            new Comment(this, "BEGIN - Multiboot Info");
            new Mov
            {
                DestinationRef = ElementReference.New("MultiBootInfo_Structure"), DestinationIsIndirect = true, SourceReg = RegistersEnum.EBX
            };
            XS.Add(XSRegisters.EBX, 4);
            XS.Set(XSRegisters.EAX, XSRegisters.EBX, sourceIsIndirect: true);
            new Mov
            {
                DestinationRef = ElementReference.New("MultiBootInfo_Memory_Low"), DestinationIsIndirect = true, SourceReg = RegistersEnum.EAX
            };
            XS.Add(XSRegisters.EBX, 4);
            XS.Set(XSRegisters.EAX, XSRegisters.EBX, sourceIsIndirect: true);
            new Mov
            {
                DestinationRef = ElementReference.New("MultiBootInfo_Memory_High"), DestinationIsIndirect = true, SourceReg = RegistersEnum.EAX
            };
            new Comment(this, "END - Multiboot Info");
            new LiteralAssemblerCode("%endif");
            WriteDebugVideo("Creating GDT.");
            CreateGDT();

            WriteDebugVideo("Configuring PIC");
            ConfigurePIC();

            WriteDebugVideo("Creating IDT.");
            CreateIDT();
#if LFB_1024_8
            new Comment("Set graphics fields");
            XS.Mov(XSRegisters.EBX, Cosmos.Assembler.ElementReference.New("MultiBootInfo_Structure"), sourceIsIndirect: true);
            XS.Mov(XSRegisters.EAX, XSRegisters.EBX, sourceDisplacement: 72);
            new Move { DestinationRef = Cosmos.Assembler.ElementReference.New("MultibootGraphicsRuntime_VbeControlInfoAddr"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
            XS.Mov(XSRegisters.EAX, XSRegisters.EBX, sourceDisplacement: 76);
            new Move { DestinationRef = Cosmos.Assembler.ElementReference.New("MultibootGraphicsRuntime_VbeModeInfoAddr"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
            XS.Mov(XSRegisters.EAX, XSRegisters.EBX, sourceDisplacement: 80);
            new Move { DestinationRef = Cosmos.Assembler.ElementReference.New("MultibootGraphicsRuntime_VbeMode"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
#endif

            //WriteDebugVideo("Initializing SSE.");
            //new Comment(this, "BEGIN - SSE Init");
            //// CR4[bit 9]=1, CR4[bit 10]=1, CR0[bit 2]=0, CR0[bit 1]=1
            //XS.Mov(XSRegisters.EAX, XSRegisters.Registers.CR4);
            //XS.Or(XSRegisters.EAX, 0x100);
            //XS.Mov(XSRegisters.CR4, XSRegisters.Registers.EAX);
            //XS.Mov(XSRegisters.EAX, XSRegisters.Registers.CR4);
            //XS.Or(XSRegisters.EAX, 0x200);
            //XS.Mov(XSRegisters.CR4, XSRegisters.Registers.EAX);
            //XS.Mov(XSRegisters.EAX, XSRegisters.Registers.CR0);

            //XS.And(XSRegisters.EAX, 0xfffffffd);
            //XS.Mov(XSRegisters.CR0, XSRegisters.Registers.EAX);
            //XS.Mov(XSRegisters.EAX, XSRegisters.Registers.CR0);

            //XS.And(XSRegisters.EAX, 1);
            //XS.Mov(XSRegisters.CR0, XSRegisters.Registers.EAX);
            //new Comment(this, "END - SSE Init");

            if (mComPort > 0)
            {
                WriteDebugVideo("Initializing DebugStub.");
                XS.Call("DebugStub_Init");
            }

            // Jump to Kernel entry point
            WriteDebugVideo("Jumping to kernel.");
            XS.Call(EntryPointName);

            new Comment(this, "Kernel done - loop till next IRQ");
            XS.Label(".loop");
            XS.ClearInterruptFlag();
            XS.Halt();
            XS.Jump(".loop");

            if (mComPort > 0)
            {
                var xGen = new AsmGenerator();

                var xGenerateAssembler =
                    new Action<object>(i =>
                                       {
                                           if (i is StreamReader)
                                           {
                                               var xAsm = xGen.Generate((StreamReader)i);
                                               CurrentInstance.Instructions.AddRange(xAsm.Instructions);
                                               CurrentInstance.DataMembers.AddRange(xAsm.DataMembers);
                                           }
                                           else if (i is string)
                                           {
                                               var xAsm = xGen.Generate((string)i);
                                               CurrentInstance.Instructions.AddRange(xAsm.Instructions);
                                               CurrentInstance.DataMembers.AddRange(xAsm.DataMembers);
                                           }
                                           else
                                           {
                                               throw new Exception("Object type '" + i.ToString() + "' not supported!");
                                           }
                                       });
                if (ReadDebugStubFromDisk)
                {
                    foreach (var xFile in Directory.GetFiles(CosmosPaths.DebugStubSrc, "*.xs"))
                    {
                        xGenerateAssembler(xFile);
                    }
                }
                else
                {
                    foreach (var xManifestName in typeof(ReferenceHelper).Assembly.GetManifestResourceNames())
                    {
                        if (!xManifestName.EndsWith(".xs", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        using (var xStream = typeof(ReferenceHelper).Assembly.GetManifestResourceStream(xManifestName))
                        {
                            using (var xReader = new StreamReader(xStream))
                            {
                                xGenerateAssembler(xReader);
                            }
                        }
                    }
                }
                OnAfterEmitDebugStub();
            }
            else
            {
                XS.Label("DebugStub_Step");
                XS.Return();
            }

            // Start emitting assembly labels
            CurrentInstance.EmitAsmLabels = true;
        }

        private void ConfigurePIC()
        {
            // initial configuration of PIC
            const byte PIC1 = 0x20; /* IO base address for master PIC */
            const byte PIC2 = 0xA0; /* IO base address for slave PIC */
            const byte PIC1_COMMAND = PIC1;
            const byte PIC1_DATA = (PIC1 + 1);
            const byte PIC2_COMMAND = PIC2;
            const byte PIC2_DATA = (PIC2 + 1);

            const byte ICW1_ICW4 = 0x01; /* ICW4 (not) needed */
            const byte ICW1_SINGLE = 0x02; /* Single (cascade) mode */
            const byte ICW1_INTERVAL4 = 0x04; /* Call address interval 4 (8) */
            const byte ICW1_LEVEL = 0x08; /* Level triggered (edge) mode */
            const byte ICW1_INIT = 0x10; /* Initialization - required! */

            const byte ICW4_8086 = 0x01; /* 8086/88 (MCS-80/85) mode */
            const byte ICW4_AUTO = 0x02; /* Auto (normal) EOI */
            const byte ICW4_BUF_SLAVE = 0x08; /* Buffered mode/slave */
            const byte ICW4_BUF_MASTER = 0x0C; /* Buffered mode/master */
            const byte ICW4_SFNM = 0x10; /* Special fully nested (not) */

            // emit helper functions:
            Action<byte, byte> xOutBytes = (port, value) =>
                                           {
                                               XS.Set(XSRegisters.DX, port);
                                               XS.Set(XSRegisters.EAX, value);
                                               XS.WriteToPortDX(XSRegisters.AL);
                                           };

            Action xIOWait = () => xOutBytes(0x80, 0x22);

            xOutBytes(PIC1_COMMAND, ICW1_INIT + ICW1_ICW4); // starts the initialization sequence (in cascade mode)
            xIOWait();
            xOutBytes(PIC2_COMMAND, ICW1_INIT + ICW1_ICW4);
            xIOWait();
            xOutBytes(PIC1_DATA, 0x20); // ICW2: Master PIC vector offset
            xIOWait();
            xOutBytes(PIC2_DATA, 0x29); // ICW2: Slave PIC vector offset
            xIOWait();
            xOutBytes(PIC1_DATA, 4); // ICW3: tell Master PIC that there is a slave PIC at IRQ2 (0000 0100)
            xIOWait();
            xOutBytes(PIC2_DATA, 2); // ICW3: tell Slave PIC its cascade identity (0000 0010)
            xIOWait();

            xOutBytes(PIC1_DATA, ICW4_8086);
            xIOWait();
            xOutBytes(PIC2_DATA, ICW4_8086);
            xIOWait();

            // for now, we don't want any irq's enabled:
            xOutBytes(PIC1_DATA, 0xFF); // restore saved masks.
            xOutBytes(PIC2_DATA, 0xFF);
        }

        protected virtual void OnAfterEmitDebugStub()
        {
            //
        }

        public const string EntryPointName = "__ENGINE_ENTRYPOINT__";

        protected byte[] GdtDescriptor(UInt32 aBase, UInt32 aSize, bool aCode)
        {
            // Limit is a confusing word. Is it the max physical address or size?
            // In fact it is the size, and 286 docs actually refer to it as size
            // rather than limit.
            // It is also size - 1, else there would be no way to specify
            // all of RAM, and a limit of 0 is invalid.

            var xResult = new byte[8];

            // Check the limit to make sure that it can be encoded
            if ((aSize > 65536) && (aSize & 0x0FFF) != 0x0FFF)
            {
                // If larger than 16 bit, must be an even page (4kb) size
                throw new Exception("Invalid size in GDT descriptor.");
            }
            // Flags nibble
            // 7: Granularity
            //    0 = bytes
            //    1 = 4kb pages
            // 6: 1 = 32 bit mode
            // 5: 0 - Reserved
            // 4: 0 - Reserved
            xResult[6] = 0x40;
            if (aSize > 65536)
            {
                // Set page sizing instead of byte sizing
                aSize = aSize >> 12;
                xResult[6] = (byte)(xResult[6] | 0x80);
            }

            xResult[0] = (byte)(aSize & 0xFF);
            xResult[1] = (byte)((aSize >> 8) & 0xFF);
            xResult[6] = (byte)(xResult[6] | ((aSize >> 16) & 0x0F));

            xResult[2] = (byte)(aBase & 0xFF);
            xResult[3] = (byte)((aBase >> 8) & 0xFF);
            xResult[4] = (byte)((aBase >> 16) & 0xFF);
            xResult[7] = (byte)((aBase >> 24) & 0xFF);

            xResult[5] = (byte)(
              // Bit 7: Present, must be 1
              0x80 |
              // Bit 6-5: Privilege, 0=kernel, 3=user
              0x00 |
              // Reserved, must be 1
              0x10 |
              // Bit 3: 1=Code, 0=Data
              (aCode ? 0x08 : 0x00) |
              // Bit 2: Direction/Conforming
              0x00 |
              // Bit 1: R/W  Data (1=Writeable, 0=Read only) Code (1=Readable, 0=Not readable)
              0x02 |
              // Bit 0: Accessed - Set to 0. Updated by CPU later.
              0x00
              );

            return xResult;
        }

        protected override void BeforeFlushText(TextWriter aOutput)
        {
            base.BeforeFlushText(aOutput);
            aOutput.WriteLine("%ifndef ELF_COMPILATION");
            aOutput.WriteLine("use32");
            aOutput.WriteLine("org 0x1000000");
            aOutput.WriteLine("[map all main.map]");
            aOutput.WriteLine("%endif");
        }

        protected override void OnBeforeFlush()
        {
            DataMembers.AddRange(new DataMember[] { new DataMember("_end_data", new byte[0]) });
        }

        protected override void OnFlushTextAfterEmitEverything(TextWriter aOutput)
        {
            base.OnFlushTextAfterEmitEverything(aOutput);

            aOutput.WriteLine("SystemExceptionOccurred:");
            aOutput.WriteLine("\tret");
            aOutput.WriteLine("global Kernel_Start");
            aOutput.WriteLine("_end_code:");
        }
    }
}
