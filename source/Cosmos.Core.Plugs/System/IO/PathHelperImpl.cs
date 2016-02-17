using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.IO
{
    [Plug(TargetName = "System.IO.PathHelper")]
    public static class PathHelperImpl
    {
        public static unsafe int GetFullPathName(ref object aThis,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* aArrayPtr)
        {
            int xLength = 0;
            while (*aArrayPtr != '\0')
            {
                xLength++;
                aArrayPtr++;
            }
            return xLength;
        }

        public static bool TryExpandShortFileName(ref object aThis)
        {
            return true;
        }
    }

}
