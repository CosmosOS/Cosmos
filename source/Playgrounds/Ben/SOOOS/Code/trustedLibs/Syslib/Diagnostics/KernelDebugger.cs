using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syslib.Diagnostics
{
    //Hook into debugger that way not dependent on debugger type

    ///REM

    /// <summary>
    /// 
    /// </summary>
    public class KernelDebugger
    {


        public static extern void Break();
        //public static void Break()
        //{
        //    //TODO hookup
        //    //DebugService.Break();
        //}

        public static void NotImplemented()
        {
            failAssert("Not implemented.");
        }

        public static void NotImplemented(String msg)
        {
            failAssert(/*"Not implemented: "+*/msg);
        }


        public static void UnReachableReached()
        {
            failAssert("Unreachable code reached.");
        }


        public static void NotReached(String msg)
        {
            failAssert(/*"Unreachable code reached: "+*/msg);
        }



        public static void Deny(bool expr)
        {
            if (expr)
            {
                failAssert(null);
            }
        }

        public static void Assert(bool expr, String s)
        {
            if (!expr)
            {
                failAssert(s);
            }
        }

        private static void failAssert(String s)
        {
            if (s != null)
            {
                Print("Assertion failed: {0}", s);
            }
            else
            {
                Print("Assertion failed.");
            }
            Break();
        }

        public static void Print(string val , params string[] args )
        {
            if (val != null)
            {
                WriteLine(val, args);
            }

        }


        //public static void Print(String format, __arglist)
        //{
        //    Print(format, new ArgIterator(__arglist));
        //}

        //public static void Print(String format, ArgIterator args)
        //{
        //    WriteLine(format, args);
        //}

        public  extern static void WriteLine(String format, params string[] args);

  
    }


}
