namespace Cosmos.Plugs
{
    //[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.OSSOCK), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
    //public static class System_Net_UnsafeNclNativeMethods_OSSOCKImpl
    //{

    //    public static System.Net.Sockets.SocketError ioctlsocket(System.Net.SafeCloseSocket socketHandle, System.Int32 cmd, System.Int32* argp)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.ioctlsocket' has not been implemented!");
    //    }

    //    public static System.Net.SafeCloseSocket+InnerSafeCloseSocket WSASocket(System.Net.Sockets.AddressFamily addressFamily, System.Net.Sockets.SocketType socketType, System.Net.Sockets.ProtocolType protocolType, System.IntPtr protocolInfo, System.UInt32 group, System.Net.SocketConstructorFlags flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSASocket' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAStartup(System.Int16 wVersionRequested, System.Net.WSAData* lpWSAData)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAStartup' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError gethostname(System.Text.StringBuilder hostName, System.Int32 bufferLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.gethostname' has not been implemented!");
    //    }

    //    public static System.Int32 inet_addr(System.String cp)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.inet_addr' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError setsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Int32* optionValue, System.Int32 optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.setsockopt' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError setsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.IntPtr* pointer, System.Int32 optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.setsockopt' has not been implemented!");
    //    }

    //    public static unsafe System.Int32 send(System.IntPtr socketHandle, System.Byte* pinnedBuffer, System.Int32 len, System.Net.Sockets.SocketFlags socketFlags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.send' has not been implemented!");
    //    }

    //    public static System.Int32 recv(System.IntPtr socketHandle, System.Byte* pinnedBuffer, System.Int32 len, System.Net.Sockets.SocketFlags socketFlags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.recv' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError listen(System.Net.SafeCloseSocket socketHandle, System.Int32 backlog)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.listen' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError bind(System.Net.SafeCloseSocket socketHandle, System.Byte[] socketAddress, System.Int32 socketAddressSize)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.bind' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError shutdown(System.Net.SafeCloseSocket socketHandle, System.Int32 how)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.shutdown' has not been implemented!");
    //    }

    //    public static System.Int32 select(System.Int32 ignoredParameter, System.IntPtr[] readfds, System.IntPtr[] writefds, System.IntPtr[] exceptfds, System.Net.Sockets.TimeValue* timeout)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.select' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAConnect(System.IntPtr socketHandle, System.Byte[] socketAddress, System.Int32 socketAddressSize, System.IntPtr inBuffer, System.IntPtr outBuffer, System.IntPtr sQOS, System.IntPtr gQOS)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAConnect' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSASend_Blocking(System.IntPtr socketHandle, System.Net.WSABuffer[] buffersArray, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags socketFlags, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSASend_Blocking' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSARecv(System.Net.SafeCloseSocket socketHandle, System.Net.WSABuffer* buffer, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags* socketFlags, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSARecv' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAEventSelect(System.Net.SafeCloseSocket socketHandle, System.Runtime.InteropServices.SafeHandle Event, System.Net.Sockets.AsyncEventBits NetworkEvents)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAEventSelect' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAEventSelect(System.Net.SafeCloseSocket socketHandle, System.IntPtr Event, System.Net.Sockets.AsyncEventBits NetworkEvents)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAEventSelect' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAIoctl(System.Net.SafeCloseSocket socketHandle, System.Int32 ioControlCode, System.Guid* guid, System.Int32 guidSize, System.IntPtr* funcPtr, System.Int32 funcPtrSize, System.Int32* bytesTransferred, System.IntPtr shouldBeNull, System.IntPtr shouldBeNull2)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAIoctl' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAIoctl_Blocking(System.IntPtr socketHandle, System.Int32 ioControlCode, System.Byte[] inBuffer, System.Int32 inBufferSize, System.Byte[] outBuffer, System.Int32 outBufferSize, System.Int32* bytesTransferred, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAIoctl_Blocking' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAAddressToString(System.Byte[] socketAddress, System.Int32 socketAddressSize, System.IntPtr lpProtocolInfo, System.Text.StringBuilder addressString, System.Int32* addressStringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAAddressToString' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError setsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Net.Linger* linger, System.Int32 optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.setsockopt' has not been implemented!");
    //    }

