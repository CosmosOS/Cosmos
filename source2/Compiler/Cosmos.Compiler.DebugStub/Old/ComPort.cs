using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Debug.Consts;
using Cosmos.Assembler.XSharp;

namespace Cosmos.Debug.DebugStub {
  public partial class DebugStub : CodeGroup {
    static public int mComNo = 0;
    static protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };
    static public UInt16 mComAddr;
    static public UInt16 mComStatusAddr;

    public class ReadByteFromComPort : CodeBlock {
      // Input: EDI
      // Output: [EDI]
      // Modified: AL, DX, EDI (+1)
      //
      // Reads a byte into [EDI] and does EDI + 1
      // http://wiki.osdev.org/Serial_ports
      public override void Assemble() {
        Call<ReadALFromComPort>();
        EDI[0] = AL;
        EDI++;
      }
    }

    public abstract class Inlines : CodeBlock {
      // INLINE
      // Modifies: Stack, EDI, AL
      // TODO: Modify X# to allow inlining better by using dynamic labels otherwise
      // repeated use of an inline will fail with conflicting labels.
      // TODO: Allow methods to emit a start label and return automatically
      // and mark inlines so this does not happen.
      //TODO: Allow inlining in X# wtih an attribute - or method like Call<>?
      protected void ReadComPortX32toStack(int xCount) {
        for (int i = 1; i <= xCount; i++) {
          // Make room on the stack for the address
          Push(0);
          // ReadByteFromComPort writes to EDI, then increments
          EDI = ESP;

          // Read address to stack via EDI
          ReadBytesFromComPort(4);
        }
      }

      protected void ReadBytesFromComPort(int xCount) {
        for (int i = 1; i <= xCount; i++) {
          Call<ReadByteFromComPort>();
        }
      }

      protected void WriteBytesToComPort(int xCount) {
        for (int i = 1; i <= xCount; i++) {
          Call<WriteByteToComPort>();
        }
      }
    }

    public class WriteByteToComPort : Inlines {
      // Input: ESI
      // Output: None
      // Modifies: EAX, EDX
      //
      // Sends byte at [ESI] to com port and does esi + 1
      //
      // This sucks to use the stack, but x86 can only read and write ports from AL and
      // we need to read a port before we can write out the value to another port.
      // The overhead is a lot, but compared to the speed of the serial and the fact
      // that we wait on the serial port anyways, its a wash.
      //
      // This could be changed to use interrupts, but that then complicates
      // the code and causes interaction with other code. DebugStub should be
      // as isolated as possible from any other code.
      public override void Assemble() {
        // Sucks again to use DX just for this, but x86 only supports
        // 8 bit address for literals on ports
        DX = mComStatusAddr;

        // Wait for serial port to be ready
        // Bit 5 (0x20) test for Transmit Holding Register to be empty.
        Label = ".Wait";
        AL = Port[DX];
        AL.Test(0x20);
        JumpIf(Flags.Zero, ".Wait");

        // Set address of port
        DX = mComAddr;
        // Get byte to send
        AL = ESI[0];
        // Send the byte
        Port[DX] = AL;

        ESI++;
      }
    }

    public class ReadALFromComPort : Inlines {
      // Modifies: AL, DX
      public override void Assemble() {
        DX = mComStatusAddr;

        // Wait for port to be ready
        Label = ".Wait";
        AL = Port[DX];
        AL.Test(0x01);
        JumpIf(Flags.Zero, ".Wait");

        // Set address of port
        DX = mComAddr;
        // Read byte
        AL = Port[DX];
      }
    }

    //public class WriteALToComPort : CodeBlock {
    //  // Input: AL
    //  // Output: None
    //  // Modifies: EDX, ESI
    //  public override void Assemble() {
    //    EAX.Push();
    //    ESI = ESP;
    //    Call<WriteByteToComPort>();
    //    // Is a local var, cant use Return(4). X# issues the return.
    //    // This also allow the function to preserve EAX.
    //    EAX.Pop();
    //  }
    //}

    public class WriteAXToComPort : Inlines {
      // Input: AX
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        WriteBytesToComPort(2);
        // Is a local var, cant use Return(4). X# issues the return.
        // This also allow the function to preserve EAX.
        EAX.Pop();
      }
    }

    public class WriteEAXToComPort : Inlines {
      // Input: EAX
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        WriteBytesToComPort(4);
        // Is a local var, cant use Return(4). X# issues the return.
        // This also allow the function to preserve EAX.
        EAX.Pop();
      }
    }

  }
}
