using System;
using System.Runtime.CompilerServices;
using Cosmos.Core;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime.CompilerServices;

[Plug(Target = typeof(RuntimeHelpers))]
public static class RuntimeHelpersImpl
{
    public static void cctor()
    {
    }

#pragma warning disable CS0108
    public static bool Equals(object aO1, object aO2)
#pragma warning restore CS0108
    {
        return aO1 == aO2; //we cant use object.Equals since it just calls this
    }

    public static void ProbeForSufficientStack()
    {
        // no implementation yet, before threading not needed
    }

    public static int GetHashCode(object o) => throw new NotImplementedException();

    public static bool IsReferenceOrContainsReferences<T>()
    {
        var t = ((CosmosRuntimeType)typeof(T)).mTypeId;
        return ContainsReference(t);
    }

    private static bool ContainsReference(uint mType)
    {
        if (!VTablesImpl.IsValueType(mType))
        {
            return true;
        }

        if (VTablesImpl.IsStruct(mType))
        {
            var fields = VTablesImpl.GetGCFieldTypes(mType);
            for (var i = 0; i < fields.Length; i++)
            {
                if (ContainsReference(fields[i]))
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    public static bool TryEnsureSufficientExecutionStack() => throw new NotImplementedException();
}
