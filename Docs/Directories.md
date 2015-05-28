

### Directories

At a first look, the directory structure of Cosmos is quite confusing. There are folders with same names and so on. I'll try to show you how the directories are structured, so that you can find what you're looking for.

#### \Artwork

Just some Cosmos artwork.

#### \Build

The build directory contains file used to build ISO images and Virtual Machines files and images. There is a folder with support files for BOCHS, for Virtual PC, for VMware, QEMU, for support for boot from ISO CD, USB, Ethernet (PXE)

#### \Demos

This folder contains some Cosmos demo projects to help you.

#### \Docs

This folder contains the Cosmos documentation that you're currently reading.

#### \QA

This folder contains the old Cosmos build scripts.
It isn't used at the moment.

#### \Resources

This folder contains the 3rd party libraries that Cosmos uses.

#### \Setup

Contains scripts and languages to build the Cosmos User Kit Installer. The creation (and execution) of the installer is started from \install.bat.


#### \Source

Contains all the code of the Cosmos project. You will spend most of your time here.including the Compiler, Installer, Debugger and the main features of Cosmos. It also contains unused code.

It also contains the solution file of Cosmos and some support libraries.

#### \Source\Cosmos.Assembler

This folder contains the Cosmos assembler, the thing that writes generated assembly code in the kernel file.

#### \Source\Cosmos.Build.*

Any code related to the building process of Cosmos with the exception of
IL2CPU which is however executed from here. Contains code for executing ld,
nasm, makeiso, MSBuild, and others.
All those projects are using during build.

#### \Source\Cosmos.Build.Builder

This is Cosmos bootstrap utility, used to build and install Cosmos user/dev kit.

#### \Source\Cosmos.Common

This folder contains shared code from compiler and debugger.

#### \Source\Cosmos.Core.Plugs

This folder contains plugs for the core components of Cosmos.

#### \Source\Cosmos.Core

This is Cosmos implementation of low-level hardware (CPU, heap, interrupts, etc...).

#### \Source\Cosmos.Debug.*

Any code related to the Cosmos debugger.

#### \Source\Cosmos.Debug.Common

Tontains common stuff, shared with compiler, but debug related.

#### \Source\Cosmos.Debug.DebugStub

This contains the code for the debug stub (the assembler code which lives in the kernel allowing you to debug it without leaving your chair).

#### \Source\Cosmos.Debug.GDB

This contains the code of the GDB client.

#### \Source\Cosmos.Debug.Kernel.Plugs

This contains plugs related to Cosmos.Debug.Kernel.

#### \Source\Cosmos.Debug.Kernel

This contains kernel level debugger helpers.

#### \Source\Cosmos.Debug.VSDebugEngine

This contains the code for the debug engine (VS debugger).

#### \Source\Cosmos.Deploy.Pixie

This is a DHCP/PXE/TFTP server for autobooting live machines (see Youtube/codeproject).

#### \Source\Cosmos.Deploy.USB

Some USB deploy related stuff.

#### \Source\Cosmos.HAL

This is one main component of Cosmos, the HAL (Hardware Abstraction Layer). It contains code and drivers for the computer's hardware (like keyboard, mouse, PCI, console, etc...).

#### \Source\Cosmos.IL2CPU.Plugs

Contains plugs for IL2CPU (see down).

#### \Source\Cosmos.IL2CPU.Profiler

Special use of the compiler for doing some speed testing.
Not used currently.

#### \Source\Cosmos.IL2CPU.Tests

Contains tests for IL2CPU.

#### \Source\Cosmos.IL2CPU

This is another main component of Cosmos. IL2CPU means it converts IL (Intermediate Language), the sortof assembly resulting of the compilation of a C# (or any other .NET language) app to Assembly. When you run a .NET app, the .NET Framework interprets the IL code. When you compile a Cosmos OS, the main project is compiled to IL, and then IL2CPU converts it to ASM so that it can be used as an OS.

#### \Source\Cosmos.System

#### \Source2\XSharp.*

This piece of code gives us the possibility to write Assembly in C#. In this way we can keep all in an OO-like style.

#### \Source2\Debug

Contains the Debug Engine and Connector code. Also gives the possibility to
work with GDB. For more informations about debugging see
[Debugger](debugger.htm)

#### \Source\IL2CPU

Code for the IL2CPU program. It's the Cosmos AOT Compiler. Form more
informations about AOT compilers and IL2CPU see [IL2CPU](il2cpu.htm)

#### \Source2\Kernel

The kernel code. One might think that in an operating system, the kernel code
must be the most complex, but in this case it is actually the simplest as we
are concentrating on debugging. As we finish the debug step, we will develop
the kernel full-time. At the time it contains three folders:

#### \Source2\Kernel\Debug

Contains helpers for the DS code.

#### \Source2\Kernel\System

Contains most of the code of the kernel, including drivers and the core.

#### \Source2\Tests

TODO

#### \Source\Unused

This folder contains work-in-progress or deprecated features, like FAT, VGA and Networking.

#### \Source2\UserKit

TODO

#### \Source2\Utils

TODO

#### \Source2\VSIP

Contains the code for the integration with Visual Studio. Adds support for the
Cosmos Kernel project tipe, and custom build steps.

#### \Source\<insert name here>.Tests

If you see a folder with a name ending by Tests, it means it contains tests.

#### \Source\<insert name here>.Plugs

If you see a folder with a name ending by Plugs, it means it contains plugs for <insert name here>.

#### \Users

Custom code playground. You can put your examples, random code, and toughts
here

