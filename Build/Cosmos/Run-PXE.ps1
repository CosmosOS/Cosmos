# call msbuild3_5 d:\dotnet\il2asm\repos\source\IL2CPU.sln

.\sub-compile

remove-item PXE\Boot\output.obj -ea SilentlyContinue
move-item output.obj PXE\Boot\output.obj

# must change dir so tftpd32 will set itself to proper dir
cd pxe
.\tftpd32
cd ..

.\sub-pause
