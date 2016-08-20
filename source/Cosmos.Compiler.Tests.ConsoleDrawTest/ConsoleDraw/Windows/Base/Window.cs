#define LoopTest
#define TEST

using ConsoleDraw.Inputs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleDraw.Windows.Base
{
    public class Window : AWindow
    {
        public Boolean Exit;
        protected Input CurrentlySelected;

        public int PostionX { get; private set; }
        public int PostionY { get; private set; }
        public int Width {get; private set;}
        public int Height { get; private set; }

        public ConsoleColor BackgroundColour = ConsoleColor.Gray;
        
        public List<Input> Inputs = new List<Input>();

        public Window(int postionX, int postionY, int width, int height, Window parentWindow)
        {
            PostionY = postionY;
            PostionX = postionX;
            Width = width;
            Height = height;

            ParentWindow = parentWindow;
        }

        public void Draw()
        {
#if TEST
            if (ParentWindow != null)
                ParentWindow.Draw();

            ReDraw();
                for(int i = 0; i < Inputs.Count; i++)
            {
                var input = Inputs[i];
                input.Draw();
            }

                if (CurrentlySelected != null)
                    CurrentlySelected.Select();
               // SetSelected();
#endif
        }

        public override void ReDraw()
        {
            
        }
        
        /// <danger>
        /// This code confuses IL2CPU.
        /// </danger>
        public void MainLoop()
        {
#if LoopTest
            while (!Exit && !ProgramInfo.ExitProgram)
            {
                var input = ReadKey();

                if (input.Key == ConsoleKey.Tab)
                    CurrentlySelected.Tab();
                else if (input.Key == ConsoleKey.Enter)
                    CurrentlySelected.Enter();
                else if (input.Key == ConsoleKey.LeftArrow)
                    CurrentlySelected.CursorMoveLeft();
                else if (input.Key == ConsoleKey.RightArrow)
                    CurrentlySelected.CursorMoveRight();
                else if (input.Key == ConsoleKey.UpArrow)
                    CurrentlySelected.CursorMoveUp();
                else if (input.Key == ConsoleKey.DownArrow)
                    CurrentlySelected.CursorMoveDown();
                else if (input.Key == ConsoleKey.Backspace)
                    CurrentlySelected.BackSpace();
                else if (input.Key == ConsoleKey.Home)
                    CurrentlySelected.CursorToStart();
                else if (input.Key == ConsoleKey.End)
                    CurrentlySelected.CursorToEnd();
                else
                    CurrentlySelected.AddLetter((Char)input.KeyChar); // Letter(input.KeyChar);
            }
#endif
        }

        public void SelectFirstItem()
        {
            if (Inputs.All(x => !x.Selectable)) //No Selectable inputs on page
                return;

            CurrentlySelected = Inputs.First(x => x.Selectable);

            SetSelected();
        }

        public void SelectItemByID(String Id)
        {
            if (Inputs.All(x => !x.Selectable)) //No Selectable inputs on page
                return;

            var newSelectedInput = Inputs.FirstOrDefault(x => x.ID == Id);
            if (newSelectedInput == null) //No Input with this ID
                return;

            CurrentlySelected = newSelectedInput;

            SetSelected();
        }

        public void MoveToNextItem()
        {
            if (Inputs.All(x => !x.Selectable)) //No Selectable inputs on page
                return;

            if (Inputs.Count(x => x.Selectable) == 1) //Only one selectable input on page, thus no point chnaging it
                return;

            var IndexOfCurrent = Inputs.IndexOf(CurrentlySelected);

            while (true)
            {
                IndexOfCurrent = MoveIndexAlongOne(IndexOfCurrent);

                if (Inputs[IndexOfCurrent].Selectable)
                    break;
            }
            CurrentlySelected = Inputs[IndexOfCurrent];

            SetSelected();
        }

        public void MoveToLastItem()
        {
            if (Inputs.All(x => !x.Selectable)) //No Selectable inputs on page
                return;

            if (Inputs.Count(x => x.Selectable) == 1) //Only one selectable input on page, thus no point chnaging it
                return;

            var IndexOfCurrent = Inputs.IndexOf(CurrentlySelected);

            while (true)
            {
                IndexOfCurrent = MoveIndexBackOne(IndexOfCurrent);

                if (Inputs[IndexOfCurrent].Selectable)
                    break;
            }
            CurrentlySelected = Inputs[IndexOfCurrent];

            SetSelected();
        }

        public void MovetoNextItemRight(int startX, int startY, int searchHeight)
        {
            if (Inputs.All(x => !x.Selectable)) //No Selectable inputs on page
                return;

            if (Inputs.Count(x => x.Selectable) == 1) //Only one selectable input on page, thus no point chnaging it
                return;

            Input nextItem = null;
            while (nextItem == null && startY <= PostionY + Width)
            {
                foreach (var input in Inputs.Where(x => x.Selectable && x != CurrentlySelected))
                {
                    var overlap = DoAreasOverlap(startX, startY, searchHeight, 1, input.Xpostion, input.Ypostion, input.Height, input.Width);
                    if (overlap)
                    {
                        nextItem = input;
                        break; //end foreach 
                    }
                }
                startY++;
            }

            if (nextItem == null) //No element found to the right
            {
                MoveToNextItem();
                return;
            }

            CurrentlySelected = nextItem;
            SetSelected();
        }

        public void MovetoNextItemLeft(int startX, int startY, int searchHeight)
        {
            if (Inputs.All(x => !x.Selectable)) //No Selectable inputs on page
                return;

            if (Inputs.Count(x => x.Selectable) == 1) //Only one selectable input on page, thus no point chnaging it
                return;

            Input nextItem = null;
            while (nextItem == null && startY > PostionY)
            {
                foreach (var input in Inputs.Where(x => x.Selectable && x != CurrentlySelected))
                {
                    var overlap = DoAreasOverlap(startX, startY - 1, searchHeight, 1, input.Xpostion, input.Ypostion, input.Height, input.Width);
                    if (overlap)
                    {
                        nextItem = input;
                        break; //end foreach 
                    }
                }
                startY--;
            }

            if (nextItem == null) //No element found
            {
                MoveToLastItem();
                return;
            }

            CurrentlySelected = nextItem;
            SetSelected();
        }

        public void MovetoNextItemDown(int startX, int startY, int searchWidth)
        {
            if (Inputs.All(x => !x.Selectable)) //No Selectable inputs on page
                return;

            if (Inputs.Count(x => x.Selectable) == 1) //Only one selectable input on page, thus no point chnaging it
                return;

            Input nextItem = null;
            while (nextItem == null && startX <= PostionX + Height)
            {
                foreach (var input in Inputs.Where(x => x.Selectable && x != CurrentlySelected))
                {
                    var overlap = DoAreasOverlap(startX, startY, 1, searchWidth, input.Xpostion, input.Ypostion, input.Height, input.Width);
                    if (overlap)
                    {
                        nextItem = input;
                        break; //end foreach 
                    }
                }
                startX++;
            }

            if (nextItem == null) //No element found
            {
                MoveToNextItem();
                return;
            }

            CurrentlySelected = nextItem;
            SetSelected();
        }

        public void MovetoNextItemUp(int startX, int startY, int searchWidth)
        {
            if (Inputs.All(x => !x.Selectable)) //No Selectable inputs on page
                return;

            if (Inputs.Count(x => x.Selectable) == 1) //Only one selectable input on page, thus no point chnaging it
                return;

            Input nextItem = null;
            while (nextItem == null && startX > PostionX)
            {
                foreach (var input in Inputs.Where(x => x.Selectable && x != CurrentlySelected))
                {
                    var overlap = DoAreasOverlap(startX - 1, startY, 1, searchWidth, input.Xpostion, input.Ypostion, input.Height, input.Width);
                    if (overlap)
                    {
                        nextItem = input;
                        break; //end foreach 
                    }
                }
                startX--;
            }

            if (nextItem == null) //No element found
            {
                MoveToLastItem();
                return;
            }

            CurrentlySelected = nextItem;
            SetSelected();
        }


        private bool DoAreasOverlap(int areaOneX, int areaOneY, int areaOneHeight, int areaOneWidth, int areaTwoX, int areaTwoY, int areaTwoHeight, int areaTwoWidth)
        {
            var areaOneEndX = areaOneX + areaOneHeight - 1;
            var areaOneEndY = areaOneY + areaOneWidth - 1;
            var areaTwoEndX = areaTwoX + areaTwoHeight - 1;
            var areaTwoEndY = areaTwoY + areaTwoWidth - 1;

            var overlapsVertically = false;
            //Check if overlap vertically
            if (areaOneX >= areaTwoX && areaOneX < areaTwoEndX ) //areaOne starts in areaTwo
                overlapsVertically = true;
            else if (areaOneEndX >= areaTwoX && areaOneEndX <= areaTwoEndX) //areaOne ends in areaTwo
                overlapsVertically = true;
            else if (areaOneX < areaTwoX && areaOneEndX >= areaTwoEndX) //areaOne start before and end after areaTwo
                overlapsVertically = true;
            //areaOne inside areaTwo is caught by first two statements

            if (!overlapsVertically) //If it does not overlap vertically, then it does not overlap.
                return false;

            var overlapsHorizontally = false;
            //Check if overlap Horizontally
            if (areaOneY >= areaTwoY && areaOneY < areaTwoEndY) //areaOne starts in areaTwo
                overlapsHorizontally = true;
            else if (areaOneEndY >= areaTwoY && areaOneEndY < areaTwoEndY) //areaOne ends in areaTwo
                overlapsHorizontally = true;
            else if (areaOneY <= areaTwoY && areaOneEndY >= areaTwoEndY) //areaOne starts before and ends after areaTwo
                overlapsHorizontally = true;
            //areaOne inside areaTwo is caught by first two statements

            if (!overlapsHorizontally) //If it does not overlap Horizontally, then it does not overlap.
                return false;

            return true; //it overlaps vertically and horizontally, thus areas must overlap
        }

        private int MoveIndexAlongOne(int index)
        {
            if (Inputs.Count() == index + 1)
                return 0;

            return index + 1;
        }

        private int MoveIndexBackOne(int index)
        {
            if (index == 0)
                return Inputs.Count() - 1;

            return index - 1;
        }

        private void SetSelected()
        {
            Inputs.ForEach(x => x.Unselect());

            if(CurrentlySelected != null)
                CurrentlySelected.Select();
        }

        private static ConsoleKeyInfo ReadKey()
        {
            ConsoleKeyInfo input = Console.ReadKey(true);
          
            return input;
        }

        public AInput GetInputById(String Id)
        {
            return Inputs.FirstOrDefault(x => x.ID == Id);
        }

        public void ExitWindow()
        {
            Exit = true;
            if (ParentWindow != null)
                ParentWindow.Draw();
            //else
                //System.Environment.Exit(1);
        }
    }
}
