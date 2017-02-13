using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL.PCInfo
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
            switch(flag)
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
                default:
                    return "unknown";
            }
        }
    }
}
