namespace Cosmos.Plugs
{
    //[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Data.Common.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
    //public static class System_Data_Common_UnsafeNativeMethodsImpl
    //{

    //    public static System.Int32 CreateWellKnownSid(System.Int32 sidType, System.Byte[] domainSid, System.Byte[] resultSid, System.UInt32* resultSidLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.CreateWellKnownSid' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLAllocHandle(System.Data.Odbc.ODBC32+SQL_HANDLE HandleType, System.IntPtr InputHandle, System.IntPtr* OutputHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLAllocHandle' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLAllocHandle(System.Data.Odbc.ODBC32+SQL_HANDLE HandleType, System.Data.Odbc.OdbcHandle InputHandle, System.IntPtr* OutputHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLAllocHandle' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLBindCol(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.UInt16 ColumnNumber, System.Data.Odbc.ODBC32+SQL_C TargetType, System.Runtime.InteropServices.HandleRef TargetValue, System.IntPtr BufferLength, System.IntPtr StrLen_or_Ind)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLBindCol' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLBindCol(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.UInt16 ColumnNumber, System.Data.Odbc.ODBC32+SQL_C TargetType, System.IntPtr TargetValue, System.IntPtr BufferLength, System.IntPtr StrLen_or_Ind)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLBindCol' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLBindParameter(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.UInt16 ParameterNumber, System.Int16 ParamDirection, System.Data.Odbc.ODBC32+SQL_C SQLCType, System.Int16 SQLType, System.IntPtr cbColDef, System.IntPtr ibScale, System.Runtime.InteropServices.HandleRef rgbValue, System.IntPtr BufferLength, System.Runtime.InteropServices.HandleRef StrLen_or_Ind)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLBindParameter' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLCancel(System.Data.Odbc.OdbcStatementHandle StatementHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLCancel' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLCloseCursor(System.Data.Odbc.OdbcStatementHandle StatementHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLCloseCursor' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLColAttributeW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.Int16 ColumnNumber, System.Int16 FieldIdentifier, System.Data.Odbc.CNativeBuffer CharacterAttribute, System.Int16 BufferLength, System.Int16* StringLength, System.IntPtr* NumericAttribute)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLColAttributeW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLColumnsW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.String CatalogName, System.Int16 NameLen1, System.String SchemaName, System.Int16 NameLen2, System.String TableName, System.Int16 NameLen3, System.String ColumnName, System.Int16 NameLen4)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLColumnsW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLDisconnect(System.IntPtr ConnectionHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLDisconnect' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLDriverConnectW(System.Data.Odbc.OdbcConnectionHandle hdbc, System.IntPtr hwnd, System.String connectionstring, System.Int16 cbConnectionstring, System.IntPtr connectionstringout, System.Int16 cbConnectionstringoutMax, System.Int16* cbConnectionstringout, System.Int16 fDriverCompletion)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLDriverConnectW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLEndTran(System.Data.Odbc.ODBC32+SQL_HANDLE HandleType, System.IntPtr Handle, System.Int16 CompletionType)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLEndTran' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLExecDirectW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.String StatementText, System.Int32 TextLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLExecDirectW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLExecute(System.Data.Odbc.OdbcStatementHandle StatementHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLExecute' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLFetch(System.Data.Odbc.OdbcStatementHandle StatementHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLFetch' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLFreeHandle(System.Data.Odbc.ODBC32+SQL_HANDLE HandleType, System.IntPtr StatementHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLFreeHandle' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLFreeStmt(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.Data.Odbc.ODBC32+STMT Option)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLFreeStmt' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetConnectAttrW(System.Data.Odbc.OdbcConnectionHandle ConnectionHandle, System.Data.Odbc.ODBC32+SQL_ATTR Attribute, System.Byte[] Value, System.Int32 BufferLength, System.Int32* StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetConnectAttrW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetData(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.UInt16 ColumnNumber, System.Data.Odbc.ODBC32+SQL_C TargetType, System.Data.Odbc.CNativeBuffer TargetValue, System.IntPtr BufferLength, System.IntPtr* StrLen_or_Ind)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetData' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetDescFieldW(System.Data.Odbc.OdbcDescriptorHandle StatementHandle, System.Int16 RecNumber, System.Data.Odbc.ODBC32+SQL_DESC FieldIdentifier, System.Data.Odbc.CNativeBuffer ValuePointer, System.Int32 BufferLength, System.Int32* StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetDescFieldW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetDiagRecW(System.Data.Odbc.ODBC32+SQL_HANDLE HandleType, System.Data.Odbc.OdbcHandle Handle, System.Int16 RecNumber, System.Text.StringBuilder rchState, System.Int32* NativeError, System.Text.StringBuilder MessageText, System.Int16 BufferLength, System.Int16* TextLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetDiagRecW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetDiagFieldW(System.Data.Odbc.ODBC32+SQL_HANDLE HandleType, System.Data.Odbc.OdbcHandle Handle, System.Int16 RecNumber, System.Int16 DiagIdentifier, System.Text.StringBuilder rchState, System.Int16 BufferLength, System.Int16* StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetDiagFieldW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetFunctions(System.Data.Odbc.OdbcConnectionHandle hdbc, System.Data.Odbc.ODBC32+SQL_API fFunction, System.Int16* pfExists)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetFunctions' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetInfoW(System.Data.Odbc.OdbcConnectionHandle hdbc, System.Data.Odbc.ODBC32+SQL_INFO fInfoType, System.Byte[] rgbInfoValue, System.Int16 cbInfoValueMax, System.Int16* pcbInfoValue)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetInfoW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetInfoW(System.Data.Odbc.OdbcConnectionHandle hdbc, System.Data.Odbc.ODBC32+SQL_INFO fInfoType, System.Byte[] rgbInfoValue, System.Int16 cbInfoValueMax, System.IntPtr pcbInfoValue)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetInfoW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetStmtAttrW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.Data.Odbc.ODBC32+SQL_ATTR Attribute, System.IntPtr* Value, System.Int32 BufferLength, System.Int32* StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetStmtAttrW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLGetTypeInfo(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.Int16 fSqlType)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLGetTypeInfo' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLMoreResults(System.Data.Odbc.OdbcStatementHandle StatementHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLMoreResults' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLNumResultCols(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.Int16* ColumnCount)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLNumResultCols' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLPrepareW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.String StatementText, System.Int32 TextLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLPrepareW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLPrimaryKeysW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.String CatalogName, System.Int16 NameLen1, System.String SchemaName, System.Int16 NameLen2, System.String TableName, System.Int16 NameLen3)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLPrimaryKeysW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLProcedureColumnsW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.String CatalogName, System.Int16 NameLen1, System.String SchemaName, System.Int16 NameLen2, System.String ProcName, System.Int16 NameLen3, System.String ColumnName, System.Int16 NameLen4)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLProcedureColumnsW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLProceduresW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.String CatalogName, System.Int16 NameLen1, System.String SchemaName, System.Int16 NameLen2, System.String ProcName, System.Int16 NameLen3)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLProceduresW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLRowCount(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.IntPtr* RowCount)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLRowCount' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSetConnectAttrW(System.Data.Odbc.OdbcConnectionHandle ConnectionHandle, System.Data.Odbc.ODBC32+SQL_ATTR Attribute, System.Transactions.IDtcTransaction Value, System.Int32 StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSetConnectAttrW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSetConnectAttrW(System.Data.Odbc.OdbcConnectionHandle ConnectionHandle, System.Data.Odbc.ODBC32+SQL_ATTR Attribute, System.String Value, System.Int32 StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSetConnectAttrW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSetConnectAttrW(System.Data.Odbc.OdbcConnectionHandle ConnectionHandle, System.Data.Odbc.ODBC32+SQL_ATTR Attribute, System.IntPtr Value, System.Int32 StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSetConnectAttrW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSetConnectAttrW(System.IntPtr ConnectionHandle, System.Data.Odbc.ODBC32+SQL_ATTR Attribute, System.IntPtr Value, System.Int32 StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSetConnectAttrW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSetDescFieldW(System.Data.Odbc.OdbcDescriptorHandle StatementHandle, System.Int16 ColumnNumber, System.Data.Odbc.ODBC32+SQL_DESC FieldIdentifier, System.Runtime.InteropServices.HandleRef CharacterAttribute, System.Int32 BufferLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSetDescFieldW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSetDescFieldW(System.Data.Odbc.OdbcDescriptorHandle StatementHandle, System.Int16 ColumnNumber, System.Data.Odbc.ODBC32+SQL_DESC FieldIdentifier, System.IntPtr CharacterAttribute, System.Int32 BufferLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSetDescFieldW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSetEnvAttr(System.Data.Odbc.OdbcEnvironmentHandle EnvironmentHandle, System.Data.Odbc.ODBC32+SQL_ATTR Attribute, System.IntPtr Value, System.Data.Odbc.ODBC32+SQL_IS StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSetEnvAttr' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSetStmtAttrW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.Int32 Attribute, System.IntPtr Value, System.Int32 StringLength)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSetStmtAttrW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLSpecialColumnsW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.Data.Odbc.ODBC32+SQL_SPECIALCOLS IdentifierType, System.String CatalogName, System.Int16 NameLen1, System.String SchemaName, System.Int16 NameLen2, System.String TableName, System.Int16 NameLen3, System.Data.Odbc.ODBC32+SQL_SCOPE Scope, System.Data.Odbc.ODBC32+SQL_NULLABILITY Nullable)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLSpecialColumnsW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLStatisticsW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.String CatalogName, System.Int16 NameLen1, System.String SchemaName, System.Int16 NameLen2, System.String TableName, System.Int16 NameLen3, System.Int16 Unique, System.Int16 Reserved)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLStatisticsW' has not been implemented!");
    //    }

