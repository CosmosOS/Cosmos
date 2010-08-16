using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;
using System.Reflection;
using System.Linq;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldsflda )]
    public class Ldsflda : ILOp
    {
        public Ldsflda( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xOpCode = ( ILOpCodes.OpField )aOpCode;
            System.Reflection.FieldInfo xField = xOpCode.Value;
            // call cctor:
            var xCctor = (xField.DeclaringType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic) ?? new ConstructorInfo[0]).SingleOrDefault();
            if (xCctor != null)
            {
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(xCctor) };
                // todo: add exception support
            }
            string xDataName =DataMember.GetStaticFieldName(xField);
            new CPUx86.Push { DestinationRef = ElementReference.New( xDataName ) };
            Assembler.Stack.Push( new StackContents.Item( 4, true, false, false ) );
        }


        // using System;
        // using System.Collections.Generic;
        // using Cosmos.IL2CPU.Compiler;
        // using CPU = Cosmos.Compiler.Assembler.X86;
        // using System.Reflection;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldsflda)]
        // 	public class Ldsflda: Op {
        // 		private string mDataName;
        // 	    private FieldInfo mField;
        // 
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //         //    FieldInfo xField = aReader.OperandValueField;
        //         //    Engine.QueueStaticField(xField);
        //         //}
        // 
        // 		public Ldsflda(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mField = aReader.OperandValueField;
        //             
        // 		}
        // 
        // 		public override void DoAssemble() {
        // 		    mDataName = GetService<IMetaDataInfoService>().GetStaticFieldLabel(mField);
        //             new CPU.Push { DestinationRef = ElementReference.New(mDataName) };
        // 			Assembler.Stack.Push(new StackContent(4, true, false, false));
        // 		}
        // 	}
        // }

    }
}
