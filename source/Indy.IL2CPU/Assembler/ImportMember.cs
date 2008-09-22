using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	public class ImportMember {
		public readonly string Name;
		public readonly string FileName;
		public readonly List<ImportMethodMember> Methods = new List<ImportMethodMember>();
		
		public ImportMember(string aName, string aFileName) {
			Name = aName;
			FileName = aFileName;
		}
	}
}
