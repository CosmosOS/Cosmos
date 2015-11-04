
using System.Collections.Generic;

namespace DuNodes.System.Console
{
    public static partial class Console
    {
        public partial class Menu
        {
            public class Entry
            {
                public string text { get; set; }
                public string executeValue { get; set; }
                public bool isExecute = false;
                public bool isBack = false;
                public List<Entry> InnerEntries { get; set; }
                public bool isExit = false;

                public Entry()
                {
                    InnerEntries =  new List<Entry>();
                }

                public Entry(string text, bool isExecuter = false, string executeValue = "", bool isBack = false,
                    bool isExit = false)
                {
                    this.text = text;
                    this.isExit = isExit;
                    this.isBack = isBack;
                    isExecute = isExecuter;
                    this.executeValue = executeValue;
                }
            }
        }
    }
}
