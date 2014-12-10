

### Topic is out of date!

### Directories

At a first look, the directory structure of Cosmos is quite confusing. There
are folders with same names, two source folders and so on. I'll try to show
you how the directories are structured, so that you can find what you're
looking for

#### \Build

The build directory contains file used to build ISO images and Virtual
Machines files and images. There is a folder with support files for BOCHS, for
VirtualPc, for VMware, QEMU, for support for boot from ISO CD, USB, Ethernet
(PXE)

There is also an additional folder **VSIP**. It normally contains a file
called _install.bat_. That file must be executed as you download the code and
everytime you change some code which is non-kernel related (eg. the debugger,
the VS integration package, the IL2CPU code etc)

It compiles all the projects to libraries, creates a Win32 Installer and
setups the Visual Studio IP. At the end it starts Visual Studio.

You should close Visual Studio before executing install.bat.

#### Docs

Just a bunch of bad-structured documentation about Cosmos

#### QA

Todo.

#### Resources

Here you can put any pdf, doc, rtf, txt which can be used as a reference help
to develop Cosmos. It also contains the folder **x86**. Inside you can find
many interesting references about the x86 architecture.

#### Setup

Contains scripts and languages to build the Cosmos User Kit Installer. The
creation (and execution) of the installer is started from
\Build\VSIP\install.bat

#### \Source

**TODO: Check correctness of the following sentences: **Contains a lot of code which is now not used. It was used previously, but it was decided to take it away and to restructure the code organization. It may be reintegrated however in future, with some code changes.

It also contains the solution file of Cosmos and some support libraries, which
IMHO should be moved to Source2

#### \Source2

Contains the real code of Cosmos. You will spend most of your time here.
However as you open it, you get confused again. Another Build, another VSIP,
two or three Debug directories, etc. Let's summarize:

#### \Source2\Build

Any code related to the building process of Cosmos with the exception of
IL2CPU which is however executed from here. Contains code for executing ld,
nasm, makeiso, MSBuild, and others.

#### \Source2\Compiler

**TODO: Check**. This piece of code gives us the possibility to write Assembly in C#. In this way we can keep all in an OO-like style.

#### \Source2\Debug

Contains the Debug Engine and Connector code. Also gives the possibility to
work with GDB. For more informations about debugging see
[Debugger](debugger.htm)

#### \Source2\Demos

Should contain examples of the use of the Cosmos User Kit. Actually only a
number Guess demo is available

#### \Source2\IL2CPU

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

#### \Source2\UserKit

TODO

#### \Source2\Users

Custom code playground. You can put your examples, random code, and toughts
here

#### \Source2\Utils

TODO

#### \Source2\VSIP

Contains the code for the integration with Visual Studio. Adds support for the
Cosmos Kernel project tipe, and custom build steps.

