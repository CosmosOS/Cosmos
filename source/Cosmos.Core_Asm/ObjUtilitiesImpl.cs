using Cosmos.Core;
using IL2CPU.API.Attribs;
using System;
using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(ObjUtilities))]
    public static unsafe class ObjUtilitiesImpl
    {
        [PlugMethod(Assembler = typeof(ObjUtilitiesGetPointer))]
        public static uint GetPointer(Delegate aVal) { return 0; }

        [PlugMethod(Assembler = typeof(ObjUtilitiesGetPointer))]
        public static uint GetPointer(Object aVal) { return 0; }
    }

    public class ObjUtilitiesGetPointer : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x8);
            XS.Push(XSRegisters.EAX);
        }
    }
}
