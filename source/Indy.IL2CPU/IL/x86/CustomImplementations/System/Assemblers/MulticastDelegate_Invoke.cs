using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPU = Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System.Assemblers
{
	public class MulticastDelegate_Invoke : AssemblerMethod, INeedsMethodInfo
	{
		/// <summary>
		/// <para>This method implements Multicast Invoke. This means that it should call all delegates
		/// in the current multicast delegate. </para>
		/// <para>The argument size is available in the <code>$$ArgSize$$</code> field. This value is already rounded to 4byte boundaries</para>
		/// </summary>
		/// <param name="aAssembler"></param>
		public override void Assemble(Assembler.Assembler aAssembler)
		{
			if (MethodInfo == null)
			{
				throw new Exception("This AssemblerMethod needs MethodInfo!");
			}
			/*
			 * EAX contains the GetInvocationList() array at the index at which it was last used
			 * EDX contains the index at which the EAX is
			 * EBX contains the number of items in the array
			 * ECX contains the argument size
			 */
			new CPU.Label("____DEBUG_FOR_MULTICAST___");
            //            new CPUx86.Cli();//DEBUG ONLY
            //#warning reenable interupts when issue is fixed!!!
			new CPU.Comment("move address of delgate to eax");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = MethodInfo.Arguments[0].VirtualAddresses[0] };
			var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
			new CPU.Comment("push address of delgate to stack");
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };//addrof this
            new CPUx86.Call { DestinationLabel = CPU.Label.GenerateLabelName(xGetInvocationListMethod) };
			new CPU.Comment("get address from return value -> eax");
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; ;//list
			new CPU.Comment("eax+=8 is where the offset where an array's count is");
            new CPUx86.Add { DestinationReg = Registers.EAX, SourceValue = 8 };//addrof list.count??
			new CPU.Comment("store count in ebx");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };//list.count
			new CPU.Comment("eax+=8 is where the offset where an array's items start");
            new CPUx86.Add { DestinationReg = Registers.EAX, SourceValue = 8 };//why? -- start of list i think? MtW: the array's .Length is at +8
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceValue = 0 };
			new CPU.Comment("ecx = ptr to delegate object");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = MethodInfo.Arguments[0].VirtualAddresses[0] };//addrof the delegate
			new CPU.Comment("ecx points to the size of the delegated methods arguments");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true, SourceDisplacement = (MethodInfo.Arguments[0].TypeInfo.Fields["$$ArgSize$$"].Offset + 12) };//the size of the arguments to the method? + 12??? -- 12 is the size of the current call stack.. i think
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX }; ;//make sure edx is 0
			new CPU.Label(".BEGIN_OF_LOOP");
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBX };//are we at the end of this list
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".END_OF_INVOKE_" };//then we better stop
			//new CPUx86.Compare("edx", 0);
			//new CPUx86.JumpIfLessOrEqual(".noreturnYet");
			//new CPUx86.Add("esp", 4);
			//new CPU.Label(".noreturnYet");
			//new CPU.Comment("space for the return value");
			//new CPUx86.Pushd("0");
			new CPUx86.Pushad();
			new CPU.Comment("esi points to where we will copy the methods argumetns from");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESP };
			new CPU.Comment("edi = ptr to delegate object");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = MethodInfo.Arguments[0].VirtualAddresses[0] };
			new CPU.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true, SourceDisplacement = (MethodInfo.Arguments[0].TypeInfo.Fields["System.Object System.Delegate._target"].Offset + 12) };//i really dont get the +12. MtW: +12 because of extra header of the type (object type, object id, field count)
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDI, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = ".NO_THIS" };
            new CPUx86.Push { DestinationReg = Registers.EDI };

			new CPU.Label(".NO_THIS");

			new CPU.Comment("make space for us to copy the arguments too");
            new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.ECX };
			new CPU.Comment("move the current delegate to edi");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
			new CPU.Comment("move the methodptr from that delegate to edi ");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true, SourceDisplacement = (MethodInfo.Arguments[0].TypeInfo.Fields["System.IntPtr System.Delegate._methodPtr"].Offset + 12) };//
			new CPU.Comment("save methodptr on the stack");
            new CPUx86.Push { DestinationReg = Registers.EDI };
			new CPU.Comment("move location to copy args to");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESP };
			new CPU.Comment("get above the saved methodptr");
            new CPUx86.Add { DestinationReg = Registers.EDI, SourceValue = 4 };
			//we allocated the argsize on the stack once, and it we need to get above the original args
			new CPU.Comment("we allocated argsize on the stack once");
			new CPU.Comment("add 32 for the Pushad + 16 for the current stack + 4 for the return value");
            new CPUx86.Add { DestinationReg = Registers.ESI, SourceValue = 52 };
            new CPUx86.Movs { Size = 8, Prefixes = InstructionPrefixes.Repeat };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDI };
            new CPUx86.Call { DestinationReg = CPUx86.Registers.EDI };
			new CPU.Comment("store return -- return stored into edi after popad");
			//new CPUx86.Move("edx", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");//addrof the delegate
			//new CPUx86.Move("edx", "[edx+" + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ReturnsValue$$"].Offset + 12) + "]");
			//new CPUx86.Compare(Registers_Old.EDX, 0);
			//new CPUx86.JumpIfEqual(".getReturn");
			//new CPUx86.Move(Registers_Old.EAX, "[esp]");
			//new CPUx86.Move("[esp+0x20]", Registers_Old.EAX);
			//new CPU.Label(".getReturn");
			new CPU.Comment("edi = ptr to delegate object");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = MethodInfo.Arguments[0].VirtualAddresses[0] };
			new CPU.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true, SourceDisplacement = (MethodInfo.Arguments[0].TypeInfo.Fields["System.Object System.Delegate._target"].Offset + 12) };//i really dont get the +12, MtW: that's for the object header
            //new CPUx86.Compare("edi", "0");
            //new CPUx86.JumpIfEqual(".noTHIStoPop");
            //new CPUx86.Move("edx", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");//addrof the delegate
            //new CPUx86.Move("edx", "[edx+" + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ReturnsValue$$"].Offset + 12) + "]");
            //new CPUx86.Compare(Registers_Old.EDX, 0);
            //new JumpIfNotEqual(".needToPopThis");
            //new CPU.Comment("ecx = ptr to delegate object");
            //new CPUx86.Move("ecx", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");//addrof the delegate
            //new CPU.Comment("ecx points to the size of the delegated methods arguments");
            //new CPUx86.Move("ecx", "[ecx + " + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ArgSize$$"].Offset + 12) + "]");//the size of the arguments to the method? + 12??? -- 12 is the size of the current call stack.. i think
            //new CPUx86.Compare("ecx", "0");
            //new CPUx86.JumpIfLessOrEqual(".noTHIStoPop");
			//new CPU.Label(".needToPopThis");
			//new CPUx86.Pop("edi");
			//new CPUx86.Move("[esp]", "edi");
			new CPU.Label(".noTHIStoPop");
			new CPUx86.Popad();
            new CPUx86.Inc { DestinationReg = Registers.EDX };
            new CPUx86.Add { DestinationReg = Registers.EAX, SourceValue = 4 };
            new CPUx86.Jump { DestinationLabel = ".BEGIN_OF_LOOP" };
			new CPU.Label(".END_OF_INVOKE_");
			new CPU.Comment("get the return value");
			//new CPUx86.Pop("eax");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = MethodInfo.Arguments[0].VirtualAddresses[0] };//addrof the delegate
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX, SourceIsIndirect = true, SourceDisplacement = (MethodInfo.Arguments[0].TypeInfo.Fields["$$ReturnsValue$$"].Offset + 12) };
            new CPUx86.Compare { DestinationReg = Registers.EDX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = ".noReturn" };
			//may have to expand the return... idk
            new CPUx86.Xchg { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceReg = Registers.EDX };
            new CPUx86.Xchg{DestinationReg=Registers.EBP, DestinationIsIndirect=true, DestinationDisplacement=4, SourceReg=Registers.EDX};
            new CPUx86.Xchg { DestinationReg = Registers.EBP, DestinationIsIndirect = true, SourceReg = Registers.EDX };
            new CPUx86.Push { DestinationReg = Registers.EDX };//ebp
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 12, SourceReg = CPUx86.Registers.EDI };
			new CPU.Label(".noReturn");
//            new CPUx86.Sti();
//#warning remove this ^ sti call when issue is fixed!!!
			//MethodInfo.Arguments[0].
			//            new CPUx86.Move("ebx", "[eax + " + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ArgSize$$"].Offset + 12) + "]");

			//new CPUx86.Move("eax", CPUx86.Registers_Old.
		}

		public MethodInformation MethodInfo
		{
			get;
			set;
		}
	}
}