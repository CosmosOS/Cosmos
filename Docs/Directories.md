

# Directories
At a first look, the directory structure of Cosmos is quite confusing. There are folders with same names and so on. I'll try to show you how the directories are structured, so that you can find what you're looking for.

## Artwork
Just some Cosmos artwork.

## Build
The build directory contains file used to build ISO images and Virtual Machines files and images. There is a folder with support files for BOCHS, for Virtual PC, for VMware, QEMU, for support for boot from ISO CD, USB, Ethernet (PXE)

## Demos
This folder contains some Cosmos demo projects to help you.

## Docs
This folder contains the Cosmos documentation that you're currently reading.

## QA
This folder contains the old Cosmos build scripts.
It isn't used at the moment.

## Resources
This folder contains the 3rd party libraries that Cosmos uses.

## Setup
Contains scripts and languages to build the Cosmos User Kit Installer. The creation (and execution) of the installer is started from \install.bat.

## Source
Contains all the code of the Cosmos project, including the Compiler, Installer, Debugger and the main features of Cosmos. It also contains unused code. You will spend most of your time here. 
It also contains the solution file of Cosmos and some support libraries.

### Cosmos.Assembler
This folder contains the Cosmos assembler, the thing that writes generated assembly code in the kernel file.

### Cosmos.Build.*
Any code related to the building process of Cosmos with the exception of
IL2CPU which is however executed from here. Contains code for executing ld,
nasm, makeiso, MSBuild, and others.
All those projects are using during build.

#### Cosmos.Build.Builder
This is Cosmos bootstrap utility, used to build and install Cosmos user/dev kit.

### Cosmos.Common
This folder contains shared code from compiler and debugger.

### Cosmos.Core
This is Cosmos implementation of low-level hardware (CPU, heap, interrupts, etc...).

#### Cosmos.Core.Plugs
This folder contains plugs for the core components of Cosmos.

### Cosmos.Debug.*
Any code related to the Cosmos debugger.

#### Cosmos.Debug.Common
Contains common stuff, shared with compiler, but debug related.

#### Cosmos.Debug.DebugStub
This contains the code for the debug stub (the assembler code which lives in the kernel allowing you to debug it without leaving your chair).

#### Cosmos.Debug.GDB
This contains the code of the GDB client.

#### Cosmos.Debug.Kernel.Plugs
This contains plugs related to Cosmos.Debug.Kernel.

#### Cosmos.Debug.Kernel
This contains kernel level debugger helpers.

#### Cosmos.Debug.VSDebugEngine
This contains the code for the debug engine (VS debugger).

### Cosmos.Deploy.Pixie
This is a DHCP/PXE/TFTP server for autobooting live machines (see Youtube/codeproject).

### Cosmos.Deploy.USB
Some USB deploy related stuff.

### Cosmos.HAL
This is one main component of Cosmos, the HAL (Hardware Abstraction Layer). It contains code and drivers for the computer's hardware (like keyboard, mouse, PCI, console, etc...).

### Cosmos.IL2CPU
This is another main component of Cosmos. IL2CPU means it converts IL (Intermediate Language), the sortof assembly resulting of the compilation of a C# (or any other .NET language) app to Assembly. When you run a .NET app, the .NET Framework interprets the IL code. When you compile a Cosmos OS, the main project is compiled to IL, and then IL2CPU converts it to ASM so that it can be used as an OS.

#### Cosmos.IL2CPU.Plugs
Contains plugs for IL2CPU.

#### Cosmos.IL2CPU.Profiler
Special use of the compiler for doing some speed testing.
Not used currently.

#### Cosmos.IL2CPU.Tests
Contains tests for IL2CPU.

### Cosmos.System
It's the system part of Cosmos. It contains wrappers TODO TODO TODO

### Cosmos.VS.*
Contains code for VS integration.

#### Cosmos.VS.Debug
Code for VS debugging engine.

#### Cosmos.VS.Package
The Cosmos VS package.

#### Cosmos.VS.Windows
The Cosmos VS windows, like the Cosmos project settings, the Assembly window, the Registers window etc.

#### Cosmos.VS.Windows.Test
The tests for Cosmos.VS.Windows.

#### Cosmos.VS.Wizards
The code for the Cosmos wizards, for example the project creation wizard, which generates the Cosmos project file.

### ElfMap2DebugDb
TODO

### IL2CPU
TODO

### MDbg4
TODO

### MPF
TODO

### Tools
Some tools for development.

### Unused
Some old unused code.

### VMWareLib
TODO

### XSharp.*
Contains all the code for the homemade Cosmos programming language, X# (XSharp).

#### XSharp.Compiler
The X# compiler.

#### XSharp.Nasm
The program that converts X# to assembly code.

#### XSharp.Test
Tests for X#.

#### XSharp.VS
VS integration for X#.

### libraries\nunit
TODO

### Build.sln
This is the build solution file for Cosmos. This is the one which will be used for Cosmos compilation (Userkit / Devkit). It only contains Cosmos code (not tests or something else).

### Builder.sln
TODO

### Cosmos.sln
This is the FULL solution of Cosmos. It's quite huge (90 KB) and contains lots of projects. It contains all the projects, including users' playgrounds, demos and others.

### XSharp.sln
This solution contains the code for compiling X# stuff (compiler, assembler, VS integration, tests etc..).

#### \Source\<insert name here>.Tests

If you see a folder with a name ending by Tests, it means it contains tests.

#### \Source\<insert name here>.Plugs

If you see a folder with a name ending by Plugs, it means it contains plugs for <insert name here>.

#### \Users

Custom code playground. You can put your examples, random code, and toughts
here

