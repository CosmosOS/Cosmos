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
			new CPUx86.Move("eax", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");
			var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
			new CPU.Comment("push address of delgate to stack");
			new CPUx86.Push("eax");//addrof this
			new CPUx86.Call(CPU.Label.GenerateLabelName(xGetInvocationListMethod));
			new CPU.Comment("get address from return value -> eax");
			new CPUx86.Pop("eax");//list
			new CPU.Comment("eax+=8 is where the offset where an array's count is");
			new CPUx86.Add("eax", "8");//addrof list.count??
			new CPU.Comment("store count in ebx");
			new CPUx86.Move("ebx", "[eax]");//list.count
			new CPU.Comment("eax+=8 is where the offset where an array's items start");
			new CPUx86.Add("eax", "8");//why? -- start of list i think?
			new CPUx86.Move("edi", "0");
			new CPU.Comment("ecx = ptr to delegate object");
			new CPUx86.Move("ecx", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");//addrof the delegate
			new CPU.Comment("ecx points to the size of the delegated methods arguments");
			new CPUx86.Move("ecx", "[ecx + " + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ArgSize$$"].Offset + 12) + "]");//the size of the arguments to the method? + 12??? -- 12 is the size of the current call stack.. i think
			new CPUx86.Xor("edx", "edx");//make sure edx is 0
			new CPU.Label(".BEGIN_OF_LOOP");
			new CPUx86.Compare("edx", "ebx");//are we at the end of this list
			new CPUx86.JumpIfEqual(".END_OF_INVOKE_");//then we better stop
			new CPUx86.Pushad();
			new CPU.Comment("esi points to where we will copy the methods argumetns from");
			new CPUx86.Move("esi", "esp");
			new CPU.Comment("edi = ptr to delegate object");
			new CPUx86.Move("edi", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");
			new CPU.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
			new CPUx86.Move("edi", "[edi + " + (MethodInfo.Arguments[0].TypeInfo.Fields["System.Object System.Delegate._target"].Offset + 12) + "]");//i really dont get the +12
			new CPU.Comment("if this == null");
			new CPUx86.Compare("edi", "0");
			new CPUx86.JumpIfZero(".NO_THIS");
			new CPUx86.Push("edi");

			new CPU.Label(".NO_THIS");

			new CPU.Comment("make space for us to copy the arguments too");
			new CPUx86.Sub("esp", "ecx");
			new CPU.Comment("move the current delegate to edi");
			new CPUx86.Move("edi", "[eax]");
			new CPU.Comment("move the methodptr from that delegate to edi ");
			new CPUx86.Move("edi", "[edi + " + (MethodInfo.Arguments[0].TypeInfo.Fields["System.IntPtr System.Delegate._methodPtr"].Offset + 12) + "]");//
			new CPU.Comment("save methodptr on the stack");
			new CPUx86.Push("edi");
			new CPU.Comment("move location to copy args to");
			new CPUx86.Move("edi", "esp");
			new CPU.Comment("get above the saved methodptr");
			new CPUx86.Add("edi", "4");
			//we allocated the argsize on the stack once, and it we need to get above the original args
			new CPU.Comment("we allocated argsize on the stack once");
			new CPU.Comment("add another to the source location 32 for the Pushad + 16 for the current stack");
			new CPUx86.Add("esi", 52);
			new CPUx86.RepeatMovsb();
			new CPUx86.Pop("edi");
			new CPUx86.Call("edi");
			if (MethodInfo.ReturnType != typeof(void))
			{
				//we return a var
			}
			new CPUx86.Popad();
			new CPUx86.Inc("edx");
			new CPUx86.Add("eax", "4");
			new CPUx86.Jump(".BEGIN_OF_LOOP");

			new CPU.Label(".END_OF_INVOKE_");
//            new CPUx86.Sti();
//#warning remove this ^ sti call when issue is fixed!!!
			//MethodInfo.Arguments[0].
			//            new CPUx86.Move("ebx", "[eax + " + (MethodInfo.Arguments[0].TypeInfo.Fields["$$ArgSize$$"].Offset + 12) + "]");

			//new CPUx86.Move("eax", CPUx86.Registers.
		}

		public MethodInformation MethodInfo
		{
			get;
			set;
		}
	}
}