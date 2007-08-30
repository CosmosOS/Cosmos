using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public class Assembler: IDisposable {
		private static Assembler mCurrentInstance;
		private List<Instruction> mInstructions = new List<Instruction>();
		private StreamWriter mOutputWriter;
		public const string EntryPointLabelName = "___ENTRYPOINT___";

		public Assembler(StreamWriter aOutputWriter) {
			if (mCurrentInstance != null) {
				throw new Exception("There already is an Assembler Instance!");
			}
			mCurrentInstance = this;
			mOutputWriter = aOutputWriter;
		}

		public static Assembler Current {
			get {
				if (mCurrentInstance == null) {
					throw new Exception("No current Assembler Instance!");
				}
				return mCurrentInstance;
			}
		}

		#region IDisposable Members
		public void Dispose() {
			// MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
			mCurrentInstance = null;
		}
		#endregion

		public void Add(Instruction aInstruction) {
			mInstructions.Add(aInstruction);
		}

		public void Flush() {
			// write .asm header
			// debug: we're generating .dll so we can test the outpu
			mOutputWriter.WriteLine("format PE dll");
			//mOutputWriter.WriteLine("entry " + EntryPointLabelName);
			mOutputWriter.WriteLine();
			mOutputWriter.WriteLine("section '.code' code readable executable");
			mOutputWriter.WriteLine();
			foreach (Instruction x in mInstructions) {
				mOutputWriter.WriteLine(x.ToString());
			}
		}
	}
}