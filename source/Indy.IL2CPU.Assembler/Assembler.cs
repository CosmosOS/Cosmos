using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using i4o;
using Mono.Cecil;

namespace Indy.IL2CPU.Assembler {
	public abstract class Assembler: IDisposable {
		// TODO: When threading is being worked on, fix this to work multithreaded!
		//public const string CurrentExceptionDataMember = "__CURRENT_EXCEPTION__";
		public static Exception CurrentException;
		public static void PrintException() {
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.ForegroundColor = ConsoleColor.White;
			string xClearLine = new String(' ', Console.WindowWidth);
			for (int i = 0; i < Console.WindowHeight; i++) {
				Console.Write("                                                                                ");
			}
			//Console.Clear();
			System.Console.WriteLine("Cosmos Kernel. Copyright 2008 The Cosmos Project.");
			System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("BSOD's Rule!");
			Console.WriteLine("");
			Console.Write("Unhandled error occurred: ");
			Console.WriteLine(CurrentException.ToString());
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
		}
		private static FieldDefinition mCurrentExceptionRef;
		public static FieldDefinition CurrentExceptionRef {
			get {
				if (mCurrentExceptionRef == null) {
					AssemblyDefinition xAsm = AssemblyFactory.GetAssembly(typeof(Assembler).Assembly.Location);
					foreach (ModuleDefinition xMod in xAsm.Modules) {
						if (xMod.Types.Contains(typeof(Assembler).FullName)) {
							mCurrentExceptionRef = xMod.Types[typeof(Assembler).FullName].Fields.GetField("CurrentException");
							break;
						}
					}
					if (mCurrentExceptionRef == null) {
						throw new Exception("Couldn't find CurrentException field!");
					}
				}
				return mCurrentExceptionRef;
			}
		}
		public const string EntryPointName = "__ENGINE_ENTRYPOINT__";
		protected IndexableCollection<KeyValuePair<string, Instruction>> mInstructions = new IndexableCollection<KeyValuePair<string, Instruction>>();
		private IndexableCollection<KeyValuePair<string, DataMember>> mDataMembers = new IndexableCollection<KeyValuePair<string, DataMember>>();
		private List<KeyValuePair<string, string>> mIncludes = new List<KeyValuePair<string, string>>();
		private IndexableCollection<KeyValuePair<string, ImportMember>> mImportMembers = new IndexableCollection<KeyValuePair<string, ImportMember>>();
		private readonly bool mInMetalMode = false;
		public readonly Stack<int> StackSizes = new Stack<int>();
		public bool DebugMode {
			get;
			set;
		}
		public static Assembler CurrentInstance {
			get;
			private set;
		}
		private Func<string, string> mGetFileNameForGroup;

		private uint mDataMemberCounter = 0;
		public string GetIdentifier(string aPrefix) {
			return aPrefix + mDataMemberCounter++.ToString("X8").ToUpper();
		}

		public Assembler(Func<string, string> aGetStreamForGroup)
			: this(aGetStreamForGroup, false) {
		}

		public Assembler(Func<string, string> aGetFileNameForGroup, bool aInMetalMode) {
			mGetFileNameForGroup = aGetFileNameForGroup;
			mInMetalMode = aInMetalMode;
			CurrentInstance = this;
			//mInstructions.AddComplexIndexDefinition(
		}

		public IndexableCollection<KeyValuePair<string, Instruction>> Instructions {
			get {
				return mInstructions;
			}
		}

		public IndexableCollection<KeyValuePair<string, DataMember>> DataMembers {
			get {
				return mDataMembers;
			}
		}

		public IndexableCollection<KeyValuePair<string, ImportMember>> ImportMembers {
			get {
				return mImportMembers;
			}
		}

		public List<KeyValuePair<string, string>> Includes {
			get {
				return mIncludes;
			}
		}

		public bool InMetalMode {
			get {
				return mInMetalMode;
			}
		}

		public void Dispose() {
			// MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
			//		Anyhow, we need a way to clear the CurrentInstance property
			mInstructions.Clear();
			mDataMembers.Clear();
			CurrentInstance = null;
		}

		public void Add(params Instruction[] aInstructions) {
			foreach (Instruction xInstruction in aInstructions) {
				mInstructions.Add(new KeyValuePair<string, Instruction>(CurrentGroup, xInstruction));
			}
		}

		public string CurrentGroup {
			get;
			set;
		}

		protected abstract void EmitHeader(string aGroup, StreamWriter aOutputWriter);

