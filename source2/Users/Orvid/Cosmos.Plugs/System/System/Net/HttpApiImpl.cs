namespace Cosmos.Plugs
{
    //[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.HttpApi), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
    //public static class System_Net_UnsafeNclNativeMethods_HttpApiImpl
    //{

    //    public static System.UInt32 HttpInitialize(System.Net.UnsafeNclNativeMethods+HttpApi+HTTPAPI_VERSION version, System.UInt32 flags, System.Void* pReserved)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpInitialize' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpReceiveRequestEntityBody(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.UInt64 requestId, System.UInt32 flags, System.Void* pEntityBuffer, System.UInt32 entityBufferLength, System.UInt32* pBytesReturned, System.Threading.NativeOverlapped* pOverlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpReceiveRequestEntityBody' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpReceiveHttpRequest(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.UInt64 requestId, System.UInt32 flags, System.Net.UnsafeNclNativeMethods+HttpApi+HTTP_REQUEST* pRequestBuffer, System.UInt32 requestBufferLength, System.UInt32* pBytesReturned, System.Threading.NativeOverlapped* pOverlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpReceiveHttpRequest' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpSendHttpResponse(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.UInt64 requestId, System.UInt32 flags, System.Net.UnsafeNclNativeMethods+HttpApi+HTTP_RESPONSE* pHttpResponse, System.Void* pCachePolicy, System.UInt32* pBytesSent, System.Net.SafeLocalFree pRequestBuffer, System.UInt32 requestBufferLength, System.Threading.NativeOverlapped* pOverlapped, System.Void* pLogData)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpSendHttpResponse' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpCreateServerSession(System.Net.UnsafeNclNativeMethods+HttpApi+HTTPAPI_VERSION version, System.UInt64* serverSessionId, System.UInt32 reserved)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpCreateServerSession' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpCreateUrlGroup(System.UInt64 serverSessionId, System.UInt64* urlGroupId, System.UInt32 reserved)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpCreateUrlGroup' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpAddUrlToUrlGroup(System.UInt64 urlGroupId, System.String pFullyQualifiedUrl, System.UInt64 context, System.UInt32 pReserved)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpAddUrlToUrlGroup' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpSetUrlGroupProperty(System.UInt64 urlGroupId, System.Net.UnsafeNclNativeMethods+HttpApi+HTTP_SERVER_PROPERTY serverProperty, System.IntPtr pPropertyInfo, System.UInt32 propertyInfoLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpSetUrlGroupProperty' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpRemoveUrlFromUrlGroup(System.UInt64 urlGroupId, System.String pFullyQualifiedUrl, System.UInt32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpRemoveUrlFromUrlGroup' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpCloseServerSession(System.UInt64 serverSessionId)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpCloseServerSession' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpCloseUrlGroup(System.UInt64 urlGroupId)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpCloseUrlGroup' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpReceiveClientCertificate(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.UInt64 connectionId, System.UInt32 flags, System.Net.UnsafeNclNativeMethods+HttpApi+HTTP_SSL_CLIENT_CERT_INFO* pSslClientCertInfo, System.UInt32 sslClientCertInfoSize, System.UInt32* pBytesReceived, System.Threading.NativeOverlapped* pOverlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpReceiveClientCertificate' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpReceiveClientCertificate(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.UInt64 connectionId, System.UInt32 flags, System.Byte* pSslClientCertInfo, System.UInt32 sslClientCertInfoSize, System.UInt32* pBytesReceived, System.Threading.NativeOverlapped* pOverlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpReceiveClientCertificate' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpAddUrl(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.String pFullyQualifiedUrl, System.Void* pReserved)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpAddUrl' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpRemoveUrl(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.String pFullyQualifiedUrl)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpRemoveUrl' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpSendResponseEntityBody(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.UInt64 requestId, System.UInt32 flags, System.UInt16 entityChunkCount, System.Net.UnsafeNclNativeMethods+HttpApi+HTTP_DATA_CHUNK* pEntityChunks, System.UInt32* pBytesSent, System.Net.SafeLocalFree pRequestBuffer, System.UInt32 requestBufferLength, System.Threading.NativeOverlapped* pOverlapped, System.Void* pLogData)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpSendResponseEntityBody' has not been implemented!");
    //    }

    //    public static System.UInt32 HttpWaitForDisconnect(System.Runtime.InteropServices.CriticalHandle requestQueueHandle, System.UInt64 connectionId, System.Threading.NativeOverlapped* pOverlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+HttpApi.HttpWaitForDisconnect' has not been implemented!");
    //    }
    //}
}
