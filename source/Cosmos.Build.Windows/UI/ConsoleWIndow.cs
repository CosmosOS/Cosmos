using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Cosmos.Compiler.Builder
{
    public class ConsoleWindow
    {

        protected int mConsoleWindow;
        [DllImport("user32.dll")]
        protected static extern int ShowWindow(int aHandle, int aShowState);

        [DllImport("kernel32.dll")]
        protected static extern int GetConsoleWindow();

        public ConsoleWindow()
        {
            mConsoleWindow = GetConsoleWindow();
            //UIEvents.HideConsoleWindowEvent += new Action(UIEvents_HideConsoleWindowEvent);
            //UIEvents.ShowConsoleWindowEvent += new Action(UIEvents_ShowConsoleWindowEvent);

        }

        private void UIEvents_ShowConsoleWindowEvent()
        {
            ShowWindow();


        }

        internal void ShowWindow()
        {
            ShowWindow(mConsoleWindow, 1);
        }

        private void UIEvents_HideConsoleWindowEvent()
        {
            HideWindow();
        }

        internal void HideWindow()
        {
            ShowWindow(mConsoleWindow, 0); //Hide!
        }
    }
}
