namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Reflection.Emit.AssemblyBuilder), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Reflection_Emit_AssemblyBuilderImpl
	{

		public static System.Reflection.RuntimeModule GetInMemoryAssemblyModule(System.Reflection.RuntimeAssembly assembly)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.GetInMemoryAssemblyModule' has not been implemented!");
		}

		public static System.Reflection.RuntimeModule GetOnDiskAssemblyModule(System.Reflection.RuntimeAssembly assembly)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.GetOnDiskAssemblyModule' has not been implemented!");
		}

		public static System.Reflection.Assembly nCreateDynamicAssembly(System.AppDomain domain, System.Reflection.AssemblyName name, System.Security.Policy.Evidence identity, System.Threading.StackCrawlMark* stackMark, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions, System.Byte[] securityRulesBlob, System.Byte[] aptcaBlob, System.Reflection.Emit.AssemblyBuilderAccess access, System.Reflection.Emit.DynamicAssemblyFlags flags, System.Security.SecurityContextSource securityContextSource)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.nCreateDynamicAssembly' has not been implemented!");
		}

		public static System.Void DefineDynamicModule(System.Reflection.RuntimeAssembly containingAssembly, System.Boolean emitSymbolInfo, System.String name, System.String filename, System.Runtime.CompilerServices.StackCrawlMarkHandle stackMark, System.IntPtr* pInternalSymWriter, System.Runtime.CompilerServices.ObjectHandleOnStack retModule, System.Boolean fIsTransient, System.Int32* tkFile)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.DefineDynamicModule' has not been implemented!");
		}

		public static System.Void PrepareForSavingManifestToDisk(System.Reflection.RuntimeAssembly assembly, System.Reflection.RuntimeModule assemblyModule)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.PrepareForSavingManifestToDisk' has not been implemented!");
		}

		public static System.Void SaveManifestToDisk(System.Reflection.RuntimeAssembly assembly, System.String strFileName, System.Int32 entryPoint, System.Int32 fileKind, System.Int32 portableExecutableKind, System.Int32 ImageFileMachine)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.SaveManifestToDisk' has not been implemented!");
		}

		public static System.Int32 AddFile(System.Reflection.RuntimeAssembly assembly, System.String strFileName)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.AddFile' has not been implemented!");
		}

		public static System.Void SetFileHashValue(System.Reflection.RuntimeAssembly assembly, System.Int32 tkFile, System.String strFullFileName)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.SetFileHashValue' has not been implemented!");
		}

		public static System.Int32 AddExportedTypeInMemory(System.Reflection.RuntimeAssembly assembly, System.String strComTypeName, System.Int32 tkAssemblyRef, System.Int32 tkTypeDef, System.Reflection.TypeAttributes flags)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.AddExportedTypeInMemory' has not been implemented!");
		}

		public static System.Int32 AddExportedTypeOnDisk(System.Reflection.RuntimeAssembly assembly, System.String strComTypeName, System.Int32 tkAssemblyRef, System.Int32 tkTypeDef, System.Reflection.TypeAttributes flags)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.AddExportedTypeOnDisk' has not been implemented!");
		}

		public static System.Void AddStandAloneResource(System.Reflection.RuntimeAssembly assembly, System.String strName, System.String strFileName, System.String strFullFileName, System.Int32 attribute)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.AddStandAloneResource' has not been implemented!");
		}

		public static System.Void AddDeclarativeSecurity(System.Reflection.RuntimeAssembly assembly, System.Security.Permissions.SecurityAction action, System.Byte[] blob, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.AddDeclarativeSecurity' has not been implemented!");
		}

		public static System.Void CreateVersionInfoResource(System.String filename, System.String title, System.String iconFilename, System.String description, System.String copyright, System.String trademark, System.String company, System.String product, System.String productVersion, System.String fileVersion, System.Int32 lcid, System.Boolean isDll, System.Runtime.CompilerServices.StringHandleOnStack retFileName)
		{
			throw new System.NotImplementedException("Method 'System.Reflection.Emit.AssemblyBuilder.CreateVersionInfoResource' has not been implemented!");
		}
	}
}
