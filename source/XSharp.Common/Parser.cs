using System;
using System.Collections.Generic;
using System.Linq;

namespace XSharp.Common {
  /// <summary>
  /// Parser recognizes the following tokens:
  /// - _123      -> Number
  /// - _REG      -> All registers
  /// - _REGADDR  -> All 32-bit registers
  /// - 1         -> Number as well
  /// - _ABC      -> Random label, used indirectly (ie, used as a field)
  /// - #_ABC     -> Random label, used for the value (ie, pointer to the field)
  /// </summary>
  public class Parser {
    /// <summary>Index in <see cref="mData"/> of the first yet unconsumed character.</summary>
    protected int mStart = 0;
    /// <summary>Initial text provided as a constructor parameter.</summary>
    protected string mData;
    /// <summary>true if whitespace tokens should be kept and propagated to the next parsing
    /// stage.</summary>
    protected bool mIncludeWhiteSpace;
    /// <summary>true while every token encountered until so far by this parser are whitespace
    /// tokens.</summary>
    protected bool mAllWhitespace;
    /// <summary>true if the parser supports patterns recognition.</summary>
    protected bool mAllowPatterns;

    /// <summary>Tokens retrieved so far by the parser.</summary>
    protected TokenList mTokens;

    /// <summary>Get a list of tokens that has been built at class instanciation.</summary>
    public TokenList Tokens {
      get { return mTokens; }
    }

    protected static readonly char[] mComma = new char[] { ',' };
    protected static readonly char[] mSpace = new char[] { ' ' };
    public static string[] mKeywords = (
      "As,All"
      + ",BYTE"
      + ",CALL,CONST"
      + ",DWORD"
      + ",exit"
      + ",function"
      + ",goto"
      + ",IF,INTERRUPT,iret"
      + ",namespace"
      + ",PORT"
      + ",return,ret,REPEAT"
      + ",times"
      + ",var"
      + ",word,while"
    ).ToUpper().Split(mComma);

    public static readonly Dictionary<string, XSRegisters.Register> Registers;
    public static readonly Dictionary<string, XSRegisters.Register> RegistersAddr;
    public static readonly Dictionary<string, XSRegisters.Register> Registers8;
    public static readonly Dictionary<string, XSRegisters.Register> Registers16;
    public static readonly Dictionary<string, XSRegisters.Register> Registers32;
    public static readonly Dictionary<string, XSRegisters.Register> RegistersIdx;
    public static readonly string[] RegisterPatterns = "_REG,_REG8,_REG16,_REG32,_REGIDX,_REGADDR".Split(mComma);
    public static readonly string[] Delimiters = ",".Split(mSpace);
    // _.$ are AlphaNum. See comments in Parser
    // # is comment and literal, but could be reused when not first char
    // string[] is used instead of string because operators can be multi char, != >= etc
    public static readonly string[] Operators = "( ) () ! = != >= <= [ [- ] + - * : { } < > ?= ?& @ ~> <~ >> << ++ -- # +# & | ^".Split(mSpace);

    static Parser() {
      Registers8 = new Dictionary<string, XSRegisters.Register>(){
        {"AL", XSRegisters.AL },
        {"AH", XSRegisters.AH },
        {"BL", XSRegisters.BL },
        {"BH", XSRegisters.BH },
        {"CL", XSRegisters.CL },
        {"CH", XSRegisters.CH },
        {"DL", XSRegisters.DL },
        {"DH", XSRegisters.DH },
                 };

      Registers16 = new Dictionary<string, XSRegisters.Register>()
                    {
        {"AX", XSRegisters.AX },
        {"BX", XSRegisters.BX },
        {"CX", XSRegisters.CX },
        {"DX", XSRegisters.DX },
                    };

      Registers32 = new Dictionary<string, XSRegisters.Register>()
                    {
        {"EAX", XSRegisters.EAX },
        {"EBX", XSRegisters.EBX },
        {"ECX", XSRegisters.ECX },
        {"EDX", XSRegisters.EDX },
                    };

      RegistersIdx = new Dictionary<string, XSRegisters.Register>()
                    {
        {"ESI", XSRegisters.ESI },
        {"EDI", XSRegisters.EDI },
        {"ESP", XSRegisters.ESP },
        {"EBP", XSRegisters.EBP },
                    };

      var xRegisters = new Dictionary<string, XSRegisters.Register>();
      xRegisters.AddRange(Registers8);
      xRegisters.AddRange(Registers16);
      xRegisters.AddRange(Registers32);
      xRegisters.AddRange(RegistersIdx);
      Registers = xRegisters;

      var xRegistersAddr = new Dictionary<string, XSRegisters.Register>();
      xRegistersAddr.AddRange(Registers32);
      xRegistersAddr.AddRange(RegistersIdx);
      RegistersAddr = xRegistersAddr;
    }

