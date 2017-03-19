using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace XSharp.Common {
  /// <summary>This class is able to translate a single X# source code line into one or more
  /// target assembler source code and data lines. The class is a group of pattern each of
  /// which defines a transformation function from the X# syntax to the target assembler
  /// syntax.</summary>
  public class TokenPatterns {
    /// <summary>Describe a single pattern with its list of tokens that might include pattern
    /// reserved syntax token and a transformation function. For ease of search and performance
    /// an hashcode value is computed on the tokens list content and later used for searching
    /// a pattern matching an actual line of X# code source.</summary>
    protected class Pattern {
      public readonly TokenList Tokens;
      public readonly int Hash;
      public readonly CodeFunc Code;
      public readonly string PatternString;

      public Pattern(TokenList aTokens, CodeFunc aCode, string patternString) {
        Tokens = aTokens;
        Hash = aTokens.GetHashCode();
        Code = aCode;
        PatternString = patternString;
      }
    }

    /// <summary>The set of blocks for the currently assembled function. Each time we begin
    /// assembling a new function this blocks collection is reset to an empty state.</summary>
    protected Blocks mBlocks = new Blocks();

    protected class Blocks : List<Block> {
      protected int mCurrentLabelID = 0;

      public void Reset() {
        mCurrentLabelID = 0;
      }

      public Block Current() {
        return base[Count - 1];
      }

      public void Start(TokenList aTokens, bool aIsCollector) {
        var xBlock = new Block();
        mCurrentLabelID++;
        xBlock.LabelID = mCurrentLabelID;
        xBlock.StartTokens = aTokens;

        // Last because we use Current() above
        Add(xBlock);
        xBlock.ParentAssembler = Assembler.CurrentInstance;
        new Assembler();
      }

      public void End()
      {
        Assembler.ClearCurrentInstance();
        RemoveAt(Count - 1);
      }
    }
    protected class Block {
      public TokenList StartTokens;
      public int LabelID;

      public Assembler ParentAssembler;

      public void AddContentsToParentAssembler()
      {
        ParentAssembler.Instructions.AddRange(Assembler.CurrentInstance.Instructions);
      }
    }

    protected string mFuncName = null;
    protected bool mFuncExitFound = false;

    public bool EmitUserComments = true;
    public delegate void CodeFunc(TokenList aTokens);
    protected List<Pattern> mPatterns = new List<Pattern>();
    protected bool mInIntHandler;
    protected string[] mCompareOps;
    protected List<string> mCompares = new List<string>();

    protected string mNamespace = null;
    protected string GetNamespace() {
      if (mNamespace == null) {
        throw new Exception("A namespace has not been defined.");
      }
      return mNamespace;
    }

    public TokenPatterns() {
      mCompareOps = "< > = != <= >= 0 !0".Split(' ');
      var xSizes = "byte , word , dword ".Split(',').ToList();
      // We must add this empty size so that we allow constructs where the size is not
      // explicitly defined in source code. For example : while eax < 0
      // otherwise we would have to write : while dword eax < 0
      xSizes.Add("");
      foreach (var xSize in xSizes) {
        foreach (var xComparison in mCompareOps) {
          // Skip 0 and !0
          if (!xComparison.Contains("0")) {
            mCompares.Add(xSize + "_REG " + xComparison + " 123");
            mCompares.Add(xSize + "_REG " + xComparison + " _REG");
            mCompares.Add(xSize + "_REG " + xComparison + " _REGADDR[1]");
            mCompares.Add(xSize + "_REG " + xComparison + " _REGADDR[-1]");
            mCompares.Add(xSize + "_REG " + xComparison + " _ABC");
            mCompares.Add(xSize + "_REG " + xComparison + " #_ABC");
            //
            mCompares.Add(xSize + "_REGADDR[1] " + xComparison + " 123");
            mCompares.Add(xSize + "_REGADDR[-1] " + xComparison + " 123");
            mCompares.Add(xSize + "_REGADDR[1] " + xComparison + " _REG");
            mCompares.Add(xSize + "_REGADDR[-1] " + xComparison + " _REG");
            mCompares.Add(xSize + "_REGADDR[1] " + xComparison + " #_ABC");
            mCompares.Add(xSize + "_REGADDR[-1] " + xComparison + " #_ABC");
            //
            mCompares.Add(xSize + "_ABC " + xComparison + " 123");
            mCompares.Add(xSize + "_ABC " + xComparison + " _REG");
            mCompares.Add(xSize + "_ABC " + xComparison + " #_ABC");
          }
        }
      }

      AddPatterns();
    }

    /// <summary>Builds a label that is suitable to denote a constant which name is given by the
    /// token.</summary>
    /// <param name="aToken"></param>
    /// <returns></returns>
    protected string ConstLabel(Token aToken) {
      return GroupLabel("Const_" + aToken);
    }

    /// <summary>Builds a label at namespace level having the given name.</summary>
    /// <param name="aLabel">Local label name at namespace level.</param>
    /// <returns>The label name</returns>
    protected string GroupLabel(string aLabel) {
      return GetNamespace() + "_" + aLabel;
    }

    /// <summary>Builds a label at function level having the given name.</summary>
    /// <param name="aLabel">Local label name at function level.</param>
    /// <returns>The label name</returns>
    protected string FuncLabel(string aLabel) {
      return GetNamespace() + "_" + mFuncName + "_" + aLabel;
    }

    /// <summary>Builds a label having the given name at current function block level.</summary>
    /// <param name="aLabel">Local label name at function block level.</param>
    /// <returns>The label name.</returns>
    protected string BlockLabel(string aLabel) {
      return FuncLabel("Block" + mBlocks.Current().LabelID + "_" + aLabel);
    }

    /// <summary>Build a label name for the given token. This method enforce the rule for .
    /// and .. prefixes and build the label at appropriate level.</summary>
    /// <param name="aToken"></param>
    /// <returns></returns>
    protected string GetLabel(Token aToken) {
      if ((aToken.Type != TokenType.AlphaNum) && !aToken.Matches("exit")) {
        throw new Exception("Label must be AlphaNum.");
      }

      string xValue = aToken.RawValue;
      if (!InFunctionBody) {
        if (xValue.StartsWith(".")) {
          return xValue.Substring(1);
        }
        return GroupLabel(xValue);
      } else {
        if (xValue.StartsWith("..")) {
          return xValue.Substring(2);
        } else if (xValue.StartsWith(".")) {
          return GroupLabel(xValue.Substring(1));
        }
        return FuncLabel(xValue);
      }
    }

    /// <summary>Get a flag that tell if we are in a function body or not. This is used by the
    /// assembler generator when end of source file is reached to make sure the last function
    /// or interrupt handler is properly closed (see issue #15666)</summary>
    internal bool InFunctionBody {
      get { return !string.IsNullOrEmpty(mFuncName); }
    }

    /// <summary>Start a new function having the given name. The current blocks collection is
    /// reset to an empty state and the function name is saved for later reuse in local to function
    /// labels' name construction.</summary>
    /// <param name="aName">Function name.</param>
    protected void StartFunc(string aName) {
      if (InFunctionBody) {
        throw new Exception(
            "Found a function/interrupt handler definition embedded inside another function/interrupt handler.");
      }
      mFuncName = aName;
      mFuncExitFound = false;
      mBlocks.Reset();
    }

    /// <summary>Terminate assembling current function. If a local to function exit label has not
    /// been explicitly defined a new one is automatically created. This is because some "return"
    /// keyword might have been used in function X# code. This keyword requires an exit label to
    /// be defined at function level. This method also automatically insert an IRET or RET instruction
    /// depending on whether the function is an interrupt handler or a standard function.</summary>
    protected void EndFunc() {
      if (null == mFuncName) {
        throw new Exception("Found a closing curly brace that doesn't match an opening curly brace.");
      }
      if (!mFuncExitFound) {
        XS.Label(GetNamespace() + "_" + mFuncName + "_Exit");
      }
      if (mInIntHandler) {
        XS.InterruptReturn();
      } else {
        XS.Set("static_field__Cosmos_Core_INTs_mLastKnownAddress", GetNamespace() + "_" + mFuncName + "_Exit", destinationIsIndirect: true, size: XSRegisters.RegisterSize.Int32);
        XS.Return();
      }
      mFuncName = null;
    }

    protected string GetSimpleRef(Token aToken) {
      return GetLabel(aToken);
    }

    private static XSRegisters.RegisterSize GetSize(Token aToken)
    {
      return GetSize(aToken.RawValue);
    }

    private static XSRegisters.RegisterSize GetSize(string value)
    {
      switch (value)
      {
        case "byte":
          return XSRegisters.RegisterSize.Byte8;
        case "word":
          return XSRegisters.RegisterSize.Short16;
        case "dword":
          return XSRegisters.RegisterSize.Int32;
        default:
          throw new Exception($"Invalid size '{value}'");
      }
    }

    private static string GetSizeString(XSRegisters.RegisterSize size)
    {
      switch (size)
      {
        case XSRegisters.RegisterSize.Byte8:
          return "byte";
        case XSRegisters.RegisterSize.Short16:
          return "word";
        case XSRegisters.RegisterSize.Int32:
          return "dword";
        default:
          throw new ArgumentOutOfRangeException(nameof(size), size, null);
      }
    }

    protected string GetRef(List<Token> aTokens, ref int rIdx, bool onlySingleTokenRefs = false)
    {
      var xToken1 = aTokens[rIdx];
      Token xToken2 = null;
      if (rIdx + 1 < aTokens.Count
          && !onlySingleTokenRefs)
      {
        xToken2 = aTokens[rIdx + 1];
      }
      if (xToken1.Type == TokenType.Register)
      {
        if (xToken2 != null
            && xToken2.RawValue == "[")
        {
          if (aTokens[rIdx + 2].RawValue == "-")
          {
            rIdx += 5;
            return "[" + xToken1 + " - " + aTokens[rIdx - 2] + "]";
          }
          rIdx += 4;
          return "[" + xToken1 + " + " + aTokens[rIdx - 2] + "]";
        }
        rIdx += 1;
        return xToken1.RawValue;
      }
      else if (xToken1.Type == TokenType.AlphaNum)
      {
        rIdx += 1;
        return "[" + GetLabel(xToken1) + "]";
      }
      else if (xToken1.Type == TokenType.ValueInt)
      {
        rIdx += 1;
        return xToken1.RawValue;
      }
      else if (xToken1.Type == TokenType.Call)
      {
        rIdx += 1;
        return "@ret_on_stack@";
      }
      else if (xToken1.RawValue == "#")
      {
        rIdx += 2;
        return ConstLabel(xToken2);
      }
      else
      {
        throw new Exception("Cannot determine reference");
      }
    }

    protected ConditionalTestEnum GetJump(string aComparison, bool aInvert = false)
    {
      if (aInvert)
      {
        if (aComparison == "<")
        {
          aComparison = ">=";
        }
        else if (aComparison == ">")
        {
          aComparison = "<=";
        }
        else if (aComparison == "=")
        {
          aComparison = "!=";
        }
        else if (aComparison == "0")
        {
          // Same as JE, but implies intent in .asm better
          aComparison = "!0";
        }
        else if (aComparison == "!0")
        {
          // Same as JE, but implies intent in .asm better
          aComparison = "0";
        }
        else if (aComparison == "!=")
        {
          aComparison = "=";
        }
        else if (aComparison == "<=")
        {
          aComparison = ">";
        }
        else if (aComparison == ">=")
        {
          aComparison = "<";
        }
        else
        {
          throw new Exception("Unrecognized symbol in conditional: " + aComparison);
        }
      }

      if (aComparison == "<")
      {
        return ConditionalTestEnum.Below; // unsigned
      }
      else if (aComparison == ">")
      {
        return ConditionalTestEnum.Above; // unsigned
      }
      else if (aComparison == "=")
      {
        return ConditionalTestEnum.Equal;
      }
      else if (aComparison == "0")
      {
        // Same as JE, but implies intent in .asm better
        return ConditionalTestEnum.Zero;
      }
      else if (aComparison == "!=")
      {
        return ConditionalTestEnum.NotEqual;
      }
      else if (aComparison == "!0")
      {
        // Same as JNE, but implies intent in .asm better
        return ConditionalTestEnum.NotZero;
      }
      else if (aComparison == "<=")
      {
        return ConditionalTestEnum.BelowOrEqual; // unsigned
      }
      else if (aComparison == ">=")
      {
        return ConditionalTestEnum.AboveOrEqual; // unsigned
      }
      else
      {
        throw new Exception("Unrecognized symbol in conditional: " + aComparison);
      }
    }

    protected void HandleIf(TokenList aTokens, string xComparison)
    {
      string xLabel;
      var xLast = aTokens.Last();
      if (xLast.RawValue == "{")
      {
        mBlocks.Start(aTokens, false);
        XS.Jump(GetJump(xComparison, true), BlockLabel("End"));
      }
      else
      {
        if (xLast.Matches("return"))
        {
          xLabel = FuncLabel("Exit");
        }
        else
        {
          xLabel = GetLabel(xLast);
        }

        XS.Jump(GetJump(xComparison), xLabel);
      }
    }

    protected void AddPatterns()
    {
      AddPattern("! Mov EAX, 0", delegate(TokenList aTokens)
                                 {
                                   XS.LiteralCode(aTokens[0].RawValue);
                                 });

      AddPattern("// Comment", delegate(TokenList aTokens)
                               {
                                 if (EmitUserComments)
                                 {
                                   string xValue = aTokens[0].RawValue;
                                   xValue = xValue.Replace("\"", "\\\"");
                                   XS.Comment(xValue);
                                 }
                               });

      // The value function returns a token containing the comparison
      var xCompares = new Dictionary<string, Func<int, TokenList, Token>>();
      var xSizes = new[]
                   {
                     "", "byte", "word", "dword"
                   };
      #region Handle all comparisons
      foreach (var xSize in xSizes)
      {
        foreach (var xComparison in mCompareOps)
        {
          var xComparisonToken = new Token(-1);
          xComparisonToken.RawValue = xComparison;
          // Skip 0 and !0
          if (!xComparison.Contains("0"))
          {
            xCompares.Add(xSize + " _REG " + xComparison + " 123",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            XSRegisters.RegisterSize? xTypedSize = null;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xTypedSize = GetSize(tokenList[xOffset]);
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].IntValue, size: xTypedSize);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REG " + xComparison + " _REG",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            XSRegisters.RegisterSize? xTypedSize = null;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xTypedSize = GetSize(tokenList[xOffset]);
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REG " + xComparison + " _REGADDR[1]",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register, sourceDisplacement: (int)tokenList[xOffset + 3].IntValue);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REG " + xComparison + " _REGADDR[-1]",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register, sourceDisplacement: -(int)tokenList[xOffset + 3].IntValue);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REG " + xComparison + " _ABC",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, GetLabel(tokenList[xOffset + 2]), sourceIsIndirect: true);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REG " + xComparison + " #_ABC",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, ConstLabel(tokenList[xOffset + 3]));
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REGADDR[1] " + xComparison + " 123",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            XSRegisters.RegisterSize? xTypedSize = null;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xTypedSize = GetSize(tokenList[xOffset]);
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 5].IntValue, destinationDisplacement: (int)tokenList[xOffset + 2].IntValue, size: xTypedSize);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REGADDR[-1] " + xComparison + " 123",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            XSRegisters.RegisterSize? xTypedSize = null;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xTypedSize = GetSize(tokenList[xOffset]);
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].IntValue, destinationDisplacement: -(int)tokenList[xOffset + 1].IntValue, size: xTypedSize);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REGADDR[1] " + xComparison + " _REG",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register, destinationDisplacement: (int)tokenList[xOffset + 1].IntValue);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REGADDR[-1] " + xComparison + " _REG",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register, destinationDisplacement: -(int)tokenList[xOffset + 1].IntValue);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REGADDR[1] " + xComparison + " #_ABC",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            XSRegisters.RegisterSize? xTypedSize = null;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xTypedSize = GetSize(tokenList[xOffset]);
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, ConstLabel(tokenList[xOffset + 2]), destinationDisplacement: (int)tokenList[xOffset + 1].IntValue, sourceIsIndirect: true, size: xTypedSize);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _REGADDR[-1] " + xComparison + " #_ABC",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            XSRegisters.RegisterSize? xTypedSize = null;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xTypedSize = GetSize(tokenList[xOffset]);
                              xOffset += 1;
                            }
                            XS.Compare(tokenList[xOffset + 0].Register, ConstLabel(tokenList[xOffset + 2]), destinationDisplacement: (int)tokenList[xOffset + 1].IntValue, size: xTypedSize);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _ABC " + xComparison + " 123",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            XSRegisters.RegisterSize? xTypedSize = null;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xTypedSize = GetSize(tokenList[xOffset]);
                              xOffset += 1;
                            }
                            XS.Compare(GetLabel(tokenList[xOffset + 0]), tokenList[xOffset + 2].IntValue, destinationIsIndirect: true, size: xTypedSize.GetValueOrDefault(XSRegisters.RegisterSize.Int32));
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _ABC " + xComparison + " _REG",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            if (tokenList[xOffset + 2].Type != TokenType.Register)
                            {
                              xOffset += 1;
                            }
                            XS.Compare(GetSimpleRef(tokenList[xOffset + 0]), tokenList[xOffset + 2].Register, destinationIsIndirect: true);
                            return xComparisonToken;
                          });
            xCompares.Add(xSize + " _ABC " + xComparison + " #_ABC",
                          (tokenOffset, tokenList) =>
                          {
                            var xOffset = tokenOffset;
                            XSRegisters.RegisterSize? xTypedSize = null;
                            if (tokenList[xOffset].Type != TokenType.Register)
                            {
                              xTypedSize = GetSize(tokenList[xOffset]);
                              xOffset += 1;
                            }
                            XS.Compare(GetSimpleRef(tokenList[xOffset + 0]), ConstLabel(tokenList[xOffset + 3]), destinationIsIndirect: true, size: xTypedSize.GetValueOrDefault(XSRegisters.RegisterSize.Int32));
                            return xComparisonToken;
                          });
          }
        }
      }
      #endregion Handle all comparisons
      // Labels
      // Local and proc level are used most, so designed to make their syntax shortest.
      // Think of the dots like a directory, . is current group, .. is above that.
      // ..Name: - Global level. Emitted exactly as is.
      // .Name: - Group level. Group_Name
      // Name: - Function level. Group_ProcName_Name

      // The Exit label is a special one that is used as a target for the return instruction.
      // It deserve special handling.
      AddPattern("Exit:", delegate(TokenList aTokens)
                          {
                            XS.Label(GetLabel(aTokens[0]));
                            mFuncExitFound = true;
                          });

      // Regular label recognition.
      AddPattern("_ABC:", delegate(TokenList aTokens)
                          {
                            XS.Label(GetLabel(aTokens[0]));
                          });

      AddPattern("Call _ABC", delegate(TokenList aTokens)
                              {
                                XS.Call(GetLabel(aTokens[1]));
                              });
      AddPattern("_PCALL", delegate(TokenList aTokens)
                           {
                             if (aTokens.Count != 1
                                 || aTokens[0].Type != TokenType.Call)
                             {
                               throw new Exception("Error occured in parametrized call parsing");
                             }
                             else
                             {
                               List<string> mparts = aTokens[0].RawValue.Remove(aTokens[0].RawValue.Length - 1).Split('(').ToList();
                               if (mparts.Count < 2)
                               {
                                 throw new Exception("Error occured in parametrized call parsing");
                               }
                               string fname = mparts[0];
                               mparts.RemoveAt(0);
                               aTokens[0].RawValue = String.Join("(", mparts).Trim();
                               string val = "";
                               int idx;

                               var xParams = new List<string>();
                               int level = 0;
                               foreach (char c in aTokens[0].RawValue)
                               {
                                 switch (c)
                                 {
                                   case ',':
                                     if (level == 0)
                                     {
                                       xParams.Add(val.Trim());
                                       val = "";
                                     }
                                     break;
                                   case '(':
                                     level++;
                                     val += c;
                                     break;
                                   case ')':
                                     level--;
                                     val += c;
                                     break;
                                   default:
                                     val += c;
                                     break;
                                 }
                               }
                               if (!String.IsNullOrEmpty(val.Trim()))
                               {
                                 xParams.Add(val);
                               }
                               if (level != 0)
                               {
                                 throw new Exception("'(' occured without closing equivalent ')' in parametrized function call");
                               }

                               Parser xParser;
                               xParams.Reverse();
                               foreach (string p in xParams)
                               {
                                 xParser = new Parser(p, 0, false, false);
                                 idx = 0;
                                 val = GetRef(xParser.Tokens, ref idx);
                                 if (val != "@ret_on_stack@")
                                 {
                                   //XS.PushLiteral(val);
                                   throw new Exception();
                                 }
                                 else
                                 {
                                   //aAsm += GetPatternCode(xParser.Tokens).GetCode(false);
                                   throw new NotImplementedException("Didn't get converted yet!");
                                 }
                               }
                               XS.Call(GroupLabel(fname));
                             }
                           });

      AddPattern("Goto _ABC", delegate(TokenList aTokens)
                              {
                                XS.Jump(GetLabel(aTokens[1]));
                              });

      // Defines a constant having the given name and initial value.
      AddPattern("const _ABC = 123", delegate(TokenList aTokens)
                                     {
                                       XS.Const(ConstLabel(aTokens[1]), aTokens[3].IntValue.ToString());
                                     });

      // Declare a double word variable having the given name and initialized to 0. The
      // variable is declared at namespace level.
      AddPattern("var _ABC", delegate(TokenList aTokens)
                             {
                               XS.DataMember(GetLabel(aTokens[1]));
                             });

      // Declare a doubleword variable having the given name and an explicit initial value. The
      // variable is declared at namespace level.
      AddPattern("var _ABC = 123", delegate(TokenList aTokens)
                                   {
                                     XS.DataMember(GetLabel(aTokens[1]), aTokens[3].IntValue);
                                   });

      // Declare a textual variable having the given name and value. The variable is defined at
      // namespace level and a null terminating byte is automatically added after the textual
      // value.
      AddPattern("var _ABC = 'Text'", delegate(TokenList aTokens)
                                      {
                                        // Fix issue #15660 by using backquotes for string surrounding and escaping embedded
                                        // back quotes.
                                        XS.DataMember(GetLabel(aTokens[1]), EscapeBackQuotes(aTokens[3].RawValue));
                                      });

      // Declare a one-dimension array of bytes, words or doublewords. All members are initialized to 0.
      // _ABC is array name. 123 is the total number of items in the array.
      AddPattern(new string[]
                 {
                   "var _ABC byte[123]", "var _ABC word[123]", "var _ABC dword[123]"
                 }, delegate(TokenList aTokens)
                    {
                      string xSize;
                      if (aTokens[2].Matches("byte"))
                      {
                        xSize = "db";
                      }
                      else if (aTokens[2].Matches("word"))
                      {
                        xSize = "dw";
                      }
                      else if (aTokens[2].Matches("dword"))
                      {
                        xSize = "dd";
                      }
                      else
                      {
                        throw new Exception("Unknown size specified");
                      }
                      XS.DataMember(GetLabel(aTokens[1]), aTokens[4].IntValue, xSize, "0");
                    });

      foreach (var xCompare in xCompares)
      {
        //          0         1  2   3     4
        AddPattern("while " + xCompare.Key + " {", delegate(TokenList aTokens)
                                                   {
                                                     mBlocks.Start(aTokens, false);
                                                     XS.Label(BlockLabel("Begin"));

                                                     Token xComparison = xCompare.Value(1, aTokens);

                                                     XS.Jump(GetJump(xComparison.RawValue, true), BlockLabel("End"));
                                                   });
      }

      foreach (var xTail in "goto _ABC|return|{".Split('|'))
      {
        // if 0 exit, etc
        foreach (var xComparison in mCompareOps)
        {
          AddPattern("if " + xComparison + " " + xTail, delegate(TokenList aTokens)
                                                        {
                                                          string xOp = aTokens[1].RawValue;

                                                          // !0 is 2 tokens
                                                          if (aTokens[1].RawValue + aTokens[2].RawValue == "!0")
                                                          {
                                                            xOp = "!0";
                                                          }

                                                          HandleIf(aTokens, xOp);
                                                        });
        }

        // if reg = x exit, etc
        foreach (var xCompare in xCompares)
        {
          //          0      1  2   3          4
          AddPattern("if " + xCompare.Key + " " + xTail,
                     delegate(TokenList aTokens)
                     {
                       Token xComparison = xCompare.Value(1, aTokens);

                       HandleIf(aTokens, xComparison.RawValue);
                     });
        }
      }

      AddPattern("_REG ?= 123", delegate(TokenList aTokens)
                                {
                                  XS.Compare(aTokens[0].Register, aTokens[2].IntValue);
                                });
      AddPattern("_REG ?= _ABC", delegate(TokenList aTokens)
                                 {
                                   XS.Compare(aTokens[0].Register, GetSimpleRef(aTokens[2]), sourceIsIndirect: true);
                                 });
      AddPattern("_REG ?= #_ABC", delegate(TokenList aTokens)
                                  {
                                    XS.Compare(aTokens[0].Register, ConstLabel(aTokens[2]));
                                  });

      AddPattern("_REG ?& 123", delegate(TokenList aTokens)
                                {
                                  XS.Test(aTokens[0].Register, aTokens[2].IntValue);
                                });
      AddPattern("_REG ?& _ABC", delegate(TokenList aTokens)
                                 {
                                   XS.Test(aTokens[0].Register, GetLabel(aTokens[2]), sourceIsIndirect: true);
                                 });
      AddPattern("_REG ?& #_ABC", delegate(TokenList aTokens)
                                  {
                                    XS.Test(aTokens[0].Register, GetLabel(aTokens[2]), sourceIsIndirect: true);
                                  });

      AddPattern("_REG ~> 123", delegate(TokenList aTokens)
                                {
                                  XS.RotateRight(aTokens[0].Register, aTokens[2].IntValue);
                                });
      AddPattern("_REG <~ 123", delegate(TokenList aTokens)
                                {
                                  XS.RotateLeft(aTokens[0].Register, aTokens[2].IntValue);
                                });
      AddPattern("_REG >> 123", delegate(TokenList aTokens)
                                {
                                  if (aTokens[2].IntValue > 255)
                                  {
                                    throw new IndexOutOfRangeException("Value is too large to be used as bitcount!");
                                  }
                                  XS.ShiftRight(aTokens[0].Register, (byte)aTokens[2].IntValue);
                                });
      AddPattern("_REG << 123", delegate(TokenList aTokens)
                                {
                                  if (aTokens[2].IntValue > 255)
                                  {
                                    throw new IndexOutOfRangeException("Value is too large to be used as bitcount!");
                                  }
                                  XS.ShiftLeft(aTokens[0].Register, (byte)aTokens[2].IntValue);
                                });

      AddPattern("_REG = 123", delegate(TokenList aTokens)
                               {
                                 XS.Set(aTokens[0].Register, aTokens[2].IntValue);
                               });
      AddPattern("_REGADDR[1] = 123", delegate(TokenList aTokens)
                                      {
                                        XS.Set(aTokens[0].Register, aTokens[5].IntValue, destinationDisplacement: (int)aTokens[2].IntValue, size: XSRegisters.RegisterSize.Int32);
                                      });
      AddPattern("_REGADDR[-1] = 123", delegate(TokenList aTokens)
                                       {
                                         XS.Set(aTokens[0].Register, aTokens[5].IntValue, destinationDisplacement: -(int)aTokens[2].IntValue, size: XSRegisters.RegisterSize.Int32);
                                       });

      AddPattern("_REG = #_ABC", delegate(TokenList aTokens)
                                 {
                                   XS.Set(aTokens[0].Register, ConstLabel(aTokens[3]));
                                 });
      AddPattern("_REGADDR[1] = #_ABC", delegate(TokenList aTokens)
                                        {
                                          //XS.Set(RegisterSize.Int32, aTokens[0].IntValue, aTokens[2].IntValue, ConstLabel(aTokens[5]));
                                          throw new NotImplementedException("");
                                        });
      AddPattern("_REGADDR[-1] = #_ABC", delegate(TokenList aTokens)
                                         {
                                           var xFirst = GetSimpleRef(aTokens[0]);
                                           var xSecond = GetSimpleRef(aTokens[2]);

                                           //XS.SetLiteral("dword [" + xFirst + " - " + xSecond + "]", ConstLabel(aTokens[5]));
                                           throw new NotImplementedException("");
                                         });

      AddPattern("_REG = _REG", delegate(TokenList aTokens)
                                {
                                  XS.Set(aTokens[0].Register, aTokens[2].Register);
                                });
      AddPattern("_REGADDR[1] = _REG", delegate(TokenList aTokens)
                                       {
                                         XS.Set(aTokens[0].Register, aTokens[5].Register, destinationDisplacement: (int)aTokens[2].IntValue);
                                       });
      AddPattern("_REGADDR[-1] = _REG", delegate(TokenList aTokens)
                                        {
                                          XS.Set(aTokens[0].Register, aTokens[5].Register, destinationDisplacement: -(int)aTokens[2].IntValue);
                                        });
      AddPattern("_REG = _REGADDR[1]", delegate(TokenList aTokens)
                                       {
                                         XS.Set(aTokens[0].Register, aTokens[2].Register, sourceDisplacement: (int)aTokens[4].IntValue);
                                       });
      AddPattern("_REG = _REGADDR[-1]", delegate(TokenList aTokens)
                                        {
                                          XS.Set(aTokens[0].Register, aTokens[2].Register, sourceDisplacement: -(int)aTokens[4].IntValue);
                                        });

      AddPattern("_REG = [_REG]", delegate(TokenList aTokens)
                                  {
                                    XS.Set(aTokens[0].Register, aTokens[3].Register, sourceIsIndirect: true);
                                  });
      AddPattern("_REG = [_REG + 1]", delegate(TokenList aTokens)
                                      {
                                        XS.Set(aTokens[0].Register, aTokens[3].Register, sourceDisplacement: (int?)aTokens[5].IntValue);
                                      });
      AddPattern("_REG = [_REG - 1]", delegate(TokenList aTokens)
                                      {
                                        XS.Set(aTokens[0].Register, aTokens[3].Register, sourceDisplacement: -(int)aTokens[5].IntValue);
                                      });
      AddPattern("[_REG] = _REG", delegate(TokenList aTokens)
                                  {
                                    XS.Set(aTokens[1].Register, aTokens[4].Register, destinationIsIndirect: true);
                                  });
      AddPattern("[_REG + 1] = _REG", delegate(TokenList aTokens)
                                      {
                                        XS.Set(aTokens[0].Register, aTokens[3].Register, destinationDisplacement: (int)aTokens[5].IntValue);
                                      });
      AddPattern("[_REG - 1] = _REG", delegate(TokenList aTokens)
                                      {
                                        XS.Set(aTokens[0].Register, aTokens[3].Register, destinationDisplacement: -(int)aTokens[5].IntValue);
                                      });

      AddPattern("_REG = _ABC", delegate(TokenList aTokens)
                                {
                                  XS.Set(aTokens[0].Register, GetLabel(aTokens[2]), sourceIsIndirect: true);
                                });

      // why not [var] like registers? Because its less frequent to access the ptr
      // and it is like a reg.. without [] to get the value...
      AddPattern("_REG = @_ABC", delegate(TokenList aTokens)
                                 {
                                   XS.Set(aTokens[0].Register, GetLabel(aTokens[3]));
                                 });

      AddPattern(new string[]
                 {
                   "Port[DX] = AL", "Port[DX] = AX", "Port[DX] = EAX"
                 }, delegate(TokenList aTokens)
                    {
                      XS.WriteToPortDX(aTokens[5].Register);
                    });
      AddPattern(new string[]
                 {
                   "AL = Port[DX]", "AX = Port[DX]", "EAX = Port[DX]"
                 }, delegate(TokenList aTokens)
                    {
                      XS.ReadFromPortDX(aTokens[0].Register);
                    });

      AddPattern("+123", delegate(TokenList aTokens)
                         {
                           XS.Push(aTokens[0].IntValue, size: XSRegisters.RegisterSize.Int32);
                         });
      AddPattern(new string[]
                 {
                   "+123 as byte", "+123 as word", "+123 as dword"
                 }, delegate(TokenList aTokens)
                    {
                      var xSize = GetSize(aTokens[1]);
                      XS.Push(aTokens[1].IntValue, size: xSize);
                    });
      AddPattern("+_REG", delegate(TokenList aTokens)
                          {
                            XS.Push(aTokens[1].Register);
                          });
      AddPattern(new string[]
                 {
                   //0  1  2   3
                   "+#_ABC", "+#_ABC as byte", "+#_ABC as word", "+#_ABC as dword"
                 }, delegate(TokenList aTokens)
                    {
                      XSRegisters.RegisterSize xSize = XSRegisters.RegisterSize.Int32;
                      if (aTokens.Count > 2)
                      {
                        xSize = GetSize(aTokens[3]);
                      }
                      XS.Push(ConstLabel(aTokens[1]), size: xSize);
                    });
      AddPattern("+All", delegate(TokenList aTokens)
                         {
                           XS.PushAllRegisters();
                         });
      AddPattern("-All", delegate(TokenList aTokens)
                         {
                           XS.PopAllRegisters();
                         });
      AddPattern("-_REG", delegate(TokenList aTokens)
                          {
                            XS.Pop(aTokens[1].Register);
                          });

      AddPattern("_ABC = _REG", delegate(TokenList aTokens)
                                {
                                  XS.Set(GetLabel(aTokens[0]), aTokens[2].Register, destinationIsIndirect: true);
                                });
      AddPattern("_ABC = #_ABC", delegate(TokenList aTokens)
                                 {
                                   XS.Set(GetLabel(aTokens[0]), ConstLabel(aTokens[3]), destinationIsIndirect: true, size: XSRegisters.RegisterSize.Int32);
                                 });
      AddPattern("_ABC = 123", delegate(TokenList aTokens)
                               {
                                 XS.Set(GetLabel(aTokens[0]), aTokens[2].IntValue, destinationIsIndirect: true);
                               });
      AddPattern(new string[]
                 {
                   "_ABC = 123 as byte", "_ABC = 123 as word", "_ABC = 123 as dword"
                 }, delegate(TokenList aTokens)
                    {
                      XS.Set(GetLabel(aTokens[0]), aTokens[2].IntValue, size: GetSize(aTokens[4]));
                    });

      AddPattern(new string[]
                 {
                   "_REG + 1",
                 }, delegate(TokenList aTokens)
                    {
                      XS.Add(aTokens[0].Register, aTokens[2].IntValue);
                    });

      AddPattern(new string[]
                 {
                   "_REG + _REG"
                 }, delegate(TokenList aTokens)
                    {
                      XS.Add(aTokens[0].Register, aTokens[2].Register);
                    });
      AddPattern(new string[]
                 {
                   "_REG - 1",
                 }, delegate(TokenList aTokens)
                    {
                      XS.Sub(aTokens[0].Register, aTokens[2].IntValue);
                    });
      AddPattern(new string[]
                 {
                   "_REG - _REG"
                 }, delegate(TokenList aTokens)
                    {
                      XS.Sub(aTokens[0].Register, aTokens[2].Register);
                    });
      AddPattern(new string[]
                 {
                   "_REG * 1",
                 }, delegate(TokenList aTokens)
                    {
                      XS.IntegerMultiply(aTokens[0].Register, aTokens[2].IntValue);
                    });
      AddPattern(new string[]
                 {
                   "_REG * _REG"
                 }, delegate(TokenList aTokens)
                    {
                      XS.IntegerMultiply(aTokens[0].Register, aTokens[2].Register);
                    });
      AddPattern("_REG++", delegate(TokenList aTokens)
                           {
                             XS.Increment(aTokens[0].Register);
                           });
      AddPattern("_REG--", delegate(TokenList aTokens)
                           {
                             XS.Decrement(aTokens[0].Register);
                           });

      AddPattern(new string[]
                 {
                   "_REG & 1",
                 }, delegate(TokenList aTokens)
                    {
                      XS.And(aTokens[0].Register, aTokens[2].IntValue);
                    });
      AddPattern(new string[]
                 {
                   "_REG & _REG"
                 }, delegate(TokenList aTokens)
                    {
                      XS.And(aTokens[0].Register, aTokens[2].Register);
                    });
      AddPattern(new string[]
                 {
                   "_REG | 1",
                 }, delegate(TokenList aTokens)
                    {
                      XS.Or(aTokens[0].Register, aTokens[2].IntValue);
                    });
      AddPattern(new string[]
                 {
                   "_REG | _REG"
                 }, delegate(TokenList aTokens)
                    {
                      XS.Or(aTokens[0].Register, aTokens[2].Register);
                    });
      AddPattern(new string[]
                 {
                   "_REG ^ 1",
                 }, delegate(TokenList aTokens)
                    {
                      XS.Xor(aTokens[0].Register, aTokens[2].IntValue);
                    });
      AddPattern(new string[]
                 {
                   "_REG ^ _REG"
                 }, delegate(TokenList aTokens)
                    {
                      XS.Xor(aTokens[0].Register, aTokens[2].Register);
                    });

      // End block. This handle both terminating a standard block as well as a function or an
      // interrupt handler.
      AddPattern("}", delegate(TokenList aTokens)
                      {
                        if (mBlocks.Count == 0)
                        {
                          EndFunc();
                        }
                        else
                        {
                          var xBlock = mBlocks.Current();
                          var xToken1 = xBlock.StartTokens[0];
                          if (xToken1.Matches("repeat"))
                          {
                            var xCount = xBlock.StartTokens[1].IntValue;
                            for (var i = 1; i <= xCount; i++)
                            {
                              xBlock.AddContentsToParentAssembler();
                            }
                          }
                          else if (xToken1.Matches("while"))
                          {
                            XS.Jump(BlockLabel("Begin"));
                            XS.Label(BlockLabel("End"));
                            xBlock.AddContentsToParentAssembler();
                          }
                          else if (xToken1.Matches("if"))
                          {
                            XS.Label(BlockLabel("End"));
                            xBlock.AddContentsToParentAssembler();
                          }
                          else
                          {
                            throw new Exception("Unknown block starter.");
                          }
                          mBlocks.End();
                        }
                      });

      AddPattern("namespace _ABC", delegate(TokenList aTokens)
                                   {
                                     mNamespace = aTokens[1].RawValue;
                                   });

      AddPattern("Return", delegate
                           {
                             XS.Jump(FuncLabel("Exit"));
                           });

      AddPattern("Repeat 4 times {", delegate(TokenList aTokens)
                                     {
                                       mBlocks.Start(aTokens, true);
                                     });

      AddPattern("Interrupt _ABC {", delegate(TokenList aTokens)
                                     {
                                       StartFunc(aTokens[1].RawValue);
                                       mInIntHandler = true;
                                       XS.Label(GetNamespace() + "_" + aTokens[1].RawValue);
                                     });

      // This needs to be different from return.
      // return jumps to exit, ret does raw x86 ret
      AddPattern("Ret", delegate(TokenList aTokens)
                        {
                          XS.Return();
                        });
      AddPattern("IRet", delegate(TokenList aTokens)
                         {
                           XS.InterruptReturn();
                         });

      AddPattern("Function _ABC {", delegate(TokenList aTokens)
                                    {
                                      StartFunc(aTokens[1].RawValue);
                                      mInIntHandler = false;
                                      XS.Label(GetNamespace() + "_" + aTokens[1].RawValue);
                                    });

      AddPattern("Checkpoint 'Text'", delegate(TokenList aTokens)
                                      {
                                        // This method emits a lot of ASM, but thats what we want becuase
                                        // at this point we need ASM as simple as possible and completely transparent.
                                        // No stack changes, no register mods, no calls, no jumps, etc.

                                        // TODO: Add an option on the debug project properties to turn this off.
                                        // Also see WriteDebugVideo in CosmosAssembler.cs
                                        var xPreBootLogging = true;
                                        if (xPreBootLogging)
                                        {
                                          UInt32 xVideo = 0xB8000;
                                          for (UInt32 i = xVideo; i < xVideo + 80 * 2; i = i + 2)
                                          {
                                            XS.SetByte(i, 0);
                                            XS.SetByte(i + 1, 2);
                                          }

                                          foreach (var xChar in aTokens[1].RawValue)
                                          {
                                            XS.SetByte(xVideo, (byte)xChar);
                                            xVideo = xVideo + 2;
                                          }
                                        }
                                      });
    }

    /// <summary>Fix issue #15660. This method escapes double quotes in the candidate string.</summary>
    /// <param name="from">The string to be sanitized.</param>
    /// <returns>The original string with escaped double quotes.</returns>
    private static string EscapeBackQuotes(string from)
    {
      StringBuilder builder = new StringBuilder();
      bool sanitized = false;
      bool escaped = false;
      foreach (char scannedCharacter in from)
      {
        switch (scannedCharacter)
        {
          case '\\':
            escaped = !escaped;
            break;
          case '`':
            if (!escaped)
            {
              sanitized = true;
              builder.Append('\\');
            }
            escaped = false;
            break;
          default:
            escaped = false;
            break;
        }
        builder.Append(scannedCharacter);
      }
      return (sanitized) ? builder.ToString() : from;
    }

    protected Pattern FindMatch(TokenList aTokens)
    {
      int xHash = aTokens.GetPatternHashCode();

      // Get a list of matching hashes, but then we have to
      // search for exact pattern match because it is possible
      // to have duplicate hashes. Hashes just provide us a quick way
      // to reduce the search.
      foreach (var xPattern in mPatterns.Where(q => q.Hash == xHash))
      {
        if (xPattern.Tokens.PatternMatches(aTokens))
        {
          return xPattern;
        }
      }
      return null;
    }

    public bool GetPatternCode(TokenList aTokens)
    {
      var xPattern = FindMatch(aTokens);
      if (xPattern == null)
      {
        return false;
      }

      xPattern.Code(aTokens);

      //// Apply {0} etc into string
      //// This happens twice for block code, but its ok because the first pass
      //// strips out all tags.
      //for (int i = 0; i < xResult.Code.Count; i++) {
      //  xResult.Code[i] = string.Format(xResult.Code[i], aTokens.ToArray());
      //}
      return true;
    }

    public bool GetNonPatternCode(TokenList aTokens)
    {
      if (aTokens.Count == 0)
      {
        return false;
      }

      var xFirst = aTokens[0];
      var xLast = aTokens[aTokens.Count - 1];

      // Find match and emit X#
      if (aTokens.Count == 2
          && xFirst.Type == TokenType.AlphaNum
          && xLast.Matches("()"))
      {
        // () could be handled by pattern, but best to keep in one place for future
        //xResult += "Call " + GroupLabel(aTokens[0].Value);
        XS.Call(GroupLabel(aTokens[0].RawValue));
      }
      return true;
    }

    public bool GetCode(string aLine, int lineNumber)
    {
      var xParser = new Parser(aLine, lineNumber, false, false);
      var xTokens = xParser.Tokens;

      var xResult = GetPatternCode(xTokens);
      if (!xResult)
      {
        if (!GetNonPatternCode(xTokens))
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>Register a single pattern with its associated transformation handler.</summary>
    /// <param name="aPattern">A single line of X# code that define the pattern optionally using
    /// pattern reserved syntax.</param>
    /// <param name="aCode">The associated code transformation handler.</param>
    protected void AddPattern(string aPattern, CodeFunc aCode)
    {
      Parser xParser = null;
      try
      {
        xParser = new Parser(aPattern, 1, false, true);
      }
      catch (Exception e)
      {
        throw new Exception(string.Format("Invalid pattern '{0}'", aPattern ?? "NULL"), e);
      }
      var xPattern = new Pattern(xParser.Tokens, aCode, aPattern);
      mPatterns.Add(xPattern);
    }

    /// <summary>Register a collection of patterns that share a single transformation handler.
    /// </summary>
    /// <param name="aPatterns">A collection of X# lines of code. Each line of code define a
    /// pattern optionally using the pattern reserved syntax.</param>
    /// <param name="aCode">The code transformation handler that is common abmongst all the
    /// patterns from the collection.</param>
    protected void AddPattern(string[] aPatterns, CodeFunc aCode)
    {
      foreach (var xPattern in aPatterns)
      {
        AddPattern(xPattern, aCode);
      }
    }
  }
}
