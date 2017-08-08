using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using Cil = EcmaCil.IL;

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
            aMethodMeta.Data[EcmaCil.DataIds.DebugMetaId] = aMethod.Method.GetFullName();
#endif
            aMethodMeta.IsVirtual = aMethod.Method.IsVirtual;
            aMethodMeta.IsPublic = aMethod.Method.IsPublic;
            var xMethod = aMethod.Method;
            aMethodMeta.StartsNewVirtualTree = aMethodMeta.IsVirtual && ((aMethod.Method.Attributes & MethodAttributes.NewSlot) == MethodAttributes.NewSlot);
            
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

            if (aMethodMeta.IsVirtual && ((aMethod.Method.Attributes & MethodAttributes.NewSlot) != MethodAttributes.NewSlot))
            {
                // method is override
                // now need to find parent method, just one level up, because when the parent method is scanned, its parent method will be found..
                var xBaseType = aMethod.Method.DeclaringType;
#if DEBUG
                if (xBaseType == null)
                {
                    throw new Exception("New virtual method found, but declaring type has no base type");
                }
#endif
                var xBindFlags = BindingFlags.Instance;
                if (xMethod.IsPublic)
                {
                    xBindFlags |= BindingFlags.Public;
                }
                else
                {
                    xBindFlags |= BindingFlags.NonPublic;
                }
                var xFoundMethod = xBaseType.GetMethod(aMethod.Method.Name,
                        xBindFlags, null, (from item in xMethodParameters
                                           select item.ParameterType).ToArray(), null);
                if (xFoundMethod != null)
                {
                    EnqueueMethod(xFoundMethod, aMethod, "Overridden method");
                }
            }

            var xMethodInfo = aMethod.Method as MethodInfo;
            var xReturnType = typeof(void);
            if (xMethodInfo != null)
            {
                xReturnType = xMethodInfo.ReturnType;
            }

            if (xReturnType != typeof(void))
            {
                aMethodMeta.ReturnType = EnqueueType(xReturnType, aMethod.Method, "Return Type");
            }
            aMethodMeta.IsStatic = aMethod.Method.IsStatic;
            ScanMethodBody(aMethod, aMethodMeta);
        }

        protected virtual void ScanMethodBody(QueuedMethod aMethod, EcmaCil.MethodMeta aMethodMeta)
        {
            var xBody = aMethod.Method.GetMethodBody();
            if (xBody != null)
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

                //List<EcmaCil.IL.BaseInstruction> xInstructions;
                var xILOffsetToInstructionOffset = new Dictionary<int, int>();
                var xInstructionOffsetToILOffset = new Dictionary<int, int>();
                var xSecondStageInits = new List<Action<EcmaCil.MethodMeta>>();

                var xILStream = xBody.GetILAsByteArray();
                foreach (var xPosition in ILStreamPositionReader.GetIndexes(xILStream))
                {
                    xILOffsetToInstructionOffset.Add(xPosition.Key, xPosition.Value);
                    xInstructionOffsetToILOffset.Add(xPosition.Value, xPosition.Key);
                }
                xBodyMeta.Instructions = ScanMethodBody_DoIt(aMethod.Method, aMethodMeta, xILOffsetToInstructionOffset, xInstructionOffsetToILOffset).ToArray();
            }
        }

        // We split this into two arrays since we have to read
        // a byte at a time anways. In the future if we need to 
        // back to a unifed array, instead of 64k entries 
        // we can change it to a signed int, and then add x0200 to the value.
        // This will reduce array size down to 768 entries.
        static readonly OpCode[] mOpCodesLo = new OpCode[256];
        static readonly OpCode[] mOpCodesHi = new OpCode[256];

        static Reader()
        {
            LoadOpCodes();
        }

        protected static void LoadOpCodes()
        {
            foreach (var xField in typeof(OpCodes).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public))
            {
                var xOpCode = (OpCode)xField.GetValue(null);
                var xValue = (ushort)xOpCode.Value;
                if (xValue <= 0xFF)
                {
                    mOpCodesLo[xValue] = xOpCode;
                }
                else
                {
                    mOpCodesHi[xValue & 0xFF] = xOpCode;
                }
            }
        }

        protected static void CheckBranch(int aTarget, int aMethodSize)
        {
            if (aTarget < 0 || aTarget >= aMethodSize)
            {
                throw new Exception("Branch jumps outside method.");
            }
        }

        protected List<EcmaCil.IL.BaseInstruction> ScanMethodBody_DoIt(MethodBase aMethod, EcmaCil.MethodMeta aMethodMeta, IDictionary<int, int> aILOffsetToInstructionIndex, IDictionary<int, int> aInstructionIndexToILOffset)
        {
            var xResult = new List<EcmaCil.IL.BaseInstruction>(aILOffsetToInstructionIndex.Count);
            var xBody = aMethod.GetMethodBody();
            // Cache for use in field and method resolution
            Type[] xTypeGenArgs = null;
            Type[] xMethodGenArgs = null;
            if (aMethod.DeclaringType.IsGenericType)
            {
                xTypeGenArgs = aMethod.DeclaringType.GetGenericArguments();
            }
            if (aMethod.IsGenericMethod)
            {
                xMethodGenArgs = aMethod.GetGenericArguments();
            }

            // Some methods return no body. Not sure why.. have to investigate
            // They arent abstracts or icalls...
            // MtW: how about externs (pinvoke, etc)
            if (xBody == null)
            {
                return null;
            }

            var xIL = xBody.GetILAsByteArray();
            int xPos = 0;
            var xInstructionIndex = 0;
            var xInitSecondStage = new List<Action>(aILOffsetToInstructionIndex.Count);
            while (xPos < xIL.Length)
            {
                ExceptionHandlingClause xCurrentHandler = null;
                #region Determine current handler
                // todo: add support for nested handlers using a stack or so..
                foreach (ExceptionHandlingClause xHandler in xBody.ExceptionHandlingClauses)
                {
                    if (xHandler.TryOffset > 0)
                    {
                        if (xHandler.TryOffset <= xPos && (xHandler.TryLength + xHandler.TryOffset + 1) > xPos) // + 1 because index should be less than the try
                        {
                            if (xCurrentHandler == null)
                            {
                                xCurrentHandler = xHandler;
                                continue;
                            }
                            else if (xHandler.TryOffset > xCurrentHandler.TryOffset && (xHandler.TryLength + xHandler.TryOffset) < (xCurrentHandler.TryLength + xCurrentHandler.TryOffset))
                            {
                                // only replace if the current found handler is narrower
                                xCurrentHandler = xHandler;
                                continue;
                            }
                        }
                    }
                    if (xHandler.HandlerOffset > 0)
                    {
                        if (xHandler.HandlerOffset <= xPos && (xHandler.HandlerOffset + xHandler.HandlerLength + 1) > xPos)
                        {
                            if (xCurrentHandler == null)
                            {
                                xCurrentHandler = xHandler;
                                continue;
                            }
                            else if (xHandler.HandlerOffset > xCurrentHandler.HandlerOffset && (xHandler.HandlerOffset + xHandler.HandlerLength) < (xCurrentHandler.HandlerOffset + xCurrentHandler.HandlerLength))
                            {
                                // only replace if the current found handler is narrower
                                xCurrentHandler = xHandler;
                                continue;
                            }
                        }
                    }
                    if ((xHandler.Flags & ExceptionHandlingClauseOptions.Filter) > 0)
                    {
                        if (xHandler.FilterOffset > 0)
                        {
                            if (xHandler.FilterOffset <= xPos)
                            {
                                if (xCurrentHandler == null)
                                {
                                    xCurrentHandler = xHandler;
                                    continue;
                                }
                                else if (xHandler.FilterOffset > xCurrentHandler.FilterOffset)
                                {
                                    // only replace if the current found handler is narrower
                                    xCurrentHandler = xHandler;
                                    continue;
                                }
                            }
                        }
                    }
                }
                #endregion
                OpCodeEnum xOpCodeVal;
                OpCode xOpCode;
                int xOpPos = xPos;
                if (xIL[xPos] == 0xFE)
                {
                    xOpCodeVal = (OpCodeEnum)(0xFE00 | xIL[xPos + 1]);
                    xOpCode = mOpCodesHi[xIL[xPos + 1]];
                    xPos = xPos + 2;
                }
                else
                {
                    xOpCodeVal = (OpCodeEnum)xIL[xPos];
                    xOpCode = mOpCodesLo[xIL[xPos]];
                    xPos++;
                }

                EcmaCil.IL.BaseInstruction xILOpCode = null;
                Cil.InstructionBranch xBranch;
                Console.WriteLine(xOpCode.ToString() + " " + xOpCode.OperandType);
                #region switch(xOpCode.OperandType)
                switch (xOpCode.OperandType)
                {
                    // No operand.
                    case OperandType.InlineNone:
                        {
                            #region Inline none options
                            // These shortcut translation regions expand shortcut ops into full ops
                            // This elminates the amount of code required in the assemblers
                            // by allowing them to ignore the shortcuts
                            switch (xOpCodeVal)
                            {
                                case OpCodeEnum.Ldarg_0:
                                    xILOpCode = new Cil.InstructionArgument(EcmaCil.IL.InstructionKindEnum.Ldarg, xInstructionIndex, aMethodMeta.Parameters[0]);
                                    break;
                                case OpCodeEnum.Ldarg_1:
                                    xILOpCode = new Cil.InstructionArgument(EcmaCil.IL.InstructionKindEnum.Ldarg, xInstructionIndex, aMethodMeta.Parameters[1]);
                                    break;
                                case OpCodeEnum.Ldarg_2:
                                    xILOpCode = new Cil.InstructionArgument(EcmaCil.IL.InstructionKindEnum.Ldarg, xInstructionIndex, aMethodMeta.Parameters[2]);
                                    break;
                                case OpCodeEnum.Ldarg_3:
                                    xILOpCode = new Cil.InstructionArgument(EcmaCil.IL.InstructionKindEnum.Ldarg, xInstructionIndex, aMethodMeta.Parameters[3]);
                                    break;
                                case OpCodeEnum.Ldc_I4_0:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 0);
                                    break;
                                case OpCodeEnum.Ldc_I4_1:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 1);
                                    break;
                                case OpCodeEnum.Ldc_I4_2:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 2);
                                    break;
                                case OpCodeEnum.Ldc_I4_3:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 3);
                                    break;
                                case OpCodeEnum.Ldc_I4_4:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 4);
                                    break;
                                case OpCodeEnum.Ldc_I4_5:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 5);
                                    break;
                                case OpCodeEnum.Ldc_I4_6:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 6);
                                    break;
                                case OpCodeEnum.Ldc_I4_7:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 7);
                                    break;
                                case OpCodeEnum.Ldc_I4_8:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, 8);
                                    break;
                                case OpCodeEnum.Ldc_I4_M1:
                                    xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, -1);
                                    break;
                                case OpCodeEnum.Ldloc_0:
                                    xILOpCode = new Cil.InstructionLocal(EcmaCil.IL.InstructionKindEnum.Ldloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[0]);
                                    break;
                                case OpCodeEnum.Ldloc_1:
                                    xILOpCode = new Cil.InstructionLocal(EcmaCil.IL.InstructionKindEnum.Ldloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[1]);
                                    break;
                                case OpCodeEnum.Ldloc_2:
                                    xILOpCode = new Cil.InstructionLocal(EcmaCil.IL.InstructionKindEnum.Ldloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[2]);
                                    break;
                                case OpCodeEnum.Ldloc_3:
                                    xILOpCode = new Cil.InstructionLocal(EcmaCil.IL.InstructionKindEnum.Ldloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[3]);
                                    break;
                                case OpCodeEnum.Stloc_0:
                                    xILOpCode = new Cil.InstructionLocal(EcmaCil.IL.InstructionKindEnum.Stloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[0]);
                                    break;
                                case OpCodeEnum.Stloc_1:
                                    xILOpCode = new Cil.InstructionLocal(EcmaCil.IL.InstructionKindEnum.Stloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[1]);
                                    break;
                                case OpCodeEnum.Stloc_2:
                                    xILOpCode = new Cil.InstructionLocal(EcmaCil.IL.InstructionKindEnum.Stloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[2]);
                                    break;
                                case OpCodeEnum.Stloc_3:
                                    xILOpCode = new Cil.InstructionLocal(EcmaCil.IL.InstructionKindEnum.Stloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[3]);
                                    break;
                                default:
                                    xILOpCode = new Cil.InstructionNone((EcmaCil.IL.InstructionKindEnum)xOpCodeVal, xInstructionIndex);
                                    break;
                            }
                            #endregion
                            break;
                        }

                    case OperandType.ShortInlineBrTarget:
                        {
                            #region Inline branch
                            // By calculating target, we assume all branches are within a method
                            // So far at least wtih csc, its true. We check it with CheckBranch
                            // just in case.
                            int xTarget = xPos + 1 + (sbyte)xIL[xPos];
                            CheckBranch(xTarget, xIL.Length);
                            switch (xOpCodeVal)
                            {
                                case OpCodeEnum.Beq_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Beq, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Bge_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Bge, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Bge_Un_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Bge_Un, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Bgt_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Bgt, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Bgt_Un_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Bgt_Un, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Ble_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Ble, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Ble_Un_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Ble_Un, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Blt_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Blt, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Blt_Un_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Blt_Un, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Bne_Un_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Bne_Un, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Br_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Br, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Brfalse_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Brfalse, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Brtrue_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Brtrue, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                case OpCodeEnum.Leave_S:
                                    xBranch = new Cil.InstructionBranch(EcmaCil.IL.InstructionKindEnum.Leave, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                                default:
                                    xBranch = new Cil.InstructionBranch((EcmaCil.IL.InstructionKindEnum)xOpCodeVal, xInstructionIndex);
                                    xILOpCode = xBranch;
                                    xInitSecondStage.Add(delegate
                                    {
                                        xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                                    });
                                    break;
                            }
                            xPos = xPos + 1;
                            break;
                            #endregion
                        }
                    case OperandType.InlineBrTarget:
                        {
                            int xTarget = xPos + 4 + (Int32)ReadInt32(xIL, xPos);
                            CheckBranch(xTarget, xIL.Length);
                            xILOpCode = xBranch = new Cil.InstructionBranch((EcmaCil.IL.InstructionKindEnum)xOpCodeVal, xInstructionIndex);
                            xInitSecondStage.Add(delegate
                            {
                                xBranch.Target = xResult[aILOffsetToInstructionIndex[xTarget]];
                            });
                            xPos = xPos + 4;
                            break;
                        }

                    case OperandType.ShortInlineI:
                        switch (xOpCodeVal)
                        {
                            case OpCodeEnum.Ldc_I4_S:
                                xILOpCode = new Cil.InstructionInt32(EcmaCil.IL.InstructionKindEnum.Ldc_I4, xInstructionIndex, xIL[xPos]);
                                break;
                            default:
                                xILOpCode = new Cil.InstructionInt32((EcmaCil.IL.InstructionKindEnum)xOpCodeVal, xInstructionIndex, xIL[xPos]);
                                break;
                        }
                        xPos = xPos + 1;
                        break;
                    case OperandType.InlineI:
                        xILOpCode = new Cil.InstructionInt32((Cil.InstructionKindEnum)xOpCodeVal, xInstructionIndex, ReadInt32(xIL, xPos));
                        xPos = xPos + 4;
                        break;
                    case OperandType.InlineI8:
                        xILOpCode = new Cil.InstructionInt64((Cil.InstructionKindEnum)xOpCodeVal, xInstructionIndex, ReadInt64(xIL, xPos));
                        xPos = xPos + 8;
                        break;

                    case OperandType.ShortInlineR:
                        // this is not correct:
                        //xILOpCode = new Cil.InstructionSingle(
                        //xILOpCode = new ILOpCodes.OpSingle(xOpCodeVal, xOpPos, xPos + 4, BitConverter.ToSingle(xIL, xPos), xCurrentHandler);
                        //xPos = xPos + 4;
                        //break;
                        throw new NotImplementedException();
                    case OperandType.InlineR:
                        // this is not correct
                        //xILOpCode = new ILOpCodes.OpDouble(xOpCodeVal, xOpPos, xPos + 8, BitConverter.ToDouble(xIL, xPos), xCurrentHandler);
                        //xPos = xPos + 8;
                        //break;
                        throw new NotImplementedException();

                    // The operand is a 32-bit metadata token.
                    case OperandType.InlineField:
                        throw new NotImplementedException();
                    //{
                    //    var xValue = aMethod.Module.ResolveField((int)ReadInt32(xIL, xPos), xTypeGenArgs, xMethodGenArgs);

                    //    xILOpCode = new ILOpCodes.OpField(xOpCodeVal, xOpPos, xPos + 4, xValue, xCurrentHandler);
                    //    xPos = xPos + 4;
                    //    break;
                    //}

                    // The operand is a 32-bit metadata token.
                    case OperandType.InlineMethod:
                        var xTargetMethod = EnqueueMethod(aMethod.DeclaringType.Module.ResolveMethod(ReadInt32(xIL, xPos)), aMethod, "Method Call");
                        xILOpCode = new Cil.InstructionMethod((Cil.InstructionKindEnum)xOpCodeVal, xInstructionIndex, xTargetMethod);
                        xPos = xPos + 4;
                        break; 
                    //{
                    //    var xValue = aMethod.Module.ResolveMethod((int)ReadInt32(xIL, xPos), xTypeGenArgs, xMethodGenArgs);
                    //    xILOpCode = new ILOpCodes.OpMethod(xOpCodeVal, xOpPos, xPos + 4, xValue, xCurrentHandler);
                    //    xPos = xPos + 4;
                    //    break;
                    //}

                    // 32-bit metadata signature token.
                    case OperandType.InlineSig:
                        throw new NotImplementedException();

                    case OperandType.InlineString:
                        xILOpCode = new Cil.InstructionString(Cil.InstructionKindEnum.Ldstr, xInstructionIndex, aMethod.Module.ResolveString(ReadInt32(xIL, xPos)));
                        xPos = xPos + 4;
                        break;

                    case OperandType.InlineSwitch:
                        throw new NotImplementedException();
                    //{
                    //    int xCount = (int)ReadInt32(xIL, xPos);
                    //    xPos = xPos + 4;
                    //    int xNextOpPos = xPos + xCount * 4;
                    //    int[] xBranchLocations = new int[xCount];
                    //    for (int i = 0; i < xCount; i++)
                    //    {
                    //        xBranchLocations[i] = xNextOpPos + (int)ReadInt32(xIL, xPos + i * 4);
                    //        CheckBranch(xBranchLocations[i], xIL.Length);
                    //    }
                    //    xILOpCode = new ILOpCodes.OpSwitch(xOpCodeVal, xOpPos, xPos, xBranchLocations, xCurrentHandler);
                    //    xPos = xNextOpPos;
                    //    break;
                    //}

                    // The operand is a FieldRef, MethodRef, or TypeRef token.
                    case OperandType.InlineTok:
                        throw new NotImplementedException();
                    //xILOpCode = new ILOpCodes.OpToken(xOpCodeVal, xOpPos, xPos + 4, ReadInt32(xIL, xPos), aMethod.Module, xTypeGenArgs, xMethodGenArgs, xCurrentHandler);
                    //                        xPos = xPos + 4;
                    //                        break;

                    // 32-bit metadata token.
                    case OperandType.InlineType:
                        throw new NotImplementedException();
                    //{
                    //    var xValue = aMethod.Module.ResolveType((int)ReadInt32(xIL, xPos), xTypeGenArgs, xMethodGenArgs);
                    //    xILOpCode = new ILOpCodes.OpType(xOpCodeVal, xOpPos, xPos + 4, xValue, xCurrentHandler);
                    //    xPos = xPos + 4;
                    //    break;
                    //}

                    case OperandType.ShortInlineVar:
                        switch (xOpCodeVal)
                        {
                            case OpCodeEnum.Ldloc_S:
                                xILOpCode = new Cil.InstructionLocal(Cil.InstructionKindEnum.Ldloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[xIL[xPos]]);
                                break;
                            case OpCodeEnum.Ldloca_S:
                                xILOpCode = new Cil.InstructionLocal(Cil.InstructionKindEnum.Ldloca, xInstructionIndex, aMethodMeta.Body.LocalVariables[xIL[xPos]]);
                                break;
                            case OpCodeEnum.Ldarg_S:
                                xILOpCode = new Cil.InstructionArgument(Cil.InstructionKindEnum.Ldarg, xInstructionIndex, aMethodMeta.Parameters[xIL[xPos]]);
                                break;
                            case OpCodeEnum.Ldarga_S:
                                xILOpCode = new Cil.InstructionArgument(Cil.InstructionKindEnum.Ldarga, xInstructionIndex, aMethodMeta.Parameters[xIL[xPos]]);
                                break;
                            case OpCodeEnum.Starg_S:
                                xILOpCode = new Cil.InstructionArgument(Cil.InstructionKindEnum.Starg, xInstructionIndex, aMethodMeta.Parameters[xIL[xPos]]);
                                break;
                            case OpCodeEnum.Stloc_S:
                                xILOpCode = new Cil.InstructionLocal(Cil.InstructionKindEnum.Stloc, xInstructionIndex, aMethodMeta.Body.LocalVariables[xIL[xPos]]);
                                break;
                            default:
                                throw new NotImplementedException();
                            //xILOpCode = new ILOpCodes.OpVar(xOpCodeVal, xOpPos, xPos + 1, xIL[xPos], xCurrentHandler);
                            //break;
                        }
                        xPos = xPos + 1;
                        break;
                    case OperandType.InlineVar:
                        //xILOpCode = new ILOpCodes.OpVar(xOpCodeVal, xOpPos, xPos + 2, ReadUInt16(xIL, xPos), xCurrentHandler);
                        //xPos = xPos + 2;
                        throw new NotImplementedException();
                        break;

                    default:
                        throw new Exception("Unknown OperandType");
                }
                #endregion switch(xOpCode.OperandType)
                xResult.Add(xILOpCode);
                xInstructionIndex++;
            }
            foreach (var xAction in xInitSecondStage)
            {
                xAction();
            }
            return xResult;
        }

        // We could use BitConvertor, unfortuantely they "hardcoded" endianness. Its fine for reading IL now...
        // but they essentially do the same as we do, just a bit slower.
        private static UInt16 ReadUInt16(byte[] aBytes, int aPos)
        {
            return (UInt16)(aBytes[aPos + 1] << 8 | aBytes[aPos]);
        }

        private static Int32 ReadInt32(byte[] aBytes, int aPos)
        {
            return (Int32)(aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos]);
        }

        private static Int64 ReadInt64(byte[] aBytes, int aPos)
        {
            //return (UInt64)(
            //  aBytes[aPos + 7] << 56 | aBytes[aPos + 6] << 48 | aBytes[aPos + 5] << 40 | aBytes[aPos + 4] << 32
            //  | aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos]);

            return BitConverter.ToInt64(aBytes, aPos);
        }

        private void ScanArrayType(QueuedArrayType aArrayType, EcmaCil.ArrayTypeMeta aArrayMeta)
        {
            aArrayMeta.Dimensions = aArrayType.ArrayType.GetArrayRank();
            // todo: fix?
            //            foreach (ArrayDimension xDimension in aType.ArrayType.Dimensions)
            //{
            //    if (xDimension.LowerBound != 0 || xDimension.UpperBound != 0)
            //    {
            //        throw new Exception("Arrays with limited dimensions not supported");
            //    }
            //}
            if (aArrayType.ArrayType.GetArrayRank() != 1)
            {
                throw new Exception("Multidimensional arrays not yet supported!");
            }

#if DEBUG
            var xSB = new StringBuilder();
            xSB.Append(aArrayMeta.ElementType.ToString());
            xSB.Append("[");
            xSB.Append(new String(',', aArrayMeta.Dimensions - 1));
            xSB.Append("]");
            aArrayMeta.Data[EcmaCil.DataIds.DebugMetaId] = xSB.ToString();
#endif
        }


        private void ScanVMT()
        {
            
        }
    }
}
