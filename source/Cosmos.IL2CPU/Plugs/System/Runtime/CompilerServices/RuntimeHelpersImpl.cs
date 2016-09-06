using System;

using Cosmos.Assembler;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.Plugs.System.Runtime.CompilerServices
{
    [Plug(Target = typeof(global::System.Runtime.CompilerServices.RuntimeHelpers))]
    public static class RuntimeHelpersImpl
    {

        public static void cctor()
        {
            //TODO: do something
        }

        public new static bool Equals(object o1, object o2)
        {
            if (o1 == null
                && o2 == null)
            {
                return true;
            }
            if (o1 == null
                || o2 == null)
            {
                return false;
            }
            return object.Equals(o1, o2);
        }

        [Inline(TargetPlatform = TargetPlatform.x86)]
        [PlugMethod]
        public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle)
        {
            // Arguments:
            //    Array aArray, RuntimeFieldHandle aFieldHandle
            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 20); // array
            XS.Set(XSRegisters.ESI, XSRegisters.EBP, sourceDisplacement: 12);  // aFieldHandle
            XS.Add(XSRegisters.EDI, 8);
            XS.Push(EDI, isIndirect: true); // element size
            XS.Add(XSRegisters.EDI, 4);
            XS.Set(EAX, EDI, sourceIsIndirect: true);
            XS.Multiply(ESP, isIndirect: true, size: RegisterSize.Int32);
            XS.Pop(XSRegisters.ECX);
            XS.Set(XSRegisters.ECX, XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, 0);
            XS.Add(XSRegisters.EDI, 4);

            XS.Label(".StartLoop");
            XS.Set(DL, ESI, sourceIsIndirect: true);
            XS.Set(EDI, DL, destinationIsIndirect: true);
            XS.Add(XSRegisters.EAX, 1);
            XS.Add(XSRegisters.ESI, 1);
            XS.Add(XSRegisters.EDI, 1);
            XS.Compare(XSRegisters.EAX, XSRegisters.ECX);
            XS.Jump(CPUx86.ConditionalTestEnum.Equal, ".EndLoop");
            XS.Jump(".StartLoop");

            XS.Label(".EndLoop");
        }
    }
}
