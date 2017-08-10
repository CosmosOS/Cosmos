using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System.Data.Common;

namespace DapperExtensions
{
    public interface IDatabase : IDisposable
    {
        bool HasActiveTransaction { get; }
        DbConnection Connection { get; }
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void Commit();
        void Rollback();
        void RunInTransaction(Action action);
        T RunInTransaction<T>(Func<T> func);
        T Get<T>(dynamic id, DbTransaction transaction, int? commandTimeout = null) where T : class;
        T Get<T>(dynamic id, int? commandTimeout = null) where T : class;
        void Insert<T>(IEnumerable<T> entities, DbTransaction transaction, int? commandTimeout = null) where T : class;
        void Insert<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class;
        dynamic Insert<T>(T entity, DbTransaction transaction, int? commandTimeout = null) where T : class;
        dynamic Insert<T>(T entity, int? commandTimeout = null) where T : class;
        bool Update<T>(T entity, DbTransaction transaction, int? commandTimeout = null) where T : class;
        bool Update<T>(T entity, int? commandTimeout = null) where T : class;
        bool Delete<T>(T entity, DbTransaction transaction, int? commandTimeout = null) where T : class;
        bool Delete<T>(T entity, int? commandTimeout = null) where T : class;
        bool Delete<T>(object predicate, DbTransaction transaction, int? commandTimeout = null) where T : class;
        bool Delete<T>(object predicate, int? commandTimeout = null) where T : class;
        IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, DbTransaction transaction, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetList<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, DbTransaction transaction, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
        IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered) where T : class;        
        int Count<T>(object predicate, DbTransaction transaction, int? commandTimeout = null) where T : class;
        int Count<T>(object predicate, int? commandTimeout = null) where T : class;
        IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout = null);
        IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, int? commandTimeout = null);
        void ClearCache();
        Guid GetNextGuid();
        IClassMapper GetMap<T>() where T : class;
    }

    public class Database : IDatabase
    {
        private readonly IDapperImplementor _dapper;

        private DbTransaction _transaction;

        public Database(DbConnection connection, ISqlGenerator sqlGenerator)
        {
            _dapper = new DapperImplementor(sqlGenerator);
            Connection = connection;
            
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        public bool HasActiveTransaction
        {
            get
            {
                return _transaction != null;
            }
        }

        public DbConnection Connection { get; private set; }

        public void Dispose()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                if (_transaction != null)
                {
                    _transaction.Rollback();
                }

                Connection.Close();
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transaction = Connection.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            _transaction.Commit();
            _transaction = null;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction = null;
        }

        public void RunInTransaction(Action action)
        {
            BeginTransaction();
            try
            {
                action();
                Commit();
            }
            catch (Exception ex)
            {
                if (HasActiveTransaction)
                {
                    Rollback();
                }

                throw ex;
            }
        }

        public T RunInTransaction<T>(Func<T> func)
        {
            BeginTransaction();
            try
            {
                T result = func();
                Commit();
                return result;
            }
            catch (Exception ex)
            {
                if (HasActiveTransaction)
                {
                    Rollback();
                }

                throw ex;
            }
        }
        
        public T Get<T>(dynamic id, DbTransaction transaction, int? commandTimeout) where T : class
        {
            return (T)_dapper.Get<T>(Connection, id, transaction, commandTimeout);
        }

        public T Get<T>(dynamic id, int? commandTimeout) where T : class
        {
            return (T)_dapper.Get<T>(Connection, id, _transaction, commandTimeout);
        }

        public void Insert<T>(IEnumerable<T> entities, DbTransaction transaction, int? commandTimeout) where T : class
        {
            _dapper.Insert<T>(Connection, entities, transaction, commandTimeout);
        }

        public void Insert<T>(IEnumerable<T> entities, int? commandTimeout) where T : class
        {
            _dapper.Insert<T>(Connection, entities, _transaction, commandTimeout);
        }

        public dynamic Insert<T>(T entity, DbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Insert<T>(Connection, entity, transaction, commandTimeout);
        }

        public dynamic Insert<T>(T entity, int? commandTimeout) where T : class
        {
            return _dapper.Insert<T>(Connection, entity, _transaction, commandTimeout);
        }

        public bool Update<T>(T entity, DbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Update<T>(Connection, entity, transaction, commandTimeout);
        }

        public bool Update<T>(T entity, int? commandTimeout) where T : class
        {
            return _dapper.Update<T>(Connection, entity, _transaction, commandTimeout);
        }

        public bool Delete<T>(T entity, DbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Delete(Connection, entity, transaction, commandTimeout);
        }

        public bool Delete<T>(T entity, int? commandTimeout) where T : class
        {
            return _dapper.Delete(Connection, entity, _transaction, commandTimeout);
        }

        public bool Delete<T>(object predicate, DbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Delete<T>(Connection, predicate, transaction, commandTimeout);
        }

        public bool Delete<T>(object predicate, int? commandTimeout) where T : class
        {
            return _dapper.Delete<T>(Connection, predicate, _transaction, commandTimeout);
        }

        public IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetList<T>(Connection, predicate, sort, transaction, commandTimeout, buffered);
        }

        public IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetList<T>(Connection, predicate, sort, _transaction, commandTimeout, buffered);
        }

        public IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        public IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, buffered);
        }

        public IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
        }

        public IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, buffered);
        }

        public int Count<T>(object predicate, DbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Count<T>(Connection, predicate, transaction, commandTimeout);
        }

        public int Count<T>(object predicate, int? commandTimeout) where T : class
        {
            return _dapper.Count<T>(Connection, predicate, _transaction, commandTimeout);
        }

        public IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout)
        {
            return _dapper.GetMultiple(Connection, predicate, transaction, commandTimeout);
        }

        public IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, int? commandTimeout)
        {
            return _dapper.GetMultiple(Connection, predicate, _transaction, commandTimeout);
        }

        public void ClearCache()
        {
            _dapper.SqlGenerator.Configuration.ClearCache();
        }

        public Guid GetNextGuid()
        {
            return _dapper.SqlGenerator.Configuration.GetNextGuid();
        }

        public IClassMapper GetMap<T>() where T : class
        {
            return _dapper.SqlGenerator.Configuration.GetMap<T>();
        }
    }
}