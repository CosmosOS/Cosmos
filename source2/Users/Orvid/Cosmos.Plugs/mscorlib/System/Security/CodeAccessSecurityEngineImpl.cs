namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.CodeAccessSecurityEngine), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_CodeAccessSecurityEngineImpl
	{

		public static System.Void SpecialDemand(System.Security.PermissionType whatPermission, System.Threading.StackCrawlMark* stackMark)
		{
			throw new System.NotImplementedException("Method 'System.Security.CodeAccessSecurityEngine.SpecialDemand' has not been implemented!");
		}

		public static System.Void Check(System.Object demand, System.Threading.StackCrawlMark* stackMark, System.Boolean isPermSet)
		{
			throw new System.NotImplementedException("Method 'System.Security.CodeAccessSecurityEngine.Check' has not been implemented!");
		}

		public static System.Boolean QuickCheckForAllDemands()
		{
			throw new System.NotImplementedException("Method 'System.Security.CodeAccessSecurityEngine.QuickCheckForAllDemands' has not been implemented!");
		}

		public static System.Security.FrameSecurityDescriptor CheckNReturnSO(System.Security.PermissionToken permToken, System.Security.CodeAccessPermission demand, System.Threading.StackCrawlMark* stackMark, System.Int32 create)
		{
			throw new System.NotImplementedException("Method 'System.Security.CodeAccessSecurityEngine.CheckNReturnSO' has not been implemented!");
		}

		public static System.Void _GetGrantedPermissionSet(System.IntPtr secDesc, System.Security.PermissionSet* grants, System.Security.PermissionSet* refused)
		{
			throw new System.NotImplementedException("Method 'System.Security.CodeAccessSecurityEngine._GetGrantedPermissionSet' has not been implemented!");
		}

		public static System.Boolean AllDomainsHomogeneousWithNoStackModifiers()
		{
			throw new System.NotImplementedException("Method 'System.Security.CodeAccessSecurityEngine.AllDomainsHomogeneousWithNoStackModifiers' has not been implemented!");
		}

		public static System.Void GetZoneAndOriginInternal(System.Collections.ArrayList zoneList, System.Collections.ArrayList originList, System.Threading.StackCrawlMark* stackMark)
		{
			throw new System.NotImplementedException("Method 'System.Security.CodeAccessSecurityEngine.GetZoneAndOriginInternal' has not been implemented!");
		}
	}
}
