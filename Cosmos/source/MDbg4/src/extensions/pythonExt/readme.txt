//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

ReadMe.txt for PythonExt project.

This is a loadable extension.  Type "load pythonext" in mdbg to get this functionality.

This extension adds ironpython scripting support to mdbg.

Note: This project is not built by default. You would need to execute the following sequence of steps in order to build this extension.

1- Download the binaries for IronPython from - http://www.codeplex.com/Wiki/View.aspx?ProjectName=IronPython
2- Add a reference to IronPython.dll from the pythonExt project in this solution.
3- Finally, build the ironpython extension project (After adding reference to ironpython.dll, you can also have this project built by default. From the Visual Studio IDE, click on the Build menu and select the configuration manager. Click checkbox for PythonExt project).
