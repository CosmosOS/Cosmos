using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System.Data.Common;

namespace DapperExtensions
{
    public interface IDapperImplementor
    {
        ISqlGenerator SqlGenerator { get; }
        T Get<T>(DbConnection connection, dynamic id, DbTransaction transaction, int? commandTimeout) where T : class;
        void Insert<T>(DbConnection connection, IEnumerable<T> entities, DbTransaction transaction, int? commandTimeout) where T : class;
        dynamic Insert<T>(DbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class;
        bool Update<T>(DbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class;
        bool Delete<T>(DbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class;
        bool Delete<T>(DbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class;
        IEnumerable<T> GetList<T>(DbConnection connection, object predicate, IList<ISort> sort, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class;        
        IEnumerable<T> GetPage<T>(DbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
        IEnumerable<T> GetSet<T>(DbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
        int Count<T>(DbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class;
        IMultipleResultReader GetMultiple(DbConnection connection, GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout);
    }

    public class DapperImplementor : IDapperImplementor
    {
        public DapperImplementor(ISqlGenerator sqlGenerator)
        {
            SqlGenerator = sqlGenerator;
        }

        public ISqlGenerator SqlGenerator { get; private set; }

        public T Get<T>(DbConnection connection, dynamic id, DbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetIdPredicate(classMap, id);
            T result = GetList<T>(connection, classMap, predicate, null, transaction, commandTimeout, true).SingleOrDefault();
            return result;
        }

        public void Insert<T>(DbConnection connection, IEnumerable<T> entities, DbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            var properties = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);

            foreach (var e in entities)
            {
                foreach (var column in properties)
                {
                    if (column.KeyType == KeyType.Guid)
                    {
                        Guid comb = SqlGenerator.Configuration.GetNextGuid();
                        column.PropertyInfo.SetValue(e, comb, null);
                    }
                }
            }

            string sql = SqlGenerator.Insert(classMap);

            connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
        }

        public dynamic Insert<T>(DbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            List<IPropertyMap> nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            foreach (var column in nonIdentityKeyProperties)
            {
                if (column.KeyType == KeyType.Guid)
                {
                    Guid comb = SqlGenerator.Configuration.GetNextGuid();
                    column.PropertyInfo.SetValue(entity, comb, null);
                }
            }

            IDictionary<string, object> keyValues = new ExpandoObject();
            string sql = SqlGenerator.Insert(classMap);
            if (identityColumn != null)
            {
                IEnumerable<long> result;
                if (SqlGenerator.SupportsMultipleStatements())
                {
                    sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
                    result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
                }
                else
                {
                    connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
                    sql = SqlGenerator.IdentitySql(classMap);
                    result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
                }

                long identityValue = result.First();
                int identityInt = Convert.ToInt32(identityValue);
                keyValues.Add(identityColumn.Name, identityInt);
                identityColumn.PropertyInfo.SetValue(entity, identityInt, null);
            }
            else
            {
                connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
            }

            foreach (var column in nonIdentityKeyProperties)
            {
                keyValues.Add(column.Name, column.PropertyInfo.GetValue(entity, null));
            }

            if (keyValues.Count == 1)
            {
                return keyValues.First().Value;
            }

            return keyValues;
        }

        public bool Update<T>(DbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetKeyPredicate<T>(classMap, entity);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Update(classMap, predicate, parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();

            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));
            foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name == property.Key)))
            {
                dynamicParameters.Add(property.Key, property.Value);
            }

            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public bool Delete<T>(DbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetKeyPredicate<T>(classMap, entity);
            return Delete<T>(connection, classMap, predicate, transaction, commandTimeout);
        }

        public bool Delete<T>(DbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            return Delete<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
        }

        public IEnumerable<T> GetList<T>(DbConnection connection, object predicate, IList<ISort> sort, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            return GetList<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, true);
        }

        public IEnumerable<T> GetPage<T>(DbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            return GetPage<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        public IEnumerable<T> GetSet<T>(DbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            return GetSet<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
        }

        public int Count<T>(DbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return (int)connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single().Total;
        }

        public IMultipleResultReader GetMultiple(DbConnection connection, GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout)
        {
            if (SqlGenerator.SupportsMultipleStatements())
            {
                return GetMultipleByBatch(connection, predicate, transaction, commandTimeout);
            }

            return GetMultipleBySequence(connection, predicate, transaction, commandTimeout);
        }

        protected IEnumerable<T> GetList<T>(DbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Select(classMap, predicate, sort, parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        protected IEnumerable<T> GetPage<T>(DbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        protected IEnumerable<T> GetSet<T>(DbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        protected bool Delete<T>(DbConnection connection, IClassMapper classMap, IPredicate predicate, DbTransaction transaction, int? commandTimeout) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Delete(classMap, predicate, parameters);
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        protected IPredicate GetPredicate(IClassMapper classMap, object predicate)
        {
            IPredicate wherePredicate = predicate as IPredicate;
            if (wherePredicate == null && predicate != null)
            {
                wherePredicate = GetEntityPredicate(classMap, predicate);
            }

            return wherePredicate;
        }

        protected IPredicate GetIdPredicate(IClassMapper classMap, object id)
        {
            bool isSimpleType = ReflectionHelper.IsSimpleType(id.GetType());
            var keys = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            IDictionary<string, object> paramValues = null;
            IList<IPredicate> predicates = new List<IPredicate>();
            if (!isSimpleType)
            {
                paramValues = ReflectionHelper.GetObjectValues(id);
            }

            foreach (var key in keys)
            {
                object value = id;
                if (!isSimpleType)
                {
                    value = paramValues[key.Name];
                }

                Type predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

                IFieldPredicate fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = key.Name;
                fieldPredicate.Value = value;
                predicates.Add(fieldPredicate);
            }

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                             {
                                 Operator = GroupOperator.And,
                                 Predicates = predicates
                             };
        }

        protected IPredicate GetKeyPredicate<T>(IClassMapper classMap, T entity) where T : class
        {
            var whereFields = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            if (!whereFields.Any())
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            IList<IPredicate> predicates = (from field in whereFields
                                            select new FieldPredicate<T>
                                                       {
                                                           Not = false,
                                                           Operator = Operator.Eq,
                                                           PropertyName = field.Name,
                                                           Value = field.PropertyInfo.GetValue(entity, null)
                                                       }).Cast<IPredicate>().ToList();

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                             {
                                 Operator = GroupOperator.And,
                                 Predicates = predicates
                             };
        }

        protected IPredicate GetEntityPredicate(IClassMapper classMap, object entity)
        {
            Type predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
            IList<IPredicate> predicates = new List<IPredicate>();
            foreach (var kvp in ReflectionHelper.GetObjectValues(entity))
            {
                IFieldPredicate fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = kvp.Key;
                fieldPredicate.Value = kvp.Value;
                predicates.Add(fieldPredicate);
            }

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                       {
                           Operator = GroupOperator.And,
                           Predicates = predicates
                       };
        }

        protected GridReaderResultReader GetMultipleByBatch(DbConnection connection, GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            StringBuilder sql = new StringBuilder();
            foreach (var item in predicate.Items)
            {
                IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
                IPredicate itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                sql.AppendLine(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters) + SqlGenerator.Configuration.Dialect.BatchSeperator);
            }

            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            SqlMapper.GridReader grid = connection.QueryMultiple(sql.ToString(), dynamicParameters, transaction, commandTimeout, CommandType.Text);
            return new GridReaderResultReader(grid);
        }

        protected SequenceReaderResultReader GetMultipleBySequence(DbConnection connection, GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout)
        {
            IList<SqlMapper.GridReader> items = new List<SqlMapper.GridReader>();
            foreach (var item in predicate.Items)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
                IPredicate itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                string sql = SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                SqlMapper.GridReader queryResult = connection.QueryMultiple(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                items.Add(queryResult);
            }

            return new SequenceReaderResultReader(items);
        }
    }
}
