using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.TestRunner.UI
{
    public class ListViewLogMessage
    {
        public ListViewLogMessage(string _date, string _level, string _message)
        {
            Date = _date;
            Level = _level;
            Message = _message;
        }

        public string Date
        {
            get;
            set;
        }

        public string Level
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
