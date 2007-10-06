using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86.CustomImplementations.System;

namespace Indy.IL2CPU.IL.NativeX86.CustomImplementations.System {
	public static class ConsoleImpl {
		public const int Columns = 80;
		public const int Lines = 24;
		public const int Attribute = 7;
		public const int VideoAddr = 0xB8000;

		private static int mCurrentLine = 0;
		private static int mCurrentChar = 0;

		public static unsafe void Clear() {
			for (int i = 0; i < Columns * Lines * 2; i++) {
				byte* xScreenPtr = (byte*)VideoAddr;
				xScreenPtr += i;
				*xScreenPtr = 0;
			}
			mCurrentLine = 0;
			mCurrentChar = 0;
		}

		private unsafe static void PutChar(int aLine, int aPos, byte aChar) {
			int xScreenOffset = ((aPos + aLine * 80) * 2);
			byte* xScreenPtr = (byte*)((0xB8000) + xScreenOffset);
			byte xVal = (byte)((aChar + 1) & 0xFF);
			*xScreenPtr = xVal;
			xScreenPtr += 1;
			*xScreenPtr = 7;
		}

		public static void OutputBytes(byte[] aBytes) {
			for (int i = 0; i < aBytes.Length; i++) {
				OutputByte(aBytes[i]);
			}
		}

