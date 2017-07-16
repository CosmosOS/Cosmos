

# Directories

At a first look, the directory structure of Cosmos is quite confusing. There are folders with same names and so on. I'll try to show you how the directories are structured, so that you can find what you're looking for.

### \Artwork

This directory cointains it contains Cosmos logos and a YouTube channel logo.

### \Build

This directory contains file used to build ISO images and Virtual Machines files and images, there is a folder with supporting files for BOCHS, for Virtual PC, for VMware, QEMU and for boot from ISO CD, USB, Ethernet (PXE).

### \Demos

This directory contains the Cosmos demos projects to help you in programming your operating system.

#### \Demos\Guess

This directory contains the basic Cosmos demo project, it's a simple guess-the-number game.

### \Docs

This directory contains the Cosmos documentation that you're currently reading.

### \QA

This directory contains the old Cosmos build scripts, it's no more used at the moment.

### \Resources

This directory contains the third party libraries that Cosmos uses.

### \Setup

This directory contains all the scripts and languages to build the Cosmos User Kit Installer. The creation (and execution) of the installer is started from '.\install-VS2015.bat'.

### \Source

This directory contains all the code of the Cosmos project. You will spend most of your time here, including the Compiler, Installer, Debugger and the main features of Cosmos. It contains unused code, the solution file of Cosmos and some support libraries.

#### \Source\Cosmos.Assembler

This directory contains the Cosmos assembler, the thing that writes generated assembly code in the kernel file.

#### \Source\Cosmos.Build

This directory contains code related to the building process of Cosmos with the exception of
IL2CPU which is however executed from here, it's contains code for executing ld,
nasm, makeiso, MSBuild, and others.

#### \Source\Cosmos.Common

This directory contains various helpers and files for Cosmos.

#### \Source\Cosmos.Core

This directory contains the core assembly of cosmos. It contains code for managing CPU, IO groups, interrupts, etc..

##### \Source\Cosmos.Core.Plugs

This directory contains the low-level plugs for Cosmos.Core.

#### \Source\Cosmos.Debug

This directory contains the Debug Engine and Connector code and also, it's gives the possibility to
work with GDB. For more informations about debugging see the [Debugger](debugger.htm) page

#### \Source\Cosmos.Deploy

This directory contains the deployment code.

#### \Source\Cosmos.HAL

This directory contains the Cosmos HAL (Hardware Abstraction Layer), means the hardware drivers for graphics, networking, HDD, etc.

#### \Source\Cosmos.IL2CPU

This directory contains the code for the IL2CPU program, the Cosmos AOT Compiler. For more
informations about the AOT compilers and IL2CPU, see [IL2CPU](https://github.com/CosmosOS/Cosmos/blob/master/Docs/Compiler/il2cpu.md) documentation.

#### \Source\Cosmos.System

This directory contains the system-level code for Cosmos. It contains wrappers for Console, networking, filesystem and also contains the base class for Kernel.

#### \Source\Cosmos.VS

This directory contains the code for the integration with Visual Studio. Adds support for the Cosmos Kernel project type, and custom build steps.

##### \Source\Cosmos.VS.Debug

This directory contains the code for the Visual Studio debugger integration.

##### \Source\Cosmos.VS.ProjectSystem

This directory contains the code for the Visual Studio package of Cosmos.

##### \Source\Cosmos.VS.Windows

This directory contains the Cosmos windows in Visual Studio, like the Registers Window.

##### \Source\Cosmos.VS.Windows.Test

This directory contains the tests for Cosmos.VS.Windows.

##### \Source\Cosmos.VS.Wizards

This directory contains the wizards projects of Cosmos.

#### \Source\Unused

This directory contains work-in-progress or deprecated features, like FAT, VGA, Networking and more.

#### \Source\XSharp

This directory contains code for the possibility to write Assembly in C#. In this way we can keep all in an OO-like style.

#### \Users

This directory contains the custom code playground. You can put your examples, random code, and toughts
here

