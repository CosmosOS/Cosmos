using System.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    public interface IClassMapper
    {
        string SchemaName { get; }
        string TableName { get; }
        IList<IPropertyMap> Properties { get; }
        Type EntityType { get; }
    }

    public interface IClassMapper<T> : IClassMapper where T : class
    {
    }

    /// <summary>
    /// Maps an entity to a table through a collection of property maps.
    /// </summary>
    public class ClassMapper<T> : IClassMapper<T> where T : class
    {
        /// <summary>
        /// Gets or sets the schema to use when referring to the corresponding table name in the database.
        /// </summary>
        public string SchemaName { get; protected set; }

        /// <summary>
        /// Gets or sets the table to use in the database.
        /// </summary>
        public string TableName { get; protected set; }

        /// <summary>
        /// A collection of properties that will map to columns in the database table.
        /// </summary>
        public IList<IPropertyMap> Properties { get; private set; }

        public Type EntityType => typeof(T);

        public ClassMapper()
        {
            PropertyTypeKeyTypeMapping = new Dictionary<Type, KeyType>
                                             {
                                                 { typeof(byte), KeyType.Identity }, { typeof(byte?), KeyType.Identity },
                                                 { typeof(sbyte), KeyType.Identity }, { typeof(sbyte?), KeyType.Identity },
                                                 { typeof(short), KeyType.Identity }, { typeof(short?), KeyType.Identity },
                                                 { typeof(ushort), KeyType.Identity }, { typeof(ushort?), KeyType.Identity },
                                                 { typeof(int), KeyType.Identity }, { typeof(int?), KeyType.Identity },
                                                 { typeof(uint), KeyType.Identity}, { typeof(uint?), KeyType.Identity },
                                                 { typeof(long), KeyType.Identity }, { typeof(long?), KeyType.Identity },
                                                 { typeof(ulong), KeyType.Identity }, { typeof(ulong?), KeyType.Identity },
                                                 { typeof(BigInteger), KeyType.Identity }, { typeof(BigInteger?), KeyType.Identity },
                                                 { typeof(Guid), KeyType.Guid }, { typeof(Guid?), KeyType.Guid },
                                             };

            Properties = new List<IPropertyMap>();
            Table(typeof(T).Name);
        }

        protected Dictionary<Type, KeyType> PropertyTypeKeyTypeMapping { get; private set; }

        public virtual void Schema(string aSchemaName)
        {
            SchemaName = aSchemaName;
        }

        public virtual void Table(string aTableName)
        {
            TableName = aTableName;
        }

        protected virtual void AutoMap()
        {
            AutoMap(null);
        }

        protected virtual void AutoMap(Func<Type, PropertyInfo, bool> aCanMap)
        {
            Type type = typeof(T);
            bool hasDefinedKey = Properties.Any(aP => aP.KeyType != KeyType.NotAKey);
            PropertyMap keyMap = null;
            foreach (var propertyInfo in type.GetProperties())
            {
                if (Properties.Any(aP => aP.Name.Equals(propertyInfo.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                if ((aCanMap != null && !aCanMap(type, propertyInfo)))
                {
                    continue;
                }

                PropertyMap map = Map(propertyInfo);
                if (!hasDefinedKey)
                {
                    if (String.Equals(map.PropertyInfo.Name, "id", StringComparison.CurrentCultureIgnoreCase))
                    {
                        keyMap = map;
                    }

                    // if (keyMap == null && map.PropertyInfo.Name.EndsWith("id", true, CultureInfo.InvariantCulture))
                    if (keyMap == null && map.PropertyInfo.Name.EndsWith("id", StringComparison.CurrentCultureIgnoreCase))
                    {
                        keyMap = map;
                    }
                }
            }

            if (keyMap != null)
            {
                keyMap.Key(PropertyTypeKeyTypeMapping.ContainsKey(keyMap.PropertyInfo.PropertyType)
                    ? PropertyTypeKeyTypeMapping[keyMap.PropertyInfo.PropertyType]
                    : KeyType.Assigned);
            }
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected PropertyMap Map(Expression<Func<T, object>> aExpression)
        {
            return Map(ReflectionHelper.GetProperty(aExpression) as PropertyInfo);
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected PropertyMap Map(PropertyInfo aPropertyInfo)
        {
            PropertyMap result = new(aPropertyInfo);
            GuardForDuplicatePropertyMap(result);
            Properties.Add(result);
            return result;
        }

        private void GuardForDuplicatePropertyMap(PropertyMap aResult)
        {
            if (Properties.Any(aP => aP.Name.Equals(aResult.Name)))
            {
                throw new ArgumentException(String.Format("Duplicate mapping for property {0} detected.",aResult.Name));
            }
        }
    }
}
