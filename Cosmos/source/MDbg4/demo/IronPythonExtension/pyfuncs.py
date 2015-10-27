#----------------------------------------------------------------
# ---------------------------------------------------------------
# Library of useful functions for the MDbg Iron Python Extension
# ---------------------------------------------------------------
#----------------------------------------------------------------

# import regular expressions module
import re

#----------------------------------------------------
# Execute an Mdbg command. 
# Parameter cmd must be a string 
#----------------------------------------------------
def ExeShellCmd(cmd):
    CommandBase.ExecuteCommand(cmd);
 
#----------------------------------------------------    
# Reload all previously loaded and since modified
# python scripts.
#----------------------------------------------------
def RefreshScripts():
    ExeShellCmd("pyr all");

#----------------------------------------------------    
# Get the current MDbgEngine
#----------------------------------------------------
def GetDebugger():
    return CommandBase.Debugger;

#----------------------------------------------------
# Get the current MDbgProcess object
#----------------------------------------------------
def CurProcess(): 
    return GetDebugger().Processes.Active

#----------------------------------------------------
# Get the current MDbgThread object
#----------------------------------------------------
def CurThread(): 
    return CurProcess().Threads.Active

#----------------------------------------------------
# Get the current frame.
#----------------------------------------------------
def CurFrame(): 
    return CurThread().CurrentFrame

#----------------------------------------------------    
# Get locals for the current frame.
# Returns an array of MdbgValue[]
#----------------------------------------------------
def CurLocals(): 
    return CurFrame().Function.GetActiveLocalVars(CurFrame())

#----------------------------------------------------
# Get locals for the current frame.
# Returns an dictionary of MdbgValue[] referenced by 
# variable name
#----------------------------------------------------
def CurLocalsByName():
    locals = CurLocals()
    localsByName = {}
    for m in CurLocals():
        localsByName[m.Name] = m
    return localsByName

#----------------------------------------------------
# Print program variable.
# Parameter v must be a MDbgValue object OR
#       a variable name string
# Calls MDbgValue.GetStringValue() with depth 0
#----------------------------------------------------
def PrintVariable(v):
    if str(type(v))!="<type 'MDbgValue'>":
        v = CurProcess().ResolveVariable(str(v), CurFrame());
    print v.Name + " = " + v.GetStringValue(0, False) + " (" + v.TypeName + ")";

#----------------------------------------------------
# Print program variable.
# Parameter v must be a MDbgValue object OR
#       a variable name string
# Parameter depth must be an integer
# Calls MDbgValue.GetStringValue() with specified depth
#----------------------------------------------------    
def PrintVariable2(v, depth):
    if str(type(v))!="<type 'MDbgValue'>":
        v = CurProcess().ResolveVariable(str(v), CurFrame());
    print v.Name + " = " + v.GetStringValue(depth, False) + " (" + v.TypeName + ")";

#----------------------------------------------------
# Print locals for the current frame.
#----------------------------------------------------
def PrintLocals():
    for v in CurLocals():
        PrintVariable(v);
        
#----------------------------------------------------
# Print the callstack
#----------------------------------------------------
def PrintStack():
    i = 0
    for x in CurThread().Frames:         
        print "%d) %s" % (i, x)
        i = i + 1
    print '------------'
    
#----------------------------------------------------
# Checks if current callstack contains given function
# Parameter function must be a string
# Returns 1 if callstack contains given function,
#       0 otherwise
#----------------------------------------------------
def StackContainsFunction(function):
    searchStr = "\." + function + " \("
    regexp = re.compile(searchStr, re.IGNORECASE)
    for f in CurThread().Frames:
        m = regexp.search(f.ToString())
        if m:
            return 1
    return 0

#----------------------------------------------------
# Checks if the current frame is the given function.
# Parameter function must be a string
# Returns 1 if current frame is given function,
#       0 otherwise
#----------------------------------------------------
def InsideFunction(function):
    searchStr = "\." + function + " \("
    regexp = re.compile(searchStr, re.IGNORECASE)
    m = regexp.search(CurFrame().ToString())
    if m:
        return 1
    return 0

