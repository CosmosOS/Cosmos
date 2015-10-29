using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EcmaCil;
using System.Xml;
using System.Diagnostics;
using System.IO;

namespace TestApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var xReader = new ReflectionToEcmaCil.Reader())
            {
                try
                {
                    var xTestClass= typeof(SimpleClassTest.Program);
                    xReader.EnableLogging(Path.ChangeExtension(xTestClass.Assembly.Location, "html"));
                    var xResult = xReader.Execute(xTestClass.Assembly.Location);
                    using (var xXmlOut = XmlWriter.Create(Path.ChangeExtension(xTestClass.Assembly.Location, "out.xml"), new XmlWriterSettings { Indent = true, IndentChars="  " }))
                    {
                        Dump.DumpTypes(xResult, xXmlOut);
                        xXmlOut.Flush();
                    }                                
                }
                catch (Exception E)
                {
                    Console.WriteLine("Error: " + E.ToString());
                    xReader.WriteScanMap();
                }
            }
            //using (var xReader = new MonoCecilToEcmaCil1.ReaderWithPlugSupport())
            //{
            //    var xSW = new Stopwatch();
            //    xSW.Start();
            //    //                for (int i = 0; i < 16; i++)
            //    //                {
            //    //                    xSW.Start();
            //    //                    var xResults = xReader.Execute(typeof(SimpleMethodsTest.Program).Assembly.Location);
            //    //                    xSW.Stop();
            //    //                    Console.WriteLine("{0} {1}", i.ToString("X"), xSW.Elapsed);
            //    //                    xSW.Reset();
            //    //                    xReader.Clear();
            //    //                }
            //    //Console.WriteLine("Number of types: {0}", xResults.Length);
            //    var xResults = xReader.Execute(typeof(SimpleMethodsTest.Program).Assembly.Location);
            //    //using (var xOut = XmlWriter.Create(@"e:\types1.xml", new XmlWriterSettings { Encoding = Encoding.Unicode }))
            //    //{
            //    //    Dump.DumpTypes(xResults, xOut);
            //    //}
            //    xReader.Clear();
            //    xResults = xReader.Execute(typeof(SimpleClassTest.Program).Assembly.Location);
            //    //using (var xOut = XmlWriter.Create(@"e:\types2.xml"))
            //    //{
            //    //    Dump.DumpTypes(xResults, xOut);
            //    //}
            //    xReader.Clear();
            //    //xReader.AddPlugAssembly(typeof(ObjectImpl).Assembly.Location);
            //    //xResults = xReader.Execute(typeof(SimpleClass2Test.Program).Assembly.Location);
            //    //using (var xOut = XmlWriter.Create(@"e:\types3.xml"))
            //    //{
            //    //    Dump.DumpTypes(xResults, xOut);
            //    //}
            //    xSW.Stop();
            //    Console.WriteLine("Took: " + xSW.Elapsed);
            //}
        }
    }
}
