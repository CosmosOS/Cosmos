namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Signature), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_SignatureImpl
	{

		public static System.Void GetSignature(System.SignatureStruct* signature, System.Void* pCorSig, System.Int32 cCorSig, System.RuntimeFieldHandleInternal fieldHandle, System.IRuntimeMethodInfo methodHandle, System.RuntimeType declaringType)
		{
			throw new System.NotImplementedException("Method 'System.Signature.GetSignature' has not been implemented!");
		}

		public static System.Void GetCustomModifiers(System.SignatureStruct* signature, System.Int32 parameter, System.Type[]* required, System.Type[]* optional)
		{
			throw new System.NotImplementedException("Method 'System.Signature.GetCustomModifiers' has not been implemented!");
		}

		public static System.Boolean CompareSig(System.SignatureStruct* left, System.SignatureStruct* right)
		{
			throw new System.NotImplementedException("Method 'System.Signature.CompareSig' has not been implemented!");
		}
	}
}
