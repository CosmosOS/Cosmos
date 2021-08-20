using System;
using System.Runtime.CompilerServices;

namespace System
{
    public class Object { public IntPtr m_pEEType; } // The layout of object is a contract with the compiler.
    public struct Void { }
    public struct Boolean { }
    public struct Char { }
    public struct SByte { }
    public struct Byte { }
    public struct Int16 { }
    public struct UInt16 { }
    public struct Int32 { }
    public struct UInt32 { }
    public struct Int64 { }
    public struct UInt64 { }
    public unsafe struct IntPtr
    {
        void* _value;

        public IntPtr(void* value) { _value = value; }
        public IntPtr(int value) { _value = (void*)value; }
        public IntPtr(uint value) { _value = (void*)value; }
        public IntPtr(long value) { _value = (void*)value; }
        public IntPtr(ulong value) { _value = (void*)value; }

        [Intrinsic]
        public static readonly IntPtr Zero;

        //public override bool Equals(object o)
        //	=> _value == ((IntPtr)o)._value;

        public bool Equals(IntPtr ptr)
            => _value == ptr._value;

        //public override int GetHashCode()
        //	=> (int)_value;

        public static explicit operator IntPtr(int value) => new IntPtr(value);
        public static explicit operator IntPtr(uint value) => new IntPtr(value);
        public static explicit operator IntPtr(long value) => new IntPtr(value);
        public static explicit operator IntPtr(ulong value) => new IntPtr(value);
        public static explicit operator IntPtr(void* value) => new IntPtr(value);
        public static explicit operator void*(IntPtr value) => value._value;

        public static explicit operator int(IntPtr value)
        {
            var l = (long)value._value;

            return checked((int)l);
        }

        public static explicit operator long(IntPtr value) => (long)value._value;
        public static explicit operator ulong(IntPtr value) => (ulong)value._value;

        public static IntPtr operator +(IntPtr a, uint b)
            => new IntPtr((byte*)a._value + b);

        public static IntPtr operator +(IntPtr a, ulong b)
            => new IntPtr((byte*)a._value + b);
    }
    public struct UIntPtr { }
    public struct Single { }
    public struct Double { }
    public abstract class ValueType { }
    public abstract class Enum : ValueType { }
    public struct Nullable<T> where T : struct { }

    public sealed class String { public readonly int Length; }
    public abstract class Array { }
    public abstract class Delegate { }
    public abstract class MulticastDelegate : Delegate { }

    public struct RuntimeTypeHandle { }
    public struct RuntimeMethodHandle { }
    public struct RuntimeFieldHandle { }

    public class Attribute { }

    public sealed class FlagsAttribute : Attribute { }

    namespace Runtime.CompilerServices
    {
        public class RuntimeHelpers
        {
            public static unsafe int OffsetToStringData => sizeof(IntPtr) + sizeof(int);
        }

        public static class RuntimeFeature
        {
            public const string UnmanagedSignatureCallingConvention = nameof(UnmanagedSignatureCallingConvention);
        }

        internal sealed class IntrinsicAttribute : Attribute { }
    }
}

namespace System.Runtime.InteropServices
{
    public class UnmanagedType { }

    sealed class StructLayoutAttribute : Attribute
    {
        public StructLayoutAttribute(LayoutKind layoutKind)
        {
        }
    }

    public sealed class FieldOffsetAttribute : Attribute
    {
        public FieldOffsetAttribute(int offset)
        {
            Value = offset;
        }

        public int Value { get; }
    }

    internal enum LayoutKind
    {
        Sequential = 0, // 0x00000008,
        Explicit = 2, // 0x00000010,
        Auto = 3, // 0x00000000,
    }

    internal enum CharSet
    {
        None = 1,       // User didn't specify how to marshal strings.
        Ansi = 2,       // Strings should be marshalled as ANSI 1 byte chars.
        Unicode = 3,    // Strings should be marshalled as Unicode 2 byte chars.
        Auto = 4,       // Marshal Strings in the right way for the target system.
    }
}

#region Things needed by ILC
namespace System
{
    namespace Runtime
    {
        internal sealed class RuntimeExportAttribute : Attribute
        {
            public RuntimeExportAttribute(string entry) { }
        }
    }

    class Array<T> : Array { }
}

namespace Internal.Runtime.CompilerHelpers
{
    using System.Runtime;
    using Internal.Runtime.CompilerServices;
    using Kernel;

    class StartupCodeHelpers
    {
        [RuntimeExport("RhpReversePInvoke2")]
        static void RhpReversePInvoke2() { }
        [RuntimeExport("RhpReversePInvokeReturn2")]
        static void RhpReversePInvokeReturn2() { }
        [RuntimeExport("__fail_fast")]
        static void FailFast() { while (true) ; }
        [RuntimeExport("RhpPInvoke")]
        static void RphPinvoke() { }
        [RuntimeExport("RhpPInvokeReturn")]
        static void RphPinvokeReturn() { }
        [RuntimeExport("RhpNewFast")]
        static unsafe object RhpNewFast(EEType* pEEType)
        {
            var size = pEEType->BaseSize;

            // Round to next power of 8
            if (size % 8 > 0)
                size = ((size / 8) + 1) * 8;

            var data = Memory.Alloc(size);
            var obj = Unsafe.As<IntPtr, object>(ref data);
            Memory.Zero(data, size);
            SetEEType(data, pEEType);

            return obj;
        }
        [RuntimeExport("RhpAssignRef")]
        static unsafe void RhpAssignRef(void** address, void* obj)
        {
            *address = obj;
        }

        internal static unsafe void SetEEType(IntPtr obj, EEType* type)
        {
            Memory.Copy(obj, (IntPtr)(&type), (ulong)sizeof(IntPtr));
        }
    }
}

namespace Internal.Runtime.CompilerServices
{
    public static unsafe class Unsafe
    {
        [Intrinsic]
        public static extern ref T Add<T>(ref T source, int elementOffset);

        [Intrinsic]
        public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

        [Intrinsic]
        public static extern void* AsPointer<T>(ref T value);

        [Intrinsic]
        public static extern ref T AsRef<T>(void* pointer);

        public static ref T AsRef<T>(IntPtr pointer)
            => ref AsRef<T>((void*)pointer);

        [Intrinsic]
        public static extern int SizeOf<T>();
    }
}
#endregion
