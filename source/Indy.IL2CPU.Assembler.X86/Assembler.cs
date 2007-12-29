using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public abstract class Assembler: Indy.IL2CPU.Assembler.Assembler {
		public Assembler(StreamWriter aOutputWriter, bool aInMetalMode)
			: base(aOutputWriter, aInMetalMode) {
		}

		public Assembler(StreamWriter aOutputWriter)
			: base(aOutputWriter) {
		}

		protected override void EmitHeader() {
			//mOutputWriter.WriteLine("format ms coff  ");
			//mOutputWriter.WriteLine("org 0220000h    ; the best place to load our kernel to. ");
			mOutputWriter.WriteLine("use32           ; the kernel will be run in 32-bit protected mode, ");
			mOutputWriter.WriteLine("");
			//			List<string> xTheLabels = new List<string>();
			//			string mLastRealLabel = "";
			//			foreach (IL2CPU.Assembler.Instruction x in mInstructions) {
			//				Label xLabel = x as Label;
			//				if (xLabel != null) {
			//					if (!xLabel.Name.StartsWith(".")) {
			//						mLastRealLabel = xLabel.Name;
			//						xTheLabels.Add(xLabel.Name);
			//					} else {
			//						xTheLabels.Add(mLastRealLabel + "@@" + xLabel.Name.Substring(1));
			//					}
			//				}
			//			}
			//			if (xTheLabels.Count > 0) {
			//	mOutputWriter.WriteLine("include 'export.inc'");
			//	mOutputWriter.WriteLine("section '.edata' export data readable");
			//	mOutputWriter.WriteLine("");
			//	mOutputWriter.WriteLine("\texport 'OUTPUT.dll', \\");
			//				for (int i = 0; i < xTheLabels.Count; i++) {
			//					if (i == (xTheLabels.Count - 1)) {
			//			mOutputWriter.WriteLine("\t\t {0},'{1}'", xTheLabels[i].Replace("@@", "."), xTheLabels[i]);
			//					} else {
			//				mOutputWriter.WriteLine("\t\t{0},'{1}', \\", xTheLabels[i].Replace("@@", "."), xTheLabels[i]);
			//					}
			// public entry as 'entry'
			//mOutputWriter.WriteLine("\tpublic {0} as '{0}'", xTheLabels[i].Replace("@@", "."), xTheLabels[i]);
			//				}
			//			}
			//mOutputWriter.WriteLine("section '.code' code readable executable");
			mOutputWriter.WriteLine("");
		}
	}
}
