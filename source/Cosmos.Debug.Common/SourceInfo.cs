using System.Collections.Generic;

namespace Cosmos.Debug.Common
{
    public class SourceInfos : SortedList<uint, SourceInfo>
    {
    }

    public class SourceInfo
    {
        public string MethodName { get; set; }

        public string SourceFile { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public int LineEnd { get; set; }

        public int ColumnEnd { get; set; }

        public static int GetIndexClosestSmallerMatch(IList<int> aList, int aValue)
        {
            int xIdx = -1;
            for (int i = 0; i < aList.Count; i++)
            {
                if (aList[i] <= aValue)
                {
                    xIdx = i;
                }
                else
                {
                    break;
                }
            }
            return xIdx;
        }
    }
}