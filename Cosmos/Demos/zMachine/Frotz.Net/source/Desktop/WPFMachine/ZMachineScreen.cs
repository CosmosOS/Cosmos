using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frotz.Screen;

namespace WPFMachine
{
    interface ZMachineScreen
    {
        void AddInput(char InputKeyPressed); // TODO This could be named better
        void SetCharsAndLines();
        ScreenMetrics Metrics { get; }
        void setFontInfo();
        void Focus();
        event EventHandler<GameSelectedEventArgs> GameSelected;
        void Reset();

    }
}
