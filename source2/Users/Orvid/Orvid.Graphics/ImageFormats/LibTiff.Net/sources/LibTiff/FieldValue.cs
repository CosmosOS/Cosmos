/* 
 * Copyright (C) 2008-2011, Bit Miracle
 * http://www.bitmiracle.com
 * 
 */
using System;

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// Holds a value of a Tiff tag.
    /// </summary>
    /// <remarks>
    /// <para>Simply put, it is a wrapper around System.Object, that helps to deal with
    /// unboxing and conversion of types a bit easier.
    /// </para><para>
    /// Please take a look at:
    /// http://blogs.msdn.com/ericlippert/archive/2009/03/19/representation-and-identity.aspx
    /// </para></remarks>
#if EXPOSE_LIBTIFF
    public
#endif
    struct FieldValue
    {
        private object m_value;
        
        internal FieldValue(object o)
        {
            m_value = o;
        }

        static internal FieldValue[] FromParams(params object[] list)
        {
            FieldValue[] values = new FieldValue[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] is FieldValue)
                    values[i] = new FieldValue(((FieldValue)(list[i])).Value);
                else
                    values[i] = new FieldValue(list[i]);
            }

            return values;
        }

        internal void Set(object o)
        {
            m_value = o;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get { return m_value; }
        }

        /// <summary>
        /// Retrieves value converted to byte.
        /// </summary>
        /// <returns>The value converted to byte.</returns>
        public byte ToByte()
        {
            return Convert.ToByte(m_value);
        }

        /// <summary>
        /// Retrieves value converted to short.
        /// </summary>
        /// <returns>The value converted to short.</returns>
        public short ToShort()
        {
            return Convert.ToInt16(m_value);
        }

        /// <summary>
        /// Retrieves value converted to ushort.
        /// </summary>
        /// <returns>The value converted to ushort.</returns>
#if EXPOSE_LIBTIFF
        [CLSCompliant(false)]
#endif
        public ushort ToUShort()
        {
            return Convert.ToUInt16(m_value);
        }

        /// <summary>
        /// Retrieves value converted to int.
        /// </summary>
        /// <returns>The value converted to int.</returns>
        public int ToInt()
        {
            return Convert.ToInt32(m_value);
        }

        /// <summary>
        /// Retrieves value converted to uint.
        /// </summary>
        /// <returns>The value converted to uint.</returns>
#if EXPOSE_LIBTIFF
        [CLSCompliant(false)]