		public static void OutputByteValue(byte aByte) {
			switch (aByte) {
				case 0:
					OutputString("0x00");
					break;
				case 1:
					OutputString("0x01");
					break;
				case 2:
					OutputString("0x02");
					break;
				case 3:
					OutputString("0x03");
					break;
				case 4:
					OutputString("0x04");
					break;
				case 5:
					OutputString("0x05");
					break;
				case 6:
					OutputString("0x06");
					break;
				case 7:
					OutputString("0x07");
					break;
				case 8:
					OutputString("0x08");
					break;
				case 9:
					OutputString("0x09");
					break;
				case 10:
					OutputString("0x0A");
					break;
				case 11:
					OutputString("0x0B");
					break;
				case 12:
					OutputString("0x0C");
					break;
				case 13:
					OutputString("0x0D");
					break;
				case 14:
					OutputString("0x0E");
					break;
				case 15:
					OutputString("0x0F");
					break;
				case 16:
					OutputString("0x10");
					break;
				case 17:
					OutputString("0x11");
					break;
				case 18:
					OutputString("0x12");
					break;
				case 19:
					OutputString("0x13");
					break;
				case 20:
					OutputString("0x14");
					break;
				case 21:
					OutputString("0x15");
					break;
				case 22:
					OutputString("0x16");
					break;
				case 23:
					OutputString("0x17");
					break;
				case 24:
					OutputString("0x18");
					break;
				case 25:
					OutputString("0x19");
					break;
				case 26:
					OutputString("0x1A");
					break;
				case 27:
					OutputString("0x1B");
					break;
				case 28:
					OutputString("0x1C");
					break;
				case 29:
					OutputString("0x1D");
					break;
				case 30:
					OutputString("0x1E");
					break;
				case 31:
					OutputString("0x1F");
					break;
				case 32:
					OutputString("0x20");
					break;
				case 33:
					OutputString("0x21");
					break;
				case 34:
					OutputString("0x22");
					break;
				case 35:
					OutputString("0x23");
					break;
				case 36:
					OutputString("0x24");
					break;
				case 37:
					OutputString("0x25");
					break;
				case 38:
					OutputString("0x26");
					break;
				case 39:
					OutputString("0x27");
					break;
				case 40:
					OutputString("0x28");
					break;
				case 41:
					OutputString("0x29");
					break;
				case 42:
					OutputString("0x2A");
					break;
				case 43:
					OutputString("0x2B");
					break;
				case 44:
					OutputString("0x2C");
					break;
				case 45:
					OutputString("0x2D");
					break;
				case 46:
					OutputString("0x2E");
					break;
				case 47:
					OutputString("0x2F");
					break;
				case 48:
					OutputString("0x30");
					break;
				case 49:
					OutputString("0x31");
					break;
				case 50:
					OutputString("0x32");
					break;
				case 51:
					OutputString("0x33");
					break;
				case 52:
					OutputString("0x34");
					break;
				case 53:
					OutputString("0x35");
					break;
				case 54:
					OutputString("0x36");
					break;
				case 55:
					OutputString("0x37");
					break;
				case 56:
					OutputString("0x38");
					break;
				case 57:
					OutputString("0x39");
					break;
				case 58:
					OutputString("0x3A");
					break;
				case 59:
					OutputString("0x3B");
					break;
				case 60:
					OutputString("0x3C");
					break;
				case 61:
					OutputString("0x3D");
					break;
				case 62:
					OutputString("0x3E");
					break;
				case 63:
					OutputString("0x3F");
					break;
				case 64:
					OutputString("0x40");
					break;
				case 65:
					OutputString("0x41");
					break;
				case 66:
					OutputString("0x42");
					break;
				case 67:
					OutputString("0x43");
					break;
				case 68:
					OutputString("0x44");
					break;
				case 69:
					OutputString("0x45");
					break;
				case 70:
					OutputString("0x46");
					break;
				case 71:
					OutputString("0x47");
					break;
				case 72:
					OutputString("0x48");
					break;
				case 73:
					OutputString("0x49");
					break;
				case 74:
					OutputString("0x4A");
					break;
				case 75:
					OutputString("0x4B");
					break;
				case 76:
					OutputString("0x4C");
					break;
				case 77:
					OutputString("0x4D");
					break;
				case 78:
					OutputString("0x4E");
					break;
				case 79:
					OutputString("0x4F");
					break;
				case 80:
					OutputString("0x50");
					break;
				case 81:
					OutputString("0x51");
					break;
				case 82:
					OutputString("0x52");
					break;
				case 83:
					OutputString("0x53");
					break;
				case 84:
					OutputString("0x54");
					break;
				case 85:
					OutputString("0x55");
					break;
				case 86:
					OutputString("0x56");
					break;
				case 87:
					OutputString("0x57");
					break;
				case 88:
					OutputString("0x58");
					break;
				case 89:
					OutputString("0x59");
					break;
				case 90:
					OutputString("0x5A");
					break;
				case 91:
					OutputString("0x5B");
					break;
				case 92:
					OutputString("0x5C");
					break;
				case 93:
					OutputString("0x5D");
					break;
				case 94:
					OutputString("0x5E");
					break;
				case 95:
					OutputString("0x5F");
					break;
				case 96:
					OutputString("0x60");
					break;
				case 97:
					OutputString("0x61");
					break;
				case 98:
					OutputString("0x62");
					break;
				case 99:
					OutputString("0x63");
					break;
				case 100:
					OutputString("0x64");
					break;
				case 101:
					OutputString("0x65");
					break;
				case 102:
					OutputString("0x66");
					break;
				case 103:
					OutputString("0x67");
					break;
				case 104:
					OutputString("0x68");
					break;
				case 105:
					OutputString("0x69");
					break;
				case 106:
					OutputString("0x6A");
					break;
				case 107:
					OutputString("0x6B");
					break;
				case 108:
					OutputString("0x6C");
					break;
				case 109:
					OutputString("0x6D");
					break;
				case 110:
					OutputString("0x6E");
					break;
				case 111:
					OutputString("0x6F");
					break;
				case 112:
					OutputString("0x70");
					break;
				case 113:
					OutputString("0x71");
					break;
				case 114:
					OutputString("0x72");
					break;
				case 115:
					OutputString("0x73");
					break;
				case 116:
					OutputString("0x74");
					break;
				case 117:
					OutputString("0x75");
					break;
				case 118:
					OutputString("0x76");
					break;
				case 119:
					OutputString("0x77");
					break;
				case 120:
					OutputString("0x78");
					break;
				case 121:
					OutputString("0x79");
					break;
				case 122:
					OutputString("0x7A");
					break;
				case 123:
					OutputString("0x7B");
					break;
				case 124:
					OutputString("0x7C");
					break;
				case 125:
					OutputString("0x7D");
					break;
				case 126:
					OutputString("0x7E");
					break;
				case 127:
					OutputString("0x7F");
					break;
				case 128:
					OutputString("0x80");
					break;
				case 129:
					OutputString("0x81");
					break;
				case 130:
					OutputString("0x82");
					break;
				case 131:
					OutputString("0x83");
					break;
				case 132:
					OutputString("0x84");
					break;
				case 133:
					OutputString("0x85");
					break;
				case 134:
					OutputString("0x86");
					break;
				case 135:
					OutputString("0x87");
					break;
				case 136:
					OutputString("0x88");
					break;
				case 137:
					OutputString("0x89");
					break;
				case 138:
					OutputString("0x8A");
					break;
				case 139:
					OutputString("0x8B");
					break;
				case 140:
					OutputString("0x8C");
					break;
				case 141:
					OutputString("0x8D");
					break;
				case 142:
					OutputString("0x8E");
					break;
				case 143:
					OutputString("0x8F");
					break;
				case 144:
					OutputString("0x90");
					break;
				case 145:
					OutputString("0x91");
					break;
				case 146:
					OutputString("0x92");
					break;
				case 147:
					OutputString("0x93");
					break;
				case 148:
					OutputString("0x94");
					break;
				case 149:
					OutputString("0x95");
					break;
				case 150:
					OutputString("0x96");
					break;
				case 151:
					OutputString("0x97");
					break;
				case 152:
					OutputString("0x98");
					break;
				case 153:
					OutputString("0x99");
					break;
				case 154:
					OutputString("0x9A");
					break;
				case 155:
					OutputString("0x9B");
					break;
				case 156:
					OutputString("0x9C");
					break;
				case 157:
					OutputString("0x9D");
					break;
				case 158:
					OutputString("0x9E");
					break;
				case 159:
					OutputString("0x9F");
					break;
				case 160:
					OutputString("0xA0");
					break;
				case 161:
					OutputString("0xA1");
					break;
				case 162:
					OutputString("0xA2");
					break;
				case 163:
					OutputString("0xA3");
					break;
				case 164:
					OutputString("0xA4");
					break;
				case 165:
					OutputString("0xA5");
					break;
				case 166:
					OutputString("0xA6");
					break;
				case 167:
					OutputString("0xA7");
					break;
				case 168:
					OutputString("0xA8");
					break;
				case 169:
					OutputString("0xA9");
					break;
				case 170:
					OutputString("0xAA");
					break;
				case 171:
					OutputString("0xAB");
					break;
				case 172:
					OutputString("0xAC");
					break;
				case 173:
					OutputString("0xAD");
					break;
				case 174:
					OutputString("0xAE");
					break;
				case 175:
					OutputString("0xAF");
					break;
				case 176:
					OutputString("0xB0");
					break;
				case 177:
					OutputString("0xB1");
					break;
				case 178:
					OutputString("0xB2");
					break;
				case 179:
					OutputString("0xB3");
					break;
				case 180:
					OutputString("0xB4");
					break;
				case 181:
					OutputString("0xB5");
					break;
				case 182:
					OutputString("0xB6");
					break;
				case 183:
					OutputString("0xB7");
					break;
				case 184:
					OutputString("0xB8");
					break;
				case 185:
					OutputString("0xB9");
					break;
				case 186:
					OutputString("0xBA");
					break;
				case 187:
					OutputString("0xBB");
					break;
				case 188:
					OutputString("0xBC");
					break;
				case 189:
					OutputString("0xBD");
					break;
				case 190:
					OutputString("0xBE");
					break;
				case 191:
					OutputString("0xBF");
					break;
				case 192:
					OutputString("0xC0");
					break;
				case 193:
					OutputString("0xC1");
					break;
				case 194:
					OutputString("0xC2");
					break;
				case 195:
					OutputString("0xC3");
					break;
				case 196:
					OutputString("0xC4");
					break;
				case 197:
					OutputString("0xC5");
					break;
				case 198:
					OutputString("0xC6");
					break;
				case 199:
					OutputString("0xC7");
					break;
				case 200:
					OutputString("0xC8");
					break;
				case 201:
					OutputString("0xC9");
					break;
				case 202:
					OutputString("0xCA");
					break;
				case 203:
					OutputString("0xCB");
					break;
				case 204:
					OutputString("0xCC");
					break;
				case 205:
					OutputString("0xCD");
					break;
				case 206:
					OutputString("0xCE");
					break;
				case 207:
					OutputString("0xCF");
					break;
				case 208:
					OutputString("0xD0");
					break;
				case 209:
					OutputString("0xD1");
					break;
				case 210:
					OutputString("0xD2");
					break;
				case 211:
					OutputString("0xD3");
					break;
				case 212:
					OutputString("0xD4");
					break;
				case 213:
					OutputString("0xD5");
					break;
				case 214:
					OutputString("0xD6");
					break;
				case 215:
					OutputString("0xD7");
					break;
				case 216:
					OutputString("0xD8");
					break;
				case 217:
					OutputString("0xD9");
					break;
				case 218:
					OutputString("0xDA");
					break;
				case 219:
					OutputString("0xDB");
					break;
				case 220:
					OutputString("0xDC");
					break;
				case 221:
					OutputString("0xDD");
					break;
				case 222:
					OutputString("0xDE");
					break;
				case 223:
					OutputString("0xDF");
					break;
				case 224:
					OutputString("0xE0");
					break;
				case 225:
					OutputString("0xE1");
					break;
				case 226:
					OutputString("0xE2");
					break;
				case 227:
					OutputString("0xE3");
					break;
				case 228:
					OutputString("0xE4");
					break;
				case 229:
					OutputString("0xE5");
					break;
				case 230:
					OutputString("0xE6");
					break;
				case 231:
					OutputString("0xE7");
					break;
				case 232:
					OutputString("0xE8");
					break;
				case 233:
					OutputString("0xE9");
					break;
				case 234:
					OutputString("0xEA");
					break;
				case 235:
					OutputString("0xEB");
					break;
				case 236:
					OutputString("0xEC");
					break;
				case 237:
					OutputString("0xED");
					break;
				case 238:
					OutputString("0xEE");
					break;
				case 239:
					OutputString("0xEF");
					break;
				case 240:
					OutputString("0xF0");
					break;
				case 241:
					OutputString("0xF1");
					break;
				case 242:
					OutputString("0xF2");
					break;
				case 243:
					OutputString("0xF3");
					break;
				case 244:
					OutputString("0xF4");
					break;
				case 245:
					OutputString("0xF5");
					break;
				case 246:
					OutputString("0xF6");
					break;
				case 247:
					OutputString("0xF7");
					break;
				case 248:
					OutputString("0xF8");
					break;
				case 249:
					OutputString("0xF9");
					break;
				case 250:
					OutputString("0xFA");
					break;
				case 251:
					OutputString("0xFB");
					break;
				case 252:
					OutputString("0xFC");
					break;
				case 253:
					OutputString("0xFD");
					break;
				case 254:
					OutputString("0xFE");
					break;
				case 255:
					OutputString("0xFF");
					break;
			}
		}

		private static void OutputByte(byte aByte) {
			PutChar(mCurrentLine, mCurrentChar, aByte);
			mCurrentChar += 1;
			if (mCurrentChar == Columns) {
				mCurrentChar = 0;
				mCurrentLine += 1;
				if (mCurrentLine == Lines) {
					Clear();
				}
			}
		}

		private static void OutputString(string aText) {
			for (int i = 0; i < aText.Length; i++) {
				OutputByte(StringImpl.GetByteFromChar(aText[i]));
			}
		}

		public static void Write(string aText) {
			OutputString(aText);
		}

		public static void WriteLine(string aLine) {
			OutputString(aLine);
			mCurrentLine += 1;
			mCurrentChar = 0;
			if (mCurrentLine == Lines) {
				Clear();
			}
		}
	}
}