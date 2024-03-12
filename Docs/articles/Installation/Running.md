# Running
Cosmos supports running on any Intel x86 environment. However for debugging
and ease of use, many virtual and physical setups have integrated support. If
you do not see your environment, this does not mean Cosmos will not work. You
can simply fall back to ISO which should work with any environment.

##  ISO

The ISO option creates an ISO image of the Cosmos output. This ISO file can be
mounted and booted by most virtualization technologies. In addition, a physical
optical disk can be burned and used to boot physical hardware.

# Linux/Posix
### DISCLAIMER: This article assumes that you are using a debian based operating system

## QEMU
Here you can use a generic run file, you can run it by using `sh run.sh -i <ISO> -m <memory size> -h <hdd image>`.
It has to be at the root directory of the project

QEMU is required and can be installed with `apt-get install qemu-system`.

To create an hdd image, you have to convert this [.vmdk](https://github.com/CosmosOS/Cosmos/blob/master/Build/VMWare/Workstation/Filesystem.vmdk?raw=true) file to a .img wich can then be used by QEMU using the following command
`qemu-img convert -f vmdk -O raw Filesystem.vmdk cosmos.img`

`run.sh:`
```sh
#!/bin/bash

helpFunction()
{
   echo ""
   echo "Usage: $0 -i <ISO> -m <memory size> -h <hdd image>"
   echo "\t-i ISO path to be used"
   echo "\t-m Memory size to allocate to the Virtual Machine"
   echo "\t-h Hard disk image location, can be created with qemu-img"
   exit 1 # Exit script after printing help
}

while getopts "i:m:h:" opt
do
   case "$opt" in
      i ) ISO="$OPTARG" ;;
      m ) MEMORY_SIZE="$OPTARG" ;;
      h ) HDD_IMAGE="$OPTARG" ;;
      ? ) helpFunction ;; # Print helpFunction in case parameter is non-existent
   esac
done

# Print helpFunction in case parameters are empty
if [ -z "$ISO" ] || [ -z "$MEMORY_SIZE" ] || [ -z "$HDD_IMAGE" ]
then
   echo "Some or all of the parameters are empty";
   helpFunction
fi

# Build the project
dotnet build

# Emulate the ISO
qemu-system-x86_64 -boot d -cdrom $ISO -m $MEMORY_SIZE -hda $HDD_IMAGE
```

## Virtualbox
Create a virtual machine and use this [.vmdk](https://github.com/CosmosOS/Cosmos/blob/master/Build/VMWare/Workstation/Filesystem.vmdk?raw=true) as HDD image

# Windows

## VMWare

WMWare Workstation and VMWare Player are the preferred testing environments
for Cosmos. This is because they are stable, reliable, and easily controlled
by the Cosmos Visual Studio integration package. This allows for easy debugging and code execution control.

VMWare Workstation is not free, however VMWare Player is. VMWare Player for
the typical Cosmos developer provides more than enough functionality.

### VMWare Player

VMWare Player can be downloaded [here](https://www.vmware.com/go/getplayer-win).

### VMWare Workstation

VMWare Workstation can be downloaded for free trial [here](https://www.vmware.com/go/getworkstation-win).

##  QEMU

Not officially supported at this time, but can be done. Just use the ISO option above with debugging turned off.

QEMU can be downloaded [here](https://www.qemu.org/download/)

## Bochs

Bochs is officially supported.

Bochs can be downloaded [here](https://bochs.sourceforge.io/getcurrent.html)

## PXE

For PXE, no special software is required. Only an ethernet connection to your router. However, if you have a computer that does not support network booting, or if you must perform the action over WiFi, you may need to look at a PXE chainloader such as [gPXE](http://etherboot.org/wiki/).
To run your Cosmos kernel in PXE you need to follow these steps:

Step #1: disable Cosmos Remote Debugger, activate PXE and compilation in bin format.
Go to Properties and copy these settings:

![image](https://user-images.githubusercontent.com/98218366/178479237-a22a7b5f-250e-432d-ba74-0da502e82f7b.png)

Boot the machine and connect it via Ethernet and will show this screen:

![image](https://user-images.githubusercontent.com/98218366/178479800-c70faa5d-1fab-4da4-8f27-05eefee89b75.png)

Wait and your os will boot up.

## Hyper-V

Not supported at this time.


