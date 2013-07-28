using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Data.Common;
using System.Linq.Expressions;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Metadata.Edm;

//Microsoft Public License (Ms-PL)
//This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
//1. Definitions
//The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//A "contribution" is the original software, or any additions or changes to the software.
//A "contributor" is any person that distributes its contribution under this license.
//"Licensed patents" are a contributor's patent claims that read directly on its contribution.
//2. Grant of Rights
//(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
//3. Conditions and Limitations
//(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

namespace Cosmos.Debug.Common {
  /// <summary>
  /// The EntityDataReader wraps a collection of CLR objects in a DbDataReader.  
  /// Only "scalar" properties are projected, with the exception that Entity Framework
  /// EntityObjects that have references to other EntityObjects will have key values for
  /// the related entity projected.
  /// 
  /// This is useful for doing high-speed data loads with SqlBulkCopy, and copying collections
  /// of entities ot a DataTable for use with SQL Server Table-Valued parameters, or for interop
  /// with older ADO.NET applciations.
  /// 
  /// For explicit control over the fields projected by the DataReader, just wrap your collection
  /// of entities in a anonymous type projection before wrapping it in an EntityDataReader.
  /// 
  /// Instead of 
  /// IEnumerable<Order> orders;
  /// ...
  /// IDataReader dr = orders.AsDataReader();
  /// 
  /// do
  /// IEnumerable<Order> orders;
  /// ...
  /// var q = from o in orders
  ///         select new 
  ///         {
  ///            ID=o.ID,
  ///            ShipDate=o.ShipDate,
  ///            ProductName=o.Product.Name,
  ///            ...
  ///         }
  /// IDataReader dr = q.AsDataReader();
  /// 
  /// </summary>
//  /// <typeparam name="T"></typeparam>
//  public sealed class EntityDataReader<T> : DbDataReader, IDataReader {

//    readonly IEnumerator<T> enumerator;
//    readonly EntityDataReaderOptions options;

//    T current;
//    bool closed = false;

//    static List<Attribute> scalarAttributes;
//    static List<Attribute> scalarAttributesPlusRelatedObjectScalarAttributes;
//    static List<Attribute> scalarAttributesPlusRelatedObjectKeyAttributes;

//    readonly List<Attribute> attributes;

//    #region Attribute inner type
//    private class Attribute {
//      //PropertyInfo propertyInfo;
//      public readonly Type Type;
//      public readonly string FullName;
//      public readonly string Name;
//      public readonly bool IsRelatedAttribute;

//      readonly Func<T, object> ValueAccessor;

//      /// <summary>
//      /// Uses Lamda expressions to create a Func<T,object> that invokes the given property getter.
//      /// The property value will be extracted and cast to type TProperty
//      /// </summary>
//      /// <typeparam name="TObject">The type of the object declaring the property.</typeparam>
//      /// <typeparam name="TProperty">The type to cast the property value to</typeparam>
//      /// <param name="pi">PropertyInfo pointing to the property to wrap</param>
//      /// <returns></returns>
//      public static Func<TObject, TProperty> MakePropertyAccessor<TObject, TProperty>(PropertyInfo pi) {
//        ParameterExpression objParam = Expression.Parameter(typeof(TObject), "obj");
//        MemberExpression typedAccessor = Expression.PropertyOrField(objParam, pi.Name);
//        UnaryExpression castToObject = Expression.Convert(typedAccessor, typeof(object));
//        LambdaExpression lambdaExpr = Expression.Lambda<Func<TObject, TProperty>>(castToObject, objParam);

//        return (Func<TObject, TProperty>)lambdaExpr.Compile();
//      }


//      public static Func<TObject, TProperty> MakeRelatedPropertyAccessor<TObject, TProperty>(PropertyInfo pi, PropertyInfo pi2) {

//        Func<TObject, object> getRelatedObject;
//        {
//          // expression like:
//          //    return (object)t.SomeProp;
//          ParameterExpression typedParam = Expression.Parameter(typeof(T), "t");
//          MemberExpression typedAccessor = Expression.PropertyOrField(typedParam, pi.Name);
//          UnaryExpression castToObject = Expression.Convert(typedAccessor, typeof(object));
//          LambdaExpression lambdaExpr = Expression.Lambda<Func<TObject, object>>(castToObject, typedParam);
//          getRelatedObject = (Func<TObject, object>)lambdaExpr.Compile();
//        }


//        Func<object, TProperty> getRelatedObjectProperty;
//        {

//          // expression like:
//          //    return (object)((PropType)o).RelatedProperty;
//          ParameterExpression objParam = Expression.Parameter(typeof(object), "o");
//          UnaryExpression typedParam = Expression.Convert(objParam, pi.PropertyType);
//          MemberExpression typedAccessor = Expression.PropertyOrField(typedParam, pi2.Name);
//          UnaryExpression castToObject = Expression.Convert(typedAccessor, typeof(TProperty));
//          LambdaExpression lambdaExpr = Expression.Lambda<Func<object, TProperty>>(castToObject, objParam);
//          getRelatedObjectProperty = (Func<object, TProperty>)lambdaExpr.Compile();
//        }

//        Func<TObject, TProperty> f = (TObject t) => {
//          object o = getRelatedObject(t);
//          if (o == null) return default(TProperty);
//          return getRelatedObjectProperty(o);
//        };

//        return f;
//      }

//      public Attribute(PropertyInfo pi) {
//        this.FullName = pi.DeclaringType.Name + "_" + pi.Name;
//        this.Name = pi.Name;
//        Type = pi.PropertyType;
//        IsRelatedAttribute = false;

//        ValueAccessor = MakePropertyAccessor<T, object>(pi);
//      }

//      public Attribute(string fullName, string name, Type type, Func<T, object> getValue, bool isRelatedAttribute) {
//        this.FullName = fullName;
//        this.Name = name;
//        this.Type = type;
//        this.ValueAccessor = getValue;
//        this.IsRelatedAttribute = isRelatedAttribute;
//      }

//      public object GetValue(T target) {
//        return ValueAccessor(target);
//      }
//    }
//    #endregion

//    #region "Scalar Types"

//    static bool IsScalarType(Type t) {
//      return scalarTypes.Contains(t);
//    }
//    static readonly HashSet<Type> scalarTypes = LoadScalarTypes();
//    static HashSet<Type> LoadScalarTypes() {
//      HashSet<Type> set = new HashSet<Type>() 
//                              { 
//                                //reference types
//                                typeof(String),
//                                typeof(Byte[]),
//                                //value types
//                                typeof(Byte),
//                                typeof(Int16),
//                                typeof(Int32),
//                                typeof(Int64),
//                                typeof(Single),
//                                typeof(Double),
//                                typeof(Decimal),
//                                typeof(DateTime),
//                                typeof(Guid),
//                                typeof(Boolean),
//                                typeof(TimeSpan),
//                                //nullable value types
//                                typeof(Byte?),
//                                typeof(Int16?),
//                                typeof(Int32?),
//                                typeof(Int64?),
//                                typeof(Single?),
//                                typeof(Double?),
//                                typeof(Decimal?),
//                                typeof(DateTime?),
//                                typeof(Guid?),
//                                typeof(Boolean?),
//                                typeof(TimeSpan?)
//                              };


//      return set;

//    }
//    #endregion

//    #region Constructors

//    public EntityDataReader(IEnumerable<T> col)
//      : this(col, EntityDataReaderOptions.Default, null) { }

//    public EntityDataReader(IEnumerable<T> col, EntityDataReaderOptions options)
//      : this(col, options, null) { }

//    public EntityDataReader(IEnumerable<T> col, EntityDataReaderOptions options, ObjectContext objectContext) {
//      this.enumerator = col.GetEnumerator();
//      this.options = options;

//      if (options.RecreateForeignKeysForEntityFrameworkEntities && objectContext == null) {
//        throw new ArgumentException("If RecreateForeignKeysForEntityFrameworkEntities=true then objectContext is required");
//      }

//      //done without a lock, so we risk running twice
//      if (scalarAttributes == null) {
//        scalarAttributes = DiscoverScalarAttributes(typeof(T));
//      }
//      if (options.FlattenRelatedObjects && scalarAttributesPlusRelatedObjectScalarAttributes == null) {
//        var atts = DiscoverRelatedObjectScalarAttributes(typeof(T));
//        scalarAttributesPlusRelatedObjectScalarAttributes = atts.Concat(scalarAttributes).ToList();
//      }
//      if (options.RecreateForeignKeysForEntityFrameworkEntities && scalarAttributesPlusRelatedObjectKeyAttributes == null) {
//        var atts = DiscoverRelatedObjectKeyAttributes(typeof(T), objectContext);
//        scalarAttributesPlusRelatedObjectKeyAttributes = atts.Concat(scalarAttributes).ToList();
//      }

//      if (options.FlattenRelatedObjects) {
//        attributes = scalarAttributesPlusRelatedObjectScalarAttributes;
//      } else if (objectContext != null) {
//        attributes = scalarAttributesPlusRelatedObjectKeyAttributes;
//      } else {
//        attributes = scalarAttributes;
//      }


//    }


//    static List<Attribute> DiscoverScalarAttributes(Type thisType) {

//      //Not a collection of entities, just an IEnumerable<String> or other scalar type.
//      //So add just a single Attribute that returns the object itself
//      if (IsScalarType(thisType)) {
//        return new List<Attribute> { new Attribute("Value", "Value", thisType, t => t, false) };
//      }


//      //find all the scalar properties
//      var allProperties = (from p in thisType.GetProperties()
//                           where IsScalarType(p.PropertyType)
//                           select p).ToList();

//      //Look for a constructor with arguments that match the properties on name and type
//      //(name modulo case, which varies between constructor args and properties in coding convention)
//      //If such an "ordering constructor" exists, return the properties ordered by the corresponding
//      //constructor args ordinal position.  
//      //An important instance of an ordering constructor, is that C# anonymous types all have one.  So
//      //this enables a simple convention to specify the order of columns projected by the EntityDataReader
//      //by simply building the EntityDataReader from an anonymous type projection.
//      //If such a constructor is found, replace allProperties with a collection of properties sorted by constructor order.
//      foreach (var completeConstructor in from ci in thisType.GetConstructors()
//                                          where ci.GetParameters().Count() == allProperties.Count()
//                                          select ci) {
//        var q = (from cp in completeConstructor.GetParameters()
//                 join p in allProperties
//                   on new { n = cp.Name.ToLower(), t = cp.ParameterType } equals new { n = p.Name.ToLower(), t = p.PropertyType }
//                 select new { cp, p }).ToList();

//        if (q.Count() == allProperties.Count()) //all constructor parameters matched by name and type to properties
//        {
//          //sort all properties by constructor ordinal position
//          allProperties = (from o in q
//                           orderby o.cp.Position
//                           select o.p).ToList();
//          break; //stop looking for an ordering consturctor
//        }


//      }

//      return allProperties.Select(p => new Attribute(p)).ToList();

//    }
//    static List<Attribute> DiscoverRelatedObjectKeyAttributes(Type thisType, ObjectContext objectContext) {

//      var attributeList = new SortedList<string, Attribute>();


//      //recreate foreign key column values
//      //by adding Attributes for any key values of referenced entities 
//      //that aren't already exposed as scalar properties
//      var mw = objectContext.MetadataWorkspace;
//      var entityTypesByName = mw.GetItems<EntityType>(DataSpace.OSpace).ToLookup(e => e.FullName);

//      //find the EntityType metadata for T 
//      EntityType thisEntity = entityTypesByName[thisType.FullName].First();
//      var thisEntityKeys = thisEntity.KeyMembers.ToDictionary(k => k.Name);

//      //TODOx use the NavigationProperties instead of the ENtityRelations -- too complicated
//      //TODO fix the attribute naming.  Probably requires marking each attribtue as direct or related.


//      var erProps = thisType.GetProperties()
//                            .Where(p => typeof(EntityReference)
//                            .IsAssignableFrom(p.PropertyType)).ToList();


//      //For each EntityRelation property add add the keys of the related Entity
//      foreach (var pi in erProps) {
//        //Find the name of the CLR Type at the other end of the reference because we need to get its key attributes.
//        //the property type is EntityReference<T>, we need T.
//        string relatedEntityCLRTypeName = pi.PropertyType.GetGenericArguments().First().FullName;

//        //Find the EntityType at the other end of the relationship because we need to get its key attributes.
//        EntityType relatedEntityEFType = entityTypesByName[relatedEntityCLRTypeName].FirstOrDefault();
//        if (relatedEntityEFType == null) {
//          throw new InvalidOperationException("Cannot find EntityType for EntityReference Property " + pi.Name);
//        }

//        //Add attributes for each key value of the related entity.  These are the properties that
//        //would probably appear in the storage object.  The names will be the same as they are on the 
//        //related entity, except prefixed with the related entity name, 
//        //and with a check to make sure that we're not introducing a duplicate.
//        // so if you have 
//        //  if OrderItem.OrderID -> Order.ID   then the column will be Order_ID
//        //  if OrderItem.OrderID -> Order.OrderID   then the column will be Order_OrderID
//        foreach (var key in relatedEntityEFType.KeyMembers) {
//          string targetKeyAttributeName = key.Name;

//          //TODO it would be better to get the NavigationProperty and find the ToEndMember name
//          //but the NavigationProperty doesn't have good way to get the EntityReference 
//          //or the related entity key.
//          string referenceName;
//          if (pi.Name.EndsWith("Reference", StringComparison.Ordinal)) {
//            referenceName = pi.Name.Substring(0, pi.Name.Length - "Reference".Length);
//          } else  //there's no rule that the EntityReference named like this so, if not just use the target type
//          {
//            referenceName = pi.PropertyType.Name;

//            //if there are multiple relations to the same target type, just uniqify them with an index
//            int ix = erProps.Where(p => p.PropertyType == pi.PropertyType).ToList().IndexOf(pi);
//            if (ix > 0) {
//              referenceName = referenceName + ix.ToString(System.Globalization.CultureInfo.InvariantCulture);
//            }

//          }
//          string fullName = referenceName + "_" + key.Name;


//          //bind out local variables for the valueAccessor closure.
//          Type kType = Type.GetType(key.TypeUsage.EdmType.FullName);
//          PropertyInfo entityReferenceProperty = pi;

//          Func<T, object> valueAccessor = o => {
//            EntityReference er = (EntityReference)entityReferenceProperty.GetValue(o, null);

//            //for nullable foregn keys, just return null
//            if (er.EntityKey == null) {
//              return null;
//            }
//            object val = er.EntityKey.EntityKeyValues.First(k => k.Key == targetKeyAttributeName).Value;
//            return val;
//          };
//          string name = key.Name;

//          attributeList.Add(name, new Attribute(fullName, name, kType, valueAccessor, true));
//        }


//      }

//      return attributeList.Values.ToList();

//    }
//    static List<Attribute> DiscoverRelatedObjectScalarAttributes(Type thisType) {

//      var atts = new List<Attribute>();

//      //get the related objects which aren't scalars, not EntityReference objects and not collections
//      var relatedObjectProperties =
//                        (from p in thisType.GetProperties()
//                         where !IsScalarType(p.PropertyType)
//                            && !typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)
//                            && !typeof(EntityReference).IsAssignableFrom(p.PropertyType)
//                            && !typeof(EntityKey).IsAssignableFrom(p.PropertyType)
//                         select p).ToList();

//      foreach (var rop in relatedObjectProperties) {
//        var type = rop.PropertyType;
//        //get the scalar properties for the related type
//        var scalars = type.GetProperties().Where(p => IsScalarType(p.PropertyType)).ToList();

//        foreach (var sp in scalars) {
//          string attName = rop.Name + "_" + sp.Name;
//          //create a value accessor which takes an instance of T, and returns the related object scalar
//          var valueAccessor = Attribute.MakeRelatedPropertyAccessor<T, object>(rop, sp);
//          string name = attName;
//          Attribute att = new Attribute(rop.Name, attName, sp.PropertyType, valueAccessor, true);
//          atts.Add(att);
//        }

//      }
//      return atts;

//    }



//    #endregion

//    #region Utility Methods
//    static Type nullable_T = typeof(System.Nullable<int>).GetGenericTypeDefinition();
//    static bool IsNullable(Type t) {
//      return (t.IsGenericType
//          && t.GetGenericTypeDefinition() == nullable_T);
//    }
//    static Type StripNullableType(Type t) {
//      return t.GetGenericArguments()[0];
//    }
//    #endregion

//    #region GetSchemaTable


//    const string shemaTableSchema = @"<?xml version=""1.0"" standalone=""yes""?>
//<xs:schema id=""NewDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
//  <xs:element name=""NewDataSet"" msdata:IsDataSet=""true"" msdata:MainDataTable=""SchemaTable"" msdata:Locale="""">
//    <xs:complexType>
//      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
//        <xs:element name=""SchemaTable"" msdata:Locale="""" msdata:MinimumCapacity=""1"">
//          <xs:complexType>
//            <xs:sequence>
//              <xs:element name=""ColumnName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""ColumnOrdinal"" msdata:ReadOnly=""true"" type=""xs:int"" default=""0"" minOccurs=""0"" />
//              <xs:element name=""ColumnSize"" msdata:ReadOnly=""true"" type=""xs:int"" minOccurs=""0"" />
//              <xs:element name=""NumericPrecision"" msdata:ReadOnly=""true"" type=""xs:short"" minOccurs=""0"" />
//              <xs:element name=""NumericScale"" msdata:ReadOnly=""true"" type=""xs:short"" minOccurs=""0"" />
//              <xs:element name=""IsUnique"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""IsKey"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""BaseServerName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""BaseCatalogName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""BaseColumnName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""BaseSchemaName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""BaseTableName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""DataType"" msdata:DataType=""System.Type, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""AllowDBNull"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""ProviderType"" msdata:ReadOnly=""true"" type=""xs:int"" minOccurs=""0"" />
//              <xs:element name=""IsAliased"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""IsExpression"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""IsIdentity"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""IsAutoIncrement"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""IsRowVersion"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""IsHidden"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""IsLong"" msdata:ReadOnly=""true"" type=""xs:boolean"" default=""false"" minOccurs=""0"" />
//              <xs:element name=""IsReadOnly"" msdata:ReadOnly=""true"" type=""xs:boolean"" minOccurs=""0"" />
//              <xs:element name=""ProviderSpecificDataType"" msdata:DataType=""System.Type, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""DataTypeName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""XmlSchemaCollectionDatabase"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""XmlSchemaCollectionOwningSchema"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""XmlSchemaCollectionName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""UdtAssemblyQualifiedName"" msdata:ReadOnly=""true"" type=""xs:string"" minOccurs=""0"" />
//              <xs:element name=""NonVersionedProviderType"" msdata:ReadOnly=""true"" type=""xs:int"" minOccurs=""0"" />
//            </xs:sequence>
//          </xs:complexType>
//        </xs:element>
//      </xs:choice>
//    </xs:complexType>
//  </xs:element>
//</xs:schema>";
//    public override DataTable GetSchemaTable() {
//      DataSet s = new DataSet();
//      s.Locale = System.Globalization.CultureInfo.CurrentCulture;
//      s.ReadXmlSchema(new System.IO.StringReader(shemaTableSchema));
//      DataTable t = s.Tables[0];
//      for (int i = 0; i < this.FieldCount; i++) {
//        DataRow row = t.NewRow();
//        row["ColumnName"] = this.GetName(i);
//        row["ColumnOrdinal"] = i;

//        Type type = this.GetFieldType(i);
//        if (type.IsGenericType
//          && type.GetGenericTypeDefinition() == typeof(System.Nullable<int>).GetGenericTypeDefinition()) {
//          type = type.GetGenericArguments()[0];
//        }
//        row["DataType"] = this.GetFieldType(i);
//        row["DataTypeName"] = this.GetDataTypeName(i);
//        row["ColumnSize"] = -1;
//        t.Rows.Add(row);
//      }
//      return t;

//    }
//    #endregion

//    #region IDataReader Members

//    public override void Close() {
//      closed = true;
//    }

//    public override int Depth {
//      get { return 1; }
//    }


//    public override bool IsClosed {
//      get { return closed; }
//    }

//    public override bool NextResult() {
//      return false;
//    }

//    int entitiesRead = 0;
//    public override bool Read() {
//      bool rv = enumerator.MoveNext();
//      if (rv) {
//        current = enumerator.Current;
//        entitiesRead += 1;
//      }
//      return rv;
//    }

//    public override int RecordsAffected {
//      get { return -1; }
//    }

//    #endregion

//    #region IDisposable Members

//    protected override void Dispose(bool disposing) {
//      Close();
//      base.Dispose(disposing);
//    }

//    #endregion

//    #region IDataRecord Members

//    public override int FieldCount {
//      get {
//        return attributes.Count;
//      }
//    }

//    TField GetValue<TField>(int i) {
//      TField val = (TField)attributes[i].GetValue(current);
//      return val;
//    }
//    public override bool GetBoolean(int i) {
//      return GetValue<bool>(i);
//    }

//    public override byte GetByte(int i) {
//      return GetValue<byte>(i);
//    }

//    public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {

//      var buf = GetValue<byte[]>(i);
//      int bytes = Math.Min(length, buf.Length - (int)fieldOffset);
//      Buffer.BlockCopy(buf, (int)fieldOffset, buffer, bufferoffset, bytes);
//      return bytes;

//    }

//    public override char GetChar(int i) {
//      return GetValue<char>(i);
//    }

//    public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
//      //throw new NotImplementedException();
//      string s = GetValue<string>(i);
//      int chars = Math.Min(length, s.Length - (int)fieldoffset);
//      s.CopyTo((int)fieldoffset, buffer, bufferoffset, chars);

//      return chars;
//    }

//    //public override DbDataReader GetData(int i)
//    //{
//    //  throw new NotImplementedException();
//    //}

//    public override string GetDataTypeName(int i) {
//      return attributes[i].Type.Name;
//    }

//    public override DateTime GetDateTime(int i) {
//      return GetValue<DateTime>(i);
//    }

//    public override decimal GetDecimal(int i) {
//      return GetValue<decimal>(i);
//    }

//    public override double GetDouble(int i) {
//      return GetValue<double>(i);
//    }

//    public override Type GetFieldType(int i) {
//      Type t = attributes[i].Type;
//      if (!options.ExposeNullableTypes && IsNullable(t)) {
//        return StripNullableType(t);
//      }
//      return t;
//    }

//    public override float GetFloat(int i) {
//      return GetValue<float>(i);
//    }

//    public override Guid GetGuid(int i) {
//      return GetValue<Guid>(i);
//    }

//    public override short GetInt16(int i) {
//      return GetValue<short>(i);
//    }

//    public override int GetInt32(int i) {
//      return GetValue<int>(i);
//    }

//    public override long GetInt64(int i) {
//      return GetValue<long>(i);
//    }

//    public override string GetName(int i) {
//      Attribute a = attributes[i];
//      if (a.IsRelatedAttribute && options.PrefixRelatedObjectColumns) {
//        return a.FullName;
//      }
//      return a.Name;
//    }

//    public override int GetOrdinal(string name) {
//      for (int i = 0; i < attributes.Count; i++) {
//        var a = attributes[i];

//        if (!a.IsRelatedAttribute && a.Name == name) {
//          return i;
//        }

//        if (options.PrefixRelatedObjectColumns && a.IsRelatedAttribute && a.FullName == name) {
//          return i;
//        }

//        if (!options.PrefixRelatedObjectColumns && a.IsRelatedAttribute && a.Name == name) {
//          return i;
//        }


//      }
//      return -1;
//    }

//    public override string GetString(int i) {
//      return GetValue<string>(i);
//    }



//    public override int GetValues(object[] values) {
//      for (int i = 0; i < attributes.Count; i++) {
//        values[i] = GetValue(i);
//      }
//      return attributes.Count;
//    }



//    public override object GetValue(int i) {
//      object o = GetValue<object>(i);
//      if (!options.ExposeNullableTypes && o == null) {
//        return DBNull.Value;
//      }
//      return o;
//    }

//    public override bool IsDBNull(int i) {
//      object o = GetValue<object>(i);
//      return (o == null);
//    }

//    public override object this[string name] {
//      get { return GetValue(GetOrdinal(name)); }
//    }

//    public override object this[int i] {
//      get { return GetValue(i); }
//    }

//    #endregion

//    #region DbDataReader Members



//    public override System.Collections.IEnumerator GetEnumerator() {
//      return this.enumerator;
//    }

//    public override bool HasRows {
//      get { throw new NotSupportedException(); }
//    }
//    #endregion

//  }

//  public class EntityDataReaderOptions {
//    public static EntityDataReaderOptions Default {
//      get { return new EntityDataReaderOptions(true, false, true, false); }
//    }

//    public EntityDataReaderOptions(
//      bool exposeNullableTypes,
//      bool flattenRelatedObjects,
//      bool prefixRelatedObjectColumns,
//      bool recreateForeignKeysForEntityFrameworkEntities) {
//      this.ExposeNullableTypes = exposeNullableTypes;
//      this.FlattenRelatedObjects = flattenRelatedObjects;
//      this.PrefixRelatedObjectColumns = prefixRelatedObjectColumns;
//      this.RecreateForeignKeysForEntityFrameworkEntities = recreateForeignKeysForEntityFrameworkEntities;
//    }

//    /// <summary>
//    /// If true nullable value types are returned directly by the DataReader.
//    /// If false, the DataReader will expose non-nullable value types and return DbNull.Value
//    /// for null values.  
//    /// When loading a DataTable this option must be set to True, since DataTable does not support
//    /// nullable types.
//    /// </summary>
//    public bool ExposeNullableTypes { get; set; }

//    /// <summary>
//    /// If True then the DataReader will project scalar properties from related objects in addition
//    /// to scalar properties from the main object.  This is especially useful for custom projecttions like
//    ///         var q = from od in db.SalesOrderDetail
//    ///         select new
//    ///         {
//    ///           od,
//    ///           ProductID=od.Product.ProductID,
//    ///           ProductName=od.Product.Name
//    ///         };
//    /// Related objects assignable to EntityKey, EntityRelation, and IEnumerable are excluded.
//    /// 
//    /// If False, then only scalar properties from teh main object will be projected.         
//    /// </summary>
//    public bool FlattenRelatedObjects { get; set; }

//    /// <summary>
//    /// If True columns projected from related objects will have column names prefixed by the
//    /// name of the relating property.  This appies to either from setting FlattenRelatedObjects to True,
//    /// or RecreateForeignKeysForEntityFrameworkEntities to True.
//    /// 
//    /// If False columns will be created for related properties that are not prefixed.  This can lead
//    /// to column name collision.
//    /// </summary>
//    public bool PrefixRelatedObjectColumns { get; set; }

//    /// <summary>
//    /// If True the DataReader will create columns for the key properties of related Entities.
//    /// You must pass an ObjectContext and have retrieved the entity with change tracking for this to work.
//    /// </summary>
//    public bool RecreateForeignKeysForEntityFrameworkEntities { get; set; }

//  }

//  public static class EntityDataReaderExtensions {

//    /// <summary>
//    /// Wraps the IEnumerable in a DbDataReader, having one column for each "scalar" property of the type T.  
//    /// The collection will be enumerated as the client calls IDataReader.Read().
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    /// <param name="collection"></param>
//    /// <returns></returns>
//    public static IDataReader AsDataReader<T>(this IEnumerable<T> collection) {

//      //For anonymous type projections default to flattening related objects and not prefixing columns
//      //The reason being that if the programmer has taken control of the projection, the default should
//      //be to expose everying in the projection and not mess with the names.
//      if (typeof(T).IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)) {
//        var options = EntityDataReaderOptions.Default;
//        options.FlattenRelatedObjects = true;
//        options.PrefixRelatedObjectColumns = false;
//        return new EntityDataReader<T>(collection, options);
//      }
//      return new EntityDataReader<T>(collection);
//    }

//    /// <summary>
//    /// Wraps the IEnumerable in a DbDataReader, having one column for each "scalar" property of the type T.  
//    /// The collection will be enumerated as the client calls IDataReader.Read().
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    /// <param name="collection"></param>
//    /// <returns></returns>
//    public static IDataReader AsDataReader<T>(this IEnumerable<T> collection, bool exposeNullableColumns, bool flattenRelatedObjects) {
//      EntityDataReaderOptions options = new EntityDataReaderOptions(exposeNullableColumns, flattenRelatedObjects, true, false);

//      return new EntityDataReader<T>(collection, options, null);
//    }


//    /// <summary>
//    /// Enumerates the collection and copies the data into a DataTable.
//    /// </summary>
//    /// <typeparam name="T">The element type of the collection.</typeparam>
//    /// <param name="collection">The collection to copy to a DataTable</param>
//    /// <returns>A DataTable containing the scalar projection of the collection.</returns>
//    public static DataTable ToDataTable<T>(this IEnumerable<T> collection) {
//      DataTable t = new DataTable();
//      t.Locale = System.Globalization.CultureInfo.CurrentCulture;
//      t.TableName = typeof(T).Name;
//      EntityDataReaderOptions options = EntityDataReaderOptions.Default;
//      options.ExposeNullableTypes = false;
//      EntityDataReader<T> dr = new EntityDataReader<T>(collection, options);
//      t.Load(dr);
//      return t;
//    }

//    /// <summary>
//    /// Wraps the collection in a DataReader, but also includes columns for the key attributes of related Entities.
//    /// </summary>
//    /// <typeparam name="T">The element type of the collection.</typeparam>
//    /// <param name="collection">A collection to wrap in a DataReader</param>
//    /// <param name="cx">The Entity Framework ObjectContext, used for metadata access</param>
//    /// <returns>A DbDataReader wrapping the collection.</returns>
//    public static IDataReader AsDataReader<T>(this IEnumerable<T> collection, ObjectContext context) where T : class {
//      EntityDataReaderOptions options = EntityDataReaderOptions.Default;
//      options.RecreateForeignKeysForEntityFrameworkEntities = true;
//      return new EntityDataReader<T>(collection, options, context);
//    }



//    /// <summary>
//    /// Wraps the collection in a DataReader, but also includes columns for the key attributes of related Entities.
//    /// </summary>
//    /// <typeparam name="T">The element type of the collectin.</typeparam>
//    /// <param name="collection">A collection to wrap in a DataReader</param>
//    /// <param name="detachObjects">Option to detach each object in the collection from the ObjectContext.  This can reduce memory usage for queries returning large numbers of objects.</param>
//    /// <param name="prefixReladObjectColumns">If True, qualify the related object keys, if False don't</param>
//    /// <returns>A DbDataReader wrapping the collection.</returns>
//    /// <summary>
//    public static IDataReader AsDataReader<T>(this IEnumerable<T> collection, ObjectContext context, bool detachObjects, bool prefixRelatedObjectColumns) where T : class {
//      EntityDataReaderOptions options = EntityDataReaderOptions.Default;
//      options.RecreateForeignKeysForEntityFrameworkEntities = true;
//      options.PrefixRelatedObjectColumns = prefixRelatedObjectColumns;

//      if (detachObjects) {
//        return new EntityDataReader<T>(collection.DetachAllFrom(context), options, context);
//      }
//      return new EntityDataReader<T>(collection, options, context);
//    }
//    static IEnumerable<T> DetachAllFrom<T>(this IEnumerable<T> col, ObjectContext cx) {
//      foreach (var t in col) {
//        cx.Detach(t);
//        yield return t;
//      }
//    }

//    /// <summary>
//    /// Enumerates the collection and copies the data into a DataTable, but also includes columns for the key attributes of related Entities.
//    /// </summary>
//    /// <typeparam name="T">The element type of the collection.</typeparam>
//    /// <param name="collection">The collection to copy to a DataTable</param>
//    /// <param name="cx">The Entity Framework ObjectContext, used for metadata access</param>
//    /// <returns>A DataTable containing the scalar projection of the collection.</returns>
//    public static DataTable ToDataTable<T>(this IEnumerable<T> collection, ObjectContext context) where T : class {
//      DataTable t = new DataTable();
//      t.Locale = System.Globalization.CultureInfo.CurrentCulture;
//      t.TableName = typeof(T).Name;

//      EntityDataReaderOptions options = EntityDataReaderOptions.Default;
//      options.RecreateForeignKeysForEntityFrameworkEntities = true;

//      EntityDataReader<T> dr = new EntityDataReader<T>(collection, options, context);
//      t.Load(dr);
//      return t;
//    }
//  }
}
