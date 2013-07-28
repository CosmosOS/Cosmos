/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity.Core.Common.CommandTrees;

    internal sealed class SqlChecker : DbExpressionVisitor<bool>
  {
#if false
    private static Type sql8rewriter;

    static SqlChecker()
    {
        string version =
#if NET_40 || NET_45
            "4.0.0.0";
#else
            "3.5.0.0";
#endif

        sql8rewriter = Type.GetType(String.Format("System.Data.SqlClient.SqlGen.Sql8ExpressionRewriter, System.Data.Entity, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089", version), false);
    }
#endif

    private SqlChecker()
    {
    }

#if false
    /// <summary>
    /// SQLite doesn't support things like SKIP and a few other things.  
    /// So determine if the query has to be rewritten
    /// </summary>
    /// <remarks>
    /// Microsoft went to all the trouble of making things like SKIP work 
    /// on Sql Server 2000 by doing a rewrite of the commandtree.
    /// However, all that fancy stuff is hidden from us.  Thanks to 
    /// reflection however, we can go ahead and use the Sql 2000 rewriter code
    /// they made.
    /// </remarks>
    /// <param name="tree">The tree to inspect for a rewrite</param>
    /// <returns>Returns a new query tree if it needs rewriting</returns>
    internal static DbQueryCommandTree Rewrite(DbQueryCommandTree tree)
    {
      SqlChecker visitor = new SqlChecker();
      if (tree.Query.Accept<bool>(visitor))
      {
        tree = sql8rewriter.InvokeMember("Rewrite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static, null, null, new object[] { tree }) as DbQueryCommandTree;
      }      
      return tree;
    }
#endif

    public override bool Visit(DbAndExpression expression)
    {
      return VisitBinaryExpression(expression);
    }

    public override bool Visit(DbApplyExpression expression)
    {
      throw new NotSupportedException("apply expression");
    }

    public override bool Visit(DbArithmeticExpression expression)
    {
      return VisitExpressionList(expression.Arguments);
    }

    public override bool Visit(DbCaseExpression expression)
    {
      bool flag1 = VisitExpressionList(expression.When);
      bool flag2 = VisitExpressionList(expression.Then);
      bool flag3 = VisitExpression(expression.Else);

      return (flag1 || flag2 || flag3);
    }

    public override bool Visit(DbCastExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbComparisonExpression expression)
    {
      return VisitBinaryExpression(expression);
    }

    public override bool Visit(DbConstantExpression expression)
    {
      return false;
    }

    public override bool Visit(DbCrossJoinExpression expression)
    {
      return VisitExpressionBindingList(expression.Inputs);
    }

    public override bool Visit(DbDerefExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbDistinctExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbElementExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbEntityRefExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbExceptExpression expression)
    {
      bool flag1 = VisitExpression(expression.Left);
      bool flag2 = VisitExpression(expression.Right);
      return (flag1 || flag2);
    }

    public override bool Visit(DbExpression expression)
    {
      throw new NotSupportedException(expression.GetType().FullName);
    }

    public override bool Visit(DbFilterExpression expression)
    {
      bool flag1 = VisitExpressionBinding(expression.Input);
      bool flag2 = VisitExpression(expression.Predicate);

      return (flag1 || flag2);
    }

    public override bool Visit(DbFunctionExpression expression)
    {
      return VisitExpressionList(expression.Arguments);
    }

    public override bool Visit(DbGroupByExpression expression)
    {
      bool flag1 = VisitExpression(expression.Input.Expression);
      bool flag2 = VisitExpressionList(expression.Keys);
      bool flag3 = VisitAggregateList(expression.Aggregates);

      return (flag1 || flag2 || flag3);
    }

    public override bool Visit(DbIntersectExpression expression)
    {
      bool flag1 = VisitExpression(expression.Left);
      bool flag2 = VisitExpression(expression.Right);
      return (flag1 || flag2);
    }

    public override bool Visit(DbIsEmptyExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbIsNullExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbIsOfExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbJoinExpression expression)
    {
      bool flag1 = VisitExpressionBinding(expression.Left);
      bool flag2 = VisitExpressionBinding(expression.Right);
      bool flag3 = VisitExpression(expression.JoinCondition);
      return (flag1 || flag2 || flag3);
    }

    public override bool Visit(DbLikeExpression expression)
    {
      bool flag1 = VisitExpression(expression.Argument);
      bool flag2 = VisitExpression(expression.Pattern);
      bool flag3 = VisitExpression(expression.Escape);
      return (flag1 || flag2 || flag3);
    }

    public override bool Visit(DbLimitExpression expression)
    {
      return VisitExpression(expression.Argument);
    }

    public override bool Visit(DbNewInstanceExpression expression)
    {
      return VisitExpressionList(expression.Arguments);
    }

    public override bool Visit(DbNotExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbNullExpression expression)
    {
      return false;
    }

    public override bool Visit(DbOfTypeExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbOrExpression expression)
    {
      return VisitBinaryExpression(expression);
    }

    public override bool Visit(DbParameterReferenceExpression expression)
    {
      return false;
    }

    public override bool Visit(DbProjectExpression expression)
    {
      bool flag1 = VisitExpressionBinding(expression.Input);
      bool flag2 = VisitExpression(expression.Projection);
      return (flag1 || flag2);
    }

    public override bool Visit(DbPropertyExpression expression)
    {
      return VisitExpression(expression.Instance);
    }

    public override bool Visit(DbQuantifierExpression expression)
    {
      bool flag1 = VisitExpressionBinding(expression.Input);
      bool flag2 = VisitExpression(expression.Predicate);
      return (flag1 || flag2);
    }

    public override bool Visit(DbRefExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbRefKeyExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbRelationshipNavigationExpression expression)
    {
      return VisitExpression(expression.NavigationSource);
    }

    public override bool Visit(DbScanExpression expression)
    {
      return false;
    }

    public override bool Visit(DbSkipExpression expression)
    {
      VisitExpressionBinding(expression.Input);
      VisitSortClauseList(expression.SortOrder);
      VisitExpression(expression.Count);
      return true;
    }

    public override bool Visit(DbSortExpression expression)
    {
      bool flag1 = VisitExpressionBinding(expression.Input);
      bool flag2 = VisitSortClauseList(expression.SortOrder);
      return (flag1 || flag2);
    }

    public override bool Visit(DbTreatExpression expression)
    {
      return VisitUnaryExpression(expression);
    }

    public override bool Visit(DbUnionAllExpression expression)
    {
      return VisitBinaryExpression(expression);
    }

    public override bool Visit(DbVariableReferenceExpression expression)
    {
      return false;
    }

    private bool VisitAggregate(DbAggregate aggregate)
    {
      return VisitExpressionList(aggregate.Arguments);
    }

    private bool VisitAggregateList(IList<DbAggregate> list)
    {
      return VisitList<DbAggregate>(new ListElementHandler<DbAggregate>(VisitAggregate), list);
    }

    private bool VisitBinaryExpression(DbBinaryExpression expr)
    {
      bool flag1 = VisitExpression(expr.Left);
      bool flag2 = VisitExpression(expr.Right);
      return (flag1 || flag2);
    }

    private bool VisitExpression(DbExpression expression)
    {
      if (expression == null)
      {
        return false;
      }
      return expression.Accept<bool>(this);
    }

    private bool VisitExpressionBinding(DbExpressionBinding expressionBinding)
    {
      return VisitExpression(expressionBinding.Expression);
    }

    private bool VisitExpressionBindingList(IList<DbExpressionBinding> list)
    {
      return VisitList<DbExpressionBinding>(new ListElementHandler<DbExpressionBinding>(VisitExpressionBinding), list);
    }

    private bool VisitExpressionList(IList<DbExpression> list)
    {
      return VisitList<DbExpression>(new ListElementHandler<DbExpression>(VisitExpression), list);
    }

    private static bool VisitList<TElementType>(ListElementHandler<TElementType> handler, IList<TElementType> list)
    {
      bool flag = false;
      foreach (TElementType local in list)
      {
        bool flag2 = handler(local);
        flag = flag || flag2;
      }
      return flag;
    }

    private bool VisitSortClause(DbSortClause sortClause)
    {
      return VisitExpression(sortClause.Expression);
    }

    private bool VisitSortClauseList(IList<DbSortClause> list)
    {
      return VisitList<DbSortClause>(new ListElementHandler<DbSortClause>(VisitSortClause), list);
    }

    private bool VisitUnaryExpression(DbUnaryExpression expr)
    {
      return VisitExpression(expr.Argument);
    }

    private delegate bool ListElementHandler<TElementType>(TElementType element);
  }
}
