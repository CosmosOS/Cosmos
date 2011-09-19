namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Data.Common.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Data_Common_NativeMethodsImpl
	{

		public static System.IntPtr MapViewOfFile(System.IntPtr hFileMappingObject, System.Int32 dwDesiredAccess, System.Int32 dwFileOffsetHigh, System.Int32 dwFileOffsetLow, System.IntPtr dwNumberOfBytesToMap)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.MapViewOfFile' has not been implemented!");
		}

		public static System.IntPtr OpenFileMappingA(System.Int32 dwDesiredAccess, System.Boolean bInheritHandle, System.String lpName)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.OpenFileMappingA' has not been implemented!");
		}

		public static System.IntPtr CreateFileMappingA(System.IntPtr hFile, System.IntPtr pAttr, System.Int32 flProtect, System.Int32 dwMaximumSizeHigh, System.Int32 dwMaximumSizeLow, System.String lpName)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.CreateFileMappingA' has not been implemented!");
		}

		public static System.Boolean UnmapViewOfFile(System.IntPtr lpBaseAddress)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.UnmapViewOfFile' has not been implemented!");
		}

		public static System.Boolean CloseHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Boolean AllocateAndInitializeSid(System.IntPtr pIdentifierAuthority, System.Byte nSubAuthorityCount, System.Int32 dwSubAuthority0, System.Int32 dwSubAuthority1, System.Int32 dwSubAuthority2, System.Int32 dwSubAuthority3, System.Int32 dwSubAuthority4, System.Int32 dwSubAuthority5, System.Int32 dwSubAuthority6, System.Int32 dwSubAuthority7, System.IntPtr* pSid)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.AllocateAndInitializeSid' has not been implemented!");
		}

		public static System.Int32 GetLengthSid(System.IntPtr pSid)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.GetLengthSid' has not been implemented!");
		}

		public static System.Boolean InitializeAcl(System.IntPtr pAcl, System.Int32 nAclLength, System.Int32 dwAclRevision)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.InitializeAcl' has not been implemented!");
		}

		public static System.Boolean AddAccessDeniedAce(System.IntPtr pAcl, System.Int32 dwAceRevision, System.Int32 AccessMask, System.IntPtr pSid)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.AddAccessDeniedAce' has not been implemented!");
		}

		public static System.Boolean AddAccessAllowedAce(System.IntPtr pAcl, System.Int32 dwAceRevision, System.UInt32 AccessMask, System.IntPtr pSid)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.AddAccessAllowedAce' has not been implemented!");
		}

		public static System.Boolean InitializeSecurityDescriptor(System.IntPtr pSecurityDescriptor, System.Int32 dwRevision)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.InitializeSecurityDescriptor' has not been implemented!");
		}

		public static System.Boolean SetSecurityDescriptorDacl(System.IntPtr pSecurityDescriptor, System.Boolean bDaclPresent, System.IntPtr pDacl, System.Boolean bDaclDefaulted)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.SetSecurityDescriptorDacl' has not been implemented!");
		}

		public static System.IntPtr FreeSid(System.IntPtr pSid)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.NativeMethods.FreeSid' has not been implemented!");
		}
	}
}
