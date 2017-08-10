using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;

using Cosmos.Debug.Symbols;

using Cosmos.IL2CPU.Extensions;

namespace Cosmos.IL2CPU {
  public class ILReader {
    // We split this into two arrays since we have to read
    // a byte at a time anways. In the future if we need to
    // back to a unifed array, instead of 64k entries
    // we can change it to a signed int, and then add x0200 to the value.
    // This will reduce array size down to 768 entries.
    protected OpCode[] mOpCodesLo = new OpCode[256];
    protected OpCode[] mOpCodesHi = new OpCode[256];

    public ILReader() {
      LoadOpCodes();
    }

    protected void LoadOpCodes() {
      foreach (var xField in typeof(OpCodes).GetTypeInfo().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)) {
        var xOpCode = (OpCode)xField.GetValue(null);
        var xValue = (ushort)xOpCode.Value;
        if (xValue <= 0xFF) {
          mOpCodesLo[xValue] = xOpCode;
        } else {
          mOpCodesHi[xValue & 0xFF] = xOpCode;
        }
      }
    }

    protected void CheckBranch(int aTarget, int aMethodSize) {
      // this method is a safety-measure. Should never occur
      if (aTarget < 0 || aTarget >= aMethodSize) {
        throw new Exception("Branch jumps outside method.");
      }
    }

