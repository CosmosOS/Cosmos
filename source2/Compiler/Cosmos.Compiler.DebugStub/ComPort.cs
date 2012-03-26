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

    public abstract class Inlines : CodeBlock {
      // INLINE
      // Modifies: Stack, EDI, AL
      // TODO: Modify X# to allow inlining better by using dynamic labels otherwise
      // repeated use of an inline will fail with conflicting labels.
      // TODO: Allow methods to emit a start label and return automatically
      // and mark inlines so this does not happen.
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

    public class WaitForDbgHandshake : Inlines {
      public override void Assemble() {
        // "Clear" the UART out
        AL = 0;
        Call<WriteALToComPort>();

        // QEMU (and possibly others) send some garbage across the serial line first.
        // Actually they send the garbage inbound, but garbage could be inbound as well so we 
        // keep this.
        // To work around this we send a signature. DC then discards everything before the signature.
        // QEMU has other serial issues too, and we dont support it anymore, but this signature is a good
        // feature so we kept it.
        Push(Cosmos.Debug.Consts.Consts.SerialSignature);
        ESI = ESP;
        WriteBytesToComPort(4);
        // Restore ESP, we actually dont care about EAX or the value on the stack anymore.
        EAX.Pop();

        // We could use the signature as the start signal, but I prefer
        // to keep the logic separate, especially in DC.
        AL = (int)DsVsip.Started; // Send the actual started signal
        Call<WriteALToComPort>();

        Call<WaitForSignature>();
        Call<ProcessCommandBatch>();
      }
    }

    public class WriteALToComPort : CodeBlock {
      // Input: AL
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        Call<WriteByteToComPort>();
        // Is a local var, cant use Return(4). X# issues the return.
        // This also allow the function to preserve EAX.
        EAX.Pop();
      }
    }

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

    public class InitSerial : CodeBlock {
      // SERIAL DOCS
      //
      // All information relating to our serial usage should be documented in 
      // this comment.
      //
      // We do not use IRQs for debugstub serial. This is becuase DebugStub (DS)
      // MUST be:
      //  - As simple as possible
      //  - Interact as minimal as possible wtih normal Cosmos code because
      //    the debugstub must *always* work even if the normal code is fubarred
      //
      // The serial port that is used for DS should be "hidden" from Cosmos main
      // so that Cosmos main pretends it does not exist.
      //
      // IRQs would create a clash/mix of code.
      // This does make the serial code in DebugStub inefficient, but its well worth
      // the benefits received by following these rules.
      //
      // Baud rate is set to 115200. Likely our code could not exceed this rate
      // anyways the way it is written and there are compatibility issues on some
      // hardware above this rate.
      //
      // We assume a minimum level of a 16550A, which should be no problem on any 
      // common hardware today. VMWare emulates the 16550A
      //
      // We do not handle flow control for outbound data (DS --> DC).
      // The DebugConnector (DC, the code in the Visual Studio side) however is threaded
      // and easily should be able to receive data faster than we can send it.
      // Most things are transactional with data being sent only when asked for, but 
      // with tracing we do send a data directly.
      //
      // Currently there is no inbound flow control either (DC --> DS)
      // For now we assume all commands in bound are 16 bytes or less to ensure
      // that they fit in the FIFO. Commands in DS must wait for a command ID ACK
      // before sending another command.
      // See notes in ProcessCommand.
      public override void Assemble() {
        // http://www.nondot.org/sabre/os/files/Communication/ser_port.txt

        // Disable interrupts
        DX = (UInt16)(mComAddr + 1);
        AL = 0;
        Port[DX] = AL;

        // Enable DLAB (set baud rate divisor)
        DX = (UInt16)(mComAddr + 3);
        AL = 0x80;
        Port[DX] = AL;

        // 0x01 - 0x00 - 115200
        // 0x02 - 0x00 - 57600
        // 0x03 - 0x00 - 38400
        //
        // Set divisor (lo byte)
        DX = mComAddr;
        AL = 0x01;
        Port[DX] = AL;
        // hi byte
        DX = (UInt16)(mComAddr + 1);
        AL = 0x00;
        Port[DX] = AL;

        // 8N1
        DX = (UInt16)(mComAddr + 3);
        AL = 0x03;
        Port[DX] = AL;

        // Enable FIFO, clear them
        // Set 14-byte threshold for IRQ.
        // We dont use IRQ, but you cant set it to 0
        // either. IRQ is enabled/diabled separately
        DX = (UInt16)(mComAddr + 2);
        AL = 0xC7;
        Port[DX] = AL;

        // 0x20 AFE Automatic Flow control Enable - 16550 (VMWare uses 16550A) is most common and does not support it
        // 0x02 RTS
        // 0x01 DTR
        // Send 0x03 if no AFE
        DX = (UInt16)(mComAddr + 4);
        AL = 0x03;
        Port[DX] = AL;
      }
    }
  
  }
}
