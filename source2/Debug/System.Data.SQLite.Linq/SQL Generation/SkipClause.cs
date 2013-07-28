//---------------------------------------------------------------------
// <copyright file="SkipClause.cs" company="Microsoft">
//      Portions of this file copyright (c) Microsoft Corporation
//      and are released under the Microsoft Pulic License.  See
//      http://archive.msdn.microsoft.com/EFSampleProvider/Project/License.aspx
//      or License.txt for details.
//      All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace System.Data.SQLite
{
    using System.Globalization;

    /// <summary>
    /// SkipClause represents the a SKIP expression in a SqlSelectStatement.
    /// It has a count property, which indicates how many rows should be skipped.
    /// </summary>
    class SkipClause : ISqlFragment
    {
        ISqlFragment skipCount;

        /// <summary>
        /// How many rows should be skipped.
        /// </summary>
        internal ISqlFragment SkipCount
        {
            get { return skipCount; }
        }

        /// <summary>
        /// Creates a SkipClause with the given skipCount.
        /// </summary>
        /// <param name="skipCount"></param>
        internal SkipClause(ISqlFragment skipCount)
        {
            this.skipCount = skipCount;
        }

        /// <summary>
        /// Creates a SkipClause with the given skipCount.
        /// </summary>
        /// <param name="skipCount"></param>
        internal SkipClause(int skipCount)
        {
            SqlBuilder sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(skipCount.ToString(CultureInfo.InvariantCulture));
            this.skipCount = sqlBuilder;
        }

        #region ISqlFragment Members
        /// <summary>
        /// Write out the SKIP part of sql select statement 
        /// It basically writes OFFSET (X).
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write(" OFFSET ");
            this.SkipCount.WriteSql(writer, sqlGenerator);
        }
        #endregion
    }
}
