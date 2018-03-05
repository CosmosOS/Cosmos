using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using UtilityClasses;
using Mono.Cecil.Cil;
using EcmaCil.IL;
using Mono.Cecil.Rocks;

namespace MonoCecilToEcmaCil1
{
    public partial class Reader : IDisposable
    {
        private Dictionary<QueuedMethod, EcmaCil.MethodMeta> mMethods = new Dictionary<QueuedMethod, EcmaCil.MethodMeta>();
        private Dictionary<QueuedType, EcmaCil.TypeMeta> mTypes = new Dictionary<QueuedType, EcmaCil.TypeMeta>();
        private Dictionary<QueuedArrayType, EcmaCil.ArrayTypeMeta> mArrayTypes = new Dictionary<QueuedArrayType, EcmaCil.ArrayTypeMeta>();
        private Dictionary<QueuedPointerType, EcmaCil.PointerTypeMeta> mPointerTypes = new Dictionary<QueuedPointerType, EcmaCil.PointerTypeMeta>();
        private Dictionary<QueuedMethod, EcmaCil.MethodMeta> mVirtuals = new Dictionary<QueuedMethod, EcmaCil.MethodMeta>();


        #region queueing system
        private Queue<object> mQueue = new Queue<object>();

        private EcmaCil.MethodMeta EnqueueMethod(MethodReference aMethod, object aSource, string aSourceType)
        {
            if (mLogEnabled)
            {
                LogMapPoint(aSource, aSourceType, aMethod);
            }
            List<TypeReference> xTypeArgs = null;
            var xGenSpec = aMethod as GenericInstanceMethod;
            TypeDefinition xTypeDef;
            MethodDefinition xMethodDef;
            TypeReference xRefType;
            if (xGenSpec != null)
            {
                xMethodDef = ResolveMethod(xGenSpec.ElementMethod);
                xRefType = xGenSpec.DeclaringType;
                xTypeArgs = new List<TypeReference>();
                foreach (TypeReference xArg in xGenSpec.GenericArguments)
                {
                    xTypeArgs.Add(xArg);
                }
            }
            else
            {
                xMethodDef = ResolveMethod(aMethod);
                xRefType = aMethod.DeclaringType;
            }
            #region resolve type
            xTypeDef = xRefType as TypeDefinition;
            if (xTypeDef == null)
            {
                var xGenType = xRefType as GenericInstanceType;
                if (xGenType != null)
                {
                    xTypeDef = ResolveType(xGenType.DeclaringType);
                    if (xTypeArgs == null)
                    {
                        xTypeArgs = new List<TypeReference>();
                    }
                    for (int i = 0; i < xGenType.GenericArguments.Count; i++)
                    {
                        xTypeArgs.Insert(i, xGenType.GenericArguments[i]);
                    }
                }
                else
                {
                    xTypeDef = ResolveType(xRefType);
                }
            }
            #endregion
            var xQueuedMethod = new QueuedMethod(xTypeDef, xMethodDef, (xTypeArgs == null ? null : xTypeArgs.ToArray()));
            EcmaCil.MethodMeta xMethodMeta;
            if(mMethods.TryGetValue(xQueuedMethod, out xMethodMeta)){
                return xMethodMeta;
            }
            var xDeclaringType = EnqueueType(xRefType, aMethod, "Declaring type");
            xMethodMeta = new EcmaCil.MethodMeta();
            xMethodMeta.DeclaringType = xDeclaringType;
            xDeclaringType.Methods.Add(xMethodMeta);
            mMethods.Add(xQueuedMethod, xMethodMeta);
            mQueue.Enqueue(xQueuedMethod);
            return xMethodMeta;
        }

