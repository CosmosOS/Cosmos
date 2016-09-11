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
using static XSharp.Compiler.XSRegisters;
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
            bool xNeedsGC = TypeIsReferenceType(aDeclaringType);
            if (xNeedsGC)
            {
                xExtraOffset = 12;
            }
            return (int)(xExtraOffset + xFieldInfo.Offset);
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
                #region Read struct value from stack

                // This is a 3-step process
                // 1. Move the actual value below the stack (negative to ESP)
                // 2. Move the value at the right spot of the stack (positive to stack)
                // 3. Adjust stack to remove the struct
                //
                // This is necessary, as the value could otherwise overwrite the struct too soon.

                var xTypeStorageSize = GetStorageSize(aDeclaringType);
                var xFieldStorageSize = xFieldInfo.Size;

                // Step 1, Move the actual value below the stack (negative to ESP)
                CopyValue(ESP, -(int)xFieldStorageSize, ESP, xOffset, xFieldStorageSize);

                // Step 2 Move the value at the right spot of the stack (positive to stack)
                var xStackOffset = (int)(Align(xTypeStorageSize, 4) - xFieldStorageSize);
                CopyValue(ESP, xStackOffset, ESP, -(int)xFieldStorageSize, xFieldStorageSize);

                // Step 3 Adjust stack to remove the struct
                XS.Add(ESP, Align((uint)(xStackOffset), 4));

                #endregion Read struct value from stack
                return;
            }

            // pushed size is always 4 or 8
            var xSize = xFieldInfo.Size;
            if (TypeIsReferenceType(aTypeOnStack))
            {
                DoNullReferenceCheck(Assembler, debugEnabled, 4);
                XS.Add(ESP, 4);
            }
            else
            {
                DoNullReferenceCheck(Assembler, debugEnabled, 0);
            }
            XS.Pop(ECX);

            XS.Add(ECX, (uint)(xOffset));

            if (xFieldInfo.IsExternalValue && aDerefExternalField)
            {
                XS.Set(ECX, ECX, sourceIsIndirect: true);
            }

            for (int i = 1; i <= (xSize / 4); i++)
            {
                XS.Set(EAX, ECX, sourceDisplacement: (int)(xSize - (i * 4)));
                XS.Push(EAX);
            }

            XS.Set(EAX, 0);

            switch (xSize % 4)
            {
                case 1:
                    XS.Set(AL, ECX, sourceIsIndirect: true);
                    XS.Push(EAX);
                    break;

                case 2:
                    XS.Set(AX, ECX, sourceIsIndirect: true);
                    XS.Push(EAX);
                    break;

                case 3: //For Release
                    XS.Set(EAX, ECX, sourceIsIndirect: true);
                    XS.ShiftRight(EAX, 8);
                    XS.Push(EAX);
                    break;

                case 0:
                    {
                        break;
                    }
                default:
                    throw new Exception(string.Format("Remainder size {0} {1:D} not supported!", xFieldInfo.FieldType.ToString(), xSize));
            }
        }
    }
}
