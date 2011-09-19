namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Web.Hosting.UnsafeIISMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Web_Hosting_UnsafeIISMethodsImpl
	{

		public static System.Int32 MgdGetRequestBasics(System.IntPtr pRequestContext, System.Int32* pContentType, System.Int32* pContentTotalLength, System.IntPtr* pPathTranslated, System.Int32* pcchPathTranslated, System.IntPtr* pCacheUrl, System.Int32* pcchCacheUrl)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetRequestBasics' has not been implemented!");
		}

		public static System.Int32 MgdGetHeaderChanges(System.IntPtr pRequestContext, System.Boolean fResponse, System.IntPtr* knownHeaderSnapshot, System.Int32* unknownHeaderSnapshotCount, System.IntPtr* unknownHeaderSnapshotNames, System.IntPtr* unknownHeaderSnapshotValues, System.IntPtr* diffKnownIndicies, System.Int32* diffUnknownCount, System.IntPtr* diffUnknownIndicies)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetHeaderChanges' has not been implemented!");
		}

		public static System.Int32 MgdGetServerVariableW(System.IntPtr pHandler, System.String pszVarName, System.IntPtr* ppBuffer, System.Int32* pcchBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetServerVariableW' has not been implemented!");
		}

		public static System.Boolean MgdHasConfigChanged()
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdHasConfigChanged' has not been implemented!");
		}

		public static System.Void MgdSetManagedHttpContext(System.IntPtr pHandler, System.IntPtr pManagedHttpContext)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetManagedHttpContext' has not been implemented!");
		}

		public static System.Int32 MgdSetKnownHeader(System.IntPtr pRequestContext, System.Boolean fRequest, System.Boolean fReplace, System.UInt16 uHeaderIndex, System.Byte[] value, System.UInt16 valueSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetKnownHeader' has not been implemented!");
		}

		public static System.Int32 MgdSetUnknownHeader(System.IntPtr pRequestContext, System.Boolean fRequest, System.Boolean fReplace, System.Byte[] header, System.Byte[] value, System.UInt16 valueSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetUnknownHeader' has not been implemented!");
		}

		public static System.Int32 MgdFlushCore(System.IntPtr pRequestContext, System.Boolean keepConnected, System.Int32 numBodyFragments, System.IntPtr[] bodyFragments, System.Int32[] bodyFragmentLengths, System.Int32[] fragmentsNative)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdFlushCore' has not been implemented!");
		}

		public static System.Int32 MgdRegisterEventSubscription(System.IntPtr pAppContext, System.String pszModuleName, System.Web.RequestNotification requestNotifications, System.Web.RequestNotification postRequestNotifications, System.String pszModuleType, System.String pszModulePrecondition, System.IntPtr moduleSpecificData, System.Boolean useHighPriority)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdRegisterEventSubscription' has not been implemented!");
		}

		public static System.Void MgdIndicateCompletion(System.IntPtr pHandler, System.Web.RequestNotificationStatus* notificationStatus)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdIndicateCompletion' has not been implemented!");
		}

		public static System.Int32 MgdGetQueryString(System.IntPtr pHandler, System.IntPtr* pBuffer, System.Int32* cchBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetQueryString' has not been implemented!");
		}

		public static System.Int32 MgdGetUserToken(System.IntPtr pHandler, System.IntPtr* pToken)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetUserToken' has not been implemented!");
		}

		public static System.Boolean MgdIsHandlerExecutionDenied(System.IntPtr pHandler)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdIsHandlerExecutionDenied' has not been implemented!");
		}

		public static System.Int32 MgdGetHandlerTypeString(System.IntPtr pHandler, System.IntPtr* ppszTypeString, System.Int32* pcchTypeString)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetHandlerTypeString' has not been implemented!");
		}

		public static System.Int32 MgdGetApplicationInfo(System.IntPtr pHandler, System.IntPtr* pVirtualPath, System.Int32* cchVirtualPath, System.IntPtr* pPhysPath, System.Int32* cchPhysPath)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetApplicationInfo' has not been implemented!");
		}

		public static System.Int32 MgdGetUriPath(System.IntPtr pHandler, System.IntPtr* ppPath, System.Int32* pcchPath, System.Boolean fIncludePathInfo, System.Boolean fUseParentContext)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetUriPath' has not been implemented!");
		}

		public static System.Int32 MgdGetPrincipal(System.IntPtr pHandler, System.IntPtr* pToken, System.IntPtr* ppAuthType, System.Int32* pcchAuthType, System.IntPtr* ppUserName, System.Int32* pcchUserName)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetPrincipal' has not been implemented!");
		}

		public static System.Int32 MgdAppDomainShutdown(System.IntPtr appContext)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdAppDomainShutdown' has not been implemented!");
		}

		public static System.IntPtr MgdGetBufferPool(System.Int32 cbBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetBufferPool' has not been implemented!");
		}

		public static System.IntPtr MgdGetBuffer(System.IntPtr pPool)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetBuffer' has not been implemented!");
		}

		public static System.IntPtr MgdReturnBuffer(System.IntPtr pBuffer)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdReturnBuffer' has not been implemented!");
		}

		public static System.Int32 MgdGetUserAgent(System.IntPtr pRequestContext, System.IntPtr* pBuffer, System.Int32* cbBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetUserAgent' has not been implemented!");
		}

		public static System.Int32 MgdGetMethod(System.IntPtr pRequestContext, System.IntPtr* pBuffer, System.Int32* cbBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetMethod' has not been implemented!");
		}

		public static System.Void MgdDisableNotifications(System.IntPtr pRequestContext, System.Web.RequestNotification notifications, System.Web.RequestNotification postNotifications)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdDisableNotifications' has not been implemented!");
		}

		public static System.Int32 MgdGetNextNotification(System.IntPtr pRequestContext, System.Web.RequestNotificationStatus dwStatus)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetNextNotification' has not been implemented!");
		}

		public static System.Int32 MgdGetResponseChunks(System.IntPtr pRequestContext, System.Int32* fragmentCount, System.IntPtr[] bodyFragments, System.Int32[] bodyFragmentLengths, System.Int32[] fragmentChunkType)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetResponseChunks' has not been implemented!");
		}

		public static System.Boolean MgdCanDisposeManagedContext(System.IntPtr pRequestContext, System.Web.RequestNotificationStatus dwStatus)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdCanDisposeManagedContext' has not been implemented!");
		}

		public static System.Boolean MgdIsLastNotification(System.IntPtr pRequestContext, System.Web.RequestNotificationStatus dwStatus)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdIsLastNotification' has not been implemented!");
		}

		public static System.Int32 MgdGetMemoryLimitKB(System.Int64* limit)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetMemoryLimitKB' has not been implemented!");
		}

		public static System.Int32 MgdGetNextModule(System.IntPtr pModuleCollection, System.UInt32* dwIndex, System.IntPtr* bstrModuleName, System.Int32* cchModuleName, System.IntPtr* bstrModuleType, System.Int32* cchModuleType, System.IntPtr* bstrModulePrecondition, System.Int32* cchModulePrecondition)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetNextModule' has not been implemented!");
		}

		public static System.Int32 MgdSetNativeConfiguration(System.IntPtr nativeConfig)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetNativeConfiguration' has not been implemented!");
		}

		public static System.Void MgdGetCurrentNotificationInfo(System.IntPtr pHandler, System.Int32* currentModuleIndex, System.Boolean* isPostNotification, System.Int32* currentNotification)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetCurrentNotificationInfo' has not been implemented!");
		}

		public static System.Int32 MgdGetServerVarChanges(System.IntPtr pRequestContext, System.Int32* count, System.IntPtr* names, System.IntPtr* values, System.Int32* diffCount, System.IntPtr* diffIndicies)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetServerVarChanges' has not been implemented!");
		}

		public static System.Int32 MgdGetServerVariableA(System.IntPtr pHandler, System.String pszVarName, System.IntPtr* ppBuffer, System.Int32* pcchBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetServerVariableA' has not been implemented!");
		}

		public static System.Void MgdSetBadRequestStatus(System.IntPtr pHandler)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetBadRequestStatus' has not been implemented!");
		}

		public static System.Int32 MgdSetStatusW(System.IntPtr pRequestContext, System.Int32 dwStatusCode, System.Int32 dwSubStatusCode, System.String pszReason, System.String pszErrorDescription, System.Boolean fTrySkipCustomErrors)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetStatusW' has not been implemented!");
		}

		public static System.Int32 MgdSetKernelCachePolicy(System.IntPtr pHandler, System.Int32 secondsToLive)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetKernelCachePolicy' has not been implemented!");
		}

		public static System.Int32 MgdFlushKernelCache(System.String cacheKey)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdFlushKernelCache' has not been implemented!");
		}

		public static System.Void MgdDisableKernelCache(System.IntPtr pHandler)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdDisableKernelCache' has not been implemented!");
		}

		public static System.Int32 MgdInsertEntityBody(System.IntPtr pHandler, System.Byte[] buffer, System.Int32 offset, System.Int32 count)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdInsertEntityBody' has not been implemented!");
		}

		public static System.Int32 MgdPostCompletion(System.IntPtr pHandler, System.Web.RequestNotificationStatus notificationStatus)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdPostCompletion' has not been implemented!");
		}

		public static System.Int32 MgdSyncReadRequest(System.IntPtr pHandler, System.Byte[] pBuffer, System.Int32 offset, System.Int32 cbBuffer, System.Int32* pBytesRead)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSyncReadRequest' has not been implemented!");
		}

		public static System.Int32 MgdGetVirtualToken(System.IntPtr pHandler, System.IntPtr* pToken)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetVirtualToken' has not been implemented!");
		}

		public static System.Boolean MgdIsClientConnected(System.IntPtr pHandler)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdIsClientConnected' has not been implemented!");
		}

		public static System.Void MgdCloseConnection(System.IntPtr pHandler)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdCloseConnection' has not been implemented!");
		}

		public static System.Int32 MgdGetPreloadedContent(System.IntPtr pHandler, System.Byte[] pBuffer, System.Int32 lOffset, System.Int32 cbLen, System.Int32* pcbReceived)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetPreloadedContent' has not been implemented!");
		}

		public static System.Int32 MgdGetPreloadedSize(System.IntPtr pHandler, System.Int32* pcbAvailable)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetPreloadedSize' has not been implemented!");
		}

		public static System.Int32 MgdIsInRole(System.IntPtr pHandler, System.String pszRoleName, System.Boolean* pfIsInRole)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdIsInRole' has not been implemented!");
		}

		public static System.IntPtr MgdAllocateRequestMemory(System.IntPtr pHandler, System.Int32 cbSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdAllocateRequestMemory' has not been implemented!");
		}

		public static System.Int32 MgdGetLocalPort(System.IntPtr context)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetLocalPort' has not been implemented!");
		}

		public static System.Int32 MgdGetRemotePort(System.IntPtr context)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetRemotePort' has not been implemented!");
		}

		public static System.Int32 MgdGetCookieHeader(System.IntPtr pRequestContext, System.IntPtr* pBuffer, System.Int32* cbBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetCookieHeader' has not been implemented!");
		}

		public static System.Int32 MgdRewriteUrl(System.IntPtr pRequestContext, System.String pszUrl, System.Boolean fResetQueryString)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdRewriteUrl' has not been implemented!");
		}

		public static System.Int32 MgdGetMaxConcurrentRequestsPerCPU()
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetMaxConcurrentRequestsPerCPU' has not been implemented!");
		}

		public static System.Int32 MgdGetMaxConcurrentThreadsPerCPU()
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetMaxConcurrentThreadsPerCPU' has not been implemented!");
		}

		public static System.Int32 MgdSetMaxConcurrentRequestsPerCPU(System.Int32 value)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetMaxConcurrentRequestsPerCPU' has not been implemented!");
		}

		public static System.Int32 MgdSetMaxConcurrentThreadsPerCPU(System.Int32 value)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetMaxConcurrentThreadsPerCPU' has not been implemented!");
		}

		public static System.Int32 MgdGetCurrentModuleName(System.IntPtr pHandler, System.IntPtr* pBuffer, System.Int32* cbBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetCurrentModuleName' has not been implemented!");
		}

		public static System.Int32 MgdGetCurrentNotification(System.IntPtr pRequestContext)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetCurrentNotification' has not been implemented!");
		}

		public static System.Int32 MgdClearResponse(System.IntPtr pRequestContext, System.Boolean fClearEntity, System.Boolean fClearHeaders)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdClearResponse' has not been implemented!");
		}

		public static System.Int32 MgdCreateNativeConfigSystem(System.IntPtr* ppConfigSystem)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdCreateNativeConfigSystem' has not been implemented!");
		}

		public static System.Int32 MgdReleaseNativeConfigSystem(System.IntPtr pConfigSystem)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdReleaseNativeConfigSystem' has not been implemented!");
		}

		public static System.Int32 MgdGetRequestTraceGuid(System.IntPtr pRequestContext, System.Guid* traceContextId)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetRequestTraceGuid' has not been implemented!");
		}

		public static System.Int32 MgdGetStatusChanges(System.IntPtr pRequestContext, System.UInt16* statusCode, System.UInt16* subStatusCode, System.IntPtr* pBuffer, System.UInt16* cbBufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetStatusChanges' has not been implemented!");
		}

		public static System.Int32 MgdEtwGetTraceConfig(System.IntPtr pRequestContext, System.Boolean* providerEnabled, System.Int32* flags, System.Int32* level)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdEtwGetTraceConfig' has not been implemented!");
		}

		public static System.Int32 MgdEmitSimpleTrace(System.IntPtr pRequestContext, System.Int32 type, System.String eventData)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdEmitSimpleTrace' has not been implemented!");
		}

		public static System.Int32 MgdEmitWebEventTrace(System.IntPtr pRequestContext, System.Int32 webEventType, System.Int32 fieldCount, System.String[] fieldNames, System.Int32[] fieldTypes, System.String[] fieldData)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdEmitWebEventTrace' has not been implemented!");
		}

		public static System.Int32 MgdSetRequestPrincipal(System.IntPtr pRequestContext, System.IntPtr pManagedPrincipal, System.String userName, System.String authType, System.IntPtr token)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetRequestPrincipal' has not been implemented!");
		}

		public static System.Boolean MgdIsWithinApp(System.IntPtr pConfigSystem, System.String siteName, System.String appPath, System.String virtualPath)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdIsWithinApp' has not been implemented!");
		}

		public static System.Int32 MgdGetSiteNameFromId(System.IntPtr pConfigSystem, System.UInt32 siteId, System.IntPtr* bstrSiteName, System.Int32* cchSiteName)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetSiteNameFromId' has not been implemented!");
		}

		public static System.Int32 MgdGetAppPathForPath(System.IntPtr pConfigSystem, System.UInt32 siteId, System.String virtualPath, System.IntPtr* bstrPath, System.Int32* cchPath)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetAppPathForPath' has not been implemented!");
		}

		public static System.Int32 MgdGetModuleCollection(System.IntPtr pConfigSystem, System.IntPtr appContext, System.IntPtr* pModuleCollection, System.Int32* count)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetModuleCollection' has not been implemented!");
		}

		public static System.Int32 MgdGetVrPathCreds(System.IntPtr pConfigSystem, System.String siteName, System.String virtualPath, System.IntPtr* bstrUserName, System.Int32* cchUserName, System.IntPtr* bstrPassword, System.Int32* cchPassword)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetVrPathCreds' has not been implemented!");
		}

		public static System.Int32 MgdGetAppCollection(System.IntPtr pConfigSystem, System.String siteName, System.String virtualPath, System.IntPtr* bstrPath, System.Int32* cchPath, System.IntPtr* pAppCollection, System.Int32* count)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetAppCollection' has not been implemented!");
		}

		public static System.Int32 MgdGetNextVPath(System.IntPtr pAppCollection, System.UInt32 dwIndex, System.IntPtr* bstrPath, System.Int32* cchPath)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetNextVPath' has not been implemented!");
		}

		public static System.Int32 MgdInitNativeConfig()
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdInitNativeConfig' has not been implemented!");
		}

		public static System.Void MgdTerminateNativeConfig()
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdTerminateNativeConfig' has not been implemented!");
		}

		public static System.Int32 MgdMapPathDirect(System.IntPtr pConfigSystem, System.String siteName, System.String virtualPath, System.IntPtr* bstrPhysicalPath, System.Int32* cchPath)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdMapPathDirect' has not been implemented!");
		}

		public static System.Int32 MgdMapHandler(System.IntPtr pHandler, System.String method, System.String virtualPath, System.IntPtr* ppszTypeString, System.Int32* pcchTypeString, System.Boolean convertNativeStaticFileModule)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdMapHandler' has not been implemented!");
		}

		public static System.Int32 MgdReMapHandler(System.IntPtr pHandler, System.String pszVirtualPath, System.IntPtr* ppszTypeString, System.Int32* pcchTypeString, System.Boolean* pfHandlerExists)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdReMapHandler' has not been implemented!");
		}

		public static System.Int32 MgdSetRemapHandler(System.IntPtr pHandler, System.String pszName, System.String ppszType)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetRemapHandler' has not been implemented!");
		}

		public static System.UInt32 MgdResolveSiteName(System.IntPtr pConfigSystem, System.String siteName)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdResolveSiteName' has not been implemented!");
		}

		public static System.Void MgdSetResponseFilter(System.IntPtr context)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetResponseFilter' has not been implemented!");
		}

		public static System.Int32 MgdGetFileChunkInfo(System.IntPtr context, System.Int32 chunkOffset, System.Int64* offset, System.Int64* length)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetFileChunkInfo' has not been implemented!");
		}

		public static System.Int32 MgdReadChunkHandle(System.IntPtr context, System.IntPtr FileHandle, System.Int64 startOffset, System.Int32* length, System.IntPtr chunkEntity)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdReadChunkHandle' has not been implemented!");
		}

		public static System.Int32 MgdExplicitFlush(System.IntPtr context)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdExplicitFlush' has not been implemented!");
		}

		public static System.Int32 MgdSetServerVariableW(System.IntPtr context, System.String variableName, System.String variableValue)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdSetServerVariableW' has not been implemented!");
		}

		public static System.Int32 MgdExecuteUrl(System.IntPtr context, System.String url, System.Boolean resetQuerystring, System.Boolean preserveForm, System.Byte[] entityBody, System.UInt32 entityBodySize, System.String method, System.Int32 numHeaders, System.String[] headersNames, System.String[] headersValues, System.Boolean preserveUser)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdExecuteUrl' has not been implemented!");
		}

		public static System.Int32 MgdGetClientCertificate(System.IntPtr pHandler, System.IntPtr* ppbClientCert, System.Int32* pcbClientCert, System.IntPtr* ppbClientCertIssuer, System.Int32* pcbClientCertIssuer, System.IntPtr* ppbClientCertPublicKey, System.Int32* pcbClientCertPublicKey, System.UInt32* pdwCertEncodingType, System.Int64* ftNotBefore, System.Int64* ftNotAfter)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetClientCertificate' has not been implemented!");
		}

		public static System.Int32 MgdGetChannelBindingToken(System.IntPtr pHandler, System.IntPtr* ppbToken, System.Int32* pcbTokenSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.Hosting.UnsafeIISMethods.MgdGetChannelBindingToken' has not been implemented!");
		}
	}
}
