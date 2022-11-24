# Visual Studio Solutions

## `Build.sln`
Currently contains every used Cosmos component for
using by Cosmos Builder to compile and pack code.
Do not use it in Visual Studio directly and do not
modify it, unless you know what you're doing.

## `Builder.sln`
Currently contains only Cosmos Builder app project 
that used to compile, pack and install Cosmos.

## `IDE.sln`
Currently contains Visual Studio extensions for VS
integration. Do not confuse with `Kernel.sln`.

## `Kernel.sln`
Currently contains the main Cosmos code that runs on
target virtual machines. If you want to modify OS
behavior more percisely, contribute to this solution.
### Cosmos.System2
Contains the highest library, that controls entire
system abstractly, as another manage mostly hardware.
### Cosmos.HAL2
Contains Hardware Abstraction Layer and most of 
system drivers used by Cosmos. These include PS/2,
PCI, PIT, etc.
### Cosmos.Core
Contains the most basic things that used by NET BCL
and Cosmos, such as Plugs and XSharp snippets.

## `Demos.sln`
Currently contains user-made Cosmos project examples.
Also, it can be used as test solution

## `Test.sln`
Currently contains all unit tests that Cosmos uses
to ensure that there are no bugs.
