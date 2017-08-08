using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace Cosmos.Debug.Symbols
{
    public class ObjectReader<T>: IDataReader
    {
        private static readonly ConcurrentDictionary<string, Tuple<string[], Func<object, object>[]>> mInfo = new ConcurrentDictionary<string, Tuple<string[], Func<object, object>[]>>();

        private readonly IEnumerable<T> mItems;
        private readonly IEnumerator<T> mItemsEnumerator;
        private int mCurrentIndex = -1;

        // -1 is before first read, -2 is closed
        public ObjectReader(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            mItems = items;
            mItemsEnumerator = items.GetEnumerator();

            // initialize
            Tuple<string[], Func<object, object>[]> xInfo;
            if (mInfo.TryGetValue(typeof(T).FullName, out xInfo))
            {
                mFieldGetters = xInfo.Item2;
                mFieldNames = xInfo.Item1;
            }
            else
            {
                Console.WriteLine("Detecting fields for type '{0}'", typeof(T).FullName);
                var props = typeof(T).GetTypeInfo().GetProperties();
                var xGetters = new List<Func<object, object>>();
                var xNames = new List<string>();
                foreach (var prop in props)
                {
                    var property = prop;
                    xNames.Add(prop.Name);
                    var dynMetho = new DynamicMethod("_" + Guid.NewGuid().ToString("n"), typeof(object), new Type[] { typeof(object) });
                    var gen = dynMetho.GetILGenerator();
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Castclass, typeof(T));
                    gen.Emit(OpCodes.Call, prop.GetGetMethod());
                    if (prop.PropertyType.GetTypeInfo().IsPrimitive || prop.PropertyType.GetTypeInfo().IsValueType)
                    {
                        gen.Emit(OpCodes.Box, prop.PropertyType);
                    }
                    gen.Emit(OpCodes.Ret);
                    xGetters.Add((Func<object, object>)dynMetho.CreateDelegate(typeof(Func<object, object>)));
                }
                mFieldNames = xNames.ToArray();
                mFieldGetters = xGetters.ToArray();
                mInfo.TryAdd(typeof(T).FullName, Tuple.Create<string[], Func<object, object>[]>(mFieldNames, mFieldGetters));
            }
        }

        private static string[] mFieldNames;
        private static Func<object, object>[] mFieldGetters;

        public void Close()
        {
            mCurrentIndex = -2;
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed
        {
            get
            {
                return mCurrentIndex == -2;
            }
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        private T mCurrentItem;

        public bool Read()
        {
            mCurrentIndex++;
            var xResult = mItemsEnumerator.MoveNext();
            if (xResult)
            {
                mCurrentItem = mItemsEnumerator.Current;
            }
            else
            {
                mCurrentItem = default(T);
            }
            return xResult;
        }

        public int RecordsAffected
        {
            get
            {
                //return mItems.Length;
                throw new InvalidOperationException();
            }
        }

        public void Dispose()
        {
            Close();
        }

        public int FieldCount
        {
            get
            {
                return mFieldNames.Length;
            }
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            return mFieldNames[i];
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            return mFieldGetters[i](mCurrentItem);
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get { throw new NotImplementedException(); }
        }
    }
}
