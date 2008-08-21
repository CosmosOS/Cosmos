using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Indy.IL2CPU.Assembler {
	public class RawAssembler : Assembler {
		public RawAssembler() : base(null, false) { }

		public string GetContents() {
			using (var sw = new StringWriter()) {
				var xLabels = new List<string>();
			    this.Flush();
                foreach (var xGroup in (from item in mMergedInstructions select item.Key).Distinct())
                {
                    EmitCodeSection(xGroup, sw, mMergedInstructions);
				}
				return sw.ToString();
			}
		}

		protected override void EmitHeader(string aGroup, TextWriter aOutputWriter) { }

	}
}
