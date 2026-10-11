using Cosmos.Kernel.Core.Runtime.GcInfo;

namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

/// <summary>
/// Arch-neutral DWARF Call-Frame-Information interpreter. Parses CIE/FDE instruction streams into
/// per-register save-location rules on an <see cref="UnwindState"/>, then resolves those rules to
/// actual caller-side register values. The arch-specific knobs (register count, RA column, default
/// CFA at function entry, code alignment factor) live in <see cref="CfiArch"/>.
/// </summary>
internal static unsafe class DwarfCfiParser
{
    // DWARF CFI opcodes (DWARF 5 §6.4.2 — architecturally agnostic).
    private const byte DW_CFA_advance_loc = 0x40;       // 0x40 + delta (high 2 bits = 01)
    private const byte DW_CFA_offset = 0x80;            // 0x80 + reg   (high 2 bits = 10), then ULEB128 offset
    private const byte DW_CFA_restore = 0xC0;           // 0xC0 + reg   (high 2 bits = 11)
    private const byte DW_CFA_nop = 0x00;
    private const byte DW_CFA_set_loc = 0x01;
    private const byte DW_CFA_advance_loc1 = 0x02;
    private const byte DW_CFA_advance_loc2 = 0x03;
    private const byte DW_CFA_advance_loc4 = 0x04;
    private const byte DW_CFA_offset_extended = 0x05;
    private const byte DW_CFA_restore_extended = 0x06;
    private const byte DW_CFA_undefined = 0x07;
    private const byte DW_CFA_same_value = 0x08;
    private const byte DW_CFA_register = 0x09;
    private const byte DW_CFA_remember_state = 0x0A;
    private const byte DW_CFA_restore_state = 0x0B;
    private const byte DW_CFA_def_cfa = 0x0C;
    private const byte DW_CFA_def_cfa_register = 0x0D;
    private const byte DW_CFA_def_cfa_offset = 0x0E;
    private const byte DW_CFA_def_cfa_expression = 0x0F;
    private const byte DW_CFA_expression = 0x10;
    private const byte DW_CFA_offset_extended_sf = 0x11;
    private const byte DW_CFA_def_cfa_sf = 0x12;
    private const byte DW_CFA_def_cfa_offset_sf = 0x13;
    private const byte DW_CFA_val_offset = 0x14;
    private const byte DW_CFA_val_offset_sf = 0x15;
    private const byte DW_CFA_val_expression = 0x16;

