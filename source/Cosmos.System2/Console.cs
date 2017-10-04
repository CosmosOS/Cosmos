using System;
using Cosmos.HAL;

namespace Cosmos.System {
    public class Console {
        protected int mX = 0;
        public int X {
            get { return mX; }
            set {
                mX = value;
                UpdateCursor();
            }
        }

        protected int mY = 0;
        public int Y {
            get { return mY; }
            set {
                mY = value;
                UpdateCursor();
            }
        }

        public int Cols {
            get { return mText.Cols; }
        }

        public int Rows {
            get { return mText.Rows; }
        }

        protected HAL.TextScreenBase mText;

        public Console(TextScreenBase textScreen)
        {
            if (textScreen == null)
            {
                mText = new TextScreen();
            }
            else
            {
                mText = textScreen;
            }
        }

        public void Clear() {
            mText.Clear();
            mX = 0;
            mY = 0;
            UpdateCursor();
        }

        //TODO: This is slow, batch it and only do it at end of updates
        protected void UpdateCursor() {
            mText.SetCursorPos(mX, mY);
        }

        public void NewLine() {
            mY++;
            mX = 0;
            if (mY == mText.Rows) {
                mText.ScrollUp();
                mY--;
            }
            UpdateCursor();
        }

        public void WriteChar(char aChar) {
            InternalWriteChar(aChar);
            UpdateCursor();
        }

