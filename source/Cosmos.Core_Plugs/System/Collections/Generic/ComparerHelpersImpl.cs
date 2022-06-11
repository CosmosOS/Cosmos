using System;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Collections.Generic;

[Plug("System.Collections.Generic.ComparerHelpers, System.Private.CoreLib")]
public static class ComparerHelpersImpl
{
    private static readonly Debugger mDebugger = new("Core", "ComparerHelpersImpl");

    public static object CreateDefaultComparer(Type aType)
    {
        //TODO: Do application level testing to determine the most frequent comparisons and do those type checks first.

        if (aType == typeof(byte))
        {
            return new ByteComparer();
        }

        //if (aType == typeof(Byte?))
        //{
        //    return new NullableByteComparer();
        //}

        if (aType == typeof(sbyte))
        {
            return new SByteComparer();
        }

        //if (aType == typeof(SByte?))
        //{
        //    return new NullableSByteComparer();
        //}

        if (aType == typeof(string))
        {
            return new StringComparer();
        }

        if (aType == typeof(int))
        {
            return new Int32Comparer();
        }

        //if (aType == typeof(Int32?))
        //{
        //    return new NullableInt32Comparer();
        //}

        if (aType == typeof(uint))
        {
            return new UInt32Comparer();
        }

        //if (aType == typeof(UInt32?))
        //{
        //    return new NullableUInt32Comparer();
        //}

        if (aType == typeof(long))
        {
            return new Int64Comparer();
        }

        //if (aType == typeof(Int64?))
        //{
        //    return new NullableInt64Comparer();
        //}

        if (aType == typeof(ulong))
        {
            return new UInt64Comparer();
        }

        //if (aType == typeof(UInt64?))
        //{
        //    return new NullableUInt64Comparer();
        //}

        if (aType == typeof(char))
        {
            return new CharComparer();
        }

        //if (aType == typeof(Char?))
        //{
        //    return new NullableCharComparer();
        //}

        if (aType == typeof(short))
        {
            return new Int16Comparer();
        }

        //if (aType == typeof(Int16?))
        //{
        //    return new NullableInt16Comparer();
        //}

        if (aType == typeof(ushort))
        {
            return new UInt16Comparer();
        }

        //if (aType == typeof(UInt16?))
        //{
        //    return new NullableUInt16Comparer();
        //}

        if (aType == typeof(Guid))
        {
            return new GuidComparer();
        }

        //if (aType == typeof(Guid?))
        //{
        //    return new NullableGuidComparer();
        //}

        if (aType.IsEnum)
        {
            switch (Type.GetTypeCode(Enum.GetUnderlyingType(aType)))
            {
                case TypeCode.SByte:
                    return new SByteComparer();
                case TypeCode.Byte:
                    return new ByteComparer();
                case TypeCode.Int16:
                    return new Int16Comparer();
                case TypeCode.UInt16:
                    return new UInt16Comparer();
                case TypeCode.Int32:
                    return new Int32Comparer();
                case TypeCode.UInt32:
                    return new UInt32Comparer();
                case TypeCode.Int64:
                    return new Int64Comparer();
                case TypeCode.UInt64:
                    return new UInt64Comparer();
                default:
                    return null;
            }
        }

        //MS framework falls back to address compare so we'll do the same.
        //mDebugger.Send($"No Comparer for type {aType}");
        return new ObjectComparer();
    }

    public static object CreateDefaultEqualityComparer(Type aType)
    {
        //TODO: Do application level testing to determine the most frequent comparisons and do those type checks first.

        if (aType == typeof(byte))
        {
            return new ByteEqualityComparer();
        }

        //if (aType == typeof(Byte?))
        //{
        //    return new NullableByteEqualityComparer();
        //}

        if (aType == typeof(sbyte))
        {
            return new SByteEqualityComparer();
        }

        //if (aType == typeof(SByte?))
        //{
        //    return new NullableSByteEqualityComparer();
        //}

        if (aType == typeof(string))
        {
            return new StringEqualityComparer();
        }

        if (aType == typeof(int))
        {
            return new Int32EqualityComparer();
        }

        //if (aType == typeof(Int32?))
        //{
        //    return new NullableInt32EqualityComparer();
        //}

        if (aType == typeof(uint))
        {
            return new UInt32EqualityComparer();
        }

        //if (aType == typeof(UInt32?))
        //{
        //    return new NullableUInt32EqualityComparer();
        //}

        if (aType == typeof(long))
        {
            return new Int64EqualityComparer();
        }

        //if (aType == typeof(Int64?))
        //{
        //    return new NullableInt64EqualityComparer();
        //}

        if (aType == typeof(ulong))
        {
            return new UInt64EqualityComparer();
        }

        //if (aType == typeof(UInt64?))
        //{
        //    return new NullableUInt64EqualityComparer();
        //}

        if (aType == typeof(char))
        {
            return new CharEqualityComparer();
        }

        //if (aType == typeof(Char?))
        //{
        //    return new NullableCharEqualityComparer();
        //}

