using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Compiler.Assembler.Assembler;
using CPUAll = Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using System.Collections.Generic;

namespace Cosmos.Core.Plugs.Assemblers {
  public class CreateGDT : AssemblerMethod {
    // http://wiki.osdev.org/Global_Descriptor_Table
    // http://http://wiki.osdev.org/GDT_Tutorial  
    // http://www.osdever.net/bkerndev/Docs/gdt.htm
    // http://en.wikibooks.org/wiki/X86_Assembly/Global_Descriptor_Table

    //TODO: Modify this to support the flags and access as individual input rather
    // than requiring user to pass pre-calculated numbers.
    protected byte[] Descriptor(UInt32 aBase, UInt32 aSize, bool aCode) {
      // Limit is a confusing word. Is it the max physical address or size?
      // In fact it is the size, and 286 docs actually refer to it as size 
      // rather than limit.
      // It is also size - 1, else there would be no way to specify
      // all of RAM, and a limit of 0 is invalid.

      var xResult = new byte[8];
      
      // Check the limit to make sure that it can be encoded
      if ((aSize > 65536) && (aSize & 0x0FFF) != 0x0FFF) {
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
      if (aSize > 65536) {
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

    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      var xAsm = (Assembler)aAssembler;

      var xGDT = new List<byte>();
      // Null Segment - Selector 0x00
      // Not used, but required by many emulators.
      xGDT.AddRange(new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
      // Code Segment
      byte xCodeSelector = (byte)xGDT.Count;
      xGDT.AddRange(Descriptor(0x00000000, 0xFFFFFFFF, true));
      // Data Segment - Selector
      byte xDataSelector = (byte)xGDT.Count;
      xGDT.AddRange(Descriptor(0x00000000, 0xFFFFFFFF, false));
      xAsm.DataMembers.Add(new CPUAll.DataMember("_NATIVE_GDT_Contents", xGDT.ToArray()));


      new CPUAll.Comment("Tell CPU about GDT");
      var xGdtDescription = new UInt16[3];
      // Size of GDT Table - 1
      xGdtDescription[0] = (UInt16)(xGDT.Count - 1); 
      xAsm.DataMembers.Add(new CPUAll.DataMember("_NATIVE_GDT_Pointer", xGdtDescription));
      new CPUx86.Move {
        DestinationRef = CPUAll.ElementReference.New("_NATIVE_GDT_Pointer"),
        DestinationIsIndirect = true,
        DestinationDisplacement = 2
        , SourceRef = CPUAll.ElementReference.New("_NATIVE_GDT_Contents") };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("_NATIVE_GDT_Pointer") };
      new CPUx86.Lgdt { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };

      new CPUAll.Comment("Set data segments");
      new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceValue = xDataSelector };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.DS, SourceReg = CPUx86.Registers.AX };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.ES, SourceReg = CPUx86.Registers.AX };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.FS, SourceReg = CPUx86.Registers.AX };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.GS, SourceReg = CPUx86.Registers.AX };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.SS, SourceReg = CPUx86.Registers.AX };

      new CPUAll.Comment("Force reload of code segment");
      new CPUx86.JumpToSegment { Segment = xCodeSelector, DestinationLabel = "flush__GDT__table" };
      new CPUAll.Label("flush__GDT__table");
    }
  }
}
