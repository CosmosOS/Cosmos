namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Messaging.Interop.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Messaging_Interop_UnsafeNativeMethodsImpl
	{

		public static System.Int32 IntMQOpenQueue(System.String formatName, System.Int32 access, System.Int32 shareMode, System.Messaging.Interop.MessageQueueHandle* handle)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQOpenQueue' has not been implemented!");
		}

		public static System.Int32 MQSendMessage(System.Messaging.Interop.MessageQueueHandle handle, System.Messaging.Interop.MessagePropertyVariants+MQPROPS properties, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.MQSendMessage' has not been implemented!");
		}

		public static System.Int32 MQSendMessage(System.Messaging.Interop.MessageQueueHandle handle, System.Messaging.Interop.MessagePropertyVariants+MQPROPS properties, System.Messaging.Interop.ITransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.MQSendMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessage(System.Messaging.Interop.MessageQueueHandle handle, System.UInt32 timeout, System.Int32 action, System.Messaging.Interop.MessagePropertyVariants+MQPROPS properties, System.Threading.NativeOverlapped* overlapped, System.Messaging.Interop.SafeNativeMethods+ReceiveCallback receiveCallback, System.Messaging.Interop.CursorHandle cursorHandle, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessage(System.Messaging.Interop.MessageQueueHandle handle, System.UInt32 timeout, System.Int32 action, System.Messaging.Interop.MessagePropertyVariants+MQPROPS properties, System.Threading.NativeOverlapped* overlapped, System.Messaging.Interop.SafeNativeMethods+ReceiveCallback receiveCallback, System.Messaging.Interop.CursorHandle cursorHandle, System.Messaging.Interop.ITransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.MQReceiveMessage' has not been implemented!");
		}

		public static System.Int32 IntMQReceiveMessageByLookupId(System.Messaging.Interop.MessageQueueHandle handle, System.Int64 lookupId, System.Int32 action, System.Messaging.Interop.MessagePropertyVariants+MQPROPS properties, System.Threading.NativeOverlapped* overlapped, System.Messaging.Interop.SafeNativeMethods+ReceiveCallback receiveCallback, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQReceiveMessageByLookupId' has not been implemented!");
		}

		public static System.Int32 IntMQReceiveMessageByLookupId(System.Messaging.Interop.MessageQueueHandle handle, System.Int64 lookupId, System.Int32 action, System.Messaging.Interop.MessagePropertyVariants+MQPROPS properties, System.Threading.NativeOverlapped* overlapped, System.Messaging.Interop.SafeNativeMethods+ReceiveCallback receiveCallback, System.Messaging.Interop.ITransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQReceiveMessageByLookupId' has not been implemented!");
		}

		public static System.Int32 IntMQCreateQueue(System.IntPtr securityDescriptor, System.Messaging.Interop.MessagePropertyVariants+MQPROPS queueProperties, System.Text.StringBuilder formatName, System.Int32* formatNameLength)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQCreateQueue' has not been implemented!");
		}

		public static System.Int32 IntMQDeleteQueue(System.String formatName)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQDeleteQueue' has not been implemented!");
		}

		public static System.Int32 IntMQLocateBegin(System.String context, System.Messaging.Interop.Restrictions+MQRESTRICTION Restriction, System.Messaging.Interop.Columns+MQCOLUMNSET columnSet, System.IntPtr sortSet, System.Messaging.Interop.LocatorHandle* enumHandle)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQLocateBegin' has not been implemented!");
		}

		public static System.Int32 IntMQGetMachineProperties(System.String machineName, System.IntPtr machineIdPointer, System.Messaging.Interop.MessagePropertyVariants+MQPROPS machineProperties)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQGetMachineProperties' has not been implemented!");
		}

		public static System.Int32 IntMQGetQueueProperties(System.String formatName, System.Messaging.Interop.MessagePropertyVariants+MQPROPS queueProperties)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQGetQueueProperties' has not been implemented!");
		}

		public static System.Int32 IntMQMgmtGetInfo(System.String machineName, System.String objectName, System.Messaging.Interop.MessagePropertyVariants+MQPROPS queueProperties)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQMgmtGetInfo' has not been implemented!");
		}

		public static System.Int32 MQPurgeQueue(System.Messaging.Interop.MessageQueueHandle handle)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.MQPurgeQueue' has not been implemented!");
		}

		public static System.Int32 IntMQSetQueueProperties(System.String formatName, System.Messaging.Interop.MessagePropertyVariants+MQPROPS queueProperties)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.IntMQSetQueueProperties' has not been implemented!");
		}

		public static System.Int32 MQGetQueueSecurity(System.String formatName, System.Int32 SecurityInformation, System.IntPtr SecurityDescriptor, System.Int32 length, System.Int32* lengthNeeded)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.MQGetQueueSecurity' has not been implemented!");
		}

		public static System.Int32 MQSetQueueSecurity(System.String formatName, System.Int32 SecurityInformation, System.Messaging.Interop.NativeMethods+SECURITY_DESCRIPTOR SecurityDescriptor)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.MQSetQueueSecurity' has not been implemented!");
		}

		public static System.Boolean GetSecurityDescriptorDacl(System.IntPtr pSD, System.Boolean* daclPresent, System.IntPtr* pDacl, System.Boolean* daclDefaulted)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.GetSecurityDescriptorDacl' has not been implemented!");
		}

		public static System.Boolean SetSecurityDescriptorDacl(System.Messaging.Interop.NativeMethods+SECURITY_DESCRIPTOR pSD, System.Boolean daclPresent, System.IntPtr pDacl, System.Boolean daclDefaulted)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.SetSecurityDescriptorDacl' has not been implemented!");
		}

		public static System.Boolean InitializeSecurityDescriptor(System.Messaging.Interop.NativeMethods+SECURITY_DESCRIPTOR SD, System.Int32 revision)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.InitializeSecurityDescriptor' has not been implemented!");
		}

		public static System.Boolean LookupAccountName(System.String lpSystemName, System.String lpAccountName, System.IntPtr sid, System.Int32* sidSize, System.Text.StringBuilder DomainName, System.Int32* DomainSize, System.Int32* pUse)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.UnsafeNativeMethods.LookupAccountName' has not been implemented!");
		}
	}
}
