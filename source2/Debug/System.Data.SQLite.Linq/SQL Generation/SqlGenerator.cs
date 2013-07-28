//---------------------------------------------------------------------
// <copyright file="SqlGenerator.cs" company="Microsoft">
//      Portions of this file copyright (c) Microsoft Corporation
//      and are released under the Microsoft Pulic License.  See
//      http://archive.msdn.microsoft.com/EFSampleProvider/Project/License.aspx
//      or License.txt for details.
//      All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace System.Data.SQLite
{
  using System;
  using System.Linq;
  using System.Diagnostics;
  using System.Globalization;
  using System.Text;
  using System.Data.Common;
  using System.Data.Entity.Core.Metadata.Edm;
  using System.Data.Entity.Core.Common.CommandTrees;
  using System.Data;
  using System.Collections.ObjectModel;
  using System.Collections.Generic;

  /// <summary>
  /// Translates the command object into a SQL string that can be executed on
  /// SQLite.
  /// </summary>
  /// <remarks>
  /// The translation is implemented as a visitor <see cref="DbExpressionVisitor{T}"/>
  /// over the query tree.  It makes a single pass over the tree, collecting the sql
  /// fragments for the various nodes in the tree <see cref="ISqlFragment"/>.
  ///
  /// The major operations are
  /// <list type="bullet">
  /// <item>Select statement minimization.  Multiple nodes in the query tree
  /// that can be part of a single SQL select statement are merged. e.g. a
  /// Filter node that is the input of a Project node can typically share the
  /// same SQL statement.</item>
  /// <item>Alpha-renaming.  As a result of the statement minimization above, there
  /// could be name collisions when using correlated subqueries
  /// <example>
  /// <code>
  /// Filter(
  ///     b = Project( c.x
  ///         c = Extent(foo)
  ///         )
  ///     exists (
  ///         Filter(
  ///             c = Extent(foo)
  ///             b.x = c.x
  ///             )
  ///     )
  /// )
  /// </code>
  /// The first Filter, Project and Extent will share the same SQL select statement.
  /// The alias for the Project i.e. b, will be replaced with c.
  /// If the alias c for the Filter within the exists clause is not renamed,
  /// we will get <c>c.x = c.x</c>, which is incorrect.
  /// Instead, the alias c within the second filter should be renamed to c1, to give
  /// <c>c.x = c1.x</c> i.e. b is renamed to c, and c is renamed to c1.
  /// </example>
  /// </item>
  /// <item>Join flattening.  In the query tree, a list of join nodes is typically
  /// represented as a tree of Join nodes, each with 2 children. e.g.
  /// <example>
  /// <code>
  /// a = Join(InnerJoin
  ///     b = Join(CrossJoin
  ///         c = Extent(foo)
  ///         d = Extent(foo)
  ///         )
  ///     e = Extent(foo)
  ///     on b.c.x = e.x
  ///     )
  /// </code>
  /// If translated directly, this will be translated to
  /// <code>
  /// FROM ( SELECT c.*, d.*
  ///         FROM foo as c
  ///         CROSS JOIN foo as d) as b
  /// INNER JOIN foo as e on b.x' = e.x
  /// </code>
  /// It would be better to translate this as
  /// <code>
  /// FROM foo as c
  /// CROSS JOIN foo as d
  /// INNER JOIN foo as e on c.x = e.x
  /// </code>
  /// This allows the optimizer to choose an appropriate join ordering for evaluation.
  /// </example>
  /// </item>
  /// <item>Select * and column renaming.  In the example above, we noticed that
  /// in some cases we add <c>SELECT * FROM ...</c> to complete the SQL
  /// statement. i.e. there is no explicit PROJECT list.
  /// In this case, we enumerate all the columns available in the FROM clause
  /// This is particularly problematic in the case of Join trees, since the columns
  /// from the extents joined might have the same name - this is illegal.  To solve
  /// this problem, we will have to rename columns if they are part of a SELECT *
  /// for a JOIN node - we do not need renaming in any other situation.
  /// <see cref="SqlGenerator.AddDefaultColumns"/>.
  /// </item>
  /// </list>
  ///
  /// <para>
  /// Renaming issues.
  /// When rows or columns are renamed, we produce names that are unique globally
  /// with respect to the query.  The names are derived from the original names,
  /// with an integer as a suffix. e.g. CustomerId will be renamed to CustomerId1,
  /// CustomerId2 etc.
  ///
  /// Since the names generated are globally unique, they will not conflict when the
  /// columns of a JOIN SELECT statement are joined with another JOIN. 
  ///
  /// </para>
  ///
  /// <para>
  /// Record flattening.
  /// SQL server does not have the concept of records.  However, a join statement
  /// produces records.  We have to flatten the record accesses into a simple
  /// <c>alias.column</c> form.  <see cref="SqlGenerator.Visit(DbPropertyExpression)"/>
  /// </para>
  ///
  /// <para>
  /// Building the SQL.
  /// There are 2 phases
  /// <list type="numbered">
  /// <item>Traverse the tree, producing a sql builder <see cref="SqlBuilder"/></item>
  /// <item>Write the SqlBuilder into a string, renaming the aliases and columns
  /// as needed.</item>
  /// </list>
  ///
  /// In the first phase, we traverse the tree.  We cannot generate the SQL string
  /// right away, since
  /// <list type="bullet">
  /// <item>The WHERE clause has to be visited before the from clause.</item>
  /// <item>extent aliases and column aliases need to be renamed.  To minimize
  /// renaming collisions, all the names used must be known, before any renaming
  /// choice is made.</item>
  /// </list>
  /// To defer the renaming choices, we use symbols <see cref="Symbol"/>.  These
  /// are renamed in the second phase.
  ///
  /// Since visitor methods cannot transfer information to child nodes through
  /// parameters, we use some global stacks,
  /// <list type="bullet">
  /// <item>A stack for the current SQL select statement.  This is needed by
  /// <see cref="SqlGenerator.Visit(DbVariableReferenceExpression)"/> to create a
  /// list of free variables used by a select statement.  This is needed for
  /// alias renaming.
  /// </item>
  /// <item>A stack for the join context.  When visiting a <see cref="DbScanExpression"/>,
  /// we need to know whether we are inside a join or not.  If we are inside
  /// a join, we do not create a new SELECT statement.</item>
  /// </list>
  /// </para>
  ///
  /// <para>
  /// Global state.
  /// To enable renaming, we maintain
  /// <list type="bullet">
  /// <item>The set of all extent aliases used.</item>
  /// <item>The set of all column aliases used.</item>
  /// </list>
  ///
  /// Finally, we have a symbol table to lookup variable references.  All references
  /// to the same extent have the same symbol.
  /// </para>
  ///
  /// <para>
  /// Sql select statement sharing.
  ///
  /// Each of the relational operator nodes
  /// <list type="bullet">
  /// <item>Project</item>
  /// <item>Filter</item>
  /// <item>GroupBy</item>
  /// <item>Sort/OrderBy</item>
  /// </list>
  /// can add its non-input (e.g. project, predicate, sort order etc.) to
  /// the SQL statement for the input, or create a new SQL statement.
  /// If it chooses to reuse the input's SQL statement, we play the following
  /// symbol table trick to accomplish renaming.  The symbol table entry for
  /// the alias of the current node points to the symbol for the input in
  /// the input's SQL statement.
  /// <example>
  /// <code>
  /// Project(b.x
  ///     b = Filter(
  ///         c = Extent(foo)
  ///         c.x = 5)
  ///     )
  /// </code>
  /// The Extent node creates a new SqlSelectStatement.  This is added to the
  /// symbol table by the Filter as {c, Symbol(c)}.  Thus, <c>c.x</c> is resolved to
  /// <c>Symbol(c).x</c>.
  /// Looking at the project node, we add {b, Symbol(c)} to the symbol table if the
  /// SQL statement is reused, and {b, Symbol(b)}, if there is no reuse.
  ///
  /// Thus, <c>b.x</c> is resolved to <c>Symbol(c).x</c> if there is reuse, and to
  /// <c>Symbol(b).x</c> if there is no reuse.
  /// </example>
  /// </para>
  /// </remarks>
  internal sealed class SqlGenerator : DbExpressionVisitor<ISqlFragment>
  {
    private SQLiteProviderManifest _manifest;

    #region Visitor parameter stacks
    /// <summary>
    /// Every relational node has to pass its SELECT statement to its children
    /// This allows them (DbVariableReferenceExpression eventually) to update the list of
    /// outer extents (free variables) used by this select statement.
    /// </summary>
    Stack<SqlSelectStatement> selectStatementStack;

    /// <summary>
    /// The top of the stack
    /// </summary>
    private SqlSelectStatement CurrentSelectStatement
    {
      // There is always something on the stack, so we can always Peek.
      get { return selectStatementStack.Peek(); }
    }

    /// <summary>
    /// Nested joins and extents need to know whether they should create
    /// a new Select statement, or reuse the parent's.  This flag
    /// indicates whether the parent is a join or not.
    /// </summary>
    Stack<bool> isParentAJoinStack;

    /// <summary>
    /// The top of the stack
    /// </summary>
    private bool IsParentAJoin
    {
      // There might be no entry on the stack if a Join node has never
      // been seen, so we return false in that case.
      get { return isParentAJoinStack.Count == 0 ? false : isParentAJoinStack.Peek(); }
    }

    #endregion

    #region Global lists and state
    Dictionary<string, int> allExtentNames;
    internal Dictionary<string, int> AllExtentNames
    {
      get { return allExtentNames; }
    }

    // For each column name, we store the last integer suffix that
    // was added to produce a unique column name.  This speeds up
    // the creation of the next unique name for this column name.
    Dictionary<string, int> allColumnNames;
    internal Dictionary<string, int> AllColumnNames
    {
      get { return allColumnNames; }
    }

    SymbolTable symbolTable = new SymbolTable();

    /// <summary>
    /// VariableReferenceExpressions are allowed only as children of DbPropertyExpression
    /// or MethodExpression.  The cheapest way to ensure this is to set the following
    /// property in DbVariableReferenceExpression and reset it in the allowed parent expressions.
    /// </summary>
    bool isVarRefSingle = false;

    #endregion

    private bool HasBuiltMapForIn(DbExpression e, KeyToListMap<DbExpression, DbExpression> values)
    {
      DbExpressionKind expressionKind = e.ExpressionKind;
      if (expressionKind != DbExpressionKind.Equals)
      {
        if (expressionKind != DbExpressionKind.IsNull)
        {
          if (expressionKind != DbExpressionKind.Or)
          {
            return false;
          }
          DbBinaryExpression expression2 = e as DbBinaryExpression;
          return (this.HasBuiltMapForIn(expression2.Left, values) && this.HasBuiltMapForIn(expression2.Right, values));
        }
      }
      else
      {
        return this.TryAddExpressionForIn((DbBinaryExpression)e, values);
      }
      DbExpression argument = ((DbIsNullExpression)e).Argument;
      if (this.IsKeyForIn(argument))
      {
        values.Add(argument, e);
        return true;
      }
      return false;
    }

    #region Statics
    static private readonly Dictionary<string, FunctionHandler> _builtInFunctionHandlers = InitializeBuiltInFunctionHandlers();
    static private readonly Dictionary<string, FunctionHandler> _canonicalFunctionHandlers = InitializeCanonicalFunctionHandlers();
    static private readonly Dictionary<string, string> _functionNameToOperatorDictionary = InitializeFunctionNameToOperatorDictionary();
    static private readonly Dictionary<string, string> _datepartKeywords = InitializeDatepartKeywords();
    static private readonly char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

    private delegate ISqlFragment FunctionHandler(SqlGenerator sqlgen, DbFunctionExpression functionExpr);

    /// <summary>
    /// All special built-in functions and their handlers
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, FunctionHandler> InitializeBuiltInFunctionHandlers()
    {
      Dictionary<string, FunctionHandler> functionHandlers = new Dictionary<string, FunctionHandler>(7, StringComparer.Ordinal);
      functionHandlers.Add("CONCAT", HandleConcatFunction);
      functionHandlers.Add("DATEPART", HandleDatepartDateFunction);
      functionHandlers.Add("DatePart", HandleDatepartDateFunction);
      functionHandlers.Add("GETDATE", HandleGetDateFunction);
      functionHandlers.Add("GETUTCDATE", HandleGetUtcDateFunction);
      return functionHandlers;
    }

    /// <summary>
    /// All special non-aggregate canonical functions and their handlers
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, FunctionHandler> InitializeCanonicalFunctionHandlers()
    {
      Dictionary<string, FunctionHandler> functionHandlers = new Dictionary<string, FunctionHandler>(16, StringComparer.Ordinal);

#if USE_INTEROP_DLL && INTEROP_EXTENSION_FUNCTIONS
      functionHandlers.Add("IndexOf", HandleCanonicalFunctionIndexOf);
#endif

      functionHandlers.Add("Length", HandleCanonicalFunctionLength);
      functionHandlers.Add("NewGuid", HandleCanonicalFunctionNewGuid);
      functionHandlers.Add("Round", HandleCanonicalFunctionRound);
      functionHandlers.Add("ToLower", HandleCanonicalFunctionToLower);
      functionHandlers.Add("ToUpper", HandleCanonicalFunctionToUpper);
      functionHandlers.Add("Trim", HandleCanonicalFunctionTrim);
      functionHandlers.Add("Left", HandleCanonicalFunctionLeft);
      functionHandlers.Add("Right", HandleCanonicalFunctionRight);
      functionHandlers.Add("CurrentDateTime", HandleGetDateFunction);
      functionHandlers.Add("CurrentUtcDateTime", HandleGetUtcDateFunction);

      //DatePartFunctions
      functionHandlers.Add("Year", HandleCanonicalFunctionDatepart);
      functionHandlers.Add("Month", HandleCanonicalFunctionDatepart);
      functionHandlers.Add("Day", HandleCanonicalFunctionDatepart);
      functionHandlers.Add("Hour", HandleCanonicalFunctionDatepart);
      functionHandlers.Add("Minute", HandleCanonicalFunctionDatepart);
      functionHandlers.Add("Second", HandleCanonicalFunctionDatepart);
      functionHandlers.Add("DateAdd", HandleCanonicalFunctionDateAdd);
      functionHandlers.Add("DateDiff", HandleCanonicalFunctionDateSubtract);
      functionHandlers.Add("DATEADD", HandleCanonicalFunctionDateAdd); // store
      functionHandlers.Add("DATEDIFF", HandleCanonicalFunctionDateSubtract); // store

      //Functions that translate to operators
      functionHandlers.Add("Concat", HandleConcatFunction);
      functionHandlers.Add("BitwiseAnd", HandleCanonicalFunctionBitwise);
      functionHandlers.Add("BitwiseNot", HandleCanonicalFunctionBitwise);
      functionHandlers.Add("BitwiseOr", HandleCanonicalFunctionBitwise);
      functionHandlers.Add("BitwiseXor", HandleCanonicalFunctionBitwise);
      return functionHandlers;
    }

    /// <summary>
    /// Valid datepart values
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, string> InitializeDatepartKeywords()
    {
      #region Datepart Keywords
      //
      // valid datepart values
      //
      Dictionary<string, string> datepartKeywords = new Dictionary<string, string>(30, StringComparer.OrdinalIgnoreCase);
      datepartKeywords.Add("d", "%d");
      datepartKeywords.Add("day", "%d");
      datepartKeywords.Add("dayofyear", "%j");
      datepartKeywords.Add("dd", "%d");
      datepartKeywords.Add("dw", "%w");
      datepartKeywords.Add("dy", "%j");
      datepartKeywords.Add("hh", "%H");
      datepartKeywords.Add("hour", "%H");
      datepartKeywords.Add("m", "%m");
      datepartKeywords.Add("mi", "%M");
      datepartKeywords.Add("millisecond", "%f");
      datepartKeywords.Add("minute", "%M");
      datepartKeywords.Add("mm", "%m");
      datepartKeywords.Add("month", "%m");
      datepartKeywords.Add("ms", "%f");
      datepartKeywords.Add("n", "%M");
      datepartKeywords.Add("s", "%S");
      datepartKeywords.Add("second", "%S");
      datepartKeywords.Add("ss", "%S");
      datepartKeywords.Add("week", "%W");
      datepartKeywords.Add("weekday", "%w");
      datepartKeywords.Add("wk", "%W");
      datepartKeywords.Add("ww", "%W");
      datepartKeywords.Add("y", "%Y");
      datepartKeywords.Add("year", "%Y");
      datepartKeywords.Add("yy", "%Y");
      datepartKeywords.Add("yyyy", "%Y");
      return datepartKeywords;
      #endregion
    }

    /// <summary>
    /// Initializes the mapping from functions to T-SQL operators
    /// for all functions that translate to T-SQL operators
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, string> InitializeFunctionNameToOperatorDictionary()
    {
      Dictionary<string, string> functionNameToOperatorDictionary = new Dictionary<string, string>(5, StringComparer.Ordinal);
      functionNameToOperatorDictionary.Add("Concat", "||");    //canonical
      functionNameToOperatorDictionary.Add("CONCAT", "||");    //store
      functionNameToOperatorDictionary.Add("BitwiseAnd", "&");
      functionNameToOperatorDictionary.Add("BitwiseNot", "~");
      functionNameToOperatorDictionary.Add("BitwiseOr", "|");
      functionNameToOperatorDictionary.Add("BitwiseXor", "^");
      return functionNameToOperatorDictionary;
    }

    #endregion

    #region Constructor
    /// <summary>
    /// Basic constructor. 
    /// </summary>
    private SqlGenerator(SQLiteProviderManifest manifest)
    {
      _manifest = manifest;
    }
    #endregion

    #region Entry points
    /// <summary>
    /// General purpose static function that can be called from System.Data assembly
    /// </summary>
    /// <param name="manifest"></param>
    /// <param name="tree">command tree</param>
    /// <param name="parameters">Parameters to add to the command tree corresponding
    /// to constants in the command tree. Used only in ModificationCommandTrees.</param>
    /// <param name="commandType"></param>
    /// <returns>The string representing the SQL to be executed.</returns>
    internal static string GenerateSql(SQLiteProviderManifest manifest, DbCommandTree tree, out List<DbParameter> parameters, out CommandType commandType)
    {
      commandType = CommandType.Text;

      //Handle Query
      DbQueryCommandTree queryCommandTree = tree as DbQueryCommandTree;
      if (queryCommandTree != null)
      {
        SqlGenerator sqlGen = new SqlGenerator(manifest);
        parameters = null;
        
        string sql = sqlGen.GenerateSql((DbQueryCommandTree)tree);

        return sql;
      }

      //Handle Function
      DbFunctionCommandTree DbFunctionCommandTree = tree as DbFunctionCommandTree;
      if (DbFunctionCommandTree != null)
      {
        SqlGenerator sqlGen = new SqlGenerator(manifest);
        parameters = null;

        string sql = sqlGen.GenerateFunctionSql(DbFunctionCommandTree, out commandType);

        return sql;
      }

      //Handle Insert
      DbInsertCommandTree insertCommandTree = tree as DbInsertCommandTree;
      if (insertCommandTree != null)
      {
        return DmlSqlGenerator.GenerateInsertSql(insertCommandTree, out parameters);
      }

      //Handle Delete
      DbDeleteCommandTree deleteCommandTree = tree as DbDeleteCommandTree;
      if (deleteCommandTree != null)
      {
        return DmlSqlGenerator.GenerateDeleteSql(deleteCommandTree, out parameters);
      }

      //Handle Update
      DbUpdateCommandTree updateCommandTree = tree as DbUpdateCommandTree;
      if (updateCommandTree != null)
      {
        return DmlSqlGenerator.GenerateUpdateSql(updateCommandTree, out parameters);
      }

      throw new NotSupportedException("Unrecognized command tree type");
    }
    #endregion

    //StringBuilder _typeDefs = new StringBuilder();

    #region Driver Methods
    /// <summary>
    /// Translate a command tree to a SQL string.
    ///
    /// The input tree could be translated to either a SQL SELECT statement
    /// or a SELECT expression.  This choice is made based on the return type
    /// of the expression
    /// CollectionType => select statement
    /// non collection type => select expression
    /// </summary>
    /// <param name="tree"></param>
    /// <returns>The string representing the SQL to be executed.</returns>
    private string GenerateSql(DbQueryCommandTree tree)
    {
#if false
      tree = SqlChecker.Rewrite(tree);
#endif

      selectStatementStack = new Stack<SqlSelectStatement>();
      isParentAJoinStack = new Stack<bool>();

      allExtentNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
      allColumnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

      // Literals will not be converted to parameters.

      ISqlFragment result;
      if (MetadataHelpers.IsCollectionType(tree.Query.ResultType))
      {
        SqlSelectStatement sqlStatement = VisitExpressionEnsureSqlStatement(tree.Query);
        Debug.Assert(sqlStatement != null, "The outer most sql statment is null");
        sqlStatement.IsTopMost = true;
        result = sqlStatement;

      }
      else
      {
        SqlBuilder sqlBuilder = new SqlBuilder();
        sqlBuilder.Append("SELECT ");
        sqlBuilder.Append(tree.Query.Accept(this));

        result = sqlBuilder;
      }

      if (isVarRefSingle)
      {
        throw new NotSupportedException();
        // A DbVariableReferenceExpression has to be a child of DbPropertyExpression or MethodExpression
      }

      // Check that the parameter stacks are not leaking.
      Debug.Assert(selectStatementStack.Count == 0);
      Debug.Assert(isParentAJoinStack.Count == 0);

      //if (_typeDefs.Length > 0)
      //{
      //  _typeDefs.Insert(0x0, "TYPES ");
      //  _typeDefs.Append(";\r\n");
      //  _typeDefs.Append(WriteSql(result));
      //  return _typeDefs.ToString();
      //}

      return WriteSql(result);
    }

    /// <summary>
    /// Translate a function command tree to a SQL string.
    /// </summary>
    private string GenerateFunctionSql(DbFunctionCommandTree tree, out CommandType commandType)
    {
      EdmFunction function = tree.EdmFunction;

      // We expect function to always have these properties
      string userCommandText = (string)function.MetadataProperties["CommandTextAttribute"].Value;
      //string userSchemaName = (string)function.MetadataProperties["Schema"].Value;
      string userFuncName = (string)function.MetadataProperties["StoreFunctionNameAttribute"].Value;

      if (String.IsNullOrEmpty(userCommandText))
      {
        // build a quoted description of the function
        commandType = CommandType.StoredProcedure;

        // if the schema name is not explicitly given, it is assumed to be the metadata namespace
        //string schemaName = String.IsNullOrEmpty(userSchemaName) ?
        //    function.NamespaceName : userSchemaName;

        // if the function store name is not explicitly given, it is assumed to be the metadata name
        string functionName = String.IsNullOrEmpty(userFuncName) ?
            function.Name : userFuncName;

        // quote elements of function text
        //string quotedSchemaName = QuoteIdentifier(schemaName);
        string quotedFunctionName = QuoteIdentifier(functionName);

        // separator
        //const string schemaSeparator = ".";

        // concatenate elements of function text
        string quotedFunctionText = /* quotedSchemaName + schemaSeparator + */ quotedFunctionName;

        return quotedFunctionText;
      }
      else
      {
        // if the user has specified the command text, pass it through verbatim and choose CommandType.Text
        commandType = CommandType.Text;
        return userCommandText;
      }
    }

    /// <summary>
    /// Convert the SQL fragments to a string.
    /// We have to setup the Stream for writing.
    /// </summary>
    /// <param name="sqlStatement"></param>
    /// <returns>A string representing the SQL to be executed.</returns>
    string WriteSql(ISqlFragment sqlStatement)
    {
      StringBuilder builder = new StringBuilder(1024);
      using (SqlWriter writer = new SqlWriter(builder))
      {
        sqlStatement.WriteSql(writer, this);
      }

      return builder.ToString();
    }
    #endregion

    private bool TryTranslateIntoIn(DbOrExpression e, out ISqlFragment sqlFragment)
    {
      KeyToListMap<DbExpression, DbExpression> values = new KeyToListMap<DbExpression, DbExpression>(KeyFieldExpressionComparer.Singleton);
      if (!(this.HasBuiltMapForIn(e, values) && (values.Keys.Count<DbExpression>() > 0)))
      {
        sqlFragment = null;
        return false;
      }
      SqlBuilder result = new SqlBuilder();
      bool flag2 = true;
      foreach (DbExpression expression in values.Keys)
      {
        ReadOnlyCollection<DbExpression> source = values.ListForKey(expression);
        if (!flag2)
        {
          result.Append(" OR ");
        }
        else
        {
          flag2 = false;
        }
        IEnumerable<DbExpression> enumerable = source.Where<DbExpression>(delegate(DbExpression v)
        {
          return v.ExpressionKind != DbExpressionKind.IsNull;
        });
        int num = enumerable.Count<DbExpression>();
        if (num == 1)
        {
          this.ParanthesizeExpressionIfNeeded(expression, result);
          result.Append(" = ");
          DbExpression expression2 = enumerable.First<DbExpression>();
          this.ParenthesizeExpressionWithoutRedundantConstantCasts(expression2, result);
        }
        if (num > 1)
        {
          this.ParanthesizeExpressionIfNeeded(expression, result);
          result.Append(" IN (");
          bool flag3 = true;
          foreach (DbExpression expression3 in enumerable)
          {
            if (!flag3)
            {
              result.Append(",");
            }
            else
            {
              flag3 = false;
            }
            this.ParenthesizeExpressionWithoutRedundantConstantCasts(expression3, result);
          }
          result.Append(")");
        }
        DbIsNullExpression expression4 = source.FirstOrDefault<DbExpression>(delegate(DbExpression v)
        {
          return (v.ExpressionKind == DbExpressionKind.IsNull);
        }) as DbIsNullExpression;
        if (expression4 != null)
        {
          if (num > 0)
          {
            result.Append(" OR ");
          }
          result.Append(this.VisitIsNullExpression(expression4, false));
        }
      }
      sqlFragment = result;
      return true;
    }

    #region DbExpressionVisitor Members

    /// <summary>
    /// Translate(left) AND Translate(right)
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/>.</returns>
    public override ISqlFragment Visit(DbAndExpression e)
    {
      return VisitBinaryExpression(" AND ", e.Left, e.Right);
    }

    /// <summary>
    /// An apply is just like a join, so it shares the common join processing
    /// in <see cref="VisitJoinExpression"/>
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
    public override ISqlFragment Visit(DbApplyExpression e)
    {
      throw new NotSupportedException("APPLY joins are not supported");
    }

    /// <summary>
    /// For binary expressions, we delegate to <see cref="VisitBinaryExpression"/>.
    /// We handle the other expressions directly.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbArithmeticExpression e)
    {
      SqlBuilder result;

      switch (e.ExpressionKind)
      {
        case DbExpressionKind.Divide:
          result = VisitBinaryExpression(" / ", e.Arguments[0], e.Arguments[1]);
          break;
        case DbExpressionKind.Minus:
          result = VisitBinaryExpression(" - ", e.Arguments[0], e.Arguments[1]);
          break;
        case DbExpressionKind.Modulo:
          result = VisitBinaryExpression(" % ", e.Arguments[0], e.Arguments[1]);
          break;
        case DbExpressionKind.Multiply:
          result = VisitBinaryExpression(" * ", e.Arguments[0], e.Arguments[1]);
          break;
        case DbExpressionKind.Plus:
          result = VisitBinaryExpression(" + ", e.Arguments[0], e.Arguments[1]);
          break;

        case DbExpressionKind.UnaryMinus:
          result = new SqlBuilder();
          result.Append(" -(");
          result.Append(e.Arguments[0].Accept(this));
          result.Append(")");
          break;

        default:
          Debug.Assert(false);
          throw new InvalidOperationException();
      }

      return result;
    }

    /// <summary>
    /// If the ELSE clause is null, we do not write it out.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbCaseExpression e)
    {
      SqlBuilder result = new SqlBuilder();

      Debug.Assert(e.When.Count == e.Then.Count);

      result.Append("CASE");
      for (int i = 0; i < e.When.Count; ++i)
      {
        result.Append(" WHEN (");
        result.Append(e.When[i].Accept(this));
        result.Append(") THEN ");
        result.Append(e.Then[i].Accept(this));
      }
      if (e.Else != null && !(e.Else is DbNullExpression))
      {
        result.Append(" ELSE ");
        result.Append(e.Else.Accept(this));
      }

      result.Append(" END");

      return result;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbCastExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      result.Append(e.Argument.Accept(this));
      return result;
    }

    /// <summary>
    /// The parser generates Not(Equals(...)) for &lt;&gt;.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/>.</returns>
    public override ISqlFragment Visit(DbComparisonExpression e)
    {
      SqlBuilder result;
      switch (e.ExpressionKind)
      {
        case DbExpressionKind.Equals:
            result = VisitBinaryExpression(" = ", e.Left, e.Right);
          break;
        case DbExpressionKind.LessThan:
          result = VisitBinaryExpression(" < ", e.Left, e.Right);
          break;
        case DbExpressionKind.LessThanOrEquals:
          result = VisitBinaryExpression(" <= ", e.Left, e.Right);
          break;
        case DbExpressionKind.GreaterThan:
          result = VisitBinaryExpression(" > ", e.Left, e.Right);
          break;
        case DbExpressionKind.GreaterThanOrEquals:
          result = VisitBinaryExpression(" >= ", e.Left, e.Right);
          break;
        // The parser does not generate the expression kind below.
        case DbExpressionKind.NotEquals:
          result = VisitBinaryExpression(" <> ", e.Left, e.Right);
          break;

        default:
          throw new InvalidOperationException();
      }

      return result;
    }

    /// <summary>
    /// Constants will be send to the store as part of the generated TSQL, not as parameters
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/>.  Strings are wrapped in single
    /// quotes and escaped.  Numbers are written literally.</returns>
    public override ISqlFragment Visit(DbConstantExpression e)
    {
      SqlBuilder result = new SqlBuilder();

      PrimitiveTypeKind typeKind;
      // Model Types can be (at the time of this implementation):
      //      Binary, Boolean, Byte, DateTime, Decimal, Double, Guid, Int16, Int32, Int64,Single, String
      if (MetadataHelpers.TryGetPrimitiveTypeKind(e.ResultType, out typeKind))
      {
        switch (typeKind)
        {
          case PrimitiveTypeKind.Int32:
            result.Append(e.Value.ToString());
            break;

          case PrimitiveTypeKind.Binary:
            result.Append(" X'");
            result.Append(ByteArrayToBinaryString((Byte[])e.Value));
            result.Append("' ");
            break;

          case PrimitiveTypeKind.Boolean:
            result.Append((bool)e.Value ? "1" : "0");
            break;

          case PrimitiveTypeKind.Byte:
            result.Append(e.Value.ToString());
            break;

          case PrimitiveTypeKind.DateTime:
            result.Append(EscapeSingleQuote(((System.DateTime)e.Value).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), false /* IsUnicode */));
            break;

          case PrimitiveTypeKind.Decimal:
            string strDecimal = ((Decimal)e.Value).ToString(CultureInfo.InvariantCulture);
            // if the decimal value has no decimal part, cast as decimal to preserve type
            // if the number has precision > int64 max precision, it will be handled as decimal by sql server
            // and does not need cast. if precision is lest then 20, then cast using Max(literal precision, sql default precision)
            if (-1 == strDecimal.IndexOf('.') && (strDecimal.TrimStart(new char[] { '-' }).Length < 20))
            {
              byte precision = (Byte)strDecimal.Length;
              FacetDescription precisionFacetDescription;
              Debug.Assert(MetadataHelpers.TryGetTypeFacetDescriptionByName(e.ResultType.EdmType, "precision", out precisionFacetDescription), "Decimal primitive type must have Precision facet");
              if (MetadataHelpers.TryGetTypeFacetDescriptionByName(e.ResultType.EdmType, "precision", out precisionFacetDescription))
              {
                if (precisionFacetDescription.DefaultValue != null)
                {
                  precision = Math.Max(precision, (byte)precisionFacetDescription.DefaultValue);
                }
              }
              Debug.Assert(precision > 0, "Precision must be greater than zero");
              result.Append(strDecimal);
            }
            else
            {
              result.Append(strDecimal);
            }
            break;

          case PrimitiveTypeKind.Double:
            result.Append(((Double)e.Value).ToString(CultureInfo.InvariantCulture));
            break;

          case PrimitiveTypeKind.Guid:
            result.Append(EscapeSingleQuote(e.Value.ToString(), false /* IsUnicode */));
            break;

          case PrimitiveTypeKind.Int16:
            result.Append(e.Value.ToString());
            break;

          case PrimitiveTypeKind.Int64:
            result.Append(e.Value.ToString());
            break;

          case PrimitiveTypeKind.Single:
            result.Append(((Single)e.Value).ToString(CultureInfo.InvariantCulture));
            break;

          case PrimitiveTypeKind.String:
            bool isUnicode = MetadataHelpers.GetFacetValueOrDefault<bool>(e.ResultType, MetadataHelpers.UnicodeFacetName, true);
            result.Append(EscapeSingleQuote(e.Value as string, isUnicode));
            break;
          case PrimitiveTypeKind.DateTimeOffset:
            throw new NotSupportedException("datetimeoffset");
          case PrimitiveTypeKind.Time:
            throw new NotSupportedException("time");
          default:
            // all known scalar types should been handled already.
            throw new NotSupportedException();
        }
      }
      else
      {
        throw new NotSupportedException();
      }

      return result;

    }

    /// <summary>
    /// <see cref="DbDerefExpression"/> is illegal at this stage
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbDerefExpression e)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// The DISTINCT has to be added to the beginning of SqlSelectStatement.Select,
    /// but it might be too late for that.  So, we use a flag on SqlSelectStatement
    /// instead, and add the "DISTINCT" in the second phase.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlSelectStatement"/></returns>
    public override ISqlFragment Visit(DbDistinctExpression e)
    {
      SqlSelectStatement result = VisitExpressionEnsureSqlStatement(e.Argument);

      if (!IsCompatible(result, e.ExpressionKind))
      {
        Symbol fromSymbol;
        TypeUsage inputType = MetadataHelpers.GetElementTypeUsage(e.Argument.ResultType);
        result = CreateNewSelectStatement(result, "DISTINCT", inputType, out fromSymbol);
        AddFromSymbol(result, "DISTINCT", fromSymbol, false);
      }

      result.IsDistinct = true;
      return result;
    }

    /// <summary>
    /// An element expression returns a scalar - so it is translated to
    /// ( Select ... )
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbElementExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      result.Append("(");
      result.Append(VisitExpressionEnsureSqlStatement(e.Argument));
      result.Append(")");

      return result;
    }

    /// <summary>
    /// <see cref="Visit(DbUnionAllExpression)"/>
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbExceptExpression e)
    {
      return VisitSetOpExpression(e.Left, e.Right, "EXCEPT");
    }

    /// <summary>
    /// Only concrete expression types will be visited.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbExpression e)
    {
      throw new InvalidOperationException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns>If we are in a Join context, returns a <see cref="SqlBuilder"/>
    /// with the extent name, otherwise, a new <see cref="SqlSelectStatement"/>
    /// with the From field set.</returns>
    public override ISqlFragment Visit(DbScanExpression e)
    {
      EntitySetBase target = e.Target;

      if (IsParentAJoin)
      {
        SqlBuilder result = new SqlBuilder();
        result.Append(GetTargetTSql(target));

        return result;
      }
      else
      {
        SqlSelectStatement result = new SqlSelectStatement();
        result.From.Append(GetTargetTSql(target));

        return result;
      }
    }


    /// <summary>
    /// Gets escaped TSql identifier describing this entity set.
    /// </summary>
    /// <returns></returns>
    internal static string GetTargetTSql(EntitySetBase entitySetBase)
    {
      // construct escaped T-SQL referencing entity set
      StringBuilder builder = new StringBuilder(50);
      string definingQuery = MetadataHelpers.TryGetValueForMetadataProperty<string>(entitySetBase, "DefiningQuery");
      if (!string.IsNullOrEmpty(definingQuery))
      {
        //definingQuery = definingQuery.TrimStart(' ', '\t', '\r', '\n');
        //if (String.Compare(definingQuery, 0, "TYPES ", 0, 6, StringComparison.OrdinalIgnoreCase) == 0)
        //  definingQuery = definingQuery.Substring(definingQuery.IndexOf(';') + 1).TrimStart(' ', '\t', '\r', '\n');
        builder.Append("(");
        builder.Append(definingQuery);
        builder.Append(")");
      }
      else
      {
        //string schemaName = MetadataHelpers.TryGetValueForMetadataProperty<string>(entitySetBase, "Schema");
        //if (!string.IsNullOrEmpty(schemaName))
        //{
        //  builder.Append(SqlGenerator.QuoteIdentifier(schemaName));
        //  builder.Append(".");
        //}

        string tableName = MetadataHelpers.TryGetValueForMetadataProperty<string>(entitySetBase, "Table");
        if (!string.IsNullOrEmpty(tableName))
        {
          builder.Append(SqlGenerator.QuoteIdentifier(tableName));
        }
        else
        {
          builder.Append(SqlGenerator.QuoteIdentifier(entitySetBase.Name));
        }
      }
      return builder.ToString();
    }

    /// <summary>
    /// The bodies of <see cref="Visit(DbFilterExpression)"/>, <see cref="Visit(DbGroupByExpression)"/>,
    /// <see cref="Visit(DbProjectExpression)"/>, <see cref="Visit(DbSortExpression)"/> are similar.
    /// Each does the following.
    /// <list type="number">
    /// <item> Visit the input expression</item>
    /// <item> Determine if the input's SQL statement can be reused, or a new
    /// one must be created.</item>
    /// <item>Create a new symbol table scope</item>
    /// <item>Push the Sql statement onto a stack, so that children can
    /// update the free variable list.</item>
    /// <item>Visit the non-input expression.</item>
    /// <item>Cleanup</item>
    /// </list>
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlSelectStatement"/></returns>
    public override ISqlFragment Visit(DbFilterExpression e)
    {
      return VisitFilterExpression(e.Input, e.Predicate, false);
    }

    /// <summary>
    /// Lambda functions are not supported.
    /// The functions supported are:
    /// <list type="number">
    /// <item>Canonical Functions - We recognize these by their dataspace, it is DataSpace.CSpace</item>
    /// <item>Store Functions - We recognize these by the BuiltInAttribute and not being Canonical</item>
    /// <item>User-defined Functions - All the rest except for Lambda functions</item>
    /// </list>
    /// We handle Canonical and Store functions the same way: If they are in the list of functions 
    /// that need special handling, we invoke the appropriate handler, otherwise we translate them to
    /// FunctionName(arg1, arg2, ..., argn).
    /// We translate user-defined functions to NamespaceName.FunctionName(arg1, arg2, ..., argn).
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbFunctionExpression e)
    {
      //
      // check if function requires special case processing, if so, delegates to it
      //
      if (IsSpecialBuiltInFunction(e))
      {
        return HandleSpecialBuiltInFunction(e);
      }

      if (IsSpecialCanonicalFunction(e))
      {
        return HandleSpecialCanonicalFunction(e);
      }

      return HandleFunctionDefault(e);
    }


    /// <summary>
    /// <see cref="DbEntityRefExpression"/> is illegal at this stage
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbEntityRefExpression e)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// <see cref="DbRefKeyExpression"/> is illegal at this stage
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbRefKeyExpression e)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// <see cref="Visit(DbFilterExpression)"/> for general details.
    /// We modify both the GroupBy and the Select fields of the SqlSelectStatement.
    /// GroupBy gets just the keys without aliases,
    /// and Select gets the keys and the aggregates with aliases.
    /// 
    /// Whenever there exists at least one aggregate with an argument that is not is not a simple
    /// <see cref="DbPropertyExpression"/>  over <see cref="DbVariableReferenceExpression"/>, 
    /// we create a nested query in which we alias the arguments to the aggregates. 
    /// That is due to the following two limitations of Sql Server:
    /// <list type="number">
    /// <item>If an expression being aggregated contains an outer reference, then that outer 
    /// reference must be the only column referenced in the expression </item>
    /// <item>Sql Server cannot perform an aggregate function on an expression containing 
    /// an aggregate or a subquery. </item>
    /// </list>
    /// 
    /// The default translation, without inner query is: 
    /// 
    ///     SELECT 
    ///         kexp1 AS key1, kexp2 AS key2,... kexpn AS keyn, 
    ///         aggf1(aexpr1) AS agg1, .. aggfn(aexprn) AS aggn
    ///     FROM input AS a
    ///     GROUP BY kexp1, kexp2, .. kexpn
    /// 
    /// When we inject an innner query, the equivalent translation is:
    /// 
    ///     SELECT 
    ///         key1 AS key1, key2 AS key2, .. keyn AS keys,  
    ///         aggf1(agg1) AS agg1, aggfn(aggn) AS aggn
    ///     FROM (
    ///             SELECT 
    ///                 kexp1 AS key1, kexp2 AS key2,... kexpn AS keyn, 
    ///                 aexpr1 AS agg1, .. aexprn AS aggn
    ///             FROM input AS a
    ///         ) as a
    ///     GROUP BY key1, key2, keyn
    /// 
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlSelectStatement"/></returns>
    public override ISqlFragment Visit(DbGroupByExpression e)
    {
      Symbol fromSymbol;
      //SqlSelectStatement result = VisitInputExpression(e.Input.Expression,
      SqlSelectStatement innerQuery = VisitInputExpression(e.Input.Expression,
          e.Input.VariableName, e.Input.VariableType, out fromSymbol);

      // GroupBy is compatible with Filter and OrderBy
      // but not with Project, GroupBy
      if (!IsCompatible(innerQuery, e.ExpressionKind))
      {
        innerQuery = CreateNewSelectStatement(innerQuery, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
      }

      selectStatementStack.Push(innerQuery);
      symbolTable.EnterScope();

      AddFromSymbol(innerQuery, e.Input.VariableName, fromSymbol);
      // This line is not present for other relational nodes.
      symbolTable.Add(e.Input.GroupVariableName, fromSymbol);


      // The enumerator is shared by both the keys and the aggregates,
      // so, we do not close it in between.
      RowType groupByType = MetadataHelpers.GetEdmType<RowType>(MetadataHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);

      //Whenever there exists at least one aggregate with an argument that is not simply a PropertyExpression 
      // over a VarRefExpression, we need a nested query in which we alias the arguments to the aggregates.
      bool needsInnerQuery = NeedsInnerQuery(e.Aggregates);

      SqlSelectStatement result;
      if (needsInnerQuery)
      {
        //Create the inner query
        result = CreateNewSelectStatement(innerQuery, e.Input.VariableName, e.Input.VariableType, false, out fromSymbol);
        AddFromSymbol(result, e.Input.VariableName, fromSymbol, false);
      }
      else
      {
        result = innerQuery;
      }

      using (IEnumerator<EdmProperty> members = groupByType.Properties.GetEnumerator())
      {
        members.MoveNext();
        Debug.Assert(result.Select.IsEmpty);

        string separator = "";

        foreach (DbExpression key in e.Keys)
        {
          EdmProperty member = members.Current;
          string alias = QuoteIdentifier(member.Name);

          result.GroupBy.Append(separator);

          ISqlFragment keySql = key.Accept(this);

          if (!needsInnerQuery)
          {
            //Default translation: Key AS Alias
            result.Select.Append(separator);
            result.Select.AppendLine();
            result.Select.Append(keySql);
            result.Select.Append(" AS ");
            result.Select.Append(alias);

            result.GroupBy.Append(keySql);
          }
          else
          {
            // The inner query contains the default translation Key AS Alias
            innerQuery.Select.Append(separator);
            innerQuery.Select.AppendLine();
            innerQuery.Select.Append(keySql);
            innerQuery.Select.Append(" AS ");
            innerQuery.Select.Append(alias);

            //The outer resulting query projects over the key aliased in the inner query: 
            //  fromSymbol.Alias AS Alias
            result.Select.Append(separator);
            result.Select.AppendLine();
            result.Select.Append(fromSymbol);
            result.Select.Append(".");
            result.Select.Append(alias);
            result.Select.Append(" AS ");
            result.Select.Append(alias);

            result.GroupBy.Append(alias);
          }

          separator = ", ";
          members.MoveNext();
        }

        foreach (DbAggregate aggregate in e.Aggregates)
        {
          EdmProperty member = members.Current;
          string alias = QuoteIdentifier(member.Name);

          Debug.Assert(aggregate.Arguments.Count == 1);
          ISqlFragment translatedAggregateArgument = aggregate.Arguments[0].Accept(this);

          object aggregateArgument;

          if (needsInnerQuery)
          {
            //In this case the argument to the aggratete is reference to the one projected out by the
            // inner query
            SqlBuilder wrappingAggregateArgument = new SqlBuilder();
            wrappingAggregateArgument.Append(fromSymbol);
            wrappingAggregateArgument.Append(".");
            wrappingAggregateArgument.Append(alias);
            aggregateArgument = wrappingAggregateArgument;

            innerQuery.Select.Append(separator);
            innerQuery.Select.AppendLine();
            innerQuery.Select.Append(translatedAggregateArgument);
            innerQuery.Select.Append(" AS ");
            innerQuery.Select.Append(alias);
          }
          else
          {
            aggregateArgument = translatedAggregateArgument;
          }

          ISqlFragment aggregateResult = VisitAggregate(aggregate, aggregateArgument);

          result.Select.Append(separator);
          result.Select.AppendLine();
          result.Select.Append(aggregateResult);
          result.Select.Append(" AS ");
          result.Select.Append(alias);

          separator = ", ";
          members.MoveNext();
        }
      }


      symbolTable.ExitScope();
      selectStatementStack.Pop();

      return result;
    }

    /// <summary>
    /// <see cref="Visit(DbUnionAllExpression)"/>
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbIntersectExpression e)
    {
      return VisitSetOpExpression(e.Left, e.Right, "INTERSECT");
    }

    /// <summary>
    /// Not(IsEmpty) has to be handled specially, so we delegate to
    /// <see cref="VisitIsEmptyExpression"/>.
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/>.
    /// <code>[NOT] EXISTS( ... )</code>
    /// </returns>
    public override ISqlFragment Visit(DbIsEmptyExpression e)
    {
      return VisitIsEmptyExpression(e, false);
    }

    /// <summary>
    /// Not(IsNull) is handled specially, so we delegate to
    /// <see cref="VisitIsNullExpression"/>
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/>
    /// <code>IS [NOT] NULL</code>
    /// </returns>
    public override ISqlFragment Visit(DbIsNullExpression e)
    {
      return VisitIsNullExpression(e, false);
    }

    /// <summary>
    /// <see cref="DbIsOfExpression"/> is illegal at this stage
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbIsOfExpression e)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// <see cref="VisitJoinExpression"/>
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
    public override ISqlFragment Visit(DbCrossJoinExpression e)
    {
      return VisitJoinExpression(e.Inputs, e.ExpressionKind, "CROSS JOIN", null);
    }

    /// <summary>
    /// <see cref="VisitJoinExpression"/>
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
    public override ISqlFragment Visit(DbJoinExpression e)
    {
      #region Map join type to a string
      string joinString;
      switch (e.ExpressionKind)
      {
        case DbExpressionKind.FullOuterJoin:
          joinString = "FULL OUTER JOIN";
          break;

        case DbExpressionKind.InnerJoin:
          joinString = "INNER JOIN";
          break;

        case DbExpressionKind.LeftOuterJoin:
          joinString = "LEFT OUTER JOIN";
          break;

        default:
          Debug.Assert(false);
          joinString = null;
          break;
      }
      #endregion

      List<DbExpressionBinding> inputs = new List<DbExpressionBinding>(2);
      inputs.Add(e.Left);
      inputs.Add(e.Right);

      return VisitJoinExpression(inputs, e.ExpressionKind, joinString, e.JoinCondition);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbLikeExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      result.Append(e.Argument.Accept(this));
      result.Append(" LIKE ");
      result.Append(e.Pattern.Accept(this));

      // if the ESCAPE expression is a DbNullExpression, then that's tantamount to 
      // not having an ESCAPE at all
      if (e.Escape.ExpressionKind != DbExpressionKind.Null)
      {
        result.Append(" ESCAPE ");
        result.Append(e.Escape.Accept(this));
      }

      return result;
    }

    /// <summary>
    ///  Translates to TOP expression.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbLimitExpression e)
    {
      Debug.Assert(e.Limit is DbConstantExpression || e.Limit is DbParameterReferenceExpression, "DbLimitExpression.Limit is of invalid expression type");

      SqlSelectStatement result = VisitExpressionEnsureSqlStatement(e.Argument, false);
      Symbol fromSymbol;

      if (!IsCompatible(result, e.ExpressionKind))
      {
        TypeUsage inputType = MetadataHelpers.GetElementTypeUsage(e.Argument.ResultType);

        result = CreateNewSelectStatement(result, "top", inputType, out fromSymbol);
        AddFromSymbol(result, "top", fromSymbol, false);
      }

      ISqlFragment topCount = HandleCountExpression(e.Limit);

      result.Top = new TopClause(topCount, e.WithTies);
      return result;
    }

    /// <summary>
    /// DbNewInstanceExpression is allowed as a child of DbProjectExpression only.
    /// If anyone else is the parent, we throw.
    /// We also perform special casing for collections - where we could convert
    /// them into Unions
    ///
    /// <see cref="VisitNewInstanceExpression"/> for the actual implementation.
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbNewInstanceExpression e)
    {
      if (MetadataHelpers.IsCollectionType(e.ResultType))
      {
        return VisitCollectionConstructor(e);
      }
      throw new NotSupportedException();
    }

    /// <summary>
    /// The Not expression may cause the translation of its child to change.
    /// These children are
    /// <list type="bullet">
    /// <item><see cref="DbNotExpression"/>NOT(Not(x)) becomes x</item>
    /// <item><see cref="DbIsEmptyExpression"/>NOT EXISTS becomes EXISTS</item>
    /// <item><see cref="DbIsNullExpression"/>IS NULL becomes IS NOT NULL</item>
    /// <item><see cref="DbComparisonExpression"/>= becomes&lt;&gt; </item>
    /// </list>
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbNotExpression e)
    {
      // Flatten Not(Not(x)) to x.
      DbNotExpression notExpression = e.Argument as DbNotExpression;
      if (notExpression != null)
      {
        return notExpression.Argument.Accept(this);
      }

      DbIsEmptyExpression isEmptyExpression = e.Argument as DbIsEmptyExpression;
      if (isEmptyExpression != null)
      {
        return VisitIsEmptyExpression(isEmptyExpression, true);
      }

      DbIsNullExpression isNullExpression = e.Argument as DbIsNullExpression;
      if (isNullExpression != null)
      {
        return VisitIsNullExpression(isNullExpression, true);
      }

      DbComparisonExpression comparisonExpression = e.Argument as DbComparisonExpression;
      if (comparisonExpression != null)
      {
        if (comparisonExpression.ExpressionKind == DbExpressionKind.Equals)
        {
          return VisitBinaryExpression(" <> ", comparisonExpression.Left, comparisonExpression.Right);
        }
      }

      SqlBuilder result = new SqlBuilder();
      result.Append(" NOT (");
      result.Append(e.Argument.Accept(this));
      result.Append(")");

      return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="e"></param>
    /// <returns><see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbNullExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      // always cast nulls - sqlserver doesn't like case expressions where the "then" clause is null
      result.Append("NULL");
      return result;
    }

    /// <summary>
    /// <see cref="DbOfTypeExpression"/> is illegal at this stage
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbOfTypeExpression e)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    /// <seealso cref="Visit(DbAndExpression)"/>
    public override ISqlFragment Visit(DbOrExpression e)
    {
      ISqlFragment sqlFragment = null;
      if (this.TryTranslateIntoIn(e, out sqlFragment))
      {
        return sqlFragment;
      }
      return VisitBinaryExpression(" OR ", e.Left, e.Right);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbParameterReferenceExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      // Do not quote this name.
      // We are not checking that e.Name has no illegal characters. e.g. space
      result.Append("@" + e.ParameterName);

      return result;
    }

    /// <summary>
    /// <see cref="Visit(DbFilterExpression)"/> for the general ideas.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlSelectStatement"/></returns>
    /// <seealso cref="Visit(DbFilterExpression)"/>
    public override ISqlFragment Visit(DbProjectExpression e)
    {
      Symbol fromSymbol;
      SqlSelectStatement result = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

      // Project is compatible with Filter
      // but not with Project, GroupBy
      if (!IsCompatible(result, e.ExpressionKind))
      {
        result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
      }

      selectStatementStack.Push(result);
      symbolTable.EnterScope();

      AddFromSymbol(result, e.Input.VariableName, fromSymbol);

      // Project is the only node that can have DbNewInstanceExpression as a child
      // so we have to check it here.
      // We call VisitNewInstanceExpression instead of Visit(DbNewInstanceExpression), since
      // the latter throws.
      DbNewInstanceExpression newInstanceExpression = e.Projection as DbNewInstanceExpression;
      if (newInstanceExpression != null)
      {
        result.Select.Append(VisitNewInstanceExpression(newInstanceExpression));
      }
      else
      {
        result.Select.Append(e.Projection.Accept(this));
      }

      symbolTable.ExitScope();
      selectStatementStack.Pop();

      return result;
    }

    /// <summary>
    /// This method handles record flattening, which works as follows.
    /// consider an expression <c>Prop(y, Prop(x, Prop(d, Prop(c, Prop(b, Var(a)))))</c>
    /// where a,b,c are joins, d is an extent and x and y are fields.
    /// b has been flattened into a, and has its own SELECT statement.
    /// c has been flattened into b.
    /// d has been flattened into c.
    ///
    /// We visit the instance, so we reach Var(a) first.  This gives us a (join)symbol.
    /// Symbol(a).b gives us a join symbol, with a SELECT statement i.e. Symbol(b).
    /// From this point on , we need to remember Symbol(b) as the source alias,
    /// and then try to find the column.  So, we use a SymbolPair.
    ///
    /// We have reached the end when the symbol no longer points to a join symbol.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="JoinSymbol"/> if we have not reached the first
    /// Join node that has a SELECT statement.
    /// A <see cref="SymbolPair"/> if we have seen the JoinNode, and it has
    /// a SELECT statement.
    /// A <see cref="SqlBuilder"/> with {Input}.propertyName otherwise.
    /// </returns>
    public override ISqlFragment Visit(DbPropertyExpression e)
    {
      SqlBuilder result;

      ISqlFragment instanceSql = e.Instance.Accept(this);

      // Since the DbVariableReferenceExpression is a proper child of ours, we can reset
      // isVarSingle.
      DbVariableReferenceExpression DbVariableReferenceExpression = e.Instance as DbVariableReferenceExpression;
      if (DbVariableReferenceExpression != null)
      {
        isVarRefSingle = false;
      }

      // We need to flatten, and have not yet seen the first nested SELECT statement.
      JoinSymbol joinSymbol = instanceSql as JoinSymbol;
      if (joinSymbol != null)
      {
        Debug.Assert(joinSymbol.NameToExtent.ContainsKey(e.Property.Name));
        if (joinSymbol.IsNestedJoin)
        {
          return new SymbolPair(joinSymbol, joinSymbol.NameToExtent[e.Property.Name]);
        }
        else
        {
          return joinSymbol.NameToExtent[e.Property.Name];
        }
      }

      // ---------------------------------------
      // We have seen the first nested SELECT statement, but not the column.
      SymbolPair symbolPair = instanceSql as SymbolPair;
      if (symbolPair != null)
      {
        JoinSymbol columnJoinSymbol = symbolPair.Column as JoinSymbol;
        if (columnJoinSymbol != null)
        {
          symbolPair.Column = columnJoinSymbol.NameToExtent[e.Property.Name];
          return symbolPair;
        }
        else
        {
          // symbolPair.Column has the base extent.
          // we need the symbol for the column, since it might have been renamed
          // when handling a JOIN.
          if (symbolPair.Column.Columns.ContainsKey(e.Property.Name))
          {
            result = new SqlBuilder();
            result.Append(symbolPair.Source);
            result.Append(".");
            result.Append(symbolPair.Column.Columns[e.Property.Name]);
            return result;
          }
        }
      }
      // ---------------------------------------

      result = new SqlBuilder();
      result.Append(instanceSql);
      result.Append(".");

      // At this point the column name cannot be renamed, so we do
      // not use a symbol.
      result.Append(QuoteIdentifier(e.Property.Name));

      return result;
    }

    /// <summary>
    /// Any(input, x) => Exists(Filter(input,x))
    /// All(input, x) => Not Exists(Filter(input, not(x))
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbQuantifierExpression e)
    {
      SqlBuilder result = new SqlBuilder();

      bool negatePredicate = (e.ExpressionKind == DbExpressionKind.All);
      if (e.ExpressionKind == DbExpressionKind.Any)
      {
        result.Append("EXISTS (");
      }
      else
      {
        Debug.Assert(e.ExpressionKind == DbExpressionKind.All);
        result.Append("NOT EXISTS (");
      }

      SqlSelectStatement filter = VisitFilterExpression(e.Input, e.Predicate, negatePredicate);
      if (filter.Select.IsEmpty)
      {
        AddDefaultColumns(filter);
      }

      result.Append(filter);
      result.Append(")");

      return result;
    }

    /// <summary>
    /// <see cref="DbRefExpression"/> is illegal at this stage
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbRefExpression e)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// <see cref="DbRelationshipNavigationExpression"/> is illegal at this stage
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbRelationshipNavigationExpression e)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// For Sql9 it translates to:
    /// SELECT Y.x1, Y.x2, ..., Y.xn
    /// FROM (
    ///     SELECT X.x1, X.x2, ..., X.xn, row_number() OVER (ORDER BY sk1, sk2, ...) AS [row_number] 
    ///     FROM input as X 
    ///     ) as Y
    /// WHERE Y.[row_number] > count 
    /// ORDER BY sk1, sk2, ...
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbSkipExpression e)
    {
        Debug.Assert(e.Count is DbConstantExpression || e.Count is DbParameterReferenceExpression, "DbLimitExpression.Count is of invalid expression type");

        Symbol fromSymbol;
        SqlSelectStatement result = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

        if (!IsCompatible(result, e.ExpressionKind))
        {
            result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
        }

        selectStatementStack.Push(result);
        symbolTable.EnterScope();

        AddFromSymbol(result, e.Input.VariableName, fromSymbol);

        AddSortKeys(result.OrderBy, e.SortOrder);

        symbolTable.ExitScope();
        selectStatementStack.Pop();

        ISqlFragment skipCount = HandleCountExpression(e.Count);

        result.Skip = new SkipClause(skipCount);
        return result;
    }

    /// <summary>
    /// <see cref="Visit(DbFilterExpression)"/>
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlSelectStatement"/></returns>
    /// <seealso cref="Visit(DbFilterExpression)"/>
    public override ISqlFragment Visit(DbSortExpression e)
    {
      Symbol fromSymbol;
      SqlSelectStatement result = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

      // OrderBy is compatible with Filter
      // and nothing else
      if (!IsCompatible(result, e.ExpressionKind))
      {
        result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
      }

      selectStatementStack.Push(result);
      symbolTable.EnterScope();

      AddFromSymbol(result, e.Input.VariableName, fromSymbol);

      AddSortKeys(result.OrderBy, e.SortOrder);

      symbolTable.ExitScope();
      selectStatementStack.Pop();

      return result;
    }

    /// <summary>
    /// <see cref="DbTreatExpression"/> is illegal at this stage
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    public override ISqlFragment Visit(DbTreatExpression e)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// This code is shared by <see cref="Visit(DbExceptExpression)"/>
    /// and <see cref="Visit(DbIntersectExpression)"/>
    ///
    /// <see cref="VisitSetOpExpression"/>
    /// Since the left and right expression may not be Sql select statements,
    /// we must wrap them up to look like SQL select statements.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override ISqlFragment Visit(DbUnionAllExpression e)
    {
      return VisitSetOpExpression(e.Left, e.Right, "UNION ALL");
    }

    /// <summary>
    /// This method determines whether an extent from an outer scope(free variable)
    /// is used in the CurrentSelectStatement.
    ///
    /// An extent in an outer scope, if its symbol is not in the FromExtents
    /// of the CurrentSelectStatement.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="Symbol"/>.</returns>
    public override ISqlFragment Visit(DbVariableReferenceExpression e)
    {
      if (isVarRefSingle)
      {
        throw new NotSupportedException();
        // A DbVariableReferenceExpression has to be a child of DbPropertyExpression or MethodExpression
        // This is also checked in GenerateSql(...) at the end of the visiting.
      }
      isVarRefSingle = true; // This will be reset by DbPropertyExpression or MethodExpression

      Symbol result = symbolTable.Lookup(e.VariableName);
      if (!CurrentSelectStatement.FromExtents.Contains(result))
      {
        CurrentSelectStatement.OuterExtents[result] = true;
      }

      return result;
    }

    #region Visits shared by multiple nodes
    /// <summary>
    /// Aggregates are not visited by the normal visitor walk.
    /// </summary>
    /// <param name="aggregate">The aggreate to be translated</param>
    /// <param name="aggregateArgument">The translated aggregate argument</param>
    /// <returns></returns>
    SqlBuilder VisitAggregate(DbAggregate aggregate, object aggregateArgument)
    {
      SqlBuilder aggregateResult = new SqlBuilder();
      DbFunctionAggregate functionAggregate = aggregate as DbFunctionAggregate;

      if (functionAggregate == null)
      {
        throw new NotSupportedException();
      }

      //The only aggregate function with different name is Big_Count
      //Note: If another such function is to be added, a dictionary should be created
      //if (MetadataHelpers.IsCanonicalFunction(functionAggregate.Function)
      //    && String.Equals(functionAggregate.Function.Name, "BigCount", StringComparison.Ordinal))
      //{
      //  aggregateResult.Append("COUNT_BIG");
      //}
      //else
      {
        WriteFunctionName(aggregateResult, functionAggregate.Function);
      }

      aggregateResult.Append("(");

      DbFunctionAggregate fnAggr = functionAggregate;
      if ((null != fnAggr) && (fnAggr.Distinct))
      {
        aggregateResult.Append("DISTINCT ");
      }

      aggregateResult.Append(aggregateArgument);

      aggregateResult.Append(")");
      return aggregateResult;
    }


    SqlBuilder VisitBinaryExpression(string op, DbExpression left, DbExpression right)
    {
      SqlBuilder result = new SqlBuilder();
      if (IsComplexExpression(left))
      {
        result.Append("(");
      }

      result.Append(left.Accept(this));

      if (IsComplexExpression(left))
      {
        result.Append(")");
      }

      result.Append(op);

      if (IsComplexExpression(right))
      {
        result.Append("(");
      }

      result.Append(right.Accept(this));

      if (IsComplexExpression(right))
      {
        result.Append(")");
      }

      return result;
    }

    /// <summary>
    /// This is called by the relational nodes.  It does the following
    /// <list>
    /// <item>If the input is not a SqlSelectStatement, it assumes that the input
    /// is a collection expression, and creates a new SqlSelectStatement </item>
    /// </list>
    /// </summary>
    /// <param name="inputExpression"></param>
    /// <param name="inputVarName"></param>
    /// <param name="inputVarType"></param>
    /// <param name="fromSymbol"></param>
    /// <returns>A <see cref="SqlSelectStatement"/> and the main fromSymbol
    /// for this select statement.</returns>
    SqlSelectStatement VisitInputExpression(DbExpression inputExpression,
        string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
    {
      SqlSelectStatement result;
      ISqlFragment sqlFragment = inputExpression.Accept(this);
      result = sqlFragment as SqlSelectStatement;

      if (result == null)
      {
        result = new SqlSelectStatement();
        WrapNonQueryExtent(result, sqlFragment, inputExpression.ExpressionKind);
      }

      if (result.FromExtents.Count == 0)
      {
        // input was an extent
        fromSymbol = new Symbol(inputVarName, inputVarType);
      }
      else if (result.FromExtents.Count == 1)
      {
        // input was Filter/GroupBy/Project/OrderBy
        // we are likely to reuse this statement.
        fromSymbol = result.FromExtents[0];
      }
      else
      {
        // input was a join.
        // we are reusing the select statement produced by a Join node
        // we need to remove the original extents, and replace them with a
        // new extent with just the Join symbol.
        JoinSymbol joinSymbol = new JoinSymbol(inputVarName, inputVarType, result.FromExtents);
        joinSymbol.FlattenedExtentList = result.AllJoinExtents;

        fromSymbol = joinSymbol;
        result.FromExtents.Clear();
        result.FromExtents.Add(fromSymbol);
      }

      return result;
    }

    /// <summary>
    /// <see cref="Visit(DbIsEmptyExpression)"/>
    /// </summary>
    /// <param name="e"></param>
    /// <param name="negate">Was the parent a DbNotExpression?</param>
    /// <returns></returns>
    SqlBuilder VisitIsEmptyExpression(DbIsEmptyExpression e, bool negate)
    {
      SqlBuilder result = new SqlBuilder();
      if (!negate)
      {
        result.Append(" NOT");
      }
      result.Append(" EXISTS (");
      result.Append(VisitExpressionEnsureSqlStatement(e.Argument));
      result.AppendLine();
      result.Append(")");

      return result;
    }


    /// <summary>
    /// Translate a NewInstance(Element(X)) expression into
    ///   "select top(1) * from X"
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private ISqlFragment VisitCollectionConstructor(DbNewInstanceExpression e)
    {
      Debug.Assert(e.Arguments.Count <= 1);

      if (e.Arguments.Count == 1 && e.Arguments[0].ExpressionKind == DbExpressionKind.Element)
      {
        DbElementExpression elementExpr = e.Arguments[0] as DbElementExpression;
        SqlSelectStatement result = VisitExpressionEnsureSqlStatement(elementExpr.Argument);

        if (!IsCompatible(result, DbExpressionKind.Element))
        {
          Symbol fromSymbol;
          TypeUsage inputType = MetadataHelpers.GetElementTypeUsage(elementExpr.Argument.ResultType);

          result = CreateNewSelectStatement(result, "element", inputType, out fromSymbol);
          AddFromSymbol(result, "element", fromSymbol, false);
        }
        result.Top = new TopClause(1, false);
        return result;
      }


      // Otherwise simply build this out as a union-all ladder
      CollectionType collectionType = MetadataHelpers.GetEdmType<CollectionType>(e.ResultType);
      Debug.Assert(collectionType != null);
      bool isScalarElement = MetadataHelpers.IsPrimitiveType(collectionType.TypeUsage);

      SqlBuilder resultSql = new SqlBuilder();
      string separator = "";

      // handle empty table
      if (e.Arguments.Count == 0)
      {
        Debug.Assert(isScalarElement);
        resultSql.Append(" SELECT NULL");
        resultSql.Append(" AS X FROM (SELECT 1) AS Y WHERE 1=0");
      }

      foreach (DbExpression arg in e.Arguments)
      {
        resultSql.Append(separator);
        resultSql.Append(" SELECT ");
        resultSql.Append(arg.Accept(this));
        // For scalar elements, no alias is appended yet. Add this.
        if (isScalarElement)
        {
          resultSql.Append(" AS X ");
        }
        separator = " UNION ALL ";
      }

      return resultSql;
    }


    /// <summary>
    /// <see cref="Visit(DbIsNullExpression)"/>
    /// </summary>
    /// <param name="e"></param>
    /// <param name="negate">Was the parent a DbNotExpression?</param>
    /// <returns></returns>
    SqlBuilder VisitIsNullExpression(DbIsNullExpression e, bool negate)
    {
      SqlBuilder result = new SqlBuilder();
      result.Append(e.Argument.Accept(this));
      if (!negate)
      {
        result.Append(" IS NULL");
      }
      else
      {
        result.Append(" IS NOT NULL");
      }

      return result;
    }

    /// <summary>
    /// This handles the processing of join expressions.
    /// The extents on a left spine are flattened, while joins
    /// not on the left spine give rise to new nested sub queries.
    ///
    /// Joins work differently from the rest of the visiting, in that
    /// the parent (i.e. the join node) creates the SqlSelectStatement
    /// for the children to use.
    ///
    /// The "parameter" IsInJoinContext indicates whether a child extent should
    /// add its stuff to the existing SqlSelectStatement, or create a new SqlSelectStatement
    /// By passing true, we ask the children to add themselves to the parent join,
    /// by passing false, we ask the children to create new Select statements for
    /// themselves.
    ///
    /// This method is called from <see cref="Visit(DbApplyExpression)"/> and
    /// <see cref="Visit(DbJoinExpression)"/>.
    /// </summary>
    /// <param name="inputs"></param>
    /// <param name="joinKind"></param>
    /// <param name="joinString"></param>
    /// <param name="joinCondition"></param>
    /// <returns> A <see cref="SqlSelectStatement"/></returns>
    ISqlFragment VisitJoinExpression(IList<DbExpressionBinding> inputs, DbExpressionKind joinKind,
        string joinString, DbExpression joinCondition)
    {
      SqlSelectStatement result;
      // If the parent is not a join( or says that it is not),
      // we should create a new SqlSelectStatement.
      // otherwise, we add our child extents to the parent's FROM clause.
      if (!IsParentAJoin)
      {
        result = new SqlSelectStatement();
        result.AllJoinExtents = new List<Symbol>();
        selectStatementStack.Push(result);
      }
      else
      {
        result = CurrentSelectStatement;
      }

      // Process each of the inputs, and then the joinCondition if it exists.
      // It would be nice if we could call VisitInputExpression - that would
      // avoid some code duplication
      // but the Join postprocessing is messy and prevents this reuse.
      symbolTable.EnterScope();

      string separator = "";
      bool isLeftMostInput = true;
      int inputCount = inputs.Count;
      for (int idx = 0; idx < inputCount; idx++)
      {
        DbExpressionBinding input = inputs[idx];

        if (separator != "")
        {
          result.From.AppendLine();
        }
        result.From.Append(separator + " ");
        // Change this if other conditions are required
        // to force the child to produce a nested SqlStatement.
        bool needsJoinContext = (input.Expression.ExpressionKind == DbExpressionKind.Scan)
                                || (isLeftMostInput &&
                                    (IsJoinExpression(input.Expression)
                                     || IsApplyExpression(input.Expression)))
                                ;

        isParentAJoinStack.Push(needsJoinContext ? true : false);
        // if the child reuses our select statement, it will append the from
        // symbols to our FromExtents list.  So, we need to remember the
        // start of the child's entries.
        int fromSymbolStart = result.FromExtents.Count;

        ISqlFragment fromExtentFragment = input.Expression.Accept(this);

        isParentAJoinStack.Pop();

        ProcessJoinInputResult(fromExtentFragment, result, input, fromSymbolStart);
        separator = joinString;

        isLeftMostInput = false;
      }

      // Visit the on clause/join condition.
      switch (joinKind)
      {
        case DbExpressionKind.FullOuterJoin:
        case DbExpressionKind.InnerJoin:
        case DbExpressionKind.LeftOuterJoin:
          result.From.Append(" ON ");
          isParentAJoinStack.Push(false);
          result.From.Append(joinCondition.Accept(this));
          isParentAJoinStack.Pop();
          break;
      }

      symbolTable.ExitScope();

      if (!IsParentAJoin)
      {
        selectStatementStack.Pop();
      }

      return result;
    }

    /// <summary>
    /// This is called from <see cref="VisitJoinExpression"/>.
    ///
    /// This is responsible for maintaining the symbol table after visiting
    /// a child of a join expression.
    ///
    /// The child's sql statement may need to be completed.
    ///
    /// The child's result could be one of
    /// <list type="number">
    /// <item>The same as the parent's - this is treated specially.</item>
    /// <item>A sql select statement, which may need to be completed</item>
    /// <item>An extent - just copy it to the from clause</item>
    /// <item>Anything else (from a collection-valued expression) -
    /// unnest and copy it.</item>
    /// </list>
    ///
    /// If the input was a Join, we need to create a new join symbol,
    /// otherwise, we create a normal symbol.
    ///
    /// We then call AddFromSymbol to add the AS clause, and update the symbol table.
    ///
    ///
    ///
    /// If the child's result was the same as the parent's, we have to clean up
    /// the list of symbols in the FromExtents list, since this contains symbols from
    /// the children of both the parent and the child.
    /// The happens when the child visited is a Join, and is the leftmost child of
    /// the parent.
    /// </summary>
    /// <param name="fromExtentFragment"></param>
    /// <param name="result"></param>
    /// <param name="input"></param>
    /// <param name="fromSymbolStart"></param>
    void ProcessJoinInputResult(ISqlFragment fromExtentFragment, SqlSelectStatement result,
        DbExpressionBinding input, int fromSymbolStart)
    {
      Symbol fromSymbol = null;

      if (result != fromExtentFragment)
      {
        // The child has its own select statement, and is not reusing
        // our select statement.
        // This should look a lot like VisitInputExpression().
        SqlSelectStatement sqlSelectStatement = fromExtentFragment as SqlSelectStatement;
        if (sqlSelectStatement != null)
        {
          if (sqlSelectStatement.Select.IsEmpty)
          {
            List<Symbol> columns = AddDefaultColumns(sqlSelectStatement);

            if (IsJoinExpression(input.Expression)
                || IsApplyExpression(input.Expression))
            {
              List<Symbol> extents = sqlSelectStatement.FromExtents;
              JoinSymbol newJoinSymbol = new JoinSymbol(input.VariableName, input.VariableType, extents);
              newJoinSymbol.IsNestedJoin = true;
              newJoinSymbol.ColumnList = columns;

              fromSymbol = newJoinSymbol;
            }
            else
            {
              // this is a copy of the code in CreateNewSelectStatement.

              // if the oldStatement has a join as its input, ...
              // clone the join symbol, so that we "reuse" the
              // join symbol.  Normally, we create a new symbol - see the next block
              // of code.
              JoinSymbol oldJoinSymbol = sqlSelectStatement.FromExtents[0] as JoinSymbol;
              if (oldJoinSymbol != null)
              {
                // Note: sqlSelectStatement.FromExtents will not do, since it might
                // just be an alias of joinSymbol, and we want an actual JoinSymbol.
                JoinSymbol newJoinSymbol = new JoinSymbol(input.VariableName, input.VariableType, oldJoinSymbol.ExtentList);
                // This indicates that the sqlSelectStatement is a blocking scope
                // i.e. it hides/renames extent columns
                newJoinSymbol.IsNestedJoin = true;
                newJoinSymbol.ColumnList = columns;
                newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

                fromSymbol = newJoinSymbol;
              }
            }

          }
          result.From.Append(" (");
          result.From.Append(sqlSelectStatement);
          result.From.Append(" )");
        }
        else if (input.Expression is DbScanExpression)
        {
          result.From.Append(fromExtentFragment);
        }
        else // bracket it
        {
          WrapNonQueryExtent(result, fromExtentFragment, input.Expression.ExpressionKind);
        }

        if (fromSymbol == null) // i.e. not a join symbol
        {
          fromSymbol = new Symbol(input.VariableName, input.VariableType);
        }


        AddFromSymbol(result, input.VariableName, fromSymbol);
        result.AllJoinExtents.Add(fromSymbol);
      }
      else // result == fromExtentFragment.  The child extents have been merged into the parent's.
      {
        // we are adding extents to the current sql statement via flattening.
        // We are replacing the child's extents with a single Join symbol.
        // The child's extents are all those following the index fromSymbolStart.
        //
        List<Symbol> extents = new List<Symbol>();

        // We cannot call extents.AddRange, since the is no simple way to
        // get the range of symbols fromSymbolStart..result.FromExtents.Count
        // from result.FromExtents.
        // We copy these symbols to create the JoinSymbol later.
        for (int i = fromSymbolStart; i < result.FromExtents.Count; ++i)
        {
          extents.Add(result.FromExtents[i]);
        }
        result.FromExtents.RemoveRange(fromSymbolStart, result.FromExtents.Count - fromSymbolStart);
        fromSymbol = new JoinSymbol(input.VariableName, input.VariableType, extents);
        result.FromExtents.Add(fromSymbol);
        // this Join Symbol does not have its own select statement, so we
        // do not set IsNestedJoin


        // We do not call AddFromSymbol(), since we do not want to add
        // "AS alias" to the FROM clause- it has been done when the extent was added earlier.
        symbolTable.Add(input.VariableName, fromSymbol);
      }
    }

    /// <summary>
    /// We assume that this is only called as a child of a Project.
    ///
    /// This replaces <see cref="Visit(DbNewInstanceExpression)"/>, since
    /// we do not allow DbNewInstanceExpression as a child of any node other than
    /// DbProjectExpression.
    ///
    /// We write out the translation of each of the columns in the record.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>A <see cref="SqlBuilder"/></returns>
    ISqlFragment VisitNewInstanceExpression(DbNewInstanceExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      RowType rowType = e.ResultType.EdmType as RowType;

      if (null != rowType)
      {
        //_typeDefs.Length = 0;
        ReadOnlyMetadataCollection<EdmProperty> members = rowType.Properties;
        string separator = "";
        for (int i = 0; i < e.Arguments.Count; ++i)
        {
          DbExpression argument = e.Arguments[i];
          if (MetadataHelpers.IsRowType(argument.ResultType))
          {
            // We do not support nested records or other complex objects.
            throw new NotSupportedException();
          }

          EdmProperty member = members[i];
          //_typeDefs.Append(separator);
          //_typeDefs.Append(GetSqlPrimitiveType(member.TypeUsage));
          result.Append(separator);
          result.AppendLine();
          result.Append(argument.Accept(this));
          result.Append(" AS ");
          result.Append(QuoteIdentifier(member.Name));
          separator = ", ";
        }
      }
      else
      {
        //
        // Types other then RowType (such as UDTs for instance) are not supported.
        //
        throw new NotSupportedException();
      }

      return result;
    }

    ISqlFragment VisitSetOpExpression(DbExpression left, DbExpression right, string separator)
    {

      SqlSelectStatement leftSelectStatement = VisitExpressionEnsureSqlStatement(left);
      SqlSelectStatement rightSelectStatement = VisitExpressionEnsureSqlStatement(right);

      SqlBuilder setStatement = new SqlBuilder();
      setStatement.Append(leftSelectStatement);
      setStatement.AppendLine();
      setStatement.Append(separator); // e.g. UNION ALL
      setStatement.AppendLine();
      setStatement.Append(rightSelectStatement);

      return setStatement;
    }


    #endregion


    #region Function Handling Helpers
    /// <summary>
    /// Determines whether the given function is a built-in function that requires special handling
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private bool IsSpecialBuiltInFunction(DbFunctionExpression e)
    {
      return IsBuiltinFunction(e.Function) && _builtInFunctionHandlers.ContainsKey(e.Function.Name);
    }

    /// <summary>
    /// Determines whether the given function is a canonical function that requires special handling
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private bool IsSpecialCanonicalFunction(DbFunctionExpression e)
    {
      return MetadataHelpers.IsCanonicalFunction(e.Function) && _canonicalFunctionHandlers.ContainsKey(e.Function.Name);
    }

    /// <summary>
    /// Default handling for functions
    /// Translates them to FunctionName(arg1, arg2, ..., argn)
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private ISqlFragment HandleFunctionDefault(DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      WriteFunctionName(result, e.Function);
      HandleFunctionArgumentsDefault(e, result);
      return result;
    }

    /// <summary>
    /// Default handling for functions with a given name.
    /// Translates them to functionName(arg1, arg2, ..., argn)
    /// </summary>
    /// <param name="e"></param>
    /// <param name="functionName"></param>
    /// <returns></returns>
    private ISqlFragment HandleFunctionDefaultGivenName(DbFunctionExpression e, string functionName)
    {
      SqlBuilder result = new SqlBuilder();
      result.Append(functionName);
      HandleFunctionArgumentsDefault(e, result);
      return result;
    }

    /// <summary>
    /// Default handling on function arguments
    /// Appends the list of arguments to the given result
    /// If the function is niladic it does not append anything,
    /// otherwise it appends (arg1, arg2, ..., argn)
    /// </summary>
    /// <param name="e"></param>
    /// <param name="result"></param>
    private void HandleFunctionArgumentsDefault(DbFunctionExpression e, SqlBuilder result)
    {
      bool isNiladicFunction = MetadataHelpers.TryGetValueForMetadataProperty<bool>(e.Function, "NiladicFunctionAttribute");
      if (isNiladicFunction && e.Arguments.Count > 0)
      {
        throw new InvalidOperationException("Niladic functions cannot have parameters");
      }

      if (!isNiladicFunction)
      {
        result.Append("(");
        string separator = "";
        foreach (DbExpression arg in e.Arguments)
        {
          result.Append(separator);
          result.Append(arg.Accept(this));
          separator = ", ";
        }
        result.Append(")");
      }
    }

    /// <summary>
    /// Handler for special built in functions
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private ISqlFragment HandleSpecialBuiltInFunction(DbFunctionExpression e)
    {
      return HandleSpecialFunction(_builtInFunctionHandlers, e);
    }

    /// <summary>
    /// Handler for special canonical functions
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private ISqlFragment HandleSpecialCanonicalFunction(DbFunctionExpression e)
    {
      return HandleSpecialFunction(_canonicalFunctionHandlers, e);
    }

    /// <summary>
    /// Dispatches the special function processing to the appropriate handler
    /// </summary>
    /// <param name="handlers"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private ISqlFragment HandleSpecialFunction(Dictionary<string, FunctionHandler> handlers, DbFunctionExpression e)
    {
      if (!handlers.ContainsKey(e.Function.Name))
        throw new InvalidOperationException("Special handling should be called only for functions in the list of special functions");

      return handlers[e.Function.Name](this, e);
    }

    /// <summary>
    /// Handles functions that are translated into TSQL operators.
    /// The given function should have one or two arguments. 
    /// Functions with one arguemnt are translated into 
    ///     op arg
    /// Functions with two arguments are translated into
    ///     arg0 op arg1
    /// Also, the arguments can be optionaly enclosed in parethesis
    /// </summary>
    /// <param name="e"></param>
    /// <param name="parenthesiseArguments">Whether the arguments should be enclosed in parethesis</param>
    /// <returns></returns>
    private ISqlFragment HandleSpecialFunctionToOperator(DbFunctionExpression e, bool parenthesiseArguments)
    {
      SqlBuilder result = new SqlBuilder();
      Debug.Assert(e.Arguments.Count > 0 && e.Arguments.Count <= 2, "There should be 1 or 2 arguments for operator");

      if (e.Arguments.Count > 1)
      {
        if (parenthesiseArguments)
        {
          result.Append("(");
        }
        result.Append(e.Arguments[0].Accept(this));
        if (parenthesiseArguments)
        {
          result.Append(")");
        }
      }
      result.Append(" ");
      Debug.Assert(_functionNameToOperatorDictionary.ContainsKey(e.Function.Name), "The function can not be mapped to an operator");
      result.Append(_functionNameToOperatorDictionary[e.Function.Name]);
      result.Append(" ");

      if (parenthesiseArguments)
      {
        result.Append("(");
      }
      result.Append(e.Arguments[e.Arguments.Count - 1].Accept(this));
      if (parenthesiseArguments)
      {
        result.Append(")");
      }
      return result;
    }


    /// <summary>
    /// <see cref="HandleSpecialFunctionToOperator"></see>
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleConcatFunction(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      return sqlgen.HandleSpecialFunctionToOperator(e, false);
    }

    /// <summary>
    /// <see cref="HandleSpecialFunctionToOperator"></see>
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionBitwise(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      return sqlgen.HandleSpecialFunctionToOperator(e, true);
    }

    private static ISqlFragment HandleGetDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      Debug.Assert(e.Arguments.Count == 0, "Canonical getdate function should have no arguments");

      switch (sqlgen._manifest._dateFormat)
      {
        case SQLiteDateFormats.Ticks:
          result.Append("(STRFTIME('%s', 'now') * 10000000 + 621355968000000000)");
          break;
        case SQLiteDateFormats.JulianDay:
          result.Append("CAST(STRFTIME('%J', 'now') AS double)");
          break;
        default:
          result.Append("STRFTIME('%Y-%m-%d %H:%M:%S', 'now')");
          break;
      }

      return result;
    }

    private static ISqlFragment HandleGetUtcDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      Debug.Assert(e.Arguments.Count == 0, "Canonical getutcdate function should have no arguments");

      switch (sqlgen._manifest._dateFormat)
      {
        case SQLiteDateFormats.Ticks:
          result.Append("(STRFTIME('%s', 'now', 'utc') * 10000000 + 621355968000000000)");
          break;
        case SQLiteDateFormats.JulianDay:
          result.Append("CAST(STRFTIME('%J', 'now', 'utc') AS double)");
          break;
        default:
          result.Append("STRFTIME('%Y-%m-%d %H:%M:%S', 'now', 'utc')");
          break;
      }

      return result;
    }

    /// <summary>
    /// Handles special case in which datepart 'type' parameter is present. all the functions
    /// handles here have *only* the 1st parameter as datepart. datepart value is passed along
    /// the QP as string and has to be expanded as TSQL keyword.
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleDatepartDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      Debug.Assert(e.Arguments.Count == 2, "datepart function must have 2 arguments");

      DbConstantExpression constExpr = e.Arguments[0] as DbConstantExpression;
      if (null == constExpr)
      {
        throw new InvalidOperationException(String.Format("DATEPART argument to function '{0}.{1}' must be a literal string", e.Function.NamespaceName, e.Function.Name));
      }

      string datepart = constExpr.Value as string;
      if (null == datepart)
      {
        throw new InvalidOperationException(String.Format("DATEPART argument to function '{0}.{1}' must be a literal string", e.Function.NamespaceName, e.Function.Name));
      }

      SqlBuilder result = new SqlBuilder();

      //
      // check if datepart value is valid
      //
      string trans;
      if (!_datepartKeywords.TryGetValue(datepart, out trans))
      {
        throw new InvalidOperationException(String.Format("{0}' is not a valid value for DATEPART argument in '{1}.{2}' function", datepart, e.Function.NamespaceName, e.Function.Name));
      }

      if (trans != "%f")
      {
        result.Append("CAST(STRFTIME('");

        // expand the datepart literal as tsql kword
        result.Append(trans);
        result.Append("', ");

        switch (sqlgen._manifest._dateFormat)
        {
          case SQLiteDateFormats.Ticks:
            result.Append(String.Format("(({0} - 621355968000000000) / 10000000.0)", e.Arguments[1].Accept(sqlgen)));
            break;
          default:
            result.Append(e.Arguments[1].Accept(sqlgen));
            break;
        }

        result.Append(") AS integer)");
      }
      else
      {
        result.Append("CAST(SUBSTR(STRFTIME('%f', ");

        switch (sqlgen._manifest._dateFormat)
        {
          case SQLiteDateFormats.Ticks:
            result.Append(String.Format("(({0} - 621355968000000000) / 10000000.0)", e.Arguments[1].Accept(sqlgen)));
            break;
          default:
            result.Append(e.Arguments[1].Accept(sqlgen));
            break;
        }

        result.Append("), 4) AS integer)");
      }

      return result;
    }

    /// <summary>
    /// DateAdd(datetime, secondsToAdd) -> DATEADD ( seconds , number,  date)
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionDateAdd(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      Debug.Assert(e.Arguments.Count == 2, "Canonical datepart functions should have exactly two arguments");

      switch (sqlgen._manifest._dateFormat)
      {
        case SQLiteDateFormats.Ticks:
          result.Append(String.Format("(STRFTIME('%s', JULIANDAY({1}) + ({0} / 86400.0)) * 10000000 + 621355968000000000)", e.Arguments[0].Accept(sqlgen), e.Arguments[1].Accept(sqlgen)));
          break;
        case SQLiteDateFormats.JulianDay:
          result.Append(String.Format("CAST(STRFTIME('%J', JULIANDAY({1}) + ({0} / 86400.0)) AS double)", e.Arguments[0].Accept(sqlgen), e.Arguments[1].Accept(sqlgen)));
          break;
        default:
          result.Append(String.Format("STRFTIME('%Y-%m-%d %H:%M:%S', JULIANDAY({1}) + ({0} / 86400.0))", e.Arguments[0].Accept(sqlgen), e.Arguments[1].Accept(sqlgen)));
          break;
      }

      return result;
    }

    /// <summary>
    /// DateSubtract(datetime1, datetime2) -> DATEDIFF ( seconds , startdate , enddate ) 
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionDateSubtract(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      Debug.Assert(e.Arguments.Count == 2, "Canonical datepart functions should have exactly two arguments");

      switch (sqlgen._manifest._dateFormat)
      {
        case SQLiteDateFormats.Ticks:
          result.Append(String.Format("CAST((({0} - 621355968000000000) / 10000000.0)  - (({1} - 621355968000000000) / 10000000.0) * 86400.0 AS integer)", e.Arguments[0].Accept(sqlgen), e.Arguments[1].Accept(sqlgen)));
          break;
        default:
          result.Append(String.Format("CAST((JULIANDAY({1}) - JULIANDAY({0})) * 86400.0 AS integer)", e.Arguments[0].Accept(sqlgen), e.Arguments[1].Accept(sqlgen)));
          break;
      }

      return result;
    }

    /// <summary>
    /// Handler for canonical functions for extracting date parts. 
    /// For example:
    ///     Year(date) -> DATEPART( year, date)
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionDatepart(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      string trans;
      if (!_datepartKeywords.TryGetValue(e.Function.Name, out trans))
      {
        throw new InvalidOperationException(String.Format("{0}' is not a valid value for STRFTIME argument", e.Function.Name));
      }

      SqlBuilder result = new SqlBuilder();
      result.Append("CAST(STRFTIME('");
      result.Append(trans);
      result.Append("', ");

      Debug.Assert(e.Arguments.Count == 1, "Canonical datepart functions should have exactly one argument");

      switch (sqlgen._manifest._dateFormat)
      {
        case SQLiteDateFormats.Ticks:
          result.Append(String.Format("(({0} - 621355968000000000) / 10000000.0)", e.Arguments[0].Accept(sqlgen)));
          break;
        default:
          result.Append(e.Arguments[0].Accept(sqlgen));
          break;
      }

      result.Append(") AS integer)");

      return result;
    }

