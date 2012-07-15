using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using XSharp.Nasm;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    protected class Pattern {
      public readonly TokenList Tokens;
      public readonly int Hash;
      public readonly CodeFunc Code;

      public Pattern(TokenList aTokens, CodeFunc aCode) {
        Tokens = aTokens;
        Hash = aTokens.GetHashCode();
        Code = aCode;
      }
    }

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
        if (aIsCollector || (Count > 0 && Current().Contents != null)) {
          xBlock.Contents = new List<string>();
        }
        // Last because we use Current() above
        Add(xBlock);
      }

      public void End() {
        RemoveAt(Count - 1);
      }
    }
    protected class Block {
      public TokenList StartTokens;
      public List<string> Contents;
      public int LabelID;
    }

    protected string mFuncName = null;
    protected bool mFuncExitFound = false;

    public bool EmitUserComments = true;
    public delegate void CodeFunc(TokenList aTokens, Assembler aAsm);
    protected List<Pattern> mPatterns = new List<Pattern>();
    protected string mGroup;
    protected bool mInIntHandler;
    protected string[] mCompareOps;
    protected List<string> mCompares = new List<string>();

    public TokenPatterns() {
      mCompareOps = "< > = != <= >= 0 !0".Split(" ".ToCharArray());
      var xSizes = "byte , word , dword ".Split(",".ToCharArray()).ToList();
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

    protected string Quoted(string aString) {
      return "\"" + aString + "\"";
    }

    protected int IntValue(Token aToken) {
      if (aToken.Value.StartsWith("0x")) {
        return int.Parse(aToken.Value.Substring(2), NumberStyles.AllowHexSpecifier);
      } else {
        return int.Parse(aToken.Value);
      }
    }

    protected string ConstLabel(Token aToken) {
      return GroupLabel("Const_" + aToken);
    }
    protected string GroupLabel(string aLabel) {
      return mGroup + "_" + aLabel;
    }
    protected string FuncLabel(string aLabel) {
      return mGroup + "_" + mFuncName + "_" + aLabel;
    }
    protected string BlockLabel(string aLabel) {
      return FuncLabel("Block" + mBlocks.Current().LabelID + "_" + aLabel);
    }
    protected string GetLabel(Token aToken) {
      if (aToken.Type != TokenType.AlphaNum && !aToken.Matches("exit")) {
        throw new Exception("Label must be AlphaNum.");
      }

      string xValue = aToken;
      if (mFuncName == null) {
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

    protected void StartFunc(string aName) {
      mFuncName = aName;
      mFuncExitFound = false;
      mBlocks.Reset();
    }

    protected void EndFunc(Assembler aAsm) {
      if (!mFuncExitFound) {
        aAsm += mGroup + "_" + mFuncName + "_Exit:";
      }
      if (mInIntHandler) {
        aAsm += "IRet";
      } else {
        aAsm += "Ret";
      }
      mFuncName = null;
    }

    protected string GetDestRegister(TokenList aTokens, int aIdx) {
      return GetRegister("Destination", aTokens, aIdx);
    }
    protected string GetSrcRegister(TokenList aTokens, int aIdx) {
      return GetRegister("Source", aTokens, aIdx);
    }
    protected string GetRegister(string aPrefix, TokenList aTokens, int aIdx) {
      var xToken = aTokens[aIdx].Type;
      Token xNext = null;
      if (aIdx + 1 < aTokens.Count) {
        xNext = aTokens[aIdx + 1];
      }

      string xResult = aPrefix + "Reg = RegistersEnum." + aTokens[aIdx].Value;
      if (xNext != null) {
        if (xNext.Value == "[") {
          string xDisplacement;
          if (aTokens[aIdx + 2].Value == "-") {
            xDisplacement = "-" + aTokens[aIdx + 2].Value;
          } else {
            xDisplacement = aTokens[aIdx + 2].Value;
          }
          xResult = xResult + ", " + aPrefix + "IsIndirect = true, " + aPrefix + "Displacement = " + xDisplacement;
        }
      }
      return xResult;
    }

    protected string GetRef(TokenList aTokens, ref int rIdx) {
      var xToken1 = aTokens[rIdx];
      Token xToken2 = null;
      if (rIdx + 1 < aTokens.Count) {
        xToken2 = aTokens[rIdx + 1];
      }
      if (xToken1.Type == TokenType.Register) {
        if (xToken2 != null && xToken2.Value == "[") {
          if (aTokens[rIdx + 2].Value == "-") {
            rIdx += 5;
            return "[" + xToken1 + " - " + aTokens[rIdx - 2] + "]";
          }
          rIdx += 4;
          return "[" + xToken1 + " + " + aTokens[rIdx - 2] + "]";
        }
        rIdx += 1;
        return xToken1;

      } else if (xToken1.Type == TokenType.AlphaNum) {
        rIdx += 1;
        return "[" + GetLabel(xToken1) + "]";

      } else if (xToken1.Type == TokenType.ValueInt) {
        rIdx += 1;
        return xToken1;

      } else if (xToken1.Value == "#") {
        rIdx += 2;
        return ConstLabel(xToken2);

      } else {
        throw new Exception("Cannot determine reference");
      }
    }

    protected void DoCompare(Assembler aAsm, TokenList aTokens, ref int rStart, out Token aComparison) {
      string xSize = "";
      if (aTokens[rStart].Type == TokenType.Keyword) {
        xSize = aTokens[rStart];
        rStart++;
      }

      string xLeft = GetRef(aTokens, ref rStart);

      aComparison = aTokens[rStart];
      rStart++;

      string xRight = GetRef(aTokens, ref rStart);

      aAsm.Cmp(xSize, xLeft, xRight);
    }

    protected string GetJump(string aComparison) {
      return GetJump(aComparison, false);
    }
    protected string GetJump(string aComparison, bool aInvert) {
      if (aInvert) {
        if (aComparison == "<") {
          aComparison = ">=";
        } else if (aComparison == ">") {
          aComparison = "<=";
        } else if (aComparison == "=") {
          aComparison = "!=";
        } else if (aComparison == "0") {
          // Same as JE, but implies intent in .asm better
          aComparison = "!0";
        } else if (aComparison == "!0") {
          // Same as JE, but implies intent in .asm better
          aComparison = "0";
        } else if (aComparison == "!=") {
          aComparison = "=";
        } else if (aComparison == "<=") {
          aComparison = ">";
        } else if (aComparison == ">=") {
          aComparison = "<";
        } else {
          throw new Exception("Unrecognized symbol in conditional: " + aComparison);
        }
      }

      if (aComparison == "<") {
        return "JB";  // unsigned
      } else if (aComparison == ">") {
        return "JA";  // unsigned
      } else if (aComparison == "=") {
        return "JE";
      } else if (aComparison == "0") {
        // Same as JE, but implies intent in .asm better
        return "JZ";
      } else if (aComparison == "!=") {
        return "JNE";
      } else if (aComparison == "!0") {
        // Same as JNE, but implies intent in .asm better
        return "JNZ";
      } else if (aComparison == "<=") {
        return "JBE"; // unsigned
      } else if (aComparison == ">=") {
        return "JAE"; // unsigned
      } else {
        throw new Exception("Unrecognized symbol in conditional: " + aComparison);
      }
    }

    protected void HandleIf(Assembler aAsm, TokenList aTokens, string xComparison) {
      string xLabel;
      var xLast = aTokens.Last();
      if (xLast.Value == "{") {
        mBlocks.Start(aTokens, false);
        aAsm += GetJump(xComparison, true) + " " + BlockLabel("End");
      } else {
        if (xLast.Matches("return")) {
          xLabel = FuncLabel("Exit");
        } else {
          xLabel = GetLabel(xLast);
        }

        aAsm += GetJump(xComparison) + " " + xLabel;
      }
    }

    protected void AddPatterns() {
      AddPattern("! Mov EAX, 0", "{0}");

      AddPattern("// Comment", delegate(TokenList aTokens, Assembler aAsm) {
        if (EmitUserComments) {
          string xValue = aTokens[0].Value;
          xValue = xValue.Replace("\"", "\\\"");
          aAsm += "; " + xValue;
        }
      });

      // Labels
      // Local and proc level are used most, so designed to make their syntax shortest.
      // Think of the dots like a directory, . is current group, .. is above that.
      // ..Name: - Global level. Emitted exactly as is.
      // .Name: - Group level. Group_Name
      // Name: - Function level. Group_ProcName_Name
      AddPattern("Exit:", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += GetLabel(aTokens[0]) + ":";
        mFuncExitFound = true;
      });
      AddPattern("_ABC:", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += GetLabel(aTokens[0]) + ":";
      });

      AddPattern("Call _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Call " + GetLabel(aTokens[1]);
      });

      AddPattern("Goto _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Jmp " + GetLabel(aTokens[1]);
      });

      AddPattern("const _ABC = 123", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += ConstLabel(aTokens[1]) + " equ " + aTokens[3];
      });

      AddPattern("var _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Data.Add(GetLabel(aTokens[1]) + " dd 0");
      });
      AddPattern("var _ABC = 123", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Data.Add(GetLabel(aTokens[1]) + " dd " + aTokens[3].Value);
      });
      AddPattern("var _ABC = 'Text'", delegate(TokenList aTokens, Assembler aAsm) {
        // , 0 adds null term to our strings.
        aAsm.Data.Add(GetLabel(aTokens[1]) + " db \"" + aTokens[3].Value + "\", 0");
      });
      AddPattern(new string[] {
        "var _ABC byte[123]",
        "var _ABC word[123]",
        "var _ABC dword[123]"
      }, delegate(TokenList aTokens, Assembler aAsm) {
        string xSize;
        if (aTokens[2].Matches("byte")) {
          xSize = "db";
        } else if (aTokens[2].Matches("word")) {
          xSize = "dw";
        } else if (aTokens[2].Matches("dword")) {
          xSize = "dd";
        } else {
          throw new Exception("Unknown size specified");
        }
        aAsm.Data.Add(GetLabel(aTokens[1]) + " TIMES " + aTokens[4].Value + " " + xSize + " 0");
      });

      foreach (var xCompare in mCompares) {
        //          0         1  2   3     4
        AddPattern("while " + xCompare + " {", delegate(TokenList aTokens, Assembler aAsm) {
          mBlocks.Start(aTokens, false);
          aAsm += BlockLabel("Begin") + ":";

          int xIdx = 1;
          Token xComparison;
          DoCompare(aAsm, aTokens, ref xIdx, out xComparison);

          aAsm += GetJump(xComparison, true) + " " + BlockLabel("End");
        });
      }

      foreach (var xTail in "goto _ABC|return|{".Split("|".ToCharArray())) {
        // if 0 exit, etc
        foreach (var xComparison in mCompareOps) {
          AddPattern("if " + xComparison + " " + xTail, delegate(TokenList aTokens, Assembler aAsm) {
            string xOp = aTokens[1];
            // !0 is 2 tokens
            if (aTokens[2] == "0") {
              xOp = "!0";
            }

            HandleIf(aAsm, aTokens, xComparison);
          });
        }

        // if reg = x exit, etc
        foreach (var xCompare in mCompares) {
          //          0      1  2   3          4
          AddPattern("if " + xCompare + " " + xTail, delegate(TokenList aTokens, Assembler aAsm) {
            int xIdx = 1;
            Token xComparison;
            DoCompare(aAsm, aTokens, ref xIdx, out xComparison);

            HandleIf(aAsm, aTokens, xComparison);
          });
        }
      }

      AddPattern("_REG ?= 123", "Cmp {0}, {2}");
      AddPattern("_REG ?= _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Cmp {0}, " + GetLabel(aTokens[2]);
      });
      AddPattern("_REG ?= #_ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Cmp {0}, " + ConstLabel(aTokens[3]);
      });

      AddPattern("_REG ?& 123", "Test {0}, {2}");
      AddPattern("_REG ?& _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Test {0}, " + GetLabel(aTokens[2]);
      });
      AddPattern("_REG ?& #_ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Test {0}, " + ConstLabel(aTokens[3]);
      });

      AddPattern("_REG ~> 123", "ROR {0}, {2}");
      AddPattern("_REG <~ 123", "ROL {0}, {2}");
      AddPattern("_REG >> 123", "SHR {0}, {2}");
      AddPattern("_REG << 123", "SHL {0}, {2}");

      AddPattern("_REG = 123", "Mov {0}, {2}");
      AddPattern("_REGADDR[1] = 123", "Mov dword [{0} + {2}], {5}");
      AddPattern("_REGADDR[-1] = 123", "Mov dword [{0} - {2}], {5}");

      AddPattern("_REG = #_ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Mov {0}, " + ConstLabel(aTokens[3]);
      });
      AddPattern("_REGADDR[1] = 123", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Mov("dword", "[{0} + {2}]", ConstLabel(aTokens[5]));
      });
      AddPattern("_REGADDR[-1] = 123", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Mov("dword", "[{0} - {2}]", ConstLabel(aTokens[5]));
      });

      AddPattern("_REG = _REG", "Mov {0}, {2}");
      AddPattern("_REGADDR[1] = _REG",  "Mov [{0} + {2}], {5}");
      AddPattern("_REGADDR[-1] = _REG", "Mov [{0} - {3}], {6}");
      AddPattern("_REG = _REGADDR[1]", "Mov {0}, [{2} + {4}]");
      AddPattern("_REG = _REGADDR[-1]", "Mov {0}, [{2} - {5}]");

      AddPattern("_REG = _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Mov(aTokens[0], "[" + GetLabel(aTokens[2]) + "]");
      });
      // why not [var] like registers? Because its less frequent to access the ptr
      // and it is like a reg.. without [] to get the value...
      AddPattern("_REG = @_ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Mov(aTokens[0], GetLabel(aTokens[3]));
      });

      AddPattern(new string[] { 
          "Port[DX] = AL", 
          "Port[DX] = AX", 
          "Port[DX] = EAX"
        },
        "Out DX, {5}"
      );
      AddPattern(new string[] { 
        "AL = Port[DX]", 
        "AX = Port[DX]", 
        "EAX = Port[DX]"},
        //
        "In {0}, DX"
      );

      AddPattern("+123", "Push dword {1}");
      AddPattern(new string[] {
        "+123 as byte",
        "+123 as word",
        "+123 as dword"
      }, "Push {3} {1}");
      AddPattern("+_REG", "Push {1}");
      AddPattern(new string[] {
        //0  1  2   3
        "+#_ABC",
        "+#_ABC as byte",
        "+#_ABC as word",
        "+#_ABC as dword"
        }, delegate(TokenList aTokens, Assembler aAsm) {
          string xSize = "dword ";
          if (aTokens.Count > 2) {
            xSize = aTokens[3].Value + " ";
          }
          aAsm += "Push " + xSize + ConstLabel(aTokens[1]);
      });
      AddPattern("+All", "Pushad");
      AddPattern("-All", "Popad");
      AddPattern("-_REG", "Pop {1}");

      AddPattern("_ABC = _REG",
        delegate(TokenList aTokens, Assembler aAsm) {
          aAsm.Mov("[" + GetLabel(aTokens[0]) + "]", aTokens[2]);
        });
      AddPattern("_ABC = #_ABC",
        delegate(TokenList aTokens, Assembler aAsm) {
          aAsm.Mov("dword", "[" + GetLabel(aTokens[0]) + "]", ConstLabel(aTokens[3]));
        });
      AddPattern("_ABC = 123", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Mov("dword", "[" + GetLabel(aTokens[0]) + "]", aTokens[2]);
      });
      AddPattern(new string[] {
        "_ABC = 123 as byte",
        "_ABC = 123 as word",
        "_ABC = 123 as dword"},
        delegate(TokenList aTokens, Assembler aAsm) {
          aAsm += "Mov {4} [" + GetLabel(aTokens[0]) + "], {2}";
        });

      // TODO: Allow asm to optimize these to Inc/Dec
      AddPattern(new string[] {
        "_REG + 1",
        "_REG + _REG"
      }, "Add {0}, {2}");
      AddPattern(new string[] {
        "_REG - 1",
        "_REG - _REG"
      }, "Sub {0}, {2}");
      AddPattern("_REG++", "Inc {0}");
      AddPattern("_REG--", "Dec {0}");

      // End block
      AddPattern("}", delegate(TokenList aTokens, Assembler aAsm) {
        if (mBlocks.Count == 0) {
          EndFunc(aAsm);
        } else {
          var xBlock = mBlocks.Current();
          var xToken1 = xBlock.StartTokens[0];
          if (xToken1.Matches("repeat")) {
            int xCount = int.Parse(xBlock.StartTokens[1]);
            for (int i = 1; i <= xCount; i++) {
              aAsm.Code.AddRange(xBlock.Contents);
            }

          } else if (xToken1.Matches("while")) {
            aAsm += "jmp " + BlockLabel("Begin");
            aAsm += BlockLabel("End") + ":";

          } else if (xToken1.Matches("if")) {
            aAsm += BlockLabel("End") + ":";

          } else {
            throw new Exception("Unknown block starter.");
          }
          mBlocks.End();
        }
      });

      AddPattern("Group _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        mGroup = aTokens[1].Value;
      });

      AddPattern("Return", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Jmp " + FuncLabel("Exit");
      });

      AddPattern("Repeat 4 times {", delegate(TokenList aTokens, Assembler aAsm) {
        mBlocks.Start(aTokens, true);
      });

      AddPattern("Interrupt _ABC {", delegate(TokenList aTokens, Assembler aAsm) {
        StartFunc(aTokens[1].Value);
        mInIntHandler = true;
        aAsm += mGroup + "_{1}:";
      });

      // This needs to be different from return.
      // return jumps to exit, ret does raw x86 ret
      AddPattern("Ret", "Ret");
      AddPattern("IRet", "IRet");

      AddPattern("Function _ABC {", delegate(TokenList aTokens, Assembler aAsm) {
        StartFunc(aTokens[1].Value);
        mInIntHandler = false;
        aAsm += mGroup + "_{1}:";
      });

      AddPattern("Checkpoint 'Text'", delegate(TokenList aTokens, Assembler aAsm) {
        // This method emits a lot of ASM, but thats what we want becuase
        // at this point we need ASM as simple as possible and completely transparent.
        // No stack changes, no register mods, no calls, no jumps, etc.

        // TODO: Add an option on the debug project properties to turn this off.
        // Also see WriteDebugVideo in CosmosAssembler.cs
        var xPreBootLogging = true;
        if (xPreBootLogging) {
          UInt32 xVideo = 0xB8000;
          for (UInt32 i = xVideo; i < xVideo + 80 * 2; i = i + 2) {
            aAsm += "mov byte [0x" + i.ToString("X") + "], 0";
            aAsm += "mov byte [0x" + (i + 1).ToString("X") + "], 0x02";
          }

          foreach (var xChar in aTokens[1].Value) {
            aAsm += "mov byte [0x" + xVideo.ToString("X") + "], " + (byte)xChar;
            xVideo = xVideo + 2;
          }
        }
      });
    }

    protected Pattern FindMatch(TokenList aTokens) {
      int xHash = aTokens.GetPatternHashCode();
      // Get a list of matching hashes, but then we have to 
      // search for exact pattern match because it is possible
      // to have duplicate hashes. Hashes just provide us a quick way
      // to reduce the search.
      foreach (var xPattern in mPatterns.Where(q => q.Hash == xHash)) {
        if (xPattern.Tokens.PatternMatches(aTokens)) {
          return xPattern;
        }
      }
      return null;
    }

    public Assembler GetPatternCode(TokenList aTokens) {
      var xPattern = FindMatch(aTokens);
      if (xPattern == null) {
        return null;
      }

      var xResult = new Assembler();
      xPattern.Code(aTokens, xResult);
      
      // Apply {0} etc into string
      // This happens twice for block code, but its ok because the first pass
      // strips out all tags.
      for (int i = 0; i < xResult.Code.Count; i++) {
        xResult.Code[i] = string.Format(xResult.Code[i], aTokens.ToArray());
      }

      return xResult;
    }

    public Assembler GetNonPatternCode(TokenList aTokens) {
      if (aTokens.Count == 0) {
        return null;
      }

      var xFirst = aTokens[0];
      var xLast = aTokens[aTokens.Count - 1];
      var xResult = new Assembler();

      // Find match and emit X#
      if (aTokens.Count == 2
        && xFirst.Type == TokenType.AlphaNum
        && xLast.Matches("()")
        ) {
        // () could be handled by pattern, but best to keep in one place for future
        xResult += "Call " + GroupLabel(aTokens[0].Value);

      } else {
        // No matches
        return null;
      }

      return xResult;
    }

    public Assembler GetCode(string aLine) {
      var xParser = new Parser(aLine, false, false);
      var xTokens = xParser.Tokens;
      var xResult = GetPatternCode(xTokens);
      if (xResult == null) {
        xResult = GetNonPatternCode(xTokens);
      }

      if (mBlocks.Count > 0 && mBlocks.Current().Contents != null) {
        mBlocks.Current().Contents.AddRange(xResult.Code);
        xResult.Code.Clear();
      }
      return xResult;
    }

    protected void AddPattern(string aPattern, CodeFunc aCode) {
      var xParser = new Parser(aPattern, false, true);
      var xPattern = new Pattern(xParser.Tokens, aCode);
      mPatterns.Add(xPattern);
    }
    protected void AddPattern(string[] aPatterns, CodeFunc aCode) {
      foreach (var xPattern in aPatterns) {
        AddPattern(xPattern, aCode);
      }
    }
    protected void AddPattern(string aPattern, string aCode) {
      AddPattern(aPattern, delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += aCode;
      });
    }
    protected void AddPattern(string[] aPatterns, string aCode) {
      foreach (var xPattern in aPatterns) {
        AddPattern(xPattern, aCode);
      }
    }

  }
}
