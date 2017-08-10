
namespace Cosmos.VS.DebugEngine.AD7.Definitions
{
    //// Flags passed by the debugger to the engine to describe the desired format and execution options for properties 
    //// (locals, arguments, etc...)
    //public enum DEBUGPROP_INFO_FLAGS
    //{
    //    DEBUGPROP_INFO_FULLNAME					= 0x00000001,
    //    DEBUGPROP_INFO_NAME						= 0x00000002,
    //    DEBUGPROP_INFO_TYPE						= 0x00000004,
    //    DEBUGPROP_INFO_VALUE					= 0x00000008,
    //    DEBUGPROP_INFO_ATTRIB					= 0x00000010,
    //    DEBUGPROP_INFO_PROP						= 0x00000020,

    //    DEBUGPROP_INFO_VALUE_AUTOEXPAND			= 0x00010000,
    //    DEBUGPROP_INFO_NOFUNCEVAL				= 0x00020000,   // Tell EE not to perform ANY type of func-eval.
    //    DEBUGPROP_INFO_VALUE_RAW				= 0x00040000,   // Tell EE not to return any beautified values or members.
    //    DEBUGPROP_INFO_VALUE_NO_TOSTRING		= 0x00080000,   // Tell EE not to return any special synthesized values (ToString() for instance).
    //    DEBUGPROP_INFO_NO_NONPUBLIC_MEMBERS		= 0x00100000,   // Tell EE to return non-public members for non-user objects.

    //    DEBUGPROP_INFO_NONE					= 0x00000000,
    //    DEBUGPROP_INFO_STANDARD				= DEBUGPROP_INFO_ATTRIB | DEBUGPROP_INFO_NAME | DEBUGPROP_INFO_TYPE | DEBUGPROP_INFO_VALUE,
    //}

    //public static class DBG_ATTRIB_FLAGS
    //{
    //    public static readonly ulong DBG_ATTRIB_NONE = 0x0000000000000000;
    //    public static readonly ulong DBG_ATTRIB_ALL = 0x00000000ffffffff;

    //    // Attributes about the object itself
    //    public static readonly ulong DBG_ATTRIB_OBJ_IS_EXPANDABLE = 0x0000000000000001;
    //    public static readonly ulong DBG_ATTRIB_OBJ_HAS_ID = 0x0000000000000002;
    //    public static readonly ulong DBG_ATTRIB_OBJ_CAN_HAVE_ID = 0x0000000000000004;

    //    // Attributes about the value of the object
    //    public static readonly ulong DBG_ATTRIB_VALUE_READONLY = 0x0000000000000010;
    //    public static readonly ulong DBG_ATTRIB_VALUE_ERROR = 0x0000000000000020;
    //    public static readonly ulong DBG_ATTRIB_VALUE_SIDE_EFFECT= 0x0000000000000040;
    //    public static readonly ulong DBG_ATTRIB_OVERLOADED_CONTAINER = 0x0000000000000080;
    //    public static readonly ulong DBG_ATTRIB_VALUE_BOOLEAN = 0x0000000000000100;
    //    public static readonly ulong DBG_ATTRIB_VALUE_BOOLEAN_TRUE = 0x0000000000000200;
    //    public static readonly ulong DBG_ATTRIB_VALUE_INVALID = 0x0000000000000400;
    //    public static readonly ulong DBG_ATTRIB_VALUE_NAT = 0x0000000000000800;
    //    public static readonly ulong DBG_ATTRIB_VALUE_AUTOEXPANDED = 0x0000000000001000;
    //    public static readonly ulong DBG_ATTRIB_VALUE_TIMEOUT = 0x0000000000002000;
    //    public static readonly ulong DBG_ATTRIB_VALUE_RAW_STRING = 0x0000000000004000;
    //    public static readonly ulong DBG_ATTRIB_VALUE_CUSTOM_VIEWER = 0x0000000000008000;

    //    // Attributes about field access types for the object
    //    public static readonly ulong DBG_ATTRIB_ACCESS_NONE = 0x0000000000010000;
    //    public static readonly ulong DBG_ATTRIB_ACCESS_PUBLIC = 0x0000000000020000;
    //    public static readonly ulong DBG_ATTRIB_ACCESS_PRIVATE = 0x0000000000040000;
    //    public static readonly ulong DBG_ATTRIB_ACCESS_PROTECTED = 0x0000000000080000;
    //    public static readonly ulong DBG_ATTRIB_ACCESS_FINAL = 0x0000000000100000;
    //    public static readonly ulong DBG_ATTRIB_ACCESS_ALL = 0x00000000001f0000;

    //    // Attributes for the storage types of the object
    //    public static readonly ulong DBG_ATTRIB_STORAGE_NONE = 0x0000000001000000;
    //    public static readonly ulong DBG_ATTRIB_STORAGE_GLOBAL = 0x0000000002000000;
    //    public static readonly ulong DBG_ATTRIB_STORAGE_STATIC = 0x0000000004000000;
    //    public static readonly ulong DBG_ATTRIB_STORAGE_REGISTER = 0x0000000008000000;
    //    public static readonly ulong DBG_ATTRIB_STORAGE_ALL= 0x000000000f000000;

    //    // Attributes for the type modifiers on the object
    //    public static readonly ulong DBG_ATTRIB_TYPE_NONE = 0x0000000100000000;
    //    public static readonly ulong DBG_ATTRIB_TYPE_VIRTUAL = 0x0000000200000000;
    //    public static readonly ulong DBG_ATTRIB_TYPE_CONSTANT = 0x0000000400000000;
    //    public static readonly ulong DBG_ATTRIB_TYPE_SYNCHRONIZED = 0x0000000800000000;
    //    public static readonly ulong DBG_ATTRIB_TYPE_VOLATILE = 0x0000001000000000;
    //    public static readonly ulong DBG_ATTRIB_TYPE_ALL = 0x0000001f00000000;

    //    // Attributes that describe the type of object
    //    public static readonly ulong DBG_ATTRIB_DATA = 0x0000010000000000;
    //    public static readonly ulong DBG_ATTRIB_METHOD = 0x0000020000000000;
    //    public static readonly ulong DBG_ATTRIB_PROPERTY = 0x0000040000000000;
    //    public static readonly ulong DBG_ATTRIB_CLASS = 0x0000080000000000;
    //    public static readonly ulong DBG_ATTRIB_BASECLASS = 0x0000100000000000;
    //    public static readonly ulong DBG_ATTRIB_INTERFACE = 0x0000200000000000;
    //    public static readonly ulong DBG_ATTRIB_INNERCLASS = 0x0000400000000000;
    //    public static readonly ulong DBG_ATTRIB_MOSTDERIVED = 0x0000800000000000;
    //    public static readonly ulong DBG_ATTRIB_CHILD_ALL = 0x0000ff0000000000;

    //}
}   