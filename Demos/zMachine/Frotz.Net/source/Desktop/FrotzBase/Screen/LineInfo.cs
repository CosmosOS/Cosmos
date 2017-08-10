using System;
using System.Collections.Generic;
using System.Text;

using Frotz.Constants;

namespace Frotz.Screen {
    public class LineInfo {
        private char[] _text;
        private CharDisplayInfo[] _styles;

        public int X { get; set; }
        public int Y { get; set; }
        public int LastCharSet { get; private set; }

        private int width;

        public LineInfo(int LineWidth) {
            _text = new char[LineWidth];
            _styles = new CharDisplayInfo[LineWidth];

            for (int i = 0; i < LineWidth; i++) {
                _text[i] = ' ';
                _styles[i] = CharDisplayInfo.Empty;
            }

            this.width = LineWidth;

            LastCharSet = -1;
        }

        public void SetChar(int pos, char c, CharDisplayInfo FandS) {
            lock (this) {
                _text[pos] = c;
                _styles[pos] = FandS;
                LastCharSet = Math.Max(pos, LastCharSet);

                _changes = null;
            }
        }

        public void AddChar(char c, CharDisplayInfo FandS) {
            SetChar(++LastCharSet, c, FandS);
        }

        public void ClearLine() {
            lock (this) {
                for (int i = 0; i < width; i++) {
                    ClearChar(i);
                }
                LastCharSet = -1;
            }
        }

        public void RemoveChars(int count) {
            lock (this) {
                LastCharSet -= count;
            }
        }

        public void ClearChar(int pos) {
            SetChar(pos, ' ', CharDisplayInfo.Empty);
        }

        public String CurrentString { get { return new String(_text, 0, LastCharSet + 1); } }

        public void Replace(int start, String NewString) {
            lock (this) {
                for (int i = 0; i < NewString.Length; i++) {
                    SetChar(start + i, NewString[i], CharDisplayInfo.Empty);
                }
            }
        }


        List<FontChanges> _changes;
        public List<FontChanges> GetTextWithFontInfo() {
            lock (this) {
                if (_changes == null)
                {
                    _changes = new List<FontChanges>();
                    string s = CurrentString;

                    FontChanges fc = new FontChanges(-1, 0, new CharDisplayInfo(-1, 0, 0, 0));
                    for (int i = 0; i < _styles.Length && i < s.Length; i++)
                    {
                        if (!_styles[i].AreSame(fc.FandS))
                        {
                            fc = new FontChanges(i, 1, _styles[i]);
                            fc.AddChar(s[i]);
                            _changes.Add(fc);
                        }
                        else
                        {
                            fc.Count++;
                            fc.AddChar(s[i]);
                        }
                    }
                }

                return _changes;

            }
        }

        public String GetString() {
            return new String(_text, 0, width);
        }

        public String GetString(int start, int length) {
            return new string(_text, start, length);
        }

        public override string ToString() {
            return new String(_text);
        }

        public CharDisplayInfo GetFontAndStyle(int Column) {
            return _styles[Column];
        }
    }
}
