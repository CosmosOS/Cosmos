using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    public class ExceptionTest
    {
        public static void RunTest()
        {
            //ThrowStaticExceptionReturnVoid(); //OK
            //ThrowStaticExceptionReturnBool(); //OK
            //ThrowStaticExceptionReturnList(); //OK

            var e = new ExceptionTest();
            //e.ThrowInstanceExceptionReturnList(); //FAIL
            e.ThrowInstanceExceptionReturnBool(); //FAIL
            //e.ThrowInstanceExceptionReturnVoid(); //OK
        }

        public static void ThrowStaticExceptionReturnVoid()
        {
            throw new Exception("Called from static method which returns VOID");
        }

        public static bool ThrowStaticExceptionReturnBool()
        {
            throw new Exception("Called from static method which returns BOOL");
        }

        public static List<string> ThrowStaticExceptionReturnList()
        {
            throw new Exception("Called from static method which returns LIST<STRING>");
        }

        public void ThrowInstanceExceptionReturnVoid()
        {
            throw new Exception("Called from instance method which returns VOID");
        }

        public List<string> ThrowInstanceExceptionReturnList()
        {
            throw new Exception("Called from instance method which returns LIST<STRING>");
        }

        public bool ThrowInstanceExceptionReturnBool()
        {
            throw new Exception("Called from instance method which returns BOOL");
        }
    }
}
