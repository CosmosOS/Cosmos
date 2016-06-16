using System;
using System.Linq;
// using System.Collections.Generic;
// using System.IO;
//
// using CPU = Cosmos.Assembler.x86;
// using System.Reflection;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Finds the value of a field in the object whose reference is currently on the evaluation stack.
    /// </summary>
    /// <remarks>
    /// MSDN:
    /// The stack transitional behavior, in sequential order, is:
    /// 1. An object reference (or pointer) is pushed onto the stack.
    /// 2. The object reference (or pointer) is popped from the stack; the value of the specified field in the object is found.
    /// 3. The value stored in the field is pushed onto the stack.
    /// The ldfld instruction pushes the value of a field located in an object onto the stack.
    /// The object must be on the stack as an object reference (type O), a managed pointer (type &),
    /// an unmanaged pointer (type native int), a transient pointer (type *), or an instance of a value type.
    /// The use of an unmanaged pointer is not permitted in verifiable code.
    /// The object's field is specified by a metadata token that must refer to a field member.
    /// The return type is the same as the one associated with the field. The field may be either an instance field
    /// (in which case the object must not be a null reference) or a static field.
    ///
    /// The ldfld instruction can be preceded by either or both of the Unaligned and Volatile prefixes.
    ///
    /// NullReferenceException is thrown if the object is null and the field is not static.
    ///
    /// MissingFieldException is thrown if the specified field is not found in the metadata.
    ///
    /// This is typically checked when Microsoft Intermediate Language (MSIL) instructions are converted to native code, not at run time.
    /// </remarks>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldfld)]
    public class Ldfld : ILOp
    {
        public Ldfld(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xOpCode = (ILOpCodes.OpField)aOpCode;
            DoExecute(Assembler, xOpCode.Value.DeclaringType, xOpCode.Value.GetFullName(), true, DebugEnabled, aOpCode.StackPopTypes[0]);
        }

        public static int GetFieldOffset(Type aDeclaringType, string aFieldId)
        {
            int xExtraOffset = 0;
            var xFieldInfo = ResolveField(aDeclaringType, aFieldId, true);
            bool xNeedsGC = TypeNeedsGC(aDeclaringType);
            if (xNeedsGC)
            {
                xExtraOffset = 12;
            }
            return (int)(xExtraOffset + xFieldInfo.Offset);
        }

        public static bool TypeNeedsGC(Type aDeclaringType)
        {
            return aDeclaringType.IsClass && !aDeclaringType.IsValueType;
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, Type aDeclaringType, string xFieldId, bool aDerefExternalField, bool debugEnabled, Type aTypeOnStack)
        {
            var xOffset = GetFieldOffset(aDeclaringType, xFieldId);
            var xFields = GetFieldsInfo(aDeclaringType, false);
            var xFieldInfo = (from item in xFields
                              where item.Id == xFieldId
                              select item).Single();
            XS.Comment("Field: " + xFieldInfo.Id);
            XS.Comment("Type: " + xFieldInfo.FieldType.ToString());
            XS.Comment("Size: " + xFieldInfo.Size);
            XS.Comment("DeclaringType: " + aDeclaringType.FullName);
            XS.Comment("TypeOnStack: " + aTypeOnStack.FullName);
            XS.Comment("Offset: " + xOffset + " (includes object header)");

            if (aDeclaringType.IsValueType && aTypeOnStack == aDeclaringType)
            {
                // the full struct is on the stack, instead of only a pointer to it
                if (xFieldInfo.Size > 4)
                {
                    throw new Exception("For now, loading fields with sizes > 4 bytes from structs on the stack is not possible!");
                }

                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);

                switch (xFieldInfo.Size)
                {
                    case 1:
                        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.AL, SourceReg = CPUx86.RegistersEnum.ESP, SourceDisplacement = xOffset, SourceIsIndirect = true };
                        break;

                    case 2:
                        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.AX, SourceReg = CPUx86.RegistersEnum.ESP, SourceDisplacement = xOffset, SourceIsIndirect = true };
                        break;

                    case 3: //For Release
                        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.ESP, SourceDisplacement = xOffset, SourceIsIndirect = true };
                        XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 8);
                        break;

                    case 4:
                        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.ESP, SourceDisplacement = xOffset, SourceIsIndirect = true };
                        break;

                    default:
                        throw new Exception(string.Format("Field size {0} not supported!", xFieldInfo.Size));
                }

                // now remove the struct from the stack
                XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), Align(GetStorageSize(aDeclaringType), 4));

                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

                return;
            }
            DoNullReferenceCheck(Assembler, debugEnabled, 0);

            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));

            // pushed size is always 4 or 8
            var xSize = xFieldInfo.Size;
            if ((!aTypeOnStack.IsPointer) && (aDeclaringType.IsClass))
            {
                // convert to real memory address
                XS.Set(XSRegisters.ECX, XSRegisters.ECX, sourceIsIndirect: true);
            }
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), (uint)(xOffset));

            if (xFieldInfo.IsExternalValue && aDerefExternalField)
            {
                XS.Set(XSRegisters.ECX, XSRegisters.ECX, sourceIsIndirect: true);
            }

            for (int i = 1; i <= (xSize / 4); i++)
            {
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), sourceDisplacement: (int)(xSize - (i * 4)));
                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            }

            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);

            switch (xSize % 4)
            {
                case 1:
                    XS.Set(XSRegisters.AL, XSRegisters.ECX, sourceIsIndirect: true);
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    break;

                case 2:
                    XS.Set(XSRegisters.AX, XSRegisters.ECX, sourceIsIndirect: true);
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    break;

                case 3: //For Release
                    XS.Set(XSRegisters.EAX, XSRegisters.ECX, sourceIsIndirect: true);
                    XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 8);
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    break;

                case 0:
                    {
                        break;
                    }
                default:
                    throw new Exception(string.Format("Remainder size {0:D} {1:D} not supported!", xFieldInfo.FieldType.ToString(), xSize));
            }
        }
    }
}
