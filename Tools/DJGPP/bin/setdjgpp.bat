@echo off

if "%2" == "" goto error

rem Everything else is set in djgpp.env now.
set DJGPP=%2/djgpp.env

rem  Don't forget to change your PATH!

goto exit

:error
echo.
echo You must call this with DJGPP's installation directory passed
echo twice, first with DOS-style slashes, then with Unix-style
echo slashes.  Example:
echo.
echo   c:\stuff\djgpp\setdjgpp c:\stuff\djgpp c:/stuff/djgpp
echo.

:exit
