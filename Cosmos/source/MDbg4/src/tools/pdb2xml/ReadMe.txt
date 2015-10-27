//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

ReadMe.txt for pdb2xml project.

The pdb2xml tool uses the ISymbolReader interface to extract all the source-level debugging information and dump it into an XML file. Vice-versa, the tool can also read the generated XML and re-generate the managed pdb using the ISymbolWriter interface. Retrieving the Source-Level Debugging info from the XML file is very trivial if you run XPath queries.