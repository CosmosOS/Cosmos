#!/bin/sh

# $1 == ToolPath
# $2 == ElfFile
# $3 == MapFile

"$1/objdump" --wide --syms "$2" > "$3"
