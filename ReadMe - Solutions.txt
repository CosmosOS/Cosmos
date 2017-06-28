Solution File ReadMe

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