    /// <summary>
    /// Decode a signed LEB128 value.
    /// </summary>
    internal static int ReadSLEB128(ref byte* p)
    {
        int result = 0;
        int shift = 0;
        byte b;

        do
        {
            b = *p++;
            result |= (b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        if (shift < 32 && (b & 0x40) != 0)
        {
            result |= ~0 << shift;
        }

        return result;
    }

    /// <summary>
    /// Decode a CIE (Common Information Entry): returns the code/data alignment factors, the
    /// return-address register, and the byte range of the CIE's "initial instructions" (which the
    /// caller will replay to seed the unwind state before applying the FDE's instructions).
    /// </summary>
    internal static bool ParseCIE(byte* cie, out int codeAlignFactor, out int dataAlignFactor,
                                  out byte returnAddressReg, out byte* initialInstructions, out byte* instructionsEnd)
    {
        codeAlignFactor = CfiArch.DefaultCodeAlignFactor;
        dataAlignFactor = -8;
        returnAddressReg = (byte)CfiArch.RaColumn;
        initialInstructions = null;
        instructionsEnd = null;

        byte* p = cie;

        uint length = *(uint*)p;
        if (length == 0 || length == 0xFFFFFFFF)
        {
            return false;
        }

        byte* cieEnd = p + 4 + length;
        p += 4;

        uint cieId = *(uint*)p;
        if (cieId != 0)
        {
            return false;   // Not a CIE.
        }
        p += 4;

        byte version = *p++;
        if (version != 1 && version != 3 && version != 4)
        {
            return false;
        }

        byte* augString = p;
        while (*p != 0)
        {
            p++;
        }
        p++;   // skip the augmentation string's null terminator

        // DWARF 4 added address_size + segment_selector_size between the augmentation string and the
        // alignment factors. ILC emits v3 on x64 and v4 on ARM64; accepting v4 on both is harmless.
        if (version == 4)
        {
            p++;   // address_size
            p++;   // segment_selector_size
        }

        codeAlignFactor = (int)MethodGcInfoLookup.ReadULEB128(ref p);
        dataAlignFactor = ReadSLEB128(ref p);

        // Version-1 CIEs encode the return-address register as a single byte; v3/v4 use ULEB128.
        if (version == 1)
        {
            returnAddressReg = *p++;
        }
        else
        {
            returnAddressReg = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
        }

        if (*augString == 'z')
        {
            uint augLen = MethodGcInfoLookup.ReadULEB128(ref p);
            p += augLen;
        }

        initialInstructions = p;
        instructionsEnd = cieEnd;
        return true;
    }

    /// <summary>
    /// Replay a stream of DWARF CFI instructions, updating <paramref name="state"/>'s CFA definition
    /// and per-register save-location rules. Stops once the synthetic PC has caught up with
    /// <paramref name="targetPC"/>; the resulting rules describe the register state at that PC.
    /// Out-of-range register numbers from a malformed FDE are silently dropped by
    /// <see cref="UnwindState.SetRegLocation"/>.
    /// </summary>
    internal static void ParseCFIInstructions(byte* instructions, byte* instructionsEnd,
                                              nuint pcBegin, nuint targetPC,
                                              int codeAlignFactor, int dataAlignFactor,
                                              ref UnwindState state)
    {
        byte* p = instructions;
        nuint currentPC = pcBegin;

        while (p < instructionsEnd && currentPC <= targetPC)
        {
            byte opcode = *p++;

            // The DW_CFA_advance_loc / DW_CFA_offset / DW_CFA_restore opcodes pack a 6-bit operand
            // into the high 2 bits of the byte; everything else uses the byte verbatim.
            byte highBits = (byte)(opcode & 0xC0);
            byte lowBits = (byte)(opcode & 0x3F);

            if (highBits == DW_CFA_advance_loc)
            {
                currentPC += (uint)(lowBits * codeAlignFactor);
            }
            else if (highBits == DW_CFA_offset)
            {
                byte reg = lowBits;
                uint offset = MethodGcInfoLookup.ReadULEB128(ref p);
                state.SetRegLocation(reg, RegSaveKind.AtCfaOffset, (int)(offset * dataAlignFactor));
            }
            else if (highBits == DW_CFA_restore)
            {
                byte reg = lowBits;
                state.SetRegLocation(reg, RegSaveKind.SameValue);
            }
            else
            {
                switch (opcode)
                {
                    case DW_CFA_nop:
                        break;

                    case DW_CFA_set_loc:
                        currentPC = *(nuint*)p;
                        p += sizeof(nuint);
                        break;

                    case DW_CFA_advance_loc1:
                        currentPC += (uint)(*p++ * codeAlignFactor);
                        break;

                    case DW_CFA_advance_loc2:
                        currentPC += (uint)(*(ushort*)p * codeAlignFactor);
                        p += 2;
                        break;

                    case DW_CFA_advance_loc4:
                        currentPC += (uint)(*(uint*)p * codeAlignFactor);
                        p += 4;
                        break;

                    case DW_CFA_def_cfa:
                        state.CfaRegister = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
                        state.CfaOffset = (int)MethodGcInfoLookup.ReadULEB128(ref p);
                        break;

                    case DW_CFA_def_cfa_register:
                        state.CfaRegister = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
                        break;

                    case DW_CFA_def_cfa_offset:
                        state.CfaOffset = (int)MethodGcInfoLookup.ReadULEB128(ref p);
                        break;

                    case DW_CFA_def_cfa_offset_sf:
                        state.CfaOffset = ReadSLEB128(ref p) * dataAlignFactor;
                        break;

                    case DW_CFA_offset_extended:
                    {
                        byte reg = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
                        uint offset = MethodGcInfoLookup.ReadULEB128(ref p);
                        state.SetRegLocation(reg, RegSaveKind.AtCfaOffset, (int)(offset * dataAlignFactor));
                    }
                    break;

                    case DW_CFA_offset_extended_sf:
                    {
                        byte reg = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
                        int offset = ReadSLEB128(ref p);
                        state.SetRegLocation(reg, RegSaveKind.AtCfaOffset, offset * dataAlignFactor);
                    }
                    break;

                    case DW_CFA_same_value:
                    {
                        byte reg = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
                        state.SetRegLocation(reg, RegSaveKind.SameValue);
                    }
                    break;

                    case DW_CFA_register:
                    {
                        byte reg = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
                        byte inReg = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
                        state.SetRegLocation(reg, RegSaveKind.InRegister, 0, inReg);
                    }
                    break;

                    case DW_CFA_undefined:
                    {
                        byte reg = (byte)MethodGcInfoLookup.ReadULEB128(ref p);
                        state.SetRegLocation(reg, RegSaveKind.Undefined);
                    }
                    break;

                    case DW_CFA_remember_state:
                    case DW_CFA_restore_state:
                        // Not used by ILC's emitted CFI; a rule-state stack would push/pop here.
                        break;

                    case DW_CFA_def_cfa_expression:
                    case DW_CFA_expression:
                    case DW_CFA_val_expression:
                    {
                        // DWARF expressions aren't evaluated — skip the length-prefixed body.
                        uint exprLen = MethodGcInfoLookup.ReadULEB128(ref p);
                        p += exprLen;
                    }
                    break;

                    default:
                        // Unknown opcode — best effort: skip; the next opcode boundary may still be valid.
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Resolve the accumulated CFI rules into actual caller-side register values. Computes the CFA,
    /// then for each register applies its <see cref="RegLocation"/> rule. Finally pins the caller
    /// SP slot to the CFA and exposes the caller's return address through
    /// <see cref="UnwindState.ReturnAddress"/>.
    /// </summary>
    internal static void ApplyUnwindRules(ref UnwindState state)
    {
        // CfaOffset is signed (DW_CFA_def_cfa_offset_sf and SLEB128 forms can be negative).
        long cfaBase = (long)state.GetRegValue(state.CfaRegister);
        nuint cfa = (nuint)(cfaBase + state.CfaOffset);

        for (int i = 0; i < CfiArch.RegCount; i++)
        {
            RegLocation* loc = state.GetRegLocation(i);

            switch (loc->Kind)
            {
                case RegSaveKind.AtCfaOffset:
                    nuint* savedLoc = (nuint*)((long)cfa + loc->Offset);
                    state.SetRegValue(i, *savedLoc);
                    break;

                case RegSaveKind.InRegister:
                    state.SetRegValue(i, state.GetRegValue(loc->Register));
                    break;

                case RegSaveKind.SameValue:
                case RegSaveKind.Undefined:
                default:
                    // Leave Regs[i] alone — same value, or its value at this PC is undefined.
                    break;
            }
        }

        // Standard CFI convention: the caller's SP equals the CFA we just resolved.
        state.SetRegValue(CfiArch.StackPointerReg, cfa);
        // After the loop, the return-address column holds the caller's IP — surface it explicitly.
        state.ReturnAddress = state.GetRegValue(CfiArch.RaColumn);
    }

    /// <summary>
    /// Reset every register's rule to <c>SameValue</c> (the callee-saved default). Shared by every
    /// site that builds a fresh <see cref="UnwindState"/> before any FDE rules are applied.
    /// </summary>
    internal static void InitRegRulesSameValue(ref UnwindState state)
    {
        for (int i = 0; i < CfiArch.RegCount; i++)
        {
            state.SetRegLocation(i, RegSaveKind.SameValue);
        }
    }
}
