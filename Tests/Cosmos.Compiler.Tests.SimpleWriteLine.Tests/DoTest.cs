using System;
using Cosmos.Compiler.TestsBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmos.Compiler.Tests.SimpleWriteLine.Tests
{
  [TestClass]
  public class DoTest
  {
    [TestMethod]
    public void Test()
    {
        var xRunner = new CompilerRunner();
        xRunner.References.Add(typeof(Kernel.Kernel).Assembly.Location);
        xRunner.References.Add(typeof(Cosmos.System.Plugs.System.TypeImpl).Assembly.Location);
    }
  }
}
