using System.IO;
using System.Linq;
using System.Reflection;
using Cosmos.ILSpyPlugs.Plugin;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.TypeSystem;
using NUnit.Framework;

namespace ILSpyPlugAddin.Tests
{
    public class UtilitiesTests
    {
        private readonly ITypeDefinition type;
        private readonly IMethod method;
        private readonly IField field;
        private readonly IProperty property;

        public UtilitiesTests()
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(currentDirectory, "TestAssembly.dll");
            var decompiler = new ICSharpCode.Decompiler.CSharp.CSharpDecompiler(assemblyPath, new DecompilerSettings());

            var typeName = new FullTypeName("TestAssembly.TestClass");
            type = decompiler.TypeSystem.MainModule.Compilation.FindType(typeName).GetDefinition();
            method = type.Methods.First();
            field = type.Fields.First();
            property = type.Properties.First();
        }

        [Test]
        public void ReturnsExpectedTypePlugEntry()
        {
            string expected = "[Plug(Target = typeof(global::TestAssembly.TestClass))]\r\npublic static class TestClassImpl\r\n{\r\n}\r\n";
            string actual = Utilities.GenerateTypePlugEntry(type);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnsExpectedMethodPlugEntry()
        {
            string expected = "public static string TestMethod(ref TestAssembly.TestClass aThis, string aTestParameter)\r\n{\r\n}\r\n";
            string actual = Utilities.GenerateMethodPlugEntry(method);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnsExpectedFieldAccessString()
        {
            string expected = "[FieldAccess(Name = \"System.String TestAssembly.TestClass._Field\")] ref string field_Field";
            string actual = Utilities.GenerateFieldAccessPlugEntry(field);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnsExpectedPropertyPlugEntry()
        {
            string expected = "public static string get_Property(ref TestAssembly.TestClass aThis)\r\n{\r\n}\r\n\r\n\r\npublic static void set_Property(ref TestAssembly.TestClass aThis, string value)\r\n{\r\n}\r\n\r\n\r\n";
            string actual = Utilities.GeneratePropertyPlugEntry(property);
            Assert.AreEqual(expected, actual);
        }
    }
}
