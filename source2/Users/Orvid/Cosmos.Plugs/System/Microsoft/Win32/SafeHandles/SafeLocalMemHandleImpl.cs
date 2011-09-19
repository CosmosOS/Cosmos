namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeLocalMemHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeLocalMemHandleImpl
	{

		public static System.IntPtr LocalFree(System.IntPtr hMem)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeLocalMemHandle.LocalFree' has not been implemented!");
		}

		public static System.Boolean ConvertStringSecurityDescriptorToSecurityDescriptor(System.String StringSecurityDescriptor, System.Int32 StringSDRevision, Microsoft.Win32.SafeHandles.SafeLocalMemHandle* pSecurityDescriptor, System.IntPtr SecurityDescriptorSize)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeLocalMemHandle.ConvertStringSecurityDescriptorToSecurityDescriptor' has not been implemented!");
		}
	}
}
