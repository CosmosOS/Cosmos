// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cosmos.Kernel.System.Graphics.CosmSl;

/// <summary>
/// Tokenizer for CosmSl sources. Produces the flat token stream the parser
/// walks with an index cursor; whitespace and <c>//</c> line comments are
/// skipped. Kept simple on purpose: the language is a shader description,
/// not a general purpose syntax.
/// </summary>
internal static class CosmSlLexer
{
    internal enum TokenKind
    {
        Identifier,
        Number,
        Symbol,
        End,
    }

    internal readonly struct Token
    {
        public readonly TokenKind Kind;
        public readonly string Text;
        public readonly float Number;
        public readonly int Line;

        public Token(TokenKind kind, string text, float number, int line)
        {
            Kind = kind;
            Text = text;
            Number = number;
            Line = line;
        }
    }

    /// <summary>Lexes the whole source into a token list ending with an End token.</summary>
    internal static List<Token> Lex(string source)
    {
        var tokens = new List<Token>();
        int position = 0;
        int line = 1;

        while (position < source.Length)
        {
            char current = source[position];

            if (current == '\n')
            {
                line++;
                position++;
            }
            else if (char.IsWhiteSpace(current))
            {
                position++;
            }
            else if (current == '/' && position + 1 < source.Length && source[position + 1] == '/')
            {
                while (position < source.Length && source[position] != '\n')
                {
                    position++;
                }
            }
            else if (char.IsLetter(current) || current == '_')
            {
                int start = position;
                while (position < source.Length && (char.IsLetterOrDigit(source[position]) || source[position] == '_'))
                {
                    position++;
                }
                tokens.Add(new Token(TokenKind.Identifier, source.Substring(start, position - start), 0f, line));
            }
            else if (char.IsDigit(current) || (current == '.' && position + 1 < source.Length && char.IsDigit(source[position + 1])))
            {
                int start = position;
                bool hasDot = false;
                while (position < source.Length && (char.IsDigit(source[position]) || (source[position] == '.' && !hasDot)))
                {
                    if (source[position] == '.')
                    {
                        hasDot = true;
                    }
                    position++;
                }
                // An 'f' suffix terminates a float literal and carries no meaning.
                if (position < source.Length && (source[position] == 'f' || source[position] == 'F'))
                {
                    position++;
                }

                string text = source.Substring(start, position - start);
                if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
                {
                    throw new CosmSlException($"Invalid number literal '{text}'.", line);
                }
                tokens.Add(new Token(TokenKind.Number, text, number, line));
            }
            else
            {
                tokens.Add(new Token(TokenKind.Symbol, current.ToString(), 0f, line));
                position++;
            }
        }

        tokens.Add(new Token(TokenKind.End, string.Empty, 0f, line));
        return tokens;
    }
}