    public List<ILOpCode> ProcessMethod(MethodBase aMethod) {
      var xResult = new List<ILOpCode>();
      var xBody = aMethod.GetMethodBody();
      // Cache for use in field and method resolution
      Type[] xTypeGenArgs = Type.EmptyTypes;
      Type[] xMethodGenArgs = Type.EmptyTypes;
      if (aMethod.DeclaringType.GetTypeInfo().IsGenericType) {
        xTypeGenArgs = aMethod.DeclaringType.GetTypeInfo().GetGenericArguments();
      }
      if (aMethod.IsGenericMethod) {
        xMethodGenArgs = aMethod.GetGenericArguments();
      }

      // Some methods return no body. Not sure why.. have to investigate
      // They arent abstracts or icalls...
      if (xBody == null) {
        return null;
      }

      var xIL = xBody.GetILBytes();
      int xPos = 0;
      while (xPos < xIL.Length) {
          _ExceptionRegionInfo xCurrentExceptionRegion = null;
          #region Determine current handler
          // todo: add support for nested handlers using a stack or so..
          foreach (_ExceptionRegionInfo xHandler in xBody.GetExceptionRegionInfos(aMethod.DeclaringType.GetTypeInfo().Module))
          {
              if (xHandler.TryOffset > 0)
              {
                  if (xHandler.TryOffset <= xPos && (xHandler.TryLength + xHandler.TryOffset ) > xPos)
                  {
                      if (xCurrentExceptionRegion == null)
                      {
                          xCurrentExceptionRegion = xHandler;
                          continue;
                      }
                      else if (xHandler.TryOffset > xCurrentExceptionRegion.TryOffset && (xHandler.TryLength + xHandler.TryOffset) < (xCurrentExceptionRegion.TryLength + xCurrentExceptionRegion.TryOffset))
                      {
                          // only replace if the current found handler is narrower
                          xCurrentExceptionRegion = xHandler;
                          continue;
                      }
                  }
              }
              if (xHandler.HandlerOffset > 0)
              {
                  if (xHandler.HandlerOffset <= xPos && (xHandler.HandlerOffset + xHandler.HandlerLength) > xPos)
                  {
                      if (xCurrentExceptionRegion == null)
                      {
                          xCurrentExceptionRegion = xHandler;
                          continue;
                      }
                      else if (xHandler.HandlerOffset > xCurrentExceptionRegion.HandlerOffset && (xHandler.HandlerOffset + xHandler.HandlerLength) < (xCurrentExceptionRegion.HandlerOffset + xCurrentExceptionRegion.HandlerLength))
                      {
                          // only replace if the current found handler is narrower
                          xCurrentExceptionRegion = xHandler;
                          continue;
                      }
                  }
              }
              if (xHandler.Kind.HasFlag(ExceptionRegionKind.Filter))
              {
                  if (xHandler.FilterOffset > 0)
                  {
                      if (xHandler.FilterOffset <= xPos)
                      {
                          if (xCurrentExceptionRegion == null)
                          {
                              xCurrentExceptionRegion = xHandler;
                              continue;
                          }
                          else if (xHandler.FilterOffset > xCurrentExceptionRegion.FilterOffset)
                          {
                              // only replace if the current found handler is narrower
                              xCurrentExceptionRegion = xHandler;
                              continue;
                          }
                      }
                  }
              }
          }
          #endregion
          ILOpCode.Code xOpCodeVal;
        OpCode xOpCode;
        int xOpPos = xPos;
        if (xIL[xPos] == 0xFE) {
          xOpCodeVal = (ILOpCode.Code)(0xFE00 | xIL[xPos + 1]);
          xOpCode = mOpCodesHi[xIL[xPos + 1]];
          xPos = xPos + 2;
        } else {
          xOpCodeVal = (ILOpCode.Code)xIL[xPos];
          xOpCode = mOpCodesLo[xIL[xPos]];
          xPos++;
        }

        ILOpCode xILOpCode = null;
        switch (xOpCode.OperandType) {
          // No operand.
          case OperandType.InlineNone:
                {
                    #region Inline none options
                    // These shortcut translation regions expand shortcut ops into full ops
                    // This elminates the amount of code required in the assemblers
                    // by allowing them to ignore the shortcuts
                    switch (xOpCodeVal) {
                    case ILOpCode.Code.Ldarg_0:
                            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, xOpPos, xPos, 0, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldarg_1:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, xOpPos, xPos, 1, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldarg_2:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, xOpPos, xPos, 2, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldarg_3:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, xOpPos, xPos, 3, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_0:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 0, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_1:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 1, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_2:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 2, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_3:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 3, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_4:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 4, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_5:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 5, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_6:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 6, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_7:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 7, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_8:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, 8, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldc_I4_M1:
                        xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos, -1, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldloc_0:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, xOpPos, xPos, 0, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldloc_1:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, xOpPos, xPos, 1, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldloc_2:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, xOpPos, xPos, 2, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Ldloc_3:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, xOpPos, xPos, 3, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Stloc_0:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, xOpPos, xPos, 0, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Stloc_1:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, xOpPos, xPos, 1, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Stloc_2:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, xOpPos, xPos, 2, xCurrentExceptionRegion);
                        break;
                    case ILOpCode.Code.Stloc_3:
                        xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, xOpPos, xPos, 3, xCurrentExceptionRegion);
                        break;
                    default:
                        xILOpCode = new ILOpCodes.OpNone(xOpCodeVal, xOpPos, xPos, xCurrentExceptionRegion);
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
                        case ILOpCode.Code.Beq_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Beq, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Bge_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Bge, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Bge_Un_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Bge_Un, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Bgt_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Bgt, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Bgt_Un_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Bgt_Un, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Ble_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Ble, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Ble_Un_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Ble_Un, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Blt_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Blt, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Blt_Un_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Blt_Un, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Bne_Un_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Bne_Un, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Br_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Br, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Brfalse_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Brfalse, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Brtrue_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Brtrue, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        case ILOpCode.Code.Leave_S:
                            xILOpCode = new ILOpCodes.OpBranch(ILOpCode.Code.Leave, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                        default:
                            xILOpCode = new ILOpCodes.OpBranch(xOpCodeVal, xOpPos, xPos + 1, xTarget, xCurrentExceptionRegion);
                            break;
                    }
                    xPos = xPos + 1;
                    break;
                    #endregion
                }
          case OperandType.InlineBrTarget: {
              int xTarget = xPos + 4 + ReadInt32(xIL, xPos);
              CheckBranch(xTarget, xIL.Length);
              xILOpCode = new ILOpCodes.OpBranch(xOpCodeVal, xOpPos, xPos + 4, xTarget, xCurrentExceptionRegion);
              xPos = xPos + 4;
              break;
            }

          case OperandType.ShortInlineI:
            switch (xOpCodeVal) {
              case ILOpCode.Code.Ldc_I4_S:
                    xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, xOpPos, xPos + 1, ((sbyte)xIL[xPos]), xCurrentExceptionRegion);
                break;
              default:
                xILOpCode = new ILOpCodes.OpInt(xOpCodeVal, xOpPos, xPos + 1, ((sbyte)xIL[xPos]), xCurrentExceptionRegion);
                break;
            }
            xPos = xPos + 1;
            break;
          case OperandType.InlineI:
            xILOpCode = new ILOpCodes.OpInt(xOpCodeVal, xOpPos, xPos + 4, ReadInt32(xIL, xPos), xCurrentExceptionRegion);
            xPos = xPos + 4;
            break;
          case OperandType.InlineI8:
            xILOpCode = new ILOpCodes.OpInt64(xOpCodeVal, xOpPos, xPos + 8, ReadUInt64(xIL, xPos), xCurrentExceptionRegion);
            xPos = xPos + 8;
            break;

          case OperandType.ShortInlineR:
            xILOpCode = new ILOpCodes.OpSingle(xOpCodeVal, xOpPos, xPos + 4, BitConverter.ToSingle(xIL, xPos), xCurrentExceptionRegion);
            xPos = xPos + 4;
            break;
          case OperandType.InlineR:
            xILOpCode = new ILOpCodes.OpDouble(xOpCodeVal, xOpPos, xPos + 8, BitConverter.ToDouble(xIL, xPos), xCurrentExceptionRegion);
            xPos = xPos + 8;
            break;

          // The operand is a 32-bit metadata token.
          case OperandType.InlineField: {
              var xValue = aMethod.Module.ResolveField(ReadInt32(xIL, xPos), xTypeGenArgs, xMethodGenArgs);
              xILOpCode = new ILOpCodes.OpField(xOpCodeVal, xOpPos, xPos + 4, xValue, xCurrentExceptionRegion);
              xPos = xPos + 4;
              break;
            }

          // The operand is a 32-bit metadata token.
          case OperandType.InlineMethod: {
              var xValue = aMethod.Module.ResolveMethod(ReadInt32(xIL, xPos), xTypeGenArgs, xMethodGenArgs);
              xILOpCode = new ILOpCodes.OpMethod(xOpCodeVal, xOpPos, xPos + 4, xValue, xCurrentExceptionRegion);
              xPos = xPos + 4;
              break;
            }

          // 32-bit metadata signature token.
          case OperandType.InlineSig:
            xILOpCode = new ILOpCodes.OpSig(xOpCodeVal, xOpPos, xPos + 4, ReadInt32(xIL, xPos), xCurrentExceptionRegion);
            xPos = xPos + 4;
            break;

          case OperandType.InlineString:
            xILOpCode = new ILOpCodes.OpString(xOpCodeVal, xOpPos, xPos + 4, aMethod.Module.ResolveString(ReadInt32(xIL, xPos)), xCurrentExceptionRegion);
            xPos = xPos + 4;
            break;

          case OperandType.InlineSwitch: {
              int xCount = ReadInt32(xIL, xPos);
              xPos = xPos + 4;
              int xNextOpPos = xPos + xCount * 4;
              var xBranchLocations = new int[xCount];
              for (int i = 0; i < xCount; i++) {
                xBranchLocations[i] = xNextOpPos + ReadInt32(xIL, xPos + i * 4);
                CheckBranch(xBranchLocations[i], xIL.Length);
              }
              xILOpCode = new ILOpCodes.OpSwitch(xOpCodeVal, xOpPos, xNextOpPos, xBranchLocations, xCurrentExceptionRegion);
              xPos = xNextOpPos;
              break;
            }

          // The operand is a FieldRef, MethodRef, or TypeRef token.
          case OperandType.InlineTok:
              xILOpCode = new ILOpCodes.OpToken(xOpCodeVal, xOpPos, xPos + 4, ReadInt32(xIL, xPos), aMethod.Module, xTypeGenArgs, xMethodGenArgs, xCurrentExceptionRegion);
            xPos = xPos + 4;
            break;

          // 32-bit metadata token.
          case OperandType.InlineType: {
              var xValue = aMethod.Module.ResolveType(ReadInt32(xIL, xPos), xTypeGenArgs, xMethodGenArgs);
              xILOpCode = new ILOpCodes.OpType(xOpCodeVal, xOpPos, xPos + 4, xValue, xCurrentExceptionRegion);
              xPos = xPos + 4;
              break;
            }

          case OperandType.ShortInlineVar:
            switch (xOpCodeVal) {
              case ILOpCode.Code.Ldloc_S:
                    xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, xOpPos, xPos + 1, xIL[xPos], xCurrentExceptionRegion);
                break;
              case ILOpCode.Code.Ldloca_S:
                xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloca, xOpPos, xPos + 1, xIL[xPos], xCurrentExceptionRegion);
                break;
              case ILOpCode.Code.Ldarg_S:
                xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, xOpPos, xPos + 1, xIL[xPos], xCurrentExceptionRegion);
                break;
              case ILOpCode.Code.Ldarga_S:
                xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarga, xOpPos, xPos + 1, xIL[xPos], xCurrentExceptionRegion);
                break;
              case ILOpCode.Code.Starg_S:
                xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Starg, xOpPos, xPos + 1, xIL[xPos], xCurrentExceptionRegion);
                break;
              case ILOpCode.Code.Stloc_S:
                xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, xOpPos, xPos + 1, xIL[xPos], xCurrentExceptionRegion);
                break;
              default:
                xILOpCode = new ILOpCodes.OpVar(xOpCodeVal, xOpPos, xPos + 1, xIL[xPos], xCurrentExceptionRegion);
                break;
            }
            xPos = xPos + 1;
            break;
          case OperandType.InlineVar:
            xILOpCode = new ILOpCodes.OpVar(xOpCodeVal, xOpPos, xPos + 2, ReadUInt16(xIL, xPos), xCurrentExceptionRegion);
            xPos = xPos + 2;
            break;

          default:
            throw new Exception("Unknown OperandType");
        }
        xILOpCode.InitStackAnalysis(aMethod);
        xResult.Add(xILOpCode);
      }
      return xResult;
    }

    // We could use BitConvertor, unfortuantely they "hardcoded" endianness. Its fine for reading IL now...
    // but they essentially do the same as we do, just a bit slower.
    private UInt16 ReadUInt16(byte[] aBytes, int aPos) {
      return (UInt16)(aBytes[aPos + 1] << 8 | aBytes[aPos]);
    }

    private Int32 ReadInt32(byte[] aBytes, int aPos) {
      return aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos];
    }

    private UInt64 ReadUInt64(byte[] aBytes, int aPos) {
      //return (UInt64)(
      //  aBytes[aPos + 7] << 56 | aBytes[aPos + 6] << 48 | aBytes[aPos + 5] << 40 | aBytes[aPos + 4] << 32
      //  | aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos]);

        return BitConverter.ToUInt64(aBytes, aPos);
    }

  }
}
