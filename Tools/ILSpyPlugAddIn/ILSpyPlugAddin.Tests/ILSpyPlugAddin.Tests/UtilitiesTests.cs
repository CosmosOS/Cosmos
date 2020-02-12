using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos.ILSpyPlugs.Plugin;
using ICSharpCode.ILSpy;
using Mono.Cecil;
using NUnit.Framework;

namespace ILSpyPlugAddin.Tests
{
    public class UtilitiesTests
    {
        [Test]
        public void ReturnsExpectedFieldAccessString()
        {
            var moduleDefinition = CreateTestModuleDefinition();
            var typeDefinition = CreateTestTypeDefinition();
            var fieldDefinition = CreateTestFieldDefinition();
            typeDefinition.Fields.Add(fieldDefinition);
            moduleDefinition.Types.Add(typeDefinition);

            string expected = "[FieldAccess(Name = \"TestNamespace.TestFieldType TestNamespace.TestType.TestField\")] ref [TestAssembly]TestNamespace.TestFieldType fieldTestField";
            string actual = Utilities.GenerateFieldAccessPlugEntry(fieldDefinition);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnsExpectedTypePlugEntry()
        {
            var typeDefinition = new TypeDefinition("TestNamespace", "TestType", TypeAttributes.Public);

            var moduleDefinition = ModuleDefinition.CreateModule("TestModule", ModuleKind.Dll);
            moduleDefinition.Types.Add(typeDefinition);

            string expected = "[Plug(Target = typeof(global::TestNamespace.TestType))]\r\npublic static class TestTypeImpl\r\n{\r\n}\r\n";
            string actual = Utilities.GenerateTypePlugEntry(typeDefinition);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnsExpectedMethodPlugEntry()
        {
            var moduleDefinition = CreateTestModuleDefinition();
            var typeDefinition = CreateTestTypeDefinition();
            var methodDefinition = CreateTestMethodDefinition();
            typeDefinition.Methods.Add(methodDefinition);
            moduleDefinition.Types.Add(typeDefinition);

            string expected = "public static [TestAssembly]TestNamespace.TestFieldType TestMethod(TestNamespace.TestType aThis)\r\n{\r\n}\r\n";
            string actual = Utilities.GenerateMethodPlugEntry(methodDefinition);
            Assert.AreEqual(expected, actual);
        }

        private static ModuleDefinition CreateTestModuleDefinition()
        {
            var definition = ModuleDefinition.CreateModule("TestModule", ModuleKind.Dll);
            return definition;
        }

        private static TypeDefinition CreateTestTypeDefinition()
        {
            var definition = new TypeDefinition("TestNamespace", "TestType", TypeAttributes.Public);
            return definition;
        }

        private static TypeReference CreateTestTypeReference()
        {
            var module = CreateTestModuleDefinition();
            var reference = new TypeReference("TestNamespace", "TestFieldType", module, new AssemblyNameDefinition("TestAssembly", new Version()));
            return reference;
        }

        private static FieldDefinition CreateTestFieldDefinition()
        {
            var typeReference = CreateTestTypeReference();
            var definition = new FieldDefinition("TestField", FieldAttributes.Public, typeReference);
            return definition;
        }

        public static MethodDefinition CreateTestMethodDefinition()
        {
            var returnType = CreateTestTypeReference();
            var definition = new MethodDefinition("TestMethod", MethodAttributes.Public, returnType);
            return definition;
        }
    }
}
