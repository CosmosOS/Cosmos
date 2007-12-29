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
		public Assembler(StreamWriter aOutputWriter)
			: base(aOutputWriter) {
		}

		public Assembler(StreamWriter aOutputWriter, bool aInMetalMode)
			: base(aOutputWriter, aInMetalMode) {
		}

		public OutputTypeEnum OutputType {
			get {
				return mOutputType;
			}
			set {
				mOutputType = value;
			}
		}

		protected override void EmitCodeSectionHeader() {
			base.EmitCodeSectionHeader();
			mOutputWriter.WriteLine("section '.text' code readable executable");
		}

		protected override void EmitDataSectionHeader() {
			base.EmitDataSectionHeader();
			mOutputWriter.WriteLine("section '.data' data writeable");
		}

		protected override void EmitIDataSectionHeader() {
			mOutputWriter.WriteLine("section '.idata' data readable import");
		}

		protected override void EmitDataSectionFooter() {
			base.EmitDataSectionFooter();
			//
		}

		protected override void EmitFooter() {
		}

		protected override void EmitImportMembers() {
			mOutputWriter.WriteLine();
			foreach (ImportMember xImportMember in ImportMembers) {
				mOutputWriter.WriteLine("\tdd 0,0,0,rva {0}_name,rva {0}_table", xImportMember.Name);
			}
			mOutputWriter.WriteLine("\tdd 0,0,0,0,0");
			mOutputWriter.WriteLine();
			foreach (ImportMember xImportMember in ImportMembers) {
				mOutputWriter.WriteLine("\t{0}_table:", xImportMember.Name);
				foreach (ImportMethodMember xImportMethod in xImportMember.Methods) {
					mOutputWriter.WriteLine("\t\t{0} dd rva _{0}", xImportMethod.Name);
				}
				mOutputWriter.WriteLine("\t\tdd 0");
				mOutputWriter.WriteLine();
			}
			foreach (ImportMember xImportMember in ImportMembers) {
				mOutputWriter.WriteLine("\t{0}_name db '{1}',0", xImportMember.Name, xImportMember.FileName);
			}
			mOutputWriter.WriteLine();
			foreach (ImportMember xImportMember in ImportMembers) {
				foreach (ImportMethodMember xImportMethod in xImportMember.Methods) {
					mOutputWriter.WriteLine("\t_{0} dw 0", xImportMethod.Name);
					mOutputWriter.WriteLine("\tdb '{0}',0", xImportMethod.Name);
				}
			}
		}

		protected override void EmitHeader() {
			switch (mOutputType) {
				case OutputTypeEnum.GUI:
					mOutputWriter.WriteLine("format PE GUI 4.0");
					break;
				case OutputTypeEnum.DLL:
					mOutputWriter.WriteLine("format PE dll");
					break;
				default:
					mOutputWriter.WriteLine("format PE console");
					break;
			}
			if (mOutputType != OutputTypeEnum.DLL) {
				mOutputWriter.WriteLine("entry " + EntryPointName);
			}
			mOutputWriter.WriteLine("stack 0x{0}", StackSize.ToString("X"));
		}
	}
}
