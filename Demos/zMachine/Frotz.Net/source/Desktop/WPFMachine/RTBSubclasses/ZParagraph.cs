using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Documents;

namespace WPFMachine.RTBSubclasses
{
    public class ZParagraph : Paragraph
    {
        public ZParagraph()
        {
            
        }

        public double Top
        {
            get;
            set;
        }

        public double Width
        {
            get
            {
                double w = 0;
                foreach (ZRun run in base.Inlines)
                {
                    w += run.Width;
                }
                return w;
            }
        }

        public new InlineCollection Inlines
        {
            get { throw new ArgumentException("Please use Add/Clear functions"); }
        }

        public void AddInline(ZRun run)
        {
            base.Inlines.Add(run);
        }

        public void RemoveInline(ZRun run)
        {
            base.Inlines.Remove(run);
        }

        public void ClearInlines()
        {
            base.Inlines.Clear();
        }
    }

}
