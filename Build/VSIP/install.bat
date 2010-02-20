rem %windir%\Microsoft.NET\Framework\V3.5\msbuild ..\..\source\Cosmos.sln

xcopy /Y ..\Tools\Cosmos.Hardware\Cosmos.Build.Common.* .
xcopy /Y ..\Tools\Cosmos.Hardware\Cosmos.IL2CPU.* .
xcopy /Y ..\Tools\Cosmos.Hardware\Cosmos.Kernel.Plugs.* .
xcopy /Y ..\Tools\Cosmos.Hardware\Cosmos.Sys.Plugs.* .

xcopy /Y ..\..\source2\Build\Cosmos.Build.MSBuild\bin\Debug\Cosmos.Build.MSBuild.* .
xcopy /Y ..\..\source2\Build\Cosmos.Build.MSBuild\Cosmos.targets .
xcopy /Y ..\..\source2\Debug\Cosmos.Debug.HostProcess\bin\Debug\Cosmos.Debug.HostProcess.exe .
xcopy /Y ..\..\source2\Debug\Cosmos.Debug.HostProcess\bin\Debug\Cosmos.Debug.HostProcess.pdb .
xcopy /Y ..\..\source2\Debug\Cosmos.Debug.VSDebugEngine\bin\Debug\Cosmos.Debug.VSDebugEngine.* .
xcopy /Y ..\..\source2\Debug\Cosmos.Debug.VSRegister\bin\Debug\Cosmos.Debug.VSRegister.pdb .
xcopy /Y ..\..\source2\Debug\Cosmos.Debug.VSRegister\bin\Debug\Cosmos.Debug.VSRegister.exe .
xcopy /Y ..\..\source2\VSIP\Cosmos.VS.Package\bin\Debug\Cosmos.VS.Package.* .


