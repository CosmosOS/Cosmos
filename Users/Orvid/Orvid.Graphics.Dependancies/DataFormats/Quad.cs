/*
    Copyright (c) 2011 Jeff Pasternack.  All rights reserved.
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    /// <summary>
    /// Quad is a signed 128-bit floating point number, stored internally as a 64-bit significand (with the most significant bit as the sign bit) and
    /// a 64-bit signed exponent, with a value == significand * 2^exponent.  Quads have both a higher precision (64 vs. 53 effective significand bits)
    /// and a much higher range (64 vs. 11 exponent bits) than doubles.  Most operations are unchecked and undefined in the event of overflow.    
    /// </summary>
    /// <remarks>
    /// <para>
    /// Exponents >= long.MaxValue - 64 and exponents &lt;= long.MinValue + 64 are reserved
    /// and constitute overflow.  0 is defined by significand bits == 0 and an exponent of long.MinValue.
    /// </para>
    /// <para>
    /// Quad multiplication and division operators are slightly imprecise for the sake of efficiency; specifically,
    /// they may assign the wrong least significant bit, such that the precision is effectively only 63 bits rather than 64.
    /// </para>
    /// <para>
    ///  Exponents >= long.MaxValue - 64 and exponents &lt;= long.MinValue + 64 are reserved, primarily because this allows slightly faster arithmetic operations
    ///  by removing the need for overflow checking in certain places.  Quads with these exponents will have undefined behavior.
    /// </para>
    /// <para>
    /// For speed, consider using instance methods (like Multiply and Add) rather
    /// than the operators (like * and +) when possible, as the former are significantly faster (by as much as 50%).
    /// </para>    
    /// </remarks>
    [System.Diagnostics.DebuggerDisplay("{ToString(),nq}")] //this attributes makes the debugger display the value without braces or quotes
    public struct Quad
    {
        #region Public constants
        public static readonly Quad Zero = new Quad(0UL, long.MinValue); //there is only one zero; all other significands with exponent long.MinValue are invalid.
        public static readonly Quad One = (Quad)1UL; //used for increment/decrement operators
        #endregion

        #region Public fields
        /// <summary>
        /// The first (most significant) bit of the significand is the sign bit; 0 for positive values, 1 for negative.
        /// The remainder of the bits represent the fractional part (after the binary point) of the significant; there is always an implicit "1"
        /// preceding the binary point, just as in IEEE's double specification, except for 0 (defined by an exponent of long.MinValue and significand bits == 0).
        /// </summary>
        public ulong SignificandBits;

        /// <summary>
        /// The value of the Quad == 1.[last 63 bits of significand] * 2^exponent, except where the exponent is long.MinValue,
        /// which (in conjunction with SignificandBits == 0) denotes 0.  Exponents >= long.MaxValue - 64 and exponents &lt;= long.MinValue + 64 are reserved.
        /// 
        /// </summary>
        public long Exponent;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Quad with the given significand bits and exponent.  The significand has a first (most significant) bit
        /// corresponding to the quad's sign (1 for positive, 0 for negative), and the rest of the bits correspond to the fractional
        /// part of the significand value (immediately after the binary point).  A "1" before the binary point is always implied.
        /// </summary>
        /// <param name="significand"></param>
        /// <param name="exponent"></param>
        public Quad(ulong significandBits, long exponent)
        {
            this.SignificandBits = significandBits;
            this.Exponent = exponent;
        }

        /// <summary>
        /// Creates a new Quad with the given significand value and exponent.
        /// </summary>
        /// <param name="significand"></param>
        /// <param name="exponent"></param>
        public Quad(long significand, long exponent)
        {
            if (significand == 0) //handle 0
            {
                SignificandBits = 0;
                Exponent = long.MinValue;
                return;
            }

            if (significand < 0)
            {
                if (significand == long.MinValue) //corner case
                {
                    SignificandBits = highestBit;
                    Exponent = 0;
                    return;
                }                

                significand = -significand;
                SignificandBits = highestBit;
            }
            else
                SignificandBits = 0;
            
            int shift = nlz((ulong)significand); //we must normalize the value such that the most significant bit is 1
            this.SignificandBits |= ~highestBit & (((ulong)significand) << shift); //mask out the highest bit--it's implicit
            this.Exponent = exponent - shift;            
        }
        #endregion

        #region Helper functions and constants
        private const double base2to10Multiplier = 0.30102999566398119521373889472449; //Math.Log(2) / Math.Log(10);
        private const ulong highestBit = 1UL << 63;
        private const ulong secondHighestBit = 1UL << 62;
        private const ulong lowWordMask = 0xffffffff; //lower 32 bits
        private const ulong highWordMask = 0xffffffff00000000; //upper 32 bits

        private const ulong b = 4294967296; // Number base (32 bits).

        private static readonly Quad e19 = (Quad)10000000000000000000UL;
        private static readonly Quad e10 = (Quad)10000000000UL;
        private static readonly Quad e5 = (Quad)100000UL;
        private static readonly Quad e3 = (Quad)1000UL;
        private static readonly Quad e1 = (Quad)10UL;

        private static readonly Quad en19 = One / e19;
        private static readonly Quad en10 = One / e10;
        private static readonly Quad en5 = One / e5;
        private static readonly Quad en3 = One / e3;
        private static readonly Quad en1 = One / e1;

        private static readonly Quad en18 = One /(Quad)1000000000000000000UL;
        private static readonly Quad en9 = One / (Quad)1000000000UL;
        private static readonly Quad en4 = One / (Quad)10000UL;
        private static readonly Quad en2 = One / (Quad)100UL;
        
                
        /// <summary>
        /// Returns the position of the highest set bit, counting from the most significant bit position (position 0).
        /// Returns 64 if no bit is set.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static int nlz(ulong x)
        {
            //Future work: might be faster with a huge, explicit nested if tree, or use of an 256-element per-byte array.            

            int n;

            if (x == 0) return (64);
            n = 0;
            if (x <= 0x00000000FFFFFFFF) { n = n + 32; x = x << 32; }
            if (x <= 0x0000FFFFFFFFFFFF) { n = n + 16; x = x << 16; }
            if (x <= 0x00FFFFFFFFFFFFFF) { n = n + 8; x = x << 8; }
            if (x <= 0x0FFFFFFFFFFFFFFF) { n = n + 4; x = x << 4; }
            if (x <= 0x3FFFFFFFFFFFFFFF) { n = n + 2; x = x << 2; }
            if (x <= 0x7FFFFFFFFFFFFFFF) { n = n + 1; }
            return n;
        }

        #endregion

        #region Struct-modifying instance arithmetic functions

        public unsafe void Multiply(double multiplier)
        {
            #region Parse the double
            ulong bits = *((ulong*)&multiplier);                        
            long multiplierExponent = (((long)bits >> 52) & 0x7ffL);

            if (multiplierExponent == 0x7ffL)
                throw new ArgumentOutOfRangeException("Cannot cast NaN or infinity to Quad");

            ulong multiplierSignificand = (bits) & 0xfffffffffffffUL;
            
            if (multiplierExponent == 0)
            {
                if (multiplierSignificand == 0)
                {
                    //multiplication by 0
                    this.SignificandBits = 0;
                    this.Exponent = long.MinValue;
                    return;
                }

                int firstSetPosition = nlz(multiplierSignificand);
                multiplierSignificand = (highestBit & bits) | (multiplierSignificand << firstSetPosition);
                multiplierExponent -= firstSetPosition - 1 + 1075;
            }            
            else
            {
                multiplierSignificand = (highestBit & bits) | (multiplierSignificand << 11);
                multiplierExponent -= 11 + 1075;
            }
            #endregion

            #region Multiply

            ulong high1 = (this.SignificandBits | highestBit) >> 32; //de-implicitize the 1
            ulong high2 = (multiplierSignificand | highestBit) >> 32;

            //because the MSB of both significands is 1, the MSB of the result will also be 1, and the product of low bits on both significands is dropped (and thus we can skip its calculation)
            ulong significandBits = high1 * high2 + (((this.SignificandBits & lowWordMask) * high2) >> 32) + ((high1 * (multiplierSignificand & lowWordMask)) >> 32);

            if (significandBits < (1UL << 63))
            {
                if (this.Exponent == long.MinValue)
                    return; //we're already 0

                this.SignificandBits = ((this.SignificandBits ^ multiplierSignificand) & highestBit) | ((significandBits << 1) & ~highestBit);
                this.Exponent = this.Exponent + multiplierExponent - 1 + 64;
            }
            else
            {
                this.SignificandBits = ((this.SignificandBits ^ multiplierSignificand) & highestBit) | (significandBits & ~highestBit);
                this.Exponent = this.Exponent + multiplierExponent + 64;
            }
            #endregion
        }

        public void Multiply(Quad multiplier)
        {
            
            ulong high1 = (this.SignificandBits | highestBit) >> 32; //de-implicitize the 1
            ulong high2 = (multiplier.SignificandBits | highestBit) >> 32;

            //because the MSB of both significands is 1, the MSB of the result will also be 1, and the product of low bits on both significands is dropped (and thus we can skip its calculation)
            ulong significandBits = high1 * high2 + (((this.SignificandBits & lowWordMask) * high2) >> 32) + ((high1 * (multiplier.SignificandBits & lowWordMask)) >> 32);

            if (significandBits < (1UL << 63))
            {                
                //Checking for zeros here is significantly faster (~15%) on my machine than at the beginning,
                //because this branch is rarely taken.  This is acceptable because a zero will have a significand of 0,
                //and thus (when the significant bit is erroneously OR'd to it) multiplying by that zero cannot yield a value
                //of significandBits greater than or equal to 1 << 63.
                if (this.Exponent == long.MinValue || multiplier.Exponent == long.MinValue)
                {
                    this.Exponent = long.MinValue;
                    this.SignificandBits = 0;
                }
                else
                {
                    this.SignificandBits = ((this.SignificandBits ^ multiplier.SignificandBits) & highestBit) | ((significandBits << 1) & ~highestBit);
                    this.Exponent = this.Exponent + multiplier.Exponent - 1 + 64;
                }

            }
            else
            {
                this.SignificandBits = ((this.SignificandBits ^ multiplier.SignificandBits) & highestBit) | (significandBits & ~highestBit);
                this.Exponent = this.Exponent + multiplier.Exponent + 64;
            }

            #region Multiply with reduced branching (slightly faster?)
            //zeros
            ////if (this.Exponent == long.MinValue)// || multiplier.Exponent == long.MinValue)
            ////{
            ////    this.Exponent = long.MinValue;
            ////    this.Significand = 0;
            ////    return;
            ////}  

            //ulong high1 = (this.Significand | highestBit ) >> 32; //de-implicitize the 1
            //ulong high2 = (multiplier.Significand | highestBit) >> 32;

            ////because the MSB of both significands is 1, the MSB of the result will also be 1, and the product of low bits on both significands is dropped (and thus we can skip its calculation)
            //ulong significandBits = high1 * high2 + (((this.Significand & lowWordMask) * high2) >> 32) + ((high1 * (multiplier.Significand & lowWordMask)) >> 32);

            //if (significandBits < (1UL << 63)) //first bit clear?
            //{
            //    long zeroMask = ((this.Exponent ^ -this.Exponent) & (multiplier.Exponent ^ -multiplier.Exponent)) >> 63;                    
            //    this.Significand = (ulong)zeroMask & ((this.Significand ^ multiplier.Significand) & highestBit) | ((significandBits << 1) & ~highestBit);
            //    this.Exponent = (zeroMask & (this.Exponent + multiplier.Exponent - 1 + 64)) | (~zeroMask & long.MinValue);
            //}
            //else
            //{
            //    this.Significand = ((this.Significand ^ multiplier.Significand) & highestBit) | (significandBits & ~highestBit);
            //    this.Exponent = this.Exponent + multiplier.Exponent + 64;
            //}

            ////long zeroMask = ((isZeroBit1 >> 63) & (isZeroBit2 >> 63));
            ////this.Significand = (ulong)zeroMask & ((this.Significand ^ multiplier.Significand) & highestBit) | ((significandBits << (int)(1 ^ (significandBits >> 63))) & ~highestBit);
            ////this.Exponent = (zeroMask & (this.Exponent + multiplier.Exponent - 1 + 64 + (long)(significandBits >> 63))) | (~zeroMask & long.MinValue);

            #endregion
        }

        public unsafe void Add(double value)
        {
            #region Parse the double
            ulong doubleBits = *((ulong*)&value);
            long valueExponent = (((long)doubleBits >> 52) & 0x7ffL);

            if (valueExponent == 0x7ffL)
                throw new ArgumentOutOfRangeException("Cannot cast NaN or infinity to Quad");

            ulong valueSignificand = (doubleBits) & 0xfffffffffffffUL;

            if (valueExponent == 0)
            {
                if (valueSignificand == 0)
                {
                    //addition with 0                    
                    return;
                }

                int firstSetPosition = nlz(valueSignificand);
                valueSignificand = (highestBit & doubleBits) | (valueSignificand << firstSetPosition);
                valueExponent -= firstSetPosition - 1 + 1075;
            }
            else
            {
                valueSignificand = (highestBit & doubleBits) | (valueSignificand << 11);
                valueExponent -= 11 + 1075;
            }
            #endregion

            #region Addition

            if ((this.SignificandBits ^ valueSignificand) >= highestBit) //this and value have different signs--use subtraction instead
            {
                Subtract(new Quad(valueSignificand ^ highestBit, valueExponent));
                return;
            }

            //note on zeros: adding 0 results in a nop because the exponent is 64 less than any valid exponent.

            if (this.Exponent > valueExponent)
            {
                if (this.Exponent >= valueExponent + 64)
                    return; //value too small to make a difference
                else
                {
                    ulong bits = (this.SignificandBits | highestBit) + ((valueSignificand | highestBit) >> (int)(this.Exponent - valueExponent));

                    if (bits < highestBit) //this can only happen in an overflow  
                    {
                        this.SignificandBits = (this.SignificandBits & highestBit) | (bits >> 1);
                        this.Exponent = this.Exponent + 1;
                    }
                    else
                    {
                        this.SignificandBits = (this.SignificandBits & highestBit) | (bits & ~highestBit);
                        //this.Exponent = this.Exponent; //exponent stays the same
                    }
                }
            }
            else if (this.Exponent < valueExponent)
            {
                if (valueExponent >= this.Exponent + 64)
                {
                    this.SignificandBits = valueSignificand; //too small to matter
                    this.Exponent = valueExponent;
                }
                else
                {
                    ulong bits = (valueSignificand | highestBit) + ((this.SignificandBits | highestBit) >> (int)(valueExponent - this.Exponent));

                    if (bits < highestBit) //this can only happen in an overflow                    
                    {
                        this.SignificandBits = (valueSignificand & highestBit) | (bits >> 1);
                        this.Exponent = valueExponent + 1;
                    }
                    else
                    {
                        this.SignificandBits = (valueSignificand & highestBit) | (bits & ~highestBit);
                        this.Exponent = valueExponent;
                    }
                }
            }
            else //expDiff == 0
            {
                if (this.Exponent == long.MinValue) //verify that we're not adding two 0's
                    return; //we are already 0, so just return

                //the MSB must have the same sign, so the MSB will become 0, and logical overflow is guaranteed in this situation (so we can shift right and increment the exponent).
                this.SignificandBits = ((this.SignificandBits + valueSignificand) >> 1) | (this.SignificandBits & highestBit);
                this.Exponent = this.Exponent + 1;
            }

            #endregion
        }

        public void Add(Quad value)
        {
            #region Addition

            if ((this.SignificandBits ^ value.SignificandBits) >= highestBit) //this and value have different signs--use subtraction instead
            {
                Subtract(new Quad(value.SignificandBits ^ highestBit, value.Exponent));
                return;
            }

            //note on zeros: adding 0 results in a nop because the exponent is 64 less than any valid exponent.

            if (this.Exponent > value.Exponent)
            {
                if (this.Exponent >= value.Exponent + 64)
                    return; //value too small to make a difference
                else
                {
                    ulong bits = (this.SignificandBits | highestBit) + ((value.SignificandBits | highestBit) >> (int)(this.Exponent - value.Exponent));

                    if (bits < highestBit) //this can only happen in an overflow  
                    {
                        this.SignificandBits = (this.SignificandBits & highestBit) | (bits >> 1);
                        this.Exponent = this.Exponent + 1;
                    }
                    else
                    {
                        this.SignificandBits = (this.SignificandBits & highestBit) | (bits & ~highestBit);
                        //this.Exponent = this.Exponent; //exponent stays the same
                    }
                }
            }
            else if (this.Exponent < value.Exponent)
            {
                if (value.Exponent >= this.Exponent + 64)
                {
                    this.SignificandBits = value.SignificandBits; //too small to matter
                    this.Exponent = value.Exponent;
                }
                else
                {
                    ulong bits = (value.SignificandBits | highestBit) + ((this.SignificandBits | highestBit) >> (int)(value.Exponent - this.Exponent));

                    if (bits < highestBit) //this can only happen in an overflow                    
                    {
                        this.SignificandBits = (value.SignificandBits & highestBit) | (bits >> 1);
                        this.Exponent = value.Exponent + 1;
                    }
                    else
                    {
                        this.SignificandBits = (value.SignificandBits & highestBit) | (bits & ~highestBit);
                        this.Exponent = value.Exponent;
                    }
                }
            }
            else //expDiff == 0
            {
                if (this.Exponent == long.MinValue) //verify that we're not adding two 0's
                    return; //we are already 0, so just return
                
                //the MSB must have the same sign, so the MSB will become 0, and logical overflow is guaranteed in this situation (so we can shift right and increment the exponent).
                this.SignificandBits = ((this.SignificandBits + value.SignificandBits) >> 1) | (this.SignificandBits & highestBit);
                this.Exponent = this.Exponent + 1;
            }

            #endregion
        }

        public unsafe void Subtract(double value)
        {
            #region Parse the double
            ulong doubleBits = *((ulong*)&value);
            long valueExponent = (((long)doubleBits >> 52) & 0x7ffL);

            if (valueExponent == 0x7ffL)
                throw new ArgumentOutOfRangeException("Cannot cast NaN or infinity to Quad");

            ulong valueSignificand = (doubleBits) & 0xfffffffffffffUL;

            if (valueExponent == 0)
            {
                if (valueSignificand == 0)
                {
                    //subtraction by 0                    
                    return;
                }

                int firstSetPosition = nlz(valueSignificand);
                valueSignificand = (highestBit & doubleBits) | (valueSignificand << firstSetPosition);
                valueExponent -= firstSetPosition - 1 + 1075;
            }
            else
            {
                valueSignificand = (highestBit & doubleBits) | (valueSignificand << 11);
                valueExponent -= 11 + 1075;
            }
            #endregion

            #region Subtraction
            if ((this.SignificandBits ^ valueSignificand) >= highestBit) //this and value have different signs--use addition instead            
            {
                this.Add(new Quad(valueSignificand ^ highestBit, valueExponent));
                return;
            }

            //as in addition, we handle 0's implicitly--they will have an exponent at least 64 less than any valid non-zero value.

            if (this.Exponent > valueExponent)
            {
                if (this.Exponent >= valueExponent + 64)
                    return; //value too small to make a difference
                else
                {
                    ulong bits = (this.SignificandBits | highestBit) - ((valueSignificand | highestBit) >> (int)(this.Exponent - valueExponent));

                    //make sure MSB is 1                       
                    int highestBitPos = nlz(bits);
                    this.SignificandBits = ((bits << highestBitPos) & ~highestBit) | (this.SignificandBits & highestBit);
                    this.Exponent = this.Exponent - highestBitPos;
                }
            }
            else if (this.Exponent < valueExponent) //must subtract our significand from value, and switch the sign
            {
                if (valueExponent >= this.Exponent + 64)
                {
                    this.SignificandBits = valueSignificand ^ highestBit;
                    this.Exponent = valueExponent;
                    return;
                }

                ulong bits = (valueSignificand | highestBit) - ((this.SignificandBits | highestBit) >> (int)(valueExponent - this.Exponent));

                //make sure MSB is 1                       
                int highestBitPos = nlz(bits);
                this.SignificandBits = ((bits << highestBitPos) & ~highestBit) | (~valueSignificand & highestBit);
                this.Exponent = valueExponent - highestBitPos;
            }
            else // (this.Exponent == valueExponent)
            {
                if (valueSignificand > this.SignificandBits) //must switch sign
                {
                    ulong bits = valueSignificand - this.SignificandBits; //notice that we don't worry about de-implicitizing the MSB--it'd be eliminated by subtraction anyway
                    int highestBitPos = nlz(bits);
                    this.SignificandBits = ((bits << highestBitPos) & ~highestBit) | (~valueSignificand & highestBit);
                    this.Exponent = valueExponent - highestBitPos;
                }
                else if (valueSignificand < this.SignificandBits) //sign remains the same
                {
                    ulong bits = this.SignificandBits - valueSignificand; //notice that we don't worry about de-implicitizing the MSB--it'd be eliminated by subtraction anyway
                    int highestBitPos = nlz(bits);
                    this.SignificandBits = ((bits << highestBitPos) & ~highestBit) | (this.SignificandBits & highestBit);
                    this.Exponent = this.Exponent - highestBitPos;
                }
                else //this == value
                {
                    //result is 0
                    this.SignificandBits = 0;
                    this.Exponent = long.MinValue;
                }
            }
            #endregion
        }

        public void Subtract(Quad value)
        {
            #region Subtraction
            if ((this.SignificandBits ^ value.SignificandBits) >= highestBit) //this and value have different signs--use addition instead            
            {
                this.Add(new Quad(value.SignificandBits ^ highestBit, value.Exponent));
                return;
            }

            //as in addition, we handle 0's implicitly--they will have an exponent at least 64 less than any valid non-zero value.

            if (this.Exponent > value.Exponent)
            {
                if (this.Exponent >= value.Exponent + 64)
                    return; //value too small to make a difference
                else
                {
                    ulong bits = (this.SignificandBits | highestBit) - ((value.SignificandBits | highestBit) >> (int)(this.Exponent - value.Exponent));

                    //make sure MSB is 1                       
                    int highestBitPos = nlz(bits);
                    this.SignificandBits = ((bits << highestBitPos) & ~highestBit) | (this.SignificandBits & highestBit);
                    this.Exponent = this.Exponent - highestBitPos;
                }
            }
            else if (this.Exponent < value.Exponent) //must subtract our significand from value, and switch the sign
            {
                if (value.Exponent >= this.Exponent + 64)
                {
                    this.SignificandBits = value.SignificandBits ^ highestBit;
                    this.Exponent = value.Exponent;
                    return;
                }

                ulong bits = (value.SignificandBits | highestBit) - ((this.SignificandBits | highestBit) >> (int)(value.Exponent - this.Exponent));

                //make sure MSB is 1                       
                int highestBitPos = nlz(bits);
                this.SignificandBits = ((bits << highestBitPos) & ~highestBit) | (~value.SignificandBits & highestBit);
                this.Exponent = value.Exponent - highestBitPos;
            }
            else // (this.Exponent == value.Exponent)
            {                
                if (value.SignificandBits > this.SignificandBits) //must switch sign
                {
                    ulong bits = value.SignificandBits - this.SignificandBits; //notice that we don't worry about de-implicitizing the MSB--it'd be eliminated by subtraction anyway
                    int highestBitPos = nlz(bits);
                    this.SignificandBits = ((bits << highestBitPos) & ~highestBit) | (~value.SignificandBits & highestBit);
                    this.Exponent = value.Exponent - highestBitPos;
                }
                else if (value.SignificandBits < this.SignificandBits) //sign remains the same
                {
                    ulong bits = this.SignificandBits - value.SignificandBits; //notice that we don't worry about de-implicitizing the MSB--it'd be eliminated by subtraction anyway
                    int highestBitPos = nlz(bits);
                    this.SignificandBits = ((bits << highestBitPos) & ~highestBit) | (this.SignificandBits & highestBit);
                    this.Exponent = this.Exponent - highestBitPos;
                }
                else //this == value
                {
                    //result is 0
                    this.SignificandBits = 0;
                    this.Exponent = long.MinValue;
                }
            }
            #endregion
        }

        public unsafe void Divide(double divisor)
        {
            #region Parse the double
            ulong doubleBits = *((ulong*)&divisor);
            long valueExponent = (((long)doubleBits >> 52) & 0x7ffL);

            if (valueExponent == 0x7ffL)
                throw new ArgumentOutOfRangeException("Cannot cast NaN or infinity to Quad");

            ulong valueSignificand = (doubleBits) & 0xfffffffffffffUL;

            if (valueExponent == 0)
            {
                if (valueSignificand == 0)
                {
                    //division by 0
                    throw new DivideByZeroException();                    
                }

                int firstSetPosition = nlz(valueSignificand);
                valueSignificand = (highestBit & doubleBits) | (valueSignificand << firstSetPosition);
                valueExponent -= firstSetPosition - 1 + 1075;
            }
            else
            {
                valueSignificand = (highestBit & doubleBits) | (valueSignificand << 11);
                valueExponent -= 11 + 1075;
            }
            #endregion

            #region Division
            if (this.Exponent == long.MinValue) //0 / non-zero == 0
                return; //we're already 0

            ulong un1 = 0,     // Norm. dividend LSD's.
                     vn1, vn0,        // Norm. divisor digits.
                     q1, q0,          // Quotient digits.
                     un21,// Dividend digit pairs.
                     rhat;            // A remainder.            

            //result.Significand = highestBit & (this.Significand ^ valueSignificand); //determine the sign bit

            //this.Significand |= highestBit; //de-implicitize the 1 before the binary point
            //valueSignificand |= highestBit;

            long adjExponent = 0;
            ulong thisAdjSignificand = this.SignificandBits | highestBit;
            ulong divisorAdjSignificand = valueSignificand | highestBit;

            if (thisAdjSignificand >= divisorAdjSignificand)
            {
                //need to make this's significand smaller than divisor's
                adjExponent = 1;
                un1 = (this.SignificandBits & 1) << 31;
                thisAdjSignificand = thisAdjSignificand >> 1;
            }

            vn1 = divisorAdjSignificand >> 32;            // Break divisor up into
            vn0 = valueSignificand & 0xFFFFFFFF;         // two 32-bit digits.            

            q1 = thisAdjSignificand / vn1;            // Compute the first
            rhat = thisAdjSignificand - q1 * vn1;     // quotient digit, q1.
        again1:
            if (q1 >= b || q1 * vn0 > b * rhat + un1)
            {
                q1 = q1 - 1;
                rhat = rhat + vn1;
                if (rhat < b) goto again1;
            }

            un21 = thisAdjSignificand * b + un1 - q1 * divisorAdjSignificand;  // Multiply and subtract.

            q0 = un21 / vn1;            // Compute the second
            rhat = un21 - q0 * vn1;     // quotient digit, q0.
        again2:
            if (q0 >= b || q0 * vn0 > b * rhat)
            {
                q0 = q0 - 1;
                rhat = rhat + vn1;
                if (rhat < b) goto again2;
            }

            thisAdjSignificand = q1 * b + q0; //convenient place to store intermediate result

            //if (this.Significand == 0) //the final significand should never be 0
            //    result.Exponent = 0;
            //else

            if (thisAdjSignificand < (1UL << 63))
            {
                this.SignificandBits = (~highestBit & (thisAdjSignificand << 1)) | ((this.SignificandBits ^ valueSignificand) & highestBit);
                this.Exponent = this.Exponent - (valueExponent + 64) - 1 + adjExponent;
            }
            else
            {
                this.SignificandBits = (~highestBit & thisAdjSignificand) | ((this.SignificandBits ^ valueSignificand) & highestBit);
                this.Exponent = this.Exponent - (valueExponent + 64) + adjExponent;
            }
            #endregion            
        }

        public void Divide(Quad divisor)
        {
            #region Division
            if (divisor.Exponent == long.MinValue) // something / 0
                throw new DivideByZeroException();
            else if (this.Exponent == long.MinValue) //0 / non-zero == 0
                return; //we're already 0

            ulong un1 = 0,     // Norm. dividend LSD's.
                     vn1, vn0,        // Norm. divisor digits.
                     q1, q0,          // Quotient digits.
                     un21,// Dividend digit pairs.
                     rhat;            // A remainder.            

            //result.Significand = highestBit & (this.Significand ^ divisor.Significand); //determine the sign bit

            //this.Significand |= highestBit; //de-implicitize the 1 before the binary point
            //divisor.Significand |= highestBit;

            long adjExponent = 0;
            ulong thisAdjSignificand = this.SignificandBits | highestBit;
            ulong divisorAdjSignificand = divisor.SignificandBits | highestBit;

            if (thisAdjSignificand >= divisorAdjSignificand)
            {
                //need to make this's significand smaller than divisor's
                adjExponent = 1;
                un1 = (this.SignificandBits & 1) << 31;
                thisAdjSignificand = thisAdjSignificand >> 1;
            }

            vn1 = divisorAdjSignificand >> 32;            // Break divisor up into
            vn0 = divisor.SignificandBits & 0xFFFFFFFF;         // two 32-bit digits.            

            q1 = thisAdjSignificand / vn1;            // Compute the first
            rhat = thisAdjSignificand - q1 * vn1;     // quotient digit, q1.
        again1:
            if (q1 >= b || q1 * vn0 > b * rhat + un1)
            {
                q1 = q1 - 1;
                rhat = rhat + vn1;
                if (rhat < b) goto again1;
            }

            un21 = thisAdjSignificand * b + un1 - q1 * divisorAdjSignificand;  // Multiply and subtract.

            q0 = un21 / vn1;            // Compute the second
            rhat = un21 - q0 * vn1;     // quotient digit, q0.
        again2:
            if (q0 >= b || q0 * vn0 > b * rhat)
            {
                q0 = q0 - 1;
                rhat = rhat + vn1;
                if (rhat < b) goto again2;
            }

            thisAdjSignificand = q1 * b + q0; //convenient place to store intermediate result

            //if (this.Significand == 0) //the final significand should never be 0
            //    result.Exponent = 0;
            //else

            if (thisAdjSignificand < (1UL << 63))
            {
                this.SignificandBits = (~highestBit & (thisAdjSignificand << 1)) | ((this.SignificandBits ^ divisor.SignificandBits) & highestBit);
                this.Exponent = this.Exponent - (divisor.Exponent + 64) - 1 + adjExponent;
            }
            else
            {
                this.SignificandBits = (~highestBit & thisAdjSignificand) | ((this.SignificandBits ^ divisor.SignificandBits) & highestBit);
                this.Exponent = this.Exponent - (divisor.Exponent + 64) + adjExponent;
            }
            #endregion            
        }

        #endregion

        #region Operators
        /// <summary>
        /// Efficiently multiplies the Quad by 2^shift.
        /// </summary>
        /// <param name="qd"></param>
        /// <param name="shift"></param>
        /// <returns></returns>        
        public static Quad operator <<(Quad qd, int shift)
        {
            if (qd.Exponent==long.MinValue)
                return Zero;
            else
                return new Quad(qd.SignificandBits,qd.Exponent + shift);
        }

        /// <summary>
        /// Efficiently divides the Quad by 2^shift.
        /// </summary>
        /// <param name="qd"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static Quad operator >>(Quad qd, int shift)
        {
            if (qd.Exponent==long.MinValue)
                return Zero;
            else
                return new Quad(qd.SignificandBits,qd.Exponent - shift);
        }

        /// <summary>
        /// Efficiently multiplies the Quad by 2^shift.
        /// </summary>
        /// <param name="qd"></param>
        /// <param name="shift"></param>
        /// <returns></returns>        
        public static Quad LeftShift(Quad qd, long shift)
        {
            if (qd.Exponent == long.MinValue)
                return Zero;
            else
                return new Quad(qd.SignificandBits, qd.Exponent + shift);
        }

        /// <summary>
        /// Efficiently divides the Quad by 2^shift.
        /// </summary>
        /// <param name="qd"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static Quad RightShift(Quad qd, long shift)
        {
            if (qd.Exponent == long.MinValue)
                return Zero;
            else
                return new Quad(qd.SignificandBits, qd.Exponent - shift);
        }

        /// <summary>
        /// Divides one Quad by another and returns the result
        /// </summary>
        /// <param name="qd1"></param>
        /// <param name="qd2"></param>
        /// <returns></returns>
        /// <remarks>
        /// This code is a heavily modified derivation of a division routine given by http://www.hackersdelight.org/HDcode/divlu.c.txt ,
        /// which has a very liberal (public domain-like) license attached: http://www.hackersdelight.org/permissions.htm
        /// </remarks>
        public static Quad operator /(Quad qd1, Quad qd2)
        {    
            
            if (qd2.Exponent == long.MinValue)
                throw new DivideByZeroException();
            else if (qd1.Exponent == long.MinValue)
                return Zero;

            ulong un1 = 0,     // Norm. dividend LSD's.
                     vn1, vn0,        // Norm. divisor digits.
                     q1, q0,          // Quotient digits.
                     un21,// Dividend digit pairs.
                     rhat;            // A remainder.                        
            
            long adjExponent=0;
            ulong qd1AdjSignificand = qd1.SignificandBits | highestBit;  //de-implicitize the 1 before the binary point
            ulong qd2AdjSignificand = qd2.SignificandBits | highestBit;  //de-implicitize the 1 before the binary point

            if (qd1AdjSignificand >= qd2AdjSignificand)
            {                
                // need to make qd1's significand smaller than qd2's
                // If we were faithful to the original code this method derives from,
                // we would branch on qd1AdjSignificand > qd2AdjSignificand instead.
                // However, this results in undesirable results like (in binary) 11/11 = 0.11111...,
                // where the result should be 1.0.  Thus, we branch on >=, which prevents this problem.
                adjExponent = 1;
                un1 = (qd1.SignificandBits & 1) << 31;
                qd1AdjSignificand = qd1AdjSignificand >> 1;
            }

            vn1 = qd2AdjSignificand >> 32;            // Break divisor up into
            vn0 = qd2.SignificandBits & 0xFFFFFFFF;         // two 32-bit digits.            

            q1 = qd1AdjSignificand / vn1;            // Compute the first
            rhat = qd1AdjSignificand - q1 * vn1;     // quotient digit, q1.
        again1:
            if (q1 >= b || q1 * vn0 > b * rhat + un1)
            {
                q1 = q1 - 1;
                rhat = rhat + vn1;
                if (rhat < b) goto again1;
            }

            un21 = qd1AdjSignificand * b + un1 - q1 * qd2AdjSignificand;  // Multiply and subtract.

            q0 = un21 / vn1;            // Compute the second
            rhat = un21 - q0 * vn1;     // quotient digit, q0.
        again2:
            if (q0 >= b || q0 * vn0 > b * rhat)
            {
                q0 = q0 - 1;
                rhat = rhat + vn1;
                if (rhat < b) goto again2;
            }

            qd1AdjSignificand = q1 * b + q0; //convenient place to store intermediate result

            //if (qd1.Significand == 0) //the final significand should never be 0
            //    result.Exponent = 0;
            //else

            if (qd1AdjSignificand < (1UL << 63))                            
                return new Quad((~highestBit & (qd1AdjSignificand << 1)) | ((qd1.SignificandBits ^ qd2.SignificandBits) & highestBit),
                                qd1.Exponent - (qd2.Exponent + 64) - 1 + adjExponent);            
            else                            
                return new Quad((~highestBit & qd1AdjSignificand) | ((qd1.SignificandBits ^ qd2.SignificandBits) & highestBit),
                                qd1.Exponent - (qd2.Exponent + 64) + adjExponent);
        }

        public static Quad operator %(Quad qd1, Quad qd2)
        {
            return qd1 - (qd2 * Truncate(qd1 / qd2));
        }

        public static Quad operator -(Quad qd)
        {
            //qd.Exponent ^ -qd.Exponent == has the MSB set iff qdExponent = long.MinValue;
            //this allows us to handle 0's (which should never get a sign bit) without branching
            return new Quad(qd.SignificandBits ^ (highestBit & (ulong)(qd.Exponent ^ -qd.Exponent)), qd.Exponent);
        }

        public static Quad operator +(Quad qd1, Quad qd2)
        {
            if ((qd1.SignificandBits ^ qd2.SignificandBits) >= highestBit) //qd1 and qd2 have different signs--use subtraction instead
            {
                return qd1 - new Quad(qd2.SignificandBits ^ highestBit, qd2.Exponent);
            }

            //note on zeros: adding 0 results in a nop because the exponent is 64 less than any valid exponent.

            if (qd1.Exponent > qd2.Exponent)
            {
                if (qd1.Exponent >= qd2.Exponent + 64)
                    return qd1; //qd2 too small to make a difference
                else
                {
                    ulong bits = (qd1.SignificandBits | highestBit) + ((qd2.SignificandBits | highestBit) >> (int)(qd1.Exponent - qd2.Exponent));

                    if (bits < highestBit) //this can only happen in an overflow                    
                        return new Quad((qd1.SignificandBits & highestBit) | (bits >> 1), qd1.Exponent + 1);
                    else
                        return new Quad((qd1.SignificandBits & highestBit) | (bits & ~highestBit), qd1.Exponent);
                }
            }
            else if (qd1.Exponent < qd2.Exponent)
            {
                if (qd2.Exponent >= qd1.Exponent + 64)
                    return qd2; //qd1 too small to matter
                else
                {
                    ulong bits = (qd2.SignificandBits | highestBit) + ((qd1.SignificandBits | highestBit) >> (int)(qd2.Exponent - qd1.Exponent));

                    if (bits < highestBit) //this can only happen in an overflow                    
                        return new Quad((qd2.SignificandBits & highestBit) | (bits >> 1), qd2.Exponent + 1);
                    else
                        return new Quad((qd2.SignificandBits & highestBit) | (bits & ~highestBit), qd2.Exponent);
                }
            }
            else //expDiff == 0
            {
                if (qd1.Exponent == long.MinValue) //verify that we're not adding two 0's
                    return Zero; //we are, return 0

                //ulong bits = (qd1.Significand | highestBit) + (qd2.Significand | highestBit);

                //the MSB must have the same sign, so the MSB will become 0, and logical overflow is guaranteed in this situation (so we can shift right and increment the exponent).
                return new Quad(((qd1.SignificandBits + qd2.SignificandBits) >> 1) | (qd1.SignificandBits & highestBit), qd1.Exponent + 1);                
            }
        }

        public static Quad operator -(Quad qd1, Quad qd2)
        {
            if ((qd1.SignificandBits ^ qd2.SignificandBits) >= highestBit) //qd1 and qd2 have different signs--use addition instead
            {
                return qd1 + new Quad(qd2.SignificandBits ^ highestBit, qd2.Exponent);
            }

            //as in addition, we handle 0's implicitly--they will have an exponent at least 64 less than any valid non-zero value.

            if (qd1.Exponent > qd2.Exponent)
            {
                if (qd1.Exponent >= qd2.Exponent + 64)
                    return qd1; //qd2 too small to make a difference
                else
                {                                        
                    ulong bits = (qd1.SignificandBits|highestBit) - ( (qd2.SignificandBits|highestBit) >> (int)(qd1.Exponent - qd2.Exponent));

                    //make sure MSB is 1                       
                    int highestBitPos = nlz(bits);                    
                    return new Quad(((bits << highestBitPos) & ~highestBit) | (qd1.SignificandBits & highestBit), qd1.Exponent - highestBitPos);                    
                }
            }
            else if (qd1.Exponent < qd2.Exponent) //must subtract qd1's significand from qd2, and switch the sign
            {
                if (qd2.Exponent >= qd1.Exponent + 64)
                    return new Quad(qd2.SignificandBits ^ highestBit, qd2.Exponent); //qd1 too small to matter, switch sign of qd2 and return

                ulong bits = (qd2.SignificandBits | highestBit) - ((qd1.SignificandBits | highestBit) >> (int)(qd2.Exponent - qd1.Exponent));

                //make sure MSB is 1                       
                int highestBitPos = nlz(bits);
                return new Quad(((bits << highestBitPos) & ~highestBit) | (~qd2.SignificandBits & highestBit), qd2.Exponent - highestBitPos);                    
            }
            else // (qd1.Exponent == qd2.Exponent)
            {
                if (qd2.SignificandBits > qd1.SignificandBits) //must switch sign
                {
                    ulong bits = qd2.SignificandBits - qd1.SignificandBits; //notice that we don't worry about de-implicitizing the MSB--it'd be eliminated by subtraction anyway
                    int highestBitPos = nlz(bits);
                    return new Quad(((bits << highestBitPos) & ~highestBit) | (~qd2.SignificandBits & highestBit), qd2.Exponent - highestBitPos);
                }
                else if (qd2.SignificandBits < qd1.SignificandBits) //sign remains the same
                {
                    ulong bits = qd1.SignificandBits - qd2.SignificandBits; //notice that we don't worry about de-implicitizing the MSB--it'd be eliminated by subtraction anyway
                    int highestBitPos = nlz(bits);
                    return new Quad(((bits << highestBitPos) & ~highestBit) | (qd1.SignificandBits & highestBit), qd1.Exponent - highestBitPos);
                }
                else //qd1 == qd2
                    return Zero;
            }
            
        }

        public static Quad operator *(Quad qd1, Quad qd2)
        {                   
            ulong high1 = (qd1.SignificandBits | highestBit) >> 32; //de-implicitize the 1
            ulong high2 = (qd2.SignificandBits | highestBit) >> 32;

            //because the MSB of both significands is 1, the MSB of the result will also be 1, and the product of low bits on both significands is dropped (and thus we can skip its calculation)
            ulong significandBits = high1 * high2 + (((qd1.SignificandBits & lowWordMask) * high2) >> 32) + ((high1 * (qd2.SignificandBits & lowWordMask)) >> 32);

            if (significandBits < (1UL << 63))
            {
                //zeros
                if (qd1.Exponent == long.MinValue || qd2.Exponent == long.MinValue)
                    return Zero;
                else
                    return new Quad(((qd1.SignificandBits ^ qd2.SignificandBits) & highestBit) | ((significandBits << 1) & ~highestBit), qd1.Exponent + qd2.Exponent - 1 + 64);
            }
            else
                return new Quad(((qd1.SignificandBits ^ qd2.SignificandBits) & highestBit) | (significandBits & ~highestBit), qd1.Exponent + qd2.Exponent + 64);
        }

        public static Quad operator ++(Quad qd)
        {
            return qd + One;
        }

        public static Quad operator --(Quad qd)
        {
            return qd - One;
        }

        #endregion

        #region Comparison

        public static bool operator ==(Quad qd1, Quad qd2)
        {
            return (qd1.SignificandBits == qd2.SignificandBits && qd1.Exponent == qd2.Exponent);// || (qd1.Exponent == long.MinValue && qd2.Exponent == long.MinValue);
        }

        public static bool operator !=(Quad qd1, Quad qd2)
        {
            return (qd1.SignificandBits != qd2.SignificandBits || qd1.Exponent != qd2.Exponent);// && (qd1.Exponent != long.MinValue || qd2.Exponent != long.MinValue);
        }
       
        public static bool operator >(Quad qd1, Quad qd2)
        {
            //There is probably a faster way to accomplish this by cleverly exploiting signed longs
            switch ((qd1.SignificandBits & highestBit) | ((qd2.SignificandBits & highestBit) >> 1))
            {
                case highestBit: //qd1 is negative, qd2 positive
                    return false;
                case secondHighestBit: //qd1 positive, qd2 negative
                    return true;
                case highestBit | secondHighestBit: //both negative
                    return qd1.Exponent < qd2.Exponent || (qd1.Exponent == qd2.Exponent && qd1.SignificandBits < qd2.SignificandBits);
                default: //both positive
                    return qd1.Exponent > qd2.Exponent || (qd1.Exponent == qd2.Exponent && qd1.SignificandBits > qd2.SignificandBits);
            }                
        }

        public static bool operator <(Quad qd1, Quad qd2)
        {
            switch ((qd1.SignificandBits & highestBit) | ((qd2.SignificandBits & highestBit) >> 1))
            {
                case highestBit: //qd1 is negative, qd2 positive
                    return true;
                case secondHighestBit: //qd1 positive, qd2 negative
                    return false;
                case highestBit | secondHighestBit: //both negative
                    return qd1.Exponent > qd2.Exponent || (qd1.Exponent == qd2.Exponent && qd1.SignificandBits > qd2.SignificandBits);
                default: //both positive
                    return qd1.Exponent < qd2.Exponent || (qd1.Exponent == qd2.Exponent && qd1.SignificandBits < qd2.SignificandBits);
            }
            
        }

        public static bool operator >=(Quad qd1, Quad qd2)
        {
            switch ((qd1.SignificandBits & highestBit) | ((qd2.SignificandBits & highestBit) >> 1))
            {
                case highestBit: //qd1 is negative, qd2 positive
                    return false;
                case secondHighestBit: //qd1 positive, qd2 negative
                    return true;
                case highestBit | secondHighestBit: //both negative
                    return qd1.Exponent < qd2.Exponent || (qd1.Exponent == qd2.Exponent && qd1.SignificandBits <= qd2.SignificandBits);
                default: //both positive
                    return qd1.Exponent > qd2.Exponent || (qd1.Exponent == qd2.Exponent && qd1.SignificandBits >= qd2.SignificandBits);
            }
        }

        public static bool operator <=(Quad qd1, Quad qd2)
        {
            switch ((qd1.SignificandBits & highestBit) | ((qd2.SignificandBits & highestBit) >> 1))
            {
                case highestBit: //qd1 is negative, qd2 positive
                    return true;
                case secondHighestBit: //qd1 positive, qd2 negative
                    return false;
                case highestBit | secondHighestBit: //both negative
                    return qd1.Exponent > qd2.Exponent || (qd1.Exponent == qd2.Exponent && qd1.SignificandBits >= qd2.SignificandBits);
                default: //both positive
                    return qd1.Exponent < qd2.Exponent || (qd1.Exponent == qd2.Exponent && qd1.SignificandBits <= qd2.SignificandBits);
            }
        }

        #endregion

        #region String parsing

        /// <summary>
        /// Parses decimal number strings in the form of "1234.5678".  Does not presently handle exponential/scientific notation.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static Quad Parse(string number)
        {
            //Can piggyback on BigInteger's parser for this, but this is inefficient.
            //Smarter way is to break the numeric string into chunks and parse each of them using long's parse method, then combine.

            bool negative = number.StartsWith("-");
            if (negative) number = number.Substring(1);

            string left=number, right=null;
            int decimalPoint = number.IndexOf('.');
            if (decimalPoint >= 0)
            {
                left = number.Substring(0,decimalPoint);
                right = number.Substring(decimalPoint+1);
            }            

            System.Numerics.BigInteger leftInt = System.Numerics.BigInteger.Parse(left);

            Quad result = (Quad)leftInt;
            if (right != null)
            {
                System.Numerics.BigInteger rightInt = System.Numerics.BigInteger.Parse(right);
                Quad fractional = (Quad)rightInt;

                // we implicitly multiplied the stuff right of the decimal point by 10^(right.length) to get an integer;
                // now we must reverse that and add this quantity to our results.
                result += fractional * (Quad.Pow(new Quad(10L, 0), -right.Length));
            }

            return negative ? -result : result;
        }
        #endregion

        #region Math functions

        /// <summary>
        /// Removes any fractional part of the provided value (rounding down for positive numbers, and rounding up for negative numbers)            
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Quad Truncate(Quad value)
        {
            if (value.Exponent <= -64) return Zero;
            else if (value.Exponent >= 0) return value;
            else
            {
                //clear least significant "-value.exponent" bits that come after the binary point by shifting
                return new Quad((value.SignificandBits >> (int)(-value.Exponent)) << (int)(-value.Exponent),value.Exponent);                
            }
        }

        /// <summary>
        /// Returns only the fractional part of the provided value.  Equivalent to value % 1.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Quad Fraction(Quad value)
        {
            if (value.Exponent >= 0) return Zero; //no fraction
            else if (value.Exponent <= -64) return value; //all fraction (or zero)
            else
            {
                //clear most significant 64+value.exponent bits before the binary point
                ulong bits = (value.SignificandBits << (int)(64 + value.Exponent)) >> (int)(64 + value.Exponent);
                if (bits == 0) return Zero; //value is an integer
                
                int shift = nlz(bits); //renormalize                

                return new Quad((~highestBit & (bits << shift)) | (highestBit & value.SignificandBits), value.Exponent - shift);
            }
        }

        /// <summary>
        /// Calculates the log (base 2) of a Quad.            
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Log2(Quad value)
        {
            if (value.SignificandBits >= highestBit) return double.NaN;
            if (value.Exponent == long.MinValue) return double.NegativeInfinity; //Log(0)

            return Math.Log(value.SignificandBits | highestBit, 2) + value.Exponent;
        }

        /// <summary>
        /// Calculates the natural log (base e) of a Quad.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Log(Quad value)
        {
            if (value.SignificandBits >= highestBit) return double.NaN;
            if (value.Exponent == long.MinValue) return double.NegativeInfinity; //Log(0)

            return Math.Log(value.SignificandBits | highestBit) + value.Exponent * 0.69314718055994530941723212145818;
        }

        /// <summary>
        /// Raise a Quad to a given exponent.  Pow returns 1 for x^0 for all x >= 0.  An exception is thrown
        /// if 0 is raised to a negative exponent (implying division by 0), or if a negative value is raised
        /// by a non-integer exponent (yielding an imaginary number).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="exponent"></param>
        /// <returns></returns>
        /// <remarks>Internally, Pow uses Math.Pow.  This effectively limits the precision of the output to a double's 53 bits.</remarks>
        public static Quad Pow(Quad value, double exponent)
        {
            if (value.Exponent == long.MinValue)
            {
                if (exponent > 0)
                    return Zero;
                else if (exponent == 0)
                    return One;
                else
                    throw new ArgumentOutOfRangeException("Cannot raise 0 to a negative exponent, as this implies division by zero.");
            }

            if (value.SignificandBits >= highestBit && exponent % 1 != 0)
                throw new ArgumentOutOfRangeException("Cannot raise a negative number to a non-integer exponent, as this yields an imaginary number.");

            double resultSignificand = Math.Pow((double)new Quad(value.SignificandBits, -63), exponent);
            double resultExponent = (value.Exponent + 63) * exponent; //exponents multiply

            resultSignificand *= Math.Pow(2, resultExponent % 1); //push the fractional exponent into the significand

            Quad result = (Quad)resultSignificand; 
            result.Exponent += (long)Math.Truncate(resultExponent);

            return result;
        }

        public static Quad Max(Quad qd1, Quad qd2)
        {
            return qd1 > qd2 ? qd1 : qd2;
        }

        public static Quad Min(Quad qd1, Quad qd2)
        {
            return qd1 < qd2 ? qd1 : qd2;
        }

        public static Quad Abs(Quad qd)
        {
            return new Quad(qd.SignificandBits & ~highestBit, qd.Exponent); //clear the sign bit
        }
        #endregion

        #region Casts
        public static explicit operator Quad(System.Numerics.BigInteger value)
        {
            bool positive = value.Sign >= 0;
            if (!positive)
                value = -value; //don't want 2's complement!

            if (value == System.Numerics.BigInteger.Zero)
                return Zero;

            if (value <= ulong.MaxValue) //easy
            {
                ulong bits = (ulong)value;
                int shift = nlz(bits);
                return new Quad((bits << shift) & ~highestBit | (positive ? 0 : highestBit), -shift);
            }
            else //can only keep some of the bits
            {
                byte[] bytes = value.ToByteArray(); //least significant byte is first

                if (bytes[bytes.Length - 1] == 0) //appended when the MSB is set to differentiate from negative values
                    return new Quad((positive ? 0 : highestBit) | (~highestBit & ((ulong)bytes[bytes.Length - 2] << 56 | (ulong)bytes[bytes.Length - 3] << 48 | (ulong)bytes[bytes.Length - 4] << 40 | (ulong)bytes[bytes.Length - 5] << 32 | (ulong)bytes[bytes.Length - 6] << 24 | (ulong)bytes[bytes.Length - 7] << 16 | (ulong)bytes[bytes.Length - 8] << 8 | (ulong)bytes[bytes.Length - 9])), (bytes.Length - 9) * 8);
                else //shift bits up
                {
                    ulong bits = (ulong)bytes[bytes.Length - 1] << 56 | (ulong)bytes[bytes.Length - 2] << 48 | (ulong)bytes[bytes.Length - 3] << 40 | (ulong)bytes[bytes.Length - 4] << 32 | (ulong)bytes[bytes.Length - 5] << 24 | (ulong)bytes[bytes.Length - 6] << 16 | (ulong)bytes[bytes.Length - 7] << 8 | (ulong)bytes[bytes.Length - 8];
                    int shift = nlz(bits);
                    bits = (bits << shift) | (((ulong)bytes[bytes.Length - 9]) >> (8 - shift));
                    return new Quad((positive ? 0 : highestBit) | (~highestBit & bits), (bytes.Length - 8) * 8 - shift);
                }
            }
        }

        public static explicit operator System.Numerics.BigInteger(Quad value)
        {
            if (value.Exponent <= -64) //fractional or zero
                return System.Numerics.BigInteger.Zero;

            if (value.Exponent < 0)
            {
                if ( (value.SignificandBits & highestBit) == highestBit )
                    return -new System.Numerics.BigInteger( (value.SignificandBits) >> ((int)-value.Exponent) );
                else
                    return new System.Numerics.BigInteger( (value.SignificandBits|highestBit) >> ((int)-value.Exponent) );
            }
            
            if (value.Exponent > int.MaxValue) //you can presumably get a BigInteger bigger than 2^int.MaxValue bits, but you probably don't want to (it'd be several hundred MB).
                throw new InvalidCastException("BigIntegers do not permit left-shifts by more than int.MaxValue bits.  Since the exponent of the quad is more than this, the conversion cannot be performed.");

            if ( (value.SignificandBits & highestBit) == highestBit ) //negative number?
                return -(new System.Numerics.BigInteger(value.SignificandBits)<<(int)value.Exponent);
            else
                return (new System.Numerics.BigInteger(value.SignificandBits|highestBit) << (int)value.Exponent);
        }

        public static explicit operator ulong(Quad value)
        {
            if (value.SignificandBits >= highestBit) throw new ArgumentOutOfRangeException("Cannot convert negative value to ulong");

            if (value.Exponent > 0)
                throw new InvalidCastException("Value too large to fit in 64-bit unsigned integer");            

            if (value.Exponent <= -64) return 0;            

            return (highestBit | value.SignificandBits) >> (int)(-value.Exponent);
        }

        public static explicit operator long(Quad value)
        {
            if (value.SignificandBits == highestBit && value.Exponent == 0) //corner case
                return long.MinValue;

            if (value.Exponent >= 0)
                throw new InvalidCastException("Value too large to fit in 64-bit signed integer");

            if (value.Exponent <= -64) return 0;

            if (value.SignificandBits >= highestBit) //negative
                return -(long)(value.SignificandBits >> (int)(-value.Exponent));
            else
                return (long)( (value.SignificandBits|highestBit) >> (int)(-value.Exponent));            
        }

        public static unsafe explicit operator double(Quad value)
        {
            if (value.Exponent == long.MinValue) return 0;            

            if (value.Exponent <= -1086)
            {
                if (value.Exponent > -1086 - 52) //can create subnormal double value
                {
                    ulong bits =  (value.SignificandBits&highestBit) | ((value.SignificandBits|highestBit) >> (int)(-value.Exponent - 1086 + 12));
                    return *((double*)&bits);
                }
                else
                    return 0;
            }
            else
            {

                ulong bits = (ulong)(value.Exponent + 1086);
                if (bits >= 0x7ffUL) return value.SignificandBits >= highestBit ? double.NegativeInfinity : double.PositiveInfinity; //too large

                bits = (value.SignificandBits&highestBit) | (bits << 52) | (value.SignificandBits & (~highestBit)) >> 11;

                return *((double*)&bits);
            }
        }

        /// <summary>
        /// Converts a 64-bit unsigned integer into a Quad.  No data can be lost, nor will any exception be thrown, by this cast;
        /// however, it is marked explicit in order to avoid ambiguity with the implicit long-to-Quad cast operator.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Quad(ulong value)
        {
            if (value == 0) return Zero;
            int firstSetPosition = nlz(value);
            return new Quad( (value << firstSetPosition) & ~highestBit, -firstSetPosition);
        }

        public static implicit operator Quad(long value)
        {            
            return new Quad(value,0);
        }

        public static unsafe explicit operator Quad(double value)
        {            
            // Translate the double into sign, exponent and mantissa.
            //long bits = BitConverter.DoubleToInt64Bits(value); //I suspect doing an unsafe pointer-conversion to get the bits would be faster
            ulong bits = *((ulong*)&value);

            // Note that the shift is sign-extended, hence the test against -1 not 1                
            long exponent = (((long)bits >> 52) & 0x7ffL);

            if (exponent == 0x7ffL)
                throw new InvalidCastException("Cannot cast NaN or infinity to Quad");

            ulong mantissa = (bits) & 0xfffffffffffffUL;

            // Subnormal numbers; exponent is effectively one higher,
            // but there's no extra normalisation bit in the mantissa
            if (exponent == 0)
            {
                if (mantissa == 0) return Zero;
                exponent++;

                int firstSetPosition = nlz(mantissa);
                mantissa <<= firstSetPosition;
                exponent -= firstSetPosition;
            }            
            else
            {
                mantissa = mantissa << 11;
                exponent -= 11;
            }

            exponent -= 1075;                     

            return new Quad( (highestBit&bits) | mantissa, exponent);
        }

        #endregion

        #region ToString
        /// <summary>
        /// Returns this number as a decimal, or in scientific notation where a decimal would be excessively long.
        /// Equivalent to ToString(QuadrupleFormat.ScientificApproximate).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(QuadrupleStringFormat.ScientificApproximate);
        }

        /// <summary>
        /// Obtains a string representation for this Quad according to the specified format.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        /// <remarks>
        /// ScientificExact returns the value in scientific notation as accurately as possible, but is still subject to imprecision due to the conversion from 
        /// binary to decimal and during the divisions or multiplications used in the conversion.  It does not use rounding, which can lead to odd-looking outputs
        /// that would otherwise be rounded by double.ToString() or the ScientificApproximate format (which uses double.ToString()).  For example, 0.1 will be rendered
        /// as the string "9.9999999999999999981e-2".
        /// </remarks>
        public string ToString(QuadrupleStringFormat format)
        {
            if (Exponent == long.MinValue) return "0";

            switch (format)
            {
                case QuadrupleStringFormat.HexExponential:
                    if (SignificandBits >= highestBit)
                        return "-" + SignificandBits.ToString("x") + "*2^" + (Exponent >= 0 ? Exponent.ToString("x") : "-" + (-Exponent).ToString("x"));
                    else
                        return (SignificandBits | highestBit).ToString("x") + "*2^" + (Exponent >= 0 ? Exponent.ToString("x") : "-" + (-Exponent).ToString("x"));

                case QuadrupleStringFormat.DecimalExponential:
                    if (SignificandBits >= highestBit)
                        return "-" + SignificandBits.ToString() + "*2^" + Exponent.ToString();
                    else
                        return (SignificandBits | highestBit).ToString() + "*2^" + Exponent.ToString();

                case QuadrupleStringFormat.ScientificApproximate:
                    if (Exponent >= -1022 && Exponent <= 1023) //can be represented as double (albeit with a precision loss)
                        return ((double)this).ToString(System.Globalization.CultureInfo.InvariantCulture);

                    double dVal = (double)new Quad(SignificandBits, -61);
                    double dExp = base2to10Multiplier * (Exponent + 61);

                    string sign = "";
                    if (dVal < 0)
                    {
                        sign = "-";
                        dVal = -dVal;
                    }

                    if (dExp >= 0)
                        dVal *= Math.Pow(10, (dExp % 1));
                    else
                        dVal *= Math.Pow(10, -((-dExp) % 1));

                    long iExp = (long)Math.Truncate(dExp);

                    while (dVal >= 10) { iExp++; dVal /= 10; }
                    while (dVal < 1) { iExp--; dVal *= 10; }

                    if (iExp >= -10 && iExp < 0)
                    {
                        string dValString = dVal.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (dValString[1] != '.')
                            goto returnScientific; //unexpected formatting; use default behavior.
                        else
                            return sign + "0." + new string('0', (int)((-iExp) - 1)) + dVal.ToString(System.Globalization.CultureInfo.InvariantCulture).Remove(1, 1);
                    }
                    else if (iExp >= 0 && iExp <= 10)
                    {
                        string dValString = dVal.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (dValString[1] != '.')
                            goto returnScientific; //unexpected formating; use default behavior.
                        else
                        {
                            dValString = dValString.Remove(1, 1);
                            if (iExp < dValString.Length - 1)
                                return sign + dValString.Substring(0, 1 + (int)iExp) + "." + dValString.Substring(1 + (int)iExp);
                            else
                                return sign + dValString + new string('0', (int)iExp - (dValString.Length - 1)) + ".0";
                        }
                    }

                returnScientific:
                    return sign + dVal.ToString(System.Globalization.CultureInfo.InvariantCulture) + "E" + (iExp >= 0 ? "+" + iExp : iExp.ToString());

                case QuadrupleStringFormat.ScientificExact:
                    if (this == Zero) return "0";
                    if (Fraction(this) == Zero && this.Exponent <= 0) //integer value that we can output directly
                        return (this.SignificandBits >= highestBit ? "-" : "") + ((this.SignificandBits | highestBit) >> (int)(-this.Exponent)).ToString();

                    Quad absValue = Abs(this);

                    long e = 0;
                    if (absValue < One)
                    {
                        while (true)
                        {
                            if (absValue < en18)
                            {
                                absValue.Multiply(e19);
                                e -= 19;
                            }
                            else if (absValue < en9)
                            {
                                absValue.Multiply(e10);
                                e -= 10;
                            }
                            else if (absValue < en4)
                            {
                                absValue.Multiply(e5);
                                e -= 5;
                            }
                            else if (absValue < en2)
                            {
                                absValue.Multiply(e3);
                                e -= 3;
                            }
                            else if (absValue < One)
                            {
                                absValue.Multiply(e1);
                                e -= 1;
                            }
                            else
                                break;
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            if (absValue >= e19)
                            {
                                absValue.Divide(e19);
                                e += 19;
                            }
                            else if (absValue >= e10)
                            {
                                absValue.Divide(e10);
                                e += 10;
                            }
                            else if (absValue >= e5)
                            {
                                absValue.Divide(e5);
                                e += 5;
                            }
                            else if (absValue >= e3)
                            {
                                absValue.Divide(e3);
                                e += 3;
                            }
                            else if (absValue >= e1)
                            {
                                absValue.Divide(e1);
                                e += 1;
                            }
                            else
                                break;
                        }
                    }

                    //absValue is now in the interval [1,10)
                    StringBuilder result = new StringBuilder();

                    result.Append(IntegerString(absValue,1) + ".");

                    while ( (absValue=Fraction(absValue)) > Zero)
                    {
                        absValue.Multiply(e19);
                        result.Append(IntegerString(absValue, 19));
                    }

                    string resultString = result.ToString().TrimEnd('0'); //trim excess 0's at the end
                    if (resultString[resultString.Length - 1] == '.') resultString += "0"; //e.g. 1.0 instead of 1.

                    return (this.SignificandBits >= highestBit ? "-" : "") + resultString + "e" + (e >= 0 ? "+" : "") + e;

                default:
                    throw new ArgumentException("Unknown format requested");
            }
        }
        
        /// <summary>
        /// Retrieves the integer portion of the quad as a string,
        /// assuming that the quad's value is less than long.MaxValue.
        /// No sign ("-") is prepended to the result in the case of negative values.
        /// </summary>
        /// <returns></returns>
        private static string IntegerString(Quad quad,int digits)
        {
            if (quad.Exponent > 0) throw new ArgumentOutOfRangeException("The given quad is larger than long.MaxValue");
            if (quad.Exponent <= -64) return "0";

            ulong significand = quad.SignificandBits | highestBit; //make explicit the implicit bit
            return (significand >> (int)(-quad.Exponent)).ToString( new string('0', digits ));
        }
        

        #endregion

        #region GetHashCode and Equals

        public override int GetHashCode()
        {
            int expHash = Exponent.GetHashCode();
            return SignificandBits.GetHashCode() ^ (expHash << 16 | expHash >> 16); //rotate expHash's bits 16 places
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            try
            {
                return this == (Quad)obj;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }


    /// <summary>
    /// The enum that specifies the possible formats for the output 
    /// of <see cref="System.Quad">Quad</see>'s ToString method.
    /// </summary>
    public enum QuadrupleStringFormat
    {
        /// <summary>
        /// Obtains the quadruple in scientific notation.  Only ~52 bits of significand 
        /// precision are used to create this string.
        /// </summary>
        ScientificApproximate,

        /// <summary>
        /// Obtains the quadruple in scientific notation with full precision. 
        /// This can be very expensive to compute and takes time linear in the value
        /// of the exponent.
        /// </summary>
        ScientificExact,

        /// <summary>
        /// Obtains the quadruple in hexadecimal exponential format, consisting 
        /// of a 64-bit hex integer followed by the binary exponent,
        /// also expressed as a (signed) 64-bit hexadecimal integer.
        /// E.g. ffffffffffffffff*2^-AB3
        /// </summary>
        HexExponential,

        /// <summary>
        /// Obtains the quadruple in decimal exponential format, consisting 
        /// of a 64-bit decimal integer followed by the 64-bit signed decimal 
        /// integer exponent.
        /// E.g. 34592233*2^34221
        /// </summary>
        DecimalExponential
    }
}
