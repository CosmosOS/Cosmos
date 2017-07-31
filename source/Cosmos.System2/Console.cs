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

                //Extended ASCII Support
                else if (aText[i] == 'Ç')
                {
                    InternalWriteChar((char)128);
                }
                else if (aText[i] == 'ü')
                {
                    InternalWriteChar((char)129);
                }
                else if (aText[i] == 'é')
                {
                    InternalWriteChar((char)130);
                }
                else if (aText[i] == 'â')
                {
                    InternalWriteChar((char)131);
                }
                else if (aText[i] == 'ä')
                {
                    InternalWriteChar((char)132);
                }
                else if (aText[i] == 'à')
                {
                    InternalWriteChar((char)133);
                }
                else if (aText[i] == 'å')
                {
                    InternalWriteChar((char)134);
                }
                else if (aText[i] == 'ç')
                {
                    InternalWriteChar((char)135);
                }
                else if (aText[i] == 'ê')
                {
                    InternalWriteChar((char)136);
                }
                else if (aText[i] == 'ë')
                {
                    InternalWriteChar((char)137);
                }
                else if (aText[i] == 'è')
                {
                    InternalWriteChar((char)138);
                }
                else if (aText[i] == 'ï')
                {
                    InternalWriteChar((char)139);
                }
                else if (aText[i] == 'î')
                {
                    InternalWriteChar((char)140);
                }
                else if (aText[i] == 'ì')
                {
                    InternalWriteChar((char)141);
                }
                else if (aText[i] == 'Ä')
                {
                    InternalWriteChar((char)142);
                }
                else if (aText[i] == 'Å')
                {
                    InternalWriteChar((char)143);
                }
                else if (aText[i] == 'É')
                {
                    InternalWriteChar((char)144);
                }
                else if (aText[i] == 'æ')
                {
                    InternalWriteChar((char)145);
                }
                else if (aText[i] == 'Æ')
                {
                    InternalWriteChar((char)146);
                }
                else if (aText[i] == 'ô')
                {
                    InternalWriteChar((char)147);
                }
                else if (aText[i] == 'ö')
                {
                    InternalWriteChar((char)148);
                }
                else if (aText[i] == 'ò')
                {
                    InternalWriteChar((char)149);
                }
                else if (aText[i] == 'û')
                {
                    InternalWriteChar((char)150);
                }
                else if (aText[i] == 'ù')
                {
                    InternalWriteChar((char)151);
                }
                else if (aText[i] == 'ÿ')
                {
                    InternalWriteChar((char)152);
                }
                else if (aText[i] == 'Ö')
                {
                    InternalWriteChar((char)153);
                }
                else if (aText[i] == 'Ü')
                {
                    InternalWriteChar((char)154);
                }
                else if (aText[i] == 'ø')
                {
                    InternalWriteChar((char)155);
                }
                else if (aText[i] == '£')
                {
                    InternalWriteChar((char)156);
                }
                else if (aText[i] == 'Ø')
                {
                    InternalWriteChar((char)157);
                }
                else if (aText[i] == '×')
                {
                    InternalWriteChar((char)158);
                }
                else if (aText[i] == 'ƒ')
                {
                    InternalWriteChar((char)159);
                }
                else if (aText[i] == 'á')
                {
                    InternalWriteChar((char)160);
                }
                else if (aText[i] == 'í')
                {
                    InternalWriteChar((char)161);
                }
                else if (aText[i] == 'ó')
                {
                    InternalWriteChar((char)162);
                }
                else if (aText[i] == 'ú')
                {
                    InternalWriteChar((char)163);
                }
                else if (aText[i] == 'ñ')
                {
                    InternalWriteChar((char)164);
                }
                else if (aText[i] == 'Ñ')
                {
                    InternalWriteChar((char)165);
                }
                else if (aText[i] == 'ª')
                {
                    InternalWriteChar((char)166);
                }
                else if (aText[i] == 'º')
                {
                    InternalWriteChar((char)167);
                }
                else if (aText[i] == '¿')
                {
                    InternalWriteChar((char)168);
                }
                else if (aText[i] == '®')
                {
                    InternalWriteChar((char)169);
                }
                else if (aText[i] == '¬')
                {
                    InternalWriteChar((char)170);
                }
                else if (aText[i] == '½')
                {
                    InternalWriteChar((char)171);
                }
                else if (aText[i] == '¼')
                {
                    InternalWriteChar((char)172);
                }
                else if (aText[i] == '¡')
                {
                    InternalWriteChar((char)173);
                }
                else if (aText[i] == '«')
                {
                    InternalWriteChar((char)174);
                }
                else if (aText[i] == '»')
                {
                    InternalWriteChar((char)175);
                }
                else if (aText[i] == '░')
                {
                    InternalWriteChar((char)176);
                }
                else if (aText[i] == '▒')
                {
                    InternalWriteChar((char)177);
                }
                else if (aText[i] == '▓')
                {
                    InternalWriteChar((char)178);
                }
                else if (aText[i] == '│')
                {
                    InternalWriteChar((char)179);
                }
                else if (aText[i] == '┤')
                {
                    InternalWriteChar((char)180);
                }
                else if (aText[i] == 'Á')
                {
                    InternalWriteChar((char)181);
                }
                else if (aText[i] == 'Â')
                {
                    InternalWriteChar((char)182);
                }
                else if (aText[i] == 'À')
                {
                    InternalWriteChar((char)183);
                }
                else if (aText[i] == '©')
                {
                    InternalWriteChar((char)184);
                }
                else if (aText[i] == '╣')
                {
                    InternalWriteChar((char)185);
                }
                else if (aText[i] == '║')
                {
                    InternalWriteChar((char)186);
                }
                else if (aText[i] == '╗')
                {
                    InternalWriteChar((char)187);
                }
                else if (aText[i] == '╝')
                {
                    InternalWriteChar((char)188);
                }
                else if (aText[i] == '¢')
                {
                    InternalWriteChar((char)189);
                }
                else if (aText[i] == '¥')
                {
                    InternalWriteChar((char)190);
                }
                else if (aText[i] == '┐')
                {
                    InternalWriteChar((char)191);
                }
                else if (aText[i] == '└')
                {
                    InternalWriteChar((char)192);
                }
                else if (aText[i] == '┴')
                {
                    InternalWriteChar((char)193);
                }
                else if (aText[i] == '┬')
                {
                    InternalWriteChar((char)194);
                }
                else if (aText[i] == '├')
                {
                    InternalWriteChar((char)195);
                }
                else if (aText[i] == '─')
                {
                    InternalWriteChar((char)196);
                }
                else if (aText[i] == '┼')
                {
                    InternalWriteChar((char)197);
                }
                else if (aText[i] == 'ã')
                {
                    InternalWriteChar((char)198);
                }
                else if (aText[i] == 'Ã')
                {
                    InternalWriteChar((char)199);
                }
                else if (aText[i] == '╚')
                {
                    InternalWriteChar((char)200);
                }
                else if (aText[i] == '╔')
                {
                    InternalWriteChar((char)201);
                }
                else if (aText[i] == '╩')
                {
                    InternalWriteChar((char)202);
                }
                else if (aText[i] == '╦')
                {
                    InternalWriteChar((char)203);
                }
                else if (aText[i] == '╠')
                {
                    InternalWriteChar((char)204);
                }
                else if (aText[i] == '═')
                {
                    InternalWriteChar((char)205);
                }
                else if (aText[i] == '╬')
                {
                    InternalWriteChar((char)206);
                }
                else if (aText[i] == '¤')
                {
                    InternalWriteChar((char)207);
                }
                else if (aText[i] == 'ð')
                {
                    InternalWriteChar((char)208);
                }
                else if (aText[i] == 'Ð')
                {
                    InternalWriteChar((char)209);
                }
                else if (aText[i] == 'Ê')
                {
                    InternalWriteChar((char)210);
                }
                else if (aText[i] == 'Ë')
                {
                    InternalWriteChar((char)211);
                }
                else if (aText[i] == 'È')
                {
                    InternalWriteChar((char)212);
                }
                else if (aText[i] == 'ı')
                {
                    InternalWriteChar((char)213);
                }
                else if (aText[i] == 'Í')
                {
                    InternalWriteChar((char)214);
                }
                else if (aText[i] == 'Î')
                {
                    InternalWriteChar((char)215);
                }
                else if (aText[i] == 'Ï')
                {
                    InternalWriteChar((char)216);
                }
                else if (aText[i] == '┘')
                {
                    InternalWriteChar((char)217);
                }
                else if (aText[i] == '┌')
                {
                    InternalWriteChar((char)218);
                }
                else if (aText[i] == '█')
                {
                    InternalWriteChar((char)219);
                }
                else if (aText[i] == '▄')
                {
                    InternalWriteChar((char)220);
                }
                else if (aText[i] == '¦')
                {
                    InternalWriteChar((char)221);
                }
                else if (aText[i] == 'Ì')
                {
                    InternalWriteChar((char)222);
                }
                else if (aText[i] == '▀')
                {
                    InternalWriteChar((char)223);
                }
                else if (aText[i] == 'Ó')
                {
                    InternalWriteChar((char)224);
                }
                else if (aText[i] == 'ß')
                {
                    InternalWriteChar((char)225);
                }
                else if (aText[i] == 'Ô')
                {
                    InternalWriteChar((char)226);
                }
                else if (aText[i] == 'Ò')
                {
                    InternalWriteChar((char)227);
                }
                else if (aText[i] == 'õ')
                {
                    InternalWriteChar((char)228);
                }
                else if (aText[i] == 'Õ')
                {
                    InternalWriteChar((char)229);
                }
                else if (aText[i] == 'µ')
                {
                    InternalWriteChar((char)230);
                }
                else if (aText[i] == 'þ')
                {
                    InternalWriteChar((char)231);
                }
                else if (aText[i] == 'Þ')
                {
                    InternalWriteChar((char)232);
                }
                else if (aText[i] == 'Ú')
                {
                    InternalWriteChar((char)233);
                }
                else if (aText[i] == 'Û')
                {
                    InternalWriteChar((char)234);
                }
                else if (aText[i] == 'Ù')
                {
                    InternalWriteChar((char)235);
                }
                else if (aText[i] == 'ý')
                {
                    InternalWriteChar((char)236);
                }
                else if (aText[i] == 'Ý')
                {
                    InternalWriteChar((char)237);
                }
                else if (aText[i] == '¯')
                {
                    InternalWriteChar((char)238);
                }
                else if (aText[i] == '´')
                {
                    InternalWriteChar((char)239);
                }
                else if (aText[i] == '≡')
                {
                    InternalWriteChar((char)240);
                }
                else if (aText[i] == '±')
                {
                    InternalWriteChar((char)241);
                }
                else if (aText[i] == '‗')
                {
                    InternalWriteChar((char)242);
                }
                else if (aText[i] == '¾')
                {
                    InternalWriteChar((char)243);
                }
                else if (aText[i] == '¶')
                {
                    InternalWriteChar((char)244);
                }
                else if (aText[i] == '§')
                {
                    InternalWriteChar((char)245);
                }
                else if (aText[i] == '÷')
                {
                    InternalWriteChar((char)246);
                }
                else if (aText[i] == '¸')
                {
                    InternalWriteChar((char)247);
                }
                else if (aText[i] == '°')
                {
                    InternalWriteChar((char)248);
                }
                else if (aText[i] == '¨')
                {
                    InternalWriteChar((char)249);
                }
                else if (aText[i] == '·')
                {
                    InternalWriteChar((char)250);
                }
                else if (aText[i] == '¹')
                {
                    InternalWriteChar((char)251);
                }
                else if (aText[i] == '³')
                {
                    InternalWriteChar((char)252);
                }
                else if (aText[i] == '²')
                {
                    InternalWriteChar((char)253);
                }
                else if (aText[i] == '■')
                {
                    InternalWriteChar((char)254);
                }
                else if (aText[i] == ' ')
                {
                    InternalWriteChar((char)255);
                }
                else {
                    InternalWriteChar(aText[i]);
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
