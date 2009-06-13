using System;
using System.Collections.Generic;
using System.Linq;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Compiler;

namespace Indy.IL2CPU.IL.X86 {
    [OpCode(OpCodeEnum.Callvirt)]
    public class Callvirt : Op {
        private string mMethodIdentifier;
        private string mNormalAddress;
        private string mMethodDescription;
        private int mThisOffset;
        private uint mArgumentCount;
        private uint mReturnSize;
        private string mLabelName;
        private MethodInformation mCurrentMethodInfo;
        private MethodInformation mTargetMethodInfo;
        private uint mCurrentILOffset;
        private int mExtraStackSpace;
        private MethodBase mMethod;

        public static void ScanOp(ILReader aReader,
                                  MethodInformation aMethodInfo,
                                  SortedList<string, object> aMethodData,
            IServiceProvider aServiceProvider)
        {
            MethodBase xMethod = aReader.OperandValueMethod;
            if (xMethod == null)
            {
                throw new Exception("Unable to determine Method!");
            }
            MethodBase xMethodDef = xMethod;
            var xTargetMethodInfo = aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(xMethodDef,
                                                                                                      false);
            foreach (var xParam in xMethodDef.GetParameters())
            {
                aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(xParam.ParameterType);
            }
            aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(xTargetMethodInfo.ReturnType);
            aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(xMethodDef, false);
            aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(VTablesImplRefs.GetMethodAddressForTypeRef, false);
            aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(typeof(NullReferenceException).GetConstructor(new Type[0]), false);
            aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(
                CPU.Assembler.CurrentExceptionOccurredRef, false);
            aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(typeof (NullReferenceException));
        }

        public Callvirt(ILReader aReader,
                        MethodInformation aMethodInfo)
            : base(aReader,
                   aMethodInfo) {
            mLabelName = GetInstructionLabel(aReader);
            mCurrentMethodInfo = aMethodInfo;
            mMethod = aReader.OperandValueMethod;
            if (mMethod == null) {
                throw new Exception("Unable to determine Method!");
            }
            mCurrentILOffset = aReader.Position;
        }

        public override void DoAssemble() {
            mMethodDescription = CPU.MethodInfoLabelGenerator.GenerateLabelName(mMethod);
            mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(mMethod,
                                                     mMethod,
                                                     mMethodDescription,
                                                     null,
                                                     mCurrentMethodInfo.DebugMode);
            if (mMethod.IsStatic || !mMethod.IsVirtual || mMethod.IsFinal)
            {
                mNormalAddress = CPU.MethodInfoLabelGenerator.GenerateLabelName(mMethod);
            }
            mMethodIdentifier = GetService<IMetaDataInfoService>().GetMethodIdLabel(mMethod);
            mArgumentCount = (uint)mTargetMethodInfo.Arguments.Length;
            mReturnSize = mTargetMethodInfo.ReturnSize;
            mThisOffset = mTargetMethodInfo.Arguments[0].Offset;
            if (mTargetMethodInfo.ExtraStackSize > 0)
            {
                mThisOffset -= mTargetMethodInfo.ExtraStackSize;
            }
            mExtraStackSpace = mTargetMethodInfo.ExtraStackSize;
            new Comment("ThisOffset = " + mThisOffset);
            Action xEmitCleanup = delegate() {
                                      foreach (MethodInformation.Argument xArg in mTargetMethodInfo.Arguments) {
                                          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = xArg.Size };
                                      }
                                  };
            if (!String.IsNullOrEmpty(mNormalAddress)) {
                EmitCompareWithNull(Assembler,
                                    mCurrentMethodInfo,
                                    delegate(CPUx86.Compare c){
                                        c.DestinationReg = CPUx86.Registers.ESP;
                                        c.DestinationIsIndirect = true;
                                        c.DestinationDisplacement = mThisOffset;
                                    },
                                    mLabelName,
                                    mLabelName + "_AfterNullRefCheck",
                                    xEmitCleanup,
                                    (int)mCurrentILOffset,
                                    GetService<IMetaDataInfoService>().GetTypeIdLabel(typeof(NullReferenceException)),
                                    GetService<IMetaDataInfoService>().GetTypeInfo(typeof(NullReferenceException)),
                                    GetService<IMetaDataInfoService>().GetMethodInfo(typeof(NullReferenceException).GetConstructor(Type.EmptyTypes), false),
                                    GetServiceProvider()
                                    );
                new CPU.Label(mLabelName + "_AfterNullRefCheck");
                if (mExtraStackSpace > 0) {
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mExtraStackSpace };
                }
                new CPUx86.Call { DestinationLabel = mNormalAddress };
            } else {
                /*
                 * On the stack now:
                 * $esp                 Params
                 * $esp + mThisOffset   This
                 */
                //Assembler.Add(new CPUx86.Pop("eax"));
                //Assembler.Add(new CPUx86.Pushd("eax"));
                EmitCompareWithNull(Assembler,
                                    mCurrentMethodInfo,
                                    delegate(CPUx86.Compare c) {
                                        c.DestinationReg = CPUx86.Registers.ESP;
                                        c.DestinationIsIndirect = true;
                                        c.DestinationDisplacement = mThisOffset;
                                    }, 
                                    mLabelName,
                                    mLabelName + "_AfterNullRefCheck",
                                    xEmitCleanup,
                                    (int)mCurrentILOffset,
                                    GetService<IMetaDataInfoService>().GetTypeIdLabel(typeof(NullReferenceException)),
                                    GetService<IMetaDataInfoService>().GetTypeInfo(typeof(NullReferenceException)),
                                    GetService<IMetaDataInfoService>().GetMethodInfo(typeof(NullReferenceException).GetConstructor(Type.EmptyTypes), false),
                                    GetServiceProvider());
                new CPU.Label(mLabelName + "_AfterNullRefCheck");
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationRef = ElementReference.New(mMethodIdentifier), DestinationIsIndirect = true };
                var xGetMethodAddrInfo =
                    GetService<IMetaDataInfoService>().GetMethodInfo(VTablesImplRefs.GetMethodAddressForTypeRef, false);
                new CPUx86.Call { DestinationLabel = xGetMethodAddrInfo.LabelName};

