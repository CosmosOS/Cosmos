

# Directories

At a first look, the directory structure of Cosmos is quite confusing. There are folders with same names and so on. I'll try to show you how the directories are structured, so that you can find what you're looking for.

### \Artwork

Just some Cosmos artwork. It contains Cosmos logo and YouTube channel logo.

### \Build

The build directory contains file used to build ISO images and Virtual Machines files and images. There is a folder with support files for BOCHS, for Virtual PC, for VMware, QEMU, for support for boot from ISO CD, USB, Ethernet (PXE).

### \Demos

This folder contains some Cosmos demo projects to help you.

#### \Demos\Guess

This is the basic Cosmos demo project. It's a simple guess-the-number game.

### \Docs

This folder contains the Cosmos documentation that you're currently reading.

### \QA

This folder contains the old Cosmos build scripts.
It isn't used at the moment.

### \Resources

This folder contains the 3rd party libraries that Cosmos uses.

### \Setup

Contains scripts and languages to build the Cosmos User Kit Installer. The creation (and execution) of the installer is started from \install-VS2015.bat.

### \Source

Contains all the code of the Cosmos project. You will spend most of your time here.including the Compiler, Installer, Debugger and the main features of Cosmos. It also contains unused code.

It also contains the solution file of Cosmos and some support libraries.

#### \Source\Cosmos.Assembler

This folder contains the Cosmos assembler, the thing that writes generated assembly code in the kernel file.

#### \Source\Cosmos.Build

Any code related to the building process of Cosmos with the exception of
IL2CPU which is however executed from here. Contains code for executing ld,
nasm, makeiso, MSBuild, and others.

#### \Source\Cosmos.Common

Contains various helpers and files for Cosmos.

#### \Source\Cosmos.Core

This is the core assembly of cosmos. It contains code for managing CPU, IO groups, interrupts, etc.

##### \Source\Cosmos.Core.Plugs

This folder contains the low-level plugs for Cosmos.Core.

#### \Source\Cosmos.Debug

Contains the Debug Engine and Connector code. Also gives the possibility to
work with GDB. For more informations about debugging see
[Debugger](debugger.htm)

#### \Source\Cosmos.Deploy

Contains the deployment code.

#### \Source\Cosmos.HAL

Contains code for Csosmos HAL (Hardware Abstraction Layer), means the hardware drivers for graphics, networking, HDD, etc.

#### \Source\Cosmos.IL2CPU

Code for the IL2CPU program. It's the Cosmos AOT Compiler. For more
informations about AOT compilers and IL2CPU see [IL2CPU](https://github.com/CosmosOS/Cosmos/blob/master/Docs/Compiler/il2cpu.md)

#### \Source\Cosmos.System

Contains the system-level code for Cosmos. It contains wrappers for Console, networking and filesystem. It also contains the base class for Kernel.

#### \Source\Cosmos.VS

Contains the code for the integration with Visual Studio. Adds support for the Cosmos Kernel project type, and custom build steps.

##### \Source\Cosmos.VS.Debug

Contains the code for the Visual Studio debugger integration.

##### \Source\Cosmos.VS.Package

Contains the code for the Visual Studio package of Cosmos.

##### \Source\Cosmos.VS.Windows

Contains the Cosmos windows in VS, like the Registers window.

##### \Source\Cosmos.VS.Windows.Test

Contains the tests for Cosmos.VS.Windows.

##### \Source\Cosmos.VS.Wizards

Contains the project wizards of Cosmos.

#### \Source\Unused

This folder contains work-in-progress or deprecated features, like FAT, VGA and Networking.

#### \Source\XSharp

This piece of code gives us the possibility to write Assembly in C#. In this way we can keep all in an OO-like style.

#### \Users

Custom code playground. You can put your examples, random code, and toughts
here

