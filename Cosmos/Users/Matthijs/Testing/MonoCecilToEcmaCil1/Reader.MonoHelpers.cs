using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using EcmaCil.IL;
using Mono.Cecil.Cil;

namespace MonoCecilToEcmaCil1
{
    partial class Reader
    {
        private MethodDefinition ResolveMethod(MethodReference aRef)
        {
            var xDef = aRef as MethodDefinition;
            if (xDef != null)
            {
                return xDef;
            }
            var xSpec = aRef as GenericInstanceMethod;
            if (xSpec != null)
            {
                throw new Exception("Queueing generic methods not yet(?) supported!");
            }
            return aRef.Resolve();
        }

        private TypeDefinition ResolveType(TypeReference aRef)
        {
            var xDef = aRef as TypeDefinition;
            if (xDef != null)
            {
                return xDef;
            }
#if DEBUG
            var xArray = aRef as ArrayType;
            if (xArray != null)
            {
                throw new NotSupportedException("Reader.ResolveType doesnt support ArrayTypes");
            }
            var xPointer = aRef as PointerType;
            if (xPointer != null)
            {
                throw new NotSupportedException("Reader.ResolveType doesnt support PointerTypes");
            }
            //var xReference = aRef as ReferenceType;
            //if (xReference != null)
            //{
            //    throw new NotSupportedException("Reader.ResolveType doesnt support ReferenceTypes");
            //}
#endif
            return aRef.Resolve();
        }

        private static InstructionKindEnum GetInstructionKind(Code value)
        {
            var xName = value.ToString();
            if (xName.EndsWith("_S"))
            {
                xName = xName.Substring(0, xName.Length - 2);
            }
            return (InstructionKindEnum)Enum.Parse(typeof(InstructionKindEnum), xName);
        }
    }
}