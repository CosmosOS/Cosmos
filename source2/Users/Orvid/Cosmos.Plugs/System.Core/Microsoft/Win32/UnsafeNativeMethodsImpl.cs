namespace Cosmos.Plugs
{
    //[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
    //public static class Microsoft_Win32_UnsafeNativeMethodsImpl
    //{

    //    public static System.Boolean FreeLibrary(System.IntPtr hModule)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.FreeLibrary' has not been implemented!");
    //    }

    //    public static System.Boolean RevertToSelf()
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.RevertToSelf' has not been implemented!");
    //    }

    //    public static System.Boolean ImpersonateNamedPipeClient(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.ImpersonateNamedPipeClient' has not been implemented!");
    //    }

    //    public static System.Boolean EvtClose(System.IntPtr handle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtClose' has not been implemented!");
    //    }

    //    public static System.Int32 GetFileType(Microsoft.Win32.SafeHandles.SafeFileHandle handle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetFileType' has not been implemented!");
    //    }

    //    public static System.Int32 WriteFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Byte* bytes, System.Int32 numBytesToWrite, System.Int32* numBytesWritten, System.Threading.NativeOverlapped* lpOverlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.WriteFile' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(System.String lpFileName, System.Int32 dwDesiredAccess, System.IO.FileShare dwShareMode, Microsoft.Win32.UnsafeNativeMethods+SECURITY_ATTRIBUTES securityAttrs, System.IO.FileMode dwCreationDisposition, System.Int32 dwFlagsAndAttributes, System.IntPtr hTemplateFile)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CreateFile' has not been implemented!");
    //    }

    //    public static System.Int32 SetErrorMode(System.Int32 newMode)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.SetErrorMode' has not been implemented!");
    //    }

    //    public static System.Int32 SetFilePointerWin32(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Int32 lo, System.Int32* hi, System.Int32 origin)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.SetFilePointerWin32' has not been implemented!");
    //    }

    //    public static System.Int32 FormatMessage(System.Int32 dwFlags, System.IntPtr lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr va_list_arguments)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.FormatMessage' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeLibraryHandle LoadLibraryEx(System.String libFilename, System.IntPtr reserved, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.LoadLibraryEx' has not been implemented!");
    //    }

    //    public static System.Boolean CloseHandle(System.IntPtr handle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CloseHandle' has not been implemented!");
    //    }

    //    public static System.IntPtr GetCurrentProcess()
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetCurrentProcess' has not been implemented!");
    //    }

    //    public static System.Boolean DuplicateHandle(System.IntPtr hSourceProcessHandle, Microsoft.Win32.SafeHandles.SafePipeHandle hSourceHandle, System.IntPtr hTargetProcessHandle, Microsoft.Win32.SafeHandles.SafePipeHandle* lpTargetHandle, System.UInt32 dwDesiredAccess, System.Boolean bInheritHandle, System.UInt32 dwOptions)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.DuplicateHandle' has not been implemented!");
    //    }

    //    public static System.Int32 GetFileType(Microsoft.Win32.SafeHandles.SafePipeHandle handle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetFileType' has not been implemented!");
    //    }

    //    public static System.Boolean CreatePipe(Microsoft.Win32.SafeHandles.SafePipeHandle* hReadPipe, Microsoft.Win32.SafeHandles.SafePipeHandle* hWritePipe, Microsoft.Win32.UnsafeNativeMethods+SECURITY_ATTRIBUTES lpPipeAttributes, System.Int32 nSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CreatePipe' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafePipeHandle CreateNamedPipeClient(System.String lpFileName, System.Int32 dwDesiredAccess, System.IO.FileShare dwShareMode, Microsoft.Win32.UnsafeNativeMethods+SECURITY_ATTRIBUTES securityAttrs, System.IO.FileMode dwCreationDisposition, System.Int32 dwFlagsAndAttributes, System.IntPtr hTemplateFile)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CreateNamedPipeClient' has not been implemented!");
    //    }

    //    public static System.Boolean ConnectNamedPipe(Microsoft.Win32.SafeHandles.SafePipeHandle handle, System.Threading.NativeOverlapped* overlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.ConnectNamedPipe' has not been implemented!");
    //    }

    //    public static System.Boolean ConnectNamedPipe(Microsoft.Win32.SafeHandles.SafePipeHandle handle, System.IntPtr overlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.ConnectNamedPipe' has not been implemented!");
    //    }

    //    public static System.Boolean WaitNamedPipe(System.String name, System.Int32 timeout)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.WaitNamedPipe' has not been implemented!");
    //    }

    //    public static System.Boolean GetNamedPipeHandleState(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe, System.Int32* lpState, System.IntPtr lpCurInstances, System.IntPtr lpMaxCollectionCount, System.IntPtr lpCollectDataTimeout, System.IntPtr lpUserName, System.Int32 nMaxUserNameSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeHandleState' has not been implemented!");
    //    }

    //    public static System.Boolean GetNamedPipeHandleState(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe, System.IntPtr lpState, System.Int32* lpCurInstances, System.IntPtr lpMaxCollectionCount, System.IntPtr lpCollectDataTimeout, System.IntPtr lpUserName, System.Int32 nMaxUserNameSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeHandleState' has not been implemented!");
    //    }

    //    public static System.Boolean GetNamedPipeHandleState(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe, System.IntPtr lpState, System.IntPtr lpCurInstances, System.IntPtr lpMaxCollectionCount, System.IntPtr lpCollectDataTimeout, System.Text.StringBuilder lpUserName, System.Int32 nMaxUserNameSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeHandleState' has not been implemented!");
    //    }

    //    public static System.Boolean GetNamedPipeInfo(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe, System.Int32* lpFlags, System.IntPtr lpOutBufferSize, System.IntPtr lpInBufferSize, System.IntPtr lpMaxInstances)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeInfo' has not been implemented!");
    //    }

    //    public static System.Boolean GetNamedPipeInfo(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe, System.IntPtr lpFlags, System.Int32* lpOutBufferSize, System.IntPtr lpInBufferSize, System.IntPtr lpMaxInstances)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeInfo' has not been implemented!");
    //    }

    //    public static System.Boolean GetNamedPipeInfo(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe, System.IntPtr lpFlags, System.IntPtr lpOutBufferSize, System.Int32* lpInBufferSize, System.IntPtr lpMaxInstances)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeInfo' has not been implemented!");
    //    }

    //    public static System.Boolean SetNamedPipeHandleState(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe, System.Int32* lpMode, System.IntPtr lpMaxCollectionCount, System.IntPtr lpCollectDataTimeout)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.SetNamedPipeHandleState' has not been implemented!");
    //    }

    //    public static System.Boolean DisconnectNamedPipe(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.DisconnectNamedPipe' has not been implemented!");
    //    }

    //    public static System.Boolean FlushFileBuffers(Microsoft.Win32.SafeHandles.SafePipeHandle hNamedPipe)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.FlushFileBuffers' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafePipeHandle CreateNamedPipe(System.String pipeName, System.Int32 openMode, System.Int32 pipeMode, System.Int32 maxInstances, System.Int32 outBufferSize, System.Int32 inBufferSize, System.Int32 defaultTimeout, Microsoft.Win32.UnsafeNativeMethods+SECURITY_ATTRIBUTES securityAttributes)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CreateNamedPipe' has not been implemented!");
    //    }

    //    public static System.Int32 ReadFile(Microsoft.Win32.SafeHandles.SafePipeHandle handle, System.Byte* bytes, System.Int32 numBytesToRead, System.IntPtr numBytesRead_mustBeZero, System.Threading.NativeOverlapped* overlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.ReadFile' has not been implemented!");
    //    }

    //    public static System.Int32 ReadFile(Microsoft.Win32.SafeHandles.SafePipeHandle handle, System.Byte* bytes, System.Int32 numBytesToRead, System.Int32* numBytesRead, System.IntPtr mustBeZero)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.ReadFile' has not been implemented!");
    //    }

    //    public static System.Int32 WriteFile(Microsoft.Win32.SafeHandles.SafePipeHandle handle, System.Byte* bytes, System.Int32 numBytesToWrite, System.IntPtr numBytesWritten_mustBeZero, System.Threading.NativeOverlapped* lpOverlapped)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.WriteFile' has not been implemented!");
    //    }

    //    public static System.Int32 WriteFile(Microsoft.Win32.SafeHandles.SafePipeHandle handle, System.Byte* bytes, System.Int32 numBytesToWrite, System.Int32* numBytesWritten, System.IntPtr mustBeZero)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.WriteFile' has not been implemented!");
    //    }

    //    public static System.Boolean SetEndOfFile(System.IntPtr hNamedPipe)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.SetEndOfFile' has not been implemented!");
    //    }

    //    public static System.UInt32 EventRegister(System.Guid* providerId, Microsoft.Win32.UnsafeNativeMethods+EtwEnableCallback enableCallback, System.Void* callbackContext, System.Int64* registrationHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventRegister' has not been implemented!");
    //    }

    //    public static System.Int32 EventUnregister(System.Int64 registrationHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventUnregister' has not been implemented!");
    //    }

    //    public static System.Int32 EventEnabled(System.Int64 registrationHandle, System.Diagnostics.Eventing.EventDescriptor* eventDescriptor)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventEnabled' has not been implemented!");
    //    }

    //    public static System.Int32 EventProviderEnabled(System.Int64 registrationHandle, System.Byte level, System.Int64 keywords)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventProviderEnabled' has not been implemented!");
    //    }

    //    public static System.UInt32 EventWrite(System.Int64 registrationHandle, System.Diagnostics.Eventing.EventDescriptor* eventDescriptor, System.UInt32 userDataCount, System.Void* userData)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventWrite' has not been implemented!");
    //    }

    //    public static System.UInt32 EventWrite(System.Int64 registrationHandle, System.Diagnostics.Eventing.EventDescriptor* eventDescriptor, System.UInt32 userDataCount, System.Void* userData)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventWrite' has not been implemented!");
    //    }

    //    public static System.UInt32 EventWriteTransfer(System.Int64 registrationHandle, System.Diagnostics.Eventing.EventDescriptor* eventDescriptor, System.Guid* activityId, System.Guid* relatedActivityId, System.UInt32 userDataCount, System.Void* userData)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventWriteTransfer' has not been implemented!");
    //    }

    //    public static System.UInt32 EventWriteString(System.Int64 registrationHandle, System.Byte level, System.Int64 keywords, System.Char* message)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventWriteString' has not been implemented!");
    //    }

    //    public static System.UInt32 EventActivityIdControl(System.Int32 ControlCode, System.Guid* ActivityId)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EventActivityIdControl' has not been implemented!");
    //    }

    //    public static System.UInt32 PerfStartProvider(System.Guid* ProviderGuid, Microsoft.Win32.UnsafeNativeMethods+PERFLIBREQUEST ControlCallback, Microsoft.Win32.SafeHandles.SafePerfProviderHandle* phProvider)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.PerfStartProvider' has not been implemented!");
    //    }

    //    public static System.UInt32 PerfStopProvider(System.IntPtr hProvider)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.PerfStopProvider' has not been implemented!");
    //    }

    //    public static System.UInt32 PerfSetCounterSetInfo(Microsoft.Win32.SafeHandles.SafePerfProviderHandle hProvider, Microsoft.Win32.UnsafeNativeMethods+PerfCounterSetInfoStruct* pTemplate, System.UInt32 dwTemplateSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.PerfSetCounterSetInfo' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.UnsafeNativeMethods+PerfCounterSetInstanceStruct* PerfCreateInstance(Microsoft.Win32.SafeHandles.SafePerfProviderHandle hProvider, System.Guid* CounterSetGuid, System.String szInstanceName, System.UInt32 dwInstance)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.PerfCreateInstance' has not been implemented!");
    //    }

    //    public static System.UInt32 PerfDeleteInstance(Microsoft.Win32.SafeHandles.SafePerfProviderHandle hProvider, Microsoft.Win32.UnsafeNativeMethods+PerfCounterSetInstanceStruct* InstanceBlock)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.PerfDeleteInstance' has not been implemented!");
    //    }

    //    public static System.UInt32 PerfSetCounterRefValue(Microsoft.Win32.SafeHandles.SafePerfProviderHandle hProvider, Microsoft.Win32.UnsafeNativeMethods+PerfCounterSetInstanceStruct* pInstance, System.UInt32 CounterId, System.Void* lpAddr)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.PerfSetCounterRefValue' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtQuery(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.String path, System.String query, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtQuery' has not been implemented!");
    //    }

    //    public static System.Boolean EvtSeek(System.Diagnostics.Eventing.Reader.EventLogHandle resultSet, System.Int64 position, System.Diagnostics.Eventing.Reader.EventLogHandle bookmark, System.Int32 timeout, Microsoft.Win32.UnsafeNativeMethods+EvtSeekFlags flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtSeek' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtSubscribe(System.Diagnostics.Eventing.Reader.EventLogHandle session, Microsoft.Win32.SafeHandles.SafeWaitHandle signalEvent, System.String path, System.String query, System.Diagnostics.Eventing.Reader.EventLogHandle bookmark, System.IntPtr context, System.IntPtr callback, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtSubscribe' has not been implemented!");
    //    }

    //    public static System.Boolean EvtNext(System.Diagnostics.Eventing.Reader.EventLogHandle queryHandle, System.Int32 eventSize, System.IntPtr[] events, System.Int32 timeout, System.Int32 flags, System.Int32* returned)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtNext' has not been implemented!");
    //    }

    //    public static System.Boolean EvtCancel(System.Diagnostics.Eventing.Reader.EventLogHandle handle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtCancel' has not been implemented!");
    //    }

    //    public static System.Boolean EvtGetEventInfo(System.Diagnostics.Eventing.Reader.EventLogHandle eventHandle, Microsoft.Win32.UnsafeNativeMethods+EvtEventPropertyId propertyId, System.Int32 bufferSize, System.IntPtr bufferPtr, System.Int32* bufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtGetEventInfo' has not been implemented!");
    //    }

    //    public static System.Boolean EvtGetQueryInfo(System.Diagnostics.Eventing.Reader.EventLogHandle queryHandle, Microsoft.Win32.UnsafeNativeMethods+EvtQueryPropertyId propertyId, System.Int32 bufferSize, System.IntPtr buffer, System.Int32* bufferRequired)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtGetQueryInfo' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtOpenPublisherMetadata(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.String publisherId, System.String logFilePath, System.Int32 locale, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtOpenPublisherMetadata' has not been implemented!");
    //    }

    //    public static System.Boolean EvtGetPublisherMetadataProperty(System.Diagnostics.Eventing.Reader.EventLogHandle publisherMetadataHandle, Microsoft.Win32.UnsafeNativeMethods+EvtPublisherMetadataPropertyId propertyId, System.Int32 flags, System.Int32 publisherMetadataPropertyBufferSize, System.IntPtr publisherMetadataPropertyBuffer, System.Int32* publisherMetadataPropertyBufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtGetPublisherMetadataProperty' has not been implemented!");
    //    }

    //    public static System.Boolean EvtGetObjectArraySize(System.Diagnostics.Eventing.Reader.EventLogHandle objectArray, System.Int32* objectArraySize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtGetObjectArraySize' has not been implemented!");
    //    }

    //    public static System.Boolean EvtGetObjectArrayProperty(System.Diagnostics.Eventing.Reader.EventLogHandle objectArray, System.Int32 propertyId, System.Int32 arrayIndex, System.Int32 flags, System.Int32 propertyValueBufferSize, System.IntPtr propertyValueBuffer, System.Int32* propertyValueBufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtGetObjectArrayProperty' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtOpenEventMetadataEnum(System.Diagnostics.Eventing.Reader.EventLogHandle publisherMetadata, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtOpenEventMetadataEnum' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtNextEventMetadata(System.Diagnostics.Eventing.Reader.EventLogHandle eventMetadataEnum, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtNextEventMetadata' has not been implemented!");
    //    }

    //    public static System.Boolean EvtGetEventMetadataProperty(System.Diagnostics.Eventing.Reader.EventLogHandle eventMetadata, Microsoft.Win32.UnsafeNativeMethods+EvtEventMetadataPropertyId propertyId, System.Int32 flags, System.Int32 eventMetadataPropertyBufferSize, System.IntPtr eventMetadataPropertyBuffer, System.Int32* eventMetadataPropertyBufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtGetEventMetadataProperty' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtOpenChannelEnum(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtOpenChannelEnum' has not been implemented!");
    //    }

    //    public static System.Boolean EvtNextChannelPath(System.Diagnostics.Eventing.Reader.EventLogHandle channelEnum, System.Int32 channelPathBufferSize, System.Text.StringBuilder channelPathBuffer, System.Int32* channelPathBufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtNextChannelPath' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtOpenPublisherEnum(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtOpenPublisherEnum' has not been implemented!");
    //    }

    //    public static System.Boolean EvtNextPublisherId(System.Diagnostics.Eventing.Reader.EventLogHandle publisherEnum, System.Int32 publisherIdBufferSize, System.Text.StringBuilder publisherIdBuffer, System.Int32* publisherIdBufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtNextPublisherId' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtOpenChannelConfig(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.String channelPath, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtOpenChannelConfig' has not been implemented!");
    //    }

    //    public static System.Boolean EvtSaveChannelConfig(System.Diagnostics.Eventing.Reader.EventLogHandle channelConfig, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtSaveChannelConfig' has not been implemented!");
    //    }

    //    public static System.Boolean EvtSetChannelConfigProperty(System.Diagnostics.Eventing.Reader.EventLogHandle channelConfig, Microsoft.Win32.UnsafeNativeMethods+EvtChannelConfigPropertyId propertyId, System.Int32 flags, Microsoft.Win32.UnsafeNativeMethods+EvtVariant* propertyValue)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtSetChannelConfigProperty' has not been implemented!");
    //    }

    //    public static System.Boolean EvtGetChannelConfigProperty(System.Diagnostics.Eventing.Reader.EventLogHandle channelConfig, Microsoft.Win32.UnsafeNativeMethods+EvtChannelConfigPropertyId propertyId, System.Int32 flags, System.Int32 propertyValueBufferSize, System.IntPtr propertyValueBuffer, System.Int32* propertyValueBufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtGetChannelConfigProperty' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtOpenLog(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.String path, System.Diagnostics.Eventing.Reader.PathType flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtOpenLog' has not been implemented!");
    //    }

    //    public static System.Boolean EvtGetLogInfo(System.Diagnostics.Eventing.Reader.EventLogHandle log, Microsoft.Win32.UnsafeNativeMethods+EvtLogPropertyId propertyId, System.Int32 propertyValueBufferSize, System.IntPtr propertyValueBuffer, System.Int32* propertyValueBufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtGetLogInfo' has not been implemented!");
    //    }

    //    public static System.Boolean EvtExportLog(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.String channelPath, System.String query, System.String targetFilePath, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtExportLog' has not been implemented!");
    //    }

    //    public static System.Boolean EvtArchiveExportedLog(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.String logFilePath, System.Int32 locale, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtArchiveExportedLog' has not been implemented!");
    //    }

    //    public static System.Boolean EvtClearLog(System.Diagnostics.Eventing.Reader.EventLogHandle session, System.String channelPath, System.String targetFilePath, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtClearLog' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtCreateRenderContext(System.Int32 valuePathsCount, System.String[] valuePaths, Microsoft.Win32.UnsafeNativeMethods+EvtRenderContextFlags flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtCreateRenderContext' has not been implemented!");
    //    }

    //    public static System.Boolean EvtRender(System.Diagnostics.Eventing.Reader.EventLogHandle context, System.Diagnostics.Eventing.Reader.EventLogHandle eventHandle, Microsoft.Win32.UnsafeNativeMethods+EvtRenderFlags flags, System.Int32 buffSize, System.Text.StringBuilder buffer, System.Int32* buffUsed, System.Int32* propCount)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtRender' has not been implemented!");
    //    }

    //    public static System.Boolean EvtRender(System.Diagnostics.Eventing.Reader.EventLogHandle context, System.Diagnostics.Eventing.Reader.EventLogHandle eventHandle, Microsoft.Win32.UnsafeNativeMethods+EvtRenderFlags flags, System.Int32 buffSize, System.IntPtr buffer, System.Int32* buffUsed, System.Int32* propCount)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtRender' has not been implemented!");
    //    }

    //    public static System.Boolean EvtFormatMessage(System.Diagnostics.Eventing.Reader.EventLogHandle publisherMetadataHandle, System.Diagnostics.Eventing.Reader.EventLogHandle eventHandle, System.UInt32 messageId, System.Int32 valueCount, Microsoft.Win32.UnsafeNativeMethods+EvtStringVariant[] values, Microsoft.Win32.UnsafeNativeMethods+EvtFormatMessageFlags flags, System.Int32 bufferSize, System.Text.StringBuilder buffer, System.Int32* bufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessage' has not been implemented!");
    //    }

    //    public static System.Boolean EvtFormatMessageBuffer(System.Diagnostics.Eventing.Reader.EventLogHandle publisherMetadataHandle, System.Diagnostics.Eventing.Reader.EventLogHandle eventHandle, System.UInt32 messageId, System.Int32 valueCount, System.IntPtr values, Microsoft.Win32.UnsafeNativeMethods+EvtFormatMessageFlags flags, System.Int32 bufferSize, System.IntPtr buffer, System.Int32* bufferUsed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageBuffer' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtOpenSession(Microsoft.Win32.UnsafeNativeMethods+EvtLoginClass loginClass, Microsoft.Win32.UnsafeNativeMethods+EvtRpcLogin* login, System.Int32 timeout, System.Int32 flags)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtOpenSession' has not been implemented!");
    //    }

    //    public static System.Diagnostics.Eventing.Reader.EventLogHandle EvtCreateBookmark(System.String bookmarkXml)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtCreateBookmark' has not been implemented!");
    //    }

    //    public static System.Boolean EvtUpdateBookmark(System.Diagnostics.Eventing.Reader.EventLogHandle bookmark, System.Diagnostics.Eventing.Reader.EventLogHandle eventHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.EvtUpdateBookmark' has not been implemented!");
    //    }

    //    public static System.Void GetSystemInfo(Microsoft.Win32.UnsafeNativeMethods+SYSTEM_INFO* lpSystemInfo)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetSystemInfo' has not been implemented!");
    //    }

    //    public static System.Boolean UnmapViewOfFile(System.IntPtr lpBaseAddress)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.UnmapViewOfFile' has not been implemented!");
    //    }

    //    public static System.Int32 GetFileSize(Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle hFile, System.Int32* highSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetFileSize' has not been implemented!");
    //    }

    //    public static System.IntPtr VirtualQuery(Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle address, Microsoft.Win32.UnsafeNativeMethods+MEMORY_BASIC_INFORMATION* buffer, System.IntPtr sizeOfBuffer)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.VirtualQuery' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle CreateFileMapping(Microsoft.Win32.SafeHandles.SafeFileHandle hFile, Microsoft.Win32.UnsafeNativeMethods+SECURITY_ATTRIBUTES lpAttributes, System.Int32 fProtect, System.Int32 dwMaximumSizeHigh, System.Int32 dwMaximumSizeLow, System.String lpName)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CreateFileMapping' has not been implemented!");
    //    }

    //    public static System.Boolean FlushViewOfFile(System.Byte* lpBaseAddress, System.IntPtr dwNumberOfBytesToFlush)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.FlushViewOfFile' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle OpenFileMapping(System.Int32 dwDesiredAccess, System.Boolean bInheritHandle, System.String lpName)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.OpenFileMapping' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle MapViewOfFile(Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle handle, System.Int32 dwDesiredAccess, System.UInt32 dwFileOffsetHigh, System.UInt32 dwFileOffsetLow, System.UIntPtr dwNumberOfBytesToMap)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.MapViewOfFile' has not been implemented!");
    //    }

    //    public static System.IntPtr VirtualAlloc(Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle address, System.UIntPtr numBytes, System.Int32 commitOrReserve, System.Int32 pageProtectionMode)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.VirtualAlloc' has not been implemented!");
    //    }

    //    public static System.Boolean GlobalMemoryStatusEx(Microsoft.Win32.UnsafeNativeMethods+MEMORYSTATUSEX lpBuffer)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GlobalMemoryStatusEx' has not been implemented!");
    //    }
    //}
}
