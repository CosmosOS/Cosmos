using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Assembler.X86 {
    // todo: cache the EncodingOption and InstructionData instances..
    public abstract class Instruction : Indy.IL2CPU.Assembler.Instruction {
        [Flags]
        public enum InstructionSizes {
            None,
            Byte = 8,
            Word = 16,
            DWord = 32,
            QWord = 64,
            All = Byte | Word| DWord
        }

        public enum InstructionSize {
            None = 0,
            Byte = 8,
            Word = 16,
            DWord = 32,
            QWord = 64
        }
        [Flags]
        public enum OperandMemoryKinds { 
            Default = Address | IndirectReg | IndirectRegOffset,
            Address = 1,
            IndirectReg = 2,
            IndirectRegOffset = 4
        }
        public class InstructionData {
            public class InstructionEncodingOption {
                public Action<byte[], Instruction> ModifyBytes;

                public byte[] OpCode;

                /// <summary>
                /// If ModR/M byte needed, set to true. If true, all other fields on <see cref="InstructionEncodingOption"/>
                /// which refer to <see cref="OpCode"/> bytes, can assume an extra ModRM byte.
                /// </summary>
                public bool NeedsModRMByte;
                public byte InitialModRMByteValue;
                public bool ReverseRegisters = false;

                public InstructionSizes AllowedSizes = InstructionSizes.All;
                public InstructionSize DefaultSize = InstructionSize.DWord;
                /// <summary>
                /// the indx in OpCode where the OperandSize bit is encoded
                /// </summary>
                public byte? OperandSizeByte;
                /// <summary>
                /// the amount of bits the operandsize bit gets shifted to left, if neccessary
                /// </summary>
                public byte OperandSizeBitShiftLeft;

                /// <summary>
                /// is this EncodingOption valid for situations where the Destination is a register?
                /// if so, DestinationReg == Guid.Empty. if it is specific to a given register, the 32bit id is put in DestinationReg
                /// </summary>
                public Guid? DestinationReg;
                /// <summary>
                /// the index in OpCode where the DestinationReg bit is encoded
                /// </summary>
                public sbyte? DestinationRegByte;
                /// <summary>
                /// the amount of bits the DestinationReg bits gets shifted to left, if neccessary
                /// </summary>
                public byte DestinationRegBitShiftLeft;


                /// <summary>
                /// is this EncodingOption valid for situations where the Destination operand is memory?
                /// </summary>
                public bool DestinationMemory;
                public OperandMemoryKinds DestinationMemoryKinds = OperandMemoryKinds.Default;
                /// <summary>
                /// is this EncodingOption valid for situations where the Destination is an immediate value
                /// </summary>
                public bool DestinationImmediate;
                public InstructionSize DestinationImmediateSize = InstructionSize.None;

                /// <summary>
                /// is this EncodingOption valid for situations where the Source is a register?
                /// if so, SourceReg == Guid.Empty. if it is specific to a given register, the 32bit id is put in SourceReg
                /// </summary>
                public Guid? SourceReg;
                /// <summary>
                /// the index in OpCode where the SourceReg bit is encoded
                /// </summary>
                public sbyte? SourceRegByte;
                /// <summary>
                /// the amount of bits the SourceReg bits gets shifted to left, if necessary
                /// </summary>
                public byte SourceRegBitShiftLeft;
                /// <summary>
                /// is this EncodingOption valid for situations where the Source operand is memory?
                /// </summary>
                public bool SourceMemory;
                public OperandMemoryKinds SourceMemoryKinds = OperandMemoryKinds.Default;
                /// <summary>
                /// is this EncodingOption valid for situations where the Source is an immediate value
                /// </summary>
                public bool SourceImmediate;
                public InstructionSize SourceImmediateSize = InstructionSize.None;

            }

            public bool HasDestinationOperand;
            public bool HasSourceOperand;

            /// <summary>
            /// True if by default large (32bit), false if small (16bit)
            /// </summary>
            //public InstructionSize DefaultSize = InstructionSize.DWord;
            public List<InstructionEncodingOption> EncodingOptions = new List<InstructionEncodingOption>();
        }

        private static SortedList<Type, InstructionData> mInstructionDatas;
        private static ReaderWriterLocker mInstructionDatasLocker;
        static Instruction() {
            mInstructionDatasLocker = new ReaderWriterLocker();
            using (mInstructionDatasLocker.AcquireWriterLock()) {
                mInstructionDatas = new SortedList<Type, InstructionData>(new TypeComparer());
                foreach (Type xType in typeof(Instruction).Assembly.GetTypes()) {
                    if (!xType.IsSubclassOf(typeof(Instruction))) {
                        continue;
                    }
                    if (!xType.Namespace.StartsWith(typeof(Instruction).Namespace)) {
                        continue;
                    }
                    if (xType.IsAbstract) {
                        continue;
                    }
                    var xAttrib = xType.GetCustomAttributes(typeof(OpCodeAttribute), true).FirstOrDefault() as OpCodeAttribute;
                    if (xAttrib == null) {
                        continue;
                    }
                    var xNewInstructionData = new InstructionData();
                    mInstructionDatas.Add(xType, xNewInstructionData);
                    if (xType.IsSubclassOf(typeof(InstructionWithDestination))) {
                        xNewInstructionData.HasDestinationOperand = true;
                    }
                    if (xType.IsSubclassOf(typeof(InstructionWithDestinationAndSource))) {
                        xNewInstructionData.HasDestinationOperand = true;
                        xNewInstructionData.HasSourceOperand = true;
                    }
                    var xMethod = xType.GetMethod("InitializeEncodingData", new Type[] { typeof(InstructionData) });
                    if (xMethod != null) {
                        xMethod.Invoke(null, new object[] { xNewInstructionData });
                    }
                }
            }
        }

        protected Instruction() {
        }

        public static bool HasEncodingOptions(Type aInstruction) {
            using(mInstructionDatasLocker.AcquireReaderLock()) {
                if(!mInstructionDatas.ContainsKey(aInstruction)) {
                    return false;
                }
                return mInstructionDatas[aInstruction].EncodingOptions.Count > 0;
            }
        }
        protected static string SizeToString(byte aSize) {
            switch (aSize) {
                case 8:
                    return "byte";
                case 16:
                    return "word";
                case 32:
                    return "dword";
                case 64:
                    return "qword";
                default:
                    throw new Exception("Invalid size: " + aSize);
            }
        }

        private static bool GetEffectiveInstructionInfo(Instruction aInstruction, IInstructionWithDestination aInstructionWithDestination, IInstructionWithSize aInstructionWithSize, IInstructionWithSource aInstructionWithSource, out InstructionData aInstructionData, out InstructionData.InstructionEncodingOption aEncodingOption) {
            if (aInstruction.mEncodingOption != null) {
                aEncodingOption = aInstruction.mEncodingOption;
                aInstructionData = aInstruction.mInstructionData;
                return true;
                
            }
            using (mInstructionDatasLocker.AcquireReaderLock()) {
                mInstructionDatas.TryGetValue(aInstruction.GetType(), out aInstructionData);
            }
            if (aInstructionData == null) {
                aEncodingOption = null;
                return false;
            }
            aEncodingOption = null;
            if (aInstruction.ToString() == "mov dword EAX, 0x0") {
                Console.Write("");
            }
            for (int i = 0; i < aInstructionData.EncodingOptions.Count; i++) {
                var xEncodingOption = aInstructionData.EncodingOptions[i];
                if(aInstructionWithSize!=null) {
                    if(((byte)xEncodingOption.AllowedSizes & aInstructionWithSize.Size) != aInstructionWithSize.Size ) {
                        continue;
                    }
                }
#region Check Destination
                if (aInstructionWithDestination != null) {
                    if (!(((xEncodingOption.DestinationMemory || xEncodingOption.DestinationReg.HasValue) && (aInstructionWithDestination.DestinationReg != Guid.Empty || aInstructionWithDestination.DestinationValue.HasValue)) ||
                         (!(xEncodingOption.DestinationMemory || xEncodingOption.DestinationReg.HasValue) && aInstructionWithDestination.DestinationReg == Guid.Empty && aInstructionWithDestination.DestinationValue.HasValue))) {
                        // mismatch
                        continue;
                    }
                    if ((!((xEncodingOption.DestinationMemory && (aInstructionWithDestination.DestinationValue != null && aInstructionWithDestination.DestinationIsIndirect)) ||
                          (!xEncodingOption.DestinationMemory && (aInstructionWithDestination.DestinationValue == null && !aInstructionWithDestination.DestinationIsIndirect))) &&
                         !((xEncodingOption.DestinationMemory && (aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationIsIndirect)) ||
                          (!xEncodingOption.DestinationMemory && (aInstructionWithDestination.DestinationReg != Guid.Empty && !aInstructionWithDestination.DestinationIsIndirect)))) && aInstructionWithDestination.DestinationIsIndirect) {
                        continue;
                    }
                    if (!((xEncodingOption.DestinationImmediate && aInstructionWithDestination.DestinationValue != null && !aInstructionWithDestination.DestinationIsIndirect) ||
                        (!xEncodingOption.DestinationImmediate && (aInstructionWithDestination.DestinationValue == null || aInstructionWithDestination.DestinationIsIndirect)))) {
                        continue;
                    }
                    if (xEncodingOption.DestinationReg.HasValue && xEncodingOption.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationIsIndirect == xEncodingOption.DestinationMemory) {
                        if (xEncodingOption.DestinationReg != aInstructionWithDestination.DestinationReg) {
                            switch (xEncodingOption.DefaultSize) {
                                case InstructionSize.Byte: {
                                        if ((xEncodingOption.AllowedSizes & InstructionSizes.Byte) != 0) {
                                            var xTheActualReg = Registers.Get8BitRegistersForRegister(aInstructionWithDestination.DestinationReg);
                                            if (xTheActualReg != xEncodingOption.DestinationReg.Value) {
                                                continue;
                                            }
                                        }
                                        break;
                                    }
                                case InstructionSize.Word: {
                                        if ((xEncodingOption.AllowedSizes & InstructionSizes.Word) != 0) {
                                            var xTheActualReg = Registers.Get16BitRegisterForRegister(aInstructionWithDestination.DestinationReg);
                                            if (xTheActualReg != xEncodingOption.DestinationReg.Value) {
                                                continue;
                                            }
                                        }
                                        break;
                                    }
                                case InstructionSize.DWord: {
                                        if ((xEncodingOption.AllowedSizes & InstructionSizes.DWord) != 0) {
                                            var xTheActualReg = Registers.Get32BitRegisterForRegister(aInstructionWithDestination.DestinationReg);
                                            if (xTheActualReg != xEncodingOption.DestinationReg.Value) {
                                                continue;
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    throw new Exception("InstructionSize not implemented yet!");
                            }
                        }
                    }
                    if ((aInstructionWithDestination.DestinationReg != Guid.Empty && !aInstructionWithDestination.DestinationIsIndirect) &&
                        !xEncodingOption.DestinationReg.HasValue) {
                        continue;
                    }
                    if (xEncodingOption.DestinationMemory) {
                        if (((xEncodingOption.DestinationMemoryKinds & OperandMemoryKinds.IndirectReg) == 0) && aInstructionWithDestination.DestinationReg != Guid.Empty) {
                            continue;
                        }
                        if (((xEncodingOption.DestinationMemoryKinds & OperandMemoryKinds.IndirectRegOffset) == 0) &&
                            aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationDisplacement > 0) {
                            continue;
                        }
                    }
                    //if(!((xEncodingOption.DestinationReg.HasValue && aInstructionWithDestination.DestinationReg != Guid.Empty && (aInstructionWithDestination.DestinationIsIndirect == xEncodingOption.DestinationMemory)) ||
                    //    (!(xEncodingOption.DestinationReg.HasValue && (aInstructionWithDestination.DestinationReg == Guid.Empty && (aInstructionWithDestination.DestinationIsIndirect == xEncodingOption.DestinationMemory)))))) {
                    //    continue;
                    //}
                    //     (!(xEncodingOption.DestinationReg.HasValue && (aInstructionWithDestination.DestinationReg==Guid.Empty) && aInstructionWithDestination.DestinationIsIndirect))
                    //    )) {
                    //    continue;
                    //}
                }
#endregion Check Destination
#region Check Source
                if (aInstructionWithSource != null) {
                    if (!(((xEncodingOption.SourceMemory || xEncodingOption.SourceReg.HasValue) && (aInstructionWithSource.SourceReg != Guid.Empty || aInstructionWithSource.SourceValue.HasValue)) ||
                         (!(xEncodingOption.SourceMemory || xEncodingOption.SourceReg.HasValue) && aInstructionWithSource.SourceReg == Guid.Empty && aInstructionWithSource.SourceValue.HasValue))) {
                        // mismatch
                        continue;
                    }

                    if ((!((xEncodingOption.SourceMemory && (aInstructionWithSource.SourceValue != null && aInstructionWithSource.SourceIsIndirect)) ||
                          (!xEncodingOption.SourceMemory && (aInstructionWithSource.SourceValue == null && !aInstructionWithSource.SourceIsIndirect))) &&
                         !((xEncodingOption.SourceMemory && (aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect)) ||
                          (!xEncodingOption.SourceMemory && (aInstructionWithSource.SourceReg != Guid.Empty && !aInstructionWithSource.SourceIsIndirect)))) && aInstructionWithSource.SourceIsIndirect) {
                        continue;
                    }
                    if (!((xEncodingOption.SourceImmediate && aInstructionWithSource.SourceValue != null && !aInstructionWithSource.SourceIsIndirect) ||
                        (!xEncodingOption.SourceImmediate && (aInstructionWithSource.SourceValue == null || aInstructionWithSource.SourceIsIndirect)))) {
                        continue;
                    }
                    if (xEncodingOption.SourceMemory) {
                        if (((xEncodingOption.SourceMemoryKinds & OperandMemoryKinds.IndirectReg) == 0)  && aInstructionWithSource.SourceReg != Guid.Empty) {
                            continue;
                        }
                        if (((xEncodingOption.SourceMemoryKinds & OperandMemoryKinds.IndirectRegOffset) == 0) &&
                            aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceDisplacement > 0) {
                            continue;
                        }
                        if (!aInstructionWithSource.SourceIsIndirect) {
                            continue;
                        }
                    }
                    if (xEncodingOption.SourceReg.HasValue && xEncodingOption.SourceReg != Guid.Empty && aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect == xEncodingOption.SourceMemory) {
                        if (xEncodingOption.SourceReg != aInstructionWithSource.SourceReg) {
                            switch (xEncodingOption.DefaultSize) {
                                case InstructionSize.Byte: {
                                        if ((xEncodingOption.AllowedSizes & InstructionSizes.Byte) != 0) {
                                            var xTheActualReg = Registers.Get8BitRegistersForRegister(aInstructionWithSource.SourceReg);
                                            if (xTheActualReg != xEncodingOption.SourceReg.Value) {
                                                continue;
                                            }
                                        }
                                        break;
                                    }
                                case InstructionSize.Word: {
                                        if ((xEncodingOption.AllowedSizes & InstructionSizes.Word) != 0) {
                                            var xTheActualReg = Registers.Get16BitRegisterForRegister(aInstructionWithSource.SourceReg);
                                            if (xTheActualReg != xEncodingOption.SourceReg.Value) {
                                                continue;
                                            }
                                        }
                                        break;
                                    }
                                case InstructionSize.DWord: {
                                        if ((xEncodingOption.AllowedSizes & InstructionSizes.DWord) != 0) {
                                            var xTheActualReg = Registers.Get32BitRegisterForRegister(aInstructionWithSource.SourceReg);
                                            if (xTheActualReg != xEncodingOption.SourceReg.Value) {
                                                continue;
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    throw new Exception("InstructionSize not implemented yet!");
                            }
                        }
                    }
                }
#endregion
                aEncodingOption = xEncodingOption;
                break;
            }
            if (aEncodingOption == null) {
                mDebugGetEffectiveEncoding = false;
                //GetEffectiveInstructionInfo(aInstruction, aInstructionWithDestination, aInstructionWithSize, aInstructionWithSource, out aInstructionData, out aEncodingOption);
                throw new Exception("No valid EncodingOption found!");
            }
            aInstruction.mInstructionData = aInstructionData;
            aInstruction.mEncodingOption = aEncodingOption;
            return true;
        }
        private static bool mDebugGetEffectiveEncoding = true;

        private static bool DetermineSize(Indy.IL2CPU.Assembler.Assembler aAssembler, out ulong aSize, Instruction aInstruction, IInstructionWithDestination aInstructionWithDestination, IInstructionWithSize aInstructionWithSize, IInstructionWithSource aInstructionWithSource, InstructionData aInstructionData, InstructionData.InstructionEncodingOption aEncodingOption) {
            aSize = 0;
            var xInstrWithPrefixes = aInstruction as IInstructionWithPrefix;
            if (xInstrWithPrefixes != null) {
                if ((xInstrWithPrefixes.Prefixes & InstructionPrefixes.Repeat) != 0) {
                    aSize++;
                }
                if ((xInstrWithPrefixes.Prefixes & InstructionPrefixes.Lock) != 0) {
                    throw new NotImplementedException();
                }
            }
            if (aInstructionWithSize != null) {
                if (aEncodingOption.DefaultSize == InstructionSize.DWord && aInstructionWithSize.Size == 16) {
                    aSize++;
                }
                if (aEncodingOption.DefaultSize == InstructionSize.Word && aInstructionWithSize.Size == 32) {
                    aSize++;
                }
                if (aEncodingOption.DefaultSize == InstructionSize.Byte && aInstructionWithSize.Size == 16) {
                    aSize++;
                }
            }
            aSize += (ulong)aEncodingOption.OpCode.LongLength;
            #region ModRM byte
            if (aEncodingOption.NeedsModRMByte) {
                aSize++;
                if (aInstructionWithDestination != null) {
                    byte? xSIB = null;
                    if (!(aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty)) {
                        if (aInstructionWithDestination.DestinationReg != Guid.Empty) {
                            if (aInstructionWithDestination.DestinationReg == Registers.ESP && aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement == 0) {
                                xSIB = 0x24;
                            }
                        }
                    }
                    if (aInstructionWithDestination.DestinationIsIndirect) {
                        if (((aInstructionWithDestination.DestinationReg == Registers.EBP || aInstructionWithDestination.DestinationReg == Registers.ESP) && (aInstructionWithDestination.DestinationIsIndirect) && aInstructionWithDestination.DestinationDisplacement == 0)) {
                            if (aInstructionWithDestination.DestinationReg == Registers.EBP) {
                                xSIB = 0;
                            }
                            if (aInstructionWithDestination.DestinationReg == Registers.ESP) {
                                aSize++;
                                xSIB = 0x24;
                            }
                        } else {
                            bool xHandled = false;
                            if (!(aInstructionWithSource != null &&
                                ((aInstructionWithSource.SourceReg == Registers.EBP && !(aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect)) ||
                                 aInstructionWithSource.SourceReg == Registers.ESP))
                                ) {
                                if (aInstructionWithDestination.DestinationValue != null && aInstructionWithDestination.DestinationReg != Guid.Empty) {
                                    aSize += 4;
                                    xHandled = true;
                                }
                            }
                            if (!xHandled && aInstructionWithDestination != null && aInstructionWithSource != null &&
                                    aInstructionWithDestination.DestinationValue.HasValue && aInstructionWithDestination.DestinationIsIndirect &&
                                    aInstructionWithSource.SourceReg != Guid.Empty && !aInstructionWithSource.SourceIsIndirect) {
                                aSize+= 4;
                                xHandled = true;
                            }
                            if (!xHandled && aInstructionWithSource == null && aInstructionWithDestination.DestinationValue.HasValue) {
                                aSize+= 4;
                                xHandled = true;
                            }
                            if (!xHandled && aInstructionWithSource != null && aInstructionWithDestination.DestinationValue.HasValue &&
                                aInstructionWithDestination.DestinationIsIndirect && aInstructionWithSource.SourceValue.HasValue && !aInstructionWithSource.SourceIsIndirect) {
                                aSize += 4;
                                xHandled = true;
                            }
                        }
                    }

                    if (aInstructionWithSource != null && aInstructionWithSource.SourceIsIndirect) {
                        //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= EncodeRegister(aInstructionWithSource.SourceReg);
                        if (((aInstructionWithSource.SourceReg == Registers.EBP || aInstructionWithSource.SourceReg == Registers.ESP) && (aInstructionWithSource.SourceIsIndirect))) {
                            if (aInstructionWithSource.SourceReg == Registers.EBP && aInstructionWithSource.SourceDisplacement == 0) {
                                aSize++;
                                xSIB = 0;
                            }
                            if (aInstructionWithSource.SourceReg == Registers.ESP) {
                                aSize++;
                                xSIB = 0x24;
                            }
                        } else {
                            if (!(aInstructionWithSource != null &&
                                ((aInstructionWithSource.SourceReg == Registers.EBP && !(aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect)) ||
                                 aInstructionWithSource.SourceReg == Registers.ESP)) && (aInstructionWithSource.SourceReg == Guid.Empty && aInstructionWithSource.SourceIsIndirect)) {
                                aSize+= 4;
                            }
                        }
                    }

                    if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationReg == Registers.EBP && aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement == 0) {
                        aSize++;
                    }
                    if (aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationIsIndirect &&
                        aInstructionWithDestination.DestinationDisplacement > 0) {
                        var xSIBOffset = 0;
                        if (aInstructionWithDestination.DestinationReg == Registers.ESP) {
                            xSIB = 0x24;
                        }
                        if (xSIB != null) {
                            //xExtraOffset++;
                            aSize++;
                            xSIB = null;
                        }
                        //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 3 << 6;
                        // todo: optimize for different displacement sizes
                        if (aInstructionWithDestination.DestinationDisplacement < 128) {
                            aSize++;                            
                        } else {
                            aSize += 4;
                        }
                    } else {
                        if (aInstructionWithDestination.DestinationReg == Registers.ESP && aInstructionWithDestination.DestinationIsIndirect &&
                            aInstructionWithDestination.DestinationDisplacement == 0 && aInstructionWithSource == null) {
                            //aSize++;
                        }
                    }

                    if (aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect && aInstructionWithSource.SourceDisplacement > 0) {
                        var xSIBOffset = 0;
                        if (aInstructionWithSource.SourceReg == Registers.ESP) {
                            xSIB = 0x24;
                        }
                        if (xSIB != null) {
                            //aSize++;
                            xSIB = null;
                        }
                        if (aInstructionWithSource.SourceDisplacement < 128) {
                            aSize+= 1;
                        } else {
                            aSize+= 4;
                        }
                        //}
                        if (xSIB != null) {
                            aSize++;
                        }
                    } else {
                        if (aInstructionWithSource != null) {
                            if (xSIB != null) {
                                //xExtraOffset++;
                                //aSize++;
                                // todo: nie tnodig?
                            }
                        }
                    }
                }
                //EncodeModRMByte(aInstruction.DestinationReg, aInstruction.DestinationIsIndirect, aInstruction.DestinationDisplacement > 0, aInstruction.DestinationDisplacement > 255, out xSIB);
            } else {
                //if (aInstructionWithDestination != null) {
                //    if (aEncodingOption.DestinationRegByte.HasValue) {
                //        xBuffer[xExtraOffset + aEncodingOption.DestinationRegByte.Value] |= (byte)(EncodeRegister(aInstructionWithDestination.DestinationReg) << aEncodingOption.DestinationRegBitShiftLeft);
                //    }
                //}
                //if (aInstructionWithSource != null) {
                //    if (aEncodingOption.SourceRegByte.HasValue && aEncodingOption.SourceRegByte.Value > -1) {
                //        xBuffer[xExtraOffset + aEncodingOption.SourceRegByte.Value] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << aEncodingOption.SourceRegBitShiftLeft);
                //    }
                //}
            }
#endregion ModRM byte
            if (aInstructionWithDestination != null) {
                if (aInstructionWithDestination.DestinationValue.HasValue && !aInstructionWithDestination.DestinationIsIndirect) {
                    if (aEncodingOption.NeedsModRMByte) {
                        //aSize++;
                    }
                    var xInstrSize = 0;
                    if (aInstructionWithSize != null) {
                        xInstrSize = aInstructionWithSize.Size / 8;
                    } else {
                        //                        throw new NotImplementedException("size not known");
                        xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                    }
                    if (aEncodingOption.DestinationImmediateSize != InstructionSize.None) {
                        xInstrSize = ((byte)aEncodingOption.DestinationImmediateSize) / 8;
                    }
                    aSize += (ulong)xInstrSize;
                } else {
                    if (aInstructionWithDestination.DestinationValue.HasValue && !aInstructionWithDestination.DestinationIsIndirect) {
                        int xInstrSize = 0;
                        if (aInstructionWithSize != null) {
                            xInstrSize = aInstructionWithSize.Size / 8;
                        } else {
                            //                        throw new NotImplementedException("size not known");
                            xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                        }
                        if (aEncodingOption.DestinationImmediateSize != InstructionSize.None) {
                            xInstrSize = ((byte)aEncodingOption.DestinationImmediateSize) / 8;
                        }
                        aSize += (ulong)xInstrSize;
                    }
                    if (aInstructionWithDestination.DestinationValue.HasValue && aInstructionWithDestination.DestinationIsIndirect && aEncodingOption.DestinationMemory && !aEncodingOption.NeedsModRMByte) {
                        int xInstrSize = 0;
                        if (aInstructionWithSize != null) {
                            xInstrSize = aInstructionWithSize.Size / 8;
                        } else {
                            //                        throw new NotImplementedException("size not known");
                            xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                        }
                        if (aEncodingOption.DestinationImmediateSize != InstructionSize.None) {
                            xInstrSize = ((byte)aEncodingOption.DestinationImmediateSize) / 8;
                        }
                        aSize += (ulong)xInstrSize;
                    }
                }
            }
            // todo: add more options
            if (aInstructionWithSource != null) {
                if (aInstructionWithSource.SourceValue.HasValue && !aInstructionWithSource.SourceIsIndirect) {
                    if (aEncodingOption.NeedsModRMByte) {
                        //aSize++;
                        // todo: niet nodig?
                    }
                    int xInstrSize = 0;
                    if (aInstructionWithSize != null) {
                        xInstrSize = aInstructionWithSize.Size / 8;
                    } else {
                        //                        throw new NotImplementedException("size not known");
                        xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                    }
                    if (aEncodingOption.SourceImmediateSize != InstructionSize.None) {
                        xInstrSize = ((byte)aEncodingOption.SourceImmediateSize) / 8;
                    }
                    aSize += (ulong)xInstrSize;
                }
                if (aInstructionWithSource.SourceValue.HasValue && aInstructionWithSource.SourceIsIndirect && aEncodingOption.SourceMemory && !aEncodingOption.NeedsModRMByte) {
                    int xInstrSize = 0;
                    if (aInstructionWithSize != null) {
                        xInstrSize = aInstructionWithSize.Size / 8;
                    } else {
                        //                        throw new NotImplementedException("size not known");
                        xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                    }
                    if (aEncodingOption.SourceImmediateSize != InstructionSize.None) {
                        xInstrSize = ((byte)aEncodingOption.SourceImmediateSize) / 8;
                    }
                    aSize += (ulong)xInstrSize;
                }
            }
            if (aInstructionWithSize != null) {
                if (aEncodingOption.OperandSizeByte.HasValue) {
                    if (aInstructionWithSize.Size != 8) {
//                        xBuffer[aEncodingOption.OperandSizeByte.Value + xOpCodeOffset] |= (byte)(1 << aEncodingOption.OperandSizeBitShiftLeft);
                    }
                }
            }
            aInstruction.mDataSize = aSize;
            return true;
        }

        public override bool DetermineSize(Indy.IL2CPU.Assembler.Assembler aAssembler, out ulong aSize) {
            var xInstructionWithDestination = this as IInstructionWithDestination;
            var xInstructionWithSource = this as IInstructionWithSource;
            var xInstructionWithSize = this as IInstructionWithSize;
            InstructionData xInstructionData = null;
            InstructionData.InstructionEncodingOption xEncodingOption = null;
            if (!GetEffectiveInstructionInfo(this, xInstructionWithDestination, xInstructionWithSize, xInstructionWithSource, out xInstructionData, out xEncodingOption)) {
                return base.DetermineSize(aAssembler, out aSize);
            }
            var xResult = DetermineSize(aAssembler, out aSize, this, xInstructionWithDestination, xInstructionWithSize, xInstructionWithSource, xInstructionData, xEncodingOption);
            mDataSize = aSize;
            return xResult;
        }

        public override bool IsComplete(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            var xWithDest = this as IInstructionWithDestination;
            var xWithSource = this as IInstructionWithSource;
            var xWithSize = this as IInstructionWithSize;
            ulong xAddress;
            if (xWithDest != null) {
                if (xWithDest.DestinationRef != null && !xWithDest.DestinationRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
            }
            if (xWithSource != null) {
                if (xWithSource.SourceRef != null && !xWithSource.SourceRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
            }
            return true;
        }

        protected ulong? mDataSize;
        protected Instruction.InstructionData.InstructionEncodingOption mEncodingOption;
        protected InstructionData mInstructionData;
        public override ulong? ActualAddress {
            get {
                if (!StartAddress.HasValue) {
                    return null;
                }
                if (!mDataSize.HasValue) {
                    return null;
                }
                return StartAddress.Value + mDataSize.Value;
            }
        }

        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            var xInstructionWithDestination = this as IInstructionWithDestination;
            var xInstructionWithSource = this as IInstructionWithSource;
            var xInstructionWithSize = this as IInstructionWithSize;
            InstructionData xInstructionData = null;
            InstructionData.InstructionEncodingOption xEncodingOption = null;
            if (!GetEffectiveInstructionInfo(this, xInstructionWithDestination, xInstructionWithSize, xInstructionWithSource, out xInstructionData, out xEncodingOption)) {
                return base.GetData(aAssembler);
            }
            return GetData(aAssembler, this, xInstructionWithDestination, xInstructionWithSize, xInstructionWithSource, xInstructionData, xEncodingOption);
        }

        private static byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler, Instruction aInstruction, IInstructionWithDestination aInstructionWithDestination, IInstructionWithSize aInstructionWithSize, IInstructionWithSource aInstructionWithSource, InstructionData aInstructionData, InstructionData.InstructionEncodingOption aEncodingOption) {
            try {
                var xBuffer = new byte[aInstruction.mDataSize.Value];
                if (xBuffer.Length == 0) {
                    return xBuffer;
                }
                int xExtraOffset = 0;
                int xOpCodeOffset = 0;
                
                var xInstrWithPrefixes = aInstruction as IInstructionWithPrefix;
                if (xInstrWithPrefixes != null) {
                    if ((xInstrWithPrefixes.Prefixes & InstructionPrefixes.Repeat) != 0) {
                        xOpCodeOffset += 1;
                        xBuffer[xExtraOffset] = 0xF3;
                        xExtraOffset++;
                    }
                    if ((xInstrWithPrefixes.Prefixes & InstructionPrefixes.Lock) != 0) {
                        throw new NotImplementedException();
                    }
                }
                if (aInstructionWithSize != null) {
                    if (aEncodingOption.DefaultSize == InstructionSize.DWord && aInstructionWithSize.Size == 16) {
                        xOpCodeOffset += 1;
                        xBuffer[xExtraOffset] = 0x66;
                        xExtraOffset++;
                    }
                    if (aEncodingOption.DefaultSize == InstructionSize.Word && aInstructionWithSize.Size == 32) {
                        xOpCodeOffset += 1;
                        xBuffer[xExtraOffset] = 0x66;
                        xExtraOffset++;
                    }
                    if (aEncodingOption.DefaultSize == InstructionSize.Byte && aInstructionWithSize.Size == 16) {
                        xOpCodeOffset += 1;
                        xBuffer[xExtraOffset] = 0x66;
                        xExtraOffset++;
                    }
                }
                Array.Copy(aEncodingOption.OpCode, 0, xBuffer, xExtraOffset, aEncodingOption.OpCode.Length);
                if (aInstructionWithDestination != null) {
                    if (aInstructionWithDestination.DestinationReg != Guid.Empty && aEncodingOption.DestinationRegByte.HasValue && aEncodingOption.DestinationRegByte.Value >-1 && !aEncodingOption.NeedsModRMByte) {
                        xBuffer[aEncodingOption.DestinationRegByte.Value + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithDestination.DestinationReg) << aEncodingOption.DestinationRegBitShiftLeft);
                    }
                }
                if (aInstruction is IInstructionWithCondition) {
                    var xCond = (IInstructionWithCondition)aInstruction;
                    if (aEncodingOption.OpCode.Length == 1) {
                        xBuffer[xExtraOffset] |= (byte)xCond.Condition;
                    } else {
                        if (aEncodingOption.OpCode.Length == 2) {
                            xBuffer[xExtraOffset + 1] |= (byte)xCond.Condition;
                        } else {
                            throw new NotImplementedException();
                        }
                    }
                }
                //if (aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty) {
                //    var xIdx = aEncodingOption.OpCode.Length + xExtraOffset;
                //    if(xIdx >= xBuffer.Length) {
                //        xIdx = xBuffer.Length - 1;
                //    }
                //    xBuffer[xIdx] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << 3);
                //}

                if (aEncodingOption.NeedsModRMByte) {
                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] = aEncodingOption.InitialModRMByteValue;
                    if (aInstructionWithDestination != null) {
                        byte? xSIB = null;
                        if (aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty) {
                            if (aInstructionWithDestination.DestinationReg != Guid.Empty) {
                                if (aEncodingOption.ReverseRegisters) {
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= EncodeRegister(aInstructionWithDestination.DestinationReg);
                                    if (!aEncodingOption.SourceRegByte.HasValue) {
                                        xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << 3);
                                    }
                                } else {
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithDestination.DestinationReg) << 3);
                                    if (!aEncodingOption.SourceRegByte.HasValue) {
                                        xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg));
                                    }
                                }
                            } else {
                                if (!aEncodingOption.SourceRegByte.HasValue) {
                                    if (aEncodingOption.ReverseRegisters) {
                                        xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << 3);
                                    } else {
                                        xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg));
                                    }
                                }
                            }
                        } else {
                            if (aInstructionWithDestination.DestinationReg != Guid.Empty) {
                                if (aEncodingOption.ReverseRegisters) {
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithDestination.DestinationReg));
                                } else {
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithDestination.DestinationReg) << 3);
                                }
                                if (aInstructionWithDestination.DestinationReg == Registers.ESP && aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement == 0) {
                                    xSIB = 0x24;                                    
                                }
                            }
                        }
                        if (aInstructionWithDestination.DestinationIsIndirect) {
                            if (((aInstructionWithDestination.DestinationReg == Registers.EBP || aInstructionWithDestination.DestinationReg == Registers.ESP) && (aInstructionWithDestination.DestinationIsIndirect) && aInstructionWithDestination.DestinationDisplacement == 0)) {
                                if (aInstructionWithDestination.DestinationReg == Registers.EBP) {
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 1 << 6;
                                    xSIB = 0;
                                }
                                if (aInstructionWithDestination.DestinationReg == Registers.ESP) {
                                    xExtraOffset++;
                                    xSIB = 0x24;
                                }
                            } else {
                                bool xHandled = false;
                                if (!(aInstructionWithSource != null &&
                                    ((aInstructionWithSource.SourceReg == Registers.EBP && !(aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect)) ||
                                     aInstructionWithSource.SourceReg == Registers.ESP))
                                    ) {
                                    if (aInstructionWithDestination.DestinationValue != null && aInstructionWithDestination.DestinationReg != Guid.Empty) {
                                        // todo: fix for 16bit mode, it should then be 0x36
                                        //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;
                                        //ulong xAddress = 0;
                                        //if (!(aInstructionWithDestination.DestinationRef != null && aInstructionWithDestination.DestinationRef.Resolve(aAssembler, out xAddress))) {
                                        //    if (aInstructionWithDestination.DestinationValue.HasValue) {
                                        //        xAddress = aInstructionWithDestination.DestinationValue.Value;
                                        //    }
                                        //}
                                        //xAddress += (ulong)aInstructionWithDestination.DestinationDisplacement;
                                        //Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                        //xExtraOffset += 4;
                                        // todo: fix for 16bit mode, it should then be 0x36
                                        xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;

                                        ulong xAddress = 0;
                                        if (!(aInstructionWithDestination.DestinationRef != null && aInstructionWithDestination.DestinationRef.Resolve(aAssembler, out xAddress))) {
                                            if (aInstructionWithDestination.DestinationValue.HasValue) {
                                                xAddress = aInstructionWithDestination.DestinationValue.Value;
                                            }
                                        }
                                        xAddress += (ulong)aInstructionWithDestination.DestinationDisplacement;
                                        Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                        xExtraOffset += 4;
                                        xHandled = true;
                                    }
                                }
                                if (!xHandled && aInstructionWithDestination != null && aInstructionWithSource != null &&
                                        aInstructionWithDestination.DestinationValue.HasValue && aInstructionWithDestination.DestinationIsIndirect &&
                                        aInstructionWithSource.SourceReg != Guid.Empty && !aInstructionWithSource.SourceIsIndirect) {
                                    if (!aEncodingOption.SourceRegByte.HasValue) {
                                        if (aEncodingOption.ReverseRegisters) {
                                            xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << 3);
                                        } else {
                                            xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg));
                                        }
                                    }
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;
                                    ulong xAddress = 0;
                                    if (!(aInstructionWithDestination.DestinationRef != null && aInstructionWithDestination.DestinationRef.Resolve(aAssembler, out xAddress))) {
                                        if (aInstructionWithDestination.DestinationValue.HasValue) {
                                            xAddress = aInstructionWithDestination.DestinationValue.Value;
                                        }
                                    }
                                    xAddress += (ulong)aInstructionWithDestination.DestinationDisplacement;
                                    Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                    xExtraOffset += 4;
                                    xHandled = true;
                                }
                                if (!xHandled && aInstructionWithSource == null && aInstructionWithDestination.DestinationValue.HasValue) {
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;
                                    ulong xAddress = 0;
                                    if (!(aInstructionWithDestination.DestinationRef != null && aInstructionWithDestination.DestinationRef.Resolve(aAssembler, out xAddress))) {
                                        if (aInstructionWithDestination.DestinationValue.HasValue) {
                                            xAddress = aInstructionWithDestination.DestinationValue.Value;
                                        }
                                    }
                                    xAddress += (ulong)aInstructionWithDestination.DestinationDisplacement;
                                    Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                    xExtraOffset += 4;
                                    xHandled = true;
                                }
                                if (!xHandled && aInstructionWithSource != null && aInstructionWithDestination.DestinationValue.HasValue &&
                                    aInstructionWithDestination.DestinationIsIndirect && aInstructionWithSource.SourceValue.HasValue && !aInstructionWithSource.SourceIsIndirect) {
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;
                                    ulong xAddress = 0;
                                    if (!(aInstructionWithDestination.DestinationRef != null && aInstructionWithDestination.DestinationRef.Resolve(aAssembler, out xAddress))) {
                                        if (aInstructionWithDestination.DestinationValue.HasValue) {
                                            xAddress = aInstructionWithDestination.DestinationValue.Value;
                                        }
                                    }
                                    xAddress += (ulong)aInstructionWithDestination.DestinationDisplacement;
                                    Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                    xExtraOffset += 4;
                                    xHandled = true;
                                }
                            }
                        }

                        if (aInstructionWithSource != null && aInstructionWithSource.SourceIsIndirect) {
                            //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= EncodeRegister(aInstructionWithSource.SourceReg);
                            if (((aInstructionWithSource.SourceReg == Registers.EBP || aInstructionWithSource.SourceReg == Registers.ESP) && (aInstructionWithSource.SourceIsIndirect))) {
                                if (aInstructionWithSource.SourceReg == Registers.EBP && aInstructionWithSource.SourceDisplacement == 0) {
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 1 << 6;
                                    xExtraOffset++;
                                    xSIB = 0;
                                }
                                if (aInstructionWithSource.SourceReg == Registers.ESP) {
                                    if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationReg != Guid.Empty && !aInstructionWithDestination.DestinationIsIndirect) {
                                        //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= EncodeRegister(aInstructionWithDestination.DestinationReg);
                                    }
                                    xExtraOffset++;
                                    xSIB = 0x24;
                                }
                            } else {
                                if (!(aInstructionWithSource != null &&
                                    ((aInstructionWithSource.SourceReg == Registers.EBP && !(aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect)) ||
                                     aInstructionWithSource.SourceReg == Registers.ESP)) && (aInstructionWithSource.SourceReg == Guid.Empty && aInstructionWithSource.SourceIsIndirect)) {
                                
                                    // todo: fix for 16bit mode, it should then be 0x36
                                    //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;
                                    //ulong xAddress = 0;
                                    //if (!(aInstructionWithSource.SourceRef != null && aInstructionWithSource.SourceRef.Resolve(aAssembler, out xAddress))) {
                                    //    if (aInstructionWithSource.SourceValue.HasValue) {
                                    //        xAddress = aInstructionWithSource.SourceValue.Value;
                                    //    }
                                    //}
                                    //xAddress += (ulong)aInstructionWithSource.SourceDisplacement;
                                    //Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                    //xExtraOffset += 4;
                                    // todo: fix for 16bit mode, it should then be 0x36
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;

                                    ulong xAddress = 0;
                                    if (!(aInstructionWithSource.SourceRef != null && aInstructionWithSource.SourceRef.Resolve(aAssembler, out xAddress))) {
                                        if (aInstructionWithSource.SourceValue.HasValue) {
                                            xAddress = aInstructionWithSource.SourceValue.Value;
                                        }
                                    }
                                    xAddress += (ulong)aInstructionWithSource.SourceDisplacement;
                                    Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                    xExtraOffset += 4;
                                }
                            }
                        }

                        if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationReg == Registers.EBP && aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement == 0) {
                            xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x40;
                            xExtraOffset++;
                        }
                        //}
                        if (aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty && aEncodingOption.SourceRegByte != null && aEncodingOption.SourceRegByte.Value != -1) {
                            xBuffer[aEncodingOption.OpCode.Length + xExtraOffset - 1 + aEncodingOption.SourceRegByte.Value] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << aEncodingOption.SourceRegBitShiftLeft);
                        }
                        //SBArray.Resize(ref xBuffer, xBuffer.Length + 1);
                        if (aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationIsIndirect &&
                            aInstructionWithDestination.DestinationDisplacement > 0) {
                            var xSIBOffset = 0;
                            if (aInstructionWithDestination.DestinationReg == Registers.ESP) {
                                xSIB = 0x24;
                            }
                            if (xSIB != null) {
                                //xExtraOffset++;
                                xSIBOffset = 1;
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset + xSIBOffset] = xSIB.Value;
                                xSIB = null;
                            }
                            int xCorrecting = 0;
                            if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithSource != null && (/*aInstructionWithSource.SourceReg == Registers.EBP || */aInstructionWithSource.SourceReg == Registers.ESP)) {
                                xCorrecting = -1;
                            }
                            //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 3 << 6;
                            // todo: optimize for different displacement sizes
                            if (aInstructionWithDestination.DestinationDisplacement < 128) {
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 2 << 5; // for now use 8bit value               
                                Array.Copy(BitConverter.GetBytes((byte)aInstructionWithDestination.DestinationDisplacement), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + xSIBOffset+1, 1);
                                xExtraOffset += 1;
                            } else {
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 2 << 6; // for now use 8bit value
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] &= 0xBF; // clear the 1 << 6
                                Array.Copy(BitConverter.GetBytes(aInstructionWithDestination.DestinationDisplacement), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + xSIBOffset + 1, 4);
                                xExtraOffset += 4;
                            }
                            //}
                            if (xSIB != null) {
                                xExtraOffset++;
                            }
                        } else {
                            if (aInstructionWithDestination.DestinationReg == Registers.ESP && aInstructionWithDestination.DestinationIsIndirect &&
                                aInstructionWithDestination.DestinationDisplacement == 0 && aInstructionWithSource==null) {
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] = 0x24;
                                xExtraOffset++;
                            }
                        }

                        if (aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect && aInstructionWithSource.SourceDisplacement > 0) {
                            var xSIBOffset = 0;
                            if (aInstructionWithSource.SourceReg == Registers.ESP) {
//                                xExtraOffset++;
                                xSIB = 0x24;
                            }
                            if (xSIB != null) {
                                if (aInstructionWithSource.SourceReg == Registers.ESP) {
                                //    xSIBOffset = 1;
                                //    xExtraOffset++;
                                }
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset + xSIBOffset] = xSIB.Value;
                                xSIB = null;
                            }
                            //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 3 << 6;
                            // todo: optimize for different displacement sizes
                            int xCorrecting = 0;
                            if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationReg !=  Guid.Empty && (/*aInstructionWithSource.SourceReg == Registers.EBP || */aInstructionWithSource.SourceReg == Registers.ESP) ){
                                xCorrecting = -1;
                            }
                            if (aInstructionWithSource.SourceDisplacement < 128) {
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 2 << 5; // for now use 8bit value
                                Array.Copy(BitConverter.GetBytes((byte)aInstructionWithSource.SourceDisplacement), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + xSIBOffset + 1, 1);
                                xExtraOffset += 1;
                            } else {
                                int xExtra = 0;
                                if ((aEncodingOption.OpCode.Length + xExtraOffset + 1 + 1) < xBuffer.Length && (aEncodingOption.OpCode.Length + xExtraOffset + 1 + 3) < xBuffer.Length) {
                                    xExtra = 1;
                                }
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset + xCorrecting] |= 2 << 6; // for now use 8bit value
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset + xCorrecting] &= 0xBF; // clear the 1 << 6
                                Array.Copy(BitConverter.GetBytes(aInstructionWithSource.SourceDisplacement), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + xExtra + xSIBOffset, 4);
                                xExtraOffset += 4;
                            }
                            //}
                            if (xSIB != null) {
                                xExtraOffset++;
                            }
                        } else {
                            if (aInstructionWithSource != null) {
                                if (xSIB != null) {
                                    //xExtraOffset++;
                                    xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] = xSIB.Value;
                                }
                            }

                        }
                    }
                    //EncodeModRMByte(aInstruction.DestinationReg, aInstruction.DestinationIsIndirect, aInstruction.DestinationDisplacement > 0, aInstruction.DestinationDisplacement > 255, out xSIB);
                } else {
                    if (aInstructionWithDestination != null) {
                        if (aEncodingOption.DestinationRegByte.HasValue && aEncodingOption.DestinationRegByte.Value> -1) {
                            xBuffer[xExtraOffset + aEncodingOption.DestinationRegByte.Value] |= (byte)(EncodeRegister(aInstructionWithDestination.DestinationReg) << aEncodingOption.DestinationRegBitShiftLeft);
                        }
                    }
                    if (aInstructionWithSource != null) {
                        if (aEncodingOption.SourceRegByte.HasValue && aEncodingOption.SourceRegByte.Value > -1) {
                            xBuffer[xExtraOffset + aEncodingOption.SourceRegByte.Value] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << aEncodingOption.SourceRegBitShiftLeft);
                        }
                    }
                }
                if (aInstructionWithDestination != null) {
                    if (aInstructionWithDestination.DestinationValue.HasValue && !aInstructionWithDestination.DestinationIsIndirect) {
                        int xOffset = aEncodingOption.OpCode.Length + xExtraOffset;
                        if (aEncodingOption.NeedsModRMByte) {
                            xOffset++;
                        }
                        var xInstrSize = 0;
                        var xValue = aInstructionWithDestination.DestinationValue.Value;
                        if (aInstructionWithSize != null) {
                            xInstrSize = aInstructionWithSize.Size / 8;
                        } else {
                            //                        throw new NotImplementedException("size not known");
                            xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                        }
                        if (aEncodingOption.DestinationImmediateSize != InstructionSize.None) {
                            xInstrSize = ((byte)aEncodingOption.DestinationImmediateSize) / 8;
                        }
                        Array.Copy(BitConverter.GetBytes(xValue), 0, xBuffer, xOffset, xInstrSize);
                    } else {
                        if (aInstructionWithDestination.DestinationValue.HasValue && !aInstructionWithDestination.DestinationIsIndirect) {
                            int xOffset = aEncodingOption.OpCode.Length + xExtraOffset;
                            if (aEncodingOption.NeedsModRMByte) {
                                xOffset++;
                            }
                            int xInstrSize = 0;
                            if (aInstructionWithSize != null) {
                                xInstrSize = aInstructionWithSize.Size / 8;
                            } else {
                                //                        throw new NotImplementedException("size not known");
                                xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                            }
                            if (aEncodingOption.DestinationImmediateSize != InstructionSize.None) {
                                xInstrSize = ((byte)aEncodingOption.DestinationImmediateSize) / 8;
                            }
                            Array.Copy(BitConverter.GetBytes(aInstructionWithDestination.DestinationValue.Value), 0, xBuffer, xOffset, xInstrSize);
                        }
                        if (aInstructionWithDestination.DestinationValue.HasValue && aInstructionWithDestination.DestinationIsIndirect && aEncodingOption.DestinationMemory && !aEncodingOption.NeedsModRMByte) {
                            int xOffset = aEncodingOption.OpCode.Length + xExtraOffset;
                            int xInstrSize = 0;
                            if (aInstructionWithSize != null) {
                                xInstrSize = aInstructionWithSize.Size / 8;
                            } else {
                                //                        throw new NotImplementedException("size not known");
                                xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                            }
                            if (aEncodingOption.DestinationImmediateSize != InstructionSize.None) {
                                xInstrSize = ((byte)aEncodingOption.DestinationImmediateSize) / 8;
                            }
                            var xAddress = aInstructionWithDestination.DestinationValue.Value;
                            xAddress += (uint)aInstructionWithDestination.DestinationDisplacement;
                            Array.Copy(BitConverter.GetBytes(xAddress), 0, xBuffer, xOffset, xInstrSize);
                        }
                    }
                }
                // todo: add more options
                if (aInstructionWithSource != null) {
                    if (aInstructionWithSource.SourceValue.HasValue && !aInstructionWithSource.SourceIsIndirect) {
                        int xOffset = aEncodingOption.OpCode.Length + xExtraOffset;
                        if (aEncodingOption.NeedsModRMByte) {
                            xOffset++;
                        }
                        int xInstrSize = 0;
                        if (aInstructionWithSize != null) {
                            xInstrSize = aInstructionWithSize.Size / 8;
                        } else {
                            //                        throw new NotImplementedException("size not known");
                            xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                        }
                        if (aEncodingOption.SourceImmediateSize != InstructionSize.None) {
                            xInstrSize = ((byte)aEncodingOption.SourceImmediateSize) / 8;
                        }
                        if ((xOffset + xInstrSize) < (xBuffer.Length)) {
                            xOffset = xBuffer.Length - xInstrSize;
                        }
                        Array.Copy(BitConverter.GetBytes(aInstructionWithSource.SourceValue.Value), 0, xBuffer, xOffset, xInstrSize);
                    }
                    if (aInstructionWithSource.SourceValue.HasValue && aInstructionWithSource.SourceIsIndirect && aEncodingOption.SourceMemory && !aEncodingOption.NeedsModRMByte) {
                        int xOffset = aEncodingOption.OpCode.Length + xExtraOffset;
                        int xInstrSize = 0;
                        if (aInstructionWithSize != null) {
                            xInstrSize = aInstructionWithSize.Size / 8;
                        } else {
                            //                        throw new NotImplementedException("size not known");
                            xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                        }
                        if (aEncodingOption.SourceImmediateSize != InstructionSize.None) {
                            xInstrSize = ((byte)aEncodingOption.SourceImmediateSize) / 8;
                        }
                        var xAddress = aInstructionWithSource.SourceValue.Value;
                        xAddress += (uint)aInstructionWithSource.SourceDisplacement;
                        if ((xOffset + xInstrSize) < (xBuffer.Length)) {
                            xOffset = xBuffer.Length - xInstrSize;
                        }
                        Array.Copy(BitConverter.GetBytes(xAddress), 0, xBuffer, xOffset, xInstrSize);
                    }
                }
                if (aInstructionWithSize != null) {
                    if (aEncodingOption.OperandSizeByte.HasValue) {
                        if (aInstructionWithSize.Size != 8) {
                            xBuffer[aEncodingOption.OperandSizeByte.Value + xOpCodeOffset] |= (byte)(1 << aEncodingOption.OperandSizeBitShiftLeft);
                        }
                    }
                }

                if (aEncodingOption.ModifyBytes != null) {
                    aEncodingOption.ModifyBytes(xBuffer, aInstruction);
                }
                //
                return xBuffer;
            } catch (Exception E) {
                throw new Exception("Error while generating binary data", E);
            }
        }

        private static byte EncodeRegister(Guid aRegister) {
            // todo: implement support for other registers
            if (aRegister == Registers.EAX) return 0x0;
            if (aRegister == Registers.ECX) return 0x1;
            if (aRegister == Registers.EDX) return 0x2;
            if (aRegister == Registers.EBX) return 0x3;
            if (aRegister == Registers.ESP) return 0x4;
            if (aRegister == Registers.EBP) return 0x5;
            if (aRegister == Registers.ESI) return 0x6;
            if (aRegister == Registers.EDI) return 0x7;

            if (aRegister == Registers.AX) return 0x0;
            if (aRegister == Registers.CX) return 0x1;
            if (aRegister == Registers.DX) return 0x2;
            if (aRegister == Registers.BX) return 0x3;
            if (aRegister == Registers.SP) return 0x4;
            if (aRegister == Registers.BP) return 0x5;
            if (aRegister == Registers.SI) return 0x6;
            if (aRegister == Registers.DI) return 0x7;

            if (aRegister == Registers.AL) return 0x0;
            if (aRegister == Registers.CL) return 0x1;
            if (aRegister == Registers.DL) return 0x2;
            if (aRegister == Registers.BL) return 0x3;
            if (aRegister == Registers.AH) return 0x4;
            if (aRegister == Registers.CH) return 0x5;
            if (aRegister == Registers.DH) return 0x6;
            if (aRegister == Registers.BH) return 0x7;
            if (aRegister == Registers.CR0) return 0x0;
            if (aRegister == Registers.CR2) return 0x2;
            if (aRegister == Registers.CR3) return 0x3;
            if (aRegister == Registers.CR4) return 0x4;

            if (aRegister == Registers.ES) return 0x0;
            if (aRegister == Registers.CS) return 0x1;
            if (aRegister == Registers.SS) return 0x2;
            if (aRegister == Registers.DS) return 0x3;
            if (aRegister == Registers.FS) return 0x4;
            if (aRegister == Registers.GS) return 0x5;

            throw new Exception("Register not supported!");
        }


        /*
        private static bool SizeIsSelected(InstructionSizes aSizes, byte aSize) {
            switch(aSize) {
                case 8: return (aSizes & InstructionSizes.Byte) != 0;
                case 16: return (aSizes & InstructionSizes.Byte) != 0;
                case 32: return (aSizes & InstructionSizes.Byte) != 0;
                default: throw new NotImplementedException();
            }
        }

        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            InstructionData xInstructionData = null;
            using (mInstructionDatasLocker.AcquireReaderLock()) {
                mInstructionDatas.TryGetValue(this.GetType(), out xInstructionData);
            }
            if (xInstructionData == null) {
                return base.GetData(aAssembler);
            }
            var xWithDestAndSourceAndSize = this as InstructionWithDestinationAndSourceAndSize;
            if (xWithDestAndSourceAndSize != null) {
                return GetData(aAssembler, xWithDestAndSourceAndSize, xInstructionData);
            }
            var xWithDestAndSource = this as InstructionWithDestinationAndSource;
            if (xWithDestAndSource != null) {
                return base.GetData(aAssembler);
            }
            var xWithDestAndSize = this as InstructionWithDestinationAndSize;
            if (xWithDestAndSize != null) {
                return base.GetData(aAssembler);
            }
            var xWithDest = this as InstructionWithDestination;
            if (xWithDest != null) {
                return base.GetData(aAssembler);
            }
            if (xInstructionData.EncodingOptions.Count > 0) {
                // todo: improve
                return xInstructionData.EncodingOptions[0].OpCode;
            }
            return null;
        }

        private static byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler, InstructionWithDestinationAndSourceAndSize aInstruction, InstructionData aInstructionData) {
            var xEncodingOption = GetInstructionEncodingOption(aInstruction, aInstructionData);
            if(aInstruction.ToString() == "mov word [ESP], 0x47") {
             Console.Write("");   
            }
            ulong xSize = 0;
            Instruction.DetermineSize(aInstruction, aInstructionData, out xSize);
            if(xSize==0) {
                return new byte[0];
            }
            var xBuffer = new byte[xSize];
            int xExtraOffset = 0;
            int xOpCodeOffset = 0;
            if (xEncodingOption.DefaultSize == InstructionSize.DWord && aInstruction.Size == 16) {
                xOpCodeOffset += 1;
                xExtraOffset++;
                xBuffer[0] = 0x66;
            }
            if (xEncodingOption.DefaultSize == InstructionSize.Word && aInstruction.Size == 32) {
                xOpCodeOffset += 1;
                xExtraOffset++;
                xBuffer[0] = 0x66;
            }
            Array.Copy(xEncodingOption.OpCode, 0, xBuffer, xExtraOffset, xEncodingOption.OpCode.Length);
            byte? xSIB = null;
            if (xEncodingOption.NeedsModRMByte) {
                xBuffer[xEncodingOption.OpCode.Length + xExtraOffset] = EncodeModRMByte(aInstruction.DestinationReg, aInstruction.DestinationIsIndirect, aInstruction.DestinationDisplacement > 0, aInstruction.DestinationDisplacement > 255, out xSIB);
                //byte 
                // = EncodeModRMByte()
                //if(aInstruction.DestinationReg != Guid.Empty && !aInstruction.DestinationIsIndirect) {
                //    xBuffer[xEncodingOption.OpCode.Length] |= 0xC0;    
                //}
                //if(aInstruction.DestinationReg != Guid.Empty && aInstruction.DestinationIsIndirect) {
                //    //
                //}
                // todo: add more ModRM stuff
            }
            if (aInstruction.DestinationReg != Guid.Empty && xEncodingOption.DestinationRegByte.HasValue && !xEncodingOption.NeedsModRMByte) {
                xBuffer[xEncodingOption.DestinationRegByte.Value + xExtraOffset] |= (byte)(EncodeRegister(aInstruction.DestinationReg) << xEncodingOption.DestinationRegBitShiftLeft);
            }
            if (aInstruction.SourceReg != Guid.Empty && xEncodingOption.SourceRegByte.HasValue) {
                xBuffer[xEncodingOption.SourceRegByte.Value + xExtraOffset] |= (byte)(EncodeRegister(aInstruction.SourceReg) << xEncodingOption.SourceRegBitShiftLeft);
            }
            if(xSIB!=null) {
                xBuffer[xEncodingOption.OpCode.Length + xExtraOffset + 1] = xSIB.Value;
                xExtraOffset++;
            }
            // todo: add more options
            if (aInstruction.SourceValue.HasValue) {
                int xOffset = xEncodingOption.OpCode.Length + xExtraOffset;
                if(xEncodingOption.NeedsModRMByte) {
                    xOffset++;
                }
                Array.Copy(BitConverter.GetBytes(aInstruction.SourceValue.Value), 0, xBuffer, xOffset, aInstruction.Size / 8);
            }
            if (xEncodingOption.OperandSizeByte.HasValue) {
                if (aInstruction.Size > 8) {
                    xBuffer[xEncodingOption.OperandSizeByte.Value + xOpCodeOffset] |= (byte)(1 << xEncodingOption.OperandSizeBitShiftLeft);
                }
            }


            //
            return xBuffer;
        }

        private static byte EncodeModRMByte(Guid aRegister, bool aIndirect, bool aOffset, bool aOffsetIs32Bit, out byte? aSIB) {
            byte xModRM = 0;
            xModRM |= EncodeRegister(aRegister);
            aSIB = null;
            if(!aOffset) {
                if (aRegister == Registers.EBP) {
                    xModRM |= 1 << 6;
                    aSIB = 0;
                }
                if(aRegister == Registers.ESP) {
                    aSIB = 0x24;
                }
            }
            if(aOffset) {
                throw new NotImplementedException("Add support for offsets");
            }
            return xModRM;
        }

        

        private static bool IsLargeRegister(Guid aRegister) {
            return Registers.Is32Bit(aRegister);
        }
         */
    }
}
