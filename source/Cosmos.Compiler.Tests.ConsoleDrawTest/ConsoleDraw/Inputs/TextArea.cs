using ConsoleDraw.Inputs.Base;
using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Inputs
{
    public class TextArea : Input
    {
        private bool Selected = false;

        private int CursorPostion;

        private int cursorDisplayX;
        private int CursorDisplayX { get { return cursorDisplayX; } set { cursorDisplayX = value; SetOffset(); } }

        private int CursorDisplayY;

        private int Offset = 0;
        private List<String> SplitText = new List<String>();
        private String text = "";
        private String Text {
            get{
                return text;
            } 
            set {
                if (OnChange != null && text != value)
                    OnChange(); 
                
                text = value;

                SplitText = CreateSplitText();
            }
        }
        private String TextWithoutNewLine { get { return RemoveNewLine(Text); } }

        private ConsoleColor TextColour = ConsoleColor.White;
        public ConsoleColor BackgroundColour = ConsoleColor.Blue;

        private Cursor cursor = new Cursor();

        public Action OnChange;

        public TextArea(int x, int y, int width, int height, String iD, Window parentWindow) : base(x, y, height, width, parentWindow, iD)
        {
            Selectable = true;
        }

        public override void Select()
        {
            if (!Selected)
            {
                Selected = true;
                Draw();
            }
        }

        public override void Unselect()
        {
            if (Selected)
            {
                Selected = false;
                Draw();
            }
        }

        public override void AddLetter(char letter)
        {
            String textBefore = Text.Substring(0, CursorPostion);
            String textAfter = Text.Substring(CursorPostion, Text.Length - CursorPostion);

            Text = textBefore + letter + textAfter;

            CursorPostion++;
            Draw();
        }

        public override void CursorMoveLeft()
        {
            if (CursorPostion != 0)
                CursorPostion--;
                       
            Draw();
        }

        public override void CursorMoveRight()
        {
            if (CursorPostion != Text.Length)
            {
                CursorPostion++;
                Draw();
            }
        }

        public override void CursorMoveDown()
        {
            var splitText = SplitText;

            if (splitText.Count == CursorDisplayX + 1) //Cursor at end of text in text area
            {
                ParentWindow.MovetoNextItemDown(Xpostion, Ypostion, Width);
                return;
            }

            var nextLine = splitText[CursorDisplayX + 1];

            var newCursor = 0;
            for (var i = 0; i < cursorDisplayX + 1; i++)
            {
                newCursor += splitText[i].Count();
            }

            if (nextLine.Count() > CursorDisplayY)
                newCursor += CursorDisplayY;
            else
                newCursor += nextLine.Where(x => x != '\n').Count();


            CursorPostion = newCursor;

            Draw();
        }

        public override void CursorMoveUp()
        {
            var splitText = SplitText;

            if (0 == CursorDisplayX) //Cursor at top of text area
            {
                ParentWindow.MovetoNextItemUp(Xpostion, Ypostion, Width);
                return;
            }

            var nextLine = splitText[CursorDisplayX - 1];

            var newCursor = 0;
            for (var i = 0; i < cursorDisplayX - 1; i++)
            {
                newCursor += splitText[i].Count();
            }

            if (nextLine.Count() >= CursorDisplayY)
                newCursor += CursorDisplayY;
            else
                newCursor += nextLine.Where(x => x!='\n').Count();

            CursorPostion = newCursor;
            Draw();
        }

        public override void CursorToStart()
        {
            var splitText = SplitText;

            var newCursor = 0;
            for (var i = 0; i < cursorDisplayX; i++)
            {
                newCursor += splitText[i].Count();
            }

            CursorPostion = newCursor;
            Draw();
        }

        public override void CursorToEnd()
        {
            var splitText = SplitText;
            var currentLine = splitText[cursorDisplayX];

            var newCursor = 0;
            for (var i = 0; i < cursorDisplayX + 1; i++)
            {
                newCursor += splitText[i].Count();
            }

            CursorPostion = newCursor - currentLine.Count(x => x == '\n');
            Draw();
        }

        public override void BackSpace()
        {
            if (CursorPostion != 0)
            {
                String textBefore = Text.Substring(0, CursorPostion);
                String textAfter = Text.Substring(CursorPostion, Text.Length - CursorPostion);

                textBefore = textBefore.Substring(0, textBefore.Length - 1);

                Text = textBefore + textAfter;
                CursorPostion--;
                Draw();
            }
        }

        public override void Enter()
        {
            AddLetter('\n');
        }

        public void SetText(String text)
        {
            Text = text;
            CursorPostion = 0;
            Draw();
        }

        public String GetText()
        {
            return Text;
        }

        public override void Draw()
        {
            RemoveCursor();

            UpdateCursorDisplayPostion();

            var lines = SplitText;

            //Draw test area
            for (var i = Offset; i < Height + Offset; i++)
            {
                var line = ' ' +  "".PadRight(Width - 1, ' ');
                if(lines.Count > i)
                    line = ' ' + RemoveNewLine(lines[i]).PadRight(Width - 1, ' ');

                WindowManager.WirteText(line, i + Xpostion - Offset, Ypostion, TextColour, BackgroundColour);
            }
               
            if (Selected)
                ShowCursor();
        
            //Draw Scroll Bar
            WindowManager.DrawColourBlock(ConsoleColor.White, Xpostion, Ypostion + Width, Xpostion + Height, Ypostion + Width + 1);
            
            double linesPerPixel = (double)lines.Count() / (Height);
            var postion = 0;
            if(linesPerPixel > 0)
              postion = (int)Math.Floor(cursorDisplayX / linesPerPixel);

            WindowManager.WirteText("■", Xpostion + postion, Ypostion + Width, ConsoleColor.DarkGray, ConsoleColor.White);
        }

        private List<String> CreateSplitText()
        {
            List<String> splitText = new List<String>();
            
            var lastSplit = 0;
            for (var i = 0; i < Text.Count() + 1; i++)
            {
                if (Text.Count() > i && Text[i] == '\n')
                {
                    splitText.Add(Text.Substring(lastSplit, i - lastSplit + 1));
                    lastSplit = i + 1;
                }
                else if (i - lastSplit == Width - 2)
                {
                    splitText.Add(Text.Substring(lastSplit, i - lastSplit));
                    lastSplit = i;
                }
                
                if (i == Text.Count())
                    splitText.Add(Text.Substring(lastSplit, Text.Count() - lastSplit));
            }

            return splitText.Select(x => x.Replace('\r', ' ')).ToList();
        }

        private void ShowCursor()
        {
            cursor.PlaceCursor(Xpostion + CursorDisplayX - Offset, Ypostion + 1 + CursorDisplayY, (Text + ' ')[CursorPostion], BackgroundColour);
        }

        private void UpdateCursorDisplayPostion()
        {
            var lines = SplitText;
            var displayX = 0;
            var displayY = 0;

            for (var i = 0; i < CursorPostion; i++)
            {
                if (lines[displayX].Count() > displayY && lines[displayX][displayY] == '\n') //Skip NewLine characters
                {
                    displayY++;
                }

                if (lines.Count > displayX)
                {
                    if (lines[displayX].Count() > displayY)
                        displayY++;
                    else if (lines.Count - 1 > displayX)
                    {
                        displayX++;
                        displayY = 0;
                    }

                }

                if (displayY == 0 && displayX - 1 >= 0 && lines[displayX - 1].Last() != '\n') //Wordwrap Stuff
                {
                    displayY++;
                }
                else if (displayY == 1 && displayX - 1 >= 0 && lines[displayX - 1].Last() != '\n')
                {
                    displayY--;
                }
                
            }

            CursorDisplayX = displayX;
            CursorDisplayY = displayY;
        }

        private void RemoveCursor()
        {
            cursor.RemoveCursor();
        }

        private void SetOffset()
        {
            while (CursorDisplayX - Offset > Height - 1)
                Offset++;

            while (CursorDisplayX - Offset < 0)
                Offset--;
        }

        private String RemoveNewLine(String text)
        {
            var toReturn = "";

            foreach (var letter in text)
            {
                if (letter != '\n')
                    toReturn += letter;
            }

            return toReturn;
        }
    }
}
