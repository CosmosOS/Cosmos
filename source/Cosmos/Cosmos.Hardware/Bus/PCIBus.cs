using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Bus {
    public abstract class PCIBus : Bus {
        //TODO: Change to Dictionary<UInt32, string> when the IL2CPU bug is fixed
        public class DeviceID {
            UInt32 key;

            public UInt32 Key {
                get { return key; }
                set { key = value; }
            }

            String value;

            public String Value {
                get { return value; }
                set { this.value = value; }
            }

            public DeviceID(UInt32 pkey, String pvalue) {
                key = pkey;
                value = pvalue;
            }
        }

        //Dont make static. We dont want all the strings loaded in RAM
        // all the time.
        public class DeviceIDs {
            protected TempDictionary<String> mVendors = new TempDictionary<String>();

            public DeviceIDs() {
                // Current QEMU hardware
                mVendors.Add(0x8086, "Intel");
                // 1237  440FX - 82441FX PMC [Natoma]
	            // 7000  82371SB PIIX3 ISA [Natoma/Triton II]
    	        // 7010  82371SB PIIX3 IDE [Natoma/Triton II]
	            // 7113  82371AB/EB/MB PIIX4 ACPI
		        //    Need to check sub attr - maybe we have this:
                //    15ad 1976  Virtual Machine Chipset
                mVendors.Add(0x1013, "Cirrus Logic");
	            // 00b8  GD 5446
                mVendors.Add(0x10EC, "Realtek Semiconductor Co., Ltd.");
                //8139  RTL-8029(AS)
                    //0357 000a  TTP-Monitoring Card V2.0
                    //1025 005a  TravelMate 290
                    //1025 8920  ALN-325
                    //1025 8921  ALN-325
                    //103c 006a  NX9500
                    //1043 1045  L8400B or L3C/S notebook
                    //1043 8109  P5P800-MX Mainboard
                    //1071 8160  MIM2000
                    //10bd 0320  EP-320X-R
                    //10ec 8139  RT8139
                    //10f7 8338  Panasonic CF-Y5 laptop
                    //1113 ec01  FNC-0107TX
                    //1186 1300  DFE-538TX
                    //1186 1320  SN5200
                    //1186 8139  DRN-32TX
                    //11f6 8139  FN22-3(A) LinxPRO Ethernet Adapter
                    //1259 2500  AT-2500TX
                    //1259 2503  AT-2500TX/ACPI
                    //1429 d010  ND010
                    //1432 9130  EN-9130TX
                    //1436 8139  RT8139
                    //144d c00c  P30/P35 notebook
                    //1458 e000  GA-7VM400M/7VT600 Motherboard
                    //1462 788c  865PE Neo2-V Mainboard
                    //146c 1439  FE-1439TX
                    //1489 6001  GF100TXRII
                    //1489 6002  GF100TXRA
                    //149c 139a  LFE-8139ATX
                    //149c 8139  LFE-8139TX
                    //14cb 0200  LNR-100 Family 10/100 Base-TX Ethernet
                    //1565 2300  P4TSV Onboard LAN (RTL8100B)
                    //1695 9001  Onboard RTL8101L 10/100 MBit
                    //1799 5000  F5D5000 PCI Card/Desktop Network PCI Card
                    //1904 8139  RTL8139D Fast Ethernet Adapter
                    //2646 0001  EtheRx
                    //8e2e 7000  KF-230TX
                    //8e2e 7100  KF-230TX/2
                    //a0a0 0007  ALN-325C
            }

            public string FindVendor(UInt32 aVendorID) {
                return mVendors[aVendorID];
            }

            /*protected List<DeviceID> mVendors = new List<DeviceID>();

            public DeviceIDs()
            {
                mVendors.Add(new DeviceID(0x8086, "Intel"));
            }

            public string FindVendor(UInt32 aVendorID)
            {
                for (int i = 0; i < mVendors.Count; i++)
                {

                    if (mVendors[i].Key == aVendorID)
                        return mVendors[i].Value;
                }
                return null;
            }*/
        }
    }
}
