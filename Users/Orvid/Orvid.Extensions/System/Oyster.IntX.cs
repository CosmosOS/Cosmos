/*

Copyright (c) 2005-2010, Andriy Kozachuk a.k.a. Oyster
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this
  list of conditions and the following disclaimer in the documentation and/or
  other materials provided with the distribution.

* Neither the name of Andriy Kozachuk nor the names of its contributors may be
  used to endorse or promote products derived from this software without
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

 
 PLEASE NOTE: This file has been heavily modified from it's original 
              version for use in this library.
 

*/


using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace System
{
	/// <summary>
	/// Numeric class which represents arbitrary-precision integers.
	/// (c) Andriy Kozachuk a.k.a. Oyster [dev.oyster@gmail.com] 2005-2010
	/// </summary>
	public sealed class IntX :
		IEquatable<IntX>, IEquatable<int>, IEquatable<uint>, IEquatable<long>, IEquatable<ulong>,
		IComparable, IComparable<IntX>, IComparable<int>, IComparable<uint>, IComparable<long>, IComparable<ulong>,
        IDisposable, ICloneable
    {

        #region Internal Classes

        #region IntXDivider
        internal static class IntXDivider
        {
            /// <summary>
            /// Returns true if it's better to use classic algorithm for given big integers.
            /// </summary>
            /// <param name="length1">First big integer length.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <returns>True if classic algorithm is better.</returns>
            private static bool IsClassicAlgorithmNeeded(uint length1, uint length2)
            {
                return
                    length1 < Constants.AutoNewtonLengthLowerBound || length2 < Constants.AutoNewtonLengthLowerBound ||
                    length1 > Constants.AutoNewtonLengthUpperBound || length2 > Constants.AutoNewtonLengthUpperBound;
            }

            /// <summary>
            /// Divides two big integers.
            /// Also modifies <paramref name="digits1" /> and <paramref name="length1"/> (it will contain remainder).
            /// </summary>
            /// <param name="digits1">First big integer digits.</param>
            /// <param name="digitsBuffer1">Buffer for first big integer digits. May also contain remainder. Can be null - in this case it's created if necessary.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digits2">Second big integer digits.</param>
            /// <param name="digitsBuffer2">Buffer for second big integer digits. Only temporarily used. Can be null - in this case it's created if necessary.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsRes">Resulting big integer digits.</param>
            /// <param name="resultFlags">Which operation results to return.</param>
            /// <param name="cmpResult">Big integers comparsion result (pass -2 if omitted).</param>
            /// <returns>Resulting big integer length.</returns>
            public static unsafe uint DivMod(uint[] digits1, uint[] digitsBuffer1, ref uint length1, uint[] digits2, uint[] digitsBuffer2, uint length2, uint[] digitsRes, DivModResultFlags resultFlags, int cmpResult)
            {
                // Maybe immediately use classic algorithm here
                if (IsClassicAlgorithmNeeded(length1, length2))
                {
                    return ClassicDivMod(digits1, digitsBuffer1, ref length1, digits2, digitsBuffer2, length2, digitsRes, resultFlags, cmpResult);
                }

                // Create some buffers if necessary
                if (digitsBuffer1 == null)
                {
                    digitsBuffer1 = new uint[length1 + 1];
                }

                fixed (uint* digitsPtr1 = digits1, digitsBufferPtr1 = digitsBuffer1, digitsPtr2 = digits2, digitsBufferPtr2 = digitsBuffer2 != null ? digitsBuffer2 : digits1, digitsResPtr = digitsRes != null ? digitsRes : digits1)
                {
                    return DivMod(
                        digitsPtr1,
                        digitsBufferPtr1,
                        ref length1,
                        digitsPtr2,
                        digitsBufferPtr2 == digitsPtr1 ? null : digitsBufferPtr2,
                        length2,
                        digitsResPtr == digitsPtr1 ? null : digitsResPtr,
                        resultFlags,
                        cmpResult);
                }
            }

            /// <summary>
            /// Divides two big integers.
            /// Also modifies <paramref name="digitsPtr1" /> and <paramref name="length1"/> (it will contain remainder).
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="digitsBufferPtr1">Buffer for first big integer digits. May also contain remainder.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digitsPtr2">Second big integer digits.</param>
            /// <param name="digitsBufferPtr2">Buffer for second big integer digits. Only temporarily used.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsResPtr">Resulting big integer digits.</param>
            /// <param name="resultFlags">Which operation results to return.</param>
            /// <param name="cmpResult">Big integers comparsion result (pass -2 if omitted).</param>
            /// <returns>Resulting big integer length.</returns>
            public static unsafe uint DivMod(uint* digitsPtr1, uint* digitsBufferPtr1, ref uint length1, uint* digitsPtr2, uint* digitsBufferPtr2, uint length2, uint* digitsResPtr, DivModResultFlags resultFlags, int cmpResult)
            {
                // Maybe immediately use classic algorithm here
                if (IsClassicAlgorithmNeeded(length1, length2))
                {
                    return ClassicDivMod(digitsPtr1, digitsBufferPtr1, ref length1, digitsPtr2, digitsBufferPtr2, length2, digitsResPtr, resultFlags, cmpResult);
                }

                // Call base (for special cases)
                uint resultLength = BaseDivMod(digitsPtr1, digitsBufferPtr1, ref length1, digitsPtr2, digitsBufferPtr2, length2, digitsResPtr, resultFlags, cmpResult);
                if (resultLength != uint.MaxValue)
                    return resultLength;


                // First retrieve opposite for the divider
                uint int2OppositeLength;
                ulong int2OppositeRightShift;
                uint[] int2OppositeDigits = NewtonHelper.GetIntegerOpposite(digitsPtr2, length2, length1, digitsBufferPtr1, out int2OppositeLength, out int2OppositeRightShift);

                // We will need to muptiply it by divident now to receive quotient.
                // Prepare digits for multiply result
                uint quotLength;
                uint[] quotDigits = new uint[length1 + int2OppositeLength];

                // Fix some arrays
                fixed (uint* oppositePtr = int2OppositeDigits, quotPtr = quotDigits)
                {
                    // Multiply
                    quotLength = IntXMultiplier.Multiply(oppositePtr, int2OppositeLength, digitsPtr1, length1, quotPtr);

                    // Calculate shift
                    uint shiftOffset = (uint)(int2OppositeRightShift / Constants.DigitBitCount);
                    int shiftCount = (int)(int2OppositeRightShift % Constants.DigitBitCount);

                    // Get the very first bit of the shifted part
                    uint highestLostBit;
                    if (shiftCount == 0)
                    {
                        highestLostBit = quotPtr[shiftOffset - 1] >> 31;
                    }
                    else
                    {
                        highestLostBit = quotPtr[shiftOffset] >> (shiftCount - 1) & 1U;
                    }

                    // After this result must be shifted to the right - this is required
                    quotLength = DigitOpHelper.Shr(quotPtr + shiftOffset, quotLength - shiftOffset, quotPtr, shiftCount, false);

                    // Maybe quotient must be corrected
                    if (highestLostBit == 1U)
                    {
                        quotLength = DigitOpHelper.Add(quotPtr, quotLength, &highestLostBit, 1U, quotPtr);
                    }

                    // Check quotient - finally it might be too big.
                    // For this we must multiply quotient by divider
                    uint quotDivLength;
                    uint[] quotDivDigits = new uint[quotLength + length2];
                    fixed (uint* quotDivPtr = quotDivDigits)
                    {
                        quotDivLength = IntXMultiplier.Multiply(quotPtr, quotLength, digitsPtr2, length2, quotDivPtr);

                        int cmpRes = DigitOpHelper.Cmp(quotDivPtr, quotDivLength, digitsPtr1, length1);
                        if (cmpRes > 0)
                        {
                            highestLostBit = 1;
                            quotLength = DigitOpHelper.Sub(quotPtr, quotLength, &highestLostBit, 1U, quotPtr);
                            quotDivLength = DigitOpHelper.Sub(quotDivPtr, quotDivLength, digitsPtr2, length2, quotDivPtr);
                        }

                        // Now everything is ready and prepared to return results

                        // First maybe fill remainder
                        if ((resultFlags & DivModResultFlags.Mod) != 0)
                        {
                            length1 = DigitOpHelper.Sub(digitsPtr1, length1, quotDivPtr, quotDivLength, digitsBufferPtr1);
                        }

                        // And finally fill quotient
                        if ((resultFlags & DivModResultFlags.Div) != 0)
                        {
                            DigitHelper.DigitsBlockCopy(quotPtr, digitsResPtr, quotLength);
                        }
                        else
                        {
                            quotLength = 0;
                        }

                        // Return some arrays to pool
                        ArrayPool<uint>.Instance.AddArray(int2OppositeDigits);

                        return quotLength;
                    }
                }
            }

            /// <summary>
            /// Divides one <see cref="IntX" /> by another.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second big integer.</param>
            /// <param name="modRes">Remainder big integer.</param>
            /// <param name="resultFlags">Which operation results to return.</param>
            /// <returns>Divident big integer.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="int1" /> or <paramref name="int2" /> is a null reference.</exception>
            /// <exception cref="DivideByZeroException"><paramref name="int2" /> equals zero.</exception>
            public static IntX DivMod(IntX int1, IntX int2, out IntX modRes, DivModResultFlags resultFlags)
            {
                // Null reference exceptions
                if (ReferenceEquals(int1, null))
                {
                    throw new ArgumentNullException("int1", "Operands can't be null.");
                }
                else if (ReferenceEquals(int2, null))
                {
                    throw new ArgumentNullException("int2", "Operands can't be null.");
                }

                // Check if int2 equals zero
                if (int2._length == 0)
                {
                    throw new DivideByZeroException();
                }

                // Get flags
                bool divNeeded = (resultFlags & DivModResultFlags.Div) != 0;
                bool modNeeded = (resultFlags & DivModResultFlags.Mod) != 0;

                // Special situation: check if int1 equals zero; in this case zero is always returned
                if (int1._length == 0)
                {
                    modRes = modNeeded ? new IntX() : null;
                    return divNeeded ? new IntX() : null;
                }

                // Special situation: check if int2 equals one - nothing to divide in this case
                if (int2._length == 1 && int2._digits[0] == 1)
                {
                    modRes = modNeeded ? new IntX() : null;
                    return divNeeded ? int2._negative ? -int1 : +int1 : null;
                }

                // Get resulting sign
                bool resultNegative = int1._negative ^ int2._negative;

                // Check if int1 > int2
                int compareResult = DigitOpHelper.Cmp(int1._digits, int1._length, int2._digits, int2._length);
                if (compareResult < 0)
                {
                    modRes = modNeeded ? new IntX(int1) : null;
                    return divNeeded ? new IntX() : null;
                }
                else if (compareResult == 0)
                {
                    modRes = modNeeded ? new IntX() : null;
                    return divNeeded ? new IntX(resultNegative ? -1 : 1) : null;
                }

                //
                // Actually divide here (by Knuth algorithm)
                //

                // Prepare divident (if needed)
                IntX divRes = null;
                if (divNeeded)
                {
                    divRes = new IntX(int1._length - int2._length + 1U, resultNegative);
                }

                // Prepare mod (if needed)
                if (modNeeded)
                {
                    modRes = new IntX(int1._length + 1U, int1._negative);
                }
                else
                {
                    modRes = null;
                }

                // Call procedure itself
                uint modLength = int1._length;
                uint divLength = DivMod(
                    int1._digits,
                    modNeeded ? modRes._digits : null,
                    ref modLength,
                    int2._digits,
                    null,
                    int2._length,
                    divNeeded ? divRes._digits : null,
                    resultFlags,
                    compareResult);

                // Maybe set new lengths and perform normalization
                if (divNeeded)
                {
                    divRes._length = divLength;
                    divRes.TryNormalize();
                }
                if (modNeeded)
                {
                    modRes._length = modLength;
                    modRes.TryNormalize();
                }

                // Return div
                return divRes;
            }

            /// <summary>
            /// Divides two big integers.
            /// Also modifies <paramref name="digits1" /> and <paramref name="length1"/> (it will contain remainder).
            /// </summary>
            /// <param name="digits1">First big integer digits.</param>
            /// <param name="digitsBuffer1">Buffer for first big integer digits. May also contain remainder. Can be null - in this case it's created if necessary.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digits2">Second big integer digits.</param>
            /// <param name="digitsBuffer2">Buffer for second big integer digits. Only temporarily used. Can be null - in this case it's created if necessary.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsRes">Resulting big integer digits.</param>
            /// <param name="resultFlags">Which operation results to return.</param>
            /// <param name="cmpResult">Big integers comparsion result (pass -2 if omitted).</param>
            /// <returns>Resulting big integer length.</returns>
            public static unsafe uint ClassicDivMod(uint[] digits1, uint[] digitsBuffer1, ref uint length1, uint[] digits2, uint[] digitsBuffer2, uint length2, uint[] digitsRes, DivModResultFlags resultFlags, int cmpResult)
            {
                // Create some buffers if necessary
                if (digitsBuffer1 == null)
                {
                    digitsBuffer1 = new uint[length1 + 1];
                }
                if (digitsBuffer2 == null)
                {
                    digitsBuffer2 = new uint[length2];
                }

                fixed (uint* digitsPtr1 = digits1, digitsBufferPtr1 = digitsBuffer1, digitsPtr2 = digits2, digitsBufferPtr2 = digitsBuffer2, digitsResPtr = digitsRes != null ? digitsRes : digits1)
                {
                    return DivMod(
                        digitsPtr1,
                        digitsBufferPtr1,
                        ref length1,
                        digitsPtr2,
                        digitsBufferPtr2,
                        length2,
                        digitsResPtr == digitsPtr1 ? null : digitsResPtr,
                        resultFlags,
                        cmpResult);
                }
            }

            /// <summary>
            /// Divides two big integers.
            /// Also modifies <paramref name="digitsPtr1" /> and <paramref name="length1"/> (it will contain remainder).
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="digitsBufferPtr1">Buffer for first big integer digits. May also contain remainder.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digitsPtr2">Second big integer digits.</param>
            /// <param name="digitsBufferPtr2">Buffer for second big integer digits. Only temporarily used.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsResPtr">Resulting big integer digits.</param>
            /// <param name="resultFlags">Which operation results to return.</param>
            /// <param name="cmpResult">Big integers comparsion result (pass -2 if omitted).</param>
            /// <returns>Resulting big integer length.</returns>
            public static unsafe uint ClassicDivMod(uint* digitsPtr1, uint* digitsBufferPtr1, ref uint length1, uint* digitsPtr2, uint* digitsBufferPtr2, uint length2, uint* digitsResPtr, DivModResultFlags resultFlags, int cmpResult)
            {
                // Call base (for special cases)
                uint resultLength = BaseDivMod(digitsPtr1, digitsBufferPtr1, ref length1, digitsPtr2, digitsBufferPtr2, length2, digitsResPtr, resultFlags, cmpResult);
                if (resultLength != uint.MaxValue)
                    return resultLength;

                bool divNeeded = (resultFlags & DivModResultFlags.Div) != 0;
                bool modNeeded = (resultFlags & DivModResultFlags.Mod) != 0;

                //
                // Prepare digitsBufferPtr1 and digitsBufferPtr2
                //

                int shift1 = 31 - Bits.Msb(digitsPtr2[length2 - 1]);
                if (shift1 == 0)
                {
                    // We don't need to shift - just copy
                    DigitHelper.DigitsBlockCopy(digitsPtr1, digitsBufferPtr1, length1);

                    // We also don't need to shift second digits
                    digitsBufferPtr2 = digitsPtr2;
                }
                else
                {
                    int rightShift1 = Constants.DigitBitCount - shift1;

                    // We do need to shift here - so copy with shift - suppose we have enough storage for this operation
                    length1 = DigitOpHelper.Shr(digitsPtr1, length1, digitsBufferPtr1 + 1, rightShift1, true) + 1U;

                    // Second digits also must be shifted
                    DigitOpHelper.Shr(digitsPtr2, length2, digitsBufferPtr2 + 1, rightShift1, true);
                }

                //
                // Division main algorithm implementation
                //

                ulong longDigit;
                ulong divEst;
                ulong modEst;

                ulong mulRes;
                uint divRes;
                long k, t;

                // Some digits2 cached digits
                uint lastDigit2 = digitsBufferPtr2[length2 - 1];
                uint preLastDigit2 = digitsBufferPtr2[length2 - 2];

                // Main divide loop
                bool isMaxLength;
                uint maxLength = length1 - length2;
                for (uint i = maxLength, iLen2 = length1, j, ji; i <= maxLength; --i, --iLen2)
                {
                    isMaxLength = iLen2 == length1;

                    // Calculate estimates
                    if (isMaxLength)
                    {
                        longDigit = digitsBufferPtr1[iLen2 - 1];
                    }
                    else
                    {
                        longDigit = (ulong)digitsBufferPtr1[iLen2] << Constants.DigitBitCount | digitsBufferPtr1[iLen2 - 1];
                    }

                    divEst = longDigit / lastDigit2;
                    modEst = longDigit - divEst * lastDigit2;

                    // Check estimate (maybe correct it)
                    for (; ; )
                    {
                        if (divEst == Constants.BitCountStepOf2 || divEst * preLastDigit2 > (modEst << Constants.DigitBitCount) + digitsBufferPtr1[iLen2 - 2])
                        {
                            --divEst;
                            modEst += lastDigit2;
                            if (modEst < Constants.BitCountStepOf2) continue;
                        }
                        break;
                    }
                    divRes = (uint)divEst;

                    // Multiply and subtract
                    k = 0;
                    for (j = 0, ji = i; j < length2; ++j, ++ji)
                    {
                        mulRes = (ulong)divRes * digitsBufferPtr2[j];
                        t = digitsBufferPtr1[ji] - k - (long)(mulRes & 0xFFFFFFFF);
                        digitsBufferPtr1[ji] = (uint)t;
                        k = (long)(mulRes >> Constants.DigitBitCount) - (t >> Constants.DigitBitCount);
                    }

                    if (!isMaxLength)
                    {
                        t = digitsBufferPtr1[iLen2] - k;
                        digitsBufferPtr1[iLen2] = (uint)t;
                    }
                    else
                    {
                        t = -k;
                    }

                    // Correct result if subtracted too much
                    if (t < 0)
                    {
                        --divRes;

                        k = 0;
                        for (j = 0, ji = i; j < length2; ++j, ++ji)
                        {
                            t = (long)digitsBufferPtr1[ji] + digitsBufferPtr2[j] + k;
                            digitsBufferPtr1[ji] = (uint)t;
                            k = t >> Constants.DigitBitCount;
                        }

                        if (!isMaxLength)
                        {
                            digitsBufferPtr1[iLen2] = (uint)(k + digitsBufferPtr1[iLen2]);
                        }
                    }

                    // Maybe save div result
                    if (divNeeded)
                    {
                        digitsResPtr[i] = divRes;
                    }
                }

                if (modNeeded)
                {
                    // First set correct mod length
                    length1 = DigitHelper.GetRealDigitsLength(digitsBufferPtr1, length2);

                    // Next maybe shift result back to the right
                    if (shift1 != 0 && length1 != 0)
                    {
                        length1 = DigitOpHelper.Shr(digitsBufferPtr1, length1, digitsBufferPtr1, shift1, false);
                    }
                }

                // Finally return length
                return !divNeeded ? 0 : (digitsResPtr[maxLength] == 0 ? maxLength : ++maxLength);
            }

            /// <summary>
            /// Divides two big integers.
            /// Also modifies <paramref name="digitsPtr1" /> and <paramref name="length1"/> (it will contain remainder).
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="digitsBufferPtr1">Buffer for first big integer digits. May also contain remainder.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digitsPtr2">Second big integer digits.</param>
            /// <param name="digitsBufferPtr2">Buffer for second big integer digits. Only temporarily used.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsResPtr">Resulting big integer digits.</param>
            /// <param name="resultFlags">Which operation results to return.</param>
            /// <param name="cmpResult">Big integers comparsion result (pass -2 if omitted).</param>
            /// <returns>Resulting big integer length.</returns>
            public static unsafe uint BaseDivMod(uint* digitsPtr1, uint* digitsBufferPtr1, ref uint length1, uint* digitsPtr2, uint* digitsBufferPtr2, uint length2, uint* digitsResPtr, DivModResultFlags resultFlags, int cmpResult)
            {
                // Base implementation covers some special cases

                bool divNeeded = (resultFlags & DivModResultFlags.Div) != 0;
                bool modNeeded = (resultFlags & DivModResultFlags.Mod) != 0;

                //
                // Special cases
                //

                // Case when length1 == 0
                if (length1 == 0) return 0;

                // Case when both lengths are 1
                if (length1 == 1 && length2 == 1)
                {
                    if (divNeeded)
                    {
                        *digitsResPtr = *digitsPtr1 / *digitsPtr2;
                        if (*digitsResPtr == 0)
                        {
                            length2 = 0;
                        }
                    }
                    if (modNeeded)
                    {
                        *digitsBufferPtr1 = *digitsPtr1 % *digitsPtr2;
                        if (*digitsBufferPtr1 == 0)
                        {
                            length1 = 0;
                        }
                    }

                    return length2;
                }

                // Compare digits first (if was not previously compared)
                if (cmpResult == -2)
                {
                    cmpResult = DigitOpHelper.Cmp(digitsPtr1, length1, digitsPtr2, length2);
                }

                // Case when first value is smaller then the second one - we will have remainder only
                if (cmpResult < 0)
                {
                    // Maybe we should copy first digits into remainder (if remainder is needed at all)
                    if (modNeeded)
                    {
                        DigitHelper.DigitsBlockCopy(digitsPtr1, digitsBufferPtr1, length1);
                    }

                    // Zero as division result
                    return 0;
                }

                // Case when values are equal
                if (cmpResult == 0)
                {
                    // Maybe remainder must be marked as empty
                    if (modNeeded)
                    {
                        length1 = 0;
                    }

                    // One as division result
                    if (divNeeded)
                    {
                        *digitsResPtr = 1;
                    }

                    return 1;
                }

                // Case when second length equals to 1
                if (length2 == 1)
                {
                    // Call method basing on fact if div is needed
                    uint modRes;
                    if (divNeeded)
                    {
                        length2 = DigitOpHelper.DivMod(digitsPtr1, length1, *digitsPtr2, digitsResPtr, out modRes);
                    }
                    else
                    {
                        modRes = DigitOpHelper.Mod(digitsPtr1, length1, *digitsPtr2);
                    }

                    // Maybe save mod result
                    if (modNeeded)
                    {
                        if (modRes != 0)
                        {
                            length1 = 1;
                            *digitsBufferPtr1 = modRes;
                        }
                        else
                        {
                            length1 = 0;
                        }
                    }

                    return length2;
                }


                // This is regular case, not special
                return uint.MaxValue;
            }

        }
        #endregion

        #region IntXMultiplier
        internal static class IntXMultiplier
        {
            /// <summary>
            /// Multiplies two big integers.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second big integer.</param>
            /// <returns>Resulting big integer.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="int1" /> or <paramref name="int2" /> is a null reference.</exception>
            /// <exception cref="ArgumentException"><paramref name="int1" /> or <paramref name="int2" /> is too big for multiply operation.</exception>
            public static IntX Multiply(IntX int1, IntX int2)
            {
                // Exceptions
                if (ReferenceEquals(int1, null))
                {
                    throw new ArgumentNullException("int1", "Operands can't be null.");
                }
                else if (ReferenceEquals(int2, null))
                {
                    throw new ArgumentNullException("int2", "Operands can't be null.");
                }

                // Special behavior for zero cases
                if (int1._length == 0 || int2._length == 0) return new IntX();

                // Get new big integer length and check it
                ulong newLength = (ulong)int1._length + int2._length;
                if (newLength >> 32 != 0)
                {
                    throw new ArgumentException("One of the operated big integers is too big.");
                }

                // Create resulting big int
                IntX newInt = new IntX((uint)newLength, int1._negative ^ int2._negative);

                // Perform actual digits multiplication
                newInt._length = Multiply(int1._digits, int1._length, int2._digits, int2._length, newInt._digits);

                // Normalization may be needed
                newInt.TryNormalize();

                return newInt;
            }

            /// <summary>
            /// Multiplies two big integers represented by their digits.
            /// </summary>
            /// <param name="digits1">First big integer digits.</param>
            /// <param name="length1">First big integer real length.</param>
            /// <param name="digits2">Second big integer digits.</param>
            /// <param name="length2">Second big integer real length.</param>
            /// <param name="digitsRes">Where to put resulting big integer.</param>
            /// <returns>Resulting big integer real length.</returns>
            private static unsafe uint Multiply(uint[] digits1, uint length1, uint[] digits2, uint length2, uint[] digitsRes)
            {
                fixed (uint* digitsPtr1 = digits1, digitsPtr2 = digits2, digitsResPtr = digitsRes)
                {
                    return Multiply(digitsPtr1, length1, digitsPtr2, length2, digitsResPtr);
                }
            }
            /// <summary>
            /// Multiplies two big integers using pointers.
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digitsPtr2">Second big integer digits.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsResPtr">Resulting big integer digits.</param>
            /// <returns>Resulting big integer real length.</returns>
            internal static unsafe uint Multiply(uint* digitsPtr1, uint length1, uint* digitsPtr2, uint length2, uint* digitsResPtr)
            {
                // Check length - maybe use classic multiplier instead
                if (length1 < Constants.AutoFhtLengthLowerBound || length2 < Constants.AutoFhtLengthLowerBound ||
                    length1 > Constants.AutoFhtLengthUpperBound || length2 > Constants.AutoFhtLengthUpperBound)
                {
                    return ClassicMultiply(digitsPtr1, length1, digitsPtr2, length2, digitsResPtr);
                }

                uint newLength = length1 + length2;

                // Do FHT for first big integer
                double[] data1 = FhtHelper.ConvertDigitsToDouble(digitsPtr1, length1, newLength);
                FhtHelper.Fht(data1, (uint)data1.LongLength);

                // Compare digits
                double[] data2;
                if (digitsPtr1 == digitsPtr2 || DigitOpHelper.Cmp(digitsPtr1, length1, digitsPtr2, length2) == 0)
                {
                    // Use the same FHT for equal big integers
                    data2 = data1;
                }
                else
                {
                    // Do FHT over second digits
                    data2 = FhtHelper.ConvertDigitsToDouble(digitsPtr2, length2, newLength);
                    FhtHelper.Fht(data2, (uint)data2.LongLength);
                }

                // Perform multiplication and reverse FHT
                FhtHelper.MultiplyFhtResults(data1, data2, (uint)data1.LongLength);
                FhtHelper.ReverseFht(data1, (uint)data1.LongLength);

                // Convert to digits
                fixed (double* slice1 = data1)
                {
                    FhtHelper.ConvertDoubleToDigits(slice1, (uint)data1.LongLength, newLength, digitsResPtr);
                }

                // Return double arrays back to pool
                ArrayPool<double>.Instance.AddArray(data1);
                if (data2 != data1)
                {
                    ArrayPool<double>.Instance.AddArray(data2);
                }

                //// Maybe check for validity using classic multiplication
                //if (System.IntX.GlobalSettings.ApplyFhtValidityCheck)
                //{
                //    uint lowerDigitCount = System.Math.Min(length2, System.Math.Min(length1, Constants.FhtValidityCheckDigitCount));

                //    // Validate result by multiplying lowerDigitCount digits using classic algorithm and comparing
                //    uint[] validationResult = new uint[lowerDigitCount * 2];
                //    fixed (uint* validationResultPtr = validationResult)
                //    {
                //        ClassicMultiply(digitsPtr1, lowerDigitCount, digitsPtr2, lowerDigitCount, validationResultPtr);
                //        if (DigitOpHelper.Cmp(validationResultPtr, lowerDigitCount, digitsResPtr, lowerDigitCount) != 0)
                //        {
                //            throw new FhtMultiplicationException(string.Format("FHT multiplication returned invalid result for IntX objects with lengths {0} and {1}.", length1, length2));
                //        }
                //    }
                //}

                return digitsResPtr[newLength - 1] == 0 ? --newLength : newLength;
            }

            /// <summary>
            /// Multiplies two big integers using pointers.
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digitsPtr2">Second big integer digits.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsResPtr">Resulting big integer digits.</param>
            /// <returns>Resulting big integer length.</returns>
            private static unsafe uint ClassicMultiply(uint* digitsPtr1, uint length1, uint* digitsPtr2, uint length2, uint* digitsResPtr)
            {
                ulong c;

                // External cycle must be always smaller
                if (length1 < length2)
                {
                    // First must be bigger - swap
                    uint lengthTemp = length1;
                    length1 = length2;
                    length2 = lengthTemp;

                    uint* ptrTemp = digitsPtr1;
                    digitsPtr1 = digitsPtr2;
                    digitsPtr2 = ptrTemp;
                }

                // Prepare end pointers
                uint* digitsPtr1End = digitsPtr1 + length1;
                uint* digitsPtr2End = digitsPtr2 + length2;

                // We must always clear first "length1" digits in result
                DigitHelper.SetBlockDigits(digitsResPtr, length1, 0U);

                // Perform digits multiplication
                uint* ptr1, ptrRes = null;
                for (; digitsPtr2 < digitsPtr2End; ++digitsPtr2, ++digitsResPtr)
                {
                    // Check for zero (sometimes may help). There is no sense to make this check in internal cycle -
                    // it would give performance gain only here
                    if (*digitsPtr2 == 0) continue;

                    c = 0;
                    for (ptr1 = digitsPtr1, ptrRes = digitsResPtr; ptr1 < digitsPtr1End; ++ptr1, ++ptrRes)
                    {
                        c += (ulong)*digitsPtr2 * *ptr1 + *ptrRes;
                        *ptrRes = (uint)c;
                        c >>= 32;
                    }
                    *ptrRes = (uint)c;
                }

                uint newLength = length1 + length2;
                if (newLength > 0 && (ptrRes == null || *ptrRes == 0))
                {
                    --newLength;
                }
                return newLength;
            }
        }
        #endregion

        #region IntXParser
        internal static class IntXParser
        {
            /// <summary>
            /// Regex pattern used on parsing stage to determine number sign and/or base
            /// </summary>
            private const string ParseRegexPattern = "(?<Sign>[+-]?)((?<BaseHex>0[Xx])|(?<BaseOct>0))?";

            /// <summary>
            /// Regex object used on parsing stage to determine number sign and/or base
            /// </summary>
            private static readonly Regex ParseRegex = new Regex(ParseRegexPattern, RegexOptions.Compiled);


            #region Pow2Parse
            /// <summary>
            /// Parses provided string representation of <see cref="IntX" /> object.
            /// </summary>
            /// <param name="value">Number as string.</param>
            /// <param name="startIndex">Index inside string from which to start.</param>
            /// <param name="endIndex">Index inside string on which to end.</param>
            /// <param name="numberBase">Number base.</param>
            /// <param name="charToDigits">Char->digit dictionary.</param>
            /// <param name="digitsRes">Resulting digits.</param>
            /// <returns>Parsed integer length.</returns>
            private static uint Pow2Parse(string value, int startIndex, int endIndex, uint numberBase, IDictionary<char, uint> charToDigits, uint[] digitsRes)
            {
                // Calculate length of input string
                int bitsInChar = Bits.Msb(numberBase);
                uint valueLength = (uint)(endIndex - startIndex + 1);
                ulong valueBitLength = (ulong)valueLength * (ulong)bitsInChar;

                // Calculate needed digits length and first shift
                uint digitsLength = (uint)(valueBitLength / Constants.DigitBitCount) + 1;
                uint digitIndex = digitsLength - 1;
                int initialShift = (int)(valueBitLength % Constants.DigitBitCount);

                // Probably correct digits length
                if (initialShift == 0)
                {
                    --digitsLength;
                }

                // Do parsing in big cycle
                uint digit;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    digit = StrRepHelper.GetDigit(charToDigits, value[i], numberBase);

                    // Correct initial digit shift
                    if (initialShift == 0)
                    {
                        // If shift is equals to zero then char is not on digit elements bounds,
                        // so just go to the previous digit
                        initialShift = Constants.DigitBitCount - bitsInChar;
                        --digitIndex;
                    }
                    else
                    {
                        // Here shift might be negative, but it's okay
                        initialShift -= bitsInChar;
                    }

                    // Insert new digit in correct place
                    digitsRes[digitIndex] |= initialShift < 0 ? digit >> -initialShift : digit << initialShift;

                    // In case if shift was negative we also must modify previous digit
                    if (initialShift < 0)
                    {
                        initialShift += Constants.DigitBitCount;
                        digitsRes[--digitIndex] |= digit << initialShift;
                    }
                }

                if (digitsRes[digitsLength - 1] == 0)
                {
                    --digitsLength;
                }
                return digitsLength;
            }
            #endregion

            #region BaseParse
            /// <summary>
            /// Parses provided string representation of <see cref="IntX" /> object.
            /// </summary>
            /// <param name="value">Number as string.</param>
            /// <param name="numberBase">Number base.</param>
            /// <param name="charToDigits">Char->digit dictionary.</param>
            /// <param name="checkFormat">Check actual format of number (0 or 0x at start).</param>
            /// <returns>Parsed object.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
            /// <exception cref="ArgumentException"><paramref name="numberBase" /> is less then 2 or more then 16.</exception>
            /// <exception cref="FormatException"><paramref name="value" /> is not in valid format.</exception>
            public static IntX Parse(string value, uint numberBase, IDictionary<char, uint> charToDigits, bool checkFormat)
            {
                // Exceptions
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (charToDigits == null)
                {
                    throw new ArgumentNullException("charToDigits");
                }
                if (numberBase < 2 || numberBase > charToDigits.Count)
                {
                    throw new ArgumentException("Base is invalid.", "numberBase");
                }

                // Initially determine start and end indices (Trim is too slow)
                int startIndex = 0;
                for (; startIndex < value.Length && char.IsWhiteSpace(value[startIndex]); ++startIndex) ;
                int endIndex = value.Length - 1;
                for (; endIndex >= startIndex && char.IsWhiteSpace(value[endIndex]); --endIndex) ;

                bool negative = false; // number sign
                bool stringNotEmpty = false; // true if string is already guaranteed to be non-empty

                // Determine sign and/or base
                Match match = ParseRegex.Match(value, startIndex, endIndex - startIndex + 1);
                if (match.Groups["Sign"].Value == "-")
                {
                    negative = true;
                }
                if (match.Groups["BaseHex"].Length != 0)
                {
                    if (checkFormat)
                    {
                        // 0x before the number - this is hex number
                        numberBase = 16U;
                    }
                    else
                    {
                        // This is an error
                        throw new FormatException("Invalid character in input.");
                    }
                }
                else if (match.Groups["BaseOct"].Length != 0)
                {
                    if (checkFormat)
                    {
                        // 0 before the number - this is octal number
                        numberBase = 8U;
                    }

                    stringNotEmpty = true;
                }

                // Skip leading sign and format
                startIndex += match.Length;

                // If on this stage string is empty, this may mean an error
                if (startIndex > endIndex && !stringNotEmpty)
                {
                    throw new FormatException("No digits in string.");
                }

                // Iterate thru string and skip all leading zeroes
                for (; startIndex <= endIndex && value[startIndex] == '0'; ++startIndex) ;

                // Return zero if length is zero
                if (startIndex > endIndex) return new IntX();

                // Determine length of new digits array and create new IntX object with given length
                int valueLength = endIndex - startIndex + 1;
                uint digitsLength = (uint)System.Math.Ceiling(System.Math.Log(numberBase) / Constants.DigitBaseLog * valueLength);
                IntX newInt = new IntX(digitsLength, negative);

                // Now we have only (in)valid string which consists from numbers only.
                // Parse it
                newInt._length = Parse(value, startIndex, endIndex, numberBase, charToDigits, newInt._digits);

                return newInt;
            }

            /// <summary>
            /// Parses provided string representation of <see cref="IntX" /> object.
            /// </summary>
            /// <param name="value">Number as string.</param>
            /// <param name="startIndex">Index inside string from which to start.</param>
            /// <param name="endIndex">Index inside string on which to end.</param>
            /// <param name="numberBase">Number base.</param>
            /// <param name="charToDigits">Char->digit dictionary.</param>
            /// <param name="digitsRes">Resulting digits.</param>
            /// <returns>Parsed integer length.</returns>
            private static uint BaseParse(string value, int startIndex, int endIndex, uint numberBase, IDictionary<char, uint> charToDigits, uint[] digitsRes)
            {
                // Default implementation - always call pow2 parser if numberBase is pow of 2
                return numberBase == 1U << Bits.Msb(numberBase)
                    ? Pow2Parse(value, startIndex, endIndex, numberBase, charToDigits, digitsRes)
                    : 0;
            }
            #endregion

            #region FastParser
            /// <summary>
            /// Parses provided string representation of <see cref="IntX" /> object.
            /// </summary>
            /// <param name="value">Number as string.</param>
            /// <param name="startIndex">Index inside string from which to start.</param>
            /// <param name="endIndex">Index inside string on which to end.</param>
            /// <param name="numberBase">Number base.</param>
            /// <param name="charToDigits">Char->digit dictionary.</param>
            /// <param name="digitsRes">Resulting digits.</param>
            /// <returns>Parsed integer length.</returns>
            private static unsafe uint Parse(string value, int startIndex, int endIndex, uint numberBase, IDictionary<char, uint> charToDigits, uint[] digitsRes)
            {
                uint newLength = BaseParse(value, startIndex, endIndex, numberBase, charToDigits, digitsRes);

                // Maybe base method already parsed this number
                if (newLength != 0) return newLength;

                // Check length - maybe use classic parser instead
                uint initialLength = (uint)digitsRes.LongLength;
                if (initialLength < Constants.FastParseLengthLowerBound || initialLength > Constants.FastParseLengthUpperBound)
                {
                    return ClassicParse(value, startIndex, endIndex, numberBase, charToDigits, digitsRes);
                }

                uint valueLength = (uint)(endIndex - startIndex + 1);
                uint digitsLength = 1U << Bits.CeilLog2(valueLength);

                // Prepare array for digits in other base
                uint[] valueDigits = ArrayPool<uint>.Instance.GetArray(digitsLength);

                // This second array will store integer lengths initially
                uint[] valueDigits2 = ArrayPool<uint>.Instance.GetArray(digitsLength);

                fixed (uint* valueDigitsStartPtr = valueDigits, valueDigitsStartPtr2 = valueDigits2)
                {
                    // In the string first digit means last in digits array
                    uint* valueDigitsPtr = valueDigitsStartPtr + valueLength - 1;
                    uint* valueDigitsPtr2 = valueDigitsStartPtr2 + valueLength - 1;

                    // Reverse copy characters into digits
                    fixed (char* valueStartPtr = value)
                    {
                        char* valuePtr = valueStartPtr + startIndex;
                        char* valueEndPtr = valuePtr + valueLength;
                        for (; valuePtr < valueEndPtr; ++valuePtr, --valueDigitsPtr, --valueDigitsPtr2)
                        {
                            // Get digit itself - this call will throw an exception if char is invalid
                            *valueDigitsPtr = StrRepHelper.GetDigit(charToDigits, *valuePtr, numberBase);

                            // Set length of this digit (zero for zero)
                            *valueDigitsPtr2 = *valueDigitsPtr == 0U ? 0U : 1U;
                        }
                    }

                    // We have retrieved lengths array from pool - it needs to be cleared before using
                    DigitHelper.SetBlockDigits(valueDigitsStartPtr2 + valueLength, digitsLength - valueLength, 0);

                    // Now start from the digit arrays beginning
                    valueDigitsPtr = valueDigitsStartPtr;
                    valueDigitsPtr2 = valueDigitsStartPtr2;

                    // Here base in needed power will be stored
                    IntX baseInt = null;

                    // Temporary variables used on swapping
                    uint[] tempDigits;
                    uint* tempPtr;

                    // Variables used in cycle
                    uint* ptr1, ptr2, valueDigitsPtrEnd;
                    uint loLength, hiLength;

                    // Outer cycle instead of recursion
                    for (uint innerStep = 1, outerStep = 2; innerStep < digitsLength; innerStep <<= 1, outerStep <<= 1)
                    {
                        // Maybe baseInt must be multiplied by itself
                        baseInt = baseInt == null ? numberBase : baseInt * baseInt;

                        // Using unsafe here
                        fixed (uint* baseDigitsPtr = baseInt._digits)
                        {
                            // Start from arrays beginning
                            ptr1 = valueDigitsPtr;
                            ptr2 = valueDigitsPtr2;

                            // vauleDigits array end
                            valueDigitsPtrEnd = valueDigitsPtr + digitsLength;

                            // Cycle thru all digits and their lengths
                            for (; ptr1 < valueDigitsPtrEnd; ptr1 += outerStep, ptr2 += outerStep)
                            {
                                // Get lengths of "lower" and "higher" value parts
                                loLength = *ptr2;
                                hiLength = *(ptr2 + innerStep);

                                if (hiLength != 0)
                                {
                                    // We always must clear an array before multiply
                                    DigitHelper.SetBlockDigits(ptr2, outerStep, 0U);

                                    // Multiply per baseInt
                                    hiLength = IntXMultiplier.Multiply(baseDigitsPtr, baseInt._length, ptr1 + innerStep, hiLength, ptr2);
                                }

                                // Sum results
                                if (hiLength != 0 || loLength != 0)
                                {
                                    *ptr1 = DigitOpHelper.Add(ptr2, hiLength, ptr1, loLength, ptr2);
                                }
                                else
                                {
                                    *ptr1 = 0U;
                                }
                            }
                        }

                        // After inner cycle valueDigits will contain lengths and valueDigits2 will contain actual values
                        // so we need to swap them here
                        tempDigits = valueDigits;
                        valueDigits = valueDigits2;
                        valueDigits2 = tempDigits;

                        tempPtr = valueDigitsPtr;
                        valueDigitsPtr = valueDigitsPtr2;
                        valueDigitsPtr2 = tempPtr;
                    }
                }

                // Determine real length of converted number
                uint realLength = valueDigits2[0];

                // Copy to result
                Array.Copy(valueDigits, digitsRes, realLength);

                // Return arrays to pool
                ArrayPool<uint>.Instance.AddArray(valueDigits);
                ArrayPool<uint>.Instance.AddArray(valueDigits2);

                return realLength;
            }
            #endregion

            #region ClassicParser
            /// <summary>
            /// Parses provided string representation of <see cref="System.IntX" /> object.
            /// </summary>
            /// <param name="value">Number as string.</param>
            /// <param name="startIndex">Index inside string from which to start.</param>
            /// <param name="endIndex">Index inside string on which to end.</param>
            /// <param name="numberBase">Number base.</param>
            /// <param name="charToDigits">Char->digit dictionary.</param>
            /// <param name="digitsRes">Resulting digits.</param>
            /// <returns>Parsed integer length.</returns>
            private static uint ClassicParse(string value, int startIndex, int endIndex, uint numberBase, IDictionary<char, uint> charToDigits, uint[] digitsRes)
            {
                uint newLength = BaseParse(value, startIndex, endIndex, numberBase, charToDigits, digitsRes);

                // Maybe base method already parsed this number
                if (newLength != 0) return newLength;

                // Do parsing in big cycle
                ulong numberBaseLong = numberBase;
                ulong digit;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    digit = StrRepHelper.GetDigit(charToDigits, value[i], numberBase);

                    // Next multiply existing values by base and add this value to them
                    if (newLength == 0)
                    {
                        if (digit != 0)
                        {
                            digitsRes[0] = (uint)digit;
                            newLength = 1;
                        }
                    }
                    else
                    {
                        for (uint j = 0; j < newLength; ++j)
                        {
                            digit += digitsRes[j] * numberBaseLong;
                            digitsRes[j] = (uint)digit;
                            digit >>= 32;
                        }
                        if (digit != 0)
                        {
                            digitsRes[newLength++] = (uint)digit;
                        }
                    }
                }

                return newLength;
            }
            #endregion

        }
        #endregion

        #region IntXStringConverter
        internal static class IntXStringConverter
        {
            /// <summary>
            /// Returns string representation of <see cref="IntX" /> object in given base.
            /// </summary>
            /// <param name="intX">Big integer to convert.</param>
            /// <param name="numberBase">Base of system in which to do output.</param>
            /// <param name="alphabet">Alphabet which contains chars used to represent big integer, char position is coresponding digit value.</param>
            /// <returns>Object string representation.</returns>
            /// <exception cref="ArgumentException"><paramref name="numberBase" /> is less then 2 or <paramref name="intX" /> is too big to fit in string.</exception>
            public static string ToString(IntX intX, uint numberBase, char[] alphabet)
            {
                // Test base
                if (numberBase < 2 || numberBase > 65536)
                {
                    throw new ArgumentException("Base must be between 2 and 65536.", "numberBase");
                }

                // Special processing for zero values
                if (intX._length == 0)
                    return "0";

                // Calculate output array length
                uint outputLength = (uint)System.Math.Ceiling(Constants.DigitBaseLog / System.Math.Log(numberBase) * intX._length);

                // Define length coefficient for string builder
                bool isBigBase = numberBase > alphabet.Length;
                uint lengthCoef = isBigBase ? (uint)System.Math.Ceiling(System.Math.Log10(numberBase)) + 2U : 1U;

                // Determine maximal possible length of string
                ulong maxBuilderLength = (ulong)outputLength * lengthCoef + 1UL;
                if (maxBuilderLength > int.MaxValue)
                {
                    // This big integer can't be transformed to string
                    throw new ArgumentException("One of the operated big integers is too big.", "intX");
                }

                // Transform digits into another base
                uint[] outputArray = FastToString(intX._digits, intX._length, numberBase, ref outputLength);

                // Output everything to the string builder
                StringBuilder outputBuilder = new StringBuilder((int)(outputLength * lengthCoef + 1));

                // Maybe append minus sign
                if (intX._negative)
                {
                    outputBuilder.Append(Constants.DigitsMinusChar);
                }

                // Output all digits
                for (uint i = outputLength - 1; i < outputLength; --i)
                {
                    if (!isBigBase)
                    {
                        // Output char-by-char for bases up to covered by alphabet
                        outputBuilder.Append(alphabet[(int)outputArray[i]]);
                    }
                    else
                    {
                        // Output digits in brackets for bigger bases
                        outputBuilder.Append(Constants.DigitOpeningBracet);
                        outputBuilder.Append(outputArray[i].ToString());
                        outputBuilder.Append(Constants.DigitClosingBracet);
                    }
                }

                return outputBuilder.ToString();
            }

            #region BaseToString
            /// <summary>
            /// Converts digits from internal representation into given base.
            /// </summary>
            /// <param name="digits">Big integer digits.</param>
            /// <param name="length">Big integer length.</param>
            /// <param name="numberBase">Base to use for output.</param>
            /// <param name="outputLength">Calculated output length (will be corrected inside).</param>
            /// <returns>Conversion result (later will be transformed to string).</returns>
            private static uint[] BaseToString(uint[] digits, uint length, uint numberBase, ref uint outputLength)
            {
                // Default implementation - always call pow2 converter if numberBase is power of 2
                return numberBase == 1U << Bits.Msb(numberBase)
                    ? Pow2ToString(digits, length, numberBase, ref outputLength)
                    : null;
            }
            #endregion

            #region FastToString
            /// <summary>
            /// Converts digits from internal representation into given base.
            /// </summary>
            /// <param name="digits">Big integer digits.</param>
            /// <param name="length">Big integer length.</param>
            /// <param name="numberBase">Base to use for output.</param>
            /// <param name="outputLength">Calculated output length (will be corrected inside).</param>
            /// <returns>Conversion result (later will be transformed to string).</returns>
            private static unsafe uint[] FastToString(uint[] digits, uint length, uint numberBase, ref uint outputLength)
            {
                uint[] outputArray = BaseToString(digits, length, numberBase, ref outputLength);

                // Maybe base method already converted this number
                if (outputArray != null) return outputArray;

                // Check length - maybe use classic converter instead
                if (length < Constants.FastConvertLengthLowerBound || length > Constants.FastConvertLengthUpperBound)
                {
                    return ClassicToString(digits, length, numberBase, ref outputLength);
                }

                int resultLengthLog2 = Bits.CeilLog2(outputLength);
                uint resultLength = 1U << resultLengthLog2;

                // Create and initially fill array for transformed numbers storing
                uint[] resultArray = ArrayPool<uint>.Instance.GetArray(resultLength);
                Array.Copy(digits, resultArray, length);

                // Create and initially fill array with lengths
                uint[] resultArray2 = ArrayPool<uint>.Instance.GetArray(resultLength);
                resultArray2[0] = length;


                // Generate all needed powers of numberBase in stack
                Stack baseIntStack = new Stack(resultLengthLog2);
                IntX baseInt = null;
                for (int i = 0; i < resultLengthLog2; ++i)
                {
                    baseInt = baseInt == null ? numberBase : IntXMultiplier.Multiply(baseInt, baseInt);
                    baseIntStack.Push(baseInt);
                }

                // Create temporary buffer for second digits when doing div operation
                uint[] tempBuffer = new uint[baseInt._length];

                // We will use unsafe code here
                fixed (uint* resultPtr1Const = resultArray, resultPtr2Const = resultArray2, tempBufferPtr = tempBuffer)
                {
                    // Results pointers which will be modified (on swap)
                    uint* resultPtr1 = resultPtr1Const;
                    uint* resultPtr2 = resultPtr2Const;

                    // Temporary variables used on swapping
                    uint[] tempArray;
                    uint* tempPtr;

                    // Variables used in cycle
                    uint* ptr1, ptr2, ptr1end;
                    uint loLength;

                    // Outer cycle instead of recursion
                    for (uint innerStep = resultLength >> 1, outerStep = resultLength; innerStep > 0; innerStep >>= 1, outerStep >>= 1)
                    {
                        // Prepare pointers
                        ptr1 = resultPtr1;
                        ptr2 = resultPtr2;
                        ptr1end = resultPtr1 + resultLength;

                        // Get baseInt from stack and fix it too
                        baseInt = (IntX)baseIntStack.Pop();
                        fixed (uint* baseIntPtr = baseInt._digits)
                        {
                            // Cycle thru all digits and their lengths
                            for (; ptr1 < ptr1end; ptr1 += outerStep, ptr2 += outerStep)
                            {
                                // Divide ptr1 (with length in *ptr2) by baseIntPtr here.
                                // Results are stored in ptr2 & (ptr2 + innerStep), lengths - in *ptr1 and (*ptr1 + innerStep)
                                loLength = *ptr2;
                                *(ptr1 + innerStep) = IntXDivider.DivMod(
                                    ptr1,
                                    ptr2,
                                    ref loLength,
                                    baseIntPtr,
                                    tempBufferPtr,
                                    baseInt._length,
                                    ptr2 + innerStep,
                                    DivModResultFlags.Div | DivModResultFlags.Mod,
                                    -2);
                                *ptr1 = loLength;
                            }
                        }

                        // After inner cycle resultArray will contain lengths and resultArray2 will contain actual values
                        // so we need to swap them here
                        tempArray = resultArray;
                        resultArray = resultArray2;
                        resultArray2 = tempArray;

                        tempPtr = resultPtr1;
                        resultPtr1 = resultPtr2;
                        resultPtr2 = tempPtr;
                    }

                    // Retrieve real output length
                    outputLength = DigitHelper.GetRealDigitsLength(resultArray2, outputLength);

                    // Create output array
                    outputArray = new uint[outputLength];

                    // Copy each digit but only if length is not null
                    fixed (uint* outputPtr = outputArray)
                    {
                        for (uint i = 0; i < outputLength; ++i)
                        {
                            if (resultPtr2[i] != 0)
                            {
                                outputPtr[i] = resultPtr1[i];
                            }
                        }
                    }
                }

                // Return temporary arrays to pool
                ArrayPool<uint>.Instance.AddArray(resultArray);
                ArrayPool<uint>.Instance.AddArray(resultArray2);

                return outputArray;
            }
            #endregion

            #region ClassicToString
            /// <summary>
            /// Converts digits from internal representaion into given base.
            /// </summary>
            /// <param name="digits">Big integer digits.</param>
            /// <param name="length">Big integer length.</param>
            /// <param name="numberBase">Base to use for output.</param>
            /// <param name="outputLength">Calculated output length (will be corrected inside).</param>
            /// <returns>Conversion result (later will be transformed to string).</returns>
            private static uint[] ClassicToString(uint[] digits, uint length, uint numberBase, ref uint outputLength)
            {
                uint[] outputArray = BaseToString(digits, length, numberBase, ref outputLength);

                // Maybe base method already converted this number
                if (outputArray != null)
                    return outputArray;

                // Create an output array for storing of number in other base
                outputArray = new uint[outputLength + 1];

                // Make a copy of initial data
                uint[] digitsCopy = new uint[length];
                Array.Copy(digits, digitsCopy, length);

                // Calculate output numbers by dividing
                uint outputIndex;
                for (outputIndex = 0; length > 0; ++outputIndex)
                {
                    length = DigitOpHelper.DivMod(digitsCopy, length, numberBase, digitsCopy, out outputArray[outputIndex]);
                }

                outputLength = outputIndex;
                return outputArray;
            }
            #endregion

            #region Pow2ToString
            /// <summary>
            /// Converts digits from internal representation into given base.
            /// </summary>
            /// <param name="digits">Big integer digits.</param>
            /// <param name="length">Big integer length.</param>
            /// <param name="numberBase">Base to use for output.</param>
            /// <param name="outputLength">Calculated output length (will be corrected inside).</param>
            /// <returns>Conversion result (later will be transformed to string).</returns>
            private static uint[] Pow2ToString(uint[] digits, uint length, uint numberBase, ref uint outputLength)
            {
                // Calculate real output length
                int bitsInChar = Bits.Msb(numberBase);
                ulong digitsBitLength = (ulong)(length - 1) * Constants.DigitBitCount + (ulong)Bits.Msb(digits[length - 1]) + 1UL;
                uint realOutputLength = (uint)(digitsBitLength / (ulong)bitsInChar);
                if (digitsBitLength % (ulong)bitsInChar != 0)
                {
                    ++realOutputLength;
                }

                // Prepare shift variables
                int nextDigitShift = Constants.DigitBitCount - bitsInChar; // after this shift next digit must be used
                int initialShift = 0;

                // We will also need bitmask for copying digits
                uint digitBitMask = numberBase - 1;

                // Create an output array for storing of number in other base
                uint[] outputArray = new uint[realOutputLength];

                // Walk thru original digits and fill output
                uint outputDigit;
                for (uint outputIndex = 0, digitIndex = 0; outputIndex < realOutputLength; ++outputIndex)
                {
                    // Get part of current digit
                    outputDigit = digits[digitIndex] >> initialShift;

                    // Maybe we need to go to the next digit
                    if (initialShift >= nextDigitShift)
                    {
                        // Go to the next digit
                        ++digitIndex;

                        // Maybe we also need a part of the next digit
                        if (initialShift != nextDigitShift && digitIndex < length)
                        {
                            outputDigit |= digits[digitIndex] << (Constants.DigitBitCount - initialShift);
                        }

                        // Modify shift so that it will be valid for the next digit
                        initialShift = (initialShift + bitsInChar) % Constants.DigitBitCount;
                    }
                    else
                    {
                        // Modify shift as usual
                        initialShift += bitsInChar;
                    }

                    // Write masked result to the output
                    outputArray[outputIndex] = outputDigit & digitBitMask;
                }

                outputLength = realOutputLength;
                return outputArray;
            }
            #endregion

        }
        #endregion

        #region Bits
        /// <summary>
        /// Contains helping methods to with with bits in dword (<see cref="UInt32" />).
        /// </summary>
        internal static class Bits
        {
            /// <summary>
            /// Returns number of leading zero bits in int.
            /// </summary>
            /// <param name="x">Int value.</param>
            /// <returns>Number of leading zero bits.</returns>
            static public int Nlz(uint x)
            {
                if (x == 0) return 32;

                int n = 1;
                if ((x >> 16) == 0) { n += 16; x <<= 16; }
                if ((x >> 24) == 0) { n += 8; x <<= 8; }
                if ((x >> 28) == 0) { n += 4; x <<= 4; }
                if ((x >> 30) == 0) { n += 2; x <<= 2; }
                return n - (int)(x >> 31);
            }

            /// <summary>
            /// Counts position of the most significant bit in int.
            /// Can also be used as Floor(Log2(<paramref name="x" />)).
            /// </summary>
            /// <param name="x">Int value.</param>
            /// <returns>Position of the most significant one bit (-1 if all zeroes).</returns>
            static public int Msb(uint x)
            {
                return 31 - Nlz(x);
            }

            /// <summary>
            /// Ceil(Log2(<paramref name="x" />)).
            /// </summary>
            /// <param name="x">Int value.</param>
            /// <returns>Ceil of the Log2.</returns>
            static public int CeilLog2(uint x)
            {
                int msb = Msb(x);
                if (x != 1U << msb)
                {
                    ++msb;
                }
                return msb;
            }
        }
        #endregion

        #region DivModResultFlags
        /// <summary>
        /// <see cref="IntX" /> divide results to return.
        /// </summary>
        [Flags]
        internal enum DivModResultFlags
        {
            /// <summary>
            /// Dividend is returned.
            /// </summary>
            Div = 1,
            /// <summary>
            /// Remainder is returned.
            /// </summary>
            Mod = 2
        }
        #endregion

        #region Constants
        /// <summary>
        /// Constants used in <see cref="System.IntX" /> and helping classes.
        /// </summary>
        static internal class Constants
        {
            #region .cctor

            static Constants()
            {
                BaseCharToDigits = StrRepHelper.CharDictionaryFromAlphabet(new string(BaseUpperChars), 16U);
                for (int i = 10; i < BaseLowerChars.Length; i++)
                {
                    BaseCharToDigits.Add(BaseLowerChars[i], (uint)i);
                }
            }

            #endregion .cctor


            #region ToString constants

            /// <summary>
            /// Chars used to parse/output big integers (upper case).
            /// </summary>
            public static readonly char[] BaseUpperChars = "0123456789ABCDEF".ToCharArray();

            /// <summary>
            /// Chars used to parse/output big integers (lower case).
            /// </summary>
            public static readonly char[] BaseLowerChars = "0123456789abcdef".ToCharArray();

            /// <summary>
            /// Standard char->digit dictionary.
            /// </summary>
            static readonly public IDictionary<char, uint> BaseCharToDigits;

            /// <summary>
            /// Digit opening bracket (used for bases bigger then 16).
            /// </summary>
            public const char DigitOpeningBracet = '{';

            /// <summary>
            /// Digit closing bracket (used for bases bigger then 16).
            /// </summary>
            public const char DigitClosingBracet = '}';

            /// <summary>
            /// Minus char (-).
            /// </summary>
            public const char DigitsMinusChar = '-';

            /// <summary>
            /// Natural logarithm of digits base (log(2^32)).
            /// </summary>
            public static readonly double DigitBaseLog = System.Math.Log(1UL << DigitBitCount);

            #endregion ToString constants

            #region Pools constants

            /// <summary>
            /// Minimal Log2(array length) which will be pooled using any array pool.
            /// </summary>
            public const int MinPooledArraySizeLog2 = 17;

            /// <summary>
            /// Maximal Log2(array length) which will be pooled using any array pool.
            /// </summary>
            public const int MaxPooledArraySizeLog2 = 31;

            /// <summary>
            /// Maximal allowed array pool items count in each stack.
            /// </summary>
            public const int MaxArrayPoolCount = 1024;

            #endregion Pools constants

            #region FHT constants

            /// <summary>
            /// <see cref="System.IntX" /> length from which FHT is used (in auto-FHT mode).
            /// Before this length usual multiply algorithm works faster.
            /// </summary>
            public const uint AutoFhtLengthLowerBound = 1U << 9;

            /// <summary>
            /// <see cref="System.IntX" /> length 'till which FHT is used (in auto-FHT mode).
            /// After this length using of FHT may be unsafe due to big precision errors.
            /// </summary>
            public const uint AutoFhtLengthUpperBound = 1U << 26;

            /// <summary>
            /// Number of lower digits used to check FHT multiplication result validity.
            /// </summary>
            public const uint FhtValidityCheckDigitCount = 10;

            #endregion FHT constants

            #region Newton constants

            /// <summary>
            /// <see cref="System.IntX" /> length from which Newton approach is used (in auto-Newton mode).
            /// Before this length usual divide algorithm works faster.
            /// </summary>
            public const uint AutoNewtonLengthLowerBound = 1U << 13;

            /// <summary>
            /// <see cref="System.IntX" /> length 'till which Newton approach is used (in auto-Newton mode).
            /// After this length using of fast division may be slow.
            /// </summary>
            public const uint AutoNewtonLengthUpperBound = 1U << 26;

            #endregion Newton constants

            #region Parsing constants

            /// <summary>
            /// <see cref="System.IntX" /> length from which fast parsing is used (in Fast parsing mode).
            /// Before this length usual parsing algorithm works faster.
            /// </summary>
            public const uint FastParseLengthLowerBound = 32;

            /// <summary>
            /// <see cref="System.IntX" /> length 'till which fast parsing is used (in Fast parsing mode).
            /// After this length using of parsing will be slow.
            /// </summary>
            public const uint FastParseLengthUpperBound = uint.MaxValue;

            #endregion Parsing constants

            #region ToString convertion constants

            /// <summary>
            /// <see cref="System.IntX" /> length from which fast conversion is used (in Fast convert mode).
            /// Before this length usual conversion algorithm works faster.
            /// </summary>
            public const uint FastConvertLengthLowerBound = 16;

            /// <summary>
            /// <see cref="System.IntX" /> length 'till which fast conversion is used (in Fast convert mode).
            /// After this length using of conversion will be slow.
            /// </summary>
            public const uint FastConvertLengthUpperBound = uint.MaxValue;

            #endregion ToString convertion constants

            /// <summary>
            /// Count of bits in one <see cref="System.IntX" /> digit.
            /// </summary>
            public const int DigitBitCount = 32;

            /// <summary>
            /// Maximum count of bits which can fit in <see cref="System.IntX" />.
            /// </summary>
            public const ulong MaxBitCount = uint.MaxValue * 32UL;

            /// <summary>
            /// 2^<see cref="DigitBitCount"/>.
            /// </summary>
            public const ulong BitCountStepOf2 = 1UL << 32;
        }
        #endregion

        #region ArrayPool
        /// <summary>
        /// Generic arrays pool on weak references.
        /// </summary>
        sealed internal class ArrayPool<T>
        {
            #region Fields

            /// <summary>
            /// Singleton instance.
            /// </summary>
            static readonly public ArrayPool<T> Instance = new ArrayPool<T>(
                Constants.MinPooledArraySizeLog2,
                Constants.MaxPooledArraySizeLog2,
                Constants.MaxArrayPoolCount);

            // Minimal and maximal Log2(uint[] length) which will be pooled
            int _minPooledArraySizeLog2;
            int _maxPooledArraySizeLog2;

            // Maximal pool items count
            int _maxPoolCount;

            Stack<WeakReference>[] _pools; // ArrayPool pools

            #endregion Fields

            #region Constructor

            // Singleton
            private ArrayPool(int minPooledArraySizeLog2, int maxPooledArraySizeLog2, int maxPoolCount)
            {
                _minPooledArraySizeLog2 = minPooledArraySizeLog2;
                _maxPooledArraySizeLog2 = maxPooledArraySizeLog2;
                _maxPoolCount = maxPoolCount;

                // Init pools
                _pools = new Stack<WeakReference>[maxPooledArraySizeLog2 - minPooledArraySizeLog2 + 1];
                for (int i = 0; i < _pools.Length; ++i)
                {
                    // Calls to those pools are sync in code
                    _pools[i] = new Stack<WeakReference>();
                }
            }

            #endregion Constructor

            #region Pool management methods

            /// <summary>
            /// Either returns array of given size from pool or creates it.
            /// </summary>
            /// <param name="length">Array length (always pow of 2).</param>
            /// <returns>Always array instance ready to use.</returns>
            public T[] GetArray(uint length)
            {
                int lengthLog2 = Bits.Msb(length);

                // Check if we can search in pool
                if (lengthLog2 >= _minPooledArraySizeLog2 && lengthLog2 <= _maxPooledArraySizeLog2)
                {
                    // Get needed pool
                    Stack<WeakReference> pool = _pools[lengthLog2 - _minPooledArraySizeLog2];

                    // Try to find at least one not collected array of given size
                    while (pool.Count > 0)
                    {
                        WeakReference arrayRef;
                        lock (pool)
                        {
                            // Double-guard here
                            if (pool.Count > 0)
                            {
                                arrayRef = pool.Pop();
                            }
                            else
                            {
                                // Well, we can exit here
                                break;
                            }
                        }

                        // Maybe return found array if link is alive
                        T[] array = (T[])arrayRef.Target;
                        if (arrayRef.IsAlive) return array;
                    }
                }

                // Array can't be found in pool - create new one
                return new T[length];
            }

            /// <summary>
            /// Adds array to pool.
            /// </summary>
            /// <param name="array">Array to add (it/s length is always pow of 2).</param>
            public void AddArray(T[] array)
            {
                int lengthLog2 = Bits.Msb((uint)array.LongLength);

                // Check if we can add in pool
                if (lengthLog2 >= _minPooledArraySizeLog2 && lengthLog2 <= _maxPooledArraySizeLog2)
                {
                    // Get needed pool
                    Stack<WeakReference> pool = _pools[lengthLog2 - _minPooledArraySizeLog2];

                    // Add array to pool (only if pool size is not too big)
                    if (pool.Count <= _maxPoolCount)
                    {
                        lock (pool)
                        {
                            // Double-guard here
                            if (pool.Count <= _maxPoolCount)
                            {
                                pool.Push(new WeakReference(array));
                            }
                        }
                    }
                }
            }

            #endregion Pool management methods
        }
        #endregion

        #region StrRepHelper
        /// <summary>
        /// Helps to work with <see cref="IntX" /> string representations.
        /// </summary>
        static internal class StrRepHelper
        {
            /// <summary>
            /// Returns digit for given char.
            /// </summary>
            /// <param name="charToDigits">Char->digit dictionary.</param>
            /// <param name="ch">Char which represents big integer digit.</param>
            /// <param name="numberBase">String representation number base.</param>
            /// <returns>Digit.</returns>
            /// <exception cref="FormatException"><paramref name="ch" /> is not in valid format.</exception>
            static public uint GetDigit(IDictionary<char, uint> charToDigits, char ch, uint numberBase)
            {
                if (charToDigits == null)
                {
                    throw new ArgumentNullException("charToDigits");
                }

                // Try to identify this digit
                uint digit;
                if (!charToDigits.TryGetValue(ch, out digit))
                {
                    throw new FormatException("Invalid character in input.");
                }
                if (digit >= numberBase)
                {
                    throw new FormatException("Digit is too big for this base.");
                }
                return digit;
            }

            /// <summary>
            /// Verfies string alphabet provider by user for validity.
            /// </summary>
            /// <param name="alphabet">Alphabet.</param>
            /// <param name="numberBase">String representation number base.</param>
            static public void AssertAlphabet(string alphabet, uint numberBase)
            {
                if (alphabet == null)
                {
                    throw new ArgumentNullException("alphabet");
                }

                // Ensure that alphabet has enough characters to represent numbers in given base
                if (alphabet.Length < numberBase)
                {
                    throw new ArgumentException(string.Format("Alphabet is too small to represent numbers in base {0}.", numberBase), "alphabet");
                }

                // Ensure that all the characters in alphabet are unique
                char[] sortedChars = alphabet.ToCharArray();
                Array.Sort(sortedChars);
                for (int i = 0; i < sortedChars.Length; i++)
                {
                    if (i > 0 && sortedChars[i] == sortedChars[i - 1])
                    {
                        throw new ArgumentException("Alphabet characters must be unique.", "alphabet");
                    }
                }
            }

            /// <summary>
            /// Generates char->digit dictionary from alphabet.
            /// </summary>
            /// <param name="alphabet">Alphabet.</param>
            /// <param name="numberBase">String representation number base.</param>
            /// <returns>Char->digit dictionary.</returns>
            static public IDictionary<char, uint> CharDictionaryFromAlphabet(string alphabet, uint numberBase)
            {
                AssertAlphabet(alphabet, numberBase);
                Dictionary<char, uint> charToDigits = new Dictionary<char, uint>((int)numberBase);
                for (int i = 0; i < numberBase; i++)
                {
                    charToDigits.Add(alphabet[i], (uint)i);
                }
                return charToDigits;
            }
        }
        #endregion

        #region OpHelper
        /// <summary>
        /// Contains helping methods for operations over <see cref="IntX" />.
        /// </summary>
        static internal class OpHelper
        {
            #region Add operation

            /// <summary>
            /// Adds two big integers.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second big integer.</param>
            /// <returns>Resulting big integer.</returns>
            /// <exception cref="ArgumentException"><paramref name="int1" /> or <paramref name="int2" /> is too big for add operation.</exception>
            static public IntX Add(IntX int1, IntX int2)
            {
                // Process zero values in special way
                if (int2._length == 0) return new IntX(int1);
                if (int1._length == 0)
                {
                    IntX x = new IntX(int2);
                    x._negative = int1._negative; // always get sign of the first big integer
                    return x;
                }

                // Determine big int with lower length
                IntX smallerInt;
                IntX biggerInt;
                DigitHelper.GetMinMaxLengthObjects(int1, int2, out smallerInt, out biggerInt);

                // Check for add operation possibility
                if (biggerInt._length == uint.MaxValue)
                {
                    throw new ArgumentException("One of the operated big integers is too big.");
                }

                // Create new big int object of needed length
                IntX newInt = new IntX(biggerInt._length + 1, int1._negative);

                // Do actual addition
                newInt._length = DigitOpHelper.Add(
                    biggerInt._digits,
                    biggerInt._length,
                    smallerInt._digits,
                    smallerInt._length,
                    newInt._digits);

                // Normalization may be needed
                newInt.TryNormalize();

                return newInt;
            }

            #endregion Add operation

            #region Subtract operation

            /// <summary>
            /// Subtracts two big integers.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second big integer.</param>
            /// <returns>Resulting big integer.</returns>
            static public IntX Sub(IntX int1, IntX int2)
            {
                // Process zero values in special way
                if (int1._length == 0) return new IntX(int2._digits, true);
                if (int2._length == 0) return new IntX(int1);

                // Determine lower big int (without sign)
                IntX smallerInt;
                IntX biggerInt;
                int compareResult = DigitOpHelper.Cmp(int1._digits, int1._length, int2._digits, int2._length);
                if (compareResult == 0) return new IntX(); // integers are equal
                if (compareResult < 0)
                {
                    smallerInt = int1;
                    biggerInt = int2;
                }
                else
                {
                    smallerInt = int2;
                    biggerInt = int1;
                }

                // Create new big int object
                IntX newInt = new IntX(biggerInt._length, ReferenceEquals(int1, smallerInt) ^ int1._negative);

                // Do actual subtraction
                newInt._length = DigitOpHelper.Sub(
                    biggerInt._digits,
                    biggerInt._length,
                    smallerInt._digits,
                    smallerInt._length,
                    newInt._digits);

                // Normalization may be needed
                newInt.TryNormalize();

                return newInt;
            }

            #endregion Subtract operation

            #region Add/Subtract operation - common methods

            /// <summary>
            /// Adds/subtracts one <see cref="IntX" /> to/from another.
            /// Determines which operation to use basing on operands signs.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second big integer.</param>
            /// <param name="subtract">Was subtraction initially.</param>
            /// <returns>Add/subtract operation result.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="int1" /> or <paramref name="int2" /> is a null reference.</exception>
            static public IntX AddSub(IntX int1, IntX int2, bool subtract)
            {
                // Exceptions
                if (ReferenceEquals(int1, null))
                {
                    throw new ArgumentNullException("int1", "Operands can't be null.");
                }
                else if (ReferenceEquals(int2, null))
                {
                    throw new ArgumentNullException("int2", "Operands can't be null.");
                }

                // Determine real operation type and result sign
                return subtract ^ int1._negative == int2._negative ? Add(int1, int2) : Sub(int1, int2);
            }

            #endregion Add/Subtract operation - common methods

            #region Power operation

            /// <summary>
            /// Returns a specified big integer raised to the specified power.
            /// </summary>
            /// <param name="value">Number to raise.</param>
            /// <param name="power">Power.</param>
            /// <returns>Number in given power.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
            static public IntX Pow(IntX value, uint power)
            {
                // Exception
                if (ReferenceEquals(value, null))
                {
                    throw new ArgumentNullException("value");
                }

                // Return one for zero pow
                if (power == 0) return 1;

                // Return the number itself from a power of one
                if (power == 1) return new IntX(value);

                // Return zero for a zero
                if (value._length == 0) return new IntX();

                // Get first one bit
                int msb = Bits.Msb(power);

                // Do actual raising
                IntX res = value;
                for (uint powerMask = 1U << (msb - 1); powerMask != 0; powerMask >>= 1)
                {
                    // Always square
                    res = IntXMultiplier.Multiply(res, res);

                    // Maybe mul
                    if ((power & powerMask) != 0)
                    {
                        res = IntXMultiplier.Multiply(res, value);
                    }
                }
                return res;
            }

            #endregion Power operation

            #region Compare operation

            /// <summary>
            /// Compares 2 <see cref="IntX" /> objects.
            /// Returns "-2" if any argument is null, "-1" if <paramref name="int1" /> &lt; <paramref name="int2" />,
            /// "0" if equal and "1" if &gt;.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second big integer.</param>
            /// <param name="throwNullException">Raises or not <see cref="NullReferenceException" />.</param>
            /// <returns>Comparison result.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="int1" /> or <paramref name="int2" /> is a null reference and <paramref name="throwNullException" /> is set to true.</exception>
            static public int Cmp(IntX int1, IntX int2, bool throwNullException)
            {
                // If one of the operands is null, throw exception or return -2
                bool isNull1 = ReferenceEquals(int1, null);
                bool isNull2 = ReferenceEquals(int2, null);
                if (isNull1 || isNull2)
                {
                    if (throwNullException)
                    {
                        throw new ArgumentNullException(isNull1 ? "int1" : "int2", "Can't use null in comparsion operations.");
                    }
                    else
                    {
                        return isNull1 && isNull2 ? 0 : -2;
                    }
                }

                // Compare sign
                if (int1._negative && !int2._negative) return -1;
                if (!int1._negative && int2._negative) return 1;

                // Compare presentation
                return DigitOpHelper.Cmp(int1._digits, int1._length, int2._digits, int2._length) * (int1._negative ? -1 : 1);
            }

            /// <summary>
            /// Compares <see cref="IntX" /> object to int.
            /// Returns "-1" if <paramref name="int1" /> &lt; <paramref name="int2" />, "0" if equal and "1" if &gt;.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second integer.</param>
            /// <returns>Comparison result.</returns>
            static public int Cmp(IntX int1, int int2)
            {
                // Special processing for zero
                if (int2 == 0) return int1._length == 0 ? 0 : (int1._negative ? -1 : 1);
                if (int1._length == 0) return int2 > 0 ? -1 : 1;

                // Compare presentation
                if (int1._length > 1) return int1._negative ? -1 : 1;
                uint digit2;
                bool negative2;
                DigitHelper.ToUInt32WithSign(int2, out digit2, out negative2);

                // Compare sign
                if (int1._negative && !negative2) return -1;
                if (!int1._negative && negative2) return 1;

                return int1._digits[0] == digit2 ? 0 : (int1._digits[0] < digit2 ^ negative2 ? -1 : 1);
            }

            /// <summary>
            /// Compares <see cref="IntX" /> object to unsigned int.
            /// Returns "-1" if <paramref name="int1" /> &lt; <paramref name="int2" />, "0" if equal and "1" if &gt;.
            /// For internal use.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second unsigned integer.</param>
            /// <returns>Comparsion result.</returns>
            static public int Cmp(IntX int1, uint int2)
            {
                // Special processing for zero
                if (int2 == 0) return int1._length == 0 ? 0 : (int1._negative ? -1 : 1);
                if (int1._length == 0) return -1;

                // Compare presentation
                if (int1._negative) return -1;
                if (int1._length > 1) return 1;
                return int1._digits[0] == int2 ? 0 : (int1._digits[0] < int2 ? -1 : 1);
            }

            #endregion Compare operation

            #region Shift operation

            /// <summary>
            /// Shifts <see cref="IntX" /> object.
            /// Determines which operation to use basing on shift sign.
            /// </summary>
            /// <param name="intX">Big integer.</param>
            /// <param name="shift">Bits count to shift.</param>
            /// <param name="toLeft">If true the shifting to the left.</param>
            /// <returns>Bitwise shift operation result.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="intX" /> is a null reference.</exception>
            static public IntX Sh(IntX intX, long shift, bool toLeft)
            {
                // Exceptions
                if (ReferenceEquals(intX, null))
                {
                    throw new ArgumentNullException("intX", "Operand can't be null.");
                }

                // Zero can't be shifted
                if (intX._length == 0) return new IntX();

                // Can't shift on zero value
                if (shift == 0) return new IntX(intX);

                // Determine real bits count and direction
                ulong bitCount;
                bool negativeShift;
                DigitHelper.ToUInt64WithSign(shift, out bitCount, out negativeShift);
                toLeft ^= negativeShift;

                // Get position of the most significant bit in intX and amount of bits in intX
                int msb = Bits.Msb(intX._digits[intX._length - 1]);
                ulong intXBitCount = (ulong)(intX._length - 1) * Constants.DigitBitCount + (ulong)msb + 1UL;

                // If shifting to the right and shift is too big then return zero
                if (!toLeft && bitCount >= intXBitCount) return new IntX();

                // Calculate new bit count
                ulong newBitCount = toLeft ? intXBitCount + bitCount : intXBitCount - bitCount;

                // If shifting to the left and shift is too big to fit in big integer, throw an exception
                if (toLeft && newBitCount > Constants.MaxBitCount)
                {
                    throw new ArgumentException("One of the operated big integers is too big.", "intX");
                }

                // Get exact length of new big integer (no normalize is ever needed here).
                // Create new big integer with given length
                uint newLength = (uint)(newBitCount / Constants.DigitBitCount + (newBitCount % Constants.DigitBitCount == 0 ? 0UL : 1UL));
                IntX newInt = new IntX(newLength, intX._negative);

                // Get full and small shift values
                uint fullDigits = (uint)(bitCount / Constants.DigitBitCount);
                int smallShift = (int)(bitCount % Constants.DigitBitCount);

                // We can just copy (no shift) if small shift is zero
                if (smallShift == 0)
                {
                    if (toLeft)
                    {
                        Array.Copy(intX._digits, 0, newInt._digits, fullDigits, intX._length);
                    }
                    else
                    {
                        Array.Copy(intX._digits, fullDigits, newInt._digits, 0, newLength);
                    }
                }
                else
                {
                    // Do copy with real shift in the needed direction
                    if (toLeft)
                    {
                        DigitOpHelper.Shr(intX._digits, 0, intX._length, newInt._digits, fullDigits + 1, Constants.DigitBitCount - smallShift);
                    }
                    else
                    {
                        // If new result length is smaller then original length we shouldn't lose any digits
                        if (newLength < intX._length)
                        {
                            newLength++;
                        }

                        DigitOpHelper.Shr(intX._digits, fullDigits, newLength, newInt._digits, 0, smallShift);
                    }
                }

                return newInt;
            }

            #endregion Shift operation
        }
        #endregion

        #region NewtonHelper
        /// <summary>
        /// Contains helping methods for fast division
        /// using Newton approximation approach and fast multiplication.
        /// </summary>
        static internal class NewtonHelper
        {
            /// <summary>
            /// Generates integer opposite to the given one using approximation.
            /// Uses algorithm from Khuth vol. 2 3rd Edition (4.3.3).
            /// </summary>
            /// <param name="digitsPtr">Initial big integer digits.</param>
            /// <param name="length">Initial big integer length.</param>
            /// <param name="maxLength">Precision length.</param>
            /// <param name="bufferPtr">Buffer in which shifted big integer may be stored.</param>
            /// <param name="newLength">Resulting big integer length.</param>
            /// <param name="rightShift">How much resulting big integer is shifted to the left (or: must be shifted to the right).</param>
            /// <returns>Resulting big integer digits.</returns>
            static unsafe public uint[] GetIntegerOpposite(uint* digitsPtr, uint length, uint maxLength, uint* bufferPtr, out uint newLength, out ulong rightShift)
            {
                // Maybe initially shift original digits a bit to the left
                // (it must have MSB on 2nd position in the highest digit)
                int msb = Bits.Msb(digitsPtr[length - 1]);
                rightShift = (ulong)(length - 1) * Constants.DigitBitCount + (ulong)msb + 1U;

                if (msb != 2)
                {
                    // Shift to the left (via actually right shift)
                    int leftShift = (2 - msb + Constants.DigitBitCount) % Constants.DigitBitCount;
                    length = DigitOpHelper.Shr(digitsPtr, length, bufferPtr + 1, Constants.DigitBitCount - leftShift, true) + 1U;
                }
                else
                {
                    // Simply use the same digits without any shifting
                    bufferPtr = digitsPtr;
                }

                // Calculate possible result length
                int lengthLog2 = Bits.CeilLog2(maxLength);
                uint newLengthMax = 1U << (lengthLog2 + 1);
                int lengthLog2Bits = lengthLog2 + Bits.Msb(Constants.DigitBitCount);

                // Create result digits
                uint[] resultDigits = ArrayPool<uint>.Instance.GetArray(newLengthMax); //new uint[newLengthMax];
                uint resultLength;

                // Create temporary digits for squared result (twice more size)
                uint[] resultDigitsSqr = ArrayPool<uint>.Instance.GetArray(newLengthMax); //new uint[newLengthMax];
                uint resultLengthSqr;

                // Create temporary digits for squared result * buffer
                uint[] resultDigitsSqrBuf = new uint[newLengthMax + length];
                uint resultLengthSqrBuf;

                // Fix some digits
                fixed (uint* resultPtrFixed = resultDigits, resultSqrPtrFixed = resultDigitsSqr, resultSqrBufPtr = resultDigitsSqrBuf)
                {
                    uint* resultPtr = resultPtrFixed;
                    uint* resultSqrPtr = resultSqrPtrFixed;

                    // Cache two first digits
                    uint bufferDigitN1 = bufferPtr[length - 1];
                    uint bufferDigitN2 = bufferPtr[length - 2];

                    // Prepare result.
                    // Initially result = floor(32 / (4*v1 + 2*v2 + v3)) / 4
                    // (last division is not floored - here we emulate fixed point)
                    resultDigits[0] = 32 / bufferDigitN1;
                    resultLength = 1;

                    // Prepare variables
                    uint nextBufferTempStorage = 0;
                    int nextBufferTempShift;
                    uint nextBufferLength = 1U;
                    uint* nextBufferPtr = &nextBufferTempStorage;

                    ulong bitsAfterDotResult;
                    ulong bitsAfterDotResultSqr;
                    ulong bitsAfterDotNextBuffer;
                    ulong bitShift;
                    uint shiftOffset;

                    uint* tempPtr;
                    uint[] tempDigits;

                    // Iterate 'till result will be precise enough
                    for (int k = 0; k < lengthLog2Bits; ++k)
                    {
                        // Get result squared
                        resultLengthSqr = IntXMultiplier.Multiply(
                            resultPtr,
                            resultLength,
                            resultPtr,
                            resultLength,
                            resultSqrPtr);

                        // Calculate current result bits after dot
                        bitsAfterDotResult = (1UL << k) + 1UL;
                        bitsAfterDotResultSqr = bitsAfterDotResult << 1;

                        // Here we will get the next portion of data from bufferPtr
                        if (k < 4)
                        {
                            // For now buffer intermediate has length 1 and we will use this fact
                            nextBufferTempShift = 1 << (k + 1);
                            nextBufferTempStorage =
                                bufferDigitN1 << nextBufferTempShift |
                                bufferDigitN2 >> (Constants.DigitBitCount - nextBufferTempShift);

                            // Calculate amount of bits after dot (simple formula here)
                            bitsAfterDotNextBuffer = (ulong)nextBufferTempShift + 3UL;
                        }
                        else
                        {
                            // Determine length to get from bufferPtr
                            nextBufferLength = System.Math.Min((1U << (k - 4)) + 1U, length);
                            nextBufferPtr = bufferPtr + (length - nextBufferLength);

                            // Calculate amount of bits after dot (simple formula here)
                            bitsAfterDotNextBuffer = (ulong)(nextBufferLength - 1U) * Constants.DigitBitCount + 3UL;
                        }

                        // Multiply result ^ 2 and nextBuffer + calculate new amount of bits after dot
                        resultLengthSqrBuf = IntXMultiplier.Multiply(
                            resultSqrPtr,
                            resultLengthSqr,
                            nextBufferPtr,
                            nextBufferLength,
                            resultSqrBufPtr);

                        bitsAfterDotNextBuffer += bitsAfterDotResultSqr;

                        // Now calculate 2 * result - resultSqrBufPtr
                        --bitsAfterDotResult;
                        --bitsAfterDotResultSqr;

                        // Shift result on a needed amount of bits to the left
                        bitShift = bitsAfterDotResultSqr - bitsAfterDotResult;
                        shiftOffset = (uint)(bitShift / Constants.DigitBitCount);
                        resultLength =
                            shiftOffset + 1U +
                            DigitOpHelper.Shr(
                                resultPtr,
                                resultLength,
                                resultSqrPtr + shiftOffset + 1U,
                                Constants.DigitBitCount - (int)(bitShift % Constants.DigitBitCount),
                                true);

                        // Swap resultPtr and resultSqrPtr pointers
                        tempPtr = resultPtr;
                        resultPtr = resultSqrPtr;
                        resultSqrPtr = tempPtr;

                        tempDigits = resultDigits;
                        resultDigits = resultDigitsSqr;
                        resultDigitsSqr = tempDigits;

                        DigitHelper.SetBlockDigits(resultPtr, shiftOffset, 0U);

                        bitShift = bitsAfterDotNextBuffer - bitsAfterDotResultSqr;
                        shiftOffset = (uint)(bitShift / Constants.DigitBitCount);

                        if (shiftOffset < resultLengthSqrBuf)
                        {
                            // Shift resultSqrBufPtr on a needed amount of bits to the right
                            resultLengthSqrBuf = DigitOpHelper.Shr(
                                resultSqrBufPtr + shiftOffset,
                                resultLengthSqrBuf - shiftOffset,
                                resultSqrBufPtr,
                                (int)(bitShift % Constants.DigitBitCount),
                                false);

                            // Now perform actual subtraction
                            resultLength = DigitOpHelper.Sub(
                                resultPtr,
                                resultLength,
                                resultSqrBufPtr,
                                resultLengthSqrBuf,
                                resultPtr);
                        }
                        else
                        {
                            // Actually we can assume resultSqrBufPtr == 0 here and have nothing to do
                        }
                    }
                }

                // Return some arrays to pool
                ArrayPool<uint>.Instance.AddArray(resultDigitsSqr);

                rightShift += (1UL << lengthLog2Bits) + 1UL;
                newLength = resultLength;
                return resultDigits;
            }
        }
        #endregion

        #region FhtHelper
        /// <summary>
        /// Contains helping methods for work with FHT (Fast Hartley Transform).
        /// FHT is a better alternative of FFT (Fast Fourier Transform) - at least for <see cref="System.IntX" />.
        /// </summary>
        static unsafe internal class FhtHelper
        {
            #region struct TrigValues

            /// <summary>
            /// Trigonometry values.
            /// </summary>
            internal struct TrigValues
            {
                /// <summary>
                /// Sin value from <see cref="SineTable" />.
                /// </summary>
                public double TableSin;

                /// <summary>
                /// Cos value from <see cref="SineTable" />.
                /// </summary>
                public double TableCos;

                /// <summary>
                /// Sin value.
                /// </summary>
                public double Sin;

                /// <summary>
                /// Cos value.
                /// </summary>
                public double Cos;
            }

            #endregion struct SinCos

            #region Private constants (or static readonly fields)

            // double[] data base
            const int DoubleDataBytes = 1;
            const int DoubleDataLengthShift = 2 - (DoubleDataBytes >> 1);
            const int DoubleDataDigitShift = DoubleDataBytes << 3;
            const long DoubleDataBaseInt = 1L << DoubleDataDigitShift;
            const double DoubleDataBase = DoubleDataBaseInt;
            const double DoubleDataBaseDiv2 = DoubleDataBase / 2.0;

            // SQRT(2) and SQRT(2) / 2
            static readonly double Sqrt2 = System.Math.Sqrt(2.0);
            static readonly double Sqrt2Div2 = Sqrt2 / 2.0;

            // SIN() table
            static readonly double[] SineTable = new double[31];

            #endregion Private constants (or static readonly fields)

            #region Constructors

            // .cctor
            static FhtHelper()
            {
                // Initialize SinTable
                FillSineTable(SineTable);
            }

            #endregion Constructors

            #region Data conversion methods

            /// <summary>
            /// Converts <see cref="System.IntX" /> digits into real representation (used in FHT).
            /// </summary>
            /// <param name="digits">Big integer digits.</param>
            /// <param name="length"><paramref name="digits" /> length.</param>
            /// <param name="newLength">Multiplication result length (must be pow of 2).</param>
            /// <returns>Double array.</returns>
            static public double[] ConvertDigitsToDouble(uint[] digits, uint length, uint newLength)
            {
                fixed (uint* digitsPtr = digits)
                {
                    return ConvertDigitsToDouble(digitsPtr, length, newLength);
                }
            }

            /// <summary>
            /// Converts <see cref="System.IntX" /> digits into real representation (used in FHT).
            /// </summary>
            /// <param name="digitsPtr">Big integer digits.</param>
            /// <param name="length"><paramref name="digitsPtr" /> length.</param>
            /// <param name="newLength">Multiplication result length (must be pow of 2).</param>
            /// <returns>Double array.</returns>
            static public double[] ConvertDigitsToDouble(uint* digitsPtr, uint length, uint newLength)
            {
                // Maybe fix newLength (make it the nearest bigger pow of 2)
                newLength = 1U << Bits.CeilLog2(newLength);

                // For better FHT accuracy we will choose length smaller then dwords.
                // So new length must be modified accordingly
                newLength <<= DoubleDataLengthShift;
                double[] data = ArrayPool<double>.Instance.GetArray(newLength);

                // Run in unsafe context
                fixed (double* slice = data)
                {
                    // Amount of units pointed by digitsPtr
                    uint unitCount = length << DoubleDataLengthShift;

                    // Copy all words from digits into new double[]
                    byte* unitDigitsPtr = (byte*)digitsPtr;
                    for (uint i = 0; i < unitCount; ++i)
                    {
                        slice[i] = unitDigitsPtr[i];
                    }

                    // Clear remaining double values (this array is from pool and may be dirty)
                    DigitHelper.SetBlockDigits(slice + unitCount, newLength - unitCount, 0.0);

                    // FHT (as well as FFT) works more accurate with "balanced" data, so let's balance it
                    double carry = 0, dataDigit;
                    for (uint i = 0; i < unitCount || i < newLength && carry != 0; ++i)
                    {
                        dataDigit = slice[i] + carry;
                        if (dataDigit >= DoubleDataBaseDiv2)
                        {
                            dataDigit -= DoubleDataBase;
                            carry = 1.0;
                        }
                        else
                        {
                            carry = 0;
                        }
                        slice[i] = dataDigit;
                    }

                    if (carry > 0)
                    {
                        slice[0] -= carry;
                    }
                }

                return data;
            }

            /// <summary>
            /// Converts real digits representation (result of FHT) into usual <see cref="System.IntX" /> digits.
            /// </summary>
            /// <param name="array">Real digits representation.</param>
            /// <param name="length"><paramref name="array" /> length.</param>
            /// <param name="digitsLength">New digits array length (we always do know the upper value for this array).</param>
            /// <param name="digitsRes">Big integer digits.</param>
            static unsafe public void ConvertDoubleToDigits(double[] array, uint length, uint digitsLength, uint[] digitsRes)
            {
                fixed (double* slice = array)
                fixed (uint* digitsResPtr = digitsRes)
                {
                    ConvertDoubleToDigits(slice, length, digitsLength, digitsResPtr);
                }
            }

            /// <summary>
            /// Converts real digits representation (result of FHT) into usual <see cref="System.IntX" /> digits.
            /// </summary>
            /// <param name="slice">Real digits representation.</param>
            /// <param name="length"><paramref name="slice" /> length.</param>
            /// <param name="digitsLength">New digits array length (we always do know the upper value for this array).</param>
            /// <param name="digitsResPtr">Resulting digits storage.</param>
            /// <returns>Big integer digits (dword values).</returns>
            static unsafe public void ConvertDoubleToDigits(double* slice, uint length, uint digitsLength, uint* digitsResPtr)
            {
                // Calculate data multiplier (don't forget about additional div 2)
                double normalizeMultiplier = 0.5 / length;

                // Count of units in digits
                uint unitCount = digitsLength << DoubleDataLengthShift;

                // Carry and current digit
                double carry = 0, dataDigit;
                long carryInt = 0, dataDigitInt;


                // Walk thru all double digits
                byte* unitDigitsPtr = (byte*)digitsResPtr;
                for (uint i = 0; i < length; ++i)
                {
                    // Get data digit (don't forget it might be balanced)
                    dataDigit = slice[i] * normalizeMultiplier;

                    // Round to the nearest
                    dataDigitInt = (long)(dataDigit < 0 ? dataDigit - 0.5 : dataDigit + 0.5) + carryInt;

                    // Get next carry floored; maybe modify data digit
                    carry = dataDigitInt / DoubleDataBase;
                    if (carry < 0)
                    {
                        carry += carry % 1.0;
                    }
                    carryInt = (long)carry;

                    dataDigitInt -= carryInt << DoubleDataDigitShift;
                    if (dataDigitInt < 0)
                    {
                        dataDigitInt += DoubleDataBaseInt;
                        --carryInt;
                    }

                    // Maybe add to the digits
                    if (i < unitCount)
                    {
                        unitDigitsPtr[i] = (byte)dataDigitInt;
                    }
                }

                // Last carry must be accounted
                if (carryInt < 0)
                {
                    digitsResPtr[0] -= (uint)-carryInt;
                }
                else if (carryInt > 0)
                {
                    uint digitsCarry = (uint)carryInt, oldDigit;
                    for (uint i = 0; digitsCarry != 0 && i < digitsLength; ++i)
                    {
                        oldDigit = digitsResPtr[i];
                        digitsResPtr[i] += digitsCarry;

                        // Check for an overflow
                        digitsCarry = digitsResPtr[i] < oldDigit ? 1U : 0U;
                    }
                }
            }

            #endregion Data conversion methods

            #region FHT

            /// <summary>
            /// Performs FHT "in place" for given double[] array.
            /// </summary>
            /// <param name="array">Double array.</param>
            /// <param name="length">Array length.</param>
            static public void Fht(double[] array, uint length)
            {
                fixed (double* slice = array)
                {
                    Fht(slice, length, Bits.Msb(length));
                }
            }

            /// <summary>
            /// Performs FHT "in place" for given double[] array slice.
            /// </summary>
            /// <param name="slice">Double array slice.</param>
            /// <param name="length">Slice length.</param>
            /// <param name="lengthLog2">Log2(<paramref name="length" />).</param>
            static public void Fht(double* slice, uint length, int lengthLog2)
            {
                // Special fast processing for length == 4
                if (length == 4)
                {
                    Fht4(slice);
                    return;
                }

                // Divide data into 2 recursively processed parts
                length >>= 1;
                --lengthLog2;
                double* rightSlice = slice + length;

                uint lengthDiv2 = length >> 1;
                uint lengthDiv4 = length >> 2;

                // Perform initial "butterfly" operations over left and right array parts
                double leftDigit = slice[0];
                double rightDigit = rightSlice[0];
                slice[0] = leftDigit + rightDigit;
                rightSlice[0] = leftDigit - rightDigit;

                leftDigit = slice[lengthDiv2];
                rightDigit = rightSlice[lengthDiv2];
                slice[lengthDiv2] = leftDigit + rightDigit;
                rightSlice[lengthDiv2] = leftDigit - rightDigit;

                // Get initial trig values
                //TrigValues trigValues = GetInitialTrigValues(lengthLog2);
                TrigValues trigValues = new TrigValues();
                GetInitialTrigValues(&trigValues, lengthLog2);

                // Perform "butterfly"
                for (uint i = 1; i < lengthDiv4; ++i)
                {
                    FhtButterfly(slice, rightSlice, i, length - i, trigValues.Cos, trigValues.Sin);
                    FhtButterfly(slice, rightSlice, lengthDiv2 - i, lengthDiv2 + i, trigValues.Sin, trigValues.Cos);

                    // Get next trig values
                    NextTrigValues(&trigValues);
                }

                // Final "butterfly"
                FhtButterfly(slice, rightSlice, lengthDiv4, length - lengthDiv4, Sqrt2Div2, Sqrt2Div2);

                // Finally perform recursive run
                Fht(slice, length, lengthLog2);
                Fht(rightSlice, length, lengthLog2);
            }

            /// <summary>
            /// Performs FHT "in place" for given double[] array slice.
            /// Fast version for length == 4.
            /// </summary>
            /// <param name="slice">Double array slice.</param>
            static private void Fht4(double* slice)
            {
                // Get 4 digits
                double d0 = slice[0];
                double d1 = slice[1];
                double d2 = slice[2];
                double d3 = slice[3];

                // Perform fast "butterfly" addition/subtraction for them.
                // In case when length == 4 we can do it without trigonometry
                double d02 = d0 + d2;
                double d13 = d1 + d3;
                slice[0] = d02 + d13;
                slice[1] = d02 - d13;

                d02 = d0 - d2;
                d13 = d1 - d3;
                slice[2] = d02 + d13;
                slice[3] = d02 - d13;
            }

            #endregion FHT

            #region FHT results multiplication

            /// <summary>
            /// Multiplies two FHT results and stores multiplication in first one.
            /// </summary>
            /// <param name="data">First FHT result.</param>
            /// <param name="data2">Second FHT result.</param>
            /// <param name="length">FHT results length.</param>
            static public void MultiplyFhtResults(double[] data, double[] data2, uint length)
            {
                fixed (double* slice = data, slice2 = data2)
                {
                    MultiplyFhtResults(slice, slice2, length);
                }
            }

            /// <summary>
            /// Multiplies two FHT results and stores multiplication in first one.
            /// </summary>
            /// <param name="slice">First FHT result.</param>
            /// <param name="slice2">Second FHT result.</param>
            /// <param name="length">FHT results length.</param>
            static public void MultiplyFhtResults(double* slice, double* slice2, uint length)
            {
                // Step0 and Step1
                slice[0] *= 2.0 * slice2[0];
                slice[1] *= 2.0 * slice2[1];

                // Perform all other steps
                double d11, d12, d21, d22, ad, sd;
                for (uint stepStart = 2, stepEnd = 4, index1, index2; stepStart < length; stepStart *= 2, stepEnd *= 2)
                {
                    for (index1 = stepStart, index2 = stepEnd - 1; index1 < stepEnd; index1 += 2, index2 -= 2)
                    {
                        d11 = slice[index1];
                        d12 = slice[index2];
                        d21 = slice2[index1];
                        d22 = slice2[index2];

                        ad = d11 + d12;
                        sd = d11 - d12;

                        slice[index1] = d21 * ad + d22 * sd;
                        slice[index2] = d22 * ad - d21 * sd;
                    }
                }
            }

            #endregion FHT results multiplication

            #region Reverse FHT

            /// <summary>
            /// Performs FHT reverse "in place" for given double[] array.
            /// </summary>
            /// <param name="array">Double array.</param>
            /// <param name="length">Array length.</param>
            static public void ReverseFht(double[] array, uint length)
            {
                fixed (double* slice = array)
                {
                    ReverseFht(slice, length, Bits.Msb(length));
                }
            }

            /// <summary>
            /// Performs reverse FHT "in place" for given double[] array slice.
            /// </summary>
            /// <param name="slice">Double array slice.</param>
            /// <param name="length">Slice length.</param>
            /// <param name="lengthLog2">Log2(<paramref name="length" />).</param>
            static public void ReverseFht(double* slice, uint length, int lengthLog2)
            {
                // Special fast processing for length == 8
                if (length == 8)
                {
                    ReverseFht8(slice);
                    return;
                }

                // Divide data into 2 recursively processed parts
                length >>= 1;
                --lengthLog2;
                double* rightSlice = slice + length;

                uint lengthDiv2 = length >> 1;
                uint lengthDiv4 = length >> 2;

                // Perform recursive run
                ReverseFht(slice, length, lengthLog2);
                ReverseFht(rightSlice, length, lengthLog2);

                // Get initial trig values
                TrigValues trigValues = new TrigValues();
                GetInitialTrigValues(&trigValues, lengthLog2);

                // Perform "butterfly"
                for (uint i = 1; i < lengthDiv4; ++i)
                {
                    ReverseFhtButterfly(slice, rightSlice, i, length - i, trigValues.Cos, trigValues.Sin);
                    ReverseFhtButterfly(slice, rightSlice, lengthDiv2 - i, lengthDiv2 + i, trigValues.Sin, trigValues.Cos);

                    // Get next trig values
                    NextTrigValues(&trigValues);
                }

                // Final "butterfly"
                ReverseFhtButterfly(slice, rightSlice, lengthDiv4, length - lengthDiv4, Sqrt2Div2, Sqrt2Div2);
                ReverseFhtButterfly2(slice, rightSlice, 0, 0, 1.0, 0);
                ReverseFhtButterfly2(slice, rightSlice, lengthDiv2, lengthDiv2, 0, 1.0);
            }

            /// <summary>
            /// Performs reverse FHT "in place" for given double[] array slice.
            /// Fast version for length == 8.
            /// </summary>
            /// <param name="slice">Double array slice.</param>
            static private void ReverseFht8(double* slice)
            {
                // Get 8 digits	
                double d0 = slice[0];
                double d1 = slice[1];
                double d2 = slice[2];
                double d3 = slice[3];
                double d4 = slice[4];
                double d5 = slice[5];
                double d6 = slice[6];
                double d7 = slice[7];

                // Calculate add and subtract pairs for first 4 digits
                double da01 = d0 + d1;
                double ds01 = d0 - d1;
                double da23 = d2 + d3;
                double ds23 = d2 - d3;

                // Calculate add and subtract pairs for first pairs
                double daa0123 = da01 + da23;
                double dsa0123 = da01 - da23;
                double das0123 = ds01 + ds23;
                double dss0123 = ds01 - ds23;

                // Calculate add and subtract pairs for next 4 digits
                double da45 = d4 + d5;
                double ds45 = (d4 - d5) * Sqrt2;
                double da67 = d6 + d7;
                double ds67 = (d6 - d7) * Sqrt2;

                // Calculate add and subtract pairs for next pairs
                double daa4567 = da45 + da67;
                double dsa4567 = da45 - da67;

                // Store digits values
                slice[0] = daa0123 + daa4567;
                slice[4] = daa0123 - daa4567;
                slice[2] = dsa0123 + dsa4567;
                slice[6] = dsa0123 - dsa4567;
                slice[1] = das0123 + ds45;
                slice[5] = das0123 - ds45;
                slice[3] = dss0123 + ds67;
                slice[7] = dss0123 - ds67;
            }

            #endregion Reverse FHT

            #region "Butterfly" methods for FHT

            /// <summary>
            /// Performs "butterfly" operation for <see cref="Fht(double*, uint, int)" />.
            /// </summary>
            /// <param name="slice1">First data array slice.</param>
            /// <param name="slice2">Second data array slice.</param>
            /// <param name="index1">First slice index.</param>
            /// <param name="index2">Second slice index.</param>
            /// <param name="cos">Cos value.</param>
            /// <param name="sin">Sin value.</param>
            static private void FhtButterfly(double* slice1, double* slice2, uint index1, uint index2, double cos, double sin)
            {
                double d11 = slice1[index1];
                double d12 = slice1[index2];

                double temp = slice2[index1];
                slice1[index1] = d11 + temp;
                d11 -= temp;

                temp = slice2[index2];
                slice1[index2] = d12 + temp;
                d12 -= temp;

                slice2[index1] = d11 * cos + d12 * sin;
                slice2[index2] = d11 * sin - d12 * cos;
            }

            /// <summary>
            /// Performs "butterfly" operation for <see cref="ReverseFht(double*, uint, int)" />.
            /// </summary>
            /// <param name="slice1">First data array slice.</param>
            /// <param name="slice2">Second data array slice.</param>
            /// <param name="index1">First slice index.</param>
            /// <param name="index2">Second slice index.</param>
            /// <param name="cos">Cos value.</param>
            /// <param name="sin">Sin value.</param>
            static private void ReverseFhtButterfly(double* slice1, double* slice2, uint index1, uint index2, double cos, double sin)
            {
                double d21 = slice2[index1];
                double d22 = slice2[index2];

                double temp = slice1[index1];
                double temp2 = d21 * cos + d22 * sin;
                slice1[index1] = temp + temp2;
                slice2[index1] = temp - temp2;

                temp = slice1[index2];
                temp2 = d21 * sin - d22 * cos;
                slice1[index2] = temp + temp2;
                slice2[index2] = temp - temp2;
            }

            /// <summary>
            /// Performs "butterfly" operation for <see cref="ReverseFht(double*, uint, int)" />.
            /// Another version.
            /// </summary>
            /// <param name="slice1">First data array slice.</param>
            /// <param name="slice2">Second data array slice.</param>
            /// <param name="index1">First slice index.</param>
            /// <param name="index2">Second slice index.</param>
            /// <param name="cos">Cos value.</param>
            /// <param name="sin">Sin value.</param>
            static private void ReverseFhtButterfly2(double* slice1, double* slice2, uint index1, uint index2, double cos, double sin)
            {
                double temp = slice1[index1];
                double temp2 = slice2[index1] * cos + slice2[index2] * sin;
                slice1[index1] = temp + temp2;
                slice2[index2] = temp - temp2;
            }

            #endregion "Butterfly" methods for FHT

            #region Trigonometry working methods

            /// <summary>
            /// Fills sine table for FHT.
            /// </summary>
            /// <param name="sineTable">Sine table to fill.</param>
            static private void FillSineTable(double[] sineTable)
            {
                for (int i = 0, p = 1; i < sineTable.Length; ++i, p *= 2)
                {
                    sineTable[i] = System.Math.Sin(System.Math.PI / p);
                }
            }

            /// <summary>
            /// Initializes trigonometry values for FHT.
            /// </summary>
            /// <param name="valuesPtr">Values to init.</param>
            /// <param name="lengthLog2">Log2(processing slice length).</param>
            static private void GetInitialTrigValues(TrigValues* valuesPtr, int lengthLog2)
            {
                valuesPtr->TableSin = SineTable[lengthLog2];
                valuesPtr->TableCos = SineTable[lengthLog2 + 1];
                valuesPtr->TableCos *= -2.0 * valuesPtr->TableCos;

                valuesPtr->Sin = valuesPtr->TableSin;
                valuesPtr->Cos = valuesPtr->TableCos + 1.0;
            }

            /// <summary>
            /// Generates next trigonometry values for FHT basing on previous ones.
            /// </summary>
            /// <param name="valuesPtr">Current trig values.</param>
            static private void NextTrigValues(TrigValues* valuesPtr)
            {
                double oldCos = valuesPtr->Cos;
                valuesPtr->Cos = valuesPtr->Cos * valuesPtr->TableCos - valuesPtr->Sin * valuesPtr->TableSin + valuesPtr->Cos;
                valuesPtr->Sin = valuesPtr->Sin * valuesPtr->TableCos + oldCos * valuesPtr->TableSin + valuesPtr->Sin;
            }

            #endregion Trigonometry working methods
        }
        #endregion

        #region DigitOpHelper
        /// <summary>
        /// Contains helping methods for operations over <see cref="System.IntX" /> digits as arrays.
        /// </summary>
        static internal class DigitOpHelper
        {
            #region Add operation

            /// <summary>
            /// Adds two big integers.
            /// </summary>
            /// <param name="digits1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digits2">Second big integer digits.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsRes">Resulting big integer digits.</param>
            /// <returns>Resulting big integer length.</returns>
            static unsafe public uint Add(uint[] digits1, uint length1, uint[] digits2, uint length2, uint[] digitsRes)
            {
                fixed (uint* digitsPtr1 = digits1, digitsPtr2 = digits2, digitsResPtr = digitsRes)
                {
                    return Add(digitsPtr1, length1, digitsPtr2, length2, digitsResPtr);
                }
            }

            /// <summary>
            /// Adds two big integers using pointers.
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digitsPtr2">Second big integer digits.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsResPtr">Resulting big integer digits.</param>
            /// <returns>Resulting big integer length.</returns>
            static unsafe public uint Add(uint* digitsPtr1, uint length1, uint* digitsPtr2, uint length2, uint* digitsResPtr)
            {
                ulong c = 0;

                if (length1 < length2)
                {
                    // First must be bigger - swap
                    uint lengthTemp = length1;
                    length1 = length2;
                    length2 = lengthTemp;

                    uint* ptrTemp = digitsPtr1;
                    digitsPtr1 = digitsPtr2;
                    digitsPtr2 = ptrTemp;
                }

                // Perform digits adding
                for (uint i = 0; i < length2; ++i)
                {
                    c += (ulong)digitsPtr1[i] + digitsPtr2[i];
                    digitsResPtr[i] = (uint)c;
                    c >>= 32;
                }

                // Perform digits + carry moving
                for (uint i = length2; i < length1; ++i)
                {
                    c += digitsPtr1[i];
                    digitsResPtr[i] = (uint)c;
                    c >>= 32;
                }

                // Account last carry
                if (c != 0)
                {
                    digitsResPtr[length1++] = (uint)c;
                }

                return length1;
            }

            #endregion Add operation

            #region Subtract operation

            /// <summary>
            /// Subtracts two big integers.
            /// </summary>
            /// <param name="digits1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digits2">Second big integer digits.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsRes">Resulting big integer digits.</param>
            /// <returns>Resulting big integer length.</returns>
            static unsafe public uint Sub(uint[] digits1, uint length1, uint[] digits2, uint length2, uint[] digitsRes)
            {
                fixed (uint* digitsPtr1 = digits1, digitsPtr2 = digits2, digitsResPtr = digitsRes)
                {
                    return Sub(digitsPtr1, length1, digitsPtr2, length2, digitsResPtr);
                }
            }

            /// <summary>
            /// Subtracts two big integers using pointers.
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digitsPtr2">Second big integer digits.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <param name="digitsResPtr">Resulting big integer digits.</param>
            /// <returns>Resulting big integer length.</returns>
            static unsafe public uint Sub(uint* digitsPtr1, uint length1, uint* digitsPtr2, uint length2, uint* digitsResPtr)
            {
                ulong c = 0;

                // Perform digits subtraction
                for (uint i = 0; i < length2; ++i)
                {
                    c = (ulong)digitsPtr1[i] - digitsPtr2[i] - c;
                    digitsResPtr[i] = (uint)c;
                    c >>= 63;
                }

                // Perform digits + carry moving
                for (uint i = length2; i < length1; ++i)
                {
                    c = digitsPtr1[i] - c;
                    digitsResPtr[i] = (uint)c;
                    c >>= 63;
                }

                return DigitHelper.GetRealDigitsLength(digitsResPtr, length1);
            }

            #endregion Subtract operation

            #region Divide/modulo operation - when second length == 1

            /// <summary>
            /// Divides one big integer represented by it's digits on another one big integer.
            /// Reminder is always filled (but not the result).
            /// </summary>
            /// <param name="digits1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="int2">Second integer.</param>
            /// <param name="divRes">Div result (can be null - not filled in this case).</param>
            /// <param name="modRes">Remainder (always filled).</param>
            /// <returns>Result length (0 if result is null).</returns>
            static unsafe public uint DivMod(uint[] digits1, uint length1, uint int2, uint[] divRes, out uint modRes)
            {
                fixed (uint* digits1Ptr = digits1, divResPtr = divRes)
                {
                    return DivMod(digits1Ptr, length1, int2, divResPtr, out modRes);
                }
            }

            /// <summary>
            /// Divides one big integer represented by it's digits on another one big integer.
            /// Reminder is always filled (but not the result).
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="int2">Second integer.</param>
            /// <param name="divResPtr">Div result (can be null - not filled in this case).</param>
            /// <param name="modRes">Remainder (always filled).</param>
            /// <returns>Result length (0 if result is null).</returns>
            static unsafe public uint DivMod(uint* digitsPtr1, uint length1, uint int2, uint* divResPtr, out uint modRes)
            {
                ulong c = 0;
                uint res;
                for (uint index = length1 - 1; index < length1; --index)
                {
                    c = (c << Constants.DigitBitCount) + digitsPtr1[index];
                    res = (uint)(c / int2);
                    c -= (ulong)res * int2;

                    divResPtr[index] = res;
                }
                modRes = (uint)c;

                return length1 - (divResPtr[length1 - 1] == 0 ? 1U : 0U);
            }


            /// <summary>
            /// Divides one big integer represented by it's digits on another one big integer.
            /// Only remainder is filled.
            /// </summary>
            /// <param name="digits1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="int2">Second integer.</param>
            /// <returns>Remainder.</returns>
            static unsafe public uint Mod(uint[] digits1, uint length1, uint int2)
            {
                fixed (uint* digitsPtr1 = digits1)
                {
                    return Mod(digitsPtr1, length1, int2);
                }
            }

            /// <summary>
            /// Divides one big integer represented by it's digits on another one big integer.
            /// Only remainder is filled.
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="int2">Second integer.</param>
            /// <returns>Remainder.</returns>
            static unsafe public uint Mod(uint* digitsPtr1, uint length1, uint int2)
            {
                ulong c = 0;
                uint res;
                for (uint* ptr1 = digitsPtr1 + length1 - 1; ptr1 >= digitsPtr1; --ptr1)
                {
                    c = (c << Constants.DigitBitCount) + *ptr1;
                    res = (uint)(c / int2);
                    c -= (ulong)res * int2;
                }

                return (uint)c;
            }

            #endregion Divide/modulo operation - when second length == 1

            #region Compare operation

            /// <summary>
            /// Compares 2 <see cref="System.IntX" /> objects represented by digits only (not taking sign into account).
            /// Returns "-1" if <paramref name="digits1" /> &lt; <paramref name="digits2" />, "0" if equal and "1" if &gt;.
            /// </summary>
            /// <param name="digits1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digits2">Second big integer digits.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <returns>Comparison result.</returns>
            static unsafe public int Cmp(uint[] digits1, uint length1, uint[] digits2, uint length2)
            {
                // Always compare length if one of the integers has zero length
                if (length1 == 0 || length2 == 0) return CmpLen(length1, length2);

                fixed (uint* digitsPtr1 = digits1, digitsPtr2 = digits2)
                {
                    return Cmp(digitsPtr1, length1, digitsPtr2, length2);
                }
            }

            /// <summary>
            /// Compares 2 <see cref="System.IntX" /> objects represented by pointers only (not taking sign into account).
            /// Returns "-1" if <paramref name="digitsPtr1" /> &lt; <paramref name="digitsPtr2" />, "0" if equal and "1" if &gt;.
            /// </summary>
            /// <param name="digitsPtr1">First big integer digits.</param>
            /// <param name="length1">First big integer length.</param>
            /// <param name="digitsPtr2">Second big integer digits.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <returns>Comparison result.</returns>
            static unsafe public int Cmp(uint* digitsPtr1, uint length1, uint* digitsPtr2, uint length2)
            {
                // Maybe length comparing will be enough
                int res = CmpLen(length1, length2);
                if (res != -2) return res;

                for (uint index = length1 - 1; index < length1; --index)
                {
                    if (digitsPtr1[index] != digitsPtr2[index]) return digitsPtr1[index] < digitsPtr2[index] ? -1 : 1;
                }
                return 0;
            }

            /// <summary>
            /// Compares two integers lengths. Returns -2 if further comparing is needed.
            /// </summary>
            /// <param name="length1">First big integer length.</param>
            /// <param name="length2">Second big integer length.</param>
            /// <returns>Comparison result.</returns>
            static int CmpLen(uint length1, uint length2)
            {
                if (length1 < length2) return -1;
                if (length1 > length2) return 1;
                return length1 == 0 ? 0 : -2;
            }

            #endregion Compare operation

            #region Shift operation

            /// <summary>
            /// Shifts big integer.
            /// </summary>
            /// <param name="digits">Big integer digits.</param>
            /// <param name="offset">Big integer digits offset.</param>
            /// <param name="length">Big integer length.</param>
            /// <param name="digitsRes">Resulting big integer digits.</param>
            /// <param name="resOffset">Resulting big integer digits offset.</param>
            /// <param name="rightShift">Shift to the right (always between 1 an 31).</param>
            static unsafe public void Shr(uint[] digits, uint offset, uint length, uint[] digitsRes, uint resOffset, int rightShift)
            {
                fixed (uint* digitsPtr = digits, digitsResPtr = digitsRes)
                {
                    Shr(digitsPtr + offset, length, digitsResPtr + resOffset, rightShift, resOffset != 0);
                }
            }

            /// <summary>
            /// Shifts big integer.
            /// </summary>
            /// <param name="digitsPtr">Big integer digits.</param>
            /// <param name="length">Big integer length.</param>
            /// <param name="digitsResPtr">Resulting big integer digits.</param>
            /// <param name="rightShift">Shift to the right (always between 1 an 31).</param>
            /// <param name="resHasOffset">True if <paramref name="digitsResPtr" /> has offset.</param>
            /// <returns>Resulting big integer length.</returns>
            unsafe static public uint Shr(uint* digitsPtr, uint length, uint* digitsResPtr, int rightShift, bool resHasOffset)
            {
                int rightShiftRev = Constants.DigitBitCount - rightShift;

                // Shift first digit in special way
                if (resHasOffset)
                {
                    digitsResPtr[-1] = digitsPtr[0] << rightShiftRev;
                }

                if (rightShift == 0)
                {
                    // Handle special situation here - only memcpy is needed (maybe)
                    if (digitsPtr != digitsResPtr)
                    {
                        DigitHelper.DigitsBlockCopy(digitsPtr, digitsResPtr, length);
                    }
                }
                else
                {
                    // Shift all digits except last one
                    uint* digitsPtrEndPrev = digitsPtr + length - 1;
                    uint* digitsPtrNext = digitsPtr + 1;
                    for (; digitsPtr < digitsPtrEndPrev; ++digitsPtr, ++digitsPtrNext, ++digitsResPtr)
                    {
                        *digitsResPtr = *digitsPtr >> rightShift | *digitsPtrNext << rightShiftRev;
                    }

                    // Shift last digit in special way
                    uint lastValue = *digitsPtr >> rightShift;
                    if (lastValue != 0)
                    {
                        *digitsResPtr = lastValue;
                    }
                    else
                    {
                        --length;
                    }
                }

                return length;
            }

            #endregion Shift operation
        }
        #endregion

        #region DigitHelper
        /// <summary>
        /// Contains big integer uint[] digits utility methods.
        /// </summary>
        static internal class DigitHelper
        {
            #region Working with digits length methods

            /// <summary>
            /// Returns real length of digits array (excluding leading zeroes).
            /// </summary>
            /// <param name="digits">Big integer digits.</param>
            /// <param name="length">Initial big integers length.</param>
            /// <returns>Real length.</returns>
            static public uint GetRealDigitsLength(uint[] digits, uint length)
            {
                for (; length > 0 && digits[length - 1] == 0; --length) ;
                return length;
            }

            /// <summary>
            /// Returns real length of digits array (excluding leading zeroes).
            /// </summary>
            /// <param name="digits">Big integer digits.</param>
            /// <param name="length">Initial big integers length.</param>
            /// <returns>Real length.</returns>
            static unsafe public uint GetRealDigitsLength(uint* digits, uint length)
            {
                for (; length > 0 && digits[length - 1] == 0; --length) ;
                return length;
            }

            /// <summary>
            /// Determines <see cref="IntX" /> object with lower length.
            /// </summary>
            /// <param name="int1">First big integer.</param>
            /// <param name="int2">Second big integer.</param>
            /// <param name="smallerInt">Resulting smaller big integer (by length only).</param>
            /// <param name="biggerInt">Resulting bigger big integer (by length only).</param>
            static public void GetMinMaxLengthObjects(IntX int1, IntX int2, out IntX smallerInt, out IntX biggerInt)
            {
                if (int1._length < int2._length)
                {
                    smallerInt = int1;
                    biggerInt = int2;
                }
                else
                {
                    smallerInt = int2;
                    biggerInt = int1;
                }
            }

            #endregion Working with digits length methods

            #region Signed to unsigned+sign conversion methods

            /// <summary>
            /// Converts int value to uint digit and value sign.
            /// </summary>
            /// <param name="value">Initial value.</param>
            /// <param name="resultValue">Resulting unsigned part.</param>
            /// <param name="negative">Resulting sign.</param>
            static public void ToUInt32WithSign(int value, out uint resultValue, out bool negative)
            {
                negative = value < 0;
                resultValue = !negative
                    ? (uint)value
                    : value != int.MinValue ? (uint)-value : int.MaxValue + 1U;
            }

            /// <summary>
            /// Converts long value to ulong digit and value sign.
            /// </summary>
            /// <param name="value">Initial value.</param>
            /// <param name="resultValue">Resulting unsigned part.</param>
            /// <param name="negative">Resulting sign.</param>
            static public void ToUInt64WithSign(long value, out ulong resultValue, out bool negative)
            {
                negative = value < 0;
                resultValue = !negative
                    ? (ulong)value
                    : value != long.MinValue ? (ulong)-value : long.MaxValue + 1UL;
            }

            #endregion Signed to unsigned+sign conversion methods

            #region Working with digits directly methods

            /// <summary>
            /// Sets digits in given block to given value.
            /// </summary>
            /// <param name="block">Block start pointer.</param>
            /// <param name="blockLength">Block length.</param>
            /// <param name="value">Value to set.</param>
            static unsafe public void SetBlockDigits(uint* block, uint blockLength, uint value)
            {
                for (uint* blockEnd = block + blockLength; block < blockEnd; *block++ = value) ;
            }

            /// <summary>
            /// Sets digits in given block to given value.
            /// </summary>
            /// <param name="block">Block start pointer.</param>
            /// <param name="blockLength">Block length.</param>
            /// <param name="value">Value to set.</param>
            unsafe static public void SetBlockDigits(double* block, uint blockLength, double value)
            {
                for (double* blockEnd = block + blockLength; block < blockEnd; *block++ = value) ;
            }

            /// <summary>
            /// Copies digits from one block to another.
            /// </summary>
            /// <param name="blockFrom">From block start pointer.</param>
            /// <param name="blockTo">To block start pointer.</param>
            /// <param name="count">Count of dwords to copy.</param>
            static unsafe public void DigitsBlockCopy(uint* blockFrom, uint* blockTo, uint count)
            {
                for (uint* blockFromEnd = blockFrom + count; blockFrom < blockFromEnd; *blockTo++ = *blockFrom++) ;
            }

            #endregion Working with digits directly methods
        }
        #endregion

        #endregion

        internal uint[] _digits; // big integer digits
		internal uint _length; // big integer digits length
		internal bool _negative; // big integer sign ("-" if true)

		#region Constructors

		/// <summary>
		/// Creates new big integer with zero value.
		/// </summary>
		public IntX() : this(0) {}

		/// <summary>
		/// Creates new big integer from integer value.
		/// </summary>
		/// <param name="value">Integer value to create big integer from.</param>
		public IntX(int value)
		{
			if (value == 0)
			{
				// Very specific fast processing for zero values
				InitFromZero();
			}
			else
			{
				// Prepare internal fields
				_digits = new uint[_length = 1];

				// Fill the only big integer digit
				DigitHelper.ToUInt32WithSign(value, out _digits[0], out _negative);
			}
		}

		/// <summary>
		/// Creates new big integer from unsigned integer value.
		/// </summary>
		/// <param name="value">Unsigned integer value to create big integer from.</param>
		public IntX(uint value)
		{
			if (value == 0)
			{
				// Very specific fast processing for zero values
				InitFromZero();
			}
			else
			{
				// Prepare internal fields
				_digits = new uint[] { value };
				_length = 1;
			}
		}

		/// <summary>
		/// Creates new big integer from long value.
		/// </summary>
		/// <param name="value">Long value to create big integer from.</param>
		public IntX(long value)
		{
			if (value == 0)
			{
				// Very specific fast processing for zero values
				InitFromZero();
			}
			else
			{
				// Fill the only big integer digit
				ulong newValue;
				DigitHelper.ToUInt64WithSign(value, out newValue, out _negative);
				InitFromUlong(newValue);
			}
		}

		/// <summary>
		/// Creates new big integer from unsigned long value.
		/// </summary>
		/// <param name="value">Unsigned long value to create big integer from.</param>
		public IntX(ulong value)
		{
			if (value == 0)
			{
				// Very specific fast processing for zero values
				InitFromZero();
			}
			else
			{
				InitFromUlong(value);
			}
		}

		/// <summary>
		/// Creates new big integer from array of it's "digits".
		/// Digit with lower index has less weight.
		/// </summary>
		/// <param name="digits">Array of <see cref="IntX" /> digits.</param>
		/// <param name="negative">True if this number is negative.</param>
		/// <exception cref="ArgumentNullException"><paramref name="digits" /> is a null reference.</exception>
		public IntX(uint[] digits, bool negative)
		{
			// Exceptions
			if (digits == null)
			{
				throw new ArgumentNullException("values");
			}

			InitFromDigits(digits, negative, DigitHelper.GetRealDigitsLength(digits, (uint)digits.LongLength));
		}


		/// <summary>
		/// Creates new <see cref="IntX" /> from string.
		/// </summary>
		/// <param name="value">Number as string.</param>
		public IntX(string value)
		{
			IntX intX = Parse(value);
			InitFromIntX(intX);
		}

		/// <summary>
		/// Creates new <see cref="IntX" /> from string.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		public IntX(string value, uint numberBase)
		{
			IntX intX = Parse(value, numberBase);
			InitFromIntX(intX);
		}


		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="value">Value to copy from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		public IntX(IntX value)
		{
			// Exceptions
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			InitFromIntX(value);
		}


		/// <summary>
		/// Creates new empty big integer with desired sign and length.
		/// 
		/// For internal use.
		/// </summary>
		/// <param name="length">Desired digits length.</param>
		/// <param name="negative">Desired integer sign.</param>
		internal IntX(uint length, bool negative)
		{
			_digits = new uint[_length = length];
			_negative = negative;
		}

		/// <summary>
		/// Creates new big integer from array of it's "digits" but with given length.
		/// Digit with lower index has less weight.
		/// 
		/// For internal use.
		/// </summary>
		/// <param name="digits">Array of <see cref="IntX" /> digits.</param>
		/// <param name="negative">True if this number is negative.</param>
		/// <param name="length">Length to use for internal digits array.</param>
		/// <exception cref="ArgumentNullException"><paramref name="digits" /> is a null reference.</exception>
		internal IntX(uint[] digits, bool negative, uint length)
		{
			// Exceptions
			if (digits == null)
			{
				throw new ArgumentNullException("values");
			}

			InitFromDigits(digits, negative, length);
		}

		#endregion Constructors
        
		#region Operators

		#region operator==

		/// <summary>
		/// Compares two <see cref="IntX" /> objects and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if equals.</returns>
		static public bool operator ==(IntX int1, IntX int2)
		{
			return OpHelper.Cmp(int1, int2, false) == 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with integer and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if equals.</returns>
		static public bool operator ==(IntX int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) == 0;
		}

		/// <summary>
		/// Compares integer with <see cref="IntX" /> object and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if equals.</returns>
		static public bool operator ==(int int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) == 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with unsinged integer and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsinged integer.</param>
		/// <returns>True if equals.</returns>
		static public bool operator ==(IntX int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) == 0;
		}

		/// <summary>
		/// Compares unsinged integer with <see cref="IntX" /> object and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First unsinged integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if equals.</returns>
		static public bool operator ==(uint int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) == 0;
		}

		#endregion operator==

		#region operator!=

		/// <summary>
		/// Compares two <see cref="IntX" /> objects and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if not equals.</returns>
		static public bool operator !=(IntX int1, IntX int2)
		{
			return OpHelper.Cmp(int1, int2, false) != 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with integer and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if not equals.</returns>
		static public bool operator !=(IntX int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) != 0;
		}

		/// <summary>
		/// Compares integer with <see cref="IntX" /> object and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if not equals.</returns>
		static public bool operator !=(int int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) != 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with unsigned integer and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsigned integer.</param>
		/// <returns>True if not equals.</returns>
		static public bool operator !=(IntX int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) != 0;
		}

		/// <summary>
		/// Compares unsigned integer with <see cref="IntX" /> object and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First unsigned integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if not equals.</returns>
		static public bool operator !=(uint int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) != 0;
		}

		#endregion operator!=

		#region operator>

		/// <summary>
		/// Compares two <see cref="IntX" /> objects and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater.</returns>
		static public bool operator >(IntX int1, IntX int2)
		{
			return OpHelper.Cmp(int1, int2, true) > 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with integer and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if first is greater.</returns>
		static public bool operator >(IntX int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) > 0;
		}

		/// <summary>
		/// Compares integer with <see cref="IntX" /> object and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater.</returns>
		static public bool operator >(int int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) < 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with unsigned integer and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsigned integer.</param>
		/// <returns>True if first is greater.</returns>
		static public bool operator >(IntX int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) > 0;
		}

		/// <summary>
		/// Compares unsigned integer with <see cref="IntX" /> object and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First unsigned integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater.</returns>
		static public bool operator >(uint int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) < 0;
		}

		#endregion operator>

		#region operator>=

		/// <summary>
		/// Compares two <see cref="IntX" /> objects and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		static public bool operator >=(IntX int1, IntX int2)
		{
			return OpHelper.Cmp(int1, int2, true) >= 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with integer and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		static public bool operator >=(IntX int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) >= 0;
		}

		/// <summary>
		/// Compares integer with <see cref="IntX" /> object and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		static public bool operator >=(int int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) <= 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with unsinged integer and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsinged integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		static public bool operator >=(IntX int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) >= 0;
		}

		/// <summary>
		/// Compares unsinged integer with <see cref="IntX" /> object and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First unsinged integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		static public bool operator >=(uint int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) <= 0;
		}

		#endregion operator>=

		#region operator<

		/// <summary>
		/// Compares two <see cref="IntX" /> objects and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter.</returns>
		static public bool operator <(IntX int1, IntX int2)
		{
			return OpHelper.Cmp(int1, int2, true) < 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with integer and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if first is lighter.</returns>
		static public bool operator <(IntX int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) < 0;
		}

		/// <summary>
		/// Compares integer with <see cref="IntX" /> object and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter.</returns>
		static public bool operator <(int int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) > 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with unsinged integer and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsinged integer.</param>
		/// <returns>True if first is lighter.</returns>
		static public bool operator <(IntX int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) < 0;
		}

		/// <summary>
		/// Compares unsinged integer with <see cref="IntX" /> object and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First unsinged integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter.</returns>
		static public bool operator <(uint int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) > 0;
		}

		#endregion operator<

		#region operator<=

		/// <summary>
		/// Compares two <see cref="IntX" /> objects and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		static public bool operator <=(IntX int1, IntX int2)
		{
			return OpHelper.Cmp(int1, int2, true) <= 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with integer and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		static public bool operator <=(IntX int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) <= 0;
		}

		/// <summary>
		/// Compares integer with <see cref="IntX" /> object and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		static public bool operator <=(int int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) >= 0;
		}

		/// <summary>
		/// Compares <see cref="IntX" /> object with unsinged integer and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsinged integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		static public bool operator <=(IntX int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) <= 0;
		}

		/// <summary>
		/// Compares unsinged integer with <see cref="IntX" /> object and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First unsinged integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		static public bool operator <=(uint int1, IntX int2)
		{
			return OpHelper.Cmp(int2, int1) >= 0;
		}

		#endregion operator<=

		#region operator+ and operator-

		/// <summary>
		/// Adds one <see cref="IntX" /> object to another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Addition result.</returns>
		static public IntX operator +(IntX int1, IntX int2)
		{
			return OpHelper.AddSub(int1, int2, false);
		}

		/// <summary>
		/// Subtracts one <see cref="IntX" /> object from another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Subtraction result.</returns>
		static public IntX operator -(IntX int1, IntX int2)
		{
			return OpHelper.AddSub(int1, int2, true);
		}

		#endregion operator+ and operator-

		#region operator*

		/// <summary>
		/// Multiplies one <see cref="IntX" /> object on another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Multiply result.</returns>
		static public IntX operator *(IntX int1, IntX int2)
        {
            return IntXMultiplier.Multiply(int1, int2);
		}

		#endregion operator*

		#region operator/ and operator%

		/// <summary>
		/// Divides one <see cref="IntX" /> object by another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Division result.</returns>
		static public IntX operator /(IntX int1, IntX int2)
		{
			IntX modRes;
			return IntXDivider.DivMod(int1, int2, out modRes, DivModResultFlags.Div);
		}

		/// <summary>
		/// Divides one <see cref="IntX" /> object by another and returns division modulo.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Modulo result.</returns>
		static public IntX operator %(IntX int1, IntX int2)
		{
			IntX modRes;
			IntXDivider.DivMod(int1, int2, out modRes, DivModResultFlags.Mod);
			return modRes;
		}

		#endregion operator/ and operator%

		#region operator<< and operator>>

        /// <summary>
        /// Shifts <see cref="IntX" /> object on selected bits count to the left.
        /// </summary>
        /// <param name="intX">Big integer.</param>
        /// <param name="shift">Bits count.</param>
        /// <returns>Shifting result.</returns>
        static public IntX operator <<(IntX intX, int shift)
        {
            return OpHelper.Sh(intX, shift, true);
        }

        /// <summary>
        /// Shifts <see cref="IntX" /> object on selected bits count to the right.
        /// </summary>
        /// <param name="intX">Big integer.</param>
        /// <param name="shift">Bits count.</param>
        /// <returns>Shifting result.</returns>
        static public IntX operator >>(IntX intX, int shift)
        {
            return OpHelper.Sh(intX, shift, false);
        }

		#endregion operator<< and operator>>

		#region +, -, ++, -- unary operators

		/// <summary>
		/// Returns the same <see cref="IntX" /> value.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <returns>The same value, but new object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public IntX operator +(IntX value)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			return new IntX(value);
		}

		/// <summary>
		/// Returns the same <see cref="IntX" /> value, but with other sign.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <returns>The same value, but with other sign.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public IntX operator -(IntX value)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			IntX newValue = new IntX(value);
			if (newValue._length != 0)
			{
				newValue._negative = !newValue._negative;
			}
			return newValue;
		}

		/// <summary>
		/// Returns increased <see cref="IntX" /> value.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <returns>Increased value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public IntX operator ++(IntX value)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			return value + 1U;
		}

		/// <summary>
		/// Returns decreased <see cref="IntX" /> value.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <returns>Decreased value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public IntX operator --(IntX value)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			return value - 1U;
		}

		#endregion +, -, ++, -- unary operators

		#region Conversion operators

		#region To IntX (Implicit)

		/// <summary>
		/// Implicitly converts <see cref="Int32" /> to <see cref="IntX" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public implicit operator IntX(int value)
		{
			return new IntX(value);
		}

		/// <summary>
		/// Implicitly converts <see cref="UInt32" /> to <see cref="IntX" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public implicit operator IntX(uint value)
		{
			return new IntX(value);
		}

		/// <summary>
		/// Implicitly converts <see cref="UInt16" /> to <see cref="IntX" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public implicit operator IntX(ushort value)
		{
			return new IntX(value);
		}

		/// <summary>
		/// Implicitly converts <see cref="Int64" /> to <see cref="IntX" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public implicit operator IntX(long value)
		{
			return new IntX(value);
		}

		/// <summary>
		/// Implicitly converts <see cref="UInt64" /> to <see cref="IntX" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public implicit operator IntX(ulong value)
		{
			return new IntX(value);
		}

		#endregion To IntX (Implicit)

		#region From IntX (Explicit)

		/// <summary>
		/// Explicitly converts <see cref="IntX" /> to <see cref="int" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public explicit operator int(IntX value)
		{
			int res = (int)(uint)value;
			return value._negative ? -res : res;
		}

		/// <summary>
		/// Explicitly converts <see cref="IntX" /> to <see cref="uint" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public explicit operator uint(IntX value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			if (value._length == 0) return 0;
			return value._digits[0];
		}

		/// <summary>
		/// Explicitly converts <see cref="IntX" /> to <see cref="long" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public explicit operator long(IntX value)
		{
			long res = (long)(ulong)value;
			return value._negative ? -res : res;
		}

		/// <summary>
		/// Explicitly converts <see cref="IntX" /> to <see cref="ulong" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public explicit operator ulong(IntX value)
		{
			ulong res = (uint)value;
			if (value._length > 1)
			{
				res |= (ulong)value._digits[1] << Constants.DigitBitCount;
			}
			return res;
		}

		/// <summary>
		/// Explicitly converts <see cref="IntX" /> to <see cref="ushort" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public explicit operator ushort(IntX value)
		{
			return (ushort)(uint)value;
		}

		#endregion From IntX (Explicit)

		#endregion Conversion operators

		#endregion Operators

		#region Math static methods

		#region Multiply

		/// <summary>
		/// Multiplies one <see cref="IntX" /> object on another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Multiply result.</returns>
		static public IntX Multiply(IntX int1, IntX int2)
		{
            return IntXMultiplier.Multiply(int1, int2);
		}

		#endregion Multiply

		#region Divide/modulo

		/// <summary>
		/// Divides one <see cref="IntX" /> object by another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Division result.</returns>
		static public IntX Divide(IntX int1, IntX int2)
		{
			IntX modRes;
            return IntXDivider.DivMod(int1, int2, out modRes, DivModResultFlags.Div);
		}

		/// <summary>
		/// Divides one <see cref="IntX" /> object by another and returns division modulo.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Modulo result.</returns>
		static public IntX Modulo(IntX int1, IntX int2)
		{
			IntX modRes;
            IntXDivider.DivMod(int1, int2, out modRes, DivModResultFlags.Mod);
			return modRes;
		}

		/// <summary>
		/// Divides one <see cref="IntX" /> object on another.
		/// Returns both divident and remainder
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="modRes">Remainder big integer.</param>
		/// <returns>Division result.</returns>
		static public IntX DivideModulo(IntX int1, IntX int2, out IntX modRes)
		{
            return IntXDivider.DivMod(int1, int2, out modRes, DivModResultFlags.Div | DivModResultFlags.Mod);
		}

		#endregion Divide/modulo

		#region Pow

		/// <summary>
		/// Returns a specified big integer raised to the specified power.
		/// </summary>
		/// <param name="value">Number to raise.</param>
		/// <param name="power">Power.</param>
		/// <returns>Number in given power.</returns>
		static public IntX Pow(IntX value, uint power)
		{
			return OpHelper.Pow(value, power);
		}

		#endregion Pow

		#endregion Math static methods

		#region ToString override

		/// <summary>
		/// Returns decimal string representation of this <see cref="IntX" /> object.
		/// </summary>
		/// <returns>Decimal number in string.</returns>
		override public string ToString()
		{
			return ToString(10U, true);
		}

		/// <summary>
		/// Returns string representation of this <see cref="IntX" /> object in given base.
		/// </summary>
		/// <param name="numberBase">Base of system in which to do output.</param>
		/// <returns>Object string representation.</returns>
		public string ToString(uint numberBase)
		{
			return ToString(numberBase, true);
		}

		/// <summary>
		/// Returns string representation of this <see cref="IntX" /> object in given base.
		/// </summary>
		/// <param name="numberBase">Base of system in which to do output.</param>
		/// <param name="upperCase">Use uppercase for bases from 11 to 16 (which use letters A-F).</param>
		/// <returns>Object string representation.</returns>
		public string ToString(uint numberBase, bool upperCase)
		{
			return IntXStringConverter.ToString(this, numberBase, upperCase ? Constants.BaseUpperChars : Constants.BaseLowerChars);
		}

		/// <summary>
		/// Returns string representation of this <see cref="IntX" /> object in given base using custom alphabet.
		/// </summary>
		/// <param name="numberBase">Base of system in which to do output.</param>
		/// <param name="alphabet">Alphabet which contains chars used to represent big integer, char position is coresponding digit value.</param>
		/// <returns>Object string representation.</returns>
		public string ToString(uint numberBase, string alphabet)
		{
			StrRepHelper.AssertAlphabet(alphabet, numberBase);
            return IntXStringConverter.ToString(this, numberBase, alphabet.ToCharArray());
		}

		#endregion ToString override

		#region Parsing methods

		/// <summary>
		/// Parses provided string representation of <see cref="IntX" /> object in decimal base.
		/// If number starts from "0" then it's treated as octal; if number starts fropm "0x"
		/// then it's treated as hexadecimal.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <returns>Parsed object.</returns>
		static public IntX Parse(string value)
		{
			return IntXParser.Parse(value, 10U, Constants.BaseCharToDigits, true);
		}

		/// <summary>
		/// Parses provided string representation of <see cref="IntX" /> object.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		/// <returns>Parsed object.</returns>
		static public IntX Parse(string value, uint numberBase)
		{
            return IntXParser.Parse(value, numberBase, Constants.BaseCharToDigits, false);
		}

		/// <summary>
		/// Parses provided string representation of <see cref="IntX" /> object using custom alphabet.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		/// <param name="alphabet">Alphabet which contains chars used to represent big integer, char position is coresponding digit value.</param>
		/// <returns>Parsed object.</returns>
		static public IntX Parse(string value, uint numberBase, string alphabet)
		{
            return IntXParser.Parse(value, numberBase, StrRepHelper.CharDictionaryFromAlphabet(alphabet, numberBase), false);
		}

		#endregion Parsing methods

		#region IEquatable/Equals/GetHashCode implementation/overrides

		/// <summary>
		/// Returns equality of this <see cref="IntX" /> with another big integer.
		/// </summary>
		/// <param name="n">Big integer to compare with.</param>
		/// <returns>True if equals.</returns>
		public bool Equals(IntX n)
		{
			return base.Equals(n) || this == n;
		}

		/// <summary>
		/// Returns equality of this <see cref="IntX" /> with another integer.
		/// </summary>
		/// <param name="n">Integer to compare with.</param>
		/// <returns>True if equals.</returns>
		public bool Equals(int n)
		{
			return this == n;
		}

		/// <summary>
		/// Returns equality of this <see cref="IntX" /> with another unsigned integer.
		/// </summary>
		/// <param name="n">Unsigned integer to compare with.</param>
		/// <returns>True if equals.</returns>
		public bool Equals(uint n)
		{
			return this == n;
		}

		/// <summary>
		/// Returns equality of this <see cref="IntX" /> with another long integer.
		/// </summary>
		/// <param name="n">Long integer to compare with.</param>
		/// <returns>True if equals.</returns>
		public bool Equals(long n)
		{
			return this == n;
		}

		/// <summary>
		/// Returns equality of this <see cref="IntX" /> with another unsigned long integer.
		/// </summary>
		/// <param name="n">Unsigned long integer to compare with.</param>
		/// <returns>True if equals.</returns>
		public bool Equals(ulong n)
		{
			return this == n;
		}


		/// <summary>
		/// Returns equality of this <see cref="IntX" /> with another object.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns>True if equals.</returns>
		override public bool Equals(object obj)
		{
			return obj is IntX && Equals((IntX)obj);
		}

		/// <summary>
		/// Returns hash code for this <see cref="IntX" /> object.
		/// </summary>
		/// <returns>Object hash code.</returns>
		override public int GetHashCode()
		{
			switch (_length)
			{
				case 0:
					return 0;
				case 1:
					return (int)(_digits[0] ^ _length ^ (_negative ? 1 : 0));
				default:
					return (int)(_digits[0] ^ _digits[_length - 1] ^ _length ^ (_negative ? 1 : 0));
			}
		}

		#endregion Equals/GetHashCode implementation/overrides

		#region IComparable implementation

		/// <summary>
		/// Compares current object with another big integer.
		/// </summary>
		/// <param name="n">Big integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		public int CompareTo(IntX n)
		{
			return OpHelper.Cmp(this, n, true);
		}

		/// <summary>
		/// Compares current object with another integer.
		/// </summary>
		/// <param name="n">Integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		public int CompareTo(int n)
		{
			return OpHelper.Cmp(this, n);
		}

		/// <summary>
		/// Compares current object with another unsigned integer.
		/// </summary>
		/// <param name="n">Unsigned integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		public int CompareTo(uint n)
		{
			return OpHelper.Cmp(this, n);
		}

		/// <summary>
		/// Compares current object with another long integer.
		/// </summary>
		/// <param name="n">Long integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		public int CompareTo(long n)
		{
			return OpHelper.Cmp(this, n, true);
		}

		/// <summary>
		/// Compares current object with another unsigned long integer.
		/// </summary>
		/// <param name="n">Unsigned long integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		public int CompareTo(ulong n)
		{
			return OpHelper.Cmp(this, n, true);
		}

		/// <summary>
		/// Compares current object with another object.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="obj" />, -1 if object is smaller than <paramref name="obj" />, 0 if they are equal.</returns>
		public int CompareTo(object obj)
		{
			if (obj is IntX)
			{
				return CompareTo((IntX)obj);
			}
			else if (obj is int)
			{
				return CompareTo((int)obj);
			}
			else if (obj is uint)
			{
				return CompareTo((uint)obj);
			}
			else if (obj is long)
			{
				return CompareTo((long)obj);
			}
			else if (obj is ulong)
			{
				return CompareTo((ulong)obj);
			}

			throw new ArgumentException("Can't compare with provided argument.", "obj");
		}

		#endregion IComparable implementation

		#region Other public methods

		/// <summary>
		/// Frees extra space not used by digits.
		/// </summary>
		public void Normalize()
		{
			if (_digits.LongLength > _length)
			{
				uint[] newDigits = new uint[_length];
				Array.Copy(_digits, newDigits, _length);
				_digits = newDigits;
			}

			if (_length == 0)
			{
				_negative = false;
			}
		}

		/// <summary>
		/// Retrieves this <see cref="IntX" /> internal state as digits array and sign.
		/// Can be used for serialization and other purposes.
		/// Note: please use constructor instead to clone <see cref="IntX" /> object.
		/// </summary>
		/// <param name="digits">Digits array.</param>
		/// <param name="negative">Is negative integer.</param>
		public void GetInternalState(out uint[] digits, out bool negative)
		{
			digits = new uint[_length];
			Array.Copy(_digits, digits, _length);

			negative = _negative;
		}

		#endregion Other public methods

		#region Init utilitary methods

		/// <summary>
		/// Initializes class instance from zero.
		/// For internal use.
		/// </summary>
		void InitFromZero()
		{
			_length = 0;
			_digits = new uint[0];
		}

		/// <summary>
		/// Initializes class instance from <see cref="UInt64" /> value.
		/// Doesn't initialize sign.
		/// For internal use.
		/// </summary>
		/// <param name="value">Unsigned long value.</param>
		void InitFromUlong(ulong value)
		{
			// Divide ulong into 2 uint values
			uint low = (uint)value;
			uint high = (uint)(value >> Constants.DigitBitCount);

			// Prepare internal fields
			if (high == 0)
			{
				_digits = new uint[] { low };
			}
			else
			{
				_digits = new uint[] { low, high };
			}
			_length = (uint)_digits.Length;
		}

		/// <summary>
		/// Initializes class instance from another <see cref="IntX" /> value.
		/// For internal use.
		/// </summary>
		/// <param name="value">Big integer value.</param>
		void InitFromIntX(IntX value)
		{
			_digits = value._digits;
			_length = value._length;
			_negative = value._negative;
		}

		/// <summary>
		/// Initializes class instance from digits array.
		/// For internal use.
		/// </summary>
		/// <param name="digits">Big integer digits.</param>
		/// <param name="negative">Big integer sign.</param>
		/// <param name="length">Big integer length.</param>
		void InitFromDigits(uint[] digits, bool negative, uint length)
		{
			_digits = new uint[_length = length];
			Array.Copy(digits, _digits, System.Math.Min((uint)digits.LongLength, length));
			if (length != 0)
			{
				_negative = negative;
			}
		}

		#endregion Init utilitary methods

		#region Other utilitary methods

		/// <summary>
		/// Frees extra space not used by digits.
		/// </summary>
		internal void TryNormalize()
		{
            Normalize();
		}

		#endregion Other utilitary methods

        #region IDisposable Implementation
        /// <summary>
        /// Disposes of the resources 
        /// </summary>
        public void Dispose()
        {
            this._digits = new uint[0];
            this._digits = null;
            System.GC.Collect();
        }
        #endregion

        #region IClonable Implementation
        /// <summary>
        /// Creates a clone of the current instance.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        object ICloneable.Clone()
        {
            return new IntX(this);
        }
        #endregion

        /// <summary>
        /// Converts this IntX to a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            System.IO.MemoryStream m = new IO.MemoryStream();
            System.IO.BinaryWriter b = new IO.BinaryWriter(m);
            for (int i = 0; i < _digits.Length; i++)
            {
                b.Write(_digits[i]);
            }
            b.Flush();
            return m.GetBuffer();
        }

        private static readonly IntX _zero = new IntX(0);
        /// <summary>
        /// Zero.
        /// </summary>
        public static IntX Zero
        {
            get
            {
                return _zero;
            }
        }
    }
}
