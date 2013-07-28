//---------------------------------------------------------------------
// <copyright file="Symbol.cs" company="Microsoft">
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
  using System.Collections.Generic;
  using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
  /// <see cref="SymbolTable"/>
  /// This class represents an extent/nested select statement,
  /// or a column.
  ///
  /// The important fields are Name, Type and NewName.
  /// NewName starts off the same as Name, and is then modified as necessary.
  ///
  ///
  /// The rest are used by special symbols.
  /// e.g. NeedsRenaming is used by columns to indicate that a new name must
  /// be picked for the column in the second phase of translation.
  ///
  /// IsUnnest is used by symbols for a collection expression used as a from clause.
  /// This allows <see cref="SqlGenerator.AddFromSymbol(SqlSelectStatement, string, Symbol, bool)"/> to add the column list
  /// after the alias.
  ///
  /// </summary>
  class Symbol : ISqlFragment
  {
    private Dictionary<string, Symbol> columns = new Dictionary<string, Symbol>(StringComparer.CurrentCultureIgnoreCase);
    internal Dictionary<string, Symbol> Columns
    {
      get { return columns; }
    }

    private bool needsRenaming = false;
    internal bool NeedsRenaming
    {
      get { return needsRenaming; }
      set { needsRenaming = value; }
    }

    bool isUnnest = false;
    internal bool IsUnnest
    {
      get { return isUnnest; }
      set { isUnnest = value; }
    }

    string name;
    public string Name
    {
      get { return name; }
    }

    string newName;
    public string NewName
    {
      get { return newName; }
      set { newName = value; }
    }

    private TypeUsage type;
    internal TypeUsage Type
    {
      get { return type; }
      set { type = value; }
    }

    public Symbol(string name, TypeUsage type)
    {
      this.name = name;
      this.newName = name;
      this.Type = type;
    }

    #region ISqlFragment Members

    /// <summary>
    /// Write this symbol out as a string for sql.  This is just
    /// the new name of the symbol (which could be the same as the old name).
    ///
    /// We rename columns here if necessary.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="sqlGenerator"></param>
    public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
    {
      if (this.NeedsRenaming)
      {
        string newName;
        int i = sqlGenerator.AllColumnNames[this.NewName];
        do
        {
          ++i;
          newName = this.Name + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
        } while (sqlGenerator.AllColumnNames.ContainsKey(newName));
        sqlGenerator.AllColumnNames[this.NewName] = i;

        // Prevent it from being renamed repeatedly.
        this.NeedsRenaming = false;
        this.NewName = newName;

        // Add this column name to list of known names so that there are no subsequent
        // collisions
        sqlGenerator.AllColumnNames[newName] = 0;
      }
      writer.Write(SqlGenerator.QuoteIdentifier(this.NewName));
    }

    #endregion
  }
}
