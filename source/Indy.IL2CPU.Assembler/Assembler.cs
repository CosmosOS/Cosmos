using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public class Assembler: IDisposable {
		public enum OutputTypeEnum {
			DLL,
			Console,
			GUI
		}

		public const string EntryPointLabelName = "___ENTRYPOINT___";
		private static Assembler mCurrentInstance;
		private List<Instruction> mInstructions = new List<Instruction>();
		private List<DataMember> mDataMembers = new List<DataMember>();
		private OutputTypeEnum mOutputType = OutputTypeEnum.DLL;
		private StreamWriter mOutputWriter;
		private List<string> mIncludes = new List<string>();

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

		public OutputTypeEnum OutputType {
			get {
				return mOutputType;
			}
			set {
				mOutputType = value;
			}
		}

		public List<string> Includes {
			get {
				return mIncludes;
			}
		}

		public List<DataMember> DataMembers {
			get {
				return mDataMembers;
			}
		}

		#region IDisposable Members
		public void Dispose() {
			// MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
			mCurrentInstance = null;
			mInstructions.Clear();
			mDataMembers.Clear();
		}
		#endregion

		public void Add(Instruction aInstruction) {
			mInstructions.Add(aInstruction);
		}

		public void Flush() {
			// write .asm header
			// debug: we're generating .dll so we can test the outpu
			switch (mOutputType) {
				case OutputTypeEnum.Console:
					mOutputWriter.WriteLine("format PE console");
					break;
				case OutputTypeEnum.GUI:
					mOutputWriter.WriteLine("format PE GUI 4.0");
					break;
				default:
					mOutputWriter.WriteLine("format PE dll");
					break;
			}
			if(mOutputType != OutputTypeEnum.DLL) {
				mOutputWriter.WriteLine("entry " + EntryPointLabelName);
			}
			mOutputWriter.WriteLine();
			foreach (string xInclude in mIncludes) {
				mOutputWriter.WriteLine("include '{0}'", xInclude);
			}
			mOutputWriter.WriteLine();
			mOutputWriter.WriteLine("section '.data' data readable writeable");
			mOutputWriter.WriteLine();
			foreach(DataMember xMember in mDataMembers) {
				mOutputWriter.WriteLine("\t" + xMember);
			}
			mOutputWriter.WriteLine();
			mOutputWriter.WriteLine("section '.code' code readable executable");
			mOutputWriter.WriteLine();
			mOutputWriter.WriteLine("    " + EntryPointLabelName + ":");
			foreach (Instruction x in mInstructions) {
				mOutputWriter.WriteLine("\t" + x);
			}
		}
	}
}