        internal void InternalWriteChar(char aChar)
        {
            char Output = aChar;

            //Extended ASCII Support
            if (aChar == 'Ç')
            {
                Output = ((char) 128);
            }
            else if (aChar == 'ü')
            {
                Output = ((char) 129);
            }
            else if (aChar == 'é')
            {
                Output = ((char) 130);
            }
            else if (aChar == 'â')
            {
                Output = ((char) 131);
            }
            else if (aChar == 'ä')
            {
                Output = ((char) 132);
            }
            else if (aChar == 'à')
            {
                Output = ((char) 133);
            }
            else if (aChar == 'å')
            {
                Output = ((char) 134);
            }
            else if (aChar == 'ç')
            {
                Output = ((char) 135);
            }
            else if (aChar == 'ê')
            {
                Output = ((char) 136);
            }
            else if (aChar == 'ë')
            {
                Output = ((char) 137);
            }
            else if (aChar == 'è')
            {
                Output = ((char) 138);
            }
            else if (aChar == 'ï')
            {
                Output = ((char) 139);
            }
            else if (aChar == 'î')
            {
                Output = ((char) 140);
            }
            else if (aChar == 'ì')
            {
                Output = ((char) 141);
            }
            else if (aChar == 'Ä')
            {
                Output = ((char) 142);
            }
            else if (aChar == 'Å')
            {
                Output = ((char) 143);
            }
            else if (aChar == 'É')
            {
                Output = ((char) 144);
            }
            else if (aChar == 'æ')
            {
                Output = ((char) 145);
            }
            else if (aChar == 'Æ')
            {
                Output = ((char) 146);
            }
            else if (aChar == 'ô')
            {
                Output = ((char) 147);
            }
            else if (aChar == 'ö')
            {
                Output = ((char) 148);
            }
            else if (aChar == 'ò')
            {
                Output = ((char) 149);
            }
            else if (aChar == 'û')
            {
                Output = ((char) 150);
            }
            else if (aChar == 'ù')
            {
                Output = ((char) 151);
            }
            else if (aChar == 'ÿ')
            {
                Output = ((char) 152);
            }
            else if (aChar == 'Ö')
            {
                Output = ((char) 153);
            }
            else if (aChar == 'Ü')
            {
                Output = ((char) 154);
            }
            else if (aChar == 'ø')
            {
                Output = ((char) 155);
            }
            else if (aChar == '£')
            {
                Output = ((char) 156);
            }
            else if (aChar == 'Ø')
            {
                Output = ((char) 157);
            }
            else if (aChar == '×')
            {
                Output = ((char) 158);
            }
            else if (aChar == 'ƒ')
            {
                Output = ((char) 159);
            }
            else if (aChar == 'á')
            {
                Output = ((char) 160);
            }
            else if (aChar == 'í')
            {
                Output = ((char) 161);
            }
            else if (aChar == 'ó')
            {
                Output = ((char) 162);
            }
            else if (aChar == 'ú')
            {
                Output = ((char) 163);
            }
            else if (aChar == 'ñ')
            {
                Output = ((char) 164);
            }
            else if (aChar == 'Ñ')
            {
                Output = ((char) 165);
            }
            else if (aChar == 'ª')
            {
                Output = ((char) 166);
            }
            else if (aChar == 'º')
            {
                Output = ((char) 167);
            }
            else if (aChar == '¿')
            {
                Output = ((char) 168);
            }
            else if (aChar == '®')
            {
                Output = ((char) 169);
            }
            else if (aChar == '¬')
            {
                Output = ((char) 170);
            }
            else if (aChar == '½')
            {
                Output = ((char) 171);
            }
            else if (aChar == '¼')
            {
                Output = ((char) 172);
            }
            else if (aChar == '¡')
            {
                Output = ((char) 173);
            }
            else if (aChar == '«')
            {
                Output = ((char) 174);
            }
            else if (aChar == '»')
            {
                Output = ((char) 175);
            }
            else if (aChar == '░')
            {
                Output = ((char) 176);
            }
            else if (aChar == '▒')
            {
                Output = ((char) 177);
            }
            else if (aChar == '▓')
            {
                Output = ((char) 178);
            }
            else if (aChar == '│')
            {
                Output = ((char) 179);
            }
            else if (aChar == '┤')
            {
                Output = ((char) 180);
            }
            else if (aChar == 'Á')
            {
                Output = ((char) 181);
            }
            else if (aChar == 'Â')
            {
                Output = ((char) 182);
            }
            else if (aChar == 'À')
            {
                Output = ((char) 183);
            }
            else if (aChar == '©')
            {
                Output = ((char) 184);
            }
            else if (aChar == '╣')
            {
                Output = ((char) 185);
            }
            else if (aChar == '║')
            {
                Output = ((char) 186);
            }
            else if (aChar == '╗')
            {
                Output = ((char) 187);
            }
            else if (aChar == '╝')
            {
                Output = ((char) 188);
            }
            else if (aChar == '¢')
            {
                Output = ((char) 189);
            }
            else if (aChar == '¥')
            {
                Output = ((char) 190);
            }
            else if (aChar == '┐')
            {
                Output = ((char) 191);
            }
            else if (aChar == '└')
            {
                Output = ((char) 192);
            }
            else if (aChar == '┴')
            {
                Output = ((char) 193);
            }
            else if (aChar == '┬')
            {
                Output = ((char) 194);
            }
            else if (aChar == '├')
            {
                Output = ((char) 195);
            }
            else if (aChar == '─')
            {
                Output = ((char) 196);
            }
            else if (aChar == '┼')
            {
                Output = ((char) 197);
            }
            else if (aChar == 'ã')
            {
                Output = ((char) 198);
            }
            else if (aChar == 'Ã')
            {
                Output = ((char) 199);
            }
            else if (aChar == '╚')
            {
                Output = ((char) 200);
            }
            else if (aChar == '╔')
            {
                Output = ((char) 201);
            }
            else if (aChar == '╩')
            {
                Output = ((char) 202);
            }
            else if (aChar == '╦')
            {
                Output = ((char) 203);
            }
            else if (aChar == '╠')
            {
                Output = ((char) 204);
            }
            else if (aChar == '═')
            {
                Output = ((char) 205);
            }
            else if (aChar == '╬')
            {
                Output = ((char) 206);
            }
            else if (aChar == '¤')
            {
                Output = ((char) 207);
            }
            else if (aChar == 'ð')
            {
                Output = ((char) 208);
            }
            else if (aChar == 'Ð')
            {
                Output = ((char) 209);
            }
            else if (aChar == 'Ê')
            {
                Output = ((char) 210);
            }
            else if (aChar == 'Ë')
            {
                Output = ((char) 211);
            }
            else if (aChar == 'È')
            {
                Output = ((char) 212);
            }
            else if (aChar == 'ı')
            {
                Output = ((char) 213);
            }
            else if (aChar == 'Í')
            {
                Output = ((char) 214);
            }
            else if (aChar == 'Î')
            {
                Output = ((char) 215);
            }
            else if (aChar == 'Ï')
            {
                Output = ((char) 216);
            }
            else if (aChar == '┘')
            {
                Output = ((char) 217);
            }
            else if (aChar == '┌')
            {
                Output = ((char) 218);
            }
            else if (aChar == '█')
            {
                Output = ((char) 219);
            }
            else if (aChar == '▄')
            {
                Output = ((char) 220);
            }
            else if (aChar == '¦')
            {
                Output = ((char) 221);
            }
            else if (aChar == 'Ì')
            {
                Output = ((char) 222);
            }
            else if (aChar == '▀')
            {
                Output = ((char) 223);
            }
            else if (aChar == 'Ó')
            {
                Output = ((char) 224);
            }
            else if (aChar == 'ß')
            {
                Output = ((char) 225);
            }
            else if (aChar == 'Ô')
            {
                Output = ((char) 226);
            }
            else if (aChar == 'Ò')
            {
                Output = ((char) 227);
            }
            else if (aChar == 'õ')
            {
                Output = ((char) 228);
            }
            else if (aChar == 'Õ')
            {
                Output = ((char) 229);
            }
            else if (aChar == 'µ')
            {
                Output = ((char) 230);
            }
            else if (aChar == 'þ')
            {
                Output = ((char) 231);
            }
            else if (aChar == 'Þ')
            {
                Output = ((char) 232);
            }
            else if (aChar == 'Ú')
            {
                Output = ((char) 233);
            }
            else if (aChar == 'Û')
            {
                Output = ((char) 234);
            }
            else if (aChar == 'Ù')
            {
                Output = ((char) 235);
            }
            else if (aChar == 'ý')
            {
                Output = ((char) 236);
            }
            else if (aChar == 'Ý')
            {
                Output = ((char) 237);
            }
            else if (aChar == '¯')
            {
                Output = ((char) 238);
            }
            else if (aChar == '´')
            {
                Output = ((char) 239);
            }
            else if (aChar == '≡')
            {
                Output = ((char) 240);
            }
            else if (aChar == '±')
            {
                Output = ((char) 241);
            }
            else if (aChar == '‗')
            {
                Output = ((char) 242);
            }
            else if (aChar == '¾')
            {
                Output = ((char) 243);
            }
            else if (aChar == '¶')
            {
                Output = ((char) 244);
            }
            else if (aChar == '§')
            {
                Output = ((char) 245);
            }
            else if (aChar == '÷')
            {
                Output = ((char) 246);
            }
            else if (aChar == '¸')
            {
                Output = ((char) 247);
            }
            else if (aChar == '°')
            {
                Output = ((char) 248);
            }
            else if (aChar == '¨')
            {
                Output = ((char) 249);
            }
            else if (aChar == '·')
            {
                Output = ((char) 250);
            }
            else if (aChar == '¹')
            {
                Output = ((char) 251);
            }
            else if (aChar == '³')
            {
                Output = ((char) 252);
            }
            else if (aChar == '²')
            {
                Output = ((char) 253);
            }
            else if (aChar == '■')
            {
                Output = ((char) 254);
            }
            else if (aChar == ' ')
            {
                Output = ((char) 255);
            }


            mText[mX, mY] = aChar;
            mX++;
            if (mX == mText.Cols)
            {
                NewLine();
            }
        }

