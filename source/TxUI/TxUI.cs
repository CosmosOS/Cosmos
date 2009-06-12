using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace Cosmos.Playground.Xenni.TxUI
{
    public sealed class TxUIManager
    {
        public static void Beep()
        {
            PIT.EnableSound();
            PIT.T2Frequency = 1536;
            PIT.Wait(150);
            PIT.DisableSound();
        }

        public static TxUIManager Instance = null;
        private List<TxCtrl> Controls = new List<TxCtrl>();
        private TxCtrl FocusTarget = null;
        private int CtrlCnt = 0;

        public delegate void dOnUIFrame();
        public dOnUIFrame OnUIFrame = null;

        public class PointInfo
        {
            public char Character;
            public ConsoleColor ForeColor;
            public ConsoleColor BackColor;
            public bool Changed = true;
        }

        public bool OveridePresent = false;

        private List<List<PointInfo>> RenderTarget = new List<List<PointInfo>>();

        public TxUIManager(VGAScreen.TextSize ScrSz)
            :base()
        {
            Cosmos.Kernel.Heap.EnableDebug = false;

            TextScreen.ReallyClearScreen();

            for (int x = 0;x < Width;x++)
            {
                RenderTarget.Add(new List<PointInfo>());
                for (int y = 0;y < Height;y++)
                {
                    RenderTarget[RenderTarget.Count - 1].Add(new PointInfo() { ForeColor = ConsoleColor.White, BackColor = ConsoleColor.Black, Character = ' ' });
                }
            }

            TextScreen.CurrentChar = -1;
            TextScreen.CurrentRow = -1;

            Present();
        }

        public void SetPoint(int x, int y, PointInfo info)
        {
            RenderTarget[x][y] = info;
        }
        public void SetPoint(int x, int y, ConsoleColor fore, ConsoleColor back, char c)
        {
            RenderTarget[x][y] = new PointInfo() { BackColor = back, ForeColor = fore, Character = c };
        }
        public PointInfo GetPoint(int x, int y)
        {
            return RenderTarget[x][y];
        }

        public void DrawAll()
        {
            if (OveridePresent)
                return;

            for (int i = 0;i < Controls.Count;i++)
                Controls[i].Draw();
        }

        public void Present()
        {
            if (OveridePresent)
                return;

            for (int x = 0;x < Width;x++)
            {
                for (int y = 0;y < Height;y++)
                {
                    if (!RenderTarget[x][y].Changed)
                    {
                        continue;
                    }

                    TextScreen.SetColors(RenderTarget[x][y].ForeColor, RenderTarget[x][y].BackColor);
                    TextScreen.PutChar(y, x, RenderTarget[x][y].Character);

                    RenderTarget[x][y].Changed = false;
                }
            }
        }

        public int Width
        {
            get
            {
                return TextScreen.Columns;
            }
        }
        public int Height
        {
            get
            {
                return TextScreen.Rows + 1;
            }
        }

        public void RegisterControl(TxCtrl ctrl)
        {
            if (ctrl._CtrlID != -1)
                throw new InvalidOperationException("Control Allready Registered!");

            Controls.Add(ctrl);
            ctrl._CtrlID = (CtrlCnt++);
        }
        public void UnregisterControl(TxCtrl ctrl)
        {
            Controls.Remove(ctrl);
            ctrl._CtrlID = -1;

            if (ctrl == FocusTarget)
                FocusTarget = null;
        }
        public void Focus(TxCtrl ctrl)
        {
            if (FocusTarget != null)
            {
                FocusTarget._HasFocus = false;
                FocusTarget.OnDefocus();
            }
            FocusTarget = ctrl;
            FocusTarget._HasFocus = true;
            FocusTarget.OnFocus();
        }
        public void Defocus()
        {
            if (FocusTarget != null)
            {
                FocusTarget._HasFocus = false;
                FocusTarget.OnDefocus();
            }

            FocusTarget = null;
        }

        public bool EnableCursor = true;

        public void OnKeyStateChange(char c)
        {
                if (Controls.Count == 0)
                {
                    return;
                }

                int csel = -1;
                switch (c)
                {
                    case '`':
                        if (Keyboard.AltPressed)
                        {
                            goto default;
                        }
                        if (FocusTarget == null)
                        {
                            Focus(Controls[Controls.Count - 1]);
                            break;
                        }

                        for (int i = 0;i < Controls.Count;i++)
                        {
                            if (Controls[i]._CtrlID == FocusTarget._CtrlID)
                            {
                                csel = i - 1;
                                break;
                            }
                        }
                        if (csel == -1)
                            csel = Controls.Count - 1;
                        Focus(Controls[csel]);
                        break;
                    case '\t':
                        if (Keyboard.AltPressed)
                        {
                            goto default;
                        }
                        if (FocusTarget == null)
                        {
                            Focus(Controls[0]);
                            break;
                        }
                        for (int i = 0;i < Controls.Count;i++)
                        {
                            if (Controls[i]._CtrlID == FocusTarget._CtrlID)
                            {
                                csel = i + 1;
                                break;
                            }
                        }
                        if (csel == Controls.Count)
                            csel = 0;
                        Focus(Controls[csel]);
                        break;
                    case '~':
                        if (Keyboard.AltPressed)
                        {
                            goto default;
                        }
                        Defocus();
                        break;
                    default:
                        if (FocusTarget == null || !FocusTarget.OnKeyPress(c))
                        {
                            TxUIManager.Beep();
                        }
                        break;
            }
        }

        private bool doRun = true;

        public void Run()
        {
            doRun = true;
            Keyboard.KeyMapping c = null;

            while (doRun)
            {
                if (OnUIFrame != null)
                {
                    OnUIFrame();
                }

                CPU.Halt();

                while (Keyboard.GetMapping(out c))
                {
                    if (c == null)
                    {
                        continue;
                    }

                    /*
                        Cosmos.Debug.Debugger.Send(
                            "Key: '" + c.ToString() + "'; " + 
                            (Keyboard.ShiftPressed ? "SHIFT " : "") +
                            (Keyboard.CtrlPressed ? "CTRL " : "") +
                            (Keyboard.AltPressed ? "ALT" : "")
                        );
                    */

                    OnKeyStateChange(c.Value);
                }
            }
        }
        public void Stop()
        {
            doRun = false;
        }
    }

    public abstract class TxCtrl
    {
        private int _X = 0;
        private int _Y = 0;
        private int _Width = 0;
        private int _Height = 0;
        internal bool _HasFocus = false;
        internal int _CtrlID = -1;

        public bool HasFocus
        {
            get
            {
                return _HasFocus;
            }
        }

        public int Width
        {
            get
            {
                return _Width;
            }
            set
            {
                _Width = value;

                OnResize();
            }
        }
        public int Height
        {
            get
            {
                return _Height;
            }
            set
            {
                _Height = value;

                OnResize();
            }
        }
        public int X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
                OnMove();
            }
        }
        public int Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;

                OnMove();
            }
        }
        public int CtrlID
        {
            get
            {
                return _CtrlID;
            }
        }

        internal abstract void Draw();
        internal virtual void OnResize()
        {
            TxUIManager.Instance.DrawAll();
            TxUIManager.Instance.Present();
        }
        internal virtual void OnMove()
        {
            TxUIManager.Instance.DrawAll();
            TxUIManager.Instance.Present();
        }
        internal virtual void OnFocus()
        {
            if (TxUIManager.Instance.EnableCursor)
            {
                TextScreen.CurrentRow = Y;
                TextScreen.CurrentChar = X;
            }
            else
            {
                TextScreen.CurrentRow = -1;
                TextScreen.CurrentChar = -1;
            }
        }
        internal virtual void OnDefocus()
        {
            TextScreen.CurrentRow = -1;
            TextScreen.CurrentChar = -1;
        }
        internal virtual bool OnKeyPress(char c)
        {
            return false;
        }

        protected void SetPoint(int x, int y, TxUIManager.PointInfo info)
        {
            TxUIManager.Instance.SetPoint(x + X, y + Y, info);
        }
        protected void SetPoint(int x, int y, ConsoleColor fore, ConsoleColor back, char c)
        {
            TxUIManager.Instance.SetPoint(x + X, y + Y, fore, back, c);
        }
        protected TxUIManager.PointInfo GetPoint(int x, int y)
        {
            return TxUIManager.Instance.GetPoint(x + X, y + Y);
        }
    }

    public class TxLabl : TxCtrl
    {
        public String Text = "";
        public ConsoleColor ForeColor = ConsoleColor.White;
        public ConsoleColor BackColor = ConsoleColor.Black;

        internal override void Draw()
        {
            char[] cText = Text.ToCharArray();

            for (int i = 0;i < Width;i++)
            {
                if (i < cText.Length)
                {
                    SetPoint(i, 0, ForeColor, BackColor, cText[i]);
                }
                else
                {
                    SetPoint(i, 0, ForeColor, BackColor, ' ');
                }
            }
        }
        internal override void OnResize()
        {
            if (Width < 1)
                Width = 1;
            if (Height != 1)
                Height = 1;
        }
    }

    public class TxBtn : TxCtrl
    {
        public String Text = "";

        public ConsoleColor UForeColor = ConsoleColor.White;
        public ConsoleColor UBackColor = ConsoleColor.Gray;
        public ConsoleColor FForeColor = ConsoleColor.White;
        public ConsoleColor FBackColor = ConsoleColor.Black;
        public ConsoleColor PForeColor = ConsoleColor.White;
        public ConsoleColor PBackColor = ConsoleColor.Black;

        private ConsoleColor _Fore = ConsoleColor.White;
        private ConsoleColor _Back = ConsoleColor.Black;

        public delegate void dOnActivate();
        public dOnActivate OnActivate = null;

        private bool SupressColorEval = false;

        internal override void OnDefocus()
        {
            Draw();
            TxUIManager.Instance.Present();

            base.OnDefocus();
        }
        internal override void OnFocus()
        {
            Draw();
            TxUIManager.Instance.Present();

            TxUIManager.Instance.EnableCursor = false;
            base.OnFocus();
            TxUIManager.Instance.EnableCursor = true;
        }
        internal override void OnResize()
        {
            if (Width < 1)
                Width = 1;
            if (Height < 1)
                Height = 1;

            base.OnResize();
        }
        internal override bool OnKeyPress(char c)
        {
            if (c == ' ' || c == '\n')
            {
                _Fore = PForeColor;
                _Back = PBackColor;

                SupressColorEval = true;
                Draw();
                SupressColorEval = false;
                TxUIManager.Instance.Present();

                if (OnActivate != null)
                    OnActivate();

                Draw();
                TxUIManager.Instance.Present();

                return true;
            }

            return base.OnKeyPress(c);
        }
        internal override void Draw()
        {
            char[] cText = Text.ToCharArray();

            if (!SupressColorEval)
            {
                if (HasFocus)
                {
                    _Fore = FForeColor;
                    _Back = FBackColor;
                }
                else
                {
                    _Fore = UForeColor;
                    _Back = UBackColor;
                }
            }

            int spaceabove = (Height / 2);
            int spacebelow = Height - (1 + spaceabove);

            for (int y = 0;y < spaceabove;y++)
            {
                for (int x = 0;x < Width;x++)
                {
                    SetPoint(x, y, _Fore, _Back, ' ');
                }
            }

            for (int x = 0;x < Width;x++)
            {
                if (x < cText.Length)
                {
                    SetPoint(x, spaceabove, _Fore, _Back, cText[x]);
                }
                else
                {
                    SetPoint(x, spaceabove, _Fore, _Back, ' ');
                }
            }

            for (int y = 0;y < spacebelow;y++)
            {
                for (int x = 0;x < Width;x++)
                {
                    SetPoint(x, y + spaceabove + 1, _Fore, _Back, ' ');
                }
            }
        }
    }

    public class TxEditbox : TxCtrl
    {
        public ConsoleColor UForeColor = ConsoleColor.White;
        public ConsoleColor UBackColor = ConsoleColor.Gray;
        public ConsoleColor FForeColor = ConsoleColor.White;
        public ConsoleColor FBackColor = ConsoleColor.Black;

        internal override void OnDefocus()
        {
            Draw();
            TxUIManager.Instance.Present();

            base.OnDefocus();
        }
        internal override void OnFocus()
        {
            _CursorPosX = 0;
            _CursorPosY = 0;

            Draw();
            TxUIManager.Instance.Present();

            base.OnFocus();
        }
        internal override void OnResize()
        {
            if (Width < 1)
                Width = 1;
            if (Height < 1)
                Height = 1;

            if (Lines.Length != Height)
            {
                String[] newlines = new String[Height];

                for (int i = 0;i < Lines.Length;i++)
                {
                    newlines[i] = Lines[i];
                }
                for (int i = Lines.Length;i < Height;i++)
                {
                    newlines[i] = "";
                }

                Lines = newlines;
            }

            for (int i = 0;i < Lines.Length;i++)
            {
                if (Lines[i].Length > Width)
                {
                    Lines[i] = Lines[i].Substring(0, Width);
                }
            }

            if (this.HasFocus)
            {
                CursorPosX = 0;
                CursorPosY = 0;
            }

            base.OnResize();
        }
        internal override bool OnKeyPress(char c)
        {
            switch (c)
            {
                case '\u2190':
                    if (Keyboard.AltPressed)
                    {
                        goto default;
                    }
                    CursorPosX--;
                    break;
                case '\u2192':
                    if (Keyboard.AltPressed)
                    {
                        goto default;
                    }
                    CursorPosX++;
                    break;
                case '\u0968':
                    if (_CursorPosX == 0)
                    {
                        TxUIManager.Beep();
                        break;
                    }
                    Lines[_CursorPosY] = Lines[_CursorPosY].Substring(0, _CursorPosX - 1) + Lines[_CursorPosY].Substring(_CursorPosX, Lines[_CursorPosY].Length - _CursorPosX);
                    CursorPosX--;

                    if (OnTextChanged != null)
                        OnTextChanged();

                    Draw();
                    TxUIManager.Instance.Present();

                    break;
                case '\u2191':
                    if (Keyboard.AltPressed)
                    {
                        goto default;
                    }
                    if (_CursorPosY == 0)
                    {
                        TxUIManager.Beep();
                        break;
                    }

                    CursorPosY--;

                    if (_CursorPosX > Lines[_CursorPosY].Length)
                    {
                        CursorPosX = Lines[_CursorPosY].Length;
                    }
                    break;
                case '\u2193':
                case '\n':
                    if (Keyboard.AltPressed)
                    {
                        goto default;
                    }
                    if (_CursorPosY == (Height - 1))
                    {
                        TxUIManager.Beep();
                        break;
                    }

                    CursorPosY++;

                    if (_CursorPosX > Lines[_CursorPosY].Length)
                    {
                        CursorPosX = Lines[_CursorPosY].Length;
                    }
                    break;
                default:
                    if (_CursorPosX == Width || Lines[_CursorPosY].Length == Width)
                    {
                        TxUIManager.Beep();
                        break;
                    }
                    Lines[_CursorPosY] = Lines[_CursorPosY].Substring(0, _CursorPosX) + c.ToString() + Lines[_CursorPosY].Substring(_CursorPosX, Lines[_CursorPosY].Length - _CursorPosX);
                    CursorPosX++;

                    if (OnTextChanged != null)
                        OnTextChanged();

                    Draw();
                    TxUIManager.Instance.Present();

                    break;
            }

            base.OnKeyPress(c);
            return true;
        }

        private int _CursorPosX = 0;
        private int _CursorPosY = 0;
        public int CursorPosX
        {
            get
            {
                return _CursorPosX;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                    TxUIManager.Beep();
                }
                if (value > Width)
                {
                    value = Width;
                    TxUIManager.Beep();
                }
                if (value > Lines[_CursorPosY].Length)
                {
                    value = Lines[_CursorPosY].Length;
                    TxUIManager.Beep();
                }

                _CursorPosX = value;
                TextScreen.CurrentChar = (X + _CursorPosX);
            }
        }
        public int CursorPosY
        {
            get
            {
                return _CursorPosY;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                    TxUIManager.Beep();
                }
                if (value >= Height)
                {
                    value = (Height - 1);
                    TxUIManager.Beep();
                }

                _CursorPosY = value;
                TextScreen.CurrentRow = (Y + _CursorPosY);
            }
        }

        public delegate void dOnTextChanged();
        public dOnTextChanged OnTextChanged = null;

        private String[] Lines = new String[0];

        internal override void Draw()
        {
            ConsoleColor ForeColor = (HasFocus ? FForeColor : UForeColor);
            ConsoleColor BackColor = (HasFocus ? FBackColor : UBackColor);

            for (int y = 0;y < Height;y++)
            {
                char[] cText = Lines[y].ToCharArray();
                for (int x = 0;x < Width;x++)
                {
                    if (x < cText.Length)
                    {
                        SetPoint(x, y, ForeColor, BackColor, cText[x]);
                    }
                    else
                    {
                        SetPoint(x, y, ForeColor, BackColor, ' ');
                    }
                }
            }
        }

        public String this[int indx]
        {
            get
            {
                return Lines[indx];
            }
            set
            {
                Lines[indx] = value;
            }
        }
        public String Text
        {
            get
            {
                String txt = "";
                for (int i = 0;i < Lines.Length;i++)
                {
                    txt = txt + Lines[i] + "\n";
                }

                return txt.Trim('\n');
            }
        }
    }
}