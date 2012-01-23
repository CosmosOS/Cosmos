using System;
using System.Collections.Generic;
using System.IO;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using System.Reflection;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;


namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Localloc )]
    public class Localloc : ILOp
    {
        public Localloc( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            //var xId = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef);
            //new CPUx86.Call { DestinationLabel = xId };
            //new CPUx86.Move
            //{
            //    DestinationReg = CPUx86.Registers.EBP,
            //    DestinationIsIndirect = true,
            //    //DestinationDisplacement = mLocallocOffset,
            //    SourceReg = CPUx86.Registers.ESP,
            //    SourceIsIndirect = true
            //};
            throw new NotImplementedException("Localloc is not yet implemented!");
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.Compiler;
        // using CPU = Cosmos.Assembler.x86;
        // using CPUx86 = Cosmos.Assembler.x86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Localloc)]
        // 	public class Localloc: Op {
        //         public const string LocAllocCountMethodDataEntry = "LocAllocCount";
        //         public const string LocAllicItemMethodDataEntryTemplate = "LocAllocItem_L{0}";
        // 
        //         public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aServiceProvider) {
        //             // xCurrentMethodLocallocCount contains the number of LocAlloc occurrences
        //             int xCurrentMethodLocallocCount = 0;
        //             if (aMethodData.ContainsKey(LocAllocCountMethodDataEntry))
        //             {
        //                 xCurrentMethodLocallocCount = (int)aMethodData[LocAllocCountMethodDataEntry];
        //             }
        //             xCurrentMethodLocallocCount++;
        //             aMethodData[LocAllocCountMethodDataEntry] = xCurrentMethodLocallocCount;
        //             string xCurrentItem = String.Format(LocAllicItemMethodDataEntryTemplate,
        //                                                 aReader.Position);
        // #if DEBUG
        //             if (aMethodData.ContainsKey(xCurrentItem))
        //             {
        //                 throw new Exception("Localloc item already exists in MethodData!");
        //             }
        // #endif
        //             aMethodData.Add(xCurrentItem, xCurrentMethodLocallocCount);
        //         }
        // 
        // 	    private readonly int mLocallocOffset = 0;
        // 		public Localloc(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		    mLocallocOffset = (int)aMethodInfo.MethodData[String.Format(LocAllicItemMethodDataEntryTemplate,
        // 		                                                                aReader.Position)];
        // 		    mLocallocOffset *= 4;
        // 		    mLocallocOffset += aMethodInfo.LocalsSize;
        // 
        // 		}
        //         public override void DoAssemble() {
        //             var xId = GetService<IMetaDataInfoService>().GetMethodInfo(RuntimeEngineRefs.Heap_AllocNewObjectRef, false);
        //             new CPUx86.Call { DestinationLabel = xId.LabelName };
        //             new CPUx86.Move {
        //                 DestinationReg = CPUx86.Registers.EBP,
        //                 DestinationIsIndirect = true,
        //                 DestinationDisplacement = mLocallocOffset,
        //                 SourceReg = Registers.ESP,
        //                 SourceIsIndirect = true
        //             };
        //         }
        // 	}
        // }

    }
}
