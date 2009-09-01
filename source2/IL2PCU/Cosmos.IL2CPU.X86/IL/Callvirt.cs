using System;
// using System.Collections.Generic;
// using System.Linq;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
// using System.Reflection;
// using Indy.IL2CPU.Assembler;
// using Indy.IL2CPU.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Callvirt )]
    public class Callvirt : ILOp
    {
        public Callvirt( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xMethodDescription = CPU.MethodInfoLabelGenerator.GenerateLabelName( aMethod.MethodBase );
            // mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(mMethod
            //   , mMethod, mMethodDescription, null, mCurrentMethodInfo.DebugMode);
            if( aMethod.MethodBase.IsStatic || !aMethod.MethodBase.IsVirtual || aMethod.MethodBase.IsFinal )
            {
                var xNormalAddress = CPU.MethodInfoLabelGenerator.GenerateLabelName( aMethod.MethodBase );
            }
            // mMethodIdentifier = GetService<IMetaDataInfoService>().GetMethodIdLabel(mMethod);
            var xMethodInfo = ( System.Reflection.MethodInfo )( ( ( Cosmos.IL2CPU.ILOpCodes.OpMethod )aOpCode ).Value );
            int xArgCount = xMethodInfo.GetParameters().Length;
            uint xReturnSize = SizeOfType( xMethodInfo.ReturnType );
            // Extracted from MethodInformation: Calculated offset
            //             var xRoundedSize = ReturnSize;
            //if (xRoundedSize % 4 > 0) {
            //    xRoundedSize += (4 - (ReturnSize % 4));
            //}
            
            

            //ExtraStackSize = (int)xRoundedSize;
            uint xExtraStackSize = ( uint )Align( xReturnSize, 4 );
            var xParameters = xMethodInfo.GetParameters(); 

            foreach (var xItem in xParameters) 
            {
                xExtraStackSize -= SizeOfType( xItem.GetType() );
            }

            //if (ExtraStackSize > 0) {
            //    for (int i = 0; i < Arguments.Length; i++) {
            //        Arguments[i].Offset += ExtraStackSize;
            //    }
            //}
             
             
            // This is finding offset to self? It looks like we dont need offsets of other
            // arguments, but only self. If so can calculate without calculating all fields
            // Might have to go to old data structure for the offset...
            // Can we add this method info somehow to the data passed in?
            // mThisOffset = mTargetMethodInfo.Arguments[0].Offset;

            //             mExtraStackSpace = mTargetMethodInfo.ExtraStackSize;
            //             if (mExtraStackSpace > 0) {
            //                 mThisOffset -= mExtraStackSpace;
            //             }
            //             new Comment("ThisOffset = " + mThisOffset);

            //             Action xEmitCleanup = delegate() {
            //                                       foreach (MethodInformation.Argument xArg in mTargetMethodInfo.Arguments) {
            //                                           new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = xArg.Size };
            //                                       }
            //                                   };
            //             if (!String.IsNullOrEmpty(mNormalAddress)) {
            //                 EmitCompareWithNull(Assembler,
            //                                     mCurrentMethodInfo,
            //                                     delegate(CPUx86.Compare c){
            //                                         c.DestinationReg = CPUx86.Registers.ESP;
            //                                         c.DestinationIsIndirect = true;
            //                                         c.DestinationDisplacement = mThisOffset;
            //                                     },
            //                                     mLabelName,
            //                                     mLabelName + "_AfterNullRefCheck",
            //                                     xEmitCleanup,
            //                                     (int)mCurrentILOffset,
            //                                     GetService<IMetaDataInfoService>().GetTypeIdLabel(typeof(NullReferenceException)),
            //                                     GetService<IMetaDataInfoService>().GetTypeInfo(typeof(NullReferenceException)),
            //                                     GetService<IMetaDataInfoService>().GetMethodInfo(typeof(NullReferenceException).GetConstructor(Type.EmptyTypes), false),
            //                                     GetServiceProvider()
            //                                     );
            //                 new CPU.Label(mLabelName + "_AfterNullRefCheck");
            //                 if (mExtraStackSpace > 0) {
            //                     new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mExtraStackSpace };
            //                 }
            //                 new CPUx86.Call { DestinationLabel = mNormalAddress };
            //             } else {
            //                 /*
            //                  * On the stack now:
            //                  * $esp                 Params
            //                  * $esp + mThisOffset   This
            //                  */
            //                 //Assembler.Add(new CPUx86.Pop("eax"));
            //                 //Assembler.Add(new CPUx86.Pushd("eax"));
            //                 EmitCompareWithNull(Assembler,
            //                                     mCurrentMethodInfo,
            //                                     delegate(CPUx86.Compare c) {
            //                                         c.DestinationReg = CPUx86.Registers.ESP;
            //                                         c.DestinationIsIndirect = true;
            //                                         c.DestinationDisplacement = mThisOffset;
            //                                     }, 
            //                                     mLabelName,
            //                                     mLabelName + "_AfterNullRefCheck",
            //                                     xEmitCleanup,
            //                                     (int)mCurrentILOffset,
            //                                     GetService<IMetaDataInfoService>().GetTypeIdLabel(typeof(NullReferenceException)),
            //                                     GetService<IMetaDataInfoService>().GetTypeInfo(typeof(NullReferenceException)),
            //                                     GetService<IMetaDataInfoService>().GetMethodInfo(typeof(NullReferenceException).GetConstructor(Type.EmptyTypes), false),
            //                                     GetServiceProvider());
            //                 new CPU.Label(mLabelName + "_AfterNullRefCheck");
            //                 new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset };
            //                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            //                 new CPUx86.Push { DestinationRef = ElementReference.New(mMethodIdentifier), DestinationIsIndirect = true };
            //                 var xGetMethodAddrInfo =
            //                     GetService<IMetaDataInfoService>().GetMethodInfo(VTablesImplRefs.GetMethodAddressForTypeRef, false);
            //                 new CPUx86.Call { DestinationLabel = xGetMethodAddrInfo.LabelName};
            // 
            //                 /*
            //                  * On the stack now:
            //                  * $esp                 Params
            //                  * $esp + mThisOffset   This
            //                  * 
            //                  * EAX contains the method to call
            //                  */
            //                 Call.EmitExceptionLogic(Assembler,
            //                                         mCurrentILOffset,
            //                                         mCurrentMethodInfo,
            //                                         mLabelName + "_AfterAddressCheck",
            //                                         true,
            //                                         xEmitCleanup);
            //                 new CPU.Label(mLabelName + "_AfterAddressCheck");
            //                 if (mTargetMethodInfo.Arguments[0].ArgumentType == typeof(object)) {
            //                     /*
            //                      * On the stack now:
            //                      * $esp                     method to call
            //                      * $esp + 4                 Params
            //                      * $esp + mThisOffset + 4   This
            //                      */
            //                     new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset + 4 };
            //                     new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = ((uint)InstanceTypeEnum.BoxedValueType), Size = 32 };
            //                     /*
            //                      * On the stack now:
            //                      * $esp                 Params
            //                      * $esp + mThisOffset   This
            //                      * 
            //                      * EAX contains the method to call
            //                      */
            //                     new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = mLabelName + "_NOT_BOXED_THIS" };
            //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            //                     /*
            //                      * On the stack now:
            //                      * $esp                 Params
            //                      * $esp + mThisOffset   This
            //                      * 
            //                      * ECX contains the method to call
            //                      */
            //                     new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset };
            //                     /*
            //                      * On the stack now:
            //                      * $esp                 Params
            //                      * $esp + mThisOffset   This
            //                      * 
            //                      * ECX contains the method to call
            //                      * EAX contains $This, but boxed
            //                      */
            //                     new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)ObjectImpl.FieldDataOffset };
            //                     new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = mThisOffset, SourceReg = CPUx86.Registers.EAX };
            //                     /*
            //                      * On the stack now:
            //                      * $esp                 Params
            //                      * $esp + mThisOffset   Pointer to address inside box
            //                      * 
            //                      * ECX contains the method to call
            //                      */
            //                     new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
            //                     /*
            //                      * On the stack now:
            //                      * $esp                    Method to call
            //                      * $esp + 4                Params
            //                      * $esp + mThisOffset + 4  This
            //                      */
            //                 }
            //                 new CPU.Label(mLabelName + "_NOT_BOXED_THIS");
            //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            //                 if (mExtraStackSpace > 0) {
            //                     new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mExtraStackSpace };
            //                 }
            //                 new CPUx86.Call { DestinationReg = CPUx86.Registers.EAX };
            //                 new CPU.Label(mLabelName + "__AFTER_NOT_BOXED_THIS");
            //             }
            //             Call.EmitExceptionLogic(Assembler,
            //                                mCurrentILOffset,
            //                                mCurrentMethodInfo,
            //                                mLabelName + "__NO_EXCEPTION_AFTER_CALL",
            //                                true,
            //                                delegate()
            //                                {
            //                                    var xResultSize = mTargetMethodInfo.ReturnSize;
            //                                    if (xResultSize % 4 != 0)
            //                                    {
            //                                        xResultSize += 4 - (xResultSize % 4);
            //                                    }
            //                                    for (int i = 0; i < xResultSize / 4; i++)
            //                                    {
            //                                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            //                                    }
            //                                });
            // 
            //             new CPU.Label(mLabelName + "__NO_EXCEPTION_AFTER_CALL");
            //             new CPU.Comment("Argument Count = " + mArgumentCount.ToString());
            //             for (int i = 0; i < mArgumentCount; i++) {
            //                 Assembler.Stack.Pop();
            //             }
            //             if (mReturnSize > 0) {
            //                 Assembler.Stack.Push(new StackContent((int)mReturnSize));
            //             }
            throw new NotImplementedException();
        }


        //     public class Callvirt : Op {
        //         private string mMethodIdentifier;
        //         private string mNormalAddress;
        //         private string mMethodDescription;
        //         private int mThisOffset;
        //         private uint mArgumentCount;
        //         private uint mReturnSize;
        //         private string mLabelName;
        //         private MethodInformation mCurrentMethodInfo;
        //         private MethodInformation mTargetMethodInfo;
        //         private uint mCurrentILOffset;
        //         private int mExtraStackSpace;
        //         private MethodBase mMethod;
        // 
        //         public static void ScanOp(ILReader aReader,
        //                                   MethodInformation aMethodInfo,
        //                                   SortedList<string, object> aMethodData,
        //             IServiceProvider aServiceProvider)
        //         {
        //             MethodBase xMethod = aReader.OperandValueMethod;
        //             if (xMethod == null)
        //             {
        //                 throw new Exception("Unable to determine Method!");
        //             }
        //             MethodBase xMethodDef = xMethod;
        //             var xTargetMethodInfo = aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(xMethodDef,
        //                                                                                                       false);
        //             foreach (var xParam in xMethodDef.GetParameters())
        //             {
        //                 aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(xParam.ParameterType);
        //             }
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(xTargetMethodInfo.ReturnType);
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(xMethodDef, false);
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(VTablesImplRefs.GetMethodAddressForTypeRef, false);
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(typeof(NullReferenceException).GetConstructor(new Type[0]), false);
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(
        //                 CPU.Assembler.CurrentExceptionOccurredRef, false);
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(typeof (NullReferenceException));
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetStaticFieldLabel(CPU.Assembler.CurrentExceptionRef);
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(
        //                     CPU.Assembler.CurrentExceptionOccurredRef, false);
        //         }
        // 
        //         public Callvirt(ILReader aReader,
        //                         MethodInformation aMethodInfo)
        //             : base(aReader,
        //                    aMethodInfo) {
        //             mLabelName = GetInstructionLabel(aReader);
        //             mCurrentMethodInfo = aMethodInfo;
        //             mMethod = aReader.OperandValueMethod;
        //             if (mMethod == null) {
        //                 throw new Exception("Unable to determine Method!");
        //             }
        //             mCurrentILOffset = aReader.Position;
        //         }
        //     }
        // }

    }
}
