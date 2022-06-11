using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System;

[Plug(Target = typeof(object))]
public static class ObjectImpl
{
    private static readonly Debugger mDebugger = new("IL2CPU", "ObjectImpl");

    public static string ToString(object aThis)
    {
        mDebugger.Send("<Object.ToString not yet implemented!>");
        return "<Object.ToString not yet implemented!>";
    }

    public static void Ctor(object aThis)
    {
    }

    public static unsafe Type GetType([ObjectPointerAccess] uint* aThis)
    {
        var xType = *aThis;
        return new CosmosRuntimeType(xType);
    }

    public static int GetHashCode(object aThis) => (int)aThis;

    public static unsafe ulong MemberwiseClone([ObjectPointerAccess] uint aThis)
    {
        var xThisPointer = (uint*)aThis;
        var xSize = ObjectUtils.FieldDataOffset + xThisPointer[2];

        var xResult = GCImplementation.AllocNewObject(xSize);

        var xThisPointerByte = (byte*)xThisPointer;
        var xThatPointerByte = (byte*)xResult;

        for (var i = 0; i < xSize; i++)
        {
            xThatPointerByte[i] = xThisPointerByte[i];
        }

        var xReturn = (ulong)xResult << (sizeof(ulong) / 2 * 8);

        return xReturn;
    }
}