        public void WriteLine(string aText) {
            Write(aText);
            NewLine();
        }

        //TODO: Optimize this
        public void Write(string aText) {
            if (aText == null)
            {
                return;
            }
            for (int i = 0; i < aText.Length; i++) {
                if (aText[i] == '\n') {
                    NewLine();
                } else if (aText[i] == '\r') {
                    mX = 0;
                    UpdateCursor();
                } else if (aText[i] == '\t') {
                    //Write("    ");
                    InternalWriteChar(' ');
                    InternalWriteChar(' ');
                    InternalWriteChar(' ');
                    InternalWriteChar(' ');
                }

                               

            }
            
            UpdateCursor();

        }

        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)(mText.GetColor() ^ (byte)((byte)Background << 4)); }
            set { mText.SetColors(value, Background); }
        }
        public ConsoleColor Background
        {
            get { return (ConsoleColor)(mText.GetColor() >> 4); }
            set { mText.SetColors(Foreground, value); }
        }

        public int CursorSize
        {
            get { return mText.GetCursorSize(); }
            set {
                // Value should be a percentage from [1, 100].
                if (value < 1 || value > 100)
                    throw new ArgumentOutOfRangeException("value", value, "CursorSize value " + value + " out of range (1 - 100)");

                mText.SetCursorSize(value);
            }
        }

        public bool CursorVisible {
            get { return mText.GetCursorVisible(); }
            set { mText.SetCursorVisible(value);  }
        }
    }
}