    //    public static System.Net.SafeCloseSocket+InnerSafeCloseSocket WSASocket(System.Net.Sockets.AddressFamily addressFamily, System.Net.Sockets.SocketType socketType, System.Net.Sockets.ProtocolType protocolType, System.Byte* pinnedBuffer, System.UInt32 group, System.Net.SocketConstructorFlags flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSASocket' has not been implemented!");
    //    }

    //    public static System.IntPtr gethostbyname(System.String host)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.gethostbyname' has not been implemented!");
    //    }

    //    public static System.IntPtr gethostbyaddr(System.Int32* addr, System.Int32 len, System.Net.Sockets.ProtocolFamily type)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.gethostbyaddr' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError getpeername(System.Net.SafeCloseSocket socketHandle, System.Byte[] socketAddress, System.Int32* socketAddressSize)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.getpeername' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError getsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Int32* optionValue, System.Int32* optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.getsockopt' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError getsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Byte[] optionValue, System.Int32* optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.getsockopt' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError getsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Net.Linger* optionValue, System.Int32* optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.getsockopt' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError getsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Net.IPMulticastRequest* optionValue, System.Int32* optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.getsockopt' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError getsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Net.IPv6MulticastRequest* optionValue, System.Int32* optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.getsockopt' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError setsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Byte[] optionValue, System.Int32 optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.setsockopt' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError setsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Net.IPMulticastRequest* mreq, System.Int32 optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.setsockopt' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError setsockopt(System.Net.SafeCloseSocket socketHandle, System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, System.Net.IPv6MulticastRequest* mreq, System.Int32 optionLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.setsockopt' has not been implemented!");
    //    }

    //    public static System.Boolean TransmitFile(System.Net.SafeCloseSocket socket, System.Runtime.InteropServices.SafeHandle fileHandle, System.Int32 numberOfBytesToWrite, System.Int32 numberOfBytesPerSend, System.IntPtr overlapped, System.Net.TransmitFileBuffers buffers, System.Net.Sockets.TransmitFileOptions flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.TransmitFile' has not been implemented!");
    //    }

    //    public static System.Boolean TransmitFile(System.Net.SafeCloseSocket socket, System.Runtime.InteropServices.SafeHandle fileHandle, System.Int32 numberOfBytesToWrite, System.Int32 numberOfBytesPerSend, System.IntPtr overlapped, System.IntPtr buffers, System.Net.Sockets.TransmitFileOptions flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.TransmitFile' has not been implemented!");
    //    }

    //    public static System.Boolean TransmitFile(System.Net.SafeCloseSocket socket, System.IntPtr fileHandle, System.Int32 numberOfBytesToWrite, System.Int32 numberOfBytesPerSend, System.IntPtr overlapped, System.IntPtr buffers, System.Net.Sockets.TransmitFileOptions flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.TransmitFile' has not been implemented!");
    //    }

    //    public static System.Boolean TransmitFile2(System.Net.SafeCloseSocket socket, System.IntPtr fileHandle, System.Int32 numberOfBytesToWrite, System.Int32 numberOfBytesPerSend, System.IntPtr overlapped, System.Net.TransmitFileBuffers buffers, System.Net.Sockets.TransmitFileOptions flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.TransmitFile2' has not been implemented!");
    //    }

    //    public static System.Boolean TransmitFile_Blocking(System.IntPtr socket, System.Runtime.InteropServices.SafeHandle fileHandle, System.Int32 numberOfBytesToWrite, System.Int32 numberOfBytesPerSend, System.IntPtr overlapped, System.Net.TransmitFileBuffers buffers, System.Net.Sockets.TransmitFileOptions flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.TransmitFile_Blocking' has not been implemented!");
    //    }

    //    public static System.Boolean TransmitFile_Blocking2(System.IntPtr socket, System.IntPtr fileHandle, System.Int32 numberOfBytesToWrite, System.Int32 numberOfBytesPerSend, System.IntPtr overlapped, System.Net.TransmitFileBuffers buffers, System.Net.Sockets.TransmitFileOptions flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.TransmitFile_Blocking2' has not been implemented!");
    //    }

    //    public static System.Int32 sendto(System.IntPtr socketHandle, System.Byte* pinnedBuffer, System.Int32 len, System.Net.Sockets.SocketFlags socketFlags, System.Byte[] socketAddress, System.Int32 socketAddressSize)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.sendto' has not been implemented!");
    //    }

