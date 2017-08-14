imdisk -d -m M:
imdisk -a -s 1G -m M: -p "/fs:NTFS /V:MemDisk /q /y"

mkdir m:\Temp

xcopy D:\MemDisk\*.* m:\*.* /E /K /H /R /Q
REM attrib -A M:\ /S /D
