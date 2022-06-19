using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACPIAML.Interupter
{
    public class EisaId
    {
        public static string ToText(long ID)
        {
            var vendor = ID & 0xFFFF;
            var device = ID >> 16;
            var device1 = device & 0xFF;
            var device2 = device >> 8;
            var vendor_rev = ((vendor & 0xFF) << 8) | vendor >> 8;
            var vendor1 = ((vendor_rev >> 10)&0x1f)+64;
            var vendor2 = ((vendor_rev >> 5)&0x1f)+64;
            var vendor3= ((vendor_rev >> 0)&0x1f)+64;
            
            string vendorStr = new(new char[] { (char)vendor1 , (char)vendor2 , (char)vendor3 });
            return vendorStr + device1.ToString("X2") + device2.ToString("X2");
        }
    }
}