    //    public static System.Int32 recvfrom(System.IntPtr socketHandle, System.Byte* pinnedBuffer, System.Int32 len, System.Net.Sockets.SocketFlags socketFlags, System.Byte[] socketAddress, System.Int32* socketAddressSize)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.recvfrom' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError getsockname(System.Net.SafeCloseSocket socketHandle, System.Byte[] socketAddress, System.Int32* socketAddressSize)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.getsockname' has not been implemented!");
    //    }

    //    public static System.Int32 select(System.Int32 ignoredParameter, System.IntPtr[] readfds, System.IntPtr[] writefds, System.IntPtr[] exceptfds, System.IntPtr nullTimeout)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.select' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSASend(System.Net.SafeCloseSocket socketHandle, System.Net.WSABuffer* buffer, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags socketFlags, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSASend' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSASend(System.Net.SafeCloseSocket socketHandle, System.Net.WSABuffer[] buffersArray, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags socketFlags, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSASend' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSASend(System.Net.SafeCloseSocket socketHandle, System.IntPtr buffers, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags socketFlags, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSASend' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSASendTo(System.Net.SafeCloseSocket socketHandle, System.Net.WSABuffer* buffer, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags socketFlags, System.IntPtr socketAddress, System.Int32 socketAddressSize, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSASendTo' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSASendTo(System.Net.SafeCloseSocket socketHandle, System.Net.WSABuffer[] buffersArray, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags socketFlags, System.IntPtr socketAddress, System.Int32 socketAddressSize, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSASendTo' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSARecv(System.Net.SafeCloseSocket socketHandle, System.Net.WSABuffer[] buffers, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags* socketFlags, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSARecv' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSARecv(System.Net.SafeCloseSocket socketHandle, System.IntPtr buffers, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags* socketFlags, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSARecv' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSARecv_Blocking(System.IntPtr socketHandle, System.Net.WSABuffer[] buffers, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags* socketFlags, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSARecv_Blocking' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSARecvFrom(System.Net.SafeCloseSocket socketHandle, System.Net.WSABuffer* buffer, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags* socketFlags, System.IntPtr socketAddressPointer, System.IntPtr socketAddressSizePointer, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSARecvFrom' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSARecvFrom(System.Net.SafeCloseSocket socketHandle, System.Net.WSABuffer[] buffers, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Net.Sockets.SocketFlags* socketFlags, System.IntPtr socketAddressPointer, System.IntPtr socketAddressSizePointer, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSARecvFrom' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAIoctl_Blocking_Internal(System.IntPtr socketHandle, System.UInt32 ioControlCode, System.IntPtr inBuffer, System.Int32 inBufferSize, System.IntPtr outBuffer, System.Int32 outBufferSize, System.Int32* bytesTransferred, System.IntPtr overlapped, System.IntPtr completionRoutine)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAIoctl_Blocking_Internal' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAEnumNetworkEvents(System.Net.SafeCloseSocket socketHandle, Microsoft.Win32.SafeHandles.SafeWaitHandle Event, System.Net.Sockets.NetworkEvents* networkEvents)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAEnumNetworkEvents' has not been implemented!");
    //    }

    //    public static System.Int32 WSADuplicateSocket(System.Net.SafeCloseSocket socketHandle, System.UInt32 targetProcessID, System.Byte* pinnedBuffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSADuplicateSocket' has not been implemented!");
    //    }

    //    public static System.Boolean WSAGetOverlappedResult(System.Net.SafeCloseSocket socketHandle, System.IntPtr overlapped, System.UInt32* bytesTransferred, System.Boolean wait, System.Net.Sockets.SocketFlags* socketFlags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAGetOverlappedResult' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError WSAStringToAddress(System.String addressString, System.Net.Sockets.AddressFamily addressFamily, System.IntPtr lpProtocolInfo, System.Byte[] socketAddress, System.Int32* socketAddressSize)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAStringToAddress' has not been implemented!");
    //    }

    //    public static System.Net.Sockets.SocketError getnameinfo(System.Byte[] sa, System.Int32 salen, System.Text.StringBuilder host, System.Int32 hostlen, System.Text.StringBuilder serv, System.Int32 servlen, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.getnameinfo' has not been implemented!");
    //    }

    //    public static System.Int32 WSAEnumProtocols(System.Int32[] lpiProtocols, System.Net.SafeLocalFree lpProtocolBuffer, System.UInt32* lpdwBufferLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+OSSOCK.WSAEnumProtocols' has not been implemented!");
    //    }
    //}
}
