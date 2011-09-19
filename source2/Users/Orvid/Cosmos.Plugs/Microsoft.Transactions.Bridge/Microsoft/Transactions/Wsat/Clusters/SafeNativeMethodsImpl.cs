namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Transactions_Wsat_Clusters_SafeNativeMethodsImpl
	{

		public static Microsoft.Transactions.Wsat.Clusters.SafeHCluster OpenCluster(System.String lpszClusterName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.OpenCluster' has not been implemented!");
		}

		public static Microsoft.Transactions.Wsat.Clusters.SafeHClusEnum ClusterOpenEnum(Microsoft.Transactions.Wsat.Clusters.SafeHCluster hCluster, Microsoft.Transactions.Wsat.Clusters.ClusterEnum dwType)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.ClusterOpenEnum' has not been implemented!");
		}

		public static System.UInt32 ClusterEnum(Microsoft.Transactions.Wsat.Clusters.SafeHClusEnum hEnum, System.UInt32 dwIndex, System.UInt32* lpdwType, System.Text.StringBuilder lpszName, System.UInt32* lpcchName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.ClusterEnum' has not been implemented!");
		}

		public static System.UInt32 ClusterResourceControl(Microsoft.Transactions.Wsat.Clusters.SafeHResource hResource, System.IntPtr hHostNode, Microsoft.Transactions.Wsat.Clusters.ClusterResourceControlCode dwControlCode, System.IntPtr lpInBuffer, System.UInt32 cbInBufferSize, System.Byte[] buffer, System.UInt32 cbOutBufferSize, System.UInt32* lpcbBytesReturned)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.ClusterResourceControl' has not been implemented!");
		}

		public static Microsoft.Transactions.Wsat.Clusters.SafeHResource OpenClusterResource(Microsoft.Transactions.Wsat.Clusters.SafeHCluster hCluster, System.String lpszResourceName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.OpenClusterResource' has not been implemented!");
		}

		public static System.Boolean GetClusterResourceNetworkName(Microsoft.Transactions.Wsat.Clusters.SafeHResource hResource, System.Text.StringBuilder lpBuffer, System.UInt32* nSize)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.GetClusterResourceNetworkName' has not been implemented!");
		}

		public static Microsoft.Transactions.Wsat.Clusters.SafeHKey GetClusterResourceKey(Microsoft.Transactions.Wsat.Clusters.SafeHResource hResource, System.Security.AccessControl.RegistryRights samDesired)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.GetClusterResourceKey' has not been implemented!");
		}

		public static System.Int32 ClusterRegOpenKey(Microsoft.Transactions.Wsat.Clusters.SafeHKey hKey, System.String lpszSubKey, System.Security.AccessControl.RegistryRights samDesired, Microsoft.Transactions.Wsat.Clusters.SafeHKey* phkResult)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.ClusterRegOpenKey' has not been implemented!");
		}

		public static System.Int32 ClusterRegQueryValue(Microsoft.Transactions.Wsat.Clusters.SafeHKey hKey, System.String lpszValueName, Microsoft.Win32.RegistryValueKind* lpdwValueType, System.Byte[] lpbData, System.UInt32* lpcbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.ClusterRegQueryValue' has not been implemented!");
		}
	}
}
