using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    protected class Pattern {
      public readonly TokenList Tokens;
      public readonly int Hash;
      public readonly CodeFunc Code;
      public bool RawAsm;

      public Pattern(TokenList aTokens, CodeFunc aCode) {
        Tokens = aTokens;
        Hash = aTokens.GetHashCode();
        Code = aCode;
      }
    }

    protected string mFuncName = null;
    protected bool mFuncExitFound = false;

    public bool EmitUserComments = true;
    public delegate void CodeFunc(TokenList aTokens, ref List<string> rCode);
    protected List<Pattern> mPatterns = new List<Pattern>();
    protected string mGroup;
    protected bool mInIntHandler;
    protected TokenList mBlockStarter = null;
    protected List<string> mBlock = null;

    public TokenPatterns() {
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
    protected string GetLabel(Token aToken) {
      if (aToken.Type != TokenType.AlphaNum) {
        throw new Exception("Label must be AlphaNum.");
      }

      string xValue = aToken.Value;
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
    }

    protected void EndFunc(ref List<string> rCode) {
      if (!mFuncExitFound) {
        rCode.Add(mGroup + "_" + mFuncName + "_Exit:");
      }
      if (mInIntHandler) {
        rCode.Add("IRet");
      } else {
        rCode.Add("Ret");
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

    protected string GetJump(Token aToken) {
      if (aToken.Value == "<") {
        return "JB";  // unsigned
      } else if (aToken.Value == ">") {
        return "JA";  // unsigned
      } else if (aToken.Value == "=") {
        return "JE";
      } else if (aToken.Value == "0") {
        // Same as JE, but implies intent in .asm better
        return "JZ";
      } else if (aToken.Value == "!=") {
        return "JNE";
      } else if (aToken.Value == "<=") {
        return "JBE"; // unsigned
      } else if (aToken.Value == ">=") {
        return "JAE"; // unsigned
      } else {
        throw new Exception("Unrecognized symbol in conditional: " + aToken.Value);
      }
    }

    protected void AddPatterns() {
      var xSpace = " ".ToCharArray();

      AddPattern("! Move EAX, 0", "{0}");

      AddPattern("// Comment", delegate(TokenList aTokens, ref List<string> rCode) {
        if (EmitUserComments) {
          string xValue = aTokens[0].Value;
          xValue = xValue.Replace("\"", "\\\"");
          rCode.Add("; " + xValue);
        }
      });

      // Labels
      // Local and proc level are used most, so designed to make their syntax shortest.
      // Think of the dots like a directory, . is current group, .. is above that.
      // ..Name: - Global level. Emitted exactly as is.
      // .Name: - Group level. Group_Name
      // Name: - Function level. Group_ProcName_Name
      AddPattern("_ABC:", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add(GetLabel(aTokens[0]) + ":");
        if (string.Equals(aTokens[0].Value, "Exit", StringComparison.InvariantCultureIgnoreCase)) {
          mFuncExitFound = true;
        }
      });

      AddPattern("Call _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Call " + GetLabel(aTokens[1]));
      });

      AddPattern("Goto _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Jmp " + GetLabel(aTokens[1]));
      });

      AddPattern("const _ABC = 123", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add(ConstLabel(aTokens[1]) + " equ " + aTokens[3]);
      });

      AddPattern(false, "var _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("mAssembler.DataMembers.Add(new DataMember(" + Quoted(GetLabel(aTokens[1])) + ", 0));");
      });
      AddPattern(false, "var _ABC = 123", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("mAssembler.DataMembers.Add(new DataMember(" + Quoted(GetLabel(aTokens[1])) + ", " + aTokens[3].Value + "));");
      });
      AddPattern(false, "var _ABC = 'Text'", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("mAssembler.DataMembers.Add(new DataMember(" + Quoted(GetLabel(aTokens[1])) + ", \"" + aTokens[3].Value + "\"));");
      });
      AddPattern(false, "var _ABC _ABC[123]", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("mAssembler.DataMembers.Add(new DataMember(" + Quoted(GetLabel(aTokens[1])) + ", new " + aTokens[2].Value + "[" + aTokens[4].Value + "]));");
      });

      // Must test separate since !0 is two tokens
      AddPattern("if !0 goto _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("JNZ " + GetLabel(aTokens[4]));
      });
      AddPattern("if !0 return", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("JNZ " + FuncLabel("Exit"));
      });
      foreach (var xComparison in "< > = != <= >= 0".Split(xSpace)) {
        foreach (var xTail in "goto _ABC|return".Split("|".ToCharArray())) {
          if (xComparison != "0") {
            AddPattern(new string[] {
              //0  1           2            3        4
              "if _REG " + xComparison + " 123 " + xTail,
              "if _REG " + xComparison + " _REG " + xTail, 
              "if _REG " + xComparison + " _ABC " + xTail, 
              //                           3  4        5
              "if _REG " + xComparison + " #_ABC " + xTail, 
              "if _ABC " + xComparison + " #_ABC " + xTail, 
            }, delegate(TokenList aTokens, ref List<string> rCode) {
              int xTailIdx = 4;

              string xLeft = aTokens[1].Value;
              if (aTokens[1].Type == TokenType.AlphaNum) {
                // Hardcoded to dword for now
                xLeft = "dword [" + GetLabel(aTokens[1]) + "]";
              }

              string xRight = aTokens[3].Value;
              if (aTokens[3].Type == TokenType.AlphaNum) {
                xRight = "[" + GetLabel(aTokens[3]) + "]";
              } else if (aTokens[3].Value == "#") {
                xRight = ConstLabel(aTokens[4]);
                xTailIdx = 5;
              }
              rCode.Add("Cmp " + xLeft + ", " + xRight);

              string xLabel;
              if (aTokens[xTailIdx].Matches("return")) {
                xLabel = FuncLabel("Exit");
              } else {
                xLabel = GetLabel(aTokens[xTailIdx + 1]);
              }

              rCode.Add(GetJump(aTokens[2]) + " " + xLabel);
            });
          }
          AddPattern("if " + xComparison + " " + xTail, delegate(TokenList aTokens, ref List<string> rCode) {
            string xLabel;
            if (string.Equals(aTokens[2].Value, "exit", StringComparison.InvariantCultureIgnoreCase)) {
              xLabel = FuncLabel("Exit");
            } else {
              xLabel = GetLabel(aTokens[3]);
            }
            rCode.Add(GetJump(aTokens[1]) + " " + xLabel);
          });
        }
      }

      AddPattern("_REG ?= 123", "Cmp {0}, {2}");
      AddPattern("_REG ?= _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Cmp {0}, " + GetLabel(aTokens[2]));
      });
      AddPattern("_REG ?= #_ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Cmp {0}, " + ConstLabel(aTokens[3]));
      });

      AddPattern("_REG ?& 123", "Test {0}, {2}");
      AddPattern("_REG ?& _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Test {0}, " + GetLabel(aTokens[2]));
      });
      AddPattern("_REG ?& #_ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Test {0}, " + ConstLabel(aTokens[3]));
      });

      AddPattern("_REG ~> 123", "ROR {0}, {2}");
      AddPattern("_REG <~ 123", "ROL {0}, {2}");
      AddPattern("_REG >> 123", "SHR {0}, {2}");
      AddPattern("_REG << 123", "SHL {0}, {2}");

      AddPattern("_REG = 123", "Mov {0}, {2}");
      AddPattern(new string[] {
          "_REG32[1] = 123",
          "_REGIDX[1] = 123"
        },
          "Mov dword [{0} + {2}], {5}"
      );
      AddPattern(new string[] {
          "_REG32[-1] = 123",
          "_REGIDX[-1] = 123"
        },
          "Mov dword [{0} - {2}], {5}"
      );

      AddPattern("_REG = #_ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Mov {0}, " + ConstLabel(aTokens[3]));
      });
      AddPattern(new string[] {
          "_REG32[1] = 123",
          "_REGIDX[1] = 123"
        }, delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Mov dword [{0} + {2}], " + ConstLabel(aTokens[5]));
      });
      AddPattern(new string[] {
          "_REG32[-1] = 123",
          "_REGIDX[-1] = 123"
        }, delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Mov dword [{0} - {2}], " + ConstLabel(aTokens[5]));
      });

      AddPattern("_REG = _REG", "Mov {0}, {2}");
      AddPattern(new string[] {
        "_REG32[1] = _REG", 
        "_REGIDX[1] = _REG"},
        //
        "Mov [{0} + {2}], {5}"
      );
      AddPattern(new string[] {
        "_REG32[-1] = _REG", 
        "_REGIDX[-1] = _REG"},
        //
        "Mov [{0} - {3}], {6}"
      );
      AddPattern(new string[] { 
        "_REG = _REG32[1]",
        "_REG = _REGIDX[1]"},
        //
        "Mov {0}, [{2} + {4}]"
      );
      AddPattern(new string[] { 
        "_REG = _REG32[-1]",
        "_REG = _REGIDX[-1]"},
        //
        "Mov {0}, [{2} - {5}]"
      );

      AddPattern("_REG = _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Mov {0}, [" + GetLabel(aTokens[2]) + "]");
      });
      // why not [var] like registers? Because its less frequent to access th ptr
      // and it is like a reg.. without [] to get the value...
      AddPattern("_REG = @_ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Mov {0}, " + GetLabel(aTokens[3]));
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
        }, delegate(TokenList aTokens, ref List<string> rCode) {
          string xSize = "dword ";
          if (aTokens.Count > 2) {
            xSize = aTokens[3].Value + " ";
          }
          rCode.Add("Push " + xSize + ConstLabel(aTokens[1]));
      });
      AddPattern("+All", "Pushad");
      AddPattern("-All", "Popad");
      AddPattern("-_REG", "Pop {1}");

      AddPattern("_ABC = _REG",
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("Mov [" + GetLabel(aTokens[0]) + "], {2}");
        });
      AddPattern("_ABC = 123", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Mov dword [" + GetLabel(aTokens[0]) + "], {2}");
      });
      AddPattern(new string[] {
        "_ABC = 123 as byte",
        "_ABC = 123 as word",
        "_ABC = 123 as dword"},
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("Mov {4} [" + GetLabel(aTokens[0]) + "], {2}");
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

      AddPattern("}", delegate(TokenList aTokens, ref List<string> rCode) {
        if (mBlock == null) {
          EndFunc(ref rCode);
        } else {
          if (mBlockStarter.PatternMatches("repeat 4 times {")) {
            int xCount = int.Parse(mBlockStarter[1].Value);
            for (int i = 1; i <= xCount; i++) {
              rCode.AddRange(mBlock);
            }
            mBlockStarter = null;
            mBlock = null;
          } else {
            throw new Exception("Unknown block starter.");
          }
        }
      });

      AddPattern("Group _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        mGroup = aTokens[1].Value;
      });

      AddPattern("Exit", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("Jmp " + FuncLabel("Exit"));
      });

      AddPattern("Repeat 4 times {", delegate(TokenList aTokens, ref List<string> rCode) {
        mBlockStarter = aTokens;
        mBlock = new List<string>();
      });

      AddPattern("Interrupt _ABC {", delegate(TokenList aTokens, ref List<string> rCode) {
        StartFunc(aTokens[1].Value);
        mInIntHandler = true;
        rCode.Add(mGroup + "_{1}:");
      });

      AddPattern("Return", "Ret");
      AddPattern("ReturnInterrupt", "IRet");
      AddPattern("PopAll", "Popad");
      AddPattern("PushAll", "Pushad");

      AddPattern("Function _ABC {", delegate(TokenList aTokens, ref List<string> rCode) {
        StartFunc(aTokens[1].Value);
        mInIntHandler = false;
        rCode.Add(mGroup + "_{1}:");
      });

      AddPattern("Checkpoint 'Text'", delegate(TokenList aTokens, ref List<string> rCode) {
        // This method emits a lot of ASM, but thats what we want becuase
        // at this point we need ASM as simple as possible and completely transparent.
        // No stack changes, no register mods, no calls, no jumps, etc.

        // TODO: Add an option on the debug project properties to turn this off.
        // Also see WriteDebugVideo in CosmosAssembler.cs
        var xPreBootLogging = true;
        if (xPreBootLogging) {
          UInt32 xVideo = 0xB8000;
          for (UInt32 i = xVideo; i < xVideo + 80 * 2; i = i + 2) {
            rCode.Add("mov byte [0x" + i.ToString("X") + "], 0");
            rCode.Add("mov byte [0x" + (i + 1).ToString("X") + "], 0x02");
          }

          foreach (var xChar in aTokens[1].Value) {
            rCode.Add("mov byte [0x" + xVideo.ToString("X") + "], " + (byte)xChar);
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

    void AsmToCSharp(List<string> aLines) {
      // Don't reformat if we are in a block because then this will occur twice
      if (mBlock != null) {
        return;
      }

      for (int i = 0; i < aLines.Count; i++) {
        aLines[i] = "new LiteralAssemblerCode(\"" + aLines[i] + "\");";
      }
    }

    public List<string> GetPatternCode(TokenList aTokens) {
      var xPattern = FindMatch(aTokens);
      if (xPattern == null) {
        return null;
      }

      var xResult = new List<string>();
      xPattern.Code(aTokens, ref xResult);
      
      // Apply {0} etc into string
      // This happens twice for block code, but its ok because the first pass
      // strips out all tags.
      for (int i = 0; i < xResult.Count; i++) {
        xResult[i] = string.Format(xResult[i], aTokens.ToArray());
      }

      if (xPattern.RawAsm) {
        AsmToCSharp(xResult);
      }

      return xResult;
    }

    public List<string> GetNonPatternCode(TokenList aTokens) {
      if (aTokens.Count == 0) {
        return null;
      }

      var xFirst = aTokens[0];
      var xLast = aTokens[aTokens.Count - 1];
      var xResult = new List<string>();

      // Find match and emit X#
      if (aTokens.Count == 2
        && xFirst.Type == TokenType.AlphaNum
        && xLast.Matches("()")
        ) {
        // () could be handled by pattern, but best to keep in one place for future
        xResult.Add("Call " + GroupLabel(aTokens[0].Value));

      } else {
        // No matches
        return null;
      }

      AsmToCSharp(xResult);
      return xResult;
    }

    public List<string> GetCode(string aLine) {
      var xParser = new Parser(aLine, false, false);
      var xTokens = xParser.Tokens;
      var xResult = GetPatternCode(xTokens);
      if (xResult == null) {
        xResult = GetNonPatternCode(xTokens);
      }

      if (mBlock != null) {
        mBlock.AddRange(xResult);
        xResult.Clear();
      }
      return xResult;
    }

    protected void AddPattern(string aPattern, string aCode) {
      AddPattern(true, aPattern, aCode);
    }
    protected void AddPattern(string aPattern, CodeFunc aCode) {
      AddPattern(true, aPattern, aCode);
    }
    protected void AddPattern(string[] aPatterns, string aCode) {
      AddPattern(true, aPatterns, aCode);
    }
    protected void AddPattern(string[] aPatterns, CodeFunc aCode) {
      AddPattern(true, aPatterns, aCode);
    }

    protected void AddPattern(bool aRawAsm, string aPattern, CodeFunc aCode) {
      var xParser = new Parser(aPattern, false, true);
      var xPattern = new Pattern(xParser.Tokens, aCode);
      xPattern.RawAsm = aRawAsm;
      mPatterns.Add(xPattern);
    }
    protected void AddPattern(bool aRawAsm, string[] aPatterns, CodeFunc aCode) {
      foreach (var xPattern in aPatterns) {
        AddPattern(aRawAsm, xPattern, aCode);
      }
    }
    protected void AddPattern(bool aRawAsm, string aPattern, string aCode) {
      AddPattern(aRawAsm, aPattern, delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add(aCode);
      });
    }
    protected void AddPattern(bool aRawAsm, string[] aPatterns, string aCode) {
      foreach (var xPattern in aPatterns) {
        AddPattern(aRawAsm, xPattern, aCode);
      }
    }

  }
}
