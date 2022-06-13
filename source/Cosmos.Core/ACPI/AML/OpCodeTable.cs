namespace ACPILib.AML
{
	public class OpCodeTable
	{
		public const byte Unkn = 0x6B;
		public const byte Text = 0x6C;
		public const byte Prfx = 0x6D;

		public static byte[] ShortOpCodeIndexes =
		{
/*              0     1     2     3     4     5     6     7  */
/*              8     9     A     B     C     D     E     F  */
/* 0x00 */    0x00, 0x01, Unkn, Unkn, Unkn, Unkn, 0x02, Unkn,
/* 0x08 */    0x03, Unkn, 0x04, 0x05, 0x06, 0x07, 0x6E, Unkn,
/* 0x10 */    0x08, 0x09, 0x0a, 0x6F, 0x0b, 0x81, Unkn, Unkn,
/* 0x18 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x20 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x28 */    Unkn, Unkn, Unkn, Unkn, Unkn, 0x63, Prfx, Prfx,
/* 0x30 */    0x67, 0x66, 0x68, 0x65, 0x69, 0x64, 0x6A, 0x7D,
/* 0x38 */    0x7F, 0x80, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x40 */    Unkn, Text, Text, Text, Text, Text, Text, Text,
/* 0x48 */    Text, Text, Text, Text, Text, Text, Text, Text,
/* 0x50 */    Text, Text, Text, Text, Text, Text, Text, Text,
/* 0x58 */    Text, Text, Text, Unkn, Prfx, Unkn, Prfx, Text,
/* 0x60 */    0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13,
/* 0x68 */    0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, Unkn,
/* 0x70 */    0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22,
/* 0x78 */    0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a,
/* 0x80 */    0x2b, 0x2c, 0x2d, 0x2e, 0x70, 0x71, 0x2f, 0x30,
/* 0x88 */    0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x72,
/* 0x90 */    0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x73, 0x74,
/* 0x98 */    0x75, 0x76, Unkn, Unkn, 0x77, 0x78, 0x79, 0x7A,
/* 0xA0 */    0x3e, 0x3f, 0x40, 0x41, 0x42, 0x43, 0x60, 0x61,
/* 0xA8 */    0x62, 0x82, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xB0 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xB8 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xC0 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xC8 */    Unkn, Unkn, Unkn, Unkn, 0x44, Unkn, Unkn, Unkn,
/* 0xD0 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xD8 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xE0 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xE8 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xF0 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0xF8 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, 0x45,
		};

		public static byte[] LongOpCodeIndexes =
		{
/*              0     1     2     3     4     5     6     7  */
/*              8     9     A     B     C     D     E     F  */
/* 0x00 */    Unkn, 0x46, 0x47, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x08 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x10 */    Unkn, Unkn, 0x48, 0x49, Unkn, Unkn, Unkn, Unkn,
/* 0x18 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, 0x7B,
/* 0x20 */    0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51,
/* 0x28 */    0x52, 0x53, 0x54, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x30 */    0x55, 0x56, 0x57, 0x7e, Unkn, Unkn, Unkn, Unkn,
/* 0x38 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x40 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x48 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x50 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x58 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x60 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x68 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x70 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x78 */    Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn, Unkn,
/* 0x80 */    0x58, 0x59, 0x5a, 0x5b, 0x5c, 0x5d, 0x5e, 0x5f,
/* 0x88 */    0x7C,
		};

		public static OpCode[] OpCodes = new OpCode[]
{
/* Index             Name								 Parser Args                          Interpreter Args                            ObjectType						 Class						  Type								Flags */
/* 00 */ new OpCode(OpCodeEnum.Zero,					 ParseArgs.ARGP_ZERO_OP,              InterpreterArgs.ARGI_ZERO_OP,               ObjectTypeEnum.Integer,           OpCodeClass.Argument,        OpCodeType.Constant,              OpCodeFlags.Constant),
/* 01 */ new OpCode(OpCodeEnum.One,						 ParseArgs.ARGP_ONE_OP,               InterpreterArgs.ARGI_ONE_OP,                ObjectTypeEnum.Integer,           OpCodeClass.Argument,        OpCodeType.Constant,              OpCodeFlags.Constant),
/* 02 */ new OpCode(OpCodeEnum.Alias,					 ParseArgs.ARGP_ALIAS_OP,             InterpreterArgs.ARGI_ALIAS_OP,              ObjectTypeEnum.LocalAlias,        OpCodeClass.NamedObject,     OpCodeType.NamedSimple,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 03 */ new OpCode(OpCodeEnum.Name,					 ParseArgs.ARGP_NAME_OP,              InterpreterArgs.ARGI_NAME_OP,               ObjectTypeEnum.Any,               OpCodeClass.NamedObject,     OpCodeType.NamedComplex,          OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 04 */ new OpCode(OpCodeEnum.Byte,					 ParseArgs.ARGP_BYTE_OP,              InterpreterArgs.ARGI_BYTE_OP,               ObjectTypeEnum.Integer,           OpCodeClass.Argument,        OpCodeType.Literal,               OpCodeFlags.Constant),
/* 05 */ new OpCode(OpCodeEnum.Word,					 ParseArgs.ARGP_WORD_OP,              InterpreterArgs.ARGI_WORD_OP,               ObjectTypeEnum.Integer,           OpCodeClass.Argument,        OpCodeType.Literal,               OpCodeFlags.Constant),
/* 06 */ new OpCode(OpCodeEnum.DWord,					 ParseArgs.ARGP_DWORD_OP,             InterpreterArgs.ARGI_DWORD_OP,              ObjectTypeEnum.Integer,           OpCodeClass.Argument,        OpCodeType.Literal,               OpCodeFlags.Constant),
/* 07 */ new OpCode(OpCodeEnum.String,					 ParseArgs.ARGP_STRING_OP,            InterpreterArgs.ARGI_STRING_OP,             ObjectTypeEnum.String,            OpCodeClass.Argument,        OpCodeType.Literal,               OpCodeFlags.Constant),
/* 08 */ new OpCode(OpCodeEnum.Scope,					 ParseArgs.ARGP_SCOPE_OP,             InterpreterArgs.ARGI_SCOPE_OP,              ObjectTypeEnum.LocalScope,        OpCodeClass.NamedObject,     OpCodeType.NamedNoObject,         OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 09 */ new OpCode(OpCodeEnum.Buffer,					 ParseArgs.ARGP_BUFFER_OP,            InterpreterArgs.ARGI_BUFFER_OP,             ObjectTypeEnum.Buffer,            OpCodeClass.Create,          OpCodeType.CreateObject,          OpCodeFlags.HasArguments | OpCodeFlags.Defer | OpCodeFlags.Constant),
/* 0A */ new OpCode(OpCodeEnum.Package,					 ParseArgs.ARGP_PACKAGE_OP,           InterpreterArgs.ARGI_PACKAGE_OP,            ObjectTypeEnum.Package,           OpCodeClass.Create,          OpCodeType.CreateObject,          OpCodeFlags.HasArguments | OpCodeFlags.Defer | OpCodeFlags.Constant),
/* 0B */ new OpCode(OpCodeEnum.Method,					 ParseArgs.ARGP_METHOD_OP,            InterpreterArgs.ARGI_METHOD_OP,             ObjectTypeEnum.Method,            OpCodeClass.NamedObject,     OpCodeType.NamedComplex,          OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named | OpCodeFlags.Defer),
/* 0C */ new OpCode(OpCodeEnum.Local0,					 ParseArgs.ARGP_LOCAL0,               InterpreterArgs.ARGI_LOCAL0,                ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.LocalVariable,         0),
/* 0D */ new OpCode(OpCodeEnum.Local1,					 ParseArgs.ARGP_LOCAL1,               InterpreterArgs.ARGI_LOCAL1,                ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.LocalVariable,         0),
/* 0E */ new OpCode(OpCodeEnum.Local2,					 ParseArgs.ARGP_LOCAL2,               InterpreterArgs.ARGI_LOCAL2,                ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.LocalVariable,         0),
/* 0F */ new OpCode(OpCodeEnum.Local3,					 ParseArgs.ARGP_LOCAL3,               InterpreterArgs.ARGI_LOCAL3,                ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.LocalVariable,         0),
/* 10 */ new OpCode(OpCodeEnum.Local4,					 ParseArgs.ARGP_LOCAL4,               InterpreterArgs.ARGI_LOCAL4,                ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.LocalVariable,         0),
/* 11 */ new OpCode(OpCodeEnum.Local5,					 ParseArgs.ARGP_LOCAL5,               InterpreterArgs.ARGI_LOCAL5,                ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.LocalVariable,         0),
/* 12 */ new OpCode(OpCodeEnum.Local6,					 ParseArgs.ARGP_LOCAL6,               InterpreterArgs.ARGI_LOCAL6,                ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.LocalVariable,         0),
/* 13 */ new OpCode(OpCodeEnum.Local7,					 ParseArgs.ARGP_LOCAL7,               InterpreterArgs.ARGI_LOCAL7,                ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.LocalVariable,         0),
/* 14 */ new OpCode(OpCodeEnum.Arg0,					 ParseArgs.ARGP_ARG0,                 InterpreterArgs.ARGI_ARG0,                  ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.MethodArgument,        0),
/* 15 */ new OpCode(OpCodeEnum.Arg1,					 ParseArgs.ARGP_ARG1,                 InterpreterArgs.ARGI_ARG1,                  ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.MethodArgument,        0),
/* 16 */ new OpCode(OpCodeEnum.Arg2,					 ParseArgs.ARGP_ARG2,                 InterpreterArgs.ARGI_ARG2,                  ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.MethodArgument,        0),
/* 17 */ new OpCode(OpCodeEnum.Arg3,					 ParseArgs.ARGP_ARG3,                 InterpreterArgs.ARGI_ARG3,                  ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.MethodArgument,        0),
/* 18 */ new OpCode(OpCodeEnum.Arg4,					 ParseArgs.ARGP_ARG4,                 InterpreterArgs.ARGI_ARG4,                  ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.MethodArgument,        0),
/* 19 */ new OpCode(OpCodeEnum.Arg5,					 ParseArgs.ARGP_ARG5,                 InterpreterArgs.ARGI_ARG5,                  ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.MethodArgument,        0),
/* 1A */ new OpCode(OpCodeEnum.Arg6,					 ParseArgs.ARGP_ARG6,                 InterpreterArgs.ARGI_ARG6,                  ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.MethodArgument,        0),
/* 1B */ new OpCode(OpCodeEnum.Store,					 ParseArgs.ARGP_STORE_OP,             InterpreterArgs.ARGI_STORE_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R),
/* 1C */ new OpCode(OpCodeEnum.ReferenceOf,              ParseArgs.ARGP_REF_OF_OP,            InterpreterArgs.ARGI_REF_OF_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_1R),
/* 1D */ new OpCode(OpCodeEnum.Add,						 ParseArgs.ARGP_ADD_OP,               InterpreterArgs.ARGI_ADD_OP,                ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 1E */ new OpCode(OpCodeEnum.Concatenate,				 ParseArgs.ARGP_CONCAT_OP,            InterpreterArgs.ARGI_CONCAT_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Constant),
/* 1F */ new OpCode(OpCodeEnum.Subtract,				 ParseArgs.ARGP_SUBTRACT_OP,          InterpreterArgs.ARGI_SUBTRACT_OP,           ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 20 */ new OpCode(OpCodeEnum.Increment,				 ParseArgs.ARGP_INCREMENT_OP,         InterpreterArgs.ARGI_INCREMENT_OP,          ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_1R | OpCodeFlags.Constant),
/* 21 */ new OpCode(OpCodeEnum.Decrement,				 ParseArgs.ARGP_DECREMENT_OP,         InterpreterArgs.ARGI_DECREMENT_OP,          ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_1R | OpCodeFlags.Constant),
/* 22 */ new OpCode(OpCodeEnum.Multiply,				 ParseArgs.ARGP_MULTIPLY_OP,          InterpreterArgs.ARGI_MULTIPLY_OP,           ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 23 */ new OpCode(OpCodeEnum.Divide,					 ParseArgs.ARGP_DIVIDE_OP,            InterpreterArgs.ARGI_DIVIDE_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_2T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_2T_1R | OpCodeFlags.Constant),
/* 24 */ new OpCode(OpCodeEnum.ShiftLeft,				 ParseArgs.ARGP_SHIFT_LEFT_OP,        InterpreterArgs.ARGI_SHIFT_LEFT_OP,         ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 25 */ new OpCode(OpCodeEnum.ShiftRight,				 ParseArgs.ARGP_SHIFT_RIGHT_OP,       InterpreterArgs.ARGI_SHIFT_RIGHT_OP,        ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 26 */ new OpCode(OpCodeEnum.BitAnd,					 ParseArgs.ARGP_BIT_AND_OP,           InterpreterArgs.ARGI_BIT_AND_OP,            ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 27 */ new OpCode(OpCodeEnum.BitNand,					 ParseArgs.ARGP_BIT_NAND_OP,          InterpreterArgs.ARGI_BIT_NAND_OP,           ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 28 */ new OpCode(OpCodeEnum.BitOr,					 ParseArgs.ARGP_BIT_OR_OP,            InterpreterArgs.ARGI_BIT_OR_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 29 */ new OpCode(OpCodeEnum.BitNor,					 ParseArgs.ARGP_BIT_NOR_OP,           InterpreterArgs.ARGI_BIT_NOR_OP,            ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 2A */ new OpCode(OpCodeEnum.BitXor,					 ParseArgs.ARGP_BIT_XOR_OP,           InterpreterArgs.ARGI_BIT_XOR_OP,            ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Math | OpCodeFlags.Constant),
/* 2B */ new OpCode(OpCodeEnum.BitNor,					 ParseArgs.ARGP_BIT_NOT_OP,           InterpreterArgs.ARGI_BIT_NOT_OP,            ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 2C */ new OpCode(OpCodeEnum.FindSetLeftBit,			 ParseArgs.ARGP_FIND_SET_LEFT_BIT_OP, InterpreterArgs.ARGI_FIND_SET_LEFT_BIT_OP,  ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 2D */ new OpCode(OpCodeEnum.FindSetRightBit,			 ParseArgs.ARGP_FIND_SET_RIGHT_BIT_OP,InterpreterArgs.ARGI_FIND_SET_RIGHT_BIT_OP, ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 2E */ new OpCode(OpCodeEnum.DereferenceOf,            ParseArgs.ARGP_DEREF_OF_OP,          InterpreterArgs.ARGI_DEREF_OF_OP,           ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_1R),
/* 2F */ new OpCode(OpCodeEnum.Notify,					 ParseArgs.ARGP_NOTIFY_OP,            InterpreterArgs.ARGI_NOTIFY_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_0T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_0T_0R),
/* 30 */ new OpCode(OpCodeEnum.SizeOf,					 ParseArgs.ARGP_SIZE_OF_OP,           InterpreterArgs.ARGI_SIZE_OF_OP,            ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_1R | OpCodeFlags.NoOperandResolve),
/* 31 */ new OpCode(OpCodeEnum.Index,					 ParseArgs.ARGP_INDEX_OP,             InterpreterArgs.ARGI_INDEX_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R),
/* 32 */ new OpCode(OpCodeEnum.Match,					 ParseArgs.ARGP_MATCH_OP,             InterpreterArgs.ARGI_MATCH_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_6A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_6A_0T_1R | OpCodeFlags.Constant),
/* 33 */ new OpCode(OpCodeEnum.CreateDWordField,		 ParseArgs.ARGP_CREATE_DWORD_FIELD_OP,InterpreterArgs.ARGI_CREATE_DWORD_FIELD_OP, ObjectTypeEnum.BufferField,       OpCodeClass.Create,          OpCodeType.CreateField,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSNode | OpCodeFlags.Defer | OpCodeFlags.Create),
/* 34 */ new OpCode(OpCodeEnum.CreateWordField,			 ParseArgs.ARGP_CREATE_WORD_FIELD_OP, InterpreterArgs.ARGI_CREATE_WORD_FIELD_OP,  ObjectTypeEnum.BufferField,       OpCodeClass.Create,          OpCodeType.CreateField,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSNode | OpCodeFlags.Defer | OpCodeFlags.Create),
/* 35 */ new OpCode(OpCodeEnum.CreateByteField,			 ParseArgs.ARGP_CREATE_BYTE_FIELD_OP, InterpreterArgs.ARGI_CREATE_BYTE_FIELD_OP,  ObjectTypeEnum.BufferField,       OpCodeClass.Create,          OpCodeType.CreateField,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSNode | OpCodeFlags.Defer | OpCodeFlags.Create),
/* 36 */ new OpCode(OpCodeEnum.CreateBitField,			 ParseArgs.ARGP_CREATE_BIT_FIELD_OP,  InterpreterArgs.ARGI_CREATE_BIT_FIELD_OP,   ObjectTypeEnum.BufferField,       OpCodeClass.Create,          OpCodeType.CreateField,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSNode | OpCodeFlags.Defer | OpCodeFlags.Create),
/* 37 */ new OpCode(OpCodeEnum.ObjectType,				 ParseArgs.ARGP_OBJECT_TYPE_OP,       InterpreterArgs.ARGI_OBJECT_TYPE_OP,        ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_1R | OpCodeFlags.NoOperandResolve),
/* 38 */ new OpCode(OpCodeEnum.LogicalAnd,               ParseArgs.ARGP_LAND_OP,              InterpreterArgs.ARGI_LAND_OP,               ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_0T_1R | OpCodeFlags.LogicalNumeric | OpCodeFlags.Constant),
/* 39 */ new OpCode(OpCodeEnum.LogicalOr,                ParseArgs.ARGP_LOR_OP,               InterpreterArgs.ARGI_LOR_OP,                ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_0T_1R | OpCodeFlags.LogicalNumeric | OpCodeFlags.Constant),
/* 3A */ new OpCode(OpCodeEnum.LogicalNot,               ParseArgs.ARGP_LNOT_OP,              InterpreterArgs.ARGI_LNOT_OP,               ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_1R | OpCodeFlags.Constant),
/* 3B */ new OpCode(OpCodeEnum.LogicalEqual,             ParseArgs.ARGP_LEQUAL_OP,            InterpreterArgs.ARGI_LEQUAL_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_0T_1R | OpCodeFlags.Logical | OpCodeFlags.Constant),
/* 3C */ new OpCode(OpCodeEnum.LogicalGreater,           ParseArgs.ARGP_LGREATER_OP,          InterpreterArgs.ARGI_LGREATER_OP,           ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_0T_1R | OpCodeFlags.Logical | OpCodeFlags.Constant),
/* 3D */ new OpCode(OpCodeEnum.LogicalLess,              ParseArgs.ARGP_LLESS_OP,             InterpreterArgs.ARGI_LLESS_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_0T_1R | OpCodeFlags.Logical | OpCodeFlags.Constant),
/* 3E */ new OpCode(OpCodeEnum.If,						 ParseArgs.ARGP_IF_OP,                InterpreterArgs.ARGI_IF_OP,                 ObjectTypeEnum.Any,               OpCodeClass.Control,         OpCodeType.Control,               OpCodeFlags.HasArguments),
/* 3F */ new OpCode(OpCodeEnum.Else,					 ParseArgs.ARGP_ELSE_OP,              InterpreterArgs.ARGI_ELSE_OP,               ObjectTypeEnum.Any,               OpCodeClass.Control,         OpCodeType.Control,               OpCodeFlags.HasArguments),
/* 40 */ new OpCode(OpCodeEnum.While,					 ParseArgs.ARGP_WHILE_OP,             InterpreterArgs.ARGI_WHILE_OP,              ObjectTypeEnum.Any,               OpCodeClass.Control,         OpCodeType.Control,               OpCodeFlags.HasArguments),
/* 41 */ new OpCode(OpCodeEnum.NoOp,					 ParseArgs.ARGP_NOOP_OP,              InterpreterArgs.ARGI_NOOP_OP,               ObjectTypeEnum.Any,               OpCodeClass.Control,         OpCodeType.Control,               0),
/* 42 */ new OpCode(OpCodeEnum.Return,					 ParseArgs.ARGP_RETURN_OP,            InterpreterArgs.ARGI_RETURN_OP,             ObjectTypeEnum.Any,               OpCodeClass.Control,         OpCodeType.Control,               OpCodeFlags.HasArguments),
/* 43 */ new OpCode(OpCodeEnum.Break,					 ParseArgs.ARGP_BREAK_OP,             InterpreterArgs.ARGI_BREAK_OP,              ObjectTypeEnum.Any,               OpCodeClass.Control,         OpCodeType.Control,               0),
/* 44 */ new OpCode(OpCodeEnum.Breakpoint,				 ParseArgs.ARGP_BREAK_POINT_OP,       InterpreterArgs.ARGI_BREAK_POINT_OP,        ObjectTypeEnum.Any,               OpCodeClass.Control,         OpCodeType.Control,               0),
/* 45 */ new OpCode(OpCodeEnum.Ones,					 ParseArgs.ARGP_ONES_OP,              InterpreterArgs.ARGI_ONES_OP,               ObjectTypeEnum.Integer,           OpCodeClass.Argument,        OpCodeType.Constant,              OpCodeFlags.Constant),

/* Prefixed opcodes (Two-byte opcodes with a prefix op) */

/* 46 */ new OpCode(OpCodeEnum.Mutex,					 ParseArgs.ARGP_MUTEX_OP,             InterpreterArgs.ARGI_MUTEX_OP,              ObjectTypeEnum.Mutex,             OpCodeClass.NamedObject,     OpCodeType.NamedSimple,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 47 */ new OpCode(OpCodeEnum.Event,					 ParseArgs.ARGP_EVENT_OP,             InterpreterArgs.ARGI_EVENT_OP,              ObjectTypeEnum.Event,             OpCodeClass.NamedObject,     OpCodeType.NamedSimple,           OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named ),
/* 48 */ new OpCode(OpCodeEnum.ConditionalReferenceOf,   ParseArgs.ARGP_COND_REF_OF_OP,       InterpreterArgs.ARGI_COND_REF_OF_OP,        ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R),
/* 49 */ new OpCode(OpCodeEnum.CreateField,				 ParseArgs.ARGP_CREATE_FIELD_OP,      InterpreterArgs.ARGI_CREATE_FIELD_OP,       ObjectTypeEnum.BufferField,       OpCodeClass.Create,          OpCodeType.CreateField,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSNode | OpCodeFlags.Defer | OpCodeFlags.Field | OpCodeFlags.Create),
/* 4A */ new OpCode(OpCodeEnum.Load,					 ParseArgs.ARGP_LOAD_OP,              InterpreterArgs.ARGI_LOAD_OP,               ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_0R),
/* 4B */ new OpCode(OpCodeEnum.Stall,					 ParseArgs.ARGP_STALL_OP,             InterpreterArgs.ARGI_STALL_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_0R),
/* 4C */ new OpCode(OpCodeEnum.Sleep,					 ParseArgs.ARGP_SLEEP_OP,             InterpreterArgs.ARGI_SLEEP_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_0R),
/* 4D */ new OpCode(OpCodeEnum.Acquire,					 ParseArgs.ARGP_ACQUIRE_OP,           InterpreterArgs.ARGI_ACQUIRE_OP,            ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_0T_1R),
/* 4E */ new OpCode(OpCodeEnum.Signal,					 ParseArgs.ARGP_SIGNAL_OP,            InterpreterArgs.ARGI_SIGNAL_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_0R),
/* 4F */ new OpCode(OpCodeEnum.Wait,					 ParseArgs.ARGP_WAIT_OP,              InterpreterArgs.ARGI_WAIT_OP,               ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_0T_1R),
/* 50 */ new OpCode(OpCodeEnum.Reset,					 ParseArgs.ARGP_RESET_OP,             InterpreterArgs.ARGI_RESET_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_0R),
/* 51 */ new OpCode(OpCodeEnum.Release,					 ParseArgs.ARGP_RELEASE_OP,           InterpreterArgs.ARGI_RELEASE_OP,            ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_0R),
/* 52 */ new OpCode(OpCodeEnum.FromBCD,					 ParseArgs.ARGP_FROM_BCD_OP,          InterpreterArgs.ARGI_FROM_BCD_OP,           ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 53 */ new OpCode(OpCodeEnum.ToBCD,					 ParseArgs.ARGP_TO_BCD_OP,            InterpreterArgs.ARGI_TO_BCD_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 54 */ new OpCode(OpCodeEnum.Unload,					 ParseArgs.ARGP_UNLOAD_OP,            InterpreterArgs.ARGI_UNLOAD_OP,             ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_0T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_0T_0R),
/* 55 */ new OpCode(OpCodeEnum.Revision,				 ParseArgs.ARGP_REVISION_OP,          InterpreterArgs.ARGI_REVISION_OP,           ObjectTypeEnum.Integer,           OpCodeClass.Argument,        OpCodeType.Constant,              0),
/* 56 */ new OpCode(OpCodeEnum.Debug,					 ParseArgs.ARGP_DEBUG_OP,             InterpreterArgs.ARGI_DEBUG_OP,              ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.Constant,              0),
/* 57 */ new OpCode(OpCodeEnum.Fatal,					 ParseArgs.ARGP_FATAL_OP,             InterpreterArgs.ARGI_FATAL_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_3A_0T_0R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_3A_0T_0R),
/* 58 */ new OpCode(OpCodeEnum.Region,					 ParseArgs.ARGP_REGION_OP,            InterpreterArgs.ARGI_REGION_OP,             ObjectTypeEnum.Region,            OpCodeClass.NamedObject,     OpCodeType.NamedComplex,          OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named | OpCodeFlags.Defer),
/* 59 */ new OpCode(OpCodeEnum.Field,					 ParseArgs.ARGP_FIELD_OP,             InterpreterArgs.ARGI_FIELD_OP,              ObjectTypeEnum.Any,               OpCodeClass.NamedObject,     OpCodeType.NamedField,            OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.Field),
/* 5A */ new OpCode(OpCodeEnum.Device,					 ParseArgs.ARGP_DEVICE_OP,            InterpreterArgs.ARGI_DEVICE_OP,             ObjectTypeEnum.Device,            OpCodeClass.NamedObject,     OpCodeType.NamedNoObject,         OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 5B */ new OpCode(OpCodeEnum.Processor,				 ParseArgs.ARGP_PROCESSOR_OP,         InterpreterArgs.ARGI_PROCESSOR_OP,          ObjectTypeEnum.Processor,         OpCodeClass.NamedObject,     OpCodeType.NamedSimple,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 5C */ new OpCode(OpCodeEnum.PowerResource,			 ParseArgs.ARGP_POWER_RES_OP,         InterpreterArgs.ARGI_POWER_RES_OP,          ObjectTypeEnum.Power,             OpCodeClass.NamedObject,     OpCodeType.NamedSimple,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 5D */ new OpCode(OpCodeEnum.ThermalZone,				 ParseArgs.ARGP_THERMAL_ZONE_OP,      InterpreterArgs.ARGI_THERMAL_ZONE_OP,       ObjectTypeEnum.Thermal,           OpCodeClass.NamedObject,     OpCodeType.NamedNoObject,         OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 5E */ new OpCode(OpCodeEnum.IndexField,				 ParseArgs.ARGP_INDEX_FIELD_OP,       InterpreterArgs.ARGI_INDEX_FIELD_OP,        ObjectTypeEnum.Any,               OpCodeClass.NamedObject,     OpCodeType.NamedField,            OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.Field),
/* 5F */ new OpCode(OpCodeEnum.BankField,				 ParseArgs.ARGP_BANK_FIELD_OP,        InterpreterArgs.ARGI_BANK_FIELD_OP,         ObjectTypeEnum.LocalBankField,    OpCodeClass.NamedObject,     OpCodeType.NamedField,            OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.Field | OpCodeFlags.Defer),

/* Internal opcodes that map to invalid AML opcodes */

/* 60 */ new OpCode(OpCodeEnum.LogicalNotEqual,          ParseArgs.ARGP_LNOTEQUAL_OP,         InterpreterArgs.ARGI_LNOTEQUAL_OP,          ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 OpCodeFlags.HasArguments | OpCodeFlags.Constant),
/* 61 */ new OpCode(OpCodeEnum.LogicalLessEqual,         ParseArgs.ARGP_LLESSEQUAL_OP,        InterpreterArgs.ARGI_LLESSEQUAL_OP,         ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 OpCodeFlags.HasArguments | OpCodeFlags.Constant),
/* 62 */ new OpCode(OpCodeEnum.LogicalGreaterEqual,      ParseArgs.ARGP_LGREATEREQUAL_OP,     InterpreterArgs.ARGI_LGREATEREQUAL_OP,      ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 OpCodeFlags.HasArguments | OpCodeFlags.Constant),
/* 63 */ new OpCode(OpCodeEnum.NamePath,				 ParseArgs.ARGP_NAMEPATH_OP,          InterpreterArgs.ARGI_NAMEPATH_OP,           ObjectTypeEnum.LocalReference,    OpCodeClass.Argument,        OpCodeType.Literal,               OpCodeFlags.NSObject | OpCodeFlags.NSNode ),
/* 64 */ new OpCode(OpCodeEnum.MethodCall,				 ParseArgs.ARGP_METHODCALL_OP,        InterpreterArgs.ARGI_METHODCALL_OP,         ObjectTypeEnum.Method,            OpCodeClass.MethodCall,      OpCodeType.MethodCall,            OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSNode),
/* 65 */ new OpCode(OpCodeEnum.ByteList,				 ParseArgs.ARGP_BYTELIST_OP,          InterpreterArgs.ARGI_BYTELIST_OP,           ObjectTypeEnum.Any,               OpCodeClass.Argument,        OpCodeType.Literal,               0),
/* 66 */ new OpCode(OpCodeEnum.ReservedField,			 ParseArgs.ARGP_RESERVEDFIELD_OP,     InterpreterArgs.ARGI_RESERVEDFIELD_OP,      ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 0),
/* 67 */ new OpCode(OpCodeEnum.NamedField,				 ParseArgs.ARGP_NAMEDFIELD_OP,        InterpreterArgs.ARGI_NAMEDFIELD_OP,         ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named ),
/* 68 */ new OpCode(OpCodeEnum.AccessField,				 ParseArgs.ARGP_ACCESSFIELD_OP,       InterpreterArgs.ARGI_ACCESSFIELD_OP,        ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 0),
/* 69 */ new OpCode(OpCodeEnum.String,					 ParseArgs.ARGP_STATICSTRING_OP,      InterpreterArgs.ARGI_STATICSTRING_OP,       ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 0),
/* 6A */ new OpCode(OpCodeEnum.ReturnValue,				 ParseArgs.ARG_NONE,                  InterpreterArgs.ARG_NONE,                   ObjectTypeEnum.Any,               OpCodeClass.ReturnValue,     OpCodeType.Return,                OpCodeFlags.HasArguments | OpCodeFlags.HasReturnValue),
/* 6B */ new OpCode("-UNKNOWN_OP-",					     ParseArgs.ARG_NONE,                  InterpreterArgs.ARG_NONE,                   ObjectTypeEnum.Invalid,           OpCodeClass.ClassUnknown,    OpCodeType.Bogus,                 OpCodeFlags.HasArguments),
/* 6C */ new OpCode("-ASCII_ONLY-",						 ParseArgs.ARG_NONE,                  InterpreterArgs.ARG_NONE,                   ObjectTypeEnum.Any,               OpCodeClass.ASCII,           OpCodeType.Bogus,                 OpCodeFlags.HasArguments),
/* 6D */ new OpCode("-PREFIX_ONLY-",					 ParseArgs.ARG_NONE,                  InterpreterArgs.ARG_NONE,                   ObjectTypeEnum.Any,               OpCodeClass.Prefix,          OpCodeType.Bogus,                 OpCodeFlags.HasArguments),

/* ACPI 2.0 opcodes */

/* 6E */ new OpCode(OpCodeEnum.QWord,					 ParseArgs.ARGP_QWORD_OP,             InterpreterArgs.ARGI_QWORD_OP,              ObjectTypeEnum.Integer,           OpCodeClass.Argument,        OpCodeType.Literal,               OpCodeFlags.Constant),
/* 6F */ new OpCode(OpCodeEnum.Package, /* Var */		 ParseArgs.ARGP_VAR_PACKAGE_OP,       InterpreterArgs.ARGI_VAR_PACKAGE_OP,        ObjectTypeEnum.Package,           OpCodeClass.Create,          OpCodeType.CreateObject,          OpCodeFlags.HasArguments | OpCodeFlags.Defer),
/* 70 */ new OpCode(OpCodeEnum.ConcatenateTemplate,		 ParseArgs.ARGP_CONCAT_RES_OP,    InterpreterArgs.ARGI_CONCAT_RES_OP,         ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Constant),
/* 71 */ new OpCode(OpCodeEnum.Mod,						 ParseArgs.ARGP_MOD_OP,               InterpreterArgs.ARGI_MOD_OP,                ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Constant),
/* 72 */ new OpCode(OpCodeEnum.CreateQWordField,		 ParseArgs.ARGP_CREATE_QWORD_FIELD_OP,InterpreterArgs.ARGI_CREATE_QWORD_FIELD_OP, ObjectTypeEnum.BufferField,       OpCodeClass.Create,          OpCodeType.CreateField,           OpCodeFlags. HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSNode | OpCodeFlags.Defer | OpCodeFlags.Create),
/* 73 */ new OpCode(OpCodeEnum.ToBuffer,				 ParseArgs.ARGP_TO_BUFFER_OP,         InterpreterArgs.ARGI_TO_BUFFER_OP,          ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 74 */ new OpCode(OpCodeEnum.ToDecimalString,			 ParseArgs.ARGP_TO_DEC_STR_OP,        InterpreterArgs.ARGI_TO_DEC_STR_OP,         ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 75 */ new OpCode(OpCodeEnum.ToHexString,				 ParseArgs.ARGP_TO_HEX_STR_OP,        InterpreterArgs.ARGI_TO_HEX_STR_OP,         ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 76 */ new OpCode(OpCodeEnum.ToInteger,				 ParseArgs.ARGP_TO_INTEGER_OP,        InterpreterArgs.ARGI_TO_INTEGER_OP,         ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R | OpCodeFlags.Constant),
/* 77 */ new OpCode(OpCodeEnum.ToString,				 ParseArgs.ARGP_TO_STRING_OP,         InterpreterArgs.ARGI_TO_STRING_OP,          ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_2A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_2A_1T_1R | OpCodeFlags.Constant),
/* 78 */ new OpCode(OpCodeEnum.CopyObject,				 ParseArgs.ARGP_COPY_OP,              InterpreterArgs.ARGI_COPY_OP,               ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_1A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_1A_1T_1R),
/* 79 */ new OpCode(OpCodeEnum.Mid,						 ParseArgs.ARGP_MID_OP,               InterpreterArgs.ARGI_MID_OP,                ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_3A_1T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_3A_1T_1R | OpCodeFlags.Constant),
/* 7A */ new OpCode(OpCodeEnum.Continue,				 ParseArgs.ARGP_CONTINUE_OP,          InterpreterArgs.ARGI_CONTINUE_OP,           ObjectTypeEnum.Any,               OpCodeClass.Control,         OpCodeType.Control,               0),
/* 7B */ new OpCode(OpCodeEnum.LoadTable,				 ParseArgs.ARGP_LOAD_TABLE_OP,        InterpreterArgs.ARGI_LOAD_TABLE_OP,         ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_6A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_6A_0T_1R),
/* 7C */ new OpCode(OpCodeEnum.DataRegion,				 ParseArgs.ARGP_DATA_REGION_OP,       InterpreterArgs.ARGI_DATA_REGION_OP,        ObjectTypeEnum.Region,            OpCodeClass.NamedObject,     OpCodeType.NamedComplex,          OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named | OpCodeFlags.Defer),
/* 7D */ new OpCode(OpCodeEnum.Scope,					 ParseArgs.ARGP_SCOPE_OP,             InterpreterArgs.ARGI_SCOPE_OP,              ObjectTypeEnum.Any,               OpCodeClass.NamedObject,     OpCodeType.NamedNoObject,         OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode),

/* ACPI 3.0 opcodes */

/* 7E */ new OpCode(OpCodeEnum.Timer,					 ParseArgs.ARGP_TIMER_OP,             InterpreterArgs.ARGI_TIMER_OP,              ObjectTypeEnum.Any,               OpCodeClass.Execute,         OpCodeType.Execute_0A_0T_1R,      OpCodeExtendedFlags.AML_FLAGS_EXEC_0A_0T_1R),

/* ACPI 5.0 opcodes */

/* 7F */ new OpCode(OpCodeEnum.FieldConnection,			 ParseArgs.ARGP_CONNECTFIELD_OP,      InterpreterArgs.ARGI_CONNECTFIELD_OP,       ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 OpCodeFlags.HasArguments),
/* 80 */ new OpCode(OpCodeEnum.ExternalAccessField,		 ParseArgs.ARGP_CONNECTFIELD_OP,      InterpreterArgs.ARGI_CONNECTFIELD_OP,       ObjectTypeEnum.Any,               OpCodeClass.Internal,        OpCodeType.Bogus,                 0),

/* ACPI 6.0 opcodes */

/* 81 */ new OpCode(OpCodeEnum.External,				 ParseArgs.ARGP_EXTERNAL_OP,          InterpreterArgs.ARGI_EXTERNAL_OP,           ObjectTypeEnum.Any,               OpCodeClass.NamedObject,     OpCodeType.NamedSimple,           OpCodeFlags.HasArguments | OpCodeFlags.NSObject | OpCodeFlags.NSOpCode | OpCodeFlags.NSNode | OpCodeFlags.Named),
/* 82 */ new OpCode(OpCodeEnum.Comment,					 ParseArgs.ARGP_COMMENT_OP,           InterpreterArgs.ARGI_COMMENT_OP,            ObjectTypeEnum.String,            OpCodeClass.Argument,        OpCodeType.Literal,               OpCodeFlags.Constant)
};

		public static OpCode GetOpcode(ushort opCodeValue)
		{
			OpCode opCode = null;

			if ((opCodeValue & 0xFF00) == 0x0)
			{
				//Simple (8-bit) opcode
				opCode = OpCodes[ShortOpCodeIndexes[(byte)opCodeValue]];
			}

			if (((opCodeValue & 0xFF00) == 0x5b00) && (((byte)opCodeValue) <= 0x88))
			{
				//Extended (16-bit) opcode
				opCode = OpCodes[LongOpCodeIndexes[(byte)opCodeValue]];
			}

			return opCode;
		}
	}
}
