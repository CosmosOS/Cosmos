

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

#### \Source\Cosmos.Common

TODO

#### \Source\Cosmos.Core.Plugs

This folder contains plugs for

#### \Source\Cosmos.Common

TODO

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
Cosmos Kernel project type, and custom build steps.

#### \Source\<insert name here>.Tests

If you see a folder with a name ending by Tests, it means it contains tests.

#### \Source\<insert name here>.Plugs

If you see a folder with a name ending by Plugs, it means it contains plugs for <insert name here>.

#### \Users

Custom code playground. You can put your examples, random code, and toughts
here

