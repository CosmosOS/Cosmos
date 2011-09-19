namespace Cosmos.Plugs
{
    //[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.DirectoryServices.AccountManagement.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
    //public static class System_DirectoryServices_AccountManagement_UnsafeNativeMethodsImpl
    //{

    //    public static System.Int32 IntADsOpenObject(System.String path, System.String userName, System.String password, System.Int32 flags, System.Guid* iid, System.Object* ppObject)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.IntADsOpenObject' has not been implemented!");
    //    }

    //    public static System.Int32 DsRoleGetPrimaryDomainInformation(System.String lpServer, System.DirectoryServices.AccountManagement.UnsafeNativeMethods+DSROLE_PRIMARY_DOMAIN_INFO_LEVEL InfoLevel, System.IntPtr* Buffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.DsRoleGetPrimaryDomainInformation' has not been implemented!");
    //    }

    //    public static System.Int32 DsRoleGetPrimaryDomainInformation(System.IntPtr lpServer, System.DirectoryServices.AccountManagement.UnsafeNativeMethods+DSROLE_PRIMARY_DOMAIN_INFO_LEVEL InfoLevel, System.IntPtr* Buffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.DsRoleGetPrimaryDomainInformation' has not been implemented!");
    //    }

    //    public static System.Int32 DsRoleFreeMemory(System.IntPtr buffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.DsRoleFreeMemory' has not been implemented!");
    //    }

    //    public static System.Int32 DsGetDcName(System.String computerName, System.String domainName, System.IntPtr domainGuid, System.String siteName, System.Int32 flags, System.IntPtr* domainControllerInfo)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.DsGetDcName' has not been implemented!");
    //    }

    //    public static System.Int32 NetWkstaGetInfo(System.String server, System.Int32 level, System.IntPtr* buffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.NetWkstaGetInfo' has not been implemented!");
    //    }

    //    public static System.Int32 NetApiBufferFree(System.IntPtr buffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.NetApiBufferFree' has not been implemented!");
    //    }

    //    public static System.Boolean ConvertSidToStringSid(System.IntPtr sid, System.String* stringSid)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.ConvertSidToStringSid' has not been implemented!");
    //    }

    //    public static System.Boolean ConvertStringSidToSid(System.String stringSid, System.IntPtr* sid)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.ConvertStringSidToSid' has not been implemented!");
    //    }

    //    public static System.Int32 GetLengthSid(System.IntPtr sid)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.GetLengthSid' has not been implemented!");
    //    }

    //    public static System.Boolean IsValidSid(System.IntPtr sid)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.IsValidSid' has not been implemented!");
    //    }

    //    public static System.IntPtr GetSidIdentifierAuthority(System.IntPtr sid)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.GetSidIdentifierAuthority' has not been implemented!");
    //    }

    //    public static System.IntPtr GetSidSubAuthority(System.IntPtr sid, System.Int32 index)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.GetSidSubAuthority' has not been implemented!");
    //    }

    //    public static System.IntPtr GetSidSubAuthorityCount(System.IntPtr sid)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.GetSidSubAuthorityCount' has not been implemented!");
    //    }

    //    public static System.Boolean EqualDomainSid(System.IntPtr pSid1, System.IntPtr pSid2, System.Boolean* equal)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.EqualDomainSid' has not been implemented!");
    //    }

    //    public static System.Int32 CopySid(System.Int32 destinationLength, System.IntPtr pSidDestination, System.IntPtr pSidSource)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.CopySid' has not been implemented!");
    //    }

    //    public static System.IntPtr LocalFree(System.IntPtr ptr)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LocalFree' has not been implemented!");
    //    }

    //    public static System.Int32 CredUIParseUserName(System.String pszUserName, System.Text.StringBuilder pszUser, System.UInt32 ulUserMaxChars, System.Text.StringBuilder pszDomain, System.UInt32 ulDomainMaxChars)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.CredUIParseUserName' has not been implemented!");
    //    }

    //    public static System.Boolean LookupAccountSid(System.String computerName, System.IntPtr sid, System.Text.StringBuilder name, System.Int32* nameLength, System.Text.StringBuilder domainName, System.Int32* domainNameLength, System.Int32* usage)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LookupAccountSid' has not been implemented!");
    //    }

    //    public static System.Boolean AuthzInitializeResourceManager(System.Int32 flags, System.IntPtr pfnAccessCheck, System.IntPtr pfnComputeDynamicGroups, System.IntPtr pfnFreeDynamicGroups, System.String name, System.IntPtr* rm)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.AuthzInitializeResourceManager' has not been implemented!");
    //    }

    //    public static System.Boolean AuthzInitializeContextFromSid(System.Int32 Flags, System.IntPtr UserSid, System.IntPtr AuthzResourceManager, System.IntPtr pExpirationTime, System.DirectoryServices.AccountManagement.UnsafeNativeMethods+LUID Identitifier, System.IntPtr DynamicGroupArgs, System.IntPtr* pAuthzClientContext)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.AuthzInitializeContextFromSid' has not been implemented!");
    //    }

    //    public static System.Boolean AuthzGetInformationFromContext(System.IntPtr hAuthzClientContext, System.Int32 InfoClass, System.Int32 BufferSize, System.Int32* pSizeRequired, System.IntPtr Buffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.AuthzGetInformationFromContext' has not been implemented!");
    //    }

    //    public static System.Boolean AuthzFreeContext(System.IntPtr AuthzClientContext)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.AuthzFreeContext' has not been implemented!");
    //    }

    //    public static System.Boolean AuthzFreeResourceManager(System.IntPtr rm)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.AuthzFreeResourceManager' has not been implemented!");
    //    }

    //    public static System.Boolean OpenThreadToken(System.IntPtr threadHandle, System.Int32 desiredAccess, System.Boolean openAsSelf, System.IntPtr* tokenHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.OpenThreadToken' has not been implemented!");
    //    }

    //    public static System.Boolean OpenProcessToken(System.IntPtr processHandle, System.Int32 desiredAccess, System.IntPtr* tokenHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.OpenProcessToken' has not been implemented!");
    //    }

    //    public static System.Boolean CloseHandle(System.IntPtr handle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.CloseHandle' has not been implemented!");
    //    }

    //    public static System.IntPtr GetCurrentThread()
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.GetCurrentThread' has not been implemented!");
    //    }

    //    public static System.IntPtr GetCurrentProcess()
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.GetCurrentProcess' has not been implemented!");
    //    }

    //    public static System.Boolean GetTokenInformation(System.IntPtr tokenHandle, System.Int32 tokenInformationClass, System.IntPtr buffer, System.Int32 bufferSize, System.Int32* returnLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.GetTokenInformation' has not been implemented!");
    //    }

    //    public static System.Int32 LsaOpenPolicy(System.IntPtr lsaUnicodeString, System.IntPtr lsaObjectAttributes, System.Int32 desiredAccess, System.IntPtr* policyHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LsaOpenPolicy' has not been implemented!");
    //    }

    //    public static System.Int32 LsaOpenPolicy(System.String SystemName, System.IntPtr lsaObjectAttributes, System.Int32 desiredAccess, System.IntPtr* policyHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LsaOpenPolicy' has not been implemented!");
    //    }

    //    public static System.Int32 LsaQueryInformationPolicy(System.IntPtr policyHandle, System.Int32 policyInformationClass, System.IntPtr* buffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LsaQueryInformationPolicy' has not been implemented!");
    //    }

    //    public static System.Int32 LsaLookupSids(System.IntPtr policyHandle, System.Int32 count, System.IntPtr[] sids, System.IntPtr* referencedDomains, System.IntPtr* names)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LsaLookupSids' has not been implemented!");
    //    }

    //    public static System.Int32 LsaFreeMemory(System.IntPtr buffer)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LsaFreeMemory' has not been implemented!");
    //    }

    //    public static System.Int32 LsaClose(System.IntPtr policyHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LsaClose' has not been implemented!");
    //    }

    //    public static System.Int32 LogonUser(System.String lpszUsername, System.String lpszDomain, System.String lpszPassword, System.Int32 dwLogonType, System.Int32 dwLogonProvider, System.IntPtr* phToken)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.LogonUser' has not been implemented!");
    //    }

    //    public static System.Int32 ImpersonateLoggedOnUser(System.IntPtr hToken)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.ImpersonateLoggedOnUser' has not been implemented!");
    //    }

    //    public static System.Int32 RevertToSelf()
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.RevertToSelf' has not been implemented!");
    //    }

    //    public static System.Int32 FormatMessageW(System.Int32 dwFlags, System.IntPtr lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr arguments)
    //    {
    //        throw new System.NotImplementedException("Method 'System.DirectoryServices.AccountManagement.UnsafeNativeMethods.FormatMessageW' has not been implemented!");
    //    }
    //}
}
