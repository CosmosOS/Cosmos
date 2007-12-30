function Pause ($Message="Press any key to continue...") {
	Write-Host -NoNewLine $Message
	$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
	Write-Host ""
}


"You must first run Set-ExecutionPolicy remotesigned"
"You may also have to unblock this file using file, properties in explorer"

# call msbuild3_5 d:\dotnet\il2asm\repos\source\IL2CPU.sln

# ----------- Compile with IL2CPU
remove-item output.asm -ea SilentlyContinue
..\..\..\source\il2cpu\bin\Debug\il2cpu "-in:..\..\..\source\Cosmos\Cosmos.Shell.Console\bin\Debug\Cosmos.Shell.Console.exe" "-plug:..\..\..\source\Cosmos\Cosmos.Kernel.Plugs\bin\Debug\Cosmos.Kernel.Plugs.dll" "-out:output.obj" "-platform:nativex86" "-asm:output.asm"
remove-item files\output.obj -ea SilentlyContinue
copy-item output.obj files\output.obj
pause

# ----------- Build ISO
remove-item cosmos.iso -ea SilentlyContinue
attrib files\boot\grub\stage2_eltorito -r
..\..\..\Tools\mkisofs\mkisofs -R -b boot/grub/stage2_eltorito -no-emul-boot -boot-load-size 4 -boot-info-table -o Cosmos.iso files
pause

# ----------- Start QEMU
remove-item serial-debug.txt -ea SilentlyContinue
cd ..\..\..\tools\qemu\
#.\qemu -L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial "file:..\..\build\Cosmos\ISO\serial-debug.txt" -S -s
#pause
$qemu = resolve-path qemu.exe
$qemuparms = '-L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial "file:..\..\build\Cosmos\ISO\serial-debug.txt" -S -s'
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
$gdb = resolve-path gdb.exe
$gdbparms = '..\..\..\Build\Cosmos\ISO\output.obj --eval-command="target remote:1234" --eval-command="b _CODE_REQUESTED_BREAK_" --eval-command="c"'
$process2 = [System.Diagnostics.Process]::Start($gdb, $gdbparms);
$process2.WaitForExit()

pause
#http://www.vistax64.com/powershell/114718-how-can-i-execute-wmi-method-asynchronously.html
#http://blogs.technet.com/industry_insiders/pages/executing-a-command-line-utility-from-powershell-and-waiting-for-it-to-finish.aspx
#invoke-item
if(!$process2.HasExited) {
	$process2.Kill()
}
if(!$process.HasExited) {
	$process.Kill()
}
