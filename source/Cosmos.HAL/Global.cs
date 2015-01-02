using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL {
  static public class Global {
    static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Hardware", "");

    static public Keyboard Keyboard;
    //static public PIT PIT = new PIT();
    // Must be static init, other static inits rely on it not being null
      static public TextScreenBase TextScreen = new TextScreen();

      public static PCI Pci;

    static void InitAta(BlockDevice.Ata.ControllerIdEnum aControllerID, BlockDevice.Ata.BusPositionEnum aBusPosition) {
      var xIO = aControllerID == BlockDevice.Ata.ControllerIdEnum.Primary ? Cosmos.Core.Global.BaseIOGroups.ATA1 : Cosmos.Core.Global.BaseIOGroups.ATA2;
      var xATA = new BlockDevice.AtaPio(xIO, aControllerID, aBusPosition);
      if (xATA.DriveType != BlockDevice.AtaPio.SpecLevel.Null) {
        BlockDevice.BlockDevice.Devices.Add(xATA);        
          var xMbrData = new byte[512];
          xATA.ReadBlock(0UL, 1U, xMbrData);
          var xMBR = new BlockDevice.MBR(xMbrData);

          if (xMBR.EBRLocation != 0)
          {
              //EBR Detected
              var xEbrData = new byte[512];
              xATA.ReadBlock(xMBR.EBRLocation, 1U, xEbrData);
              var xEBR = new BlockDevice.EBR(xEbrData);

              for (int i = 0; i < xEBR.Partitions.Count; i++)
              {
                  //var xPart = xEBR.Partitions[i];
                  //var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
                  //BlockDevice.BlockDevice.Devices.Add(xPartDevice);
              }
          }

          // TODO Change this to foreach when foreach is supported
          Console.WriteLine("Number of MBR partitions found:  " + xMBR.Partitions.Count);
          for (int i = 0; i < xMBR.Partitions.Count; i++)
          {
              var xPart = xMBR.Partitions[i];
              if (xPart == null)
              {
                  Console.WriteLine("Null partition found at idx " + i);
              }
              else
              {
                  var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
                  BlockDevice.BlockDevice.Devices.Add(xPartDevice);
                  Console.WriteLine("Found partition at idx " + i);
              }
          }
      }
    }

    // Init devices that are "static"/mostly static. These are devices
    // that all PCs are expected to have. Keyboards, screens, ATA hard drives etc.
    // Despite them being static, some discovery is required. For example, to see if
    // a hard drive is connected or not and if so what type.
    static internal void InitStaticDevices() {
      //TextScreen = new TextScreen();
      Global.Dbg.Send("CLS");
        
      TextScreen.Clear();

      Global.Dbg.Send("Keyboard");
      Keyboard = new Keyboard();

      // Find hardcoded ATA controllers
      Global.Dbg.Send("ATA Master");
      InitAta(BlockDevice.Ata.ControllerIdEnum.Primary, BlockDevice.Ata.BusPositionEnum.Slave);

      Global.Dbg.Send("ATA Slave");
      InitAta(BlockDevice.Ata.ControllerIdEnum.Primary, BlockDevice.Ata.BusPositionEnum.Master);

      //TODO Need to change code to detect if ATA controllers are present or not. How to do this? via PCI enum? 
      // They do show up in PCI space as well as the fixed space. 
      // Or is it always here, and was our compiler stack corruption issue?
      //InitAta(BlockDevice.Ata.ControllerIdEnum.Secondary, BlockDevice.Ata.BusPositionEnum.Master);
      //InitAta(BlockDevice.Ata.ControllerIdEnum.Secondary, BlockDevice.Ata.BusPositionEnum.Slave);
    }

    static internal void InitPciDevices() {
      //TODO Redo this - Global init should be other.
      // Move PCI detection to hardware? Or leave it in core? Is Core PC specific, or deeper?
      // If we let hardware do it, we need to protect it from being used by System.
      // Probably belongs in hardware, and core is more specific stuff like CPU, memory, etc.
      //Core.PCI.OnPCIDeviceFound = PCIDeviceFound;

      //TODO: Since this is FCL, its "common". Otherwise it should be
      // system level and not accessible from Core. Need to think about this
      // for the future.
      Console.WriteLine("Finding PCI Devices");
      PCI.Setup();

    }

    static public void Init(TextScreenBase textScreen)
    {
      if (textScreen != null)
      {
        TextScreen = textScreen;
      }
      Core.Bootstrap.Init();
      Core.Global.Init();
      Global.Dbg.Send("Static Devices");
      InitStaticDevices();
      Global.Dbg.Send("PCI Devices");
      InitPciDevices();
    }

    //static void PCIDeviceFound(Core.PCI.PciInfo aInfo, Core.IOGroup.PciDevice aIO) {
      // Later we need to dynamically load these, but we need to finish the design first.
    //  if ((aInfo.VendorID == 0x8086) && (aInfo.DeviceID == 0x7111)) {
        //ATA1 = new ATA(Core.Global.BaseIOGroups.ATA1);
    //  }
    //}

  }
}