        private EcmaCil.TypeMeta EnqueueType(TypeReference aTypeRef, object aSource, string aSourceType)
        {
            if (mLogEnabled)
            {
                LogMapPoint(aSource, aSourceType, aTypeRef);
            } 
            List<TypeReference> xTypeArgs = null;
            TypeDefinition xTypeDef;

            var xArrayType = aTypeRef as ArrayType;
            if (xArrayType != null)
            {
                var xQueuedArrayType = new QueuedArrayType(xArrayType);
                EcmaCil.ArrayTypeMeta xArrayMeta;
                if (mArrayTypes.TryGetValue(xQueuedArrayType, out xArrayMeta))
                {
                    return xArrayMeta;
                }
                var xElemMeta = EnqueueType(xQueuedArrayType.ArrayType.ElementType, aTypeRef, "Array element type");
                xArrayMeta = new EcmaCil.ArrayTypeMeta { ElementType = xElemMeta };
                mArrayTypes.Add(xQueuedArrayType, xArrayMeta);
                mQueue.Enqueue(xQueuedArrayType);
                return xArrayMeta;
            }
            var xPointerType = aTypeRef as PointerType;
            if (xPointerType != null)
            {
                var xQueuedPointerType = new QueuedPointerType(xPointerType);
                EcmaCil.PointerTypeMeta xPointerMeta;
                if (mPointerTypes.TryGetValue(xQueuedPointerType, out xPointerMeta))
                {
                    return xPointerMeta;
                }
                var xElemMeta = EnqueueType(xQueuedPointerType.PointerType.ElementType, aTypeRef, "Array element type");
                xPointerMeta = new EcmaCil.PointerTypeMeta { ElementType = xElemMeta };
                mPointerTypes.Add(xQueuedPointerType, xPointerMeta);
                mQueue.Enqueue(xQueuedPointerType);
                return xPointerMeta;
            }
            var xGenSpec = aTypeRef as GenericInstanceType;
            if (xGenSpec != null)
            {
                xTypeDef = ResolveType(xGenSpec.ElementType);
                xTypeArgs = new List<TypeReference>();
                foreach (TypeReference xArg in xGenSpec.GenericArguments)
                {
                    xTypeArgs.Add(xArg);
                }
            }
            else
            {
                xTypeDef = ResolveType(aTypeRef);
            }
            var xQueuedType = new QueuedType(xTypeDef, (xTypeArgs == null ? null : xTypeArgs.ToArray()));
            EcmaCil.TypeMeta xTypeMeta;
            if (mTypes.TryGetValue(xQueuedType, out xTypeMeta))
            {
                return xTypeMeta;
            }
            xTypeMeta = new EcmaCil.TypeMeta();
            if (xTypeDef.BaseType != null)
            {
                xTypeMeta.BaseType = EnqueueType(xTypeDef.BaseType, aTypeRef, "Base type");
            }
            mTypes.Add(xQueuedType, xTypeMeta);
            mQueue.Enqueue(xQueuedType);
            return xTypeMeta;
        }

        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            mMethods.Clear();
            mTypes.Clear();
            mArrayTypes.Clear();
            mPointerTypes.Clear();
        }

        public IEnumerable<EcmaCil.TypeMeta> Execute(string assembly)
        {
            var xAssemblyDef = AssemblyDefinition.ReadAssembly(assembly);
            if (xAssemblyDef.EntryPoint == null)
            {
                throw new ArgumentException("Main assembly should have entry point!");
            }
            if (xAssemblyDef.EntryPoint.GenericParameters.Count > 0 || xAssemblyDef.EntryPoint.DeclaringType.GenericParameters.Count > 0)
            {
                throw new ArgumentException("Main assembly's entry point has generic parameters. This is not supported!");
            }
            EnqueueMethod(xAssemblyDef.EntryPoint, null, "entry point");

            // handle queue
            while (mQueue.Count > 0)
            {
                var xItem = mQueue.Dequeue();
                if (xItem is QueuedMethod)
                {
                    var xMethod = (QueuedMethod)xItem;
                    ScanMethod(xMethod, mMethods[xMethod]);
                    continue;
                }
                if (xItem is QueuedArrayType)
                {
                    var xType = (QueuedArrayType)xItem;
                    ScanArrayType(xType, mArrayTypes[xType]);
                    continue;
                }
                if (xItem is QueuedPointerType)
                {
                    var xType = (QueuedPointerType)xItem;
                    ScanPointerType(xType, mPointerTypes[xType]);
                    continue;
                }
                if (xItem is QueuedType)
                {
                    var xType = (QueuedType)xItem;
                    ScanType(xType, mTypes[xType]);
                    continue;
                }
                throw new Exception("Queue item not supported: '" + xItem.GetType().FullName + "'!");
            }

            return mTypes.Values.Cast<EcmaCil.TypeMeta>()
                .Union(mArrayTypes.Values.Cast<EcmaCil.TypeMeta>())
                .Union(mPointerTypes.Values.Cast<EcmaCil.TypeMeta>());
        }

        private void ScanPointerType(QueuedPointerType xType, EcmaCil.PointerTypeMeta pointerTypeMeta)
        {
#if DEBUG
            pointerTypeMeta.Data[EcmaCil.DataIds.DebugMetaId] = "&" + pointerTypeMeta.ElementType.ToString();
#endif
        }

