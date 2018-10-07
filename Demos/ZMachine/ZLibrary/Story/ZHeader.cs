using ZLibrary.Constants;
using ZLibrary.Machine;

namespace ZLibrary.Story
{
    public class ZHeader
    {
        private readonly ZMemory _memory;

        public byte Version;
        public byte ConfigFlags;
        public ushort Release;
        public ushort DataSize;
        public ushort StartPC;
        public ushort DictionaryOffset;
        public ushort ObjectsOffset;
        public ushort GlobalsOffset;
        public ushort DynamicSize;
        public ushort Flags;
        public byte[] Serial = new byte[6];
        public ushort AbbreviationsOffset;
        public ushort FileSize;
        public ushort Checksum;
        public byte InterpreterNumber = InterpreterType.MSDOS;
        public byte InterpreterVersion;
        public ushort AlphabetOffset;

        public byte screen_cols;
        public byte screen_rows;
        public byte terminating_keys;
        public ushort strings_offset;
        public ushort routines_offset;
        public byte default_background;
        public byte default_foreground;
        public byte standard_high;
        public byte standard_low;
        public byte[] user_name = new byte[8];
        public int screen_height;
        public ushort screen_width;
        public int font_height = 1;
        public int font_width = 1;

        public ZHeader(ZMemory aMemory)
        {
            _memory = aMemory;
            aMemory.GetByte(HeaderOffset.VERSION, out Version);
            aMemory.GetByte(HeaderOffset.CONFIG, out ConfigFlags);
            aMemory.GetWord(HeaderOffset.RELEASE, out Release);
            aMemory.GetWord(HeaderOffset.DATA_SIZE, out DataSize);
            aMemory.GetWord(HeaderOffset.START_PC, out StartPC);
            aMemory.GetWord(HeaderOffset.DICTIONARY_OFFSET, out DictionaryOffset);
            aMemory.GetWord(HeaderOffset.OBJECTS_OFFSET, out ObjectsOffset);
            aMemory.GetWord(HeaderOffset.GLOBALS_OFFSET, out GlobalsOffset);
            aMemory.GetWord(HeaderOffset.DYNAMIC_SIZE, out DynamicSize);
            aMemory.GetWord(HeaderOffset.FLAGS, out Flags);

            for (int i = 0; i < Serial.Length; i++)
            {
                aMemory.GetByte((ushort) (HeaderOffset.SERIAL + i), out Serial[i]);
            }

            aMemory.GetWord(HeaderOffset.ABBREVIATIONS_OFFSET, out AbbreviationsOffset);
            aMemory.GetWord(HeaderOffset.FILE_SIZE, out FileSize);
            aMemory.GetWord(HeaderOffset.CHECKSUM, out Checksum);
            aMemory.GetByte(HeaderOffset.INTERPRETER_NUMBER, out InterpreterNumber);
            aMemory.GetByte(HeaderOffset.INTERPRETER_VERSION, out InterpreterVersion);
            aMemory.GetWord(HeaderOffset.ALPHABET_OFFSET, out AlphabetOffset);
        }
    }
}
