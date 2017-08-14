using System;
using System.IO;

namespace Orvid.Compression
{
    public static class LZMA
    {
        #region Properties
        static int dictionary = 1 << 23;
        static bool eos = false;
        static CoderPropID[] propIDs = {
					CoderPropID.DictionarySize,
					CoderPropID.PosStateBits,
					CoderPropID.LitContextBits,
					CoderPropID.LitPosBits,
					CoderPropID.Algorithm,
					CoderPropID.NumFastBytes,
					CoderPropID.MatchFinder,
					CoderPropID.EndMarker
				};

        static object[] properties = {
					(Int32)(dictionary),
					(Int32)(2),
					(Int32)(3),
					(Int32)(0),
					(Int32)(2),
					(Int32)(128),
					"bt4",
					eos
				};
        #endregion

        public static byte[] Compress(byte[] inputBytes)
        {

            MemoryStream inStream = new MemoryStream(inputBytes);
            MemoryStream outStream = new MemoryStream();
            LZMAEncoder encoder = new LZMAEncoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            long fileSize = inStream.Length;
            for (int i = 0; i < 8; i++)
                outStream.WriteByte((Byte)(fileSize >> (8 * i)));
            encoder.Code(inStream, outStream, -1, -1, null);
            return outStream.ToArray();
        }

        public static byte[] Decompress(byte[] inputBytes)
        {
            MemoryStream newInStream = new MemoryStream(inputBytes);
            LZMADecoder decoder = new LZMADecoder();

            newInStream.Seek(0, 0);
            MemoryStream newOutStream = new MemoryStream();

            byte[] properties2 = new byte[5];
            if (newInStream.Read(properties2, 0, 5) != 5)
                throw (new Exception("input .lzma is too short"));
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = newInStream.ReadByte();
                if (v < 0)
                    throw (new Exception("Can't Read 1"));
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties2);

            long compressedSize = newInStream.Length - newInStream.Position;
            decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);

            byte[] b = newOutStream.ToArray();

