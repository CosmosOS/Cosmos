using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class TracerEntry : Cosmos.Assembler.Code {

		public TracerEntry(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("We need to make sure Int3 can never run more than one instance at a time.");

			new Comment("We are not threaded yet, when we are we have to change stuff to thread vars and a lot of other stuff.");

			new Comment("Two Int3 calls currently can never happen at the same time normally, but IRQs can happen while the DebugStub is");

			new Comment("running. We also need to make sure IRQs are allowed to run during DebugStub as DebugStub can wait for");

			new Comment("a long time on commands.");

			new Comment("So we need to disable interrupts immediately and set a flag, then reenable interrupts if they were enabled");

			new Comment("when we disabled them. Later this can be replaced by some kind of critical section / lock around this code.");

			new Comment("Currently IRQs are disabled - we need to fix DS before we can reenable them and add support for critical sections / locks here.");

			new Comment("-http:#www.codemaestro.com/reviews/8");

			new Comment("-http:#en.wikipedia.org/wiki/Spinlock - Uses a register which is a problem for us");

			new Comment("-http:#wiki.osdev.org/Spinlock");

			new Comment("-Looks good and also allows testing intead of waiting");

			new Comment("-Wont require us to disable / enable IRQs");

			new Comment("Comment");

			new Comment("X#: Group DebugStub");

			new Comment("X#: InterruptHandler TracerEntry {");
			new Label("DebugStub_TracerEntry");

			new Comment("This code is temporarily disabled as IRQs are not enabled right now.");

			new Comment("LockOrExit");

			new Comment("EBP is restored by PopAll, but SendFrame uses it. Could");

			new Comment("get it from the PushAll data, but this is easier.");

			new Comment("X#: .CallerEBP = EBP");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_CallerEBP"), DestinationIsIndirect = true , SourceReg = RegistersEnum.EBP };

			new Comment("Could get ESP from PushAll but this is easier.");

			new Comment("Also allows us to use the stack before PushAll if we ever need it.");

			new Comment("We cant modify any registers since we havent done PushAll yet");

			new Comment("Maybe we could do a sub(4) on memory direct..");

			new Comment("But for now we remove from ESP which the Int3 produces,");

			new Comment("store ESP, then restore ESP so we don't cause stack corruption.");

			new Comment("12 bytes for EFLAGS, CS, EIP");

			new Comment("X#: ESP + 12");
			new Add { DestinationReg = RegistersEnum.ESP, SourceValue = 12 };

			new Comment("X#: .CallerESP = ESP");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_CallerESP"), DestinationIsIndirect = true , SourceReg = RegistersEnum.ESP };

			new Comment("X#: ESP - 12");
			new Sub { DestinationReg = RegistersEnum.ESP, SourceValue = 12 };

			new Comment("X#: PushAll");
			new Pushad();

			new Comment("Save current ESP so we can look at the results of PushAll later");

			new Comment("X#: .PushAllPtr = ESP");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_PushAllPtr"), DestinationIsIndirect = true , SourceReg = RegistersEnum.ESP };

			new Comment("Get current ESP and add 32. This will skip over the PushAll and point");

			new Comment("us at the call data from Int3.");

			new Comment("X#: EBP = ESP");
			new Mov{ DestinationReg = RegistersEnum.EBP, SourceReg = RegistersEnum.ESP };

			new Comment("X#: EBP + 32");
			new Add { DestinationReg = RegistersEnum.EBP, SourceValue = 32 };

			new Comment("Caller EIP");

			new Comment("X#: EAX = EBP[0]");
			new Mov{ DestinationReg = RegistersEnum.EAX, SourceReg = RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 0 };

			new Comment("EIP is pointer to op after our call. Int3 is 1 byte so we subtract 1.");

			new Comment("Note - when we used call it was 5 (size of our call + address)");

			new Comment("so we get the EIP as IL2CPU records it. Its also useful for when we");

			new Comment("wil be changing ops that call this stub.");

			new Comment("X#: EAX - 1");
			new Dec { DestinationReg = RegistersEnum.EAX };

			new Comment("Store it for later use.");

			new Comment("X#: .CallerEIP = EAX");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_CallerEIP"), DestinationIsIndirect = true , SourceReg = RegistersEnum.EAX };

			new Comment("X#: Call .Executing");
			new Call { DestinationLabel = "DebugStub_Executing" };

			new Comment("X#: PopAll");
			new Popad();

			new Comment("Temp disabled, see comment on LockOrExit above");

			new Comment("Unlock");

			new Comment("X#: }");
			new Label("DebugStub_TracerEntry_Exit");
			new IRET();

		}
	}
}
