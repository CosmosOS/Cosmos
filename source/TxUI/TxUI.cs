using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Cosmos.Hardware;

namespace Cosmos.Playground.Xenni.TxUI
{
    public sealed class TxUIManager
    {
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
                    TextScreen.SetColors(RenderTarget[x][y].ForeColor, RenderTarget[x][y].BackColor);
                    TextScreen.PutChar(y, x, RenderTarget[x][y].Character);
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
                        Defocus();
                        break;
                    default:
                        if (FocusTarget == null || !FocusTarget.OnKeyPress(c))
                        {
                            Console.Beep();
                        }
                        break;
            }
        }

        private bool doRun = true;

        public void Run()
        {
            doRun = true;
            char c = '\0';

            while (doRun)
            {
                if (OnUIFrame != null)
                {
                    OnUIFrame();
                }

                while (Keyboard.GetChar(out c))
                {
                    if (c == '\0')
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

                    OnKeyStateChange(c);
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

        public abstract void Draw();
        public virtual void OnResize() { TxUIManager.Instance.DrawAll(); TxUIManager.Instance.Present(); }
        public virtual void OnMove() { TxUIManager.Instance.DrawAll(); TxUIManager.Instance.Present(); }
        public virtual void OnFocus() { TextScreen.CurrentRow = Y; TextScreen.CurrentChar = X; }
        public virtual void OnDefocus() { TextScreen.CurrentRow = -1; TextScreen.CurrentChar = -1; }
        public virtual bool OnKeyPress(char c) { return false; }

        public void SetPoint(int x, int y, TxUIManager.PointInfo info)
        {
            TxUIManager.Instance.SetPoint(x + X, y + Y, info);
        }
        public void SetPoint(int x, int y, ConsoleColor fore, ConsoleColor back, char c)
        {
            TxUIManager.Instance.SetPoint(x + X, y + Y, fore, back, c);
        }
        public TxUIManager.PointInfo GetPoint(int x, int y)
        {
            return TxUIManager.Instance.GetPoint(x + X, y + Y);
        }
    }

    public class TxLabl : TxCtrl
    {
        public String Text = "";
        public ConsoleColor ForeColor = ConsoleColor.White;
        public ConsoleColor BackColor = ConsoleColor.Black;

        public override void Draw()
        {
            char[] cText = Text.ToCharArray();

            for (int i = 0;i < Math.Min(cText.Length, Width);i++)
                SetPoint(i, 0, ForeColor, BackColor, cText[i]);
        }
        public override void OnResize()
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

        public override void OnDefocus()
        {
            Draw();
            TxUIManager.Instance.Present();

            base.OnDefocus();
        }
        public override void OnFocus()
        {
            Draw();
            TxUIManager.Instance.Present();

            base.OnFocus();
        }
        public override void OnResize()
        {
            if (Width < 1)
                Width = 1;
            if (Height != 1)
                Height = 1;
        }
        public override bool OnKeyPress(char c)
        {
            if (c == ' ')
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
        public override void Draw()
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

            for (int i = 0;i < Math.Min(cText.Length, Width);i++)
                SetPoint(i, 0, _Fore, _Back, cText[i]);
        }
    }
}
