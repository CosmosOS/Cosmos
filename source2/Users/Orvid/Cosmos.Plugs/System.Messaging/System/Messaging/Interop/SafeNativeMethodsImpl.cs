namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Messaging.Interop.SafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Messaging_Interop_SafeNativeMethodsImpl
	{

		public static System.Int32 MQCloseQueue(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.MQCloseQueue' has not been implemented!");
		}

		public static System.Int32 MQCloseCursor(System.IntPtr cursorHandle)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.MQCloseCursor' has not been implemented!");
		}

		public static System.Void MQFreeSecurityContext(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.MQFreeSecurityContext' has not been implemented!");
		}

		public static System.Int32 MQLocateEnd(System.IntPtr enumHandle)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.MQLocateEnd' has not been implemented!");
		}

		public static System.Int32 IntMQBeginTransaction(System.Messaging.Interop.ITransaction* refTransaction)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.IntMQBeginTransaction' has not been implemented!");
		}

		public static System.Int32 IntMQPathNameToFormatName(System.String pathName, System.Text.StringBuilder formatName, System.Int32* count)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.IntMQPathNameToFormatName' has not been implemented!");
		}

		public static System.Int32 IntMQInstanceToFormatName(System.Byte[] id, System.Text.StringBuilder formatName, System.Int32* count)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.IntMQInstanceToFormatName' has not been implemented!");
		}

		public static System.Int32 MQCreateCursor(System.Messaging.Interop.MessageQueueHandle handle, System.Messaging.Interop.CursorHandle* cursorHandle)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.MQCreateCursor' has not been implemented!");
		}

		public static System.Int32 MQLocateNext(System.Messaging.Interop.LocatorHandle enumHandle, System.Int32* propertyCount, System.Messaging.Interop.MQPROPVARIANTS[] variantArray)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.MQLocateNext' has not been implemented!");
		}

		public static System.Void MQFreeMemory(System.IntPtr memory)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.MQFreeMemory' has not been implemented!");
		}

		public static System.Boolean GetHandleInformation(System.Runtime.InteropServices.SafeHandle handle, System.Int32* handleInformation)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.GetHandleInformation' has not been implemented!");
		}

		public static System.IntPtr LocalFree(System.IntPtr hMem)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.LocalFree' has not been implemented!");
		}

		public static System.Int32 SetEntriesInAclW(System.Int32 count, System.IntPtr entries, System.IntPtr oldacl, System.IntPtr* newAcl)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.SetEntriesInAclW' has not been implemented!");
		}

		public static System.Boolean GetComputerName(System.Text.StringBuilder lpBuffer, System.Int32[] nSize)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.GetComputerName' has not been implemented!");
		}

		public static System.Int32 FormatMessage(System.Int32 dwFlags, System.IntPtr lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr arguments)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.SafeNativeMethods.FormatMessage' has not been implemented!");
		}
	}
}
