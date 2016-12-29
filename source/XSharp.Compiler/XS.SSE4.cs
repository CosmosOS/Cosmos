using Cosmos.Assembler.x86.SSE;
using static XSharp.Compiler.XSRegisters;
using static XSharp.Compiler.XS.SSE4.Rounding;

namespace XSharp.Compiler
{
  partial class XS
  {
    public static class SSE4
    {
      #region Rounding

      public static class Rounding
      {
        /// <summary>
        /// Processor behaviour for a precision exception.
        /// <para><see cref="Normal"/>: If source register has value of SNaN, a precision exception will be signaled.</para>
        /// <para><see cref="Inexact"/>: If source register has value of SNaN, a precision exception will not be signaled.</para>
        /// </summary>
        public enum RoundingPrecision : byte
        {
          Normal,
          Inexact
        }

        /// <summary>
        /// Source of the rounding mode control.
        /// <para><see cref="Imm8_RC"/>: The rounding mode is determined by the provided imm8 argument.</para>
        /// <para><see cref="MXCSR_RC"/>: The rounding mode is determined by the MXCSR register.</para>
        /// </summary>
        public enum RoundingSelect : byte
        {
          Imm8_RC = 0,
          MXCSR_RC = 1
        }

        /// <summary>
        /// Rounding Mode for round instructions.
        /// <para><see cref="RoundToNearest"/>: Rounded result is the closest to the infinitely precise result. If two values are equally close, the result is the even value (i.e., the integer value with the least-significant bit of zero).</para>
        /// <para><see cref="RoundDown"/>: Rounded result is closest to but no greater than the infinitely precise result.</para>
        /// <para><see cref="RoundUp"/>: Rounded result is closest to but no less than the infinitely precise result.</para>
        /// <para><see cref="RoundTowardZero"/>: Rounded result is closest to but no greater in absolute value than the infinitely precise result.</para>
        /// </summary>
        public enum RoundingMode : byte
        {
          RoundToNearest = 0,       // 0b00
          RoundDown = 1,            // 0b01
          RoundUp = 2,              // 0b10
          RoundTowardZero = 3       // 0b11
        }

        /// <summary>
        /// Calculates the byte for round instructions.
        /// </summary>
        /// <param name="precision">The rounding precision.</param>
        /// <param name="source">The rounding source.</param>
        /// <param name="mode">The rounding mode.</param>
        /// <returns>Returns the byte for round instructions.</returns>
        public static byte GetRoundingByte(RoundingPrecision precision, RoundingSelect source, RoundingMode mode)
        {
          return (byte)((byte)precision << 3 | (byte)source << 2 | (byte)mode);
        }
      }

      #endregion

      public static void RoundSS(RegisterXMM destination, RegisterXMM source, RoundingPrecision roundingPrecision = RoundingPrecision.Normal, RoundingSelect roundingSelect = RoundingSelect.Imm8_RC, RoundingMode roundingMode = RoundingMode.RoundToNearest)
      {
        new RoundSS()
        {
          DestinationReg = destination,
          SourceReg = source,
          ArgumentValue = (byte)roundingMode
        };
      }

      public static void RoundSD(RegisterXMM destination, RegisterXMM source, RoundingPrecision roundingPrecision = RoundingPrecision.Normal, RoundingSelect roundingSelect = RoundingSelect.Imm8_RC, RoundingMode roundingMode = RoundingMode.RoundToNearest)
      {
        new RoundSD()
        {
          DestinationReg = destination,
          SourceReg = source,
          ArgumentValue = GetRoundingByte(roundingPrecision, roundingSelect, roundingMode)
        };
      }
    }
  }
}
