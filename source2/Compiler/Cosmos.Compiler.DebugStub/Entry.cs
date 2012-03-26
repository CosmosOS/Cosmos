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
    public class TracerEntry : CodeBlock {
      [XSharp(IsInteruptHandler = true)]
      // Int3 entry point
      public override void Assemble() {
        // We need to make sure Int3 can never run more than one instance at a time.
        // We are not threaded yet, when we are we have to change stuff to thread vars and a lot of other stuff.
        // Two Int3s can never be called at the same time normally, but IRQs can happen while the DebugStub is
        // running. We also need to make sure IRQs are allowed to run during DebugStub as DebugStub can wait for
        // a long time on commands.
        // So we need to disable interrupts immediately and set a flag, then reenable interrupts if they were enabled
        // when we disabled them. Later this can be replaced by some kind of critical section / lock around this code.
        // Currently IRQs are disabled - we need to fix DS before we can reenable them and add support for critical sections / locks here.
        // -http://www.codemaestro.com/reviews/8
        // -http://en.wikipedia.org/wiki/Spinlock - Uses a register which is a problem for us
        // -http://wiki.osdev.org/Spinlock
        //   -Looks good and also allows testing intead of waiting
        //   -Wont require us to disable / enable IRQs

        // This code is temporarily disabled as IRQs are not enabled right now.
        // LockOrExit()
        {
          SaveRegisters();
          {
            Call<Executing>();
          }
          PopAll(); // Restore registers
        }
        // Unlock();
      }

      protected void SaveRegisters() {
        // EBP is restored by PopAll, but SendFrame uses it. Could
        // get it from the PushAll data, but this is easier.
        CallerEBP.Value = EBP;

        // Could get ESP from PushAll but this is easier.
        // Also allows us to use the stack before PushAll if we ever need it.
        //
        // We cant modify any registers since we havent done PushAll yet
        // Maybe we could do a sub(4) on memory direct.. 
        // But for now we remove from ESP which the Int3 produces,
        // store ESP, then restore ESP so we don't cause stack corruption.
        ESP = ESP + 12; // 12 bytes for EFLAGS, CS, EIP
        CallerESP.Value = ESP;
        ESP = ESP - 12;

        PushAll();

        // Save current ESP so we can look at the results of PushAll later
        PushAllPtr.Value = ESP;

        // Get current ESP and add 32. This will skip over the PushAll and point us at the call data from Int3.
        EBP = ESP;
        EBP = EBP + 32;

        // Caller EIP
        EAX = EBP[0];
        // EIP is pointer to op after our call. Int3 is 1 byte so we subtract 1.
        // Note - when we used call it was 5 (the size of our call + address)
        // so we get the EIP as IL2CPU records it. Its also useful for when we will
        // be changing ops that call this stub.
        EAX--;
        // Store it for later use.
        CallerEIP.Value = EAX;
      }
    }
  }
}
