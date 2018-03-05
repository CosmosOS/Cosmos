using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL.PCInformation
{
    /// <summary>
    /// Unknown needs to be always the last
    /// </summary>
    public enum ProcessorFlags
    {
            fpu,
            vme,
            de,
            pse,
            tsc,
            msr,
            pae,
            mce,
            cx8,
            apic,
            sep,
            mtrr,
            pge,
            mca,
            cmov,
            pat,
            pse36,
            psn,
            clfsh,
            ds,
            acpi,
            mmx,
            fxsr,
            sse,
            sse2,
            ss,
            htt,
            tm,
            ia64,
            pbe,
            sse3,
        pclmulqdq,
        dtes64,
        monitor,
        dscpl,
        vmx,
        smx,
        est,
        tm2,
        ssse3,
        cnxtd,
        sdbg,
        fma,
        cx16,
        xtpr,
        pdcm,
        pcid,
        dca,
        sse41,
        sse42,
        x2apic,
        movbe,
        popcnt,
        tscdeadline,
        aes,
        xsave,
        osxsave,
        avx,
        f16c,
        rdrnd,
        hypervisor,
        unknown
    }

    /// <summary>
    /// I really hate myself for doing this...
    /// You can't cast an Int to Enum (stack corruption)
    /// You can't do a list of enums (il2cpu error)
    /// You can't convert an enum to string (Object.ToString() not implemented)
    /// As you can see, it's nearly impossible to overcome the bugs
    /// So i did the simplest solution (and bruteforce) ever: a big switch.
    /// </summary>
    public static class ProcessorFlagsExtensions
    {
        public static ProcessorFlags ConvertIntToEnum(int flag)
        {
            switch (flag)
            {
                case (int)ProcessorFlags.fpu: return ProcessorFlags.fpu;
                case (int)ProcessorFlags.vme:
                    return ProcessorFlags.vme;
                case (int)ProcessorFlags.de:
                    return ProcessorFlags.de;
                case (int)ProcessorFlags.pse:
                    return ProcessorFlags.pse;
                case (int)ProcessorFlags.tsc:
                    return ProcessorFlags.tsc;
                case (int)ProcessorFlags.msr:
                    return ProcessorFlags.msr;
                case (int)ProcessorFlags.pae:
                    return ProcessorFlags.pae;
                case (int)ProcessorFlags.mce:
                    return ProcessorFlags.mce;
                case (int)ProcessorFlags.cx8:
                    return ProcessorFlags.cx8;
                case (int)ProcessorFlags.apic:
                    return ProcessorFlags.apic;
                case (int)ProcessorFlags.sep:
                    return ProcessorFlags.sep;
                case (int)ProcessorFlags.mtrr:
                    return ProcessorFlags.mtrr;
                case (int)ProcessorFlags.pge:
                    return ProcessorFlags.pge;
                case (int)ProcessorFlags.mca:
                    return ProcessorFlags.mca;
                case (int)ProcessorFlags.cmov:
                    return ProcessorFlags.cmov;
                case (int)ProcessorFlags.pat:
                    return ProcessorFlags.pat;
                case (int)ProcessorFlags.pse36:
                    return ProcessorFlags.pse36;
                case (int)ProcessorFlags.psn:
                    return ProcessorFlags.psn;
                case (int)ProcessorFlags.clfsh:
                    return ProcessorFlags.clfsh;
                case (int)ProcessorFlags.ds:
                    return ProcessorFlags.ds;
                case (int)ProcessorFlags.acpi:
                    return ProcessorFlags.acpi;
                case (int)ProcessorFlags.mmx:
                    return ProcessorFlags.mmx;
                case (int)ProcessorFlags.fxsr:
                    return ProcessorFlags.fxsr;
                case (int)ProcessorFlags.sse:
                    return ProcessorFlags.sse;
                case (int)ProcessorFlags.sse2:
                    return ProcessorFlags.sse2;
                case (int)ProcessorFlags.ss:
                    return ProcessorFlags.ss;
                case (int)ProcessorFlags.htt:
                    return ProcessorFlags.htt;
                case (int)ProcessorFlags.tm:
                    return ProcessorFlags.tm;
                case (int)ProcessorFlags.ia64:
                    return ProcessorFlags.ia64;
                case (int)ProcessorFlags.pbe:
                    return ProcessorFlags.pbe;
                case (int)ProcessorFlags.sse3:
                    return ProcessorFlags.sse3;
                case (int)ProcessorFlags.pclmulqdq:
                    return ProcessorFlags.pclmulqdq;
                case (int)ProcessorFlags.dtes64: return ProcessorFlags.dtes64;
                case (int)ProcessorFlags.monitor: return ProcessorFlags.monitor;
                case (int)ProcessorFlags.dscpl: return ProcessorFlags.dscpl;
                case (int)ProcessorFlags.vmx: return ProcessorFlags.vmx;
                case (int)ProcessorFlags.smx: return ProcessorFlags.smx;
                case (int)ProcessorFlags.est: return ProcessorFlags.est;
                case (int)ProcessorFlags.tm2: return ProcessorFlags.tm2;
                case (int)ProcessorFlags.ssse3: return ProcessorFlags.ssse3;
                case (int)ProcessorFlags.cnxtd: return ProcessorFlags.cnxtd;
                case (int)ProcessorFlags.sdbg: return ProcessorFlags.sdbg;
                case (int)ProcessorFlags.fma: return ProcessorFlags.fma;
                case (int)ProcessorFlags.cx16: return ProcessorFlags.cx16;
                case (int)ProcessorFlags.xtpr: return ProcessorFlags.xtpr;
                case (int)ProcessorFlags.pdcm: return ProcessorFlags.pdcm;
                case (int)ProcessorFlags.pcid: return ProcessorFlags.pcid;
                case (int)ProcessorFlags.dca: return ProcessorFlags.dca;
                case (int)ProcessorFlags.sse41: return ProcessorFlags.sse41;
                case (int)ProcessorFlags.sse42: return ProcessorFlags.sse42;
                case (int)ProcessorFlags.x2apic: return ProcessorFlags.x2apic;
                case (int)ProcessorFlags.movbe: return ProcessorFlags.movbe;
                case (int)ProcessorFlags.popcnt: return ProcessorFlags.popcnt;
                case (int)ProcessorFlags.tscdeadline: return ProcessorFlags.tscdeadline;
                case (int)ProcessorFlags.aes: return ProcessorFlags.aes;
                case (int)ProcessorFlags.xsave: return ProcessorFlags.xsave;
                case (int)ProcessorFlags.osxsave: return ProcessorFlags.osxsave;
                case (int)ProcessorFlags.avx: return ProcessorFlags.avx;
                case (int)ProcessorFlags.f16c: return ProcessorFlags.f16c;
                case (int)ProcessorFlags.rdrnd: return ProcessorFlags.rdrnd;
                case (int)ProcessorFlags.hypervisor: return ProcessorFlags.hypervisor;

                default:
                    return ProcessorFlags.unknown;
            }
        }

        public static string ConvertEnumToString(ProcessorFlags flag)
        {
            switch (flag)
            {
                case ProcessorFlags.fpu:
                    return "fpu";
                case ProcessorFlags.vme:
                    return "vme";
                case ProcessorFlags.de:
                    return "de";
                case ProcessorFlags.pse:
                    return "pse";
                case ProcessorFlags.tsc:
                    return "tsc";
                case ProcessorFlags.msr: return "msr";
                case ProcessorFlags.pae: return "pae";
                case ProcessorFlags.mce: return "mce";
                case ProcessorFlags.cx8: return "cx8";
                case ProcessorFlags.apic: return "apic";
                case ProcessorFlags.sep: return "sep";
                case ProcessorFlags.mtrr: return "mtrr";
                case ProcessorFlags.pge: return "pge";
                case ProcessorFlags.mca: return "mca";
                case ProcessorFlags.cmov: return "cmov";
                case ProcessorFlags.pat: return "pat";
                case ProcessorFlags.pse36: return "pse36";
                case ProcessorFlags.psn: return "psn";
                case ProcessorFlags.clfsh: return "clfsh";
                case ProcessorFlags.ds: return "ds";
                case ProcessorFlags.acpi: return "acpi";
                case ProcessorFlags.mmx: return "mmx";
                case ProcessorFlags.fxsr: return "fxsr";
                case ProcessorFlags.sse: return "sse";
                case ProcessorFlags.sse2: return "sse2";
                case ProcessorFlags.ss: return "ss";
                case ProcessorFlags.htt: return "htt";
                case ProcessorFlags.tm: return "tm";
                case ProcessorFlags.ia64: return "ia64";
                case ProcessorFlags.pbe: return "pbe";
                case ProcessorFlags.sse3: return "sse3";
                case ProcessorFlags.pclmulqdq: return "pclmulqdq";
                case ProcessorFlags.dtes64: return "dtes64";
                case ProcessorFlags.monitor: return "monitor";
                case ProcessorFlags.dscpl: return "dscpl";
                case ProcessorFlags.vmx: return "vmx";
                case ProcessorFlags.smx: return "smx";
                case ProcessorFlags.est: return "est";
                case ProcessorFlags.tm2: return "tm2";
                case ProcessorFlags.ssse3: return "ssse3";
                case ProcessorFlags.cnxtd: return "cnxtd";
                case ProcessorFlags.sdbg: return "sdbg";
                case ProcessorFlags.fma: return "fma";
                case ProcessorFlags.cx16: return "cx16";
                case ProcessorFlags.xtpr: return "xtpr";
                case ProcessorFlags.pdcm: return "pdcm";
                case ProcessorFlags.pcid: return "pcid";
                case ProcessorFlags.dca: return "dca";
                case ProcessorFlags.sse41: return "sse41";
                case ProcessorFlags.sse42: return "sse42";
                case ProcessorFlags.x2apic: return "x2apic";
                case ProcessorFlags.movbe: return "movbe";
                case ProcessorFlags.popcnt: return "popcnt";
                case ProcessorFlags.tscdeadline: return "tscdeadline";
                case ProcessorFlags.aes: return "aes";
                case ProcessorFlags.xsave: return "xsave";
                case ProcessorFlags.osxsave: return "osxsave";
                case ProcessorFlags.avx: return "avx";
                case ProcessorFlags.f16c: return "f16c";
                case ProcessorFlags.rdrnd: return "rdrnd";
                case ProcessorFlags.hypervisor: return "hypervisor";
                default:
                   
                    return "unknown";
            }
        }
    }
}
