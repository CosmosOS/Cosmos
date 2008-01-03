@echo off
echo You must first run Set-ExecutionPolicy remotesigned
echo You may also have to unblock this file using file, properties in explorer
echo -

powershell -command "%1"
