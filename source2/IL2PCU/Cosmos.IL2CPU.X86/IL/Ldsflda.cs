using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldsflda )]
    public class Ldsflda : ILOp
    {
        public Ldsflda( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xOpCode = ( ILOpCodes.OpField )aOpCode;
            System.Reflection.FieldInfo xField = xOpCode.Value;
            string xDataName = "static_field__" + MethodInfoLabelGenerator.GetFullName( xField.DeclaringType ) + "." + xField.Name;
            new CPUx86.Push { DestinationRef = Indy.IL2CPU.Assembler.ElementReference.New( xDataName ) };
            Assembler.Stack.Push( new StackContents.Item( 4, true, false, false ) );
        }


        // using System;
        // using System.Collections.Generic;
        // using Indy.IL2CPU.Compiler;
        // using CPU = Indy.IL2CPU.Assembler.X86;
        // using System.Reflection;
        // using Indy.IL2CPU.Assembler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
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