		private IEnumerable<string> GetAllGroupNames() {
			List<string> xNames = new List<string>();
			xNames.AddRange((from item in mInstructions
							 where !xNames.Contains(item.Key, StringComparer.InvariantCultureIgnoreCase)
							 select item.Key));
			xNames.AddRange((from item in mDataMembers
							 where !xNames.Contains(item.Key, StringComparer.InvariantCultureIgnoreCase)
							 select item.Key));
			xNames.AddRange((from item in mImportMembers
							 where !xNames.Contains(item.Key, StringComparer.InvariantCultureIgnoreCase)
							 select item.Key));
			xNames.AddRange((from item in mIncludes
							 where !xNames.Contains(item.Key, StringComparer.InvariantCultureIgnoreCase)
							 select item.Key));
			xNames.RemoveAll(x => String.IsNullOrEmpty(x));
			return xNames;
		}

		public virtual void Flush() {
			foreach (string xGroup in GetAllGroupNames()) {
				using (StreamWriter xOutputWriter = new StreamWriter(mGetFileNameForGroup(xGroup))) {
					// write .asm header
					EmitHeader(xGroup, xOutputWriter);
					if (xGroup == MainGroup) {
						foreach (string xTheGroup in GetAllGroupNames()) {
							if (xGroup != xTheGroup) {
								mIncludes.Add(new KeyValuePair<string, string>(xGroup, mGetFileNameForGroup(xTheGroup)));
							}
						}
					}
					xOutputWriter.WriteLine();
					EmitIncludes(xGroup, xOutputWriter);
					xOutputWriter.WriteLine();
					if (mDataMembers.Count > 0) {
						EmitDataSectionHeader(xGroup, xOutputWriter);
						xOutputWriter.WriteLine();
						foreach (DataMember xMember in (from item in mDataMembers
														where String.Equals(item.Key, xGroup, StringComparison.InvariantCultureIgnoreCase)
														select item.Value)) {
							xOutputWriter.WriteLine("\t" + xMember);
						}
						EmitDataSectionFooter(xGroup, xOutputWriter);
						xOutputWriter.WriteLine();
					}
					List<string> xLabels = new List<string>();
					if (mInstructions.Count > 0) {
						EmitCodeSectionHeader(xGroup, xOutputWriter);
						xOutputWriter.WriteLine();
						string xMainLabel = "";
						foreach (Instruction x in (from item in mInstructions
												   where String.Equals(item.Key, xGroup, StringComparison.InvariantCultureIgnoreCase)
												   select item.Value)) {
							string prefix = "\t\t\t";
							Label xLabel = x as Label;
							if (xLabel != null) {
								string xFullName;
								if (xLabel.Name[0] != '.') {
									xMainLabel = xLabel.Name;
									xFullName = xMainLabel;
								} else {
									xFullName = xMainLabel + xLabel.Name;
									xLabels.Add(xFullName);
								}
								xOutputWriter.WriteLine();
								if (x.ToString()[0] == '.') {
									prefix = "\t\t";
								} else {
									prefix = "\t";
								}
								xOutputWriter.WriteLine(prefix + xFullName.Replace(".", "__DOT__") + ":");
								continue;
							}
							xOutputWriter.WriteLine(prefix + x);
						}
						EmitCodeSectionFooter(xGroup, xOutputWriter);
						xOutputWriter.WriteLine();
					}
					if (mImportMembers.Count > 0) {
						EmitIDataSectionHeader(xGroup, xOutputWriter);
						EmitImportMembers(xGroup, xOutputWriter);
						EmitIDataSectionFooter(xGroup, xOutputWriter);
					}
					EmitFooter(xGroup, xOutputWriter);
				}
			}
		}

		protected virtual void EmitIncludes(string aGroup, StreamWriter aOutputWriter) {
		}

		protected abstract void EmitImportMembers(string aGroup, StreamWriter aOutputWriter);

		protected virtual void EmitIDataSectionHeader(string aGroup, StreamWriter aOutputWriter) {
		}

		protected virtual void EmitIDataSectionFooter(string aGroup, StreamWriter aOutputWriter) {
		}

		protected virtual void EmitCodeSectionHeader(string aGroup, StreamWriter aOutputWriter) {
		}

		protected virtual void EmitCodeSectionFooter(string aGroup, StreamWriter aOutputWriter) {
		}

		protected virtual void EmitDataSectionHeader(string aGroup, StreamWriter aOutputWriter) {
		}

		protected virtual void EmitDataSectionFooter(string aGroup, StreamWriter aOutputWriter) {
		}

		public string MainGroup {
			get;
			set;
		}

		protected virtual void EmitFooter(string aGroup, StreamWriter aOutputWriter) {
		}
	}
}