using System;
using System.Reflection;
using XSharp.Assembler;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.X86.IL;
using XSharp;
using x86 = XSharp.Assembler.x86;

// ReSharper disable once CheckNamespace
namespace Cosmos.CPU_Asm {
    public class DelegateInvokeAsm : AssemblerMethod {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo) {
            var xAssembler = aAssembler;
            var xMethodInfo = (_MethodInfo)aMethodInfo;
            var xMethodBaseAsInfo = xMethodInfo.MethodBase as global::System.Reflection.MethodInfo;
            if (xMethodBaseAsInfo.ReturnType != typeof(void)) {
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
            XS.Add(XSRegisters.ESP, 4);
            XS.Pop(XSRegisters.EAX);
            XS.Add(XSRegisters.EAX, 8);
            XS.Set(XSRegisters.EBX, XSRegisters.EAX, sourceIsIndirect: true);

            XS.Comment("Get invoke method");
            XS.Add(XSRegisters.EAX, 8);
            XS.Set(XSRegisters.EDI, XSRegisters.EAX, sourceIsIndirect: true, sourceDisplacement: 4);

            XS.Comment("Get ArgSize");
            int xArgSizeOffset = Ldfld.GetFieldOffset(typeof(global::System.Delegate), "$$ArgSize$$");
            Ldarg.DoExecute(aAssembler, xMethodInfo, 0);
            XS.Add(XSRegisters.ESP, 4);
            XS.Pop(XSRegisters.ECX);
            XS.Add(XSRegisters.ECX, (uint)xArgSizeOffset);
            XS.Set(XSRegisters.ECX, XSRegisters.ECX, sourceIsIndirect: true);

            XS.Comment("Set current invoke list index");
            XS.Set(XSRegisters.EDX, 0);

            XS.Label(".BEGIN_OF_LOOP");
            {
                XS.Compare(XSRegisters.EDX, XSRegisters.EBX);
                XS.Jump(x86.ConditionalTestEnum.GreaterThanOrEqualTo, ".END_OF_INVOKE");

                XS.PushAllRegisters();

                XS.Comment("Check if delegate has $this");
                XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
                XS.Add(XSRegisters.EDI, 4);
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.Object System.Delegate._target"));
                XS.Compare(XSRegisters.EDI, 0);
                XS.Jump(x86.ConditionalTestEnum.Zero, ".NO_THIS");
                XS.Label(".HAS_THIS");
                XS.Push(XSRegisters.EDI);
                XS.Push(0);
                XS.Label(".NO_THIS");
                XS.Set(XSRegisters.EDI, XSRegisters.EAX, sourceIsIndirect: true, sourceDisplacement: 4);
                XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "System.IntPtr System.Delegate._methodPtr"));

                XS.Comment("Check if delegate has args");
                XS.Compare(XSRegisters.ECX, 0);
                XS.Jump(x86.ConditionalTestEnum.Zero, ".NO_ARGS");
                XS.Label(".HAS_ARGS");
                XS.Sub(XSRegisters.ESP, XSRegisters.ECX);
                XS.Push(XSRegisters.EDI);
                XS.Set(XSRegisters.EDI, XSRegisters.ESP);
                XS.Add(XSRegisters.EDI, 4);
                XS.Set(XSRegisters.ESI, XSRegisters.EBP);
                XS.Add(XSRegisters.ESI, 8);
                new x86.Movs { Size = 8, Prefixes = x86.InstructionPrefixes.Repeat };
                XS.Pop(XSRegisters.EDI);
                XS.Label(".NO_ARGS");
                XS.Call(XSRegisters.EDI);

                XS.PopAllRegisters();
                XS.Increment(XSRegisters.EDX);
                XS.Jump(".BEGIN_OF_LOOP");
            }

            XS.Label(".END_OF_INVOKE");
            XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: Ldarg.GetArgumentDisplacement(xMethodInfo, 0));
            XS.Set(XSRegisters.EDX, XSRegisters.EDX, sourceDisplacement: Ldfld.GetFieldOffset(xMethodInfo.MethodBase.DeclaringType, "$$ReturnsValue$$"));
            XS.Compare(XSRegisters.EDX, 0);
            XS.Jump(x86.ConditionalTestEnum.Equal, ".NO_RETURN");

            XS.Label(".HAS_RETURN");
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationDisplacement: 8);
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationDisplacement: 4);
            XS.Exchange(XSRegisters.EBP, XSRegisters.EDX, destinationIsIndirect: true);
            XS.Push(XSRegisters.EDX);
            XS.Set(XSRegisters.ESP, XSRegisters.EDI, destinationDisplacement: 12);

            XS.Label(".NO_RETURN");
            XS.EnableInterrupts();
        }
    }
}
