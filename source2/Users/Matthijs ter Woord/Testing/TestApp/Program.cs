using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EcmaCil;
using System.Xml;
using System.Diagnostics;

namespace TestApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var xReader = new MonoCecilToEcmaCil1.ReaderWithPlugSupport())
            {
                //                var xSW = new Stopwatch();
                //                for (int i = 0; i < 16; i++)
                //                {
                //                    xSW.Start();
                //                    var xResults = xReader.Execute(typeof(SimpleMethodsTest.Program).Assembly.Location);
                //                    xSW.Stop();
                //                    Console.WriteLine("{0} {1}", i.ToString("X"), xSW.Elapsed);
                //                    xSW.Reset();
                //                    xReader.Clear();
                //                }
                //Console.WriteLine("Number of types: {0}", xResults.Length);
                var xResults = xReader.Execute(typeof(SimpleMethodsTest.Program).Assembly.Location);
                //using (var xOut = XmlWriter.Create(@"e:\types1.xml", new XmlWriterSettings { Encoding = Encoding.Unicode }))
                //{
                //    Dump.DumpTypes(xResults, xOut);
                //}
                xReader.Clear();
                xResults = xReader.Execute(typeof(SimpleClassTest.Program).Assembly.Location);
                //using (var xOut = XmlWriter.Create(@"e:\types2.xml"))
                //{
                //    Dump.DumpTypes(xResults, xOut);
                //}
                xReader.Clear();
                //xReader.AddPlugAssembly(typeof(ObjectImpl).Assembly.Location);
                //xResults = xReader.Execute(typeof(SimpleClass2Test.Program).Assembly.Location);
                //using (var xOut = XmlWriter.Create(@"e:\types3.xml"))
                //{
                //    Dump.DumpTypes(xResults, xOut);
                //}
            }
        }
    }
}
