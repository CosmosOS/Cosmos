using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Cosmos.Debug.Kernel")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Cosmos.Debug.Kernel")]
[assembly: AssemblyCopyright("Copyright ©  2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: InternalsVisibleTo("Cosmos.Core, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3ef5198fa2f8926f006b5d2053eb3b3c875e74695675a6b97bd27ba6b0c5cbee26710c04277f7975927ace4a037692eddb71340a4c3f11e06c645c6a4cebad303301228943b39378bf3222f9432ff9c72c31d1a5e936db6cf9f18c23bd52a43c091fc803ce2139cd390a9678553d1e6061656c3d0196ddbd2233143fc433195")]
[assembly: InternalsVisibleTo("Cosmos.Core.Memory, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3ef5198fa2f8926f006b5d2053eb3b3c875e74695675a6b97bd27ba6b0c5cbee26710c04277f7975927ace4a037692eddb71340a4c3f11e06c645c6a4cebad303301228943b39378bf3222f9432ff9c72c31d1a5e936db6cf9f18c23bd52a43c091fc803ce2139cd390a9678553d1e6061656c3d0196ddbd2233143fc433195")]
[assembly: InternalsVisibleTo("Cosmos.HAL, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3ef5198fa2f8926f006b5d2053eb3b3c875e74695675a6b97bd27ba6b0c5cbee26710c04277f7975927ace4a037692eddb71340a4c3f11e06c645c6a4cebad303301228943b39378bf3222f9432ff9c72c31d1a5e936db6cf9f18c23bd52a43c091fc803ce2139cd390a9678553d1e6061656c3d0196ddbd2233143fc433195")]
[assembly: InternalsVisibleTo("Cosmos.System, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3ef5198fa2f8926f006b5d2053eb3b3c875e74695675a6b97bd27ba6b0c5cbee26710c04277f7975927ace4a037692eddb71340a4c3f11e06c645c6a4cebad303301228943b39378bf3222f9432ff9c72c31d1a5e936db6cf9f18c23bd52a43c091fc803ce2139cd390a9678553d1e6061656c3d0196ddbd2233143fc433195")]
[assembly: InternalsVisibleTo("Cosmos.IL2CPU, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3ef5198fa2f8926f006b5d2053eb3b3c875e74695675a6b97bd27ba6b0c5cbee26710c04277f7975927ace4a037692eddb71340a4c3f11e06c645c6a4cebad303301228943b39378bf3222f9432ff9c72c31d1a5e936db6cf9f18c23bd52a43c091fc803ce2139cd390a9678553d1e6061656c3d0196ddbd2233143fc433195")]
[assembly: InternalsVisibleTo("Cosmos.Compiler.Tests.SimpleWriteLine.Kernel, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3ef5198fa2f8926f006b5d2053eb3b3c875e74695675a6b97bd27ba6b0c5cbee26710c04277f7975927ace4a037692eddb71340a4c3f11e06c645c6a4cebad303301228943b39378bf3222f9432ff9c72c31d1a5e936db6cf9f18c23bd52a43c091fc803ce2139cd390a9678553d1e6061656c3d0196ddbd2233143fc433195")]
[assembly: InternalsVisibleTo("Cosmos.TestRunner.TestController, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3ef5198fa2f8926f006b5d2053eb3b3c875e74695675a6b97bd27ba6b0c5cbee26710c04277f7975927ace4a037692eddb71340a4c3f11e06c645c6a4cebad303301228943b39378bf3222f9432ff9c72c31d1a5e936db6cf9f18c23bd52a43c091fc803ce2139cd390a9678553d1e6061656c3d0196ddbd2233143fc433195")]
[assembly: InternalsVisibleTo("Cosmos.Common, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3ef5198fa2f8926f006b5d2053eb3b3c875e74695675a6b97bd27ba6b0c5cbee26710c04277f7975927ace4a037692eddb71340a4c3f11e06c645c6a4cebad303301228943b39378bf3222f9432ff9c72c31d1a5e936db6cf9f18c23bd52a43c091fc803ce2139cd390a9678553d1e6061656c3d0196ddbd2233143fc433195")]