#endif
        public uint ToUInt()
        {
            return Convert.ToUInt32(m_value);
        }

        /// <summary>
        /// Retrieves value converted to float.
        /// </summary>
        /// <returns>The value converted to float.</returns>
        public float ToFloat()
        {
            return Convert.ToSingle(m_value);
        }

        /// <summary>
        /// Retrieves value converted to double.
        /// </summary>
        /// <returns>The value converted to double.</returns>
        public double ToDouble()
        {
            return Convert.ToDouble(m_value);
        }

        /// <summary>
        /// Retrieves value converted to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.        
        /// </returns>
        /// <remarks>If value is a byte array, then it gets converted to string using
        /// Latin1 encoding encoder.</remarks>
        public override string ToString()
        {
            if (m_value is byte[])
                return Tiff.Latin1Encoding.GetString(m_value as byte[]);

            return Convert.ToString(m_value);
        }

        /// <summary>
        /// Retrieves value converted to byte array.
        /// </summary>
        /// <returns>Value converted to byte array.</returns>
        /// <remarks>
        /// <para>If value is byte array then it retrieved unaltered.</para>
        /// <para>If value is array of short, ushort, int, uint, float or double values then this
        /// array is converted to byte array</para><para>
        /// If value is a string then it gets converted to byte array using Latin1 encoding
        /// encoder.</para><para>
        /// If value is of any other type then <c>null</c> is returned.</para>
        /// </remarks>
        public byte[] GetBytes()
        {
            if (m_value == null)
                return null;

            Type t = m_value.GetType();
            if (t.IsArray)
            {
                if (m_value is byte[])
                    return m_value as byte[];
                else if (m_value is short[])
                {
                    short[] temp = m_value as short[];
                    byte[] result = new byte[temp.Length * sizeof(short)];
                    Buffer.BlockCopy(temp, 0, result, 0, result.Length);
                    return result;
                }
                else if (m_value is ushort[])
                {
                    ushort[] temp = m_value as ushort[];
                    byte[] result = new byte[temp.Length * sizeof(ushort)];
                    Buffer.BlockCopy(temp, 0, result, 0, result.Length);
                    return result;
                }
                else if (m_value is int[])
                {
                    int[] temp = m_value as int[];
                    byte[] result = new byte[temp.Length * sizeof(int)];
                    Buffer.BlockCopy(temp, 0, result, 0, result.Length);
                    return result;
                }
                else if (m_value is uint[])
                {
                    uint[] temp = m_value as uint[];
                    byte[] result = new byte[temp.Length * sizeof(uint)];
                    Buffer.BlockCopy(temp, 0, result, 0, result.Length);
                    return result;
                }
                else if (m_value is float[])
                {
                    float[] temp = m_value as float[];
                    byte[] result = new byte[temp.Length * sizeof(float)];
                    Buffer.BlockCopy(temp, 0, result, 0, result.Length);
                    return result;
                }
                else if (m_value is double[])
                {
                    double[] temp = m_value as double[];
                    byte[] result = new byte[temp.Length * sizeof(double)];
                    Buffer.BlockCopy(temp, 0, result, 0, result.Length);
                    return result;
                }
            }
            else if (m_value is string)
            {
                return Tiff.Latin1Encoding.GetBytes(m_value as string);
            }

            return null;
        }

        /// <summary>
        /// Retrieves value converted to array of bytes.
        /// </summary>
        /// <returns>Value converted to array of bytes.</returns>
        /// <remarks><para>If value is array of bytes then it retrieved unaltered.</para>
        /// <para>If value is array of short, ushort, int or uint values then each element of
        /// field value gets converted to byte and added to resulting array.</para>
        /// <para>If value is string then it gets converted to byte[] using Latin1 encoding
        /// encoder.</para><para>
        /// If value is of any other type then null is returned.</para></remarks>
        public byte[] ToByteArray()
        {
            if (m_value == null)
                return null;

            Type t = m_value.GetType();
            if (t.IsArray)
            {
                if (m_value is byte[])
                    return m_value as byte[];
                else if (m_value is short[])
                {
                    short[] temp = m_value as short[];
                    byte[] result = new byte[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (byte)temp[i];

                    return result;
                }
                else if (m_value is ushort[])
                {
                    ushort[] temp = m_value as ushort[];
                    byte[] result = new byte[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (byte)temp[i];

                    return result;
                }
                else if (m_value is int[])
                {
                    int[] temp = m_value as int[];
                    byte[] result = new byte[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (byte)temp[i];

                    return result;
                }
                else if (m_value is uint[])
                {
                    uint[] temp = m_value as uint[];
                    byte[] result = new byte[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (byte)temp[i];

                    return result;
                }
            }
            else if (m_value is string)
                return Tiff.Latin1Encoding.GetBytes(m_value as string);

            return null;
        }

        /// <summary>
        /// Retrieves value converted to array of short values.
        /// </summary>
        /// <returns>Value converted to array of short values.</returns>
        /// <remarks><para>If value is array of short values then it retrieved unaltered.</para>
        /// <para>If value is array of bytes then each pair of bytes is converted to short and
        /// added to resulting array. If value contains odd amount of bytes, then null is
        /// returned.</para><para>
        /// If value is array of ushort, int or uint values then each element of field value gets
        /// converted to short and added to resulting array.</para><para>
        /// If value is of any other type then null is returned.</para></remarks>
        public short[] ToShortArray()
        {
            if (m_value == null)
                return null;

            Type t = m_value.GetType();
            if (t.IsArray)
            {
                if (m_value is short[])
                    return m_value as short[];
                else if (m_value is byte[])
                {
                    byte[] temp = m_value as byte[];
                    if (temp.Length % sizeof(short) != 0)
                        return null;

                    int totalShorts = temp.Length / sizeof(short);
                    short[] result = new short[totalShorts];

                    int byteOffset = 0;
                    for (int i = 0; i < totalShorts; i++)
                    {
                        short s = BitConverter.ToInt16(temp, byteOffset);
                        result[i] = s;
                        byteOffset += sizeof(short);
                    }

                    return result;
                }
                else if (m_value is ushort[])
                {
                    ushort[] temp = m_value as ushort[];
                    short[] result = new short[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (short)temp[i];

                    return result;
                }
                else if (m_value is int[])
                {
                    int[] temp = m_value as int[];
                    short[] result = new short[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (short)temp[i];

                    return result;
                }
                else if (m_value is uint[])
                {
                    uint[] temp = m_value as uint[];
                    short[] result = new short[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (short)temp[i];

                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves value converted to array of ushort values.
        /// </summary>
        /// <returns>Value converted to array of ushort values.</returns>
        /// <remarks><para>If value is array of ushort values then it retrieved unaltered.</para>
        /// <para>If value is array of bytes then each pair of bytes is converted to ushort and
        /// added to resulting array. If value contains odd amount of bytes, then null is
        /// returned.</para><para>
        /// If value is array of short, int or uint values then each element of field value gets
        /// converted to ushort and added to resulting array.</para><para>
        /// If value is of any other type then null is returned.</para></remarks>
#if EXPOSE_LIBTIFF
        [CLSCompliant(false)]
#endif
        public ushort[] ToUShortArray()
        {
            if (m_value == null)
                return null;

            Type t = m_value.GetType();
            if (t.IsArray)
            {
                if (m_value is ushort[])
                    return m_value as ushort[];
                else if (m_value is byte[])
                {
                    byte[] temp = m_value as byte[];
                    if (temp.Length % sizeof(ushort) != 0)
                        return null;

                    int totalUShorts = temp.Length / sizeof(ushort);
                    ushort[] result = new ushort[totalUShorts];

                    int byteOffset = 0;
                    for (int i = 0; i < totalUShorts; i++)
                    {
                        ushort s = BitConverter.ToUInt16(temp, byteOffset);
                        result[i] = s;
                        byteOffset += sizeof(ushort);
                    }

                    return result;
                }
                else if (m_value is short[])
                {
                    short[] temp = m_value as short[];
                    ushort[] result = new ushort[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (ushort)temp[i];

                    return result;
                }
                else if (m_value is int[])
                {
                    int[] temp = m_value as int[];
                    ushort[] result = new ushort[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (ushort)temp[i];

                    return result;
                }
                else if (m_value is uint[])
                {
                    uint[] temp = m_value as uint[];
                    ushort[] result = new ushort[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (ushort)temp[i];

                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves value converted to array of int values.
        /// </summary>
        /// <returns>Value converted to array of int values.</returns>
        /// <remarks><para>If value is array of int values then it retrieved unaltered.</para>
        /// <para>If value is array of bytes then each 4 bytes are converted to int and added to
        /// resulting array. If value contains amount of bytes that can't be divided by 4 without
        /// remainder, then null is returned.</para>
        /// <para>If value is array of short, ushort or uint values then each element of
        /// field value gets converted to int and added to resulting array.</para><para>
        /// If value is of any other type then null is returned.</para></remarks>
        public int[] ToIntArray()
        {
            if (m_value == null)
                return null;

            Type t = m_value.GetType();
            if (t.IsArray)
            {
                if (m_value is int[])
                    return m_value as int[];
                else if (m_value is byte[])
                {
                    byte[] temp = m_value as byte[];
                    if (temp.Length % sizeof(int) != 0)
                        return null;

                    int totalInts = temp.Length / sizeof(int);
                    int[] result = new int[totalInts];

                    int byteOffset = 0;
                    for (int i = 0; i < totalInts; i++)
                    {
                        int s = BitConverter.ToInt32(temp, byteOffset);
                        result[i] = s;
                        byteOffset += sizeof(int);
                    }

                    return result;
                }
                else if (m_value is short[])
                {
                    short[] temp = m_value as short[];
                    int[] result = new int[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (int)temp[i];

                    return result;
                }
                else if (m_value is ushort[])
                {
                    ushort[] temp = m_value as ushort[];
                    int[] result = new int[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (int)temp[i];

                    return result;
                }
                else if (m_value is uint[])
                {
                    uint[] temp = m_value as uint[];
                    int[] result = new int[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (int)temp[i];

                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves value converted to array of uint values.
        /// </summary>
        /// <returns>Value converted to array of uint values.</returns>
        /// <remarks><para>If value is array of uint values then it retrieved unaltered.</para>
        /// <para>If value is array of bytes then each 4 bytes are converted to uint and added to
        /// resulting array. If value contains amount of bytes that can't be divided by 4 without
        /// remainder, then null is returned.</para>
        /// <para>If value is array of short, ushort or int values then each element of
        /// field value gets converted to uint and added to resulting array.</para><para>
        /// If value is of any other type then null is returned.</para></remarks>
#if EXPOSE_LIBTIFF
        [CLSCompliant(false)]
#endif
        public uint[] ToUIntArray()
        {
            if (m_value == null)
                return null;

            Type t = m_value.GetType();
            if (t.IsArray)
            {
                if (m_value is uint[])
                    return m_value as uint[];
                else if (m_value is byte[])
                {
                    byte[] temp = m_value as byte[];
                    if (temp.Length % sizeof(uint) != 0)
                        return null;

                    int totalUInts = temp.Length / sizeof(uint);
                    uint[] result = new uint[totalUInts];

                    int byteOffset = 0;
                    for (int i = 0; i < totalUInts; i++)
                    {
                        uint s = BitConverter.ToUInt32(temp, byteOffset);
                        result[i] = s;
                        byteOffset += sizeof(uint);
                    }

                    return result;
                }
                else if (m_value is short[])
                {
                    short[] temp = m_value as short[];
                    uint[] result = new uint[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (uint)temp[i];

                    return result;
                }
                else if (m_value is ushort[])
                {
                    ushort[] temp = m_value as ushort[];
                    uint[] result = new uint[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (uint)temp[i];

                    return result;
                }
                else if (m_value is int[])
                {
                    int[] temp = m_value as int[];
                    uint[] result = new uint[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (uint)temp[i];

                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves value converted to array of float values.
        /// </summary>
        /// <returns>Value converted to array of float values.</returns>
        /// <remarks><para>If value is array of float values then it retrieved unaltered.</para>
        /// <para>If value is array of bytes then each 4 bytes are converted to float and added to
        /// resulting array. If value contains amount of bytes that can't be divided by 4 without
        /// remainder, then null is returned.</para>
        /// <para>If value is array of double values then each element of field value gets
        /// converted to float and added to resulting array.</para><para>
        /// If value is of any other type then null is returned.</para></remarks>
        public float[] ToFloatArray()
        {
            if (m_value == null)
                return null;

            Type t = m_value.GetType();
            if (t.IsArray)
            {
                if (m_value is float[])
                    return m_value as float[];
                else if (m_value is double[])
                {
                    double[] temp = m_value as double[];
                    float[] result = new float[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (float)temp[i];

                    return result;
                }
                else if (m_value is byte[])
                {
                    byte[] temp = m_value as byte[];
                    if (temp.Length % sizeof(float) != 0)
                        return null;

                    int tempPos = 0; 
                    
                    int floatCount = temp.Length / sizeof(float);
                    float[] result = new float[floatCount];
                    
                    for (int i = 0; i < floatCount; i++)
                    {
                        float f = BitConverter.ToSingle(temp, tempPos);
                        result[i] = f;
                        tempPos += sizeof(float);
                    }

                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves value converted to array of double values.
        /// </summary>
        /// <returns>Value converted to array of double values.</returns>
        /// <remarks><para>If value is array of double values then it retrieved unaltered.</para>
        /// <para>If value is array of bytes then each 8 bytes are converted to double and added to
        /// resulting array. If value contains amount of bytes that can't be divided by 8 without
        /// remainder, then null is returned.</para>
        /// <para>If value is array of float values then each element of field value gets
        /// converted to double and added to resulting array.</para><para>
        /// If value is of any other type then null is returned.</para></remarks>
        public double[] ToDoubleArray()
        {
            if (m_value == null)
                return null;

            Type t = m_value.GetType();
            if (t.IsArray)
            {
                if (m_value is double[])
                    return m_value as double[];
                else if (m_value is float[])
                {
                    float[] temp = m_value as float[];
                    double[] result = new double[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                        result[i] = (double)temp[i];

                    return result;
                }
                else if (m_value is byte[])
                {
                    byte[] temp = m_value as byte[];
                    if (temp.Length % sizeof(double) != 0)
                        return null;

                    int tempPos = 0;

                    int floatCount = temp.Length / sizeof(double);
                    double[] result = new double[floatCount];

                    for (int i = 0; i < floatCount; i++)
                    {
                        double d = BitConverter.ToDouble(temp, tempPos);
                        result[i] = d;
                        tempPos += sizeof(double);
                    }

                    return result;
                }
            }

            return null;
        }
    }
}
