using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Assembler
{
    public enum CompilerStyles
    {
        NAsm,
        GNU
    }

    public static class CompilerStyle
    {
        private static Dictionary<CompilerStyles, char> mCommentChars = new Dictionary<CompilerStyles, char>()
        {
            { CompilerStyles.NAsm, ';' },
            { CompilerStyles.GNU, '@' }
        };

        public static char GetCommentChar(this CompilerStyles aThis)
        {
            return mCommentChars[aThis];
        }
    }
}
