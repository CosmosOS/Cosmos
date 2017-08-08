using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MonoCecilToEcmaCil1
{
    partial class Reader
    {
        protected bool mLogEnabled = false;
        protected string mMapPathname;
        protected TextWriter mLogWriter;
        protected struct LogItem
        {
            public string SrcType;
            public object Item;
        }
        protected Dictionary<object, List<LogItem>> mLogMap;


        private void LogMapPoint(object aSrc, string aSrcType, object aItem)
        {
            // Keys cant be null. If null, we just say ILScanner is the source
            if (aSrc == null)
            {
                aSrc = typeof(Reader);
            }

            var xLogItem = new LogItem()
            {
                SrcType = aSrcType,
                Item = aItem
            };
            List<LogItem> xList;
            if (!mLogMap.TryGetValue(aSrc, out xList))
            {
                xList = new List<LogItem>();
                mLogMap.Add(aSrc, xList);
            }
            xList.Add(xLogItem);
        }
    }
}