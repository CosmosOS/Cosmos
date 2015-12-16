using Cosmos.IL2CPU.Plugs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plug = Cosmos.IL2CPU.Plugs.PlugAttribute;

namespace Cosmos.System.Plugs.System
{
    [@Plug(Target = typeof(string))]
    public static class StringImpl
    {
        public static bool StartsWith(this string str, string value)
        {
            Char[] di = str.ToCharArray();
            Char[] ci = value.ToCharArray();
            if (value.Length > str.Length)
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

        public static bool Contains(this string str, string comp)
        {
            Char[] di = str.ToCharArray();
            Char[] ci = comp.ToCharArray();
            if (comp.Length == str.Length)
            {
                if (comp == str)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (!(comp.Length > str.Length) && (comp.Length != str.Length))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (di[i] == ci[0])
                    {
                        for (int j = 1; j < comp.Length; j++)
                        {
                            if (di[i + j] != ci[j])
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool EndsWith(this string str, string comp)
        {
            Char[] di = str.ToCharArray();
            Char[] ci = comp.ToCharArray();
            if(str.Length == comp.Length)
            {
                if(str == comp)
                {
                    return true;
                }
                return false;
            }
            else if(comp.Length > str.Length)
            {
                return false;
            }
            else
            {
                for (int i = str.Length - comp.Length; i < str.Length; i++)
                {
                    if (di[str.Length - comp.Length + i] != ci[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
