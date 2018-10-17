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

            if (aType == typeof(string))
            {
                return new StringEqualityComparer();
            }

            if (aType == typeof(char))
            {
                return new CharEqualityComparer();
            }

            if (aType == typeof(int))
            {
                return new Int32EqualityComparer();
            }

            if (aType == typeof(byte))
            {
                return new ByteEqualityComparer();
            }

            // TODO: Nullable<>

            // TODO: Enum (Comparer is special to avoid boxing)

            mDebugger.Send($"No EqualityComparer for type {aType}");
            return null;
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

    public class CharEqualityComparer : EqualityComparer<char>
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

    public class ByteEqualityComparer : EqualityComparer<byte>
    {
        public override bool Equals(byte x, byte y)
        {
            return x == y;
        }

        public override int GetHashCode(byte val)
        {
            return val.GetHashCode();
        }
    }

    public class Int32EqualityComparer : EqualityComparer<int>
    {
        public override bool Equals(int x, int y)
        {
            return x == y;
        }

        public override int GetHashCode(int val)
        {
            return val.GetHashCode();
        }
    }
}
