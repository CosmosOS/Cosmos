namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Runtime.InteropServices.Marshal), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Runtime_InteropServices_MarshalImpl
	{

		public static System.String PtrToStringAnsi(System.IntPtr ptr, System.Int32 len)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.PtrToStringAnsi' has not been implemented!");
		}

		public static System.String PtrToStringUni(System.IntPtr ptr, System.Int32 len)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.PtrToStringUni' has not been implemented!");
		}

		public static System.UInt32 SizeOfType(System.Type type)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.SizeOfType' has not been implemented!");
		}

		public static System.UInt32 AlignedSizeOfType(System.Type type)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.AlignedSizeOfType' has not been implemented!");
		}

		public static System.Int32 SizeOfHelper(System.Type t, System.Boolean throwIfNotMarshalable)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.SizeOfHelper' has not been implemented!");
		}

		public static System.IntPtr UnsafeAddrOfPinnedArrayElement(System.Array arr, System.Int32 index)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement' has not been implemented!");
		}

		public static System.Void CopyToNative(System.Object source, System.Int32 startIndex, System.IntPtr destination, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.CopyToNative' has not been implemented!");
		}

		public static System.Void CopyToManaged(System.IntPtr source, System.Object destination, System.Int32 startIndex, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.CopyToManaged' has not been implemented!");
		}

		public static System.Byte ReadByte(System.Object ptr, System.Int32 ofs)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.ReadByte' has not been implemented!");
		}

		public static System.Int16 ReadInt16(System.Object ptr, System.Int32 ofs)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.ReadInt16' has not been implemented!");
		}

		public static System.Int32 ReadInt32(System.Object ptr, System.Int32 ofs)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.ReadInt32' has not been implemented!");
		}

		public static System.Int64 ReadInt64(System.Object ptr, System.Int32 ofs)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.ReadInt64' has not been implemented!");
		}

		public static System.Void WriteByte(System.Object ptr, System.Int32 ofs, System.Byte val)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.WriteByte' has not been implemented!");
		}

		public static System.Void WriteInt16(System.Object ptr, System.Int32 ofs, System.Int16 val)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.WriteInt16' has not been implemented!");
		}

		public static System.Void WriteInt32(System.Object ptr, System.Int32 ofs, System.Int32 val)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.WriteInt32' has not been implemented!");
		}

		public static System.Void WriteInt64(System.Object ptr, System.Int32 ofs, System.Int64 val)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.WriteInt64' has not been implemented!");
		}

		public static System.Int32 GetLastWin32Error()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetLastWin32Error' has not been implemented!");
		}

		public static System.Void SetLastWin32Error(System.Int32 error)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.SetLastWin32Error' has not been implemented!");
		}

		public static System.IntPtr GetExceptionPointers()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetExceptionPointers' has not been implemented!");
		}

		public static System.Int32 GetExceptionCode()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetExceptionCode' has not been implemented!");
		}

		public static System.Void StructureToPtr(System.Object structure, System.IntPtr ptr, System.Boolean fDeleteOld)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.StructureToPtr' has not been implemented!");
		}

		public static System.Void PtrToStructureHelper(System.IntPtr ptr, System.Object structure, System.Boolean allowValueClasses)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.PtrToStructureHelper' has not been implemented!");
		}

		public static System.Void DestroyStructure(System.IntPtr ptr, System.Type structuretype)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.DestroyStructure' has not been implemented!");
		}

		public static System.Void ThrowExceptionForHRInternal(System.Int32 errorCode, System.IntPtr errorInfo)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.ThrowExceptionForHRInternal' has not been implemented!");
		}

		public static System.Exception GetExceptionForHRInternal(System.Int32 errorCode, System.IntPtr errorInfo)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetExceptionForHRInternal' has not been implemented!");
		}

		public static System.Int32 GetHRForException(System.Exception e)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetHRForException' has not been implemented!");
		}

		public static System.IntPtr GetUnmanagedThunkForManagedMethodPtr(System.IntPtr pfnMethodToWrap, System.IntPtr pbSignature, System.Int32 cbSignature)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetUnmanagedThunkForManagedMethodPtr' has not been implemented!");
		}

		public static System.IntPtr GetManagedThunkForUnmanagedMethodPtr(System.IntPtr pfnMethodToWrap, System.IntPtr pbSignature, System.Int32 cbSignature)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetManagedThunkForUnmanagedMethodPtr' has not been implemented!");
		}

		public static System.Int32 GetTypeLibLcid(System.Runtime.InteropServices.ComTypes.ITypeLib typelib)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetTypeLibLcid' has not been implemented!");
		}

		public static System.Void GetTypeLibVersion(System.Runtime.InteropServices.ComTypes.ITypeLib typeLibrary, System.Int32* major, System.Int32* minor)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetTypeLibVersion' has not been implemented!");
		}

		public static System.IntPtr GetITypeInfoForType(System.Type t)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetITypeInfoForType' has not been implemented!");
		}

		public static System.IntPtr GetIUnknownForObjectNative(System.Object o, System.Boolean onlyInContext)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetIUnknownForObjectNative' has not been implemented!");
		}

		public static System.IntPtr GetIDispatchForObjectNative(System.Object o, System.Boolean onlyInContext)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetIDispatchForObjectNative' has not been implemented!");
		}

		public static System.IntPtr GetComInterfaceForObjectNative(System.Object o, System.Type t, System.Boolean onlyInContext, System.Boolean fEnalbeCustomizedQueryInterface)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetComInterfaceForObjectNative' has not been implemented!");
		}

		public static System.Object GetObjectForIUnknown(System.IntPtr pUnk)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetObjectForIUnknown' has not been implemented!");
		}

		public static System.Object GetUniqueObjectForIUnknown(System.IntPtr unknown)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetUniqueObjectForIUnknown' has not been implemented!");
		}

		public static System.Object GetTypedObjectForIUnknown(System.IntPtr pUnk, System.Type t)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetTypedObjectForIUnknown' has not been implemented!");
		}

		public static System.IntPtr CreateAggregatedObject(System.IntPtr pOuter, System.Object o)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.CreateAggregatedObject' has not been implemented!");
		}

		public static System.Void CleanupUnusedObjectsInCurrentContext()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.CleanupUnusedObjectsInCurrentContext' has not been implemented!");
		}

		public static System.Boolean AreComObjectsAvailableForCleanup()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.AreComObjectsAvailableForCleanup' has not been implemented!");
		}

		public static System.Boolean IsComObject(System.Object o)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.IsComObject' has not been implemented!");
		}

		public static System.Int32 InternalReleaseComObject(System.Object o)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalReleaseComObject' has not been implemented!");
		}

		public static System.Void InternalFinalReleaseComObject(System.Object o)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalFinalReleaseComObject' has not been implemented!");
		}

		public static System.Boolean IsTypeVisibleFromCom(System.Type t)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.IsTypeVisibleFromCom' has not been implemented!");
		}

		public static System.Int32 QueryInterface(System.IntPtr pUnk, System.Guid* iid, System.IntPtr* ppv)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.QueryInterface' has not been implemented!");
		}

		public static System.Int32 AddRef(System.IntPtr pUnk)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.AddRef' has not been implemented!");
		}

		public static System.Int32 Release(System.IntPtr pUnk)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.Release' has not been implemented!");
		}

		public static System.Void GetNativeVariantForObject(System.Object obj, System.IntPtr pDstNativeVariant)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetNativeVariantForObject' has not been implemented!");
		}

		public static System.Object GetObjectForNativeVariant(System.IntPtr pSrcNativeVariant)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant' has not been implemented!");
		}

		public static System.Object[] GetObjectsForNativeVariants(System.IntPtr aSrcNativeVariant, System.Int32 cVars)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetObjectsForNativeVariants' has not been implemented!");
		}

		public static System.Int32 GetStartComSlot(System.Type t)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetStartComSlot' has not been implemented!");
		}

		public static System.Int32 GetEndComSlot(System.Type t)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetEndComSlot' has not been implemented!");
		}

		public static System.Reflection.MemberInfo GetMethodInfoForComSlot(System.Type t, System.Int32 slot, System.Runtime.InteropServices.ComMemberType* memberType)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetMethodInfoForComSlot' has not been implemented!");
		}

		public static System.Boolean InternalSwitchCCW(System.Object oldtp, System.Object newtp)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalSwitchCCW' has not been implemented!");
		}

		public static System.Object InternalWrapIUnknownWithComObject(System.IntPtr i)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalWrapIUnknownWithComObject' has not been implemented!");
		}

		public static System.Void ChangeWrapperHandleStrength(System.Object otp, System.Boolean fIsWeak)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.ChangeWrapperHandleStrength' has not been implemented!");
		}

		public static System.Delegate GetDelegateForFunctionPointerInternal(System.IntPtr ptr, System.Type t)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointerInternal' has not been implemented!");
		}

		public static System.IntPtr GetFunctionPointerForDelegateInternal(System.Delegate d)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegateInternal' has not been implemented!");
		}

		public static System.Int32 GetSystemMaxDBCSCharSize()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetSystemMaxDBCSCharSize' has not been implemented!");
		}

		public static System.IntPtr OffsetOfHelper(System.IRuntimeFieldInfo f)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.OffsetOfHelper' has not been implemented!");
		}

		public static System.Void InternalPrelink(System.IRuntimeMethodInfo m)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalPrelink' has not been implemented!");
		}

		public static System.Int32 InternalNumParamBytes(System.IRuntimeMethodInfo m)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalNumParamBytes' has not been implemented!");
		}

		public static System.IntPtr GetHINSTANCE(System.Reflection.RuntimeModule m)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetHINSTANCE' has not been implemented!");
		}

		public static System.Threading.Thread InternalGetThreadFromFiberCookie(System.Int32 cookie)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalGetThreadFromFiberCookie' has not been implemented!");
		}

		public static System.Void FCallGetTypeLibGuid(System.Guid* result, System.Runtime.InteropServices.ComTypes.ITypeLib pTLB)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.FCallGetTypeLibGuid' has not been implemented!");
		}

		public static System.Void FCallGetTypeInfoGuid(System.Guid* result, System.Runtime.InteropServices.ComTypes.ITypeInfo typeInfo)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.FCallGetTypeInfoGuid' has not been implemented!");
		}

		public static System.Void FCallGetTypeLibGuidForAssembly(System.Guid* result, System.Reflection.RuntimeAssembly asm)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.FCallGetTypeLibGuidForAssembly' has not been implemented!");
		}

		public static System.Void _GetTypeLibVersionForAssembly(System.Reflection.RuntimeAssembly inputAssembly, System.Int32* majorVersion, System.Int32* minorVersion)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal._GetTypeLibVersionForAssembly' has not been implemented!");
		}

		public static System.Type GetLoadedTypeForGUID(System.Guid* guid)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetLoadedTypeForGUID' has not been implemented!");
		}

		public static System.Object InternalCreateWrapperOfType(System.Object o, System.Type t)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalCreateWrapperOfType' has not been implemented!");
		}

		public static System.Int32 InternalGetComSlotForMethodInfo(System.IRuntimeMethodInfo m)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.InternalGetComSlotForMethodInfo' has not been implemented!");
		}

		public static System.Void FCallGenerateGuidForType(System.Guid* result, System.Type type)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.FCallGenerateGuidForType' has not been implemented!");
		}

		public static System.Void CLSIDFromProgIDEx(System.String progId, System.Guid* clsid)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.CLSIDFromProgIDEx' has not been implemented!");
		}

		public static System.Void CLSIDFromProgID(System.String progId, System.Guid* clsid)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.CLSIDFromProgID' has not been implemented!");
		}

		public static System.Void CreateBindCtx(System.UInt32 reserved, System.Runtime.InteropServices.ComTypes.IBindCtx* ppbc)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.CreateBindCtx' has not been implemented!");
		}

		public static System.Void MkParseDisplayName(System.Runtime.InteropServices.ComTypes.IBindCtx pbc, System.String szUserName, System.UInt32* pchEaten, System.Runtime.InteropServices.ComTypes.IMoniker* ppmk)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.MkParseDisplayName' has not been implemented!");
		}

		public static System.Void BindMoniker(System.Runtime.InteropServices.ComTypes.IMoniker pmk, System.UInt32 grfOpt, System.Guid* iidResult, System.Object* ppvResult)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.BindMoniker' has not been implemented!");
		}

		public static System.Void GetActiveObject(System.Guid* rclsid, System.IntPtr reserved, System.Object* ppunk)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.Marshal.GetActiveObject' has not been implemented!");
		}
	}
}
