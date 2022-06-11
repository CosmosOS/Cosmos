using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Globalization;

internal class OrdinalImpl
{
    [PlugMethod(IsOptional = false, PlugRequired = true)]
    public static unsafe int CompareStringIgnoreCaseNonAscii(char* aStrA, int aLengthA, char* aStrB, int aLengthB)
    {
        if (aLengthA < aLengthB)
        {
            return -1;
        }

        if (aLengthA > aLengthB)
        {
            return 1;
        }

        for (var i = 0; i < aLengthA; i++)
        {
            if (aStrA[i] < aStrB[i])
            {
                return -1;
            }

            if (aStrA[i] < aStrB[i])
            {
                return 1;
            }
        }

        return 0;
    }
}