        private void ScanArrayType(QueuedArrayType aType, EcmaCil.ArrayTypeMeta arrayTypeMeta)
        {
            arrayTypeMeta.Dimensions = aType.ArrayType.Dimensions.Count;
// todo: fix?
            foreach (ArrayDimension xDimension in aType.ArrayType.Dimensions)
            {
                if (xDimension.LowerBound != 0 || xDimension.UpperBound != 0)
                {
                    throw new Exception("Arrays with limited dimensions not supported");
                }
            }


#if DEBUG
            var xSB = new StringBuilder();
            xSB.Append(arrayTypeMeta.ElementType.ToString());
            xSB.Append("[");
            xSB.Append(new String(',', arrayTypeMeta.Dimensions - 1));
            xSB.Append("]");
            arrayTypeMeta.Data[EcmaCil.DataIds.DebugMetaId] = xSB.ToString();
#endif

        }

        private void ScanType(QueuedType aType, EcmaCil.TypeMeta aTypeMeta)
        {
#if DEBUG
            var xSB = new StringBuilder();
            xSB.Append(aType.Type.ToString());
            if (aType.Args.Length > 0)
            {
                xSB.Append("<");
                for (int i = 0; i < aType.Args.Length; i++)
                {
                    xSB.Append(aType.Args[i].ToString());
                    if (i < (aType.Args.Length - 1))
                    {
                        xSB.Append(", ");
                    }
                }
                xSB.Append(">");
            }
            aTypeMeta.Data[EcmaCil.DataIds.DebugMetaId] = xSB.ToString();
#endif
        }

        private void ScanMethod(QueuedMethod aMethod, EcmaCil.MethodMeta aMethodMeta)
        {
            // todo: add support for plugs
#if DEBUG
            aMethodMeta.Data[EcmaCil.DataIds.DebugMetaId] = aMethod.Method.ToString();
#endif
            var xParamOffset = 0;
            if (!aMethod.Method.IsStatic)
            {
                xParamOffset = 1;
            }
            aMethodMeta.Parameters = new EcmaCil.MethodParameterMeta[aMethod.Method.Parameters.Count + xParamOffset];
            if (!aMethod.Method.IsStatic)
            {
                aMethodMeta.Parameters[0] = new EcmaCil.MethodParameterMeta
                {
                    IsByRef = aMethod.Method.DeclaringType.IsValueType,
                    PropertyType = EnqueueType(aMethod.Method.DeclaringType, aMethod, "Declaring type")
                };
#if DEBUG
                aMethodMeta.Parameters[0].Data[EcmaCil.DataIds.DebugMetaId] = "$this";
#endif

            }
            for(int i = 0; i < aMethod.Method.Parameters.Count;i++){
                var xParam = aMethod.Method.Parameters[i];
                var xParamType = xParam.ParameterType;
                if (xParamType is GenericParameter)
                {          
                    // todo: resolve generics.
                    throw new NotImplementedException();
                }

                var xParamMeta = aMethodMeta.Parameters[i + xParamOffset] = new EcmaCil.MethodParameterMeta();
                //if (xParamType is ReferenceType)
                //{
                //    xParamType = ((ReferenceType)xParamType).ElementType;
                //    xParamMeta.IsByRef = true;
                //}

                var xType = EnqueueType(xParamType, aMethod, "parameter");
#if DEBUG
                var xSB = new StringBuilder();
                xSB.Append(xParam.Name);
                xSB.Append(": ");
                if (xParamMeta.IsByRef)
                {
                    xSB.Append("ref ");
                }
                xSB.Append(xParamType.ToString());
                xParamMeta.Data[EcmaCil.DataIds.DebugMetaId] = xSB.ToString();
#endif
                xParamMeta.PropertyType = xType;
            }
            if (aMethod.Method.ReturnType.FullName != "System.Void")
            {
                aMethodMeta.ReturnType = EnqueueType(aMethod.Method.ReturnType, aMethod.Method, "Return Type");
            }
            aMethodMeta.IsStatic = aMethod.Method.IsStatic;
            ScanMethodBody(aMethod, aMethodMeta);
            #region Virtuals scan
            if (aMethod.Method.IsVirtual)
            {
                if (aMethod.Method.HasGenericParameters)
                {
                    throw new Exception("GEnerics not yet fully supported");
                }
                // For virtuals we need to climb up the type tree
                // and find the top base method. We then add that top
                // node to the mVirtuals list. We don't need to add the 
                // types becuase adding DeclaringType will already cause
                // all ancestor types to be added.

                var xVirtMethod = aMethod.Method;
                TypeReference xVirtType = aMethod.Method.DeclaringType;

                xVirtMethod = xVirtMethod.GetOriginalBaseMethod();
                #region old code
                //                MethodReference xNewVirtMethod = null;
//                while (true)
//                {
//                    var xNewVirtType = xVirtType.Resolve();
//                    if (xNewVirtType.HasGenericParameters)
//                    {
//                        throw new Exception("Generics not fully supported yet!");
//                    }
//                    if (xNewVirtType == null)
//                    {
//                        xVirtType = null;
//                    }
//                    else
//                    {
//#warning // todo: verify if next code works ok with generics
//                        var xTempNewVirtMethod = xNewVirtType.  .m.Methods..GetMethod(aMethod.Method.Name, aMethod.Method.Parameters);
//                        if (xTempNewVirtMethod !=null)
//                        {
//                            if (xTempNewVirtMethod.IsVirtual)
//                            {
//                                xNewVirtMethod = xTempNewVirtMethod;
//                            }
//                        }
//                        else
//                        {
//                            xNewVirtMethod = null;
//                        }
//                    }
//                    if (xNewVirtMethod == null)
//                    {
//                        if (mVirtuals.ContainsKey(aMethod))
//                        {
//                            xVirtMethod = null;
//                        }
//                        break;
//                    }
//                    xVirtMethod = xNewVirtMethod.Resolve();
//                    xVirtType = xNewVirtType.BaseType;
//                    if (xVirtType == null)
//                    {
//                        break;
//                    }
                //                }
                #endregion old code
                if (xVirtMethod!=null)
                {
                    EnqueueMethod(xVirtMethod, aMethod, "Virtual Base");

                    foreach (var xType in mTypes)
                    {
                        if (xType.Key.Type.IsSubclassOf(xVirtMethod.DeclaringType))
                        {
                            //xType.Key.Type.res
                            //var xNewMethod = xType.Key.Type.Methods.GetMethod(aMethod.Method.Name, aMethod.Method.Parameters);
                            //if (xNewMethod != null)
                            //{
                            //                        // We need to check IsVirtual, a non virtual could
                            //                        // "replace" a virtual above it?
                            //    // MtW: correct
                            //    if (xNewMethod.IsVirtual)
                            //    {
                            //        EnqueueMethod(xNewMethod, aMethod, "Virtual Downscan");
                            //    }
                            //}
                            throw new NotImplementedException();
                        }
                    }
                }

            }
            #endregion

        }

