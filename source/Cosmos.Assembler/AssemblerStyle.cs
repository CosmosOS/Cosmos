using System.Collections.Generic;

namespace Cosmos.Assembler
{
    public enum AssemblerStyles
    {
        NAsm,
        GNU
    }

    public static class AssemblerStyle
    {
        private static Dictionary<AssemblerStyles, char> mCommentChars = new Dictionary<AssemblerStyles, char>()
        {
            { AssemblerStyles.NAsm, ';' },
            { AssemblerStyles.GNU, '@' }
        };

        public static char GetCommentChar(this AssemblerStyles aThis)
        {
            return mCommentChars[aThis];
        }
    }
}
