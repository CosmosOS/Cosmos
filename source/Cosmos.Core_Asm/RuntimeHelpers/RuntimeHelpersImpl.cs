using System;
using System.Runtime.CompilerServices;

using IL2CPU.API;
using IL2CPU.API.Attribs;

using XSharp;
using CPUx86 = XSharp.Assembler.x86;

namespace Cosmos.Core_Asm
{
    [Plug(typeof(RuntimeHelpers))]
    public static class RuntimeHelpersImpl
    {
        [Inline(TargetPlatform = TargetPlatform.x86)]
        [PlugMethod]
        public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle)
        {
            // Arguments:
            //    Array aArray, RuntimeFieldHandle aFieldHandle
            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 20); // array
            XS.Set(XSRegisters.ESI, XSRegisters.EBP, sourceDisplacement: 12);  // aFieldHandle
            XS.Add(XSRegisters.EDI, 8);
            XS.Push(XSRegisters.EDI, isIndirect: true); // element size
            XS.Add(XSRegisters.EDI, 4);
            XS.Set(XSRegisters.EAX, XSRegisters.EDI, sourceIsIndirect: true);
            XS.Multiply(XSRegisters.ESP, isIndirect: true, size: XSRegisters.RegisterSize.Int32);
            XS.Pop(XSRegisters.ECX);
            XS.Set(XSRegisters.ECX, XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, 0);
            XS.Add(XSRegisters.EDI, 4);

            XS.Label(".StartLoop");
            XS.Set(XSRegisters.DL, XSRegisters.ESI, sourceIsIndirect: true);
            XS.Set(XSRegisters.EDI, XSRegisters.DL, destinationIsIndirect: true);
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
