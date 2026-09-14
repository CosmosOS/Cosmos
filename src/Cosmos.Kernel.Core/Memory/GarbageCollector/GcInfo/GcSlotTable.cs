// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// Port of GcSlotDecoder::DecodeSlotTable from dotnet/runtime/src/coreclr/vm/gcinfodecoder.cpp.
// Decodes the GCInfo slot table (registers, then tracked stack slots, then untracked stack slots)
// eagerly into a caller-provided buffer. See issue #346.

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector.GcInfo;

/// <summary>Decoded GCInfo slot table for a single method. Backs onto caller-supplied storage.</summary>
internal unsafe struct GcSlotTable
{
    private GcSlotDesc* _slots;
    private uint _capacity;
    private uint _numSlots;       // registers + tracked stack + untracked stack
    private uint _numRegisters;
    private uint _numUntracked;
    private bool _overflowed;

    public readonly uint NumSlots => _numSlots;
    public readonly uint NumRegisters => _numRegisters;
    public readonly uint NumUntracked => _numUntracked;
    public readonly uint NumTracked => _numSlots - _numUntracked;
    /// <summary>True if the slot table did not fit in the supplied buffer (decode incomplete).</summary>
    public readonly bool Overflowed => _overflowed;

    public ref GcSlotDesc this[uint index] => ref _slots[index];

    /// <summary>
    /// Decode the slot table from <paramref name="reader"/> (positioned at the start of the slot table)
    /// into <paramref name="buffer"/>. The reader is advanced past the slot table even on overflow,
    /// so subsequent reads stay aligned with the C++ decoder.
    /// </summary>
    public void DecodeSlotTable(ref GcInfoBitStreamReader reader, GcSlotDesc* buffer, uint capacity)
    {
        _slots = buffer;
        _capacity = capacity;
        _overflowed = false;

        _numRegisters = reader.ReadOneFast() != 0
            ? (uint)reader.DecodeVarLengthUnsigned(GcInfoEncoding.NUM_REGISTERS_ENCBASE)
            : 0;

        uint numStackSlots;
        if (reader.ReadOneFast() != 0)
        {
            numStackSlots = (uint)reader.DecodeVarLengthUnsigned(GcInfoEncoding.NUM_STACK_SLOTS_ENCBASE);
            _numUntracked = (uint)reader.DecodeVarLengthUnsigned(GcInfoEncoding.NUM_UNTRACKED_SLOTS_ENCBASE);
        }
        else
        {
            numStackSlots = 0;
            _numUntracked = 0;
        }

        _numSlots = _numRegisters + numStackSlots + _numUntracked;

        if (_numSlots > _capacity)
        {
            _overflowed = true;
            Serial.WriteString("[GcInfo] slot table overflow: 0x");
            Serial.WriteHex(_numSlots);
            Serial.WriteString(" slots > buffer 0x");
            Serial.WriteHex(_capacity);
            Serial.WriteString("\n");
        }

        // --- Registers ---
        if (_numRegisters > 0)
        {
            uint normRegNum = (uint)reader.DecodeVarLengthUnsigned(GcInfoEncoding.REGISTER_ENCBASE);
            GcSlotFlags flags = (GcSlotFlags)reader.Read(2);
            Store(0, isRegister: true, normRegNum, 0, GcStackSlotBase.GC_CALLER_SP_REL, flags);

            for (uint i = 1; i < _numRegisters; i++)
            {
                if (flags != 0)
                {
                    normRegNum = (uint)reader.DecodeVarLengthUnsigned(GcInfoEncoding.REGISTER_ENCBASE);
                    flags = (GcSlotFlags)reader.Read(2);
                }
                else
                {
                    uint normRegDelta = (uint)reader.DecodeVarLengthUnsigned(GcInfoEncoding.REGISTER_DELTA_ENCBASE) + 1;
                    normRegNum += normRegDelta;
                }
                Store(i, isRegister: true, normRegNum, 0, GcStackSlotBase.GC_CALLER_SP_REL, flags);
            }
        }

        // --- Tracked stack slots ---
        if (numStackSlots > 0)
        {
            uint slotIndex = _numRegisters;
            GcStackSlotBase spBase = (GcStackSlotBase)reader.Read(2);
            int normSpOffset = (int)reader.DecodeVarLengthSigned(GcInfoEncoding.STACK_SLOT_ENCBASE);
            GcSlotFlags flags = (GcSlotFlags)reader.Read(2);
            Store(slotIndex, isRegister: false, 0, GcInfoEncoding.DenormalizeStackSlot(normSpOffset), spBase, flags);

            uint loopEnd = _numRegisters + numStackSlots;
            for (slotIndex++; slotIndex < loopEnd; slotIndex++)
            {
                spBase = (GcStackSlotBase)reader.Read(2);
                if (flags != 0)
                {
                    normSpOffset = (int)reader.DecodeVarLengthSigned(GcInfoEncoding.STACK_SLOT_ENCBASE);
                    flags = (GcSlotFlags)reader.Read(2);
                }
                else
                {
                    int normSpOffsetDelta = (int)reader.DecodeVarLengthUnsigned(GcInfoEncoding.STACK_SLOT_DELTA_ENCBASE);
                    normSpOffset += normSpOffsetDelta;
                }
                Store(slotIndex, isRegister: false, 0, GcInfoEncoding.DenormalizeStackSlot(normSpOffset), spBase, flags);
            }
        }

        // --- Untracked stack slots ---
        if (_numUntracked > 0)
        {
            uint slotIndex = _numRegisters + numStackSlots;
            GcStackSlotBase spBase = (GcStackSlotBase)reader.Read(2);
            int normSpOffset = (int)reader.DecodeVarLengthSigned(GcInfoEncoding.STACK_SLOT_ENCBASE);
            GcSlotFlags flags = (GcSlotFlags)reader.Read(2);
            Store(slotIndex, isRegister: false, 0, GcInfoEncoding.DenormalizeStackSlot(normSpOffset), spBase, flags);

            for (slotIndex++; slotIndex < _numSlots; slotIndex++)
            {
                spBase = (GcStackSlotBase)reader.Read(2);
                if (flags != 0)
                {
                    normSpOffset = (int)reader.DecodeVarLengthSigned(GcInfoEncoding.STACK_SLOT_ENCBASE);
                    flags = (GcSlotFlags)reader.Read(2);
                }
                else
                {
                    int normSpOffsetDelta = (int)reader.DecodeVarLengthUnsigned(GcInfoEncoding.STACK_SLOT_DELTA_ENCBASE);
                    normSpOffset += normSpOffsetDelta;
                }
                Store(slotIndex, isRegister: false, 0, GcInfoEncoding.DenormalizeStackSlot(normSpOffset), spBase, flags);
            }
        }
    }

    private void Store(uint index, bool isRegister, uint regNum, int spOffset, GcStackSlotBase spBase, GcSlotFlags flags)
    {
        if (index >= _capacity)
        {
            return;
        }
        ref GcSlotDesc s = ref _slots[index];
        s.IsRegister = isRegister;
        s.RegisterNumber = regNum;
        s.SpOffset = spOffset;
        s.StackBase = spBase;
        s.Flags = flags;
    }
}
