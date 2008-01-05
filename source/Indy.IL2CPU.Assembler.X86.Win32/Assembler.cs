using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.Win32 {
	public class Assembler: IL2CPU.Assembler.X86.Assembler {
		public int StackSize = 64 * 1024;
		public enum OutputTypeEnum {
			DLL,
			Console,
			GUI
		}
		private OutputTypeEnum mOutputType = OutputTypeEnum.Console;
		public Assembler(Func<string, string> aGetFileNameForGroup)
			: base(aGetFileNameForGroup) {
		}

		public Assembler(Func<string, string> aGetFileNameForGroup, bool aInMetalMode)
			: base(aGetFileNameForGroup, aInMetalMode) {
		}

		public OutputTypeEnum OutputType {
			get {
				return mOutputType;
			}
			set {
				mOutputType = value;
			}
		}

		protected override void EmitCodeSectionHeader(string aGroup, StreamWriter aOutputWriter) {
			base.EmitCodeSectionHeader(aGroup, aOutputWriter);
			aOutputWriter.WriteLine("section '.text' code readable executable");
		}

		protected override void EmitDataSectionHeader(string aGroup, StreamWriter aOutputWriter) {
			base.EmitDataSectionHeader(aGroup, aOutputWriter);
			aOutputWriter.WriteLine("section '.data' data writeable");
		}

		protected override void EmitIDataSectionHeader(string aGroup, StreamWriter aOutputWriter) {
			aOutputWriter.WriteLine("section '.idata' data readable import");
		}

		protected override void EmitDataSectionFooter(string aGroup, StreamWriter aOutputWriter) {
			base.EmitDataSectionFooter(aGroup, aOutputWriter);
			//
		}

		protected override void EmitFooter(string aGroup, StreamWriter aOutputWriter) {
		}

		protected override void EmitImportMembers(string aGroup, StreamWriter aOutputWriter) {
			aOutputWriter.WriteLine();
			foreach (ImportMember xImportMember in (from item in ImportMembers
														where String.Equals(item.Key, aGroup, StringComparison.InvariantCultureIgnoreCase)
														select item.Value)) {
				aOutputWriter.WriteLine("\tdd 0,0,0,rva {0}_name,rva {0}_table", xImportMember.Name);
			}
			aOutputWriter.WriteLine("\tdd 0,0,0,0,0");
			aOutputWriter.WriteLine();
			foreach (ImportMember xImportMember in (from item in ImportMembers
													where String.Equals(item.Key, aGroup, StringComparison.InvariantCultureIgnoreCase)
													select item.Value)) {
				aOutputWriter.WriteLine("\t{0}_table:", xImportMember.Name);
				foreach (ImportMethodMember xImportMethod in xImportMember.Methods) {
					aOutputWriter.WriteLine("\t\t{0} dd rva _{0}", xImportMethod.Name);
				}
				aOutputWriter.WriteLine("\t\tdd 0");
				aOutputWriter.WriteLine();
			}
			foreach (ImportMember xImportMember in (from item in ImportMembers
													where String.Equals(item.Key, aGroup, StringComparison.InvariantCultureIgnoreCase)
													select item.Value)) {
				aOutputWriter.WriteLine("\t{0}_name db '{1}',0", xImportMember.Name, xImportMember.FileName);
			}
			aOutputWriter.WriteLine();
			foreach (ImportMember xImportMember in (from item in ImportMembers
													where String.Equals(item.Key, aGroup, StringComparison.InvariantCultureIgnoreCase)
													select item.Value)) {
				foreach (ImportMethodMember xImportMethod in xImportMember.Methods) {
					aOutputWriter.WriteLine("\t_{0} dw 0", xImportMethod.Name);
					aOutputWriter.WriteLine("\tdb '{0}',0", xImportMethod.Name);
				}
			}
		}

		protected override void EmitHeader(string aGroup, StreamWriter aOutputWriter) {
			if (aGroup == MainGroup) {
				switch (mOutputType) {
					case OutputTypeEnum.GUI:
						aOutputWriter.WriteLine("format PE GUI 4.0");
						break;
					case OutputTypeEnum.DLL:
						aOutputWriter.WriteLine("format PE dll");
						break;
					default:
						aOutputWriter.WriteLine("format PE console");
						break;
				}
				if (mOutputType != OutputTypeEnum.DLL) {
					aOutputWriter.WriteLine("entry " + EntryPointName);
				}
				aOutputWriter.WriteLine("stack 0x{0}", StackSize.ToString("X"));
			}
		}
	}
}
