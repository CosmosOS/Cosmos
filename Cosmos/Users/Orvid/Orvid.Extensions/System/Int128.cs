#region Copyright and License
/*
 * SharpAssembler
 * Library for .NET that assembles a predetermined list of
 * instructions into machine code.
 * 
 * Copyright (C) 2011 Daniël Pelsmaeker
 * 
 * This file is part of SharpAssembler.
 * 
 * SharpAssembler is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * SharpAssembler is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with SharpAssembler.  If not, see <http://www.gnu.org/licenses/>.
 */
#endregion
using System;
using System.Diagnostics.Contracts;

namespace SharpAssembler.Core
{
	/// <summary>
	/// A 128-bit signed integer.
	/// </summary>
	/// <remarks>
	/// This implementation is based on the code described in
	/// <see href="http://www.informit.com/guides/content.aspx?g=dotnet&amp;seqNum=636"/>.
	/// </remarks>
	public struct Int128 : IFormattable, IConvertible,
		IComparable, IComparable<Int128>, IEquatable<Int128>
	{
		#region Constants
		/// <summary>
		/// The maximum value a <see cref="Int128"/> can represent.
		/// </summary>
		public static readonly Int128 MaxValue = new Int128(0xFFFFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF);
		/// <summary>
		/// The minimum value a <see cref="Int128"/> can represent.
		/// </summary>
		public static readonly Int128 MinValue = new Int128(0, unchecked((long)0x8000000000000000));

		/// <summary>
		/// A <see cref="Int128"/> value of -1.
		/// </summary>
		public static readonly Int128 MinusOne = new Int128(0xFFFFFFFFFFFFFFFF, unchecked((long)0xFFFFFFFFFFFFFFFF));
		/// <summary>
		/// A <see cref="Int128"/> value of 0.
		/// </summary>
		public static readonly Int128 Zero = new Int128(0, 0);
		/// <summary>
		/// A <see cref="Int128"/> value of 1.
		/// </summary>
		public static readonly Int128 One = new Int128(1, 0);
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Int128"/> struct.
		/// </summary>
		/// <param name="low">The least significant 64-bits of the integer.</param>
		/// <param name="high">The most significant 64-bits of the integer.</param>
		public Int128(ulong low, long high)
		{
			this.low = low;
			this.high = high;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets whether the value of this <see cref="Int128"/> is an even number.
		/// </summary>
		/// <value><see langword="true"/> when this value is an even number;
		/// otherwise, <see langword="false"/>.</value>
		/// <remarks>
		/// Zero is also an even number.
		/// </remarks>
		public bool IsEven
		{
			get
			{
				return (low & 1) == 0;
			}
		}

		/// <summary>
		/// Gets whether the value of this <see cref="Int128"/> is zero.
		/// </summary>
		/// <value><see langword="true"/> when this value is zero;
		/// otherwise, <see langword="false"/>.</value>
		public bool IsZero
		{
			get { return high == 0 && low == 0; }
		}

		/// <summary>
		/// Gets whether the value of this <see cref="Int128"/> is one.
		/// </summary>
		/// <value><see langword="true"/> when this value is one;
		/// otherwise, <see langword="false"/>.</value>
		public bool IsOne
		{
			get { return high == 0 && low == 1; }
		}

		/// <summary>
		/// Gets whether the value of this <see cref="Int128"/> is a power of two.
		/// </summary>
		/// <value><see langword="true"/> when this value is a power of two;
		/// otherwise, <see langword="false"/>.</value>
		public bool IsPowerOfTwo
		{
			get { return !IsZero && (this & (this - 1)) == 0; }
		}

		/// <summary>
		/// Gets a number that indicates the sign (negative, positive, or zero) of the current <see cref="Int128"/>.
		/// </summary>
		/// <value>A number that indicates the sign. The return value is 0 when the value is zero, 1 when the value
		/// is positive or -1 when the value is negative.</value>
		public int Sign
		{
			get { return this.CompareTo(0); }
		}

		/// <summary>
		/// Gets the 64 least significant bits of the value.
		/// </summary>
		/// <value>The 64 least significant bits.</value>
		public ulong Low
		{
			get { return this.low; }
		}

		/// <summary>
		/// Gets the 64 most significant bits of the value.
		/// </summary>
		/// <value>The 64 most significant bits.</value>
		public long High
		{
			get { return this.high; }
		}
		#endregion

		#region Equality
		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns><see langword="true"/> if <paramref name="obj"/> and this instance are the same type and represent
		/// the same value; otherwise, <see langword="false"/>.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(obj, null) ||
				!(obj is Int128))
				return false;
			return Equals((Int128)obj);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns><see langword="true"/> if the current object is equal to the other parameter;
		/// otherwise, <see langword="false"/>.</returns>
		public bool Equals(Int128 other)
		{
			return this.high == other.high && this.low == other.low;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + low.GetHashCode();
				hash = hash * 23 + high.GetHashCode();
				return hash;
			}
		}

