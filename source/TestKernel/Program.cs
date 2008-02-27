using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.Windows;

namespace TestKernel {
	class Program {
		private class TestInfo {
			public Type ExpectedException;
			public string Name;
			public string TestGroup;
			public bool IsExperimental;
		}
		[STAThread]
		public static void Main(string[] aArgs) {
			if (aArgs.Length == 0) {
				var xBuilder = new Builder();
				xBuilder.Build();
			} else {
				List<KeyValuePair<string, TestInfo>> xTests = new List<KeyValuePair<string, TestInfo>>();
				#region detect all current tests
				foreach (var xType in typeof(Program).Assembly.GetTypes()) {
					string xBaseName = "";
					var xFixture = xType.GetCustomAttributes(typeof(TestFixtureAttribute), true).FirstOrDefault() as TestFixtureAttribute;
					if (xFixture != null) {
						xBaseName = xFixture.BaseName;
					}
					if (String.IsNullOrEmpty(xBaseName)) {
						xBaseName = xType.FullName;
					}
					foreach (var xMethod in xType.GetMethods()) {
						var xAttrib = xMethod.GetCustomAttributes(typeof(TestAttribute), true).FirstOrDefault() as TestAttribute;
						if (xAttrib != null) {
							string xName = xAttrib.Name;
							if (String.IsNullOrEmpty(xName)) {
								xName = xBaseName + "." + xMethod.Name;
							}
							Type xExpectedException = null;
							var xExpException = xMethod.GetCustomAttributes(typeof(ExpectedExceptionAttribute), true).FirstOrDefault() as ExpectedExceptionAttribute;
							if (xExpException != null) {
								xExpectedException = xExpException.Exception;
							}
							xTests.Add(new KeyValuePair<string, TestInfo>(xType.FullName.Replace('+', '.') + "." + xMethod.Name, new TestInfo() {
								ExpectedException = xExpectedException,
								Name = xName,
								IsExperimental = xAttrib.IsExperimental,
								TestGroup = xAttrib.TestGroup
							}));
						}
					}
				}
				#endregion
				Console.WriteLine("public static void Init() {");
				Console.WriteLine("string xTestResult=null;");
				Console.WriteLine("bool xError;");
				foreach (var xTest in (from item in xTests
									   where item.Value.TestGroup == aArgs[0] && ((!item.Value.IsExperimental) || ((aArgs.Length > 1) && (aArgs[1] == "-experimental")))
									   select item)) {
					Console.WriteLine("Console.WriteLine(\"Running test '{0}'\");", xTest.Value.Name);
					Console.WriteLine("try{");
					Console.WriteLine("xError = false;");
					Console.WriteLine("{0}();", xTest.Key);
					if (xTest.Value.ExpectedException != null) {
						Console.WriteLine("}}catch({0}){{", xTest.Value.ExpectedException.FullName.Replace("+", "."));
					}
					if (xTest.Value.ExpectedException != typeof(Exception)) {
						Console.WriteLine("}catch(Exception E){");
						Console.WriteLine("Console.Write(\"    \");");
						Console.WriteLine("Console.WriteLine(E.Message);");
						Console.WriteLine("xError = true;");
					}
					Console.WriteLine("}");
					Console.WriteLine("if(!xError){");
					Console.WriteLine("Console.Write(\"    \");");
					Console.WriteLine("Console.WriteLine(\"Success\");");
					Console.WriteLine("}");
				}

				Console.WriteLine("Console.WriteLine(\"All tests executed!\");");
				Console.WriteLine("while (true)");
				Console.WriteLine(";");
				Console.WriteLine("}");
			}
		}

		public static void Init() {
			string xTestResult = null;
			bool xError;
			Console.WriteLine("Running test 'Whoohoo'");
			try {
				xError = false;
				TestKernel.Tests.NoInit.StringTests.OurTest();
			} catch (Exception E) {
				Console.Write("    ");
				if (E == null) {
					Console.WriteLine("<<NO EXCEPTION>>");
				} else {
					Console.WriteLine(E.Message);
				}
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("All tests executed!");
			while (true)
				;
		}
	}
}
