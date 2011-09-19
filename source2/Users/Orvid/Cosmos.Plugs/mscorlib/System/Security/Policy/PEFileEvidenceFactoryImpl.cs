namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Policy.PEFileEvidenceFactory), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Policy_PEFileEvidenceFactoryImpl
	{

		public static System.Void FireEvidenceGeneratedEvent(Microsoft.Win32.SafeHandles.SafePEFileHandle peFile, System.Security.Policy.EvidenceTypeGenerated type)
		{
			throw new System.NotImplementedException("Method 'System.Security.Policy.PEFileEvidenceFactory.FireEvidenceGeneratedEvent' has not been implemented!");
		}

		public static System.Void GetLocationEvidence(Microsoft.Win32.SafeHandles.SafePEFileHandle peFile, System.Security.SecurityZone* zone, System.Runtime.CompilerServices.StringHandleOnStack retUrl)
		{
			throw new System.NotImplementedException("Method 'System.Security.Policy.PEFileEvidenceFactory.GetLocationEvidence' has not been implemented!");
		}

		public static System.Void GetPublisherCertificate(Microsoft.Win32.SafeHandles.SafePEFileHandle peFile, System.Runtime.CompilerServices.ObjectHandleOnStack retCertificate)
		{
			throw new System.NotImplementedException("Method 'System.Security.Policy.PEFileEvidenceFactory.GetPublisherCertificate' has not been implemented!");
		}

		public static System.Void GetAssemblySuppliedEvidence(Microsoft.Win32.SafeHandles.SafePEFileHandle peFile, System.Runtime.CompilerServices.ObjectHandleOnStack retSerializedEvidence)
		{
			throw new System.NotImplementedException("Method 'System.Security.Policy.PEFileEvidenceFactory.GetAssemblySuppliedEvidence' has not been implemented!");
		}
	}
}
