//---------------------------------------------------------------------
// <copyright file="ISqlFragment.cs" company="Microsoft">
//      Portions of this file copyright (c) Microsoft Corporation
//      and are released under the Microsoft Pulic License.  See
//      http://archive.msdn.microsoft.com/EFSampleProvider/Project/License.aspx
//      or License.txt for details.
//      All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace System.Data.SQLite
{
  using System.Data.Entity.Core.Common.CommandTrees;

  /// <summary>
  /// Represents the sql fragment for any node in the query tree.
  /// </summary>
  /// <remarks>
  /// The nodes in a query tree produce various kinds of sql
  /// <list type="bullet">
  /// <item>A select statement.</item>
  /// <item>A reference to an extent. (symbol)</item>
  /// <item>A raw string.</item>
  /// </list>
  /// We have this interface to allow for a common return type for the methods
  /// in the expression visitor <see cref="DbExpressionVisitor{T}"/>
  /// 
  /// At the end of translation, the sql fragments are converted into real strings.
  /// </remarks>
  internal interface ISqlFragment
  {
    /// <summary>
    /// Write the string represented by this fragment into the stream.
    /// </summary>
    /// <param name="writer">The stream that collects the strings.</param>
    /// <param name="sqlGenerator">Context information used for renaming.
    /// The global lists are used to generated new names without collisions.</param>
    void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
  }
}
