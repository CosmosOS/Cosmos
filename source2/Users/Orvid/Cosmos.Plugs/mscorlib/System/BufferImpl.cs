namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Buffer), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_BufferImpl
	{

		public static System.Void BlockCopy(System.Array src, System.Int32 srcOffset, System.Array dst, System.Int32 dstOffset, System.Int32 count)
		{
			throw new System.NotImplementedException("Method 'System.Buffer.BlockCopy' has not been implemented!");
		}

		public static System.Void InternalBlockCopy(System.Array src, System.Int32 srcOffsetBytes, System.Array dst, System.Int32 dstOffsetBytes, System.Int32 byteCount)
		{
			throw new System.NotImplementedException("Method 'System.Buffer.InternalBlockCopy' has not been implemented!");
		}

		public static System.Boolean IsPrimitiveTypeArray(System.Array array)
		{
			throw new System.NotImplementedException("Method 'System.Buffer.IsPrimitiveTypeArray' has not been implemented!");
		}

		public static System.Byte _GetByte(System.Array array, System.Int32 index)
		{
			throw new System.NotImplementedException("Method 'System.Buffer._GetByte' has not been implemented!");
		}

		public static System.Void _SetByte(System.Array array, System.Int32 index, System.Byte value)
		{
			throw new System.NotImplementedException("Method 'System.Buffer._SetByte' has not been implemented!");
		}

		public static System.Int32 _ByteLength(System.Array array)
		{
			throw new System.NotImplementedException("Method 'System.Buffer._ByteLength' has not been implemented!");
		}
	}
}
