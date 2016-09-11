using System;
using System.Linq;
// using System.Collections.Generic;
// using System.Linq;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using System.Reflection;

using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Callvirt)]
    public class Callvirt : ILOp
    {
        public Callvirt(Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xOpMethod = aOpCode as OpMethod;
            DoExecute(Assembler, aMethod, xOpMethod.Value, xOpMethod.ValueUID, aOpCode, DebugEnabled);
        }

        public static void DoExecute(Assembler.Assembler Assembler, MethodInfo aMethod, MethodBase aTargetMethod, uint aTargetMethodUID, ILOpCode aOp, bool debugEnabled)
        {
            string xCurrentMethodLabel = GetLabel(aMethod, aOp.Position);
            Type xPopType = aOp.StackPopTypes.Last();

            string xNormalAddress = "";
            if (aTargetMethod.IsStatic || !aTargetMethod.IsVirtual || aTargetMethod.IsFinal)
            {
                xNormalAddress = LabelName.Get(aTargetMethod);
            }

            uint xReturnSize = 0;
            var xMethodInfo = aTargetMethod as SysReflection.MethodInfo;
            if (xMethodInfo != null)
            {
                xReturnSize = Align(SizeOfType(xMethodInfo.ReturnType), 4);
            }

            uint xExtraStackSize = Call.GetStackSizeToReservate(aTargetMethod, xPopType);
            uint xThisOffset = 0;
            var xParameters = aTargetMethod.GetParameters();
            foreach (var xItem in xParameters)
            {
                xThisOffset += Align(SizeOfType(xItem.ParameterType), 4);
            }

            // This is finding offset to self? It looks like we dont need offsets of other
            // arguments, but only self. If so can calculate without calculating all fields
            // Might have to go to old data structure for the offset...
            // Can we add this method info somehow to the data passed in?
            // mThisOffset = mTargetMethodInfo.Arguments[0].Offset;

            XS.Comment("ThisOffset = " + xThisOffset);

            if (TypeIsReferenceType(xPopType))
            {
                DoNullReferenceCheck(Assembler, debugEnabled, (int)xThisOffset + 4);
            }
            else
            {
                DoNullReferenceCheck(Assembler, debugEnabled, (int)xThisOffset);
            }

            if (!String.IsNullOrEmpty(xNormalAddress))
            {
                if (xExtraStackSize > 0)
                {
                    XS.Sub(ESP, xExtraStackSize);
                }
                XS.Call(xNormalAddress);
            }
            else
            {
                /*
                * On the stack now:
                * $esp                 Params
                * $esp + mThisOffset   This
                */
                if ((xPopType.IsPointer) || (xPopType.IsByRef))
                {
                    xPopType = xPopType.GetElementType();
                    string xTypeId = GetTypeIDLabel(xPopType);
                    XS.Push(xTypeId, isIndirect: true);
                }
                else
                {
                    XS.Set(EAX, ESP, sourceDisplacement: (int)xThisOffset + 4);
                    XS.Push(EAX, isIndirect: true);
                }
                XS.Push(aTargetMethodUID);
                XS.Call(LabelName.Get(VTablesImplRefs.GetMethodAddressForTypeRef));
                if (xExtraStackSize > 0)
                {
                    xThisOffset -= xExtraStackSize;
                }

                /*
                 * On the stack now:
                 * $esp                 Params
                 * $esp + mThisOffset   This
                 */
                XS.Pop(ECX);

                XS.Label(xCurrentMethodLabel + ".AfterAddressCheck");

                if (TypeIsReferenceType(xPopType))
                {
                    /*
                    * On the stack now:
                    * $esp + 0              Params
                    * $esp + mThisOffset    This
                    */
                    // we need to see if $this is a boxed object, and if so, we need to box it
                    XS.Set(EAX, ESP, sourceDisplacement: (int)xThisOffset + 4);
                    XS.Compare(EAX, (int)InstanceTypeEnum.BoxedValueType, destinationIsIndirect: true, destinationDisplacement: 4, size: RegisterSize.Int32);

                    /*
                    * On the stack now:
                    * $esp                 Params
                    * $esp + mThisOffset   This
                    *
                    * ECX contains the method to call
                    * EAX contains the type pointer (not the handle!!)
                    */
                    XS.Jump(CPU.ConditionalTestEnum.NotEqual, xCurrentMethodLabel + ".NotBoxedThis");

                    /*
                    * On the stack now:
                    * $esp                 Params
                    * $esp + mThisOffset   This
                    *
                    * ECX contains the method to call
                    * EAX contains the type pointer (not the handle!!)
                    */
                    XS.Add(EAX, (uint)ObjectImpl.FieldDataOffset);
                    XS.Set(ESP, EAX, destinationDisplacement: (int)xThisOffset + 4);

                    /*
                    * On the stack now:
                    * $esp                 Params
                    * $esp + mThisOffset   Pointer to address inside box
                    *
                    * ECX contains the method to call
                    */
                }
                XS.Label(xCurrentMethodLabel + ".NotBoxedThis");
                if (xExtraStackSize > 0)
                {
                    XS.Sub(ESP, xExtraStackSize);
                }
                XS.Call(ECX);
                XS.Label(xCurrentMethodLabel + ".AfterNotBoxedThis");
            }
            EmitExceptionLogic(Assembler, aMethod, aOp, true,
                delegate
                {
                    var xStackOffsetBefore = aOp.StackOffsetBeforeExecution.Value;

                    uint xPopSize = 0;
                    foreach (var type in aOp.StackPopTypes)
                    {
                        xPopSize += Align(SizeOfType(type), 4);
                    }

                    var xResultSize = xReturnSize;
                    if (xResultSize % 4 != 0)
                    {
                        xResultSize += 4 - (xResultSize % 4);
                    }

                    EmitExceptionCleanupAfterCall(Assembler, xResultSize, xStackOffsetBefore, xPopSize);
                });
            XS.Label(xCurrentMethodLabel + ".NoExceptionAfterCall");
            XS.Comment("Argument Count = " + xParameters.Length);
        }
    }
}