        if (aType == typeof(short))
        {
            return new Int16EqualityComparer();
        }

        //if (aType == typeof(Int16?))
        //{
        //    return new NullableInt16EqualityComparer();
        //}

        if (aType == typeof(ushort))
        {
            return new UInt16EqualityComparer();
        }

        //if (aType == typeof(UInt16?))
        //{
        //    return new NullableUInt16EqualityComparer();
        //}

        if (aType == typeof(Guid))
        {
            return new GuidEqualityComparer();
        }

        //if (aType == typeof(Guid?))
        //{
        //    return new NullableGuidEqualityComparer();
        //}

        //if (aType.IsEnum)
        //{
        //    switch (Type.GetTypeCode(Enum.GetUnderlyingType(aType)))
        //    {
        //        case TypeCode.SByte:
        //            return new SByteEqualityComparer();
        //        case TypeCode.Byte:
        //            return new ByteEqualityComparer();
        //        case TypeCode.Int16:
        //            return new Int16EqualityComparer();
        //        case TypeCode.UInt16:
        //            return new UInt16EqualityComparer();
        //        case TypeCode.Int32:
        //            return new Int32EqualityComparer();
        //        case TypeCode.UInt32:
        //            return new UInt32EqualityComparer();
        //        case TypeCode.Int64:
        //            return new Int64EqualityComparer();
        //        case TypeCode.UInt64:
        //            return new UInt64EqualityComparer();
        //        default:
        //            return null;
        //    }
        //}

        //MS framework falls back to address compare so we'll do the same.
        //mDebugger.Send($"No EqualityComparer for type {aType}");
        //return null;
        return new ObjectEqualityComparer();
    }
}

#region "Comparer"

public class StringComparer : Comparer<string>
{
    public override int Compare(string x, string y) => String.Compare(x, y);
}

public class CharComparer : Comparer<char>
{
    public override int Compare(char x, char y) => x.CompareTo(y);
}

//public class NullableCharComparer : Comparer<Char?>
//{
//    public override int Compare(Char? x, Char? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class ByteComparer : Comparer<byte>
{
    public override int Compare(byte x, byte y) => x.CompareTo(y);
}

//public class NullableByteComparer : Comparer<Byte?>
//{
//    public override int Compare(Byte? x, Byte? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class SByteComparer : Comparer<sbyte>
{
    public override int Compare(sbyte x, sbyte y) => x.CompareTo(y);
}

//public class NullableSByteComparer : Comparer<SByte?>
//{
//    public override int Compare(SByte? x, SByte? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class Int16Comparer : Comparer<short>
{
    public override int Compare(short x, short y) => x.CompareTo(y);
}

//public class NullableInt16Comparer : Comparer<Int16?>
//{
//    public override int Compare(Int16? x, Int16? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class UInt16Comparer : Comparer<ushort>
{
    public override int Compare(ushort x, ushort y) => x.CompareTo(y);
}

//public class NullableUInt16Comparer : Comparer<UInt16?>
//{
//    public override int Compare(UInt16? x, UInt16? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class Int32Comparer : Comparer<int>
{
    public override int Compare(int x, int y) => x.CompareTo(y);
}

//public class NullableInt32Comparer : Comparer<Int32?>
//{
//    public override int Compare(Int32? x, Int32? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class UInt32Comparer : Comparer<uint>
{
    public override int Compare(uint x, uint y) => x.CompareTo(y);
}

//public class NullableUInt32Comparer : Comparer<UInt32?>
//{
//    public override int Compare(UInt32? x, UInt32? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class Int64Comparer : Comparer<long>
{
    public override int Compare(long x, long y) => x.CompareTo(y);
}

//public class NullableInt64Comparer : Comparer<Int64?>
//{
//    public override int Compare(Int64? x, Int64? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class UInt64Comparer : Comparer<ulong>
{
    public override int Compare(ulong x, ulong y) => x.CompareTo(y);
}

//public class NullableUInt64Comparer : Comparer<UInt64?>
//{
//    public override int Compare(UInt64? x, UInt64? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class GuidComparer : Comparer<Guid>
{
    public override int Compare(Guid x, Guid y) => x.CompareTo(y);
}

//public class NullableGuidComparer : Comparer<Guid?>
//{
//    public override int Compare(Guid? x, Guid? y)
//    {
//        if (x.HasValue && y.HasValue)
//        {
//            return x.Value.CompareTo(y.Value);
//        }

//        if (!x.HasValue)
//        {
//            return -1;
//        }

//        if (!y.HasValue)
//        {
//            return 1;
//        }

//        return 0;
//    }
//}

public class ObjectComparer : Comparer<object>
{
    public override int Compare(object x, object y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        var text = x as string;
        var text2 = y as string;
        if (text != null && text2 != null)
        {
            return String.Compare(text, text2);
        }

        var comparable = x as IComparable;
        if (comparable != null)
        {
            return comparable.CompareTo(y);
        }

        var comparable2 = y as IComparable;
        if (comparable2 != null)
        {
            return -comparable2.CompareTo(x);
        }

        throw new NotImplementedException();
    }
}

