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
		private int mResultSize;
		private int? TotalArgumentSize = null;
		private bool mIsDebugger_Break = false;
		private int[] ArgumentSizes = new int[0];
		private MethodInformation mMethodInfo;
		private MethodInformation mTargetMethodInfo;
		private string mNextLabelName;
		private int mCurrentILOffset;
		public Call(MethodBase aMethod, int aCurrentILOffset)
			: base(null, null) {
			if (aMethod == null) {
				throw new ArgumentNullException("aMethod");
			}
			Initialize(aMethod, aCurrentILOffset);
		}

		public static void EmitExceptionLogic(Assembler.Assembler aAssembler, int aCurrentOpOffset, MethodInformation aMethodInfo, string aNextLabel, bool aDoTest, Action aCleanup) {
			string xJumpTo = MethodFooterOp.EndOfMethodLabelNameException;
			if (aMethodInfo != null && aMethodInfo.CurrentHandler != null) {
				if (aMethodInfo.CurrentHandler.HandlerOffset >= aCurrentOpOffset && (aMethodInfo.CurrentHandler.HandlerLength + aMethodInfo.CurrentHandler.HandlerOffset) <= aCurrentOpOffset) {
					return;
				}
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
			if (!aDoTest) {
				//new CPUx86.Call("_CODE_REQUESTED_BREAK_");
				new CPUx86.JumpAlways(xJumpTo);
			} else {
				new CPUx86.Test(CPUx86.Registers.ECX, 2);
				if (aCleanup != null) {
					new CPUx86.JumpIfEquals(aNextLabel);
					aCleanup();
					new CPUx86.JumpAlways(xJumpTo);
				} else {
					new CPUx86.JumpIfNotEquals(xJumpTo);
				}
			}
		}

		private void Initialize(MethodBase aMethod, int aCurrentILOffset) {
			mIsDebugger_Break = aMethod.GetFullName() == "System.Void  System.Diagnostics.Debugger.Break()";
			if (mIsDebugger_Break) {
				return;
			}
			mCurrentILOffset = aCurrentILOffset;
			mTargetMethodInfo = Engine.GetMethodInfo(aMethod, aMethod, Label.GenerateLabelName(aMethod), Engine.GetTypeInfo(aMethod.DeclaringType));
			mResultSize = 0;
			if (mTargetMethodInfo != null) {
				mResultSize = mTargetMethodInfo.ReturnSize;
				if (mResultSize > 8) {
					throw new Exception("ReturnValues of sizes larger than 8 bytes not supported yet (" + mResultSize + ")");
				}
			}
			LabelName = CPU.Label.GenerateLabelName(aMethod);
			Engine.QueueMethod(aMethod);
			bool needsCleanup = false;
			List<int> xArgumentSizes = new List<int>();
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
				TotalArgumentSize = ArgumentSizes.Sum();
			}
			// todo: add support for other argument sizes
		}

		public Call(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			MethodBase xMethod = aReader.OperandValueMethod;
			mMethodInfo = aMethodInfo;
			if (!aReader.EndOfStream) {
				mNextLabelName = GetInstructionLabel(aReader.NextPosition);
			}
			Initialize(xMethod, (int)aReader.Position);
		}
		public void Assemble(string aMethod, int aArgumentCount) {
			new CPUx86.Call(aMethod);
			EmitExceptionLogic(Assembler, mCurrentILOffset, mMethodInfo, mNextLabelName, true, null);
			for (int i = 0; i < aArgumentCount; i++) {
				Assembler.StackContents.Pop();
			}
			if (mResultSize == 0) {
				return;
			}
			if (mResultSize <= 4) {
				new CPUx86.Push(CPUx86.Registers.EAX);
				Assembler.StackContents.Push(new StackContent(mResultSize, ((MethodInfo)mTargetMethodInfo.Method).ReturnType));
				return;
			}
			if (mResultSize <= 8) {
				new CPUx86.Push(CPUx86.Registers.EBX);
				new CPUx86.Push(CPUx86.Registers.EAX);
				Assembler.StackContents.Push(new StackContent(mResultSize, ((MethodInfo)mTargetMethodInfo.Method).ReturnType));
				return;
			}
		}

		protected virtual void HandleDebuggerBreak() {
			//
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