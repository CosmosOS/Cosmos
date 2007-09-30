using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.Win32 {
	public class Assembler: IL2CPU.Assembler.X86.Assembler {
		public enum OutputTypeEnum {
			DLL,
			Console,
			GUI
		}
		private OutputTypeEnum mOutputType = OutputTypeEnum.DLL;
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

		protected override void EmitHeader() {
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
		}
	}
}
