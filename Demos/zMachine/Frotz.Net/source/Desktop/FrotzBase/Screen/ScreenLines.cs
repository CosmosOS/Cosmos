using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frotz.Screen {
    public class ScreenLines {
        List<LineInfo> _lines = new List<LineInfo>();

        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public ScreenLines(int Rows, int Columns) {
            this.Rows = Rows;
            this.Columns = Columns;
            
            _lines = new List<LineInfo>();
            for (int i = 0; i < Rows; i++) {
                _lines.Add(new LineInfo(Columns * 3));
            }
        }

        public void SetChar(int Row, int Col, char c, CharDisplayInfo FandS) {
            // TODO Check boundaries
            _lines[Row].SetChar(Col, c, FandS);
        }

        public void ScrollLines(int top, int numlines) {
            // TODO Check boundaries
            for (int i = 0; i < numlines; i++) {
                if (_lines.Count > 0)
                    if (_lines.Count > top) {
                        _lines.RemoveAt(top);
                    } else {
                        _lines.RemoveAt(0);
                    }
            }

            // TODO Fix up this so I don't have to add lines back in
            addLines();
        }

        public void ScrollArea(int top, int bottom, int left, int right, int units) {
            // TODO Do something with units
            // TODO Check Boundaries
            int numchars = right - left + 1;
            String replace = "".PadRight(numchars);
            for (int i = bottom - 1; i >= top; i--) {
                String temp = _lines[i].GetString(left, numchars);
                _lines[i].Replace(left, replace);
                replace = temp;
            }
        }

        public void Clear() {
            ClearArea(0, 0, Rows, Columns * 3);
        }

        public void ClearArea(int top, int left, int bottom, int right) {
            // TODO Check this boundary
            for (int i = top; i < bottom; i++) {
                for (int j = left; j < right; j++) {
                    _lines[i].ClearChar(j);
                }
            }
        }

        private void addLines() {
            while (_lines.Count <= Rows * 2) {
                lock (_lines) {
                    _lines.Add(new LineInfo(Columns * 3));
                }
            }
        }

        public void RemoveChars(int Row, int Col, int count) {
            // TODO Check this boundary
            _lines[Row].RemoveChars(count);
        }

        public String GetText(out List<FontChanges> Changes) {
            return GetTextToLine(Rows, out Changes);
        }

        public List<LineInfo> GetLines()
        {
            return _lines;
        }

        public String GetTextToLine(int Line, out List<FontChanges> Changes) {
            int pos = 0;

            Changes = new List<FontChanges>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Line; i++) {
                sb.Append(_lines[i].GetString());
                sb.Append("\r\n");

                // Start col needs to stay per line, and there needs to be pos offset per line
                var tempChanges = _lines[i].GetTextWithFontInfo();
                foreach (var c in tempChanges) {
                    c.Offset += pos;
                    c.Line = i;
                }

                Changes.AddRange(tempChanges);

                pos = sb.Length;
            }

            return sb.ToString();
        }

        public String GetTextAtLine(int line) {
            return _lines[line].GetString();
        }

        public CharDisplayInfo GetFontAndStyle(int Row, int Column) {
            return _lines[Row].GetFontAndStyle(Column);
        }
    }
}
