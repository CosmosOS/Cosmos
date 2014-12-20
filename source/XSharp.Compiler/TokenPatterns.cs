using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using XSharp.Nasm;

namespace XSharp.Compiler {
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

      public Pattern(TokenList aTokens, CodeFunc aCode) {
        Tokens = aTokens;
        Hash = aTokens.GetHashCode();
        Code = aCode;
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
      mCompareOps = "< > = != <= >= 0 !0".Split(" ".ToCharArray());
      var xSizes = "byte , word , dword ".Split(",".ToCharArray()).ToList();
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

    // BlueSkeye : Seems to be unused. Quoted out.
    //protected string Quoted(string aString) {
    //  return "\"" + aString + "\"";
    //}

    // BlueSkeye : Seems to be unused. Quoted out.
    //protected int IntValue(Token aToken)
    //{
    //  if (aToken.Value.StartsWith("0x")) {
    //    return int.Parse(aToken.Value.Substring(2), NumberStyles.AllowHexSpecifier);
    //  } else {
    //    return int.Parse(aToken.Value);
    //  }
    //}

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

      string xValue = aToken;
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
    /// <param name="aAsm"></param>
    protected void EndFunc(Assembler aAsm) {
      if (null == mFuncName) {
        throw new Exception("Found a closing curly brace that doesn't match an opening curly brace.");
      }
      if (!mFuncExitFound) {
        aAsm += GetNamespace() + "_" + mFuncName + "_Exit:";
      }
      if (mInIntHandler) {
        aAsm += "IRet";
      } else {
        aAsm += "mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], " + GetNamespace() + "_" + mFuncName + "_Exit";
        aAsm += "Ret";
      }
      mFuncName = null;
    }

    // BlueSkeye : Seems to be unused. Commented out.
    //protected string GetDestRegister(TokenList aTokens, int aIdx) {
    //  return GetRegister("Destination", aTokens, aIdx);
    //}

    // BlueSkeye : Seems to be unused. Commented out.
    //protected string GetSrcRegister(TokenList aTokens, int aIdx) {
    //  return GetRegister("Source", aTokens, aIdx);
    //}

    // BlueSkeye : Seems to be unused. Commented out.
    //protected string GetRegister(string aPrefix, TokenList aTokens, int aIdx)
    //{
    //  var xToken = aTokens[aIdx].Type;
    //  Token xNext = null;
    //  if (aIdx + 1 < aTokens.Count) {
    //    xNext = aTokens[aIdx + 1];
    //  }

    //  string xResult = aPrefix + "Reg = RegistersEnum." + aTokens[aIdx].Value;
    //  if (xNext != null) {
    //    if (xNext.Value == "[") {
    //      string xDisplacement;
    //      if (aTokens[aIdx + 2].Value == "-") {
    //        xDisplacement = "-" + aTokens[aIdx + 2].Value;
    //      } else {
    //        xDisplacement = aTokens[aIdx + 2].Value;
    //      }
    //      xResult = xResult + ", " + aPrefix + "IsIndirect = true, " + aPrefix + "Displacement = " + xDisplacement;
    //    }
    //  }
    //  return xResult;
    //}

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

      } else if (xToken1.Type == TokenType.Call) {
        rIdx += 1;
        return "@ret_on_stack@";
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

      // The Exit label is a special one that is used as a target for the return instruction.
      // It deserve special handling.
      AddPattern("Exit:", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += GetLabel(aTokens[0]) + ":";
        mFuncExitFound = true;
      });
      // Regular label recognition.
      AddPattern("_ABC:", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += GetLabel(aTokens[0]) + ":";
      });

      AddPattern("Call _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Call " + GetLabel(aTokens[1]);
      });
      AddPattern("_PCALL", delegate(TokenList aTokens, Assembler aAsm) {
        if (aTokens.Count != 1 || aTokens[0].Type != TokenType.Call) {
          throw new Exception("Error occured in parametrized call parsing");
        }  else {
          List<string> mparts = aTokens[0].Value.Remove(aTokens[0].Value.Length - 1).Split('(').ToList();
          if (mparts.Count < 2) {
            throw new Exception("Error occured in parametrized call parsing");
          }
          string fname = mparts[0];
          mparts.RemoveAt(0);
          aTokens[0].Value = String.Join("(", mparts).Trim();
          string val = "";
          int idx;

          var xParams = new List<string>();
          int level = 0;
          foreach (char c in aTokens[0].Value) {
            switch (c) {
              case ',':
                if (level == 0) {
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
          if (!String.IsNullOrEmpty(val.Trim())) {
            xParams.Add(val);
          }
          if (level != 0) {
            throw new Exception("'(' occured without closing equivalent ')' in parametrized function call");
          }

          Parser xParser;
          xParams.Reverse();
          foreach (string p in xParams) {
            xParser = new Parser(p, 0, false, false);
            idx = 0;
            val = GetRef(xParser.Tokens, ref idx);
            if (val != "@ret_on_stack@") {
              aAsm += "Push " + val;
            } else {
              aAsm += GetPatternCode(xParser.Tokens).GetCode(false);
            }
          }
          aAsm += "Call " + GroupLabel(fname);
        }
      });

      AddPattern("Goto _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += "Jmp " + GetLabel(aTokens[1]);
      });


      // Defines a constant having the given name and initial value.
      AddPattern("const _ABC = 123", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += ConstLabel(aTokens[1]) + " equ " + aTokens[3];
      });

      // Declare a double word variable having the given name and initialized to 0. The
      // variable is declared at namespace level.
      AddPattern("var _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Data.Add(GetLabel(aTokens[1]) + " dd 0");
      });
      // Declare a doubleword variable having the given name and an explicit initial value. The
      // variable is declared at namespace level.
      AddPattern("var _ABC = 123", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Data.Add(GetLabel(aTokens[1]) + " dd " + aTokens[3].Value);
      });
      // Declare a textual variable having the given name and value. The variable is defined at
      // namespace level and a null terminating byte is automatically added after the textual
      // value.
      AddPattern("var _ABC = 'Text'", delegate(TokenList aTokens, Assembler aAsm) {
        // Fix issue #15660 by using backquotes for string surrounding and escaping embedded
        // back quotes.
        aAsm.Data.Add(GetLabel(aTokens[1]) + " db `" + EscapeBackQuotes(aTokens[3].Value) + "`, 0");
      });
      // Declare a one-dimension array of bytes, words or doublewords. All members are initialized to 0.
      // _ABC is array name. 123 is the total number of items in the array.
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
            if (aTokens[1] + aTokens[2] == "!0") {
              xOp = "!0";
            }

            HandleIf(aAsm, aTokens, xOp);
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
      AddPattern("_REGADDR[1] = #_ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Mov("dword", "[{0} + {2}]", ConstLabel(aTokens[5]));
      });
      AddPattern("_REGADDR[-1] = #_ABC", delegate(TokenList aTokens, Assembler aAsm) {
        aAsm.Mov("dword", "[{0} - {2}]", ConstLabel(aTokens[5]));
      });

      AddPattern("_REG = _REG", "Mov {0}, {2}");
      AddPattern("_REGADDR[1] = _REG", "Mov [{0} + {2}], {5}");
      AddPattern("_REGADDR[-1] = _REG", "Mov [{0} - {2}], {5}");
      AddPattern("_REG = _REGADDR[1]", "Mov {0}, [{2} + {4}]");
      AddPattern("_REG = _REGADDR[-1]", "Mov {0}, [{2} - {4}]");

      AddPattern("_REG = [_REG]", "Mov {0}, [{3}]");
      AddPattern("_REG = [_REG + 1]", "Mov {0}, [{3} + {5}]");
      AddPattern("_REG = [_REG - 1]", "Mov {0}, [{3} - {5}]");
      AddPattern("[_REG] = _REG", "Mov [{1}], {4}");
      AddPattern("[_REG + 1] = _REG", "Mov [{1} + {3}], {4}");
      AddPattern("[_REG - 1] = _REG", "Mov [{1} - {3}], {4}");

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
      AddPattern(new string[] {
        "_REG * 1",
        "_REG * _REG"
      }, delegate(TokenList aTokens, Assembler aAsm) {
        int targetRegisterSize = 0;
        for (int index = 0; index < 2; index++) {
          Token scannedToken = (0 == index) ? aTokens[0] : aTokens[2];

          if (TokenType.Register != scannedToken.Type) { continue; }
          string canonicScannedTokenValue = scannedToken.Value.ToUpper();

          if (Parser.Registers8.Contains<string>(scannedToken.Value.ToUpper())) {
            throw new Exception(string.Format(
                "Multiplication is not supported on byte sized register '{0}' at line {1}, col {2}",
                scannedToken.Value, scannedToken.LineNumber, scannedToken.SrcPosStart));
          }
          if (0 == index) {
            if (Parser.Registers16.Contains<string>(canonicScannedTokenValue)) { targetRegisterSize = 16; } else if (Parser.Registers32.Contains<string>(canonicScannedTokenValue)) { targetRegisterSize = 32; } else { throw new Exception("Algorithmic error."); }
          } else {
            int sourceRegisterSize;
            if (Parser.Registers16.Contains<string>(canonicScannedTokenValue)) { sourceRegisterSize = 16; } else if (Parser.Registers32.Contains<string>(canonicScannedTokenValue)) { sourceRegisterSize = 32; } else { throw new Exception("Algorithmic error."); }

            if (sourceRegisterSize != targetRegisterSize) {
              throw new Exception(string.Format("Register '{0}' and '{1}' must be of the same size for multiplication on line {2}.",
                  aTokens[0], aTokens[2], aTokens[0].LineNumber));
            }
          }
        }
        aAsm += string.Format("Imul {0}, {1}", aTokens[0], aTokens[2]);
      });
      AddPattern("_REG++", "Inc {0}");
      AddPattern("_REG--", "Dec {0}");

      AddPattern(new string[] {
        "_REG & 1",
        "_REG & _REG"
      }, "And {0}, {2}");
      AddPattern(new string[] {
        "_REG | 1",
        "_REG | _REG"
      }, "Or {0}, {2}");
      AddPattern(new string[] {
        "_REG ^ 1",
        "_REG ^ _REG"
      }, "Xor {0}, {2}");

      // End block. This handle both terminating a standard block as well as a function or an
      // interrupt handler.
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

      AddPattern("namespace _ABC", delegate(TokenList aTokens, Assembler aAsm) {
        mNamespace = aTokens[1].Value;
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
        aAsm += GetNamespace() + "_{1}:";
      });

      // This needs to be different from return.
      // return jumps to exit, ret does raw x86 ret
      AddPattern("Ret", "Ret");
      AddPattern("IRet", "IRet");

      AddPattern("Function _ABC {", delegate(TokenList aTokens, Assembler aAsm) {
        StartFunc(aTokens[1].Value);
        mInIntHandler = false;
        aAsm += GetNamespace() + "_{1}:";
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

    /// <summary>Fix issue #15660. This method escapes double quotes in the candidate string.</summary>
    /// <param name="from">The string to be sanitized.</param>
    /// <returns>The original string with escaped double quotes.</returns>
    private static string EscapeBackQuotes(string from) {
      StringBuilder builder = new StringBuilder();
      bool sanitized = false;
      bool escaped = false;
      foreach (char scannedCharacter in from) {
        switch (scannedCharacter) {
          case '\\':
            escaped = !escaped;
            break;
          case '`':
            if (!escaped) {
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

    public Assembler GetCode(string aLine, int lineNumber) {
      var xParser = new Parser(aLine, lineNumber, false, false);
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

    /// <summary>Register a single pattern with its associated transformation handler.</summary>
    /// <param name="aPattern">A single line of X# code that define the pattern optionally using
    /// pattern reserved syntax.</param>
    /// <param name="aCode">The associated code transformation handler.</param>
    protected void AddPattern(string aPattern, CodeFunc aCode) {
      Parser xParser = null;
      try { xParser = new Parser(aPattern, 1, false, true); } catch (Exception e) {
        throw new Exception(string.Format("Invalid pattern '{0}'", aPattern ?? "NULL"), e);
      }
      var xPattern = new Pattern(xParser.Tokens, aCode);
      mPatterns.Add(xPattern);
    }

    /// <summary>Register a collection of patterns that share a single transformation handler.
    /// </summary>
    /// <param name="aPatterns">A collection of X# lines of code. Each line of code define a
    /// pattern optionally using the pattern reserved syntax.</param>
    /// <param name="aCode">The code transformation handler that is common abmongst all the
    /// patterns from the collection.</param>
    protected void AddPattern(string[] aPatterns, CodeFunc aCode) {
      foreach (var xPattern in aPatterns) {
        AddPattern(xPattern, aCode);
      }
    }

    /// <summary>Register a single pattern with a fixed transformation result.</summary>
    /// <param name="aPattern">A single line of X# code that define the pattern optionally using
    /// pattern reserved syntax.</param>
    /// <param name="aCode">The constant transformation result.</param>
    protected void AddPattern(string aPattern, string aCode) {
      AddPattern(aPattern, delegate(TokenList aTokens, Assembler aAsm) {
        aAsm += aCode;
      });
    }

    /// <summary>Register a collection of patterns that share a single constant transformation
    /// handler.</summary>
    /// <param name="aPatterns">A collection of X# lines of code. Each line of code define a
    /// pattern optionally using the pattern reserved syntax.</param>
    /// <param name="aCode">The constant transformation resultthat is common abmongst all the
    /// patterns from the collection.
    protected void AddPattern(string[] aPatterns, string aCode) {
      foreach (var xPattern in aPatterns) {
        AddPattern(xPattern, aCode);
      }
    }
  }
}
