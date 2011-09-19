namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IO.Log.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IO_Log_UnsafeNativeMethodsImpl
	{

		public static Microsoft.Win32.SafeHandles.SafeFileHandle _CreateLogFile(System.String pszLogFileName, System.Int32 fDesiredAccess, System.IO.FileShare dwShareMode, System.IO.Log.SECURITY_ATTRIBUTES psaLogFile, System.IO.FileMode fCreateDisposition, System.Int32 fFlagsAndAttributes)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._CreateLogFile' has not been implemented!");
		}

		public static System.Boolean _DeleteLogFile(System.String pszLogFileName, System.IO.Log.SECURITY_ATTRIBUTES psaSecurityAttributes)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._DeleteLogFile' has not been implemented!");
		}

		public static System.Boolean _GetLogFileInformation(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_INFORMATION* pinfoBuffer, System.Int32* cbBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._GetLogFileInformation' has not been implemented!");
		}

		public static System.Boolean _FlushLogToLsnSync(System.IO.Log.SafeMarshalContext pvMarshalContext, System.UInt64* plsnFlush, System.UInt64* plsnLastFlushed, System.Threading.NativeOverlapped* overlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._FlushLogToLsnSync' has not been implemented!");
		}

		public static System.Boolean _FlushLogToLsnAsync(System.IO.Log.SafeMarshalContext pvMarshalContext, System.UInt64* plsnFlush, System.IntPtr plsnLastFlushed, System.Threading.NativeOverlapped* overlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._FlushLogToLsnAsync' has not been implemented!");
		}

		public static System.Boolean _CreateLogMarshallingArea(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IntPtr pfnAllocBuffer, System.IntPtr pfnFreeBuffer, System.IntPtr pvBlockAllocContext, System.Int32 cbMarshallingBlock, System.Int32 cMaxWriteBlocks, System.Int32 cMaxReadBlocks, System.IO.Log.SafeMarshalContext* ppvMarshal)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._CreateLogMarshallingArea' has not been implemented!");
		}

		public static System.Boolean _AlignReservedLogSingle(System.IO.Log.SafeMarshalContext pvMarshal, System.Int32 cReservedRecords, System.Int64* rgcbReservation, System.Int64* pcbAlignReservation)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._AlignReservedLogSingle' has not been implemented!");
		}

		public static System.Boolean _AllocReservedLog(System.IO.Log.SafeMarshalContext pvMarshal, System.Int32 cReservedRecords, System.Int64* pcbAdjustment)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._AllocReservedLog' has not been implemented!");
		}

		public static System.Boolean _FreeReservedLog(System.IO.Log.SafeMarshalContext pvMarshal, System.UInt32 cReservedRecords, System.Int64* pcbAdjustment)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._FreeReservedLog' has not been implemented!");
		}

		public static System.Boolean _ReadLogRestartArea(System.IO.Log.SafeMarshalContext pvMarshal, System.Byte** ppvRestartBuffer, System.Int32* pcbRestartBuffer, System.UInt64* plsn, System.IO.Log.SafeReadContext* ppvContext, System.Threading.NativeOverlapped* overlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._ReadLogRestartArea' has not been implemented!");
		}

		public static System.Boolean _ReadPreviousLogRestartArea(System.IO.Log.SafeReadContext pvReadContext, System.Byte** ppvRestartBuffer, System.Int32* pcbRestartBuffer, System.UInt64* plsnRestart, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._ReadPreviousLogRestartArea' has not been implemented!");
		}

		public static System.Boolean _ReadLogRecord(System.IO.Log.SafeMarshalContext pvMarshal, System.UInt64* plsnFirst, System.IO.Log.CLFS_CONTEXT_MODE ecxMode, System.Byte** ppvReadBuffer, System.Int32* pcbReadBuffer, System.Byte* peRecordType, System.UInt64* plsnUndoNext, System.UInt64* plsnPrevious, System.IO.Log.SafeReadContext* ppvReadContext, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._ReadLogRecord' has not been implemented!");
		}

		public static System.Boolean _ReadNextLogRecord(System.IO.Log.SafeReadContext pvReadContext, System.Byte** ppvReadBuffer, System.Int32* pcbReadBuffer, System.Byte* peRecordType, System.IntPtr plsnUser, System.UInt64* plsnUndoNext, System.UInt64* plsnPrevious, System.UInt64* plsnRecord, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._ReadNextLogRecord' has not been implemented!");
		}

		public static System.Boolean _ReserveAndAppendLog(System.IO.Log.SafeMarshalContext pvMarshal, System.IO.Log.CLFS_WRITE_ENTRY[] rgWriteEntries, System.Int32 cWriteEntries, System.UInt64* plsnUndoNext, System.UInt64* plsnPrevious, System.Int32 cReserveRecords, System.Int64[] rgcbReservation, System.Int32 fFlags, System.IntPtr plsn, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._ReserveAndAppendLog' has not been implemented!");
		}

		public static System.Boolean _WriteLogRestartArea(System.IO.Log.SafeMarshalContext pvMarshal, System.Byte* pvRestartBuffer, System.Int32 cbRestartBuffer, System.UInt64* plsnBase, System.Int32 fFlags, System.IntPtr pcbWritten, System.IntPtr plsnRestart, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._WriteLogRestartArea' has not been implemented!");
		}

		public static System.Boolean _WriteLogRestartAreaNoBase(System.IO.Log.SafeMarshalContext pvMarshal, System.Byte* pvRestartBuffer, System.Int32 cbRestartBuffer, System.IntPtr mustBeZero, System.Int32 fFlags, System.IntPtr pcbWritten, System.IntPtr plsnRestart, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._WriteLogRestartAreaNoBase' has not been implemented!");
		}

		public static System.Boolean _AddLogContainer(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.UInt64* pcbContainer, System.String pwszContainerPath, System.IntPtr pReserved)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._AddLogContainer' has not been implemented!");
		}

		public static System.Boolean _AddLogContainerNoSize(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IntPtr mbz, System.String pwszContainerPath, System.IntPtr pReserved)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._AddLogContainerNoSize' has not been implemented!");
		}

		public static System.Boolean _RemoveLogContainer(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.String pszContainerPath, System.Boolean fForce, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._RemoveLogContainer' has not been implemented!");
		}

		public static System.Boolean _CreateLogContainerScanContext(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.UInt32 cFromContainer, System.UInt32 cContainers, System.IO.Log.CLFS_SCAN_MODE eScanMode, System.IO.Log.CLFS_SCAN_CONTEXT* pcxScan, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._CreateLogContainerScanContext' has not been implemented!");
		}

		public static System.Boolean _ScanLogContainers(System.IO.Log.CLFS_SCAN_CONTEXT* pcxScan, System.IO.Log.CLFS_SCAN_MODE eScanMode, System.IntPtr pReserved)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._ScanLogContainers' has not been implemented!");
		}

		public static System.Boolean _SetLogArchiveTail(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.UInt64* plsnArchiveTail, System.Threading.NativeOverlapped* pverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._SetLogArchiveTail' has not been implemented!");
		}

		public static System.Boolean _SetEndOfLog(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.UInt64* plsnEnd, System.Threading.NativeOverlapped* lpOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._SetEndOfLog' has not been implemented!");
		}

		public static System.Boolean _TruncateLog(System.IO.Log.SafeMarshalContext pvMarshal, System.UInt64* plsnEnd, System.Threading.NativeOverlapped* lpOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._TruncateLog' has not been implemented!");
		}

		public static System.Boolean _PrepareLogArchive(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.Text.StringBuilder pszBaseLogFileName, System.Int32 cLen, System.UInt64* pLsnLow, System.UInt64* pLsnHigh, System.Int32* pcActualLength, System.UInt64* poffBaseLogFileData, System.UInt64* pcbBaseLogFileLength, System.UInt64* plsnBase, System.UInt64* plsnLast, System.UInt64* plsnCurrentArchiveTail, System.IO.Log.SafeArchiveContext* ppvArchiveContext)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._PrepareLogArchive' has not been implemented!");
		}

		public static System.Boolean _ReadLogArchiveMetadata(System.IO.Log.SafeArchiveContext pvArchiveContext, System.Int32 cbOffset, System.Int32 cbBytesToRead, System.Byte* pbReadBuffer, System.UInt32* pcbBytesRead)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._ReadLogArchiveMetadata' has not been implemented!");
		}

		public static System.Boolean _GetNextLogArchiveExtent(System.IO.Log.SafeArchiveContext pvArchiveContext, System.IO.Log.CLFS_ARCHIVE_DESCRIPTOR* rgadExtent, System.Int32 cDescriptors, System.Int32* pcDescriptorsReturned)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._GetNextLogArchiveExtent' has not been implemented!");
		}

		public static System.Boolean _GetLogContainerName(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.Int32 cidLogicalContainer, System.Text.StringBuilder pwstrContainerName, System.Int32 cLenContainerName, System.Int32* pcActualLenContainerName)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._GetLogContainerName' has not been implemented!");
		}

		public static System.Boolean _SetLogArchiveMode(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_LOG_ARCHIVE_MODE eNewMode)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._SetLogArchiveMode' has not been implemented!");
		}

		public static System.Boolean _QueryLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_TYPE ePolicyType, System.IO.Log.CLFS_MGMT_POLICY_MAXIMUMSIZE* buffer, System.UInt32* pcbPolicyBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._QueryLogPolicy' has not been implemented!");
		}

		public static System.Boolean _QueryLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_TYPE ePolicyType, System.IO.Log.CLFS_MGMT_POLICY_MINIMUMSIZE* buffer, System.UInt32* pcbPolicyBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._QueryLogPolicy' has not been implemented!");
		}

		public static System.Boolean _QueryLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_TYPE ePolicyType, System.IO.Log.CLFS_MGMT_POLICY_GROWTHRATE* buffer, System.UInt32* pcbPolicyBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._QueryLogPolicy' has not been implemented!");
		}

		public static System.Boolean _QueryLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_TYPE ePolicyType, System.IO.Log.CLFS_MGMT_POLICY_LOGTAIL* buffer, System.UInt32* pcbPolicyBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._QueryLogPolicy' has not been implemented!");
		}

		public static System.Boolean _QueryLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_TYPE ePolicyType, System.IO.Log.CLFS_MGMT_POLICY_AUTOSHRINK* buffer, System.UInt32* pcbPolicyBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._QueryLogPolicy' has not been implemented!");
		}

		public static System.Boolean _QueryLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_TYPE ePolicyType, System.IO.Log.CLFS_MGMT_POLICY_AUTOGROW* buffer, System.UInt32* pcbPolicyBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._QueryLogPolicy' has not been implemented!");
		}

		public static System.Boolean _QueryLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_TYPE ePolicyType, System.IO.Log.CLFS_MGMT_POLICY_NEWCONTAINERPREFIX* buffer, System.UInt32* pcbPolicyBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._QueryLogPolicy' has not been implemented!");
		}

		public static System.Boolean _QueryLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_TYPE ePolicyType, System.IO.Log.CLFS_MGMT_POLICY_NEXTCONTAINERSUFFIX* buffer, System.UInt32* pcbPolicyBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._QueryLogPolicy' has not been implemented!");
		}

		public static System.Boolean _InstallLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_MAXIMUMSIZE* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._InstallLogPolicy' has not been implemented!");
		}

		public static System.Boolean _InstallLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_MINIMUMSIZE* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._InstallLogPolicy' has not been implemented!");
		}

		public static System.Boolean _InstallLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_GROWTHRATE* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._InstallLogPolicy' has not been implemented!");
		}

		public static System.Boolean _InstallLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_LOGTAIL* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._InstallLogPolicy' has not been implemented!");
		}

		public static System.Boolean _InstallLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_AUTOSHRINK* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._InstallLogPolicy' has not been implemented!");
		}

		public static System.Boolean _InstallLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_AUTOGROW* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._InstallLogPolicy' has not been implemented!");
		}

		public static System.Boolean _InstallLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_NEWCONTAINERPREFIX* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._InstallLogPolicy' has not been implemented!");
		}

		public static System.Boolean _InstallLogPolicy(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_POLICY_NEXTCONTAINERSUFFIX* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._InstallLogPolicy' has not been implemented!");
		}

		public static System.Boolean _HandleLogFull(Microsoft.Win32.SafeHandles.SafeFileHandle hLog)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._HandleLogFull' has not been implemented!");
		}

		public static System.Boolean _ReadLogNotification(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IO.Log.CLFS_MGMT_NOTIFICATION pNotification, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._ReadLogNotification' has not been implemented!");
		}

		public static System.Boolean _RegisterManageableLogClient(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.IntPtr pCallbacks)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._RegisterManageableLogClient' has not been implemented!");
		}

		public static System.Boolean _AdvanceLogBase(System.IO.Log.SafeMarshalContext pvMarshal, System.UInt64* plsnBase, System.Int32 fFlags, System.Threading.NativeOverlapped* pOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._AdvanceLogBase' has not been implemented!");
		}

		public static System.Boolean _LogTailAdvanceFailure(Microsoft.Win32.SafeHandles.SafeFileHandle hLog, System.Int32 reason)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.UnsafeNativeMethods._LogTailAdvanceFailure' has not been implemented!");
		}
	}
}