    /// <summary>Parse next token from currently parsed line, starting at given position and
    /// add the retrieved token at end of given token list.</summary>
    /// <param name="aList">The token list where to add the newly recognized token.</param>
    /// <param name="lineNumber">Line number for diagnostics and debugging purpose.</param>
    /// <param name="rPos">The index in current source code line of the first not yet consumed
    /// character. On return this parameter will be updated to account for characters that would
    /// have been consumed.</param>
    protected void NewToken(TokenList aList, int lineNumber, ref int rPos) {
      #region Pattern Notes
      // All patterns start with _, this makes them reserved. User can use too, but at own risk of conflict.
      //
      // Wildcards
      // -_REG or ??X
      // -_REG8 or ?H,?L
      // -_REG16 or ?X
      // -_REG32 or E?X
      //     - ? based ones are ugly and less clear
      // -_Keyword
      // -_ABC
      //
      //
      // Multiple Options (All caps only) - Registers only
      // Used to suport EAX,EBX - ie lists. But found out wasnt really needed. May add again later.
      //
      // -AX/AL - Conflict if we ever use /
      // -AX|AL - Conflict if we ever use |
      // -AX,AL - , is unlikely to ever be used as an operator and is logical as a separator. Method calls might use, but likely better to use a space
      //          since we will only allow simple arguments, not compound.
      // -_REG:AX|AL - End terminator issue
      // -_REG[AX|AL] - Conflict with existing indirect access. Is indirect access always numeric? I think x86 has some register based ones too.
      //
      //
      // Specific: Register, Keyword, AlphaNum
      // -EAX
      #endregion

      string xString = null;
      char xChar1 = mData[mStart];
      var xToken = new Token(lineNumber);

      // Recognize comments and literal assembler code.
      if (mAllWhitespace && "/!".Contains(xChar1)) {
        rPos = mData.Length; // This will account for the dummy whitespace at the end.
        xString = mData.Substring(mStart + 1, rPos - mStart - 1).Trim();
        // So ToString/Format wont generate error
        xString = xString.Replace("{", "{{");
        xString = xString.Replace("}", "}}");
        // Fix issue #15662 with string length check.
        // Fix issue #15663 with comparing from mData and not from xString anymore.
        if (('/' == xChar1) && (2 <= xString.Length) && ('/' == mData[mStart + 1])) {
          xString = xString.Substring(1);
          xToken.Type = TokenType.Comment;
        } else if (xChar1 == '!') {
          // Literal assembler code.
          xToken.Type = TokenType.LiteralAsm;
        }
      } else {
        xString = mData.Substring(mStart, rPos - mStart);

        if (string.IsNullOrWhiteSpace(xString) && xString.Length > 0) {
          xToken.Type = TokenType.WhiteSpace;

        } else if (xChar1 == '\'') {
          xToken.Type = TokenType.ValueString;
          xString = xString.Substring(1, xString.Length - 2);

        } else if (char.IsDigit(xChar1)) {
          xToken.Type = TokenType.ValueInt;
          if (xString.StartsWith("0x"))
          {
            xToken.SetIntValue(Convert.ToUInt32(xString, 16));
          }
          else
          {
            xToken.SetIntValue(uint.Parse(xString));
          }
        } else if (xChar1 == '$') {
          xToken.Type = TokenType.ValueInt;
          // Remove surrounding '
          xString = "0x" + xString.Substring(1);
          if (xString.StartsWith("0x"))
          {
            xToken.SetIntValue(Convert.ToUInt32(xString, 16));
          }
          else
          {
            xToken.SetIntValue(uint.Parse(xString));
          }
        } else if (IsAlphaNum(xChar1)) { // This must be after check for ValueInt
          string xUpper = xString.ToUpper();

          // Special parsing when in pattern mode. We recognize some special strings
          // which would otherwise be considered as simple AlphaNum token otherwise.
          if (mAllowPatterns) {
            if (RegisterPatterns.Contains(xUpper)) {
              xToken.Type = TokenType.Register;
            } else if (xUpper == "_KEYWORD") {
              xToken.Type = TokenType.Keyword;
              xString = null;
            } else if (xUpper == "_ABC") {
              xToken.Type = TokenType.AlphaNum;
              xString = null;
            }
            else if (xUpper == "_PCALL") {
              xString = null;
              xToken.Type = TokenType.Call;
            }
          }

          if (xToken.Type == TokenType.Unknown)
          {
            XSRegisters.Register xRegister;
            if (Registers.TryGetValue(xUpper, out xRegister)) {
              xToken.Type = TokenType.Register;
              xToken.SetRegister(xRegister);
            } else if (mKeywords.Contains(xUpper))
            {
              xToken.Type = TokenType.Keyword;
            } else if(xString.Contains("(") && xString.Contains(")") && IsAlphaNum(xChar1)) {
                xToken.Type = TokenType.Call;
            } else {
              xToken.Type = TokenType.AlphaNum;
            }
          }

        } else if (Delimiters.Contains(xString)) {
          xToken.Type = TokenType.Delimiter;
        } else if (Operators.Contains(xString)) {
          xToken.Type = TokenType.Operator;
        }
      }

      xToken.RawValue = xString;
      xToken.SrcPosStart = mStart;
      xToken.SrcPosEnd = xToken.Type == TokenType.Call ? rPos : rPos - 1;
      if (mAllWhitespace && (xToken.Type != TokenType.WhiteSpace)) {
        mAllWhitespace = false;
      }
      mStart = xToken.Type == TokenType.Call ? rPos + 1 : rPos;

      if (mIncludeWhiteSpace || (xToken.Type != TokenType.WhiteSpace)) {
        aList.Add(xToken);
      }
    }