#endregion

#region "EqualityComparer"

public class StringEqualityComparer : EqualityComparer<string>
{
    public override bool Equals(string x, string y) => String.Equals(x, y);

    public override int GetHashCode(string obj) => obj.GetHashCode();
}

public class CharEqualityComparer : EqualityComparer<char>
{
    public override bool Equals(char x, char y) => Char.Equals(x, y);

    public override int GetHashCode(char val) => val.GetHashCode();
}

//public class NullableCharEqualityComparer : EqualityComparer<Char?>
//{
//    public override bool Equals(Char? x, Char? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return Char.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(Char? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class ByteEqualityComparer : EqualityComparer<byte>
{
    public override bool Equals(byte x, byte y) => Byte.Equals(x, y);

    public override int GetHashCode(byte val) => val.GetHashCode();
}

//public class NullableByteEqualityComparer : EqualityComparer<Byte?>
//{
//    public override bool Equals(Byte? x, Byte? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return Byte.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(Byte? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class SByteEqualityComparer : EqualityComparer<sbyte>
{
    public override bool Equals(sbyte x, sbyte y) => SByte.Equals(x, y);

    public override int GetHashCode(sbyte val) => val.GetHashCode();
}

//public class NullableSByteEqualityComparer : EqualityComparer<SByte?>
//{
//    public override bool Equals(SByte? x, SByte? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return SByte.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(SByte? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class Int16EqualityComparer : EqualityComparer<short>
{
    public override bool Equals(short x, short y) => Int16.Equals(x, y);

    public override int GetHashCode(short val) => val.GetHashCode();
}

//public class NullableInt16EqualityComparer : EqualityComparer<Int16?>
//{
//    public override bool Equals(Int16? x, Int16? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return Int16.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(Int16? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class UInt16EqualityComparer : EqualityComparer<ushort>
{
    public override bool Equals(ushort x, ushort y) => UInt16.Equals(x, y);

    public override int GetHashCode(ushort val) => val.GetHashCode();
}

//public class NullableUInt16EqualityComparer : EqualityComparer<UInt16?>
//{
//    public override bool Equals(UInt16? x, UInt16? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return UInt16.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(UInt16? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class Int32EqualityComparer : EqualityComparer<int>
{
    public override bool Equals(int x, int y) => Int32.Equals(x, y);

    public override int GetHashCode(int val) => val.GetHashCode();
}

//public class NullableInt32EqualityComparer : EqualityComparer<Int32?>
//{
//    public override bool Equals(Int32? x, Int32? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return Int32.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(Int32? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class UInt32EqualityComparer : EqualityComparer<uint>
{
    public override bool Equals(uint x, uint y) => UInt32.Equals(x, y);

    public override int GetHashCode(uint val) => val.GetHashCode();
}

//public class NullableUInt32EqualityComparer : EqualityComparer<UInt32?>
//{
//    public override bool Equals(UInt32? x, UInt32? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return UInt32.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(UInt32? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class Int64EqualityComparer : EqualityComparer<long>
{
    public override bool Equals(long x, long y) => Int64.Equals(x, y);

    public override int GetHashCode(long val) => val.GetHashCode();
}

//public class NullableInt64EqualityComparer : EqualityComparer<Int64?>
//{
//    public override bool Equals(Int64? x, Int64? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return Int64.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(Int64? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class UInt64EqualityComparer : EqualityComparer<ulong>
{
    public override bool Equals(ulong x, ulong y) => UInt64.Equals(x, y);

    public override int GetHashCode(ulong val) => val.GetHashCode();
}

//public class NullableUInt64EqualityComparer : EqualityComparer<UInt64?>
//{
//    public override bool Equals(UInt64? x, UInt64? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return UInt64.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(UInt64? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class GuidEqualityComparer : EqualityComparer<Guid>
{
    public override bool Equals(Guid x, Guid y) => Guid.Equals(x, y);

    public override int GetHashCode(Guid val) => val.GetHashCode();
}

//public class NullableGuidEqualityComparer : EqualityComparer<Guid?>
//{
//    public override bool Equals(Guid? x, Guid? y)
//    {
//        if (x.HasValue)
//        {
//            if (y.HasValue)
//            {
//                return Guid.Equals(x.Value, y.Value);
//            }
//            return false;
//        }
//        if (y.HasValue)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override int GetHashCode(Guid? val)
//    {
//        return val.HasValue ? val.Value.GetHashCode() : 0;
//    }
//}

public class ObjectEqualityComparer : EqualityComparer<object>
{
    public override bool Equals(object x, object y)
    {
        if (x != null)
        {
            if (y != null)
            {
                return x.Equals(y);
            }

            return false;
        }

        if (y != null)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode(object val) => val?.GetHashCode() ?? 0;
}

#endregion
