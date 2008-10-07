using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Builder;
using System.Collections;

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
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestCtor'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestCtor();
			} catch (Exception E) {
				Console.Write("    ");
				System.Diagnostics.Debugger.Break();
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestCapacity'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestCapacity();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestCount'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestCount();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestIsFixed'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestIsFixed();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestIsReadOnly'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestIsReadOnly();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestIsSynchronized'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestIsSynchronized();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestItem'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestItem();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestAdapter'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestAdapter();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestAdd'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestAdd();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestAddRange'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestAddRange();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestBinarySearch'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestBinarySearch();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.BinarySearch_IndexOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.BinarySearch_IndexOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.BinarySearch_CountOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.BinarySearch_CountOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.BinarySearch_Null'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.BinarySearch_Null();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestClear'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestClear();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestClone'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestClone();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestContains'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestContains();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestCopyTo'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestCopyTo();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.CopyTo_IndexOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.CopyTo_IndexOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.CopyTo_ArrayIndexOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.CopyTo_ArrayIndexOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.CopyTo_CountOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.CopyTo_CountOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestFixedSize'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestFixedSize();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestGetRange'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestGetRange();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.GetRange_IndexOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.GetRange_IndexOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.GetRange_CountOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.GetRange_CountOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestIndexOf'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestIndexOf();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.IndexOf_StartIndexOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.IndexOf_StartIndexOverflow();
			} catch (System.ArgumentOutOfRangeException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.IndexOf_CountOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.IndexOf_CountOverflow();
			} catch (System.ArgumentOutOfRangeException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestInsert'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestInsert();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestInsertRange'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestInsertRange();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestLastIndexOf'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestLastIndexOf();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.LastIndexOf_StartIndexOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.LastIndexOf_StartIndexOverflow();
			} catch (System.ArgumentOutOfRangeException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.LastIndexOf_CountOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.LastIndexOf_CountOverflow();
			} catch (System.ArgumentOutOfRangeException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestReadOnly'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestReadOnly();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestRemove'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestRemove();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestRemoveAt'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestRemoveAt();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestRemoveRange'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestRemoveRange();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.RemoveRange_IndexOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.RemoveRange_IndexOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.RemoveRange_CountOverflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.RemoveRange_CountOverflow();
			} catch (System.ArgumentException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestRepeat'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestRepeat();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestSetRange'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestSetRange();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.SetRange_Overflow'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.SetRange_Overflow();
			} catch (System.ArgumentOutOfRangeException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TestInsertRange_this'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TestInsertRange_this();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TrimToSize_ReadOnly'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TrimToSize_ReadOnly();
			} catch (System.NotSupportedException) {
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.TrimToSize'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.TrimToSize();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.BinarySearch1_EmptyList'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.BinarySearch1_EmptyList();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.BinarySearch2_EmptyList'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.BinarySearch2_EmptyList();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.BinarySearch3_EmptyList'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.BinarySearch3_EmptyList();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
				xError = true;
			}
			if (!xError) {
				Console.Write("    ");
				Console.WriteLine("Success");
			}
			Console.WriteLine("Running test 'MonoTests.System.Collections.ArrayListTest.AddRange_GetRange'");
			try {
				xError = false;
				MonoTests.System.Collections.ArrayListTest.AddRange_GetRange();
			} catch (Exception E) {
				Console.Write("    ");
				Console.WriteLine(E.Message);
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
