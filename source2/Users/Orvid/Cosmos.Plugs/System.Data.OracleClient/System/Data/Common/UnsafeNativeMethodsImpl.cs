namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Data.Common.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Data_Common_UnsafeNativeMethodsImpl
	{

		public static System.Boolean CheckTokenMembership(System.IntPtr tokenHandle, System.Byte[] sidToCheck, System.Boolean* isMember)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.CheckTokenMembership' has not been implemented!");
		}

		public static System.Boolean ConvertSidToStringSidW(System.IntPtr sid, System.IntPtr* stringSid)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.ConvertSidToStringSidW' has not been implemented!");
		}

		public static System.Int32 CreateWellKnownSid(System.Int32 sidType, System.Byte[] domainSid, System.Byte[] resultSid, System.UInt32* resultSidLength)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.CreateWellKnownSid' has not been implemented!");
		}

		public static System.Boolean GetTokenInformation(System.IntPtr tokenHandle, System.UInt32 token_class, System.IntPtr tokenStruct, System.UInt32 tokenInformationLength, System.UInt32* tokenString)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.GetTokenInformation' has not been implemented!");
		}

		public static System.Boolean IsTokenRestricted(System.IntPtr tokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.IsTokenRestricted' has not been implemented!");
		}

		public static System.Int32 lstrlenA(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.lstrlenA' has not been implemented!");
		}

		public static System.Int32 lstrlenW(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.lstrlenW' has not been implemented!");
		}

		public static System.Void SetLastError(System.Int32 dwErrCode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SetLastError' has not been implemented!");
		}

		public static System.Int32 OraMTSEnlCtxGet(System.Byte[] lpUname, System.Byte[] lpPsswd, System.Byte[] lpDbnam, System.Data.OracleClient.OciHandle pOCISvc, System.Data.OracleClient.OciHandle pOCIErr, System.UInt32 dwFlags, System.IntPtr* pCtxt)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OraMTSEnlCtxGet' has not been implemented!");
		}

		public static System.Int32 OraMTSEnlCtxRel(System.IntPtr pCtxt)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OraMTSEnlCtxRel' has not been implemented!");
		}

		public static System.Int32 OraMTSOCIErrGet(System.Int32* dwErr, System.Data.OracleClient.NativeBuffer lpcEMsg, System.Int32* lpdLen)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OraMTSOCIErrGet' has not been implemented!");
		}

		public static System.Int32 OraMTSJoinTxn(System.Data.OracleClient.OciEnlistContext pCtxt, System.Transactions.IDtcTransaction pTrans)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OraMTSJoinTxn' has not been implemented!");
		}

		public static System.Int32 oermsg(System.Int16 rcode, System.Data.OracleClient.NativeBuffer buf)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.oermsg' has not been implemented!");
		}

		public static System.Int32 OCIAttrGet(System.Data.OracleClient.OciHandle trgthndlp, System.Data.OracleClient.OCI+HTYPE trghndltyp, System.Data.OracleClient.OciHandle attributep, System.UInt32* sizep, System.Data.OracleClient.OCI+ATTR attrtype, System.Data.OracleClient.OciHandle errhp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIAttrGet' has not been implemented!");
		}

		public static System.Int32 OCIAttrGet(System.Data.OracleClient.OciHandle trgthndlp, System.Data.OracleClient.OCI+HTYPE trghndltyp, System.Int32* attributep, System.UInt32* sizep, System.Data.OracleClient.OCI+ATTR attrtype, System.Data.OracleClient.OciHandle errhp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIAttrGet' has not been implemented!");
		}

		public static System.Int32 OCIAttrGet(System.Data.OracleClient.OciHandle trgthndlp, System.Data.OracleClient.OCI+HTYPE trghndltyp, System.IntPtr* attributep, System.UInt32* sizep, System.Data.OracleClient.OCI+ATTR attrtype, System.Data.OracleClient.OciHandle errhp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIAttrGet' has not been implemented!");
		}

		public static System.Int32 OCIAttrSet(System.Data.OracleClient.OciHandle trgthndlp, System.Data.OracleClient.OCI+HTYPE trghndltyp, System.Data.OracleClient.OciHandle attributep, System.UInt32 size, System.Data.OracleClient.OCI+ATTR attrtype, System.Data.OracleClient.OciHandle errhp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIAttrSet' has not been implemented!");
		}

		public static System.Int32 OCIAttrSet(System.Data.OracleClient.OciHandle trgthndlp, System.Data.OracleClient.OCI+HTYPE trghndltyp, System.Int32* attributep, System.UInt32 size, System.Data.OracleClient.OCI+ATTR attrtype, System.Data.OracleClient.OciHandle errhp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIAttrSet' has not been implemented!");
		}

		public static System.Int32 OCIAttrSet(System.Data.OracleClient.OciHandle trgthndlp, System.Data.OracleClient.OCI+HTYPE trghndltyp, System.Byte[] attributep, System.UInt32 size, System.Data.OracleClient.OCI+ATTR attrtype, System.Data.OracleClient.OciHandle errhp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIAttrSet' has not been implemented!");
		}

		public static System.Int32 OCIBindByName(System.Data.OracleClient.OciHandle stmtp, System.IntPtr* bindpp, System.Data.OracleClient.OciHandle errhp, System.Byte[] placeholder, System.Int32 placeh_len, System.IntPtr valuep, System.Int32 value_sz, System.Data.OracleClient.OCI+DATATYPE dty, System.IntPtr indp, System.IntPtr alenp, System.IntPtr rcodep, System.UInt32 maxarr_len, System.IntPtr curelap, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIBindByName' has not been implemented!");
		}

		public static System.Int32 OCICharSetToUnicode(System.Data.OracleClient.OciHandle hndl, System.IntPtr dst, System.UInt32 dstsz, System.IntPtr src, System.UInt32 srcsz, System.UInt32* size)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCICharSetToUnicode' has not been implemented!");
		}

		public static System.Int32 OCIDateTimeFromArray(System.Data.OracleClient.OciHandle hndl, System.Data.OracleClient.OciHandle err, System.Byte[] inarray, System.UInt32 len, System.Byte type, System.Data.OracleClient.OciHandle datetime, System.Data.OracleClient.OciHandle reftz, System.Byte fsprec)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIDateTimeFromArray' has not been implemented!");
		}

		public static System.Int32 OCIDateTimeToArray(System.Data.OracleClient.OciHandle hndl, System.Data.OracleClient.OciHandle err, System.Data.OracleClient.OciHandle datetime, System.Data.OracleClient.OciHandle reftz, System.Byte[] outarray, System.UInt32* len, System.Byte fsprec)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIDateTimeToArray' has not been implemented!");
		}

		public static System.Int32 OCIDateTimeGetTimeZoneOffset(System.Data.OracleClient.OciHandle hndl, System.Data.OracleClient.OciHandle err, System.Data.OracleClient.OciHandle datetime, System.SByte* hour, System.SByte* min)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIDateTimeGetTimeZoneOffset' has not been implemented!");
		}

		public static System.Int32 OCIDefineArrayOfStruct(System.Data.OracleClient.OciHandle defnp, System.Data.OracleClient.OciHandle errhp, System.UInt32 pvskip, System.UInt32 indskip, System.UInt32 rlskip, System.UInt32 rcskip)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIDefineArrayOfStruct' has not been implemented!");
		}

		public static System.Int32 OCIDefineByPos(System.Data.OracleClient.OciHandle stmtp, System.IntPtr* hndlpp, System.Data.OracleClient.OciHandle errhp, System.UInt32 position, System.IntPtr valuep, System.Int32 value_sz, System.Data.OracleClient.OCI+DATATYPE dty, System.IntPtr indp, System.IntPtr alenp, System.IntPtr rcodep, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIDefineByPos' has not been implemented!");
		}

		public static System.Int32 OCIDefineDynamic(System.Data.OracleClient.OciHandle defnp, System.Data.OracleClient.OciHandle errhp, System.IntPtr octxp, System.Data.OracleClient.OCI+Callback+OCICallbackDefine ocbfp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIDefineDynamic' has not been implemented!");
		}

		public static System.Int32 OCIDescriptorAlloc(System.Data.OracleClient.OciHandle parenth, System.IntPtr* descp, System.Data.OracleClient.OCI+HTYPE type, System.IntPtr xtramem_sz, System.IntPtr usrmempp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIDescriptorAlloc' has not been implemented!");
		}

		public static System.Int32 OCIDescriptorFree(System.IntPtr hndlp, System.Data.OracleClient.OCI+HTYPE type)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIDescriptorFree' has not been implemented!");
		}

		public static System.Int32 OCIEnvCreate(System.IntPtr* envhpp, System.Data.OracleClient.OCI+MODE mode, System.IntPtr ctxp, System.IntPtr malocfp, System.IntPtr ralocfp, System.IntPtr mfreefp, System.IntPtr xtramemsz, System.IntPtr usrmempp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIEnvCreate' has not been implemented!");
		}

		public static System.Int32 OCIEnvNlsCreate(System.IntPtr* envhpp, System.Data.OracleClient.OCI+MODE mode, System.IntPtr ctxp, System.IntPtr malocfp, System.IntPtr ralocfp, System.IntPtr mfreefp, System.IntPtr xtramemsz, System.IntPtr usrmempp, System.UInt16 charset, System.UInt16 ncharset)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIEnvNlsCreate' has not been implemented!");
		}

		public static System.Int32 OCIErrorGet(System.Data.OracleClient.OciHandle hndlp, System.UInt32 recordno, System.IntPtr sqlstate, System.Int32* errcodep, System.Data.OracleClient.NativeBuffer bufp, System.UInt32 bufsiz, System.Data.OracleClient.OCI+HTYPE type)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIErrorGet' has not been implemented!");
		}

		public static System.Int32 OCIHandleAlloc(System.Data.OracleClient.OciHandle parenth, System.IntPtr* hndlpp, System.Data.OracleClient.OCI+HTYPE type, System.IntPtr xtramemsz, System.IntPtr usrmempp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIHandleAlloc' has not been implemented!");
		}

		public static System.Int32 OCIHandleFree(System.IntPtr hndlp, System.Data.OracleClient.OCI+HTYPE type)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIHandleFree' has not been implemented!");
		}

		public static System.Int32 OCILobAppend(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle dst_locp, System.Data.OracleClient.OciHandle src_locp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobAppend' has not been implemented!");
		}

		public static System.Int32 OCILobClose(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobClose' has not been implemented!");
		}

		public static System.Int32 OCILobCopy(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle dst_locp, System.Data.OracleClient.OciHandle src_locp, System.UInt32 amount, System.UInt32 dst_offset, System.UInt32 src_offset)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobCopy' has not been implemented!");
		}

		public static System.Int32 OCILobCopy2(System.IntPtr svchp, System.IntPtr errhp, System.IntPtr dst_locp, System.IntPtr src_locp, System.UInt64 amount, System.UInt64 dst_offset, System.UInt64 src_offset)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobCopy2' has not been implemented!");
		}

		public static System.Int32 OCILobCreateTemporary(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.UInt16 csid, System.Data.OracleClient.OCI+CHARSETFORM csfrm, System.Data.OracleClient.OCI+LOB_TYPE lobtype, System.Int32 cache, System.Data.OracleClient.OCI+DURATION duration)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobCreateTemporary' has not been implemented!");
		}

		public static System.Int32 OCILobErase(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.UInt32* amount, System.UInt32 offset)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobErase' has not been implemented!");
		}

		public static System.Int32 OCILobFileExists(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.Int32* flag)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobFileExists' has not been implemented!");
		}

		public static System.Int32 OCILobFileGetName(System.Data.OracleClient.OciHandle envhp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle filep, System.IntPtr dir_alias, System.UInt16* d_length, System.IntPtr filename, System.UInt16* f_length)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobFileGetName' has not been implemented!");
		}

		public static System.Int32 OCILobFileSetName(System.Data.OracleClient.OciHandle envhp, System.Data.OracleClient.OciHandle errhp, System.IntPtr* filep, System.Byte[] dir_alias, System.UInt16 d_length, System.Byte[] filename, System.UInt16 f_length)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobFileSetName' has not been implemented!");
		}

		public static System.Int32 OCILobFreeTemporary(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobFreeTemporary' has not been implemented!");
		}

		public static System.Int32 OCILobGetChunkSize(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.UInt32* lenp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobGetChunkSize' has not been implemented!");
		}

		public static System.Int32 OCILobGetLength(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.UInt32* lenp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobGetLength' has not been implemented!");
		}

		public static System.Int32 OCILobIsOpen(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.Int32* flag)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobIsOpen' has not been implemented!");
		}

		public static System.Int32 OCILobIsTemporary(System.Data.OracleClient.OciHandle envhp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.Int32* flag)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobIsTemporary' has not been implemented!");
		}

		public static System.Int32 OCILobLoadFromFile(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle dst_locp, System.Data.OracleClient.OciHandle src_locp, System.UInt32 amount, System.UInt32 dst_offset, System.UInt32 src_offset)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobLoadFromFile' has not been implemented!");
		}

		public static System.Int32 OCILobOpen(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.Byte mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobOpen' has not been implemented!");
		}

		public static System.Int32 OCILobRead(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.UInt32* amtp, System.UInt32 offset, System.IntPtr bufp, System.UInt32 bufl, System.IntPtr ctxp, System.IntPtr cbfp, System.UInt16 csid, System.Data.OracleClient.OCI+CHARSETFORM csfrm)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobRead' has not been implemented!");
		}

		public static System.Int32 OCILobTrim(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.UInt32 newlen)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobTrim' has not been implemented!");
		}

		public static System.Int32 OCILobWrite(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle locp, System.UInt32* amtp, System.UInt32 offset, System.IntPtr bufp, System.UInt32 buflen, System.Byte piece, System.IntPtr ctxp, System.IntPtr cbfp, System.UInt16 csid, System.Data.OracleClient.OCI+CHARSETFORM csfrm)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCILobWrite' has not been implemented!");
		}

		public static System.Int32 OCINumberAbs(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberAbs' has not been implemented!");
		}

		public static System.Int32 OCINumberAdd(System.Data.OracleClient.OciHandle err, System.Byte[] number1, System.Byte[] number2, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberAdd' has not been implemented!");
		}

		public static System.Int32 OCINumberArcCos(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberArcCos' has not been implemented!");
		}

		public static System.Int32 OCINumberArcSin(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberArcSin' has not been implemented!");
		}

		public static System.Int32 OCINumberArcTan(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberArcTan' has not been implemented!");
		}

		public static System.Int32 OCINumberArcTan2(System.Data.OracleClient.OciHandle err, System.Byte[] number1, System.Byte[] number2, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberArcTan2' has not been implemented!");
		}

		public static System.Int32 OCINumberCeil(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberCeil' has not been implemented!");
		}

		public static System.Int32 OCINumberCmp(System.Data.OracleClient.OciHandle err, System.Byte[] number1, System.Byte[] number2, System.Int32* result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberCmp' has not been implemented!");
		}

		public static System.Int32 OCINumberCos(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberCos' has not been implemented!");
		}

		public static System.Int32 OCINumberDiv(System.Data.OracleClient.OciHandle err, System.Byte[] number1, System.Byte[] number2, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberDiv' has not been implemented!");
		}

		public static System.Int32 OCINumberExp(System.Data.OracleClient.OciHandle err, System.Byte[] p, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberExp' has not been implemented!");
		}

		public static System.Int32 OCINumberFloor(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberFloor' has not been implemented!");
		}

		public static System.Int32 OCINumberFromInt(System.Data.OracleClient.OciHandle err, System.Int32* inum, System.UInt32 inum_length, System.Data.OracleClient.OCI+SIGN inum_s_flag, System.Byte[] number)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberFromInt' has not been implemented!");
		}

		public static System.Int32 OCINumberFromInt(System.Data.OracleClient.OciHandle err, System.UInt32* inum, System.UInt32 inum_length, System.Data.OracleClient.OCI+SIGN inum_s_flag, System.Byte[] number)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberFromInt' has not been implemented!");
		}

		public static System.Int32 OCINumberFromInt(System.Data.OracleClient.OciHandle err, System.Int64* inum, System.UInt32 inum_length, System.Data.OracleClient.OCI+SIGN inum_s_flag, System.Byte[] number)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberFromInt' has not been implemented!");
		}

		public static System.Int32 OCINumberFromInt(System.Data.OracleClient.OciHandle err, System.UInt64* inum, System.UInt32 inum_length, System.Data.OracleClient.OCI+SIGN inum_s_flag, System.Byte[] number)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberFromInt' has not been implemented!");
		}

		public static System.Int32 OCINumberFromReal(System.Data.OracleClient.OciHandle err, System.Double* rnum, System.UInt32 rnum_length, System.Byte[] number)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberFromReal' has not been implemented!");
		}

		public static System.Int32 OCINumberFromText(System.Data.OracleClient.OciHandle err, System.String str, System.UInt32 str_length, System.String fmt, System.UInt32 fmt_length, System.IntPtr nls_params, System.UInt32 nls_p_length, System.Byte[] number)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberFromText' has not been implemented!");
		}

		public static System.Int32 OCINumberHypCos(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberHypCos' has not been implemented!");
		}

		public static System.Int32 OCINumberHypSin(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberHypSin' has not been implemented!");
		}

		public static System.Int32 OCINumberHypTan(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberHypTan' has not been implemented!");
		}

		public static System.Int32 OCINumberIntPower(System.Data.OracleClient.OciHandle err, System.Byte[] baseNumber, System.Int32 exponent, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberIntPower' has not been implemented!");
		}

		public static System.Int32 OCINumberIsInt(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Int32* result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberIsInt' has not been implemented!");
		}

		public static System.Int32 OCINumberLn(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberLn' has not been implemented!");
		}

		public static System.Int32 OCINumberLog(System.Data.OracleClient.OciHandle err, System.Byte[] b, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberLog' has not been implemented!");
		}

		public static System.Int32 OCINumberMod(System.Data.OracleClient.OciHandle err, System.Byte[] number1, System.Byte[] number2, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberMod' has not been implemented!");
		}

		public static System.Int32 OCINumberMul(System.Data.OracleClient.OciHandle err, System.Byte[] number1, System.Byte[] number2, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberMul' has not been implemented!");
		}

		public static System.Int32 OCINumberNeg(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberNeg' has not been implemented!");
		}

		public static System.Int32 OCINumberPower(System.Data.OracleClient.OciHandle err, System.Byte[] baseNumber, System.Byte[] exponent, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberPower' has not been implemented!");
		}

		public static System.Int32 OCINumberRound(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Int32 decplace, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberRound' has not been implemented!");
		}

		public static System.Int32 OCINumberShift(System.Data.OracleClient.OciHandle err, System.Byte[] baseNumber, System.Int32 nDig, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberShift' has not been implemented!");
		}

		public static System.Int32 OCINumberSign(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Int32* result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberSign' has not been implemented!");
		}

		public static System.Int32 OCINumberSin(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberSin' has not been implemented!");
		}

		public static System.Int32 OCINumberSqrt(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberSqrt' has not been implemented!");
		}

		public static System.Int32 OCINumberSub(System.Data.OracleClient.OciHandle err, System.Byte[] number1, System.Byte[] number2, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberSub' has not been implemented!");
		}

		public static System.Int32 OCINumberTan(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberTan' has not been implemented!");
		}

		public static System.Int32 OCINumberToInt(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.UInt32 rsl_length, System.Data.OracleClient.OCI+SIGN rsl_flag, System.Int32* rsl)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberToInt' has not been implemented!");
		}

		public static System.Int32 OCINumberToInt(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.UInt32 rsl_length, System.Data.OracleClient.OCI+SIGN rsl_flag, System.UInt32* rsl)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberToInt' has not been implemented!");
		}

		public static System.Int32 OCINumberToInt(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.UInt32 rsl_length, System.Data.OracleClient.OCI+SIGN rsl_flag, System.Int64* rsl)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberToInt' has not been implemented!");
		}

		public static System.Int32 OCINumberToInt(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.UInt32 rsl_length, System.Data.OracleClient.OCI+SIGN rsl_flag, System.UInt64* rsl)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberToInt' has not been implemented!");
		}

		public static System.Int32 OCINumberToReal(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.UInt32 rsl_length, System.Double* rsl)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberToReal' has not been implemented!");
		}

		public static System.Int32 OCINumberToText(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.String fmt, System.Int32 fmt_length, System.IntPtr nls_params, System.UInt32 nls_p_length, System.UInt32* buf_size, System.Byte[] buffer)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberToText' has not been implemented!");
		}

		public static System.Int32 OCINumberTrunc(System.Data.OracleClient.OciHandle err, System.Byte[] number, System.Int32 decplace, System.Byte[] result)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCINumberTrunc' has not been implemented!");
		}

		public static System.Int32 OCIParamGet(System.Data.OracleClient.OciHandle hndlp, System.Data.OracleClient.OCI+HTYPE htype, System.Data.OracleClient.OciHandle errhp, System.IntPtr* paramdpp, System.UInt32 pos)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIParamGet' has not been implemented!");
		}

		public static System.Int32 OCIRowidToChar(System.Data.OracleClient.OciHandle rowidDesc, System.Data.OracleClient.NativeBuffer outbfp, System.UInt16* outbflp, System.Data.OracleClient.OciHandle errhp)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIRowidToChar' has not been implemented!");
		}

		public static System.Int32 OCIServerAttach(System.Data.OracleClient.OciHandle srvhp, System.Data.OracleClient.OciHandle errhp, System.Byte[] dblink, System.Int32 dblink_len, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIServerAttach' has not been implemented!");
		}

		public static System.Int32 OCIServerDetach(System.IntPtr srvhp, System.IntPtr errhp, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIServerDetach' has not been implemented!");
		}

		public static System.Int32 OCIServerVersion(System.Data.OracleClient.OciHandle hndlp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.NativeBuffer bufp, System.UInt32 bufsz, System.Byte hndltype)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIServerVersion' has not been implemented!");
		}

		public static System.Int32 OCISessionBegin(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OciHandle usrhp, System.Data.OracleClient.OCI+CRED credt, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCISessionBegin' has not been implemented!");
		}

		public static System.Int32 OCISessionEnd(System.IntPtr svchp, System.IntPtr errhp, System.IntPtr usrhp, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCISessionEnd' has not been implemented!");
		}

		public static System.Int32 OCIStmtExecute(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle stmtp, System.Data.OracleClient.OciHandle errhp, System.UInt32 iters, System.UInt32 rowoff, System.IntPtr snap_in, System.IntPtr snap_out, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIStmtExecute' has not been implemented!");
		}

		public static System.Int32 OCIStmtFetch(System.Data.OracleClient.OciHandle stmtp, System.Data.OracleClient.OciHandle errhp, System.UInt32 nrows, System.Data.OracleClient.OCI+FETCH orientation, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIStmtFetch' has not been implemented!");
		}

		public static System.Int32 OCIStmtPrepare(System.Data.OracleClient.OciHandle stmtp, System.Data.OracleClient.OciHandle errhp, System.Byte[] stmt, System.UInt32 stmt_len, System.Data.OracleClient.OCI+SYNTAX language, System.Data.OracleClient.OCI+MODE mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIStmtPrepare' has not been implemented!");
		}

		public static System.Int32 OCITransCommit(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OCI+MODE flags)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCITransCommit' has not been implemented!");
		}

		public static System.Int32 OCITransRollback(System.Data.OracleClient.OciHandle svchp, System.Data.OracleClient.OciHandle errhp, System.Data.OracleClient.OCI+MODE flags)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCITransRollback' has not been implemented!");
		}

		public static System.Int32 OCIUnicodeToCharSet(System.Data.OracleClient.OciHandle hndl, System.IntPtr dst, System.UInt32 dstsz, System.IntPtr src, System.UInt32 srcsz, System.UInt32* size)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.OCIUnicodeToCharSet' has not been implemented!");
		}
	}
}
