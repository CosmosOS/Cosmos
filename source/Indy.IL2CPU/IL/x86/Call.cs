using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Call)]
	public class Call: Op {
		private string LabelName;
		private uint mResultSize;
		private uint? TotalArgumentSize = null;
		private bool mIsDebugger_Break = false;
		private uint[] ArgumentSizes = new uint[0];
		private MethodInformation mMethodInfo;
		private MethodInformation mTargetMethodInfo;
		private string mNextLabelName;
		private uint mCurrentILOffset;

        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            MethodBase xMethod = aReader.OperandValueMethod;
            ScanOp(xMethod);
        }

        public static void ScanOp(MethodBase aTargetMethod) {
            Engine.QueueMethod(aTargetMethod);
            foreach (ParameterInfo xParam in aTargetMethod.GetParameters())
            {
                Engine.RegisterType(xParam.ParameterType);
            }
            var xTargetMethodInfo = Engine.GetMethodInfo(aTargetMethod, aTargetMethod, Label.GenerateLabelName(aTargetMethod), Engine.GetTypeInfo(aTargetMethod.DeclaringType), false);
            Engine.RegisterType(xTargetMethodInfo.ReturnType);
        }

	    public Call(MethodBase aMethod,
	                uint aCurrentILOffset,
	                bool aDebugMode,
	                uint aExtraStackSpace,
	                string aNormalNext): base(null, null)
        {
            if (aMethod == null)
            {
                throw new ArgumentNullException("aMethod");
            }
            Initialize(aMethod, aCurrentILOffset, aDebugMode);
	        mNextLabelName = aNormalNext;
        }

		public Call(MethodBase aMethod, uint aCurrentILOffset, bool aDebugMode,string aNormalNext)
			: this(aMethod, aCurrentILOffset, aDebugMode, 0, aNormalNext) {
		}

		public static void EmitExceptionLogic(Assembler.Assembler aAssembler, uint aCurrentOpOffset, MethodInformation aMethodInfo, string aNextLabel, bool aDoTest, Action aCleanup) {
			string xJumpTo = MethodFooterOp.EndOfMethodLabelNameException;
			if (aMethodInfo != null && aMethodInfo.CurrentHandler != null) {
                // todo add support for nested handlers, see comment in Engine.cs
                //if (!((aMethodInfo.CurrentHandler.HandlerOffset < aCurrentOpOffset) || (aMethodInfo.CurrentHandler.HandlerLength + aMethodInfo.CurrentHandler.HandlerOffset) <= aCurrentOpOffset)) {
                new CPU.Comment(String.Format("CurrentOffset = {0}, HandlerStartOffset = {1}", aCurrentOpOffset, aMethodInfo.CurrentHandler.HandlerOffset));
                if(aMethodInfo.CurrentHandler.HandlerOffset > aCurrentOpOffset) {
                    switch (aMethodInfo.CurrentHandler.Flags) {
                        case ExceptionHandlingClauseOptions.Clause: {
                            xJumpTo = Op.GetInstructionLabel(aMethodInfo.CurrentHandler.HandlerOffset);
                            break;
                        }
                        case ExceptionHandlingClauseOptions.Finally: {
                            xJumpTo = Op.GetInstructionLabel(aMethodInfo.CurrentHandler.HandlerOffset);
                            break;
                        }
                        default: {
                            throw new Exception("ExceptionHandlerType '" + aMethodInfo.CurrentHandler.Flags.ToString() + "' not supported yet!");
                        }
                    }
                }
			}
			if (!aDoTest) {
				//new CPUx86.Call("_CODE_REQUESTED_BREAK_");
                new CPUx86.Jump { DestinationLabel = xJumpTo };
			} else {
                new CPUx86.Test { DestinationReg = CPUx86.Registers.ECX, SourceValue = 2 };
				if (aCleanup != null) {
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = aNextLabel };
					aCleanup();
                    new CPUx86.Jump { DestinationLabel = xJumpTo };
				} else {
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = xJumpTo };
				}
			}
		}

		private void Initialize(MethodBase aMethod, uint aCurrentILOffset, bool aDebugMode) {
			mIsDebugger_Break = aMethod.GetFullName() == "System.Void  System.Diagnostics.Debugger.Break()";
		    if (mIsDebugger_Break) {
				return;
			}
			mCurrentILOffset = aCurrentILOffset;
			mTargetMethodInfo = Engine.GetMethodInfo(aMethod, aMethod, Label.GenerateLabelName(aMethod), Engine.GetTypeInfo(aMethod.DeclaringType), aDebugMode);
			mResultSize = 0;
			if (mTargetMethodInfo != null) {
				mResultSize = mTargetMethodInfo.ReturnSize;
			}
			LabelName = CPU.Label.GenerateLabelName(aMethod);
			Engine.QueueMethod(aMethod);
			bool needsCleanup = false;
			List<uint> xArgumentSizes = new List<uint>();
			ParameterInfo[] xParams = aMethod.GetParameters();
			foreach (ParameterInfo xParam in xParams) {
				xArgumentSizes.Add(Engine.GetFieldStorageSize(xParam.ParameterType));
			}
			if (!aMethod.IsStatic) {
				xArgumentSizes.Insert(0, 4);
			}
			ArgumentSizes = xArgumentSizes.ToArray();
			foreach (ParameterInfo xParam in xParams) {
				if (xParam.IsOut) {
					needsCleanup = true;
					break;
				}
			}
			if (needsCleanup) {
                TotalArgumentSize = 0;
                foreach (var xArgSize in ArgumentSizes) {
                    TotalArgumentSize += xArgSize;
                }
			}
			// todo: add support for other argument sizes
		}

		public Call(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			MethodBase xMethod = aReader.OperandValueMethod;
			mMethodInfo = aMethodInfo;
			if (!aReader.EndOfStream) {
				mNextLabelName = GetInstructionLabel(aReader.NextPosition);
			}else {
			    mNextLabelName = X86MethodFooterOp.EndOfMethodLabelNameNormal;
			}
		    Initialize(xMethod, (uint)aReader.Position,aMethodInfo.DebugMode);
		}
        public void Assemble(string aMethod, int aArgumentCount) {
            if (mTargetMethodInfo.ExtraStackSize > 0) {
                new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mTargetMethodInfo.ExtraStackSize };
            }
            new CPUx86.Call { DestinationLabel = aMethod };
            //if (mResultSize != 0) {
                //new CPUx86.Pop("eax");
            //}
            EmitExceptionLogic(Assembler,
                               mCurrentILOffset,
                               mMethodInfo,
                               mNextLabelName,
                               true,
                               delegate() {
                                   var xResultSize = mTargetMethodInfo.ReturnSize;
                                   if(xResultSize %4!=0) {
                                       xResultSize += 4 - (xResultSize % 4);
                                   }
                                   for(int i = 0; i< xResultSize/4;i++) {
                                       new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                                   }
                               });
            for (int i = 0; i < aArgumentCount; i++) {
                Assembler.StackContents.Pop();
            }
            if (mResultSize == 0) {
                return;
            }

            Assembler.StackContents.Push(new StackContent((int)mResultSize,
                                                          ((MethodInfo)mTargetMethodInfo.Method).ReturnType));
        }

	    protected virtual void HandleDebuggerBreak() {
            new CPUx86.Call { DestinationLabel = "DebugStub_Step" };
		}

		public override void DoAssemble() {
			if (mIsDebugger_Break) {
				HandleDebuggerBreak();
			} else {
				Assemble(LabelName, ArgumentSizes.Length);
			}
		}
	}
}