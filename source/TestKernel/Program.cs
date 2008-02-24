using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.Windows;

namespace TestKernel {
	class Program {
		[STAThread]
		public static void Main(string[] aArgs) {
			if (aArgs.Length == 0) {
				var xBuilder = new Builder();
				xBuilder.Build();
			} else {
				List<KeyValuePair<string, TestAttribute>> xTests = new List<KeyValuePair<string, TestAttribute>>();
				#region detect all current tests
				foreach (var xType in typeof(Program).Assembly.GetTypes()) {
					foreach (var xMethod in xType.GetMethods()) {
						var xAttrib = xMethod.GetCustomAttributes(typeof(TestAttribute), true).FirstOrDefault() as TestAttribute;
						if (xAttrib != null) {
							xTests.Add(new KeyValuePair<string, TestAttribute>(xType.FullName.Replace('+', '.') + "." + xMethod.Name, xAttrib));
						}
					}
				}
				#endregion
				Console.WriteLine("public static void Init() {");
				Console.WriteLine("string xTestResult=null;");
				foreach (var xTest in (from item in xTests
										   where item.Value.TestGroup == aArgs[0] && ((!item.Value.IsExperimental) || ((aArgs.Length > 1) && (aArgs[1] == "-experimental")))
										   select item)) {
					Console.WriteLine("Console.WriteLine(\"Running test '{0}'\");", xTest.Value.Description);
					Console.WriteLine("xTestResult = {0}();", xTest.Key);
					Console.WriteLine("Console.Write(\"    \");");
					Console.WriteLine("if (xTestResult == null) {");
					Console.WriteLine("Console.WriteLine(\"Success\");");
					Console.WriteLine("} else {");
					Console.WriteLine("Console.Write(\"Error: \");");
					Console.WriteLine("Console.WriteLine(xTestResult);}");
				}

				Console.WriteLine("Console.WriteLine(\"All tests executed!\");");
				Console.WriteLine("while (true)");
				Console.WriteLine(";");
				Console.WriteLine("}");
			}
		}

		public static void Init() {
			string xTestResult = null;
			Console.WriteLine("Running test 'System.String_EmbeddedStringContents'");
			xTestResult = TestKernel.Tests.NoInit.StringTests.TestEmbedded();
			Console.Write("    ");
			if (xTestResult == null) {
				Console.WriteLine("Success");
			} else {
				Console.Write("Error: ");
				Console.WriteLine(xTestResult);
			}
			Console.WriteLine("All tests executed!");
			while (true)
				;
		}
	}
}
