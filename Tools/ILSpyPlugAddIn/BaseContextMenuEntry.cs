using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.ILSpy;

namespace Cosmos.ILSpyPlugs.Plugin
{
    public abstract class BaseContextMenuEntry: IContextMenuEntry
    {
        public virtual bool IsVisible(TextViewContext context)
        {
            return true;
        }

        public virtual bool IsEnabled(TextViewContext context)
        {
            return true;
        }

        public abstract void Execute(TextViewContext context);
    }
}