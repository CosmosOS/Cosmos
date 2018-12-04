using System;
using System.Text;

namespace Cosmos.System.FileSystem.FAT
{
    internal abstract class DataStructure
    {
        protected DataStructure(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        protected byte[] Data { get; }

        public T GetValue<T>(Field<T> field) => field.GetValue(Data);

        public void SetValue<T>(Field<T> field, T value) => field.SetValue(Data, value);
    }

    internal abstract class Field<T>
    {
        public Field(int position, int length)
        {
            Position = position;
            Length = length;
        }

        /// <summary>
        /// Position, in bytes, of the field.
        /// </summary>
        public int Position { get; }
        /// <summary>
        /// Length, in bytes, of the field.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the value for this field from the data structure.
        /// </summary>
        /// <param name="data">A byte array which contains a data structure for which this field was defined.</param>
        /// <returns>The field value.</returns>
        public abstract T GetValue(byte[] data);

        /// <summary>
        /// Sets the value for this field on the data structure.
        /// </summary>
        /// <param name="data">A byte array which contains a data structure for which this field was defined.</param>
        /// <param name="value">The value to set this field to.</param>
        public abstract void SetValue(byte[] data, T value);
    }

    internal class ByteField : Field<byte>
    {
        public ByteField(int position)
            : base(position, 1)
        {
        }

        public override byte GetValue(byte[] data) => data[Position];

        public override void SetValue(byte[] data, byte value) => data[Position] = value;
    }

    internal class UInt16Field : Field<ushort>
    {
        public UInt16Field(int position)
            : base(position, 2)
        {
        }

        public override ushort GetValue(byte[] data) => BitConverter.ToUInt16(data, Position);

        public override void SetValue(byte[] data, ushort value)
        {
            data[Position] = (byte)value;
            data[Position + 1] = (byte)(value >> 8);
        }
    }

    internal class Int32Field : Field<int>
    {
        public Int32Field(int position)
            : base(position, 4)
        {
        }

        public override int GetValue(byte[] data) => BitConverter.ToInt32(data, Position);

        public override void SetValue(byte[] data, int value)
        {
            data[Position] = (byte)value;
            data[Position + 1] = (byte)(value >> 8);
            data[Position + 2] = (byte)(value >> 16);
            data[Position + 3] = (byte)(value >> 24);
        }
    }

    internal class UInt32Field : Field<uint>
    {
        public UInt32Field(int position)
            : base(position, 4)
        {
        }

        public override uint GetValue(byte[] data) => BitConverter.ToUInt32(data, Position);

        public override void SetValue(byte[] data, uint value)
        {
            data[Position] = (byte)value;
            data[Position + 1] = (byte)(value >> 8);
            data[Position + 2] = (byte)(value >> 16);
            data[Position + 3] = (byte)(value >> 24);
        }
    }

    internal class StringField : Field<string>
    {
        public StringField(int position, int length, Encoding encoding)
            : base(position, length)
        {
            Encoding = encoding;
        }

        public Encoding Encoding { get; }

        public override string GetValue(byte[] data) => Encoding.GetString(data, Position, Length);

        public override void SetValue(byte[] data, string value) => Encoding.GetBytes(value, 0, Length, data, Position);
    }

    internal class EnumField<TEnum, TEnumUnderlyingType> : Field<TEnum>
    {
        private static readonly EnumField<Fat32ExtendedFlags, int> test = new EnumField<Fat32ExtendedFlags, int>(new Int32Field(12));

        private readonly Field<TEnumUnderlyingType> _innerField;

        public EnumField(Field<TEnumUnderlyingType> innerField)
            : base(innerField.Position, innerField.Length)
        {
            _innerField = innerField;
        }

        public override TEnum GetValue(byte[] data) => (TEnum)(object)_innerField.GetValue(data);

        public override void SetValue(byte[] data, TEnum value) => _innerField.SetValue(data, (TEnumUnderlyingType)(object)value);
    }
}