            return b;
        }


        #region LZMA

        #region LZMABase
        private abstract class LZMABase
        {
            public const uint kNumRepDistances = 4;
            public const uint kNumStates = 12;

            public struct State
            {
                public uint Index;
                public void Init() { Index = 0; }
                public void UpdateChar()
                {
                    if (Index < 4) Index = 0;
                    else if (Index < 10) Index -= 3;
                    else Index -= 6;
                }
                public void UpdateMatch() { Index = (uint)(Index < 7 ? 7 : 10); }
                public void UpdateRep() { Index = (uint)(Index < 7 ? 8 : 11); }
                public void UpdateShortRep() { Index = (uint)(Index < 7 ? 9 : 11); }
                public bool IsCharState() { return Index < 7; }
            }

            public const int kNumPosSlotBits = 6;
            public const int kDicLogSizeMin = 0;

            public const int kNumLenToPosStatesBits = 2;
            public const uint kNumLenToPosStates = 1 << kNumLenToPosStatesBits;

            public const uint kMatchMinLen = 2;

            public static uint GetLenToPosState(uint len)
            {
                len -= kMatchMinLen;
                if (len < kNumLenToPosStates)
                    return len;
                return (uint)(kNumLenToPosStates - 1);
            }

            public const int kNumAlignBits = 4;
            public const uint kAlignTableSize = 1 << kNumAlignBits;
            public const uint kAlignMask = (kAlignTableSize - 1);

            public const uint kStartPosModelIndex = 4;
            public const uint kEndPosModelIndex = 14;
            public const uint kNumPosModels = kEndPosModelIndex - kStartPosModelIndex;

            public const uint kNumFullDistances = 1 << ((int)kEndPosModelIndex / 2);

            public const uint kNumLitPosStatesBitsEncodingMax = 4;
            public const uint kNumLitContextBitsMax = 8;

            public const int kNumPosStatesBitsMax = 4;
            public const uint kNumPosStatesMax = (1 << kNumPosStatesBitsMax);
            public const int kNumPosStatesBitsEncodingMax = 4;
            public const uint kNumPosStatesEncodingMax = (1 << kNumPosStatesBitsEncodingMax);

            public const int kNumLowLenBits = 3;
            public const int kNumMidLenBits = 3;
            public const int kNumHighLenBits = 8;
            public const uint kNumLowLenSymbols = 1 << kNumLowLenBits;
            public const uint kNumMidLenSymbols = 1 << kNumMidLenBits;
            public const uint kNumLenSymbols = kNumLowLenSymbols + kNumMidLenSymbols + (1 << kNumHighLenBits);
            public const uint kMatchMaxLen = kMatchMinLen + kNumLenSymbols - 1;
        }
        #endregion

        #region LZMADecoder
        private class LZMADecoder : ICoder
        {
            class LenDecoder
            {
                LZMARangeBitDecoder m_Choice = new LZMARangeBitDecoder();
                LZMARangeBitDecoder m_Choice2 = new LZMARangeBitDecoder();
                LZMARangeBitTreeDecoder[] m_LowCoder = new LZMARangeBitTreeDecoder[LZMABase.kNumPosStatesMax];
                LZMARangeBitTreeDecoder[] m_MidCoder = new LZMARangeBitTreeDecoder[LZMABase.kNumPosStatesMax];
                LZMARangeBitTreeDecoder m_HighCoder = new LZMARangeBitTreeDecoder(LZMABase.kNumHighLenBits);
                uint m_NumPosStates = 0;

                public void Create(uint numPosStates)
                {
                    for (uint posState = m_NumPosStates; posState < numPosStates; posState++)
                    {
                        m_LowCoder[posState] = new LZMARangeBitTreeDecoder(LZMABase.kNumLowLenBits);
                        m_MidCoder[posState] = new LZMARangeBitTreeDecoder(LZMABase.kNumMidLenBits);
                    }
                    m_NumPosStates = numPosStates;
                }

                public void Init()
                {
                    m_Choice.Init();
                    for (uint posState = 0; posState < m_NumPosStates; posState++)
                    {
                        m_LowCoder[posState].Init();
                        m_MidCoder[posState].Init();
                    }
                    m_Choice2.Init();
                    m_HighCoder.Init();
                }

                public uint Decode(LZMARangeDecoder rangeDecoder, uint posState)
                {
                    if (m_Choice.Decode(rangeDecoder) == 0)
                        return m_LowCoder[posState].Decode(rangeDecoder);
                    else
                    {
                        uint symbol = LZMABase.kNumLowLenSymbols;
                        if (m_Choice2.Decode(rangeDecoder) == 0)
                            symbol += m_MidCoder[posState].Decode(rangeDecoder);
                        else
                        {
                            symbol += LZMABase.kNumMidLenSymbols;
                            symbol += m_HighCoder.Decode(rangeDecoder);
                        }
                        return symbol;
                    }
                }
            }

            class LiteralDecoder
            {
                struct Decoder2
                {
                    LZMARangeBitDecoder[] m_Decoders;
                    public void Create() { m_Decoders = new LZMARangeBitDecoder[0x300]; }
                    public void Init() { for (int i = 0; i < 0x300; i++) m_Decoders[i].Init(); }

                    public byte DecodeNormal(LZMARangeDecoder rangeDecoder)
                    {
                        uint symbol = 1;
                        do
                            symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
                        while (symbol < 0x100);
                        return (byte)symbol;
                    }

                    public byte DecodeWithMatchByte(LZMARangeDecoder rangeDecoder, byte matchByte)
                    {
                        uint symbol = 1;
                        do
                        {
                            uint matchBit = (uint)(matchByte >> 7) & 1;
                            matchByte <<= 1;
                            uint bit = m_Decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
                            symbol = (symbol << 1) | bit;
                            if (matchBit != bit)
                            {
                                while (symbol < 0x100)
                                    symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
                                break;
                            }
                        }
                        while (symbol < 0x100);
                        return (byte)symbol;
                    }
                }

                Decoder2[] m_Coders;
                int m_NumPrevBits;
                int m_NumPosBits;
                uint m_PosMask;

                public void Create(int numPosBits, int numPrevBits)
                {
                    if (m_Coders != null && m_NumPrevBits == numPrevBits &&
                        m_NumPosBits == numPosBits)
                        return;
                    m_NumPosBits = numPosBits;
                    m_PosMask = ((uint)1 << numPosBits) - 1;
                    m_NumPrevBits = numPrevBits;
                    uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
                    m_Coders = new Decoder2[numStates];
                    for (uint i = 0; i < numStates; i++)
                        m_Coders[i].Create();
                }

                public void Init()
                {
                    uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);

                    for (uint i = 0; i < numStates; i++)
                    {

                        m_Coders[i].Init();

                    }
                }

                uint GetState(uint pos, byte prevByte)
                { return ((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> (8 - m_NumPrevBits)); }

                public byte DecodeNormal(LZMARangeDecoder rangeDecoder, uint pos, byte prevByte)
                { return m_Coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder); }

                public byte DecodeWithMatchByte(LZMARangeDecoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
                { return m_Coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte); }
            };

            LZOutWindow m_OutWindow = new LZOutWindow();
            LZMARangeDecoder m_RangeDecoder = new LZMARangeDecoder();

            LZMARangeBitDecoder[] m_IsMatchDecoders = new LZMARangeBitDecoder[LZMABase.kNumStates << LZMABase.kNumPosStatesBitsMax];
            LZMARangeBitDecoder[] m_IsRepDecoders = new LZMARangeBitDecoder[LZMABase.kNumStates];
            LZMARangeBitDecoder[] m_IsRepG0Decoders = new LZMARangeBitDecoder[LZMABase.kNumStates];
            LZMARangeBitDecoder[] m_IsRepG1Decoders = new LZMARangeBitDecoder[LZMABase.kNumStates];
            LZMARangeBitDecoder[] m_IsRepG2Decoders = new LZMARangeBitDecoder[LZMABase.kNumStates];
            LZMARangeBitDecoder[] m_IsRep0LongDecoders = new LZMARangeBitDecoder[LZMABase.kNumStates << LZMABase.kNumPosStatesBitsMax];

            LZMARangeBitTreeDecoder[] m_PosSlotDecoder = new LZMARangeBitTreeDecoder[LZMABase.kNumLenToPosStates];
            LZMARangeBitDecoder[] m_PosDecoders = new LZMARangeBitDecoder[LZMABase.kNumFullDistances - LZMABase.kEndPosModelIndex];

            LZMARangeBitTreeDecoder m_PosAlignDecoder = new LZMARangeBitTreeDecoder(LZMABase.kNumAlignBits);

            LenDecoder m_LenDecoder = new LenDecoder();
            LenDecoder m_RepLenDecoder = new LenDecoder();

            LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

            uint m_DictionarySize;
            uint m_DictionarySizeCheck;

            uint m_PosStateMask;

            public LZMADecoder()
            {
                m_DictionarySize = 0xFFFFFFFF;
                for (int i = 0; i < LZMABase.kNumLenToPosStates; i++)
                    m_PosSlotDecoder[i] = new LZMARangeBitTreeDecoder(LZMABase.kNumPosSlotBits);
            }

            void SetDictionarySize(uint dictionarySize)
            {
                if (m_DictionarySize != dictionarySize)
                {
                    m_DictionarySize = dictionarySize;
                    m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1);
                    uint blockSize = Math.Max(m_DictionarySizeCheck, (1 << 12));
                    m_OutWindow.Create(blockSize);
                }
            }

            void SetLiteralProperties(int lp, int lc)
            {
                if (lp > 8)
                    throw new Exception();
                if (lc > 8)
                    throw new Exception();
                m_LiteralDecoder.Create(lp, lc);
            }

            void SetPosBitsProperties(int pb)
            {
                if (pb > LZMABase.kNumPosStatesBitsMax)
                    throw new Exception();
                uint numPosStates = (uint)1 << pb;
                m_LenDecoder.Create(numPosStates);
                m_RepLenDecoder.Create(numPosStates);
                m_PosStateMask = numPosStates - 1;
            }

            void Init(System.IO.Stream inStream, System.IO.Stream outStream)
            {
                m_RangeDecoder.Init(inStream);
                m_OutWindow.Init(outStream);

                uint i;
                for (i = 0; i < LZMABase.kNumStates; i++)
                {
                    for (uint j = 0; j <= m_PosStateMask; j++)
                    {
                        uint index = (i << LZMABase.kNumPosStatesBitsMax) + j;
                        m_IsMatchDecoders[index].Init();
                        m_IsRep0LongDecoders[index].Init();
                    }
                    m_IsRepDecoders[i].Init();
                    m_IsRepG0Decoders[i].Init();
                    m_IsRepG1Decoders[i].Init();
                    m_IsRepG2Decoders[i].Init();
                }

                m_LiteralDecoder.Init();
                for (i = 0; i < LZMABase.kNumLenToPosStates; i++)
                    m_PosSlotDecoder[i].Init();
                // m_PosSpecDecoder.Init();
                for (i = 0; i < LZMABase.kNumFullDistances - LZMABase.kEndPosModelIndex; i++)
                    m_PosDecoders[i].Init();

                m_LenDecoder.Init();
                m_RepLenDecoder.Init();
                m_PosAlignDecoder.Init();
            }

            public override void Code(System.IO.Stream inStream, System.IO.Stream outStream, Int64 inSize, Int64 outSize, ICodeProgress progress)
            {
                Init(inStream, outStream);

                LZMABase.State state = new LZMABase.State();
                state.Init();
                uint rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;

                UInt64 nowPos64 = 0;
                UInt64 outSize64 = (UInt64)outSize;
                if (nowPos64 < outSize64)
                {
                    if (m_IsMatchDecoders[state.Index << LZMABase.kNumPosStatesBitsMax].Decode(m_RangeDecoder) != 0)
                        throw new Exception();
                    state.UpdateChar();
                    byte b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, 0, 0);
                    m_OutWindow.PutByte(b);
                    nowPos64++;
                }
                while (nowPos64 < outSize64)
                {
                    // UInt64 next = Math.Min(nowPos64 + (1 << 18), outSize64);
                    // while(nowPos64 < next)
                    {
                        uint posState = (uint)nowPos64 & m_PosStateMask;
                        if (m_IsMatchDecoders[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) == 0)
                        {
                            byte b;
                            byte prevByte = m_OutWindow.GetByte(0);
                            if (!state.IsCharState())
                                b = m_LiteralDecoder.DecodeWithMatchByte(m_RangeDecoder,
                                    (uint)nowPos64, prevByte, m_OutWindow.GetByte(rep0));
                            else
                                b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, (uint)nowPos64, prevByte);
                            m_OutWindow.PutByte(b);
                            state.UpdateChar();
                            nowPos64++;
                        }
                        else
                        {
                            uint len;
                            if (m_IsRepDecoders[state.Index].Decode(m_RangeDecoder) == 1)
                            {
                                if (m_IsRepG0Decoders[state.Index].Decode(m_RangeDecoder) == 0)
                                {
                                    if (m_IsRep0LongDecoders[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) == 0)
                                    {
                                        state.UpdateShortRep();
                                        m_OutWindow.PutByte(m_OutWindow.GetByte(rep0));
                                        nowPos64++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    UInt32 distance;
                                    if (m_IsRepG1Decoders[state.Index].Decode(m_RangeDecoder) == 0)
                                    {
                                        distance = rep1;
                                    }
                                    else
                                    {
                                        if (m_IsRepG2Decoders[state.Index].Decode(m_RangeDecoder) == 0)
                                            distance = rep2;
                                        else
                                        {
                                            distance = rep3;
                                            rep3 = rep2;
                                        }
                                        rep2 = rep1;
                                    }
                                    rep1 = rep0;
                                    rep0 = distance;
                                }
                                len = m_RepLenDecoder.Decode(m_RangeDecoder, posState) + LZMABase.kMatchMinLen;
                                state.UpdateRep();
                            }
                            else
                            {
                                rep3 = rep2;
                                rep2 = rep1;
                                rep1 = rep0;
                                len = LZMABase.kMatchMinLen + m_LenDecoder.Decode(m_RangeDecoder, posState);
                                state.UpdateMatch();
                                uint posSlot = m_PosSlotDecoder[LZMABase.GetLenToPosState(len)].Decode(m_RangeDecoder);
                                if (posSlot >= LZMABase.kStartPosModelIndex)
                                {
                                    int numDirectBits = (int)((posSlot >> 1) - 1);
                                    rep0 = ((2 | (posSlot & 1)) << numDirectBits);
                                    if (posSlot < LZMABase.kEndPosModelIndex)
                                        rep0 += LZMARangeBitTreeDecoder.ReverseDecode(m_PosDecoders,
                                                rep0 - posSlot - 1, m_RangeDecoder, numDirectBits);
                                    else
                                    {
                                        rep0 += (m_RangeDecoder.DecodeDirectBits(
                                            numDirectBits - LZMABase.kNumAlignBits) << LZMABase.kNumAlignBits);
                                        rep0 += m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
                                    }
                                }
                                else
                                    rep0 = posSlot;
                            }
                            if (rep0 >= nowPos64 || rep0 >= m_DictionarySizeCheck)
                            {
                                if (rep0 == 0xFFFFFFFF)
                                    break;
                                throw new Exception();
                            }
                            m_OutWindow.CopyBlock(rep0, len);
                            nowPos64 += len;
                        }
                    }
                }
                m_OutWindow.Flush();
                m_OutWindow.ReleaseStream();
                m_RangeDecoder.ReleaseStream();
            }

            public void SetDecoderProperties(byte[] properties)
            {
                if (properties.Length < 5)
                    throw new Exception();
                int lc = properties[0] % 9;
                int remainder = properties[0] / 9;
                int lp = remainder % 5;
                int pb = remainder / 5;
                if (pb > LZMABase.kNumPosStatesBitsMax)
                    throw new Exception();
                UInt32 dictionarySize = 0;
                for (int i = 0; i < 4; i++)
                    dictionarySize += ((UInt32)(properties[1 + i])) << (i * 8);
                SetDictionarySize(dictionarySize);
                SetLiteralProperties(lp, lc);
                SetPosBitsProperties(pb);
            }
        }
        #endregion

        #region LZMAEncoder
        private class LZMAEncoder : ICoder
        {
            enum EMatchFinderType
            {
                BT2,
                BT4,
            };

            const UInt32 kIfinityPrice = 0xFFFFFFF;

            static Byte[] g_FastPos = new Byte[1 << 11];

            static LZMAEncoder()
            {
                const Byte kFastSlots = 22;
                int c = 2;
                g_FastPos[0] = 0;
                g_FastPos[1] = 1;
                for (Byte slotFast = 2; slotFast < kFastSlots; slotFast++)
                {
                    UInt32 k = ((UInt32)1 << ((slotFast >> 1) - 1));
                    for (UInt32 j = 0; j < k; j++, c++)
                        g_FastPos[c] = slotFast;
                }
            }

            static UInt32 GetPosSlot(UInt32 pos)
            {
                if (pos < (1 << 11))
                    return g_FastPos[pos];
                if (pos < (1 << 21))
                    return (UInt32)(g_FastPos[pos >> 10] + 20);
                return (UInt32)(g_FastPos[pos >> 20] + 40);
            }

            static UInt32 GetPosSlot2(UInt32 pos)
            {
                if (pos < (1 << 17))
                    return (UInt32)(g_FastPos[pos >> 6] + 12);
                if (pos < (1 << 27))
                    return (UInt32)(g_FastPos[pos >> 16] + 32);
                return (UInt32)(g_FastPos[pos >> 26] + 52);
            }

            LZMABase.State _state = new LZMABase.State();
            Byte _previousByte;
            UInt32[] _repDistances = new UInt32[LZMABase.kNumRepDistances];

            void BaseInit()
            {
                _state.Init();
                _previousByte = 0;
                for (UInt32 i = 0; i < LZMABase.kNumRepDistances; i++)
                    _repDistances[i] = 0;
            }

            const int kDefaultDictionaryLogSize = 22;
            const UInt32 kNumFastBytesDefault = 0x20;

            class LiteralEncoder
            {
                public struct Encoder2
                {
                    LZMARangeBitEncoder[] m_Encoders;

                    public void Create() { m_Encoders = new LZMARangeBitEncoder[0x300]; }

                    public void Init() { for (int i = 0; i < 0x300; i++) m_Encoders[i].Init(); }

                    public void Encode(LZMARangeEncoder rangeEncoder, byte symbol)
                    {
                        uint context = 1;
                        for (int i = 7; i >= 0; i--)
                        {
                            uint bit = (uint)((symbol >> i) & 1);
                            m_Encoders[context].Encode(rangeEncoder, bit);
                            context = (context << 1) | bit;
                        }
                    }

                    public void EncodeMatched(LZMARangeEncoder rangeEncoder, byte matchByte, byte symbol)
                    {
                        uint context = 1;
                        bool same = true;
                        for (int i = 7; i >= 0; i--)
                        {
                            uint bit = (uint)((symbol >> i) & 1);
                            uint state = context;
                            if (same)
                            {
                                uint matchBit = (uint)((matchByte >> i) & 1);
                                state += ((1 + matchBit) << 8);
                                same = (matchBit == bit);
                            }
                            m_Encoders[state].Encode(rangeEncoder, bit);
                            context = (context << 1) | bit;
                        }
                    }

                    public uint GetPrice(bool matchMode, byte matchByte, byte symbol)
                    {
                        uint price = 0;
                        uint context = 1;
                        int i = 7;
                        if (matchMode)
                        {
                            for (; i >= 0; i--)
                            {
                                uint matchBit = (uint)(matchByte >> i) & 1;
                                uint bit = (uint)(symbol >> i) & 1;
                                price += m_Encoders[((1 + matchBit) << 8) + context].GetPrice(bit);
                                context = (context << 1) | bit;
                                if (matchBit != bit)
                                {
                                    i--;
                                    break;
                                }
                            }
                        }
                        for (; i >= 0; i--)
                        {
                            uint bit = (uint)(symbol >> i) & 1;
                            price += m_Encoders[context].GetPrice(bit);
                            context = (context << 1) | bit;
                        }
                        return price;
                    }
                }

                Encoder2[] m_Coders;
                int m_NumPrevBits;
                int m_NumPosBits;
                uint m_PosMask;

                public void Create(int numPosBits, int numPrevBits)
                {
                    if (m_Coders != null && m_NumPrevBits == numPrevBits && m_NumPosBits == numPosBits)
                        return;
                    m_NumPosBits = numPosBits;
                    m_PosMask = ((uint)1 << numPosBits) - 1;
                    m_NumPrevBits = numPrevBits;
                    uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
                    m_Coders = new Encoder2[numStates];
                    for (uint i = 0; i < numStates; i++)
                        m_Coders[i].Create();
                }

                public void Init()
                {
                    uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
                    for (uint i = 0; i < numStates; i++)
                        m_Coders[i].Init();
                }

                public Encoder2 GetSubCoder(UInt32 pos, Byte prevByte)
                { return m_Coders[((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> (8 - m_NumPrevBits))]; }
            }

            class LenEncoder
            {
                LZMARangeBitEncoder _choice = new LZMARangeBitEncoder();
                LZMARangeBitEncoder _choice2 = new LZMARangeBitEncoder();
                LZMARangeBitTreeEncoder[] _lowCoder = new LZMARangeBitTreeEncoder[LZMABase.kNumPosStatesEncodingMax];
                LZMARangeBitTreeEncoder[] _midCoder = new LZMARangeBitTreeEncoder[LZMABase.kNumPosStatesEncodingMax];
                LZMARangeBitTreeEncoder _highCoder = new LZMARangeBitTreeEncoder(LZMABase.kNumHighLenBits);

                public LenEncoder()
                {
                    for (UInt32 posState = 0; posState < LZMABase.kNumPosStatesEncodingMax; posState++)
                    {
                        _lowCoder[posState] = new LZMARangeBitTreeEncoder(LZMABase.kNumLowLenBits);
                        _midCoder[posState] = new LZMARangeBitTreeEncoder(LZMABase.kNumMidLenBits);
                    }
                }

                public void Init(UInt32 numPosStates)
                {
                    _choice.Init();
                    _choice2.Init();
                    for (UInt32 posState = 0; posState < numPosStates; posState++)
                    {
                        _lowCoder[posState].Init();
                        _midCoder[posState].Init();
                    }
                    _highCoder.Init();
                }

                public void Encode(LZMARangeEncoder rangeEncoder, UInt32 symbol, UInt32 posState)
                {
                    if (symbol < LZMABase.kNumLowLenSymbols)
                    {
                        _choice.Encode(rangeEncoder, 0);
                        _lowCoder[posState].Encode(rangeEncoder, symbol);
                    }
                    else
                    {
                        symbol -= LZMABase.kNumLowLenSymbols;
                        _choice.Encode(rangeEncoder, 1);
                        if (symbol < LZMABase.kNumMidLenSymbols)
                        {
                            _choice2.Encode(rangeEncoder, 0);
                            _midCoder[posState].Encode(rangeEncoder, symbol);
                        }
                        else
                        {
                            _choice2.Encode(rangeEncoder, 1);
                            _highCoder.Encode(rangeEncoder, symbol - LZMABase.kNumMidLenSymbols);
                        }
                    }
                }

                public void SetPrices(UInt32 posState, UInt32 numSymbols, UInt32[] prices, UInt32 st)
                {
                    UInt32 a0 = _choice.GetPrice0();
                    UInt32 a1 = _choice.GetPrice1();
                    UInt32 b0 = a1 + _choice2.GetPrice0();
                    UInt32 b1 = a1 + _choice2.GetPrice1();
                    UInt32 i = 0;
                    for (i = 0; i < LZMABase.kNumLowLenSymbols; i++)
                    {
                        if (i >= numSymbols)
                            return;
                        prices[st + i] = a0 + _lowCoder[posState].GetPrice(i);
                    }
                    for (; i < LZMABase.kNumLowLenSymbols + LZMABase.kNumMidLenSymbols; i++)
                    {
                        if (i >= numSymbols)
                            return;
                        prices[st + i] = b0 + _midCoder[posState].GetPrice(i - LZMABase.kNumLowLenSymbols);
                    }
                    for (; i < numSymbols; i++)
                        prices[st + i] = b1 + _highCoder.GetPrice(i - LZMABase.kNumLowLenSymbols - LZMABase.kNumMidLenSymbols);
                }
            };

            const UInt32 kNumLenSpecSymbols = LZMABase.kNumLowLenSymbols + LZMABase.kNumMidLenSymbols;

            class LenPriceTableEncoder : LenEncoder
            {
                UInt32[] _prices = new UInt32[LZMABase.kNumLenSymbols << LZMABase.kNumPosStatesBitsEncodingMax];
                UInt32 _tableSize;
                UInt32[] _counters = new UInt32[LZMABase.kNumPosStatesEncodingMax];

                public void SetTableSize(UInt32 tableSize) { _tableSize = tableSize; }

                public UInt32 GetPrice(UInt32 symbol, UInt32 posState)
                {
                    return _prices[posState * LZMABase.kNumLenSymbols + symbol];
                }

                void UpdateTable(UInt32 posState)
                {
                    SetPrices(posState, _tableSize, _prices, posState * LZMABase.kNumLenSymbols);
                    _counters[posState] = _tableSize;
                }

                public void UpdateTables(UInt32 numPosStates)
                {
                    for (UInt32 posState = 0; posState < numPosStates; posState++)
                        UpdateTable(posState);
                }

                public new void Encode(LZMARangeEncoder rangeEncoder, UInt32 symbol, UInt32 posState)
                {
                    base.Encode(rangeEncoder, symbol, posState);
                    if (--_counters[posState] == 0)
                        UpdateTable(posState);
                }
            }

            const UInt32 kNumOpts = 1 << 12;
            class Optimal
            {
                public LZMABase.State State;

                public bool Prev1IsChar;
                public bool Prev2;

                public UInt32 PosPrev2;
                public UInt32 BackPrev2;

                public UInt32 Price;
                public UInt32 PosPrev;
                public UInt32 BackPrev;

                public UInt32 Backs0;
                public UInt32 Backs1;
                public UInt32 Backs2;
                public UInt32 Backs3;

                public void MakeAsChar() { BackPrev = 0xFFFFFFFF; Prev1IsChar = false; }
                public void MakeAsShortRep() { BackPrev = 0; ; Prev1IsChar = false; }
                public bool IsShortRep() { return (BackPrev == 0); }
            };
            Optimal[] _optimum = new Optimal[kNumOpts];
            LZBinTree _matchFinder = null;
            LZMARangeEncoder _rangeEncoder = new LZMARangeEncoder();

            LZMARangeBitEncoder[] _isMatch = new LZMARangeBitEncoder[LZMABase.kNumStates << LZMABase.kNumPosStatesBitsMax];
            LZMARangeBitEncoder[] _isRep = new LZMARangeBitEncoder[LZMABase.kNumStates];
            LZMARangeBitEncoder[] _isRepG0 = new LZMARangeBitEncoder[LZMABase.kNumStates];
            LZMARangeBitEncoder[] _isRepG1 = new LZMARangeBitEncoder[LZMABase.kNumStates];
            LZMARangeBitEncoder[] _isRepG2 = new LZMARangeBitEncoder[LZMABase.kNumStates];
            LZMARangeBitEncoder[] _isRep0Long = new LZMARangeBitEncoder[LZMABase.kNumStates << LZMABase.kNumPosStatesBitsMax];

            LZMARangeBitTreeEncoder[] _posSlotEncoder = new LZMARangeBitTreeEncoder[LZMABase.kNumLenToPosStates];

            LZMARangeBitEncoder[] _posEncoders = new LZMARangeBitEncoder[LZMABase.kNumFullDistances - LZMABase.kEndPosModelIndex];
            LZMARangeBitTreeEncoder _posAlignEncoder = new LZMARangeBitTreeEncoder(LZMABase.kNumAlignBits);

            LenPriceTableEncoder _lenEncoder = new LenPriceTableEncoder();
            LenPriceTableEncoder _repMatchLenEncoder = new LenPriceTableEncoder();

            LiteralEncoder _literalEncoder = new LiteralEncoder();

            UInt32[] _matchDistances = new UInt32[LZMABase.kMatchMaxLen * 2 + 2];

            UInt32 _numFastBytes = kNumFastBytesDefault;
            UInt32 _longestMatchLength;
            UInt32 _numDistancePairs;

            UInt32 _additionalOffset;

            UInt32 _optimumEndIndex;
            UInt32 _optimumCurrentIndex;

            bool _longestMatchWasFound;

            UInt32[] _posSlotPrices = new UInt32[1 << (LZMABase.kNumPosSlotBits + LZMABase.kNumLenToPosStatesBits)];
            UInt32[] _distancesPrices = new UInt32[LZMABase.kNumFullDistances << LZMABase.kNumLenToPosStatesBits];
            UInt32[] _alignPrices = new UInt32[LZMABase.kAlignTableSize];
            UInt32 _alignPriceCount;

            UInt32 _distTableSize = (kDefaultDictionaryLogSize * 2);

            int _posStateBits = 2;
            UInt32 _posStateMask = (4 - 1);
            int _numLiteralPosStateBits = 0;
            int _numLiteralContextBits = 3;

            UInt32 _dictionarySize = (1 << kDefaultDictionaryLogSize);
            UInt32 _dictionarySizePrev = 0xFFFFFFFF;
            UInt32 _numFastBytesPrev = 0xFFFFFFFF;

            Int64 nowPos64;
            bool _finished;
            System.IO.Stream _inStream;

            EMatchFinderType _matchFinderType = EMatchFinderType.BT4;
            bool _writeEndMark = false;

            bool _needReleaseMFStream;

            void Create()
            {
                if (_matchFinder == null)
                {
                    LZBinTree bt = new LZBinTree();
                    int numHashBytes = 4;
                    if (_matchFinderType == EMatchFinderType.BT2)
                        numHashBytes = 2;
                    bt.SetType(numHashBytes);
                    _matchFinder = bt;
                }
                _literalEncoder.Create(_numLiteralPosStateBits, _numLiteralContextBits);

                if (_dictionarySize == _dictionarySizePrev && _numFastBytesPrev == _numFastBytes)
                    return;
                _matchFinder.Create(_dictionarySize, kNumOpts, _numFastBytes, LZMABase.kMatchMaxLen + 1);
                _dictionarySizePrev = _dictionarySize;
                _numFastBytesPrev = _numFastBytes;
            }

            public LZMAEncoder()
            {
                for (int i = 0; i < kNumOpts; i++)
                    _optimum[i] = new Optimal();
                for (int i = 0; i < LZMABase.kNumLenToPosStates; i++)
                    _posSlotEncoder[i] = new LZMARangeBitTreeEncoder(LZMABase.kNumPosSlotBits);
            }

            void SetWriteEndMarkerMode(bool writeEndMarker)
            {
                _writeEndMark = writeEndMarker;
            }

            void Init()
            {
                BaseInit();
                _rangeEncoder.Init();

                uint i;
                for (i = 0; i < LZMABase.kNumStates; i++)
                {
                    for (uint j = 0; j <= _posStateMask; j++)
                    {
                        uint complexState = (i << LZMABase.kNumPosStatesBitsMax) + j;
                        _isMatch[complexState].Init();
                        _isRep0Long[complexState].Init();
                    }
                    _isRep[i].Init();
                    _isRepG0[i].Init();
                    _isRepG1[i].Init();
                    _isRepG2[i].Init();
                }
                _literalEncoder.Init();
                for (i = 0; i < LZMABase.kNumLenToPosStates; i++)
                    _posSlotEncoder[i].Init();
                for (i = 0; i < LZMABase.kNumFullDistances - LZMABase.kEndPosModelIndex; i++)
                    _posEncoders[i].Init();

                _lenEncoder.Init((UInt32)1 << _posStateBits);
                _repMatchLenEncoder.Init((UInt32)1 << _posStateBits);

                _posAlignEncoder.Init();

                _longestMatchWasFound = false;
                _optimumEndIndex = 0;
                _optimumCurrentIndex = 0;
                _additionalOffset = 0;
            }

            void ReadMatchDistances(out UInt32 lenRes, out UInt32 numDistancePairs)
            {
                lenRes = 0;
                numDistancePairs = _matchFinder.GetMatches(_matchDistances);
                if (numDistancePairs > 0)
                {
                    lenRes = _matchDistances[numDistancePairs - 2];
                    if (lenRes == _numFastBytes)
                        lenRes += _matchFinder.GetMatchLen((int)lenRes - 1, _matchDistances[numDistancePairs - 1],
                            LZMABase.kMatchMaxLen - lenRes);
                }
                _additionalOffset++;
            }


            void MovePos(UInt32 num)
            {
                if (num > 0)
                {
                    _matchFinder.Skip(num);
                    _additionalOffset += num;
                }
            }

            UInt32 GetRepLen1Price(LZMABase.State state, UInt32 posState)
            {
                return _isRepG0[state.Index].GetPrice0() +
                        _isRep0Long[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice0();
            }

            UInt32 GetPureRepPrice(UInt32 repIndex, LZMABase.State state, UInt32 posState)
            {
                UInt32 price;
                if (repIndex == 0)
                {
                    price = _isRepG0[state.Index].GetPrice0();
                    price += _isRep0Long[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice1();
                }
                else
                {
                    price = _isRepG0[state.Index].GetPrice1();
                    if (repIndex == 1)
                        price += _isRepG1[state.Index].GetPrice0();
                    else
                    {
                        price += _isRepG1[state.Index].GetPrice1();
                        price += _isRepG2[state.Index].GetPrice(repIndex - 2);
                    }
                }
                return price;
            }

            UInt32 GetRepPrice(UInt32 repIndex, UInt32 len, LZMABase.State state, UInt32 posState)
            {
                UInt32 price = _repMatchLenEncoder.GetPrice(len - LZMABase.kMatchMinLen, posState);
                return price + GetPureRepPrice(repIndex, state, posState);
            }

            UInt32 GetPosLenPrice(UInt32 pos, UInt32 len, UInt32 posState)
            {
                UInt32 price;
                UInt32 lenToPosState = LZMABase.GetLenToPosState(len);
                if (pos < LZMABase.kNumFullDistances)
                    price = _distancesPrices[(lenToPosState * LZMABase.kNumFullDistances) + pos];
                else
                    price = _posSlotPrices[(lenToPosState << LZMABase.kNumPosSlotBits) + GetPosSlot2(pos)] +
                        _alignPrices[pos & LZMABase.kAlignMask];
                return price + _lenEncoder.GetPrice(len - LZMABase.kMatchMinLen, posState);
            }

            UInt32 Backward(out UInt32 backRes, UInt32 cur)
            {
                _optimumEndIndex = cur;
                UInt32 posMem = _optimum[cur].PosPrev;
                UInt32 backMem = _optimum[cur].BackPrev;
                do
                {
                    if (_optimum[cur].Prev1IsChar)
                    {
                        _optimum[posMem].MakeAsChar();
                        _optimum[posMem].PosPrev = posMem - 1;
                        if (_optimum[cur].Prev2)
                        {
                            _optimum[posMem - 1].Prev1IsChar = false;
                            _optimum[posMem - 1].PosPrev = _optimum[cur].PosPrev2;
                            _optimum[posMem - 1].BackPrev = _optimum[cur].BackPrev2;
                        }
                    }
                    UInt32 posPrev = posMem;
                    UInt32 backCur = backMem;

                    backMem = _optimum[posPrev].BackPrev;
                    posMem = _optimum[posPrev].PosPrev;

                    _optimum[posPrev].BackPrev = backCur;
                    _optimum[posPrev].PosPrev = cur;
                    cur = posPrev;
                }
                while (cur > 0);
                backRes = _optimum[0].BackPrev;
                _optimumCurrentIndex = _optimum[0].PosPrev;
                return _optimumCurrentIndex;
            }

            UInt32[] reps = new UInt32[LZMABase.kNumRepDistances];
            UInt32[] repLens = new UInt32[LZMABase.kNumRepDistances];


            UInt32 GetOptimum(UInt32 position, out UInt32 backRes)
            {
                if (_optimumEndIndex != _optimumCurrentIndex)
                {
                    UInt32 lenRes = _optimum[_optimumCurrentIndex].PosPrev - _optimumCurrentIndex;
                    backRes = _optimum[_optimumCurrentIndex].BackPrev;
                    _optimumCurrentIndex = _optimum[_optimumCurrentIndex].PosPrev;
                    return lenRes;
                }
                _optimumCurrentIndex = _optimumEndIndex = 0;

                UInt32 lenMain, numDistancePairs;
                if (!_longestMatchWasFound)
                {
                    ReadMatchDistances(out lenMain, out numDistancePairs);
                }
                else
                {
                    lenMain = _longestMatchLength;
                    numDistancePairs = _numDistancePairs;
                    _longestMatchWasFound = false;
                }

                UInt32 numAvailableBytes = _matchFinder.GetNumAvailableBytes() + 1;
                if (numAvailableBytes < 2)
                {
                    backRes = 0xFFFFFFFF;
                    return 1;
                }
                if (numAvailableBytes > LZMABase.kMatchMaxLen)
                    numAvailableBytes = LZMABase.kMatchMaxLen;

                UInt32 repMaxIndex = 0;
                UInt32 i;
                for (i = 0; i < LZMABase.kNumRepDistances; i++)
                {
                    reps[i] = _repDistances[i];
                    repLens[i] = _matchFinder.GetMatchLen(0 - 1, reps[i], LZMABase.kMatchMaxLen);
                    if (repLens[i] > repLens[repMaxIndex])
                        repMaxIndex = i;
                }
                if (repLens[repMaxIndex] >= _numFastBytes)
                {
                    backRes = repMaxIndex;
                    UInt32 lenRes = repLens[repMaxIndex];
                    MovePos(lenRes - 1);
                    return lenRes;
                }

                if (lenMain >= _numFastBytes)
                {
                    backRes = _matchDistances[numDistancePairs - 1] + LZMABase.kNumRepDistances;
                    MovePos(lenMain - 1);
                    return lenMain;
                }

                Byte currentByte = _matchFinder.GetIndexByte(0 - 1);
                Byte matchByte = _matchFinder.GetIndexByte((Int32)(0 - _repDistances[0] - 1 - 1));

                if (lenMain < 2 && currentByte != matchByte && repLens[repMaxIndex] < 2)
                {
                    backRes = (UInt32)0xFFFFFFFF;
                    return 1;
                }

                _optimum[0].State = _state;

                UInt32 posState = (position & _posStateMask);

                _optimum[1].Price = _isMatch[(_state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice0() +
                        _literalEncoder.GetSubCoder(position, _previousByte).GetPrice(!_state.IsCharState(), matchByte, currentByte);
                _optimum[1].MakeAsChar();

                UInt32 matchPrice = _isMatch[(_state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice1();
                UInt32 repMatchPrice = matchPrice + _isRep[_state.Index].GetPrice1();

                if (matchByte == currentByte)
                {
                    UInt32 shortRepPrice = repMatchPrice + GetRepLen1Price(_state, posState);
                    if (shortRepPrice < _optimum[1].Price)
                    {
                        _optimum[1].Price = shortRepPrice;
                        _optimum[1].MakeAsShortRep();
                    }
                }

                UInt32 lenEnd = ((lenMain >= repLens[repMaxIndex]) ? lenMain : repLens[repMaxIndex]);

                if (lenEnd < 2)
                {
                    backRes = _optimum[1].BackPrev;
                    return 1;
                }

                _optimum[1].PosPrev = 0;

                _optimum[0].Backs0 = reps[0];
                _optimum[0].Backs1 = reps[1];
                _optimum[0].Backs2 = reps[2];
                _optimum[0].Backs3 = reps[3];

                UInt32 len = lenEnd;
                do
                    _optimum[len--].Price = kIfinityPrice;
                while (len >= 2);

                for (i = 0; i < LZMABase.kNumRepDistances; i++)
                {
                    UInt32 repLen = repLens[i];
                    if (repLen < 2)
                        continue;
                    UInt32 price = repMatchPrice + GetPureRepPrice(i, _state, posState);
                    do
                    {
                        UInt32 curAndLenPrice = price + _repMatchLenEncoder.GetPrice(repLen - 2, posState);
                        Optimal optimum = _optimum[repLen];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = 0;
                            optimum.BackPrev = i;
                            optimum.Prev1IsChar = false;
                        }
                    }
                    while (--repLen >= 2);
                }

                UInt32 normalMatchPrice = matchPrice + _isRep[_state.Index].GetPrice0();

                len = ((repLens[0] >= 2) ? repLens[0] + 1 : 2);
                if (len <= lenMain)
                {
                    UInt32 offs = 0;
                    while (len > _matchDistances[offs])
                        offs += 2;
                    for (; ; len++)
                    {
                        UInt32 distance = _matchDistances[offs + 1];
                        UInt32 curAndLenPrice = normalMatchPrice + GetPosLenPrice(distance, len, posState);
                        Optimal optimum = _optimum[len];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = 0;
                            optimum.BackPrev = distance + LZMABase.kNumRepDistances;
                            optimum.Prev1IsChar = false;
                        }
                        if (len == _matchDistances[offs])
                        {
                            offs += 2;
                            if (offs == numDistancePairs)
                                break;
                        }
                    }
                }

                UInt32 cur = 0;

                while (true)
                {
                    cur++;
                    if (cur == lenEnd)
                        return Backward(out backRes, cur);
                    UInt32 newLen;
                    ReadMatchDistances(out newLen, out numDistancePairs);
                    if (newLen >= _numFastBytes)
                    {
                        _numDistancePairs = numDistancePairs;
                        _longestMatchLength = newLen;
                        _longestMatchWasFound = true;
                        return Backward(out backRes, cur);
                    }
                    position++;
                    UInt32 posPrev = _optimum[cur].PosPrev;
                    LZMABase.State state;
                    if (_optimum[cur].Prev1IsChar)
                    {
                        posPrev--;
                        if (_optimum[cur].Prev2)
                        {
                            state = _optimum[_optimum[cur].PosPrev2].State;
                            if (_optimum[cur].BackPrev2 < LZMABase.kNumRepDistances)
                                state.UpdateRep();
                            else
                                state.UpdateMatch();
                        }
                        else
                            state = _optimum[posPrev].State;
                        state.UpdateChar();
                    }
                    else
                        state = _optimum[posPrev].State;
                    if (posPrev == cur - 1)
                    {
                        if (_optimum[cur].IsShortRep())
                            state.UpdateShortRep();
                        else
                            state.UpdateChar();
                    }
                    else
                    {
                        UInt32 pos;
                        if (_optimum[cur].Prev1IsChar && _optimum[cur].Prev2)
                        {
                            posPrev = _optimum[cur].PosPrev2;
                            pos = _optimum[cur].BackPrev2;
                            state.UpdateRep();
                        }
                        else
                        {
                            pos = _optimum[cur].BackPrev;
                            if (pos < LZMABase.kNumRepDistances)
                                state.UpdateRep();
                            else
                                state.UpdateMatch();
                        }
                        Optimal opt = _optimum[posPrev];
                        if (pos < LZMABase.kNumRepDistances)
                        {
                            if (pos == 0)
                            {
                                reps[0] = opt.Backs0;
                                reps[1] = opt.Backs1;
                                reps[2] = opt.Backs2;
                                reps[3] = opt.Backs3;
                            }
                            else if (pos == 1)
                            {
                                reps[0] = opt.Backs1;
                                reps[1] = opt.Backs0;
                                reps[2] = opt.Backs2;
                                reps[3] = opt.Backs3;
                            }
                            else if (pos == 2)
                            {
                                reps[0] = opt.Backs2;
                                reps[1] = opt.Backs0;
                                reps[2] = opt.Backs1;
                                reps[3] = opt.Backs3;
                            }
                            else
                            {
                                reps[0] = opt.Backs3;
                                reps[1] = opt.Backs0;
                                reps[2] = opt.Backs1;
                                reps[3] = opt.Backs2;
                            }
                        }
                        else
                        {
                            reps[0] = (pos - LZMABase.kNumRepDistances);
                            reps[1] = opt.Backs0;
                            reps[2] = opt.Backs1;
                            reps[3] = opt.Backs2;
                        }
                    }
                    _optimum[cur].State = state;
                    _optimum[cur].Backs0 = reps[0];
                    _optimum[cur].Backs1 = reps[1];
                    _optimum[cur].Backs2 = reps[2];
                    _optimum[cur].Backs3 = reps[3];
                    UInt32 curPrice = _optimum[cur].Price;

                    currentByte = _matchFinder.GetIndexByte(0 - 1);
                    matchByte = _matchFinder.GetIndexByte((Int32)(0 - reps[0] - 1 - 1));

                    posState = (position & _posStateMask);

                    UInt32 curAnd1Price = curPrice +
                        _isMatch[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice0() +
                        _literalEncoder.GetSubCoder(position, _matchFinder.GetIndexByte(0 - 2)).
                        GetPrice(!state.IsCharState(), matchByte, currentByte);

                    Optimal nextOptimum = _optimum[cur + 1];

                    bool nextIsChar = false;
                    if (curAnd1Price < nextOptimum.Price)
                    {
                        nextOptimum.Price = curAnd1Price;
                        nextOptimum.PosPrev = cur;
                        nextOptimum.MakeAsChar();
                        nextIsChar = true;
                    }

                    matchPrice = curPrice + _isMatch[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice1();
                    repMatchPrice = matchPrice + _isRep[state.Index].GetPrice1();

                    if (matchByte == currentByte &&
                        !(nextOptimum.PosPrev < cur && nextOptimum.BackPrev == 0))
                    {
                        UInt32 shortRepPrice = repMatchPrice + GetRepLen1Price(state, posState);
                        if (shortRepPrice <= nextOptimum.Price)
                        {
                            nextOptimum.Price = shortRepPrice;
                            nextOptimum.PosPrev = cur;
                            nextOptimum.MakeAsShortRep();
                            nextIsChar = true;
                        }
                    }

                    UInt32 numAvailableBytesFull = _matchFinder.GetNumAvailableBytes() + 1;
                    numAvailableBytesFull = Math.Min(kNumOpts - 1 - cur, numAvailableBytesFull);
                    numAvailableBytes = numAvailableBytesFull;

                    if (numAvailableBytes < 2)
                        continue;
                    if (numAvailableBytes > _numFastBytes)
                        numAvailableBytes = _numFastBytes;
                    if (!nextIsChar && matchByte != currentByte)
                    {
                        // try Literal + rep0
                        UInt32 t = Math.Min(numAvailableBytesFull - 1, _numFastBytes);
                        UInt32 lenTest2 = _matchFinder.GetMatchLen(0, reps[0], t);
                        if (lenTest2 >= 2)
                        {
                            LZMABase.State state2 = state;
                            state2.UpdateChar();
                            UInt32 posStateNext = (position + 1) & _posStateMask;
                            UInt32 nextRepMatchPrice = curAnd1Price +
                                _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice1() +
                                _isRep[state2.Index].GetPrice1();
                            {
                                UInt32 offset = cur + 1 + lenTest2;
                                while (lenEnd < offset)
                                    _optimum[++lenEnd].Price = kIfinityPrice;
                                UInt32 curAndLenPrice = nextRepMatchPrice + GetRepPrice(
                                    0, lenTest2, state2, posStateNext);
                                Optimal optimum = _optimum[offset];
                                if (curAndLenPrice < optimum.Price)
                                {
                                    optimum.Price = curAndLenPrice;
                                    optimum.PosPrev = cur + 1;
                                    optimum.BackPrev = 0;
                                    optimum.Prev1IsChar = true;
                                    optimum.Prev2 = false;
                                }
                            }
                        }
                    }

                    UInt32 startLen = 2; // speed optimization 

                    for (UInt32 repIndex = 0; repIndex < LZMABase.kNumRepDistances; repIndex++)
                    {
                        UInt32 lenTest = _matchFinder.GetMatchLen(0 - 1, reps[repIndex], numAvailableBytes);
                        if (lenTest < 2)
                            continue;
                        UInt32 lenTestTemp = lenTest;
                        do
                        {
                            while (lenEnd < cur + lenTest)
                                _optimum[++lenEnd].Price = kIfinityPrice;
                            UInt32 curAndLenPrice = repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState);
                            Optimal optimum = _optimum[cur + lenTest];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur;
                                optimum.BackPrev = repIndex;
                                optimum.Prev1IsChar = false;
                            }
                        }
                        while (--lenTest >= 2);
                        lenTest = lenTestTemp;

                        if (repIndex == 0)
                            startLen = lenTest + 1;

                        // if (_maxMode)
                        if (lenTest < numAvailableBytesFull)
                        {
                            UInt32 t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
                            UInt32 lenTest2 = _matchFinder.GetMatchLen((Int32)lenTest, reps[repIndex], t);
                            if (lenTest2 >= 2)
                            {
                                LZMABase.State state2 = state;
                                state2.UpdateRep();
                                UInt32 posStateNext = (position + lenTest) & _posStateMask;
                                UInt32 curAndLenCharPrice =
                                        repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState) +
                                        _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice0() +
                                        _literalEncoder.GetSubCoder(position + lenTest,
                                        _matchFinder.GetIndexByte((Int32)lenTest - 1 - 1)).GetPrice(true,
                                        _matchFinder.GetIndexByte((Int32)((Int32)lenTest - 1 - (Int32)(reps[repIndex] + 1))),
                                        _matchFinder.GetIndexByte((Int32)lenTest - 1));
                                state2.UpdateChar();
                                posStateNext = (position + lenTest + 1) & _posStateMask;
                                UInt32 nextMatchPrice = curAndLenCharPrice + _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice1();
                                UInt32 nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                // for(; lenTest2 >= 2; lenTest2--)
                                {
                                    UInt32 offset = lenTest + 1 + lenTest2;
                                    while (lenEnd < cur + offset)
                                        _optimum[++lenEnd].Price = kIfinityPrice;
                                    UInt32 curAndLenPrice = nextRepMatchPrice + GetRepPrice(0, lenTest2, state2, posStateNext);
                                    Optimal optimum = _optimum[cur + offset];
                                    if (curAndLenPrice < optimum.Price)
                                    {
                                        optimum.Price = curAndLenPrice;
                                        optimum.PosPrev = cur + lenTest + 1;
                                        optimum.BackPrev = 0;
                                        optimum.Prev1IsChar = true;
                                        optimum.Prev2 = true;
                                        optimum.PosPrev2 = cur;
                                        optimum.BackPrev2 = repIndex;
                                    }
                                }
                            }
                        }
                    }

                    if (newLen > numAvailableBytes)
                    {
                        newLen = numAvailableBytes;
                        for (numDistancePairs = 0; newLen > _matchDistances[numDistancePairs]; numDistancePairs += 2) ;
                        _matchDistances[numDistancePairs] = newLen;
                        numDistancePairs += 2;
                    }
                    if (newLen >= startLen)
                    {
                        normalMatchPrice = matchPrice + _isRep[state.Index].GetPrice0();
                        while (lenEnd < cur + newLen)
                            _optimum[++lenEnd].Price = kIfinityPrice;

                        UInt32 offs = 0;
                        while (startLen > _matchDistances[offs])
                            offs += 2;

                        for (UInt32 lenTest = startLen; ; lenTest++)
                        {
                            UInt32 curBack = _matchDistances[offs + 1];
                            UInt32 curAndLenPrice = normalMatchPrice + GetPosLenPrice(curBack, lenTest, posState);
                            Optimal optimum = _optimum[cur + lenTest];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur;
                                optimum.BackPrev = curBack + LZMABase.kNumRepDistances;
                                optimum.Prev1IsChar = false;
                            }

                            if (lenTest == _matchDistances[offs])
                            {
                                if (lenTest < numAvailableBytesFull)
                                {
                                    UInt32 t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
                                    UInt32 lenTest2 = _matchFinder.GetMatchLen((Int32)lenTest, curBack, t);
                                    if (lenTest2 >= 2)
                                    {
                                        LZMABase.State state2 = state;
                                        state2.UpdateMatch();
                                        UInt32 posStateNext = (position + lenTest) & _posStateMask;
                                        UInt32 curAndLenCharPrice = curAndLenPrice +
                                            _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice0() +
                                            _literalEncoder.GetSubCoder(position + lenTest,
                                            _matchFinder.GetIndexByte((Int32)lenTest - 1 - 1)).
                                            GetPrice(true,
                                            _matchFinder.GetIndexByte((Int32)lenTest - (Int32)(curBack + 1) - 1),
                                            _matchFinder.GetIndexByte((Int32)lenTest - 1));
                                        state2.UpdateChar();
                                        posStateNext = (position + lenTest + 1) & _posStateMask;
                                        UInt32 nextMatchPrice = curAndLenCharPrice + _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice1();
                                        UInt32 nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                        UInt32 offset = lenTest + 1 + lenTest2;
                                        while (lenEnd < cur + offset)
                                            _optimum[++lenEnd].Price = kIfinityPrice;
                                        curAndLenPrice = nextRepMatchPrice + GetRepPrice(0, lenTest2, state2, posStateNext);
                                        optimum = _optimum[cur + offset];
                                        if (curAndLenPrice < optimum.Price)
                                        {
                                            optimum.Price = curAndLenPrice;
                                            optimum.PosPrev = cur + lenTest + 1;
                                            optimum.BackPrev = 0;
                                            optimum.Prev1IsChar = true;
                                            optimum.Prev2 = true;
                                            optimum.PosPrev2 = cur;
                                            optimum.BackPrev2 = curBack + LZMABase.kNumRepDistances;
                                        }
                                    }
                                }
                                offs += 2;
                                if (offs == numDistancePairs)
                                    break;
                            }
                        }
                    }
                }
            }

            bool ChangePair(UInt32 smallDist, UInt32 bigDist)
            {
                const int kDif = 7;
                return (smallDist < ((UInt32)(1) << (32 - kDif)) && bigDist >= (smallDist << kDif));
            }

            void WriteEndMarker(UInt32 posState)
            {
                if (!_writeEndMark)
                    return;

                _isMatch[(_state.Index << LZMABase.kNumPosStatesBitsMax) + posState].Encode(_rangeEncoder, 1);
                _isRep[_state.Index].Encode(_rangeEncoder, 0);
                _state.UpdateMatch();
                UInt32 len = LZMABase.kMatchMinLen;
                _lenEncoder.Encode(_rangeEncoder, len - LZMABase.kMatchMinLen, posState);
                UInt32 posSlot = (1 << LZMABase.kNumPosSlotBits) - 1;
                UInt32 lenToPosState = LZMABase.GetLenToPosState(len);
                _posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);
                int footerBits = 30;
                UInt32 posReduced = (((UInt32)1) << footerBits) - 1;
                _rangeEncoder.EncodeDirectBits(posReduced >> LZMABase.kNumAlignBits, footerBits - LZMABase.kNumAlignBits);
                _posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & LZMABase.kAlignMask);
            }

            void Flush(UInt32 nowPos)
            {
                ReleaseMFStream();
                WriteEndMarker(nowPos & _posStateMask);
                _rangeEncoder.FlushData();
                _rangeEncoder.FlushStream();
            }

            public void CodeOneBlock(out Int64 inSize, out Int64 outSize, out bool finished)
            {
                inSize = 0;
                outSize = 0;
                finished = true;

                if (_inStream != null)
                {
                    _matchFinder.SetStream(_inStream);
                    _matchFinder.Init();
                    _needReleaseMFStream = true;
                    _inStream = null;
                }

                if (_finished)
                    return;
                _finished = true;


                Int64 progressPosValuePrev = nowPos64;
                if (nowPos64 == 0)
                {
                    if (_matchFinder.GetNumAvailableBytes() == 0)
                    {
                        Flush((UInt32)nowPos64);
                        return;
                    }
                    UInt32 len, numDistancePairs; // it's not used
                    ReadMatchDistances(out len, out numDistancePairs);
                    UInt32 posState = (UInt32)(nowPos64) & _posStateMask;
                    _isMatch[(_state.Index << LZMABase.kNumPosStatesBitsMax) + posState].Encode(_rangeEncoder, 0);
                    _state.UpdateChar();
                    Byte curByte = _matchFinder.GetIndexByte((Int32)(0 - _additionalOffset));
                    _literalEncoder.GetSubCoder((UInt32)(nowPos64), _previousByte).Encode(_rangeEncoder, curByte);
                    _previousByte = curByte;
                    _additionalOffset--;
                    nowPos64++;
                }
                if (_matchFinder.GetNumAvailableBytes() == 0)
                {
                    Flush((UInt32)nowPos64);
                    return;
                }
                while (true)
                {
                    UInt32 pos;
                    UInt32 len = GetOptimum((UInt32)nowPos64, out pos);

                    UInt32 posState = ((UInt32)nowPos64) & _posStateMask;
                    UInt32 complexState = (_state.Index << LZMABase.kNumPosStatesBitsMax) + posState;
                    if (len == 1 && pos == 0xFFFFFFFF)
                    {
                        _isMatch[complexState].Encode(_rangeEncoder, 0);
                        Byte curByte = _matchFinder.GetIndexByte((Int32)(0 - _additionalOffset));
                        LiteralEncoder.Encoder2 subCoder = _literalEncoder.GetSubCoder((UInt32)nowPos64, _previousByte);
                        if (!_state.IsCharState())
                        {
                            Byte matchByte = _matchFinder.GetIndexByte((Int32)(0 - _repDistances[0] - 1 - _additionalOffset));
                            subCoder.EncodeMatched(_rangeEncoder, matchByte, curByte);
                        }
                        else
                            subCoder.Encode(_rangeEncoder, curByte);
                        _previousByte = curByte;
                        _state.UpdateChar();
                    }
                    else
                    {
                        _isMatch[complexState].Encode(_rangeEncoder, 1);
                        if (pos < LZMABase.kNumRepDistances)
                        {
                            _isRep[_state.Index].Encode(_rangeEncoder, 1);
                            if (pos == 0)
                            {
                                _isRepG0[_state.Index].Encode(_rangeEncoder, 0);
                                if (len == 1)
                                    _isRep0Long[complexState].Encode(_rangeEncoder, 0);
                                else
                                    _isRep0Long[complexState].Encode(_rangeEncoder, 1);
                            }
                            else
                            {
                                _isRepG0[_state.Index].Encode(_rangeEncoder, 1);
                                if (pos == 1)
                                    _isRepG1[_state.Index].Encode(_rangeEncoder, 0);
                                else
                                {
                                    _isRepG1[_state.Index].Encode(_rangeEncoder, 1);
                                    _isRepG2[_state.Index].Encode(_rangeEncoder, pos - 2);
                                }
                            }
                            if (len == 1)
                                _state.UpdateShortRep();
                            else
                            {
                                _repMatchLenEncoder.Encode(_rangeEncoder, len - LZMABase.kMatchMinLen, posState);
                                _state.UpdateRep();
                            }
                            UInt32 distance = _repDistances[pos];
                            if (pos != 0)
                            {
                                for (UInt32 i = pos; i >= 1; i--)
                                    _repDistances[i] = _repDistances[i - 1];
                                _repDistances[0] = distance;
                            }
                        }
                        else
                        {
                            _isRep[_state.Index].Encode(_rangeEncoder, 0);
                            _state.UpdateMatch();
                            _lenEncoder.Encode(_rangeEncoder, len - LZMABase.kMatchMinLen, posState);
                            pos -= LZMABase.kNumRepDistances;
                            UInt32 posSlot = GetPosSlot(pos);
                            UInt32 lenToPosState = LZMABase.GetLenToPosState(len);
                            _posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);

                            if (posSlot >= LZMABase.kStartPosModelIndex)
                            {
                                int footerBits = (int)((posSlot >> 1) - 1);
                                UInt32 baseVal = ((2 | (posSlot & 1)) << footerBits);
                                UInt32 posReduced = pos - baseVal;

                                if (posSlot < LZMABase.kEndPosModelIndex)
                                    LZMARangeBitTreeEncoder.ReverseEncode(_posEncoders, baseVal - posSlot - 1, _rangeEncoder, footerBits, posReduced);
                                else
                                {
                                    _rangeEncoder.EncodeDirectBits(posReduced >> LZMABase.kNumAlignBits, footerBits - LZMABase.kNumAlignBits);
                                    _posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & LZMABase.kAlignMask);
                                    _alignPriceCount++;
                                }
                            }
                            UInt32 distance = pos;
                            for (UInt32 i = LZMABase.kNumRepDistances - 1; i >= 1; i--)
                                _repDistances[i] = _repDistances[i - 1];
                            _repDistances[0] = distance;
                            _matchPriceCount++;
                        }
                        _previousByte = _matchFinder.GetIndexByte((Int32)(len - 1 - _additionalOffset));
                    }
                    _additionalOffset -= len;
                    nowPos64 += len;
                    if (_additionalOffset == 0)
                    {
                        // if (!_fastMode)
                        if (_matchPriceCount >= (1 << 7))
                            FillDistancesPrices();
                        if (_alignPriceCount >= LZMABase.kAlignTableSize)
                            FillAlignPrices();
                        inSize = nowPos64;
                        outSize = _rangeEncoder.GetProcessedSizeAdd();
                        if (_matchFinder.GetNumAvailableBytes() == 0)
                        {
                            Flush((UInt32)nowPos64);
                            return;
                        }

                        if (nowPos64 - progressPosValuePrev >= (1 << 12))
                        {
                            _finished = false;
                            finished = false;
                            return;
                        }
                    }
                }
            }

            void ReleaseMFStream()
            {
                if (_matchFinder != null && _needReleaseMFStream)
                {
                    _matchFinder.ReleaseStream();
                    _needReleaseMFStream = false;
                }
            }

            void SetOutStream(System.IO.Stream outStream) { _rangeEncoder.SetStream(outStream); }
            void ReleaseOutStream() { _rangeEncoder.ReleaseStream(); }

            void ReleaseStreams()
            {
                ReleaseMFStream();
                ReleaseOutStream();
            }

            void SetStreams(System.IO.Stream inStream, System.IO.Stream outStream,
                    Int64 inSize, Int64 outSize)
            {
                _inStream = inStream;
                _finished = false;
                Create();
                SetOutStream(outStream);
                Init();

                // if (!_fastMode)
                {
                    FillDistancesPrices();
                    FillAlignPrices();
                }

                _lenEncoder.SetTableSize(_numFastBytes + 1 - LZMABase.kMatchMinLen);
                _lenEncoder.UpdateTables((UInt32)1 << _posStateBits);
                _repMatchLenEncoder.SetTableSize(_numFastBytes + 1 - LZMABase.kMatchMinLen);
                _repMatchLenEncoder.UpdateTables((UInt32)1 << _posStateBits);

                nowPos64 = 0;
            }


            public override void Code(System.IO.Stream inStream, System.IO.Stream outStream, Int64 inSize, Int64 outSize, ICodeProgress progress)
            {
                _needReleaseMFStream = false;
                try
                {
                    SetStreams(inStream, outStream, inSize, outSize);
                    while (true)
                    {
                        Int64 processedInSize;
                        Int64 processedOutSize;
                        bool finished;
                        CodeOneBlock(out processedInSize, out processedOutSize, out finished);
                        if (finished)
                            return;
                        if (progress != null)
                        {
                            progress.SetProgress(processedInSize, processedOutSize);
                        }
                    }
                }
                finally
                {
                    ReleaseStreams();
                }
            }

            const int kPropSize = 5;
            Byte[] properties = new Byte[kPropSize];

            public void WriteCoderProperties(System.IO.Stream outStream)
            {
                properties[0] = (Byte)((_posStateBits * 5 + _numLiteralPosStateBits) * 9 + _numLiteralContextBits);
                for (int i = 0; i < 4; i++)
                    properties[1 + i] = (Byte)(_dictionarySize >> (8 * i));
                outStream.Write(properties, 0, kPropSize);
            }

            UInt32[] tempPrices = new UInt32[LZMABase.kNumFullDistances];
            UInt32 _matchPriceCount;

            void FillDistancesPrices()
            {
                for (UInt32 i = LZMABase.kStartPosModelIndex; i < LZMABase.kNumFullDistances; i++)
                {
                    UInt32 posSlot = GetPosSlot(i);
                    int footerBits = (int)((posSlot >> 1) - 1);
                    UInt32 baseVal = ((2 | (posSlot & 1)) << footerBits);
                    tempPrices[i] = LZMARangeBitTreeEncoder.ReverseGetPrice(_posEncoders, baseVal - posSlot - 1, footerBits, i - baseVal);
                }

                for (UInt32 lenToPosState = 0; lenToPosState < LZMABase.kNumLenToPosStates; lenToPosState++)
                {
                    UInt32 posSlot;
                    LZMARangeBitTreeEncoder encoder = _posSlotEncoder[lenToPosState];

                    UInt32 st = (lenToPosState << LZMABase.kNumPosSlotBits);
                    for (posSlot = 0; posSlot < _distTableSize; posSlot++)
                        _posSlotPrices[st + posSlot] = encoder.GetPrice(posSlot);
                    for (posSlot = LZMABase.kEndPosModelIndex; posSlot < _distTableSize; posSlot++)
                        _posSlotPrices[st + posSlot] += ((((posSlot >> 1) - 1) - LZMABase.kNumAlignBits) << LZMARangeBitEncoder.kNumBitPriceShiftBits);

                    UInt32 st2 = lenToPosState * LZMABase.kNumFullDistances;
                    UInt32 i;
                    for (i = 0; i < LZMABase.kStartPosModelIndex; i++)
                        _distancesPrices[st2 + i] = _posSlotPrices[st + i];
                    for (; i < LZMABase.kNumFullDistances; i++)
                        _distancesPrices[st2 + i] = _posSlotPrices[st + GetPosSlot(i)] + tempPrices[i];
                }
                _matchPriceCount = 0;
            }

            void FillAlignPrices()
            {
                for (UInt32 i = 0; i < LZMABase.kAlignTableSize; i++)
                    _alignPrices[i] = _posAlignEncoder.ReverseGetPrice(i);
                _alignPriceCount = 0;
            }


            static string[] kMatchFinderIDs = 
		{
			"BT2",
			"BT4",
		};

            static int FindMatchFinder(string s)
            {
                for (int m = 0; m < kMatchFinderIDs.Length; m++)
                    if (s == kMatchFinderIDs[m])
                        return m;
                return -1;
            }

            public void SetCoderProperties(CoderPropID[] propIDs, object[] properties)
            {
                for (UInt32 i = 0; i < properties.Length; i++)
                {
                    object prop = properties[i];
                    switch (propIDs[i])
                    {
                        case CoderPropID.NumFastBytes:
                            {
                                if (!(prop is Int32))
                                    throw new Exception();
                                Int32 numFastBytes = (Int32)prop;
                                if (numFastBytes < 5 || numFastBytes > LZMABase.kMatchMaxLen)
                                    throw new Exception();
                                _numFastBytes = (UInt32)numFastBytes;
                                break;
                            }
                        case CoderPropID.Algorithm:
                            {
                                break;
                            }
                        case CoderPropID.MatchFinder:
                            {
                                if (!(prop is String))
                                    throw new Exception();
                                EMatchFinderType matchFinderIndexPrev = _matchFinderType;
                                int m = FindMatchFinder(((string)prop).ToUpper());
                                if (m < 0)
                                    throw new Exception();
                                _matchFinderType = (EMatchFinderType)m;
                                if (_matchFinder != null && matchFinderIndexPrev != _matchFinderType)
                                {
                                    _dictionarySizePrev = 0xFFFFFFFF;
                                    _matchFinder = null;
                                }
                                break;
                            }
                        case CoderPropID.DictionarySize:
                            {
                                const int kDicLogSizeMaxCompress = 30;
                                if (!(prop is Int32))
                                    throw new Exception(); ;
                                Int32 dictionarySize = (Int32)prop;
                                if (dictionarySize < (UInt32)(1 << LZMABase.kDicLogSizeMin) ||
                                    dictionarySize > (UInt32)(1 << kDicLogSizeMaxCompress))
                                    throw new Exception();
                                _dictionarySize = (UInt32)dictionarySize;
                                int dicLogSize;
                                for (dicLogSize = 0; dicLogSize < (UInt32)kDicLogSizeMaxCompress; dicLogSize++)
                                    if (dictionarySize <= ((UInt32)(1) << dicLogSize))
                                        break;
                                _distTableSize = (UInt32)dicLogSize * 2;
                                break;
                            }
                        case CoderPropID.PosStateBits:
                            {
                                if (!(prop is Int32))
                                    throw new Exception();
                                Int32 v = (Int32)prop;
                                if (v < 0 || v > (UInt32)LZMABase.kNumPosStatesBitsEncodingMax)
                                    throw new Exception();
                                _posStateBits = (int)v;
                                _posStateMask = (((UInt32)1) << (int)_posStateBits) - 1;
                                break;
                            }
                        case CoderPropID.LitPosBits:
                            {
                                if (!(prop is Int32))
                                    throw new Exception();
                                Int32 v = (Int32)prop;
                                if (v < 0 || v > (UInt32)LZMABase.kNumLitPosStatesBitsEncodingMax)
                                    throw new Exception();
                                _numLiteralPosStateBits = (int)v;
                                break;
                            }
                        case CoderPropID.LitContextBits:
                            {
                                if (!(prop is Int32))
                                    throw new Exception();
                                Int32 v = (Int32)prop;
                                if (v < 0 || v > (UInt32)LZMABase.kNumLitContextBitsMax)
                                    throw new Exception(); ;
                                _numLiteralContextBits = (int)v;
                                break;
                            }
                        case CoderPropID.EndMarker:
                            {
                                if (!(prop is Boolean))
                                    throw new Exception();
                                SetWriteEndMarkerMode((Boolean)prop);
                                break;
                            }
                        default:
                            throw new Exception();
                    }
                }
            }
        }
        #endregion

        #endregion

        #region LZ

        #region IInWindowStream
        private abstract class IInWindowStream
        {
            public abstract void SetStream(System.IO.Stream inStream);
            public abstract void Init();
            public abstract void ReleaseStream();
            public abstract Byte GetIndexByte(Int32 index);
            public abstract UInt32 GetMatchLen(Int32 index, UInt32 distance, UInt32 limit);
            public abstract UInt32 GetNumAvailableBytes();
        }
        #endregion

        #region IMatchFinder
        private abstract class IMatchFinder : IInWindowStream
        {
            public abstract void Create(UInt32 historySize, UInt32 keepAddBufferBefore, UInt32 matchMaxLen, UInt32 keepAddBufferAfter);
            public abstract UInt32 GetMatches(UInt32[] distances);
            public abstract void Skip(UInt32 num);
        }
        #endregion

        #region LZBinTree
        private class LZBinTree : LZInWindow
        {
            UInt32 _cyclicBufferPos;
            UInt32 _cyclicBufferSize = 0;
            UInt32 _matchMaxLen;

            UInt32[] _son;
            UInt32[] _hash;

            UInt32 _cutValue = 0xFF;
            UInt32 _hashMask;
            UInt32 _hashSizeSum = 0;

            bool HASH_ARRAY = true;

            const UInt32 kHash2Size = 1 << 10;
            const UInt32 kHash3Size = 1 << 16;
            const UInt32 kBT2HashSize = 1 << 16;
            const UInt32 kStartMaxLen = 1;
            const UInt32 kHash3Offset = kHash2Size;
            const UInt32 kEmptyHashValue = 0;
            const UInt32 kMaxValForNormalize = ((UInt32)1 << 31) - 1;

            UInt32 kNumHashDirectBytes = 0;
            UInt32 kMinMatchCheck = 4;
            UInt32 kFixHashSize = kHash2Size + kHash3Size;

            public void SetType(int numHashBytes)
            {
                HASH_ARRAY = (numHashBytes > 2);
                if (HASH_ARRAY)
                {
                    kNumHashDirectBytes = 0;
                    kMinMatchCheck = 4;
                    kFixHashSize = kHash2Size + kHash3Size;
                }
                else
                {
                    kNumHashDirectBytes = 2;
                    kMinMatchCheck = 2 + 1;
                    kFixHashSize = 0;
                }
            }

            public new void SetStream(System.IO.Stream stream) { base.SetStream(stream); }
            public new void ReleaseStream() { base.ReleaseStream(); }

            public new void Init()
            {
                base.Init();
                for (UInt32 i = 0; i < _hashSizeSum; i++)
                    _hash[i] = kEmptyHashValue;
                _cyclicBufferPos = 0;
                ReduceOffsets(-1);
            }

            public new void MovePos()
            {
                if (++_cyclicBufferPos >= _cyclicBufferSize)
                    _cyclicBufferPos = 0;
                base.MovePos();
                if (_pos == kMaxValForNormalize)
                    Normalize();
            }

            public new Byte GetIndexByte(Int32 index) { return base.GetIndexByte(index); }

            public new UInt32 GetMatchLen(Int32 index, UInt32 distance, UInt32 limit)
            { return base.GetMatchLen(index, distance, limit); }

            public new UInt32 GetNumAvailableBytes() { return base.GetNumAvailableBytes(); }

            public void Create(UInt32 historySize, UInt32 keepAddBufferBefore,
                    UInt32 matchMaxLen, UInt32 keepAddBufferAfter)
            {
                if (historySize > kMaxValForNormalize - 256)
                    throw new Exception();
                _cutValue = 16 + (matchMaxLen >> 1);

                UInt32 windowReservSize = (historySize + keepAddBufferBefore +
                        matchMaxLen + keepAddBufferAfter) / 2 + 256;

                base.Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, windowReservSize);

                _matchMaxLen = matchMaxLen;

                UInt32 cyclicBufferSize = historySize + 1;
                if (_cyclicBufferSize != cyclicBufferSize)
                    _son = new UInt32[(_cyclicBufferSize = cyclicBufferSize) * 2];

                UInt32 hs = kBT2HashSize;

                if (HASH_ARRAY)
                {
                    hs = historySize - 1;
                    hs |= (hs >> 1);
                    hs |= (hs >> 2);
                    hs |= (hs >> 4);
                    hs |= (hs >> 8);
                    hs >>= 1;
                    hs |= 0xFFFF;
                    if (hs > (1 << 24))
                        hs >>= 1;
                    _hashMask = hs;
                    hs++;
                    hs += kFixHashSize;
                }
                if (hs != _hashSizeSum)
                    _hash = new UInt32[_hashSizeSum = hs];
            }

            public UInt32 GetMatches(UInt32[] distances)
            {
                UInt32 lenLimit;
                if (_pos + _matchMaxLen <= _streamPos)
                    lenLimit = _matchMaxLen;
                else
                {
                    lenLimit = _streamPos - _pos;
                    if (lenLimit < kMinMatchCheck)
                    {
                        MovePos();
                        return 0;
                    }
                }

                UInt32 offset = 0;
                UInt32 matchMinPos = (_pos > _cyclicBufferSize) ? (_pos - _cyclicBufferSize) : 0;
                UInt32 cur = _bufferOffset + _pos;
                UInt32 maxLen = kStartMaxLen; // to avoid items for len < hashSize;
                UInt32 hashValue, hash2Value = 0, hash3Value = 0;

                if (HASH_ARRAY)
                {
                    UInt32 temp = CRC.Table[_bufferBase[cur]] ^ _bufferBase[cur + 1];
                    hash2Value = temp & (kHash2Size - 1);
                    temp ^= ((UInt32)(_bufferBase[cur + 2]) << 8);
                    hash3Value = temp & (kHash3Size - 1);
                    hashValue = (temp ^ (CRC.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
                }
                else
                    hashValue = _bufferBase[cur] ^ ((UInt32)(_bufferBase[cur + 1]) << 8);

                UInt32 curMatch = _hash[kFixHashSize + hashValue];
                if (HASH_ARRAY)
                {
                    UInt32 curMatch2 = _hash[hash2Value];
                    UInt32 curMatch3 = _hash[kHash3Offset + hash3Value];
                    _hash[hash2Value] = _pos;
                    _hash[kHash3Offset + hash3Value] = _pos;
                    if (curMatch2 > matchMinPos)
                        if (_bufferBase[_bufferOffset + curMatch2] == _bufferBase[cur])
                        {
                            distances[offset++] = maxLen = 2;
                            distances[offset++] = _pos - curMatch2 - 1;
                        }
                    if (curMatch3 > matchMinPos)
                        if (_bufferBase[_bufferOffset + curMatch3] == _bufferBase[cur])
                        {
                            if (curMatch3 == curMatch2)
                                offset -= 2;
                            distances[offset++] = maxLen = 3;
                            distances[offset++] = _pos - curMatch3 - 1;
                            curMatch2 = curMatch3;
                        }
                    if (offset != 0 && curMatch2 == curMatch)
                    {
                        offset -= 2;
                        maxLen = kStartMaxLen;
                    }
                }

                _hash[kFixHashSize + hashValue] = _pos;

                UInt32 ptr0 = (_cyclicBufferPos << 1) + 1;
                UInt32 ptr1 = (_cyclicBufferPos << 1);

                UInt32 len0, len1;
                len0 = len1 = kNumHashDirectBytes;

                if (kNumHashDirectBytes != 0)
                {
                    if (curMatch > matchMinPos)
                    {
                        if (_bufferBase[_bufferOffset + curMatch + kNumHashDirectBytes] !=
                                _bufferBase[cur + kNumHashDirectBytes])
                        {
                            distances[offset++] = maxLen = kNumHashDirectBytes;
                            distances[offset++] = _pos - curMatch - 1;
                        }
                    }
                }

                UInt32 count = _cutValue;

                while (true)
                {
                    if (curMatch <= matchMinPos || count-- == 0)
                    {
                        _son[ptr0] = _son[ptr1] = kEmptyHashValue;
                        break;
                    }
                    UInt32 delta = _pos - curMatch;
                    UInt32 cyclicPos = ((delta <= _cyclicBufferPos) ?
                                (_cyclicBufferPos - delta) :
                                (_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;

                    UInt32 pby1 = _bufferOffset + curMatch;
                    UInt32 len = Math.Min(len0, len1);
                    if (_bufferBase[pby1 + len] == _bufferBase[cur + len])
                    {
                        while (++len != lenLimit)
                            if (_bufferBase[pby1 + len] != _bufferBase[cur + len])
                                break;
                        if (maxLen < len)
                        {
                            distances[offset++] = maxLen = len;
                            distances[offset++] = delta - 1;
                            if (len == lenLimit)
                            {
                                _son[ptr1] = _son[cyclicPos];
                                _son[ptr0] = _son[cyclicPos + 1];
                                break;
                            }
                        }
                    }
                    if (_bufferBase[pby1 + len] < _bufferBase[cur + len])
                    {
                        _son[ptr1] = curMatch;
                        ptr1 = cyclicPos + 1;
                        curMatch = _son[ptr1];
                        len1 = len;
                    }
                    else
                    {
                        _son[ptr0] = curMatch;
                        ptr0 = cyclicPos;
                        curMatch = _son[ptr0];
                        len0 = len;
                    }
                }
                MovePos();
                return offset;
            }

            public void Skip(UInt32 num)
            {
                do
                {
                    UInt32 lenLimit;
                    if (_pos + _matchMaxLen <= _streamPos)
                        lenLimit = _matchMaxLen;
                    else
                    {
                        lenLimit = _streamPos - _pos;
                        if (lenLimit < kMinMatchCheck)
                        {
                            MovePos();
                            continue;
                        }
                    }

                    UInt32 matchMinPos = (_pos > _cyclicBufferSize) ? (_pos - _cyclicBufferSize) : 0;
                    UInt32 cur = _bufferOffset + _pos;

                    UInt32 hashValue;

                    if (HASH_ARRAY)
                    {
                        UInt32 temp = CRC.Table[_bufferBase[cur]] ^ _bufferBase[cur + 1];
                        UInt32 hash2Value = temp & (kHash2Size - 1);
                        _hash[hash2Value] = _pos;
                        temp ^= ((UInt32)(_bufferBase[cur + 2]) << 8);
                        UInt32 hash3Value = temp & (kHash3Size - 1);
                        _hash[kHash3Offset + hash3Value] = _pos;
                        hashValue = (temp ^ (CRC.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
                    }
                    else
                        hashValue = _bufferBase[cur] ^ ((UInt32)(_bufferBase[cur + 1]) << 8);

                    UInt32 curMatch = _hash[kFixHashSize + hashValue];
                    _hash[kFixHashSize + hashValue] = _pos;

                    UInt32 ptr0 = (_cyclicBufferPos << 1) + 1;
                    UInt32 ptr1 = (_cyclicBufferPos << 1);

                    UInt32 len0, len1;
                    len0 = len1 = kNumHashDirectBytes;

                    UInt32 count = _cutValue;
                    while (true)
                    {
                        if (curMatch <= matchMinPos || count-- == 0)
                        {
                            _son[ptr0] = _son[ptr1] = kEmptyHashValue;
                            break;
                        }

                        UInt32 delta = _pos - curMatch;
                        UInt32 cyclicPos = ((delta <= _cyclicBufferPos) ?
                                    (_cyclicBufferPos - delta) :
                                    (_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;

                        UInt32 pby1 = _bufferOffset + curMatch;
                        UInt32 len = Math.Min(len0, len1);
                        if (_bufferBase[pby1 + len] == _bufferBase[cur + len])
                        {
                            while (++len != lenLimit)
                                if (_bufferBase[pby1 + len] != _bufferBase[cur + len])
                                    break;
                            if (len == lenLimit)
                            {
                                _son[ptr1] = _son[cyclicPos];
                                _son[ptr0] = _son[cyclicPos + 1];
                                break;
                            }
                        }
                        if (_bufferBase[pby1 + len] < _bufferBase[cur + len])
                        {
                            _son[ptr1] = curMatch;
                            ptr1 = cyclicPos + 1;
                            curMatch = _son[ptr1];
                            len1 = len;
                        }
                        else
                        {
                            _son[ptr0] = curMatch;
                            ptr0 = cyclicPos;
                            curMatch = _son[ptr0];
                            len0 = len;
                        }
                    }
                    MovePos();
                }
                while (--num != 0);
            }

            void NormalizeLinks(UInt32[] items, UInt32 numItems, UInt32 subValue)
            {
                for (UInt32 i = 0; i < numItems; i++)
                {
                    UInt32 value = items[i];
                    if (value <= subValue)
                        value = kEmptyHashValue;
                    else
                        value -= subValue;
                    items[i] = value;
                }
            }

            void Normalize()
            {
                UInt32 subValue = _pos - _cyclicBufferSize;
                NormalizeLinks(_son, _cyclicBufferSize * 2, subValue);
                NormalizeLinks(_hash, _hashSizeSum, subValue);
                ReduceOffsets((Int32)subValue);
            }

            public void SetCutValue(UInt32 cutValue) { _cutValue = cutValue; }
        }
        #endregion

        #region LZInWindow
        private class LZInWindow
        {
            public Byte[] _bufferBase = null;
            System.IO.Stream _stream;
            UInt32 _posLimit;
            bool _streamEndWasReached;

            UInt32 _pointerToLastSafePosition;

            public UInt32 _bufferOffset;

            public UInt32 _blockSize;
            public UInt32 _pos;
            UInt32 _keepSizeBefore;
            UInt32 _keepSizeAfter;
            public UInt32 _streamPos;

            public void MoveBlock()
            {
                UInt32 offset = (UInt32)(_bufferOffset) + _pos - _keepSizeBefore;
                if (offset > 0)
                    offset--;

                UInt32 numBytes = (UInt32)(_bufferOffset) + _streamPos - offset;

                for (UInt32 i = 0; i < numBytes; i++)
                    _bufferBase[i] = _bufferBase[offset + i];
                _bufferOffset -= offset;
            }

            public virtual void ReadBlock()
            {
                if (_streamEndWasReached)
                    return;
                while (true)
                {
                    int size = (int)((0 - _bufferOffset) + _blockSize - _streamPos);
                    if (size == 0)
                        return;
                    int numReadBytes = _stream.Read(_bufferBase, (int)(_bufferOffset + _streamPos), size);
                    if (numReadBytes == 0)
                    {
                        _posLimit = _streamPos;
                        UInt32 pointerToPostion = _bufferOffset + _posLimit;
                        if (pointerToPostion > _pointerToLastSafePosition)
                            _posLimit = (UInt32)(_pointerToLastSafePosition - _bufferOffset);

                        _streamEndWasReached = true;
                        return;
                    }
                    _streamPos += (UInt32)numReadBytes;
                    if (_streamPos >= _pos + _keepSizeAfter)
                        _posLimit = _streamPos - _keepSizeAfter;
                }
            }

            void Free() { _bufferBase = null; }

            public void Create(UInt32 keepSizeBefore, UInt32 keepSizeAfter, UInt32 keepSizeReserv)
            {
                _keepSizeBefore = keepSizeBefore;
                _keepSizeAfter = keepSizeAfter;
                UInt32 blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;
                if (_bufferBase == null || _blockSize != blockSize)
                {
                    Free();
                    _blockSize = blockSize;
                    _bufferBase = new Byte[_blockSize];
                }
                _pointerToLastSafePosition = _blockSize - keepSizeAfter;
            }

            public void SetStream(System.IO.Stream stream) { _stream = stream; }
            public void ReleaseStream() { _stream = null; }

            public void Init()
            {
                _bufferOffset = 0;
                _pos = 0;
                _streamPos = 0;
                _streamEndWasReached = false;
                ReadBlock();
            }

            public void MovePos()
            {
                _pos++;
                if (_pos > _posLimit)
                {
                    UInt32 pointerToPostion = _bufferOffset + _pos;
                    if (pointerToPostion > _pointerToLastSafePosition)
                        MoveBlock();
                    ReadBlock();
                }
            }

            public Byte GetIndexByte(Int32 index) { return _bufferBase[_bufferOffset + _pos + index]; }

            public UInt32 GetMatchLen(Int32 index, UInt32 distance, UInt32 limit)
            {
                if (_streamEndWasReached)
                    if ((_pos + index) + limit > _streamPos)
                        limit = _streamPos - (UInt32)(_pos + index);
                distance++;
                UInt32 pby = _bufferOffset + _pos + (UInt32)index;

                UInt32 i;
                for (i = 0; i < limit && _bufferBase[pby + i] == _bufferBase[pby + i - distance]; i++) ;
                return i;
            }

            public UInt32 GetNumAvailableBytes() { return _streamPos - _pos; }

            public void ReduceOffsets(Int32 subValue)
            {
                _bufferOffset += (UInt32)subValue;
                _posLimit -= (UInt32)subValue;
                _pos -= (UInt32)subValue;
                _streamPos -= (UInt32)subValue;
            }
        }
        #endregion

        #region LZOutWindow
        private class LZOutWindow
        {
            byte[] _buffer = null;
            uint _pos;
            uint _windowSize = 0;
            uint _streamPos;
            System.IO.Stream _stream;

            public void Create(uint windowSize)
            {
                if (_windowSize != windowSize)
                {
                    // System.GC.Collect();
                    _buffer = new byte[windowSize];
                }
                _windowSize = windowSize;
                _pos = 0;
                _streamPos = 0;
            }

            public void Init(System.IO.Stream stream, bool solid)
            {
                ReleaseStream();
                _stream = stream;
                if (!solid)
                {
                    _streamPos = 0;
                    _pos = 0;
                }
            }

            public void Init(System.IO.Stream stream) { Init(stream, false); }

            public void ReleaseStream()
            {
                Flush();
                _stream = null;
            }

            public void Flush()
            {
                uint size = _pos - _streamPos;
                if (size == 0)
                    return;
                _stream.Write(_buffer, (int)_streamPos, (int)size);
                if (_pos >= _windowSize)
                    _pos = 0;
                _streamPos = _pos;
            }

            public void CopyBlock(uint distance, uint len)
            {
                uint pos = _pos - distance - 1;
                if (pos >= _windowSize)
                    pos += _windowSize;
                for (; len > 0; len--)
                {
                    if (pos >= _windowSize)
                        pos = 0;
                    _buffer[_pos++] = _buffer[pos++];
                    if (_pos >= _windowSize)
                        Flush();
                }
            }

            public void PutByte(byte b)
            {
                _buffer[_pos++] = b;
                if (_pos >= _windowSize)
                    Flush();
            }

            public byte GetByte(uint distance)
            {
                uint pos = _pos - distance - 1;
                if (pos >= _windowSize)
                    pos += _windowSize;
                return _buffer[pos];
            }
        }
        #endregion

        #endregion

        #region CRC
        private class CRC
        {
            public static readonly uint[] Table;

            static CRC()
            {
                Table = new uint[256];
                const uint kPoly = 0xEDB88320;
                for (uint i = 0; i < 256; i++)
                {
                    uint r = i;
                    for (int j = 0; j < 8; j++)
                        if ((r & 1) != 0)
                            r = (r >> 1) ^ kPoly;
                        else
                            r >>= 1;
                    Table[i] = r;
                }
            }

            uint _value = 0xFFFFFFFF;

            public void Init() { _value = 0xFFFFFFFF; }

            public void UpdateByte(byte b)
            {
                _value = Table[(((byte)(_value)) ^ b)] ^ (_value >> 8);
            }

            public void Update(byte[] data, uint offset, uint size)
            {
                for (uint i = 0; i < size; i++)
                    _value = Table[(((byte)(_value)) ^ data[offset + i])] ^ (_value >> 8);
            }

            public uint GetDigest() { return _value ^ 0xFFFFFFFF; }

            static uint CalculateDigest(byte[] data, uint offset, uint size)
            {
                CRC crc = new CRC();
                crc.Update(data, offset, size);
                return crc.GetDigest();
            }

            static bool VerifyDigest(uint digest, byte[] data, uint offset, uint size)
            {
                return (CalculateDigest(data, offset, size) == digest);
            }
        }
        #endregion

        #region RangeCoder

        #region LZMARangeEncoder
        private class LZMARangeEncoder
        {
            public const uint kTopValue = (1 << 24);
            System.IO.Stream Stream;
            public UInt64 Low;
            public uint Range;
            uint _cacheSize;
            byte _cache;

            long StartPosition;

            public void SetStream(System.IO.Stream stream)
            {
                Stream = stream;
            }

            public void ReleaseStream()
            {
                Stream = null;
            }

            public void Init()
            {
                StartPosition = Stream.Position;

                Low = 0;
                Range = 0xFFFFFFFF;
                _cacheSize = 1;
                _cache = 0;
            }

            public void FlushData()
            {
                for (int i = 0; i < 5; i++)
                    ShiftLow();
            }

            public void FlushStream()
            {
                Stream.Flush();
            }

            public void CloseStream()
            {
                Stream.Close();
            }

            public void Encode(uint start, uint size, uint total)
            {
                Low += start * (Range /= total);
                Range *= size;
                while (Range < kTopValue)
                {
                    Range <<= 8;
                    ShiftLow();
                }
            }

            public void ShiftLow()
            {
                if ((uint)Low < (uint)0xFF000000 || (uint)(Low >> 32) == 1)
                {
                    byte temp = _cache;
                    do
                    {
                        Stream.WriteByte((byte)(temp + (Low >> 32)));
                        temp = 0xFF;
                    } while (--_cacheSize != 0);

                    _cache = (byte)(((uint)Low) >> 24);
                }
                _cacheSize++;
                Low = ((uint)Low) << 8;
            }

            public void EncodeDirectBits(uint v, int numTotalBits)
            {
                for (int i = numTotalBits - 1; i >= 0; i--)
                {
                    Range >>= 1;
                    if (((v >> i) & 1) == 1)
                        Low += Range;
                    if (Range < kTopValue)
                    {
                        Range <<= 8;
                        ShiftLow();
                    }
                }
            }

            public void EncodeBit(uint size0, int numTotalBits, uint symbol)
            {
                uint newBound = (Range >> numTotalBits) * size0;
                if (symbol == 0)
                    Range = newBound;
                else
                {
                    Low += newBound;
                    Range -= newBound;
                }
                while (Range < kTopValue)
                {
                    Range <<= 8;
                    ShiftLow();
                }
            }

            public long GetProcessedSizeAdd()
            {
                return _cacheSize + Stream.Position - StartPosition + 4;
            }
        }
        #endregion

        #region LZMARangeDecoder
        private class LZMARangeDecoder
        {
            public const uint kTopValue = (1 << 24);
            public uint Range;
            public uint Code;
            public System.IO.Stream Stream;

            public void Init(System.IO.Stream stream)
            {
                Stream = stream;

                Code = 0;
                Range = 0xFFFFFFFF;
                for (int i = 0; i < 5; i++)
                    Code = (Code << 8) | (byte)Stream.ReadByte();
            }

            public void ReleaseStream()
            {
                Stream = null;
            }

            public void CloseStream()
            {
                Stream.Close();
            }

            public void Normalize()
            {
                while (Range < kTopValue)
                {
                    Code = (Code << 8) | (byte)Stream.ReadByte();
                    Range <<= 8;
                }
            }

            public void Normalize2()
            {
                if (Range < kTopValue)
                {
                    Code = (Code << 8) | (byte)Stream.ReadByte();
                    Range <<= 8;
                }
            }

            public uint GetThreshold(uint total)
            {
                return Code / (Range /= total);
            }

            public void Decode(uint start, uint size, uint total)
            {
                Code -= start * Range;
                Range *= size;
                Normalize();
            }

            public uint DecodeDirectBits(int numTotalBits)
            {
                uint range = Range;
                uint code = Code;
                uint result = 0;
                for (int i = numTotalBits; i > 0; i--)
                {
                    range >>= 1;
                    uint t = (code - range) >> 31;
                    code -= range & (t - 1);
                    result = (result << 1) | (1 - t);

                    if (range < kTopValue)
                    {
                        code = (code << 8) | (byte)Stream.ReadByte();
                        range <<= 8;
                    }
                }
                Range = range;
                Code = code;
                return result;
            }

            public uint DecodeBit(uint size0, int numTotalBits)
            {
                uint newBound = (Range >> numTotalBits) * size0;
                uint symbol;
                if (Code < newBound)
                {
                    symbol = 0;
                    Range = newBound;
                }
                else
                {
                    symbol = 1;
                    Code -= newBound;
                    Range -= newBound;
                }
                Normalize();
                return symbol;
            }
        }
        #endregion

        #region LZMARangeBitEncoder
        private struct LZMARangeBitEncoder
        {
            public const int kNumBitModelTotalBits = 11;
            public const uint kBitModelTotal = (1 << kNumBitModelTotalBits);
            const int kNumMoveBits = 5;
            const int kNumMoveReducingBits = 2;
            public const int kNumBitPriceShiftBits = 6;

            uint Prob;

            public void Init() { Prob = kBitModelTotal >> 1; }

            public void UpdateModel(uint symbol)
            {
                if (symbol == 0)
                    Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
                else
                    Prob -= (Prob) >> kNumMoveBits;
            }

            public void Encode(LZMARangeEncoder encoder, uint symbol)
            {
                // encoder.EncodeBit(Prob, kNumBitModelTotalBits, symbol);
                // UpdateModel(symbol);
                uint newBound = (encoder.Range >> kNumBitModelTotalBits) * Prob;
                if (symbol == 0)
                {
                    encoder.Range = newBound;
                    Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
                }
                else
                {
                    encoder.Low += newBound;
                    encoder.Range -= newBound;
                    Prob -= (Prob) >> kNumMoveBits;
                }
                if (encoder.Range < LZMARangeEncoder.kTopValue)
                {
                    encoder.Range <<= 8;
                    encoder.ShiftLow();
                }
            }

            private static UInt32[] ProbPrices = new UInt32[kBitModelTotal >> kNumMoveReducingBits];

            static LZMARangeBitEncoder()
            {
                const int kNumBits = (kNumBitModelTotalBits - kNumMoveReducingBits);
                for (int i = kNumBits - 1; i >= 0; i--)
                {
                    UInt32 start = (UInt32)1 << (kNumBits - i - 1);
                    UInt32 end = (UInt32)1 << (kNumBits - i);
                    for (UInt32 j = start; j < end; j++)
                        ProbPrices[j] = ((UInt32)i << kNumBitPriceShiftBits) +
                            (((end - j) << kNumBitPriceShiftBits) >> (kNumBits - i - 1));
                }
            }

            public uint GetPrice(uint symbol)
            {
                return ProbPrices[(((Prob - symbol) ^ ((-(int)symbol))) & (kBitModelTotal - 1)) >> kNumMoveReducingBits];
            }
            public uint GetPrice0() { return ProbPrices[Prob >> kNumMoveReducingBits]; }
            public uint GetPrice1() { return ProbPrices[(kBitModelTotal - Prob) >> kNumMoveReducingBits]; }
        }
        #endregion

        #region LZMARangeBitDecoder
        private struct LZMARangeBitDecoder
        {
            public const int kNumBitModelTotalBits = 11;
            public const uint kBitModelTotal = (1 << kNumBitModelTotalBits);
            const int kNumMoveBits = 5;

            uint Prob;

            public void UpdateModel(int numMoveBits, uint symbol)
            {
                if (symbol == 0)
                    Prob += (kBitModelTotal - Prob) >> numMoveBits;
                else
                    Prob -= (Prob) >> numMoveBits;
            }

            public void Init() { Prob = kBitModelTotal >> 1; }

            public uint Decode(LZMARangeDecoder rangeDecoder)
            {
                uint newBound = (uint)(rangeDecoder.Range >> kNumBitModelTotalBits) * (uint)Prob;
                if (rangeDecoder.Code < newBound)
                {
                    rangeDecoder.Range = newBound;
                    Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
                    if (rangeDecoder.Range < LZMARangeDecoder.kTopValue)
                    {
                        rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
                        rangeDecoder.Range <<= 8;
                    }
                    return 0;
                }
                else
                {
                    rangeDecoder.Range -= newBound;
                    rangeDecoder.Code -= newBound;
                    Prob -= (Prob) >> kNumMoveBits;
                    if (rangeDecoder.Range < LZMARangeDecoder.kTopValue)
                    {
                        rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
                        rangeDecoder.Range <<= 8;
                    }
                    return 1;
                }
            }
        }
        #endregion

        #region LZMARangeBitTreeEncoder
        private struct LZMARangeBitTreeEncoder
        {
            LZMARangeBitEncoder[] Models;
            int NumBitLevels;

            public LZMARangeBitTreeEncoder(int numBitLevels)
            {
                NumBitLevels = numBitLevels;
                Models = new LZMARangeBitEncoder[1 << numBitLevels];
            }

            public void Init()
            {
                for (uint i = 1; i < (1 << NumBitLevels); i++)
                    Models[i].Init();
            }

            public void Encode(LZMARangeEncoder rangeEncoder, UInt32 symbol)
            {
                UInt32 m = 1;
                for (int bitIndex = NumBitLevels; bitIndex > 0; )
                {
                    bitIndex--;
                    UInt32 bit = (symbol >> bitIndex) & 1;
                    Models[m].Encode(rangeEncoder, bit);
                    m = (m << 1) | bit;
                }
            }

            public void ReverseEncode(LZMARangeEncoder rangeEncoder, UInt32 symbol)
            {
                UInt32 m = 1;
                for (UInt32 i = 0; i < NumBitLevels; i++)
                {
                    UInt32 bit = symbol & 1;
                    Models[m].Encode(rangeEncoder, bit);
                    m = (m << 1) | bit;
                    symbol >>= 1;
                }
            }

            public UInt32 GetPrice(UInt32 symbol)
            {
                UInt32 price = 0;
                UInt32 m = 1;
                for (int bitIndex = NumBitLevels; bitIndex > 0; )
                {
                    bitIndex--;
                    UInt32 bit = (symbol >> bitIndex) & 1;
                    price += Models[m].GetPrice(bit);
                    m = (m << 1) + bit;
                }
                return price;
            }

            public UInt32 ReverseGetPrice(UInt32 symbol)
            {
                UInt32 price = 0;
                UInt32 m = 1;
                for (int i = NumBitLevels; i > 0; i--)
                {
                    UInt32 bit = symbol & 1;
                    symbol >>= 1;
                    price += Models[m].GetPrice(bit);
                    m = (m << 1) | bit;
                }
                return price;
            }

            public static UInt32 ReverseGetPrice(LZMARangeBitEncoder[] Models, UInt32 startIndex,
                int NumBitLevels, UInt32 symbol)
            {
                UInt32 price = 0;
                UInt32 m = 1;
                for (int i = NumBitLevels; i > 0; i--)
                {
                    UInt32 bit = symbol & 1;
                    symbol >>= 1;
                    price += Models[startIndex + m].GetPrice(bit);
                    m = (m << 1) | bit;
                }
                return price;
            }

            public static void ReverseEncode(LZMARangeBitEncoder[] Models, UInt32 startIndex,
                LZMARangeEncoder rangeEncoder, int NumBitLevels, UInt32 symbol)
            {
                UInt32 m = 1;
                for (int i = 0; i < NumBitLevels; i++)
                {
                    UInt32 bit = symbol & 1;
                    Models[startIndex + m].Encode(rangeEncoder, bit);
                    m = (m << 1) | bit;
                    symbol >>= 1;
                }
            }
        }
        #endregion

        #region LZMARangeBitTreeDecoder
        private struct LZMARangeBitTreeDecoder
        {
            LZMARangeBitDecoder[] Models;
            int NumBitLevels;

            public LZMARangeBitTreeDecoder(int numBitLevels)
            {
                NumBitLevels = numBitLevels;
                Models = new LZMARangeBitDecoder[1 << numBitLevels];
            }

            public void Init()
            {
                for (uint i = 1; i < (1 << NumBitLevels); i++)
                    Models[i].Init();
            }

            public uint Decode(LZMARangeDecoder rangeDecoder)
            {
                uint m = 1;
                for (int bitIndex = NumBitLevels; bitIndex > 0; bitIndex--)
                    m = (m << 1) + Models[m].Decode(rangeDecoder);
                return m - ((uint)1 << NumBitLevels);
            }

            public uint ReverseDecode(LZMARangeDecoder rangeDecoder)
            {
                uint m = 1;
                uint symbol = 0;
                for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
                {
                    uint bit = Models[m].Decode(rangeDecoder);
                    m <<= 1;
                    m += bit;
                    symbol |= (bit << bitIndex);
                }
                return symbol;
            }

            public static uint ReverseDecode(LZMARangeBitDecoder[] Models, UInt32 startIndex,
                LZMARangeDecoder rangeDecoder, int NumBitLevels)
            {
                uint m = 1;
                uint symbol = 0;
                for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
                {
                    uint bit = Models[startIndex + m].Decode(rangeDecoder);
                    m <<= 1;
                    m += bit;
                    symbol |= (bit << bitIndex);
                }
                return symbol;
            }
        }
        #endregion

        #endregion

        #region ICoder

        #region ICodeProgress
        private abstract class ICodeProgress
        {
            /// <summary>
            /// Callback progress.
            /// </summary>
            /// <param name="inSize">
            /// input size. -1 if unknown.
            /// </param>
            /// <param name="outSize">
            /// output size. -1 if unknown.
            /// </param>
            public abstract void SetProgress(Int64 inSize, Int64 outSize);
        }
        #endregion

        #region ICoder
        private abstract class ICoder
        {
            /// <summary>
            /// Codes streams.
            /// </summary>
            /// <param name="inStream">
            /// input Stream.
            /// </param>
            /// <param name="outStream">
            /// output Stream.
            /// </param>
            /// <param name="inSize">
            /// input Size. -1 if unknown.
            /// </param>
            /// <param name="outSize">
            /// output Size. -1 if unknown.
            /// </param>
            /// <param name="progress">
            /// callback progress reference.
            /// </param>
            /// <exception cref="SevenZip.DataErrorException">
            /// if input stream is not valid
            /// </exception>
            public abstract void Code(Stream inStream, Stream outStream, Int64 inSize, Int64 outSize, ICodeProgress progress);
        }
        #endregion

        #region CoderPropID
        /// <summary>
        /// Provides the fields that represent properties idenitifiers for compressing.
        /// </summary>
        private enum CoderPropID
        {
            /// <summary>
            /// Specifies size of dictionary.
            /// </summary>
            DictionarySize = 0x400,
            /// <summary>
            /// Specifies size of memory for PPM*.
            /// </summary>
            UsedMemorySize,
            /// <summary>
            /// Specifies order for PPM methods.
            /// </summary>
            Order,
            /// <summary>
            /// Specifies number of postion state bits for LZMA (0 <= x <= 4).
            /// </summary>
            PosStateBits = 0x440,
            /// <summary>
            /// Specifies number of literal context bits for LZMA (0 <= x <= 8).
            /// </summary>
            LitContextBits,
            /// <summary>
            /// Specifies number of literal position bits for LZMA (0 <= x <= 4).
            /// </summary>
            LitPosBits,
            /// <summary>
            /// Specifies number of fast bytes for LZ*.
            /// </summary>
            NumFastBytes = 0x450,
            /// <summary>
            /// Specifies match finder. LZMA: "BT2", "BT4" or "BT4B".
            /// </summary>
            MatchFinder,
            /// <summary>
            /// Specifies number of passes.
            /// </summary>
            NumPasses = 0x460,
            /// <summary>
            /// Specifies number of algorithm.
            /// </summary>
            Algorithm = 0x470,
            /// <summary>
            /// Specifies multithread mode.
            /// </summary>
            MultiThread = 0x480,
            /// <summary>
            /// Specifies mode with end marker.
            /// </summary>
            EndMarker = 0x490
        }
        #endregion

        #region ISetCoderProperties
        private abstract class ISetCoderProperties
        {
            public abstract void SetCoderProperties(CoderPropID[] propIDs, object[] properties);
        }
        #endregion

        #region IWriteCoderProperties
        private abstract class IWriteCoderProperties
        {
            public abstract void WriteCoderProperties(System.IO.Stream outStream);
        }
        #endregion

        #region ISetDecoderProperties
        private abstract class ISetDecoderProperties
        {
            public abstract void SetDecoderProperties(byte[] properties);
        }
        #endregion

        #endregion
    }
}