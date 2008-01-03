# call msbuild3_5 d:\dotnet\il2asm\repos\source\IL2CPU.sln

.\sub-compile
.\sub-pause

remove-item PXE\Boot\output.obj -ea SilentlyContinue
move-item output.obj PXE\Boot\output.obj
