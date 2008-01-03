# call msbuild3_5 d:\dotnet\il2asm\repos\source\IL2CPU.sln

.\sub-compile
.\sub-pause

.\sub-MakeISO
.\sub-pause

cd iso
# ----------- Start QEMU
remove-item serial-debug.txt -ea SilentlyContinue
cd ..\..\..\tools\qemu\
#.\qemu -L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial "file:..\..\build\Cosmos\ISO\serial-debug.txt" -S -s
$qemu = resolve-path qemu.exe
$qemuparms = '-L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial "file:..\..\build\Cosmos\ISO\serial-debug.txt" -no-kqemu -S -s'
# Still failing - because its a command line exe? run under cmd.exe?
$processInfo = new-object System.Diagnostics.ProcessStartInfo
$processInfo.FileName = $qemu
$processInfo.WorkingDirectory = [System.IO.Path]::GetDirectoryName($qemu);
$processInfo.Arguments = $qemuparms;
$processInfo.UseShellExecute = $False
$processInfo.RedirectStandardOutput = $False
$processInfo.RedirectStandardError = $False
$process = [System.Diagnostics.Process]::Start($processInfo)
#if(!($process.WaitForExit(60*1000) -and ($process.ExitCode -eq 0))) 
#{
#	Write-Host $process.StandardOutput.ReadToEnd()
#	Write-Host $process.StandardError.ReadToEnd()
#}
[System.Threading.Thread]::Sleep(1000)
cd ..\gdb\bin\
$blaat = resolve-path ..\..\..\Build\Cosmos\ISO\files\output.obj
$gdb = resolve-path gdb.exe
$gdbparms =  [System.String]::Concat($blaat, ' --eval-command="target remote:1234" --eval-command="b _CODE_REQUESTED_BREAK_" --eval-command="c"');
$process2 = [System.Diagnostics.Process]::Start($gdb, $gdbparms);
$process2.WaitForExit()

..\sub-pause

#http://www.vistax64.com/powershell/114718-how-can-i-execute-wmi-method-asynchronously.html
#http://blogs.technet.com/industry_insiders/pages/executing-a-command-line-utility-from-powershell-and-waiting-for-it-to-finish.aspx
#invoke-item
if(!$process2.HasExited) {
	$process2.Kill()
}
if(!$process.HasExited) {
	$process.Kill()
}

#PXE
#remove-item PXE\Boot\output.obj -ea SilentlyContinue
#move-item output.obj PXE\Boot\output.obj


