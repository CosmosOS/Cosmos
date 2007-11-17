using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using i4o;

namespace Indy.IL2CPU.Assembler {
	public abstract class Assembler: IDisposable {
		public const string EntryPointName = "__ENGINE_ENTRYPOINT__";
		protected List<Instruction> mInstructions = new List<Instruction>();
		private IndexableCollection<DataMember> mDataMembers = new IndexableCollection<DataMember>();
		protected StreamWriter mOutputWriter;
		private List<string> mIncludes = new List<string>();
		private IndexableCollection<ImportMember> mImportMembers = new IndexableCollection<ImportMember>();
		private readonly bool mInMetalMode = false;
		public readonly Stack<int> StackSizes = new Stack<int>();

		private uint mDataMemberCounter = 0;
		public string GetIdentifier(string aPrefix) {
			return aPrefix + mDataMemberCounter++.ToString("X8").ToUpper();
		}

		public Assembler(StreamWriter aOutputWriter)
			: this(aOutputWriter, false) {
		}

		public Assembler(StreamWriter aOutputWriter, bool aInMetalMode) {
			mOutputWriter = aOutputWriter;
			mInMetalMode = aInMetalMode;
		}

		public List<string> Includes {
			get {
				return mIncludes;
			}
		}

		public IndexableCollection<DataMember> DataMembers {
			get {
				return mDataMembers;
			}
		}

		public IndexableCollection<ImportMember> ImportMembers {
			get {
				return mImportMembers;
			}
		}

		public bool InMetalMode {
			get {
				return mInMetalMode;
			}
		}

		public void Dispose() {
			// MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
			mInstructions.Clear();
			mDataMembers.Clear();
		}

		public void Add(params Instruction[] aInstructions) {
			foreach (Instruction xInstruction in aInstructions) {
				mInstructions.Add(xInstruction);
			}
		}

		protected abstract void EmitHeader();

		public virtual void Flush() {
			// write .asm header
			EmitHeader();
			mOutputWriter.WriteLine();
			foreach (string xInclude in mIncludes) {
				mOutputWriter.WriteLine("include '{0}'", xInclude);
			}
			mOutputWriter.WriteLine();
			if (mDataMembers.Count > 0) {
				EmitDataSectionHeader();
				mOutputWriter.WriteLine();
				foreach (DataMember xMember in mDataMembers) {
					mOutputWriter.WriteLine("\t" + xMember);
				}
				mOutputWriter.WriteLine();
			}
			if (mInstructions.Count > 0) {
				EmitCodeSectionHeader();
				mOutputWriter.WriteLine();
				foreach (Instruction x in mInstructions) {
					string prefix = "\t\t\t";
					if (x is Label) {
						mOutputWriter.WriteLine();
						if (x.ToString()[0] == '.') {
							prefix = "\t\t";
						} else {
							prefix = "\t";
						}
					}
					mOutputWriter.WriteLine(prefix + x);
				}
				mOutputWriter.WriteLine();
			}
			if (mImportMembers.Count > 0) {
				EmitIDataSectionHeader();
				mOutputWriter.WriteLine();
				foreach (ImportMember xImportMember in mImportMembers) {
					mOutputWriter.WriteLine("\tdd 0,0,0,rva {0}_name,rva {0}_table", xImportMember.Name);
				}
				mOutputWriter.WriteLine("\tdd 0,0,0,0,0");
				mOutputWriter.WriteLine();
				foreach (ImportMember xImportMember in mImportMembers) {
					mOutputWriter.WriteLine("\t{0}_table:", xImportMember.Name);
					foreach (ImportMethodMember xImportMethod in xImportMember.Methods) {
						mOutputWriter.WriteLine("\t\t{0} dd rva _{0}", xImportMethod.Name);
					}
					mOutputWriter.WriteLine("\t\tdd 0");
					mOutputWriter.WriteLine();
				}
				foreach (ImportMember xImportMember in mImportMembers) {
					mOutputWriter.WriteLine("\t{0}_name db '{1}',0", xImportMember.Name, xImportMember.FileName);
				}
				mOutputWriter.WriteLine();
				foreach (ImportMember xImportMember in mImportMembers) {
					foreach (ImportMethodMember xImportMethod in xImportMember.Methods) {
						mOutputWriter.WriteLine("\t_{0} dw 0", xImportMethod.Name);
						mOutputWriter.WriteLine("\tdb '{0}',0", xImportMethod.Name);
					}
				}
			}
			EmitFooter();
		}

		protected virtual void EmitIDataSectionHeader() {
			mOutputWriter.WriteLine("section '.idata' import data readable writeable");
		}

		protected virtual void EmitCodeSectionHeader() {
			mOutputWriter.WriteLine("section '.code' code readable executable");
		}

		protected virtual void EmitDataSectionHeader() {
			mOutputWriter.WriteLine("section '.data' data readable writeable");
		}

		protected virtual void EmitFooter() {
		}

		public List<Instruction> Instructions {
			get {
				return mInstructions;
			}
		}
	}
}