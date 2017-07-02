================ Solution File ReadMe ================ 

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
Builder

Projects used to build Cosmos itself. Essentially this is what makes User Kit go.

-C.B.Builder - Essentially the user kit. A C# program which automates the building and installation of Cosmos wtih a GUI.

-C.B.Insaller - This is the UAC workaround task?

******* Add C.B.Common & C.B.MSBuild or to compiler??

--------------------------------------------
Compiler

Parts:
-Assembler
-Compiler
-XSharp (Compiler Parts)
-Tools (Compiler Parts)

Notes:
-In future XSharp may be separated or a SLN with only XSharp parts in it. But it is needed often when using compiler.

Bin Cache Dependencies:


--------------------------------------------
IDE

TODO Exists, but needs cleaned up still a lot.

Debug, Deploy, Visual Studio, XSharp (IDE Parts), Tools (IDE Parts)

--------------------------------------------
Kernel

-Kernel, Demos (not  User), Tests

--------------------------------------------
Test

-All unit tests

--------------------------------------------
Tools

-Misc Tools

--------------------------------------------

