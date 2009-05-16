using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Cosmos.Compiler.Builder
{
    public interface IMainWindow
    {

        //Toggle
        void ShowOptions();
        void ShowBuildProgress();

        void AddToLog(string logMsg);

        void ThreadedClose(); 

        IOptionUC OptionUC { get;}
        IBuildProgressUC BuildProgressUC { get; }

        Dispatcher Dispatcher { get; }


    }
}
