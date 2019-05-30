using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;

namespace Cosmos.Core_Plugs.System.Collections.Generic
{
    [Plug("System.Collections.Generic.ComparerHelpers, System.Private.CoreLib")]
    public static class ComparerHelpersImpl
    {
        private static readonly Debugger mDebugger = new Debugger("Core", "ComparerHelpersImpl");

        public static object CreateDefaultComparer(Type aType)
        {
            Debugger.DoBochsBreak();

            if (aType == typeof(string))
            {
                return new StringComparer();
            }

            return null;
        }

        public static object CreateDefaultEqualityComparer(Type aType)
        {
            Debugger.DoBochsBreak();

            //TODO: Do application level testing to determine the most frequent comparisons and do those type checks first.

            /*
             * For now I've made some assumptions about how common each type is.
             * Strings are very common for controls, messages, console input/output, etc.
             * Bytes will be used for binary operations and protocols.
             * Int I believe will be used for return codes and 32 registers (which I expect to be common with x86 and ARM).
             * Long should be semi-unique identifiers.
             * I don't except Char to be as common as string but I expect to be used in roughly the same way.
             * The only use case I see for short off-hand is PCM data but that might be more useful for audio support at a later date.
             */

            if (aType == typeof(Byte))
            {
                return new ByteEqualityComparer();
            }

            if (aType == typeof(Byte?))
            {
                return new NullableByteEqualityComparer();
            }

            if (aType == typeof(SByte))
            {
                return new SByteEqualityComparer();
            }

            if (aType == typeof(SByte?))
            {
                return new NullableSByteEqualityComparer();
            }

            if (aType == typeof(string))
            {
                return new StringEqualityComparer();
            }

            if (aType == typeof(Int32))
            {
                return new Int32EqualityComparer();
            }

            if (aType == typeof(Int32?))
            {
                return new NullableInt32EqualityComparer();
            }

            if (aType == typeof(UInt32))
            {
                return new UInt32EqualityComparer();
            }

            if (aType == typeof(UInt32?))
            {
                return new NullableUInt32EqualityComparer();
            }

            if (aType == typeof(Int64))
            {
                return new Int64EqualityComparer();
            }

            if (aType == typeof(Int64?))
            {
                return new NullableInt64EqualityComparer();
            }

            if (aType == typeof(UInt64))
            {
                return new UInt64EqualityComparer();
            }

            if (aType == typeof(UInt64?))
            {
                return new NullableUInt64EqualityComparer();
            }

            if (aType == typeof(Char))
            {
                return new CharEqualityComparer();
            }

            if (aType == typeof(Char?))
            {
                return new NullableCharEqualityComparer();
            }

            if (aType == typeof(Int16))
            {
                return new Int16EqualityComparer();
            }

            if (aType == typeof(Int16?))
            {
                return new NullableInt16EqualityComparer();
            }

            if (aType == typeof(UInt16))
            {
                return new UInt16EqualityComparer();
            }

            if (aType == typeof(UInt16?))
            {
                return new NullableUInt16EqualityComparer();
            }

            if (aType == typeof(Guid))
            {
                return new GuidEqualityComparer();
            }

            if (aType == typeof(Guid?))
            {
                return new NullableGuidEqualityComparer();
            }

            if (aType.IsEnum)
            {
                switch (Type.GetTypeCode(Enum.GetUnderlyingType(aType)))
                {
                    case TypeCode.SByte:
                        return new SByteEqualityComparer();
                    case TypeCode.Byte:
                        return new ByteEqualityComparer();
                    case TypeCode.Int16:
                        return new Int16EqualityComparer();
                    case TypeCode.UInt16:
                        return new UInt16EqualityComparer();
                    case TypeCode.Int32:
                        return new Int32EqualityComparer();
                    case TypeCode.UInt32:
                        return new UInt32EqualityComparer();
                    case TypeCode.Int64:
                        return new Int64EqualityComparer();
                    case TypeCode.UInt64:
                        return new UInt64EqualityComparer();
                    default:
                        return null;
                }
            }

            //MS framework falls back to address compare so we'll do the same.
            //mDebugger.Send($"No EqualityComparer for type {aType}");
            return new ObjectEqualityComparer();
        }
    }

