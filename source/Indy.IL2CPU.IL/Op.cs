using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using System.Reflection;

namespace Indy.IL2CPU.IL {
	public abstract class Op {
		private readonly string mCurrentInstructionLabel;
		private readonly string mILComment;
		public static string GetInstructionLabel(ILReader aReader) {
			return GetInstructionLabel(aReader.Position);
		}
		public static string GetInstructionLabel(long aPosition) {
			return ".L" + aPosition.ToString("X8");
		}

		public delegate void QueueMethodHandler(MethodBase aMethod);

		public delegate void QueueStaticFieldHandler(FieldInfo aField);

		public void Assemble() {
			if (!String.IsNullOrEmpty(mCurrentInstructionLabel)) {
				new Label(mCurrentInstructionLabel);
			}
			if (!String.IsNullOrEmpty(mILComment)) {
				new Comment(mILComment);
			}
			AssembleHeader();
			DoAssemble();
		}

		protected virtual void AssembleHeader() {
		}

		public abstract void DoAssemble();

		public Op(ILReader aReader, MethodInformation aMethodInfo) {
			if (aReader != null) {
				mCurrentInstructionLabel = GetInstructionLabel(aReader);
				// todo: need to add the real operand here?
				mILComment = "; IL: " + aReader.OpCode + " " + aReader.Operand;
			}
		}

		// This is a prop and not a constructor arg for two reasons. Ok, mostly for one
		// 1 - Adding a parameterized triggers annoying C# constructor hell, every descendant we'd have to reimplement it
		// 2 - This helps to allow changing of assembler while its in use. Currently no idea why we would ever want to do that
		// rather than construct a new one though....
		// If we end up with more things we need, probably will change to Initialize(x, y), or someone can go thorugh and add
		// all the friggin constructors
		protected Assembler.Assembler mAssembler;
		public Assembler.Assembler Assembler {
			get {
				return mAssembler;
			}
			set {
				mAssembler = value;
			}
		}

		protected void DoQueueMethod(MethodBase aMethod) {
			if (QueueMethod == null) {
				throw new Exception("IL Op wants to queue a method, but no QueueMethod handler suppplied!");
			}
			QueueMethod(aMethod);
		}

		protected void DoQueueStaticField(FieldInfo aField) {
			if (QueueStaticField == null) {
				throw new Exception("IL Op wants to queue a static field, but no QueueStaticField handler supplied!");
			}
			if (!aField.IsStatic) {
				throw new Exception("Cannot add instance fields to the StaticField queue!");
			}
			QueueStaticField(aField);
		}

		public static QueueMethodHandler QueueMethod;
		public static QueueStaticFieldHandler QueueStaticField;
	}
}
