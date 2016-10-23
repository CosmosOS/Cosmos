using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cosmos.Assembler;

namespace ASharp.Compiler
{
    using static ASConditions;
    using static ASRegisters;

    /// <summary>This class is able to translate a single A# source code line into one or more
    /// target assembler source code and data lines. The class is a group of pattern each of
    /// which defines a transformation function from the A# syntax to the target assembler
    /// syntax.</summary>
    public class TokenPatterns
    {
        /// <summary>Describe a single pattern with its list of tokens that might include pattern
        /// reserved syntax token and a transformation function. For ease of search and performance
        /// an hashcode value is computed on the tokens list content and later used for searching
        /// a pattern matching an actual line of A# code source.</summary>
        protected class Pattern
        {
            public readonly TokenList Tokens;
            public readonly int Hash;
            public readonly CodeFunc Code;
            public readonly string PatternString;

            public Pattern(TokenList aTokens, CodeFunc aCode, string patternString)
            {
                Tokens = aTokens;
                Hash = aTokens.GetHashCode();
                Code = aCode;
                PatternString = patternString;
            }
        }

        /// <summary>The set of blocks for the currently assembled function. Each time we begin
        /// assembling a new function this blocks collection is reset to an empty state.</summary>
        protected Blocks mBlocks = new Blocks();

        protected class Blocks : List<Block>
        {
            protected int mCurrentLabelID = 0;

            public void Reset()
            {
                mCurrentLabelID = 0;
            }

            public Block Current()
            {
                return base[Count - 1];
            }

