using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ReflectionToEcmaCil
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

        public void EnableLogging(string aPathname)
        {
            mLogMap = new Dictionary<object, List<LogItem>>();
            mMapPathname = aPathname;
            mLogEnabled = true;
        }

        public void WriteScanMap()
        {
            if (mLogEnabled)
            {
                // Create bookmarks, but also a dictionary that
                // we can find the items in
                var xBookmarks = new Dictionary<object, int>();
                int xBookmark = 0;
                foreach (var xList in mLogMap)
                {
                    foreach (var xItem in xList.Value)
                    {
                        if (!xBookmarks.ContainsKey(xItem.Item))
                        {
                            xBookmarks.Add(xItem.Item, xBookmark);
                            xBookmark++;
                        }
                    }
                }
                using (mLogWriter = new StreamWriter(mMapPathname, false))
                {
                    mLogWriter.WriteLine("<html><body>");
                    foreach (var xList in mLogMap)
                    {
                        mLogWriter.WriteLine("<hr>");

                        // Emit bookmarks above source, so when clicking links user doesn't need
                        // to constantly scroll up.
                        foreach (var xItem in xList.Value)
                        {
                            mLogWriter.WriteLine("<a name=\"Item" + xBookmarks[xItem.Item].ToString() + "\"></a>");
                        }

                        int xHref;
                        if (!xBookmarks.TryGetValue(xList.Key, out xHref))
                        {
                            xHref = -1;
                        }
                        mLogWriter.Write("<p>");
                        if (xHref >= 0)
                        {
                            mLogWriter.WriteLine("<a href=\"#Item" + xHref.ToString() + "\">");
                        }
                        if (xList.Key == null)
                        {
                            mLogWriter.WriteLine("Unspecified Source");
                        }
                        else
                        {
                            mLogWriter.WriteLine(LogItemText(xList.Key));
                        }
                        if (xHref >= 0)
                        {
                            mLogWriter.Write("</a>");
                        }
                        mLogWriter.WriteLine("</a></p>");

                        mLogWriter.WriteLine("<ul>");
                        foreach (var xItem in xList.Value)
                        {
                            mLogWriter.Write("<li>" + LogItemText(xItem.Item) + "</li>");

                            mLogWriter.WriteLine("<ul>");
                            mLogWriter.WriteLine("<li>" + xItem.SrcType + "</<li>");
                            mLogWriter.WriteLine("</ul>");
                        }
                        mLogWriter.WriteLine("</ul>");
                    }
                    mLogWriter.WriteLine("</body></html>");
                }
            }
        }

        protected string LogItemText(object aItem)
        {
            if (aItem is MethodBase)
            {
                var x = (MethodBase)aItem;
                return "Method: " + x.DeclaringType + "." + x.Name + "<br>" + x.ToString();
            }
            else if (aItem is Type)
            {
                var x = (Type)aItem;
                return "Type: " + x.FullName;
            }
            else
            {
                return "Other: " + aItem.ToString();
            }
        }

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