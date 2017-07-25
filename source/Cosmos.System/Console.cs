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
                    WriteChar(' ');
                    WriteChar(' ');
                    WriteChar(' ');
                    WriteChar(' ');
                }

                //Extended ASCII Support
                else if (aText[i] == 'Ç')
                {
                    WriteChar((char)128);
                }
                else if (aText[i] == 'ü')
                {
                    WriteChar((char)129);
                }
                else if (aText[i] == 'é')
                {
                    WriteChar((char)130);
                }
                else if (aText[i] == 'â')
                {
                    WriteChar((char)131);
                }
                else if (aText[i] == 'ä')
                {
                    WriteChar((char)132);
                }
                else if (aText[i] == 'à')
                {
                    WriteChar((char)133);
                }
                else if (aText[i] == 'å')
                {
                    WriteChar((char)134);
                }
                else if (aText[i] == 'ç')
                {
                    WriteChar((char)135);
                }
                else if (aText[i] == 'ê')
                {
                    WriteChar((char)136);
                }
                else if (aText[i] == 'ë')
                {
                    WriteChar((char)137);
                }
                else if (aText[i] == 'è')
                {
                    WriteChar((char)138);
                }
                else if (aText[i] == 'ï')
                {
                    WriteChar((char)139);
                }
                else if (aText[i] == 'î')
                {
                    WriteChar((char)140);
                }
                else if (aText[i] == 'ì')
                {
                    WriteChar((char)141);
                }
                else if (aText[i] == 'Ä')
                {
                    WriteChar((char)142);
                }
                else if (aText[i] == 'Å')
                {
                    WriteChar((char)143);
                }
                else if (aText[i] == 'É')
                {
                    WriteChar((char)144);
                }
                else if (aText[i] == 'æ')
                {
                    WriteChar((char)145);
                }
                else if (aText[i] == 'Æ')
                {
                    WriteChar((char)146);
                }
                else if (aText[i] == 'ô')
                {
                    WriteChar((char)147);
                }
                else if (aText[i] == 'ö')
                {
                    WriteChar((char)148);
                }
                else if (aText[i] == 'ò')
                {
                    WriteChar((char)149);
                }
                else if (aText[i] == 'û')
                {
                    WriteChar((char)150);
                }
                else if (aText[i] == 'ù')
                {
                    WriteChar((char)151);
                }
                else if (aText[i] == 'ÿ')
                {
                    WriteChar((char)152);
                }
                else if (aText[i] == 'Ö')
                {
                    WriteChar((char)153);
                }
                else if (aText[i] == 'Ü')
                {
                    WriteChar((char)154);
                }
                else if (aText[i] == 'ø')
                {
                    WriteChar((char)155);
                }
                else if (aText[i] == '£')
                {
                    WriteChar((char)156);
                }
                else if (aText[i] == 'Ø')
                {
                    WriteChar((char)157);
                }
                else if (aText[i] == '×')
                {
                    WriteChar((char)158);
                }
                else if (aText[i] == 'ƒ')
                {
                    WriteChar((char)159);
                }
                else if (aText[i] == 'á')
                {
                    WriteChar((char)160);
                }
                else if (aText[i] == 'í')
                {
                    WriteChar((char)161);
                }
                else if (aText[i] == 'ó')
                {
                    WriteChar((char)162);
                }
                else if (aText[i] == 'ú')
                {
                    WriteChar((char)163);
                }
                else if (aText[i] == 'ñ')
                {
                    WriteChar((char)164);
                }
                else if (aText[i] == 'Ñ')
                {
                    WriteChar((char)165);
                }
                else if (aText[i] == 'ª')
                {
                    WriteChar((char)166);
                }
                else if (aText[i] == 'º')
                {
                    WriteChar((char)167);
                }
                else if (aText[i] == '¿')
                {
                    WriteChar((char)168);
                }
                else if (aText[i] == '®')
                {
                    WriteChar((char)169);
                }
                else if (aText[i] == '¬')
                {
                    WriteChar((char)170);
                }
                else if (aText[i] == '½')
                {
                    WriteChar((char)171);
                }
                else if (aText[i] == '¼')
                {
                    WriteChar((char)172);
                }
                else if (aText[i] == '¡')
                {
                    WriteChar((char)173);
                }
                else if (aText[i] == '«')
                {
                    WriteChar((char)174);
                }
                else if (aText[i] == '»')
                {
                    WriteChar((char)175);
                }
                else if (aText[i] == '░')
                {
                    WriteChar((char)176);
                }
                else if (aText[i] == '▒')
                {
                    WriteChar((char)177);
                }
                else if (aText[i] == '▓')
                {
                    WriteChar((char)178);
                }
                else if (aText[i] == '│')
                {
                    WriteChar((char)179);
                }
                else if (aText[i] == '┤')
                {
                    WriteChar((char)180);
                }
                else if (aText[i] == 'Á')
                {
                    WriteChar((char)181);
                }
                else if (aText[i] == 'Â')
                {
                    WriteChar((char)182);
                }
                else if (aText[i] == 'À')
                {
                    WriteChar((char)183);
                }
                else if (aText[i] == '©')
                {
                    WriteChar((char)184);
                }
                else if (aText[i] == '╣')
                {
                    WriteChar((char)185);
                }
                else if (aText[i] == '║')
                {
                    WriteChar((char)186);
                }
                else if (aText[i] == '╗')
                {
                    WriteChar((char)187);
                }
                else if (aText[i] == '╝')
                {
                    WriteChar((char)188);
                }
                else if (aText[i] == '¢')
                {
                    WriteChar((char)189);
                }
                else if (aText[i] == '¥')
                {
                    WriteChar((char)190);
                }
                else if (aText[i] == '┐')
                {
                    WriteChar((char)191);
                }
                else if (aText[i] == '└')
                {
                    WriteChar((char)192);
                }
                else if (aText[i] == '┴')
                {
                    WriteChar((char)193);
                }
                else if (aText[i] == '┬')
                {
                    WriteChar((char)194);
                }
                else if (aText[i] == '├')
                {
                    WriteChar((char)195);
                }
                else if (aText[i] == '─')
                {
                    WriteChar((char)196);
                }
                else if (aText[i] == '┼')
                {
                    WriteChar((char)197);
                }
                else if (aText[i] == 'ã')
                {
                    WriteChar((char)198);
                }
                else if (aText[i] == 'Ã')
                {
                    WriteChar((char)199);
                }
                else if (aText[i] == '╚')
                {
                    WriteChar((char)200);
                }
                else if (aText[i] == '╔')
                {
                    WriteChar((char)201);
                }
                else if (aText[i] == '╩')
                {
                    WriteChar((char)202);
                }
                else if (aText[i] == '╦')
                {
                    WriteChar((char)203);
                }
                else if (aText[i] == '╠')
                {
                    WriteChar((char)204);
                }
                else if (aText[i] == '═')
                {
                    WriteChar((char)205);
                }
                else if (aText[i] == '╬')
                {
                    WriteChar((char)206);
                }
                else if (aText[i] == '¤')
                {
                    WriteChar((char)207);
                }
                else if (aText[i] == 'ð')
                {
                    WriteChar((char)208);
                }
                else if (aText[i] == 'Ð')
                {
                    WriteChar((char)209);
                }
                else if (aText[i] == 'Ê')
                {
                    WriteChar((char)210);
                }
                else if (aText[i] == 'Ë')
                {
                    WriteChar((char)211);
                }
                else if (aText[i] == 'È')
                {
                    WriteChar((char)212);
                }
                else if (aText[i] == 'ı')
                {
                    WriteChar((char)213);
                }
                else if (aText[i] == 'Í')
                {
                    WriteChar((char)214);
                }
                else if (aText[i] == 'Î')
                {
                    WriteChar((char)215);
                }
                else if (aText[i] == 'Ï')
                {
                    WriteChar((char)216);
                }
                else if (aText[i] == '┘')
                {
                    WriteChar((char)217);
                }
                else if (aText[i] == '┌')
                {
                    WriteChar((char)218);
                }
                else if (aText[i] == '█')
                {
                    WriteChar((char)219);
                }
                else if (aText[i] == '▄')
                {
                    WriteChar((char)220);
                }
                else if (aText[i] == '¦')
                {
                    WriteChar((char)221);
                }
                else if (aText[i] == 'Ì')
                {
                    WriteChar((char)222);
                }
                else if (aText[i] == '▀')
                {
                    WriteChar((char)223);
                }
                else if (aText[i] == 'Ó')
                {
                    WriteChar((char)224);
                }
                else if (aText[i] == 'ß')
                {
                    WriteChar((char)225);
                }
                else if (aText[i] == 'Ô')
                {
                    WriteChar((char)226);
                }
                else if (aText[i] == 'Ò')
                {
                    WriteChar((char)227);
                }
                else if (aText[i] == 'õ')
                {
                    WriteChar((char)228);
                }
                else if (aText[i] == 'Õ')
                {
                    WriteChar((char)229);
                }
                else if (aText[i] == 'µ')
                {
                    WriteChar((char)230);
                }
                else if (aText[i] == 'þ')
                {
                    WriteChar((char)231);
                }
                else if (aText[i] == 'Þ')
                {
                    WriteChar((char)232);
                }
                else if (aText[i] == 'Ú')
                {
                    WriteChar((char)233);
                }
                else if (aText[i] == 'Û')
                {
                    WriteChar((char)234);
                }
                else if (aText[i] == 'Ù')
                {
                    WriteChar((char)235);
                }
                else if (aText[i] == 'ý')
                {
                    WriteChar((char)236);
                }
                else if (aText[i] == 'Ý')
                {
                    WriteChar((char)237);
                }
                else if (aText[i] == '¯')
                {
                    WriteChar((char)238);
                }
                else if (aText[i] == '´')
                {
                    WriteChar((char)239);
                }
                else if (aText[i] == '≡')
                {
                    WriteChar((char)240);
                }
                else if (aText[i] == '±')
                {
                    WriteChar((char)241);
                }
                else if (aText[i] == '‗')
                {
                    WriteChar((char)242);
                }
                else if (aText[i] == '¾')
                {
                    WriteChar((char)243);
                }
                else if (aText[i] == '¶')
                {
                    WriteChar((char)244);
                }
                else if (aText[i] == '§')
                {
                    WriteChar((char)245);
                }
                else if (aText[i] == '÷')
                {
                    WriteChar((char)246);
                }
                else if (aText[i] == '¸')
                {
                    WriteChar((char)247);
                }
                else if (aText[i] == '°')
                {
                    WriteChar((char)248);
                }
                else if (aText[i] == '¨')
                {
                    WriteChar((char)249);
                }
                else if (aText[i] == '·')
                {
                    WriteChar((char)250);
                }
                else if (aText[i] == '¹')
                {
                    WriteChar((char)251);
                }
                else if (aText[i] == '³')
                {
                    WriteChar((char)252);
                }
                else if (aText[i] == '²')
                {
                    WriteChar((char)253);
                }
                else if (aText[i] == '■')
                {
                    WriteChar((char)254);
                }
                else if (aText[i] == ' ')
                {
                    WriteChar((char)255);
                }
                else {
                    WriteChar(aText[i]);
                }
            }
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
