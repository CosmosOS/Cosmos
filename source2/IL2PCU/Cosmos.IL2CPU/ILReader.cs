using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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
      foreach (var xField in typeof(OpCodes).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)) {
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
      if (aTarget < 0 || aTarget >= aMethodSize) {
        throw new Exception("Branch jumps outside method.");
      }
    }

    public List<ILOpCode> ProcessMethod(MethodBase aMethod) {
      var xResult = new List<ILOpCode>();
      var xBody = aMethod.GetMethodBody();
      // Cache for use in field and method resolution
      Type[] xTypeGenArgs = null;
      Type[] xMethodGenArgs = null;
      if (aMethod.DeclaringType.IsGenericType) {
        xTypeGenArgs = aMethod.DeclaringType.GetGenericArguments();
      }
      if (aMethod.IsGenericMethod) {
        xMethodGenArgs = aMethod.GetGenericArguments();
      }

      // Some methods return no body. Not sure why.. have to investigate
      // They arent abstracts or icalls...
      // MtW: how about externs (pinvoke, etc)
      if (xBody == null) {
        return null;
      }

      var xIL = xBody.GetILAsByteArray();
      int xPos = 0;
      while (xPos < xIL.Length) {
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
            xILOpCode = new ILOpCodes.OpNone(xOpCodeVal);
            break;

          //TODO: Branches could be outside the current method -
          // do we need to support this? Maybe add a check inside here
          // and see if it pops up or not?
          case OperandType.ShortInlineBrTarget: {
              int xTarget = xOpPos + (sbyte)xIL[xPos];
              CheckBranch(xTarget, xIL.Length);
              xILOpCode = new ILOpCodes.OpBranch(xOpCodeVal, xTarget);
              xPos = xPos + 1;
              break;
            }
          case OperandType.InlineBrTarget: {
            //todo: fix this, branches are relative to the next op, not current.
              int xTarget = xOpPos + (Int32)ReadUInt32(xIL, xPos);
              CheckBranch(xTarget, xIL.Length);
              xILOpCode = new ILOpCodes.OpBranch(xOpCodeVal, xTarget);
              xPos = xPos + 4;
              break;
            }

          case OperandType.ShortInlineI:
            xILOpCode = new ILOpCodes.OpInt(xOpCodeVal, xIL[xPos]);
            xPos = xPos + 1;
            break;
          case OperandType.InlineI:
            xILOpCode = new ILOpCodes.OpInt(xOpCodeVal, ReadUInt32(xIL, xPos));
            xPos = xPos + 4;
            break;
          case OperandType.InlineI8:
            xILOpCode = new ILOpCodes.OpInt64(xOpCodeVal, ReadUInt64(xIL, xPos));
            xPos = xPos + 8;
            break;

          case OperandType.ShortInlineR:
            xILOpCode = new ILOpCodes.OpSingle(xOpCodeVal, BitConverter.ToSingle(xIL, xPos));
            xPos = xPos + 4;
            break;
          case OperandType.InlineR:
            xILOpCode = new ILOpCodes.OpDouble(xOpCodeVal, BitConverter.ToDouble(xIL, xPos));
            xPos = xPos + 8;
            break;

          // The operand is a 32-bit metadata token.
          case OperandType.InlineField: {
              //TODO: Complete this section
              //public FieldInfo OperandValueField {
              //                mOperandValueField = mModule.ResolveField(OperandValueInt32,
              //                                                          xTypeGenArgs,
              //                                                          xMethodGenArgs);
              //        return mOperandValueField;
              xILOpCode = new ILOpCodes.OpField(xOpCodeVal, 0);
              xPos = xPos + 4;
              break;
            }

          // The operand is a 32-bit metadata token.
          case OperandType.InlineMethod: {
              var xValue = aMethod.Module.ResolveMethod((int)ReadUInt32(xIL, xPos), xTypeGenArgs, xMethodGenArgs);
              xILOpCode = new ILOpCodes.OpMethod(xOpCodeVal, xValue);
              xPos = xPos + 4;
              break;
            }

          // 32-bit metadata signature token.
          case OperandType.InlineSig:
            //TODO: Complete this section
            xILOpCode = new ILOpCodes.OpSig(xOpCodeVal, 0);
            xPos = xPos + 4;
            break;

          case OperandType.InlineString:
            xILOpCode = new ILOpCodes.OpString(xOpCodeVal, aMethod.Module.ResolveString((int)ReadUInt32(xIL, xPos)));
            xPos = xPos + 4;
            break;

          case OperandType.InlineSwitch: {
              //TODO: Complete this section
              int xCount = (int)ReadUInt32(xIL, xPos);
              int[] xBranchLocations = new int[xCount];
              uint[] xBranchValues = new uint[xCount];
              for (int i = 0; i < xCount; i++) {
                xBranchLocations[i] = xIL[xPos + i + 5];
                //xBranchValues[i] = 
                //                if ((mPosition + xBranchLocations1[i]) < 0) {
                //                    xResult[i] = (uint)xBranchLocations1[i];
                //                } else {
                //                    xResult[i] = (uint)(mPosition + xBranchLocations1[i]);
                //                }
              }
              xILOpCode = new ILOpCodes.OpSwitch(xOpCodeVal);
              xPos = xPos + 4 + xCount * 4;
              break;
            }

          // The operand is a FieldRef, MethodRef, or TypeRef token.
          case OperandType.InlineTok:
            //TODO: Complete this section
            xILOpCode = new ILOpCodes.OpToken(xOpCodeVal, 0);
            xPos = xPos + 4;
            break;

          // 32-bit metadata token.
          case OperandType.InlineType: {
              //TODO: Complete this section
              //                Type[] xTypeGenArgs = null;
              //                Type[] xMethodGenArgs = null;
              //                if (mMethod.DeclaringType.IsGenericType) {
              //                    xTypeGenArgs = mMethod.DeclaringType.GetGenericArguments();
              //                }
              //                if (mMethod.IsGenericMethod) {
              //                    xMethodGenArgs = mMethod.GetGenericArguments();
              //                }
              //                mOperandValueType = mModule.ResolveType(OperandValueInt32,
              //                                                        xTypeGenArgs,
              //                                                        xMethodGenArgs);
              //        return mOperandValueType;
              xILOpCode = new ILOpCodes.OpType(xOpCodeVal, null);
              xPos = xPos + 4;
              break;
            }

          case OperandType.ShortInlineVar:
            xILOpCode = new ILOpCodes.OpVar(xOpCodeVal, xIL[xPos]);
            xPos = xPos + 1;
            break;
          case OperandType.InlineVar:
            xILOpCode = new ILOpCodes.OpVar(xOpCodeVal, ReadUInt16(xIL, xPos));
            xPos = xPos + 2;
            break;

          default:
            throw new Exception("Unknown OperandType");
        }

        //TODO: Complete these shortcuts
        #region Expand shortcuts
        // This region expands shortcut ops into full ops
        // This elminates the amount of code required in the assemblers
        // by allowing them to ignore the shortcuts
        switch (xOpCodeVal) {
          case ILOpCode.Code.Beq_S:
            //TODO: xILOpCode = new ILOpCodes.xxx(ILOpCode.Code.Beq, xILOpCode.value);
            break;
          case ILOpCode.Code.Bge_S:
            //TODO: return Code.Bge;
            break;
          case ILOpCode.Code.Bge_Un_S:
            //return Code.Bge_Un;
            break;
          case ILOpCode.Code.Bgt_S:
            //TODO: return Code.Bgt;
            break;
          case ILOpCode.Code.Bgt_Un_S:
            //return Code.Bgt_Un;
            break;
          case ILOpCode.Code.Ble_S:
            //TODO: return Code.Ble;
            break;
          case ILOpCode.Code.Ble_Un_S:
            //TODO: return Code.Ble_Un;
            break;
          case ILOpCode.Code.Blt_S:
            //TODO: return Code.Blt;
            break;
          case ILOpCode.Code.Blt_Un_S:
            //TODO: return Code.Blt_Un;
            break;
          case ILOpCode.Code.Bne_Un_S:
            //TODO: return Code.Bne_Un;
            break;
          case ILOpCode.Code.Br_S:
            //TODO: return Code.Br;
            break;
          case ILOpCode.Code.Brfalse_S:
            //TODO: return Code.Brfalse;
            break;
          case ILOpCode.Code.Brtrue_S:
            //TODO: return Code.Brtrue;
            break;

          case ILOpCode.Code.Ldarg_0:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, 0);
            break;
          case ILOpCode.Code.Ldarg_1:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, 1);
            break;
          case ILOpCode.Code.Ldarg_2:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, 2);
            break;
          case ILOpCode.Code.Ldarg_3:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, 3);
            break;
          case ILOpCode.Code.Ldarg_S:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarg, ((ILOpCodes.OpVar)xILOpCode).Value);
            break;

          case ILOpCode.Code.Ldarga_S:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldarga, ((ILOpCodes.OpVar)xILOpCode).Value);
            break;

          case ILOpCode.Code.Ldc_I4_0:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 0);
            break;
          case ILOpCode.Code.Ldc_I4_1:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 1);
            break;
          case ILOpCode.Code.Ldc_I4_2:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 2);
            break;
          case ILOpCode.Code.Ldc_I4_3:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 3);
            break;
          case ILOpCode.Code.Ldc_I4_4:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 4);
            break;
          case ILOpCode.Code.Ldc_I4_5:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 5);
            break;
          case ILOpCode.Code.Ldc_I4_6:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 6);
            break;
          case ILOpCode.Code.Ldc_I4_7:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 7);
            break;
          case ILOpCode.Code.Ldc_I4_8:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 8);
            break;
          case ILOpCode.Code.Ldc_I4_M1:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, 0xFFFFFFFF);
            break;
          case ILOpCode.Code.Ldc_I4_S:
            xILOpCode = new ILOpCodes.OpInt(ILOpCode.Code.Ldc_I4, ((ILOpCodes.OpInt)xILOpCode).Value);
            break;

          case ILOpCode.Code.Ldloc_0:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, 0);
            break;
          case ILOpCode.Code.Ldloc_1:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, 1);
            break;
          case ILOpCode.Code.Ldloc_2:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, 2);
            break;
          case ILOpCode.Code.Ldloc_3:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, 3);
            break;
          case ILOpCode.Code.Ldloc_S:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloc, ((ILOpCodes.OpVar)xILOpCode).Value);
            break;

          case ILOpCode.Code.Ldloca_S:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Ldloca, ((ILOpCodes.OpVar)xILOpCode).Value);
            break;

          case ILOpCode.Code.Leave_S:
            //TODO: return Code.Leave;
            break;

          case ILOpCode.Code.Starg_S:
            //TODO: return Code.Starg;
            break;

          case ILOpCode.Code.Stloc_0:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, 0);
            break;
          case ILOpCode.Code.Stloc_1:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, 1);
            break;
          case ILOpCode.Code.Stloc_2:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, 2);
            break;
          case ILOpCode.Code.Stloc_3:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, 3);
            break;
          case ILOpCode.Code.Stloc_S:
            xILOpCode = new ILOpCodes.OpVar(ILOpCode.Code.Stloc, ((ILOpCodes.OpVar)xILOpCode).Value);
            break;
        }
#endregion

        xResult.Add(xILOpCode);
      }
      return xResult;
    }

    // We could use BitConvertor, unfortuantely they "hardcoded" endianness. Its fine for reading IL now...
    // but they essentially do the same as we do, just a bit slower.
    private UInt16 ReadUInt16(byte[] aBytes, int aPos) {
      return (UInt16)(aBytes[aPos + 1] << 8 | aBytes[aPos]);
    }

    private UInt32 ReadUInt32(byte[] aBytes, int aPos) {
      return (UInt32)(aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos]);
    }

    private UInt64 ReadUInt64(byte[] aBytes, int aPos) {
      return (UInt64)(
        aBytes[aPos + 7] << 56 | aBytes[aPos + 6] << 48 | aBytes[aPos + 5] << 40 | aBytes[aPos + 4] << 32
        | aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos]);
    }

  }
}