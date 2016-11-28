using System;
using System.Linq;
using Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using System.Reflection;

using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Newobj)]
    public class Newobj : ILOp
    {
        public Newobj(Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            OpMethod xMethod = (OpMethod)aOpCode;
            string xCurrentLabel = GetLabel(aMethod, aOpCode);
            var xType = xMethod.Value.DeclaringType;

            Assemble(Assembler, aMethod, xMethod, xCurrentLabel, xType, xMethod.Value);
        }

        public static void Assemble(Assembler.Assembler aAssembler, MethodInfo aMethod, OpMethod xMethod, string currentLabel, Type objectType, MethodBase constructor)
        {
            // call cctor:
            if (aMethod != null)
            {
                var xCctor = (objectType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic) ?? new ConstructorInfo[0]).SingleOrDefault();
                if (xCctor != null)
                {
                    XS.Call(LabelName.Get(xCctor));
                    EmitExceptionLogic(aAssembler, aMethod, xMethod, true, null, ".AfterCCTorExceptionCheck");
                    XS.Label(".AfterCCTorExceptionCheck");
                }
            }

            if (objectType.IsValueType)
            {
                #region Valuetypes

                XS.Comment("ValueType");
                XS.Comment("Type: " + objectType);

                /*
                 * Current sitation on stack:
                 *   $ESP       Arg
                 *   $ESP+..    other items
                 *
                 * What should happen:
                 *  + The stack should be increased to allow space to contain:
                 *         + .ctor arguments
                 *         + struct _pointer_ (ref to start of emptied space)
                 *         + empty space for struct
                 *  + arguments should be copied to the new place
                 *  + old place where arguments were should be cleared
                 *  + pointer should be set
                 *  + call .ctor
                 */

                // Size of return value - we need to make room for this on the stack.
                uint xStorageSize = Align(SizeOfType(objectType), 4);
                XS.Comment("StorageSize: " + xStorageSize);
                if (xStorageSize == 0)
                {
                    throw new Exception("ValueType storage size cannot be 0.");
                }

                //var xStorageSize = aCtorDeclTypeInfo.StorageSize;

                uint xArgSize = 0;
                var xParameterList = constructor.GetParameters();
                foreach (var xParam in xParameterList)
                {
                    xArgSize = xArgSize + Align(SizeOfType(xParam.ParameterType), 4);
                }
                XS.Comment("ArgSize: " + xArgSize);

                // Set ESP so we can push the struct ptr
                int xShift = (int)(xArgSize - xStorageSize);
                XS.Comment("Shift: " + xShift);
                if (xShift < 0)
                {
                    XS.Sub(ESP, (uint)Math.Abs(xShift));
                }
                else if (xShift > 0)
                {
                    XS.Add(ESP, (uint)xShift);
                }

                // push struct ptr
                XS.Push(ESP);

                // Shift args
                foreach (var xParam in xParameterList)
                {
                    uint xArgSizeForThis = Align(SizeOfType(xParam.ParameterType), 4);
                    for (int i = 1; i <= xArgSizeForThis / 4; i++)
                    {
                        new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)xStorageSize };
                    }
                }

                new Call(aAssembler).Execute(aMethod, xMethod);

                // Need to put these *after* the call because the Call pops the args from the stack
                // and we have mucked about on the stack, so this makes it right before the next
                // op.

                #endregion Valuetypes
            }
            else
            {
                // If not ValueType, then we need gc

                var xParams = constructor.GetParameters();

                // array length + 8
                bool xHasCalcSize = false;

                #region Special string handling
                // try calculating size:
                if (constructor.DeclaringType == typeof(string))
                {
                    if (xParams.Length == 1 && xParams[0].ParameterType == typeof(char[]))
                    {
                        xHasCalcSize = true;
                        XS.Set(EAX, ESP, sourceDisplacement: 4, sourceIsIndirect: true); // address
                        XS.Set(EAX, EAX, sourceDisplacement: 8, sourceIsIndirect: true); // element count
                        XS.Set(EDX, 2); // element size
                        XS.Multiply(EDX);
                        XS.Push(EAX);
                    }
                    else if (xParams.Length == 3
                             && (xParams[0].ParameterType == typeof(char[]) || xParams[0].ParameterType == typeof(char*))
                             && xParams[1].ParameterType == typeof(int)
                             && xParams[2].ParameterType == typeof(int))
                    {
                        xHasCalcSize = true;
                        XS.Set(EAX, ESP, sourceIsIndirect: true);
                        XS.ShiftLeft(EAX, 1);
                        XS.Push(EAX);
                    }
                    else if (xParams.Length == 2
                             && xParams[0].ParameterType == typeof(char)
                             && xParams[1].ParameterType == typeof(int))
                    {
                        xHasCalcSize = true;
                        XS.Set(EAX, ESP, sourceIsIndirect: true);
                        XS.ShiftLeft(EAX, 1);
                        XS.Push(EAX);
                    }
                    else
                    {
                        throw new NotImplementedException("In NewObj, a string ctor implementation is missing!");
                    }
                }
                #endregion Special string handling

                uint xMemSize = GetStorageSize(objectType);
                int xExtraSize = 12; // additional size for set values after alloc
                XS.Push((uint)(xMemSize + xExtraSize));
                if (xHasCalcSize)
                {
                    XS.Pop(EAX);
                    XS.Add(ESP, EAX, destinationIsIndirect: true);
                }

                // todo: probably we want to check for exceptions after calling Alloc
                XS.Call(LabelName.Get(GCImplementationRefs.AllocNewObjectRef));
                XS.Label(".AfterAlloc");
                XS.Push(ESP, isIndirect: true);
                XS.Push(ESP, isIndirect: true);
                // it's on the stack now 3 times. Once from the Alloc return value, twice from the pushes

                // todo: use a cleaner approach here. this class shouldnt assemble the string
                string strTypeId = GetTypeIDLabel(constructor.DeclaringType);

                XS.Pop(EAX);
                XS.Set(EBX, strTypeId, sourceIsIndirect: true);
                XS.Set(EAX, EBX, destinationIsIndirect: true);
                XS.Set(EAX, (uint)InstanceTypeEnum.NormalObject, destinationDisplacement: 4, destinationIsIndirect: true, size: RegisterSize.Int32);
                XS.Set(EAX, xMemSize, destinationDisplacement: 8, destinationIsIndirect: true, size: RegisterSize.Int32);
                uint xSize = (uint)(from item in xParams
                                    let xQSize = Align(SizeOfType(item.ParameterType), 4)
                                    select (int)xQSize).Take(xParams.Length).Sum();
                XS.Push(0);

                foreach (var xParam in xParams)
                {
                    uint xParamSize = Align(SizeOfType(xParam.ParameterType), 4);
                    XS.Comment($"Arg {xParam.Name}: {xParamSize}");
                    for (int i = 0; i < xParamSize; i += 4)
                    {
                        XS.Push(ESP, isIndirect: true, displacement: (int)(xSize + 8));
                    }
                }


                XS.Call(LabelName.Get(constructor));
                // should the complete error handling happen by ILOp.EmitExceptionLogic?
                if (aMethod != null)
                {
                    // todo: only happening for real methods now, not for ctor's ?
                    XS.Test(ECX, 2);
                    string xNoErrorLabel = currentLabel + ".NoError" + LabelName.LabelCount.ToString();
                    XS.Jump(CPUx86.ConditionalTestEnum.Equal, xNoErrorLabel);

                    PushAlignedParameterSize(constructor);

                    // an exception occurred, we need to cleanup the stack, and jump to the exit
                    XS.Add(ESP, 4);

                    new Comment(aAssembler, "[ Newobj.Execute cleanup end ]");
                    Jump_Exception(aMethod);
                    XS.Label(xNoErrorLabel);
                }
                XS.Pop(EAX);

                PushAlignedParameterSize(constructor);

                XS.Push(EAX);
                XS.Push(0);
            }
        }

        private static void PushAlignedParameterSize(MethodBase aMethod)
        {
            ParameterInfo[] xParams = aMethod.GetParameters();

            uint xSize;
            XS.Comment("[ Newobj.PushAlignedParameterSize start count = " + xParams.Length.ToString() + " ]");
            for (int i = 0; i < xParams.Length; i++)
            {
                xSize = SizeOfType(xParams[i].ParameterType);
                XS.Add(ESP, Align(xSize, 4));
            }
            XS.Comment("[ Newobj.PushAlignedParameterSize end ]");
        }
    }
}
