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
            XS.Set(XSRegisters.RDI, XSRegisters.RBP, sourceDisplacement: 20); // array
            XS.Set(XSRegisters.RSI, XSRegisters.RBP, sourceDisplacement: 12);  // aFieldHandle
            XS.Add(XSRegisters.RDI, 8);
            XS.Push(XSRegisters.RDI, isIndirect: true); // element size
            XS.Add(XSRegisters.RDI, 4);
            XS.Set(XSRegisters.RAX, XSRegisters.RDI, sourceIsIndirect: true);
            XS.Multiply(XSRegisters.RSP, isIndirect: true, size: XSRegisters.RegisterSize.Long64);
            XS.Pop(XSRegisters.RCX);
            XS.Set(XSRegisters.RCX, XSRegisters.RAX);
            XS.Set(XSRegisters.RAX, 0);
            XS.Add(XSRegisters.RDI, 4);

            XS.Label(".StartLoop");
            XS.Set(XSRegisters.DL, XSRegisters.RSI, sourceIsIndirect: true);
            XS.Set(XSRegisters.RDI, XSRegisters.DL, destinationIsIndirect: true);
            XS.Add(XSRegisters.RAX, 1);
            XS.Add(XSRegisters.RSI, 1);
            XS.Add(XSRegisters.RDI, 1);
            XS.Compare(XSRegisters.RAX, XSRegisters.RCX);
            XS.Jump(CPUx86.ConditionalTestEnum.Equal, ".EndLoop");
            XS.Jump(".StartLoop");

            XS.Label(".EndLoop");
        }
    }
}
