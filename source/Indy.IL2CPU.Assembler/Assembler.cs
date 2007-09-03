using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	public class Assembler: IDisposable {
		public enum OutputTypeEnum {
			DLL,
			Console,
			GUI
		}

		public const string EntryPointLabelName = "___ENTRYPOINT___";
		private List<Instruction> mInstructions = new List<Instruction>();
		private List<DataMember> mDataMembers = new List<DataMember>();
		private OutputTypeEnum mOutputType = OutputTypeEnum.DLL;
		private StreamWriter mOutputWriter;
		private List<string> mIncludes = new List<string>();
		private List<ImportMember> mImportMembers = new List<ImportMember>();

		private uint mDataMemberCounter = 0;
		public string GetIdentifier(string aPrefix) {
			return aPrefix + mDataMemberCounter++.ToString("X8").ToUpper();
		}

		public Assembler(StreamWriter aOutputWriter) {
			mOutputWriter = aOutputWriter;
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

		public List<ImportMember> ImportMembers {
			get {
				return mImportMembers;
			}
		}

		public void Dispose() {
			// MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
			mInstructions.Clear();
			mDataMembers.Clear();
		}

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
			if (mOutputType != OutputTypeEnum.DLL) {
				mOutputWriter.WriteLine("entry " + EntryPointLabelName);
			}
			mOutputWriter.WriteLine();
			foreach (string xInclude in mIncludes) {
				mOutputWriter.WriteLine("include '{0}'", xInclude);
			}
			mOutputWriter.WriteLine();
			if (mDataMembers.Count > 0) {
				mOutputWriter.WriteLine("section '.data' data readable writeable");
				mOutputWriter.WriteLine();
				foreach (DataMember xMember in mDataMembers) {
					mOutputWriter.WriteLine("\t" + xMember);
				}
				mOutputWriter.WriteLine();
			}
			if (mInstructions.Count > 0) {
				mOutputWriter.WriteLine("section '.code' code readable executable");
				mOutputWriter.WriteLine();
				mOutputWriter.WriteLine("\t" + EntryPointLabelName + ":");
				foreach (Instruction x in mInstructions) {
					string prefix = "\t\t";
					if (x is Label) {
						mOutputWriter.WriteLine();
						prefix = "    ";
					}
					mOutputWriter.WriteLine(prefix + x);
				}
				mOutputWriter.WriteLine();
			}
			if (mImportMembers.Count > 0) {
				mOutputWriter.WriteLine("section '.idata' import data readable writeable");
				mOutputWriter.WriteLine();
				foreach (ImportMember xImportMember in mImportMembers) {
					mOutputWriter.WriteLine("\t" + xImportMember);
				}
			}
		}

	}
}