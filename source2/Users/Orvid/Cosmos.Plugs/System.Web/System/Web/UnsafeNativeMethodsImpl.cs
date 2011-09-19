namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Web.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Web_UnsafeNativeMethodsImpl
	{

		public static System.Int32 OpenThreadToken(System.IntPtr thread, System.Int32 access, System.Boolean openAsSelf, System.IntPtr* hToken)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.OpenThreadToken' has not been implemented!");
		}

		public static System.Int32 GetFileSecurity(System.String filename, System.Int32 requestedInformation, System.Byte[] securityDescriptor, System.Int32 length, System.Int32* lengthNeeded)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetFileSecurity' has not been implemented!");
		}

		public static System.Int32 lstrlenW(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.lstrlenW' has not been implemented!");
		}

		public static System.Int32 lstrlenA(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.lstrlenA' has not been implemented!");
		}

		public static System.Boolean FindClose(System.IntPtr hndFindFile)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.FindClose' has not been implemented!");
		}

		public static System.IntPtr FindFirstFile(System.String pFileName, System.Web.UnsafeNativeMethods+WIN32_FIND_DATA* pFindFileData)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.FindFirstFile' has not been implemented!");
		}

		public static System.Boolean FindNextFile(System.IntPtr hndFindFile, System.Web.UnsafeNativeMethods+WIN32_FIND_DATA* pFindFileData)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.FindNextFile' has not been implemented!");
		}

		public static System.Boolean GetFileAttributesEx(System.String name, System.Int32 fileInfoLevel, System.Web.UnsafeNativeMethods+WIN32_FILE_ATTRIBUTE_DATA* data)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetFileAttributesEx' has not been implemented!");
		}

		public static System.Int32 GetProcessAffinityMask(System.IntPtr handle, System.IntPtr* processAffinityMask, System.IntPtr* systemAffinityMask)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetProcessAffinityMask' has not been implemented!");
		}

		public static System.Int32 GetModuleFileName(System.IntPtr module, System.Text.StringBuilder filename, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetModuleFileName' has not been implemented!");
		}

		public static System.IntPtr GetModuleHandle(System.String moduleName)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetModuleHandle' has not been implemented!");
		}

		public static System.Void GetSystemInfo(System.Web.UnsafeNativeMethods+SYSTEM_INFO* si)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetSystemInfo' has not been implemented!");
		}

		public static System.IntPtr FindResource(System.IntPtr hModule, System.IntPtr lpName, System.IntPtr lpType)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.FindResource' has not been implemented!");
		}

		public static System.Int32 SizeofResource(System.IntPtr hModule, System.IntPtr hResInfo)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SizeofResource' has not been implemented!");
		}

		public static System.IntPtr LoadResource(System.IntPtr hModule, System.IntPtr hResInfo)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.LoadResource' has not been implemented!");
		}

		public static System.IntPtr LockResource(System.IntPtr hResData)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.LockResource' has not been implemented!");
		}

		public static System.Int32 GlobalMemoryStatusEx(System.Web.UnsafeNativeMethods+MEMORYSTATUSEX* memoryStatusEx)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GlobalMemoryStatusEx' has not been implemented!");
		}

		public static System.IntPtr GetCurrentThread()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetCurrentThread' has not been implemented!");
		}

		public static System.IntPtr CreateUserToken(System.String name, System.String password, System.Int32 fImpersonationToken, System.Text.StringBuilder strError, System.Int32 iErrorSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.CreateUserToken' has not been implemented!");
		}

		public static System.Void GetDirMonConfiguration(System.Int32* FCNMode)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetDirMonConfiguration' has not been implemented!");
		}

		public static System.Void DirMonClose(System.Runtime.InteropServices.HandleRef dirMon)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.DirMonClose' has not been implemented!");
		}

		public static System.Int32 DirMonOpen(System.String dir, System.String appId, System.Boolean watchSubtree, System.UInt32 notifyFilter, System.Web.NativeFileChangeNotification callback, System.IntPtr* pCompletion)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.DirMonOpen' has not been implemented!");
		}

		public static System.Int32 EcbGetBasicsContentInfo(System.IntPtr pECB, System.Int32[] contentInfo)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetBasicsContentInfo' has not been implemented!");
		}

		public static System.Int32 EcbGetTraceContextId(System.IntPtr pECB, System.Guid* traceContextId)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetTraceContextId' has not been implemented!");
		}

		public static System.Int32 EcbGetUnicodeServerVariable(System.IntPtr pECB, System.String name, System.IntPtr buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetUnicodeServerVariable' has not been implemented!");
		}

		public static System.Int32 EcbGetUnicodeServerVariableByIndex(System.IntPtr pECB, System.Int32 nameIndex, System.IntPtr buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetUnicodeServerVariableByIndex' has not been implemented!");
		}

		public static System.Int32 EcbGetUnicodeServerVariables(System.IntPtr pECB, System.IntPtr buffer, System.Int32 bufferSizeInChars, System.Int32[] serverVarLengths, System.Int32 serverVarCount, System.Int32 startIndex, System.Int32* requiredSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetUnicodeServerVariables' has not been implemented!");
		}

		public static System.Int32 EcbGetVersion(System.IntPtr pECB)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetVersion' has not been implemented!");
		}

		public static System.Int32 EcbGetQueryStringRawBytes(System.IntPtr pECB, System.Byte[] buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetQueryStringRawBytes' has not been implemented!");
		}

		public static System.Int32 EcbGetPreloadedPostedContent(System.IntPtr pECB, System.Byte[] bytes, System.Int32 offset, System.Int32 bufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetPreloadedPostedContent' has not been implemented!");
		}

		public static System.Int32 EcbFlushCore(System.IntPtr pECB, System.Byte[] status, System.Byte[] header, System.Int32 keepConnected, System.Int32 totalBodySize, System.Int32 numBodyFragments, System.IntPtr[] bodyFragments, System.Int32[] bodyFragmentLengths, System.Int32 doneWithSession, System.Int32 finalStatus, System.Int32 kernelCache, System.Int32 async, System.Web.Hosting.ISAPIAsyncCompletionCallback asyncCompletionCallback)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbFlushCore' has not been implemented!");
		}

		public static System.Int32 EcbIsClientConnected(System.IntPtr pECB)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbIsClientConnected' has not been implemented!");
		}

		public static System.Void FreeFileSecurityDescriptor(System.IntPtr securityDesciptor)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.FreeFileSecurityDescriptor' has not been implemented!");
		}

		public static System.IntPtr GetFileSecurityDescriptor(System.String strFile)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetFileSecurityDescriptor' has not been implemented!");
		}

		public static System.Int32 GetHMACSHA1Hash(System.Byte[] data1, System.Int32 dataOffset1, System.Int32 dataSize1, System.Byte[] data2, System.Int32 dataSize2, System.Byte[] innerKey, System.Int32 innerKeySize, System.Byte[] outerKey, System.Int32 outerKeySize, System.Byte[] hash, System.Int32 hashSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetHMACSHA1Hash' has not been implemented!");
		}

		public static System.Int32 GetPrivateBytesIIS6(System.Int64* privatePageCount, System.Boolean nocache)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetPrivateBytesIIS6' has not been implemented!");
		}

		public static System.Int32 GetW3WPMemoryLimitInKB()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetW3WPMemoryLimitInKB' has not been implemented!");
		}

		public static System.Void SetClrThreadPoolLimits(System.Int32 maxWorkerThreads, System.Int32 maxIoThreads)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SetClrThreadPoolLimits' has not been implemented!");
		}

		public static System.Void SetMinRequestsExecutingToDetectDeadlock(System.Int32 minRequestsExecutingToDetectDeadlock)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SetMinRequestsExecutingToDetectDeadlock' has not been implemented!");
		}

		public static System.Void InitializeLibrary(System.Boolean reduceMaxThreads)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InitializeLibrary' has not been implemented!");
		}

		public static System.Void InitializeHealthMonitor(System.Int32 deadlockIntervalSeconds, System.Int32 requestQueueLimit)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InitializeHealthMonitor' has not been implemented!");
		}

		public static System.Int32 IsAccessToFileAllowed(System.IntPtr securityDesciptor, System.IntPtr iThreadToken, System.Int32 iAccess)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsAccessToFileAllowed' has not been implemented!");
		}

		public static System.Int32 EcbCallISAPI(System.IntPtr pECB, System.Web.UnsafeNativeMethods+CallISAPIFunc iFunction, System.Byte[] bufferIn, System.Int32 sizeIn, System.Byte[] bufferOut, System.Int32 sizeOut)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbCallISAPI' has not been implemented!");
		}

		public static System.IntPtr InstrumentedMutexCreate(System.String name)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InstrumentedMutexCreate' has not been implemented!");
		}

		public static System.Void InstrumentedMutexDelete(System.Runtime.InteropServices.HandleRef mutex)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InstrumentedMutexDelete' has not been implemented!");
		}

		public static System.Int32 InstrumentedMutexGetLock(System.Runtime.InteropServices.HandleRef mutex, System.Int32 timeout)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InstrumentedMutexGetLock' has not been implemented!");
		}

		public static System.Int32 InstrumentedMutexReleaseLock(System.Runtime.InteropServices.HandleRef mutex)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InstrumentedMutexReleaseLock' has not been implemented!");
		}

		public static System.Int32 IsapiAppHostMapPath(System.String appId, System.String virtualPath, System.Text.StringBuilder buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsapiAppHostMapPath' has not been implemented!");
		}

		public static System.Int32 IsapiAppHostGetAppPath(System.String aboPath, System.Text.StringBuilder buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsapiAppHostGetAppPath' has not been implemented!");
		}

		public static System.Int32 IsapiAppHostGetUncUser(System.String appId, System.Text.StringBuilder usernameBuffer, System.Int32 usernameSize, System.Text.StringBuilder passwordBuffer, System.Int32 passwordSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsapiAppHostGetUncUser' has not been implemented!");
		}

		public static System.Int32 IsapiAppHostGetSiteName(System.String appId, System.Text.StringBuilder buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsapiAppHostGetSiteName' has not been implemented!");
		}

		public static System.IntPtr BufferPoolGetPool(System.Int32 bufferSize, System.Int32 maxFreeListCount)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.BufferPoolGetPool' has not been implemented!");
		}

		public static System.IntPtr BufferPoolGetBuffer(System.IntPtr pool)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.BufferPoolGetBuffer' has not been implemented!");
		}

		public static System.Void BufferPoolReleaseBuffer(System.IntPtr buffer)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.BufferPoolReleaseBuffer' has not been implemented!");
		}

		public static System.IntPtr PerfOpenGlobalCounters()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfOpenGlobalCounters' has not been implemented!");
		}

		public static System.Web.PerfInstanceDataHandle PerfOpenAppCounters(System.String AppName)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfOpenAppCounters' has not been implemented!");
		}

		public static System.Void PerfCloseAppCounters(System.IntPtr pCounters)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfCloseAppCounters' has not been implemented!");
		}

		public static System.Void PerfIncrementCounter(System.IntPtr pCounters, System.Int32 number)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfIncrementCounter' has not been implemented!");
		}

		public static System.Void PerfDecrementCounter(System.IntPtr pCounters, System.Int32 number)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfDecrementCounter' has not been implemented!");
		}

		public static System.Void PerfIncrementCounterEx(System.IntPtr pCounters, System.Int32 number, System.Int32 increment)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfIncrementCounterEx' has not been implemented!");
		}

		public static System.Void PerfSetCounter(System.IntPtr pCounters, System.Int32 number, System.Int32 increment)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfSetCounter' has not been implemented!");
		}

		public static System.Void GetEtwValues(System.Int32* level, System.Int32* flags)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetEtwValues' has not been implemented!");
		}

		public static System.Boolean IsValidResource(System.IntPtr hModule, System.IntPtr ip, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsValidResource' has not been implemented!");
		}

		public static System.Int32 DeleteShadowCache(System.String pwzCachePath, System.String pwzAppName)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.DeleteShadowCache' has not been implemented!");
		}

		public static System.Int32 RaiseEventlogEvent(System.Int32 eventType, System.String[] dataFields, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.RaiseEventlogEvent' has not been implemented!");
		}

		public static System.IntPtr GetEcb(System.IntPtr pHttpCompletion)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetEcb' has not been implemented!");
		}

		public static System.IntPtr GetExtensionlessUrlAppendage()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetExtensionlessUrlAppendage' has not been implemented!");
		}

		public static System.Int32 SetThreadToken(System.IntPtr threadref, System.IntPtr token)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SetThreadToken' has not been implemented!");
		}

		public static System.Int32 RevertToSelf()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.RevertToSelf' has not been implemented!");
		}

		public static System.Int32 LogonUser(System.String username, System.String domain, System.String password, System.Int32 dwLogonType, System.Int32 dwLogonProvider, System.IntPtr* phToken)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.LogonUser' has not been implemented!");
		}

		public static System.Int32 ConvertStringSidToSid(System.String stringSid, System.IntPtr* pSid)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.ConvertStringSidToSid' has not been implemented!");
		}

		public static System.Int32 LookupAccountSid(System.String systemName, System.IntPtr pSid, System.Text.StringBuilder szName, System.Int32* nameSize, System.Text.StringBuilder szDomain, System.Int32* domainSize, System.Int32* eUse)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.LookupAccountSid' has not been implemented!");
		}

		public static System.Void STWNDCloseConnection(System.IntPtr tracker)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDCloseConnection' has not been implemented!");
		}

		public static System.Void STWNDDeleteStateItem(System.IntPtr stateItem)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDDeleteStateItem' has not been implemented!");
		}

		public static System.Void STWNDEndOfRequest(System.IntPtr tracker)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDEndOfRequest' has not been implemented!");
		}

		public static System.Void STWNDGetLocalAddress(System.IntPtr tracker, System.Text.StringBuilder buf)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDGetLocalAddress' has not been implemented!");
		}

		public static System.Int32 STWNDGetLocalPort(System.IntPtr tracker)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDGetLocalPort' has not been implemented!");
		}

		public static System.Void STWNDGetRemoteAddress(System.IntPtr tracker, System.Text.StringBuilder buf)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDGetRemoteAddress' has not been implemented!");
		}

		public static System.Int32 STWNDGetRemotePort(System.IntPtr tracker)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDGetRemotePort' has not been implemented!");
		}

		public static System.Boolean STWNDIsClientConnected(System.IntPtr tracker)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDIsClientConnected' has not been implemented!");
		}

		public static System.Void STWNDSendResponse(System.IntPtr tracker, System.Text.StringBuilder status, System.Int32 statusLength, System.Text.StringBuilder headers, System.Int32 headersLength, System.IntPtr unmanagedState)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.STWNDSendResponse' has not been implemented!");
		}

		public static System.Boolean MoveFileEx(System.String oldFilename, System.String newFilename, System.UInt32 flags)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.MoveFileEx' has not been implemented!");
		}

		public static System.Boolean CloseHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Int32 GetComputerName(System.Text.StringBuilder nameBuffer, System.Int32* bufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetComputerName' has not been implemented!");
		}

		public static System.IntPtr LoadLibrary(System.String libFilename)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.LoadLibrary' has not been implemented!");
		}

		public static System.Boolean FreeLibrary(System.IntPtr hModule)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.FreeLibrary' has not been implemented!");
		}

		public static System.IntPtr LocalFree(System.IntPtr pMem)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.LocalFree' has not been implemented!");
		}

		public static System.Void AppDomainRestart(System.String appId)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.AppDomainRestart' has not been implemented!");
		}

		public static System.Int32 AspCompatProcessRequest(System.Web.Util.AspCompatCallback callback, System.Object context, System.Boolean sharedActivity, System.Int32 activityHash)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.AspCompatProcessRequest' has not been implemented!");
		}

		public static System.Int32 AspCompatOnPageStart(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.AspCompatOnPageStart' has not been implemented!");
		}

		public static System.Int32 AspCompatOnPageEnd()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.AspCompatOnPageEnd' has not been implemented!");
		}

		public static System.Int32 AspCompatIsApartmentComponent(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.AspCompatIsApartmentComponent' has not been implemented!");
		}

		public static System.Int32 AttachDebugger(System.String clsId, System.String sessId, System.IntPtr userToken)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.AttachDebugger' has not been implemented!");
		}

		public static System.Int32 ChangeAccessToKeyContainer(System.String containerName, System.String accountName, System.String csp, System.Int32 options)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.ChangeAccessToKeyContainer' has not been implemented!");
		}

		public static System.Int32 CookieAuthParseTicket(System.Byte[] pData, System.Int32 iDataLen, System.Text.StringBuilder szName, System.Int32 iNameLen, System.Text.StringBuilder szData, System.Int32 iUserDataLen, System.Text.StringBuilder szPath, System.Int32 iPathLen, System.Byte[] pBytes, System.Int64[] pDates)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.CookieAuthParseTicket' has not been implemented!");
		}

		public static System.Int32 CookieAuthConstructTicket(System.Byte[] pData, System.Int32 iDataLen, System.String szName, System.String szData, System.String szPath, System.Byte[] pBytes, System.Int64[] pDates)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.CookieAuthConstructTicket' has not been implemented!");
		}

		public static System.Int32 GrowFileNotificationBuffer(System.String appId, System.Boolean fWatchSubtree)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GrowFileNotificationBuffer' has not been implemented!");
		}

		public static System.Void EcbFreeExecUrlEntityInfo(System.IntPtr pEntity)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbFreeExecUrlEntityInfo' has not been implemented!");
		}

		public static System.Int32 EcbGetBasics(System.IntPtr pECB, System.Byte[] buffer, System.Int32 size, System.Int32[] contentInfo)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetBasics' has not been implemented!");
		}

		public static System.Int32 EcbGetTraceFlags(System.IntPtr pECB, System.Int32[] contentInfo)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetTraceFlags' has not been implemented!");
		}

		public static System.Int32 EcbEmitSimpleTrace(System.IntPtr pECB, System.Int32 type, System.String eventData)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbEmitSimpleTrace' has not been implemented!");
		}

		public static System.Int32 EcbEmitWebEventTrace(System.IntPtr pECB, System.Int32 webEventType, System.Int32 fieldCount, System.String[] fieldNames, System.Int32[] fieldTypes, System.String[] fieldData)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbEmitWebEventTrace' has not been implemented!");
		}

		public static System.Int32 EcbGetClientCertificate(System.IntPtr pECB, System.Byte[] buffer, System.Int32 size, System.Int32[] pInts, System.Int64[] pDates)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetClientCertificate' has not been implemented!");
		}

		public static System.Int32 EcbGetExecUrlEntityInfo(System.Int32 entityLength, System.Byte[] entity, System.IntPtr* ppEntity)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetExecUrlEntityInfo' has not been implemented!");
		}

		public static System.Int32 EcbGetServerVariable(System.IntPtr pECB, System.String name, System.Byte[] buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetServerVariable' has not been implemented!");
		}

		public static System.Int32 EcbGetServerVariableByIndex(System.IntPtr pECB, System.Int32 nameIndex, System.Byte[] buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetServerVariableByIndex' has not been implemented!");
		}

		public static System.Int32 EcbGetQueryString(System.IntPtr pECB, System.Int32 encode, System.Text.StringBuilder buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetQueryString' has not been implemented!");
		}

		public static System.Int32 EcbGetAdditionalPostedContent(System.IntPtr pECB, System.Byte[] bytes, System.Int32 offset, System.Int32 bufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetAdditionalPostedContent' has not been implemented!");
		}

		public static System.Int32 EcbCloseConnection(System.IntPtr pECB)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbCloseConnection' has not been implemented!");
		}

		public static System.Int32 EcbMapUrlToPath(System.IntPtr pECB, System.String url, System.Byte[] buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbMapUrlToPath' has not been implemented!");
		}

		public static System.IntPtr EcbGetImpersonationToken(System.IntPtr pECB, System.IntPtr processHandle)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetImpersonationToken' has not been implemented!");
		}

		public static System.IntPtr EcbGetVirtualPathToken(System.IntPtr pECB, System.IntPtr processHandle)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetVirtualPathToken' has not been implemented!");
		}

		public static System.Int32 EcbAppendLogParameter(System.IntPtr pECB, System.String logParam)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbAppendLogParameter' has not been implemented!");
		}

		public static System.Int32 EcbExecuteUrlUnicode(System.IntPtr pECB, System.String url, System.String method, System.String childHeaders, System.Boolean sendHeaders, System.Boolean addUserIndo, System.IntPtr token, System.String name, System.String authType, System.IntPtr pEntity, System.Web.Hosting.ISAPIAsyncCompletionCallback asyncCompletionCallback)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbExecuteUrlUnicode' has not been implemented!");
		}

		public static System.Void InvalidateKernelCache(System.String key)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InvalidateKernelCache' has not been implemented!");
		}

		public static System.IntPtr GetFileHandleForTransmitFile(System.String strFile)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetFileHandleForTransmitFile' has not been implemented!");
		}

		public static System.Int32 GetGroupsForUser(System.IntPtr token, System.Text.StringBuilder allGroups, System.Int32 allGrpSize, System.Text.StringBuilder error, System.Int32 errorSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetGroupsForUser' has not been implemented!");
		}

		public static System.Int32 GetProcessMemoryInformation(System.UInt32 pid, System.UInt32* privatePageCount, System.UInt32* peakPagefileUsage, System.Boolean nocache)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetProcessMemoryInformation' has not been implemented!");
		}

		public static System.Int32 GetSHA1Hash(System.Byte[] data, System.Int32 dataSize, System.Byte[] hash, System.Int32 hashSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetSHA1Hash' has not been implemented!");
		}

		public static System.Void PerfCounterInitialize()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfCounterInitialize' has not been implemented!");
		}

		public static System.Int32 IsUserInRole(System.IntPtr token, System.String rolename, System.Text.StringBuilder error, System.Int32 errorSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsUserInRole' has not been implemented!");
		}

		public static System.Void UpdateLastActivityTimeForHealthMonitor()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.UpdateLastActivityTimeForHealthMonitor' has not been implemented!");
		}

		public static System.Int32 GetCredentialFromRegistry(System.String strRegKey, System.Text.StringBuilder buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetCredentialFromRegistry' has not been implemented!");
		}

		public static System.Int32 EcbGetChannelBindingToken(System.IntPtr pECB, System.IntPtr* token, System.Int32* tokenSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.EcbGetChannelBindingToken' has not been implemented!");
		}

		public static System.Int32 PassportVersion()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportVersion' has not been implemented!");
		}

		public static System.Int32 PassportCreateHttpRaw(System.String szRequestLine, System.String szHeaders, System.Int32 fSecure, System.Text.StringBuilder szBufOut, System.Int32 dwRetBufSize, System.IntPtr* passportManager)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportCreateHttpRaw' has not been implemented!");
		}

		public static System.Int32 PassportTicket(System.IntPtr pManager, System.String szAttr, System.Object* pReturn)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportTicket' has not been implemented!");
		}

		public static System.Int32 PassportGetCurrentConfig(System.IntPtr pManager, System.String szAttr, System.Object* pReturn)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetCurrentConfig' has not been implemented!");
		}

		public static System.Int32 PassportLogoutURL(System.IntPtr pManager, System.String szReturnURL, System.String szCOBrandArgs, System.Int32 iLangID, System.String strDomain, System.Int32 iUseSecureAuth, System.Text.StringBuilder szAuthVal, System.Int32 iAuthValSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportLogoutURL' has not been implemented!");
		}

		public static System.Int32 PassportGetOption(System.IntPtr pManager, System.String szOption, System.Object* vOut)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetOption' has not been implemented!");
		}

		public static System.Int32 PassportSetOption(System.IntPtr pManager, System.String szOption, System.Object vOut)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportSetOption' has not been implemented!");
		}

		public static System.Int32 PassportGetLoginChallenge(System.IntPtr pManager, System.String szRetURL, System.Int32 iTimeWindow, System.Int32 fForceLogin, System.String szCOBrandArgs, System.Int32 iLangID, System.String strNameSpace, System.Int32 iKPP, System.Int32 iUseSecureAuth, System.Object vExtraParams, System.Text.StringBuilder szOut, System.Int32 iOutSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetLoginChallenge' has not been implemented!");
		}

		public static System.Int32 PassportHexPUID(System.IntPtr pManager, System.Text.StringBuilder szOut, System.Int32 iOutSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportHexPUID' has not been implemented!");
		}

		public static System.Int32 PassportCreate(System.String szQueryStrT, System.String szQueryStrP, System.String szAuthCookie, System.String szProfCookie, System.String szProfCCookie, System.Text.StringBuilder szAuthCookieRet, System.Text.StringBuilder szProfCookieRet, System.Int32 iRetBufSize, System.IntPtr* passportManager)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportCreate' has not been implemented!");
		}

		public static System.Int32 PassportAuthURL(System.IntPtr iPassport, System.String szReturnURL, System.Int32 iTimeWindow, System.Int32 fForceLogin, System.String szCOBrandArgs, System.Int32 iLangID, System.String strNameSpace, System.Int32 iKPP, System.Int32 iUseSecureAuth, System.Text.StringBuilder szAuthVal, System.Int32 iAuthValSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportAuthURL' has not been implemented!");
		}

		public static System.Int32 PassportAuthURL2(System.IntPtr iPassport, System.String szReturnURL, System.Int32 iTimeWindow, System.Int32 fForceLogin, System.String szCOBrandArgs, System.Int32 iLangID, System.String strNameSpace, System.Int32 iKPP, System.Int32 iUseSecureAuth, System.Text.StringBuilder szAuthVal, System.Int32 iAuthValSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportAuthURL2' has not been implemented!");
		}

		public static System.Int32 PassportGetError(System.IntPtr iPassport)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetError' has not been implemented!");
		}

		public static System.Int32 PassportDomainFromMemberName(System.IntPtr iPassport, System.String szDomain, System.Text.StringBuilder szMember, System.Int32 iMemberSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportDomainFromMemberName' has not been implemented!");
		}

		public static System.Int32 PassportGetFromNetworkServer(System.IntPtr iPassport)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetFromNetworkServer' has not been implemented!");
		}

		public static System.Int32 PassportGetDomainAttribute(System.IntPtr iPassport, System.String szAttributeName, System.Int32 iLCID, System.String szDomain, System.Text.StringBuilder szValue, System.Int32 iValueSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetDomainAttribute' has not been implemented!");
		}

		public static System.Int32 PassportHasProfile(System.IntPtr iPassport, System.String szProfile)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportHasProfile' has not been implemented!");
		}

		public static System.Int32 PassportHasFlag(System.IntPtr iPassport, System.Int32 iFlagMask)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportHasFlag' has not been implemented!");
		}

		public static System.Int32 PassportHasConsent(System.IntPtr iPassport, System.Int32 iFullConsent, System.Int32 iNeedBirthdate)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportHasConsent' has not been implemented!");
		}

		public static System.Int32 PassportGetHasSavedPassword(System.IntPtr iPassport)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetHasSavedPassword' has not been implemented!");
		}

		public static System.Int32 PassportHasTicket(System.IntPtr iPassport)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportHasTicket' has not been implemented!");
		}

		public static System.Int32 PassportIsAuthenticated(System.IntPtr iPassport, System.Int32 iTimeWindow, System.Int32 fForceLogin, System.Int32 iUseSecureAuth)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportIsAuthenticated' has not been implemented!");
		}

		public static System.Int32 PassportLogoTag(System.IntPtr iPassport, System.String szRetURL, System.Int32 iTimeWindow, System.Int32 fForceLogin, System.String szCOBrandArgs, System.Int32 iLangID, System.Int32 fSecure, System.String strNameSpace, System.Int32 iKPP, System.Int32 iUseSecureAuth, System.Text.StringBuilder szValue, System.Int32 iValueSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportLogoTag' has not been implemented!");
		}

		public static System.Int32 PassportLogoTag2(System.IntPtr iPassport, System.String szRetURL, System.Int32 iTimeWindow, System.Int32 fForceLogin, System.String szCOBrandArgs, System.Int32 iLangID, System.Int32 fSecure, System.String strNameSpace, System.Int32 iKPP, System.Int32 iUseSecureAuth, System.Text.StringBuilder szValue, System.Int32 iValueSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportLogoTag2' has not been implemented!");
		}

		public static System.Int32 PassportGetProfile(System.IntPtr iPassport, System.String szProfile, System.Object* rOut)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetProfile' has not been implemented!");
		}

		public static System.Int32 PassportGetTicketAge(System.IntPtr iPassport)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetTicketAge' has not been implemented!");
		}

		public static System.Int32 PassportGetTimeSinceSignIn(System.IntPtr iPassport)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportGetTimeSinceSignIn' has not been implemented!");
		}

		public static System.Void PassportDestroy(System.IntPtr iPassport)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportDestroy' has not been implemented!");
		}

		public static System.Int32 PassportCrypt(System.Int32 iFunctionID, System.String szSrc, System.Text.StringBuilder szDest, System.Int32 iDestLength)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportCrypt' has not been implemented!");
		}

		public static System.Int32 PassportCryptPut(System.Int32 iFunctionID, System.String szSrc)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportCryptPut' has not been implemented!");
		}

		public static System.Int32 PassportCryptIsValid()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PassportCryptIsValid' has not been implemented!");
		}

		public static System.Int32 PostThreadPoolWorkItem(System.Web.Util.WorkItemCallback callback)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PostThreadPoolWorkItem' has not been implemented!");
		}

		public static System.Void InstrumentedMutexSetState(System.Runtime.InteropServices.HandleRef mutex, System.Int32 state)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InstrumentedMutexSetState' has not been implemented!");
		}

		public static System.Int32 IsapiAppHostGetSiteId(System.String site, System.Text.StringBuilder buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsapiAppHostGetSiteId' has not been implemented!");
		}

		public static System.Int32 IsapiAppHostGetNextVirtualSubdir(System.String aboPath, System.Boolean inApp, System.Int32* index, System.Text.StringBuilder sb, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.IsapiAppHostGetNextVirtualSubdir' has not been implemented!");
		}

		public static System.Int32 PMGetTraceContextId(System.IntPtr pMsg, System.Guid* traceContextId)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetTraceContextId' has not been implemented!");
		}

		public static System.Int32 PMGetHistoryTable(System.Int32 iRows, System.Int32[] dwPIDArr, System.Int32[] dwReqExecuted, System.Int32[] dwReqPending, System.Int32[] dwReqExecuting, System.Int32[] dwReasonForDeath, System.Int32[] dwPeakMemoryUsed, System.Int64[] tmCreateTime, System.Int64[] tmDeathTime)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetHistoryTable' has not been implemented!");
		}

		public static System.Int32 PMGetCurrentProcessInfo(System.Int32* dwReqExecuted, System.Int32* dwReqExecuting, System.Int32* dwPeakMemoryUsed, System.Int64* tmCreateTime, System.Int32* pid)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetCurrentProcessInfo' has not been implemented!");
		}

		public static System.Int32 PMGetMemoryLimitInMB()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetMemoryLimitInMB' has not been implemented!");
		}

		public static System.Int32 PMGetBasics(System.IntPtr pMsg, System.Byte[] buffer, System.Int32 size, System.Int32[] contentInfo)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetBasics' has not been implemented!");
		}

		public static System.Int32 PMGetClientCertificate(System.IntPtr pMsg, System.Byte[] buffer, System.Int32 size, System.Int32[] pInts, System.Int64[] pDates)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetClientCertificate' has not been implemented!");
		}

		public static System.Int64 PMGetStartTimeStamp(System.IntPtr pMsg)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetStartTimeStamp' has not been implemented!");
		}

		public static System.Int32 PMGetAllServerVariables(System.IntPtr pMsg, System.Byte[] buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetAllServerVariables' has not been implemented!");
		}

		public static System.Int32 PMGetQueryString(System.IntPtr pMsg, System.Int32 encode, System.Text.StringBuilder buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetQueryString' has not been implemented!");
		}

		public static System.Int32 PMGetQueryStringRawBytes(System.IntPtr pMsg, System.Byte[] buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetQueryStringRawBytes' has not been implemented!");
		}

		public static System.Int32 PMGetPreloadedPostedContent(System.IntPtr pMsg, System.Byte[] bytes, System.Int32 offset, System.Int32 bufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetPreloadedPostedContent' has not been implemented!");
		}

		public static System.Int32 PMGetAdditionalPostedContent(System.IntPtr pMsg, System.Byte[] bytes, System.Int32 offset, System.Int32 bufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetAdditionalPostedContent' has not been implemented!");
		}

		public static System.Int32 PMEmptyResponse(System.IntPtr pMsg)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMEmptyResponse' has not been implemented!");
		}

		public static System.Int32 PMIsClientConnected(System.IntPtr pMsg)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMIsClientConnected' has not been implemented!");
		}

		public static System.Int32 PMCloseConnection(System.IntPtr pMsg)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMCloseConnection' has not been implemented!");
		}

		public static System.Int32 PMMapUrlToPath(System.IntPtr pMsg, System.String url, System.Byte[] buffer, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMMapUrlToPath' has not been implemented!");
		}

		public static System.IntPtr PMGetImpersonationToken(System.IntPtr pMsg)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetImpersonationToken' has not been implemented!");
		}

		public static System.IntPtr PMGetVirtualPathToken(System.IntPtr pMsg)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMGetVirtualPathToken' has not been implemented!");
		}

		public static System.Int32 PMAppendLogParameter(System.IntPtr pMsg, System.String logParam)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMAppendLogParameter' has not been implemented!");
		}

		public static System.Int32 PMFlushCore(System.IntPtr pMsg, System.Byte[] status, System.Byte[] header, System.Int32 keepConnected, System.Int32 totalBodySize, System.Int32 bodyFragmentsOffset, System.Int32 numBodyFragments, System.IntPtr[] bodyFragments, System.Int32[] bodyFragmentLengths, System.Int32 doneWithSession, System.Int32 finalStatus)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMFlushCore' has not been implemented!");
		}

		public static System.Int32 PMCallISAPI(System.IntPtr pECB, System.Web.UnsafeNativeMethods+CallISAPIFunc iFunction, System.Byte[] bufferIn, System.Int32 sizeIn, System.Byte[] bufferOut, System.Int32 sizeOut)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMCallISAPI' has not been implemented!");
		}

		public static System.IntPtr PerfOpenStateCounters()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfOpenStateCounters' has not been implemented!");
		}

		public static System.Int32 PerfGetCounter(System.IntPtr pCounters, System.Int32 number)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PerfGetCounter' has not been implemented!");
		}

		public static System.Void TraceRaiseEventMgdHandler(System.Int32 eventType, System.IntPtr pRequestContext, System.String data1, System.String data2, System.String data3, System.String data4)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.TraceRaiseEventMgdHandler' has not been implemented!");
		}

		public static System.Void TraceRaiseEventWithEcb(System.Int32 eventType, System.IntPtr ecb, System.String data1, System.String data2, System.String data3, System.String data4)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.TraceRaiseEventWithEcb' has not been implemented!");
		}

		public static System.Void PMTraceRaiseEvent(System.Int32 eventType, System.IntPtr pMsg, System.String data1, System.String data2, System.String data3, System.String data4)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.PMTraceRaiseEvent' has not been implemented!");
		}

		public static System.Int32 SessionNDConnectToService(System.String server)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SessionNDConnectToService' has not been implemented!");
		}

		public static System.Int32 SessionNDMakeRequest(System.Runtime.InteropServices.HandleRef socket, System.String server, System.Int32 port, System.Int32 networkTimeout, System.Web.UnsafeNativeMethods+StateProtocolVerb verb, System.String uri, System.Web.UnsafeNativeMethods+StateProtocolExclusive exclusive, System.Int32 extraFlags, System.Int32 timeout, System.Int32 lockCookie, System.Byte[] body, System.Int32 cb, System.Boolean checkVersion, System.Web.UnsafeNativeMethods+SessionNDMakeRequestResults* results)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SessionNDMakeRequest' has not been implemented!");
		}

		public static System.Void SessionNDFreeBody(System.Runtime.InteropServices.HandleRef body)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SessionNDFreeBody' has not been implemented!");
		}

		public static System.Void SessionNDCloseConnection(System.Runtime.InteropServices.HandleRef socket)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SessionNDCloseConnection' has not been implemented!");
		}

		public static System.Int32 TransactManagedCallback(System.Web.Util.TransactedExecCallback callback, System.Int32 mode)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.TransactManagedCallback' has not been implemented!");
		}

		public static System.Int32 GetCachePath(System.Int32 dwCacheFlags, System.Text.StringBuilder pwzCachePath, System.Int32* pcchPath)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.GetCachePath' has not been implemented!");
		}

		public static System.Int32 InitializeWmiManager()
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.InitializeWmiManager' has not been implemented!");
		}

		public static System.Int32 DoesKeyContainerExist(System.String containerName, System.String provider, System.Int32 useMachineContainer)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.DoesKeyContainerExist' has not been implemented!");
		}

		public static System.Int32 RaiseWmiEvent(System.Web.UnsafeNativeMethods+WmiData* pWmiData, System.Boolean IsInAspCompatMode)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.RaiseWmiEvent' has not been implemented!");
		}

		public static System.Void LogWebeventProviderFailure(System.String appUrl, System.String providerName, System.String exception)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.LogWebeventProviderFailure' has not been implemented!");
		}

		public static System.Void SetDoneWithSessionCalled(System.IntPtr pHttpCompletion)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.SetDoneWithSessionCalled' has not been implemented!");
		}

		public static System.Void ReportUnhandledException(System.String eventInfo)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.ReportUnhandledException' has not been implemented!");
		}

		public static System.Void RaiseFileMonitoringEventlogEvent(System.String eventInfo, System.String path, System.String appVirtualPath, System.Int32 hr)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.RaiseFileMonitoringEventlogEvent' has not been implemented!");
		}

		public static System.Int32 CoCreateInstanceEx(System.Guid* clsid, System.IntPtr pUnkOuter, System.Int32 dwClsContext, System.Web.Configuration.COSERVERINFO srv, System.Int32 num, System.Web.Configuration.MULTI_QI[] amqi)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.CoCreateInstanceEx' has not been implemented!");
		}

		public static System.Int32 CoCreateInstanceEx(System.Guid* clsid, System.IntPtr pUnkOuter, System.Int32 dwClsContext, System.Web.Configuration.COSERVERINFO_X64 srv, System.Int32 num, System.Web.Configuration.MULTI_QI_X64[] amqi)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.CoCreateInstanceEx' has not been implemented!");
		}

		public static System.Int32 CoSetProxyBlanket(System.IntPtr pProxy, System.Web.Configuration.RpcAuthent authent, System.Web.Configuration.RpcAuthor author, System.String serverprinc, System.Web.Configuration.RpcLevel level, System.Web.Configuration.RpcImpers impers, System.IntPtr ciptr, System.Int32 dwCapabilities)
		{
			throw new System.NotImplementedException("Method 'System.Web.UnsafeNativeMethods.CoSetProxyBlanket' has not been implemented!");
		}
	}
}
