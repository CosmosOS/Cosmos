using System;
using System.Linq;

using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stfld )]
    public class Stfld : ILOp
    {
        public Stfld( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
          var xOpCode = (ILOpCodes.OpField)aOpCode;
          var xField = xOpCode.Value;
          DoExecute(Assembler, aMethod, xField);
        }

        public static void DoExecute(Assembler aAssembler, MethodInfo aMethod, string aFieldId, Type aDeclaringObject, bool aNeedsGC) {
          
          var xType = aMethod.MethodBase.DeclaringType;

          int xExtraOffset = 0;
          if (aNeedsGC) {
            xExtraOffset = 12;
          }

          if (aFieldId == "MatthijsTest.Program+TEst+MyStruct MatthijsTest.Program+TEst.mStruct")
          {
              Console.Write("");
          }
          var xFields = GetFieldsInfo(aDeclaringObject);
          var xFieldInfo = (from item in xFields
                            where item.Id == aFieldId
                            select item).Single();
          var xActualOffset = xFieldInfo.Offset + xExtraOffset;
          var xSize = xFieldInfo.Size;
          new Comment("Field: " + xFieldInfo.Id);
          new Comment("Type: " + xFieldInfo.FieldType.ToString());
          new Comment("Size: " + xFieldInfo.Size);

          aAssembler.Stack.Pop();

          uint xRoundedSize = Align(xSize, 4);

#if! SKIP_GC_CODE
          if (aNeedsGC) {
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4 };
            //Ldfld(aAssembler, aType, aField, false);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)(xActualOffset) };
            new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
          }
#endif
          new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (int)xRoundedSize };
          new CPUx86.Add {
            DestinationReg = CPUx86.Registers.ECX,
            SourceValue = (uint)(xActualOffset)
          };
          for (int i = 0; i < (xSize / 4); i++) {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)((i * 4)), SourceReg = CPUx86.Registers.EAX };
          }
          switch (xSize % 4) {
            case 1: {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.AL };
                break;
              }
            case 2: {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.AX };
                break;
              }

            case 3: //TODO 
              throw new NotImplementedException();
              break;
            case 0: {
                break;
              }
            default:
              throw new Exception("Remainder size " + (xSize % 4) + " not supported!");
          }
#if! SKIP_GC_CODE
          if (aNeedsGC) {
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
            new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
          }
#endif
          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
          aAssembler.Stack.Pop();
        }

        public static void DoExecute(Assembler aAssembler, MethodInfo aMethod, System.Reflection.FieldInfo aField )
        {

          bool xNeedsGC = aField.DeclaringType.IsClass && !aField.DeclaringType.IsValueType;

          DoExecute(aAssembler, aMethod, aField.GetFullName(), aField.DeclaringType, xNeedsGC);
         
        }


        // using System;
        // using System.Collections;
        // using System.Collections.Generic;		 
        // using System.Linq;
        // 
        // 
        // using CPU = Cosmos.Compiler.Assembler.X86;
        // using System.Reflection;
        // using Cosmos.IL2CPU.Compiler;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Stfld)]
        // 	public class Stfld: Op {
        // 		private TypeInformation.Field mField;
        // 		private TypeInformation mType;
        //         private Type mDeclaringType;
        // 
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //         //    FieldInfo xField = aReader.OperandValueField;
        //         //    if (xField == null)
        //         //    {
        //         //        throw new Exception("Field not found!");
        //         //    }
        //         //    Engine.RegisterType(xField.FieldType);
        //         //}
        // 
        // 		public Stfld(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			if (aReader == null) {
        // 				throw new ArgumentNullException("aReader");
        // 			}
        // 			if (aMethodInfo == null) {
        // 				throw new ArgumentNullException("aMethodInfo");
        // 			}
        // 			FieldInfo xField = aReader.OperandValueField;
        // 			if (xField == null) {
        // 				throw new Exception("Field not found!");
        // 			}
        // 			mFieldId = xField.GetFullName();
        //             mDeclaringType = xField.DeclaringType;
        // 			
        // 		}
        // 
        //         private string mFieldId;
        // 
        // 		public override void DoAssemble() {
        //             mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //             if (!mType.Fields.ContainsKey(mFieldId))
        //             {
        //                 Console.Write("");
        //                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //                 throw new Exception("Field not found!");
        //             }
        //             mField = mType.Fields[mFieldId];
        // 			Stfld(Assembler, mType, mField);
        // 		}
        // 	}
        // }

    }
}
