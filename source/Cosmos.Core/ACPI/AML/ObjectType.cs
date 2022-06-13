using System;

namespace ACPILib.AML
{
	[Flags]
	public enum ObjectTypeEnum : uint
	{
		Any = 0x00,
		Integer = 0x01,
		String = 0x02,
		Buffer = 0x03,
		Package = 0x04,
		FieldUnit = 0x05,
		Device = 0x06,
		Event = 0x07,
		Method = 0x08,
		Mutex = 0x09,
		Region = 0x0A,
		Power = 0x0B,
		Processor = 0x0C,
		Thermal = 0x0D,
		BufferField = 0x0E,
		DDBHandle = 0x0F,
		DebugObject = 0x10,
		ExternalMax = 0x10,

		LocalRegionFIeld = 0x11,
		LocalBankField = 0x12,
		LocalIndexField = 0x13,
		LocalReference = 0x14,
		LocalAlias = 0x15,
		LocalMethodAlias = 0x16,
		LocalNotify = 0x17,
		LocalAddressHandler = 0x18,
		LocalResource = 0x19,
		LocalResourceField = 0x1A,
		LocalScope = 0x1B,

		NsNodeMax = 0x1B,

		LocalExtra = 0x1C,
		LocalData = 0x1D,

		LocalMax = 0x1D,

		Invalid = 0x1E,
		NotFound = 0xFF,
	}
}