    public class StringComparer : Comparer<string>
    {
        public override int Compare(string x, string y)
        {
            throw new NotImplementedException();
        }
    }

    public class StringEqualityComparer : EqualityComparer<string>
    {
        public override bool Equals(string x, string y)
        {
            return String.Equals(x, y);
        }

        public override int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }

    public class CharEqualityComparer : EqualityComparer<Char>
    {
        public override bool Equals(char x, char y)
        {
            return x == y;
        }

        public override int GetHashCode(char val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableCharEqualityComparer : EqualityComparer<Char?>
    {
        public override bool Equals(Char? x, Char? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Char? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class ByteEqualityComparer : EqualityComparer<Byte>
    {
        public override bool Equals(Byte x, Byte y)
        {
            return x == y;
        }

        public override int GetHashCode(Byte val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableByteEqualityComparer : EqualityComparer<Byte?>
    {
        public override bool Equals(Byte? x, Byte? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Byte? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class SByteEqualityComparer : EqualityComparer<SByte>
    {
        public override bool Equals(SByte x, SByte y)
        {
            return x == y;
        }

        public override int GetHashCode(SByte val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableSByteEqualityComparer : EqualityComparer<SByte?>
    {
        public override bool Equals(SByte? x, SByte? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(SByte? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class Int16EqualityComparer : EqualityComparer<Int16>
    {
        public override bool Equals(Int16 x, Int16 y)
        {
            return x == y;
        }

        public override int GetHashCode(Int16 val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableInt16EqualityComparer : EqualityComparer<Int16?>
    {
        public override bool Equals(Int16? x, Int16? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Int16? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class UInt16EqualityComparer : EqualityComparer<UInt16>
    {
        public override bool Equals(UInt16 x, UInt16 y)
        {
            return x == y;
        }

        public override int GetHashCode(UInt16 val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableUInt16EqualityComparer : EqualityComparer<UInt16?>
    {
        public override bool Equals(UInt16? x, UInt16? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(UInt16? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class Int32EqualityComparer : EqualityComparer<Int32>
    {
        public override bool Equals(Int32 x, Int32 y)
        {
            return x == y;
        }

        public override int GetHashCode(Int32 val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableInt32EqualityComparer : EqualityComparer<Int32?>
    {
        public override bool Equals(Int32? x, Int32? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Int32? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class UInt32EqualityComparer : EqualityComparer<UInt32>
    {
        public override bool Equals(UInt32 x, UInt32 y)
        {
            return x == y;
        }

        public override int GetHashCode(UInt32 val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableUInt32EqualityComparer : EqualityComparer<UInt32?>
    {
        public override bool Equals(UInt32? x, UInt32? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(UInt32? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class Int64EqualityComparer : EqualityComparer<Int64>
    {
        public override bool Equals(Int64 x, Int64 y)
        {
            return x == y;
        }

        public override int GetHashCode(Int64 val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableInt64EqualityComparer : EqualityComparer<Int64?>
    {
        public override bool Equals(Int64? x, Int64? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Int64? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class UInt64EqualityComparer : EqualityComparer<UInt64>
    {
        public override bool Equals(UInt64 x, UInt64 y)
        {
            return x == y;
        }

        public override int GetHashCode(UInt64 val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableUInt64EqualityComparer : EqualityComparer<UInt64?>
    {
        public override bool Equals(UInt64? x, UInt64? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(UInt64? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class GuidEqualityComparer : EqualityComparer<Guid>
    {
        public override bool Equals(Guid x, Guid y)
        {
            return x == y;
        }

        public override int GetHashCode(Guid val)
        {
            return val.GetHashCode();
        }
    }

    public class NullableGuidEqualityComparer : EqualityComparer<Guid?>
    {
        public override bool Equals(Guid? x, Guid? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.Value.Equals(y.Value);
                }
                return false;
            }
            if (y.HasValue)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Guid? val)
        {
            return val.HasValue ? val.Value.GetHashCode() : 0;
        }
    }

    public class ObjectEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(Object x, Object y)
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

        public override int GetHashCode(Object val)
        {
            return val?.GetHashCode() ?? 0;
        }
    }

}
