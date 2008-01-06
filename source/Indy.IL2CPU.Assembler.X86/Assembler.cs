using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public abstract class Assembler: Indy.IL2CPU.Assembler.Assembler {
		public Assembler(Func<string, string> aGetFileNameForGroup, bool aInMetalMode)
			: base(aGetFileNameForGroup, aInMetalMode) {
		}

		public Assembler(Func<string, string> aGetFileNameForGroup)
			: base(aGetFileNameForGroup) {
		}

		private static string GetValidGroupName(string aGroup) {
			return aGroup.Replace('-','_').Replace('.', '_');
		}

		protected override void EmitHeader(string aGroup, StreamWriter aOutputWriter) {
			//mOutputWriter.WriteLine("format ms coff  ");
			//mOutputWriter.WriteLine("org 0220000h    ; the best place to load our kernel to. ");
			aOutputWriter.WriteLine("use32           ; the kernel will be run in 32-bit protected mode, ");
			aOutputWriter.WriteLine("");
			aOutputWriter.WriteLine("%ifdef {0}", GetValidGroupName(aGroup));
			aOutputWriter.WriteLine("%else");
			aOutputWriter.WriteLine("  %define {0} 1", GetValidGroupName(aGroup));
			aOutputWriter.WriteLine("");
		}

		protected override void EmitIncludes(string aGroup, StreamWriter aOutputWriter) {
			foreach (string xInclude in (from item in Includes
										 where String.Equals(item.Key, aGroup, StringComparison.InvariantCultureIgnoreCase)
										 select item.Value).Distinct(StringComparer.InvariantCultureIgnoreCase)) {
				aOutputWriter.WriteLine("%include '{0}'", xInclude);
			}
		}

		protected override void EmitFooter(string aGroup, StreamWriter aOutputWriter) {
			aOutputWriter.WriteLine("%endif");
		}
	}
}
