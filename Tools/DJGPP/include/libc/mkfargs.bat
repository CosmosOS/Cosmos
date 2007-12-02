@echo off

echo /* special version for libc - uses %%gs instead of %%fs.  Ignore comments */> farptrgs.h
echo.>>farptrgs.h
sed -e 's/%%fs/%%gs/g' -e 's/0x64/0x65/g' < ..\sys\farptr.h >> farptrgs.h
