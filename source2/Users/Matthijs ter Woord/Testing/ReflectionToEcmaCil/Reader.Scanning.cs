using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ReflectionToEcmaCil
{
    partial class Reader
    {
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
            var xMethodParameters = aMethod.Method.GetParameters();
            aMethodMeta.Parameters = new EcmaCil.MethodParameterMeta[xMethodParameters.Length + xParamOffset];
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
            for (int i = 0; i < xMethodParameters.Length; i++)
            {
                var xParam = xMethodParameters[i];
                var xParamType = xParam.ParameterType;
                var xParamMeta = aMethodMeta.Parameters[i + xParamOffset] = new EcmaCil.MethodParameterMeta();
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
            var xMethodInfo = aMethod.Method as MethodInfo;
            var xReturnType = typeof(void);
            if (xMethodInfo != null)
            {
                xReturnType = xMethodInfo.ReturnType;
            }

            if (xReturnType!= typeof(void))
            {
                aMethodMeta.ReturnType = EnqueueType(xReturnType, aMethod.Method, "Return Type");
            }
            aMethodMeta.IsStatic = aMethod.Method.IsStatic;
            ScanMethodBody(aMethod, aMethodMeta);
            #region temporary disabled
            
            //#region Virtuals scan
            //if (aMethod.Method.IsVirtual)
            //{
            //    // For virtuals we need to climb up the type tree
            //    // and find the top base method. We then add that top
            //    // node to the mVirtuals list. We don't need to add the 
            //    // types becuase adding DeclaringType will already cause
            //    // all ancestor types to be added.

            //    var xVirtMethod = aMethod.Method;
            //    TypeReference xVirtType = aMethod.Method.DeclaringType;

            //    xVirtMethod = xVirtMethod.GetOriginalBaseMethod();
            //    #region old code
            //    //                MethodReference xNewVirtMethod = null;
            //    //                while (true)
            //    //                {
            //    //                    var xNewVirtType = xVirtType.Resolve();
            //    //                    if (xNewVirtType.HasGenericParameters)
            //    //                    {
            //    //                        throw new Exception("Generics not fully supported yet!");
            //    //                    }
            //    //                    if (xNewVirtType == null)
            //    //                    {
            //    //                        xVirtType = null;
            //    //                    }
            //    //                    else
            //    //                    {
            //    //#warning // todo: verify if next code works ok with generics
            //    //                        var xTempNewVirtMethod = xNewVirtType.  .m.Methods..GetMethod(aMethod.Method.Name, aMethod.Method.Parameters);
            //    //                        if (xTempNewVirtMethod !=null)
            //    //                        {
            //    //                            if (xTempNewVirtMethod.IsVirtual)
            //    //                            {
            //    //                                xNewVirtMethod = xTempNewVirtMethod;
            //    //                            }
            //    //                        }
            //    //                        else
            //    //                        {
            //    //                            xNewVirtMethod = null;
            //    //                        }
            //    //                    }
            //    //                    if (xNewVirtMethod == null)
            //    //                    {
            //    //                        if (mVirtuals.ContainsKey(aMethod))
            //    //                        {
            //    //                            xVirtMethod = null;
            //    //                        }
            //    //                        break;
            //    //                    }
            //    //                    xVirtMethod = xNewVirtMethod.Resolve();
            //    //                    xVirtType = xNewVirtType.BaseType;
            //    //                    if (xVirtType == null)
            //    //                    {
            //    //                        break;
            //    //                    }
            //    //                }
            //    #endregion old code
            //    if (xVirtMethod != null)
            //    {
            //        EnqueueMethod(xVirtMethod, aMethod, "Virtual Base");

            //        foreach (var xType in mTypes)
            //        {
            //            if (xType.Key.Type.IsSubclassOf(xVirtMethod.DeclaringType))
            //            {
            //                //xType.Key.Type.res
            //                //var xNewMethod = xType.Key.Type.Methods.GetMethod(aMethod.Method.Name, aMethod.Method.Parameters);
            //                //if (xNewMethod != null)
            //                //{
            //                //                        // We need to check IsVirtual, a non virtual could
            //                //                        // "replace" a virtual above it?
            //                //    // MtW: correct
            //                //    if (xNewMethod.IsVirtual)
            //                //    {
            //                //        EnqueueMethod(xNewMethod, aMethod, "Virtual Downscan");
            //                //    }
            //                //}
            //                throw new NotImplementedException();
            //            }
            //        }
            //    }

            //}
            //#endregion
            #endregion
        }

        protected virtual void ScanMethodBody(QueuedMethod aMethod, EcmaCil.MethodMeta aMethodMeta)
        {
            var xBody = aMethod.Method.GetMethodBody();
            if (xBody!=null)
            {
                var xBodyMeta = aMethodMeta.Body = new EcmaCil.MethodBodyMeta();
                xBodyMeta.InitLocals = xBody.InitLocals;
                #region handle exception handling clauses
                if (xBody.ExceptionHandlingClauses.Count > 0)
                {
                    throw new Exception("ExceptionHandlers are not supported yet");
                }
                #endregion
                #region handle locals
                xBodyMeta.LocalVariables = new EcmaCil.LocalVariableMeta[xBody.LocalVariables.Count];
                for (int i = 0; i < xBody.LocalVariables.Count; i++)
                {
                    var xVar = xBody.LocalVariables[i];
                    var xVarMeta = xBodyMeta.LocalVariables[i] = new EcmaCil.LocalVariableMeta();
                    xVarMeta.LocalType = EnqueueType(xVar.LocalType, aMethod, "Local variable");
#if DEBUG
                    xVarMeta.Data[EcmaCil.DataIds.DebugMetaId] = xVar.LocalType.ToString();
#endif
                }
                #endregion

                var xInstructions = new List<EcmaCil.IL.BaseInstruction>(16);
                //xBodyMeta.Instructions = new EcmaCil.IL.BaseInstruction[xBody.Instructions.Count];
                var xILOffsetToInstructionOffset = new Dictionary<int, int>();
                var xInstructionOffsetToILOffset = new Dictionary<int, int>();
                var xSecondStageInits = new List<Action<EcmaCil.MethodMeta>>();

                //for (int i = 0; i < xBody.Instructions.Count; i++)
                //{
                //    xILOffsetToInstructionOffset.Add(xBody.Instructions[i].Offset, i);
                //    xInstructionOffsetToILOffset.Add(i, xBody.Instructions[i].Offset);
                //}

                //for (int i = 0; i < xBody.Instructions.Count; i++)
                //{
                //    var xInstr = xBody.Instructions[i];
                //    xBodyMeta.Instructions[i] =
                //        CreateInstructionMeta(aMethod, aMethodMeta, xInstr, xILOffsetToInstructionOffset, xInstructionOffsetToILOffset, i, xSecondStageInits, i + 1);
                //}

                //if (xSecondStageInits.Count > 0)
                //{
                //    foreach (var xInit in xSecondStageInits)
                //    {
                //        xInit(aMethodMeta);
                //    }
                //}
            }
        }
    }
}