#if USE_INTEROP_DLL && INTEROP_EXTENSION_FUNCTIONS
    /// <summary>
    ///  Function rename IndexOf -> CHARINDEX
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionIndexOf(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      return sqlgen.HandleFunctionDefaultGivenName(e, "CHARINDEX");
    }
#endif

    /// <summary>
    ///  Function rename NewGuid -> NEWID
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionNewGuid(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();
      result.Append("RANDOMBLOB(16)");
      return result;
    }

    /// <summary>
    ///  Length(arg) -> LEN(arg + '.') - LEN('.')
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionLength(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();

      result.Append("LENGTH(");

      Debug.Assert(e.Arguments.Count == 1, "Len should have one argument");
      result.Append(e.Arguments[0].Accept(sqlgen));

      result.Append(")");
      //result.Append(" + '.') - LEN('.')");

      return result;
    }

    /// <summary>
    /// Round(numericExpression) -> Round(numericExpression, 0);
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionRound(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();

      result.Append("ROUND(");

      Debug.Assert(e.Arguments.Count == 1, "Round should have one argument");
      result.Append(e.Arguments[0].Accept(sqlgen));

      result.Append(", 0)");

      return result;
    }

    /// <summary>
    /// TRIM(string) -> LTRIM(RTRIM(string))
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionTrim(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      SqlBuilder result = new SqlBuilder();

      result.Append("TRIM(");

      Debug.Assert(e.Arguments.Count == 1, "Trim should have one argument");
      result.Append(e.Arguments[0].Accept(sqlgen));

      result.Append(")");

      return result;
    }

    /// <summary>
    /// LEFT(string, length) -> SUBSTR(string, 1, length)
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionLeft(SqlGenerator sqlgen, DbFunctionExpression e)
    {
        SqlBuilder result = new SqlBuilder();

        result.Append("SUBSTR(");

        Debug.Assert(e.Arguments.Count == 2, "Left should have two arguments");
        result.Append(e.Arguments[0].Accept(sqlgen));
        result.Append(", 1, ");
        result.Append(e.Arguments[1].Accept(sqlgen));
        result.Append(")");

        return result;
    }

    /// <summary>
    /// RIGHT(string, length) -> SUBSTR(string, -(length), length)
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionRight(SqlGenerator sqlgen, DbFunctionExpression e)
    {
        SqlBuilder result = new SqlBuilder();

        result.Append("SUBSTR(");

        Debug.Assert(e.Arguments.Count == 2, "Right should have two arguments");
        result.Append(e.Arguments[0].Accept(sqlgen));
        result.Append(", -(");
        result.Append(e.Arguments[1].Accept(sqlgen));
        result.Append("), ");
        result.Append(e.Arguments[1].Accept(sqlgen));
        result.Append(")");

        return result;
    }

    /// <summary>
    ///  Function rename ToLower -> LOWER
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionToLower(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      return sqlgen.HandleFunctionDefaultGivenName(e, "LOWER");
    }

    /// <summary>
    ///  Function rename ToUpper -> UPPER
    /// </summary>
    /// <param name="sqlgen"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static ISqlFragment HandleCanonicalFunctionToUpper(SqlGenerator sqlgen, DbFunctionExpression e)
    {
      return sqlgen.HandleFunctionDefaultGivenName(e, "UPPER");
    }

    #endregion


    #endregion

    #region Helper methods for the DbExpressionVisitor
    /// <summary>
    /// <see cref="AddDefaultColumns"/>
    /// Add the column names from the referenced extent/join to the
    /// select statement.
    ///
    /// If the symbol is a JoinSymbol, we recursively visit all the extents,
    /// halting at real extents and JoinSymbols that have an associated SqlSelectStatement.
    ///
    /// The column names for a real extent can be derived from its type.
    /// The column names for a Join Select statement can be got from the
    /// list of columns that was created when the Join's select statement
    /// was created.
    ///
    /// We do the following for each column.
    /// <list type="number">
    /// <item>Add the SQL string for each column to the SELECT clause</item>
    /// <item>Add the column to the list of columns - so that it can
    /// become part of the "type" of a JoinSymbol</item>
    /// <item>Check if the column name collides with a previous column added
    /// to the same select statement.  Flag both the columns for renaming if true.</item>
    /// <item>Add the column to a name lookup dictionary for collision detection.</item>
    /// </list>
    /// </summary>
    /// <param name="selectStatement">The select statement that started off as SELECT *</param>
    /// <param name="symbol">The symbol containing the type information for
    /// the columns to be added.</param>
    /// <param name="columnList">Columns that have been added to the Select statement.
    /// This is created in <see cref="AddDefaultColumns"/>.</param>
    /// <param name="columnDictionary">A dictionary of the columns above.</param>
    /// <param name="separator">Comma or nothing, depending on whether the SELECT
    /// clause is empty.</param>
    void AddColumns(SqlSelectStatement selectStatement, Symbol symbol,
        List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary, ref string separator)
    {
      JoinSymbol joinSymbol = symbol as JoinSymbol;
      if (joinSymbol != null)
      {
        if (!joinSymbol.IsNestedJoin)
        {
          // Recurse if the join symbol is a collection of flattened extents
          foreach (Symbol sym in joinSymbol.ExtentList)
          {
            // if sym is ScalarType means we are at base case in the
            // recursion and there are not columns to add, just skip
            if (MetadataHelpers.IsPrimitiveType(sym.Type))
            {
              continue;
            }

            AddColumns(selectStatement, sym, columnList, columnDictionary, ref separator);
          }
        }
        else
        {
          foreach (Symbol joinColumn in joinSymbol.ColumnList)
          {
            // we write tableName.columnName
            // rather than tableName.columnName as alias
            // since the column name is unique (by the way we generate new column names)
            //
            // We use the symbols for both the table and the column,
            // since they are subject to renaming.
            selectStatement.Select.Append(separator);
            selectStatement.Select.Append(symbol);
            selectStatement.Select.Append(".");
            selectStatement.Select.Append(joinColumn);

            // check for name collisions.  If there is,
            // flag both the colliding symbols.
            if (columnDictionary.ContainsKey(joinColumn.Name))
            {
              columnDictionary[joinColumn.Name].NeedsRenaming = true; // the original symbol
              joinColumn.NeedsRenaming = true; // the current symbol.
            }
            else
            {
              columnDictionary[joinColumn.Name] = joinColumn;
            }

            columnList.Add(joinColumn);

            separator = ", ";
          }
        }
      }
      else
      {
        // This is a non-join extent/select statement, and the CQT type has
        // the relevant column information.

        // The type could be a record type(e.g. Project(...),
        // or an entity type ( e.g. EntityExpression(...)
        // so, we check whether it is a structuralType.

        // Consider an expression of the form J(a, b=P(E))
        // The inner P(E) would have been translated to a SQL statement
        // We should not use the raw names from the type, but the equivalent
        // symbols (they are present in symbol.Columns) if they exist.
        //
        // We add the new columns to the symbol's columns if they do
        // not already exist.
        //

        foreach (EdmProperty property in MetadataHelpers.GetProperties(symbol.Type))
        {
          string recordMemberName = property.Name;
          // Since all renaming happens in the second phase
          // we lose nothing by setting the next column name index to 0
          // many times.
          allColumnNames[recordMemberName] = 0;

          // Create a new symbol/reuse existing symbol for the column
          Symbol columnSymbol;
          if (!symbol.Columns.TryGetValue(recordMemberName, out columnSymbol))
          {
            // we do not care about the types of columns, so we pass null
            // when construction the symbol.
            columnSymbol = new Symbol(recordMemberName, null);
            symbol.Columns.Add(recordMemberName, columnSymbol);
          }

          selectStatement.Select.Append(separator);
          selectStatement.Select.Append(symbol);
          selectStatement.Select.Append(".");

          // We use the actual name before the "AS", the new name goes
          // after the AS.
          selectStatement.Select.Append(QuoteIdentifier(recordMemberName));

          selectStatement.Select.Append(" AS ");
          selectStatement.Select.Append(columnSymbol);

          // Check for column name collisions.
          if (columnDictionary.ContainsKey(recordMemberName))
          {
            columnDictionary[recordMemberName].NeedsRenaming = true;
            columnSymbol.NeedsRenaming = true;
          }
          else
          {
            columnDictionary[recordMemberName] = symbol.Columns[recordMemberName];
          }

          columnList.Add(columnSymbol);

          separator = ", ";
        }
      }
    }

    /// <summary>
    /// Expands Select * to "select the_list_of_columns"
    /// If the columns are taken from an extent, they are written as
    /// {original_column_name AS Symbol(original_column)} to allow renaming.
    ///
    /// If the columns are taken from a Join, they are written as just
    /// {original_column_name}, since there cannot be a name collision.
    ///
    /// We concatenate the columns from each of the inputs to the select statement.
    /// Since the inputs may be joins that are flattened, we need to recurse.
    /// The inputs are inferred from the symbols in FromExtents.
    /// </summary>
    /// <param name="selectStatement"></param>
    /// <returns></returns>
    List<Symbol> AddDefaultColumns(SqlSelectStatement selectStatement)
    {
      // This is the list of columns added in this select statement
      // This forms the "type" of the Select statement, if it has to
      // be expanded in another SELECT *
      List<Symbol> columnList = new List<Symbol>();

      // A lookup for the previous set of columns to aid column name
      // collision detection.
      Dictionary<string, Symbol> columnDictionary = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);

      string separator = "";
      // The Select should usually be empty before we are called,
      // but we do not mind if it is not.
      if (!selectStatement.Select.IsEmpty)
      {
        separator = ", ";
      }

      foreach (Symbol symbol in selectStatement.FromExtents)
      {
        AddColumns(selectStatement, symbol, columnList, columnDictionary, ref separator);
      }

      return columnList;
    }

    /// <summary>
    /// <see cref="AddFromSymbol(SqlSelectStatement, string, Symbol, bool)"/>
    /// </summary>
    /// <param name="selectStatement"></param>
    /// <param name="inputVarName"></param>
    /// <param name="fromSymbol"></param>
    void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol)
    {
      AddFromSymbol(selectStatement, inputVarName, fromSymbol, true);
    }

    /// <summary>
    /// This method is called after the input to a relational node is visited.
    /// <see cref="Visit(DbProjectExpression)"/> and <see cref="ProcessJoinInputResult"/>
    /// There are 2 scenarios
    /// <list type="number">
    /// <item>The fromSymbol is new i.e. the select statement has just been
    /// created, or a join extent has been added.</item>
    /// <item>The fromSymbol is old i.e. we are reusing a select statement.</item>
    /// </list>
    ///
    /// If we are not reusing the select statement, we have to complete the
    /// FROM clause with the alias
    /// <code>
    /// -- if the input was an extent
    /// FROM = [SchemaName].[TableName]
    /// -- if the input was a Project
    /// FROM = (SELECT ... FROM ... WHERE ...)
    /// </code>
    ///
    /// These become
    /// <code>
    /// -- if the input was an extent
    /// FROM = [SchemaName].[TableName] AS alias
    /// -- if the input was a Project
    /// FROM = (SELECT ... FROM ... WHERE ...) AS alias
    /// </code>
    /// and look like valid FROM clauses.
    ///
    /// Finally, we have to add the alias to the global list of aliases used,
    /// and also to the current symbol table.
    /// </summary>
    /// <param name="selectStatement"></param>
    /// <param name="inputVarName">The alias to be used.</param>
    /// <param name="fromSymbol"></param>
    /// <param name="addToSymbolTable"></param>
    void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol, bool addToSymbolTable)
    {
      // the first check is true if this is a new statement
      // the second check is true if we are in a join - we do not
      // check if we are in a join context.
      // We do not want to add "AS alias" if it has been done already
      // e.g. when we are reusing the Sql statement.
      if (selectStatement.FromExtents.Count == 0 || fromSymbol != selectStatement.FromExtents[0])
      {
        selectStatement.FromExtents.Add(fromSymbol);
        selectStatement.From.Append(" AS ");
        selectStatement.From.Append(fromSymbol);

        // We have this inside the if statement, since
        // we only want to add extents that are actually used.
        allExtentNames[fromSymbol.Name] = 0;
      }

      if (addToSymbolTable)
      {
        symbolTable.Add(inputVarName, fromSymbol);
      }
    }

    /// <summary>
    /// Translates a list of SortClauses.
    /// Used in the translation of OrderBy 
    /// </summary>
    /// <param name="orderByClause">The SqlBuilder to which the sort keys should be appended</param>
    /// <param name="sortKeys"></param>
    void AddSortKeys(SqlBuilder orderByClause, IList<DbSortClause> sortKeys)
    {
      string separator = "";
      foreach (DbSortClause sortClause in sortKeys)
      {
        orderByClause.Append(separator);
        orderByClause.Append(sortClause.Expression.Accept(this));
        Debug.Assert(sortClause.Collation != null);
        if (!String.IsNullOrEmpty(sortClause.Collation))
        {
          orderByClause.Append(" COLLATE ");
          orderByClause.Append(sortClause.Collation);
        }

        orderByClause.Append(sortClause.Ascending ? " ASC" : " DESC");

        separator = ", ";
      }
    }

    /// <summary>
    /// <see cref="CreateNewSelectStatement(SqlSelectStatement, string, TypeUsage, bool, out Symbol) " />
    /// </summary>
    /// <param name="oldStatement"></param>
    /// <param name="inputVarName"></param>
    /// <param name="inputVarType"></param>
    /// <param name="fromSymbol"></param>
    /// <returns>A new select statement, with the old one as the from clause.</returns>
    SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement,
        string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
    {
      return CreateNewSelectStatement(oldStatement, inputVarName, inputVarType, true, out fromSymbol);
    }


    /// <summary>
    /// This is called after a relational node's input has been visited, and the
    /// input's sql statement cannot be reused.  <see cref="Visit(DbProjectExpression)"/>
    ///
    /// When the input's sql statement cannot be reused, we create a new sql
    /// statement, with the old one as the from clause of the new statement.
    ///
    /// The old statement must be completed i.e. if it has an empty select list,
    /// the list of columns must be projected out.
    ///
    /// If the old statement being completed has a join symbol as its from extent,
    /// the new statement must have a clone of the join symbol as its extent.
    /// We cannot reuse the old symbol, but the new select statement must behave
    /// as though it is working over the "join" record.
    /// </summary>
    /// <param name="oldStatement"></param>
    /// <param name="inputVarName"></param>
    /// <param name="inputVarType"></param>
    /// <param name="finalizeOldStatement"></param>
    /// <param name="fromSymbol"></param>
    /// <returns>A new select statement, with the old one as the from clause.</returns>
    SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement,
        string inputVarName, TypeUsage inputVarType, bool finalizeOldStatement, out Symbol fromSymbol)
    {
      fromSymbol = null;

      // Finalize the old statement
      if (finalizeOldStatement && oldStatement.Select.IsEmpty)
      {
        List<Symbol> columns = AddDefaultColumns(oldStatement);

        // Thid could not have been called from a join node.
        Debug.Assert(oldStatement.FromExtents.Count == 1);

        // if the oldStatement has a join as its input, ...
        // clone the join symbol, so that we "reuse" the
        // join symbol.  Normally, we create a new symbol - see the next block
        // of code.
        JoinSymbol oldJoinSymbol = oldStatement.FromExtents[0] as JoinSymbol;
        if (oldJoinSymbol != null)
        {
          // Note: oldStatement.FromExtents will not do, since it might
          // just be an alias of joinSymbol, and we want an actual JoinSymbol.
          JoinSymbol newJoinSymbol = new JoinSymbol(inputVarName, inputVarType, oldJoinSymbol.ExtentList);
          // This indicates that the oldStatement is a blocking scope
          // i.e. it hides/renames extent columns
          newJoinSymbol.IsNestedJoin = true;
          newJoinSymbol.ColumnList = columns;
          newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

          fromSymbol = newJoinSymbol;
        }
      }

      if (fromSymbol == null)
      {
        // This is just a simple extent/SqlSelectStatement,
        // and we can get the column list from the type.
        fromSymbol = new Symbol(inputVarName, inputVarType);
      }

      // Observe that the following looks like the body of Visit(ExtentExpression).
      SqlSelectStatement selectStatement = new SqlSelectStatement();
      selectStatement.From.Append("( ");
      selectStatement.From.Append(oldStatement);
      selectStatement.From.AppendLine();
      selectStatement.From.Append(") ");


      return selectStatement;
    }


    /// <summary>
    /// Before we embed a string literal in a SQL string, we should
    /// convert all ' to '', and enclose the whole string in single quotes.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="isUnicode"></param>
    /// <returns>The escaped sql string.</returns>
    private static string EscapeSingleQuote(string s, bool isUnicode)
    {
      return "'" + s.Replace("'", "''") + "'";
    }

    /// <summary>
    /// Returns the sql primitive/native type name. 
    /// It will include size, precision or scale depending on type information present in the 
    /// type facets
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string GetSqlPrimitiveType(TypeUsage type)
    {
      PrimitiveType primitiveType = MetadataHelpers.GetEdmType<PrimitiveType>(type);

      string typeName = primitiveType.Name;
      bool isUnicode = true;
      bool isFixedLength = false;
      int maxLength = 0;
      string length = "max";
      bool preserveSeconds = true;
      byte decimalPrecision = 0;
      byte decimalScale = 0;

      switch (primitiveType.PrimitiveTypeKind)
      {
        case PrimitiveTypeKind.Binary:
          maxLength = MetadataHelpers.GetFacetValueOrDefault<int>(type, MetadataHelpers.MaxLengthFacetName, MetadataHelpers.BinaryMaxMaxLength);
          if (maxLength == MetadataHelpers.BinaryMaxMaxLength)
          {
            length = "max";
          }
          else
          {
            length = maxLength.ToString(CultureInfo.InvariantCulture);
          }
          isFixedLength = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.FixedLengthFacetName, false);
          typeName = (isFixedLength ? "binary(" : "varbinary(") + length + ")";
          break;

        case PrimitiveTypeKind.String:
          // Question: How do we handle ntext?
          isUnicode = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.UnicodeFacetName, true);
          isFixedLength = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.FixedLengthFacetName, false);
          maxLength = MetadataHelpers.GetFacetValueOrDefault<int>(type, MetadataHelpers.MaxLengthFacetName, Int32.MinValue);
          if (maxLength == Int32.MinValue)
          {
            length = "max";
          }
          else
          {
            length = maxLength.ToString(CultureInfo.InvariantCulture);
          }
          if (isUnicode && !isFixedLength && maxLength > 4000)
            length = "max";
          if (!isUnicode && !isFixedLength && maxLength > 8000)
            length = "max";
          if (isFixedLength)
          {
            typeName = (isUnicode ? "nchar(" : "char(") + length + ")";
          }
          else
          {
            typeName = (isUnicode ? "nvarchar(" : "varchar(") + length + ")";
          }
          break;

        case PrimitiveTypeKind.DateTime:
          preserveSeconds = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.PreserveSecondsFacetName, false);
          typeName = preserveSeconds ? "datetime" : "smalldatetime";
          break;

        case PrimitiveTypeKind.Decimal:
          decimalPrecision = MetadataHelpers.GetFacetValueOrDefault<byte>(type, MetadataHelpers.PrecisionFacetName, 18);
          Debug.Assert(decimalPrecision > 0, "decimal precision must be greater than zero");
          decimalScale = MetadataHelpers.GetFacetValueOrDefault<byte>(type, MetadataHelpers.ScaleFacetName, 0);
          Debug.Assert(decimalPrecision >= decimalScale, "decimalPrecision must be greater or equal to decimalScale");
          Debug.Assert(decimalPrecision <= 53, "decimalPrecision must be less than or equal to 53");
          typeName = typeName + "(" + decimalPrecision + "," + decimalScale + ")";
          break;

        case PrimitiveTypeKind.Int32:
          typeName = "int";
          break;

        case PrimitiveTypeKind.Int64:
          typeName = "bigint";
          break;

        case PrimitiveTypeKind.Int16:
          typeName = "smallint";
          break;

        case PrimitiveTypeKind.Byte:
          typeName = "tinyint";
          break;

        case PrimitiveTypeKind.Boolean:
          typeName = "bit";
          break;

        case PrimitiveTypeKind.Single:
          typeName = "real";
          break;

        case PrimitiveTypeKind.Double:
          typeName = "float";
          break;

        case PrimitiveTypeKind.Guid:
          typeName = "uniqueidentifier";
          break;

        default:
          throw new NotSupportedException("Unsupported EdmType: " + primitiveType.PrimitiveTypeKind);
      }

      return typeName;
    }

    /// <summary>
    /// Handles the expression represending DbLimitExpression.Limit and DbSkipExpression.Count.
    /// If it is a constant expression, it simply does to string thus avoiding casting it to the specific value
    /// (which would be done if <see cref="Visit(DbConstantExpression)"/> is called)
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private ISqlFragment HandleCountExpression(DbExpression e)
    {
      ISqlFragment result;

      if (e.ExpressionKind == DbExpressionKind.Constant)
      {
        //For constant expression we should not cast the value, 
        // thus we don't go throught the default DbConstantExpression handling
        SqlBuilder sqlBuilder = new SqlBuilder();
        sqlBuilder.Append(((DbConstantExpression)e).Value.ToString());
        result = sqlBuilder;
      }
      else
      {
        result = e.Accept(this);
      }

      return result;
    }

    /// <summary>
    /// This is used to determine if a particular expression is an Apply operation.
    /// This is only the case when the DbExpressionKind is CrossApply or OuterApply.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    bool IsApplyExpression(DbExpression e)
    {
      return (DbExpressionKind.CrossApply == e.ExpressionKind || DbExpressionKind.OuterApply == e.ExpressionKind);
    }

    private bool IsKeyForIn(DbExpression e)
    {
      if ((e.ExpressionKind != DbExpressionKind.Property) && (e.ExpressionKind != DbExpressionKind.VariableReference))
      {
        return (e.ExpressionKind == DbExpressionKind.ParameterReference);
      }
      return true;
    }

    /// <summary>
    /// This is used to determine if a particular expression is a Join operation.
    /// This is true for DbCrossJoinExpression and DbJoinExpression, the
    /// latter of which may have one of several different ExpressionKinds.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    bool IsJoinExpression(DbExpression e)
    {
      return (DbExpressionKind.CrossJoin == e.ExpressionKind ||
              DbExpressionKind.FullOuterJoin == e.ExpressionKind ||
              DbExpressionKind.InnerJoin == e.ExpressionKind ||
              DbExpressionKind.LeftOuterJoin == e.ExpressionKind);
    }

    /// <summary>
    /// This is used to determine if a calling expression needs to place
    /// round brackets around the translation of the expression e.
    ///
    /// Constants, parameters and properties do not require brackets,
    /// everything else does.
    /// </summary>
    /// <param name="e"></param>
    /// <returns>true, if the expression needs brackets </returns>
    bool IsComplexExpression(DbExpression e)
    {
      switch (e.ExpressionKind)
      {
        case DbExpressionKind.Constant:
        case DbExpressionKind.ParameterReference:
        case DbExpressionKind.Property:
          return false;

        default:
          return true;
      }
    }

    /// <summary>
    /// Determine if the owner expression can add its unique sql to the input's
    /// SqlSelectStatement
    /// </summary>
    /// <param name="result">The SqlSelectStatement of the input to the relational node.</param>
    /// <param name="expressionKind">The kind of the expression node(not the input's)</param>
    /// <returns></returns>
    bool IsCompatible(SqlSelectStatement result, DbExpressionKind expressionKind)
    {
      switch (expressionKind)
      {
        case DbExpressionKind.Distinct:
          return result.Top == null
            // The projection after distinct may not project all 
            // columns used in the Order By
              && result.OrderBy.IsEmpty;

        case DbExpressionKind.Filter:
          return result.Select.IsEmpty
                  && result.Where.IsEmpty
                  && result.GroupBy.IsEmpty
                  && result.Top == null;

        case DbExpressionKind.GroupBy:
          return result.Select.IsEmpty
                  && result.GroupBy.IsEmpty
                  && result.OrderBy.IsEmpty
                  && result.Top == null;

        case DbExpressionKind.Limit:
        case DbExpressionKind.Element:
          return result.Top == null;

        case DbExpressionKind.Project:
          return result.Select.IsEmpty
                  && result.GroupBy.IsEmpty;

        case DbExpressionKind.Skip:
          return result.Select.IsEmpty
                  && result.GroupBy.IsEmpty
                  && result.OrderBy.IsEmpty
                  && !result.IsDistinct;

        case DbExpressionKind.Sort:
          return result.Select.IsEmpty
                  && result.GroupBy.IsEmpty
                  && result.OrderBy.IsEmpty;

        default:
          Debug.Assert(false);
          throw new InvalidOperationException();
      }

    }

    private void ParenthesizeExpressionWithoutRedundantConstantCasts(DbExpression value, SqlBuilder sqlBuilder)
    {
      if (value.ExpressionKind == DbExpressionKind.Constant)
      {
        sqlBuilder.Append(this.Visit((DbConstantExpression)value));
      }
      else
      {
        this.ParanthesizeExpressionIfNeeded(value, sqlBuilder);
          }
      }

    private void ParanthesizeExpressionIfNeeded(DbExpression e, SqlBuilder result)
    {
      if (IsComplexExpression(e))
      {
        result.Append("(");
        result.Append(e.Accept<ISqlFragment>(this));
        result.Append(")");
      }
      else
      {
        result.Append(e.Accept<ISqlFragment>(this));
      }
    }

    /// <summary>
    /// We use the normal box quotes for SQL server.  We do not deal with ANSI quotes
    /// i.e. double quotes.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    internal static string QuoteIdentifier(string name)
    {
      Debug.Assert(!String.IsNullOrEmpty(name));
      // We assume that the names are not quoted to begin with.
      return "[" + name.Replace("]", "]]") + "]";
    }

    private bool TryAddExpressionForIn(DbBinaryExpression e, KeyToListMap<DbExpression, DbExpression> values)
    {
      if (this.IsKeyForIn(e.Left))
      {
        values.Add(e.Left, e.Right);
        return true;
      }
      if (this.IsKeyForIn(e.Right))
      {
        values.Add(e.Right, e.Left);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Simply calls <see cref="VisitExpressionEnsureSqlStatement(DbExpression, bool)"/>
    /// with addDefaultColumns set to true
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e)
    {
      return VisitExpressionEnsureSqlStatement(e, true);
    }

    /// <summary>
    /// This is called from <see cref="GenerateSql(DbQueryCommandTree)"/> and nodes which require a
    /// select statement as an argument e.g. <see cref="Visit(DbIsEmptyExpression)"/>,
    /// <see cref="Visit(DbUnionAllExpression)"/>.
    ///
    /// SqlGenerator needs its child to have a proper alias if the child is
    /// just an extent or a join.
    ///
    /// The normal relational nodes result in complete valid SQL statements.
    /// For the rest, we need to treat them as there was a dummy
    /// <code>
    /// -- originally {expression}
    /// -- change that to
    /// SELECT *
    /// FROM {expression} as c
    /// </code>
    /// 
    /// DbLimitExpression needs to start the statement but not add the default columns
    /// </summary>
    /// <param name="e"></param>
    /// <param name="addDefaultColumns"></param>
    /// <returns></returns>
    SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e, bool addDefaultColumns)
    {
      Debug.Assert(MetadataHelpers.IsCollectionType(e.ResultType));

      SqlSelectStatement result;
      switch (e.ExpressionKind)
      {
        case DbExpressionKind.Project:
        case DbExpressionKind.Filter:
        case DbExpressionKind.GroupBy:
        case DbExpressionKind.Sort:
          result = e.Accept(this) as SqlSelectStatement;
          break;

        default:
          Symbol fromSymbol;
          string inputVarName = "c";  // any name will do - this is my random choice.
          symbolTable.EnterScope();

          TypeUsage type = null;
          switch (e.ExpressionKind)
          {
            case DbExpressionKind.Scan:
            case DbExpressionKind.CrossJoin:
            case DbExpressionKind.FullOuterJoin:
            case DbExpressionKind.InnerJoin:
            case DbExpressionKind.LeftOuterJoin:
            case DbExpressionKind.CrossApply:
            case DbExpressionKind.OuterApply:
              type = MetadataHelpers.GetElementTypeUsage(e.ResultType);
              break;

            default:
              Debug.Assert(MetadataHelpers.IsCollectionType(e.ResultType));
              type = MetadataHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage;
              break;
          }

          result = VisitInputExpression(e, inputVarName, type, out fromSymbol);
          AddFromSymbol(result, inputVarName, fromSymbol);
          symbolTable.ExitScope();
          break;
      }

      if (addDefaultColumns && result.Select.IsEmpty)
      {
        AddDefaultColumns(result);
      }

      return result;
    }

    /// <summary>
    /// This method is called by <see cref="Visit(DbFilterExpression)"/> and
    /// <see cref="Visit(DbQuantifierExpression)"/>
    ///
    /// </summary>
    /// <param name="input"></param>
    /// <param name="predicate"></param>
    /// <param name="negatePredicate">This is passed from <see cref="Visit(DbQuantifierExpression)"/>
    /// in the All(...) case.</param>
    /// <returns></returns>
    SqlSelectStatement VisitFilterExpression(DbExpressionBinding input, DbExpression predicate, bool negatePredicate)
    {
      Symbol fromSymbol;
      SqlSelectStatement result = VisitInputExpression(input.Expression,
          input.VariableName, input.VariableType, out fromSymbol);

      // Filter is compatible with OrderBy
      // but not with Project, another Filter or GroupBy
      if (!IsCompatible(result, DbExpressionKind.Filter))
      {
        result = CreateNewSelectStatement(result, input.VariableName, input.VariableType, out fromSymbol);
      }

      selectStatementStack.Push(result);
      symbolTable.EnterScope();

      AddFromSymbol(result, input.VariableName, fromSymbol);

      if (negatePredicate)
      {
        result.Where.Append("NOT (");
      }
      result.Where.Append(predicate.Accept(this));
      if (negatePredicate)
      {
        result.Where.Append(")");
      }

      symbolTable.ExitScope();
      selectStatementStack.Pop();

      return result;
    }

    /// <summary>
    /// If the sql fragment for an input expression is not a SqlSelect statement
    /// or other acceptable form (e.g. an extent as a SqlBuilder), we need
    /// to wrap it in a form acceptable in a FROM clause.  These are
    /// primarily the
    /// <list type="bullet">
    /// <item>The set operation expressions - union all, intersect, except</item>
    /// <item>TVFs, which are conceptually similar to tables</item>
    /// </list>
    /// </summary>
    /// <param name="result"></param>
    /// <param name="sqlFragment"></param>
    /// <param name="expressionKind"></param>
    void WrapNonQueryExtent(SqlSelectStatement result, ISqlFragment sqlFragment, DbExpressionKind expressionKind)
    {
      switch (expressionKind)
      {
        case DbExpressionKind.Function:
          // TVF
          result.From.Append(sqlFragment);
          break;

        default:
          result.From.Append(" (");
          result.From.Append(sqlFragment);
          result.From.Append(")");
          break;
      }
    }

    /// <summary>
    /// Is this a builtin function (ie) does it have the builtinAttribute specified?
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    private static bool IsBuiltinFunction(EdmFunction function)
    {
      return MetadataHelpers.TryGetValueForMetadataProperty<bool>(function, "BuiltInAttribute");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="function"></param>
    /// <param name="result"></param>
    void WriteFunctionName(SqlBuilder result, EdmFunction function)
    {
      string storeFunctionName = MetadataHelpers.TryGetValueForMetadataProperty<string>(function, "StoreFunctionNameAttribute");

      if (string.IsNullOrEmpty(storeFunctionName))
      {
        storeFunctionName = function.Name;
      }

      // If the function is a builtin (ie) the BuiltIn attribute has been
      // specified, then, the function name should not be quoted; additionally,
      // no namespace should be used.
      if (IsBuiltinFunction(function))
      {
        if (function.NamespaceName == "Edm")
        {
          switch (storeFunctionName.ToUpperInvariant())
          {
            default:
              result.Append(storeFunctionName);
              break;
          }

        }
        else
        {
          result.Append(storeFunctionName);
        }

      }
      else
      {
        // Should we actually support this?
        //result.Append(QuoteIdentifier((string)function.MetadataProperties["Schema"].Value ?? "dbo"));
        //result.Append(".");
        result.Append(QuoteIdentifier(storeFunctionName));
      }
    }

    static string ByteArrayToBinaryString(Byte[] binaryArray)
    {
      StringBuilder sb = new StringBuilder(binaryArray.Length * 2);
      for (int i = 0; i < binaryArray.Length; i++)
      {
        sb.Append(hexDigits[(binaryArray[i] & 0xF0) >> 4]).Append(hexDigits[binaryArray[i] & 0x0F]);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Helper method for the Group By visitor
    /// Returns true if at least one of the aggregates in the given list
    /// has an argument that is not a <see cref="DbPropertyExpression"/> 
    /// over <see cref="DbVariableReferenceExpression"/>
    /// </summary>
    /// <param name="aggregates"></param>
    /// <returns></returns>
    static bool NeedsInnerQuery(IList<DbAggregate> aggregates)
    {
      foreach (DbAggregate aggregate in aggregates)
      {
        Debug.Assert(aggregate.Arguments.Count == 1);
        if (!IsPropertyOverVarRef(aggregate.Arguments[0]))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Determines whether the given expression is a <see cref="DbPropertyExpression"/> 
    /// over <see cref="DbVariableReferenceExpression"/>
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    static bool IsPropertyOverVarRef(DbExpression expression)
    {
      DbPropertyExpression propertyExpression = expression as DbPropertyExpression;
      if (propertyExpression == null)
      {
        return false;
      }
      DbVariableReferenceExpression varRefExpression = propertyExpression.Instance as DbVariableReferenceExpression;
      if (varRefExpression == null)
      {
        return false;
      }
      return true;
    }

    #endregion
    
    private class KeyFieldExpressionComparer : IEqualityComparer<DbExpression>
    {
      // Fields
      internal static readonly SqlGenerator.KeyFieldExpressionComparer Singleton = new SqlGenerator.KeyFieldExpressionComparer();

      // Methods
      private KeyFieldExpressionComparer()
      {
      }

      public bool Equals(DbExpression x, DbExpression y)
      {
        if (x.ExpressionKind == y.ExpressionKind)
        {
          DbExpressionKind expressionKind = x.ExpressionKind;
          if (expressionKind <= DbExpressionKind.ParameterReference)
          {
            switch (expressionKind)
            {
              case DbExpressionKind.Cast:
                {
                  DbCastExpression expression5 = (DbCastExpression)x;
                  DbCastExpression expression6 = (DbCastExpression)y;
                  return ((expression5.ResultType == expression6.ResultType) && this.Equals(expression5.Argument, expression6.Argument));
                }
              case DbExpressionKind.ParameterReference:
                {
                  DbParameterReferenceExpression expression3 = (DbParameterReferenceExpression)x;
                  DbParameterReferenceExpression expression4 = (DbParameterReferenceExpression)y;
                  return (expression3.ParameterName == expression4.ParameterName);
                }
            }
            goto Label_00CC;
          }
          if (expressionKind != DbExpressionKind.Property)
          {
            if (expressionKind == DbExpressionKind.VariableReference)
            {
              return (x == y);
            }
            goto Label_00CC;
          }
          DbPropertyExpression expression = (DbPropertyExpression)x;
          DbPropertyExpression expression2 = (DbPropertyExpression)y;
          if (expression.Property == expression2.Property)
          {
            return this.Equals(expression.Instance, expression2.Instance);
          }
        }
        return false;
      Label_00CC:
        return false;
      }

      public int GetHashCode(DbExpression obj)
      {
        switch (obj.ExpressionKind)
        {
          case DbExpressionKind.Cast:
            return this.GetHashCode(((DbCastExpression)obj).Argument);

          case DbExpressionKind.ParameterReference:
            return (((DbParameterReferenceExpression)obj).ParameterName.GetHashCode() ^ 0x7fffffff);

          case DbExpressionKind.Property:
            return ((DbPropertyExpression)obj).Property.GetHashCode();

          case DbExpressionKind.VariableReference:
            return ((DbVariableReferenceExpression)obj).VariableName.GetHashCode();
        }
        return obj.GetHashCode();
      }
    }
  }
}

