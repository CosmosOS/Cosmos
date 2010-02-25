using System;
using System.Linq;
using CPUx86 = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldflda )]
    public class Ldflda : ILOp
    {
        public Ldflda( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
          var xOpCode = (ILOpCodes.OpField)aOpCode;
          DoExecute(Assembler, aMethod, xOpCode.Value.DeclaringType, xOpCode.Value.GetFullName(), true);
        }

        public static void DoExecute(Assembler Assembler, MethodInfo aMethod, Type aDeclaringType, string aField, bool aDerefValue) {

          var xFields = GetFieldsInfo(aDeclaringType);
          var xFieldInfo = (from item in xFields
                            where item.Id == aField
                            select item).Single();

          int xExtraOffset = 0;
          var xType = aMethod.MethodBase.DeclaringType;
          bool xNeedsGC = aDeclaringType.IsClass && !aDeclaringType.IsValueType;

          if (xNeedsGC) {
            xExtraOffset = 12;
          }
          uint xOffset = 0;

          var xActualOffset = xFieldInfo.Offset + xExtraOffset;
          var xSize = xFieldInfo.Size;

          new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };

          new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)(xActualOffset) };
          Assembler.Stack.Pop();
          Assembler.Stack.Push(new StackContents.Item(4, xType));
          if(aDerefValue && xFieldInfo.IsExternalValue )
          {
              new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
          }
          else
          {
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
          }
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        // 
        // 
        // using CPUx86 = Cosmos.IL2CPU.X86;	    
        // using System.Reflection;
        // using Cosmos.IL2CPU.Compiler;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldflda)]
        // 	public class Ldflda: Op {
        //         private Type mType;
        // 		private TypeInformation mTypeInfo;
        // 		private TypeInformation.Field mField;
        //         private string mFieldId;
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
        //         //{
        //         //    FieldInfo xField = aReader.OperandValueField;
        //         //    if (xField == null)
        //         //    {
        //         //        throw new Exception("Field not found!");
        //         //    }
        //         //    Engine.RegisterType(xField.DeclaringType);
        //         //    Engine.RegisterType(xField.FieldType);
        //         //}
        // 
        // 		public Ldflda(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			FieldInfo xField = aReader.OperandValueField;
        // 			if (xField == null) {
        // 					throw new Exception("Field not found!");
        // 			}
        // 			mFieldId = xField.GetFullName();
        //             mType = xField.DeclaringType;
        // 		}
        // 
        // 		public override void DoAssemble() {
        //             mTypeInfo = GetService<IMetaDataInfoService>().GetTypeInfo(mType);
        //             mField = mTypeInfo.Fields[mFieldId];
        // 			Ldflda(Assembler, mTypeInfo, mField);
        // 		}
        // 	}
        // }

    }
}