		/// <summary>
		/// Returns a value that indicates whether two <see cref="Int128"/> objects have the same value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> parameters have
		/// the same value; otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(Int128 left, Int128 right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Returns a value that indicates whether two <see cref="Int128"/> objects have different values.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> parameters have
		/// different values; otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(Int128 left, Int128 right)
		{
			return !left.Equals(right);
		}
		#endregion

		#region Comparisons
		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared.</returns>
		public int CompareTo(object obj)
		{
			if (obj is Int128)
				return CompareTo((Int128)obj);
			else
				throw new ArgumentException("The specified object is not the same type as this instance.", "obj");
		}

		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared.</returns>
		public int CompareTo(Int128 other)
		{
			int result = this.high.CompareTo(other.high);
			if (result == 0)
				result = this.low.CompareTo(other.low);
			return result;
		}

		/// <summary>
		/// Returns a value that indicates whether a <see cref="Int128"/> value is greater than another
		/// <see cref="Int128"/> value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true"/> if the value of <paramref name="left"/> is greater than the value of
		/// <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
		public static bool operator >(Int128 left, Int128 right)
		{
			return left.CompareTo(right) > 0;
		}

		/// <summary>
		/// Returns a value that indicates whether a <see cref="Int128"/> value is less than another
		/// <see cref="Int128"/> value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true"/> if the value of <paramref name="left"/> is less than the value of
		/// <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
		public static bool operator <(Int128 left, Int128 right)
		{
			return left.CompareTo(right) < 0;
		}

		/// <summary>
		/// Returns a value that indicates whether a <see cref="Int128"/> value is greater than or equal to another
		/// <see cref="Int128"/> value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true"/> if the value of <paramref name="left"/> is greater than or equal to the
		/// value of <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
		public static bool operator >=(Int128 left, Int128 right)
		{
			return left.CompareTo(right) >= 0;
		}

		/// <summary>
		/// Returns a value that indicates whether a <see cref="Int128"/> value is less than or equal to another
		/// <see cref="Int128"/> value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true"/> if the value of <paramref name="left"/> is less than or equal to the value
		/// of <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
		public static bool operator <=(Int128 left, Int128 right)
		{
			return left.CompareTo(right) <= 0;
		}
		#endregion

		#region Conversions
		/// <summary>
		/// Converts the specified unsigned 64-bit value to a signed 128-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 128-bit value.</returns>
		public static implicit operator Int128(ulong value)
		{
			return new Int128(value, 0);
		}

		/// <summary>
		/// Converts the specified signed 64-bit value to a signed 128-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 128-bit value.</returns>
		public static implicit operator Int128(long value)
		{
			return new Int128(unchecked((ulong)value), (value < 0 ? -1 : 0));
		}

		/// <summary>
		/// Converts the specified unsigned 32-bit value to a signed 128-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 128-bit value.</returns>
		public static implicit operator Int128(uint value)
		{
			return (Int128)(ulong)value;
		}