            public void Start(TokenList aTokens, bool aIsCollector)
            {
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
        protected class Block
        {
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

        protected string GetNamespace()
        {
            if (mNamespace == null)
            {
                throw new Exception("A namespace has not been defined.");
            }

            return mNamespace;
        }

        public TokenPatterns()
        {
            mCompareOps = "< > = != <= >= 0 !0".Split(' ');
            var xSizes = "byte , word , dword ".Split(',').ToList();
            // We must add this empty size so that we allow constructs where the size is not
            // explicitly defined in source code. For example : while eax < 0
            // otherwise we would have to write : while dword eax < 0
            xSizes.Add("");
            foreach (var xSize in xSizes)
            {
                foreach (var xComparison in mCompareOps)
                {
                    // Skip 0 and !0
                    if (!xComparison.Contains("0"))
                    {
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
        protected string ConstLabel(Token aToken)
        {
            return GroupLabel("Const_" + aToken);
        }

        /// <summary>Builds a label at namespace level having the given name.</summary>
        /// <param name="aLabel">Local label name at namespace level.</param>
        /// <returns>The label name</returns>
        protected string GroupLabel(string aLabel)
        {
            return GetNamespace() + "_" + aLabel;
        }

        /// <summary>Builds a label at function level having the given name.</summary>
        /// <param name="aLabel">Local label name at function level.</param>
        /// <returns>The label name</returns>
        protected string FuncLabel(string aLabel)
        {
            return GetNamespace() + "_" + mFuncName + "_" + aLabel;
        }

        /// <summary>Builds a label having the given name at current function block level.</summary>
        /// <param name="aLabel">Local label name at function block level.</param>
        /// <returns>The label name.</returns>
        protected string BlockLabel(string aLabel)
        {
            return FuncLabel("Block" + mBlocks.Current().LabelID + "_" + aLabel);
        }

        /// <summary>Build a label name for the given token. This method enforce the rule for .
        /// and .. prefixes and build the label at appropriate level.</summary>
        /// <param name="aToken"></param>
        /// <returns></returns>
        protected string GetLabel(Token aToken)
        {
            if ((aToken.Type != TokenType.AlphaNum) && !aToken.Matches("exit"))
            {
                throw new Exception("Label must be AlphaNum.");
            }

            string xValue = aToken.RawValue;
            if (!InFunctionBody)
            {
                if (xValue.StartsWith("."))
                {
                    return xValue.Substring(1);
                }
                return GroupLabel(xValue);
            }
            else
            {
                if (xValue.StartsWith(".."))
                {
                    return xValue.Substring(2);
                }
                else if (xValue.StartsWith("."))
                {
                    return GroupLabel(xValue.Substring(1));
                }
                return FuncLabel(xValue);
            }
        }

        /// <summary>Get a flag that tell if we are in a function body or not. This is used by the
        /// assembler generator when end of source file is reached to make sure the last function
        /// or interrupt handler is properly closed (see issue #15666)</summary>
        internal bool InFunctionBody
        {
            get { return !string.IsNullOrEmpty(mFuncName); }
        }

        /// <summary>Start a new function having the given name. The current blocks collection is
        /// reset to an empty state and the function name is saved for later reuse in local to function
        /// labels' name construction.</summary>
        /// <param name="aName">Function name.</param>
        protected void StartFunc(string aName)
        {
            if (InFunctionBody)
            {
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
        protected void EndFunc()
        {
            if (mFuncName == null)
            {
                throw new Exception("Found a closing curly brace that doesn't match an opening curly brace.");
            }
            if (!mFuncExitFound)
            {
                AS.Label(GetNamespace() + "_" + mFuncName + "_Exit");
            }
            if (mInIntHandler)
            {
                //TODO: Check this
                AS.ExceptionReturn();
            }
            else
            {
                //XS.Set("static_field__Cosmos_Core_INTs_mLastKnownAddress", GetNamespace() + "_" + mFuncName + "_Exit", destinationIsIndirect: true, size: RegisterSize.Int32);
                //TODO: Change this: AS.Move("static_field__Cosmos_Core_INTs_mLastKnownAddress", GetNamespace() + "_" + mFuncName + "_Exit", destinationIsIndirect: true);

                //XS.Return();
                AS.BranchAndExchange(lr);
            }
            mFuncName = null;
        }

        protected string GetSimpleRef(Token aToken)
        {
            return GetLabel(aToken);
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

        protected Condition GetCondition(string aComparison, bool aInvert = false)
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
                    aComparison = "!0";
                }
                else if (aComparison == "!0")
                {
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
                return UnsignedLower; // unsigned
            }
            else if (aComparison == ">")
            {
                return UnsignedHigher; // unsigned
            }
            else if (aComparison == "=")
            {
                return Equal;
            }
            else if (aComparison == "0")
            {
                return EqualsZero;
            }
            else if (aComparison == "!=")
            {
                return NotEqual;
            }
            else if (aComparison == "!0")
            {
                return NotEqual;
            }
            else if (aComparison == "<=")
            {
                return UnsignedLowerOrSame; // unsigned
            }
            else if (aComparison == ">=")
            {
                return UnsignedHigherOrSame; // unsigned
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
                AS.Branch(BlockLabel("End"), condition: GetCondition(xComparison, true));
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

                AS.Branch(xLabel, condition: GetCondition(xComparison));
            }
        }

        protected void AddPatterns()
        {
            AddPattern("! Mov EAX, 0", delegate (TokenList aTokens)
                                       {
                                           AS.LiteralCode(aTokens[0].RawValue);
                                       });

            AddPattern("// Comment", delegate (TokenList aTokens)
                                     {
                                         if (EmitUserComments)
                                         {
                                             string xValue = aTokens[0].RawValue;
                                             xValue = xValue.Replace("\"", "\\\"");
                                             AS.Comment(xValue);
                                         }
                                     });

            // The value function returns a token containing the comparison
            var xCompares = new Dictionary<string, Func<int, TokenList, Token>>();


            //#region Handle all comparisons

            //foreach (var xSize in xSizes)
            //{
            //    foreach (var xComparison in mCompareOps)
            //    {
            //        var xComparisonToken = new Token(-1);
            //        xComparisonToken.RawValue = xComparison;
            //        // Skip 0 and !0
            //        if (!xComparison.Contains("0"))
            //        {
            //            xCompares.Add(xSize + " _REG " + xComparison + " 123",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              RegisterSize? xTypedSize = null;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xTypedSize = GetSize(tokenList[xOffset]);
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].IntValue, size: xTypedSize);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REG " + xComparison + " _REG",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              RegisterSize? xTypedSize = null;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xTypedSize = GetSize(tokenList[xOffset]);
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REG " + xComparison + " _REGADDR[1]",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register, sourceDisplacement: (int)tokenList[xOffset + 3].IntValue);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REG " + xComparison + " _REGADDR[-1]",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register, sourceDisplacement: -(int)tokenList[xOffset + 3].IntValue);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REG " + xComparison + " _ABC",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, GetLabel(tokenList[xOffset + 2]), sourceIsIndirect: true);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REG " + xComparison + " #_ABC",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, ConstLabel(tokenList[xOffset + 3]));
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REGADDR[1] " + xComparison + " 123",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              RegisterSize? xTypedSize = null;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xTypedSize = GetSize(tokenList[xOffset]);
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 5].IntValue, destinationDisplacement: (int)tokenList[xOffset + 2].IntValue, size: xTypedSize);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REGADDR[-1] " + xComparison + " 123",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              RegisterSize? xTypedSize = null;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xTypedSize = GetSize(tokenList[xOffset]);
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].IntValue, destinationDisplacement: -(int)tokenList[xOffset + 1].IntValue, size: xTypedSize);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REGADDR[1] " + xComparison + " _REG",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register, destinationDisplacement: (int)tokenList[xOffset + 1].IntValue);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REGADDR[-1] " + xComparison + " _REG",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, tokenList[xOffset + 2].Register, destinationDisplacement: -(int)tokenList[xOffset + 1].IntValue);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REGADDR[1] " + xComparison + " #_ABC",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              RegisterSize? xTypedSize = null;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xTypedSize = GetSize(tokenList[xOffset]);
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, ConstLabel(tokenList[xOffset + 2]), destinationDisplacement: (int)tokenList[xOffset + 1].IntValue, sourceIsIndirect: true, size: xTypedSize);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _REGADDR[-1] " + xComparison + " #_ABC",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              RegisterSize? xTypedSize = null;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xTypedSize = GetSize(tokenList[xOffset]);
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(tokenList[xOffset + 0].Register, ConstLabel(tokenList[xOffset + 2]), destinationDisplacement: (int)tokenList[xOffset + 1].IntValue, size: xTypedSize);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _ABC " + xComparison + " 123",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              RegisterSize? xTypedSize = null;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xTypedSize = GetSize(tokenList[xOffset]);
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(GetLabel(tokenList[xOffset + 0]), tokenList[xOffset + 2].IntValue, destinationIsIndirect: true, size: xTypedSize.GetValueOrDefault(RegisterSize.Int32));
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _ABC " + xComparison + " _REG",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              if (tokenList[xOffset + 2].Type != TokenType.Register)
            //                              {
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(GetSimpleRef(tokenList[xOffset + 0]), tokenList[xOffset + 2].Register, destinationIsIndirect: true);
            //                              return xComparisonToken;
            //                          });
            //            xCompares.Add(xSize + " _ABC " + xComparison + " #_ABC",
            //                          (tokenOffset, tokenList) =>
            //                          {
            //                              var xOffset = tokenOffset;
            //                              RegisterSize? xTypedSize = null;
            //                              if (tokenList[xOffset].Type != TokenType.Register)
            //                              {
            //                                  xTypedSize = GetSize(tokenList[xOffset]);
            //                                  xOffset += 1;
            //                              }
            //                              AS.Compare(GetSimpleRef(tokenList[xOffset + 0]), ConstLabel(tokenList[xOffset + 3]), destinationIsIndirect: true, size: xTypedSize.GetValueOrDefault(RegisterSize.Int32));
            //                              return xComparisonToken;
            //                          });
            //        }
            //    }
            //}

