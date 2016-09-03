using System;

using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs;
using Cosmos.IL2CPU.X86.IL;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using MethodBase = System.Reflection.MethodBase;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class InvokeImplAssembler : AssemblerMethod
    {
        private uint GetArgumentStartOffset(MethodBase aMethod, int aArgument)
        {
            return 0;
        }

        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            var xAssembler = (Cosmos.Assembler.Assembler)aAssembler;
            var xMethodInfo = (Cosmos.IL2CPU.MethodInfo)aMethodInfo;
            var xMethodBaseAsInfo = xMethodInfo.MethodBase as global::System.Reflection.MethodInfo;
            if (xMethodBaseAsInfo.ReturnType != typeof(void))
            {
                throw new Exception("Events with return type not yet supported!");
            }
            XS.Comment("XXXXXXX");

            /*
            * EAX contains the GetInvocationList() array at the index at which it was last used
            * EDX contains the index at which the EAX is
            * EBX contains the number of items in the array
            * ECX contains the argument size
            */
            XS.ClearInterruptFlag();
            XS.Label(".DEBUG");
            //XS.Label("____DEBUG_FOR_MULTICAST___");
            XS.Comment("move address of delegate to eax");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));

            var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
            XS.Comment("push address of delgate to stack");
            XS.Push(XSRegisters.EAX);//addrof this
            XS.Call(LabelName.Get(xGetInvocationListMethod));
            XS.Comment("get address from return value -> eax");
            XS.Pop(XSRegisters.EAX);
            ;//list
            XS.Comment("eax+=8 is where the offset where an array's count is");
            XS.Add(XSRegisters.EAX, 8);//addrof list.Length
            XS.Comment("store count in ebx");
            XS.Set(XSRegisters.EBX, XSRegisters.EAX, sourceIsIndirect: true);//list.count
            XS.Comment("eax+=8 is where the offset where an array's items start");
            XS.Add(XSRegisters.EAX, 8); // Put pointer at the first item in the list.
            XS.Set(XSRegisters.EDI, 0);
            XS.Comment("ecx = ptr to delegate object");
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));//addrof the delegate
            XS.Comment("ecx points to the size of the delegated methods arguments");

            XS.Set(XSRegisters.ECX, XSRegisters.ECX, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "$$ArgSize$$"));//the size of the arguments to the method? + 12??? -- 12 is the size of the current call stack.. i think
            XS.Xor(XSRegisters.EDX, XSRegisters.EDX);
            ;//make sure edx is 0
            XS.Label(".BEGIN_OF_LOOP");
            {
                XS.Compare(XSRegisters.EDX, XSRegisters.EBX);//are we at the end of this list
                XS.Jump(CPUx86.ConditionalTestEnum.GreaterThanOrEqualTo, ".END_OF_INVOKE_");//then we better stop
                XS.PushAllRegisters();
                XS.Comment("esi points to where we will copy the methods argumetns from");
                XS.Set(XSRegisters.ESI, XSRegisters.ESP);
                XS.Comment("edi = ptr to delegate object");
                XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
                XS.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target"));
                XS.Compare(XSRegisters.EDI, 0);
                XS.Jump(CPUx86.ConditionalTestEnum.Zero, ".NO_THIS");
                XS.Push(XSRegisters.EDI);

                XS.Label(".NO_THIS");

                XS.Comment("make space for us to copy the arguments too");
                XS.Sub(XSRegisters.ESP, XSRegisters.ECX);
                XS.Comment("move the current delegate to edi");
                XS.Set(XSRegisters.EDI, XSRegisters.EAX, sourceIsIndirect: true);
                XS.Comment("move the methodptr from that delegate to edi ");
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr"));//
                XS.Comment("save methodptr on the stack");
                XS.Push(XSRegisters.EDI);
                XS.Comment("move location to copy args to");
                XS.Set(XSRegisters.EDI, XSRegisters.ESP);
                XS.Add(XSRegisters.EDI, 4);
                //new CPU.Comment("get above the saved methodptr");
                //XS.Sub(XSRegisters.ESP, 4);
                //we allocated the argsize on the stack once, and it we need to get above the original args
                XS.Comment("we allocated argsize on the stack once");
                XS.Comment("add 32 for the Pushad + 16 for the current stack + 4 for the return value");
                //uint xToAdd = 32; // skip pushad data
                //xToAdd += 4; // method pointer
                XS.Set(XSRegisters.ESI, XSRegisters.EBP);
                XS.Add(XSRegisters.ESI, 8); // ebp+8 is first argument
                new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
                XS.Pop(XSRegisters.EDI);
                XS.Label(".BeforeCall");
                XS.Call(XSRegisters.EDI);
                XS.Comment("store return -- return stored into edi after popad");
                XS.Comment("edi = ptr to delegate object");
                XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
                XS.Comment("edi = ptr to delegate object should be a pointer to the delgates context ie (this) for the methods ");
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target"));//i really dont get the +12, MtW: that's for the object header
                XS.Label(".noTHIStoPop");
                XS.PopAllRegisters();
                XS.Increment(XSRegisters.EDX);
                XS.Add(XSRegisters.EAX, 4);
                XS.Jump(".BEGIN_OF_LOOP");
            }
            XS.Label(".END_OF_INVOKE_");
            XS.Comment("get the return value");
            XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));//addrof the delegate
            XS.Set(XSRegisters.EDX, XSRegisters.EDX, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "$$ReturnsValue$$"));
            XS.Compare(XSRegisters.EDX, 0);
            XS.Jump(CPUx86.ConditionalTestEnum.Equal, ".noReturn");
            //may have to expand the return... idk
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationDisplacement: 8);
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationDisplacement: 4);
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationIsIndirect: true);
            XS.Push(XSRegisters.EDX);//ebp
            XS.Set(XSRegisters.ESP, XSRegisters.EDI, destinationDisplacement: 12);
            XS.Label(".noReturn");
            XS.EnableInterrupts();
        }
    }
}