		/// <summary>
		/// Converts the specified signed 32-bit value to a signed 128-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 128-bit value.</returns>
		public static implicit operator Int128(int value)
		{
			return (Int128)(long)value;
		}

		/// <summary>
		/// Converts the specified unsigned 16-bit value to a signed 128-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 128-bit value.</returns>
		public static implicit operator Int128(ushort value)
		{
			return (Int128)(ulong)value;
		}

		/// <summary>
		/// Converts the specified signed 16-bit value to a signed 128-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 128-bit value.</returns>
		public static implicit operator Int128(short value)
		{
			return (Int128)(long)value;
		}

		/// <summary>
		/// Converts the specified unsigned 8-bit value to a signed 128-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 128-bit value.</returns>
		public static implicit operator Int128(byte value)
		{
			return (Int128)(ulong)value;
		}

		/// <summary>
		/// Converts the specified signed 8-bit value to a signed 128-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 128-bit value.</returns>
		public static implicit operator Int128(sbyte value)
		{
			return (Int128)(long)value;
		}

		/// <summary>
		/// Converts the specified signed 128-bit value to an unsigned 64-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 64-bit value.</returns>
		public static explicit operator ulong(Int128 value)
		{
			return (ulong)value.low;
		}

		/// <summary>
		/// Converts the specified signed 128-bit value to a signed 64-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 64-bit value.</returns>
		public static explicit operator long(Int128 value)
		{
			return (long)value.low;
		}

		/// <summary>
		/// Converts the specified signed 128-bit value to an unsigned 32-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 32-bit value.</returns>
		public static explicit operator uint(Int128 value)
		{
			return (uint)value.low;
		}

		/// <summary>
		/// Converts the specified signed 128-bit value to a signed 32-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 32-bit value.</returns>
		public static explicit operator int(Int128 value)
		{
			return (int)value.low;
		}

		/// <summary>
		/// Converts the specified signed 128-bit value to an unsigned 16-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 16-bit value.</returns>
		public static explicit operator ushort(Int128 value)
		{
			return (ushort)value.low;
		}

		/// <summary>
		/// Converts the specified signed 128-bit value to a signed 16-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 16-bit value.</returns>
		public static explicit operator short(Int128 value)
		{
			return (short)value.low;
		}

		/// <summary>
		/// Converts the specified signed 128-bit value to an unsigned 8-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 8-bit value.</returns>
		public static explicit operator byte(Int128 value)
		{
			return (byte)value.low;
		}

		/// <summary>
		/// Converts the specified signed 128-bit value to a signed 8-bit value.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The resulting 8-bit value.</returns>
		public static explicit operator sbyte(Int128 value)
		{
			return (sbyte)value.low;
		}

		/// <summary>
		/// Returns the <see cref="TypeCode"/> for this instance.
		/// </summary>
		/// <returns>The enumerated constant that is the <see cref="TypeCode"/> of the class or value type that
		/// implements this interface.</returns>
		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		/// <summary>
		/// Converts the value of this instance to an <see cref="Object"/> of the specified <see cref="Type"/> that has
		/// an equivalent value, using the specified culture-specific formatting information.
		/// </summary>
		/// <param name="conversionType">The <see cref="Type"/> to which the value of this instance is
		/// converted.</param>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>An <see cref="Object"/> instance of type <paramref name="conversionType"/> whose value is
		/// equivalent to the value of this instance.</returns>
		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			IConvertible conv = (IConvertible)this;
			
