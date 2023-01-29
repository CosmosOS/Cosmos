using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.Drivers.PCI.Network;

namespace Cosmos.HAL.Network
{
    public class NetworkInit
    {
        public static void Init()
        {
            int NetworkDeviceID = 0;

            Console.WriteLine("Searching for Ethernet Controllers...");

            foreach (PCIDevice device in PCI.Devices)
            {
                if (device.ClassCode == 0x02 && device.Subclass == 0x00 && // is Ethernet Controller
                    device == PCI.GetDevice(device.bus, device.slot, device.function))
                {

                    Console.WriteLine("Found " + PCIDevice.DeviceClass.GetDeviceString(device) + " on PCI " + device.bus + ":" + device.slot + ":" + device.function);

                    #region PCNETII

                    if (device.VendorID == (ushort)VendorID.AMD && device.DeviceID == (ushort)DeviceID.PCNETII)
                    {

                        Console.WriteLine("NIC IRQ: " + device.InterruptLine);

                        var AMDPCNetIIDevice = new AMDPCNetII(device);

                        AMDPCNetIIDevice.NameID = "eth" + NetworkDeviceID;

                        Console.WriteLine("Registered at " + AMDPCNetIIDevice.NameID + " (" + AMDPCNetIIDevice.MACAddress.ToString() + ")");

                        AMDPCNetIIDevice.Enable();

                        NetworkDeviceID++;
                    }

                    #endregion
                    #region RTL8139

                    if (device.VendorID == 0x10EC && device.DeviceID == 0x8139)
                    {
                        var RTL8139Device = new RTL8139(device);

                        RTL8139Device.NameID = "eth" + NetworkDeviceID;

                        RTL8139Device.Enable();

                        NetworkDeviceID++;
                    }

                    #endregion
                    #region E1000

                    if (device.VendorID == 0x8086)
                    {
                        if (
                           device.DeviceID == (ushort)E1000DeviceID.Intel82542 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82543GC ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82543GC_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82544EI ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82544EI_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82543EI ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82544GC ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82540EM ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82545EM ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546EB ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82545EM_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546EB_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82541EI ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82541ER ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82540EM_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82540EP ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82540EP_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82541EI_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82547EI ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82547EI_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546EB_2 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82540EP_2 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82545GM ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82545GM_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82545GM_2 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82566MM_ICH8 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82566DM_ICH8 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82566DC_ICH8 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82562V_ICH8 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82566MC_ICH8 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82571EB ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82571EB_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82571EB_2 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82547EI_2 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82541GI ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82547EI_3 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82541ER_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546EB_3 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546EB_4 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546EB_5 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82541PI ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82572EI ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82572EI_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82572EI_2 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546GB ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82573E ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82573E_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel80003ES2LAN ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel80003ES2LAN_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546GB_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82573L ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82571EB_3 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82575 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82575_serdes ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82546GB_2 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82572EI_3 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel80003ES2LAN_2 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel80003ES2LAN_3 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82571EB_4 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82566DM_ICH9 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82562GT_ICH8 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82562G_ICH8 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82576 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82574L ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82575_quadcopper ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82567V_ICH9 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82567LM_4_ICH9 ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82577LM ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82577LC ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82578DM ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82578DC ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82567LM_ICH9_egDellE6400Notebook ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82579LM ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82579V ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82576NS ||
                           device.DeviceID == (ushort)E1000DeviceID.Intel82580 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI350 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI210 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI210_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI217LM ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI217VA ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI218V ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI218LM ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI218LM2 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI218V_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI218LM3 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI218V3 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI219LM ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI219V ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI219LM2 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI219V2 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI219LM3 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI219LM_1 ||
                           device.DeviceID == (ushort)E1000DeviceID.IntelI219LM_2
                           )
                        {
                            var E1000Device = new E1000(device);

                            E1000Device.NameID = ("eth" + NetworkDeviceID);

                            E1000Device.Enable();

                            NetworkDeviceID++;
                        }
                    }

                    #endregion
                }
            }

            if (NetworkDevice.Devices.Count == 0)
            {
                Console.WriteLine("No supported network card found!!");
            }
            else
            {
                Console.WriteLine("Network initialization done!");
            }
        }
    }
}
