namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Text.Normalization), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Text_NormalizationImpl
	{

		public static System.Boolean nativeLoadNormalizationDLL()
		{
			throw new System.NotImplementedException("Method 'System.Text.Normalization.nativeLoadNormalizationDLL' has not been implemented!");
		}

		public static System.Int32 nativeNormalizationNormalizeString(System.Text.NormalizationForm NormForm, System.Int32* iError, System.String lpSrcString, System.Int32 cwSrcLength, System.Char[] lpDstString, System.Int32 cwDstLength)
		{
			throw new System.NotImplementedException("Method 'System.Text.Normalization.nativeNormalizationNormalizeString' has not been implemented!");
		}

		public static System.Int32 nativeNormalizationIsNormalizedString(System.Text.NormalizationForm NormForm, System.Int32* iError, System.String lpString, System.Int32 cwLength)
		{
			throw new System.NotImplementedException("Method 'System.Text.Normalization.nativeNormalizationIsNormalizedString' has not been implemented!");
		}

		public static System.Byte* nativeNormalizationInitNormalization(System.Text.NormalizationForm NormForm, System.Byte* pTableData)
		{
			throw new System.NotImplementedException("Method 'System.Text.Normalization.nativeNormalizationInitNormalization' has not been implemented!");
		}
	}
}