			if (conversionType == typeof(bool))
				return conv.ToBoolean(provider);
			if (conversionType == typeof(byte))
				return conv.ToByte(provider);
			if (conversionType == typeof(char))
				return conv.ToChar(provider);
			if (conversionType == typeof(DateTime))
				return conv.ToDateTime(provider);
			if (conversionType == typeof(Decimal))
				return conv.ToDecimal(provider);
			if (conversionType == typeof(Double))
				return conv.ToDouble(provider);
			if (conversionType == typeof(Int16))
				return conv.ToInt16(provider);
			if (conversionType == typeof(Int32))
				return conv.ToInt32(provider);
			if (conversionType == typeof(Int64))
				return conv.ToInt64(provider);
			if (conversionType == typeof(SByte))
				return conv.ToSByte(provider);
			if (conversionType == typeof(Single))
				return conv.ToSingle(provider);
			if (conversionType == typeof(String))
				return conv.ToString(provider);
			if (conversionType == typeof(UInt16))
				return conv.ToUInt16(provider);
			if (conversionType == typeof(UInt32))
				return conv.ToUInt32(provider);
			if (conversionType == typeof(UInt64))
				return conv.ToUInt64(provider);
			if (conversionType == typeof(Int128))
				return this;

			throw new InvalidCastException();
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent boolean value using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A boolean value equivalent to the value of this instance.</returns>
		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return !(this.low == 0 && this.high == 0);
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent Unicode character using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A Unicode character equivalent to the value of this instance.</returns>
		Char IConvertible.ToChar(IFormatProvider provider)
		{
			if (this < Char.MinValue || this > Char.MaxValue)
				throw new OverflowException();
			return (Char)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent 8-bit unsigned integer using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A 8-bit unsigned integer equivalent to the value of this instance.</returns>
		Byte IConvertible.ToByte(IFormatProvider provider)
		{
			if (this < Byte.MinValue || this > Byte.MaxValue)
				throw new OverflowException();
			return (Byte)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent 8-bit signed integer using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A 8-bit signed integer equivalent to the value of this instance.</returns>
		SByte IConvertible.ToSByte(IFormatProvider provider)
		{
			if (this < SByte.MinValue || this > SByte.MaxValue)
				throw new OverflowException();
			return (SByte)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent 16-bit signed integer using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A 16-bit signed integer equivalent to the value of this instance.</returns>
		Int16 IConvertible.ToInt16(IFormatProvider provider)
		{
			if (this < Int16.MinValue || this > Int16.MaxValue)
				throw new OverflowException();
			return (Int16)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent 32-bit signed integer using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A 32-bit signed integer equivalent to the value of this instance.</returns>
		Int32 IConvertible.ToInt32(IFormatProvider provider)
		{
			if (this < Int32.MinValue || this > Int32.MaxValue)
				throw new OverflowException();
			return (Int32)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent 64-bit signed integer using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A 64-bit signed integer equivalent to the value of this instance.</returns>
		Int64 IConvertible.ToInt64(IFormatProvider provider)
		{
			if (this < Int64.MinValue || this > Int64.MaxValue)
				throw new OverflowException();
			return (Int64)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent 16-bit unsigned integer using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A 16-bit unsigned integer equivalent to the value of this instance.</returns>
		UInt16 IConvertible.ToUInt16(IFormatProvider provider)
		{
			if (this < UInt16.MinValue || this > UInt16.MaxValue)
				throw new OverflowException();
			return (UInt16)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent 32-bit unsigned integer using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A 32-bit unsigned integer equivalent to the value of this instance.</returns>
		UInt32 IConvertible.ToUInt32(IFormatProvider provider)
		{
			if (this < UInt32.MinValue || this > UInt32.MaxValue)
				throw new OverflowException();
			return (UInt32)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent 64-bit unsigned integer using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A 64-bit unsigned integer equivalent to the value of this instance.</returns>
		UInt64 IConvertible.ToUInt64(IFormatProvider provider)
		{
			if (this < UInt64.MinValue || this > UInt64.MaxValue)
				throw new OverflowException();
			return (UInt64)this;
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent <see cref="DateTime"/> value using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A <see cref="DateTime"/> value equivalent to the value of this instance.</returns>
		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent decimal value using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A decimal value equivalent to the value of this instance.</returns>
		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent floating-point value using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A floating-point value equivalent to the value of this instance.</returns>
		double IConvertible.ToDouble(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent floating-point value using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A floating-point value equivalent to the value of this instance.</returns>
		float IConvertible.ToSingle(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		/// <summary>
		/// Converts the value of this instance to an equivalent string value using the specified
		/// culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>A string value equivalent to the value of this instance.</returns>
		string IConvertible.ToString(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		#endregion

		#region Arithmetic

		#region Unary
		/// <summary>
		/// Returns the value of the <see cref="Int128"/>.
		/// </summary>
		/// <param name="value">An integer value.</param>
		/// <returns>The value of the <paramref name="value"/> parameter.</returns>
		public static Int128 operator +(Int128 value)
		{
			return value;
		}

		/// <summary>
		/// Negates a <see cref="Int128"/> value.
		/// </summary>
		/// <param name="value">The value to negate.</param>
		/// <returns>The result of the value parameter multiplied by negative one (-1).</returns>
		public static Int128 operator -(Int128 value)
		{
			return Negate(value);
		}

		/// <summary>
		/// Negates an <see cref="Int128"/>.
		/// </summary>
		/// <param name="value">The value to negate.</param>
		/// <returns>The result of the value parameter multiplied by negative one (-1).</returns>
		public static Int128 Negate(Int128 value)
		{
			value = ~value;
			value++;
			return value;
		}

		/// <summary>
		/// Returns the bitwise one's complement of an <see cref="Int128"/> value.
		/// </summary>
		/// <param name="value">An integer value.</param>
		/// <returns>The bitwise one's complement of <paramref name="value"/>.</returns>
		public static Int128 operator ~(Int128 value)
		{
			return new Int128(~value.low, ~value.high);
		}

		/// <summary>
		/// Increments a <see cref="Int128"/> value by 1.
		/// </summary>
		/// <param name="value">The value to increment.</param>
		/// <returns>The value of the <paramref name="value"/> parameter incremented by 1.</returns>
		public static Int128 operator ++(Int128 value)
		{
			value.low++;
			if (value.low == 0)
				value.high++;
			return value;
		}

		/// <summary>
		/// Decrements a <see cref="Int128"/> value by 1.
		/// </summary>
		/// <param name="value">The value to decrement.</param>
		/// <returns>The value of the <paramref name="value"/> parameter decremented by 1.</returns>
		public static Int128 operator --(Int128 value)
		{
			if (value.low == 0)
				value.high--;
			value.low--;
			return value;
		}
		#endregion

		#region Binary
		/// <summary>
		/// Adds the values of two specified <see cref="Int128"/> values.
		/// </summary>
		/// <param name="left">The first value to add.</param>
		/// <param name="right">The second value to add.</param>
		/// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
		public static Int128 operator +(Int128 left, Int128 right)
		{
			return Add(left, right);
		}

		/// <summary>
		/// Adds two <see cref="Int128"/> values and returns the result.
		/// </summary>
		/// <param name="left">The first value to add.</param>
		/// <param name="right">The second value to add.</param>
		/// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
		public static Int128 Add(Int128 left, Int128 right)
		{
			var oldLow = left.low;

			left.low += right.low;
			left.high += right.high;
			if (left.low < oldLow)
				left.high++;

			return left;
		}

		/// <summary>
		/// Subtracts an <see cref="Int128"/> from another <see cref="Int128"/> value.
		/// </summary>
		/// <param name="left">The value to subtract from.</param>
		/// <param name="right">The value to subtract.</param>
		/// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
		public static Int128 operator -(Int128 left, Int128 right)
		{
			return Subtract(left, right);
		}

		/// <summary>
		/// Subtracts one <see cref="Int128"/> from another and returns the result.
		/// </summary>
		/// <param name="left">The value to subtract from.</param>
		/// <param name="right">The value to subtract.</param>
		/// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
		public static Int128 Subtract(Int128 left, Int128 right)
		{
			return left + (-right);
		}

		/// <summary>
		/// Performs a bitwise AND operation on two <see cref="Int128"/> values.
		/// </summary>
		/// <param name="left">The first value.</param>
		/// <param name="right">The second value.</param>
		/// <returns>The result of the bitwise AND operation.</returns>
		public static Int128 operator &(Int128 left, Int128 right)
		{
			return new Int128(left.low & right.low, left.high & right.high);
		}

		/// <summary>
		/// Performs a bitwise OR operation on two <see cref="Int128"/> values.
		/// </summary>
		/// <param name="left">The first value.</param>
		/// <param name="right">The second value.</param>
		/// <returns>The result of the bitwise OR operation.</returns>
		public static Int128 operator |(Int128 left, Int128 right)
		{
			return new Int128(left.low | right.low, left.high | right.high);
		}

		/// <summary>
		/// Performs a bitwise exclusive OR (XOR) operation on two <see cref="Int128"/> values.
		/// </summary>
		/// <param name="left">The first value.</param>
		/// <param name="right">The second value.</param>
		/// <returns>The result of the bitwise XOR operation.</returns>
		public static Int128 operator ^(Int128 left, Int128 right)
		{
			return new Int128(left.low ^ right.low, left.high ^ right.high);
		}

		/// <summary>
		/// Shifts an <see cref="Int128"/> value a specified number of bits to the left.
		/// </summary>
		/// <param name="value">The value whose bits are to be shifted.</param>
		/// <param name="shift">The number of bits to shift <paramref name="value"/> to the left.</param>
		/// <returns>A value that has been shifted to the left by the specified number of bits.</returns>
		public static Int128 operator <<(Int128 value, int shift)
		{
			if (shift == 0)
				return value;
			if (shift < 0)
				return value >> -shift;

			// Shifting more than 127 bits would shift out any bits.
			if (shift > 127)
				return 0;

			// Shifting more than 64 bits would shift all the low bits at least
			// to the high bits.
			if (shift > 63)
			{
				shift -= 64;
				value.high = unchecked((long)value.low);
				value.low = 0;
			}

			if (shift > 0)
			{
				long highbits = unchecked((long)(value.low >> (64 - shift)));
				value.low <<= shift;
				value.high <<= shift;
				value.high |= highbits;
			}

			return value;
		}

		/// <summary>
		/// Shifts an <see cref="Int128"/> value a specified number of bits to the right.
		/// </summary>
		/// <param name="value">The value whose bits are to be shifted.</param>
		/// <param name="shift">The number of bits to shift <paramref name="value"/> to the right.</param>
		/// <returns>A value that has been shifted to the right by the specified number of bits.</returns>
		public static Int128 operator >>(Int128 value, int shift)
		{
			if (shift == 0)
				return value;
			if (shift < 0)
				return value << -shift;

			// Shifting more than 127 bits would shift out any bits.
			if (shift > 127)
				return 0;

			// Shifting more than 64 bits would shift all the high bits at least
			// to the low bits.
			if (shift > 63)
			{
				shift -= 64;
				value.low = unchecked((ulong)value.high);
				value.high = 0;
			}

			if (shift > 0)
			{
				ulong lowbits = unchecked((ulong)(value.high << (64 - shift)));
				value.low >>= shift;
				value.high >>= shift;
				value.low |= lowbits;
			}

			return value;
		}

		/// <summary>
		/// Multiplies two specified <see cref="Int128"/> values.
		/// </summary>
		/// <param name="left">The first value to multiply.</param>
		/// <param name="right">The second value to multiply.</param>
		/// <returns>The product of <paramref name="left"/> and <paramref name="right"/>.</returns>
		public static Int128 operator *(Int128 left, Int128 right)
		{
			return Multiply(left, right);
		}

		/// <summary>
		/// Returns the product of two <see cref="Int128"/> values.
		/// </summary>
		/// <param name="left">The first number to multiply.</param>
		/// <param name="right">The second number to multiply.</param>
		/// <returns>The product of <paramref name="left"/> and <paramref name="right"/>.</returns>
		public static Int128 Multiply(Int128 left, Int128 right)
		{
			uint left3 = (uint)(left.high >> 32);
			uint left2 = (uint)left.high;
			uint left1 = (uint)(left.low >> 32);
			uint left0 = (uint)left.low;

			ulong right3 = (uint)(right.high >> 32);
			ulong right2 = (uint)right.high;
			ulong right1 = (uint)(right.low >> 32);
			ulong right0 = (uint)right.low;

			Int128 value00 = (Int128)(left0 * right0);
			Int128 value10 = (Int128)(left1 * right0) << 32;
			Int128 value20 = new Int128(0, (long)(left2 * right0));
			Int128 value30 = new Int128(0, (long)((left3 * right0) << 32));

			Int128 value01 = (Int128)(left0 * right1) << 32;
			Int128 value11 = new Int128(0, (long)(left1 * right1));
			Int128 value21 = new Int128(0, (long)((left2 * right1) << 32));

			Int128 value02 = new Int128(0, (long)(left0 * right2));
			Int128 value12 = new Int128(0, (long)((left1 * right2) << 32));

			Int128 value03 = new Int128(0, (long)((left0 * right3) << 32));

			return value00 + value10 + value20 + value30
				+ value01 + value11 + value21
				+ value02 + value21
				+ value03;
		}

		/// <summary>
		/// Divides a specified <see cref="Int128"/> value by another specified <see cref="Int128"/> value by using integer division.
		/// </summary>
		/// <param name="dividend">The value to be divided.</param>
		/// <param name="divisor">The value to divide by.</param>
		/// <returns>The integral result of the division.</returns>
		public static Int128 operator /(Int128 dividend, Int128 divisor)
		{
			#region Contract
			Contract.Requires<DivideByZeroException>(divisor != 0);
			#endregion

			return Divide(dividend, divisor);
		}

		/// <summary>
		/// Divides one <see cref="Int128"/> by another and returns the result.
		/// </summary>
		/// <param name="dividend">The value to be divided.</param>
		/// <param name="divisor">The value to divide by.</param>
		/// <returns>The quotient of the division.</returns>
		public static Int128 Divide(Int128 dividend, Int128 divisor)
		{
			#region Contract
			Contract.Requires<DivideByZeroException>(divisor != 0);
			#endregion

			Int128 remainder;
			return DivRem(dividend, divisor, out remainder);
		}

		/// <summary>
		/// Returns the remainder that results from division with two specified <see cref="Int128"/> values.
		/// </summary>
		/// <param name="dividend">The value to be divided.</param>
		/// <param name="divisor">The value to divide by.</param>
		/// <returns>The remainder that results from the division.</returns>
		public static Int128 operator %(Int128 dividend, Int128 divisor)
		{
			#region Contract
			Contract.Requires<DivideByZeroException>(divisor != 0);
			#endregion

			return Remainder(dividend, divisor);
		}

		/// <summary>
		/// Performs integer division on two <see cref="Int128"/> values and returns the remainder.
		/// </summary>
		/// <param name="dividend">The value to be divided.</param>
		/// <param name="divisor">The value to divide by.</param>
		/// <returns>The remainder after dividing <paramref name="dividend"/> by <paramref name="divisor"/>.</returns>
		public static Int128 Remainder(Int128 dividend, Int128 divisor)
		{
			#region Contract
			Contract.Requires<DivideByZeroException>(divisor != 0);
			#endregion

			Int128 remainder;
			DivRem(dividend, divisor, out remainder);
			return remainder;
		}
		#endregion

		/// <summary>
		/// Gets the absolute value of an <see cref="Int128"/>.
		/// </summary>
		/// <param name="value">A value.</param>
		/// <returns>The absolute value of <paramref name="value"/>.</returns>
		public static Int128 Abs(Int128 value)
		{
			Int128 result = value;
			if (result < 0)
				result = -result;
			return result;
		}

		/// <summary>
		/// Divides one <see cref="Int128"/> value by another, using signed integer division, and returns the result
		/// and the remainder.
		/// </summary>
		/// <param name="dividend">The value to be divided.</param>
		/// <param name="divisor">The value to divide by.</param>
		/// <param name="remainder">The remainder from the division.</param>
		/// <returns>The quotient of the division.</returns>
		public static Int128 DivRem(Int128 dividend, Int128 divisor, out Int128 remainder)
		{
			Int128 quotient;

			int remainderSign = 1;
			if (dividend < 0)
			{
				dividend = -dividend;
				remainderSign = -1;
			}

			int quotientSign = 1;
			if (divisor < 0)
			{
				divisor = -divisor;
				quotientSign = -1;
			}
			quotientSign *= remainderSign;

			quotient = UnsignedDivRem(dividend, divisor, out remainder);

			quotient *= quotientSign;
			remainder *= remainderSign;

			return quotient;
		}

		/// <summary>
		/// Divides one <see cref="Int128"/> value by another, using unsigned integer division, and returns the result
		/// and the remainder.
		/// </summary>
		/// <param name="dividend">The value to be divided.</param>
		/// <param name="divisor">The value to divide by.</param>
		/// <param name="remainder">The remainder from the division.</param>
		/// <returns>The quotient of the division.</returns>
		public static Int128 UnsignedDivRem(Int128 dividend, Int128 divisor, out Int128 remainder)
		{
			Int128 quotient = dividend;
			remainder = 0;
			for (int i = 0; i < 128; i++)
			{
				remainder <<= 1;
				if (quotient < 0)
					remainder.low |= 1;
				quotient <<= 1;

				if (remainder >= divisor)
				{
					remainder -= divisor;
					quotient++;
				}
			}

			return quotient;
		}

		/// <summary>
		/// Returns the larger of two <see cref="Int128"/> values.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>The <paramref name="left"/> or <paramref name="right"/> parameter, whichever is larger.</returns>
		public static Int128 Max(Int128 left, Int128 right)
		{
			Int128 result = left;
			if (right > left)
				result = right;
			return result;
		}

		/// <summary>
		/// Returns the smaller of two <see cref="Int128"/> values.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>The <paramref name="left"/> or <paramref name="right"/> parameter, whichever is smaller.</returns>
		public static Int128 Min(Int128 left, Int128 right)
		{
			Int128 result = left;
			if (right < left)
				result = right;
			return result;
		}

		/// <summary>
		/// Calculates the padding required to align the value to the next specified boundary.
		/// </summary>
		/// <param name="boundary">The boundary to align to, which must be a power of two.</param>
		/// <returns>The number of padding bytes required to align the address to the specified boundary.</returns>
		public Int128 GetPadding(int boundary)
		{
            return (boundary + ((this - 1) & ~(boundary - 1))) - this;
		}

		/// <summary>
		/// Aligns the value to the next specified boundary.
		/// </summary>
		/// <param name="boundary">The boundary to align to, which must be a power of two.</param>
		/// <returns>The address aligned to the specified boundary.</returns>
		public Int128 Align(int boundary)
		{
            return (boundary + ((this - 1) & ~(boundary - 1)));
		}
		#endregion

		#region Strings
		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format"><para>The format to use.</para>
		/// -or-
		/// <para><see langword="null"/> to use the default format defined for the type of the
		/// <see cref="IFormattable"/> implementation.</para></param>
		/// <param name="formatProvider"><para>The provider to use to format the value.</para>
		/// -or-
		/// <para><see langword="null"/> to obtain the numeric format information from the current locale setting of
		/// the operating system.</para></param>
		/// <returns>The value of the current instance in the specified format.</returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			// TODO: Implement.
			return ToString();
		}
		#endregion

		#region Fields
		/// <summary>
		/// The low part of the integer.
		/// </summary>
		private ulong low;
		/// <summary>
		/// The high part of the integer.
		/// </summary>
		private long high;
		#endregion
	}
}
