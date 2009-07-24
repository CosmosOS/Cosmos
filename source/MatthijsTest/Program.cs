using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Cosmos.IL2CPU;
using System.Reflection;
using System.Diagnostics;
using Cosmos.IL2CPU.X86;


namespace MatthijsTest
{
	public class Program
	{
		//private static Stopwatch mSW = new Stopwatch();
		//private static void Measure(string aName, Action aAction)
		//{
		//  mSW.Start();
		//  aAction();
		//  mSW.Stop();
		//  Console.WriteLine("{0} took: {1}", aName, mSW.Elapsed);
		//}
		public static void Main()
		{
			//  var xAssemblerBaseOp = typeof(ILOpX86);
			//  var xScanner = new ILScanner(xAssemblerBaseOp);
			//  var xMethod = typeof(Program).GetMethod("CompileTest", BindingFlags.Public | BindingFlags.Static);
			//  Measure("Compiling", () => xScanner.Execute(xMethod));
			GenCode();
		}

		//public static void CompileTest()
		//{
		//}


		public static void GenCode()
		{
			throw new Exception("Watch out. enable this exception again after use.");
			foreach (var xFile in Directory.GetFiles(@"E:\Cosmos\Temp", "*.cs"))
			{
				using (var xWriter = new StreamWriter(@"E:\Cosmos\source2\IL2PCU\Cosmos.IL2CPU.X86\IL\" + Path.GetFileName(xFile)))
				{
					xWriter.WriteLine("using System;");
					xWriter.WriteLine();
					xWriter.WriteLine("namespace Cosmos.IL2CPU.X86.IL");
					xWriter.WriteLine("{");
					xWriter.WriteLine("\t[Cosmos.IL2CPU.OpCode(ILOpCode.Code.{0})]", Path.GetFileNameWithoutExtension(xFile));
					xWriter.WriteLine("\tpublic class {0}: ILOpX86", Path.GetFileNameWithoutExtension(xFile));
					xWriter.WriteLine("\t{");
					xWriter.WriteLine("\t\tpublic {0}(ILOpCode aOpCode):base(aOpCode)", Path.GetFileNameWithoutExtension(xFile));
					xWriter.WriteLine("\t\t{");
					xWriter.WriteLine("\t\t}");
					xWriter.WriteLine();
					xWriter.WriteLine("\t\t#region Old code");
					foreach (var xLine in File.ReadAllLines(xFile))
					{
						xWriter.WriteLine("\t\t// {0}", xLine);
					}
					xWriter.WriteLine("\t\t#endregion Old code");
					xWriter.WriteLine("\t}");
					xWriter.WriteLine("}");

//            using System;

//namespace Cosmos.IL2CPU.X86.IL
//{
//  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Add)]
//  public class Add: ILOpX86
//  {
				}
			}
		}

	}
}