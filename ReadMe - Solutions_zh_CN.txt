================ 项目文件ReadMe ================ 

--------------------------------------------
Cosmos
-旧的解决方案。
-将在以后分离后删除。
-对于某些用户来说，在所有的部分被拆分之前，他们还是得使用这个。

--------------------------------------------
Build
-由生成进程使用(Builder)。
-用户不会使用这个。
-除非你知道你在干什么，否则你不要尝试去修改它。

--------------------------------------------
Builder

-用来编译Cosmos本身的项目。从本质上说，这就是用户工具包。
-C.B.Builder - 基本上是用户工具包。一个C#程序，通过图形用户界面自动建立和安装Cosmos。
-C.B.Insaller - 这是给UAC的解决方案？

******* Add C.B.Common & C.B.MSBuild or to compiler??

--------------------------------------------
Compiler

部分:
-Assembler
-Compiler
-XSharp (编译组件)
-Tools (编译组件)

Notes:
-In future XSharp may be separated or a SLN with only XSharp parts in it. But it is needed often when using compiler.

Bin Cache Dependencies:


--------------------------------------------
IDE

TODO Exists, but needs cleaned up still a lot.

Debug, Deploy, Visual Studio, XSharp (IDE Parts), Tools (IDE Parts)

--------------------------------------------
Kernel

-Kernel, Demos (not  User), Tests

--------------------------------------------
Test

-所有单元测试项目。(Unit test)

--------------------------------------------
Tools

-Misc Tools

--------------------------------------------

