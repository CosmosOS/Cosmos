using System;
using Cosmos.Assembler;
using Cosmos.IL2CPU.X86.IL;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.Plugs.Assemblers.Delegate
{
    public class DelegateInvokeAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            var xAssembler = aAssembler;
            var xMethodInfo = (MethodInfo) aMethodInfo;
            var xMethodBaseAsInfo = xMethodInfo.MethodBase as global::System.Reflection.MethodInfo;
            if (xMethodBaseAsInfo.ReturnType != typeof(void))
            {
                throw new Exception("Events with return type not yet supported!");
            }

            /*
            * EAX contains the GetInvocationList() array at the index at which it was last used
            * EDX contains the index at which the EAX is
            * EBX contains the number of items in the array
            * ECX contains the argument size
            */

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
            XS.Set(EDI, EAX, sourceIsIndirect: true, sourceDisplacement: 4);

            XS.Comment("Get ArgSize");
            int xArgSizeOffset = Ldfld.GetFieldOffset(typeof(global::System.Delegate), "$$ArgSize$$");
            Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
            XS.Add(ESP, 4);
            XS.Pop(ECX);
            XS.Add(ECX, (uint) xArgSizeOffset);
            XS.Set(ECX, ECX, sourceIsIndirect: true);

            XS.Comment("Set current invoke list index");
            XS.Set(EDX, 0);

            XS.Label(".BEGIN_OF_LOOP");
            {
                XS.Compare(EDX, EBX);
                XS.Jump(Assembler.x86.ConditionalTestEnum.GreaterThanOrEqualTo, ".END_OF_INVOKE");

                XS.PushAllRegisters();

                XS.Comment("Check if delegate has $this");
                XS.Set(EDI, EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
                XS.Add(EDI, 4);
                XS.Set(EDI, EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target"));
                XS.Compare(EDI, 0);
                XS.Jump(Assembler.x86.ConditionalTestEnum.Zero, ".NO_THIS");
                XS.Label(".HAS_THIS");
                XS.Push(EDI);
                XS.Push(0);
                XS.Label(".NO_THIS");
                XS.Set(EDI, EAX, sourceIsIndirect: true, sourceDisplacement: 4);
                XS.Set(EDI, EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr"));

                XS.Comment("Check if delegate has args");
                XS.Compare(ECX, 0);
                XS.Jump(Assembler.x86.ConditionalTestEnum.Zero, ".NO_ARGS");
                XS.Label(".HAS_ARGS");
                XS.Sub(ESP, ECX);
                XS.Push(EDI);
                XS.Set(EDI, ESP);
                XS.Add(EDI, 4);
                XS.Set(ESI, EBP);
                XS.Add(ESI, 8);
                new Assembler.x86.Movs { Size = 8, Prefixes = Assembler.x86.InstructionPrefixes.Repeat };
                XS.Pop(EDI);
                XS.Label(".NO_ARGS");
                XS.Call(EDI);

                XS.PopAllRegisters();
                XS.Increment(EDX);
                XS.Jump(".BEGIN_OF_LOOP");
            }

            XS.Label(".END_OF_INVOKE");
            XS.Set(EDX, EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
            XS.Set(EDX, EDX, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "$$ReturnsValue$$"));
            XS.Compare(EDX, 0);
            XS.Jump(Assembler.x86.ConditionalTestEnum.Equal, ".NO_RETURN");

            XS.Label(".HAS_RETURN");
            XS.Exchange(EBP, EDX, destinationDisplacement: 8);
            XS.Exchange(EBP, EDX, destinationDisplacement: 4);
            XS.Exchange(EBP, EDX, destinationIsIndirect: true);
            XS.Push(EDX);
            XS.Set(ESP, EDI, destinationDisplacement: 12);

            XS.Label(".NO_RETURN");
            XS.EnableInterrupts();
        }
    }
}
