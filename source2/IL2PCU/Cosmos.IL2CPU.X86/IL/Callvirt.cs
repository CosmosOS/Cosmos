using System;
// using System.Collections.Generic;
// using System.Linq;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Cosmos.IL2CPU.ILOpCodes;
using Indy.IL2CPU;
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
          var xOpMethod = aOpCode as OpMethod;
          var xMethodInfo = (System.Reflection.MethodInfo)xOpMethod.Value;
            string xCurrentMethodLabel = CPU.MethodInfoLabelGenerator.GenerateLabelName( aMethod.MethodBase );

            // mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(mMethod
            //   , mMethod, mMethodDescription, null, mCurrentMethodInfo.DebugMode);
            string xNormalAddress = "";
            if( xMethodInfo.IsStatic || !xMethodInfo.IsVirtual || xMethodInfo.IsFinal )
            {
                xNormalAddress = CPU.MethodInfoLabelGenerator.GenerateLabelName( xMethodInfo );
            }
            // mMethodIdentifier = GetService<IMetaDataInfoService>().GetMethodIdLabel(mMethod);

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
            uint xThisOffset = 0;
            foreach (var xItem in xParameters) {
              xExtraStackSize -= SizeOfType(xItem.GetType());
              xThisOffset += SizeOfType(xItem.GetType());
            }

            // This is finding offset to self? It looks like we dont need offsets of other
            // arguments, but only self. If so can calculate without calculating all fields
            // Might have to go to old data structure for the offset...
            // Can we add this method info somehow to the data passed in?
            // mThisOffset = mTargetMethodInfo.Arguments[0].Offset;

            if (xExtraStackSize > 0) {
              xThisOffset -= xExtraStackSize;
                         }
            new Comment( "ThisOffset = " + xThisOffset );

            //             Action xEmitCleanup = delegate() {
            //                                       foreach (MethodInformation.Argument xArg in mTargetMethodInfo.Arguments) {
            //                                           new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = xArg.Size };
            //                                       }

            //                                   };

            //EmitCompareWithNull( Assembler,
            //                    mCurrentMethodInfo,
            //                    delegate( CPUx86.Compare c )
            //                    {
            //                        c.DestinationReg = CPUx86.Registers.ESP;
            //                        c.DestinationIsIndirect = true;
            //                        c.DestinationDisplacement = mThisOffset;
            //                    },
            //                    mLabelName,
            //                    mLabelName + "_AfterNullRefCheck",
            //                    xEmitCleanup,
            //                    ( int )mCurrentILOffset,
            //                    GetService<IMetaDataInfoService>().GetTypeIdLabel( typeof( NullReferenceException ) ),
            //                    GetService<IMetaDataInfoService>().GetTypeInfo( typeof( NullReferenceException ) ),
            //                    GetService<IMetaDataInfoService>().GetMethodInfo( typeof( NullReferenceException ).GetConstructor( Type.EmptyTypes ), false ),
            //                    GetServiceProvider() );
            // todo: add exception support
#warning todo: add exception support

            new CPU.Label( xCurrentMethodLabel + "_AfterNullRefCheck" );

            if( !String.IsNullOrEmpty( xNormalAddress ) )
            {
                if( xExtraStackSize > 0 )
                {
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = ( uint )xExtraStackSize };
                }
                new CPUx86.Call { DestinationLabel = xNormalAddress };
            }
            else
            {
                /*
                 * On the stack now:
                 * $esp                 Params
                 * $esp + mThisOffset   This
                 */

                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (int)xThisOffset };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                new CPUx86.Push {
                  DestinationValue = xOpMethod.ValueUID
                };
                new CPUx86.Call {
                  DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(VTablesImplRefs.GetMethodAddressForTypeRef)
                };

                /*
                 * On the stack now:
                 * $esp                 Params
                 * $esp + mThisOffset   This            
                 */

                //Call.EmitExceptionLogic( Assembler,
                //                        mCurrentILOffset,
                //                        mCurrentMethodInfo,
                //                        mLabelName + "_AfterAddressCheck",
                //                        true,
                //                        xEmitCleanup );

                new CPU.Label( xCurrentMethodLabel + "_AfterAddressCheck" );
                if( xMethodInfo.DeclaringType == typeof( object ) )
                {
                    /*
                     * On the stack now:
                     * $esp                     method to call
                     * $esp + 4                 Params
                     * $esp + mThisOffset + 4   This
                     */
                  // we need to see if $this is a boxed object, and if so, we need to box it
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (int)(xThisOffset + 4) };
                    //new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = ( ( uint )InstanceTypeEnum.BoxedValueType ), Size = 32 };
                    
                    //InstanceTypeEnum.BoxedValueType == 3 =>
                    new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = 3, Size = 32 };

                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * EAX contains the method to call
                     */
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = xCurrentMethodLabel + "_NOT_BOXED_THIS" };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * ECX contains the method to call
                     */
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = ( int )xThisOffset };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * ECX contains the method to call
                     * EAX contains $This, but boxed
                     */
                    
                    //new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = ( uint )ObjectImpl.FieldDataOffset };
                    //public const int FieldDataOffset = 12; // ObjectImpl says that. so..
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 12 };

                    new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = ( int )xThisOffset, SourceReg = CPUx86.Registers.EAX };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   Pointer to address inside box
                     * 
                     * ECX contains the method to call
                     */
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                    /*
                     * On the stack now:
                     * $esp                    Method to call
                     * $esp + 4                Params
                     * $esp + mThisOffset + 4  This
                     */
                }
                new CPU.Label( xCurrentMethodLabel + "_NOT_BOXED_THIS" );
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                if( xExtraStackSize > 0 )
                {
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = xExtraStackSize };
                }
                new CPUx86.Call { DestinationReg = CPUx86.Registers.EAX };
                new CPU.Label( xCurrentMethodLabel + "__AFTER_NOT_BOXED_THIS" );
            }
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
            new CPU.Label( xCurrentMethodLabel + "__NO_EXCEPTION_AFTER_CALL" );
            new CPU.Comment( "Argument Count = " + xParameters.Length.ToString() );
            for( int i = 0; i < xParameters.Length; i++ )
            {
                Assembler.Stack.Pop();
            }
            if( xReturnSize > 0 )
            {
                Assembler.Stack.Push( new StackContents.Item( (int)xReturnSize ) );
            }
            //throw new NotImplementedException();
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
