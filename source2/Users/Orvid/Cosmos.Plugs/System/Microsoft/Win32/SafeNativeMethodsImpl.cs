namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeNativeMethodsImpl
	{

		public static System.Boolean CloseHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Boolean ReleaseSemaphore(Microsoft.Win32.SafeHandles.SafeWaitHandle handle, System.Int32 releaseCount, System.Int32* previousCount)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.ReleaseSemaphore' has not been implemented!");
		}

		public static System.Void OutputDebugString(System.String message)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.OutputDebugString' has not been implemented!");
		}

		public static System.Int32 FormatMessage(System.Int32 dwFlags, System.Runtime.InteropServices.HandleRef lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr arguments)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.FormatMessage' has not been implemented!");
		}

		public static System.Boolean CloseHandle(System.Runtime.InteropServices.HandleRef handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Boolean QueryPerformanceCounter(System.Int64* value)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.QueryPerformanceCounter' has not been implemented!");
		}

		public static System.Boolean QueryPerformanceFrequency(System.Int64* value)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.QueryPerformanceFrequency' has not been implemented!");
		}

		public static System.Boolean GetComputerName(System.Text.StringBuilder lpBuffer, System.Int32[] nSize)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.GetComputerName' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle CreateSemaphore(Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES lpSecurityAttributes, System.Int32 initialCount, System.Int32 maximumCount, System.String name)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.CreateSemaphore' has not been implemented!");
		}

		public static System.Boolean GetTextMetrics(System.IntPtr hDC, Microsoft.Win32.NativeMethods+TEXTMETRIC tm)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.GetTextMetrics' has not been implemented!");
		}

		public static System.IntPtr GetStockObject(System.Int32 nIndex)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.GetStockObject' has not been implemented!");
		}

		public static System.Int32 MessageBox(System.IntPtr hWnd, System.String text, System.String caption, System.Int32 type)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.MessageBox' has not been implemented!");
		}

		public static System.Int32 FormatMessage(System.Int32 dwFlags, System.Runtime.InteropServices.SafeHandle lpSource, System.UInt32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr[] arguments)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.FormatMessage' has not been implemented!");
		}

		public static System.Int32 RegisterWindowMessage(System.String msg)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.RegisterWindowMessage' has not been implemented!");
		}

		public static System.IntPtr LoadLibrary(System.String libFilename)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.LoadLibrary' has not been implemented!");
		}

		public static System.Boolean FreeLibrary(System.Runtime.InteropServices.HandleRef hModule)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.FreeLibrary' has not been implemented!");
		}

		public static System.Int32 FormatFromRawValue(System.UInt32 dwCounterType, System.UInt32 dwFormat, System.Int64* pTimeBase, Microsoft.Win32.NativeMethods+PDH_RAW_COUNTER pRawValue1, Microsoft.Win32.NativeMethods+PDH_RAW_COUNTER pRawValue2, Microsoft.Win32.NativeMethods+PDH_FMT_COUNTERVALUE pFmtValue)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.FormatFromRawValue' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle OpenSemaphore(System.Int32 desiredAccess, System.Boolean inheritHandle, System.String name)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.OpenSemaphore' has not been implemented!");
		}

		public static System.Boolean IsWow64Process(Microsoft.Win32.SafeHandles.SafeProcessHandle hProcess, System.Boolean* Wow64Process)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeNativeMethods.IsWow64Process' has not been implemented!");
		}
	}
}
