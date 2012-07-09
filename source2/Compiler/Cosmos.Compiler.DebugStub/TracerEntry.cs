using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class TracerEntry : Cosmos.Assembler.Code {

		public TracerEntry(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new LiteralAssemblerCode(";  We need to make sure Int3 can never run more than one instance at a time.");

			new LiteralAssemblerCode(";  We are not threaded yet, when we are we have to change stuff to thread vars and a lot of other stuff.");

			new LiteralAssemblerCode(";  Two Int3 calls currently can never happen at the same time normally, but IRQs can happen while the DebugStub is");

			new LiteralAssemblerCode(";  running. We also need to make sure IRQs are allowed to run during DebugStub as DebugStub can wait for");

			new LiteralAssemblerCode(";  a long time on commands.");

			new LiteralAssemblerCode(";  So we need to disable interrupts immediately and set a flag, then reenable interrupts if they were enabled");

			new LiteralAssemblerCode(";  when we disabled them. Later this can be replaced by some kind of critical section / lock around this code.");

			new LiteralAssemblerCode(";  Currently IRQs are disabled - we need to fix DS before we can reenable them and add support for critical sections / locks here.");

			new LiteralAssemblerCode(";  -http://www.codemaestro.com/reviews/8");

			new LiteralAssemblerCode(";  -http://en.wikipedia.org/wiki/Spinlock - Uses a register which is a problem for us");

			new LiteralAssemblerCode(";  -http://wiki.osdev.org/Spinlock");

			new LiteralAssemblerCode(";    -Looks good and also allows testing intead of waiting");

			new LiteralAssemblerCode(";    -Wont require us to disable / enable IRQs");

			new Comment("X#: Group DebugStub");

			new Comment("X#: InterruptHandler TracerEntry {");
			new LiteralAssemblerCode("DebugStub_TracerEntry:");

			new LiteralAssemblerCode(";  This code is temporarily disabled as IRQs are not enabled right now.");

			new LiteralAssemblerCode(";  LockOrExit");

			new LiteralAssemblerCode(";  EBP is restored by PopAll, but SendFrame uses it. Could");

			new LiteralAssemblerCode(";  get it from the PushAll data, but this is easier.");

			new Comment("X#: .CallerEBP = EBP");
			new LiteralAssemblerCode("Mov [DebugStub_CallerEBP], EBP");

			new LiteralAssemblerCode(";  Could get ESP from PushAll but this is easier.");

			new LiteralAssemblerCode(";  Also allows us to use the stack before PushAll if we ever need it.");

			new LiteralAssemblerCode(";  We cant modify any registers since we havent done PushAll yet");

			new LiteralAssemblerCode(";  Maybe we could do a sub(4) on memory direct..");

			new LiteralAssemblerCode(";  But for now we remove from ESP which the Int3 produces,");

			new LiteralAssemblerCode(";  store ESP, then restore ESP so we don't cause stack corruption.");

			new LiteralAssemblerCode(";  12 bytes for EFLAGS, CS, EIP");

			new Comment("X#: ESP + 12");
			new LiteralAssemblerCode("Add ESP, 12");

			new Comment("X#: .CallerESP = ESP");
			new LiteralAssemblerCode("Mov [DebugStub_CallerESP], ESP");

			new Comment("X#: ESP - 12");
			new LiteralAssemblerCode("Sub ESP, 12");

			new Comment("X#: PushAll");
			new LiteralAssemblerCode("Pushad");

			new LiteralAssemblerCode(";  Save current ESP so we can look at the results of PushAll later");

			new Comment("X#: .PushAllPtr = ESP");
			new LiteralAssemblerCode("Mov [DebugStub_PushAllPtr], ESP");

			new LiteralAssemblerCode(";  Get current ESP and add 32. This will skip over the PushAll and point");

			new LiteralAssemblerCode(";  us at the call data from Int3.");

			new Comment("X#: EBP = ESP");
			new LiteralAssemblerCode("Mov EBP, ESP");

			new Comment("X#: EBP + 32");
			new LiteralAssemblerCode("Add EBP, 32");

			new LiteralAssemblerCode(";  Caller EIP");

			new Comment("X#: EAX = EBP[0]");
			new LiteralAssemblerCode("Mov EAX, [EBP + 0]");

			new LiteralAssemblerCode(";  EIP is pointer to op after our call. Int3 is 1 byte so we subtract 1.");

			new LiteralAssemblerCode(";  Note - when we used call it was 5 (size of our call + address)");

			new LiteralAssemblerCode(";  so we get the EIP as IL2CPU records it. Its also useful for when we");

			new LiteralAssemblerCode(";  wil be changing ops that call this stub.");

			new Comment("X#: EAX--");
			new LiteralAssemblerCode("Dec EAX");

			new LiteralAssemblerCode(";  Store it for later use.");

			new Comment("X#: .CallerEIP = EAX");
			new LiteralAssemblerCode("Mov [DebugStub_CallerEIP], EAX");

			new Comment("X#: Executing()");
			new LiteralAssemblerCode("Call DebugStub_Executing");

			new Comment("X#: PopAll");
			new LiteralAssemblerCode("Popad");

			new LiteralAssemblerCode(";  Temp disabled, see comment on LockOrExit above");

			new LiteralAssemblerCode(";  Unlock");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_TracerEntry_Exit:");
			new LiteralAssemblerCode("IRet");

		}
	}
}
