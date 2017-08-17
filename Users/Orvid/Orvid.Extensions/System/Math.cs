using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    /// <summary>
    /// Contains various extensions to <see cref="System.Math"/>.
    /// </summary>
    public static class MathExtensions
    {
        #region Internal things

        private const ulong QuadhighestBit = 1UL << 63;

        private static int Quadnlz(ulong x)
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

        /// <summary>
        /// Removes any fractional part of the provided value (rounding down for positive numbers, and rounding up for negative numbers)            
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Quad Truncate(Quad value)
        {
            if (value.Exponent <= -64) 
                return Quad.Zero;
            else if (value.Exponent >= 0)
                return value;
            else
            {
                //clear least significant "-value.exponent" bits that come after the binary point by shifting
                return new Quad((value.SignificandBits >> (int)(-value.Exponent)) << (int)(-value.Exponent), value.Exponent);
            }
        }

        /// <summary>
        /// Returns only the fractional part of the provided value.  Equivalent to value % 1.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Quad Fraction(Quad value)
        {
            if (value.Exponent >= 0) return Quad.Zero; //no fraction
            else if (value.Exponent <= -64) return value; //all fraction (or zero)
            else
            {
                //clear most significant 64+value.exponent bits before the binary point
                ulong bits = (value.SignificandBits << (int)(64 + value.Exponent)) >> (int)(64 + value.Exponent);
                if (bits == 0) return Quad.Zero; //value is an integer

                int shift = Quadnlz(bits); //renormalize                

                return new Quad((~QuadhighestBit & (bits << shift)) | (QuadhighestBit & value.SignificandBits), value.Exponent - shift);
            }
        }

        /// <summary>
        /// Calculates the log (base 2) of a Quad.            
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Log2(Quad value)
        {
            if (value.SignificandBits >= QuadhighestBit) return double.NaN;
            if (value.Exponent == long.MinValue) return double.NegativeInfinity; //Log(0)

            return Math.Log(value.SignificandBits | QuadhighestBit, 2) + value.Exponent;
        }

        /// <summary>
        /// Calculates the natural log (base e) of a Quad.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Log(Quad value)
        {
            if (value.SignificandBits >= QuadhighestBit)
                return double.NaN;
            if (value.Exponent == long.MinValue)
                return double.NegativeInfinity; //Log(0)

            return Math.Log(value.SignificandBits | QuadhighestBit) + value.Exponent * 0.69314718055994530941723212145818;
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
                    return Quad.Zero;
                else if (exponent == 0)
                    return Quad.One;
                else
                    throw new ArgumentOutOfRangeException("Cannot raise 0 to a negative exponent, as this implies division by zero.");
            }

            if (value.SignificandBits >= QuadhighestBit && exponent % 1 != 0)
                throw new ArgumentOutOfRangeException("Cannot raise a negative number to a non-integer exponent, as this yields an imaginary number.");

            double resultSignificand = Math.Pow((double)new Quad(value.SignificandBits, -63), exponent);
            double resultExponent = (value.Exponent + 63) * exponent; //exponents multiply

            resultSignificand *= Math.Pow(2, resultExponent % 1); //push the fractional exponent into the significand

            Quad result = (Quad)resultSignificand;
            result.Exponent += (long)Math.Truncate(resultExponent);

            return result;
        }

        /// <summary>
        /// Returns the larger of the 2 values.
        /// </summary>
        /// <param name="qd1"></param>
        /// <param name="qd2"></param>
        /// <returns></returns>
        public static Quad Max(Quad qd1, Quad qd2)
        {
            return qd1 > qd2 ? qd1 : qd2;
        }

        /// <summary>
        /// Returns the smaller of the 2 values.
        /// </summary>
        /// <param name="qd1"></param>
        /// <param name="qd2"></param>
        /// <returns></returns>
        public static Quad Min(Quad qd1, Quad qd2)
        {
            return qd1 < qd2 ? qd1 : qd2;
        }

        /// <summary>
        /// Returns the absolute value of the specified <see cref="Quad"/>.
        /// </summary>
        /// <param name="qd"></param>
        /// <returns></returns>
        public static Quad Abs(Quad qd)
        {
            return new Quad(qd.SignificandBits & ~QuadhighestBit, qd.Exponent); //clear the sign bit
        }

        /// <summary>
        /// Returns a value indicating the sign of a half-precision floating-point number.
        /// </summary>
        /// <param name="value">A signed number.</param>
        /// <returns>
        /// A number indicating the sign of value. Number Description -1 value is less
        /// than zero. 0 value is equal to zero. 1 value is greater than zero.
        /// </returns>
        /// <exception cref="System.ArithmeticException">value is equal to System.Half.NaN.</exception>
        public static int Sign(Half value)
        {
            if (value < 0)
            {
                return -1;
            }
            else if (value > 0)
            {
                return 1;
            }
            else
            {
                if (value != 0)
                {
                    throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
                }
            }

            return 0;
        }
        /// <summary>
        /// Returns the absolute value of a half-precision floating-point number.
        /// </summary>
        /// <param name="value">A number in the range System.Half.MinValue ≤ value ≤ System.Half.MaxValue.</param>
        /// <returns>A half-precision floating-point number, x, such that 0 ≤ x ≤System.Half.MaxValue.</returns>
        public static Half Abs(Half value)
        {
            return Half.ToHalf((ushort)(value.value & 0x7fff));
        }

        /// <summary>
        /// Returns the larger of two half-precision floating-point numbers.
        /// </summary>
        /// <param name="value1">The first of two half-precision floating-point numbers to compare.</param>
        /// <param name="value2">The second of two half-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter value1 or value2, whichever is larger. If value1, or value2, or both val1
        /// and value2 are equal to System.Half.NaN, System.Half.NaN is returned.
        /// </returns>
        public static Half Max(Half value1, Half value2)
        {
            return (value1 < value2) ? value2 : value1;
        }

        /// <summary>
        /// Returns the smaller of two half-precision floating-point numbers.
        /// </summary>
        /// <param name="value1">The first of two half-precision floating-point numbers to compare.</param>
        /// <param name="value2">The second of two half-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter value1 or value2, whichever is smaller. If value1, or value2, or both val1
        /// and value2 are equal to System.Half.NaN, System.Half.NaN is returned.
        /// </returns>
        public static Half Min(Half value1, Half value2)
        {
            return (value1 < value2) ? value1 : value2;
        }
    }
}
