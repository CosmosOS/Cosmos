//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

ReadMe.txt for corapi project.

The corapi project represents a managed view of Debugging API. It covers all the functionality of native COM API, and makes its use simpler at the same time.
For every COM object there is a managed class that managed code is using instead.
Following are main differences between native and managed versions of the API:
- Managed version is using properties instead of functions calls for certain methods. 
- Managed version has only one class declaring all methods as opposed to native version that has typically 2 interfaces to access functions. E.g. ICorDebugProcess and ICorDebugProcess2.
- Managed version dispatches debugging events as managed events to individual CorProcess classes. Native version needs to register a global callback interfaces.

These wrappers should not require any modifications unless you find a bug.
