using System;
using System.Reflection;
using Cosmos.IL2CPU;
using IL2CPU.API;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Plugs.System
{
    class GCImplCreateNewArrayAsm : AssemblerMethod
    {
        // this code is mostly copied from Newarr.cs in Il2CPU, just the code to find the size and length is different
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            string xTypeID = ILOp.GetTypeIDLabel(typeof(Array));
            MethodBase xCtor = typeof(Array).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)[0];
            string xCtorName = LabelName.Get(xCtor);

            XS.Set(RCX, RBP, sourceDisplacement: 8); // size
            XS.Set(RDX, RBP, sourceDisplacement: 12); // length

            XS.Push(RCX);  // size of element
            XS.Set(RAX, RCX);
            XS.Multiply(RDX); // total element size
            XS.Add(RAX, ObjectUtils.FieldDataOffset + 4); // total array size
            XS.Push(RAX);
            XS.Call(LabelName.Get(GCImplementationRefs.AllocNewObjectRef));
            XS.Label(".AfterAlloc");
            XS.Pop(RAX);
            XS.Pop(RSI);
            XS.Push(RAX);
            XS.Push(RSP, isIndirect: true);
            XS.Push(RSP, isIndirect: true);
            // it's on the stack 3 times now, once from the return value, twice from the pushes;

            XS.Pop(RAX);
            XS.Set(RBX, xTypeID, sourceIsIndirect: true);  // array type id
            XS.Set(RAX, RBX, destinationIsIndirect: true); // array type id
            XS.Set(RAX, (uint)ObjectUtils.InstanceTypeEnum.Array, destinationDisplacement: 4, destinationIsIndirect: true);
            XS.Set(RAX, RSI, destinationDisplacement: 8, destinationIsIndirect: true);
            XS.Set(RAX, RCX);
            XS.Push(0);
            XS.Call(xCtorName);
            XS.Push(0);
        }
    }
}
