using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware {

    public class PCIBus {
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

            public DeviceIDs()
            {
                // Current QEMU hardware
                mVendors.Add(0x8086, "Intel Corporation");
                // 1237  440FX - 82441FX PMC [Natoma]
                // 7000  82371SB PIIX3 ISA [Natoma/Triton II]
                // 7010  82371SB PIIX3 IDE [Natoma/Triton II]
                // 7113  82371AB/EB/MB PIIX4 ACPI
                //    Need to check sub attr - maybe we have this:
                //    15ad 1976  Virtual Machine Chipset
                mVendors.Add(0x1013, "Cirrus Logic");
                // 00b8  GD 5446
                mVendors.Add(0x10EC, "Realtek Semiconductor");
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

                //Found on ESX Server:
                mVendors.Add(0x15AD, "VMware Inc.");
                mVendors.Add(0x1000, "LSI Logic 53C810 Device");
                mVendors.Add(0x1022, "Advanced Micro Devices");

                #region PCIHelper

                //lines of this region are generated with PCIHelper

                // Source:  http://www.pcidatabase.com/vendors.php?sort=id


                //For now it is too slow to add all vendors
                /*
                mVendors.Add(0x0033, "Paradyne Corp.");
                mVendors.Add(0x003D, "master");
                mVendors.Add(0x0070, "Hauppauge Computer Works Inc.");
                mVendors.Add(0x0100, "Ncipher Corp. Ltd");
                mVendors.Add(0x0123, "General Dynamics");
                mVendors.Add(0x0315, "SK - Electronics Co., Ltd.");
                mVendors.Add(0x0A89, "BREA Technologies Inc.");
                mVendors.Add(0x0E11, "Compaq Computer Corp.");
                mVendors.Add(0x1000, "LSI Logic 53C810 Device");
                mVendors.Add(0x1001, "Kolter Electronic - Germany");
                mVendors.Add(0x1002, "ATI");
                mVendors.Add(0x1003, "ULSI");
                mVendors.Add(0x1004, "VLSI Technology");
                mVendors.Add(0x1006, "Reply Group");
                mVendors.Add(0x1007, "Netframe Systems Inc.");
                mVendors.Add(0x1008, "Epson");
                mVendors.Add(0x100A, "Phoenix Technologies Ltd.");
                mVendors.Add(0x100B, "National Semiconductors");
                mVendors.Add(0x100C, "Tseng Labs");
                mVendors.Add(0x100D, "AST Research");
                mVendors.Add(0x100E, "Weitek");
                mVendors.Add(0x1010, "Video Logic Ltd.");
                mVendors.Add(0x1011, "Digital Equipment Corporation");
                mVendors.Add(0x1012, "Micronics Computers Inc.");
                mVendors.Add(0x1013, "Cirrus Logic");
                mVendors.Add(0x1014, "International Business Machines Corp.");
                mVendors.Add(0x1016, "Fujitsu ICL Computers");
                mVendors.Add(0x1017, "Spea Software AG");
                mVendors.Add(0x1018, "Unisys Systems");
                mVendors.Add(0x1019, "Elitegroup Computer System");
                mVendors.Add(0x101A, "NCR Corporation");
                mVendors.Add(0x101B, "Vitesse Semiconductor");
                mVendors.Add(0x101E, "American Megatrends Inc.");
                mVendors.Add(0x101F, "PictureTel Corp.");
                mVendors.Add(0x1020, "Hitachi Computer Electronics");
                mVendors.Add(0x1021, "Oki Electric Industry");
                mVendors.Add(0x1022, "Advanced Micro Devices");
                mVendors.Add(0x1023, "TRIDENT MICRO");
                mVendors.Add(0x1025, "Acer Incorporated");
                mVendors.Add(0x1028, "Dell Computer Corporation");
                mVendors.Add(0x102A, "LSI Logic Headland Division");
                mVendors.Add(0x102B, "Matrox Electronic Systems Ltd.");
                mVendors.Add(0x102C, "Asiliant (Chips And Technologies)");
                mVendors.Add(0x102D, "Wyse Technologies");
                mVendors.Add(0x102E, "Olivetti Advanced Technology");
                mVendors.Add(0x102F, "Toshiba America");
                mVendors.Add(0x1030, "TMC Research");
                mVendors.Add(0x1031, "miro Computer Products AG");
                mVendors.Add(0x1033, "NEC Electronics");
                mVendors.Add(0x1034, "Burndy Corporation");
                mVendors.Add(0x1036, "Future Domain");
                mVendors.Add(0x1037, "Hitachi Micro Systems Inc");
                mVendors.Add(0x1038, "AMP Incorporated");
                mVendors.Add(0x1039, "Silicon Integrated Systems");
                mVendors.Add(0x103A, "Seiko Epson Corporation");
                mVendors.Add(0x103B, "Tatung Corp. Of America");
                mVendors.Add(0x103C, "Hewlett-Packard Company");
                mVendors.Add(0x103E, "Solliday Engineering");
                mVendors.Add(0x103F, "Logic Modeling");
                mVendors.Add(0x1041, "Computrend");
                mVendors.Add(0x1043, "Asustek Computer Inc.");
                mVendors.Add(0x1044, "Distributed Processing Tech");
                mVendors.Add(0x1045, "OPTi Inc.");
                mVendors.Add(0x1046, "IPC Corporation LTD");
                mVendors.Add(0x1047, "Genoa Systems Corp.");
                mVendors.Add(0x1048, "ELSA GmbH");
                mVendors.Add(0x1049, "Fountain Technology");
                mVendors.Add(0x104A, "STMicroelectronics");
                mVendors.Add(0x104B, "Mylex / Buslogic");
                mVendors.Add(0x104C, "Texas Instruments");
                mVendors.Add(0x104D, "Sony Corporation");
                mVendors.Add(0x104E, "Oak Technology");
                mVendors.Add(0x104F, "Co-Time Computer Ltd.");
                mVendors.Add(0x1050, "Winbond Electronics Corp.");
                mVendors.Add(0x1051, "Anigma Corp.");
                mVendors.Add(0x1053, "Young Micro Systems");
                mVendors.Add(0x1054, "Hitachi Ltd");
                mVendors.Add(0x1055, "Standard Microsystems Corp.");
                mVendors.Add(0x1056, "ICL");
                mVendors.Add(0x1057, "Motorola");
                mVendors.Add(0x1058, "Electronics & Telecommunication Res");
                mVendors.Add(0x1059, "Kontron Canada");
                mVendors.Add(0x105A, "Promise Technology");
                mVendors.Add(0x105B, "Foxconn International Inc.");
                mVendors.Add(0x105C, "Wipro Infotech Limited");
                mVendors.Add(0x105D, "Number Nine Visual Technology");
                mVendors.Add(0x105E, "Vtech Engineering Canada Ltd.");
                mVendors.Add(0x105F, "Infotronic America Inc.");
                mVendors.Add(0x1060, "United Microelectronics");
                mVendors.Add(0x1061, "8x8 Inc.");
                mVendors.Add(0x1062, "Maspar Computer Corp.");
                mVendors.Add(0x1063, "Ocean Office Automation");
                mVendors.Add(0x1064, "Alcatel Cit");
                mVendors.Add(0x1065, "Texas Microsystems");
                mVendors.Add(0x1066, "Picopower Technology");
                mVendors.Add(0x1067, "Mitsubishi Electronics");
                mVendors.Add(0x1068, "Diversified Technology");
                mVendors.Add(0x106A, "Aten Research Inc.");
                mVendors.Add(0x106B, "Apple Computer Inc.");
                mVendors.Add(0x106C, "Hyundai Electronics America");
                mVendors.Add(0x106D, "Sequent Computer Systems");
                mVendors.Add(0x106E, "DFI Inc.");
                mVendors.Add(0x106F, "City Gate Development LTD");
                mVendors.Add(0x1070, "Daewoo Telecom Ltd.");
                mVendors.Add(0x1071, "Mitac");
                mVendors.Add(0x1072, "GIT Co. Ltd.");
                mVendors.Add(0x1073, "Yamaha Corporation");
                mVendors.Add(0x1074, "Nexgen Microsystems");
                mVendors.Add(0x1075, "Advanced Integration Research");
                mVendors.Add(0x1077, "QLogic Corporation");
                mVendors.Add(0x1078, "Cyrix Corporation");
                mVendors.Add(0x1079, "I-Bus");
                mVendors.Add(0x107A, "Networth");
                mVendors.Add(0x107B, "Gateway 2000");
                mVendors.Add(0x107C, "Goldstar Co. Ltd.");
                mVendors.Add(0x107D, "Leadtek Research");
                mVendors.Add(0x107E, "Testernec");
                mVendors.Add(0x107F, "Data Technology Corporation");
                mVendors.Add(0x1080, "Cypress Semiconductor");
                mVendors.Add(0x1081, "Radius Inc.");
                mVendors.Add(0x1082, "EFA Corporation Of America");
                mVendors.Add(0x1083, "Forex Computer Corporation");
                mVendors.Add(0x1084, "Parador");
                mVendors.Add(0x1085, "Tulip Computers Int'l BV");
                mVendors.Add(0x1086, "J. Bond Computer Systems");
                mVendors.Add(0x1087, "Cache Computer");
                mVendors.Add(0x1088, "Microcomputer Systems (M) Son");
                mVendors.Add(0x1089, "Data General Corporation");
                mVendors.Add(0x108A, "SBS Operations");
                mVendors.Add(0x108C, "Oakleigh Systems Inc.");
                mVendors.Add(0x108D, "Olicom");
                mVendors.Add(0x108E, "Sun Microsystems");
                mVendors.Add(0x108F, "Systemsoft Corporation");
                mVendors.Add(0x1090, "Encore Computer Corporation");
                mVendors.Add(0x1091, "Intergraph Corporation");
                mVendors.Add(0x1092, "Diamond Computer Systems");
                mVendors.Add(0x1093, "National Instruments");
                mVendors.Add(0x1094, "First Int'l Computers");
                mVendors.Add(0x1095, "Silicon Image, Inc.");
                mVendors.Add(0x1096, "Alacron");
                mVendors.Add(0x1097, "Appian Graphics");
                mVendors.Add(0x1098, "Quantum Designs Ltd.");
                mVendors.Add(0x1099, "Samsung Electronics Co. Ltd.");
                mVendors.Add(0x109A, "Packard Bell");
                mVendors.Add(0x109B, "Gemlight Computer Ltd.");
                mVendors.Add(0x109C, "Megachips Corporation");
                mVendors.Add(0x109D, "Zida Technologies Ltd.");
                mVendors.Add(0x109E, "Brooktree Corporation");
                mVendors.Add(0x109F, "Trigem Computer Inc.");
                mVendors.Add(0x10A0, "Meidensha Corporation");
                mVendors.Add(0x10A1, "Juko Electronics Inc. Ltd.");
                mVendors.Add(0x10A2, "Quantum Corporation");
                mVendors.Add(0x10A3, "Everex Systems Inc.");
                mVendors.Add(0x10A4, "Globe Manufacturing Sales");
                mVendors.Add(0x10A5, "Racal Interlan");
                mVendors.Add(0x10A8, "Sierra Semiconductor");
                mVendors.Add(0x10A9, "Silicon Graphics");
                mVendors.Add(0x10AB, "Digicom");
                mVendors.Add(0x10AC, "Honeywell IASD");
                mVendors.Add(0x10AD, "Winbond Systems Labs");
                mVendors.Add(0x10AE, "Cornerstone Technology");
                mVendors.Add(0x10AF, "Micro Computer Systems Inc.");
                mVendors.Add(0x10B0, "CardExpert Technology");
                mVendors.Add(0x10B1, "Cabletron Systems Inc.");
                mVendors.Add(0x10B2, "Raytheon Company");
                mVendors.Add(0x10B3, "Databook Inc.");
                mVendors.Add(0x10B4, "STB Systems");
                mVendors.Add(0x10B5, "PLX Technology Inc.");
                mVendors.Add(0x10B6, "Madge Networks");
                mVendors.Add(0x10B7, "3Com Corporation");
                mVendors.Add(0x10B8, "Standard Microsystems Corporation");
                mVendors.Add(0x10B9, "Ali Corporation");
                mVendors.Add(0x10BA, "Mitsubishi Electronics Corp.");
                mVendors.Add(0x10BB, "Dapha Electronics Corporation");
                mVendors.Add(0x10BC, "Advanced Logic Research Inc.");
                mVendors.Add(0x10BD, "Surecom Technology");
                mVendors.Add(0x10BE, "Tsenglabs International Corp.");
                mVendors.Add(0x10BF, "MOST Corp.");
                mVendors.Add(0x10C0, "Boca Research Inc.");
                mVendors.Add(0x10C1, "ICM Corp. Ltd.");
                mVendors.Add(0x10C2, "Auspex Systems Inc.");
                mVendors.Add(0x10C3, "Samsung Semiconductors");
                mVendors.Add(0x10C4, "Award Software Int'l Inc.");
                mVendors.Add(0x10C5, "Xerox Corporation");
                mVendors.Add(0x10C6, "Rambus Inc.");
                mVendors.Add(0x10C8, "Neomagic Corporation");
                mVendors.Add(0x10C9, "Dataexpert Corporation");
                mVendors.Add(0x10CA, "Fujitsu Siemens");
                mVendors.Add(0x10CB, "Omron Corporation");
                mVendors.Add(0x10CD, "Advanced System Products");
                mVendors.Add(0x10CF, "Fujitsu Ltd.");
                mVendors.Add(0x10D1, "Future+ Systems");
                mVendors.Add(0x10D2, "Molex Incorporated");
                mVendors.Add(0x10D3, "Jabil Circuit Inc.");
                mVendors.Add(0x10D4, "Hualon Microelectronics");
                mVendors.Add(0x10D5, "Autologic Inc.");
                mVendors.Add(0x10D6, "Cetia");
                mVendors.Add(0x10D7, "BCM Advanced Research");
                mVendors.Add(0x10D8, "Advanced Peripherals Labs");
                mVendors.Add(0x10D9, "Macronix International Co. Ltd.");
                mVendors.Add(0x10DB, "Rohm Research");
                mVendors.Add(0x10DC, "CERN-European Lab. for Particle Physics");
                mVendors.Add(0x10DD, "Evans & Sutherland");
                mVendors.Add(0x10DE, "NVIDIA");
                mVendors.Add(0x10DF, "Emulex Corporation");
                mVendors.Add(0x10E1, "Tekram Technology Corp. Ltd.");
                mVendors.Add(0x10E2, "Aptix Corporation");
                mVendors.Add(0x10E3, "Tundra Semiconductor Corp.");
                mVendors.Add(0x10E4, "Tandem Computers");
                mVendors.Add(0x10E5, "Micro Industries Corporation");
                mVendors.Add(0x10E6, "Gainbery Computer Products Inc.");
                mVendors.Add(0x10E7, "Vadem");
                mVendors.Add(0x10E8, "Applied Micro Circuits Corp.");
                mVendors.Add(0x10E9, "Alps Electronic Corp. Ltd.");
                mVendors.Add(0x10EA, "Tvia, Inc.");
                mVendors.Add(0x10EB, "Artist Graphics");
                mVendors.Add(0x10EC, "Realtek Semiconductor");
                mVendors.Add(0x10ED, "Ascii Corporation");
                mVendors.Add(0x10EE, "Xilinx Corporation");
                mVendors.Add(0x10EF, "Racore Computer Products");
                mVendors.Add(0x10F0, "Curtiss-Wright Controls Embedded Computing");
                mVendors.Add(0x10F1, "Tyan Computer");
                mVendors.Add(0x10F2, "Achme Computer Inc. - GONE !!!!");
                mVendors.Add(0x10F3, "Alaris Inc.");
                mVendors.Add(0x10F4, "S-Mos Systems");
                mVendors.Add(0x10F5, "NKK Corporation");
                mVendors.Add(0x10F6, "Creative Electronic Systems SA");
                mVendors.Add(0x10F7, "Matsushita Electric Industrial Corp.");
                mVendors.Add(0x10F8, "Altos India Ltd.");
                mVendors.Add(0x10F9, "PC Direct");
                mVendors.Add(0x10FA, "Truevision");
                mVendors.Add(0x10FB, "Thesys Microelectronic's");
                mVendors.Add(0x10FC, "I-O Data Device Inc.");
                mVendors.Add(0x10FD, "Soyo Technology Corp. Ltd.");
                mVendors.Add(0x10FE, "Fast Electronic GmbH");
                mVendors.Add(0x10FF, "Ncube");
                mVendors.Add(0x1100, "Jazz Multimedia");
                mVendors.Add(0x1101, "Initio Corporation");
                mVendors.Add(0x1102, "Creative Technology LTD.");
                mVendors.Add(0x1103, "Triones Technologies Inc. (HighPoint)");
                mVendors.Add(0x1104, "Rasterops");
                mVendors.Add(0x1105, "Sigma Designs Inc.");
                mVendors.Add(0x1106, "VIA Technology");
                mVendors.Add(0x1107, "Stratus Computer");
                mVendors.Add(0x1108, "Proteon Inc.");
                mVendors.Add(0x1109, "Adaptec/Cogent Data Technologies");
                mVendors.Add(0x110A, "Siemens Nixdorf AG");
                mVendors.Add(0x110B, "Chromatic Research Inc");
                mVendors.Add(0x110C, "Mini-Max Technology Inc.");
                mVendors.Add(0x110D, "ZNYX Corporation");
                mVendors.Add(0x110E, "CPU Technology");
                mVendors.Add(0x110F, "Ross Technology");
                mVendors.Add(0x1112, "Osicom Technologies Inc.");
                mVendors.Add(0x1113, "Accton Technology Corporation");
                mVendors.Add(0x1114, "Atmel Corp.");
                mVendors.Add(0x1116, "Data Translation, Inc.");
                mVendors.Add(0x1117, "Datacube Inc.");
                mVendors.Add(0x1118, "Berg Electronics");
                mVendors.Add(0x1119, "ICP vortex Computersysteme GmbH");
                mVendors.Add(0x111A, "Efficent Networks");
                mVendors.Add(0x111C, "Tricord Systems Inc.");
                mVendors.Add(0x111D, "Integrated Device Technology Inc.");
                mVendors.Add(0x111F, "Precision Digital Images");
                mVendors.Add(0x1120, "EMC Corp.");
                mVendors.Add(0x1121, "Zilog");
                mVendors.Add(0x1123, "Excellent Design Inc.");
                mVendors.Add(0x1124, "Leutron Vision AG");
                mVendors.Add(0x1125, "Eurocore/Vigra");
                mVendors.Add(0x1127, "FORE Systems");
                mVendors.Add(0x1129, "Firmworks");
                mVendors.Add(0x112A, "Hermes Electronics Co. Ltd.");
                mVendors.Add(0x112C, "Zenith Data Systems");
                mVendors.Add(0x112D, "Ravicad");
                mVendors.Add(0x112E, "Infomedia");
                mVendors.Add(0x1130, "Computervision");
                mVendors.Add(0x1131, "Philips Semiconductors");
                mVendors.Add(0x1132, "Mitel Corp.");
                mVendors.Add(0x1133, "Eicon Networks Corporation");
                mVendors.Add(0x1134, "Mercury Computer Systems Inc.");
                mVendors.Add(0x1135, "Fuji Xerox Co Ltd");
                mVendors.Add(0x1136, "Momentum Data Systems");
                mVendors.Add(0x1137, "Cisco Systems Inc");
                mVendors.Add(0x1138, "Ziatech Corporation");
                mVendors.Add(0x1139, "Dynamic Pictures Inc");
                mVendors.Add(0x113A, "FWB Inc");
                mVendors.Add(0x113B, "Network Computing Devices");
                mVendors.Add(0x113C, "Cyclone Microsystems Inc.");
                mVendors.Add(0x113D, "Leading Edge Products Inc");
                mVendors.Add(0x113E, "Sanyo Electric Co");
                mVendors.Add(0x113F, "Equinox Systems");
                mVendors.Add(0x1140, "Intervoice Inc");
                mVendors.Add(0x1141, "Crest Microsystem Inc");
                mVendors.Add(0x1142, "Alliance Semiconductor");
                mVendors.Add(0x1143, "Netpower Inc");
                mVendors.Add(0x1144, "Cincinnati Milacron");
                mVendors.Add(0x1145, "Workbit Corp");
                mVendors.Add(0x1146, "Force Computers");
                mVendors.Add(0x1147, "Interface Corp");
                mVendors.Add(0x1148, "Marvell Semiconductor Germany GmbH");
                mVendors.Add(0x1149, "Win System Corporation");
                mVendors.Add(0x114A, "VMIC");
                mVendors.Add(0x114B, "Canopus corporation");
                mVendors.Add(0x114C, "Annabooks");
                mVendors.Add(0x114D, "IC Corporation");
                mVendors.Add(0x114E, "Nikon Systems Inc");
                mVendors.Add(0x114F, "Digi International");
                mVendors.Add(0x1150, "Thinking Machines Corporation");
                mVendors.Add(0x1151, "JAE Electronics Inc.");
                mVendors.Add(0x1153, "Land Win Electronic Corp");
                mVendors.Add(0x1154, "Melco Inc");
                mVendors.Add(0x1155, "Pine Technology Ltd");
                mVendors.Add(0x1156, "Periscope Engineering");
                mVendors.Add(0x1157, "Avsys Corporation");
                mVendors.Add(0x1158, "Voarx R&D Inc");
                mVendors.Add(0x1159, "Mutech");
                mVendors.Add(0x115A, "Harlequin Ltd");
                mVendors.Add(0x115B, "Parallax Graphics");
                mVendors.Add(0x115C, "Photron Ltd.");
                mVendors.Add(0x115D, "Xircom");
                mVendors.Add(0x115E, "Peer Protocols Inc");
                mVendors.Add(0x115F, "Maxtor Corporation");
                mVendors.Add(0x1160, "Megasoft Inc");
                mVendors.Add(0x1161, "PFU Ltd");
                mVendors.Add(0x1162, "OA Laboratory Co Ltd");
                mVendors.Add(0x1163, "Rendition Inc");
                mVendors.Add(0x1164, "Advanced Peripherals Tech");
                mVendors.Add(0x1165, "Imagraph Corporation");
                mVendors.Add(0x1166, "Broadcom / ServerWorks");
                mVendors.Add(0x1167, "Mutoh Industries Inc");
                mVendors.Add(0x1168, "Thine Electronics Inc");
                mVendors.Add(0x1169, "Centre f/Dev. of Adv. Computing");
                mVendors.Add(0x116A, "Polaris Communications");
                mVendors.Add(0x116B, "Connectware Inc");
                mVendors.Add(0x116C, "Intelligent Resources");
                mVendors.Add(0x116E, "Electronics for Imaging");
                mVendors.Add(0x1170, "Inventec Corporation");
                mVendors.Add(0x1172, "Altera Corporation");
                mVendors.Add(0x1173, "Adobe Systems");
                mVendors.Add(0x1174, "Bridgeport Machines");
                mVendors.Add(0x1175, "Mitron Computer Inc.");
                mVendors.Add(0x1176, "SBE");
                mVendors.Add(0x1177, "Silicon Engineering");
                mVendors.Add(0x1178, "Alfa Inc");
                mVendors.Add(0x1179, "Toshiba America Info Systems");
                mVendors.Add(0x117A, "A-Trend Technology");
                mVendors.Add(0x117B, "LG (Lucky Goldstar) Electronics Inc.");
                mVendors.Add(0x117C, "Atto Technology");
                mVendors.Add(0x117D, "Becton & Dickinson");
                mVendors.Add(0x117E, "T/R Systems");
                mVendors.Add(0x117F, "Integrated Circuit Systems");
                mVendors.Add(0x1180, "Ricoh Company, Ltd.");
                mVendors.Add(0x1183, "Fujikura Ltd");
                mVendors.Add(0x1184, "Forks Inc");
                mVendors.Add(0x1185, "Dataworld");
                mVendors.Add(0x1186, "D-Link System Inc");
                mVendors.Add(0x1187, "Advanced Technology Laboratories");
                mVendors.Add(0x1188, "Shima Seiki Manufacturing Ltd.");
                mVendors.Add(0x1189, "Matsushita Electronics");
                mVendors.Add(0x118A, "Hilevel Technology");
                mVendors.Add(0x118B, "Hypertec Pty Ltd");
                mVendors.Add(0x118C, "Corollary Inc");
                mVendors.Add(0x118D, "BitFlow Inc");
                mVendors.Add(0x118E, "Hermstedt AG");
                mVendors.Add(0x118F, "Green Logic");
                mVendors.Add(0x1190, "Tripace");
                mVendors.Add(0x1191, "Acard Technology Corp.");
                mVendors.Add(0x1192, "Densan Co. Ltd");
                mVendors.Add(0x1194, "Toucan Technology");
                mVendors.Add(0x1195, "Ratoc System Inc");
                mVendors.Add(0x1196, "Hytec Electronics Ltd");
                mVendors.Add(0x1197, "Gage Applied Technologies");
                mVendors.Add(0x1198, "Lambda Systems Inc");
                mVendors.Add(0x1199, "Attachmate Corp.");
                mVendors.Add(0x119A, "Mind/Share Inc.");
                mVendors.Add(0x119B, "Omega Micro Inc.");
                mVendors.Add(0x119C, "Information Technology Inst.");
                mVendors.Add(0x119D, "Bug Sapporo Japan");
                mVendors.Add(0x119E, "Fujitsu Microelectronics Ltd.");
                mVendors.Add(0x119F, "Bull Hn Information Systems");
                mVendors.Add(0x11A1, "Hamamatsu Photonics K.K.");
                mVendors.Add(0x11A2, "Sierra Research and Technology");
                mVendors.Add(0x11A3, "Deuretzbacher GmbH & Co. Eng. KG");
                mVendors.Add(0x11A4, "Barco");
                mVendors.Add(0x11A5, "MicroUnity Systems Engineering Inc.");
                mVendors.Add(0x11A6, "Pure Data");
                mVendors.Add(0x11A7, "Power Computing Corp.");
                mVendors.Add(0x11A8, "Systech Corp.");
                mVendors.Add(0x11A9, "InnoSys Inc.");
                mVendors.Add(0x11AA, "Actel");
                mVendors.Add(0x11AB, "Marvell Semiconductor");
                mVendors.Add(0x11AC, "Canon Information Systems");
                mVendors.Add(0x11AD, "Lite-On Technology Corp.");
                mVendors.Add(0x11AE, "Scitex Corporation Ltd");
                mVendors.Add(0x11AF, "Avid Technology Inc.");
                mVendors.Add(0x11B0, "Quicklogic Corp");
                mVendors.Add(0x11B1, "Apricot Computers");
                mVendors.Add(0x11B2, "Eastman Kodak");
                mVendors.Add(0x11B3, "Barr Systems Inc.");
                mVendors.Add(0x11B4, "Leitch Technology International");
                mVendors.Add(0x11B5, "Radstone Technology Ltd.");
                mVendors.Add(0x11B6, "United Video Corp");
                mVendors.Add(0x11B7, "Motorola");
                mVendors.Add(0x11B8, "Xpoint Technologies Inc");
                mVendors.Add(0x11B9, "Pathlight Technology Inc.");
                mVendors.Add(0x11BA, "Videotron Corp");
                mVendors.Add(0x11BB, "Pyramid Technology");
                mVendors.Add(0x11BC, "Network Peripherals Inc");
                mVendors.Add(0x11BD, "Pinnacle Systems Inc.");
                mVendors.Add(0x11BE, "International Microcircuits Inc");
                mVendors.Add(0x11BF, "Astrodesign Inc.");
                mVendors.Add(0x11C1, "Agere Systems");
                mVendors.Add(0x11C2, "Sand Microelectronics");
                mVendors.Add(0x11C4, "Document Technologies Ind.");
                mVendors.Add(0x11C5, "Shiva Corporatin");
                mVendors.Add(0x11C6, "Dainippon Screen Mfg. Co");
                mVendors.Add(0x11C7, "D.C.M. Data Systems");
                mVendors.Add(0x11C8, "Dolphin Interconnect Solutions");
                mVendors.Add(0x11C9, "MAGMA");
                mVendors.Add(0x11CA, "LSI Systems Inc");
                mVendors.Add(0x11CB, "Specialix International Ltd.");
                mVendors.Add(0x11CC, "Michels & Kleberhoff Computer GmbH");
                mVendors.Add(0x11CD, "HAL Computer Systems Inc.");
                mVendors.Add(0x11CE, "Primary Rate Inc");
                mVendors.Add(0x11CF, "Pioneer Electronic Corporation");
                mVendors.Add(0x11D0, "BAE SYSTEMS - Manassas");
                mVendors.Add(0x11D1, "AuraVision Corporation");
                mVendors.Add(0x11D2, "Intercom Inc.");
                mVendors.Add(0x11D3, "Trancell Systems Inc");
                mVendors.Add(0x11D4, "Analog Devices, Inc.");
                mVendors.Add(0x11D5, "Tahoma Technology");
                mVendors.Add(0x11D6, "Tekelec Technologies");
                mVendors.Add(0x11D7, "TRENTON Technology, Inc.");
                mVendors.Add(0x11D8, "Image Technologies Development");
                mVendors.Add(0x11D9, "Tec Corporation");
                mVendors.Add(0x11DA, "Novell");
                mVendors.Add(0x11DB, "Sega Enterprises Ltd");
                mVendors.Add(0x11DC, "Questra Corp");
                mVendors.Add(0x11DD, "Crosfield Electronics Ltd");
                mVendors.Add(0x11DE, "Zoran Corporation");
                mVendors.Add(0x11E1, "Gec Plessey Semi Inc");
                mVendors.Add(0x11E2, "Samsung Information Systems America");
                mVendors.Add(0x11E3, "Quicklogic Corp");
                mVendors.Add(0x11E4, "Second Wave Inc");
                mVendors.Add(0x11E5, "IIX Consulting");
                mVendors.Add(0x11E6, "Mitsui-Zosen System Research");
                mVendors.Add(0x11E8, "Digital Processing Systems Inc");
                mVendors.Add(0x11E9, "Highwater Designs Ltd");
                mVendors.Add(0x11EA, "Elsag Bailey");
                mVendors.Add(0x11EB, "Formation, Inc");
                mVendors.Add(0x11EC, "Coreco Inc");
                mVendors.Add(0x11ED, "Mediamatics");
                mVendors.Add(0x11EE, "Dome Imaging Systems Inc");
                mVendors.Add(0x11EF, "Nicolet Technologies BV");
                mVendors.Add(0x11F0, "Compu-Shack GmbH");
                mVendors.Add(0x11F2, "Picture Tel Japan KK");
                mVendors.Add(0x11F3, "Keithley Metrabyte");
                mVendors.Add(0x11F4, "Kinetic Systems Corporation");
                mVendors.Add(0x11F5, "Computing Devices Intl");
                mVendors.Add(0x11F6, "Powermatic Data Systems Ltd");
                mVendors.Add(0x11F7, "Scientific Atlanta");
                mVendors.Add(0x11F8, "PMC-Sierra Inc.");
                mVendors.Add(0x11F9, "I-Cube Inc");
                mVendors.Add(0x11FA, "Kasan Electronics Co Ltd");
                mVendors.Add(0x11FB, "Datel Inc");
                mVendors.Add(0x11FD, "High Street Consultants");
                mVendors.Add(0x11FE, "Comtrol Corp");
                mVendors.Add(0x11FF, "Scion Corp");
                mVendors.Add(0x1200, "CSS Corp");
                mVendors.Add(0x1201, "Vista Controls Corp");
                mVendors.Add(0x1202, "Network General Corp");
                mVendors.Add(0x1203, "Bayer Corporation Agfa Div");
                mVendors.Add(0x1204, "Lattice Semiconductor Corp");
                mVendors.Add(0x1205, "Array Corp");
                mVendors.Add(0x1206, "Amdahl Corp");
                mVendors.Add(0x1208, "Parsytec GmbH");
                mVendors.Add(0x1209, "Sci Systems Inc");
                mVendors.Add(0x120A, "Synaptel");
                mVendors.Add(0x120B, "Adaptive Solutions");
                mVendors.Add(0x120D, "Compression Labs Inc.");
                mVendors.Add(0x120E, "Cyclades Corporation");
                mVendors.Add(0x120F, "Essential Communications");
                mVendors.Add(0x1210, "Hyperparallel Technologies");
                mVendors.Add(0x1211, "Braintech Inc");
                mVendors.Add(0x1213, "Applied Intelligent Systems Inc");
                mVendors.Add(0x1214, "Performance Technologies Inc");
                mVendors.Add(0x1215, "Interware Co Ltd");
                mVendors.Add(0x1216, "Purup-Eskofot A/S");
                mVendors.Add(0x1217, "O2Micro Inc");
                mVendors.Add(0x1218, "Hybricon Corp");
                mVendors.Add(0x1219, "First Virtual Corp");
                mVendors.Add(0x121A, "3dfx Interactive Inc");
                mVendors.Add(0x121B, "Advanced Telecommunications Modules");
                mVendors.Add(0x121C, "Nippon Texa Co Ltd");
                mVendors.Add(0x121D, "Lippert Automationstechnik GmbH");
                mVendors.Add(0x121E, "CSPI");
                mVendors.Add(0x121F, "Arcus Technology Inc");
                mVendors.Add(0x1220, "Ariel Corporation");
                mVendors.Add(0x1221, "Contec Microelectronics Europe BV");
                mVendors.Add(0x1222, "Ancor Communications Inc");
                mVendors.Add(0x1223, "Emerson Network Power, Embedded Computing");
                mVendors.Add(0x1224, "Interactive Images");
                mVendors.Add(0x1225, "Power I/O Inc.");
                mVendors.Add(0x1227, "Tech-Source");
                mVendors.Add(0x1228, "Norsk Elektro Optikk A/S");
                mVendors.Add(0x1229, "Data Kinesis Inc.");
                mVendors.Add(0x122A, "Integrated Telecom");
                mVendors.Add(0x122B, "LG Industrial Systems Co. Ltd.");
                mVendors.Add(0x122C, "sci-worx GmbH");
                mVendors.Add(0x122D, "Aztech System Ltd");
                mVendors.Add(0x122E, "Xyratex");
                mVendors.Add(0x122F, "Andrew Corp.");
                mVendors.Add(0x1230, "Fishcamp Engineering");
                mVendors.Add(0x1231, "Woodward McCoach Inc.");
                mVendors.Add(0x1233, "Bus-Tech Inc.");
                mVendors.Add(0x1234, "Technical Corp");
                mVendors.Add(0x1236, "Sigma Designs, Inc");
                mVendors.Add(0x1237, "Alta Technology Corp.");
                mVendors.Add(0x1238, "Adtran");
                mVendors.Add(0x1239, "The 3DO Company");
                mVendors.Add(0x123A, "Visicom Laboratories Inc.");
                mVendors.Add(0x123B, "Seeq Technology Inc.");
                mVendors.Add(0x123C, "Century Systems Inc.");
                mVendors.Add(0x123D, "Engineering Design Team Inc.");
                mVendors.Add(0x123F, "C-Cube Microsystems");
                mVendors.Add(0x1240, "Marathon Technologies Corp.");
                mVendors.Add(0x1241, "DSC Communications");
                mVendors.Add(0x1242, "JNI Corporation");
                mVendors.Add(0x1243, "Delphax");
                mVendors.Add(0x1244, "AVM AUDIOVISUELLES MKTG & Computer GmbH");
                mVendors.Add(0x1245, "APD S.A.");
                mVendors.Add(0x1246, "Dipix Technologies Inc");
                mVendors.Add(0x1247, "Xylon Research Inc.");
                mVendors.Add(0x1248, "Central Data Corp.");
                mVendors.Add(0x1249, "Samsung Electronics Co. Ltd.");
                mVendors.Add(0x124A, "AEG Electrocom GmbH");
                mVendors.Add(0x124C, "Solitron Technologies Inc.");
                mVendors.Add(0x124D, "Stallion Technologies");
                mVendors.Add(0x124E, "Cylink");
                mVendors.Add(0x124F, "Infortrend Technology Inc");
                mVendors.Add(0x1250, "Hitachi Microcomputer System Ltd.");
                mVendors.Add(0x1251, "VLSI Solution OY");
                mVendors.Add(0x1253, "Guzik Technical Enterprises");
                mVendors.Add(0x1254, "Linear Systems Ltd.");
                mVendors.Add(0x1255, "Optibase Ltd.");
                mVendors.Add(0x1256, "Perceptive Solutions Inc.");
                mVendors.Add(0x1257, "Vertex Networks Inc.");
                mVendors.Add(0x1258, "Gilbarco Inc.");
                mVendors.Add(0x1259, "Allied Telesyn International");
                mVendors.Add(0x125A, "ABB Power Systems");
                mVendors.Add(0x125B, "Asix Electronics Corp.");
                mVendors.Add(0x125C, "Aurora Technologies Inc.");
                mVendors.Add(0x125D, "ESS Technology");
                mVendors.Add(0x125E, "Specialvideo Engineering SRL");
                mVendors.Add(0x125F, "Concurrent Technologies Inc.");
                mVendors.Add(0x1260, "Intersil Corporation");
                mVendors.Add(0x1261, "Matsushita-Kotobuki Electronics Indu");
                mVendors.Add(0x1262, "ES Computer Co. Ltd.");
                mVendors.Add(0x1263, "Sonic Solutions");
                mVendors.Add(0x1264, "Aval Nagasaki Corp.");
                mVendors.Add(0x1265, "Casio Computer Co. Ltd.");
                mVendors.Add(0x1266, "Microdyne Corp.");
                mVendors.Add(0x1267, "S.A. Telecommunications");
                mVendors.Add(0x1268, "Tektronix");
                mVendors.Add(0x1269, "Thomson-CSF/TTM");
                mVendors.Add(0x126A, "Lexmark International Inc.");
                mVendors.Add(0x126B, "Adax Inc.");
                mVendors.Add(0x126C, "Nortel Networks Corp.");
                mVendors.Add(0x126D, "Splash Technology Inc.");
                mVendors.Add(0x126E, "Sumitomo Metal Industries Ltd.");
                mVendors.Add(0x126F, "Silicon Motion");
                mVendors.Add(0x1270, "Olympus Optical Co. Ltd.");
                mVendors.Add(0x1271, "GW Instruments");
                mVendors.Add(0x1272, "Telematics International");
                mVendors.Add(0x1273, "Hughes Network Systems");
                mVendors.Add(0x1274, "Ensoniq");
                mVendors.Add(0x1275, "Network Appliance");
                mVendors.Add(0x1276, "Switched Network Technologies Inc.");
                mVendors.Add(0x1277, "Comstream");
                mVendors.Add(0x1278, "Transtech Parallel Systems");
                mVendors.Add(0x1279, "Transmeta Corp.");
                mVendors.Add(0x127B, "Pixera Corp");
                mVendors.Add(0x127C, "Crosspoint Solutions Inc.");
                mVendors.Add(0x127D, "Vela Research LP");
                mVendors.Add(0x127E, "Winnov L.P.");
                mVendors.Add(0x127F, "Fujifilm");
                mVendors.Add(0x1280, "Photoscript Group Ltd.");
                mVendors.Add(0x1281, "Yokogawa Electronic Corp.");
                mVendors.Add(0x1282, "Davicom Semiconductor Inc.");
                mVendors.Add(0x1283, "chandresh.j.mehta");
                mVendors.Add(0x1285, "Platform Technologies Inc.");
                mVendors.Add(0x1286, "MAZeT GmbH");
                mVendors.Add(0x1287, "LuxSonor Inc.");
                mVendors.Add(0x1288, "Timestep Corp.");
                mVendors.Add(0x1289, "AVC Technology Inc.");
                mVendors.Add(0x128A, "Asante Technologies Inc.");
                mVendors.Add(0x128B, "Transwitch Corp.");
                mVendors.Add(0x128C, "Retix Corp.");
                mVendors.Add(0x128D, "G2 Networks Inc.");
                mVendors.Add(0x128F, "Tateno Dennou Inc.");
                mVendors.Add(0x1290, "Sord Computer Corp.");
                mVendors.Add(0x1291, "NCS Computer Italia");
                mVendors.Add(0x1292, "Tritech Microelectronics Intl PTE");
                mVendors.Add(0x1293, "Media Reality Technology");
                mVendors.Add(0x1294, "Rhetorex Inc.");
                mVendors.Add(0x1295, "Imagenation Corp.");
                mVendors.Add(0x1296, "Kofax Image Products");
                mVendors.Add(0x1297, "Shuttle Computer");
                mVendors.Add(0x1298, "Spellcaster Telecommunications Inc.");
                mVendors.Add(0x1299, "Knowledge Technology Laboratories");
                mVendors.Add(0x129A, "VMETRO Inc.");
                mVendors.Add(0x129B, "Image Access");
                mVendors.Add(0x129D, "CompCore Multimedia Inc.");
                mVendors.Add(0x129E, "Victor Co. of Japan Ltd.");
                mVendors.Add(0x129F, "OEC Medical Systems Inc.");
                mVendors.Add(0x12A0, "Allen Bradley Co.");
                mVendors.Add(0x12A1, "Simpact Inc");
                mVendors.Add(0x12A2, "NewGen Systems Corp.");
                mVendors.Add(0x12A3, "Lucent Technologies AMR");
                mVendors.Add(0x12A4, "NTT Electronics Technology Co.");
                mVendors.Add(0x12A5, "Vision Dynamics Ltd.");
                mVendors.Add(0x12A6, "Scalable Networks Inc.");
                mVendors.Add(0x12A7, "AMO GmbH");
                mVendors.Add(0x12A8, "News Datacom");
                mVendors.Add(0x12A9, "Xiotech Corp.");
                mVendors.Add(0x12AA, "SDL Communications Inc.");
                mVendors.Add(0x12AB, "Yuan Yuan Enterprise Co. Ltd.");
                mVendors.Add(0x12AC, "MeasureX Corp.");
                mVendors.Add(0x12AD, "Multidata GmbH");
                mVendors.Add(0x12AE, "Alteon Networks Inc.");
                mVendors.Add(0x12AF, "TDK USA Corp.");
                mVendors.Add(0x12B0, "Jorge Scientific Corp.");
                mVendors.Add(0x12B1, "GammaLink");
                mVendors.Add(0x12B2, "General Signal Networks");
                mVendors.Add(0x12B3, "Inter-Face Co. Ltd.");
                mVendors.Add(0x12B4, "Future Tel Inc.");
                mVendors.Add(0x12B5, "Granite Systems Inc.");
                mVendors.Add(0x12B7, "Acumen");
                mVendors.Add(0x12B8, "Korg");
                mVendors.Add(0x12B9, "3Com Corporation");
                mVendors.Add(0x12BA, "Bittware, Inc");
                mVendors.Add(0x12BB, "Nippon Unisoft Corp.");
                mVendors.Add(0x12BC, "Array Microsystems");
                mVendors.Add(0x12BD, "Computerm Corp.");
                mVendors.Add(0x12BF, "Fujifilm Microdevices");
                mVendors.Add(0x12C0, "Infimed");
                mVendors.Add(0x12C1, "GMM Research Corp.");
                mVendors.Add(0x12C2, "Mentec Ltd.");
                mVendors.Add(0x12C3, "Holtek Microelectronics Inc.");
                mVendors.Add(0x12C4, "Connect Tech Inc.");
                mVendors.Add(0x12C5, "Picture Elements Inc.");
                mVendors.Add(0x12C6, "Mitani Corp.");
                mVendors.Add(0x12C7, "Dialogic Corp.");
                mVendors.Add(0x12C8, "G Force Co. Ltd.");
                mVendors.Add(0x12C9, "Gigi Operations");
                mVendors.Add(0x12CA, "Integrated Computing Engines, Inc.");
                mVendors.Add(0x12CB, "Antex Electronics Corp.");
                mVendors.Add(0x12CC, "Pluto Technologies International");
                mVendors.Add(0x12CD, "Aims Lab");
                mVendors.Add(0x12CE, "Netspeed Inc.");
                mVendors.Add(0x12CF, "Prophet Systems Inc.");
                mVendors.Add(0x12D0, "GDE Systems Inc.");
                mVendors.Add(0x12D1, "PsiTech");
                mVendors.Add(0x12D3, "Vingmed Sound A/S");
                mVendors.Add(0x12D4, "Ulticom, Inc.");
                mVendors.Add(0x12D5, "Equator Technologies");
                mVendors.Add(0x12D6, "Analogic Corp.");
                mVendors.Add(0x12D7, "Biotronic SRL");
                mVendors.Add(0x12D8, "Pericom Semiconductor");
                mVendors.Add(0x12D9, "Aculab Plc.");
                mVendors.Add(0x12DA, "TrueTime");
                mVendors.Add(0x12DB, "Annapolis Micro Systems Inc.");
                mVendors.Add(0x12DC, "Symicron Computer Communication Ltd.");
                mVendors.Add(0x12DD, "Management Graphics Inc.");
                mVendors.Add(0x12DE, "Rainbow Technologies");
                mVendors.Add(0x12DF, "SBS Technologies Inc.");
                mVendors.Add(0x12E0, "Chase Research PLC");
                mVendors.Add(0x12E1, "Nintendo Co. Ltd.");
                mVendors.Add(0x12E2, "Datum Inc. Bancomm-Timing Division");
                mVendors.Add(0x12E3, "Imation Corp. - Medical Imaging Syst");
                mVendors.Add(0x12E4, "Brooktrout Technology Inc.");
                mVendors.Add(0x12E6, "Cirel Systems");
                mVendors.Add(0x12E7, "Sebring Systems Inc");
                mVendors.Add(0x12E8, "CRISC Corp.");
                mVendors.Add(0x12E9, "GE Spacenet");
                mVendors.Add(0x12EB, "Aureal Semiconductor");
                mVendors.Add(0x12EC, "3A International Inc.");
                mVendors.Add(0x12ED, "Optivision Inc.");
                mVendors.Add(0x12EE, "Orange Micro, Inc.");
                mVendors.Add(0x12EF, "Vienna Systems");
                mVendors.Add(0x12F0, "Pentek");
                mVendors.Add(0x12F1, "Sorenson Vision Inc.");
                mVendors.Add(0x12F2, "Gammagraphx Inc.");
                mVendors.Add(0x12F4, "Megatel");
                mVendors.Add(0x12F5, "Forks");
                mVendors.Add(0x12F7, "Cognex");
                mVendors.Add(0x12F8, "Electronic-Design GmbH");
                mVendors.Add(0x12F9, "FourFold Technologies");
                mVendors.Add(0x12FB, "Spectrum Signal Processing");
                mVendors.Add(0x12FC, "Capital Equipment Corp");
                mVendors.Add(0x12FE, "esd Electronic System Design GmbH");
                mVendors.Add(0x1303, "Innovative Integration");
                mVendors.Add(0x1304, "Juniper Networks Inc.");
                mVendors.Add(0x1307, "ComputerBoards");
                mVendors.Add(0x1308, "Jato Technologies Inc.");
                mVendors.Add(0x130A, "Mitsubishi Electric Microcomputer");
                mVendors.Add(0x130B, "Colorgraphic Communications Corp");
                mVendors.Add(0x130F, "Advanet Inc.");
                mVendors.Add(0x1310, "Gespac");
                mVendors.Add(0x1312, "Robotic Vision Systems Incorporated");
                mVendors.Add(0x1313, "Yaskawa Electric Co.");
                mVendors.Add(0x1316, "Teradyne Inc.");
                mVendors.Add(0x1317, "ADMtek Inc");
                mVendors.Add(0x1318, "Packet Engines, Inc.");
                mVendors.Add(0x1319, "Forte Media");
                mVendors.Add(0x131F, "SIIG");
                mVendors.Add(0x1325, "Salix Technologies Inc");
                mVendors.Add(0x1326, "Seachange International");
                mVendors.Add(0x1328, "CIFELLI SYSTEMS CORPORATION");
                mVendors.Add(0x1331, "RadiSys Corporation");
                mVendors.Add(0x1332, "VMetro");
                mVendors.Add(0x1335, "Videomail Inc.");
                mVendors.Add(0x133D, "Prisa Networks");
                mVendors.Add(0x133F, "SCM Microsystems");
                mVendors.Add(0x1342, "Promax Systems Inc");
                mVendors.Add(0x1344, "Micron Technology, Inc.");
                mVendors.Add(0x1347, "Spectracom Corporation");
                mVendors.Add(0x134A, "DTC Technology Corp.");
                mVendors.Add(0x134B, "ARK Research Corp.");
                mVendors.Add(0x134C, "Chori Joho System Co. Ltd");
                mVendors.Add(0x134D, "PCTEL Inc.");
                mVendors.Add(0x135A, "Brain Boxes Limited");
                mVendors.Add(0x135B, "Giganet Inc.");
                mVendors.Add(0x135C, "Quatech Inc");
                mVendors.Add(0x135D, "ABB Network Partner AB");
                mVendors.Add(0x135E, "Sealevel Systems Inc.");
                mVendors.Add(0x135F, "I-Data International A-S");
                mVendors.Add(0x1360, "Meinberg Funkuhren GmbH & Co. KG");
                mVendors.Add(0x1361, "Soliton Systems K.K.");
                mVendors.Add(0x1363, "Phoenix Technologies Ltd");
                mVendors.Add(0x1365, "Hypercope Corp.");
                mVendors.Add(0x1366, "Teijin Seiki Co. Ltd.");
                mVendors.Add(0x1367, "Hitachi Zosen Corporation");
                mVendors.Add(0x1368, "Skyware Corporation");
                mVendors.Add(0x1369, "Digigram");
                mVendors.Add(0x136B, "Kawasaki Steel Corporation");
                mVendors.Add(0x136C, "Adtek System Science Co Ltd");
                mVendors.Add(0x1375, "Boeing - Sunnyvale");
                mVendors.Add(0x137A, "Mark Of The Unicorn Inc");
                mVendors.Add(0x137B, "PPT Vision");
                mVendors.Add(0x137C, "Iwatsu Electric Co Ltd");
                mVendors.Add(0x137D, "Dynachip Corporation");
                mVendors.Add(0x137E, "Patriot Scientific Corp.");
                mVendors.Add(0x1380, "Sanritz Automation Co LTC");
                mVendors.Add(0x1381, "Brains Co. Ltd");
                mVendors.Add(0x1382, "Marian - Electronic & Software");
                mVendors.Add(0x1384, "Stellar Semiconductor Inc");
                mVendors.Add(0x1385, "Netgear");
                mVendors.Add(0x1387, "Curtiss-Wright Controls Embedded Computing");
                mVendors.Add(0x1388, "Hitachi Information Technology Co Ltd");
                mVendors.Add(0x1389, "Applicom International");
                mVendors.Add(0x138B, "Tokimec Inc");
                mVendors.Add(0x138E, "Basler GMBH");
                mVendors.Add(0x138F, "Patapsco Designs Inc");
                mVendors.Add(0x1390, "Concept Development Inc.");
                mVendors.Add(0x1393, "Moxa Technologies Co Ltd");
                mVendors.Add(0x1394, "Level One Communications");
                mVendors.Add(0x1395, "Ambicom Inc");
                mVendors.Add(0x1396, "Cipher Systems Inc");
                mVendors.Add(0x1397, "Cologne Chip Designs GmbH");
                mVendors.Add(0x1398, "Clarion Co. Ltd");
                mVendors.Add(0x139A, "Alacritech Inc");
                mVendors.Add(0x139D, "Xstreams PLC/ EPL Limited");
                mVendors.Add(0x139E, "Echostar Data Networks");
                mVendors.Add(0x13A0, "Crystal Group Inc");
                mVendors.Add(0x13A1, "Kawasaki Heavy Industries Ltd");
                mVendors.Add(0x13A3, "HI-FN Inc.");
                mVendors.Add(0x13A4, "Rascom Inc");
                mVendors.Add(0x13A7, "amc330");
                mVendors.Add(0x13A8, "Exar Corp.");
                mVendors.Add(0x13A9, "Siemens Medical Solutions");
                mVendors.Add(0x13AA, "Nortel Networks - BWA Division");
                mVendors.Add(0x13AF, "T.Sqware");
                mVendors.Add(0x13B1, "Tamura Corporation");
                mVendors.Add(0x13B4, "Wellbean Co Inc");
                mVendors.Add(0x13B5, "ARM Ltd");
                mVendors.Add(0x13B6, "DLoG GMBH");
                mVendors.Add(0x13B8, "Nokia Telecommunications OY");
                mVendors.Add(0x13BD, "Sharp Corporation");
                mVendors.Add(0x13BF, "Sharewave Inc");
                mVendors.Add(0x13C0, "Microgate Corp.");
                mVendors.Add(0x13C1, "3ware Inc.");
                mVendors.Add(0x13C2, "Technotrend Systemtechnik GMBH");
                mVendors.Add(0x13C3, "Janz Computer AG");
                mVendors.Add(0x13C7, "Blue Chip Technology Ltd");
                mVendors.Add(0x13CC, "Metheus Corporation");
                mVendors.Add(0x13CF, "Studio Audio & Video Ltd");
                mVendors.Add(0x13D0, "B2C2 Inc");
                mVendors.Add(0x13D1, "AboCom Systems, Inc");
                mVendors.Add(0x13D4, "Graphics Microsystems Inc");
                mVendors.Add(0x13D6, "K.I. Technology Co Ltd");
                mVendors.Add(0x13D7, "Toshiba Engineering Corporation");
                mVendors.Add(0x13D8, "Phobos Corporation");
                mVendors.Add(0x13D9, "Apex Inc");
                mVendors.Add(0x13DC, "Netboost Corporation");
                mVendors.Add(0x13DE, "ABB Robotics Products AB");
                mVendors.Add(0x13DF, "E-Tech Inc.");
                mVendors.Add(0x13E0, "GVC Corporation");
                mVendors.Add(0x13E3, "Nest Inc");
                mVendors.Add(0x13E4, "Calculex Inc");
                mVendors.Add(0x13E5, "Telesoft Design Ltd");
                mVendors.Add(0x13E9, "Intraserver Technology Inc");
                mVendors.Add(0x13EA, "Dallas Semiconductor");
                mVendors.Add(0x13F0, "IC Plus Corporation");
                mVendors.Add(0x13F1, "OCE - Industries S.A.");
                mVendors.Add(0x13F4, "Troika Networks Inc");
                mVendors.Add(0x13F6, "C-Media Electronics Inc.");
                mVendors.Add(0x13F9, "NTT Advanced Technology Corp.");
                mVendors.Add(0x13FA, "Pentland Systems Ltd.");
                mVendors.Add(0x13FB, "Aydin Corp");
                mVendors.Add(0x13FD, "Micro Science Inc");
                mVendors.Add(0x13FE, "Advantech Co., Ltd.");
                mVendors.Add(0x13FF, "Silicon Spice Inc.");
                mVendors.Add(0x1400, "ArtX Inc");
                mVendors.Add(0x1402, "Meilhaus Electronic GmbH Germany");
                mVendors.Add(0x1404, "Fundamental Software Inc");
                mVendors.Add(0x1406, "Oce Print Logics Technologies S.A.");
                mVendors.Add(0x1407, "Lava Computer MFG Inc.");
                mVendors.Add(0x1408, "Aloka Co. Ltd");
                mVendors.Add(0x1409, "SUNIX Co., Ltd.");
                mVendors.Add(0x140A, "DSP Research Inc");
                mVendors.Add(0x140B, "Ramix Inc");
                mVendors.Add(0x140D, "Matsushita Electric Works Ltd");
                mVendors.Add(0x140F, "Salient Systems Corp");
                mVendors.Add(0x1412, "IC Ensemble, Inc.");
                mVendors.Add(0x1413, "Addonics");
                mVendors.Add(0x1415, "Oxford Semiconductor Ltd");
                mVendors.Add(0x1418, "Kyushu Electronics Systems Inc");
                mVendors.Add(0x1419, "Excel Switching Corp");
                mVendors.Add(0x141B, "Zoom Telephonics Inc");
                mVendors.Add(0x141E, "Fanuc Co. Ltd");
                mVendors.Add(0x141F, "Visiontech Ltd");
                mVendors.Add(0x1420, "Psion Dacom PLC");
                mVendors.Add(0x1425, "ASIC Designers Inc");
                mVendors.Add(0x1428, "Edec Co Ltd");
                mVendors.Add(0x1429, "Unex Technology Corp.");
                mVendors.Add(0x142A, "Kingmax Technology Inc");
                mVendors.Add(0x142B, "Radiolan");
                mVendors.Add(0x142C, "Minton Optic Industry Co Ltd");
                mVendors.Add(0x142D, "Pixstream Inc");
                mVendors.Add(0x1430, "ITT Aerospace/Communications Division");
                mVendors.Add(0x1433, "Eltec Elektronik AG");
                mVendors.Add(0x1435, "RTD Embedded Technologies, Inc.");
                mVendors.Add(0x1436, "CIS Technology Inc");
                mVendors.Add(0x1437, "Nissin Inc Co");
                mVendors.Add(0x1438, "Atmel-Dream");
                mVendors.Add(0x143F, "Lightwell Co Ltd - Zax Division");
                mVendors.Add(0x1441, "Agie SA.");
                mVendors.Add(0x1443, "Unibrain S.A.");
                mVendors.Add(0x1445, "Logical Co Ltd");
                mVendors.Add(0x1446, "Graphin Co. Ltd");
                mVendors.Add(0x1447, "Aim GMBH");
                mVendors.Add(0x1448, "Alesis Studio");
                mVendors.Add(0x144A, "ADLINK Technology Inc");
                mVendors.Add(0x144B, "Loronix Information Systems, Inc.");
                mVendors.Add(0x144D, "sanyo");
                mVendors.Add(0x1450, "Octave Communications Ind.");
                mVendors.Add(0x1451, "SP3D Chip Design GMBH");
                mVendors.Add(0x1453, "Mycom Inc");
                mVendors.Add(0x1458, "Giga-Byte Technologies");
                mVendors.Add(0x145C, "Cryptek");
                mVendors.Add(0x145F, "Baldor Electric Company");
                mVendors.Add(0x1460, "Dynarc Inc");
                mVendors.Add(0x1462, "Micro-Star International Co Ltd");
                mVendors.Add(0x1463, "Fast Corporation");
                mVendors.Add(0x1464, "Interactive Circuits & Systems Ltd");
                mVendors.Add(0x1468, "Ambit Microsystems Corp.");
                mVendors.Add(0x1469, "Cleveland Motion Controls");
                mVendors.Add(0x146C, "Ruby Tech Corp.");
                mVendors.Add(0x146D, "Tachyon Inc.");
                mVendors.Add(0x146E, "WMS Gaming");
                mVendors.Add(0x1471, "Integrated Telecom Express Inc");
                mVendors.Add(0x1473, "Zapex Technologies Inc");
                mVendors.Add(0x1474, "Doug Carson & Associates");
                mVendors.Add(0x1477, "Net Insight");
                mVendors.Add(0x1478, "Diatrend Corporation");
                mVendors.Add(0x147B, "Abit Computer Corp.");
                mVendors.Add(0x147F, "Nihon Unisys Ltd.");
                mVendors.Add(0x1482, "Isytec - Integrierte Systemtechnik Gmbh");
                mVendors.Add(0x1483, "Labway Coporation");
                mVendors.Add(0x1485, "Erma - Electronic GMBH");
                mVendors.Add(0x1489, "KYE Systems Corporation");
                mVendors.Add(0x148A, "Opto 22");
                mVendors.Add(0x148B, "Innomedialogic Inc.");
                mVendors.Add(0x148C, "C.P. Technology Co. Ltd");
                mVendors.Add(0x148D, "Digicom Systems Inc.");
                mVendors.Add(0x148E, "OSI Plus Corporation");
                mVendors.Add(0x148F, "Plant Equipment Inc.");
                mVendors.Add(0x1490, "TC Labs Pty Ltd.");
                mVendors.Add(0x1493, "Maker Communications");
                mVendors.Add(0x1495, "Tokai Communications Industry Co. Ltd");
                mVendors.Add(0x1496, "Joytech Computer Co. Ltd.");
                mVendors.Add(0x1497, "SMA Technologie AG");
                mVendors.Add(0x1498, "Tews Technologies");
                mVendors.Add(0x1499, "Micro-Technology Co Ltd");
                mVendors.Add(0x149A, "Andor Technology Ltd");
                mVendors.Add(0x149B, "Seiko Instruments Inc");
                mVendors.Add(0x149E, "Mapletree Networks Inc.");
                mVendors.Add(0x149F, "Lectron Co Ltd");
                mVendors.Add(0x14A0, "Softing GMBH");
                mVendors.Add(0x14A2, "Millennium Engineering Inc");
                mVendors.Add(0x14A4, "GVC/BCM Advanced Research");
                mVendors.Add(0x14A9, "Hivertec Inc.");
                mVendors.Add(0x14AB, "Mentor Graphics Corp.");
                mVendors.Add(0x14B1, "Nextcom K.K.");
                mVendors.Add(0x14B3, "Xpeed Inc.");
                mVendors.Add(0x14B4, "Philips Business Electronics B.V.");
                mVendors.Add(0x14B5, "Creamware GmbH");
                mVendors.Add(0x14B6, "Quantum Data Corp.");
                mVendors.Add(0x14B7, "Proxim Inc.");
                mVendors.Add(0x14B9, "Aironet Wireless Communication");
                mVendors.Add(0x14BA, "Internix Inc.");
                mVendors.Add(0x14BB, "Semtech Corporation");
                mVendors.Add(0x14BE, "L3 Communications");
                mVendors.Add(0x14C0, "Compal Electronics, Inc.");
                mVendors.Add(0x14C1, "Myricom Inc.");
                mVendors.Add(0x14C2, "DTK Computer");
                mVendors.Add(0x14C4, "Iwasaki Information Systems Co Ltd");
                mVendors.Add(0x14C5, "ABB Automation Products AB");
                mVendors.Add(0x14C6, "Data Race Inc");
                mVendors.Add(0x14C7, "Modular Technology Ltd.");
                mVendors.Add(0x14C8, "Turbocomm Tech Inc");
                mVendors.Add(0x14C9, "Odin Telesystems Inc");
                mVendors.Add(0x14CB, "Billionton Systems Inc./Cadmus Micro Inc");
                mVendors.Add(0x14CD, "Universal Scientific Ind.");
                mVendors.Add(0x14CF, "TEK Microsystems Inc.");
                mVendors.Add(0x14D4, "Panacom Technology Corporation");
                mVendors.Add(0x14D5, "Nitsuko Corporation");
                mVendors.Add(0x14D6, "Accusys Inc");
                mVendors.Add(0x14D7, "Hirakawa Hewtech Corp");
                mVendors.Add(0x14D8, "Hopf Elektronik GMBH");
                mVendors.Add(0x14D9, "Alpha Processor Inc");
                mVendors.Add(0x14DB, "Avlab Technology Inc.");
                mVendors.Add(0x14DC, "Amplicon Liveline Limited");
                mVendors.Add(0x14DD, "Imodl Inc.");
                mVendors.Add(0x14DE, "Applied Integration Corporation");
                mVendors.Add(0x14E3, "Amtelco");
                mVendors.Add(0x14E4, "Broadcom Corporation");
                mVendors.Add(0x14EA, "Planex Communications, Inc.");
                mVendors.Add(0x14EB, "Seiko Epson Corporation");
                mVendors.Add(0x14EC, "Acqiris");
                mVendors.Add(0x14ED, "Datakinetics Ltd");
                mVendors.Add(0x14EF, "Carry Computer Eng. Co Ltd");
                mVendors.Add(0x14F1, "Conexant Systems, Inc.");
                mVendors.Add(0x14F2, "Mobility Electronics, Inc.");
                mVendors.Add(0x14F4, "Tokyo Electronic Industry Co. Ltd.");
                mVendors.Add(0x14F5, "Sopac Ltd");
                mVendors.Add(0x14F6, "Coyote Technologies LLC");
                mVendors.Add(0x14F7, "Wolf Technology Inc");
                mVendors.Add(0x14F8, "Audiocodes Inc");
                mVendors.Add(0x14F9, "AG Communications");
                mVendors.Add(0x14FB, "Transas Marine (UK) Ltd");
                mVendors.Add(0x14FC, "Quadrics Ltd");
                mVendors.Add(0x14FD, "Silex Technology Inc.");
                mVendors.Add(0x14FE, "Archtek Telecom Corp.");
                mVendors.Add(0x14FF, "Twinhead International Corp.");
                mVendors.Add(0x1501, "Banksoft Canada Ltd");
                mVendors.Add(0x1502, "Mitsubishi Electric Logistics Support Co");
                mVendors.Add(0x1503, "Kawasaki LSI USA Inc");
                mVendors.Add(0x1504, "Kaiser Electronics");
                mVendors.Add(0x1506, "Chameleon Systems Inc");
                mVendors.Add(0x1507, "Htec Ltd.");
                mVendors.Add(0x1509, "First International Computer Inc");
                mVendors.Add(0x150B, "Yamashita Systems Corp");
                mVendors.Add(0x150C, "Kyopal Co Ltd");
                mVendors.Add(0x150D, "Warpspped Inc");
                mVendors.Add(0x150E, "C-Port Corporation");
                mVendors.Add(0x150F, "Intec GMBH");
                mVendors.Add(0x1510, "Behavior Tech Computer Corp");
                mVendors.Add(0x1511, "Centillium Technology Corp");
                mVendors.Add(0x1512, "Rosun Technologies Inc");
                mVendors.Add(0x1513, "Raychem");
                mVendors.Add(0x1514, "TFL LAN Inc");
                mVendors.Add(0x1515, "ICS Advent");
                mVendors.Add(0x1516, "Myson Technology Inc");
                mVendors.Add(0x1517, "Echotek Corporation");
                mVendors.Add(0x1518, "Kontron Modular Computers GmbH (PEP Modular Computers GMBH)");
                mVendors.Add(0x1519, "Telefon Aktiebolaget LM Ericsson");
                mVendors.Add(0x151A, "Globetek Inc.");
                mVendors.Add(0x151B, "Combox Ltd");
                mVendors.Add(0x151C, "Digital Audio Labs Inc");
                mVendors.Add(0x151D, "Fujitsu Computer Products Of America");
                mVendors.Add(0x151E, "Matrix Corp.");
                mVendors.Add(0x151F, "Topic Semiconductor Corp");
                mVendors.Add(0x1520, "Chaplet System Inc");
                mVendors.Add(0x1521, "Bell Corporation");
                mVendors.Add(0x1522, "Mainpine Limited");
                mVendors.Add(0x1523, "Music Semiconductors");
                mVendors.Add(0x1524, "ENE Technology Inc");
                mVendors.Add(0x1525, "Impact Technologies");
                mVendors.Add(0x1526, "ISS Inc");
                mVendors.Add(0x1527, "Solectron");
                mVendors.Add(0x1528, "Acksys");
                mVendors.Add(0x1529, "American Microsystems Inc");
                mVendors.Add(0x152A, "Quickturn Design Systems");
                mVendors.Add(0x152B, "Flytech Technology Co Ltd");
                mVendors.Add(0x152C, "Macraigor Systems LLC");
                mVendors.Add(0x152D, "Quanta Computer Inc");
                mVendors.Add(0x152E, "Melec Inc");
                mVendors.Add(0x152F, "Philips - Crypto");
                mVendors.Add(0x1532, "Echelon Corporation");
                mVendors.Add(0x1533, "Baltimore");
                mVendors.Add(0x1534, "Road Corporation");
                mVendors.Add(0x1535, "Evergreen Technologies Inc");
                mVendors.Add(0x1537, "Datalex Communcations");
                mVendors.Add(0x1538, "Aralion Inc.");
                mVendors.Add(0x1539, "Atelier Informatiques et Electronique Et");
                mVendors.Add(0x153A, "ONO Sokki");
                mVendors.Add(0x153B, "Terratec Electronic GMBH");
                mVendors.Add(0x153C, "Antal Electronic");
                mVendors.Add(0x153D, "Filanet Corporation");
                mVendors.Add(0x153E, "Techwell Inc");
                mVendors.Add(0x153F, "MIPS Technologies, Inc");
                mVendors.Add(0x1540, "Provideo Multimedia Co Ltd");
                mVendors.Add(0x1541, "Telocity Inc.");
                mVendors.Add(0x1542, "Vivid Technology Inc");
                mVendors.Add(0x1543, "Silicon Laboratories");
                mVendors.Add(0x1544, "DCM Technologies Ltd.");
                mVendors.Add(0x1545, "VisionTek");
                mVendors.Add(0x1546, "IOI Technology Corp.");
                mVendors.Add(0x1547, "Mitutoyo Corporation");
                mVendors.Add(0x1548, "Jet Propulsion Laboratory");
                mVendors.Add(0x1549, "Interconnect Systems Solutions");
                mVendors.Add(0x154A, "Max Technologies Inc.");
                mVendors.Add(0x154B, "Computex Co Ltd");
                mVendors.Add(0x154C, "Visual Technology Inc.");
                mVendors.Add(0x154D, "PAN International Industrial Corp");
                mVendors.Add(0x154E, "Servotest Ltd");
                mVendors.Add(0x154F, "Stratabeam Technology");
                mVendors.Add(0x1550, "Open Network Co Ltd");
                mVendors.Add(0x1551, "Smart Electronic Development GMBH");
                mVendors.Add(0x1553, "Chicony Electronics Co Ltd");
                mVendors.Add(0x1554, "Prolink Microsystems Corp.");
                mVendors.Add(0x1555, "Gesytec GmbH");
                mVendors.Add(0x1556, "PLD Applications");
                mVendors.Add(0x1557, "Mediastar Co. Ltd");
                mVendors.Add(0x1558, "Clevo/Kapok Computer");
                mVendors.Add(0x1559, "SI Logic Ltd");
                mVendors.Add(0x155A, "Innomedia Inc");
                mVendors.Add(0x155B, "Protac International Corp");
                mVendors.Add(0x155C, "s");
                mVendors.Add(0x155D, "MAC System Co Ltd");
                mVendors.Add(0x155E, "KUKA Roboter GmbH");
                mVendors.Add(0x155F, "Perle Systems Limited");
                mVendors.Add(0x1560, "Terayon Communications Systems");
                mVendors.Add(0x1561, "Viewgraphics Inc");
                mVendors.Add(0x1562, "Symbol Technologies, Inc.");
                mVendors.Add(0x1563, "A-Trend Technology Co Ltd");
                mVendors.Add(0x1564, "Yamakatsu Electronics Industry Co Ltd");
                mVendors.Add(0x1565, "Biostar Microtech Intl Corp");
                mVendors.Add(0x1566, "Ardent Technologies Inc");
                mVendors.Add(0x1567, "Jungsoft");
                mVendors.Add(0x1568, "DDK Electronics Inc");
                mVendors.Add(0x1569, "Palit Microsystems Inc");
                mVendors.Add(0x156A, "Avtec Systems Inc");
                mVendors.Add(0x156B, "S2io Inc");
                mVendors.Add(0x156C, "Vidac Electronics GMBH");
                mVendors.Add(0x156D, "Alpha-Top Corp");
                mVendors.Add(0x156E, "Alfa Inc.");
                mVendors.Add(0x156F, "M-Systems Flash Disk Pioneers Ltd");
                mVendors.Add(0x1570, "Lecroy Corporation");
                mVendors.Add(0x1571, "Contemporary Controls");
                mVendors.Add(0x1572, "Otis Elevator Company");
                mVendors.Add(0x1573, "Lattice - Vantis");
                mVendors.Add(0x1574, "Fairchild Semiconductor");
                mVendors.Add(0x1575, "Voltaire Advanced Data Security Ltd");
                mVendors.Add(0x1576, "Viewcast Com");
                mVendors.Add(0x1578, "Hitt");
                mVendors.Add(0x1579, "Dual Technology Corporation");
                mVendors.Add(0x157A, "Japan Elecronics Ind. Inc");
                mVendors.Add(0x157B, "Star Multimedia Corp.");
                mVendors.Add(0x157C, "Eurosoft (UK)");
                mVendors.Add(0x157D, "Gemflex Networks");
                mVendors.Add(0x157E, "Transition Networks");
                mVendors.Add(0x157F, "PX Instruments Technology Ltd");
                mVendors.Add(0x1580, "Primex Aerospace Co.");
                mVendors.Add(0x1581, "SEH Computertechnik GMBH");
                mVendors.Add(0x1582, "Cytec Corporation");
                mVendors.Add(0x1583, "Inet Technologies Inc");
                mVendors.Add(0x1584, "Uniwill Computer Corp.");
                mVendors.Add(0x1585, "Marconi Commerce Systems SRL");
                mVendors.Add(0x1586, "Lancast Inc");
                mVendors.Add(0x1587, "Konica Corporation");
                mVendors.Add(0x1588, "Solidum Systems Corp");
                mVendors.Add(0x1589, "Atlantek Microsystems Pty Ltd");
                mVendors.Add(0x158A, "Digalog Systems Inc");
                mVendors.Add(0x158B, "Allied Data Technologies");
                mVendors.Add(0x158C, "Hitachi Semiconductor & Devices Sales Co");
                mVendors.Add(0x158D, "Point Multimedia Systems");
                mVendors.Add(0x158E, "Lara Technology Inc");
                mVendors.Add(0x158F, "Ditect Coop");
                mVendors.Add(0x1590, "3pardata Inc.");
                mVendors.Add(0x1591, "ARN");
                mVendors.Add(0x1592, "Syba Tech Ltd.");
                mVendors.Add(0x1593, "Bops Inc");
                mVendors.Add(0x1594, "Netgame Ltd");
                mVendors.Add(0x1595, "Diva Systems Corp.");
                mVendors.Add(0x1596, "Folsom Research Inc");
                mVendors.Add(0x1597, "Memec Design Services");
                mVendors.Add(0x1598, "Granite Microsystems");
                mVendors.Add(0x1599, "Delta Electronics Inc");
                mVendors.Add(0x159A, "General Instrument");
                mVendors.Add(0x159B, "Faraday Technology Corp");
                mVendors.Add(0x159C, "Stratus Computer Systems");
                mVendors.Add(0x159D, "Ningbo Harrison Electronics Co Ltd");
                mVendors.Add(0x159E, "A-Max Technology Co Ltd");
                mVendors.Add(0x159F, "Galea Network Security");
                mVendors.Add(0x15A0, "Compumaster SRL");
                mVendors.Add(0x15A1, "Geocast Network Systems Inc");
                mVendors.Add(0x15A2, "Catalyst Enterprises Inc");
                mVendors.Add(0x15A3, "Italtel");
                mVendors.Add(0x15A4, "X-Net OY");
                mVendors.Add(0x15A5, "Toyota MACS Inc");
                mVendors.Add(0x15A6, "Sunlight Ultrasound Technologies Ltd");
                mVendors.Add(0x15A7, "SSE Telecom Inc");
                mVendors.Add(0x15A8, "Shanghai Communications Technologies Cen");
                mVendors.Add(0x15AA, "Moreton Bay");
                mVendors.Add(0x15AB, "Bluesteel Networks Inc");
                mVendors.Add(0x15AC, "North Atlantic Instruments");
                mVendors.Add(0x15AD, "VMware Inc.");
                mVendors.Add(0x15AE, "Amersham Pharmacia Biotech");
                mVendors.Add(0x15B0, "Zoltrix International Limited");
                mVendors.Add(0x15B1, "Source Technology Inc");
                mVendors.Add(0x15B2, "Mosaid Technologies Inc.");
                mVendors.Add(0x15B3, "Mellanox Technology");
                mVendors.Add(0x15B4, "CCI/Triad");
                mVendors.Add(0x15B5, "Cimetrics Inc");
                mVendors.Add(0x15B6, "Texas Memory Systems Inc");
                mVendors.Add(0x15B7, "Sandisk Corp.");
                mVendors.Add(0x15B8, "Addi-Data GMBH");
                mVendors.Add(0x15B9, "Maestro Digital Communications");
                mVendors.Add(0x15BA, "Impacct Technology Corp");
                mVendors.Add(0x15BB, "Portwell Inc");
                mVendors.Add(0x15BC, "Agilent Technologies");
                mVendors.Add(0x15BD, "DFI Inc.");
                mVendors.Add(0x15BE, "Sola Electronics");
                mVendors.Add(0x15BF, "High Tech Computer Corp (HTC)");
                mVendors.Add(0x15C0, "BVM Limited");
                mVendors.Add(0x15C1, "Quantel");
                mVendors.Add(0x15C2, "Newer Technology Inc");
                mVendors.Add(0x15C3, "Taiwan Mycomp Co Ltd");
                mVendors.Add(0x15C4, "EVSX Inc");
                mVendors.Add(0x15C5, "Procomp Informatics Ltd");
                mVendors.Add(0x15C6, "Technical University Of Budapest");
                mVendors.Add(0x15C7, "Tateyama System Laboratory Co Ltd");
                mVendors.Add(0x15C8, "Penta Media Co. Ltd");
                mVendors.Add(0x15C9, "Serome Technology Inc");
                mVendors.Add(0x15CA, "Bitboys OY");
                mVendors.Add(0x15CB, "AG Electronics Ltd");
                mVendors.Add(0x15CC, "Hotrail Inc.");
                mVendors.Add(0x15CD, "Dreamtech Co Ltd");
                mVendors.Add(0x15CE, "Genrad Inc.");
                mVendors.Add(0x15CF, "Hilscher GMBH");
                mVendors.Add(0x15D1, "Infineon Technologies AG");
                mVendors.Add(0x15D2, "FIC (First International Computer Inc)");
                mVendors.Add(0x15D3, "NDS Technologies Israel Ltd");
                mVendors.Add(0x15D4, "Iwill Corporation");
                mVendors.Add(0x15D5, "Tatung Co.");
                mVendors.Add(0x15D6, "Entridia Corporation");
                mVendors.Add(0x15D7, "Rockwell-Collins Inc");
                mVendors.Add(0x15D8, "Cybernetics Technology Co Ltd");
                mVendors.Add(0x15D9, "Super Micro Computer Inc");
                mVendors.Add(0x15DA, "Cyberfirm Inc.");
                mVendors.Add(0x15DB, "Applied Computing Systems Inc.");
                mVendors.Add(0x15DC, "Litronic Inc.");
                mVendors.Add(0x15DD, "Sigmatel Inc.");
                mVendors.Add(0x15DE, "Malleable Technologies Inc");
                mVendors.Add(0x15E0, "Cacheflow Inc");
                mVendors.Add(0x15E1, "Voice Technologies Group");
                mVendors.Add(0x15E2, "Quicknet Technologies Inc");
                mVendors.Add(0x15E3, "Networth Technologies Inc");
                mVendors.Add(0x15E4, "VSN Systemen BV");
                mVendors.Add(0x15E5, "Valley Technologies Inc");
                mVendors.Add(0x15E6, "Agere Inc.");
                mVendors.Add(0x15E7, "GET Engineering Corp.");
                mVendors.Add(0x15E8, "National Datacomm Corp.");
                mVendors.Add(0x15E9, "Pacific Digital Corp.");
                mVendors.Add(0x15EA, "Tokyo Denshi Sekei K.K.");
                mVendors.Add(0x15EB, "Drsearch GMBH");
                mVendors.Add(0x15EC, "Beckhoff GMBH");
                mVendors.Add(0x15ED, "Macrolink Inc");
                mVendors.Add(0x15EE, "IN Win Development Inc.");
                mVendors.Add(0x15EF, "Intelligent Paradigm Inc");
                mVendors.Add(0x15F0, "B-Tree Systems Inc");
                mVendors.Add(0x15F1, "Times N Systems Inc");
                mVendors.Add(0x15F2, "Diagnostic Instruments Inc");
                mVendors.Add(0x15F3, "Digitmedia Corp.");
                mVendors.Add(0x15F4, "Valuesoft");
                mVendors.Add(0x15F5, "Power Micro Research");
                mVendors.Add(0x15F6, "Extreme Packet Device Inc");
                mVendors.Add(0x15F7, "Banctec");
                mVendors.Add(0x15F8, "Koga Electronics Co");
                mVendors.Add(0x15F9, "Zenith Electronics Corporation");
                mVendors.Add(0x15FA, "Axzam Corporation");
                mVendors.Add(0x15FB, "Zilog Inc.");
                mVendors.Add(0x15FC, "Techsan Electronics Co Ltd");
                mVendors.Add(0x15FD, "N-Cubed.Net");
                mVendors.Add(0x15FE, "Kinpo Electronics Inc");
                mVendors.Add(0x15FF, "Fastpoint Technologies Inc.");
                mVendors.Add(0x1600, "Northrop Grumman - Canada Ltd");
                mVendors.Add(0x1601, "Tenta Technology");
                mVendors.Add(0x1602, "Prosys-TEC Inc.");
                mVendors.Add(0x1603, "Nokia Wireless Business Communications");
                mVendors.Add(0x1604, "Central System Research Co Ltd");
                mVendors.Add(0x1605, "Pairgain Technologies");
                mVendors.Add(0x1606, "Europop AG");
                mVendors.Add(0x1607, "Lava Semiconductor Manufacturing Inc.");
                mVendors.Add(0x1608, "Automated Wagering International");
                mVendors.Add(0x1609, "Sciemetric Instruments Inc");
                mVendors.Add(0x160A, "Kollmorgen Servotronix");
                mVendors.Add(0x160B, "Onkyo Corp.");
                mVendors.Add(0x160C, "Oregon Micro Systems Inc.");
                mVendors.Add(0x160D, "Aaeon Electronics Inc");
                mVendors.Add(0x160E, "CML Emergency Services");
                mVendors.Add(0x160F, "ITEC Co Ltd");
                mVendors.Add(0x1610, "Tottori Sanyo Electric Co Ltd");
                mVendors.Add(0x1611, "Bel Fuse Inc.");
                mVendors.Add(0x1612, "Telesynergy Research Inc.");
                mVendors.Add(0x1613, "System Craft Inc.");
                mVendors.Add(0x1614, "Jace Tech Inc.");
                mVendors.Add(0x1615, "Equus Computer Systems Inc");
                mVendors.Add(0x1616, "Iotech Inc.");
                mVendors.Add(0x1617, "Rapidstream Inc");
                mVendors.Add(0x1618, "Esec SA");
                mVendors.Add(0x1619, "FarSite Communications Limited");
                mVendors.Add(0x161B, "Mobilian Israel Ltd");
                mVendors.Add(0x161C, "Berkshire Products");
                mVendors.Add(0x161D, "Gatec");
                mVendors.Add(0x161E, "Kyoei Sangyo Co Ltd");
                mVendors.Add(0x161F, "Arima Computer Corporation");
                mVendors.Add(0x1620, "Sigmacom Co Ltd");
                mVendors.Add(0x1621, "Lynx Studio Technology Inc");
                mVendors.Add(0x1622, "Nokia Home Communications");
                mVendors.Add(0x1623, "KRF Tech Ltd");
                mVendors.Add(0x1624, "CE Infosys GMBH");
                mVendors.Add(0x1625, "Warp Nine Engineering");
                mVendors.Add(0x1626, "TDK Semiconductor Corp.");
                mVendors.Add(0x1627, "BCom Electronics Inc");
                mVendors.Add(0x1629, "Kongsberg Spacetec a.s.");
                mVendors.Add(0x162A, "Sejin Computerland Co Ltd");
                mVendors.Add(0x162B, "Shanghai Bell Company Limited");
                mVendors.Add(0x162C, "C&H Technologies Inc");
                mVendors.Add(0x162D, "Reprosoft Co Ltd");
                mVendors.Add(0x162E, "Margi Systems Inc");
                mVendors.Add(0x162F, "Rohde & Schwarz GMBH & Co KG");
                mVendors.Add(0x1630, "Sky Computers Inc");
                mVendors.Add(0x1631, "NEC Computer International");
                mVendors.Add(0x1632, "Verisys Inc");
                mVendors.Add(0x1633, "Adac Corporation");
                mVendors.Add(0x1634, "Visionglobal Network Corp.");
                mVendors.Add(0x1635, "Decros");
                mVendors.Add(0x1636, "Jean Company Ltd");
                mVendors.Add(0x1637, "NSI");
                mVendors.Add(0x1638, "Eumitcom Technology Inc");
                mVendors.Add(0x163A, "Air Prime Inc");
                mVendors.Add(0x163B, "Glotrex Co Ltd");
                mVendors.Add(0x163C, "intel");
                mVendors.Add(0x163D, "Heidelberg Digital LLC");
                mVendors.Add(0x163E, "3dpower");
                mVendors.Add(0x163F, "Renishaw PLC");
                mVendors.Add(0x1640, "Intelliworxx Inc");
                mVendors.Add(0x1641, "MKNet Corporation");
                mVendors.Add(0x1642, "Bitland");
                mVendors.Add(0x1643, "Hajime Industries Ltd");
                mVendors.Add(0x1644, "Western Avionics Ltd");
                mVendors.Add(0x1645, "Quick-Serv. Computer Co. Ltd");
                mVendors.Add(0x1646, "Nippon Systemware Co Ltd");
                mVendors.Add(0x1647, "Hertz Systemtechnik GMBH");
                mVendors.Add(0x1648, "MeltDown Systems LLC");
                mVendors.Add(0x1649, "Jupiter Systems");
                mVendors.Add(0x164A, "Aiwa Co. Ltd");
                mVendors.Add(0x164C, "Department Of Defense");
                mVendors.Add(0x164D, "Ishoni Networks");
                mVendors.Add(0x164E, "Micrel Inc.");
                mVendors.Add(0x164F, "Datavoice (Pty) Ltd.");
                mVendors.Add(0x1650, "Admore Technology Inc.");
                mVendors.Add(0x1651, "Chaparral Network Storage");
                mVendors.Add(0x1652, "Spectrum Digital Inc.");
                mVendors.Add(0x1653, "Nature Worldwide Technology Corp");
                mVendors.Add(0x1654, "Sonicwall Inc");
                mVendors.Add(0x1655, "Dazzle Multimedia Inc.");
                mVendors.Add(0x1656, "Insyde Software Corp");
                mVendors.Add(0x1657, "Brocade Communications Systems");
                mVendors.Add(0x1658, "Med Associates Inc.");
                mVendors.Add(0x1659, "Shiba Denshi Systems Inc.");
                mVendors.Add(0x165A, "Epix Inc.");
                mVendors.Add(0x165B, "Real-Time Digital Inc.");
                mVendors.Add(0x165C, "Kondo Kagaku");
                mVendors.Add(0x165D, "Hsing Tech. Enterprise Co. Ltd.");
                mVendors.Add(0x165E, "Hyunju Computer Co. Ltd.");
                mVendors.Add(0x165F, "Comartsystem Korea");
                mVendors.Add(0x1660, "Network Security Technologies Inc. (Net");
                mVendors.Add(0x1661, "Worldspace Corp.");
                mVendors.Add(0x1662, "Int Labs");
                mVendors.Add(0x1663, "Elmec Inc. Ltd.");
                mVendors.Add(0x1664, "Fastfame Technology Co. Ltd.");
                mVendors.Add(0x1665, "Edax Inc.");
                mVendors.Add(0x1666, "Norpak Corporation");
                mVendors.Add(0x1667, "CoSystems Inc.");
                mVendors.Add(0x1668, "Actiontec Electronics Inc.");
                mVendors.Add(0x166A, "Komatsu Ltd.");
                mVendors.Add(0x166B, "Supernet Inc.");
                mVendors.Add(0x166C, "Shade Ltd.");
                mVendors.Add(0x166D, "Sibyte Inc.");
                mVendors.Add(0x166E, "Schneider Automation Inc.");
                mVendors.Add(0x166F, "Televox Software Inc.");
                mVendors.Add(0x1670, "Rearden Steel");
                mVendors.Add(0x1671, "Atan Technology Inc.");
                mVendors.Add(0x1672, "Unitec Co. Ltd.");
                mVendors.Add(0x1673, "pctel");
                mVendors.Add(0x1675, "Square Wave Technology");
                mVendors.Add(0x1676, "Emachines Inc.");
                mVendors.Add(0x1677, "Bernecker + Rainer");
                mVendors.Add(0x1678, "INH Semiconductor");
                mVendors.Add(0x1679, "Tokyo Electron Device Ltd.");
                mVendors.Add(0x167F, "iba AG");
                mVendors.Add(0x1680, "Dunti Corp.");
                mVendors.Add(0x1681, "Hercules");
                mVendors.Add(0x1682, "PINE Technology, Ltd.");
                mVendors.Add(0x1688, "CastleNet Technology Inc.");
                mVendors.Add(0x168A, "Utimaco Safeware AG");
                mVendors.Add(0x168B, "Circut Assembly Corp.");
                mVendors.Add(0x168C, "Atheros Communications Inc.");
                mVendors.Add(0x168D, "NMI Electronics Ltd.");
                mVendors.Add(0x168E, "Hyundai MultiCAV Computer Co. Ltd.");
                mVendors.Add(0x168F, "KDS Innotech Corp.");
                mVendors.Add(0x1690, "NetContinuum, Inc.");
                mVendors.Add(0x1693, "FERMA");
                mVendors.Add(0x1695, "EPoX Computer Co., Ltd.");
                mVendors.Add(0x16AE, "SafeNet Inc.");
                mVendors.Add(0x16B3, "CNF Mobile Solutions");
                mVendors.Add(0x16B8, "Sonnet Technologies, Inc.");
                mVendors.Add(0x16CA, "Cenatek Inc.");
                mVendors.Add(0x16CB, "Minolta Co. Ltd.");
                mVendors.Add(0x16CC, "Inari Inc.");
                mVendors.Add(0x16D0, "Systemax");
                mVendors.Add(0x16E0, "Third Millenium Test Solutions, Inc.");
                mVendors.Add(0x16E5, "Intellon Corporation");
                mVendors.Add(0x16EC, "U.S. Robotics");
                mVendors.Add(0x16F0, "TLA Inc.");
                mVendors.Add(0x16F1, "Adicti Corp.");
                mVendors.Add(0x16F3, "Jetway Information Co., Ltd");
                mVendors.Add(0x16F6, "VideoTele.com Inc.");
                mVendors.Add(0x1700, "Antara LLC");
                mVendors.Add(0x1701, "Interactive Computer Products Inc.");
                mVendors.Add(0x1702, "Internet Machines Corp.");
                mVendors.Add(0x1703, "Desana Systems");
                mVendors.Add(0x1704, "Clearwater Networks");
                mVendors.Add(0x1705, "Digital First");
                mVendors.Add(0x1706, "Pacific Broadband Communications");
                mVendors.Add(0x1707, "Cogency Semiconductor Inc.");
                mVendors.Add(0x1708, "Harris Corp.");
                mVendors.Add(0x1709, "Zarlink Semiconductor");
                mVendors.Add(0x170A, "Alpine Electronics Inc.");
                mVendors.Add(0x170B, "NetOctave Inc.");
                mVendors.Add(0x170C, "YottaYotta Inc.");
                mVendors.Add(0x170D, "SensoMotoric Instruments GmbH");
                mVendors.Add(0x170E, "San Valley Systems, Inc.");
                mVendors.Add(0x170F, "Cyberdyne Inc.");
                mVendors.Add(0x1710, "Pelago Nutworks");
                mVendors.Add(0x1711, "MyName Technologies, Inc.");
                mVendors.Add(0x1712, "NICE Systems Inc.");
                mVendors.Add(0x1713, "TOPCON Corp.");
                mVendors.Add(0x1725, "Vitesse Semiconductor");
                mVendors.Add(0x1734, "Fujitsu-Siemens Computers GmbH");
                mVendors.Add(0x1737, "LinkSys");
                mVendors.Add(0x173B, "Altima Communications Inc.");
                mVendors.Add(0x1743, "Peppercon AG");
                mVendors.Add(0x174B, "PC Partner Limited");
                mVendors.Add(0x1752, "Global Brands Manufacture Ltd.");
                mVendors.Add(0x1753, "TeraRecon, Inc.");
                mVendors.Add(0x1755, "Alchemy Semiconductor Inc.");
                mVendors.Add(0x176A, "General Dynamics Canada");
                mVendors.Add(0x1789, "Ennyah Technologies Corp");
                mVendors.Add(0x1793, "Unitech Electronics Co., Ltd");
                mVendors.Add(0x17A7, "Start Network Technology Co., Ltd.");
                mVendors.Add(0x17AA, "Legend Ltd. (Beijing)");
                mVendors.Add(0x17AB, "Phillips Components");
                mVendors.Add(0x17AF, "Hightech Information Systems, Ltd.");
                mVendors.Add(0x17BE, "Philips Semiconductors");
                mVendors.Add(0x17C0, "Wistron Corp.");
                mVendors.Add(0x17C4, "Movita");
                mVendors.Add(0x17CC, "NetChip");
                mVendors.Add(0x17D5, "Neterion Inc.");
                mVendors.Add(0x17E9, "DH electronics GmbH");
                mVendors.Add(0x17EE, "Connect Components, Ltd.");
                mVendors.Add(0x1813, "Ambient Technologies Inc.");
                mVendors.Add(0x1814, "Ralink Technology, Corp");
                mVendors.Add(0x1815, "devolo AG");
                mVendors.Add(0x1820, "InfiniCon Systems, Inc.");
                mVendors.Add(0x1824, "Avocent");
                mVendors.Add(0x1860, "Primagraphics Ltd.");
                mVendors.Add(0x186C, "Humusoft S.R.O");
                mVendors.Add(0x1887, "Elan Digital Systems Ltd");
                mVendors.Add(0x1888, "Varisys Limited");
                mVendors.Add(0x188D, "Millogic Ltd.");
                mVendors.Add(0x1890, "Egenera, Inc.");
                mVendors.Add(0x18BC, "Info-Tek Corp.");
                mVendors.Add(0x18C9, "ARVOO Engineering BV");
                mVendors.Add(0x18CA, "XGI Technology Inc");
                mVendors.Add(0x18F1, "Spectrum Systementwicklung Microelectronic GmbH");
                mVendors.Add(0x18F4, "Napatech A/S");
                mVendors.Add(0x18F7, "Commtech, Inc.");
                mVendors.Add(0x18FB, "Resilience Corporation");
                mVendors.Add(0x1905, "WIS Technology, Inc.");
                mVendors.Add(0x1910, "Seaway Networks");
                mVendors.Add(0x1971, "AGEIA Technologies, Inc.");
                mVendors.Add(0x19A8, "DAQDATA GmbH");
                mVendors.Add(0x19AC, "Kasten Chase Applied Research");
                mVendors.Add(0x19E2, "Vector Informatik GmbH");
                mVendors.Add(0x1A08, "Linux Networx");
                mVendors.Add(0x1A42, "Imaginant");
                mVendors.Add(0x1B13, "Jaton Corporation USA");
                mVendors.Add(0x1DE1, "Tekram");
                mVendors.Add(0x1FCF, "Miranda Technologies Ltd.");
                mVendors.Add(0x2001, "Temporal Research Ltd");
                mVendors.Add(0x2646, "Kingston Technology Co.");
                mVendors.Add(0x270F, "ChainTech Computer Co. Ltd.");
                mVendors.Add(0x2EC1, "Zenic Inc");
                mVendors.Add(0x3388, "Hint Corp.");
                mVendors.Add(0x3411, "Quantum Designs (H.K.) Inc.");
                mVendors.Add(0x3513, "ARCOM Control Systems Ltd.");
                mVendors.Add(0x38EF, "4links");
                mVendors.Add(0x3D3D, "3Dlabs, Inc. Ltd");
                mVendors.Add(0x4005, "Avance Logic Inc.");
                mVendors.Add(0x4144, "Alpha Data");
                mVendors.Add(0x416C, "Aladdin Knowledge Systems");
                mVendors.Add(0x4680, "UMAX Computer Corp.");
                mVendors.Add(0x4843, "Hercules Computer Technology");
                mVendors.Add(0x4943, "Growth Networks");
                mVendors.Add(0x4954, "Integral Technologies");
                mVendors.Add(0x4978, "Axil Computer Inc.");
                mVendors.Add(0x4C48, "Lung Hwa Electronics");
                mVendors.Add(0x4C53, "SBS-OR Industrial Computers");
                mVendors.Add(0x4CA1, "Seanix Technology Inc");
                mVendors.Add(0x4D51, "Mediaq Inc.");
                mVendors.Add(0x4D54, "Microtechnica Co Ltd");
                mVendors.Add(0x4DDC, "ILC Data Device Corp.");
                mVendors.Add(0x5053, "TBS/Voyetra Technologies");
                mVendors.Add(0x5136, "S S Technologies");
                mVendors.Add(0x5143, "Qualcomm Inc.");
                mVendors.Add(0x5333, "S3 Graphics Co., Ltd");
                mVendors.Add(0x544C, "Teralogic Inc");
                mVendors.Add(0x5555, "Genroco Inc.");
                mVendors.Add(0x6409, "Logitec Corp.");
                mVendors.Add(0x6666, "Decision Computer International Co.");
                mVendors.Add(0x7604, "O.N. Electric Co. Ltd.");
                mVendors.Add(0x8086, "Intel Corporation");
                mVendors.Add(0x8866, "T-Square Design Inc.");
                mVendors.Add(0x8888, "Silicon Magic");
                mVendors.Add(0x8E0E, "Computone Corporation");
                mVendors.Add(0x9004, "Adaptec Inc");
                mVendors.Add(0x9005, "Adaptec Inc");
                mVendors.Add(0x919A, "Gigapixel Corp");
                mVendors.Add(0x9412, "Holtek");
                mVendors.Add(0x9699, "Omni Media Technology Inc.");
                mVendors.Add(0x9902, "StarGen, Inc.");
                mVendors.Add(0xA0A0, "Aopen Inc.");
                mVendors.Add(0xA0F1, "Unisys Corporation");
                mVendors.Add(0xA200, "NEC Corp.");
                mVendors.Add(0xA259, "Hewlett Packard");
                mVendors.Add(0xA304, "Sony");
                mVendors.Add(0xA727, "3com Corporation");
                mVendors.Add(0xAA42, "Scitex Digital Video");
                mVendors.Add(0xAC1E, "Digital Receiver Technology Inc");
                mVendors.Add(0xB1B3, "Shiva Europe Ltd.");
                mVendors.Add(0xB894, "Brown & Sharpe Mfg. Co.");
                mVendors.Add(0xBEEF, "Mindstream Computing");
                mVendors.Add(0xC001, "TSI Telsys");
                mVendors.Add(0xC0A9, "Micron/Crucial Technology");
                mVendors.Add(0xC0DE, "Motorola");
                mVendors.Add(0xC0FE, "Motion Engineering Inc.");
                mVendors.Add(0xC622, "Hudson Soft Co Ltd");
                mVendors.Add(0xCA50, "Varian Australia Pty. Ltd.");
                mVendors.Add(0xCAFE, "Chrysalis-ITS");
                mVendors.Add(0xCCCC, "Catapult Communications");
                mVendors.Add(0xD4D4, "Curtiss-Wright Controls Embedded Computing");
                mVendors.Add(0xDC93, "Dawicontrol");
                mVendors.Add(0xDEAD, "Indigita Corporation");
                mVendors.Add(0xDEAF, "Middle Digital, Inc");
                mVendors.Add(0xE159, "Tiger Jet Network Inc");
                mVendors.Add(0xE4BF, "EKF Elektronik GMBH");
                mVendors.Add(0xEA01, "Eagle Technology");
                mVendors.Add(0xEABB, "Aashima Technology B.V.");
                mVendors.Add(0xEACE, "Endace Measurement Systems Ltd.");
                mVendors.Add(0xECC0, "Echo Digital Audio Corporation");
                mVendors.Add(0xEDD8, "ARK Logic, Inc");
                mVendors.Add(0xF5F5, "F5 Networks Inc.");
                mVendors.Add(0xFA57, "Interagon AS");
                */

                #endregion PCIHelper

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

        protected static PCIDevice[] mDevices;
        public static PCIDevice[] Devices {
            get { return mDevices; }
        }

        protected static bool mEnumerationCompleted = false;

        public static PCIDevice GetPCIDevice(byte bus, byte slot, byte function) {
            if (!mEnumerationCompleted) {
                Init();
            }

            foreach (PCIDevice dev in PCIBus.Devices) {
                if (dev.Bus == bus &&
                    dev.Slot == slot &&
                    dev.Function == function)
                    return dev;
            }

            return null;
        }

        public static void Init() {
            var xDevices = new List<PCIDevice>();
            EnumerateBus(0, ref xDevices);
            mDevices = xDevices.ToArray();
            mEnumerationCompleted = true;
        }

        /// <summary>
        /// Enumerates a given bus, adding devices to Devices
        /// </summary>
        /// <param name="Bus">The bus number to enumerate</param>
        /// <param name="Devices">The list of Devices</param>
        private static void EnumerateBus(byte aBus, ref List<PCIDevice> rDevices) {
            //Console.WriteLine("Enumerate " + Bus ); 
            
            for (byte xSlot = 0; xSlot < 32; xSlot++) {                
                byte xMaxFunctions = 1;
                for (byte xFunction = 0; xFunction < xMaxFunctions; xFunction++) {
                    PCIDevice xPCIDevice = new PCIDeviceNormal(aBus, xSlot, xFunction);

                    if (xPCIDevice.DeviceExists) {
                        //if (xPCIDevice.HeaderType == 0 /* PCIHeaderType.Normal */)
                        //  xPCIDevice = xPCIDevice;

                        if (xPCIDevice.HeaderType == 2) { /* PCIHeaderType.Cardbus */
                            xPCIDevice = new PCIDeviceCardBus(aBus, xSlot, xFunction);
                        }

                        if (xPCIDevice.HeaderType == 1) { /* PCIHeaderType.Bridge */
                            xPCIDevice = new PCIDeviceBridge(aBus, xSlot, xFunction);
                        }

                        rDevices.Add(xPCIDevice);

                        if (xPCIDevice is PCIDeviceBridge) {
                            EnumerateBus(((PCIDeviceBridge)xPCIDevice).SecondaryBus, ref rDevices);
                        }

                        if (xPCIDevice.IsMultiFunction) {
                            xMaxFunctions = 8;
                        }
                    }
                }
            }
        }
    }

    [Flags]
    public enum PCICommand : short
    {
        IO = 0x1,     /* Enable response in I/O space */
        Memort = 0x2,     /* Enable response in Memory space */
        Master = 0x4,     /* Enable bus mastering */
        Special = 0x8,     /* Enable response to special cycles */
        Invalidate = 0x10,    /* Use memory write and invalidate */
        VGA_Pallete = 0x20,   /* Enable palette snooping */
        Parity = 0x40,    /* Enable parity checking */
        Wait = 0x80,    /* Enable address/data stepping */
        SERR = 0x100,   /* Enable SERR */
        Fast_Back = 0x200,   /* Enable back-to-back writes */
    }

    [Flags]
    public enum PCIStatus : int
    {
        CAP_LIST = 0x10,   /* Support Capability List */
        SUPPORT_66MHZ = 0x20,    /* Support 66 Mhz PCI 2.1 bus */
        UDF = 0x40,    /* Support User Definable Features [obsolete] */
        FAST_BACK = 0x80,    /* Accept fast-back to back */
        PARITY = 0x100,   /* Detected parity error */
        DEVSEL_MASK = 0x600,   /* DEVSEL timing */
        DEVSEL_FAST = 0x000,
        DEVSEL_MEDIUM = 0x200,
        DEVSEL_SLOW = 0x400,
        SIG_TARGET_ABORT = 0x800, /* Set on target abort */
        REC_TARGET_ABORT = 0x1000, /* Master ack of " */
        REC_MASTER_ABORT = 0x2000, /* Set on master abort */
        SIG_SYSTEM_ERROR = 0x4000, /* Set when we drive SERR */
        DETECTED_PARITY = 0x8000 /* Set on parity error */
    }


    public enum PCIHeaderType : byte
    {
        Normal = 0,
        Bridge = 1,
        Cardbus = 2
    }

    
    [Flags]
    public enum PCIBist : byte
    {
        CocdMask = 0x0f,   /* Return result */
        Start = 0x40,   /* 1 to start BIST, 2 secs or less */
        Capable = 0x80    /* 1 if BIST capable */
    }


    /// <summary>
    /// Class representing CardBus PCI Device device
    /// </summary>
    public class PCIDeviceCardBus : PCIDevice
    {
        public PCIDeviceCardBus(byte bus, byte slot, byte function)
            : base (bus,slot,function)
        {
        }

        public override int NumberOfBaseAddresses()
        {
            //get
            //{
                return 6;
            //}
        }
    }

    /// <summary>
    /// Class representing a PCI to PCI Bridge PCI Device
    /// </summary>
    public class PCIDeviceBridge : PCIDevice
    {
        public PCIDeviceBridge(byte bus, byte slot, byte function)
            : base (bus,slot,function)
        {
        }

        public override int NumberOfBaseAddresses()
        {
           //  get
           // {
                return 2;
            ////}
        }

        
        public byte PrimaryBus { get { return Read8(0x18); } set { Write8(0x18, value); } }
        public byte SecondaryBus { get { return Read8(0x19); } set { Write8(0x19, value); } }
        public byte SubordinateBus { get { return Read8(0x1a); } set { Write8(0x1a, value); } }
        public byte SecondaryLatencyTime { get { return Read8(0x1b); } set { Write8(0x1b, value); } }
    }


    /// <summary>
    /// Class representing a Normal PCI Device
    /// </summary>
    public class PCIDeviceNormal : PCIDevice
    {
        public PCIDeviceNormal(byte bus, byte slot, byte function)
            : base (bus,slot,function)
        {
        }

        public override int NumberOfBaseAddresses()
        {
            //get
            //{
                return 6;
            //}
        }

        [Obsolete("Use PciDevice.GetAddressSpace(2)")]
        public UInt32 BaseAddress2 { get { return Read32(0x18); } set { Write32(0x18, value); } }
        [Obsolete("Use PciDevice.GetAddressSpace(3)")]
        public UInt32 BaseAddress3 { get { return Read32(0x1a); } set { Write32(0x1a, value); } }
        [Obsolete("Use PciDevice.GetAddressSpace(4)")]
        public UInt32 BaseAddress4 { get { return Read32(0x20); } set { Write32(0x20, value); } }
        [Obsolete("Use PciDevice.GetAddressSpace(5)")]
        public UInt32 BaseAddress5 { get { return Read32(0x24); } set { Write32(0x24, value); } }
       
    }

    public abstract class PCIDevice
    {
        /// <summary>
        /// Gets the number of address spaces this device type has
        /// </summary>
        /// <returns></returns>
        public abstract int NumberOfBaseAddresses();
       //{
        //   get;
       //}
           

       private static string[] classtext = new string[]          
       {
        "pre pci 2.0",		// 00
        "disk",		// 01
        "network",		// 02
        "display",		// 03
        "multimedia",	// 04
        "memory",		// 05
        "bridge",		// 06
        "communication",	// 07
        "system peripheral",// 08
        "input",		// 09
        "docking station",	// 0A
        "CPU",		// 0B
        "serial bus",	// 0C
       };

        private static string[][] subclasstext = new string[][]
        { 
            new string[] { "VGA Device", "non VGA device"},
            new string[] { "SCSI" ,"IDE" , "floppy","IPI","RAID", "other" },
            new string[] { "Ethernet", "TokenRing", "FDDI" , "ATM" , "other" },
            new string[] { "VGA", "SuperVGA","XGA", "other"},
            new string[] { "video" ,"audio", "other"},
            new string[] { "RAM", "Flash memory" , "other"},
            new string[] { "CPU/PCI" ,"PCI/ISA" , "PCI/EISA" , "PCI/MCA","PCI/PCI" , "PCI/PCMCIA", "PCI/NuBus", "PCI/CardBus", "other"},
            new string[] { "serial", "parallel", "other"},
            new string[] { "PIC", "DMAC" , "timer" ,"RTC", "other"},
            new string[] { "keyboard","digitizer","mouse", "other" },
            new string[] { "generic" , "other" },
            new string[] { "386", "486","Pentium" , "P6" ,"Alpha","coproc","other" },
            new string[] { "Firewire", "ACCESS.bus" , "SSA", "USB" ,"Fiber Channel" , "other"},
        };


        public string GetClassInfo()
        {
            int cc = ClassCode;
            int sc = SubClass; 
            
            if (cc >= classtext.Length)            
                return "unknown class (" + cc.ToString() + ") / subclass (" + sc.ToString() + ")";
            
            
            if (sc >= subclasstext[cc].Length)            
                return String.Concat(classtext[cc], " / unknown subclass (", sc.ToString(), ")");
            
            return String.Concat( classtext[cc] , " / " , subclasstext[cc][sc]);
        }

        private const UInt32 PCI_BASE_ADDRESS_SPACE = 0x01;    /* 0 = memory, 1 = I/O */
        private const UInt32 PCI_BASE_ADDRESS_SPACE_IO = 0x01;
        private const UInt32 PCI_BASE_ADDRESS_SPACE_MEMORY = 0x00;
        private const UInt32 PCI_BASE_ADDRESS_MEM_TYPE_MASK = 0x06;
        private const UInt32 PCI_BASE_ADDRESS_MEM_TYPE_32 = 0x00;   /* 32 bit address */
        private const UInt32 PCI_BASE_ADDRESS_MEM_TYPE_1M = 0x02;   /* Below 1M [obsolete] */
        private const UInt32 PCI_BASE_ADDRESS_MEM_TYPE_64 = 0x04;   /* 64 bit address */
        private const UInt32 PCI_BASE_ADDRESS_MEM_PREFETCH = 0x08;  /* prefetchable? */
        private const UInt32 PCI_BASE_ADDRESS_MEM_MASK = 0xfffffff0;
        private const UInt32 PCI_BASE_ADDRESS_IO_MASK = 0xfffffffc;


        protected PCIDevice(byte bus, byte slot, byte function)
        {
            this.Bus = bus;
            this.Slot = slot;
            this.Function = function;
        }

        private bool NeedsIO = false;
        private bool NeedsMemory = false;

        private bool _NeedsLayingout = true;

        /// <summary>
        /// Creates the AddressSpaces for this device.
        /// </summary>
        private void LayoutIO()
        {
            //Console.WriteLine("Checking AdressSpaces of PCI(" + Bus + ", " + Slot + ", " + Function + ")");

            IOMaps = new Kernel.AddressSpace[NumberOfBaseAddresses()];

            for (byte i = 0; i < NumberOfBaseAddresses(); i++)
            {
                UInt32 address = GetBaseAddressInternal(i);
                SetBaseAddressInternal(i, 0xffffffff);
                UInt32 flags = GetBaseAddressInternal(i);
                SetBaseAddressInternal(i, address);

                if (address == 0)
                {
                    //Console.WriteLine("register " + i + " - none " + PCIBus.ToHex(flags,8));

                    IOMaps[i] = null;
                }
                else if ((address & PCI_BASE_ADDRESS_SPACE) == PCI_BASE_ADDRESS_SPACE_MEMORY)
                {
                    UInt32 size = ~(PCI_BASE_ADDRESS_MEM_MASK & flags)+1;

                    IOMaps[i] = new Kernel.MemoryAddressSpace(address, size);
                    //Console.WriteLine("register " + i + " - " + size + "b mem");

                    NeedsIO = true;
                }
                else if ((address & PCI_BASE_ADDRESS_SPACE) == PCI_BASE_ADDRESS_SPACE_IO)
                {
                    UInt32 size = ~(PCI_BASE_ADDRESS_IO_MASK & flags) +1;

                    IOMaps[i] = new Kernel.IOAddressSpace(address, size);
                    //Console.WriteLine("register " + i + " - " + size + "b io");

                    NeedsMemory = true;
                }
            }

            _NeedsLayingout = false;
        }

        private Kernel.AddressSpace[] IOMaps;

        /// <summary>
        /// Gets the AddressSpace object assosiated with the device
        /// </summary>
        /// <param name="index">Address Space to return</param>
        /// <returns></returns>
        public Kernel.AddressSpace GetAddressSpace(byte index)
        {
            if (index < 0 || index >= NumberOfBaseAddresses())
                throw new ArgumentOutOfRangeException("index");
            
            if (_NeedsLayingout)
                LayoutIO();

            return IOMaps[index];
        }

        /// <summary>
        /// The bus the device is on
        /// </summary>
        public byte Bus { get; private set; }
        /// <summary>
        /// The slot the device is using
        /// </summary>
        public byte Slot { get; private set; }
        /// <summary>
        /// The function number of the device
        /// </summary>
        public byte Function { get; private set; }

        /// <summary>
        /// Is this an actual device?
        /// </summary>
        public bool DeviceExists { get { return VendorID != 0xFFFF && VendorID != 0x0; } }
        /// <summary>
        /// Is this a multifunction devie?
        /// </summary>
        public bool IsMultiFunction { get { return (Read8(0x0e) & 0xf0) != 0; } }

        /// <summary>
        /// The Vendor ID
        /// </summary>
        public UInt32 VendorID { get { return Read16(0x0); } }
        /// <summary>
        /// The Device ID
        /// </summary>
        public UInt16 DeviceID { get { return Read16(0x2); } }

        /// <summary>
        /// the command register of this PCI Device
        /// </summary>
        public PCICommand Command { get { return (PCICommand)Read16(0x4); } set { Write16(0x4, (ushort)value); } }
        
        /// <summary>
        /// The Status Register of this PCI Device
        /// </summary>
        public PCIStatus Status { get { return (PCIStatus)Read16(0x6); } set { Write16(0x6, (ushort)value); } }

        /// <summary>
        /// The Revision ID of this PCI Device
        /// </summary>
        public byte RevisionID { get { return Read8(0x8); } }
        /// <summary>
        /// The Programming Interface Number of this PCI Device
        /// </summary>
        public byte ProgIF { get { return Read8(0x9); } }
        /// <summary>
        /// The Sub class of this PCI Device
        /// </summary>
        public byte SubClass { get { return Read8(0xa); } }
        /// <summary>
        /// The Class of this PCI Device
        /// </summary>
        public byte ClassCode { get { return Read8(0xb); } }

        /// <summary>
        /// The Cache Line Size of this PCI Device
        /// </summary>
        public byte CacheLineSize { get { return Read8(0x0c); } set { Write8(0x0c, value); } }
        /// <summary>
        /// The Latency Timer of this PCI Device
        /// </summary>
        public byte LatencyTimer { get { return Read8(0x0d); } set { Write8(0x0d, value); } }
        /// <summary>
        /// The header type of this PCI Device
        /// </summary>
        public byte HeaderType { get { return (byte)(Read8(0x0e) & 0xf); } }
        /// <summary>
        /// The BIST Register of this PCI Device
        /// </summary>
        public PCIBist Bist { get { return (PCIBist)Read8(0x0f); } set { Write8(0x0f, (byte)value); } }

        [Obsolete("This function should be private. Use PciDevice.GetAddressSpace(index)")]
        public UInt32 GetBaseAddress(byte index)
        {
            return Read32((byte)(0x10 + index * 4));
        }

        public UInt32 GetBaseAddressInternal(byte index)
        {
            return Read32((byte)(0x10 + index * 4));
        }
        
        private void SetBaseAddressInternal(byte index, UInt32 value)
        {
            Write32((byte)(0x10 + index * 4), value);
        }

        [Obsolete("Use PciDevice.GetAddressSpace(0)")]
        public UInt32 BaseAddress0 { get { return Read32(0x10); } set { Write32(0x10, value); } }
        [Obsolete("Use PciDevice.GetAddressSpace(1)")]
        public UInt32 BaseAddress1 { get { return Read32(0x14); } set { Write32(0x14, value); } }
         
        public byte InterruptLine { get { return Read8(0x3c); } set { Write8(0x3c, value); } }
        public byte InterruptPin { get { return Read8(0x3d); } set { Write8(0x3d, value); } }
        public byte MinGNT { get { return Read8(0x3e); } set { Write8(0x3e, value); } }
        public byte MaxLAT { get { return Read8(0x3f); } set { Write8(0x3f, value); } }

        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        /// <summary>
        /// Disables the device
        /// </summary>
        public void DisableDevice()
        {
            Command = Command & (~PCICommand.IO & ~PCICommand.Master & PCICommand.Memort);
        }

        /// <summary>
        /// enables the device
        /// </summary>
        public void EnableDevice()
        {
            Command = Command & ((NeedsIO ? PCICommand.IO : 0) & PCICommand.Master & (NeedsMemory ? PCICommand.Memort : 0));
        }

        private UInt32 GetAddress(byte aRegister)
        {
            return (UInt32)(
                // Bits 23-16
                ((UInt32)Bus << 16)
                // Bits 15-11
                | (((UInt32)Slot & 0x1F) << 11)
                // Bits 10-8
                | (((UInt32)Function & 0x07) << 8)
                // Bits 7-0
                | ((UInt32)aRegister & 0xFF)
                // Enable bit - must be set
                | 0x80000000);
        }

        protected UInt32 Read32(byte aRegister)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return Kernel.CPUBus.Read32(ConfigData);
        }

        protected UInt16 Read16(byte aRegister)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return Kernel.CPUBus.Read16(ConfigData);
        }

        protected byte Read8(byte aRegister)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return Kernel.CPUBus.Read8(ConfigData);
        }

        protected void Write32(byte aRegister, UInt32 value)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            Kernel.CPUBus.Write32(ConfigData, value);
        }

        protected void Write16(byte aRegister, UInt16 value)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            Kernel.CPUBus.Write16(ConfigData, value);
        }

        protected void Write8(byte aRegister, byte value)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            Kernel.CPUBus.Write8(ConfigData, value);
        } 
    }
}