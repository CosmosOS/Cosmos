using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Config;
using System.Data.SQLite;
using System.Reflection;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;

namespace Cosmos.Debug.Common
{
    public class DebugModelConfiguration: DbConfiguration
    {
        public DebugModelConfiguration()
        {
            //var asm = Assembly.LoadWithPartialName("System.Data.SQLite.Linq");

            AddDbProviderFactory("System.Data.SQLite", new SQLiteFactory());
            var fact = new SQLiteProviderServices();
            AddDbProviderServices("System.Data.SQLite", fact);
            AddSecondaryResolver(new OurResolver(fact));
            this.SetDatabaseInitializer<Entities>(new OurInitializer<Entities>());
            this.SetDefaultConnectionFactory(new SQLiteConnectionFactory());
        }

        private class OurInitializer<T> : IDatabaseInitializer<T> where T: DbContext
        {
            public void InitializeDatabase(T context)
            {
            }
        }

        private class OurResolver: IDbDependencyResolver
        {
            public OurResolver(IDbDependencyResolver res)
            {
                mRes = res;
            }

            private IDbDependencyResolver mRes;
            private OurFactory mFactory=new OurFactory();
            public object GetService(Type type, object key)
            {
                if (type == typeof(IDbProviderFactoryService))
                {
                    return mFactory;
                }
                //Console.WriteLine("Get service '{0}', key = '{1}'", type.FullName, key);
                return mRes.GetService(type, key);
            }

            public IEnumerable<object> GetServices(Type type, object key)
            {
                yield break;
            }
        }

        //private class OurFactory: 
    }

    public class OurFactory : IDbProviderFactoryService
    {
        public System.Data.Common.DbProviderFactory GetProviderFactory(System.Data.Common.DbConnection connection)
        {
            if (connection is SQLiteConnection)
            {
                return SQLiteFactory.Instance;
            }
            if (connection is EntityConnection)
            {
                var econ = (EntityConnection)connection;
                return GetProviderFactory(econ.StoreConnection);
            }
            throw new Exception("Connection type '" + connection.GetType().FullName + "' not supported!");
        }
    }
}