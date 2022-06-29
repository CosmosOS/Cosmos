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

            XS.Set(ECX, EBP, sourceDisplacement: 8); // size
            XS.Set(EDX, EBP, sourceDisplacement: 12); // length



            XS.Push(ECX);  // size of element
            XS.Set(EAX, ECX);
            XS.Multiply(EDX); // total element size
            XS.Add(EAX, ObjectUtils.FieldDataOffset + 4); // total array size
            XS.Push(EAX);
            XS.Call(LabelName.Get(GCImplementationRefs.AllocNewObjectRef));
            XS.Label(".AfterAlloc");
            XS.Pop(EAX);
            XS.Pop(ESI);
            XS.Push(EAX);
            XS.Push(ESP, isIndirect: true);
            XS.Push(ESP, isIndirect: true);
            // it's on the stack 3 times now, once from the return value, twice from the pushes;

            XS.Pop(EAX);
            XS.Set(EBX, xTypeID, sourceIsIndirect: true);  // array type id
            XS.Set(EAX, EBX, destinationIsIndirect: true); // array type id
            XS.Set(EAX, (uint)ObjectUtils.InstanceTypeEnum.Array, destinationDisplacement: 4, destinationIsIndirect: true);
            XS.Set(EAX, ESI, destinationDisplacement: 8, destinationIsIndirect: true);
            XS.Set(EAX, ECX);
            XS.Push(0);
            XS.Call(xCtorName);
            XS.Push(0);
        }
    }
}
