using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class GDT {
		[StructLayout(LayoutKind.Sequential)]
		public struct GDTEntry {
			public ushort LimitLow;
			public ushort BaseLow;
			public byte BaseMiddle;
			public byte Access;
			public byte Granularity;
			public byte BaseHigh;
		}

		private static void GDT_InitEntry(ref GDTEntry aEntry, uint aBase, uint aLimit, ushort aFlags) {
			aEntry.BaseLow = (ushort)(aBase & 0xFFFF);
			aEntry.BaseMiddle = (byte)((aBase >> 16) & 0xFF);
			aEntry.BaseHigh = (byte)((aBase >> 24) & 0xFF);
			aEntry.LimitLow = (ushort)(aLimit & 0xFFFF);
			aEntry.Granularity = (byte)((aLimit >> 16) & 0x0F);
			aEntry.Granularity |= (byte)((aFlags >> 4) & 0xF0);
			aEntry.Access = (byte)(aFlags & 0xFF);
		}

		[GlueField(FieldType=GlueFieldTypeEnum.GDT_Array)]
		private static GDTEntry[] mGDTEntries;

		public const ushort CodeSelector = 8;
		public const ushort DataSelector = 16;

		[GlueField(FieldType = GlueFieldTypeEnum.GDT_Pointer)]
		private static DTPointerStruct mGDTPointer;


		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.GDT_Register)]
		private static void RegisterGDT() {
		}

		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.GDT_LoadArray)]
		private static void LoadArray() {
		}

		public static void Setup() {
			LoadArray();
			GDT_InitEntry(ref mGDTEntries[0], 0, 0, 0);
			GDT_InitEntry(ref mGDTEntries[CodeSelector >> 3], 0, 0xFFFFFFFF, 0x9A);
			GDT_InitEntry(ref mGDTEntries[DataSelector >> 3], 0, 0xFFFFFFFF, 0x92);
			RegisterGDT();
		}
	}
}