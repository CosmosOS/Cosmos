//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

ReadMe.txt for mdbgeng project.

This layer provides a synchronized debugger programming model.  
Access using methods like Next, Step, Go.
Methods return when the program is in stopped state again.
You can specify when you want to stop (StopOnException, StopOnModule)
The reason why it is stopped is saved in StopReason property.

This is a library that could be used to write a different managed debugger than mdbg.