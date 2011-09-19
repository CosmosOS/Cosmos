namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Diagnostics.Eventing.ManifestEtw), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Diagnostics_Eventing_EventProvider+ManifestEtwImpl
	{

		public static System.UInt32 EventRegister(System.Guid* providerId, System.Diagnostics.Eventing.EventProvider+ManifestEtw+EtwEnableCallback enableCallback, System.Void* callbackContext, System.Int64* registrationHandle)
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Eventing.EventProvider+ManifestEtw.EventRegister' has not been implemented!");
		}

		public static System.UInt32 EventUnregister(System.Int64 registrationHandle)
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Eventing.EventProvider+ManifestEtw.EventUnregister' has not been implemented!");
		}

		public static System.UInt32 EventWrite(System.Int64 registrationHandle, System.Diagnostics.Eventing.EventDescriptorInternal* eventDescriptor, System.UInt32 userDataCount, System.Diagnostics.Eventing.EventProvider+EventData* userData)
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Eventing.EventProvider+ManifestEtw.EventWrite' has not been implemented!");
		}

		public static System.UInt32 EventWriteTransfer(System.Int64 registrationHandle, System.Diagnostics.Eventing.EventDescriptorInternal* eventDescriptor, System.Guid* activityId, System.Guid* relatedActivityId, System.UInt32 userDataCount, System.Diagnostics.Eventing.EventProvider+EventData* userData)
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Eventing.EventProvider+ManifestEtw.EventWriteTransfer' has not been implemented!");
		}

		public static System.UInt32 EventWriteString(System.Int64 registrationHandle, System.Byte level, System.Int64 keywords, System.Char* message)
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Eventing.EventProvider+ManifestEtw.EventWriteString' has not been implemented!");
		}

		public static System.UInt32 EventActivityIdControl(System.Int32 ControlCode, System.Guid* ActivityId)
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Eventing.EventProvider+ManifestEtw.EventActivityIdControl' has not been implemented!");
		}
	}
}
