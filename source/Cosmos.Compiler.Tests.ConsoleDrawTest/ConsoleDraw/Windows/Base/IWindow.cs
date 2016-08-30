using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDraw.Windows.Base
{
    public abstract class AWindow
    {
        abstract public void ReDraw();

        public Window ParentWindow;
    }
}
