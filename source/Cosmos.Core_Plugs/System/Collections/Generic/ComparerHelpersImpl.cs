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

            if (aType == typeof(int))
            {
                return new Int32EqualityComparer();
            }

            // TODO: Nullable<>

            // TODO: Enum (Comparer is special to avoid boxing)

            //else
            //{
            //    xResult = new ObjectComparer<object>();
            //}
            mDebugger.Send($"No EqualityComparer for type {aType}");
            return null;
        }
    }

    public class StringComparer : Comparer<string>
    {
        private readonly Debugger mDebugger = new Debugger("Core", "String Comparer");

        public override int Compare(string x, string y)
        {
            mDebugger.Send("StringComparer.Compare");

            throw new NotImplementedException();
        }
    }

    public class StringEqualityComparer : EqualityComparer<string>
    {
        private readonly Debugger mDebugger = new Debugger("Core", "String Equality Comparer");

        public override bool Equals(string x, string y)
        {
            mDebugger.Send("StringEqualityComparer.Equals");
            return String.Equals(x, y);
        }

        public override int GetHashCode(string obj)
        {
            mDebugger.Send("StringEqualityComparer.GetHashCode");

            return obj.GetHashCode();
        }
    }

    public class Int32EqualityComparer : EqualityComparer<int>
    {
        private readonly Debugger mDebugger = new Debugger("Core", "Int32 Equality Comparer");

        public override bool Equals(int x, int y)
        {
            mDebugger.Send("Int32EqualityComparer.Equals");
            return x == y;
        }

        public override int GetHashCode(int val)
        {
            mDebugger.Send("Int32EqualityComparer.GetHashCode");

            return val.GetHashCode();
        }
    }
}
