namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Messaging.Interop.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Messaging_Interop_NativeMethodsImpl
	{

		public static System.Int32 IntMQGetSecurityContextEx(System.IntPtr lpCertBuffer, System.Int32 dwCertBufferLength, System.Messaging.Interop.SecurityContextHandle* phSecurityContext)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.NativeMethods.IntMQGetSecurityContextEx' has not been implemented!");
		}

		public static System.Object OleLoadFromStream(System.Messaging.Interop.IStream stream, System.Guid* iid)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.NativeMethods.OleLoadFromStream' has not been implemented!");
		}

		public static System.Void OleSaveToStream(System.Messaging.Interop.IPersistStream persistStream, System.Messaging.Interop.IStream stream)
		{
			throw new System.NotImplementedException("Method 'System.Messaging.Interop.NativeMethods.OleSaveToStream' has not been implemented!");
		}
	}
}
