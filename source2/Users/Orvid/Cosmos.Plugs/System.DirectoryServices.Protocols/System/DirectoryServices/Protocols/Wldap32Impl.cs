namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.DirectoryServices.Protocols.Wldap32), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_DirectoryServices_Protocols_Wldap32Impl
	{

		public static System.Int32 ldap_bind_s(System.IntPtr ldapHandle, System.String dn, System.DirectoryServices.Protocols.SEC_WINNT_AUTH_IDENTITY_EX credentials, System.DirectoryServices.Protocols.BindMethod method)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_bind_s' has not been implemented!");
		}

		public static System.IntPtr ldap_init(System.String hostName, System.Int32 portNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_init' has not been implemented!");
		}

		public static System.Int32 ldap_connect(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LDAP_TIMEVAL timeout)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_connect' has not been implemented!");
		}

		public static System.Int32 ldap_unbind(System.IntPtr ldapHandle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_unbind' has not been implemented!");
		}

		public static System.Int32 ldap_get_option_int(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.Int32* outValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_get_option_int' has not been implemented!");
		}

		public static System.Int32 ldap_set_option_int(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.Int32* inValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_set_option_int' has not been implemented!");
		}

		public static System.Int32 ldap_get_option_ptr(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.IntPtr* outValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_get_option_ptr' has not been implemented!");
		}

		public static System.Int32 ldap_set_option_ptr(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.IntPtr* inValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_set_option_ptr' has not been implemented!");
		}

		public static System.Int32 ldap_get_option_sechandle(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.DirectoryServices.Protocols.SecurityHandle* outValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_get_option_sechandle' has not been implemented!");
		}

		public static System.Int32 ldap_get_option_secInfo(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.DirectoryServices.Protocols.SecurityPackageContextConnectionInformation outValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_get_option_secInfo' has not been implemented!");
		}

		public static System.Int32 ldap_set_option_referral(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.DirectoryServices.Protocols.LdapReferralCallback* outValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_set_option_referral' has not been implemented!");
		}

		public static System.Int32 ldap_set_option_clientcert(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.DirectoryServices.Protocols.QUERYCLIENTCERT outValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_set_option_clientcert' has not been implemented!");
		}

		public static System.Int32 ldap_set_option_servercert(System.IntPtr ldapHandle, System.DirectoryServices.Protocols.LdapOption option, System.DirectoryServices.Protocols.VERIFYSERVERCERT outValue)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_set_option_servercert' has not been implemented!");
		}

		public static System.Int32 LdapGetLastError()
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.LdapGetLastError' has not been implemented!");
		}

		public static System.IntPtr cldap_open(System.String hostName, System.Int32 portNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.cldap_open' has not been implemented!");
		}

		public static System.Int32 ldap_simple_bind_s(System.IntPtr ldapHandle, System.String distinguishedName, System.String password)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_simple_bind_s' has not been implemented!");
		}

		public static System.Int32 ldap_delete_ext(System.IntPtr ldapHandle, System.String dn, System.IntPtr servercontrol, System.IntPtr clientcontrol, System.Int32* messageNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_delete_ext' has not been implemented!");
		}

		public static System.Int32 ldap_result(System.IntPtr ldapHandle, System.Int32 messageId, System.Int32 all, System.DirectoryServices.Protocols.LDAP_TIMEVAL timeout, System.IntPtr* Mesage)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_result' has not been implemented!");
		}

		public static System.Int32 ldap_parse_result(System.IntPtr ldapHandle, System.IntPtr result, System.Int32* serverError, System.IntPtr* dn, System.IntPtr* message, System.IntPtr* referral, System.IntPtr* control, System.Byte freeIt)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_parse_result' has not been implemented!");
		}

		public static System.Int32 ldap_parse_result_referral(System.IntPtr ldapHandle, System.IntPtr result, System.IntPtr serverError, System.IntPtr dn, System.IntPtr message, System.IntPtr* referral, System.IntPtr control, System.Byte freeIt)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_parse_result_referral' has not been implemented!");
		}

		public static System.Void ldap_memfree(System.IntPtr value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_memfree' has not been implemented!");
		}

		public static System.Int32 ldap_value_free(System.IntPtr value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_value_free' has not been implemented!");
		}

		public static System.Int32 ldap_controls_free(System.IntPtr value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_controls_free' has not been implemented!");
		}

		public static System.Int32 ldap_abandon(System.IntPtr ldapHandle, System.Int32 messagId)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_abandon' has not been implemented!");
		}

		public static System.Int32 ldap_start_tls(System.IntPtr ldapHandle, System.Int32* ServerReturnValue, System.IntPtr* Message, System.IntPtr ServerControls, System.IntPtr ClientControls)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_start_tls' has not been implemented!");
		}

		public static System.Byte ldap_stop_tls(System.IntPtr ldapHandle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_stop_tls' has not been implemented!");
		}

		public static System.Int32 ldap_rename(System.IntPtr ldapHandle, System.String dn, System.String newRdn, System.String newParentDn, System.Int32 deleteOldRdn, System.IntPtr servercontrol, System.IntPtr clientcontrol, System.Int32* messageNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_rename' has not been implemented!");
		}

		public static System.Int32 ldap_compare(System.IntPtr ldapHandle, System.String dn, System.String attributeName, System.String strValue, System.DirectoryServices.Protocols.berval binaryValue, System.IntPtr servercontrol, System.IntPtr clientcontrol, System.Int32* messageNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_compare' has not been implemented!");
		}

		public static System.Int32 ldap_add(System.IntPtr ldapHandle, System.String dn, System.IntPtr attrs, System.IntPtr servercontrol, System.IntPtr clientcontrol, System.Int32* messageNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_add' has not been implemented!");
		}

		public static System.Int32 ldap_modify(System.IntPtr ldapHandle, System.String dn, System.IntPtr attrs, System.IntPtr servercontrol, System.IntPtr clientcontrol, System.Int32* messageNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_modify' has not been implemented!");
		}

		public static System.Int32 ldap_extended_operation(System.IntPtr ldapHandle, System.String oid, System.DirectoryServices.Protocols.berval data, System.IntPtr servercontrol, System.IntPtr clientcontrol, System.Int32* messageNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_extended_operation' has not been implemented!");
		}

		public static System.Int32 ldap_parse_extended_result(System.IntPtr ldapHandle, System.IntPtr result, System.IntPtr* oid, System.IntPtr* data, System.Byte freeIt)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_parse_extended_result' has not been implemented!");
		}

		public static System.Int32 ldap_msgfree(System.IntPtr result)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_msgfree' has not been implemented!");
		}

		public static System.Int32 ldap_search(System.IntPtr ldapHandle, System.String dn, System.Int32 scope, System.String filter, System.IntPtr attributes, System.Boolean attributeOnly, System.IntPtr servercontrol, System.IntPtr clientcontrol, System.Int32 timelimit, System.Int32 sizelimit, System.Int32* messageNumber)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_search' has not been implemented!");
		}

		public static System.IntPtr ldap_first_entry(System.IntPtr ldapHandle, System.IntPtr result)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_first_entry' has not been implemented!");
		}

		public static System.IntPtr ldap_next_entry(System.IntPtr ldapHandle, System.IntPtr result)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_next_entry' has not been implemented!");
		}

		public static System.IntPtr ldap_first_reference(System.IntPtr ldapHandle, System.IntPtr result)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_first_reference' has not been implemented!");
		}

		public static System.IntPtr ldap_next_reference(System.IntPtr ldapHandle, System.IntPtr result)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_next_reference' has not been implemented!");
		}

		public static System.IntPtr ldap_get_dn(System.IntPtr ldapHandle, System.IntPtr result)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_get_dn' has not been implemented!");
		}

		public static System.IntPtr ldap_first_attribute(System.IntPtr ldapHandle, System.IntPtr result, System.IntPtr* address)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_first_attribute' has not been implemented!");
		}

		public static System.IntPtr ldap_next_attribute(System.IntPtr ldapHandle, System.IntPtr result, System.IntPtr address)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_next_attribute' has not been implemented!");
		}

		public static System.IntPtr ber_free(System.IntPtr berelement, System.Int32 option)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_free' has not been implemented!");
		}

		public static System.IntPtr ldap_get_values_len(System.IntPtr ldapHandle, System.IntPtr result, System.String name)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_get_values_len' has not been implemented!");
		}

		public static System.IntPtr ldap_value_free_len(System.IntPtr berelement)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_value_free_len' has not been implemented!");
		}

		public static System.Int32 ldap_parse_reference(System.IntPtr ldapHandle, System.IntPtr result, System.IntPtr* referrals)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_parse_reference' has not been implemented!");
		}

		public static System.IntPtr ber_alloc(System.Int32 option)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_alloc' has not been implemented!");
		}

		public static System.Int32 ber_printf_emptyarg(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.String format)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_printf_emptyarg' has not been implemented!");
		}

		public static System.Int32 ber_printf_int(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.String format, System.Int32 value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_printf_int' has not been implemented!");
		}

		public static System.Int32 ber_printf_bytearray(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.String format, System.DirectoryServices.Protocols.HGlobalMemHandle value, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_printf_bytearray' has not been implemented!");
		}

		public static System.Int32 ber_printf_berarray(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.String format, System.IntPtr value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_printf_berarray' has not been implemented!");
		}

		public static System.Int32 ber_flatten(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.IntPtr* value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_flatten' has not been implemented!");
		}

		public static System.IntPtr ber_init(System.DirectoryServices.Protocols.berval value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_init' has not been implemented!");
		}

		public static System.Int32 ber_scanf(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.String format)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_scanf' has not been implemented!");
		}

		public static System.Int32 ber_scanf_int(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.String format, System.Int32* value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_scanf_int' has not been implemented!");
		}

		public static System.Int32 ber_scanf_ptr(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.String format, System.IntPtr* value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_scanf_ptr' has not been implemented!");
		}

		public static System.Int32 ber_scanf_bitstring(System.DirectoryServices.Protocols.BerSafeHandle berElement, System.String format, System.IntPtr* value, System.Int32* length)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_scanf_bitstring' has not been implemented!");
		}

		public static System.Int32 ber_bvfree(System.IntPtr value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_bvfree' has not been implemented!");
		}

		public static System.Int32 ber_bvecfree(System.IntPtr value)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ber_bvecfree' has not been implemented!");
		}

		public static System.Int32 ldap_create_sort_control(System.DirectoryServices.Protocols.ConnectionHandle handle, System.IntPtr keys, System.Byte critical, System.IntPtr* control)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_create_sort_control' has not been implemented!");
		}

		public static System.Int32 ldap_control_free(System.IntPtr control)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_control_free' has not been implemented!");
		}

		public static System.Int32 CertFreeCRLContext(System.IntPtr certContext)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.CertFreeCRLContext' has not been implemented!");
		}

		public static System.Int32 ldap_result2error(System.IntPtr ldapHandle, System.IntPtr result, System.Int32 freeIt)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Protocols.Wldap32.ldap_result2error' has not been implemented!");
		}
	}
}
