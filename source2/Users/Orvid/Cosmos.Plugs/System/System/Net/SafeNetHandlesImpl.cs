namespace Cosmos.Plugs
{
    //[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.SafeNetHandles), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
    //public static class System_Net_UnsafeNclNativeMethods+SafeNetHandlesImpl
    //{

    //    public static System.IntPtr LocalFree(System.IntPtr handle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.LocalFree' has not been implemented!");
    //    }

    //    public static System.Boolean FreeLibrary(System.IntPtr hModule)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.FreeLibrary' has not been implemented!");
    //    }

    //    public static System.Void CertFreeCertificateChain(System.IntPtr pChainContext)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.CertFreeCertificateChain' has not been implemented!");
    //    }

    //    public static System.Boolean CertFreeCertificateContext(System.IntPtr certContext)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.CertFreeCertificateContext' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError closesocket(System.IntPtr socketHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.closesocket' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError ioctlsocket(System.IntPtr handle, System.Int32 cmd, System.Int32* argp)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.ioctlsocket' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAEventSelect(System.IntPtr handle, System.IntPtr Event, System.Net.Sockets.AsyncEventBits NetworkEvents)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.WSAEventSelect' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError setsockopt(System.IntPtr handle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Net.Linger* linger, System.Int32 optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.setsockopt' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpCreateRequestQueue(System.Net.UnsafeNclNativeMethods+HttpApi+HTTPAPI_VERSION version, System.String pName, Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES pSecurityAttributes, System.UInt32 flags, System.Net.HttpRequestQueueV2Handle* pReqQueueHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.HttpCreateRequestQueue' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpCloseRequestQueue(System.IntPtr pReqQueueHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.HttpCloseRequestQueue' has not been implemented!");
    //    }

    //    public static System.Net.SafeLocalFree LocalAlloc(System.Int32 uFlags, System.UIntPtr sizetdwBytes)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.LocalAlloc' has not been implemented!");
    //    }

    //    public static System.Net.SafeLoadLibrary LoadLibraryExW(System.String lpwLibFileName, System.Void* hFile, System.UInt32 dwFlags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.LoadLibraryExW' has not been implemented!");
    //    }

    //    public static System.Net.SafeCloseSocket+InnerSafeCloseSocket accept(System.IntPtr socketHandle, System.Byte[] socketAddress, System.Int32* socketAddressSize)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.accept' has not been implemented!");
    //    }

    //    public static System.Boolean CloseHandle(System.IntPtr handle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.CloseHandle' has not been implemented!");
    //    }

    //    public static System.IntPtr GlobalFree(System.IntPtr handle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.GlobalFree' has not been implemented!");
    //    }

    //    public static System.Boolean RetrieveUrlCacheEntryFileW(System.Char* urlName, System.Byte* entryPtr, System.Int32* entryBufSize, System.Int32 dwReserved)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.RetrieveUrlCacheEntryFileW' has not been implemented!");
    //    }

    //    public static System.Boolean UnlockUrlCacheEntryFileW(System.Char* urlName, System.Int32 dwReserved)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.UnlockUrlCacheEntryFileW' has not been implemented!");
    //    }

    //    public static System.Int32 QuerySecurityContextToken(System.Net.SSPIHandle* phContext, System.Net.SafeCloseHandle* handle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.QuerySecurityContextToken' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpCreateHttpHandle(System.Net.SafeCloseHandle* pReqQueueHandle, System.UInt32 options)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.HttpCreateHttpHandle' has not been implemented!");
    //    }

    //    public static System.Net.SafeLocalFreeChannelBinding LocalAllocChannelBinding(System.Int32 uFlags, System.UIntPtr sizetdwBytes)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.LocalAllocChannelBinding' has not been implemented!");
    //    }

    //    public static System.Net.SafeLoadLibrary LoadLibraryExA(System.String lpwLibFileName, System.Void* hFile, System.UInt32 dwFlags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles.LoadLibraryExA' has not been implemented!");
    //    }
    //}
}
