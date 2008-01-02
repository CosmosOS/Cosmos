using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs
{
    [Plug(Target=typeof(System.String))]
    public class String
    {
        /*public int IndexOf(char c)
        {
            // TODO: We can't get 'this'
            //string me = ToString();
            //for (int i = 0; i < me.Length; i++)
            //{
            //    if (me[i] == c)
            //        return i;
            //}
            return -1;
        }*/
    }
}
