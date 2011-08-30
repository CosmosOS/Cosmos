using System;

namespace Orvid.Graphics.FontSupport.bdf
{
    /// <summary>
    /// Describes the input token stream.
    /// </summary>
    public class Token
    {
        public int kind;
        public int beginLine, beginColumn, endLine, endColumn;
        public String image;
        public Token next;
        public Token specialToken;
        public String toString()
        {
            return image;
        }
        public static Token newToken(int ofKind)
        {
            return new Token();
        }

    }
}
