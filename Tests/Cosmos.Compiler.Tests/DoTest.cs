namespace Cosmos.Compiler.Tests
{
    //[TestClass]
    //public class DoTest : BaseTest
    //{
    //    [TestMethod]
    //    [TestCategory("Compiler")]
    //    public void SimpleWriteLineTest()
    //    {
    //        // these files contain the output to be verified. don't clean them.
    //        var xOutputFile = Path.GetTempFileName();
    //        var xLogFile = Path.GetTempFileName();

    //        var xRunner = new CompilerRunner();

    //        xRunner.References.Add(typeof(Cosmos.Compiler.Tests.SimpleWriteLine.Kernel.Kernel).Assembly.Location);
    //        xRunner.References.Add(typeof(Cosmos.System.Plugs.System.TypeImpl).Assembly.Location);
    //        xRunner.References.Add(typeof(Cosmos.Core.Plugs.CPUImpl).Assembly.Location);
    //        xRunner.References.Add(typeof(Cosmos.Debug.Kernel.Plugs.DebugBreak).Assembly.Location);

    //        xRunner.AssemblerLogFile = xLogFile;
    //        xRunner.OutputFile = xOutputFile;
    //        xRunner.Execute();

    //        Verify("Output", File.ReadAllText(xOutputFile));
    //        Verify("Logfile", File.ReadAllText(xLogFile));
    //    }

    //    [TestMethod]
    //    [TestCategory("Compiler")]
    //    public void InterfacesTest()
    //    {
    //        // these files contain the output to be verified. don't clean them.
    //        var xOutputFile = Path.GetTempFileName();
    //        var xLogFile = Path.GetTempFileName();

    //        var xRunner = new CompilerRunner();

    //        xRunner.References.Add(typeof(Cosmos.Compiler.Tests.Interfaces.Kernel.Kernel).Assembly.Location);
    //        xRunner.References.Add(typeof(Cosmos.System.Plugs.System.TypeImpl).Assembly.Location);
    //        xRunner.References.Add(typeof(Cosmos.Core.Plugs.CPUImpl).Assembly.Location);
    //        xRunner.References.Add(typeof(Cosmos.Debug.Kernel.Plugs.DebugBreak).Assembly.Location);

    //        xRunner.AssemblerLogFile = xLogFile;
    //        xRunner.OutputFile = xOutputFile;
    //        xRunner.Execute();

    //        Verify("Output", File.ReadAllText(xOutputFile));
    //        Verify("Logfile", File.ReadAllText(xLogFile));
    //    }
    //}
}
