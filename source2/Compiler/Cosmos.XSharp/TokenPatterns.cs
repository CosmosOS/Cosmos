using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    protected class Pattern {
      public TokenList Tokens;
      public int Hash;
      public CodeFunc Code;
    }

    public bool EmitUserComments = true;
    public delegate void CodeFunc(TokenList aTokens, ref List<string> rCode);
    protected List<Pattern> mPatterns = new List<Pattern>();
    protected string mGroup;
    protected string mProcedureName = null;
    protected bool mInIntHandler;

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

    protected string GroupLabel(int aIdx) {
      return GroupLabel("{" + aIdx + "}");
    }
    protected string GroupLabel(string aLabel) {
      return mGroup + "_" + aLabel;
    }

    protected string ProcLabel(int aIdx) {
      return ProcLabel("{" + aIdx + "}");
    }
    protected string ProcLabel(string aLabel) {
      return mGroup + "_" + mProcedureName + "_" + aLabel;
    }

    protected string GetLabel(Token aToken) {
      if (aToken.Type != TokenType.AlphaNum) {
        throw new Exception("Label must be AlphaNum.");
      }

      string xValue = aToken.Value;

      if (mProcedureName == null) {
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
        return ProcLabel(xValue);
      }
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

    protected string GetCondition(Token aToken) {
      if (aToken.Value == "<") {
        return "ConditionalTestEnum.LessThan";
      } else if (aToken.Value == ">") {
        return "ConditionalTestEnum.GreaterThan";
      } else if (aToken.Value == "=" || aToken.Value == "0") {
        return "ConditionalTestEnum.Zero";
      } else if (aToken.Value == "!=") {
        return "ConditionalTestEnum.NotZero";
      } else if (aToken.Value == "<=") {
        return "ConditionalTestEnum.BelowOrEqual";
      } else if (aToken.Value == ">=") {
        return "ConditionalTestEnum.AboveOrEqual";
      } else {
        throw new Exception("Unrecognized symbol in conditional: " + aToken.Value);
      }
    }

    protected void AddPatterns() {
      AddPattern("! Move EAX, 0",
        "new LiteralAssemblerCode(\"{0}\");"
      );

      AddPattern("# Comment", delegate(TokenList aTokens, ref List<string> rCode) {
        if (EmitUserComments) {
          string xValue = aTokens[0].Value;
          xValue = xValue.Replace("\"", "\\\""); 
          rCode.Add("new Comment(" + Quoted(xValue) + ");");
        }
      });

      // Labels
      // Local and proc level are used most, so designed to make their syntax shortest.
      // Think of the dots like a directory, . is current group, .. is above that.
      // ..Name: - Global level. Emitted exactly as is.
      // .Name: - Group level. Group_Name
      // Name: - Procedure level. Group_ProcName_Name
      AddPattern(new string[] { "_ABC:" },
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("new Label(" + Quoted(GetLabel(aTokens[0])) + ");");
        }
      );

      AddPattern("Call _ABC" ,
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("new Call {{ DestinationLabel = " + Quoted(GetLabel(aTokens[1])) + " }};");
        }
      );

      AddPattern("Goto _ABC",
        delegate(TokenList aTokens, ref List<string> rCode) {
          string xLabel = GetLabel(aTokens[1]);
          rCode.Add("new Jump {{ DestinationLabel = " + Quoted(xLabel) + " }};");
        }
      );

      AddPattern("var _ABC",
        delegate(TokenList aTokens, ref List<string> rCode) {
          string xLabel = GetLabel(aTokens[1]);
          rCode.Add("mAssembler.DataMembers.Add(new DataMember(" + Quoted(xLabel) + ", 0));");
        }
      );
      AddPattern("var _ABC = 123",
        delegate(TokenList aTokens, ref List<string> rCode) {
          string xLabel = GetLabel(aTokens[1]);
          rCode.Add("mAssembler.DataMembers.Add(new DataMember(" + Quoted(xLabel) + ", " + aTokens[3].Value + "));");
        }
      );
      AddPattern("var _ABC = 'Text'",
        delegate(TokenList aTokens, ref List<string> rCode) {
          string xLabel = GetLabel(aTokens[1]);
          rCode.Add("mAssembler.DataMembers.Add(new DataMember(" + Quoted(xLabel) + ", \"" + aTokens[3].Value + "\"));");
        }
      );
      AddPattern("var _ABC _ABC[123]",
        delegate(TokenList aTokens, ref List<string> rCode) {
          string xLabel = GetLabel(aTokens[1]);
          rCode.Add("mAssembler.DataMembers.Add(new DataMember(" + Quoted(xLabel) + ", new " + aTokens[2].Value + "[" + aTokens[4].Value + "]));");
        }
      );

      AddPattern(new string[] {
          "if 0 goto _ABC", 
          "if < goto _ABC", 
          "if > goto _ABC", 
          "if = goto _ABC",
          "if != goto _ABC",
          "if <= goto _ABC", 
          "if >= goto _ABC" 
        },
        delegate(TokenList aTokens, ref List<string> rCode) {
          string xLabel = GetLabel(aTokens[3]);
          var xCondition = GetCondition(aTokens[1]);
          rCode.Add("new ConditionalJump {{ Condition = " + xCondition + ", DestinationLabel = " + Quoted(xLabel) + " }};");
        }
      );
      AddPattern(new string[] {
          "if 0 Exit", 
          "if < Exit", 
          "if > Exit", 
          "if = Exit",
          "if != Exit",
          "if <= Exit", 
          "if >= Exit" 
        },
        delegate(TokenList aTokens, ref List<string> rCode) {
          var xCondition = GetCondition(aTokens[1]);
          rCode.Add("new ConditionalJump {{ Condition = " + xCondition + ", DestinationLabel = " + Quoted(ProcLabel("Exit")) + " }};");
        }
      );
      // Must test separate since !0 is two tokens
      AddPattern("if !0 goto _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        string xLabel = GetLabel(aTokens[4]);
        rCode.Add("new ConditionalJump {{ Condition = ConditionalTestEnum.NotZero, DestinationLabel = " + Quoted(xLabel) + " }};");
      });
      AddPattern("if !0 Exit", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new ConditionalJump {{ Condition = ConditionalTestEnum.NotZero, DestinationLabel = " + Quoted(ProcLabel("Exit")) + " }};");
      });

      AddPattern(new string[] {
          //0 1  2  3  4 5  6    7
          "if (_REG < 123) goto _ABC",
          "if (_REG > 123) goto _ABC",
          "if (_REG = 123) goto _ABC",
          "if (_REG != 123) goto _ABC",
          "if (_REG <= 123) goto _ABC",
          "if (_REG >= 123) goto _ABC"
        },
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("new Compare {{ DestinationReg = RegistersEnum.{2}, SourceValue = {4} }};");

          var xCondition = GetCondition(aTokens[3]);
          string xLabel = GetLabel(aTokens[7]);
          rCode.Add("new ConditionalJump {{ Condition = " + xCondition + ", DestinationLabel = " + Quoted(xLabel) + " }};");
        }
      );
      AddPattern(new string[] {
          //0 1  2  3  4 5  6    
          "if (_REG < 123) Exit",
          "if (_REG > 123) Exit",
          "if (_REG = 123) Exit",
          "if (_REG != 123) Exit",
          "if (_REG <= 123) Exit",
          "if (_REG >= 123) Exit"
        },
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("new Compare {{ DestinationReg = RegistersEnum.{2}, SourceValue = {4} }};");

          var xCondition = GetCondition(aTokens[3]);
          rCode.Add("new ConditionalJump {{ Condition = " + xCondition + ", DestinationLabel = " + Quoted(ProcLabel("Exit")) + " }};");
        }
      );

      AddPattern("_REG ?= 123",
        "new Compare {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );
      AddPattern("_REG ?= _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        string xLabel = GetLabel(aTokens[2]);
        rCode.Add("new Compare {{ DestinationReg = RegistersEnum.{0}, SourceIsIndirect = true, SourceRef = Cosmos.Assembler.ElementReference.New(" + Quoted(xLabel) + ") }};");
      });

      AddPattern("_REG ?& 123",
        "new Test {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );

      // ~ "infinite" shift because it loops
      AddPattern("_REG ~> 123",
        "new LiteralAssemblerCode(\"ROR {0}, {2}\");"
      );
      AddPattern("_REG <~ 123",
        "new LiteralAssemblerCode(\"ROL {0}, {2}\");"
      );
      AddPattern("_REG >> 123",
        "new LiteralAssemblerCode(\"SHR {0}, {2}\");"
      );
      AddPattern("_REG << 123",
        "new LiteralAssemblerCode(\"SHL {0}, {2}\");"
      );

      AddPattern(new string[] { 
          "_REG = 123", 
          "_REG = _REG",
          "_REG = _REG32[1]",
          "_REG = _REG[-1]",
          
          "_REG32[1] = 123",
          "_REG32[1] = _REG",

          "_REG32[-1] = 123",
          "_REG32[-1] = _REG"
        },
        delegate(TokenList aTokens, ref List<string> rCode) {
          int xEqIdx = -1;
          for (int i = 0; i < aTokens.Count; i++) {
            if (aTokens[i].Value == "=") {
              xEqIdx = i;
              break;
            }
          }

          string xDestReg = GetDestRegister(aTokens, 0);
          if (aTokens[xEqIdx + 1].Type == TokenType.ValueInt) {
            rCode.Add("new Mov{{ " + xDestReg + ", SourceValue = " + aTokens[xEqIdx + 1].Value + " }};");
          } else {
            string xSrcReg = GetSrcRegister(aTokens, xEqIdx + 1);
            rCode.Add("new Mov{{ " + xDestReg + ", " + xSrcReg + " }};");
          }
        }
      );

      AddPattern(new string[] { 
          "Port[DX] = AL", 
          "Port[DX] = AX", 
          "Port[DX] = EAX"
        },
        "new Out {{ DestinationReg = RegistersEnum.{5}}};"
      );
      AddPattern(new string[] { 
          "AL = Port[DX]", 
          "AX = Port[DX]", 
          "EAX = Port[DX]"
        },
        "new IN {{ DestinationReg = RegistersEnum.{0}}};"
      );

      AddPattern("+123",
        "new Push {{"
          + " DestinationValue = {1}, Size = 32 "
          + "}};"
      );
      AddPattern("+123:12",
        "new Push {{"
          + " DestinationValue = {1}, Size = {3} "
          + "}};"
      );
      AddPattern("+_REG",
        "new Push {{"
          + " DestinationReg = RegistersEnum.{1}"
          + "}};"
      );
      AddPattern("-_REG",
        "new Pop {{"
          + " DestinationReg = RegistersEnum.{1}"
          + "}};"
      );

      AddPattern("_REG = _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        string xLabel = GetLabel(aTokens[2]);
        rCode.Add("new Mov {{"
         + " DestinationReg = RegistersEnum.{0}"
         + " , SourceRef = Cosmos.Assembler.ElementReference.New(" + Quoted(xLabel) + "), SourceIsIndirect = true"
         + " }};");
      });
	    // why not [var] like registers? Because its less frequent to access th ptr
	    // and it is like a reg.. without [] to get the value...
      AddPattern("_REG = @_ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        string xLabel = GetLabel(aTokens[3]);
        rCode.Add("new Mov {{"
         + " DestinationReg = RegistersEnum.{0}"
         + " , SourceRef = Cosmos.Assembler.ElementReference.New(" + Quoted(xLabel) + ")"
         + " }};");
      });

      AddPattern("_ABC = _REG", delegate(TokenList aTokens, ref List<string> rCode) {
        string xLabel = GetLabel(aTokens[0]);
        rCode.Add("new Mov {{"
         + " DestinationRef = Cosmos.Assembler.ElementReference.New(" + Quoted(xLabel) + "), DestinationIsIndirect = true"
         + " , SourceReg = RegistersEnum.{2}"
         + " }};");
      });
      AddPattern("_ABC = 123", delegate(TokenList aTokens, ref List<string> rCode) {
        string xLabel = GetLabel(aTokens[0]);
        rCode.Add("new Mov {{"
         + " DestinationRef = Cosmos.Assembler.ElementReference.New(" + Quoted(xLabel) + "), DestinationIsIndirect = true"
         + " , SourceValue = {2}"
         + " }};");
      });

      // TODO: Allow asm to optimize these to Inc/Dec
      AddPattern("_REG + 1", delegate(TokenList aTokens, ref List<string> rCode) {
        if (IntValue(aTokens[2]) == 1) {
          rCode.Add("new INC {{ DestinationReg = RegistersEnum.{0} }};");
        } else {
          rCode.Add("new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};");
        }
      });
      AddPattern("_REG++", 
        "new INC {{ DestinationReg = RegistersEnum.{0} }};"
      );

      AddPattern("_REG - 1", delegate(TokenList aTokens, ref List<string> rCode) {
        if (IntValue(aTokens[2]) == 1) {
          rCode.Add("new Dec {{ DestinationReg = RegistersEnum.{0} }};");
        } else {
          rCode.Add("new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};");
        }
      });
      AddPattern("_REG--",
        "new Dec {{ DestinationReg = RegistersEnum.{0} }};"
      );

      AddPattern("}", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Label(\"" + mGroup + "_" + mProcedureName + "_Exit\");");
        if (mInIntHandler) {
          rCode.Add("new IRET();");
        } else {
          rCode.Add("new Return();");
        }
        mProcedureName = null;
      });

      AddPattern("Group _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        mGroup = aTokens[1].Value;
      });

      AddPattern("Exit", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Jump {{ DestinationLabel = " + Quoted(ProcLabel("Exit")) + " }};");
      });

      AddPattern("InterruptHandler _ABC {", delegate(TokenList aTokens, ref List<string> rCode) {
        mInIntHandler = true;
        mProcedureName = aTokens[1].Value;
        rCode.Add("new Label(\"" + mGroup + "_{1}\");");
      });

      AddPattern("Jump _ABC", 
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("new Jump {{ DestinationLabel = \"" + mGroup + "_{1}\" }};");
        }
      );

      AddPattern("Return", "new Return();");
      AddPattern("ReturnInterrupt", "new IRET();");
      AddPattern("PopAll", "new Popad();");
      AddPattern("PushAll", "new Pushad();");

      AddPattern("Procedure _ABC {", delegate(TokenList aTokens, ref List<string> rCode) {
        mInIntHandler = false;
        mProcedureName = aTokens[1].Value;
        rCode.Add("new Label(\"" + mGroup + "_{1}\");");
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
            rCode.Add("new LiteralAssemblerCode(" + Quoted("mov byte [0x" + i.ToString("X") + "], 0") + ");");
            rCode.Add("new LiteralAssemblerCode(" + Quoted("mov byte [0x" + (i + 1).ToString("X") + "], 0x02") + ");");
          }

          foreach (var xChar in aTokens[1].Value) {
            rCode.Add("new LiteralAssemblerCode(" + Quoted("mov byte [0x" + xVideo.ToString("X") + "], " + (byte)xChar) + ");");
            xVideo = xVideo + 2;
          }
        }
      });
    }

    public List<string> GetCode(string aLine) {
      var xParser = new Parser(aLine, false, false);
      var xTokens = xParser.Tokens;
      var xResult = new List<string>();
      int xHash = xTokens.GetPatternHashCode();

      // Get a list of matching hashes, but then we have to 
      // search for exact pattern match because it is possible
      // to have duplicate hashes. Hashes just provide us a quick way
      // to reduce the search.
      var xPatterns = mPatterns.Where(q => q.Hash == xHash);
      Pattern xPattern = null;
      foreach (var x in xPatterns) {
        if (x.Tokens.PatternMatches(xTokens)) {
          xPattern = x;
          break;
        }
      }
      if (xPattern == null) {
        throw new Exception("Token pattern not found: " + aLine);
      }

      xPattern.Code(xTokens, ref xResult); 

      for(int i = 0; i < xResult.Count; i++) {
        xResult[i] = string.Format(xResult[i], xTokens.Select(c => c.Value).ToArray());
      }
      return xResult;
    }

    protected void AddPattern(string[] aPatterns, CodeFunc aCode) {
      foreach (var xPattern in aPatterns) {
        AddPattern(xPattern, aCode);
      }
    }
    protected void AddPattern(string aPattern, CodeFunc aCode) {
      var xParser = new Parser(aPattern, false, true);

      var xPattern = new Pattern() {
        Tokens = xParser.Tokens,
        Hash = xParser.Tokens.GetHashCode(),
        Code = aCode
      };

      mPatterns.Add(xPattern);
    }
    protected void AddPattern(string[] aPatterns, string aCode) {
      AddPattern(aPatterns, delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add(aCode);
      });
    }
    protected void AddPattern(string aPattern, string aCode) {
      AddPattern(aPattern, delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add(aCode);
      });
    }

  }
}
