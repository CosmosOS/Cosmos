using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;

namespace Cosmos.Core_Plugs.System.Collections.Generic
{
    [Plug(TargetName = "System.Collections.Generic.ComparerHelpers")]
    public static class ComparerHelpersImpl
    {
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

            // TODO: Nullable<>

            // TODO: Enum (Comparer is special to avoid boxing)

            //else
            //{
            //    xResult = new ObjectComparer<object>();
            //}

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

            throw new NotImplementedException();
        }

        public override int GetHashCode(string obj)
        {
            mDebugger.Send("StringEqualityComparer.GetHashCode");

            throw new NotImplementedException();
        }
    }
}
