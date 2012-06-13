using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace MatthijsPlayground
{
	public class Test {
		public void Assemble() {
new Comment("From Entry.CS");
new Comment("EBP is restored by PopAll, but SendFrame uses it. Could");
new Comment("get it from the PushAll data, but this is easier.");
//new ;
new Comment("Could get ESP from PushAll but this is easier.");
new Comment("Also allows us to use the stack before PushAll if we ever need it.");
new Comment("We cant modify any registers since we havent done PushAll yet");
new Comment("Maybe we could do a sub(4) on memory direct..");
new Comment("But for now we remove from ESP which the Int3 produces,");
new Comment("store ESP, then restore ESP so we don't cause stack corruption.");
new Comment("12 bytes for EFLAGS, CS, EIP");
new Add { DestinationReg = RegistersEnum.ESP, SourceValue = 12 };
//new ;
new Sub { DestinationReg = RegistersEnum.ESP, SourceValue = 12 };
//new ;
new Comment("Save current ESP so we can look at the results of PushAll later");
//new ;
new Comment("Get current ESP and add 32. This will skip over the PushAll and point");
new Comment("us at the call data from Int3.");
new Mov{ DestinationReg = RegistersEnum.EBP, SourceReg = RegistersEnum.ESP };
new Add { DestinationReg = RegistersEnum.EBP, SourceValue = 32 };
new Comment("Caller EIP");
//new ;
new Comment("EIP is pointer to op after our call. Int3 is 1 byte so we subtract 1.");
new Comment("Note - when we used call it was 5 (size of our call + address)");
new Comment("so we get the EIP as IL2CPU records it. Its also useful for when we");
new Comment("wil be changing ops that call this stub.");
new Sub { DestinationReg = RegistersEnum.EAX, SourceValue = 1 };
new Comment("Store it for later use.");
//new ;
		}
	}
}
