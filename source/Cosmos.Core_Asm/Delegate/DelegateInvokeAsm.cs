using System;

using IL2CPU.API;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.X86.IL;

using XSharp;
using XSharp.Assembler;
using x86 = XSharp.Assembler.x86;

using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class DelegateInvokeAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            var xMethodInfo = (Il2cpuMethodInfo) aMethodInfo;

            /*
            * EAX contains the GetInvocationList() array at the index at which it was last used
            * EBX contains the number of items in the array
            * ECX contains the argument size
            * EDX contains the current index in the array
            * ESI contains the size of the return value
            * EDI contains the function pointer
            */

            XS.ClearInterruptFlag();

            XS.Comment("Get Invoke list count");
            var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
            Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
            XS.Call(LabelName.Get(xGetInvocationListMethod));
            XS.Add(RSP, 4);
            XS.Pop(RAX);
            XS.Add(RAX, 8);
            XS.Set(RBX, RAX, sourceIsIndirect: true);

            XS.Comment("Get invoke method");
            XS.Add(RAX, 8);
            XS.Set(RDI, RAX, sourceIsIndirect: true, sourceDisplacement: 4); // this line can propably can be removed

            XS.Comment("Get ArgSize");
            int xArgSizeOffset = Ldfld.GetFieldOffset(typeof(Delegate), "$$ArgSize$$");
            Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
            XS.Add(RSP, 4);
            XS.Pop(RCX);
            XS.Add(RCX, (uint) xArgSizeOffset);
            XS.Set(RCX, RCX, sourceIsIndirect: true);

            XS.Comment("Set current invoke list index");
            XS.Set(RDX, 0);

            XS.Comment("Make space for return value");
            int returnSizeOffset = Ldfld.GetFieldOffset(typeof(Delegate), "$$ReturnSize$$");
            Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
            XS.Add(RSP, 4);
            XS.Pop(RSI);
            XS.Add(RSI, (uint)returnSizeOffset);
            XS.Set(RSI, RSI, sourceIsIndirect: true);
            XS.Sub(RSP, RSI);

            XS.Label(".BEGIN_OF_LOOP");
            {
                XS.Compare(RDX, RBX);
                XS.Jump(x86.ConditionalTestEnum.GreaterThanOrEqualTo, ".END_OF_INVOKE");
                XS.PushAllRegisters();

                XS.Comment("Check if delegate has $this");
                XS.Set(RDI, RBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
                XS.Add(RDI, 4);
                XS.Set(RDI, RDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target"));
                XS.Set(RDX, RCX); // edx contains the size of the arguments including $this
                XS.Compare(RDI, 0);
                XS.Jump(x86.ConditionalTestEnum.Zero, ".NO_THIS");
                XS.Label(".HAS_THIS");
                XS.Push(RDI);
                XS.Set(RDI, RDI, sourceIsIndirect: true); // get type of target object
                XS.Add(RDX, 4); // we have at least one int of $this

                //TODO: In future we might be able to replace the following call with a check
                //if the object is boxed and in that case assume its a struct

                // safe info from registers which get trashed
                XS.Push(RAX);
                XS.Push(RBX);
                XS.Push(RCX);
                XS.Push(RDX);

                XS.Push(RDI);
                XS.Call(LabelName.Get(VTablesImplRefs.IsStructRef));
                XS.Pop(RDI);

                // restore values
                XS.Pop(RDX);
                XS.Pop(RCX);
                XS.Pop(RBX);
                XS.Pop(RAX);

                // now check if target turned out to be struct
                XS.Compare(RDI, 1);
                XS.Jump(x86.ConditionalTestEnum.Equal, ".Struct"); //structs are just the pointer so we are already done
                XS.Push(0);
                XS.Add(RDX, 4);
                XS.Jump(".NO_THIS");
                XS.Label(".Struct");
                XS.Add(RSP, ObjectUtils.FieldDataOffset, destinationIsIndirect: true);
                XS.Label(".NO_THIS");
                XS.Set(RDI, RAX, sourceIsIndirect: true, sourceDisplacement: 4);
                XS.Set(RDI, RDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr"));

                XS.Set(RBX, 0); // initialise required extra space to 0
                XS.Compare(RSI, RDX);
                XS.Jump(x86.ConditionalTestEnum.LessThanOrEqualTo, ".NO_RETURN_VALUE_SPACE");
                XS.Set(RBX, RSI);
                XS.Sub(RBX, RCX);
                XS.Label(".NO_RETURN_VALUE_SPACE");

                XS.Comment("Check if delegate has args");
                XS.Compare(RCX, 0);
                XS.Jump(x86.ConditionalTestEnum.Zero, ".NO_ARGS");
                XS.Label(".HAS_ARGS");
                XS.Sub(RSP, RCX);
                XS.Push(RDI);
                XS.Set(RDI, RSP);
                XS.Add(RDI, 4);
                XS.Set(RSI, RBP);
                XS.Compare(RBX, 0);
                XS.Jump(x86.ConditionalTestEnum.Equal, ".NO_RETURN_EXTRA");
                XS.Add(RSI, RBX); // to skip the extra space reserved for the return value
                XS.Jump(".AFTER_ADJUST_ESI");
                XS.Label(".NO_RETURN_EXTRA");
                XS.Add(RSI, 8);
                XS.Label(".AFTER_ADJUST_ESI");
                new x86.Movs { Size = 8, Prefixes = x86.InstructionPrefixes.Repeat };
                XS.Pop(RDI);
                XS.Label(".NO_ARGS");

                XS.Sub(RSP, RBX); // make extra space for the return value
                XS.Call(RDI);

                XS.Comment("If there is a return value copy it to holding place now");
                Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
                XS.Add(RSP, 4);
                XS.Pop(RAX);
                XS.Add(RAX, (uint)returnSizeOffset);
                XS.Set(RAX, RAX, sourceIsIndirect: true); // got size of return value

                XS.Set(RDI, RBP);
                XS.Sub(RDI, RAX);
                XS.Label(".RETURN_VALUE_LOOP_START");
                XS.Compare(RAX, 0);
                XS.Jump(x86.ConditionalTestEnum.LessThanOrEqualTo, ".RETURN_VALUE_LOOP_END");
                XS.Pop(RBX);
                XS.Set(RDI, RBX, destinationIsIndirect: true);
                XS.Add(RDI, 4);
                XS.Sub(RAX, 4);
                XS.Jump(".RETURN_VALUE_LOOP_START");
                XS.Label(".RETURN_VALUE_LOOP_END");

                XS.PopAllRegisters();
                XS.Increment(RDX);
                XS.Jump(".BEGIN_OF_LOOP");
            }

            XS.Label(".END_OF_INVOKE");

            XS.EnableInterrupts();
        }
    }
}
