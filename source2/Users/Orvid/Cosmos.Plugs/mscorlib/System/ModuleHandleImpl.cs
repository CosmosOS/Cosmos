namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ModuleHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ModuleHandleImpl
	{

		public static System.IRuntimeMethodInfo GetDynamicMethod(System.Reflection.Emit.DynamicMethod method, System.Reflection.RuntimeModule module, System.String name, System.Byte[] sig, System.Resolver resolver)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.GetDynamicMethod' has not been implemented!");
		}

		public static System.Int32 GetToken(System.Reflection.RuntimeModule module)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.GetToken' has not been implemented!");
		}

		public static System.Void GetModuleType(System.Reflection.RuntimeModule handle, System.Runtime.CompilerServices.ObjectHandleOnStack type)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.GetModuleType' has not been implemented!");
		}

		public static System.Int32 GetMDStreamVersion(System.Reflection.RuntimeModule module)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.GetMDStreamVersion' has not been implemented!");
		}

		public static System.IntPtr _GetMetadataImport(System.Reflection.RuntimeModule module)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle._GetMetadataImport' has not been implemented!");
		}

		public static System.Void ResolveType(System.Reflection.RuntimeModule module, System.Int32 typeToken, System.IntPtr* typeInstArgs, System.Int32 typeInstCount, System.IntPtr* methodInstArgs, System.Int32 methodInstCount, System.Runtime.CompilerServices.ObjectHandleOnStack type)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.ResolveType' has not been implemented!");
		}

		public static System.RuntimeMethodHandleInternal ResolveMethod(System.Reflection.RuntimeModule module, System.Int32 methodToken, System.IntPtr* typeInstArgs, System.Int32 typeInstCount, System.IntPtr* methodInstArgs, System.Int32 methodInstCount)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.ResolveMethod' has not been implemented!");
		}

		public static System.Void ResolveField(System.Reflection.RuntimeModule module, System.Int32 fieldToken, System.IntPtr* typeInstArgs, System.Int32 typeInstCount, System.IntPtr* methodInstArgs, System.Int32 methodInstCount, System.Runtime.CompilerServices.ObjectHandleOnStack retField)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.ResolveField' has not been implemented!");
		}

		public static System.Boolean _ContainsPropertyMatchingHash(System.Reflection.RuntimeModule module, System.Int32 propertyToken, System.UInt32 hash)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle._ContainsPropertyMatchingHash' has not been implemented!");
		}

		public static System.Void GetAssembly(System.Reflection.RuntimeModule handle, System.Runtime.CompilerServices.ObjectHandleOnStack retAssembly)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.GetAssembly' has not been implemented!");
		}

		public static System.Void GetPEKind(System.Reflection.RuntimeModule handle, System.Int32* peKind, System.Int32* machine)
		{
			throw new System.NotImplementedException("Method 'System.ModuleHandle.GetPEKind' has not been implemented!");
		}
	}
}