            //#endregion Handle all comparisons

            // Labels
            // Local and proc level are used most, so designed to make their syntax shortest.
            // Think of the dots like a directory, . is current group, .. is above that.
            // ..Name: - Global level. Emitted exactly as is.
            // .Name: - Group level. Group_Name
            // Name: - Function level. Group_ProcName_Name

            // The Exit label is a special one that is used as a target for the return instruction.
            // It deserve special handling.
            AddPattern("Exit:", delegate (TokenList aTokens)
                                {
                                    AS.Label(GetLabel(aTokens[0]));
                                    mFuncExitFound = true;
                                });

            // Regular label recognition.
            AddPattern("_ABC:", delegate (TokenList aTokens)
                                {
                                    AS.Label(GetLabel(aTokens[0]));
                                });

            AddPattern("Call _ABC", delegate (TokenList aTokens)
                                    {
                                        AS.BranchWithLink(GetLabel(aTokens[1]));
                                    });
            AddPattern("_PCALL", delegate (TokenList aTokens)
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
                                           //AS.PushLiteral(val);
                                           throw new Exception();
                                             }
                                             else
                                             {
                                           //aAsm += GetPatternCode(xParser.Tokens).GetCode(false);
                                           throw new NotImplementedException("Didn't get converted yet!");
                                             }
                                         }
                                         AS.BranchWithLink(GroupLabel(fname));
                                     }
                                 });

            AddPattern("Goto _ABC", delegate (TokenList aTokens)
                                    {
                                        AS.Branch(GetLabel(aTokens[1]));
                                    });

            // Defines a constant having the given name and initial value.
            AddPattern("const _ABC = 123", delegate (TokenList aTokens)
                                           {
                                               AS.Const(ConstLabel(aTokens[1]), aTokens[3].IntValue.ToString());
                                           });

            // Declare a double word variable having the given name and initialized to 0. The
            // variable is declared at namespace level.
            AddPattern("var _ABC", delegate (TokenList aTokens)
                                   {
                                       AS.DataMember(GetLabel(aTokens[1]));
                                   });

            // Declare a doubleword variable having the given name and an explicit initial value. The
            // variable is declared at namespace level.
            AddPattern("var _ABC = 123", delegate (TokenList aTokens)
                                         {
                                             AS.DataMember(GetLabel(aTokens[1]), aTokens[3].IntValue);
                                         });

            // Declare a textual variable having the given name and value. The variable is defined at
            // namespace level and a null terminating byte is automatically added after the textual
            // value.
            AddPattern("var _ABC = 'Text'", delegate (TokenList aTokens)
                                            {
                                          // Fix issue #15660 by using backquotes for string surrounding and escaping embedded
                                          // back quotes.
                                          AS.DataMember(GetLabel(aTokens[1]), EscapeBackQuotes(aTokens[3].RawValue));
                                            });

            // Declare a one-dimension array of bytes, words or doublewords. All members are initialized to 0.
            // _ABC is array name. 123 is the total number of items in the array.
            AddPattern(new string[]
                       {
                   "var _ABC byte[123]", "var _ABC word[123]", "var _ABC dword[123]"
                       }, delegate (TokenList aTokens)
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
                              AS.DataMember(GetLabel(aTokens[1]), aTokens[4].IntValue, xSize, "0");
                          });

            foreach (var xCompare in xCompares)
            {
                //          0         1  2   3     4
                AddPattern("while " + xCompare.Key + " {", delegate (TokenList aTokens)
                                                           {
                                                               mBlocks.Start(aTokens, false);
                                                               AS.Label(BlockLabel("Begin"));

                                                               int xIdx = 1;
                                                               Token xComparison = xCompare.Value(1, aTokens);

                                                               AS.Branch(BlockLabel("End"), condition: GetCondition(xComparison.RawValue, true));
                                                           });
            }

            foreach (var xTail in "goto _ABC|return|{".Split('|'))
            {
                // if 0 exit, etc
                foreach (var xComparison in mCompareOps)
                {
                    AddPattern("if " + xComparison + " " + xTail, delegate (TokenList aTokens)
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
                               delegate (TokenList aTokens)
                               {
                                   int xIdx = 1;
                                   Token xComparison = xCompare.Value(1, aTokens);

                                   HandleIf(aTokens, xComparison.RawValue);
                               });
                }
            }

            AddPattern("_REG ?= 123", delegate (TokenList aTokens)
                                      {
                                          //TODO: Change this
                                          //AS.Compare(aTokens[0].Register, aTokens[2].IntValue);
                                      });
            AddPattern("_REG ?= _ABC", delegate (TokenList aTokens)
                                       {
                                           //AS.Compare(aTokens[0].Register, GetSimpleRef(aTokens[2]), sourceIsIndirect: true);
                                       });
            AddPattern("_REG ?= #_ABC", delegate (TokenList aTokens)
                                        {
                                            //TODO: Change this
                                            //AS.Compare(aTokens[0].Register, ConstLabel(aTokens[2]));
                                        });

            AddPattern("_REG ?& 123", delegate (TokenList aTokens)
                                      {
                                          //TODO: Change this
                                          //AS.Test(aTokens[0].Register, aTokens[2].IntValue);
                                      });
            AddPattern("_REG ?& _ABC", delegate (TokenList aTokens)
                                       {
                                           //TODO: Change this
                                           //AS.Test(aTokens[0].Register, GetLabel(aTokens[2]), sourceIsIndirect: true);
                                       });
            AddPattern("_REG ?& #_ABC", delegate (TokenList aTokens)
                                        {
                                            //TODO: Change this
                                            //AS.Test(aTokens[0].Register, GetLabel(aTokens[2]), sourceIsIndirect: true);
                                        });

            AddPattern("_REG ~> 123", delegate (TokenList aTokens)
                                      {
                                          //TODO: Change this
                                          //AS.RotateRight(aTokens[0].Register, aTokens[2].IntValue);
                                      });
            AddPattern("_REG <~ 123", delegate (TokenList aTokens)
                                      {
                                          //TODO: Change this
                                          //AS.RotateLeft(aTokens[0].Register, aTokens[2].IntValue);
                                      });
            AddPattern("_REG >> 123", delegate (TokenList aTokens)
                                      {
                                          if (aTokens[2].IntValue > 255)
                                          {
                                              throw new IndexOutOfRangeException("Value is too large to be used as bitcount!");
                                          }

                                          //TODO: Check this
                                          AS.LogicalShiftRight(aTokens[0].Register, aTokens[0].Register, (byte)aTokens[2].IntValue);
                                      });
            AddPattern("_REG << 123", delegate (TokenList aTokens)
                                      {
                                          if (aTokens[2].IntValue > 255)
                                          {
                                              throw new IndexOutOfRangeException("Value is too large to be used as bitcount!");
                                          }

                                          //TODO: Check this
                                          AS.LogicalShiftLeft(aTokens[0].Register, aTokens[0].Register, (byte)aTokens[2].IntValue);
                                      });

            AddPattern("_REG = 123", delegate (TokenList aTokens)
                                     {
                                         AS.Move(aTokens[0].Register, aTokens[2].IntValue);
                                     });
            //AddPattern("_REGADDR[1] = 123", delegate (TokenList aTokens)
            //                                {
            //                                    AS.Move(aTokens[0].Register, aTokens[5].IntValue, destinationDisplacement: (int)aTokens[2].IntValue); //TODO: Change this
            //                                });
            //AddPattern("_REGADDR[-1] = 123", delegate (TokenList aTokens)
            //                                 {
            //                                     AS.Move(aTokens[0].Register, aTokens[5].IntValue, destinationDisplacement: -(int)aTokens[2].IntValue); //TODO: Change this
            //                                 });

            AddPattern("_REG = #_ABC", delegate (TokenList aTokens)
                                       {
                                           //TODO: Implement LoadRegister for Labels
                                           //AS.LoadRegister(aTokens[0].Register, ConstLabel(aTokens[3]));
                                           AS.LiteralCode("LDR " + aTokens[0].Register.Name + ", =" + ConstLabel(aTokens[3]));
                                       });
            AddPattern("_REGADDR[1] = #_ABC", delegate (TokenList aTokens)
                                              {
                                            //AS.Set(RegisterSize.Int32, aTokens[0].IntValue, aTokens[2].IntValue, ConstLabel(aTokens[5]));
                                            throw new NotImplementedException("");
                                              });
            AddPattern("_REGADDR[-1] = #_ABC", delegate (TokenList aTokens)
                                               {
                                                   var xFirst = GetSimpleRef(aTokens[0]);
                                                   var xSecond = GetSimpleRef(aTokens[2]);

                                             //AS.SetLiteral("dword [" + xFirst + " - " + xSecond + "]", ConstLabel(aTokens[5]));
                                             throw new NotImplementedException("");
                                               });

            AddPattern("_REG = _REG", delegate (TokenList aTokens)
                                      {
                                          AS.Move(aTokens[0].Register, aTokens[2].Register);
                                      });
            AddPattern("_REGADDR[1] = _REG", delegate (TokenList aTokens)
                                             {
                                                 //TODO: Check this
                                                 //AS.Move(aTokens[0].Register, aTokens[5].Register, new Operand2Shift(Operand2ShiftType.LogicalShiftLeft, (byte)aTokens[2].IntValue));
                                                 //AS.Move(aTokens[0].Register, aTokens[5].Register, destinationDisplacement: (int)aTokens[2].IntValue);
                                             });
            AddPattern("_REGADDR[-1] = _REG", delegate (TokenList aTokens)
                                              {
                                                  //TODO: Check this
                                                  //AS.Move(aTokens[0].Register, aTokens[5].Register, new Operand2Shift(Operand2ShiftType.LogicalShiftRight, (byte)aTokens[2].IntValue));
                                                  //AS.Move(aTokens[0].Register, aTokens[5].Register, destinationDisplacement: -(int)aTokens[2].IntValue);
                                              });
            //AddPattern("_REG = _REGADDR[1]", delegate (TokenList aTokens)
            //                                 {
            //                                     AS.Move(aTokens[0].Register, aTokens[2].Register, sourceDisplacement: (int)aTokens[4].IntValue);
            //                                 });
            //AddPattern("_REG = _REGADDR[-1]", delegate (TokenList aTokens)
            //                                  {
            //                                      AS.Move(aTokens[0].Register, aTokens[2].Register, sourceDisplacement: -(int)aTokens[4].IntValue);
            //                                  });

            AddPattern("_REG = [_REG]", delegate (TokenList aTokens)
                                        {
                                            AS.LoadRegister(aTokens[0].Register, aTokens[3].Register);
                                            //AS.Move(aTokens[0].Register, aTokens[3].Register, sourceIsIndirect: true);
                                        });
            //AddPattern("_REG = [_REG + 1]", delegate (TokenList aTokens)
            //                                {
            //                                    AS.Move(aTokens[0].Register, aTokens[3].Register, sourceDisplacement: (int?)aTokens[5].IntValue);
            //                                });
            //AddPattern("_REG = [_REG - 1]", delegate (TokenList aTokens)
            //                                {
            //                                    AS.Move(aTokens[0].Register, aTokens[3].Register, sourceDisplacement: -(int)aTokens[5].IntValue);
            //                                });
            //AddPattern("[_REG] = _REG", delegate (TokenList aTokens)
            //                            {
            //                                AS.Move(aTokens[1].Register, aTokens[4].Register, destinationIsIndirect: true);
            //                            });
            //AddPattern("[_REG + 1] = _REG", delegate (TokenList aTokens)
            //                                {
            //                                    AS.Move(aTokens[0].Register, aTokens[3].Register, destinationDisplacement: (int)aTokens[5].IntValue);
            //                                });
            //AddPattern("[_REG - 1] = _REG", delegate (TokenList aTokens)
            //                                {
            //                                    AS.Move(aTokens[0].Register, aTokens[3].Register, destinationDisplacement: -(int)aTokens[5].IntValue);
            //                                });

            AddPattern("_REG = _ABC", delegate (TokenList aTokens)
                                      {
                                          //TODO: Implement LoadRegister for Labels
                                          //AS.LoadRegister(aTokens[0].Register, GetLabel(aTokens[2]));
                                          AS.LiteralCode("LDR " + aTokens[0].Register.Name + ", =" + GetLabel(aTokens[2]));
                                      });

            // why not [var] like registers? Because its less frequent to access the ptr
            // and it is like a reg.. without [] to get the value...
            AddPattern("_REG = @_ABC", delegate (TokenList aTokens)
                                       {
                                           //TODO: Implement LoadRegister for Labels
                                           //AS.LoadRegister(aTokens[0].Register, GetLabel(aTokens[3]));
                                           AS.LiteralCode("LDR " + aTokens[0].Register.Name + ", =" + GetLabel(aTokens[3]));
                                       });

            //AddPattern(new string[]
            //           {
            //       "Port[DX] = AL", "Port[DX] = AX", "Port[DX] = EAX"
            //           }, delegate (TokenList aTokens)
            //              {
            //                  AS.WriteToPortDX(aTokens[5].Register);
            //              });
            //AddPattern(new string[]
            //           {
            //       "AL = Port[DX]", "AX = Port[DX]", "EAX = Port[DX]"
            //           }, delegate (TokenList aTokens)
            //              {
            //                  AS.ReadFromPortDX(aTokens[0].Register);
            //              });

            AddPattern("+123", delegate (TokenList aTokens)
                               {
                                   AS.Move(r12, aTokens[0].IntValue);
                                   AS.Push(r12);
                               });
            //AddPattern(new string[]
            //           {
            //       "+123 as byte", "+123 as word", "+123 as dword"
            //           }, delegate (TokenList aTokens)
            //              {
            //                  var xSize = GetSize(aTokens[1]);
            //                  AS.Push(aTokens[1].IntValue, size: xSize);
            //              });
            AddPattern("+_REG", delegate (TokenList aTokens)
                                {
                                    AS.Push(aTokens[1].Register);
                                });
            AddPattern(new string[]
                       {
                   //0  1  2   3
                   "+#_ABC", "+#_ABC as byte", "+#_ABC as word", "+#_ABC as dword"
                       }, delegate (TokenList aTokens)
                          {
                              //TODO: Implement LoadRegister for Labels
                              //AS.LoadRegister(r12, ConstLabel(aTokens[1]));
                              AS.LiteralCode("LDR " + r12.Name + ", =" + ConstLabel(aTokens[1]));
                              AS.Push(r12);
                          });
            //AddPattern("+All", delegate (TokenList aTokens)
            //                   {
            //                       AS.PushAllRegisters();
            //                   });
            //AddPattern("-All", delegate (TokenList aTokens)
            //                   {
            //                       AS.PopAllRegisters();
            //                   });
            AddPattern("-_REG", delegate (TokenList aTokens)
                                {
                                    AS.Pop(aTokens[1].Register);
                                });

            AddPattern("_ABC = _REG", delegate (TokenList aTokens)
                                      {
                                          //TODO: Implement LoadRegister for Labels
                                          //AS.LoadRegister(r12, GetLabel(aTokens[0]));
                                          AS.LiteralCode("LDR " + r12.Name + ", =" + GetLabel(aTokens[0]));
                                          AS.StoreRegister(aTokens[2].Register, r12);
                                      });
            AddPattern("_ABC = #_ABC", delegate (TokenList aTokens)
                                       {
                                           //TODO: Check this
                                           AS.LiteralCode("LDR " + r12.Name + ", =" + GetLabel(aTokens[0]));
                                           AS.LiteralCode("LDR " + r11.Name + ", =" + ConstLabel(aTokens[3]));
                                           AS.StoreRegister(r11, r12);
                                           //AS.Move(GetLabel(aTokens[0]), ConstLabel(aTokens[3]), destinationIsIndirect: true, size: RegisterSize.Int32);
                                       });
            AddPattern("_ABC = 123", delegate (TokenList aTokens)
                                     {
                                         //TODO: Check this
                                         AS.Move(r12, aTokens[2].IntValue);
                                         AS.LiteralCode("LDR " + r11.Name + ", =" + GetLabel(aTokens[0]));
                                         AS.StoreRegister(r12, r11);
                                         //AS.Move(GetLabel(aTokens[0]), aTokens[2].IntValue, destinationIsIndirect: true);
                                     });
            //AddPattern(new string[]
            //           {
            //       "_ABC = 123 as byte", "_ABC = 123 as word", "_ABC = 123 as dword"
            //           }, delegate (TokenList aTokens)
            //              {
            //                  AS.Move(GetLabel(aTokens[0]), aTokens[2].IntValue, size: GetSize(aTokens[4]));
            //              });

            AddPattern(new string[]
                       {
                   "_REG + 1",
                       }, delegate (TokenList aTokens)
                          {
                              AS.Add(aTokens[0].Register, aTokens[2].IntValue);
                          });

            AddPattern(new string[]
                       {
                   "_REG + _REG"
                       }, delegate (TokenList aTokens)
                          {
                              AS.Add(aTokens[0].Register, aTokens[2].Register);
                          });
            AddPattern(new string[]
                       {
                   "_REG - 1",
                       }, delegate (TokenList aTokens)
                          {
                              AS.Subtract(aTokens[0].Register, aTokens[2].IntValue);
                          });
            AddPattern(new string[]
                       {
                   "_REG - _REG"
                       }, delegate (TokenList aTokens)
                          {
                              AS.Subtract(aTokens[0].Register, aTokens[2].Register);
                          });
            AddPattern(new string[]
                       {
                   "_REG * 1",
                       }, delegate (TokenList aTokens)
                          {
                              AS.Move(r12, aTokens[2].IntValue);
                              AS.Multiply(aTokens[0].Register, r12);
                          });
            AddPattern(new string[]
                       {
                   "_REG * _REG"
                       }, delegate (TokenList aTokens)
                          {
                              AS.Multiply(aTokens[0].Register, aTokens[2].Register);
                          });
            AddPattern("_REG++", delegate (TokenList aTokens)
                                 {
                                     AS.Add(aTokens[0].Register, 1);
                                 });
            AddPattern("_REG--", delegate (TokenList aTokens)
                                 {
                                     AS.Subtract(aTokens[0].Register, 1);
                                 });

            AddPattern(new string[]
                       {
                   "_REG & 1",
                       }, delegate (TokenList aTokens)
                          {
                              //TODO: Check this
                              AS.And(aTokens[0].Register, aTokens[0].Register, aTokens[2].IntValue);
                          });
            AddPattern(new string[]
                       {
                   "_REG & _REG"
                       }, delegate (TokenList aTokens)
                          {
                              //TODO: Check this
                              AS.And(aTokens[0].Register, aTokens[0].Register, aTokens[2].Register);
                          });
            AddPattern(new string[]
                       {
                   "_REG | 1",
                       }, delegate (TokenList aTokens)
                          {
                              //TODO: Check this
                              AS.Or(aTokens[0].Register, aTokens[0].Register, aTokens[2].IntValue);
                          });
            AddPattern(new string[]
                       {
                   "_REG | _REG"
                       }, delegate (TokenList aTokens)
                          {
                              //TODO: Check this
                              AS.Or(aTokens[0].Register, aTokens[0].Register, aTokens[2].Register);
                          });
            AddPattern(new string[]
                       {
                   "_REG ^ 1",
                       }, delegate (TokenList aTokens)
                          {
                              //TODO: Check this
                              AS.ExclusiveOr(aTokens[0].Register, aTokens[0].Register, aTokens[2].IntValue);
                          });
            AddPattern(new string[]
                       {
                   "_REG ^ _REG"
                       }, delegate (TokenList aTokens)
                          {
                              //TODO: Check this
                              AS.ExclusiveOr(aTokens[0].Register, aTokens[0].Register, aTokens[2].Register);
                          });

            // End block. This handle both terminating a standard block as well as a function or an
            // interrupt handler.
            AddPattern("}", delegate (TokenList aTokens)
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
                                        AS.Branch(BlockLabel("Begin"));
                                        AS.Label(BlockLabel("End"));
                                        xBlock.AddContentsToParentAssembler();
                                    }
                                    else if (xToken1.Matches("if"))
                                    {
                                        AS.Label(BlockLabel("End"));
                                        xBlock.AddContentsToParentAssembler();
                                    }
                                    else
                                    {
                                        throw new Exception("Unknown block starter.");
                                    }
                                    mBlocks.End();
                                }
                            });

            AddPattern("namespace _ABC", delegate (TokenList aTokens)
                                         {
                                             mNamespace = aTokens[1].RawValue;
                                         });

            AddPattern("Return", delegate
                                 {
                                     AS.Branch(FuncLabel("Exit"));
                                 });

            AddPattern("Repeat 4 times {", delegate (TokenList aTokens)
                                           {
                                               mBlocks.Start(aTokens, true);
                                           });

            AddPattern("Interrupt _ABC {", delegate (TokenList aTokens)
                                           {
                                               StartFunc(aTokens[1].RawValue);
                                               mInIntHandler = true;
                                               AS.Label(GetNamespace() + "_" + aTokens[1].RawValue);
                                           });

            // This needs to be different from return.
            // return jumps to exit, ret does raw x86 ret
            AddPattern("Ret", delegate (TokenList aTokens)
                              {
                                  AS.BranchAndExchange(lr);
                              });
            AddPattern("IRet", delegate (TokenList aTokens)
                               {
                                   AS.ExceptionReturn();
                               });

            AddPattern("Function _ABC {", delegate (TokenList aTokens)
                                          {
                                              StartFunc(aTokens[1].RawValue);
                                              mInIntHandler = false;
                                              AS.Label(GetNamespace() + "_" + aTokens[1].RawValue);
                                          });

            AddPattern("Checkpoint 'Text'", delegate (TokenList aTokens)
                                            {
                                          // This method emits a lot of ASM, but thats what we want becuase
                                          // at this point we need ASM as simple as possible and completely transparent.
                                          // No stack changes, no register mods, no calls, no jumps, etc.

                                          // TODO: Add an option on the debug project properties to turn this off.
                                          // Also see WriteDebugVideo in CosmosAssembler.cs
                                          var xPreBootLogging = true;
                                                if (xPreBootLogging)
                                                {
                                                    uint xVideo = 0xB8000;
                                                    for (uint i = xVideo; i < xVideo + 80 * 2; i = i + 2)
                                                    {
                                                        AS.SetByte(i, 0);
                                                        AS.SetByte(i + 1, 2);
                                                    }

                                                    foreach (var xChar in aTokens[1].RawValue)
                                                    {
                                                        AS.SetByte(xVideo, (byte)xChar);
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
                AS.BranchWithLink(GroupLabel(aTokens[0].RawValue));
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
