using System;
// using System.Collections.Generic;
// using System.Linq;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using System.Reflection;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Callvirt )]
    public class Callvirt : ILOp
    {
        public Callvirt( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
          var xOpMethod = aOpCode as OpMethod;
          DoExecute(Assembler, aMethod, xOpMethod.Value, xOpMethod.ValueUID, aOpCode, DebugEnabled);
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, MethodBase aTargetMethod, uint aTargetMethodUID, ILOpCode aOp, bool debugEnabled) {

          string xCurrentMethodLabel = GetLabel(aMethod, aOp.Position);

          // mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(mMethod
          //   , mMethod, mMethodDescription, null, mCurrentMethodInfo.DebugMode);
          string xNormalAddress = "";
          if (aTargetMethod.IsStatic || !aTargetMethod.IsVirtual || aTargetMethod.IsFinal) {
            xNormalAddress = LabelName.Get(aTargetMethod);
          }
          // mMethodIdentifier = GetService<IMetaDataInfoService>().GetMethodIdLabel(mMethod);

          int xArgCount = aTargetMethod.GetParameters().Length;
          uint xReturnSize = 0;
          var xMethodInfo = aTargetMethod as SysReflection.MethodInfo;
          if (xMethodInfo != null) {
            xReturnSize = Align(SizeOfType(xMethodInfo.ReturnType), 4);
          }
          // Extracted from MethodInformation: Calculated offset
          //             var xRoundedSize = ReturnSize;
          //if (xRoundedSize % 4 > 0) {
          //    xRoundedSize += (4 - (ReturnSize % 4));
          //}



          //ExtraStackSize = (int)xRoundedSize;
          uint xExtraStackSize = Call.GetStackSizeToReservate(aTargetMethod);
          uint xThisOffset = 0;
          var xParameters = aTargetMethod.GetParameters();
          foreach (var xItem in xParameters) {
            xThisOffset += Align(SizeOfType(xItem.ParameterType), 4);
          }

          // This is finding offset to self? It looks like we dont need offsets of other
          // arguments, but only self. If so can calculate without calculating all fields
          // Might have to go to old data structure for the offset...
          // Can we add this method info somehow to the data passed in?
          // mThisOffset = mTargetMethodInfo.Arguments[0].Offset;


          new Comment(Assembler, "ThisOffset = " + xThisOffset);
          Call.DoNullReferenceCheck(Assembler, debugEnabled, xThisOffset);

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

          new Label(xCurrentMethodLabel + ".AfterNullRefCheck");

          if (!String.IsNullOrEmpty(xNormalAddress)) {
            if (xExtraStackSize > 0) {
              new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xExtraStackSize };
            }
            new CPUx86.Call { DestinationLabel = xNormalAddress };
          } else {
            /*
             * On the stack now:
             * $esp                 Params
             * $esp + mThisOffset   This
             */

            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (int)xThisOffset };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            new CPUx86.Push { DestinationValue = aTargetMethodUID };
            new CPUx86.Call {
              DestinationLabel = LabelName.Get(VTablesImplRefs.GetMethodAddressForTypeRef)
            };
            if (xExtraStackSize > 0)
            {
                xThisOffset -= xExtraStackSize;
            }
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

            new Label(xCurrentMethodLabel + ".AfterAddressCheck");
            if (xMethodInfo.DeclaringType == typeof(object)) {
              /*
               * On the stack now:
               * $esp                     method to call
               * $esp + 4                 Params
               * $esp + mThisOffset + 4   This
               */
              // we need to see if $this is a boxed object, and if so, we need to box it
              new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (int)(xThisOffset + 4) };
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
              new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = xCurrentMethodLabel + ".NotBoxedThis" };
              new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
              /*
               * On the stack now:
               * $esp                 Params
               * $esp + mThisOffset   This
               *
               * ECX contains the method to call
               */
              new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (int)xThisOffset };
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
              new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)xThisOffset, SourceReg = CPUx86.Registers.EAX};
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
            new Label(xCurrentMethodLabel + ".NotBoxedThis");
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            if (xExtraStackSize > 0) {
              new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = xExtraStackSize };
            }
            new CPUx86.Call { DestinationReg = CPUx86.Registers.EAX };
            new Label(xCurrentMethodLabel + ".AfterNotBoxedThis");
          }
          ILOp.EmitExceptionLogic(Assembler, aMethod, aOp, true,
                                  delegate()
                                  {
                                      var xResultSize = xReturnSize;
                                      if (xResultSize % 4 != 0)
                                      {
                                          xResultSize += 4 - (xResultSize % 4);
                                      }
                                      for (int i = 0; i < xResultSize / 4; i++)
                                      {
                                          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                                      }
                                  });
          new Label(xCurrentMethodLabel + ".NoExceptionAfterCall");
          new Comment(Assembler, "Argument Count = " + xParameters.Length.ToString());
        }
    }
}
