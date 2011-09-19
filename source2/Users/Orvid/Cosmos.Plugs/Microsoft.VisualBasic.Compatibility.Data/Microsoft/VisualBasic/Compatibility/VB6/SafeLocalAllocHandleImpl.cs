namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.VisualBasic.Compatibility.VB6.SafeLocalAllocHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_VisualBasic_Compatibility_VB6_SafeLocalAllocHandleImpl
	{

		public static System.IntPtr LocalFree(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.VisualBasic.Compatibility.VB6.SafeLocalAllocHandle.LocalFree' has not been implemented!");
		}

		public static Microsoft.VisualBasic.Compatibility.VB6.SafeLocalAllocHandle LocalAlloc(System.Int32 uFlags, System.IntPtr sizetdwBytes)
		{
			throw new System.NotImplementedException("Method 'Microsoft.VisualBasic.Compatibility.VB6.SafeLocalAllocHandle.LocalAlloc' has not been implemented!");
		}
	}
}
