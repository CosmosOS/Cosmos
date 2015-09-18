using System;

namespace Cosmos.System
{
    public class sf
    {
        public static Boolean dStartsWith(string str, string comp)
        {
            Char[] di = str.ToCharArray();
            Char[] ci = comp.ToCharArray();
            if (comp.Length > str.Length)
            {
                return false;
            }
            for (int i = 0; i < ci.Length; i++)
            {
                if (di[i] != ci[i])
                {
                    return false;

                }
            }
            return true;
        }


        public static Boolean dContains(string str, string comp)
        {
            Char[] di = str.ToCharArray();
            Char[] ci = comp.ToCharArray();
            for (int i=0;i<=str.Length;i++)
            {
                if (di[i] == ci[0])
                {
                    for (int j = 1; j <= comp.Length; j++)
                    {
                        if(di[i+j] != ci[j])
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public static Boolean dEndsWith(string str, string comp)
        {
            Char[] di = str.ToCharArray();
            Char[] ci = comp.ToCharArray();
           
            int sl = di.Length - ci.Length;
            if (comp.Length > str.Length)
            {
                return false;
            }
            for (int i = sl;i <= di.Length; i++)
            {
                if (di[i] != ci[i - sl])
                {
                    return false;
                }
                
                
            }
            return true;
        }
    }
}
