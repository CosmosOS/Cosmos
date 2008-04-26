using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Indy.IL2CPU.Assembler {
	public class DumbAssembler: Assembler {
		public DumbAssembler() : base(null, false) {
		}
		public string GetContents() {
			using (StringWriter sw = new StringWriter()) {
				var xLabels = new List<string>();
				foreach (var xGroup in (from item in Instructions
										select item.Key).Distinct()) {
					EmitCodeSection(xGroup, sw, xLabels);
				}
				return sw.ToString();
			}
		}

		protected override void EmitHeader(string aGroup, TextWriter aOutputWriter) {
		}

		protected override void EmitImportMembers(string aGroup, TextWriter aOutputWriter) {
		}
	}
}
