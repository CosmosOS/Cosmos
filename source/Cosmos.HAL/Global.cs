using System;

using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.HAL
{
  public static class Global
  {
    public static readonly Debugger mDebugger = new Debugger("HAL", "Global");

    //static public PIT PIT = new PIT();
    // Must be static init, other static inits rely on it not being null
    public static TextScreenBase TextScreen = new TextScreen();

    public static PCI Pci;

    private static void InitAta(Ata.ControllerIdEnum aControllerID,
        Ata.BusPositionEnum aBusPosition)
    {
      var xIO = aControllerID == Ata.ControllerIdEnum.Primary
          ? Core.Global.BaseIOGroups.ATA1
          : Core.Global.BaseIOGroups.ATA2;
      var xATA = new AtaPio(xIO, aControllerID, aBusPosition);
      if (xATA.DriveType == AtaPio.SpecLevel.Null)
      {
        return;
      }
      if (xATA.DriveType == AtaPio.SpecLevel.ATA)
      {
        BlockDevice.BlockDevice.Devices.Add(xATA);
        Ata.AtaDebugger.Send("ATA device with speclevel ATA found.");
      }
      else
      {
        //Ata.AtaDebugger.Send("ATA device with spec level " + (int)xATA.DriveType +
        //                     " found, which is not supported!");
        return;
      }
      var xMbrData = new byte[512];
      xATA.ReadBlock(0UL, 1U, xMbrData);
      var xMBR = new MBR(xMbrData);

      if (xMBR.EBRLocation != 0)
      {
        //EBR Detected
        var xEbrData = new byte[512];
        xATA.ReadBlock(xMBR.EBRLocation, 1U, xEbrData);
        var xEBR = new EBR(xEbrData);

        for (int i = 0; i < xEBR.Partitions.Count; i++)
        {
          //var xPart = xEBR.Partitions[i];
          //var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
          //BlockDevice.BlockDevice.Devices.Add(xPartDevice);
        }
      }

      // TODO Change this to foreach when foreach is supported
      Ata.AtaDebugger.Send("Number of MBR partitions found:");
      Ata.AtaDebugger.SendNumber(xMBR.Partitions.Count);
      for (int i = 0; i < xMBR.Partitions.Count; i++)
      {
        var xPart = xMBR.Partitions[i];
        if (xPart == null)
        {
          Console.WriteLine("Null partition found at idx: " + i);
        }
        else
        {
          var xPartDevice = new Partition(xATA, xPart.StartSector, xPart.SectorCount);
          BlockDevice.BlockDevice.Devices.Add(xPartDevice);
          Console.WriteLine("Found partition at idx" + i);
        }
      }
    }

    // Init devices that are "static"/mostly static. These are devices
    // that all PCs are expected to have. Keyboards, screens, ATA hard drives etc.
    // Despite them being static, some discovery is required. For example, to see if
    // a hard drive is connected or not and if so what type.
    internal static void InitStaticDevices()
    {
      //TextScreen = new TextScreen();
      mDebugger.Send("CLS");
      //TODO: Since this is FCL, its "common". Otherwise it should be
      // system level and not accessible from Core. Need to think about this
      // for the future.
      mDebugger.Send("Finding PCI Devices");
      //PCI.Setup();
    }

    static public void Init(TextScreenBase textScreen)
    {
      if (textScreen != null)
      {
        TextScreen = textScreen;
      }

      mDebugger.Send("Before Core.Global.Init");
      Core.Global.Init();
      mDebugger.Send("Static Devices");
      InitStaticDevices();
      mDebugger.Send("PCI Devices");
      InitPciDevices();
      mDebugger.Send("Done initializing Cosmos.HAL.Global");

      mDebugger.Send("ATA Primary Master");
      InitAta(Ata.ControllerIdEnum.Primary, Ata.BusPositionEnum.Master);

      //TODO Need to change code to detect if ATA controllers are present or not. How to do this? via PCI enum?
      // They do show up in PCI space as well as the fixed space.
      // Or is it always here, and was our compiler stack corruption issue?
      mDebugger.Send("ATA Secondary Master");
      InitAta(Ata.ControllerIdEnum.Secondary, Ata.BusPositionEnum.Master);
      //InitAta(BlockDevice.Ata.ControllerIdEnum.Secondary, BlockDevice.Ata.BusPositionEnum.Slave);
    }

    internal static void InitPciDevices()
    {
      //TODO Redo this - Global init should be other.
      // Move PCI detection to hardware? Or leave it in core? Is Core PC specific, or deeper?
      // If we let hardware do it, we need to protect it from being used by System.
      // Probably belongs in hardware, and core is more specific stuff like CPU, memory, etc.
      //Core.PCI.OnPCIDeviceFound = PCIDeviceFound;

      //TODO: Since this is FCL, its "common". Otherwise it should be
      // system level and not accessible from Core. Need to think about this
      // for the future.
      Console.WriteLine("Finding PCI Devices");
      Console.WriteLine();
      ;
      ;
      ;
      PCI.Setup();
    }

    public static void EnableInterrupts()
    {
      CPU.EnableInterrupts();
    }

    public static bool InterruptsEnabled
    {
      get
      {
        return CPU.mInterruptsEnabled;
      }
    }
  }
}
