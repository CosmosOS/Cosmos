namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceModel.ComIntegration.SafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceModel_ComIntegration_SafeNativeMethodsImpl
	{

		public static System.Int32 RegOpenKeyEx(System.ServiceModel.ComIntegration.RegistryHandle hKey, System.String lpSubKey, System.Int32 ulOptions, System.Int32 samDesired, System.ServiceModel.ComIntegration.RegistryHandle* hkResult)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.RegOpenKeyEx' has not been implemented!");
		}

		public static System.Int32 RegSetValueEx(System.ServiceModel.ComIntegration.RegistryHandle hKey, System.String lpValueName, System.Int32 Reserved, System.Int32 dwType, System.String val, System.Int32 cbData)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.RegSetValueEx' has not been implemented!");
		}

		public static System.Int32 RegCloseKey(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.RegCloseKey' has not been implemented!");
		}

		public static System.Int32 RegQueryValueEx(System.ServiceModel.ComIntegration.RegistryHandle hKey, System.String lpValueName, System.Int32[] lpReserved, System.Int32* lpType, System.Byte[] lpData, System.Int32* lpcbData)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.RegQueryValueEx' has not been implemented!");
		}

		public static System.Int32 RegEnumKey(System.ServiceModel.ComIntegration.RegistryHandle hKey, System.Int32 index, System.Text.StringBuilder lpName, System.Int32* len)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.RegEnumKey' has not been implemented!");
		}

		public static System.Int32 RegDeleteKey(System.ServiceModel.ComIntegration.RegistryHandle hKey, System.String lpValueName)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.RegDeleteKey' has not been implemented!");
		}

		public static System.Boolean DuplicateTokenEx(System.IdentityModel.SafeCloseHandle ExistingToken, System.Security.Principal.TokenAccessLevels DesiredAccess, System.IntPtr TokenAttributes, System.ServiceModel.ComIntegration.SecurityImpersonationLevel ImpersonationLevel, System.ServiceModel.ComIntegration.TokenType TokenType, System.IdentityModel.SafeCloseHandle* NewToken)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.DuplicateTokenEx' has not been implemented!");
		}

		public static System.Boolean AccessCheck(System.Byte[] SecurityDescriptor, System.IdentityModel.SafeCloseHandle ClientToken, System.Int32 DesiredAccess, System.ServiceModel.ComIntegration.GENERIC_MAPPING GenericMapping, System.ServiceModel.ComIntegration.PRIVILEGE_SET* PrivilegeSet, System.UInt32* PrivilegeSetLength, System.UInt32* GrantedAccess, System.Boolean* AccessStatus)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.AccessCheck' has not been implemented!");
		}

		public static System.Boolean ImpersonateAnonymousUserOnCurrentThread(System.IntPtr CurrentThread)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.ImpersonateAnonymousUserOnCurrentThread' has not been implemented!");
		}

		public static System.Boolean OpenCurrentThreadToken(System.IntPtr ThreadHandle, System.Security.Principal.TokenAccessLevels DesiredAccess, System.Boolean OpenAsSelf, System.IdentityModel.SafeCloseHandle* TokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.OpenCurrentThreadToken' has not been implemented!");
		}

		public static System.Boolean SetCurrentThreadToken(System.IntPtr ThreadHandle, System.IdentityModel.SafeCloseHandle TokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.SetCurrentThreadToken' has not been implemented!");
		}

		public static System.IntPtr GetCurrentThread()
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GetCurrentThread' has not been implemented!");
		}

		public static System.Int32 GetCurrentThreadId()
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GetCurrentThreadId' has not been implemented!");
		}

		public static System.Boolean RevertToSelf()
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.RevertToSelf' has not been implemented!");
		}

		public static System.Boolean GetTokenInformation(System.IdentityModel.SafeCloseHandle TokenHandle, System.ServiceModel.ComIntegration.TOKEN_INFORMATION_CLASS TokenInformationClass, System.Runtime.InteropServices.SafeHandle TokenInformation, System.UInt32 TokenInformationLength, System.UInt32* ReturnLength)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GetTokenInformation' has not been implemented!");
		}

		public static System.IntPtr GetCurrentProcess()
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GetCurrentProcess' has not been implemented!");
		}

		public static System.Boolean GetCurrentProcessToken(System.IntPtr ProcessHandle, System.Security.Principal.TokenAccessLevels DesiredAccess, System.IdentityModel.SafeCloseHandle* TokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GetCurrentProcessToken' has not been implemented!");
		}

		public static System.Object CoCreateInstance(System.Guid rclsid, System.Object pUnkOuter, System.ServiceModel.ComIntegration.CLSCTX dwClsContext, System.Guid riid)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.CoCreateInstance' has not been implemented!");
		}

		public static System.Runtime.InteropServices.ComTypes.IStream CreateStreamOnHGlobal(System.IdentityModel.SafeHGlobalHandle hGlobal, System.Boolean fDeleteOnRelease)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.CreateStreamOnHGlobal' has not been implemented!");
		}

		public static System.IdentityModel.SafeHGlobalHandle GetHGlobalFromStream(System.Runtime.InteropServices.ComTypes.IStream stream)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GetHGlobalFromStream' has not been implemented!");
		}

		public static System.Object CoGetObjectContext(System.Guid riid)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.CoGetObjectContext' has not been implemented!");
		}

		public static System.Object CoCreateActivity(System.Object pIUnknown, System.Guid riid)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.CoCreateActivity' has not been implemented!");
		}

		public static System.IntPtr CoSwitchCallContext(System.IntPtr newSecurityObject)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.CoSwitchCallContext' has not been implemented!");
		}

		public static System.IntPtr GlobalLock(System.IdentityModel.SafeHGlobalHandle hGlobal)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GlobalLock' has not been implemented!");
		}

		public static System.Boolean GlobalUnlock(System.IdentityModel.SafeHGlobalHandle hGlobal)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GlobalUnlock' has not been implemented!");
		}

		public static System.IntPtr GlobalSize(System.IdentityModel.SafeHGlobalHandle hGlobal)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.GlobalSize' has not been implemented!");
		}

		public static System.Int32 LoadRegTypeLib(System.Guid* rguid, System.UInt16 major, System.UInt16 minor, System.Int32 lcid, System.Object* typeLib)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.LoadRegTypeLib' has not been implemented!");
		}

		public static System.Int32 SafeArrayGetDim(System.IntPtr pSafeArray)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.SafeArrayGetDim' has not been implemented!");
		}

		public static System.Int32 SafeArrayGetElemsize(System.IntPtr pSafeArray)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.SafeArrayGetElemsize' has not been implemented!");
		}

		public static System.Int32 SafeArrayGetLBound(System.IntPtr pSafeArray, System.Int32 cDims)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.SafeArrayGetLBound' has not been implemented!");
		}

		public static System.Int32 SafeArrayGetUBound(System.IntPtr pSafeArray, System.Int32 cDims)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.SafeArrayGetUBound' has not been implemented!");
		}

		public static System.IntPtr SafeArrayAccessData(System.IntPtr pSafeArray)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.SafeArrayAccessData' has not been implemented!");
		}

		public static System.Void SafeArrayUnaccessData(System.IntPtr pSafeArray)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.SafeArrayUnaccessData' has not been implemented!");
		}

		public static System.Boolean TranslateName(System.String input, System.ServiceModel.ComIntegration.EXTENDED_NAME_FORMAT inputFormat, System.ServiceModel.ComIntegration.EXTENDED_NAME_FORMAT outputFormat, System.Text.StringBuilder outputString, System.UInt32* size)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.ComIntegration.SafeNativeMethods.TranslateName' has not been implemented!");
		}
	}
}
