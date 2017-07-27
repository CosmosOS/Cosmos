using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            mText[mX, mY] = aChar;
            mX++;
            if (mX == mText.Cols) {
                NewLine();
            }
            UpdateCursor();
        }

        internal void iWriteChar(char aChar)
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
                    iWriteChar(' ');
                    iWriteChar(' ');
                    iWriteChar(' ');
                    iWriteChar(' ');
                }

                //Extended ASCII Support
                else if (aText[i] == 'Ç')
                {
                    iWriteChar((char)128);
                }
                else if (aText[i] == 'ü')
                {
                    iWriteChar((char)129);
                }
                else if (aText[i] == 'é')
                {
                    iWriteChar((char)130);
                }
                else if (aText[i] == 'â')
                {
                    iWriteChar((char)131);
                }
                else if (aText[i] == 'ä')
                {
                    iWriteChar((char)132);
                }
                else if (aText[i] == 'à')
                {
                    iWriteChar((char)133);
                }
                else if (aText[i] == 'å')
                {
                    iWriteChar((char)134);
                }
                else if (aText[i] == 'ç')
                {
                    iWriteChar((char)135);
                }
                else if (aText[i] == 'ê')
                {
                    iWriteChar((char)136);
                }
                else if (aText[i] == 'ë')
                {
                    iWriteChar((char)137);
                }
                else if (aText[i] == 'è')
                {
                    iWriteChar((char)138);
                }
                else if (aText[i] == 'ï')
                {
                    iWriteChar((char)139);
                }
                else if (aText[i] == 'î')
                {
                    iWriteChar((char)140);
                }
                else if (aText[i] == 'ì')
                {
                    iWriteChar((char)141);
                }
                else if (aText[i] == 'Ä')
                {
                    iWriteChar((char)142);
                }
                else if (aText[i] == 'Å')
                {
                    iWriteChar((char)143);
                }
                else if (aText[i] == 'É')
                {
                    iWriteChar((char)144);
                }
                else if (aText[i] == 'æ')
                {
                    iWriteChar((char)145);
                }
                else if (aText[i] == 'Æ')
                {
                    iWriteChar((char)146);
                }
                else if (aText[i] == 'ô')
                {
                    iWriteChar((char)147);
                }
                else if (aText[i] == 'ö')
                {
                    iWriteChar((char)148);
                }
                else if (aText[i] == 'ò')
                {
                    iWriteChar((char)149);
                }
                else if (aText[i] == 'û')
                {
                    iWriteChar((char)150);
                }
                else if (aText[i] == 'ù')
                {
                    iWriteChar((char)151);
                }
                else if (aText[i] == 'ÿ')
                {
                    iWriteChar((char)152);
                }
                else if (aText[i] == 'Ö')
                {
                    iWriteChar((char)153);
                }
                else if (aText[i] == 'Ü')
                {
                    iWriteChar((char)154);
                }
                else if (aText[i] == 'ø')
                {
                    iWriteChar((char)155);
                }
                else if (aText[i] == '£')
                {
                    iWriteChar((char)156);
                }
                else if (aText[i] == 'Ø')
                {
                    iWriteChar((char)157);
                }
                else if (aText[i] == '×')
                {
                    iWriteChar((char)158);
                }
                else if (aText[i] == 'ƒ')
                {
                    iWriteChar((char)159);
                }
                else if (aText[i] == 'á')
                {
                    iWriteChar((char)160);
                }
                else if (aText[i] == 'í')
                {
                    iWriteChar((char)161);
                }
                else if (aText[i] == 'ó')
                {
                    iWriteChar((char)162);
                }
                else if (aText[i] == 'ú')
                {
                    iWriteChar((char)163);
                }
                else if (aText[i] == 'ñ')
                {
                    iWriteChar((char)164);
                }
                else if (aText[i] == 'Ñ')
                {
                    iWriteChar((char)165);
                }
                else if (aText[i] == 'ª')
                {
                    iWriteChar((char)166);
                }
                else if (aText[i] == 'º')
                {
                    iWriteChar((char)167);
                }
                else if (aText[i] == '¿')
                {
                    iWriteChar((char)168);
                }
                else if (aText[i] == '®')
                {
                    iWriteChar((char)169);
                }
                else if (aText[i] == '¬')
                {
                    iWriteChar((char)170);
                }
                else if (aText[i] == '½')
                {
                    iWriteChar((char)171);
                }
                else if (aText[i] == '¼')
                {
                    iWriteChar((char)172);
                }
                else if (aText[i] == '¡')
                {
                    iWriteChar((char)173);
                }
                else if (aText[i] == '«')
                {
                    iWriteChar((char)174);
                }
                else if (aText[i] == '»')
                {
                    iWriteChar((char)175);
                }
                else if (aText[i] == '░')
                {
                    iWriteChar((char)176);
                }
                else if (aText[i] == '▒')
                {
                    iWriteChar((char)177);
                }
                else if (aText[i] == '▓')
                {
                    iWriteChar((char)178);
                }
                else if (aText[i] == '│')
                {
                    iWriteChar((char)179);
                }
                else if (aText[i] == '┤')
                {
                    iWriteChar((char)180);
                }
                else if (aText[i] == 'Á')
                {
                    iWriteChar((char)181);
                }
                else if (aText[i] == 'Â')
                {
                    iWriteChar((char)182);
                }
                else if (aText[i] == 'À')
                {
                    iWriteChar((char)183);
                }
                else if (aText[i] == '©')
                {
                    iWriteChar((char)184);
                }
                else if (aText[i] == '╣')
                {
                    iWriteChar((char)185);
                }
                else if (aText[i] == '║')
                {
                    iWriteChar((char)186);
                }
                else if (aText[i] == '╗')
                {
                    iWriteChar((char)187);
                }
                else if (aText[i] == '╝')
                {
                    iWriteChar((char)188);
                }
                else if (aText[i] == '¢')
                {
                    iWriteChar((char)189);
                }
                else if (aText[i] == '¥')
                {
                    iWriteChar((char)190);
                }
                else if (aText[i] == '┐')
                {
                    iWriteChar((char)191);
                }
                else if (aText[i] == '└')
                {
                    iWriteChar((char)192);
                }
                else if (aText[i] == '┴')
                {
                    iWriteChar((char)193);
                }
                else if (aText[i] == '┬')
                {
                    iWriteChar((char)194);
                }
                else if (aText[i] == '├')
                {
                    iWriteChar((char)195);
                }
                else if (aText[i] == '─')
                {
                    iWriteChar((char)196);
                }
                else if (aText[i] == '┼')
                {
                    iWriteChar((char)197);
                }
                else if (aText[i] == 'ã')
                {
                    iWriteChar((char)198);
                }
                else if (aText[i] == 'Ã')
                {
                    iWriteChar((char)199);
                }
                else if (aText[i] == '╚')
                {
                    iWriteChar((char)200);
                }
                else if (aText[i] == '╔')
                {
                    iWriteChar((char)201);
                }
                else if (aText[i] == '╩')
                {
                    iWriteChar((char)202);
                }
                else if (aText[i] == '╦')
                {
                    iWriteChar((char)203);
                }
                else if (aText[i] == '╠')
                {
                    iWriteChar((char)204);
                }
                else if (aText[i] == '═')
                {
                    iWriteChar((char)205);
                }
                else if (aText[i] == '╬')
                {
                    iWriteChar((char)206);
                }
                else if (aText[i] == '¤')
                {
                    iWriteChar((char)207);
                }
                else if (aText[i] == 'ð')
                {
                    iWriteChar((char)208);
                }
                else if (aText[i] == 'Ð')
                {
                    iWriteChar((char)209);
                }
                else if (aText[i] == 'Ê')
                {
                    iWriteChar((char)210);
                }
                else if (aText[i] == 'Ë')
                {
                    iWriteChar((char)211);
                }
                else if (aText[i] == 'È')
                {
                    iWriteChar((char)212);
                }
                else if (aText[i] == 'ı')
                {
                    iWriteChar((char)213);
                }
                else if (aText[i] == 'Í')
                {
                    iWriteChar((char)214);
                }
                else if (aText[i] == 'Î')
                {
                    iWriteChar((char)215);
                }
                else if (aText[i] == 'Ï')
                {
                    iWriteChar((char)216);
                }
                else if (aText[i] == '┘')
                {
                    iWriteChar((char)217);
                }
                else if (aText[i] == '┌')
                {
                    iWriteChar((char)218);
                }
                else if (aText[i] == '█')
                {
                    iWriteChar((char)219);
                }
                else if (aText[i] == '▄')
                {
                    iWriteChar((char)220);
                }
                else if (aText[i] == '¦')
                {
                    iWriteChar((char)221);
                }
                else if (aText[i] == 'Ì')
                {
                    iWriteChar((char)222);
                }
                else if (aText[i] == '▀')
                {
                    iWriteChar((char)223);
                }
                else if (aText[i] == 'Ó')
                {
                    iWriteChar((char)224);
                }
                else if (aText[i] == 'ß')
                {
                    iWriteChar((char)225);
                }
                else if (aText[i] == 'Ô')
                {
                    iWriteChar((char)226);
                }
                else if (aText[i] == 'Ò')
                {
                    iWriteChar((char)227);
                }
                else if (aText[i] == 'õ')
                {
                    iWriteChar((char)228);
                }
                else if (aText[i] == 'Õ')
                {
                    iWriteChar((char)229);
                }
                else if (aText[i] == 'µ')
                {
                    iWriteChar((char)230);
                }
                else if (aText[i] == 'þ')
                {
                    iWriteChar((char)231);
                }
                else if (aText[i] == 'Þ')
                {
                    iWriteChar((char)232);
                }
                else if (aText[i] == 'Ú')
                {
                    iWriteChar((char)233);
                }
                else if (aText[i] == 'Û')
                {
                    iWriteChar((char)234);
                }
                else if (aText[i] == 'Ù')
                {
                    iWriteChar((char)235);
                }
                else if (aText[i] == 'ý')
                {
                    iWriteChar((char)236);
                }
                else if (aText[i] == 'Ý')
                {
                    iWriteChar((char)237);
                }
                else if (aText[i] == '¯')
                {
                    iWriteChar((char)238);
                }
                else if (aText[i] == '´')
                {
                    iWriteChar((char)239);
                }
                else if (aText[i] == '≡')
                {
                    iWriteChar((char)240);
                }
                else if (aText[i] == '±')
                {
                    iWriteChar((char)241);
                }
                else if (aText[i] == '‗')
                {
                    iWriteChar((char)242);
                }
                else if (aText[i] == '¾')
                {
                    iWriteChar((char)243);
                }
                else if (aText[i] == '¶')
                {
                    iWriteChar((char)244);
                }
                else if (aText[i] == '§')
                {
                    iWriteChar((char)245);
                }
                else if (aText[i] == '÷')
                {
                    iWriteChar((char)246);
                }
                else if (aText[i] == '¸')
                {
                    iWriteChar((char)247);
                }
                else if (aText[i] == '°')
                {
                    iWriteChar((char)248);
                }
                else if (aText[i] == '¨')
                {
                    iWriteChar((char)249);
                }
                else if (aText[i] == '·')
                {
                    iWriteChar((char)250);
                }
                else if (aText[i] == '¹')
                {
                    iWriteChar((char)251);
                }
                else if (aText[i] == '³')
                {
                    iWriteChar((char)252);
                }
                else if (aText[i] == '²')
                {
                    iWriteChar((char)253);
                }
                else if (aText[i] == '■')
                {
                    iWriteChar((char)254);
                }
                else if (aText[i] == ' ')
                {
                    iWriteChar((char)255);
                }
                else {
                    iWriteChar(aText[i]);
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
