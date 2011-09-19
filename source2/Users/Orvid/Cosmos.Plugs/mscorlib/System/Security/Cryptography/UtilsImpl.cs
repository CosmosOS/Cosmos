namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.Utils), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_UtilsImpl
	{

		public static System.Security.Cryptography.SafeHashHandle CreateHash(System.Security.Cryptography.SafeProvHandle hProv, System.Int32 algid)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.CreateHash' has not been implemented!");
		}

		public static System.Boolean _GetEnforceFipsPolicySetting()
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._GetEnforceFipsPolicySetting' has not been implemented!");
		}

		public static System.Byte[] _GetKeyParameter(System.Security.Cryptography.SafeKeyHandle hKey, System.UInt32 paramID)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._GetKeyParameter' has not been implemented!");
		}

		public static System.Int32 _GetUserKey(System.Security.Cryptography.SafeProvHandle hProv, System.Int32 keyNumber, System.Security.Cryptography.SafeKeyHandle* hKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._GetUserKey' has not been implemented!");
		}

		public static System.Int32 _OpenCSP(System.Security.Cryptography.CspParameters param, System.UInt32 flags, System.Security.Cryptography.SafeProvHandle* hProv)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._OpenCSP' has not been implemented!");
		}

		public static System.Void _AcquireCSP(System.Security.Cryptography.CspParameters param, System.Security.Cryptography.SafeProvHandle* hProv)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._AcquireCSP' has not been implemented!");
		}

		public static System.Void EndHash(System.Security.Cryptography.SafeHashHandle hHash, System.Runtime.CompilerServices.ObjectHandleOnStack retHash)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.EndHash' has not been implemented!");
		}

		public static System.Void HashData(System.Security.Cryptography.SafeHashHandle hHash, System.Byte[] data, System.Int32 cbData, System.Int32 ibStart, System.Int32 cbSize)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.HashData' has not been implemented!");
		}

		public static System.Boolean GetPersistKeyInCsp(System.Security.Cryptography.SafeProvHandle hProv)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.GetPersistKeyInCsp' has not been implemented!");
		}

		public static System.Void SetKeyParamDw(System.Security.Cryptography.SafeKeyHandle hKey, System.Int32 param, System.Int32 dwValue)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.SetKeyParamDw' has not been implemented!");
		}

		public static System.Void SetKeyParamRgb(System.Security.Cryptography.SafeKeyHandle hKey, System.Int32 param, System.Byte[] value, System.Int32 cbValue)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.SetKeyParamRgb' has not been implemented!");
		}

		public static System.Void SetPersistKeyInCsp(System.Security.Cryptography.SafeProvHandle hProv, System.Boolean fPersistKeyInCsp)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.SetPersistKeyInCsp' has not been implemented!");
		}

		public static System.Void SetProviderParameter(System.Security.Cryptography.SafeProvHandle hProv, System.Int32 keyNumber, System.UInt32 paramID, System.IntPtr pbData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.SetProviderParameter' has not been implemented!");
		}

		public static System.Void _CreateCSP(System.Security.Cryptography.CspParameters param, System.Boolean randomKeyContainer, System.Security.Cryptography.SafeProvHandle* hProv)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._CreateCSP' has not been implemented!");
		}

		public static System.Int32 _DecryptData(System.Security.Cryptography.SafeKeyHandle hKey, System.Byte[] data, System.Int32 ib, System.Int32 cb, System.Byte[]* outputBuffer, System.Int32 outputOffset, System.Security.Cryptography.PaddingMode PaddingMode, System.Boolean fDone)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._DecryptData' has not been implemented!");
		}

		public static System.Int32 _EncryptData(System.Security.Cryptography.SafeKeyHandle hKey, System.Byte[] data, System.Int32 ib, System.Int32 cb, System.Byte[]* outputBuffer, System.Int32 outputOffset, System.Security.Cryptography.PaddingMode PaddingMode, System.Boolean fDone)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._EncryptData' has not been implemented!");
		}

		public static System.Void _ExportKey(System.Security.Cryptography.SafeKeyHandle hKey, System.Int32 blobType, System.Object cspObject)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._ExportKey' has not been implemented!");
		}

		public static System.Void _GenerateKey(System.Security.Cryptography.SafeProvHandle hProv, System.Int32 algid, System.Security.Cryptography.CspProviderFlags flags, System.Int32 keySize, System.Security.Cryptography.SafeKeyHandle* hKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._GenerateKey' has not been implemented!");
		}

		public static System.Byte[] _GetKeySetSecurityInfo(System.Security.Cryptography.SafeProvHandle hProv, System.Security.AccessControl.SecurityInfos securityInfo, System.Int32* error)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._GetKeySetSecurityInfo' has not been implemented!");
		}

		public static System.Object _GetProviderParameter(System.Security.Cryptography.SafeProvHandle hProv, System.Int32 keyNumber, System.UInt32 paramID)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._GetProviderParameter' has not been implemented!");
		}

		public static System.Void _ImportBulkKey(System.Security.Cryptography.SafeProvHandle hProv, System.Int32 algid, System.Boolean useSalt, System.Byte[] key, System.Security.Cryptography.SafeKeyHandle* hKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._ImportBulkKey' has not been implemented!");
		}

		public static System.Int32 _ImportCspBlob(System.Byte[] keyBlob, System.Security.Cryptography.SafeProvHandle hProv, System.Security.Cryptography.CspProviderFlags flags, System.Security.Cryptography.SafeKeyHandle* hKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._ImportCspBlob' has not been implemented!");
		}

		public static System.Void _ImportKey(System.Security.Cryptography.SafeProvHandle hCSP, System.Int32 keyNumber, System.Security.Cryptography.CspProviderFlags flags, System.Object cspObject, System.Security.Cryptography.SafeKeyHandle* hKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._ImportKey' has not been implemented!");
		}

		public static System.Boolean _ProduceLegacyHmacValues()
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils._ProduceLegacyHmacValues' has not been implemented!");
		}

		public static System.Void ExportCspBlob(System.Security.Cryptography.SafeKeyHandle hKey, System.Int32 blobType, System.Runtime.CompilerServices.ObjectHandleOnStack retBlob)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.ExportCspBlob' has not been implemented!");
		}

		public static System.Boolean SearchForAlgorithm(System.Security.Cryptography.SafeProvHandle hProv, System.Int32 algID, System.Int32 keyLength)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.SearchForAlgorithm' has not been implemented!");
		}

		public static System.Int32 SetKeySetSecurityInfo(System.Security.Cryptography.SafeProvHandle hProv, System.Security.AccessControl.SecurityInfos securityInfo, System.Byte[] sd)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.SetKeySetSecurityInfo' has not been implemented!");
		}

		public static System.Void SignValue(System.Security.Cryptography.SafeKeyHandle hKey, System.Int32 keyNumber, System.Int32 calgKey, System.Int32 calgHash, System.Byte[] hash, System.Int32 cbHash, System.Runtime.CompilerServices.ObjectHandleOnStack retSignature)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.SignValue' has not been implemented!");
		}

		public static System.Boolean VerifySign(System.Security.Cryptography.SafeKeyHandle hKey, System.Int32 calgKey, System.Int32 calgHash, System.Byte[] hash, System.Int32 cbHash, System.Byte[] signature, System.Int32 cbSignature)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.Utils.VerifySign' has not been implemented!");
		}
	}
}
