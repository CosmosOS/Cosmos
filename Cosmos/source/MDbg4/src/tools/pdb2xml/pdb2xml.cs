//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Samples.Debugging.CorSymbolStore;

[assembly: CLSCompliant(false)]
[assembly: System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, UnmanagedCode = true)]

namespace Pdb2Xml
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Test harness for PDB2XML.");
            if (args.Length < 2)
            {
                Usage();
                return 2;
            }

            // We always take an assembly because both the reader and writer need metadata
            string asmPath = args[0];
            if (!File.Exists(asmPath))
            {
                Console.WriteLine("Error: Can't find the specified assembly: {0}", asmPath);
                return 2;
            }
            string xmlPath = args[1];

            // Parse the rest of the command-line options
            bool flagReverse = false;
            bool expandAttrs = true;
            SymbolFormat symFormat = SymbolFormat.PDB;
            for (int i = 2; i < args.Length; i++)
            {
                string arg = args[i].ToLowerInvariant();
                if (arg == "/reverse")
                {
                    flagReverse = true;
                }
                else if(arg == "/ildb")
                {
                    symFormat = SymbolFormat.ILDB;
                }
                else if (arg == "/dontexpandattrs")
                {
                    expandAttrs = false;
                }
                else
                {
                    Usage();
                    return 2;
                }
            }

            // Do the conversion
            if (flagReverse)
            {
                if (!expandAttrs)
                    Console.WriteLine("WARNING: Ignoring /dontExpandAttrs attribute when in reverse mode");
                XMLToPdb(asmPath, xmlPath, symFormat);
            }
            else
            {
                PdbToXML(asmPath, xmlPath, symFormat, expandAttrs);
            }

            return 0;
        } // end Main

        private static void Usage()
        {
            Console.WriteLine("Usage: Pdb2Xml <assembly-path> <xml-file> [/reverse] [/ildb] [/dontExpandAttrs]");
            Console.WriteLine(" assembly-path is the path to the assembly (managed exe or dll).");
            Console.WriteLine(" The corresponding PDB will be found automatically.");
            Console.WriteLine(" /reverse says to write a new PDB using the specified xml-file as input,");
            Console.WriteLine("  otherwise, the XML file is generated as output from the PDB file.");
            Console.WriteLine(" /ildb says to use the ILDB format instead of PDB, which requires ildbsymbols.dll to be available");
            Console.WriteLine(" /dontExpandAttrs says to not expand known attribute values into detailed data structures.");
        }

        private static void PdbToXML(string asmPath, string outputXml, SymbolFormat symFormat, bool expandAttrs)
        {
            Console.WriteLine("Reading the {0} symbol file for assembly: {1}", symFormat, asmPath);
            Console.WriteLine("Writing XML file: {0}", outputXml);

            // Read the PDB into a SymbolData object
            SymbolDataReader reader = new SymbolDataReader(asmPath, symFormat, expandAttrs);
            SymbolData symData = reader.ReadSymbols();

            if (symData != null)
            {
                // Use XML serialization to write out the symbol data
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Document;
                settings.Indent = true;
                XmlSerializer ser = new XmlSerializer(typeof(SymbolData));
                using (XmlWriter writer = XmlWriter.Create(outputXml, settings))
                {
                    ser.Serialize(writer, symData);
                }
            }
        }

        private static void XMLToPdb(string asmPath, string inputXml, SymbolFormat symFormat)
        {
            Console.WriteLine("Reading XML file: {0}", inputXml);
            Console.WriteLine("Writing a {0} symbol file and updating assembly: {1}", symFormat, asmPath);

            // Use XML serialization to read in the symbol data
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            XmlSerializer ser = new XmlSerializer(typeof(SymbolData));
            SymbolData symData;
            using (XmlReader reader = XmlReader.Create(inputXml, settings))
            {
                symData = (SymbolData)ser.Deserialize(reader);
            }

            // Emit PDB
            SymbolDataWriter writer = new SymbolDataWriter(asmPath, symFormat);
            writer.WritePdb(symData);
        }
    }

} 
