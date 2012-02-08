using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System.Runtime.CompilerServices {
	[Plug(TargetName = "System.Runtime.CompilerServices.RuntimeHelpers, mscorlib", IsMicrosoftdotNETOnly=true)]
	public class RuntimeHelpers {

		//pluged in IL2CPU\Cosmos.IL2CPU.X86\Plugs\System.Runtime.CompilerServices\RuntimeHelpersImpl.cs
		//public static void InitializeArray(int[] aArray, int[] aFieldHandle) {
		//    for (int i = 0; i < aArray.Length; i++) {
		//        aArray[i] = aFieldHandle[i];
		//    }
		//}

		public void ProbeForSufficientStack() {
			// no implementation yet, before threading not needed
		}
	}
}