    protected enum CharType { WhiteSpace, Identifier, Symbol, String };

    protected bool IsAlphaNum(char aChar) {
      return char.IsLetterOrDigit(aChar) || aChar == '_' || aChar == '.' || aChar == '$';
    }

    /// <summary>Consume text that has been provided to the class constructor, splitting it into
    /// a list of tokens.</summary>
    /// <param name="lineNumber">Line number for diagnostics and debugging.</param>
    /// <returns>The resulting tokens list.</returns>
    protected TokenList Parse(int lineNumber) {
      // Save in comment, might be useful in future. Already had to dig it out of TFS once
      //var xRegex = new System.Text.RegularExpressions.Regex(@"(\W)");

      var xResult = new TokenList();
      CharType xLastCharType = CharType.WhiteSpace;
      char xChar;
      CharType xCharType = CharType.WhiteSpace;
      int i = 0;
      for (i = 0; i < mData.Length; i++) {
        xChar = mData[i];
        // Extract string literal (surrounded with single quote characters).
        if (xChar == '\'') {
          // Take data before the ' as a token.
          NewToken(xResult, lineNumber, ref i);
          // Now scan to the next ' taking into account escaped single quotes.
          bool escapedCharacter = false;
          for (i = i + 1; i < mData.Length; i++) {
            bool done = false;
            switch(mData[i])
            {
              case '\'':
                if (!escapedCharacter) { done = true; }
                break;
              case '\\':
                escapedCharacter = !escapedCharacter;
                break;
              default:
                escapedCharacter = false;
                break;
            }
            if (done) { break; }
          }
          if (i == mData.Length) {
            throw new Exception("Unterminated string.");
          }
          i++;
          xCharType = CharType.String;
        }
        else if (xChar == '(')
        {
            for (i += 1; i < mData.Length; i++)
            {
                if (mData[i] == ')' && mData.LastIndexOf(")") <= i)
                {
                    i++;
                    NewToken(xResult, lineNumber, ref i);
                    break;
                }
            }
        }
        else if (char.IsWhiteSpace(xChar))
        {
            xCharType = CharType.WhiteSpace;
        }
        else if (IsAlphaNum(xChar))
        {
            // _ and . were never likely to stand on their own. ie ESP _ 2 and ESP . 2 are never likely to be used.
            // Having them on their own required a lot of code
            // to treat them as a single unit where we did use them. So we treat them as AlphaNum.
            xCharType = CharType.Identifier;
        }
        else
        {
            xCharType = CharType.Symbol;
        }

        // i > 0 - Never do NewToken on first char. i = 0 is just a pass to get char and set lastchar.
        // But its faster as the second short circuit rather than a separate if.
        if ((xCharType != xLastCharType) && (0 < i)) {
          NewToken(xResult, lineNumber, ref i);
        }

        xLastCharType = xCharType;
      }

      // Last token
      if (mStart < mData.Length) {
        NewToken(xResult, lineNumber, ref i);
      }

      return xResult;
    }

    /// <summary>Create a new Parser instance and immediately consume the given <paramref name="aData"/>
    /// string. On return the <seealso cref="Tokens"/> property is available for enumeration.</summary>
    /// <param name="aData">The text to be parsed. WARNING : This is expected to be a single full line
    /// of text. The parser can be create with a special "pattern recognition" mode.</param>
    /// <param name="aIncludeWhiteSpace"></param>
    /// <param name="aAllowPatterns">True if <paramref name="aData"/> is a pattern and thus the parsing
    /// should be performed specifically.</param>
    /// <exception cref="Exception">At least one unrecognized token has been parsed.</exception>
    public Parser(string aData, int lineNumber, bool aIncludeWhiteSpace, bool aAllowPatterns) {
      mData = aData;
      mIncludeWhiteSpace = aIncludeWhiteSpace;
      mAllowPatterns = aAllowPatterns;
      mAllWhitespace = true;

      mTokens = Parse(lineNumber);
      if (mTokens.Count(q => q.Type == TokenType.Unknown) > 0) {

        foreach(Token token in mTokens)
        {
          if (TokenType.Unknown == token.Type) {
            throw new Exception(string.Format("Unknown token '{0}' found at {1}/{2}.",
              token.RawValue ?? "NULL", token.LineNumber, token.SrcPosStart));
          }
        }
      }
    }
  }
}
