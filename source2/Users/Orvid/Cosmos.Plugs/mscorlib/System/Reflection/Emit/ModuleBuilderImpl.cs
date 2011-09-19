namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Reflection.Emit.ModuleBuilder), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Reflection_Emit_ModuleBuilderImpl
	{

		public static System.IntPtr nCreateISymWriterForDynamicModule(System.Reflection.Module module, System.String filename)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.nCreateISymWriterForDynamicModule' has not been implemented!");
		}

		public static System.Void SetFieldRVAContent(System.Reflection.RuntimeModule module, System.Int32 fdToken, System.Byte[] data, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.SetFieldRVAContent' has not been implemented!");
		}

		public static System.Int32 GetTypeRef(System.Reflection.RuntimeModule module, System.String strFullName, System.Reflection.RuntimeModule refedModule, System.String strRefedModuleFileName, System.Int32 tkResolution)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.GetTypeRef' has not been implemented!");
		}

		public static System.Int32 GetMemberRef(System.Reflection.RuntimeModule module, System.Reflection.RuntimeModule refedModule, System.Int32 tr, System.Int32 defToken)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.GetMemberRef' has not been implemented!");
		}

		public static System.Int32 GetMemberRefFromSignature(System.Reflection.RuntimeModule module, System.Int32 tr, System.String methodName, System.Byte[] signature, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.GetMemberRefFromSignature' has not been implemented!");
		}

		public static System.Int32 GetMemberRefOfMethodInfo(System.Reflection.RuntimeModule module, System.Int32 tr, System.IRuntimeMethodInfo method)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.GetMemberRefOfMethodInfo' has not been implemented!");
		}

		public static System.Int32 GetMemberRefOfFieldInfo(System.Reflection.RuntimeModule module, System.Int32 tkType, System.RuntimeTypeHandle declaringType, System.Int32 tkField)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.GetMemberRefOfFieldInfo' has not been implemented!");
		}

		public static System.Int32 GetTokenFromTypeSpec(System.Reflection.RuntimeModule pModule, System.Byte[] signature, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.GetTokenFromTypeSpec' has not been implemented!");
		}

		public static System.Int32 GetArrayMethodToken(System.Reflection.RuntimeModule module, System.Int32 tkTypeSpec, System.String methodName, System.Byte[] signature, System.Int32 sigLength)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.GetArrayMethodToken' has not been implemented!");
		}

		public static System.Int32 GetStringConstant(System.Reflection.RuntimeModule module, System.String str, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.GetStringConstant' has not been implemented!");
		}

		public static System.Void PreSavePEFile(System.Reflection.RuntimeModule module, System.Int32 portableExecutableKind, System.Int32 imageFileMachine)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.PreSavePEFile' has not been implemented!");
		}

		public static System.Void SavePEFile(System.Reflection.RuntimeModule module, System.String fileName, System.Int32 entryPoint, System.Int32 isExe, System.Boolean isManifestFile)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.SavePEFile' has not been implemented!");
		}

		public static System.Void AddResource(System.Reflection.RuntimeModule module, System.String strName, System.Byte[] resBytes, System.Int32 resByteCount, System.Int32 tkFile, System.Int32 attribute, System.Int32 portableExecutableKind, System.Int32 imageFileMachine)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.AddResource' has not been implemented!");
		}

		public static System.Void SetModuleName(System.Reflection.RuntimeModule module, System.String strModuleName)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.SetModuleName' has not been implemented!");
		}

		public static System.Void DefineNativeResourceFile(System.Reflection.RuntimeModule module, System.String strFilename, System.Int32 portableExecutableKind, System.Int32 ImageFileMachine)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.DefineNativeResourceFile' has not been implemented!");
		}

		public static System.Void DefineNativeResourceBytes(System.Reflection.RuntimeModule module, System.Byte[] pbResource, System.Int32 cbResource, System.Int32 portableExecutableKind, System.Int32 imageFileMachine)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.ModuleBuilder.DefineNativeResourceBytes' has not been implemented!");
		}
	}
}
