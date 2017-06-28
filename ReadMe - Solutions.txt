Unable to resolve 'C:\source\Cosmos\source\Cosmos.IL2CPU\Cosmos.IL2CPU.csproj' for '.NETStandard,Version=v1.5'.
Unable to resolve 'C:\source\Cosmos\source\XSharp.Common\XSharp.Common.csproj' for '.NETStandard,Version=v1.5'.
Unable to resolve 'C:\source\Cosmos\source\Cosmos.Assembler\Cosmos.Assembler.csproj' for '.NETStandard,Version=v1.5'.
Bin refs to core.plugs.asm
add typeextensions

shorten test kernel names
-------------------

Solution File ReadMe

TODO
-Update project templates to use build\bin for kernel and other refs
-Update build to use bin dir, and post build actions too. A bat which can be called like CT.
-Cosmos.TestRunner.* needs in some project, was removed from Kernel.

--------------------------------------------
Cosmos
-Old monolithic solution.
-Will be removed after all smaller parts split out.
-Use of this solution is necessary for some users until all parts are split out.

--------------------------------------------
Build
-Used by the build process (Builder).
-Not meant for direct end user use.
-Do not change unless you know what you are doing.

--------------------------------------------
Kernel
-Kernel, Demos (not  User), Tests
*Modify tests to use TestRunner as a debug launch action - or change refs to bin ref

--------------------------------------------
Builder

Projects used to build Cosmos itself. Essentially this is what makes User Kit go.

-C.B.Builder - Essentially the user kit. A C# program which automates the building and installation of Cosmos wtih a GUI.

-C.B.Insaller - Think this is the UAC workaround task?

******* Add C.B.Common & C.B.MSBuild or to compiler??

--------------------------------------------
--------------------------------------------
--------------------------------------------
--------------------------------------------
Compiler

Assembler, Compiler, XSharp (Compiler Parts), Tools (Compiler Parts)

--------------------------------------------
IDE
Debug, Deploy, Visual Studio, XSharp (IDE Parts), Tools (IDE Parts)

--------------------------------------------
XSharp

Recheck - keep or merge with Compiler?
Links broken - 0 of 4 projects load.