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
            XS.Exchange(BX, BX);
            XS.ClearInterruptFlag();

            XS.Comment("Get Invoke list count");
            var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
            Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
            XS.Call(LabelName.Get(xGetInvocationListMethod));
            XS.Add(ESP, 4);
            XS.Pop(EAX);
            XS.Add(EAX, 8);
            XS.Set(EBX, EAX, sourceIsIndirect: true);

            XS.Comment("Get invoke method");
            XS.Add(EAX, 8);
            XS.Set(EDI, EAX, sourceIsIndirect: true, sourceDisplacement: 4); // this line can propably can be removed

            XS.Comment("Get ArgSize");
            int xArgSizeOffset = Ldfld.GetFieldOffset(typeof(Delegate), "$$ArgSize$$");
            Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
            XS.Add(ESP, 4);
            XS.Pop(ECX);
            XS.Add(ECX, (uint) xArgSizeOffset);
            XS.Set(ECX, ECX, sourceIsIndirect: true);

            XS.Comment("Set current invoke list index");
            XS.Set(EDX, 0);

            XS.Comment("Make space for return value");
            int returnSizeOffset = Ldfld.GetFieldOffset(typeof(Delegate), "$$ReturnSize$$");
            Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
            XS.Add(ESP, 4);
            XS.Pop(ESI);
            XS.Add(ESI, (uint)returnSizeOffset);
            XS.Set(ESI, ESI, sourceIsIndirect: true);
            XS.Sub(ESP, ESI);

            XS.Label(".BEGIN_OF_LOOP");
            {
                XS.Compare(EDX, EBX);
                XS.Jump(x86.ConditionalTestEnum.GreaterThanOrEqualTo, ".END_OF_INVOKE");

                XS.PushAllRegisters();

                XS.Comment("Check if delegate has $this");
                XS.Set(EDI, EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
                XS.Add(EDI, 4);
                XS.Set(EDI, EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target"));
                XS.Compare(EDI, 0);
                XS.Jump(x86.ConditionalTestEnum.Zero, ".NO_THIS");
                XS.Label(".HAS_THIS");
                XS.Push(EDI);
                XS.Push(0);
                XS.Label(".NO_THIS");
                XS.Set(EDI, EAX, sourceIsIndirect: true, sourceDisplacement: 4);
                XS.Set(EDI, EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr"));

                XS.Sub(ESI, ECX); // determine how much more space the return value needs than the arguments
                XS.Compare(ESI, 0);
                XS.Jump(x86.ConditionalTestEnum.LessThanOrEqualTo, ".HANDLE_ARGUMENTS");
                XS.Sub(ESP, ESI); // make extra space for the return value

                XS.Label(".HANDLE_ARGUMENTS");
                XS.Comment("Check if delegate has args");
                XS.Compare(ECX, 0);
                XS.Jump(x86.ConditionalTestEnum.Zero, ".NO_ARGS");
                XS.Label(".HAS_ARGS");
                XS.Sub(ESP, ECX);
                XS.Push(EDI);
                XS.Set(EDI, ESP);
                XS.Add(EDI, 4);
                XS.Set(ESI, EBP);
                XS.Add(ESI, 8);
                new x86.Movs { Size = 8, Prefixes = x86.InstructionPrefixes.Repeat };
                XS.Pop(EDI);
                XS.Label(".NO_ARGS");
                XS.Call(EDI);

                XS.Comment("If there is a return value copy it to holding place now");
                Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
                XS.Add(ESP, 4);
                XS.Pop(EAX);
                XS.Add(EAX, (uint)returnSizeOffset);
                XS.Set(EAX, EAX, sourceIsIndirect: true); // got size of return value

                XS.Set(EDI, EBP);
                XS.Sub(EDI, EAX);
                XS.Label(".RETURN_VALUE_LOOP_START");
                XS.Compare(EAX, 0);
                XS.Jump(x86.ConditionalTestEnum.LessThanOrEqualTo, ".RETURN_VALUE_LOOP_END");
                XS.Pop(EBX);
                XS.Set(EDI, EBX, destinationIsIndirect: true);
                XS.Add(EDI, 4);
                XS.Sub(EAX, 4);
                XS.Jump(".RETURN_VALUE_LOOP_START");
                XS.Label(".RETURN_VALUE_LOOP_END");

                XS.PopAllRegisters();
                XS.Increment(EDX);
                XS.Jump(".BEGIN_OF_LOOP");
            }

            XS.Label(".END_OF_INVOKE");

            XS.EnableInterrupts();
        }
    }
}
