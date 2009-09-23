using System;
using System.Linq;
// using System.Collections.Generic;
// using System.IO;
// 
// using CPU = Cosmos.IL2CPU.X86;
// using System.Reflection;
using Indy.IL2CPU;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.IL2CPU.X86;
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
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldfld )]
    public class Ldfld : ILOp
    {
        public Ldfld( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
          if (GetLabel(aMethod, aOpCode) == "System_Void__System_Collections_Generic_List_1___System_IO_FileInfo__Add_System_IO_FileInfo___DOT__00000026") {
            Console.Write("");
          }
          var xOpCode = (ILOpCodes.OpField)aOpCode;
          NewMethod(Assembler, xOpCode.Value);
        }

        public static void NewMethod(Assembler Assembler, System.Reflection.FieldInfo xField) {
          Assembler.Stack.Pop();
          int xExtraOffset = 0;
          bool xNeedsGC = xField.DeclaringType.IsClass && !xField.DeclaringType.IsValueType;
          if (xNeedsGC) {
            xExtraOffset = 12;
          }
          new Comment(Assembler, "Type = '" + xField.FieldType.FullName + "', NeedsGC = " + xNeedsGC);
          new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };

          var xFields = GetFieldsInfo(xField.DeclaringType);
          var xFieldInfo = (from item in xFields
                            where item.Id == xField.GetFullName()
                            select item).Single();

          var xActualOffset = xFieldInfo.Offset + xExtraOffset;
          var xSize = xFieldInfo.Size;

          new CPUx86.Add { DestinationReg = CPUx86.Registers.ECX, SourceValue = (uint)(xActualOffset) };

          //if( aField.IsExternalField/* && aDerefExternalField */)
          //{
          //    new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
          //}
          //*******

          for (int i = 1; i <= (xSize / 4); i++) {
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true, SourceDisplacement = (int)(xSize - (i * 4)) };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
          }
          switch (xSize % 4) {
            case 1: {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                break;
              }
            case 2: {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                break;
              }

            case 3: //For Release
                    {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EAX, SourceValue = 8 };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                break;
              }
            case 0: {
                      break;
                    }
            default:
              throw new Exception("Remainder size " + xField.FieldType.ToString() + (xSize) + " not supported!");
          }
          if (xNeedsGC) {
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
            new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef) };
          }
          Assembler.Stack.Push(new StackContents.Item((int)xSize, xField.FieldType));
        }

        // 	public class Ldfld: Op {
        //         private Type mDeclaringType;
        // 		private TypeInformation.Field mFieldInfo;
        // 		private readonly TypeInformation mTypeInfo;
        //         private string mFieldId;
        // 
        // 		public Ldfld(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			FieldInfo xField = aReader.OperandValueField;
        // 			if (xField == null) {
        // 					throw new Exception("Field not found!");
        // 			}
        // 			mFieldId = xField.GetFullName();
        //             mDeclaringType = xField.DeclaringType;
        // 		}

    }
}
