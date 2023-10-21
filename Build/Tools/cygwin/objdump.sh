#!/bin/bash

# %1 == ToolPath
# %2 == ElfFile
# %3 == MapFile
objdump --wide --syms "$2" > "$3"