        protected virtual void ScanMethodBody(QueuedMethod aMethod, EcmaCil.MethodMeta aMethodMeta)
        {
            if (aMethod.Method.HasBody)
            {
                var xBody = aMethod.Method.Body;
                var xBodyMeta = aMethodMeta.Body = new EcmaCil.MethodBodyMeta();
                xBodyMeta.InitLocals = xBody.InitLocals;
                #region handle exception handling clauses
                if (xBody.HasExceptionHandlers)
                {
                    throw new Exception("ExceptionHandlers are not supported yet");
                }
                #endregion
                #region handle locals
                xBodyMeta.LocalVariables = new EcmaCil.LocalVariableMeta[xBody.Variables.Count];
                for (int i = 0; i < xBody.Variables.Count; i++)
                {
                    var xVar = xBody.Variables[i];
                    var xVarMeta = xBodyMeta.LocalVariables[i] = new EcmaCil.LocalVariableMeta();
                    xVarMeta.LocalType = EnqueueType(xVar.VariableType, aMethod, "Local variable");
#if DEBUG
                    xVarMeta.Data[EcmaCil.DataIds.DebugMetaId] = xVar.Name + ":" + xVar.VariableType.ToString();
#endif              
                }
                #endregion

                xBodyMeta.Instructions = new EcmaCil.IL.BaseInstruction[xBody.Instructions.Count];
                var xILOffsetToInstructionOffset = new Dictionary<int, int>();
                var xInstructionOffsetToILOffset = new Dictionary<int, int>();
                var xSecondStageInits = new List<Action<EcmaCil.MethodMeta>>();
                for (int i = 0; i < xBody.Instructions.Count; i++)
                {
                    xILOffsetToInstructionOffset.Add(xBody.Instructions[i].Offset, i);
                    xInstructionOffsetToILOffset.Add(i, xBody.Instructions[i].Offset);
                }

                for (int i = 0; i < xBody.Instructions.Count; i++)
                {
                    var xInstr = xBody.Instructions[i];
                    xBodyMeta.Instructions[i] =
                        CreateInstructionMeta(aMethod, aMethodMeta, xInstr, xILOffsetToInstructionOffset, xInstructionOffsetToILOffset, i, xSecondStageInits, i + 1);
                }

                if (xSecondStageInits.Count > 0)
                {
                    foreach (var xInit in xSecondStageInits)
                    {
                        xInit(aMethodMeta);
                    }
                }
            }
        }

