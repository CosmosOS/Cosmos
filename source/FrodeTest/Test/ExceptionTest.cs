using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test {
    public class ExceptionTest {

        public static int CauseException()
        {
            throw new Exception("No ID found");
        }

        public static void RunTest() {

            var strings = new string[2];
            strings[0] = "Hello";
            strings[1] = "World";

            var result = strings[CauseException()]; //Kills Qemu

            //try {
            //    ThrowStaticExceptionReturnVoid(); //OK}
            //} catch {
            //    Console.WriteLine("1");
            //    Cosmos.Hardware.DebugUtil.SendMessage("ExceptionTest", "1 done");
            //}
            //try
            //{
            //    ThrowStaticExceptionReturnBool(); //OK
            //}
            //catch
            //{
            //    Console.WriteLine("2");
            //    Cosmos.Hardware.DebugUtil.SendMessage("ExceptionTest", "2 done");
            //}
            //try
            //{
            //    ThrowStaticExceptionReturnList(); //OK
            //}
            //catch
            //{
            //    Console.WriteLine("3");
            //    Cosmos.Hardware.DebugUtil.SendMessage("ExceptionTest", "3 done");
            //}
            //ExceptionTest e = new ExceptionTest();
            //try
            //{
            //    e.ThrowInstanceExceptionReturnList(); //FAIL
            //}
            //catch
            //{
            //    Console.WriteLine("5");
            //    Cosmos.Hardware.DebugUtil.SendMessage("ExceptionTest", "5 done");
            //}
            //try
            //{
            //    e.ThrowInstanceExceptionReturnBool(); //FAIL
            //}
            //catch
            //{
            //    Console.WriteLine("6");
            //    Cosmos.Hardware.DebugUtil.SendMessage("ExceptionTest", "6 done");
            //}
            //try
            //{
            //    e.ThrowInstanceExceptionReturnVoid(); //OK
            //}
            //catch
            //{
            //    Console.WriteLine("7");
            //    Cosmos.Hardware.DebugUtil.SendMessage("ExceptionTest", "7 done");
            //}
        }

        public static void ThrowStaticExceptionReturnVoid() {
            throw new Exception("Called from static method which returns VOID");
        }

        public static bool ThrowStaticExceptionReturnBool() {
            throw new Exception("Called from static method which returns BOOL");
        }

        public static List<string> ThrowStaticExceptionReturnList() {
            throw new Exception("Called from static method which returns LIST<STRING>");
        }

        public void ThrowInstanceExceptionReturnVoid() {
            throw new Exception("Called from instance method which returns VOID");
        }

        public List<string> ThrowInstanceExceptionReturnList() {
            throw new Exception("Called from instance method which returns LIST<STRING>");
        }

        public bool ThrowInstanceExceptionReturnBool() {
            throw new Exception("Called from instance method which returns BOOL");
        }
    }
}