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
                mVendors.Add(0x8086, "Intel");
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
