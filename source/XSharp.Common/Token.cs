using System;

namespace XSharp.Common
{
  public enum TokenType
  {
    // Line based
    Comment, LiteralAsm
    //
    , Register, Keyword, AlphaNum
    // Values
    , ValueInt, ValueString
    //
    , WhiteSpace, Operator, Delimiter
    , Call
    // For now used during scanning while user is typing, but in future could be user methods we have to find etc
    , Unknown
  }

  public class Token
  {
    public TokenType Type = TokenType.Unknown;
    public string RawValue;
    public int SrcPosStart;
    public int SrcPosEnd;

    public uint IntValue
    {
      get
      {
        return mIntValue.Value;
      }
    }

    private uint? mIntValue;
    private XSRegisters.Register mRegister;

    public void SetIntValue(uint value)
    {
      mIntValue = value;
    }

    public XSRegisters.Register Register
    {
      get
      {
        if (mRegister == null)
        {
          throw new InvalidOperationException();
        }
        return mRegister;
      }
    }

    public void SetRegister(XSRegisters.Register register)
    {
      mRegister = register;
    }

    /// <summary>Get line number this token belongs to.</summary>
    public int LineNumber { get; private set; }

    public Token(int lineNumber)
    {
      LineNumber = lineNumber;
    }

    public override string ToString()
    {
      return RawValue;
    }

    public bool Matches(string aText)
    {
      return string.Equals(RawValue, aText, StringComparison.OrdinalIgnoreCase);
    }
  }
}
