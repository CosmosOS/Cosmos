

The security model of&nbsp;_place_holder; Cosmos will evolve and mature as
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

Currently only the structure exists, no checking occurs but in the future will
occur via attributes and other methods. Future checks to include:

  1. Restriction of core level exclusive abilities at the assembly level by attribute.
  2. Restriction of references list to higher levels only. ie HAL can reference Core but not vice versa. Also via attribute. That is an assembly level attribute to mark which level an assembly is.
  3. Restrict access to BCL and other assemblies. ie Core should never need XML or other higher level services. Make a defined list of what is allowed and verify against it.

For now we must be vigilant to watch these restrictions manually.

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

### 0 Core Level

Core level has special permissions which basically let it do anyting it wants.
core code should be very limited with most kernel code existing in the System
level. Core code should be kept to a minimum with code not requiring special
permissions should be moved or split into the system level.

Special abilities that core has exclusively:

  1. Assembly plugs

Core 0 exists to provide access to direct hardware. C# code should mostly
exist in level 1 instead, with level 0 being intended for code that can exist
only in level 0 such as assembly code.

Examples:

  1. Plugs that must be implemented in assembly
  2. Memory management
  3. Direct generic IO access to map to C# functions

### 1 HAL (Hardware) Level

From the HAL (hardware abstraction level) level up, none of the special
permissions available in core level are available. If such code is needed, it
must be split between system and core.

HAL in some senses might be thought of as a sub level of level 0, but exists
to help separate assembly and other such code. HAL must not simply proxy
access to code. For example, level 0 will expose functions to directly access
IO ports, but level 2 should never be able to perform such raw access. HAL
must wrap and encapselate such access. For example, instead of proxying access
to a specific port drivers exists in HAL to expose access to a specific device
such as an IDE controller.

Examples:

  1. Hardware specific drivers - any code that needs to talk to hardware via core.0.

### 2 System Level

System level contains all such code that is not wrapping hardware directly but
instead adding higher level functionality. ie file systems, etc. The
distinction between level 1 and 2 is a manual one and no technical
restrictions currently exist aside from the restriction of higher level
reference only.

System.2 should not contain system applications, but instead system
functionality.

Examples:

  1. File systems
  2. BCL plugs
  3. Network stacks

### 3 User Level

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

#  Older Ring Docs

###  Core Ring

In the core ring, "anything goes". This also means that code which does not
require such privileges should not exist in the core ring. The core ring
should be restricted to code which truly needs such unfettered access.

Priviliges that exist only in the core ring:

  * Use of pointers and direct access to memory
  * Access to x86 IO ports.
  * Explicit assembly language code
  * Unsafe code

The core ring includes functionality such as:

  * Threading
  * Memory management

Assemblies which are part of the core are restricted.

The core ring exposes objects which allow the hardware ring to access memory
indirectly, and in a controlled restricted fashion. For example to allow a
driver to acccess video RAM, a memory object can be created by the core ring
which allows access to only the specific block of memory needed by the video
driver.

###  Hardware Ring

The hardware ring contains drivers and code for accessing hardware via more
generic objects exposed by the kernel ring.

The hardware ring includes functionality such as:

  * IDE
  * SATA
  * Ethernet
  * USB Controller

The hardware ring may be further split into two levels at a later date. For
example, a driver may exist for the USB Controller and it would require access
to the core ring. But drivers implementing USB devices do not need to talk to
hardware directly, but instead talk to the USB Controller driver.