                /*
                 * On the stack now:
                 * $esp                 Params
                 * $esp + mThisOffset   This
                 * 
                 * EAX contains the method to call
                 */
                Call.EmitExceptionLogic(Assembler,
                                        mCurrentILOffset,
                                        mCurrentMethodInfo,
                                        mLabelName + "_AfterAddressCheck",
                                        true,
                                        xEmitCleanup);
                new CPU.Label(mLabelName + "_AfterAddressCheck");
                if (mTargetMethodInfo.Arguments[0].ArgumentType == typeof(object)) {
                    /*
                     * On the stack now:
                     * $esp                     method to call
                     * $esp + 4                 Params
                     * $esp + mThisOffset + 4   This
                     */
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset + 4 };
                    new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = ((uint)InstanceTypeEnum.BoxedValueType), Size = 32 };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * EAX contains the method to call
                     */
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = mLabelName + "_NOT_BOXED_THIS" };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * ECX contains the method to call
                     */
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * ECX contains the method to call
                     * EAX contains $This, but boxed
                     */
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)ObjectImpl.FieldDataOffset };
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = mThisOffset, SourceReg = CPUx86.Registers.EAX };
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
                new CPU.Label(mLabelName + "_NOT_BOXED_THIS");
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                if (mExtraStackSpace > 0) {
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mExtraStackSpace };
                }
                new CPUx86.Call { DestinationReg = CPUx86.Registers.EAX };
                new CPU.Label(mLabelName + "__AFTER_NOT_BOXED_THIS");
            }
            Call.EmitExceptionLogic(Assembler,
                               mCurrentILOffset,
                               mCurrentMethodInfo,
                               mLabelName + "__NO_EXCEPTION_AFTER_CALL",
                               true,
                               delegate()
                               {
                                   var xResultSize = mTargetMethodInfo.ReturnSize;
                                   if (xResultSize % 4 != 0)
                                   {
                                       xResultSize += 4 - (xResultSize % 4);
                                   }
                                   for (int i = 0; i < xResultSize / 4; i++)
                                   {
                                       new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                                   }
                               });

            new CPU.Label(mLabelName + "__NO_EXCEPTION_AFTER_CALL");
            new CPU.Comment("Argument Count = " + mArgumentCount.ToString());
            for (int i = 0; i < mArgumentCount; i++) {
                Assembler.StackContents.Pop();
            }
            if (mReturnSize > 0) {
                Assembler.StackContents.Push(new StackContent((int)mReturnSize));
            }
        }
    }
}
