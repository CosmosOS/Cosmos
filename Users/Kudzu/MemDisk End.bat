del m:\temp\*.* /S /Q /F
rmdir m:\temp

REM Have to copy all, not just archived, else deleted files will reappear on boot
del D:\MemDisk\*.* /S /Q /F
REM Get the pesky .suo files
del D:\MemDisk\*.* /S /Q /F /A:H

xcopy m:\*.* D:\MemDisk\ /E /K /H /R /Q

pause