    //    public static System.Data.Odbc.ODBC32+RetCode SQLTablesW(System.Data.Odbc.OdbcStatementHandle StatementHandle, System.String CatalogName, System.Int16 NameLen1, System.String SchemaName, System.Int16 NameLen2, System.String TableName, System.Int16 NameLen3, System.String TableType, System.Int16 NameLen4)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SQLTablesW' has not been implemented!");
    //    }

    //    public static System.Data.OleDb.OleDbHResult GetErrorInfo(System.Int32 dwReserved, System.Data.Common.UnsafeNativeMethods+IErrorInfo* ppIErrorInfo)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.GetErrorInfo' has not been implemented!");
    //    }

    //    public static System.UInt32 GetEffectiveRightsFromAclW(System.Byte[] pAcl, System.Data.Common.UnsafeNativeMethods+Trustee* pTrustee, System.UInt32* pAccessMask)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.GetEffectiveRightsFromAclW' has not been implemented!");
    //    }

    //    public static System.Boolean CheckTokenMembership(System.IntPtr tokenHandle, System.Byte[] sidToCheck, System.Boolean* isMember)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.CheckTokenMembership' has not been implemented!");
    //    }

    //    public static System.Boolean ConvertSidToStringSidW(System.IntPtr sid, System.IntPtr* stringSid)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.ConvertSidToStringSidW' has not been implemented!");
    //    }

    //    public static System.Boolean GetTokenInformation(System.IntPtr tokenHandle, System.UInt32 token_class, System.IntPtr tokenStruct, System.UInt32 tokenInformationLength, System.UInt32* tokenString)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.GetTokenInformation' has not been implemented!");
    //    }

    //    public static System.Boolean IsTokenRestricted(System.IntPtr tokenHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.IsTokenRestricted' has not been implemented!");
    //    }

    //    public static System.Int32 lstrlenW(System.IntPtr ptr)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.lstrlenW' has not been implemented!");
    //    }

    //    public static System.Void SetLastError(System.Int32 dwErrCode)
    //    {
    //        throw new System.NotImplementedException("Method 'System.Data.Common.UnsafeNativeMethods.SetLastError' has not been implemented!");
    //    }
    //}
}
