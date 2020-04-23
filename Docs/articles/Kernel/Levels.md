The security model of Cosmos will evolve and mature as
Cosmos does, however the base model is presented here.

###  Goals

Most operating systems today use hardware to enforce security between the
kernel, user code, etc. This creates a lot of overhead at runtime and
decreases performance. Intel CPUs have built this into the hardware, but adds
a fair bit of circuitry and complexity. Despite being in hardware, it still
incurs a measureable performance penalty. Since Cosmos can control execution
and rebuild its compiler and recompile all code at any time, Cosmos can be
much more flexible and most restrictions can be enforced at compile or
installation time rather than the need to perform such CPU checks in live CPU
code. This creates huge performance gains but also allows a lot more
flexibility.

###  Future

Currently only basic checking is implemented into the compiler. This means that some restrictions are implemented whilst some must be checked manually. 

Currently only this is implemented:

* Restriction of references list to higher levels only. ie HAL can reference Core
but not vice versa. Also via attribute. That is an assembly level attribute to mark which level an assembly is.

In the future both of these will also be implemented:

  1. Restriction of core level exclusive abilities at the assembly level by attribute.
  2. Restrict access to BCL and other assemblies. ie Core should never need XML
 or other higher level services. Make a defined list of what is allowed and verify against it.

For now we must be vigilant to watch these restrictions manually.

### Using rings
It is not possible to run Hardware code in the kernel because it is in ring USER. This means that you cant access the display code in your kernel. To get around this one must create some code in ring 2 (SYSTEM) which allows you to interface between the two. To do this follow this guide:

### Guide: 
To do this right click on your solution on the right hand side and click Add->New Project. Then add a new COSMOS C# OS, call it Hardware. Now you need to both delete the NEW Cosmos boot project (called HardwareBoot) and the Kernel.cs file found within the main Hardware project.

The next job is to allow this project to talk to the screen. To do this right click on the references section of the Hardware project->add references. Now search for cosmos and add all the references (by selecting them with shift-click then pressing ENTER)

Now go into the AssemblyInfo.cs of the Hardware project file and add the lines bellow:

using Cosmos.Common;
[assembly: Ring(Ring.System)]
Well done, you now have a project running in Ring 2.

###  Levels

Many operating systems refer to their security "areas" as rings. ie Kernel
ring, etc. Rings are harder to draw in text and make diagramming a bit harder,
so for now we will refer to our "areas" as levels.

Cosmos is split into the following levels:

  * 0 Core
  * 1 Hardware
  * 2 System
  * 3 User

Each level can only communicate with adjacent levels. That is User can only
talk to System, but not Core.

To make an assembly part of a specific level, you'll have to mark it with the
attribute below to specify on what ring this code is (add the attribute in
AssemblyInfo.cs).

``` [assembly: Ring(Ring.User)] ```

The following are valid enums for the attribute.

```
public enum Ring
{
    Core = 0,
    HAL = 1,
    System = 2,
    User = 3,

}
```

### 0 Core

Summary: "Anything goes"
Implements: IO and memory classes
Specificity: CPU
Assemblies: Cosmos Only

Core exists to provide access to hardware. C# code should mostly
exist in level 1 instead, with level 0 being intended for code that can exist
only in core such as assembly code.

Core has special permissions which essentially allow it do anything.
Core code is to be very limited with most kernel code existing in the System
level. Core code is to be kept to a minimum. Code not requiring special
permissions should be moved or split into the system level.

Things that can exist only in Core - no other levels.

  1. Assembly plugs
  2. Unsafe code (pointers)

Core via unsafe code and assembly plugs is used to expose classes to system allowing
system to interact with hardware.

Core can contain only assemblies of Cosmos itself. No drivers or non Cosmos code is allowed in Core.

Things that Core implements and exposes to HAL:

  1. IO Ports (x86 and some others). Exposes safe interface and implemented using assembly plugs.
  2. Memory Access. Exposes safe interfaces which keep all pointer access in Core. HAL and lower use 
     objects which expose byte arrays and other methods to allow HAL to communicate with memory without
     using pointers itself.
  3. IRQs and other CPU specific functions.
  4. Threading
  5. Memory Management.
  7. PCI

Core also manages consumers of its services to avoid conflicts and security to prevent a specific driver from accessing "everything".

Notes:
https://github.com/joeduffy/slice.net

### 1 HAL (Hardware Abstraction Layer)

Summary: Hardware control
Specificity: Hardware (ie Keyboard, USB, Video), but not CPU
Assemblies: Cosmos, Drivers

HAL uses Core to access hardware and implements the logic details in HAL. Core implements only raw 
classes for accessing hardware, but knows nothing about most of the hardware itself. ie, HAL would implement the
logic for talking to the keyboard using the hardwarwe access classes in Core. Core simply facilitates the 
communication for HAL to the hardware.

Examples:
  1. Raw graphics access (Send this command to graphics card, etc)
  2. Raw disk access
  3. USB

### 2 System

Summary: Main part of OS
Specificity: None (not tied to any hardware or CPU)
Assemblies: Cosmos, Drivers

System level contains all such code that is not wrapping hardware directly but
instead adding higher level functionality. ie file systems, etc. The
distinction between level 1 and 2 is a manual one and no technical
restrictions currently exist aside from the restriction of higher level
reference only.

System should not contain system applications, but instead system
functionality. System applications belong in User.

Examples:

  1. File systems
  2. BCL plugs
  3. Network stacks
  4. Graphics with higher level API consumable by user apps. (Things that cannot be accomplished by hardware. Loading images from files, drawing things not supported by a graphics card command)

### 3 User Level

Summary: User applications
Specificity: None
Assemblies: Any

No Cosmos.3.User project exists because level 3 is user code. ie applications.
When a user creates a new application (or currently monolithic project) the
code is level 3.

Examples:

  1. User applications
  2. System applications

We considered adding a special "playground" ring, which is able to do more.
This would aid in adding new code in playground which is targetted at System
level. However, this adds a possibility you functionally don't really need:
You only need to add 1 or maybe 2 extra projects, but this give you the
physical separation from the start, preventing you to make circular
references.

# Examples

## Disk

0 IO/Memory access (Cosmos)
1 IDE / SATA
2 Block Device, Filesystem
3 Application

## Graphics

0 IO/Memory access (Cosmos)
1 DirectX / VESA / Graphics Memory
2 WinForms, WPF, Other UI, Canvas, JPG/PNG/GIF
3 Application

## Network

0 IO/Memory access (Cosmos)
1 Ethernet 
2 TCP/IP
3 Application
