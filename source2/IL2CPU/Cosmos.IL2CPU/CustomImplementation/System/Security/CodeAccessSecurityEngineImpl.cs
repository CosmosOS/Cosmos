using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.CompilerServices {
	[Plug(TargetName = "System.Security.CodeAccessSecurityEngine, mscorlib", IsMicrosoftdotNETOnly = true)]
	public class CodeAccessSecurityEngine {

		//TODO check if ref is linked right
		public void Check(object demand, [FieldTypeAttribute(Name = "System.Threading.StackCrawlMark, mscorlib")]ref AttributeTargets stackMark, bool isPermSet) {
			// no implementation yet, before threading not needed
		}
	}
}