        private BaseInstruction CreateInstructionMeta(QueuedMethod aMethod, EcmaCil.MethodMeta aMethodMeta, Instruction curInstruction, IDictionary<int, int> aILToInstructionOffset, IDictionary<int, int> aInstructionToILOffset, int aCurrentIndex, IList<Action<EcmaCil.MethodMeta>> aSecondStageInits, int aNextIndex)
        {
            switch (curInstruction.OpCode.Code)
            {
                case Code.Nop:
                    return new InstructionNone(InstructionKindEnum.Nop, aCurrentIndex);
                case Code.Pop:
                    return new InstructionNone(InstructionKindEnum.Pop, aCurrentIndex);
                case Code.Ldc_I4_S:
                    return new InstructionInt32(GetInstructionKind(curInstruction.OpCode.Code), aCurrentIndex, (int)(sbyte)curInstruction.Operand);
                case Code.Stloc_0:
                    return new InstructionInt32(InstructionKindEnum.Stloc, aCurrentIndex, 0);
                case Code.Stloc_1:
                    return new InstructionInt32(InstructionKindEnum.Stloc, aCurrentIndex, 1);
                case Code.Ldloc_0:
                    return new InstructionLocal(InstructionKindEnum.Ldloc, aCurrentIndex, aMethodMeta.Body.LocalVariables[0]);
                case Code.Ldloc_1:
                    return new InstructionLocal(InstructionKindEnum.Ldloc, aCurrentIndex, aMethodMeta.Body.LocalVariables[1]);
                case Code.Ldloc_2:
                    return new InstructionLocal(InstructionKindEnum.Ldloc, aCurrentIndex, aMethodMeta.Body.LocalVariables[2]);
                case Code.Call:
                    return new InstructionMethod(InstructionKindEnum.Call, aCurrentIndex, EnqueueMethod(curInstruction.Operand as MethodReference, aMethod.Method, "Operand value"));
                case Code.Callvirt:
                    return new InstructionMethod(InstructionKindEnum.Callvirt, aCurrentIndex, EnqueueMethod(curInstruction.Operand as MethodReference, aMethod.Method, "Operand value"));
                case Code.Stloc_2:
                    return new InstructionLocal(InstructionKindEnum.Stloc, aCurrentIndex, aMethodMeta.Body.LocalVariables[2]);
                case Code.Stloc_3:
                    return new InstructionLocal(InstructionKindEnum.Stloc, aCurrentIndex, aMethodMeta.Body.LocalVariables[3]);
                case Code.Ret:
                    return new InstructionNone(InstructionKindEnum.Ret, aCurrentIndex);
                case Code.Ldarg_0:
                    return new InstructionArgument(InstructionKindEnum.Ldarg, aCurrentIndex, aMethodMeta.Parameters[0]);
                case Code.Ldarg_1:
                    return new InstructionArgument(InstructionKindEnum.Ldarg, aCurrentIndex, aMethodMeta.Parameters[1]);
                case Code.Add:
                    return new InstructionNone(InstructionKindEnum.Add, aCurrentIndex);
                case Code.Ldstr:
                    return new InstructionString(InstructionKindEnum.Ldstr, aCurrentIndex, (string)curInstruction.Operand);
                case Code.Newobj:
                    return new InstructionMethod(InstructionKindEnum.Newobj, aCurrentIndex, EnqueueMethod(curInstruction.Operand as MethodReference, aMethod.Method, "Operand value"));
                case Code.Br_S:
                case Code.Brfalse_S:
                    var xInstr = new InstructionBranch(GetInstructionKind(curInstruction.OpCode.Code), aCurrentIndex);
                    var xTargetInstr = (Instruction)curInstruction.Operand;
                    var xTargetOffset = xTargetInstr.Offset;
                    aSecondStageInits.Add(delegate(EcmaCil.MethodMeta aTheMethod)
                    {
                        xInstr.Target = aTheMethod.Body.Instructions[aILToInstructionOffset[xTargetOffset]];
                    });
                    return xInstr;

                //    case 
                //case OperandType.InlineNone:
                //    {
                //        xInstrMeta = new InstructionNone(GetInstructionKind(xInstr.OpCode.Code), i + xOffset);
                //        break;
                //    }
                //case OperandType.ShortInlineI:
                //    {
                //        xInstrMeta = new InstructionInt32(GetInstructionKind(xInstr.OpCode.Code), i + xOffset, (int)(sbyte)xInstr.Operand);
                //        break;
                //    }
                default: throw new Exception("Op '" + curInstruction.OpCode.Code + "' not implemented!");
            }
        }
    }
}