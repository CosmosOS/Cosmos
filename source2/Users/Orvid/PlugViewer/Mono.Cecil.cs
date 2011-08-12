#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Slot = Mono.Cecil.Metadata.Row<string, string>;
using Mono.Cecil.Metadata;
using Mono.Cecil.Cil;
using System.Text;
using Mono.Cecil.PE;
using Mono.Collections.Generic;
using SR = System.Reflection;
using RVA = System.UInt32;
using MD = Mono.Cecil.Metadata;
using System.Security.Cryptography;
using System.Globalization;
using System.Runtime.InteropServices;


namespace Mono
{
    #region Root
    delegate void Action();
    delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    delegate TResult Func<TResult>();
    delegate TResult Func<T, TResult>(T arg1);
    delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
    delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    static class Empty<T>
    {

        public static readonly T[] Array = new T[0];
    }
    #endregion

    #region Collections.Generic
    namespace Collections.Generic
    {
        public class Collection<T> : IList<T>, IList
        {

            internal T[] items;
            internal int size;
            int version;

            public int Count
            {
                get { return size; }
            }

            public T this[int index]
            {
                get
                {
                    if (index >= size)
                        throw new ArgumentOutOfRangeException();

                    return items[index];
                }
                set
                {
                    CheckIndex(index);
                    if (index == size)
                        throw new ArgumentOutOfRangeException();

                    OnSet(value, index);

                    items[index] = value;
                }
            }

            bool ICollection<T>.IsReadOnly
            {
                get { return false; }
            }

            bool IList.IsFixedSize
            {
                get { return false; }
            }

            bool IList.IsReadOnly
            {
                get { return false; }
            }

            object IList.this[int index]
            {
                get { return this[index]; }
                set
                {
                    CheckIndex(index);

                    try
                    {
                        this[index] = (T)value;
                        return;
                    }
                    catch (InvalidCastException)
                    {
                    }
                    catch (NullReferenceException)
                    {
                    }

                    throw new ArgumentException();
                }
            }

            int ICollection.Count
            {
                get { return Count; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return this; }
            }

            public Collection()
            {
                items = Empty<T>.Array;
            }

            public Collection(int capacity)
            {
                if (capacity < 0)
                    throw new ArgumentOutOfRangeException();

                items = new T[capacity];
            }

            public Collection(ICollection<T> items)
            {
                this.items = new T[items.Count];
                items.CopyTo(this.items, 0);
                this.size = this.items.Length;
            }

            public void Add(T item)
            {
                if (size == items.Length)
                    Grow(1);

                OnAdd(item, size);

                items[size++] = item;
                version++;
            }

            public bool Contains(T item)
            {
                return IndexOf(item) != -1;
            }

            public int IndexOf(T item)
            {
                return Array.IndexOf(items, item, 0, size);
            }

            public void Insert(int index, T item)
            {
                CheckIndex(index);
                if (size == items.Length)
                    Grow(1);

                OnInsert(item, index);

                Shift(index, 1);
                items[index] = item;
                version++;
            }

            public void RemoveAt(int index)
            {
                if (index < 0 || index >= size)
                    throw new ArgumentOutOfRangeException();

                var item = items[index];

                OnRemove(item, index);

                Shift(index, -1);
                Array.Clear(items, size, 1);
                version++;
            }

            public bool Remove(T item)
            {
                var index = IndexOf(item);
                if (index == -1)
                    return false;

                OnRemove(item, index);

                Shift(index, -1);
                Array.Clear(items, size, 1);
                version++;

                return true;
            }

            public void Clear()
            {
                OnClear();

                Array.Clear(items, 0, size);
                size = 0;
                version++;
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                Array.Copy(items, 0, array, arrayIndex, size);
            }

            public T[] ToArray()
            {
                var array = new T[size];
                Array.Copy(items, 0, array, 0, size);
                return array;
            }

            void CheckIndex(int index)
            {
                if (index < 0 || index > size)
                    throw new ArgumentOutOfRangeException();
            }

            void Shift(int start, int delta)
            {
                if (delta < 0)
                    start -= delta;

                if (start < size)
                    Array.Copy(items, start, items, start + delta, size - start);

                size += delta;

                if (delta < 0)
                    Array.Clear(items, size, -delta);
            }

            protected virtual void OnAdd(T item, int index)
            {
            }

            protected virtual void OnInsert(T item, int index)
            {
            }

            protected virtual void OnSet(T item, int index)
            {
            }

            protected virtual void OnRemove(T item, int index)
            {
            }

            protected virtual void OnClear()
            {
            }

            internal virtual void Grow(int desired)
            {
                int new_size = size + desired;
                if (new_size <= items.Length)
                    return;

                const int default_capacity = 4;

                new_size = System.Math.Max(
                    System.Math.Max(items.Length * 2, default_capacity),
                    new_size);
                Array.Resize(ref items, new_size);
            }

            int IList.Add(object value)
            {
                try
                {
                    Add((T)value);
                    return size - 1;
                }
                catch (InvalidCastException)
                {
                }
                catch (NullReferenceException)
                {
                }

                throw new ArgumentException();
            }

            void IList.Clear()
            {
                Clear();
            }

            bool IList.Contains(object value)
            {
                return ((IList)this).IndexOf(value) > -1;
            }

            int IList.IndexOf(object value)
            {
                try
                {
                    return IndexOf((T)value);
                }
                catch (InvalidCastException)
                {
                }
                catch (NullReferenceException)
                {
                }

                return -1;
            }

            void IList.Insert(int index, object value)
            {
                CheckIndex(index);

                try
                {
                    Insert(index, (T)value);
                    return;
                }
                catch (InvalidCastException)
                {
                }
                catch (NullReferenceException)
                {
                }

                throw new ArgumentException();
            }

            void IList.Remove(object value)
            {
                try
                {
                    Remove((T)value);
                }
                catch (InvalidCastException)
                {
                }
                catch (NullReferenceException)
                {
                }
            }

            void IList.RemoveAt(int index)
            {
                RemoveAt(index);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                Array.Copy(items, 0, array, index, size);
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new Enumerator(this);
            }

            public struct Enumerator : IEnumerator<T>, IDisposable
            {

                Collection<T> collection;
                T current;

                int next;
                readonly int version;

                public T Current
                {
                    get { return current; }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        CheckState();

                        if (next <= 0)
                            throw new InvalidOperationException();

                        return current;
                    }
                }

                internal Enumerator(Collection<T> collection)
                    : this()
                {
                    this.collection = collection;
                    this.version = collection.version;
                }

                public bool MoveNext()
                {
                    CheckState();

                    if (next < 0)
                        return false;

                    if (next < collection.size)
                    {
                        current = collection.items[next++];
                        return true;
                    }

                    next = -1;
                    return false;
                }

                public void Reset()
                {
                    CheckState();

                    next = 0;
                }

                void CheckState()
                {
                    if (collection == null)
                        throw new ObjectDisposedException(GetType().FullName);

                    if (version != collection.version)
                        throw new InvalidOperationException();
                }

                public void Dispose()
                {
                    collection = null;
                }
            }
        }


        public sealed class ReadOnlyCollection<T> : Collection<T>, IList
        {

            static ReadOnlyCollection<T> empty;

            public static ReadOnlyCollection<T> Empty
            {
                get { return empty ?? (empty = new ReadOnlyCollection<T>()); }
            }

            bool IList.IsReadOnly
            {
                get { return true; }
            }

            private ReadOnlyCollection()
            {
            }

            public ReadOnlyCollection(T[] array)
            {
                if (array == null)
                    throw new ArgumentNullException();

                this.items = array;
                this.size = array.Length;
            }

            public ReadOnlyCollection(Collection<T> collection)
            {
                if (collection == null)
                    throw new ArgumentNullException();

                this.items = collection.items;
                this.size = collection.size;
            }

            internal override void Grow(int desired)
            {
                throw new InvalidOperationException();
            }

            protected override void OnAdd(T item, int index)
            {
                throw new InvalidOperationException();
            }

            protected override void OnClear()
            {
                throw new InvalidOperationException();
            }

            protected override void OnInsert(T item, int index)
            {
                throw new InvalidOperationException();
            }

            protected override void OnRemove(T item, int index)
            {
                throw new InvalidOperationException();
            }

            protected override void OnSet(T item, int index)
            {
                throw new InvalidOperationException();
            }
        }
    }
    #endregion

    #region Cecil
    namespace Cecil
    {

        #region Root
        public sealed class AssemblyDefinition : ICustomAttributeProvider, ISecurityDeclarationProvider
        {

            AssemblyNameDefinition name;

            internal ModuleDefinition main_module;
            Collection<ModuleDefinition> modules;
            Collection<CustomAttribute> custom_attributes;
            Collection<SecurityDeclaration> security_declarations;

            public AssemblyNameDefinition Name
            {
                get { return name; }
                set { name = value; }
            }

            public string FullName
            {
                get { return name != null ? name.FullName : string.Empty; }
            }

            public MetadataToken MetadataToken
            {
                get { return new MetadataToken(TokenType.Assembly, 1); }
                set { }
            }

            public Collection<ModuleDefinition> Modules
            {
                get
                {
                    if (modules != null)
                        return modules;

                    if (main_module.HasImage)
                        return modules = main_module.Read(this, (_, reader) => reader.ReadModules());

                    return modules = new Collection<ModuleDefinition> { main_module };
                }
            }

            public ModuleDefinition MainModule
            {
                get { return main_module; }
            }

            public MethodDefinition EntryPoint
            {
                get { return main_module.EntryPoint; }
                set { main_module.EntryPoint = value; }
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(main_module);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(main_module)); }
            }

            public bool HasSecurityDeclarations
            {
                get
                {
                    if (security_declarations != null)
                        return security_declarations.Count > 0;

                    return this.GetHasSecurityDeclarations(main_module);
                }
            }

            public Collection<SecurityDeclaration> SecurityDeclarations
            {
                get { return security_declarations ?? (security_declarations = this.GetSecurityDeclarations(main_module)); }
            }

            internal AssemblyDefinition()
            {
            }

            public static AssemblyDefinition ReadAssembly(string fileName)
            {
                return ReadAssembly(ModuleDefinition.ReadModule(fileName));
            }

            public static AssemblyDefinition ReadAssembly(string fileName, ReaderParameters parameters)
            {
                return ReadAssembly(ModuleDefinition.ReadModule(fileName, parameters));
            }

            static AssemblyDefinition ReadAssembly(ModuleDefinition module)
            {
                var assembly = module.Assembly;
                if (assembly == null)
                    throw new ArgumentException();

                return assembly;
            }

            public override string ToString()
            {
                return this.FullName;
            }
        }


        public struct ArrayDimension
        {

            int? lower_bound;
            int? upper_bound;

            public int? LowerBound
            {
                get { return lower_bound; }
                set { lower_bound = value; }
            }

            public int? UpperBound
            {
                get { return upper_bound; }
                set { upper_bound = value; }
            }

            public bool IsSized
            {
                get { return lower_bound.HasValue || upper_bound.HasValue; }
            }

            public ArrayDimension(int? lowerBound, int? upperBound)
            {
                this.lower_bound = lowerBound;
                this.upper_bound = upperBound;
            }

            public override string ToString()
            {
                return !IsSized
                    ? string.Empty
                    : lower_bound + "..." + upper_bound;
            }
        }

        public sealed class ArrayType : TypeSpecification
        {

            Collection<ArrayDimension> dimensions;

            public Collection<ArrayDimension> Dimensions
            {
                get
                {
                    if (dimensions != null)
                        return dimensions;

                    dimensions = new Collection<ArrayDimension>();
                    dimensions.Add(new ArrayDimension());
                    return dimensions;
                }
            }

            public int Rank
            {
                get { return dimensions == null ? 1 : dimensions.Count; }
            }

            public bool IsVector
            {
                get
                {
                    if (dimensions == null)
                        return true;

                    if (dimensions.Count > 1)
                        return false;

                    var dimension = dimensions[0];

                    return !dimension.IsSized;
                }
            }

            public override bool IsValueType
            {
                get { return false; }
                set { throw new InvalidOperationException(); }
            }

            public override string Name
            {
                get { return base.Name + Suffix; }
            }

            public override string FullName
            {
                get { return base.FullName + Suffix; }
            }

            string Suffix
            {
                get
                {
                    if (IsVector)
                        return "[]";

                    var suffix = new StringBuilder();
                    suffix.Append("[");
                    for (int i = 0; i < dimensions.Count; i++)
                    {
                        if (i > 0)
                            suffix.Append(",");

                        suffix.Append(dimensions[i].ToString());
                    }
                    suffix.Append("]");

                    return suffix.ToString();
                }
            }

            public override bool IsArray
            {
                get { return true; }
            }

            public ArrayType(TypeReference type)
                : base(type)
            {
                Mixin.CheckType(type);
                this.etype = MD.ElementType.Array;
            }

            public ArrayType(TypeReference type, int rank)
                : this(type)
            {
                Mixin.CheckType(type);

                if (rank == 1)
                    return;

                dimensions = new Collection<ArrayDimension>(rank);
                for (int i = 0; i < rank; i++)
                    dimensions.Add(new ArrayDimension());
                this.etype = MD.ElementType.Array;
            }
        }

        [Flags]
        public enum AssemblyAttributes : uint
        {
            PublicKey = 0x0001,
            SideBySideCompatible = 0x0000,
            Retargetable = 0x0100,
            DisableJITCompileOptimizer = 0x4000,
            EnableJITCompileTracking = 0x8000,
        }


        public enum AssemblyHashAlgorithm : uint
        {
            None = 0x0000,
            Reserved = 0x8003,
            SHA1 = 0x8004
        }


        public sealed class AssemblyLinkedResource : Resource
        {

            AssemblyNameReference reference;

            public AssemblyNameReference Assembly
            {
                get { return reference; }
                set { reference = value; }
            }

            public override ResourceType ResourceType
            {
                get { return ResourceType.AssemblyLinked; }
            }

            public AssemblyLinkedResource(string name, ManifestResourceAttributes flags)
                : base(name, flags)
            {
            }

            public AssemblyLinkedResource(string name, ManifestResourceAttributes flags, AssemblyNameReference reference)
                : base(name, flags)
            {
                this.reference = reference;
            }
        }
        public sealed class AssemblyNameDefinition : AssemblyNameReference
        {

            public override byte[] Hash
            {
                get { return Empty<byte>.Array; }
            }

            internal AssemblyNameDefinition()
            {
                this.token = new MetadataToken(TokenType.Assembly, 1);
            }

            public AssemblyNameDefinition(string name, Version version)
                : base(name, version)
            {
                this.token = new MetadataToken(TokenType.Assembly, 1);
            }
        }


        public class AssemblyNameReference : IMetadataScope
        {

            string name;
            string culture;
            Version version;
            uint attributes;
            byte[] public_key;
            byte[] public_key_token;
            AssemblyHashAlgorithm hash_algorithm;
            byte[] hash;

            internal MetadataToken token;

            string full_name;

            public string Name
            {
                get { return name; }
                set
                {
                    name = value;
                    full_name = null;
                }
            }

            public string Culture
            {
                get { return culture; }
                set
                {
                    culture = value;
                    full_name = null;
                }
            }

            public Version Version
            {
                get { return version; }
                set
                {
                    version = value;
                    full_name = null;
                }
            }

            public AssemblyAttributes Attributes
            {
                get { return (AssemblyAttributes)attributes; }
                set { attributes = (uint)value; }
            }

            public bool HasPublicKey
            {
                get { return attributes.GetAttributes((uint)AssemblyAttributes.PublicKey); }
                set { attributes = attributes.SetAttributes((uint)AssemblyAttributes.PublicKey, value); }
            }

            public bool IsSideBySideCompatible
            {
                get { return attributes.GetAttributes((uint)AssemblyAttributes.SideBySideCompatible); }
                set { attributes = attributes.SetAttributes((uint)AssemblyAttributes.SideBySideCompatible, value); }
            }

            public bool IsRetargetable
            {
                get { return attributes.GetAttributes((uint)AssemblyAttributes.Retargetable); }
                set { attributes = attributes.SetAttributes((uint)AssemblyAttributes.Retargetable, value); }
            }

            public byte[] PublicKey
            {
                get { return public_key; }
                set
                {
                    public_key = value;
                    HasPublicKey = !public_key.IsNullOrEmpty();
                    public_key_token = Empty<byte>.Array;
                    full_name = null;
                }
            }

            public byte[] PublicKeyToken
            {
                get
                {
                    if (public_key_token.IsNullOrEmpty() && !public_key.IsNullOrEmpty())
                    {
                        var hash = HashPublicKey();
                        public_key_token = new byte[8];
                        Array.Copy(hash, (hash.Length - 8), public_key_token, 0, 8);
                        Array.Reverse(public_key_token, 0, 8);
                    }
                    return public_key_token;
                }
                set
                {
                    public_key_token = value;
                    full_name = null;
                }
            }

            byte[] HashPublicKey()
            {
                HashAlgorithm algorithm;

                switch (hash_algorithm)
                {
                    case AssemblyHashAlgorithm.Reserved:
                        algorithm = MD5.Create();
                        break;
                    default:
                        algorithm = SHA1.Create();
                        break;
                }

                using (algorithm)
                    return algorithm.ComputeHash(public_key);
            }

            public virtual MetadataScopeType MetadataScopeType
            {
                get { return MetadataScopeType.AssemblyNameReference; }
            }

            public string FullName
            {
                get
                {
                    if (full_name != null)
                        return full_name;

                    const string sep = ", ";

                    var builder = new StringBuilder();
                    builder.Append(name);
                    if (version != null)
                    {
                        builder.Append(sep);
                        builder.Append("Version=");
                        builder.Append(version.ToString());
                    }
                    builder.Append(sep);
                    builder.Append("Culture=");
                    builder.Append(string.IsNullOrEmpty(culture) ? "neutral" : culture);
                    builder.Append(sep);
                    builder.Append("PublicKeyToken=");

                    if (this.PublicKeyToken != null && public_key_token.Length > 0)
                    {
                        for (int i = 0; i < public_key_token.Length; i++)
                        {
                            builder.Append(public_key_token[i].ToString("x2"));
                        }
                    }
                    else
                        builder.Append("null");

                    return full_name = builder.ToString();
                }
            }

            public static AssemblyNameReference Parse(string fullName)
            {
                if (fullName == null)
                    throw new ArgumentNullException("fullName");
                if (fullName.Length == 0)
                    throw new ArgumentException("Name can not be empty");

                var name = new AssemblyNameReference();
                var tokens = fullName.Split(',');
                for (int i = 0; i < tokens.Length; i++)
                {
                    var token = tokens[i].Trim();

                    if (i == 0)
                    {
                        name.Name = token;
                        continue;
                    }

                    var parts = token.Split('=');
                    if (parts.Length != 2)
                        throw new ArgumentException("Malformed name");

                    switch (parts[0])
                    {
                        case "Version":
                            name.Version = new Version(parts[1]);
                            break;
                        case "Culture":
                            name.Culture = parts[1];
                            break;
                        case "PublicKeyToken":
                            string pk_token = parts[1];
                            if (pk_token == "null")
                                break;

                            name.PublicKeyToken = new byte[pk_token.Length / 2];
                            for (int j = 0; j < name.PublicKeyToken.Length; j++)
                            {
                                name.PublicKeyToken[j] = Byte.Parse(pk_token.Substring(j * 2, 2), NumberStyles.HexNumber);
                            }
                            break;
                    }
                }

                return name;
            }

            public AssemblyHashAlgorithm HashAlgorithm
            {
                get { return hash_algorithm; }
                set { hash_algorithm = value; }
            }

            public virtual byte[] Hash
            {
                get { return hash; }
                set { hash = value; }
            }

            public MetadataToken MetadataToken
            {
                get { return token; }
                set { token = value; }
            }

            internal AssemblyNameReference()
            {
            }

            public AssemblyNameReference(string name, Version version)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                this.name = name;
                this.version = version;
                this.hash_algorithm = AssemblyHashAlgorithm.None;
                this.token = new MetadataToken(TokenType.AssemblyRef);
            }

            public override string ToString()
            {
                return this.FullName;
            }
        }


        abstract class ModuleReader
        {

            readonly protected Image image;
            readonly protected ModuleDefinition module;

            protected ModuleReader(Image image, ReadingMode mode)
            {
                this.image = image;
                this.module = new ModuleDefinition(image);
                this.module.ReadingMode = mode;
            }

            protected abstract void ReadModule();

            protected void ReadModuleManifest(MetadataReader reader)
            {
                reader.Populate(module);

                ReadAssembly(reader);
            }

            void ReadAssembly(MetadataReader reader)
            {
                var name = reader.ReadAssemblyNameDefinition();
                if (name == null)
                {
                    module.kind = ModuleKind.NetModule;
                    return;
                }

                var assembly = new AssemblyDefinition();
                assembly.Name = name;

                module.assembly = assembly;
                assembly.main_module = module;
            }

            public static ModuleDefinition CreateModuleFrom(Image image, ReaderParameters parameters)
            {
                var module = ReadModule(image, parameters);

                ReadSymbols(module, parameters);

                if (parameters.AssemblyResolver != null)
                    module.assembly_resolver = parameters.AssemblyResolver;

                return module;
            }

            static void ReadSymbols(ModuleDefinition module, ReaderParameters parameters)
            {
                var symbol_reader_provider = parameters.SymbolReaderProvider;

                if (symbol_reader_provider == null && parameters.ReadSymbols)
                    symbol_reader_provider = SymbolProvider.GetPlatformReaderProvider();

                if (symbol_reader_provider != null)
                {
                    module.SymbolReaderProvider = symbol_reader_provider;

                    var reader = parameters.SymbolStream != null
                        ? symbol_reader_provider.GetSymbolReader(module, parameters.SymbolStream)
                        : symbol_reader_provider.GetSymbolReader(module, module.FullyQualifiedName);

                    module.ReadSymbols(reader);
                }
            }

            static ModuleDefinition ReadModule(Image image, ReaderParameters parameters)
            {
                var reader = CreateModuleReader(image, parameters.ReadingMode);
                reader.ReadModule();
                return reader.module;
            }

            static ModuleReader CreateModuleReader(Image image, ReadingMode mode)
            {
                switch (mode)
                {
                    case ReadingMode.Immediate:
                        return new ImmediateModuleReader(image);
                    case ReadingMode.Deferred:
                        return new DeferredModuleReader(image);
                    default:
                        throw new ArgumentException();
                }
            }
        }

        sealed class ImmediateModuleReader : ModuleReader
        {

            public ImmediateModuleReader(Image image)
                : base(image, ReadingMode.Immediate)
            {
            }

            protected override void ReadModule()
            {
                this.module.Read(this.module, (module, reader) =>
                {
                    ReadModuleManifest(reader);
                    ReadModule(module);
                    return module;
                });
            }

            public static void ReadModule(ModuleDefinition module)
            {
                if (module.HasAssemblyReferences)
                    Read(module.AssemblyReferences);
                if (module.HasResources)
                    Read(module.Resources);
                if (module.HasModuleReferences)
                    Read(module.ModuleReferences);
                if (module.HasTypes)
                    ReadTypes(module.Types);
                if (module.HasExportedTypes)
                    Read(module.ExportedTypes);
                if (module.HasCustomAttributes)
                    Read(module.CustomAttributes);

                var assembly = module.Assembly;
                if (assembly == null)
                    return;

                if (assembly.HasCustomAttributes)
                    Read(assembly.CustomAttributes);
                if (assembly.HasSecurityDeclarations)
                    Read(assembly.SecurityDeclarations);
            }

            static void ReadTypes(Collection<TypeDefinition> types)
            {
                for (int i = 0; i < types.Count; i++)
                    ReadType(types[i]);
            }

            static void ReadType(TypeDefinition type)
            {
                ReadGenericParameters(type);

                if (type.HasInterfaces)
                    Read(type.Interfaces);

                if (type.HasNestedTypes)
                    ReadTypes(type.NestedTypes);

                if (type.HasLayoutInfo)
                    Read(type.ClassSize);

                if (type.HasFields)
                    ReadFields(type);

                if (type.HasMethods)
                    ReadMethods(type);

                if (type.HasProperties)
                    ReadProperties(type);

                if (type.HasEvents)
                    ReadEvents(type);

                ReadSecurityDeclarations(type);
                ReadCustomAttributes(type);
            }

            static void ReadGenericParameters(IGenericParameterProvider provider)
            {
                if (!provider.HasGenericParameters)
                    return;

                var parameters = provider.GenericParameters;

                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];

                    if (parameter.HasConstraints)
                        Read(parameter.Constraints);

                    if (parameter.HasCustomAttributes)
                        Read(parameter.CustomAttributes);
                }
            }

            static void ReadSecurityDeclarations(ISecurityDeclarationProvider provider)
            {
                if (provider.HasSecurityDeclarations)
                    Read(provider.SecurityDeclarations);
            }

            static void ReadCustomAttributes(ICustomAttributeProvider provider)
            {
                if (provider.HasCustomAttributes)
                    Read(provider.CustomAttributes);
            }

            static void ReadFields(TypeDefinition type)
            {
                var fields = type.Fields;

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];

                    if (field.HasConstant)
                        Read(field.Constant);

                    if (field.HasLayoutInfo)
                        Read(field.Offset);

                    if (field.RVA > 0)
                        Read(field.InitialValue);

                    if (field.HasMarshalInfo)
                        Read(field.MarshalInfo);

                    ReadCustomAttributes(field);
                }
            }

            static void ReadMethods(TypeDefinition type)
            {
                var methods = type.Methods;

                for (int i = 0; i < methods.Count; i++)
                {
                    var method = methods[i];

                    ReadGenericParameters(method);

                    if (method.HasParameters)
                        ReadParameters(method);

                    if (method.HasOverrides)
                        Read(method.Overrides);

                    if (method.IsPInvokeImpl)
                        Read(method.PInvokeInfo);

                    ReadSecurityDeclarations(method);
                    ReadCustomAttributes(method);

                    var return_type = method.MethodReturnType;
                    if (return_type.HasConstant)
                        Read(return_type.Constant);

                    if (return_type.HasMarshalInfo)
                        Read(return_type.MarshalInfo);

                    ReadCustomAttributes(return_type);
                }
            }

            static void ReadParameters(MethodDefinition method)
            {
                var parameters = method.Parameters;

                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];

                    if (parameter.HasConstant)
                        Read(parameter.Constant);

                    if (parameter.HasMarshalInfo)
                        Read(parameter.MarshalInfo);

                    ReadCustomAttributes(parameter);
                }
            }

            static void ReadProperties(TypeDefinition type)
            {
                var properties = type.Properties;

                for (int i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];

                    Read(property.GetMethod);

                    if (property.HasConstant)
                        Read(property.Constant);

                    ReadCustomAttributes(property);
                }
            }

            static void ReadEvents(TypeDefinition type)
            {
                var events = type.Events;

                for (int i = 0; i < events.Count; i++)
                {
                    var @event = events[i];

                    Read(@event.AddMethod);

                    ReadCustomAttributes(@event);
                }
            }

            static void Read(object collection)
            {
            }
        }

        sealed class DeferredModuleReader : ModuleReader
        {

            public DeferredModuleReader(Image image)
                : base(image, ReadingMode.Deferred)
            {
            }

            protected override void ReadModule()
            {
                this.module.Read(this.module, (module, reader) =>
                {
                    ReadModuleManifest(reader);
                    return module;
                });
            }
        }

        sealed class MetadataReader : ByteBuffer
        {

            readonly internal Image image;
            readonly internal ModuleDefinition module;
            readonly internal MetadataSystem metadata;

            internal IGenericContext context;
            internal CodeReader code;

            uint Position
            {
                get { return (uint)base.position; }
                set { base.position = (int)value; }
            }

            public MetadataReader(ModuleDefinition module)
                : base(module.Image.MetadataSection.Data)
            {
                this.image = module.Image;
                this.module = module;
                this.metadata = module.MetadataSystem;
                this.code = CodeReader.CreateCodeReader(this);
            }

            int GetCodedIndexSize(CodedIndex index)
            {
                return image.GetCodedIndexSize(index);
            }

            uint ReadByIndexSize(int size)
            {
                if (size == 4)
                    return ReadUInt32();
                else
                    return ReadUInt16();
            }

            byte[] ReadBlob()
            {
                var blob_heap = image.BlobHeap;
                if (blob_heap == null)
                {
                    position += 2;
                    return Empty<byte>.Array;
                }

                return blob_heap.Read(ReadBlobIndex());
            }

            byte[] ReadBlob(uint signature)
            {
                var blob_heap = image.BlobHeap;
                if (blob_heap == null)
                    return Empty<byte>.Array;

                return blob_heap.Read(signature);
            }

            uint ReadBlobIndex()
            {
                var blob_heap = image.BlobHeap;
                return ReadByIndexSize(blob_heap != null ? blob_heap.IndexSize : 2);
            }

            string ReadString()
            {
                return image.StringHeap.Read(ReadByIndexSize(image.StringHeap.IndexSize));
            }

            uint ReadStringIndex()
            {
                return ReadByIndexSize(image.StringHeap.IndexSize);
            }

            uint ReadTableIndex(Table table)
            {
                return ReadByIndexSize(image.GetTableIndexSize(table));
            }

            MetadataToken ReadMetadataToken(CodedIndex index)
            {
                return index.GetMetadataToken(ReadByIndexSize(GetCodedIndexSize(index)));
            }

            int MoveTo(Table table)
            {
                var info = image.TableHeap[table];
                if (info.Length != 0)
                    Position = info.Offset;

                return (int)info.Length;
            }

            bool MoveTo(Table table, uint row)
            {
                var info = image.TableHeap[table];
                var length = info.Length;
                if (length == 0 || row > length)
                    return false;

                Position = info.Offset + (info.RowSize * (row - 1));
                return true;
            }

            public AssemblyNameDefinition ReadAssemblyNameDefinition()
            {
                if (MoveTo(Table.Assembly) == 0)
                    return null;

                var name = new AssemblyNameDefinition();

                name.HashAlgorithm = (AssemblyHashAlgorithm)ReadUInt32();

                PopulateVersionAndFlags(name);

                name.PublicKey = ReadBlob();

                PopulateNameAndCulture(name);

                return name;
            }

            public ModuleDefinition Populate(ModuleDefinition module)
            {
                if (MoveTo(Table.Module) == 0)
                    return module;

                Advance(2);

                module.Name = ReadString();
                module.Mvid = image.GuidHeap.Read(ReadByIndexSize(image.GuidHeap.IndexSize));

                return module;
            }

            public Collection<AssemblyNameReference> ReadAssemblyReferences()
            {
                int length = MoveTo(Table.AssemblyRef);
                var references = new Collection<AssemblyNameReference>(length);

                for (uint i = 1; i <= length; i++)
                {
                    var reference = new AssemblyNameReference();
                    reference.token = new MetadataToken(TokenType.AssemblyRef, i);

                    PopulateVersionAndFlags(reference);

                    reference.PublicKeyToken = ReadBlob();

                    PopulateNameAndCulture(reference);

                    reference.Hash = ReadBlob();

                    references.Add(reference);
                }

                return references;
            }

            public MethodDefinition ReadEntryPoint()
            {
                if (module.Kind != ModuleKind.Console && module.Kind != ModuleKind.Windows)
                    return null;

                var token = new MetadataToken(module.Image.EntryPointToken);

                return GetMethodDefinition(token.RID);
            }

            public Collection<ModuleDefinition> ReadModules()
            {
                var modules = new Collection<ModuleDefinition>(1);
                modules.Add(this.module);

                int length = MoveTo(Table.File);
                for (uint i = 1; i <= length; i++)
                {
                    var attributes = (FileAttributes)ReadUInt32();
                    var name = ReadString();
                    ReadBlobIndex();

                    if (attributes != FileAttributes.ContainsMetaData)
                        continue;

                    var parameters = new ReaderParameters
                    {
                        ReadingMode = module.ReadingMode,
                        SymbolReaderProvider = module.SymbolReaderProvider,
                    };

                    modules.Add(ModuleDefinition.ReadModule(
                        GetModuleFileName(name), parameters));
                }

                return modules;
            }

            string GetModuleFileName(string name)
            {
                if (module.FullyQualifiedName == null)
                    throw new NotSupportedException();

                var path = Path.GetDirectoryName(module.FullyQualifiedName);
                return Path.Combine(path, name);
            }

            public Collection<ModuleReference> ReadModuleReferences()
            {
                int length = MoveTo(Table.ModuleRef);
                var references = new Collection<ModuleReference>(length);

                for (uint i = 1; i <= length; i++)
                {
                    var reference = new ModuleReference(ReadString());
                    reference.token = new MetadataToken(TokenType.ModuleRef, i);

                    references.Add(reference);
                }

                return references;
            }

            public bool HasFileResource()
            {
                int length = MoveTo(Table.File);
                if (length == 0)
                    return false;

                for (uint i = 1; i <= length; i++)
                    if (ReadFileRecord(i).Col1 == FileAttributes.ContainsNoMetaData)
                        return true;

                return false;
            }

            public Collection<Resource> ReadResources()
            {
                int length = MoveTo(Table.ManifestResource);
                var resources = new Collection<Resource>(length);

                for (int i = 1; i <= length; i++)
                {
                    var offset = ReadUInt32();
                    var flags = (ManifestResourceAttributes)ReadUInt32();
                    var name = ReadString();
                    var implementation = ReadMetadataToken(CodedIndex.Implementation);

                    Resource resource;

                    if (implementation.RID == 0)
                    {
                        resource = new EmbeddedResource(name, flags, offset, this);
                    }
                    else if (implementation.TokenType == TokenType.AssemblyRef)
                    {
                        resource = new AssemblyLinkedResource(name, flags)
                        {
                            Assembly = (AssemblyNameReference)GetTypeReferenceScope(implementation),
                        };
                    }
                    else if (implementation.TokenType == TokenType.File)
                    {
                        var file_record = ReadFileRecord(implementation.RID);

                        resource = new LinkedResource(name, flags)
                        {
                            File = file_record.Col2,
                            hash = ReadBlob(file_record.Col3)
                        };
                    }
                    else
                        throw new NotSupportedException();

                    resources.Add(resource);
                }

                return resources;
            }

            Row<FileAttributes, string, uint> ReadFileRecord(uint rid)
            {
                var position = this.position;

                if (!MoveTo(Table.File, rid))
                    throw new ArgumentException();

                var record = new Row<FileAttributes, string, uint>(
                    (FileAttributes)ReadUInt32(),
                    ReadString(),
                    ReadBlobIndex());

                this.position = position;

                return record;
            }

            public MemoryStream GetManagedResourceStream(uint offset)
            {
                var rva = image.Resources.VirtualAddress;
                var section = image.GetSectionAtVirtualAddress(rva);
                var position = (rva - section.VirtualAddress) + offset;
                var buffer = section.Data;

                var length = buffer[position]
                    | (buffer[position + 1] << 8)
                    | (buffer[position + 2] << 16)
                    | (buffer[position + 3] << 24);

                return new MemoryStream(buffer, (int)position + 4, length);
            }

            void PopulateVersionAndFlags(AssemblyNameReference name)
            {
                name.Version = new Version(
                    ReadUInt16(),
                    ReadUInt16(),
                    ReadUInt16(),
                    ReadUInt16());

                name.Attributes = (AssemblyAttributes)ReadUInt32();
            }

            void PopulateNameAndCulture(AssemblyNameReference name)
            {
                name.Name = ReadString();
                name.Culture = ReadString();
            }

            public TypeDefinitionCollection ReadTypes()
            {
                InitializeTypeDefinitions();
                var mtypes = metadata.Types;
                var type_count = mtypes.Length - metadata.NestedTypes.Count;
                var types = new TypeDefinitionCollection(module, type_count);

                for (int i = 0; i < mtypes.Length; i++)
                {
                    var type = mtypes[i];
                    if (IsNested(type.Attributes))
                        continue;

                    types.Add(type);
                }

                return types;
            }

            void InitializeTypeDefinitions()
            {
                if (metadata.Types != null)
                    return;

                InitializeNestedTypes();
                InitializeFields();
                InitializeMethods();

                int length = MoveTo(Table.TypeDef);
                var types = metadata.Types = new TypeDefinition[length];

                for (uint i = 0; i < length; i++)
                {
                    if (types[i] != null)
                        continue;

                    types[i] = ReadType(i + 1);
                }
            }

            static bool IsNested(TypeAttributes attributes)
            {
                switch (attributes & TypeAttributes.VisibilityMask)
                {
                    case TypeAttributes.NestedAssembly:
                    case TypeAttributes.NestedFamANDAssem:
                    case TypeAttributes.NestedFamily:
                    case TypeAttributes.NestedFamORAssem:
                    case TypeAttributes.NestedPrivate:
                    case TypeAttributes.NestedPublic:
                        return true;
                    default:
                        return false;
                }
            }

            public bool HasNestedTypes(TypeDefinition type)
            {
                uint[] mapping;
                InitializeNestedTypes();

                if (!metadata.TryGetNestedTypeMapping(type, out mapping))
                    return false;

                return mapping.Length > 0;
            }

            public Collection<TypeDefinition> ReadNestedTypes(TypeDefinition type)
            {
                InitializeNestedTypes();
                uint[] mapping;
                if (!metadata.TryGetNestedTypeMapping(type, out mapping))
                    return new MemberDefinitionCollection<TypeDefinition>(type);

                var nested_types = new MemberDefinitionCollection<TypeDefinition>(type, mapping.Length);

                for (int i = 0; i < mapping.Length; i++)
                    nested_types.Add(GetTypeDefinition(mapping[i]));

                metadata.RemoveNestedTypeMapping(type);

                return nested_types;
            }

            void InitializeNestedTypes()
            {
                if (metadata.NestedTypes != null)
                    return;

                var length = MoveTo(Table.NestedClass);

                metadata.NestedTypes = new Dictionary<uint, uint[]>(length);
                metadata.ReverseNestedTypes = new Dictionary<uint, uint>(length);

                if (length == 0)
                    return;

                for (int i = 1; i <= length; i++)
                {
                    var nested = ReadTableIndex(Table.TypeDef);
                    var declaring = ReadTableIndex(Table.TypeDef);

                    AddNestedMapping(declaring, nested);
                }
            }

            void AddNestedMapping(uint declaring, uint nested)
            {
                metadata.SetNestedTypeMapping(declaring, AddMapping(metadata.NestedTypes, declaring, nested));
                metadata.SetReverseNestedTypeMapping(nested, declaring);
            }

            static TValue[] AddMapping<TKey, TValue>(Dictionary<TKey, TValue[]> cache, TKey key, TValue value)
            {
                TValue[] mapped;
                if (!cache.TryGetValue(key, out mapped))
                {
                    mapped = new[] { value };
                    return mapped;
                }

                var new_mapped = new TValue[mapped.Length + 1];
                Array.Copy(mapped, new_mapped, mapped.Length);
                new_mapped[mapped.Length] = value;
                return new_mapped;
            }

            TypeDefinition ReadType(uint rid)
            {
                if (!MoveTo(Table.TypeDef, rid))
                    return null;

                var attributes = (TypeAttributes)ReadUInt32();
                var name = ReadString();
                var @namespace = ReadString();
                var type = new TypeDefinition(@namespace, name, attributes);
                type.token = new MetadataToken(TokenType.TypeDef, rid);
                type.scope = module;
                type.module = module;

                metadata.AddTypeDefinition(type);

                this.context = type;

                type.BaseType = GetTypeDefOrRef(ReadMetadataToken(CodedIndex.TypeDefOrRef));

                type.fields_range = ReadFieldsRange(rid);
                type.methods_range = ReadMethodsRange(rid);

                if (IsNested(attributes))
                    type.DeclaringType = GetNestedTypeDeclaringType(type);

                return type;
            }

            TypeDefinition GetNestedTypeDeclaringType(TypeDefinition type)
            {
                uint declaring_rid;
                if (!metadata.TryGetReverseNestedTypeMapping(type, out declaring_rid))
                    return null;

                metadata.RemoveReverseNestedTypeMapping(type);
                return GetTypeDefinition(declaring_rid);
            }

            Range ReadFieldsRange(uint type_index)
            {
                return ReadListRange(type_index, Table.TypeDef, Table.Field);
            }

            Range ReadMethodsRange(uint type_index)
            {
                return ReadListRange(type_index, Table.TypeDef, Table.Method);
            }

            Range ReadListRange(uint current_index, Table current, Table target)
            {
                var list = new Range();

                list.Start = ReadTableIndex(target);

                uint next_index;
                var current_table = image.TableHeap[current];

                if (current_index == current_table.Length)
                    next_index = image.TableHeap[target].Length + 1;
                else
                {
                    var position = Position;
                    Position += (uint)(current_table.RowSize - image.GetTableIndexSize(target));
                    next_index = ReadTableIndex(target);
                    Position = position;
                }

                list.Length = next_index - list.Start;

                return list;
            }

            public Row<short, int> ReadTypeLayout(TypeDefinition type)
            {
                InitializeTypeLayouts();
                Row<ushort, uint> class_layout;
                var rid = type.token.RID;
                if (!metadata.ClassLayouts.TryGetValue(rid, out class_layout))
                    return new Row<short, int>(Mixin.NoDataMarker, Mixin.NoDataMarker);

                type.PackingSize = (short)class_layout.Col1;
                type.ClassSize = (int)class_layout.Col2;

                metadata.ClassLayouts.Remove(rid);

                return new Row<short, int>((short)class_layout.Col1, (int)class_layout.Col2);
            }

            void InitializeTypeLayouts()
            {
                if (metadata.ClassLayouts != null)
                    return;

                int length = MoveTo(Table.ClassLayout);

                var class_layouts = metadata.ClassLayouts = new Dictionary<uint, Row<ushort, uint>>(length);

                for (uint i = 0; i < length; i++)
                {
                    var packing_size = ReadUInt16();
                    var class_size = ReadUInt32();

                    var parent = ReadTableIndex(Table.TypeDef);

                    class_layouts.Add(parent, new Row<ushort, uint>(packing_size, class_size));
                }
            }

            public TypeReference GetTypeDefOrRef(MetadataToken token)
            {
                return (TypeReference)LookupToken(token);
            }

            public TypeDefinition GetTypeDefinition(uint rid)
            {
                InitializeTypeDefinitions();

                var type = metadata.GetTypeDefinition(rid);
                if (type != null)
                    return type;

                return ReadTypeDefinition(rid);
            }

            TypeDefinition ReadTypeDefinition(uint rid)
            {
                if (!MoveTo(Table.TypeDef, rid))
                    return null;

                return ReadType(rid);
            }

            void InitializeTypeReferences()
            {
                if (metadata.TypeReferences != null)
                    return;

                metadata.TypeReferences = new TypeReference[image.GetTableLength(Table.TypeRef)];
            }

            public TypeReference GetTypeReference(string scope, string full_name)
            {
                InitializeTypeReferences();

                var length = metadata.TypeReferences.Length;

                for (uint i = 1; i <= length; i++)
                {
                    var type = GetTypeReference(i);

                    if (type.FullName != full_name)
                        continue;

                    if (string.IsNullOrEmpty(scope))
                        return type;

                    if (type.Scope.Name == scope)
                        return type;
                }

                return null;
            }

            TypeReference GetTypeReference(uint rid)
            {
                InitializeTypeReferences();

                var type = metadata.GetTypeReference(rid);
                if (type != null)
                    return type;

                return ReadTypeReference(rid);
            }

            TypeReference ReadTypeReference(uint rid)
            {
                if (!MoveTo(Table.TypeRef, rid))
                    return null;

                TypeReference declaring_type = null;
                IMetadataScope scope;

                var scope_token = ReadMetadataToken(CodedIndex.ResolutionScope);

                var name = ReadString();
                var @namespace = ReadString();

                var type = new TypeReference(
                    @namespace,
                    name,
                    module,
                    null);

                type.token = new MetadataToken(TokenType.TypeRef, rid);

                metadata.AddTypeReference(type);

                if (scope_token.TokenType == TokenType.TypeRef)
                {
                    declaring_type = GetTypeDefOrRef(scope_token);

                    scope = declaring_type != null
                        ? declaring_type.Scope
                        : module;
                }
                else
                    scope = GetTypeReferenceScope(scope_token);

                type.scope = scope;
                type.DeclaringType = declaring_type;

                MetadataSystem.TryProcessPrimitiveType(type);

                return type;
            }

            IMetadataScope GetTypeReferenceScope(MetadataToken scope)
            {
                switch (scope.TokenType)
                {
                    case TokenType.AssemblyRef:
                        return module.AssemblyReferences[(int)scope.RID - 1];
                    case TokenType.ModuleRef:
                        return module.ModuleReferences[(int)scope.RID - 1];
                    case TokenType.Module:
                        return module;
                    default:
                        throw new NotSupportedException();
                }
            }

            public IEnumerable<TypeReference> GetTypeReferences()
            {
                InitializeTypeReferences();

                var length = image.GetTableLength(Table.TypeRef);

                var type_references = new TypeReference[length];

                for (uint i = 1; i <= length; i++)
                    type_references[i - 1] = GetTypeReference(i);

                return type_references;
            }

            TypeReference GetTypeSpecification(uint rid)
            {
                if (!MoveTo(Table.TypeSpec, rid))
                    return null;

                var reader = ReadSignature(ReadBlobIndex());
                var type = reader.ReadTypeSignature();
                if (type.token.RID == 0)
                    type.token = new MetadataToken(TokenType.TypeSpec, rid);

                return type;
            }

            SignatureReader ReadSignature(uint signature)
            {
                return new SignatureReader(signature, this);
            }

            public bool HasInterfaces(TypeDefinition type)
            {
                InitializeInterfaces();
                MetadataToken[] mapping;

                return metadata.TryGetInterfaceMapping(type, out mapping);
            }

            public Collection<TypeReference> ReadInterfaces(TypeDefinition type)
            {
                InitializeInterfaces();
                MetadataToken[] mapping;

                if (!metadata.TryGetInterfaceMapping(type, out mapping))
                    return new Collection<TypeReference>();

                var interfaces = new Collection<TypeReference>(mapping.Length);

                this.context = type;

                for (int i = 0; i < mapping.Length; i++)
                    interfaces.Add(GetTypeDefOrRef(mapping[i]));

                metadata.RemoveInterfaceMapping(type);

                return interfaces;
            }

            void InitializeInterfaces()
            {
                if (metadata.Interfaces != null)
                    return;

                int length = MoveTo(Table.InterfaceImpl);

                metadata.Interfaces = new Dictionary<uint, MetadataToken[]>(length);

                for (int i = 0; i < length; i++)
                {
                    var type = ReadTableIndex(Table.TypeDef);
                    var @interface = ReadMetadataToken(CodedIndex.TypeDefOrRef);

                    AddInterfaceMapping(type, @interface);
                }
            }

            void AddInterfaceMapping(uint type, MetadataToken @interface)
            {
                metadata.SetInterfaceMapping(type, AddMapping(metadata.Interfaces, type, @interface));
            }

            public Collection<FieldDefinition> ReadFields(TypeDefinition type)
            {
                var fields_range = type.fields_range;
                if (fields_range.Length == 0)
                    return new MemberDefinitionCollection<FieldDefinition>(type);

                var fields = new MemberDefinitionCollection<FieldDefinition>(type, (int)fields_range.Length);
                this.context = type;

                MoveTo(Table.Field, fields_range.Start);
                for (uint i = 0; i < fields_range.Length; i++)
                    fields.Add(ReadField(fields_range.Start + i));

                return fields;
            }

            FieldDefinition ReadField(uint field_rid)
            {
                var attributes = (FieldAttributes)ReadUInt16();
                var name = ReadString();
                var signature = ReadBlobIndex();

                var field = new FieldDefinition(name, attributes, ReadFieldType(signature));
                field.token = new MetadataToken(TokenType.Field, field_rid);
                metadata.AddFieldDefinition(field);

                return field;
            }

            void InitializeFields()
            {
                if (metadata.Fields != null)
                    return;

                metadata.Fields = new FieldDefinition[image.GetTableLength(Table.Field)];
            }

            TypeReference ReadFieldType(uint signature)
            {
                var reader = ReadSignature(signature);

                const byte field_sig = 0x6;

                if (reader.ReadByte() != field_sig)
                    throw new NotSupportedException();

                return reader.ReadTypeSignature();
            }

            public int ReadFieldRVA(FieldDefinition field)
            {
                InitializeFieldRVAs();
                var rid = field.token.RID;

                RVA rva;
                if (!metadata.FieldRVAs.TryGetValue(rid, out rva))
                    return 0;

                var size = GetFieldTypeSize(field.FieldType);

                if (size == 0 || rva == 0)
                    return 0;

                metadata.FieldRVAs.Remove(rid);

                field.InitialValue = GetFieldInitializeValue(size, rva);

                return (int)rva;
            }

            byte[] GetFieldInitializeValue(int size, RVA rva)
            {
                var section = image.GetSectionAtVirtualAddress(rva);
                if (section == null)
                    return Empty<byte>.Array;

                var value = new byte[size];
                Buffer.BlockCopy(section.Data, (int)(rva - section.VirtualAddress), value, 0, size);
                return value;
            }

            static int GetFieldTypeSize(TypeReference type)
            {
                int size = 0;

                switch (type.etype)
                {
                    case ElementType.Boolean:
                    case ElementType.U1:
                    case ElementType.I1:
                        size = 1;
                        break;
                    case ElementType.U2:
                    case ElementType.I2:
                    case ElementType.Char:
                        size = 2;
                        break;
                    case ElementType.U4:
                    case ElementType.I4:
                    case ElementType.R4:
                        size = 4;
                        break;
                    case ElementType.U8:
                    case ElementType.I8:
                    case ElementType.R8:
                        size = 8;
                        break;
                    case ElementType.Ptr:
                    case ElementType.FnPtr:
                        size = IntPtr.Size;
                        break;
                    case ElementType.CModOpt:
                    case ElementType.CModReqD:
                        return GetFieldTypeSize(((IModifierType)type).ElementType);
                    default:
                        var field_type = type.CheckedResolve();
                        if (field_type.HasLayoutInfo)
                            size = field_type.ClassSize;

                        break;
                }

                return size;
            }

            void InitializeFieldRVAs()
            {
                if (metadata.FieldRVAs != null)
                    return;

                int length = MoveTo(Table.FieldRVA);

                var field_rvas = metadata.FieldRVAs = new Dictionary<uint, uint>(length);

                for (int i = 0; i < length; i++)
                {
                    var rva = ReadUInt32();
                    var field = ReadTableIndex(Table.Field);

                    field_rvas.Add(field, rva);
                }
            }

            public int ReadFieldLayout(FieldDefinition field)
            {
                InitializeFieldLayouts();
                var rid = field.token.RID;
                uint offset;
                if (!metadata.FieldLayouts.TryGetValue(rid, out offset))
                    return Mixin.NoDataMarker;

                metadata.FieldLayouts.Remove(rid);

                return (int)offset;
            }

            void InitializeFieldLayouts()
            {
                if (metadata.FieldLayouts != null)
                    return;

                int length = MoveTo(Table.FieldLayout);

                var field_layouts = metadata.FieldLayouts = new Dictionary<uint, uint>(length);

                for (int i = 0; i < length; i++)
                {
                    var offset = ReadUInt32();
                    var field = ReadTableIndex(Table.Field);

                    field_layouts.Add(field, offset);
                }
            }

            public bool HasEvents(TypeDefinition type)
            {
                InitializeEvents();

                Range range;
                if (!metadata.TryGetEventsRange(type, out range))
                    return false;

                return range.Length > 0;
            }

            public Collection<EventDefinition> ReadEvents(TypeDefinition type)
            {
                InitializeEvents();
                Range range;

                if (!metadata.TryGetEventsRange(type, out range))
                    return new MemberDefinitionCollection<EventDefinition>(type);

                var events = new MemberDefinitionCollection<EventDefinition>(type, (int)range.Length);

                metadata.RemoveEventsRange(type);

                if (range.Length == 0 || !MoveTo(Table.Event, range.Start))
                    return events;

                this.context = type;

                for (uint i = 0; i < range.Length; i++)
                    events.Add(ReadEvent(range.Start + i));

                return events;
            }

            EventDefinition ReadEvent(uint event_rid)
            {
                var attributes = (EventAttributes)ReadUInt16();
                var name = ReadString();
                var event_type = GetTypeDefOrRef(ReadMetadataToken(CodedIndex.TypeDefOrRef));

                var @event = new EventDefinition(name, attributes, event_type);
                @event.token = new MetadataToken(TokenType.Event, event_rid);
                return @event;
            }

            void InitializeEvents()
            {
                if (metadata.Events != null)
                    return;

                int length = MoveTo(Table.EventMap);

                metadata.Events = new Dictionary<uint, Range>(length);

                for (uint i = 1; i <= length; i++)
                {
                    var type_rid = ReadTableIndex(Table.TypeDef);
                    Range events_range = ReadEventsRange(i);
                    metadata.AddEventsRange(type_rid, events_range);
                }
            }

            Range ReadEventsRange(uint rid)
            {
                return ReadListRange(rid, Table.EventMap, Table.Event);
            }

            public bool HasProperties(TypeDefinition type)
            {
                InitializeProperties();

                Range range;
                if (!metadata.TryGetPropertiesRange(type, out range))
                    return false;

                return range.Length > 0;
            }

            public Collection<PropertyDefinition> ReadProperties(TypeDefinition type)
            {
                InitializeProperties();

                Range range;

                if (!metadata.TryGetPropertiesRange(type, out range))
                    return new MemberDefinitionCollection<PropertyDefinition>(type);

                metadata.RemovePropertiesRange(type);

                var properties = new MemberDefinitionCollection<PropertyDefinition>(type, (int)range.Length);

                if (range.Length == 0 || !MoveTo(Table.Property, range.Start))
                    return properties;

                this.context = type;

                for (uint i = 0; i < range.Length; i++)
                    properties.Add(ReadProperty(range.Start + i));

                return properties;
            }

            PropertyDefinition ReadProperty(uint property_rid)
            {
                var attributes = (PropertyAttributes)ReadUInt16();
                var name = ReadString();
                var signature = ReadBlobIndex();

                var reader = ReadSignature(signature);
                const byte property_signature = 0x8;

                var calling_convention = reader.ReadByte();

                if ((calling_convention & property_signature) == 0)
                    throw new NotSupportedException();

                var has_this = (calling_convention & 0x20) != 0;

                reader.ReadCompressedUInt32(); // count

                var property = new PropertyDefinition(name, attributes, reader.ReadTypeSignature());
                property.HasThis = has_this;
                property.token = new MetadataToken(TokenType.Property, property_rid);

                return property;
            }

            void InitializeProperties()
            {
                if (metadata.Properties != null)
                    return;

                int length = MoveTo(Table.PropertyMap);

                metadata.Properties = new Dictionary<uint, Range>(length);

                for (uint i = 1; i <= length; i++)
                {
                    var type_rid = ReadTableIndex(Table.TypeDef);
                    var properties_range = ReadPropertiesRange(i);
                    metadata.AddPropertiesRange(type_rid, properties_range);
                }
            }

            Range ReadPropertiesRange(uint rid)
            {
                return ReadListRange(rid, Table.PropertyMap, Table.Property);
            }

            MethodSemanticsAttributes ReadMethodSemantics(MethodDefinition method)
            {
                InitializeMethodSemantics();
                Row<MethodSemanticsAttributes, MetadataToken> row;
                if (!metadata.Semantics.TryGetValue(method.token.RID, out row))
                    return MethodSemanticsAttributes.None;

                var type = method.DeclaringType;

                switch (row.Col1)
                {
                    case MethodSemanticsAttributes.AddOn:
                        GetEvent(type, row.Col2).add_method = method;
                        break;
                    case MethodSemanticsAttributes.Fire:
                        GetEvent(type, row.Col2).invoke_method = method;
                        break;
                    case MethodSemanticsAttributes.RemoveOn:
                        GetEvent(type, row.Col2).remove_method = method;
                        break;
                    case MethodSemanticsAttributes.Getter:
                        GetProperty(type, row.Col2).get_method = method;
                        break;
                    case MethodSemanticsAttributes.Setter:
                        GetProperty(type, row.Col2).set_method = method;
                        break;
                    case MethodSemanticsAttributes.Other:
                        switch (row.Col2.TokenType)
                        {
                            case TokenType.Event:
                                {
                                    var @event = GetEvent(type, row.Col2);
                                    if (@event.other_methods == null)
                                        @event.other_methods = new Collection<MethodDefinition>();

                                    @event.other_methods.Add(method);
                                    break;
                                }
                            case TokenType.Property:
                                {
                                    var property = GetProperty(type, row.Col2);
                                    if (property.other_methods == null)
                                        property.other_methods = new Collection<MethodDefinition>();

                                    property.other_methods.Add(method);

                                    break;
                                }
                            default:
                                throw new NotSupportedException();
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }

                metadata.Semantics.Remove(method.token.RID);

                return row.Col1;
            }

            static EventDefinition GetEvent(TypeDefinition type, MetadataToken token)
            {
                if (token.TokenType != TokenType.Event)
                    throw new ArgumentException();

                return GetMember(type.Events, token);
            }

            static PropertyDefinition GetProperty(TypeDefinition type, MetadataToken token)
            {
                if (token.TokenType != TokenType.Property)
                    throw new ArgumentException();

                return GetMember(type.Properties, token);
            }

            static TMember GetMember<TMember>(Collection<TMember> members, MetadataToken token) where TMember : IMemberDefinition
            {
                for (int i = 0; i < members.Count; i++)
                {
                    var member = members[i];
                    if (member.MetadataToken == token)
                        return member;
                }

                throw new ArgumentException();
            }

            void InitializeMethodSemantics()
            {
                if (metadata.Semantics != null)
                    return;

                int length = MoveTo(Table.MethodSemantics);

                var semantics = metadata.Semantics = new Dictionary<uint, Row<MethodSemanticsAttributes, MetadataToken>>(0);

                for (uint i = 0; i < length; i++)
                {
                    var attributes = (MethodSemanticsAttributes)ReadUInt16();
                    var method_rid = ReadTableIndex(Table.Method);
                    var association = ReadMetadataToken(CodedIndex.HasSemantics);

                    semantics[method_rid] = new Row<MethodSemanticsAttributes, MetadataToken>(attributes, association);
                }
            }

            public PropertyDefinition ReadMethods(PropertyDefinition property)
            {
                ReadAllSemantics(property.DeclaringType);
                return property;
            }

            public EventDefinition ReadMethods(EventDefinition @event)
            {
                ReadAllSemantics(@event.DeclaringType);
                return @event;
            }

            public MethodSemanticsAttributes ReadAllSemantics(MethodDefinition method)
            {
                ReadAllSemantics(method.DeclaringType);

                return method.SemanticsAttributes;
            }

            void ReadAllSemantics(TypeDefinition type)
            {
                var methods = type.Methods;
                for (int i = 0; i < methods.Count; i++)
                {
                    var method = methods[i];
                    if (method.sem_attrs.HasValue)
                        continue;

                    method.sem_attrs = ReadMethodSemantics(method);
                }
            }

            Range ReadParametersRange(uint method_rid)
            {
                return ReadListRange(method_rid, Table.Method, Table.Param);
            }

            public Collection<MethodDefinition> ReadMethods(TypeDefinition type)
            {
                var methods_range = type.methods_range;
                if (methods_range.Length == 0)
                    return new MemberDefinitionCollection<MethodDefinition>(type);

                var methods = new MemberDefinitionCollection<MethodDefinition>(type, (int)methods_range.Length);

                MoveTo(Table.Method, methods_range.Start);
                for (uint i = 0; i < methods_range.Length; i++)
                    ReadMethod(methods_range.Start + i, methods);

                return methods;
            }

            void InitializeMethods()
            {
                if (metadata.Methods != null)
                    return;

                metadata.Methods = new MethodDefinition[image.GetTableLength(Table.Method)];
            }

            void ReadMethod(uint method_rid, Collection<MethodDefinition> methods)
            {
                var method = new MethodDefinition();
                method.rva = ReadUInt32();
                method.ImplAttributes = (MethodImplAttributes)ReadUInt16();
                method.Attributes = (MethodAttributes)ReadUInt16();
                method.Name = ReadString();
                method.token = new MetadataToken(TokenType.Method, method_rid);

                methods.Add(method);

                var signature = ReadBlobIndex();
                var param_range = ReadParametersRange(method_rid);

                this.context = method;

                ReadMethodSignature(signature, method);
                metadata.AddMethodDefinition(method);

                if (param_range.Length == 0)
                    return;

                var position = base.position;
                ReadParameters(method, param_range);
                base.position = position;
            }

            void ReadParameters(MethodDefinition method, Range param_range)
            {
                MoveTo(Table.Param, param_range.Start);
                for (uint i = 0; i < param_range.Length; i++)
                {
                    var attributes = (ParameterAttributes)ReadUInt16();
                    var sequence = ReadUInt16();
                    var name = ReadString();

                    var parameter = sequence == 0
                        ? method.MethodReturnType.Parameter
                        : method.Parameters[sequence - 1];

                    parameter.token = new MetadataToken(TokenType.Param, param_range.Start + i);
                    parameter.Name = name;
                    parameter.Attributes = attributes;
                }
            }

            void ReadMethodSignature(uint signature, IMethodSignature method)
            {
                var reader = ReadSignature(signature);
                reader.ReadMethodSignature(method);
            }

            public PInvokeInfo ReadPInvokeInfo(MethodDefinition method)
            {
                InitializePInvokes();
                Row<PInvokeAttributes, uint, uint> row;

                var rid = method.token.RID;

                if (!metadata.PInvokes.TryGetValue(rid, out row))
                    return null;

                metadata.PInvokes.Remove(rid);

                return new PInvokeInfo(
                    row.Col1,
                    image.StringHeap.Read(row.Col2),
                    module.ModuleReferences[(int)row.Col3 - 1]);
            }

            void InitializePInvokes()
            {
                if (metadata.PInvokes != null)
                    return;

                int length = MoveTo(Table.ImplMap);

                var pinvokes = metadata.PInvokes = new Dictionary<uint, Row<PInvokeAttributes, uint, uint>>(length);

                for (int i = 1; i <= length; i++)
                {
                    var attributes = (PInvokeAttributes)ReadUInt16();
                    var method = ReadMetadataToken(CodedIndex.MemberForwarded);
                    var name = ReadStringIndex();
                    var scope = ReadTableIndex(Table.File);

                    if (method.TokenType != TokenType.Method)
                        continue;

                    pinvokes.Add(method.RID, new Row<PInvokeAttributes, uint, uint>(attributes, name, scope));
                }
            }

            public bool HasGenericParameters(IGenericParameterProvider provider)
            {
                InitializeGenericParameters();

                Range range;
                if (!metadata.TryGetGenericParameterRange(provider, out range))
                    return false;

                return range.Length > 0;
            }

            public Collection<GenericParameter> ReadGenericParameters(IGenericParameterProvider provider)
            {
                InitializeGenericParameters();

                Range range;
                if (!metadata.TryGetGenericParameterRange(provider, out range)
                    || !MoveTo(Table.GenericParam, range.Start))
                    return new Collection<GenericParameter>();

                metadata.RemoveGenericParameterRange(provider);

                var generic_parameters = new Collection<GenericParameter>((int)range.Length);

                for (uint i = 0; i < range.Length; i++)
                {
                    ReadUInt16();
                    var flags = (GenericParameterAttributes)ReadUInt16();
                    ReadMetadataToken(CodedIndex.TypeOrMethodDef);
                    var name = ReadString();

                    var parameter = new GenericParameter(name, provider);
                    parameter.token = new MetadataToken(TokenType.GenericParam, range.Start + i);
                    parameter.Attributes = flags;

                    generic_parameters.Add(parameter);
                }

                return generic_parameters;
            }

            void InitializeGenericParameters()
            {
                if (metadata.GenericParameters != null)
                    return;

                metadata.GenericParameters = InitializeRanges(
                    Table.GenericParam, () =>
                    {
                        Advance(4);
                        var next = ReadMetadataToken(CodedIndex.TypeOrMethodDef);
                        ReadStringIndex();
                        return next;
                    });
            }

            Dictionary<MetadataToken, Range> InitializeRanges(Table table, Func<MetadataToken> get_next)
            {
                int length = MoveTo(table);
                var ranges = new Dictionary<MetadataToken, Range>(length);

                if (length == 0)
                    return ranges;

                MetadataToken owner = MetadataToken.Zero;
                Range range = new Range(1, 0);

                for (uint i = 1; i <= length; i++)
                {
                    var next = get_next();

                    if (i == 1)
                    {
                        owner = next;
                        range.Length++;
                    }
                    else if (next != owner)
                    {
                        if (owner.RID != 0)
                            ranges.Add(owner, range);
                        range = new Range(i, 1);
                        owner = next;
                    }
                    else
                        range.Length++;
                }

                if (owner != MetadataToken.Zero)
                    ranges.Add(owner, range);

                return ranges;
            }

            public bool HasGenericConstraints(GenericParameter generic_parameter)
            {
                InitializeGenericConstraints();

                MetadataToken[] mapping;
                if (!metadata.TryGetGenericConstraintMapping(generic_parameter, out mapping))
                    return false;

                return mapping.Length > 0;
            }

            public Collection<TypeReference> ReadGenericConstraints(GenericParameter generic_parameter)
            {
                InitializeGenericConstraints();

                MetadataToken[] mapping;
                if (!metadata.TryGetGenericConstraintMapping(generic_parameter, out mapping))
                    return new Collection<TypeReference>();

                var constraints = new Collection<TypeReference>(mapping.Length);

                this.context = (IGenericContext)generic_parameter.Owner;

                for (int i = 0; i < mapping.Length; i++)
                    constraints.Add(GetTypeDefOrRef(mapping[i]));

                metadata.RemoveGenericConstraintMapping(generic_parameter);

                return constraints;
            }

            void InitializeGenericConstraints()
            {
                if (metadata.GenericConstraints != null)
                    return;

                var length = MoveTo(Table.GenericParamConstraint);

                metadata.GenericConstraints = new Dictionary<uint, MetadataToken[]>(length);

                for (int i = 1; i <= length; i++)
                    AddGenericConstraintMapping(
                        ReadTableIndex(Table.GenericParam),
                        ReadMetadataToken(CodedIndex.TypeDefOrRef));
            }

            void AddGenericConstraintMapping(uint generic_parameter, MetadataToken constraint)
            {
                metadata.SetGenericConstraintMapping(
                    generic_parameter,
                    AddMapping(metadata.GenericConstraints, generic_parameter, constraint));
            }

            public bool HasOverrides(MethodDefinition method)
            {
                InitializeOverrides();
                MetadataToken[] mapping;

                if (!metadata.TryGetOverrideMapping(method, out mapping))
                    return false;

                return mapping.Length > 0;
            }

            public Collection<MethodReference> ReadOverrides(MethodDefinition method)
            {
                InitializeOverrides();

                MetadataToken[] mapping;
                if (!metadata.TryGetOverrideMapping(method, out mapping))
                    return new Collection<MethodReference>();

                var overrides = new Collection<MethodReference>(mapping.Length);

                this.context = method;

                for (int i = 0; i < mapping.Length; i++)
                    overrides.Add((MethodReference)LookupToken(mapping[i]));

                metadata.RemoveOverrideMapping(method);

                return overrides;
            }

            void InitializeOverrides()
            {
                if (metadata.Overrides != null)
                    return;

                var length = MoveTo(Table.MethodImpl);

                metadata.Overrides = new Dictionary<uint, MetadataToken[]>(length);

                for (int i = 1; i <= length; i++)
                {
                    ReadTableIndex(Table.TypeDef);

                    var method = ReadMetadataToken(CodedIndex.MethodDefOrRef);
                    if (method.TokenType != TokenType.Method)
                        throw new NotSupportedException();

                    var @override = ReadMetadataToken(CodedIndex.MethodDefOrRef);

                    AddOverrideMapping(method.RID, @override);
                }
            }

            void AddOverrideMapping(uint method_rid, MetadataToken @override)
            {
                metadata.SetOverrideMapping(
                    method_rid,
                    AddMapping(metadata.Overrides, method_rid, @override));
            }

            public MethodBody ReadMethodBody(MethodDefinition method)
            {
                return code.ReadMethodBody(method);
            }

            public CallSite ReadCallSite(MetadataToken token)
            {
                if (!MoveTo(Table.StandAloneSig, token.RID))
                    return null;

                var signature = ReadBlobIndex();

                var call_site = new CallSite();

                ReadMethodSignature(signature, call_site);

                call_site.MetadataToken = token;

                return call_site;
            }

            public VariableDefinitionCollection ReadVariables(MetadataToken local_var_token)
            {
                if (!MoveTo(Table.StandAloneSig, local_var_token.RID))
                    return null;

                var reader = ReadSignature(ReadBlobIndex());
                const byte local_sig = 0x7;

                if (reader.ReadByte() != local_sig)
                    throw new NotSupportedException();

                var count = reader.ReadCompressedUInt32();
                if (count == 0)
                    return null;

                var variables = new VariableDefinitionCollection((int)count);

                for (int i = 0; i < count; i++)
                    variables.Add(new VariableDefinition(reader.ReadTypeSignature()));

                return variables;
            }

            public IMetadataTokenProvider LookupToken(MetadataToken token)
            {
                var rid = token.RID;

                if (rid == 0)
                    return null;

                IMetadataTokenProvider element;
                var position = this.position;
                var context = this.context;

                switch (token.TokenType)
                {
                    case TokenType.TypeDef:
                        element = GetTypeDefinition(rid);
                        break;
                    case TokenType.TypeRef:
                        element = GetTypeReference(rid);
                        break;
                    case TokenType.TypeSpec:
                        element = GetTypeSpecification(rid);
                        break;
                    case TokenType.Field:
                        element = GetFieldDefinition(rid);
                        break;
                    case TokenType.Method:
                        element = GetMethodDefinition(rid);
                        break;
                    case TokenType.MemberRef:
                        element = GetMemberReference(rid);
                        break;
                    case TokenType.MethodSpec:
                        element = GetMethodSpecification(rid);
                        break;
                    default:
                        return null;
                }

                this.position = position;
                this.context = context;

                return element;
            }

            public FieldDefinition GetFieldDefinition(uint rid)
            {
                InitializeTypeDefinitions();

                var field = metadata.GetFieldDefinition(rid);
                if (field != null)
                    return field;

                return LookupField(rid);
            }

            FieldDefinition LookupField(uint rid)
            {
                var type = metadata.GetFieldDeclaringType(rid);
                if (type == null)
                    return null;

                InitializeCollection(type.Fields);

                return metadata.GetFieldDefinition(rid);
            }

            public MethodDefinition GetMethodDefinition(uint rid)
            {
                InitializeTypeDefinitions();

                var method = metadata.GetMethodDefinition(rid);
                if (method != null)
                    return method;

                return LookupMethod(rid);
            }

            MethodDefinition LookupMethod(uint rid)
            {
                var type = metadata.GetMethodDeclaringType(rid);
                if (type == null)
                    return null;

                InitializeCollection(type.Methods);

                return metadata.GetMethodDefinition(rid);
            }

            MethodSpecification GetMethodSpecification(uint rid)
            {
                if (!MoveTo(Table.MethodSpec, rid))
                    return null;

                var element_method = (MethodReference)LookupToken(
                    ReadMetadataToken(CodedIndex.MethodDefOrRef));
                var signature = ReadBlobIndex();

                var method_spec = ReadMethodSpecSignature(signature, element_method);
                method_spec.token = new MetadataToken(TokenType.MethodSpec, rid);
                return method_spec;
            }

            MethodSpecification ReadMethodSpecSignature(uint signature, MethodReference method)
            {
                var reader = ReadSignature(signature);
                const byte methodspec_sig = 0x0a;

                var call_conv = reader.ReadByte();

                if (call_conv != methodspec_sig)
                    throw new NotSupportedException();

                var instance = new GenericInstanceMethod(method);

                reader.ReadGenericInstanceSignature(method, instance);

                return instance;
            }

            MemberReference GetMemberReference(uint rid)
            {
                InitializeMemberReferences();

                var member = metadata.GetMemberReference(rid);
                if (member != null)
                    return member;

                member = ReadMemberReference(rid);
                if (member != null && !member.ContainsGenericParameter)
                    metadata.AddMemberReference(member);
                return member;
            }

            MemberReference ReadMemberReference(uint rid)
            {
                if (!MoveTo(Table.MemberRef, rid))
                    return null;

                var token = ReadMetadataToken(CodedIndex.MemberRefParent);
                var name = ReadString();
                var signature = ReadBlobIndex();

                MemberReference member;

                switch (token.TokenType)
                {
                    case TokenType.TypeDef:
                    case TokenType.TypeRef:
                    case TokenType.TypeSpec:
                        member = ReadTypeMemberReference(token, name, signature);
                        break;
                    case TokenType.Method:
                        member = ReadMethodMemberReference(token, name, signature);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                member.token = new MetadataToken(TokenType.MemberRef, rid);

                return member;
            }

            MemberReference ReadTypeMemberReference(MetadataToken type, string name, uint signature)
            {
                var declaring_type = GetTypeDefOrRef(type);

                this.context = declaring_type;

                var member = ReadMemberReferenceSignature(signature, declaring_type);
                member.Name = name;

                return member;
            }

            MemberReference ReadMemberReferenceSignature(uint signature, TypeReference declaring_type)
            {
                var reader = ReadSignature(signature);
                const byte field_sig = 0x6;

                if (reader.buffer[reader.position] == field_sig)
                {
                    reader.position++;
                    var field = new FieldReference();
                    field.DeclaringType = declaring_type;
                    field.FieldType = reader.ReadTypeSignature();
                    return field;
                }
                else
                {
                    var method = new MethodReference();
                    method.DeclaringType = declaring_type;
                    reader.ReadMethodSignature(method);
                    return method;
                }
            }

            MemberReference ReadMethodMemberReference(MetadataToken token, string name, uint signature)
            {
                var method = GetMethodDefinition(token.RID);

                this.context = method;

                var member = ReadMemberReferenceSignature(signature, method.DeclaringType);
                member.Name = name;

                return member;
            }

            void InitializeMemberReferences()
            {
                if (metadata.MemberReferences != null)
                    return;

                metadata.MemberReferences = new MemberReference[image.GetTableLength(Table.MemberRef)];
            }

            public IEnumerable<MemberReference> GetMemberReferences()
            {
                InitializeMemberReferences();

                var length = image.GetTableLength(Table.MemberRef);

                var type_system = module.TypeSystem;

                var context = new MethodReference(string.Empty, type_system.Void);
                context.DeclaringType = new TypeReference(string.Empty, string.Empty, module, type_system.Corlib);

                var member_references = new MemberReference[length];

                for (uint i = 1; i <= length; i++)
                {
                    this.context = context;
                    member_references[i - 1] = GetMemberReference(i);
                }

                return member_references;
            }

            void InitializeConstants()
            {
                if (metadata.Constants != null)
                    return;

                var length = MoveTo(Table.Constant);

                var constants = metadata.Constants = new Dictionary<MetadataToken, Row<ElementType, uint>>(length);

                for (uint i = 1; i <= length; i++)
                {
                    var type = (ElementType)ReadUInt16();
                    var owner = ReadMetadataToken(CodedIndex.HasConstant);
                    var signature = ReadBlobIndex();

                    constants.Add(owner, new Row<ElementType, uint>(type, signature));
                }
            }

            public object ReadConstant(IConstantProvider owner)
            {
                InitializeConstants();

                Row<ElementType, uint> row;
                if (!metadata.Constants.TryGetValue(owner.MetadataToken, out row))
                    return Mixin.NoValue;

                metadata.Constants.Remove(owner.MetadataToken);

                switch (row.Col1)
                {
                    case ElementType.Class:
                    case ElementType.Object:
                        return null;
                    case ElementType.String:
                        return ReadConstantString(ReadBlob(row.Col2));
                    default:
                        return ReadConstantPrimitive(row.Col1, row.Col2);
                }
            }

            static string ReadConstantString(byte[] blob)
            {
                var length = blob.Length;
                if ((length & 1) == 1)
                    length--;

                return Encoding.Unicode.GetString(blob, 0, length);
            }

            object ReadConstantPrimitive(ElementType type, uint signature)
            {
                var reader = ReadSignature(signature);
                return reader.ReadConstantSignature(type);
            }

            void InitializeCustomAttributes()
            {
                if (metadata.CustomAttributes != null)
                    return;

                metadata.CustomAttributes = InitializeRanges(
                    Table.CustomAttribute, () =>
                    {
                        var next = ReadMetadataToken(CodedIndex.HasCustomAttribute);
                        ReadMetadataToken(CodedIndex.CustomAttributeType);
                        ReadBlobIndex();
                        return next;
                    });
            }

            public bool HasCustomAttributes(ICustomAttributeProvider owner)
            {
                InitializeCustomAttributes();

                Range range;
                if (!metadata.TryGetCustomAttributeRange(owner, out range))
                    return false;

                return range.Length > 0;
            }

            public Collection<CustomAttribute> ReadCustomAttributes(ICustomAttributeProvider owner)
            {
                InitializeCustomAttributes();

                Range range;
                if (!metadata.TryGetCustomAttributeRange(owner, out range)
                    || !MoveTo(Table.CustomAttribute, range.Start))
                    return new Collection<CustomAttribute>();

                var custom_attributes = new Collection<CustomAttribute>((int)range.Length);

                for (int i = 0; i < range.Length; i++)
                {
                    ReadMetadataToken(CodedIndex.HasCustomAttribute);

                    var constructor = (MethodReference)LookupToken(
                        ReadMetadataToken(CodedIndex.CustomAttributeType));

                    var signature = ReadBlobIndex();

                    custom_attributes.Add(new CustomAttribute(signature, constructor));
                }

                metadata.RemoveCustomAttributeRange(owner);

                return custom_attributes;
            }

            public byte[] ReadCustomAttributeBlob(uint signature)
            {
                return ReadBlob(signature);
            }

            public void ReadCustomAttributeSignature(CustomAttribute attribute)
            {
                var reader = ReadSignature(attribute.signature);
                if (reader.ReadUInt16() != 0x0001)
                    throw new InvalidOperationException();

                var constructor = attribute.Constructor;
                if (constructor.HasParameters)
                    reader.ReadCustomAttributeConstructorArguments(attribute, constructor.Parameters);

                if (!reader.CanReadMore())
                    return;

                var named = reader.ReadUInt16();

                if (named == 0)
                    return;

                reader.ReadCustomAttributeNamedArguments(named, ref attribute.fields, ref attribute.properties);
            }

            void InitializeMarshalInfos()
            {
                if (metadata.FieldMarshals != null)
                    return;

                var length = MoveTo(Table.FieldMarshal);

                var marshals = metadata.FieldMarshals = new Dictionary<MetadataToken, uint>(length);

                for (int i = 0; i < length; i++)
                {
                    var token = ReadMetadataToken(CodedIndex.HasFieldMarshal);
                    var signature = ReadBlobIndex();
                    if (token.RID == 0)
                        continue;

                    marshals.Add(token, signature);
                }
            }

            public bool HasMarshalInfo(IMarshalInfoProvider owner)
            {
                InitializeMarshalInfos();

                return metadata.FieldMarshals.ContainsKey(owner.MetadataToken);
            }

            public MarshalInfo ReadMarshalInfo(IMarshalInfoProvider owner)
            {
                InitializeMarshalInfos();

                uint signature;
                if (!metadata.FieldMarshals.TryGetValue(owner.MetadataToken, out signature))
                    return null;

                var reader = ReadSignature(signature);

                metadata.FieldMarshals.Remove(owner.MetadataToken);

                return reader.ReadMarshalInfo();
            }

            void InitializeSecurityDeclarations()
            {
                if (metadata.SecurityDeclarations != null)
                    return;

                metadata.SecurityDeclarations = InitializeRanges(
                    Table.DeclSecurity, () =>
                    {
                        ReadUInt16();
                        var next = ReadMetadataToken(CodedIndex.HasDeclSecurity);
                        ReadBlobIndex();
                        return next;
                    });
            }

            public bool HasSecurityDeclarations(ISecurityDeclarationProvider owner)
            {
                InitializeSecurityDeclarations();

                Range range;
                if (!metadata.TryGetSecurityDeclarationRange(owner, out range))
                    return false;

                return range.Length > 0;
            }

            public Collection<SecurityDeclaration> ReadSecurityDeclarations(ISecurityDeclarationProvider owner)
            {
                InitializeSecurityDeclarations();

                Range range;
                if (!metadata.TryGetSecurityDeclarationRange(owner, out range)
                    || !MoveTo(Table.DeclSecurity, range.Start))
                    return new Collection<SecurityDeclaration>();

                var security_declarations = new Collection<SecurityDeclaration>((int)range.Length);

                for (int i = 0; i < range.Length; i++)
                {
                    var action = (SecurityAction)ReadUInt16();
                    ReadMetadataToken(CodedIndex.HasDeclSecurity);
                    var signature = ReadBlobIndex();

                    security_declarations.Add(new SecurityDeclaration(action, signature, module));
                }

                metadata.RemoveSecurityDeclarationRange(owner);

                return security_declarations;
            }

            public byte[] ReadSecurityDeclarationBlob(uint signature)
            {
                return ReadBlob(signature);
            }

            public void ReadSecurityDeclarationSignature(SecurityDeclaration declaration)
            {
                var signature = declaration.signature;
                var reader = ReadSignature(signature);

                if (reader.buffer[reader.position] != '.')
                {
                    ReadXmlSecurityDeclaration(signature, declaration);
                    return;
                }

                reader.ReadByte();
                var count = reader.ReadCompressedUInt32();
                var attributes = new Collection<SecurityAttribute>((int)count);

                for (int i = 0; i < count; i++)
                    attributes.Add(reader.ReadSecurityAttribute());

                declaration.security_attributes = attributes;
            }

            void ReadXmlSecurityDeclaration(uint signature, SecurityDeclaration declaration)
            {
                var blob = ReadBlob(signature);
                var attributes = new Collection<SecurityAttribute>(1);

                var attribute = new SecurityAttribute(
                    module.TypeSystem.LookupType("System.Security.Permissions", "PermissionSetAttribute"));

                attribute.properties = new Collection<CustomAttributeNamedArgument>(1);
                attribute.properties.Add(
                    new CustomAttributeNamedArgument(
                        "XML",
                        new CustomAttributeArgument(
                            module.TypeSystem.String,
                            Encoding.Unicode.GetString(blob, 0, blob.Length))));

                attributes.Add(attribute);

                declaration.security_attributes = attributes;
            }

            public Collection<ExportedType> ReadExportedTypes()
            {
                var length = MoveTo(Table.ExportedType);
                if (length == 0)
                    return new Collection<ExportedType>();

                var exported_types = new Collection<ExportedType>(length);

                for (int i = 1; i <= length; i++)
                {
                    var attributes = (TypeAttributes)ReadUInt32();
                    var identifier = ReadUInt32();
                    var name = ReadString();
                    var @namespace = ReadString();
                    var implementation = ReadMetadataToken(CodedIndex.Implementation);

                    ExportedType declaring_type = null;
                    IMetadataScope scope = null;

                    switch (implementation.TokenType)
                    {
                        case TokenType.AssemblyRef:
                        case TokenType.File:
                            scope = GetExportedTypeScope(implementation);
                            break;
                        case TokenType.ExportedType:
                            declaring_type = exported_types[(int)implementation.RID - 1];
                            break;
                    }

                    var exported_type = new ExportedType(@namespace, name, scope)
                    {
                        Attributes = attributes,
                        Identifier = (int)identifier,
                        DeclaringType = declaring_type,
                    };
                    exported_type.token = new MetadataToken(TokenType.ExportedType, i);

                    exported_types.Add(exported_type);
                }

                return exported_types;
            }

            IMetadataScope GetExportedTypeScope(MetadataToken token)
            {
                switch (token.TokenType)
                {
                    case TokenType.AssemblyRef:
                        return module.AssemblyReferences[(int)token.RID - 1];
                    case TokenType.File:
                        var position = this.position;
                        var reference = GetModuleReferenceFromFile(token);
                        this.position = position;

                        if (reference == null)
                            throw new NotSupportedException();

                        return reference;
                    default:
                        throw new NotSupportedException();
                }
            }

            ModuleReference GetModuleReferenceFromFile(MetadataToken token)
            {
                if (!MoveTo(Table.File, token.RID))
                    return null;

                ReadUInt32();
                var file_name = ReadString();
                var modules = module.ModuleReferences;

                ModuleReference reference = null;
                for (int i = 0; i < modules.Count; i++)
                {
                    var module_reference = modules[i];
                    if (module_reference.Name != file_name)
                        continue;

                    reference = module_reference;
                    break;
                }

                return reference;
            }

            static void InitializeCollection(object o)
            {
            }
        }

        sealed class SignatureReader : ByteBuffer
        {

            readonly MetadataReader reader;
            readonly uint start, sig_length;

            TypeSystem TypeSystem
            {
                get { return reader.module.TypeSystem; }
            }

            public SignatureReader(uint blob, MetadataReader reader)
                : base(reader.buffer)
            {
                this.reader = reader;

                MoveToBlob(blob);

                this.sig_length = ReadCompressedUInt32();
                this.start = (uint)position;
            }

            void MoveToBlob(uint blob)
            {
                position = (int)(reader.image.BlobHeap.Offset + blob);
            }

            MetadataToken ReadTypeTokenSignature()
            {
                return CodedIndex.TypeDefOrRef.GetMetadataToken(ReadCompressedUInt32());
            }

            GenericParameter GetGenericParameter(GenericParameterType type, uint var)
            {
                var context = reader.context;

                if (context == null)
                    throw new NotSupportedException();

                IGenericParameterProvider provider;

                switch (type)
                {
                    case GenericParameterType.Type:
                        provider = context.Type;
                        break;
                    case GenericParameterType.Method:
                        provider = context.Method;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                int index = (int)var;

                if (!context.IsDefinition)
                    CheckGenericContext(provider, index);

                return provider.GenericParameters[index];
            }

            static void CheckGenericContext(IGenericParameterProvider owner, int index)
            {
                var owner_parameters = owner.GenericParameters;

                for (int i = owner_parameters.Count; i <= index; i++)
                    owner_parameters.Add(new GenericParameter(owner));
            }

            public void ReadGenericInstanceSignature(IGenericParameterProvider provider, IGenericInstance instance)
            {
                var arity = ReadCompressedUInt32();

                if (!provider.IsDefinition)
                    CheckGenericContext(provider, (int)arity - 1);

                var instance_arguments = instance.GenericArguments;

                for (int i = 0; i < arity; i++)
                    instance_arguments.Add(ReadTypeSignature());
            }

            ArrayType ReadArrayTypeSignature()
            {
                var array = new ArrayType(ReadTypeSignature());

                var rank = ReadCompressedUInt32();

                var sizes = new uint[ReadCompressedUInt32()];
                for (int i = 0; i < sizes.Length; i++)
                    sizes[i] = ReadCompressedUInt32();

                var low_bounds = new int[ReadCompressedUInt32()];
                for (int i = 0; i < low_bounds.Length; i++)
                    low_bounds[i] = ReadCompressedInt32();

                array.Dimensions.Clear();

                for (int i = 0; i < rank; i++)
                {
                    int? lower = null, upper = null;

                    if (i < low_bounds.Length)
                        lower = low_bounds[i];

                    if (i < sizes.Length)
                        upper = lower + (int)sizes[i] - 1;

                    array.Dimensions.Add(new ArrayDimension(lower, upper));
                }

                return array;
            }

            TypeReference GetTypeDefOrRef(MetadataToken token)
            {
                return reader.GetTypeDefOrRef(token);
            }

            public TypeReference ReadTypeSignature()
            {
                return ReadTypeSignature((ElementType)ReadByte());
            }

            TypeReference ReadTypeSignature(ElementType etype)
            {
                switch (etype)
                {
                    case ElementType.ValueType:
                        {
                            var value_type = GetTypeDefOrRef(ReadTypeTokenSignature());
                            value_type.IsValueType = true;
                            return value_type;
                        }
                    case ElementType.Class:
                        return GetTypeDefOrRef(ReadTypeTokenSignature());
                    case ElementType.Ptr:
                        return new PointerType(ReadTypeSignature());
                    case ElementType.FnPtr:
                        {
                            var fptr = new FunctionPointerType();
                            ReadMethodSignature(fptr);
                            return fptr;
                        }
                    case ElementType.ByRef:
                        return new ByReferenceType(ReadTypeSignature());
                    case ElementType.Pinned:
                        return new PinnedType(ReadTypeSignature());
                    case ElementType.SzArray:
                        return new ArrayType(ReadTypeSignature());
                    case ElementType.Array:
                        return ReadArrayTypeSignature();
                    case ElementType.CModOpt:
                        return new OptionalModifierType(
                            GetTypeDefOrRef(ReadTypeTokenSignature()), ReadTypeSignature());
                    case ElementType.CModReqD:
                        return new RequiredModifierType(
                            GetTypeDefOrRef(ReadTypeTokenSignature()), ReadTypeSignature());
                    case ElementType.Sentinel:
                        return new SentinelType(ReadTypeSignature());
                    case ElementType.Var:
                        return GetGenericParameter(GenericParameterType.Type, ReadCompressedUInt32());
                    case ElementType.MVar:
                        return GetGenericParameter(GenericParameterType.Method, ReadCompressedUInt32());
                    case ElementType.GenericInst:
                        {
                            var is_value_type = ReadByte() == (byte)ElementType.ValueType;
                            var element_type = GetTypeDefOrRef(ReadTypeTokenSignature());
                            var generic_instance = new GenericInstanceType(element_type);

                            ReadGenericInstanceSignature(element_type, generic_instance);

                            if (is_value_type)
                            {
                                generic_instance.IsValueType = true;
                                element_type.GetElementType().IsValueType = true;
                            }

                            return generic_instance;
                        }
                    case ElementType.Object: return TypeSystem.Object;
                    case ElementType.Void: return TypeSystem.Void;
                    case ElementType.TypedByRef: return TypeSystem.TypedReference;
                    case ElementType.I: return TypeSystem.IntPtr;
                    case ElementType.U: return TypeSystem.UIntPtr;
                    default: return GetPrimitiveType(etype);
                }
            }

            public void ReadMethodSignature(IMethodSignature method)
            {
                var calling_convention = ReadByte();
                method.CallingConvention = (MethodCallingConvention)calling_convention;
                method.HasThis = (calling_convention & 0x20) != 0;
                method.ExplicitThis = (calling_convention & 0x40) != 0;

                var generic_context = method as MethodReference;
                if (generic_context != null)
                    reader.context = generic_context;

                if ((calling_convention & 0x10) != 0)
                {
                    var arity = ReadCompressedUInt32();

                    if (generic_context != null && !generic_context.IsDefinition)
                        CheckGenericContext(generic_context, (int)arity - 1);
                }
                var param_count = ReadCompressedUInt32();

                method.MethodReturnType.ReturnType = ReadTypeSignature();

                if (param_count == 0)
                    return;

                Collection<ParameterDefinition> parameters;

                var method_ref = method as MethodReference;
                if (method_ref != null)
                    parameters = method_ref.parameters = new ParameterDefinitionCollection(method, (int)param_count);
                else
                    parameters = method.Parameters;

                for (int i = 0; i < param_count; i++)
                    parameters.Add(new ParameterDefinition(ReadTypeSignature()));
            }

            public object ReadConstantSignature(ElementType type)
            {
                return ReadPrimitiveValue(type);
            }

            public void ReadCustomAttributeConstructorArguments(CustomAttribute attribute, Collection<ParameterDefinition> parameters)
            {
                var count = parameters.Count;
                if (count == 0)
                    return;

                attribute.arguments = new Collection<CustomAttributeArgument>(count);

                for (int i = 0; i < count; i++)
                    attribute.arguments.Add(
                        ReadCustomAttributeFixedArgument(parameters[i].ParameterType));
            }

            CustomAttributeArgument ReadCustomAttributeFixedArgument(TypeReference type)
            {
                if (type.IsArray)
                    return ReadCustomAttributeFixedArrayArgument((ArrayType)type);

                return ReadCustomAttributeElement(type);
            }

            public void ReadCustomAttributeNamedArguments(ushort count, ref Collection<CustomAttributeNamedArgument> fields, ref Collection<CustomAttributeNamedArgument> properties)
            {
                for (int i = 0; i < count; i++)
                    ReadCustomAttributeNamedArgument(ref fields, ref properties);
            }

            void ReadCustomAttributeNamedArgument(ref Collection<CustomAttributeNamedArgument> fields, ref Collection<CustomAttributeNamedArgument> properties)
            {
                var kind = ReadByte();
                var type = ReadCustomAttributeFieldOrPropType();
                var name = ReadUTF8String();

                Collection<CustomAttributeNamedArgument> container;
                switch (kind)
                {
                    case 0x53:
                        container = GetCustomAttributeNamedArgumentCollection(ref fields);
                        break;
                    case 0x54:
                        container = GetCustomAttributeNamedArgumentCollection(ref properties);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                container.Add(new CustomAttributeNamedArgument(name, ReadCustomAttributeFixedArgument(type)));
            }

            static Collection<CustomAttributeNamedArgument> GetCustomAttributeNamedArgumentCollection(ref Collection<CustomAttributeNamedArgument> collection)
            {
                if (collection != null)
                    return collection;

                return collection = new Collection<CustomAttributeNamedArgument>();
            }

            CustomAttributeArgument ReadCustomAttributeFixedArrayArgument(ArrayType type)
            {
                var length = ReadUInt32();

                if (length == 0xffffffff)
                    return new CustomAttributeArgument(type, null);

                if (length == 0)
                    return new CustomAttributeArgument(type, Empty<CustomAttributeArgument>.Array);

                var arguments = new CustomAttributeArgument[length];
                var element_type = type.ElementType;

                for (int i = 0; i < length; i++)
                    arguments[i] = ReadCustomAttributeElement(element_type);

                return new CustomAttributeArgument(type, arguments);
            }

            CustomAttributeArgument ReadCustomAttributeElement(TypeReference type)
            {
                if (type.IsArray)
                    return ReadCustomAttributeFixedArrayArgument((ArrayType)type);

                return new CustomAttributeArgument(
                    type,
                    type.etype == ElementType.Object
                        ? ReadCustomAttributeElement(ReadCustomAttributeFieldOrPropType())
                        : ReadCustomAttributeElementValue(type));
            }

            object ReadCustomAttributeElementValue(TypeReference type)
            {
                var etype = type.etype;

                switch (etype)
                {
                    case ElementType.String:
                        return ReadUTF8String();
                    case ElementType.None:
                        if (type.IsTypeOf("System", "Type"))
                            return ReadTypeReference();

                        return ReadCustomAttributeEnum(type);
                    default:
                        return ReadPrimitiveValue(etype);
                }
            }

            object ReadPrimitiveValue(ElementType type)
            {
                switch (type)
                {
                    case ElementType.Boolean:
                        return ReadByte() == 1;
                    case ElementType.I1:
                        return (sbyte)ReadByte();
                    case ElementType.U1:
                        return ReadByte();
                    case ElementType.Char:
                        return (char)ReadUInt16();
                    case ElementType.I2:
                        return ReadInt16();
                    case ElementType.U2:
                        return ReadUInt16();
                    case ElementType.I4:
                        return ReadInt32();
                    case ElementType.U4:
                        return ReadUInt32();
                    case ElementType.I8:
                        return ReadInt64();
                    case ElementType.U8:
                        return ReadUInt64();
                    case ElementType.R4:
                        return ReadSingle();
                    case ElementType.R8:
                        return ReadDouble();
                    default:
                        throw new NotImplementedException(type.ToString());
                }
            }

            TypeReference GetPrimitiveType(ElementType etype)
            {
                switch (etype)
                {
                    case ElementType.Boolean:
                        return TypeSystem.Boolean;
                    case ElementType.Char:
                        return TypeSystem.Char;
                    case ElementType.I1:
                        return TypeSystem.SByte;
                    case ElementType.U1:
                        return TypeSystem.Byte;
                    case ElementType.I2:
                        return TypeSystem.Int16;
                    case ElementType.U2:
                        return TypeSystem.UInt16;
                    case ElementType.I4:
                        return TypeSystem.Int32;
                    case ElementType.U4:
                        return TypeSystem.UInt32;
                    case ElementType.I8:
                        return TypeSystem.Int64;
                    case ElementType.U8:
                        return TypeSystem.UInt64;
                    case ElementType.R4:
                        return TypeSystem.Single;
                    case ElementType.R8:
                        return TypeSystem.Double;
                    case ElementType.String:
                        return TypeSystem.String;
                    default:
                        throw new NotImplementedException(etype.ToString());
                }
            }

            TypeReference ReadCustomAttributeFieldOrPropType()
            {
                var etype = (ElementType)ReadByte();

                switch (etype)
                {
                    case ElementType.Boxed:
                        return TypeSystem.Object;
                    case ElementType.SzArray:
                        return new ArrayType(ReadCustomAttributeFieldOrPropType());
                    case ElementType.Enum:
                        return ReadTypeReference();
                    case ElementType.Type:
                        return TypeSystem.LookupType("System", "Type");
                    default:
                        return GetPrimitiveType(etype);
                }
            }

            public TypeReference ReadTypeReference()
            {
                return TypeParser.ParseType(reader.module, ReadUTF8String());
            }

            object ReadCustomAttributeEnum(TypeReference enum_type)
            {
                var type = enum_type.CheckedResolve();
                if (!type.IsEnum)
                    throw new ArgumentException();

                return ReadCustomAttributeElementValue(type.GetEnumUnderlyingType());
            }

            public SecurityAttribute ReadSecurityAttribute()
            {
                var attribute = new SecurityAttribute(ReadTypeReference());

                ReadCompressedUInt32();

                ReadCustomAttributeNamedArguments(
                    (ushort)ReadCompressedUInt32(),
                    ref attribute.fields,
                    ref attribute.properties);

                return attribute;
            }

            public MarshalInfo ReadMarshalInfo()
            {
                var native = ReadNativeType();
                switch (native)
                {
                    case NativeType.Array:
                        {
                            var array = new ArrayMarshalInfo();
                            if (CanReadMore())
                                array.element_type = ReadNativeType();
                            if (CanReadMore())
                                array.size_parameter_index = (int)ReadCompressedUInt32();
                            if (CanReadMore())
                                array.size = (int)ReadCompressedUInt32();
                            if (CanReadMore())
                                array.size_parameter_multiplier = (int)ReadCompressedUInt32();
                            return array;
                        }
                    case NativeType.SafeArray:
                        {
                            var array = new SafeArrayMarshalInfo();
                            if (CanReadMore())
                                array.element_type = ReadVariantType();
                            return array;
                        }
                    case NativeType.FixedArray:
                        {
                            var array = new FixedArrayMarshalInfo();
                            if (CanReadMore())
                                array.size = (int)ReadCompressedUInt32();
                            if (CanReadMore())
                                array.element_type = ReadNativeType();
                            return array;
                        }
                    case NativeType.FixedSysString:
                        {
                            var sys_string = new FixedSysStringMarshalInfo();
                            if (CanReadMore())
                                sys_string.size = (int)ReadCompressedUInt32();
                            return sys_string;
                        }
                    case NativeType.CustomMarshaler:
                        {
                            var marshaler = new CustomMarshalInfo();
                            var guid_value = ReadUTF8String();
                            marshaler.guid = !string.IsNullOrEmpty(guid_value) ? new Guid(guid_value) : Guid.Empty;
                            marshaler.unmanaged_type = ReadUTF8String();
                            marshaler.managed_type = ReadTypeReference();
                            marshaler.cookie = ReadUTF8String();
                            return marshaler;
                        }
                    default:
                        return new MarshalInfo(native);
                }
            }

            NativeType ReadNativeType()
            {
                return (NativeType)ReadByte();
            }

            VariantType ReadVariantType()
            {
                return (VariantType)ReadByte();
            }

            string ReadUTF8String()
            {
                if (buffer[position] == 0xff)
                {
                    position++;
                    return null;
                }

                var length = (int)ReadCompressedUInt32();
                if (length == 0)
                    return string.Empty;

                var @string = Encoding.UTF8.GetString(buffer, position,
                    buffer[position + length - 1] == 0 ? length - 1 : length);

                position += length;
                return @string;
            }

            public bool CanReadMore()
            {
                return position - start < sig_length;
            }
        }



        public delegate AssemblyDefinition AssemblyResolveEventHandler(object sender, AssemblyNameReference reference);

        public sealed class AssemblyResolveEventArgs : EventArgs
        {

            readonly AssemblyNameReference reference;

            public AssemblyNameReference AssemblyReference
            {
                get { return reference; }
            }

            public AssemblyResolveEventArgs(AssemblyNameReference reference)
            {
                this.reference = reference;
            }
        }

        public abstract class BaseAssemblyResolver : IAssemblyResolver
        {

            static readonly bool on_mono = Type.GetType("Mono.Runtime") != null;

            readonly Collection<string> directories;

            Collection<string> gac_paths;

            public void AddSearchDirectory(string directory)
            {
                directories.Add(directory);
            }

            public void RemoveSearchDirectory(string directory)
            {
                directories.Remove(directory);
            }

            public string[] GetSearchDirectories()
            {
                var directories = new string[this.directories.size];
                Array.Copy(this.directories.items, directories, directories.Length);
                return directories;
            }

            public virtual AssemblyDefinition Resolve(string fullName)
            {
                return Resolve(fullName, new ReaderParameters());
            }

            public virtual AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
            {
                if (fullName == null)
                    throw new ArgumentNullException("fullName");

                return Resolve(AssemblyNameReference.Parse(fullName), parameters);
            }

            public event AssemblyResolveEventHandler ResolveFailure;

            protected BaseAssemblyResolver()
            {
                directories = new Collection<string>(2) { ".", "bin" };
            }

            AssemblyDefinition GetAssembly(string file, ReaderParameters parameters)
            {
                if (parameters.AssemblyResolver == null)
                    parameters.AssemblyResolver = this;

                return ModuleDefinition.ReadModule(file, parameters).Assembly;
            }

            public virtual AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return Resolve(name, new ReaderParameters());
            }

            public virtual AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                if (parameters == null)
                    parameters = new ReaderParameters();

                var assembly = SearchDirectory(name, directories, parameters);
                if (assembly != null)
                    return assembly;

                var framework_dir = Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName);

                if (IsZero(name.Version))
                {
                    assembly = SearchDirectory(name, new[] { framework_dir }, parameters);
                    if (assembly != null)
                        return assembly;
                }

                if (name.Name == "mscorlib")
                {
                    assembly = GetCorlib(name, parameters);
                    if (assembly != null)
                        return assembly;
                }

                assembly = GetAssemblyInGac(name, parameters);
                if (assembly != null)
                    return assembly;

                assembly = SearchDirectory(name, new[] { framework_dir }, parameters);
                if (assembly != null)
                    return assembly;

                if (ResolveFailure != null)
                {
                    assembly = ResolveFailure(this, name);
                    if (assembly != null)
                        return assembly;
                }

                throw new FileNotFoundException("Could not resolve: " + name);
            }

            AssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
            {
                var extensions = new[] { ".exe", ".dll" };
                foreach (var directory in directories)
                {
                    foreach (var extension in extensions)
                    {
                        string file = Path.Combine(directory, name.Name + extension);
                        if (File.Exists(file))
                            return GetAssembly(file, parameters);
                    }
                }

                return null;
            }

            static bool IsZero(Version version)
            {
                return version == null || (version.Major == 0 && version.Minor == 0 && version.Build == 0 && version.Revision == 0);
            }

            AssemblyDefinition GetCorlib(AssemblyNameReference reference, ReaderParameters parameters)
            {
                var version = reference.Version;
                var corlib = typeof(object).Assembly.GetName();

                if (corlib.Version == version || IsZero(version))
                    return GetAssembly(typeof(object).Module.FullyQualifiedName, parameters);

                var path = Directory.GetParent(
                    Directory.GetParent(
                        typeof(object).Module.FullyQualifiedName).FullName
                    ).FullName;

                if (on_mono)
                {
                    if (version.Major == 1)
                        path = Path.Combine(path, "1.0");
                    else if (version.Major == 2)
                    {
                        if (version.MajorRevision == 5)
                            path = Path.Combine(path, "2.1");
                        else
                            path = Path.Combine(path, "2.0");
                    }
                    else if (version.Major == 4)
                        path = Path.Combine(path, "4.0");
                    else
                        throw new NotSupportedException("Version not supported: " + version);
                }
                else
                {
                    switch (version.Major)
                    {
                        case 1:
                            if (version.MajorRevision == 3300)
                                path = Path.Combine(path, "v1.0.3705");
                            else
                                path = Path.Combine(path, "v1.0.5000.0");
                            break;
                        case 2:
                            path = Path.Combine(path, "v2.0.50727");
                            break;
                        case 4:
                            path = Path.Combine(path, "v4.0.30319");
                            break;
                        default:
                            throw new NotSupportedException("Version not supported: " + version);
                    }
                }

                var file = Path.Combine(path, "mscorlib.dll");
                if (File.Exists(file))
                    return GetAssembly(file, parameters);

                return null;
            }

            static Collection<string> GetGacPaths()
            {
                if (on_mono)
                    return GetDefaultMonoGacPaths();

                var paths = new Collection<string>(2);
                var windir = Environment.GetEnvironmentVariable("WINDIR");
                if (windir == null)
                    return paths;

                paths.Add(Path.Combine(windir, "assembly"));
                paths.Add(Path.Combine(windir, Path.Combine("Microsoft.NET", "assembly")));
                return paths;
            }

            static Collection<string> GetDefaultMonoGacPaths()
            {
                var paths = new Collection<string>(1);
                var gac = GetCurrentMonoGac();
                if (gac != null)
                    paths.Add(gac);

                var gac_paths_env = Environment.GetEnvironmentVariable("MONO_GAC_PREFIX");
                if (string.IsNullOrEmpty(gac_paths_env))
                    return paths;

                var prefixes = gac_paths_env.Split(Path.PathSeparator);
                foreach (var prefix in prefixes)
                {
                    if (string.IsNullOrEmpty(prefix))
                        continue;

                    var gac_path = Path.Combine(Path.Combine(Path.Combine(prefix, "lib"), "mono"), "gac");
                    if (Directory.Exists(gac_path) && !paths.Contains(gac))
                        paths.Add(gac_path);
                }

                return paths;
            }

            static string GetCurrentMonoGac()
            {
                return Path.Combine(
                    Directory.GetParent(
                        Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName)).FullName,
                    "gac");
            }

            AssemblyDefinition GetAssemblyInGac(AssemblyNameReference reference, ReaderParameters parameters)
            {
                if (reference.PublicKeyToken == null || reference.PublicKeyToken.Length == 0)
                    return null;

                if (gac_paths == null)
                    gac_paths = GetGacPaths();

                if (on_mono)
                    return GetAssemblyInMonoGac(reference, parameters);

                return GetAssemblyInNetGac(reference, parameters);
            }

            AssemblyDefinition GetAssemblyInMonoGac(AssemblyNameReference reference, ReaderParameters parameters)
            {
                for (int i = 0; i < gac_paths.Count; i++)
                {
                    var gac_path = gac_paths[i];
                    var file = GetAssemblyFile(reference, string.Empty, gac_path);
                    if (File.Exists(file))
                        return GetAssembly(file, parameters);
                }

                return null;
            }

            AssemblyDefinition GetAssemblyInNetGac(AssemblyNameReference reference, ReaderParameters parameters)
            {
                var gacs = new[] { "GAC_MSIL", "GAC_32", "GAC" };
                var prefixes = new[] { string.Empty, "v4.0_" };

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < gacs.Length; j++)
                    {
                        var gac = Path.Combine(gac_paths[i], gacs[j]);
                        var file = GetAssemblyFile(reference, prefixes[i], gac);
                        if (Directory.Exists(gac) && File.Exists(file))
                            return GetAssembly(file, parameters);
                    }
                }

                return null;
            }

            static string GetAssemblyFile(AssemblyNameReference reference, string prefix, string gac)
            {
                var gac_folder = new StringBuilder();
                gac_folder.Append(prefix);
                gac_folder.Append(reference.Version);
                gac_folder.Append("__");
                for (int i = 0; i < reference.PublicKeyToken.Length; i++)
                    gac_folder.Append(reference.PublicKeyToken[i].ToString("x2"));

                return Path.Combine(
                    Path.Combine(
                        Path.Combine(gac, reference.Name), gac_folder.ToString()),
                    reference.Name + ".dll");
            }
        }



        public sealed class CallSite : MethodReference
        {

            public override string FullName
            {
                get
                {
                    var signature = new StringBuilder();
                    signature.Append(ReturnType.FullName);
                    this.MethodSignatureFullName(signature);
                    return signature.ToString();
                }
            }

            public override ModuleDefinition Module
            {
                get { return ReturnType.Module; }
            }

            public override MethodDefinition Resolve()
            {
                return null;
            }
        }


        public struct CustomAttributeArgument
        {

            readonly TypeReference type;
            readonly object value;

            public TypeReference Type
            {
                get { return type; }
            }

            public object Value
            {
                get { return value; }
            }

            public CustomAttributeArgument(TypeReference type, object value)
            {
                Mixin.CheckType(type);
                this.type = type;
                this.value = value;
            }
        }

        public struct CustomAttributeNamedArgument
        {

            readonly string name;
            readonly CustomAttributeArgument argument;

            public string Name
            {
                get { return name; }
            }

            public CustomAttributeArgument Argument
            {
                get { return argument; }
            }

            public CustomAttributeNamedArgument(string name, CustomAttributeArgument argument)
            {
                Mixin.CheckName(name);
                this.name = name;
                this.argument = argument;
            }
        }

        public interface ICustomAttribute
        {

            TypeReference AttributeType { get; }

            bool HasFields { get; }
            bool HasProperties { get; }
            Collection<CustomAttributeNamedArgument> Fields { get; }
            Collection<CustomAttributeNamedArgument> Properties { get; }
        }

        public sealed class CustomAttribute : ICustomAttribute
        {

            readonly internal uint signature;
            internal bool resolved;
            MethodReference constructor;
            byte[] blob;
            internal Collection<CustomAttributeArgument> arguments;
            internal Collection<CustomAttributeNamedArgument> fields;
            internal Collection<CustomAttributeNamedArgument> properties;

            public MethodReference Constructor
            {
                get { return constructor; }
                set { constructor = value; }
            }

            public TypeReference AttributeType
            {
                get { return constructor.DeclaringType; }
            }

            public bool IsResolved
            {
                get { return resolved; }
            }

            public bool HasConstructorArguments
            {
                get
                {
                    Resolve();

                    return !arguments.IsNullOrEmpty();
                }
            }

            public Collection<CustomAttributeArgument> ConstructorArguments
            {
                get
                {
                    Resolve();

                    return arguments ?? (arguments = new Collection<CustomAttributeArgument>());
                }
            }

            public bool HasFields
            {
                get
                {
                    Resolve();

                    return !fields.IsNullOrEmpty();
                }
            }

            public Collection<CustomAttributeNamedArgument> Fields
            {
                get
                {
                    Resolve();

                    return fields ?? (fields = new Collection<CustomAttributeNamedArgument>());
                }
            }

            public bool HasProperties
            {
                get
                {
                    Resolve();

                    return !properties.IsNullOrEmpty();
                }
            }

            public Collection<CustomAttributeNamedArgument> Properties
            {
                get
                {
                    Resolve();

                    return properties ?? (properties = new Collection<CustomAttributeNamedArgument>());
                }
            }

            internal bool HasImage
            {
                get { return constructor != null && constructor.HasImage; }
            }

            internal ModuleDefinition Module
            {
                get { return constructor.Module; }
            }

            internal CustomAttribute(uint signature, MethodReference constructor)
            {
                this.signature = signature;
                this.constructor = constructor;
                this.resolved = false;
            }

            public CustomAttribute(MethodReference constructor)
            {
                this.constructor = constructor;
                this.resolved = true;
            }

            public CustomAttribute(MethodReference constructor, byte[] blob)
            {
                this.constructor = constructor;
                this.resolved = false;
                this.blob = blob;
            }

            public byte[] GetBlob()
            {
                if (blob != null)
                    return blob;

                if (!HasImage || signature == 0)
                    throw new NotSupportedException();

                return blob = Module.Read(this, (attribute, reader) => reader.ReadCustomAttributeBlob(attribute.signature));
            }

            void Resolve()
            {
                if (resolved || !HasImage)
                    return;

                try
                {
                    Module.Read(this, (attribute, reader) =>
                    {
                        reader.ReadCustomAttributeSignature(attribute);
                        return this;
                    });

                    resolved = true;
                }
                catch (ResolutionException)
                {
                    if (arguments != null)
                        arguments.Clear();
                    if (fields != null)
                        fields.Clear();
                    if (properties != null)
                        properties.Clear();

                    resolved = false;
                }
            }
        }

        static partial class Mixin
        {

            public static void CheckName(string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                if (name.Length == 0)
                    throw new ArgumentException("Empty name");
            }
        }


        public static class GlobalAssemblyResolver
        {

            public static readonly IAssemblyResolver Instance = new DefaultAssemblyResolver();
        }

        public class DefaultAssemblyResolver : BaseAssemblyResolver
        {

            readonly IDictionary<string, AssemblyDefinition> cache;

            public DefaultAssemblyResolver()
            {
                cache = new Dictionary<string, AssemblyDefinition>();
            }

            public override AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                AssemblyDefinition assembly;
                if (cache.TryGetValue(name.FullName, out assembly))
                    return assembly;

                assembly = base.Resolve(name);
                cache[name.FullName] = assembly;

                return assembly;
            }

            protected void RegisterAssembly(AssemblyDefinition assembly)
            {
                if (assembly == null)
                    throw new ArgumentNullException("assembly");

                var name = assembly.Name.FullName;
                if (cache.ContainsKey(name))
                    return;

                cache[name] = assembly;
            }
        }



        public sealed class EmbeddedResource : Resource
        {

            readonly MetadataReader reader;

            uint? offset;
            byte[] data;
            Stream stream;

            public override ResourceType ResourceType
            {
                get { return ResourceType.Embedded; }
            }

            public EmbeddedResource(string name, ManifestResourceAttributes attributes, byte[] data) :
                base(name, attributes)
            {
                this.data = data;
            }

            public EmbeddedResource(string name, ManifestResourceAttributes attributes, Stream stream) :
                base(name, attributes)
            {
                this.stream = stream;
            }

            internal EmbeddedResource(string name, ManifestResourceAttributes attributes, uint offset, MetadataReader reader)
                : base(name, attributes)
            {
                this.offset = offset;
                this.reader = reader;
            }

            public Stream GetResourceStream()
            {
                if (stream != null)
                    return stream;

                if (data != null)
                    return new MemoryStream(data);

                if (offset.HasValue)
                    return reader.GetManagedResourceStream(offset.Value);

                throw new InvalidOperationException();
            }

            public byte[] GetResourceData()
            {
                if (stream != null)
                    return ReadStream(stream);

                if (data != null)
                    return data;

                if (offset.HasValue)
                    return reader.GetManagedResourceStream(offset.Value).ToArray();

                throw new InvalidOperationException();
            }

            static byte[] ReadStream(Stream stream)
            {
                var length = (int)stream.Length;
                var data = new byte[length];
                int offset = 0, read;

                while ((read = stream.Read(data, offset, length - offset)) > 0)
                    offset += read;

                return data;
            }
        }



        static partial class Mixin
        {

            public static bool IsNullOrEmpty<T>(this T[] self)
            {
                return self == null || self.Length == 0;
            }

            public static bool IsNullOrEmpty<T>(this ICollection<T> self)
            {
                return self == null || self.Count == 0;
            }
        }


        [Flags]
        public enum EventAttributes : ushort
        {
            None = 0x0000,
            SpecialName = 0x0200,
            RTSpecialName = 0x0400
        }


        public sealed class EventDefinition : EventReference, IMemberDefinition
        {

            ushort attributes;

            Collection<CustomAttribute> custom_attributes;

            internal MethodDefinition add_method;
            internal MethodDefinition invoke_method;
            internal MethodDefinition remove_method;
            internal Collection<MethodDefinition> other_methods;

            public EventAttributes Attributes
            {
                get { return (EventAttributes)attributes; }
                set { attributes = (ushort)value; }
            }

            public MethodDefinition AddMethod
            {
                get
                {
                    if (add_method != null)
                        return add_method;

                    InitializeMethods();
                    return add_method;
                }
                set { add_method = value; }
            }

            public MethodDefinition InvokeMethod
            {
                get
                {
                    if (invoke_method != null)
                        return invoke_method;

                    InitializeMethods();
                    return invoke_method;
                }
                set { invoke_method = value; }
            }

            public MethodDefinition RemoveMethod
            {
                get
                {
                    if (remove_method != null)
                        return remove_method;

                    InitializeMethods();
                    return remove_method;
                }
                set { remove_method = value; }
            }

            public bool HasOtherMethods
            {
                get
                {
                    if (other_methods != null)
                        return other_methods.Count > 0;

                    InitializeMethods();
                    return !other_methods.IsNullOrEmpty();
                }
            }

            public Collection<MethodDefinition> OtherMethods
            {
                get
                {
                    if (other_methods != null)
                        return other_methods;

                    InitializeMethods();

                    if (other_methods != null)
                        return other_methods;

                    return other_methods = new Collection<MethodDefinition>();
                }
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(Module);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(Module)); }
            }

            public bool IsSpecialName
            {
                get { return attributes.GetAttributes((ushort)EventAttributes.SpecialName); }
                set { attributes = attributes.SetAttributes((ushort)EventAttributes.SpecialName, value); }
            }

            public bool IsRuntimeSpecialName
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.RTSpecialName); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.RTSpecialName, value); }
            }

            public new TypeDefinition DeclaringType
            {
                get { return (TypeDefinition)base.DeclaringType; }
                set { base.DeclaringType = value; }
            }

            public override bool IsDefinition
            {
                get { return true; }
            }

            public EventDefinition(string name, EventAttributes attributes, TypeReference eventType)
                : base(name, eventType)
            {
                this.attributes = (ushort)attributes;
                this.token = new MetadataToken(TokenType.Event);
            }

            void InitializeMethods()
            {
                if (add_method != null
                    || invoke_method != null
                    || remove_method != null)
                    return;

                var module = this.Module;
                if (!module.HasImage())
                    return;

                module.Read(this, (@event, reader) => reader.ReadMethods(@event));
            }
        }



        public abstract class EventReference : MemberReference
        {

            TypeReference event_type;

            public TypeReference EventType
            {
                get { return event_type; }
                set { event_type = value; }
            }

            public override string FullName
            {
                get { return event_type.FullName + " " + MemberFullName(); }
            }

            protected EventReference(string name, TypeReference eventType)
                : base(name)
            {
                if (eventType == null)
                    throw new ArgumentNullException("eventType");

                event_type = eventType;
            }
        }


        public class ExportedType : IMetadataTokenProvider
        {

            string @namespace;
            string name;
            uint attributes;
            IMetadataScope scope;
            int identifier;
            ExportedType declaring_type;
            internal MetadataToken token;

            public string Namespace
            {
                get { return @namespace; }
                set { @namespace = value; }
            }

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public TypeAttributes Attributes
            {
                get { return (TypeAttributes)attributes; }
                set { attributes = (uint)value; }
            }

            public IMetadataScope Scope
            {
                get
                {
                    if (declaring_type != null)
                        return declaring_type.Scope;

                    return scope;
                }
            }

            public ExportedType DeclaringType
            {
                get { return declaring_type; }
                set { declaring_type = value; }
            }

            public MetadataToken MetadataToken
            {
                get { return token; }
                set { token = value; }
            }

            public int Identifier
            {
                get { return identifier; }
                set { identifier = value; }
            }

            public bool IsNotPublic
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NotPublic); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NotPublic, value); }
            }

            public bool IsPublic
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.Public); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.Public, value); }
            }

            public bool IsNestedPublic
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedPublic); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedPublic, value); }
            }

            public bool IsNestedPrivate
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedPrivate); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedPrivate, value); }
            }

            public bool IsNestedFamily
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamily); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamily, value); }
            }

            public bool IsNestedAssembly
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedAssembly); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedAssembly, value); }
            }

            public bool IsNestedFamilyAndAssembly
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamANDAssem); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamANDAssem, value); }
            }

            public bool IsNestedFamilyOrAssembly
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamORAssem); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamORAssem, value); }
            }

            public bool IsAutoLayout
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.AutoLayout); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.AutoLayout, value); }
            }

            public bool IsSequentialLayout
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.SequentialLayout); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.SequentialLayout, value); }
            }

            public bool IsExplicitLayout
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.ExplicitLayout); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.ExplicitLayout, value); }
            }

            public bool IsClass
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.ClassSemanticMask, (uint)TypeAttributes.Class); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.ClassSemanticMask, (uint)TypeAttributes.Class, value); }
            }

            public bool IsInterface
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.ClassSemanticMask, (uint)TypeAttributes.Interface); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.ClassSemanticMask, (uint)TypeAttributes.Interface, value); }
            }

            public bool IsAbstract
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Abstract); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Abstract, value); }
            }

            public bool IsSealed
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Sealed); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Sealed, value); }
            }

            public bool IsSpecialName
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.SpecialName); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.SpecialName, value); }
            }

            public bool IsImport
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Import); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Import, value); }
            }

            public bool IsSerializable
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Serializable); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Serializable, value); }
            }

            public bool IsAnsiClass
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AnsiClass); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AnsiClass, value); }
            }

            public bool IsUnicodeClass
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.UnicodeClass); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.UnicodeClass, value); }
            }

            public bool IsAutoClass
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AutoClass); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AutoClass, value); }
            }

            public bool IsBeforeFieldInit
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.BeforeFieldInit); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.BeforeFieldInit, value); }
            }

            public bool IsRuntimeSpecialName
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.RTSpecialName); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.RTSpecialName, value); }
            }

            public bool HasSecurity
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.HasSecurity); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.HasSecurity, value); }
            }

            public bool IsForwarder
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Forwarder); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Forwarder, value); }
            }

            public string FullName
            {
                get
                {
                    if (declaring_type != null)
                        return declaring_type.FullName + "/" + name;

                    if (string.IsNullOrEmpty(@namespace))
                        return name;

                    return @namespace + "." + name;
                }
            }

            public ExportedType(string @namespace, string name, IMetadataScope scope)
            {
                this.@namespace = @namespace;
                this.name = name;
                this.scope = scope;
            }

            public override string ToString()
            {
                return FullName;
            }
        }



        [Flags]
        public enum FieldAttributes : ushort
        {
            FieldAccessMask = 0x0007,
            CompilerControlled = 0x0000,
            Private = 0x0001,
            FamANDAssem = 0x0002,
            Assembly = 0x0003,
            Family = 0x0004,
            FamORAssem = 0x0005,
            Public = 0x0006,
            Static = 0x0010,
            InitOnly = 0x0020,
            Literal = 0x0040,
            NotSerialized = 0x0080,
            SpecialName = 0x0200,
            PInvokeImpl = 0x2000,
            RTSpecialName = 0x0400,
            HasFieldMarshal = 0x1000,
            HasDefault = 0x8000,
            HasFieldRVA = 0x0100
        }


        public sealed class FieldDefinition : FieldReference, IMemberDefinition, IConstantProvider, IMarshalInfoProvider
        {

            ushort attributes;
            Collection<CustomAttribute> custom_attributes;

            int offset = Mixin.NotResolvedMarker;

            internal int rva = Mixin.NotResolvedMarker;
            byte[] initial_value;

            object constant = Mixin.NotResolved;

            MarshalInfo marshal_info;

            void ResolveLayout()
            {
                if (offset != Mixin.NotResolvedMarker)
                    return;

                if (!HasImage)
                {
                    offset = Mixin.NoDataMarker;
                    return;
                }

                offset = Module.Read(this, (field, reader) => reader.ReadFieldLayout(field));
            }

            public bool HasLayoutInfo
            {
                get
                {
                    if (offset >= 0)
                        return true;

                    ResolveLayout();

                    return offset >= 0;
                }
            }

            public int Offset
            {
                get
                {
                    if (offset >= 0)
                        return offset;

                    ResolveLayout();

                    return offset >= 0 ? offset : -1;
                }
                set { offset = value; }
            }

            void ResolveRVA()
            {
                if (rva != Mixin.NotResolvedMarker)
                    return;

                if (!HasImage)
                    return;

                rva = Module.Read(this, (field, reader) => reader.ReadFieldRVA(field));
            }

            public int RVA
            {
                get
                {
                    if (rva > 0)
                        return rva;

                    ResolveRVA();

                    return rva > 0 ? rva : 0;
                }
            }

            public byte[] InitialValue
            {
                get
                {
                    if (initial_value != null)
                        return initial_value;

                    ResolveRVA();

                    if (initial_value == null)
                        initial_value = Empty<byte>.Array;

                    return initial_value;
                }
                set { initial_value = value; }
            }

            public FieldAttributes Attributes
            {
                get { return (FieldAttributes)attributes; }
                set { attributes = (ushort)value; }
            }

            public bool HasConstant
            {
                get
                {
                    ResolveConstant();

                    return constant != Mixin.NoValue;
                }
                set { if (!value) constant = Mixin.NoValue; }
            }

            public object Constant
            {
                get { return HasConstant ? constant : null; }
                set { constant = value; }
            }

            void ResolveConstant()
            {
                if (constant != Mixin.NotResolved)
                    return;

                this.ResolveConstant(ref constant, Module);
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(Module);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(Module)); }
            }

            public bool HasMarshalInfo
            {
                get
                {
                    if (marshal_info != null)
                        return true;

                    return this.GetHasMarshalInfo(Module);
                }
            }

            public MarshalInfo MarshalInfo
            {
                get { return marshal_info ?? (marshal_info = this.GetMarshalInfo(Module)); }
                set { marshal_info = value; }
            }

            public bool IsCompilerControlled
            {
                get { return attributes.GetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.CompilerControlled); }
                set { attributes = attributes.SetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.CompilerControlled, value); }
            }

            public bool IsPrivate
            {
                get { return attributes.GetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.Private); }
                set { attributes = attributes.SetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.Private, value); }
            }

            public bool IsFamilyAndAssembly
            {
                get { return attributes.GetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.FamANDAssem); }
                set { attributes = attributes.SetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.FamANDAssem, value); }
            }

            public bool IsAssembly
            {
                get { return attributes.GetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.Assembly); }
                set { attributes = attributes.SetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.Assembly, value); }
            }

            public bool IsFamily
            {
                get { return attributes.GetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.Family); }
                set { attributes = attributes.SetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.Family, value); }
            }

            public bool IsFamilyOrAssembly
            {
                get { return attributes.GetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.FamORAssem); }
                set { attributes = attributes.SetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.FamORAssem, value); }
            }

            public bool IsPublic
            {
                get { return attributes.GetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.Public); }
                set { attributes = attributes.SetMaskedAttributes((ushort)FieldAttributes.FieldAccessMask, (ushort)FieldAttributes.Public, value); }
            }

            public bool IsStatic
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.Static); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.Static, value); }
            }

            public bool IsInitOnly
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.InitOnly); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.InitOnly, value); }
            }

            public bool IsLiteral
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.Literal); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.Literal, value); }
            }

            public bool IsNotSerialized
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.NotSerialized); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.NotSerialized, value); }
            }

            public bool IsSpecialName
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.SpecialName); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.SpecialName, value); }
            }

            public bool IsPInvokeImpl
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.PInvokeImpl); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.PInvokeImpl, value); }
            }

            public bool IsRuntimeSpecialName
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.RTSpecialName); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.RTSpecialName, value); }
            }

            public bool HasDefault
            {
                get { return attributes.GetAttributes((ushort)FieldAttributes.HasDefault); }
                set { attributes = attributes.SetAttributes((ushort)FieldAttributes.HasDefault, value); }
            }

            public override bool IsDefinition
            {
                get { return true; }
            }

            public new TypeDefinition DeclaringType
            {
                get { return (TypeDefinition)base.DeclaringType; }
                set { base.DeclaringType = value; }
            }

            public FieldDefinition(string name, FieldAttributes attributes, TypeReference fieldType)
                : base(name, fieldType)
            {
                this.attributes = (ushort)attributes;
            }

            public override FieldDefinition Resolve()
            {
                return this;
            }
        }

        static partial class Mixin
        {

            public const int NotResolvedMarker = -2;
            public const int NoDataMarker = -1;
        }



        public class FieldReference : MemberReference
        {

            TypeReference field_type;

            public TypeReference FieldType
            {
                get { return field_type; }
                set { field_type = value; }
            }

            public override string FullName
            {
                get { return field_type.FullName + " " + MemberFullName(); }
            }

            internal override bool ContainsGenericParameter
            {
                get { return field_type.ContainsGenericParameter || base.ContainsGenericParameter; }
            }

            internal FieldReference()
            {
                this.token = new MetadataToken(TokenType.MemberRef);
            }

            public FieldReference(string name, TypeReference fieldType)
                : base(name)
            {
                if (fieldType == null)
                    throw new ArgumentNullException("fieldType");

                this.field_type = fieldType;
                this.token = new MetadataToken(TokenType.MemberRef);
            }

            public FieldReference(string name, TypeReference fieldType, TypeReference declaringType)
                : this(name, fieldType)
            {
                if (declaringType == null)
                    throw new ArgumentNullException("declaringType");

                this.DeclaringType = declaringType;
            }

            public virtual FieldDefinition Resolve()
            {
                var module = this.Module;
                if (module == null)
                    throw new NotSupportedException();

                return module.Resolve(this);
            }
        }



        enum FileAttributes : uint
        {
            ContainsMetaData = 0x0000,
            ContainsNoMetaData = 0x0001,
        }


        public sealed class FunctionPointerType : TypeSpecification, IMethodSignature
        {

            readonly MethodReference function;

            public bool HasThis
            {
                get { return function.HasThis; }
                set { function.HasThis = value; }
            }

            public bool ExplicitThis
            {
                get { return function.ExplicitThis; }
                set { function.ExplicitThis = value; }
            }

            public MethodCallingConvention CallingConvention
            {
                get { return function.CallingConvention; }
                set { function.CallingConvention = value; }
            }

            public bool HasParameters
            {
                get { return function.HasParameters; }
            }

            public Collection<ParameterDefinition> Parameters
            {
                get { return function.Parameters; }
            }

            public TypeReference ReturnType
            {
                get { return function.MethodReturnType.ReturnType; }
                set { function.MethodReturnType.ReturnType = value; }
            }

            public MethodReturnType MethodReturnType
            {
                get { return function.MethodReturnType; }
            }

            public override string Name
            {
                get { return function.Name; }
                set { throw new InvalidOperationException(); }
            }

            public override string Namespace
            {
                get { return string.Empty; }
                set { throw new InvalidOperationException(); }
            }

            public override ModuleDefinition Module
            {
                get { return ReturnType.Module; }
            }

            public override IMetadataScope Scope
            {
                get { return function.ReturnType.Scope; }
            }

            public override bool IsFunctionPointer
            {
                get { return true; }
            }

            internal override bool ContainsGenericParameter
            {
                get { return function.ContainsGenericParameter; }
            }

            public override string FullName
            {
                get
                {
                    var signature = new StringBuilder();
                    signature.Append(function.Name);
                    signature.Append(" ");
                    signature.Append(function.ReturnType.FullName);
                    signature.Append(" *");
                    this.MethodSignatureFullName(signature);
                    return signature.ToString();
                }
            }

            public FunctionPointerType()
                : base(null)
            {
                this.function = new MethodReference();
                this.function.Name = "method";
                this.etype = MD.ElementType.FnPtr;
            }

            public override TypeDefinition Resolve()
            {
                return null;
            }

            public override TypeReference GetElementType()
            {
                return this;
            }
        }



        public sealed class GenericInstanceMethod : MethodSpecification, IGenericInstance, IGenericContext
        {

            Collection<TypeReference> arguments;

            public bool HasGenericArguments
            {
                get { return !arguments.IsNullOrEmpty(); }
            }

            public Collection<TypeReference> GenericArguments
            {
                get
                {
                    if (arguments == null)
                        arguments = new Collection<TypeReference>();

                    return arguments;
                }
            }

            public override bool IsGenericInstance
            {
                get { return true; }
            }

            IGenericParameterProvider IGenericContext.Method
            {
                get { return ElementMethod; }
            }

            IGenericParameterProvider IGenericContext.Type
            {
                get { return ElementMethod.DeclaringType; }
            }

            internal override bool ContainsGenericParameter
            {
                get { return this.ContainsGenericParameter() || base.ContainsGenericParameter; }
            }

            public override string FullName
            {
                get
                {
                    var signature = new StringBuilder();
                    var method = this.ElementMethod;
                    signature.Append(method.ReturnType.FullName)
                        .Append(" ")
                        .Append(method.DeclaringType.FullName)
                        .Append("::")
                        .Append(method.Name);
                    this.GenericInstanceFullName(signature);
                    this.MethodSignatureFullName(signature);
                    return signature.ToString();

                }
            }

            public GenericInstanceMethod(MethodReference method)
                : base(method)
            {
            }
        }



        public sealed class GenericInstanceType : TypeSpecification, IGenericInstance, IGenericContext
        {

            Collection<TypeReference> arguments;

            public bool HasGenericArguments
            {
                get { return !arguments.IsNullOrEmpty(); }
            }

            public Collection<TypeReference> GenericArguments
            {
                get
                {
                    if (arguments == null)
                        arguments = new Collection<TypeReference>();

                    return arguments;
                }
            }

            public override TypeReference DeclaringType
            {
                get { return ElementType.DeclaringType; }
                set { throw new NotSupportedException(); }
            }

            public override string FullName
            {
                get
                {
                    var name = new StringBuilder();
                    name.Append(base.FullName);
                    this.GenericInstanceFullName(name);
                    return name.ToString();
                }
            }

            public override bool IsGenericInstance
            {
                get { return true; }
            }

            internal override bool ContainsGenericParameter
            {
                get { return this.ContainsGenericParameter() || base.ContainsGenericParameter; }
            }

            IGenericParameterProvider IGenericContext.Type
            {
                get { return ElementType; }
            }

            public GenericInstanceType(TypeReference type)
                : base(type)
            {
                base.IsValueType = type.IsValueType;
                this.etype = MD.ElementType.GenericInst;
            }
        }



        public sealed class GenericParameter : TypeReference, ICustomAttributeProvider
        {

            readonly IGenericParameterProvider owner;

            ushort attributes;
            Collection<TypeReference> constraints;
            Collection<CustomAttribute> custom_attributes;

            public GenericParameterAttributes Attributes
            {
                get { return (GenericParameterAttributes)attributes; }
                set { attributes = (ushort)value; }
            }

            public int Position
            {
                get
                {
                    if (owner == null)
                        return -1;

                    return owner.GenericParameters.IndexOf(this);
                }
            }

            public IGenericParameterProvider Owner
            {
                get { return owner; }
            }

            public bool HasConstraints
            {
                get
                {
                    if (constraints != null)
                        return constraints.Count > 0;

                    if (HasImage)
                        return Module.Read(this, (generic_parameter, reader) => reader.HasGenericConstraints(generic_parameter));

                    return false;
                }
            }

            public Collection<TypeReference> Constraints
            {
                get
                {
                    if (constraints != null)
                        return constraints;

                    if (HasImage)
                        return constraints = Module.Read(this, (generic_parameter, reader) => reader.ReadGenericConstraints(generic_parameter));

                    return constraints = new Collection<TypeReference>();
                }
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(Module);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(Module)); }
            }

            internal new bool HasImage
            {
                get { return Module != null && Module.HasImage; }
            }

            public override IMetadataScope Scope
            {
                get
                {
                    if (owner.GenericParameterType == GenericParameterType.Method)
                        return ((MethodReference)owner).DeclaringType.Scope;

                    return ((TypeReference)owner).Scope;
                }
            }

            public override ModuleDefinition Module
            {
                get { return ((MemberReference)owner).Module; }
            }

            public override string Name
            {
                get
                {
                    if (!string.IsNullOrEmpty(base.Name))
                        return base.Name;

                    return base.Name = (owner.GenericParameterType == GenericParameterType.Type ? "!" : "!!") + Position;
                }
            }

            public override string Namespace
            {
                get { return string.Empty; }
                set { throw new InvalidOperationException(); }
            }

            public override string FullName
            {
                get { return Name; }
            }

            public override bool IsGenericParameter
            {
                get { return true; }
            }

            internal override bool ContainsGenericParameter
            {
                get { return true; }
            }

            public override MetadataType MetadataType
            {
                get { return (MetadataType)etype; }
            }

            public bool IsNonVariant
            {
                get { return attributes.GetMaskedAttributes((ushort)GenericParameterAttributes.VarianceMask, (ushort)GenericParameterAttributes.NonVariant); }
                set { attributes = attributes.SetMaskedAttributes((ushort)GenericParameterAttributes.VarianceMask, (ushort)GenericParameterAttributes.NonVariant, value); }
            }

            public bool IsCovariant
            {
                get { return attributes.GetMaskedAttributes((ushort)GenericParameterAttributes.VarianceMask, (ushort)GenericParameterAttributes.Covariant); }
                set { attributes = attributes.SetMaskedAttributes((ushort)GenericParameterAttributes.VarianceMask, (ushort)GenericParameterAttributes.Covariant, value); }
            }

            public bool IsContravariant
            {
                get { return attributes.GetMaskedAttributes((ushort)GenericParameterAttributes.VarianceMask, (ushort)GenericParameterAttributes.Contravariant); }
                set { attributes = attributes.SetMaskedAttributes((ushort)GenericParameterAttributes.VarianceMask, (ushort)GenericParameterAttributes.Contravariant, value); }
            }

            public bool HasReferenceTypeConstraint
            {
                get { return attributes.GetAttributes((ushort)GenericParameterAttributes.ReferenceTypeConstraint); }
                set { attributes = attributes.SetAttributes((ushort)GenericParameterAttributes.ReferenceTypeConstraint, value); }
            }

            public bool HasNotNullableValueTypeConstraint
            {
                get { return attributes.GetAttributes((ushort)GenericParameterAttributes.NotNullableValueTypeConstraint); }
                set { attributes = attributes.SetAttributes((ushort)GenericParameterAttributes.NotNullableValueTypeConstraint, value); }
            }

            public bool HasDefaultConstructorConstraint
            {
                get { return attributes.GetAttributes((ushort)GenericParameterAttributes.DefaultConstructorConstraint); }
                set { attributes = attributes.SetAttributes((ushort)GenericParameterAttributes.DefaultConstructorConstraint, value); }
            }

            public GenericParameter(IGenericParameterProvider owner)
                : this(string.Empty, owner)
            {
            }

            public GenericParameter(string name, IGenericParameterProvider owner)
                : base(string.Empty, name)
            {
                if (owner == null)
                    throw new ArgumentNullException();

                this.owner = owner;
                this.etype = owner.GenericParameterType == GenericParameterType.Type ? ElementType.Var : ElementType.MVar;
            }

            public override TypeDefinition Resolve()
            {
                return null;
            }
        }



        [Flags]
        public enum GenericParameterAttributes : ushort
        {
            VarianceMask = 0x0003,
            NonVariant = 0x0000,
            Covariant = 0x0001,
            Contravariant = 0x0002,

            SpecialConstraintMask = 0x001c,
            ReferenceTypeConstraint = 0x0004,
            NotNullableValueTypeConstraint = 0x0008,
            DefaultConstructorConstraint = 0x0010
        }


        public interface IConstantProvider : IMetadataTokenProvider
        {

            bool HasConstant { get; set; }
            object Constant { get; set; }
        }

        static partial class Mixin
        {

            internal static object NoValue = new object();
            internal static object NotResolved = new object();

            public static void ResolveConstant(
                this IConstantProvider self,
                ref object constant,
                ModuleDefinition module)
            {
                constant = module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.ReadConstant(provider))
                    : Mixin.NoValue;
            }
        }


        public interface ICustomAttributeProvider : IMetadataTokenProvider
        {

            Collection<CustomAttribute> CustomAttributes { get; }

            bool HasCustomAttributes { get; }
        }

        static partial class Mixin
        {

            public static bool GetHasCustomAttributes(
                this ICustomAttributeProvider self,
                ModuleDefinition module)
            {
                return module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.HasCustomAttributes(provider))
                    : false;
            }

            public static Collection<CustomAttribute> GetCustomAttributes(
                this ICustomAttributeProvider self,
                ModuleDefinition module)
            {
                return module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.ReadCustomAttributes(provider))
                    : new Collection<CustomAttribute>();
            }
        }


        public interface IGenericInstance : IMetadataTokenProvider
        {

            bool HasGenericArguments { get; }
            Collection<TypeReference> GenericArguments { get; }
        }

        static partial class Mixin
        {

            public static bool ContainsGenericParameter(this IGenericInstance self)
            {
                var arguments = self.GenericArguments;

                for (int i = 0; i < arguments.Count; i++)
                    if (arguments[i].ContainsGenericParameter)
                        return true;

                return false;
            }

            public static void GenericInstanceFullName(this IGenericInstance self, StringBuilder builder)
            {
                builder.Append("<");
                var arguments = self.GenericArguments;
                for (int i = 0; i < arguments.Count; i++)
                {
                    if (i > 0)
                        builder.Append(",");
                    builder.Append(arguments[i].FullName);
                }
                builder.Append(">");
            }
        }


        public interface IGenericParameterProvider : IMetadataTokenProvider
        {

            bool HasGenericParameters { get; }
            bool IsDefinition { get; }
            ModuleDefinition Module { get; }
            Collection<GenericParameter> GenericParameters { get; }
            GenericParameterType GenericParameterType { get; }
        }

        public enum GenericParameterType
        {
            Type,
            Method
        }

        interface IGenericContext
        {

            bool IsDefinition { get; }
            IGenericParameterProvider Type { get; }
            IGenericParameterProvider Method { get; }
        }

        static partial class Mixin
        {

            public static bool GetHasGenericParameters(
                this IGenericParameterProvider self,
                ModuleDefinition module)
            {
                return module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.HasGenericParameters(provider))
                    : false;
            }

            public static Collection<GenericParameter> GetGenericParameters(
                this IGenericParameterProvider self,
                ModuleDefinition module)
            {
                return module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.ReadGenericParameters(provider))
                    : new Collection<GenericParameter>();
            }
        }



        public interface IMarshalInfoProvider : IMetadataTokenProvider
        {

            bool HasMarshalInfo { get; }
            MarshalInfo MarshalInfo { get; set; }
        }

        static partial class Mixin
        {

            public static bool GetHasMarshalInfo(
                this IMarshalInfoProvider self,
                ModuleDefinition module)
            {
                return module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.HasMarshalInfo(provider))
                    : false;
            }

            public static MarshalInfo GetMarshalInfo(
                this IMarshalInfoProvider self,
                ModuleDefinition module)
            {
                return module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.ReadMarshalInfo(provider))
                    : null;
            }
        }


        public interface IMemberDefinition : ICustomAttributeProvider
        {

            string Name { get; set; }
            string FullName { get; }

            bool IsSpecialName { get; set; }
            bool IsRuntimeSpecialName { get; set; }

            TypeDefinition DeclaringType { get; set; }
        }

        static partial class Mixin
        {

            public static bool GetAttributes(this uint self, uint attributes)
            {
                return (self & attributes) != 0;
            }

            public static uint SetAttributes(this uint self, uint attributes, bool value)
            {
                if (value)
                    return self | attributes;

                return self & ~attributes;
            }

            public static bool GetMaskedAttributes(this uint self, uint mask, uint attributes)
            {
                return (self & mask) == attributes;
            }

            public static uint SetMaskedAttributes(this uint self, uint mask, uint attributes, bool value)
            {
                if (value)
                {
                    self &= ~mask;
                    return self | attributes;
                }

                return self & ~(mask & attributes);
            }

            public static bool GetAttributes(this ushort self, ushort attributes)
            {
                return (self & attributes) != 0;
            }

            public static ushort SetAttributes(this ushort self, ushort attributes, bool value)
            {
                if (value)
                    return (ushort)(self | attributes);

                return (ushort)(self & ~attributes);
            }

            public static bool GetMaskedAttributes(this ushort self, ushort mask, uint attributes)
            {
                return (self & mask) == attributes;
            }

            public static ushort SetMaskedAttributes(this ushort self, ushort mask, uint attributes, bool value)
            {
                if (value)
                {
                    self = (ushort)(self & ~mask);
                    return (ushort)(self | attributes);
                }

                return (ushort)(self & ~(mask & attributes));
            }
        }



        public enum MetadataScopeType
        {
            AssemblyNameReference,
            ModuleReference,
            ModuleDefinition,
        }

        public interface IMetadataScope : IMetadataTokenProvider
        {
            MetadataScopeType MetadataScopeType { get; }
            string Name { get; set; }
        }


        public interface IMetadataTokenProvider
        {

            MetadataToken MetadataToken { get; set; }
        }


        public interface IMethodSignature : IMetadataTokenProvider
        {

            bool HasThis { get; set; }
            bool ExplicitThis { get; set; }
            MethodCallingConvention CallingConvention { get; set; }

            bool HasParameters { get; }
            Collection<ParameterDefinition> Parameters { get; }
            TypeReference ReturnType { get; set; }
            MethodReturnType MethodReturnType { get; }
        }

        static partial class Mixin
        {

            public static void MethodSignatureFullName(this IMethodSignature self, StringBuilder builder)
            {
                builder.Append("(");

                if (self.HasParameters)
                {
                    var parameters = self.Parameters;
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        var parameter = parameters[i];
                        if (i > 0)
                            builder.Append(", ");

                        if (parameter.ParameterType.IsSentinel)
                            builder.Append("..., ");

                        builder.Append(parameter.ParameterType.Name);
                        builder.Append(" ");
                        builder.Append(parameter.Name);
                    }
                }

                builder.Append(")");
            }
        }



        enum ImportGenericKind
        {
            Definition,
            Open,
        }

        class MetadataImporter
        {

            readonly ModuleDefinition module;

            public MetadataImporter(ModuleDefinition module)
            {
                this.module = module;
            }

            static readonly Dictionary<Type, ElementType> type_etype_mapping = new Dictionary<Type, ElementType>(18) {
			{ typeof (void), ElementType.Void },
			{ typeof (bool), ElementType.Boolean },
			{ typeof (char), ElementType.Char },
			{ typeof (sbyte), ElementType.I1 },
			{ typeof (byte), ElementType.U1 },
			{ typeof (short), ElementType.I2 },
			{ typeof (ushort), ElementType.U2 },
			{ typeof (int), ElementType.I4 },
			{ typeof (uint), ElementType.U4 },
			{ typeof (long), ElementType.I8 },
			{ typeof (ulong), ElementType.U8 },
			{ typeof (float), ElementType.R4 },
			{ typeof (double), ElementType.R8 },
			{ typeof (string), ElementType.String },
			{ typeof (TypedReference), ElementType.TypedByRef },
			{ typeof (IntPtr), ElementType.I },
			{ typeof (UIntPtr), ElementType.U },
			{ typeof (object), ElementType.Object },
		};

            public TypeReference ImportType(Type type, IGenericContext context)
            {
                return ImportType(type, context, ImportGenericKind.Open);
            }

            public TypeReference ImportType(Type type, IGenericContext context, ImportGenericKind import_kind)
            {
                if (IsTypeSpecification(type) || ImportOpenGenericType(type, import_kind))
                    return ImportTypeSpecification(type, context);

                var reference = new TypeReference(
                    string.Empty,
                    type.Name,
                    module,
                    ImportScope(type.Assembly),
                    type.IsValueType);

                reference.etype = ImportElementType(type);

                if (IsNestedType(type))
                    reference.DeclaringType = ImportType(type.DeclaringType, context, import_kind);
                else
                    reference.Namespace = type.Namespace;

                if (type.IsGenericType)
                    ImportGenericParameters(reference, type.GetGenericArguments());

                return reference;
            }

            static bool ImportOpenGenericType(Type type, ImportGenericKind import_kind)
            {
                return type.IsGenericType && type.IsGenericTypeDefinition && import_kind == ImportGenericKind.Open;
            }

            static bool ImportOpenGenericMethod(SR.MethodBase method, ImportGenericKind import_kind)
            {
                return method.IsGenericMethod && method.IsGenericMethodDefinition && import_kind == ImportGenericKind.Open;
            }

            static bool IsNestedType(Type type)
            {
                return type.IsNested;
            }

            TypeReference ImportTypeSpecification(Type type, IGenericContext context)
            {
                if (type.IsByRef)
                    return new ByReferenceType(ImportType(type.GetElementType(), context));

                if (type.IsPointer)
                    return new PointerType(ImportType(type.GetElementType(), context));

                if (type.IsArray)
                    return new ArrayType(ImportType(type.GetElementType(), context), type.GetArrayRank());

                if (type.IsGenericType)
                    return ImportGenericInstance(type, context);

                if (type.IsGenericParameter)
                    return ImportGenericParameter(type, context);

                throw new NotSupportedException(type.FullName);
            }

            static TypeReference ImportGenericParameter(Type type, IGenericContext context)
            {
                if (context == null)
                    throw new InvalidOperationException();

                var owner = type.DeclaringMethod != null
                    ? context.Method
                    : context.Type;

                if (owner == null)
                    throw new InvalidOperationException();

                return owner.GenericParameters[type.GenericParameterPosition];
            }

            TypeReference ImportGenericInstance(Type type, IGenericContext context)
            {
                var element_type = ImportType(type.GetGenericTypeDefinition(), context, ImportGenericKind.Definition);
                var instance = new GenericInstanceType(element_type);
                var arguments = type.GetGenericArguments();
                var instance_arguments = instance.GenericArguments;

                for (int i = 0; i < arguments.Length; i++)
                    instance_arguments.Add(ImportType(arguments[i], context ?? element_type));

                return instance;
            }

            static bool IsTypeSpecification(Type type)
            {
                return type.HasElementType
                    || IsGenericInstance(type)
                    || type.IsGenericParameter;
            }

            static bool IsGenericInstance(Type type)
            {
                return type.IsGenericType && !type.IsGenericTypeDefinition;
            }

            static ElementType ImportElementType(Type type)
            {
                ElementType etype;
                if (!type_etype_mapping.TryGetValue(type, out etype))
                    return ElementType.None;

                return etype;
            }

            AssemblyNameReference ImportScope(SR.Assembly assembly)
            {
                AssemblyNameReference scope;
                var name = assembly.GetName();

                if (TryGetAssemblyNameReference(name, out scope))
                    return scope;

                scope = new AssemblyNameReference(name.Name, name.Version)
                {
                    Culture = name.CultureInfo.Name,
                    PublicKeyToken = name.GetPublicKeyToken(),
                    HashAlgorithm = (AssemblyHashAlgorithm)name.HashAlgorithm,
                };

                module.AssemblyReferences.Add(scope);

                return scope;
            }

            bool TryGetAssemblyNameReference(SR.AssemblyName name, out AssemblyNameReference assembly_reference)
            {
                var references = module.AssemblyReferences;

                for (int i = 0; i < references.Count; i++)
                {
                    var reference = references[i];
                    if (name.FullName != reference.FullName)
                        continue;

                    assembly_reference = reference;
                    return true;
                }

                assembly_reference = null;
                return false;
            }

            public FieldReference ImportField(SR.FieldInfo field, IGenericContext context)
            {
                var declaring_type = ImportType(field.DeclaringType, context);

                if (IsGenericInstance(field.DeclaringType))
                    field = ResolveFieldDefinition(field);

                return new FieldReference
                {
                    Name = field.Name,
                    DeclaringType = declaring_type,
                    FieldType = ImportType(field.FieldType, context ?? declaring_type),
                };
            }

            static SR.FieldInfo ResolveFieldDefinition(SR.FieldInfo field)
            {
                return field.Module.ResolveField(field.MetadataToken);
            }

            public MethodReference ImportMethod(SR.MethodBase method, IGenericContext context, ImportGenericKind import_kind)
            {
                if (IsMethodSpecification(method) || ImportOpenGenericMethod(method, import_kind))
                    return ImportMethodSpecification(method, context);

                var declaring_type = ImportType(method.DeclaringType, context);

                if (IsGenericInstance(method.DeclaringType))
                    method = method.Module.ResolveMethod(method.MetadataToken);

                var reference = new MethodReference
                {
                    Name = method.Name,
                    HasThis = HasCallingConvention(method, SR.CallingConventions.HasThis),
                    ExplicitThis = HasCallingConvention(method, SR.CallingConventions.ExplicitThis),
                    DeclaringType = ImportType(method.DeclaringType, context, ImportGenericKind.Definition),
                };

                if (HasCallingConvention(method, SR.CallingConventions.VarArgs))
                    reference.CallingConvention &= MethodCallingConvention.VarArg;

                if (method.IsGenericMethod)
                    ImportGenericParameters(reference, method.GetGenericArguments());

                var method_info = method as SR.MethodInfo;
                reference.ReturnType = method_info != null
                    ? ImportType(method_info.ReturnType, context ?? reference)
                    : ImportType(typeof(void), null);

                var parameters = method.GetParameters();
                var reference_parameters = reference.Parameters;

                for (int i = 0; i < parameters.Length; i++)
                    reference_parameters.Add(
                        new ParameterDefinition(ImportType(parameters[i].ParameterType, context ?? reference)));

                reference.DeclaringType = declaring_type;

                return reference;
            }

            static void ImportGenericParameters(IGenericParameterProvider provider, Type[] arguments)
            {
                var provider_parameters = provider.GenericParameters;

                for (int i = 0; i < arguments.Length; i++)
                    provider_parameters.Add(new GenericParameter(arguments[i].Name, provider));
            }

            static bool IsMethodSpecification(SR.MethodBase method)
            {
                return method.IsGenericMethod && !method.IsGenericMethodDefinition;
            }

            MethodReference ImportMethodSpecification(SR.MethodBase method, IGenericContext context)
            {
                var method_info = method as SR.MethodInfo;
                if (method_info == null)
                    throw new InvalidOperationException();

                var element_method = ImportMethod(method_info.GetGenericMethodDefinition(), context, ImportGenericKind.Definition);
                var instance = new GenericInstanceMethod(element_method);
                var arguments = method.GetGenericArguments();
                var instance_arguments = instance.GenericArguments;

                for (int i = 0; i < arguments.Length; i++)
                    instance_arguments.Add(ImportType(arguments[i], context ?? element_method));

                return instance;
            }

            static bool HasCallingConvention(SR.MethodBase method, SR.CallingConventions conventions)
            {
                return (method.CallingConvention & conventions) != 0;
            }

            public TypeReference ImportType(TypeReference type, IGenericContext context)
            {
                if (type.IsTypeSpecification())
                    return ImportTypeSpecification(type, context);

                var reference = new TypeReference(
                    type.Namespace,
                    type.Name,
                    module,
                    ImportScope(type.Scope),
                    type.IsValueType);

                MetadataSystem.TryProcessPrimitiveType(reference);

                if (type.IsNested)
                    reference.DeclaringType = ImportType(type.DeclaringType, context);

                if (type.HasGenericParameters)
                    ImportGenericParameters(reference, type);

                return reference;
            }

            IMetadataScope ImportScope(IMetadataScope scope)
            {
                switch (scope.MetadataScopeType)
                {
                    case MetadataScopeType.AssemblyNameReference:
                        return ImportAssemblyName((AssemblyNameReference)scope);
                    case MetadataScopeType.ModuleDefinition:
                        return ImportAssemblyName(((ModuleDefinition)scope).Assembly.Name);
                    case MetadataScopeType.ModuleReference:
                        throw new NotImplementedException();
                }

                throw new NotSupportedException();
            }

            AssemblyNameReference ImportAssemblyName(AssemblyNameReference name)
            {
                AssemblyNameReference reference;
                if (TryGetAssemblyNameReference(name, out reference))
                    return reference;

                reference = new AssemblyNameReference(name.Name, name.Version)
                {
                    Culture = name.Culture,
                    HashAlgorithm = name.HashAlgorithm,
                };

                var pk_token = !name.PublicKeyToken.IsNullOrEmpty()
                    ? new byte[name.PublicKeyToken.Length]
                    : Empty<byte>.Array;

                if (pk_token.Length > 0)
                    Buffer.BlockCopy(name.PublicKeyToken, 0, pk_token, 0, pk_token.Length);

                reference.PublicKeyToken = pk_token;

                module.AssemblyReferences.Add(reference);

                return reference;
            }

            bool TryGetAssemblyNameReference(AssemblyNameReference name_reference, out AssemblyNameReference assembly_reference)
            {
                var references = module.AssemblyReferences;

                for (int i = 0; i < references.Count; i++)
                {
                    var reference = references[i];
                    if (name_reference.FullName != reference.FullName)
                        continue;

                    assembly_reference = reference;
                    return true;
                }

                assembly_reference = null;
                return false;
            }

            static void ImportGenericParameters(IGenericParameterProvider imported, IGenericParameterProvider original)
            {
                var parameters = original.GenericParameters;
                var imported_parameters = imported.GenericParameters;

                for (int i = 0; i < parameters.Count; i++)
                    imported_parameters.Add(new GenericParameter(parameters[i].Name, imported));
            }

            TypeReference ImportTypeSpecification(TypeReference type, IGenericContext context)
            {
                switch (type.etype)
                {
                    case ElementType.SzArray:
                        var vector = (ArrayType)type;
                        return new ArrayType(ImportType(vector.ElementType, context));
                    case ElementType.Ptr:
                        var pointer = (PointerType)type;
                        return new PointerType(ImportType(pointer.ElementType, context));
                    case ElementType.ByRef:
                        var byref = (ByReferenceType)type;
                        return new ByReferenceType(ImportType(byref.ElementType, context));
                    case ElementType.Pinned:
                        var pinned = (PinnedType)type;
                        return new PinnedType(ImportType(pinned.ElementType, context));
                    case ElementType.Sentinel:
                        var sentinel = (SentinelType)type;
                        return new SentinelType(ImportType(sentinel.ElementType, context));
                    case ElementType.CModOpt:
                        var modopt = (OptionalModifierType)type;
                        return new OptionalModifierType(
                            ImportType(modopt.ModifierType, context),
                            ImportType(modopt.ElementType, context));
                    case ElementType.CModReqD:
                        var modreq = (RequiredModifierType)type;
                        return new RequiredModifierType(
                            ImportType(modreq.ModifierType, context),
                            ImportType(modreq.ElementType, context));
                    case ElementType.Array:
                        var array = (ArrayType)type;
                        var imported_array = new ArrayType(ImportType(array.ElementType, context));
                        if (array.IsVector)
                            return imported_array;

                        var dimensions = array.Dimensions;
                        var imported_dimensions = imported_array.Dimensions;

                        imported_dimensions.Clear();

                        for (int i = 0; i < dimensions.Count; i++)
                        {
                            var dimension = dimensions[i];

                            imported_dimensions.Add(new ArrayDimension(dimension.LowerBound, dimension.UpperBound));
                        }

                        return imported_array;
                    case ElementType.GenericInst:
                        var instance = (GenericInstanceType)type;
                        var element_type = ImportType(instance.ElementType, context);
                        var imported_instance = new GenericInstanceType(element_type);

                        var arguments = instance.GenericArguments;
                        var imported_arguments = imported_instance.GenericArguments;

                        for (int i = 0; i < arguments.Count; i++)
                            imported_arguments.Add(ImportType(arguments[i], context));

                        return imported_instance;
                    case ElementType.Var:
                        if (context == null || context.Type == null)
                            throw new InvalidOperationException();

                        return ((TypeReference)context.Type).GetElementType().GenericParameters[((GenericParameter)type).Position];
                    case ElementType.MVar:
                        if (context == null || context.Method == null)
                            throw new InvalidOperationException();

                        return context.Method.GenericParameters[((GenericParameter)type).Position];
                }

                throw new NotSupportedException(type.etype.ToString());
            }

            public FieldReference ImportField(FieldReference field, IGenericContext context)
            {
                var declaring_type = ImportType(field.DeclaringType, context);

                return new FieldReference
                {
                    Name = field.Name,
                    DeclaringType = declaring_type,
                    FieldType = ImportType(field.FieldType, context ?? declaring_type),
                };
            }

            public MethodReference ImportMethod(MethodReference method, IGenericContext context)
            {
                if (method.IsGenericInstance)
                    return ImportMethodSpecification(method, context);

                var declaring_type = ImportType(method.DeclaringType, context);

                var reference = new MethodReference
                {
                    Name = method.Name,
                    HasThis = method.HasThis,
                    ExplicitThis = method.ExplicitThis,
                    DeclaringType = declaring_type,
                };

                reference.CallingConvention = method.CallingConvention;

                if (method.HasGenericParameters)
                    ImportGenericParameters(reference, method);

                reference.ReturnType = ImportType(method.ReturnType, context ?? reference);

                if (!method.HasParameters)
                    return reference;

                var reference_parameters = reference.Parameters;

                var parameters = method.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                    reference_parameters.Add(
                        new ParameterDefinition(ImportType(parameters[i].ParameterType, context ?? reference)));

                return reference;
            }

            MethodSpecification ImportMethodSpecification(MethodReference method, IGenericContext context)
            {
                if (!method.IsGenericInstance)
                    throw new NotSupportedException();

                var instance = (GenericInstanceMethod)method;
                var element_method = ImportMethod(instance.ElementMethod, context);
                var imported_instance = new GenericInstanceMethod(element_method);

                var arguments = instance.GenericArguments;
                var imported_arguments = imported_instance.GenericArguments;

                for (int i = 0; i < arguments.Count; i++)
                    imported_arguments.Add(ImportType(arguments[i], context));

                return imported_instance;
            }
        }



        public sealed class LinkedResource : Resource
        {

            internal byte[] hash;
            string file;

            public byte[] Hash
            {
                get { return hash; }
            }

            public string File
            {
                get { return file; }
                set { file = value; }
            }

            public override ResourceType ResourceType
            {
                get { return ResourceType.Linked; }
            }

            public LinkedResource(string name, ManifestResourceAttributes flags)
                : base(name, flags)
            {
            }

            public LinkedResource(string name, ManifestResourceAttributes flags, string file)
                : base(name, flags)
            {
                this.file = file;
            }
        }


        [Flags]
        public enum ManifestResourceAttributes : uint
        {
            VisibilityMask = 0x0007,
            Public = 0x0001,
            Private = 0x0002
        }


        public class MarshalInfo
        {

            internal NativeType native;

            public NativeType NativeType
            {
                get { return native; }
                set { native = value; }
            }

            public MarshalInfo(NativeType native)
            {
                this.native = native;
            }
        }

        public sealed class ArrayMarshalInfo : MarshalInfo
        {

            internal NativeType element_type;
            internal int size_parameter_index;
            internal int size;
            internal int size_parameter_multiplier;

            public NativeType ElementType
            {
                get { return element_type; }
                set { element_type = value; }
            }

            public int SizeParameterIndex
            {
                get { return size_parameter_index; }
                set { size_parameter_index = value; }
            }

            public int Size
            {
                get { return size; }
                set { size = value; }
            }

            public int SizeParameterMultiplier
            {
                get { return size_parameter_multiplier; }
                set { size_parameter_multiplier = value; }
            }

            public ArrayMarshalInfo()
                : base(NativeType.Array)
            {
                element_type = NativeType.None;
                size_parameter_index = -1;
                size = -1;
                size_parameter_multiplier = -1;
            }
        }

        public sealed class CustomMarshalInfo : MarshalInfo
        {

            internal Guid guid;
            internal string unmanaged_type;
            internal TypeReference managed_type;
            internal string cookie;

            public Guid Guid
            {
                get { return guid; }
                set { guid = value; }
            }

            public string UnmanagedType
            {
                get { return unmanaged_type; }
                set { unmanaged_type = value; }
            }

            public TypeReference ManagedType
            {
                get { return managed_type; }
                set { managed_type = value; }
            }

            public string Cookie
            {
                get { return cookie; }
                set { cookie = value; }
            }

            public CustomMarshalInfo()
                : base(NativeType.CustomMarshaler)
            {
            }
        }

        public sealed class SafeArrayMarshalInfo : MarshalInfo
        {

            internal VariantType element_type;

            public VariantType ElementType
            {
                get { return element_type; }
                set { element_type = value; }
            }

            public SafeArrayMarshalInfo()
                : base(NativeType.SafeArray)
            {
                element_type = VariantType.None;
            }
        }

        public sealed class FixedArrayMarshalInfo : MarshalInfo
        {

            internal NativeType element_type;
            internal int size;

            public NativeType ElementType
            {
                get { return element_type; }
                set { element_type = value; }
            }

            public int Size
            {
                get { return size; }
                set { size = value; }
            }

            public FixedArrayMarshalInfo()
                : base(NativeType.FixedArray)
            {
                element_type = NativeType.None;
            }
        }

        public sealed class FixedSysStringMarshalInfo : MarshalInfo
        {

            internal int size;

            public int Size
            {
                get { return size; }
                set { size = value; }
            }

            public FixedSysStringMarshalInfo()
                : base(NativeType.FixedSysString)
            {
                size = -1;
            }
        }



        class MemberDefinitionCollection<T> : Collection<T> where T : IMemberDefinition
        {

            TypeDefinition container;

            internal MemberDefinitionCollection(TypeDefinition container)
            {
                this.container = container;
            }

            internal MemberDefinitionCollection(TypeDefinition container, int capacity)
                : base(capacity)
            {
                this.container = container;
            }

            protected override void OnAdd(T item, int index)
            {
                Attach(item);
            }

            protected sealed override void OnSet(T item, int index)
            {
                Attach(item);
            }

            protected sealed override void OnInsert(T item, int index)
            {
                Attach(item);
            }

            protected sealed override void OnRemove(T item, int index)
            {
                Detach(item);
            }

            protected sealed override void OnClear()
            {
                foreach (var definition in this)
                    Detach(definition);
            }

            void Attach(T element)
            {
                if (element.DeclaringType == container)
                    return;

                if (element.DeclaringType != null)
                    throw new ArgumentException("Member already attached");

                element.DeclaringType = this.container;
            }

            static void Detach(T element)
            {
                element.DeclaringType = null;
            }
        }



        public abstract class MemberReference : IMetadataTokenProvider
        {

            string name;
            TypeReference declaring_type;

            internal MetadataToken token;

            public virtual string Name
            {
                get { return name; }
                set { name = value; }
            }

            public abstract string FullName
            {
                get;
            }

            public virtual TypeReference DeclaringType
            {
                get { return declaring_type; }
                set { declaring_type = value; }
            }

            public MetadataToken MetadataToken
            {
                get { return token; }
                set { token = value; }
            }

            internal bool HasImage
            {
                get
                {
                    var module = Module;
                    if (module == null)
                        return false;

                    return module.HasImage;
                }
            }

            public virtual ModuleDefinition Module
            {
                get { return declaring_type != null ? declaring_type.Module : null; }
            }

            public virtual bool IsDefinition
            {
                get { return false; }
            }

            internal virtual bool ContainsGenericParameter
            {
                get { return declaring_type != null && declaring_type.ContainsGenericParameter; }
            }

            internal MemberReference()
            {
            }

            internal MemberReference(string name)
            {
                this.name = name ?? string.Empty;
            }

            internal string MemberFullName()
            {
                if (declaring_type == null)
                    return name;

                return declaring_type.FullName + "::" + name;
            }

            public override string ToString()
            {
                return FullName;
            }
        }



        public interface IAssemblyResolver
        {
            AssemblyDefinition Resolve(AssemblyNameReference name);
            AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters);

            AssemblyDefinition Resolve(string fullName);
            AssemblyDefinition Resolve(string fullName, ReaderParameters parameters);
        }

        public class ResolutionException : Exception
        {

            readonly MemberReference member;

            public MemberReference Member
            {
                get { return member; }
            }

            public ResolutionException(MemberReference member)
                : base("Failed to resolve " + member.FullName)
            {
                this.member = member;
            }
        }

        static class MetadataResolver
        {

            public static TypeDefinition Resolve(IAssemblyResolver resolver, TypeReference type)
            {
                type = type.GetElementType();

                var scope = type.Scope;
                switch (scope.MetadataScopeType)
                {
                    case MetadataScopeType.AssemblyNameReference:
                        var assembly = resolver.Resolve((AssemblyNameReference)scope);
                        if (assembly == null)
                            return null;

                        return GetType(assembly.MainModule, type);
                    case MetadataScopeType.ModuleDefinition:
                        return GetType((ModuleDefinition)scope, type);
                    case MetadataScopeType.ModuleReference:
                        var modules = type.Module.Assembly.Modules;
                        var module_ref = (ModuleReference)scope;
                        for (int i = 0; i < modules.Count; i++)
                        {
                            var netmodule = modules[i];
                            if (netmodule.Name == module_ref.Name)
                                return GetType(netmodule, type);
                        }
                        break;
                }

                throw new NotSupportedException();
            }

            static TypeDefinition GetType(ModuleDefinition module, TypeReference type)
            {
                if (!type.IsNested)
                    return module.GetType(type.Namespace, type.Name);

                var declaring_type = type.DeclaringType.Resolve();
                if (declaring_type == null)
                    return null;

                return declaring_type.GetNestedType(type.Name);
            }

            public static FieldDefinition Resolve(IAssemblyResolver resolver, FieldReference field)
            {
                var type = Resolve(resolver, field.DeclaringType);
                if (type == null)
                    return null;

                if (!type.HasFields)
                    return null;

                return GetField(resolver, type, field);
            }

            static FieldDefinition GetField(IAssemblyResolver resolver, TypeDefinition type, FieldReference reference)
            {
                while (type != null)
                {
                    var field = GetField(type.Fields, reference);
                    if (field != null)
                        return field;

                    if (type.BaseType == null)
                        return null;

                    type = Resolve(resolver, type.BaseType);
                }

                return null;
            }

            static FieldDefinition GetField(IList<FieldDefinition> fields, FieldReference reference)
            {
                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];

                    if (field.Name != reference.Name)
                        continue;

                    if (!AreSame(field.FieldType, reference.FieldType))
                        continue;

                    return field;
                }

                return null;
            }

            public static MethodDefinition Resolve(IAssemblyResolver resolver, MethodReference method)
            {
                var type = Resolve(resolver, method.DeclaringType);
                if (type == null)
                    return null;

                method = method.GetElementMethod();

                if (!type.HasMethods)
                    return null;

                return GetMethod(resolver, type, method);
            }

            static MethodDefinition GetMethod(IAssemblyResolver resolver, TypeDefinition type, MethodReference reference)
            {
                while (type != null)
                {
                    var method = GetMethod(type.Methods, reference);
                    if (method != null)
                        return method;

                    if (type.BaseType == null)
                        return null;

                    type = Resolve(resolver, type.BaseType);
                }

                return null;
            }

            public static MethodDefinition GetMethod(IList<MethodDefinition> methods, MethodReference reference)
            {
                for (int i = 0; i < methods.Count; i++)
                {
                    var method = methods[i];

                    if (method.Name != reference.Name)
                        continue;

                    if (!AreSame(method.ReturnType, reference.ReturnType))
                        continue;

                    if (method.HasParameters != reference.HasParameters)
                        continue;

                    if (!method.HasParameters && !reference.HasParameters)
                        return method;

                    if (!AreSame(method.Parameters, reference.Parameters))
                        continue;

                    return method;
                }

                return null;
            }

            static bool AreSame(Collection<ParameterDefinition> a, Collection<ParameterDefinition> b)
            {
                var count = a.Count;

                if (count != b.Count)
                    return false;

                if (count == 0)
                    return true;

                for (int i = 0; i < count; i++)
                    if (!AreSame(a[i].ParameterType, b[i].ParameterType))
                        return false;

                return true;
            }

            static bool AreSame(TypeSpecification a, TypeSpecification b)
            {
                if (!AreSame(a.ElementType, b.ElementType))
                    return false;

                if (a.IsGenericInstance)
                    return AreSame((GenericInstanceType)a, (GenericInstanceType)b);

                if (a.IsRequiredModifier || a.IsOptionalModifier)
                    return AreSame((IModifierType)a, (IModifierType)b);

                if (a.IsArray)
                    return AreSame((ArrayType)a, (ArrayType)b);

                return true;
            }

            static bool AreSame(ArrayType a, ArrayType b)
            {
                if (a.Rank != b.Rank)
                    return false;

                return true;
            }

            static bool AreSame(IModifierType a, IModifierType b)
            {
                return AreSame(a.ModifierType, b.ModifierType);
            }

            static bool AreSame(GenericInstanceType a, GenericInstanceType b)
            {
                if (!a.HasGenericArguments)
                    return !b.HasGenericArguments;

                if (!b.HasGenericArguments)
                    return false;

                if (a.GenericArguments.Count != b.GenericArguments.Count)
                    return false;

                for (int i = 0; i < a.GenericArguments.Count; i++)
                    if (!AreSame(a.GenericArguments[i], b.GenericArguments[i]))
                        return false;

                return true;
            }

            static bool AreSame(GenericParameter a, GenericParameter b)
            {
                return a.Position == b.Position;
            }

            static bool AreSame(TypeReference a, TypeReference b)
            {
                if (a.etype != b.etype)
                    return false;

                if (a.IsGenericParameter)
                    return AreSame((GenericParameter)a, (GenericParameter)b);

                if (a.IsTypeSpecification())
                    return AreSame((TypeSpecification)a, (TypeSpecification)b);

                return a.FullName == b.FullName;
            }
        }



        struct Range
        {
            public uint Start;
            public uint Length;

            public Range(uint index, uint length)
            {
                this.Start = index;
                this.Length = length;
            }
        }

        sealed class MetadataSystem
        {

            internal TypeDefinition[] Types;
            internal TypeReference[] TypeReferences;

            internal FieldDefinition[] Fields;
            internal MethodDefinition[] Methods;
            internal MemberReference[] MemberReferences;

            internal Dictionary<uint, uint[]> NestedTypes;
            internal Dictionary<uint, uint> ReverseNestedTypes;
            internal Dictionary<uint, MetadataToken[]> Interfaces;
            internal Dictionary<uint, Row<ushort, uint>> ClassLayouts;
            internal Dictionary<uint, uint> FieldLayouts;
            internal Dictionary<uint, uint> FieldRVAs;
            internal Dictionary<MetadataToken, uint> FieldMarshals;
            internal Dictionary<MetadataToken, Row<ElementType, uint>> Constants;
            internal Dictionary<uint, MetadataToken[]> Overrides;
            internal Dictionary<MetadataToken, Range> CustomAttributes;
            internal Dictionary<MetadataToken, Range> SecurityDeclarations;
            internal Dictionary<uint, Range> Events;
            internal Dictionary<uint, Range> Properties;
            internal Dictionary<uint, Row<MethodSemanticsAttributes, MetadataToken>> Semantics;
            internal Dictionary<uint, Row<PInvokeAttributes, uint, uint>> PInvokes;
            internal Dictionary<MetadataToken, Range> GenericParameters;
            internal Dictionary<uint, MetadataToken[]> GenericConstraints;

            static Dictionary<string, Row<ElementType, bool>> primitive_value_types;

            static void InitializePrimitives()
            {
                primitive_value_types = new Dictionary<string, Row<ElementType, bool>>(18) {
				{ "Void", new Row<ElementType, bool> (ElementType.Void, false) },
				{ "Boolean", new Row<ElementType, bool> (ElementType.Boolean, true) },
				{ "Char", new Row<ElementType, bool> (ElementType.Char, true) },
				{ "SByte", new Row<ElementType, bool> (ElementType.I1, true) },
				{ "Byte", new Row<ElementType, bool> (ElementType.U1, true) },
				{ "Int16", new Row<ElementType, bool> (ElementType.I2, true) },
				{ "UInt16", new Row<ElementType, bool> (ElementType.U2, true) },
				{ "Int32", new Row<ElementType, bool> (ElementType.I4, true) },
				{ "UInt32", new Row<ElementType, bool> (ElementType.U4, true) },
				{ "Int64", new Row<ElementType, bool> (ElementType.I8, true) },
				{ "UInt64", new Row<ElementType, bool> (ElementType.U8, true) },
				{ "Single", new Row<ElementType, bool> (ElementType.R4, true) },
				{ "Double", new Row<ElementType, bool> (ElementType.R8, true) },
				{ "String", new Row<ElementType, bool> (ElementType.String, false) },
				{ "TypedReference", new Row<ElementType, bool> (ElementType.TypedByRef, false) },
				{ "IntPtr", new Row<ElementType, bool> (ElementType.I, true) },
				{ "UIntPtr", new Row<ElementType, bool> (ElementType.U, true) },
				{ "Object", new Row<ElementType, bool> (ElementType.Object, false) },
			};
            }

            public static void TryProcessPrimitiveType(TypeReference type)
            {
                var scope = type.scope;
                if (scope == null)
                    return;

                if (scope.MetadataScopeType != MetadataScopeType.AssemblyNameReference)
                    return;

                if (scope.Name != "mscorlib")
                    return;

                if (type.Namespace != "System")
                    return;

                if (primitive_value_types == null)
                    InitializePrimitives();

                Row<ElementType, bool> primitive_data;
                if (!primitive_value_types.TryGetValue(type.Name, out primitive_data))
                    return;

                type.etype = primitive_data.Col1;
                type.IsValueType = primitive_data.Col2;
            }

            public void Clear()
            {
                if (NestedTypes != null) NestedTypes.Clear();
                if (ReverseNestedTypes != null) ReverseNestedTypes.Clear();
                if (Interfaces != null) Interfaces.Clear();
                if (ClassLayouts != null) ClassLayouts.Clear();
                if (FieldLayouts != null) FieldLayouts.Clear();
                if (FieldRVAs != null) FieldRVAs.Clear();
                if (FieldMarshals != null) FieldMarshals.Clear();
                if (Constants != null) Constants.Clear();
                if (Overrides != null) Overrides.Clear();
                if (CustomAttributes != null) CustomAttributes.Clear();
                if (SecurityDeclarations != null) SecurityDeclarations.Clear();
                if (Events != null) Events.Clear();
                if (Properties != null) Properties.Clear();
                if (Semantics != null) Semantics.Clear();
                if (PInvokes != null) PInvokes.Clear();
                if (GenericParameters != null) GenericParameters.Clear();
                if (GenericConstraints != null) GenericConstraints.Clear();
            }

            public TypeDefinition GetTypeDefinition(uint rid)
            {
                if (rid < 1 || rid > Types.Length)
                    return null;

                return Types[rid - 1];
            }

            public void AddTypeDefinition(TypeDefinition type)
            {
                Types[type.token.RID - 1] = type;
            }

            public TypeReference GetTypeReference(uint rid)
            {
                if (rid < 1 || rid > TypeReferences.Length)
                    return null;

                return TypeReferences[rid - 1];
            }

            public void AddTypeReference(TypeReference type)
            {
                TypeReferences[type.token.RID - 1] = type;
            }

            public FieldDefinition GetFieldDefinition(uint rid)
            {
                if (rid < 1 || rid > Fields.Length)
                    return null;

                return Fields[rid - 1];
            }

            public void AddFieldDefinition(FieldDefinition field)
            {
                Fields[field.token.RID - 1] = field;
            }

            public MethodDefinition GetMethodDefinition(uint rid)
            {
                if (rid < 1 || rid > Methods.Length)
                    return null;

                return Methods[rid - 1];
            }

            public void AddMethodDefinition(MethodDefinition method)
            {
                Methods[method.token.RID - 1] = method;
            }

            public MemberReference GetMemberReference(uint rid)
            {
                if (rid < 1 || rid > MemberReferences.Length)
                    return null;

                return MemberReferences[rid - 1];
            }

            public void AddMemberReference(MemberReference member)
            {
                MemberReferences[member.token.RID - 1] = member;
            }

            public bool TryGetNestedTypeMapping(TypeDefinition type, out uint[] mapping)
            {
                return NestedTypes.TryGetValue(type.token.RID, out mapping);
            }

            public void SetNestedTypeMapping(uint type_rid, uint[] mapping)
            {
                NestedTypes[type_rid] = mapping;
            }

            public void RemoveNestedTypeMapping(TypeDefinition type)
            {
                NestedTypes.Remove(type.token.RID);
            }

            public bool TryGetReverseNestedTypeMapping(TypeDefinition type, out uint declaring)
            {
                return ReverseNestedTypes.TryGetValue(type.token.RID, out declaring);
            }

            public void SetReverseNestedTypeMapping(uint nested, uint declaring)
            {
                ReverseNestedTypes.Add(nested, declaring);
            }

            public void RemoveReverseNestedTypeMapping(TypeDefinition type)
            {
                ReverseNestedTypes.Remove(type.token.RID);
            }

            public bool TryGetInterfaceMapping(TypeDefinition type, out MetadataToken[] mapping)
            {
                return Interfaces.TryGetValue(type.token.RID, out mapping);
            }

            public void SetInterfaceMapping(uint type_rid, MetadataToken[] mapping)
            {
                Interfaces[type_rid] = mapping;
            }

            public void RemoveInterfaceMapping(TypeDefinition type)
            {
                Interfaces.Remove(type.token.RID);
            }

            public void AddPropertiesRange(uint type_rid, Range range)
            {
                Properties.Add(type_rid, range);
            }

            public bool TryGetPropertiesRange(TypeDefinition type, out Range range)
            {
                return Properties.TryGetValue(type.token.RID, out range);
            }

            public void RemovePropertiesRange(TypeDefinition type)
            {
                Properties.Remove(type.token.RID);
            }

            public void AddEventsRange(uint type_rid, Range range)
            {
                Events.Add(type_rid, range);
            }

            public bool TryGetEventsRange(TypeDefinition type, out Range range)
            {
                return Events.TryGetValue(type.token.RID, out range);
            }

            public void RemoveEventsRange(TypeDefinition type)
            {
                Events.Remove(type.token.RID);
            }

            public bool TryGetGenericParameterRange(IGenericParameterProvider owner, out Range range)
            {
                return GenericParameters.TryGetValue(owner.MetadataToken, out range);
            }

            public void RemoveGenericParameterRange(IGenericParameterProvider owner)
            {
                GenericParameters.Remove(owner.MetadataToken);
            }

            public bool TryGetCustomAttributeRange(ICustomAttributeProvider owner, out Range range)
            {
                return CustomAttributes.TryGetValue(owner.MetadataToken, out range);
            }

            public void RemoveCustomAttributeRange(ICustomAttributeProvider owner)
            {
                CustomAttributes.Remove(owner.MetadataToken);
            }

            public bool TryGetSecurityDeclarationRange(ISecurityDeclarationProvider owner, out Range range)
            {
                return SecurityDeclarations.TryGetValue(owner.MetadataToken, out range);
            }

            public void RemoveSecurityDeclarationRange(ISecurityDeclarationProvider owner)
            {
                SecurityDeclarations.Remove(owner.MetadataToken);
            }

            public bool TryGetGenericConstraintMapping(GenericParameter generic_parameter, out MetadataToken[] mapping)
            {
                return GenericConstraints.TryGetValue(generic_parameter.token.RID, out mapping);
            }

            public void SetGenericConstraintMapping(uint gp_rid, MetadataToken[] mapping)
            {
                GenericConstraints[gp_rid] = mapping;
            }

            public void RemoveGenericConstraintMapping(GenericParameter generic_parameter)
            {
                GenericConstraints.Remove(generic_parameter.token.RID);
            }

            public bool TryGetOverrideMapping(MethodDefinition method, out MetadataToken[] mapping)
            {
                return Overrides.TryGetValue(method.token.RID, out mapping);
            }

            public void SetOverrideMapping(uint rid, MetadataToken[] mapping)
            {
                Overrides[rid] = mapping;
            }

            public void RemoveOverrideMapping(MethodDefinition method)
            {
                Overrides.Remove(method.token.RID);
            }

            public TypeDefinition GetFieldDeclaringType(uint field_rid)
            {
                return BinaryRangeSearch(Types, field_rid, true);
            }

            public TypeDefinition GetMethodDeclaringType(uint method_rid)
            {
                return BinaryRangeSearch(Types, method_rid, false);
            }

            static TypeDefinition BinaryRangeSearch(TypeDefinition[] types, uint rid, bool field)
            {
                int min = 0;
                int max = types.Length - 1;
                while (min <= max)
                {
                    int mid = min + ((max - min) / 2);
                    var type = types[mid];
                    var range = field ? type.fields_range : type.methods_range;

                    if (rid < range.Start)
                        max = mid - 1;
                    else if (rid >= range.Start + range.Length)
                        min = mid + 1;
                    else
                        return type;
                }

                return null;
            }
        }



        public struct MetadataToken
        {

            readonly uint token;

            public uint RID
            {
                get { return token & 0x00ffffff; }
            }

            public TokenType TokenType
            {
                get { return (TokenType)(token & 0xff000000); }
            }

            public static readonly MetadataToken Zero = new MetadataToken((uint)0);

            public MetadataToken(uint token)
            {
                this.token = token;
            }

            public MetadataToken(TokenType type)
                : this(type, 0)
            {
            }

            public MetadataToken(TokenType type, uint rid)
            {
                token = (uint)type | rid;
            }

            public MetadataToken(TokenType type, int rid)
            {
                token = (uint)type | (uint)rid;
            }

            public int ToInt32()
            {
                return (int)token;
            }

            public uint ToUInt32()
            {
                return token;
            }

            public override int GetHashCode()
            {
                return (int)token;
            }

            public override bool Equals(object obj)
            {
                if (obj is MetadataToken)
                {
                    var other = (MetadataToken)obj;
                    return other.token == token;
                }

                return false;
            }

            public static bool operator ==(MetadataToken one, MetadataToken other)
            {
                return one.token == other.token;
            }

            public static bool operator !=(MetadataToken one, MetadataToken other)
            {
                return one.token != other.token;
            }

            public override string ToString()
            {
                return string.Format("[{0}:0x{1}]", TokenType, RID.ToString("x4"));
            }
        }



        [Flags]
        public enum MethodAttributes : ushort
        {
            MemberAccessMask = 0x0007,
            CompilerControlled = 0x0000,
            Private = 0x0001,
            FamANDAssem = 0x0002,
            Assembly = 0x0003,
            Family = 0x0004,
            FamORAssem = 0x0005,
            Public = 0x0006,
            Static = 0x0010,
            Final = 0x0020,
            Virtual = 0x0040,
            HideBySig = 0x0080,
            VtableLayoutMask = 0x0100,
            ReuseSlot = 0x0000,
            NewSlot = 0x0100,
            CheckAccessOnOverride = 0x0200,
            Abstract = 0x0400,
            SpecialName = 0x0800,
            PInvokeImpl = 0x2000,
            UnmanagedExport = 0x0008,
            RTSpecialName = 0x1000,
            HasSecurity = 0x4000,
            RequireSecObject = 0x8000
        }


        public enum MethodCallingConvention : byte
        {
            Default = 0x0,
            C = 0x1,
            StdCall = 0x2,
            ThisCall = 0x3,
            FastCall = 0x4,
            VarArg = 0x5,
            Generic = 0x10,
        }


        public sealed class MethodDefinition : MethodReference, IMemberDefinition, ISecurityDeclarationProvider
        {

            ushort attributes;
            ushort impl_attributes;
            internal MethodSemanticsAttributes? sem_attrs;
            Collection<CustomAttribute> custom_attributes;
            Collection<SecurityDeclaration> security_declarations;

            internal RVA rva;
            internal PInvokeInfo pinvoke;
            Collection<MethodReference> overrides;

            internal MethodBody body;

            public MethodAttributes Attributes
            {
                get { return (MethodAttributes)attributes; }
                set { attributes = (ushort)value; }
            }

            public MethodImplAttributes ImplAttributes
            {
                get { return (MethodImplAttributes)impl_attributes; }
                set { impl_attributes = (ushort)value; }
            }

            public MethodSemanticsAttributes SemanticsAttributes
            {
                get
                {
                    if (sem_attrs.HasValue)
                        return sem_attrs.Value;

                    if (HasImage)
                    {
                        ReadSemantics();
                        return sem_attrs.Value;
                    }

                    sem_attrs = MethodSemanticsAttributes.None;
                    return sem_attrs.Value;
                }
                set { sem_attrs = value; }
            }

            internal void ReadSemantics()
            {
                if (sem_attrs.HasValue)
                    return;

                var module = this.Module;
                if (module == null)
                    return;

                if (!module.HasImage)
                    return;

                module.Read(this, (method, reader) => reader.ReadAllSemantics(method));
            }

            public bool HasSecurityDeclarations
            {
                get
                {
                    if (security_declarations != null)
                        return security_declarations.Count > 0;

                    return this.GetHasSecurityDeclarations(Module);
                }
            }

            public Collection<SecurityDeclaration> SecurityDeclarations
            {
                get { return security_declarations ?? (security_declarations = this.GetSecurityDeclarations(Module)); }
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(Module);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(Module)); }
            }

            public int RVA
            {
                get { return (int)rva; }
            }

            public bool HasBody
            {
                get
                {
                    return (attributes & (ushort)MethodAttributes.Abstract) == 0 &&
                        (attributes & (ushort)MethodAttributes.PInvokeImpl) == 0 &&
                        (impl_attributes & (ushort)MethodImplAttributes.InternalCall) == 0 &&
                        (impl_attributes & (ushort)MethodImplAttributes.Native) == 0 &&
                        (impl_attributes & (ushort)MethodImplAttributes.Unmanaged) == 0 &&
                        (impl_attributes & (ushort)MethodImplAttributes.Runtime) == 0;
                }
            }

            public MethodBody Body
            {
                get
                {
                    if (body != null)
                        return body;

                    if (!HasBody)
                        return null;

                    if (HasImage && rva != 0)
                        return body = Module.Read(this, (method, reader) => reader.ReadMethodBody(method));

                    return body = new MethodBody(this);
                }
                set { body = value; }
            }

            public bool HasPInvokeInfo
            {
                get
                {
                    if (pinvoke != null)
                        return true;

                    return IsPInvokeImpl;
                }
            }

            public PInvokeInfo PInvokeInfo
            {
                get
                {
                    if (pinvoke != null)
                        return pinvoke;

                    if (HasImage && IsPInvokeImpl)
                        return pinvoke = Module.Read(this, (method, reader) => reader.ReadPInvokeInfo(method));

                    return null;
                }
                set
                {
                    IsPInvokeImpl = true;
                    pinvoke = value;
                }
            }

            public bool HasOverrides
            {
                get
                {
                    if (overrides != null)
                        return overrides.Count > 0;

                    if (HasImage)
                        return Module.Read(this, (method, reader) => reader.HasOverrides(method));

                    return false;
                }
            }

            public Collection<MethodReference> Overrides
            {
                get
                {
                    if (overrides != null)
                        return overrides;

                    if (HasImage)
                        return overrides = Module.Read(this, (method, reader) => reader.ReadOverrides(method));

                    return overrides = new Collection<MethodReference>();
                }
            }

            public override bool HasGenericParameters
            {
                get
                {
                    if (generic_parameters != null)
                        return generic_parameters.Count > 0;

                    return this.GetHasGenericParameters(Module);
                }
            }

            public override Collection<GenericParameter> GenericParameters
            {
                get { return generic_parameters ?? (generic_parameters = this.GetGenericParameters(Module)); }
            }

            public bool IsCompilerControlled
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.CompilerControlled); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.CompilerControlled, value); }
            }

            public bool IsPrivate
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Private); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Private, value); }
            }

            public bool IsFamilyAndAssembly
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.FamANDAssem); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.FamANDAssem, value); }
            }

            public bool IsAssembly
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Assembly); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Assembly, value); }
            }

            public bool IsFamily
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Family); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Family, value); }
            }

            public bool IsFamilyOrAssembly
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.FamORAssem); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.FamORAssem, value); }
            }

            public bool IsPublic
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Public); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Public, value); }
            }

            public bool IsStatic
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.Static); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.Static, value); }
            }

            public bool IsFinal
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.Final); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.Final, value); }
            }

            public bool IsVirtual
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.Virtual); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.Virtual, value); }
            }

            public bool IsHideBySig
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.HideBySig); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.HideBySig, value); }
            }

            public bool IsReuseSlot
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.VtableLayoutMask, (ushort)MethodAttributes.ReuseSlot); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.VtableLayoutMask, (ushort)MethodAttributes.ReuseSlot, value); }
            }

            public bool IsNewSlot
            {
                get { return attributes.GetMaskedAttributes((ushort)MethodAttributes.VtableLayoutMask, (ushort)MethodAttributes.NewSlot); }
                set { attributes = attributes.SetMaskedAttributes((ushort)MethodAttributes.VtableLayoutMask, (ushort)MethodAttributes.NewSlot, value); }
            }

            public bool IsCheckAccessOnOverride
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.CheckAccessOnOverride); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.CheckAccessOnOverride, value); }
            }

            public bool IsAbstract
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.Abstract); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.Abstract, value); }
            }

            public bool IsSpecialName
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.SpecialName); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.SpecialName, value); }
            }

            public bool IsPInvokeImpl
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.PInvokeImpl); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.PInvokeImpl, value); }
            }

            public bool IsUnmanagedExport
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.UnmanagedExport); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.UnmanagedExport, value); }
            }

            public bool IsRuntimeSpecialName
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.RTSpecialName); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.RTSpecialName, value); }
            }

            public bool HasSecurity
            {
                get { return attributes.GetAttributes((ushort)MethodAttributes.HasSecurity); }
                set { attributes = attributes.SetAttributes((ushort)MethodAttributes.HasSecurity, value); }
            }

            public bool IsIL
            {
                get { return impl_attributes.GetMaskedAttributes((ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.IL); }
                set { impl_attributes = impl_attributes.SetMaskedAttributes((ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.IL, value); }
            }

            public bool IsNative
            {
                get { return impl_attributes.GetMaskedAttributes((ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.Native); }
                set { impl_attributes = impl_attributes.SetMaskedAttributes((ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.Native, value); }
            }

            public bool IsRuntime
            {
                get { return impl_attributes.GetMaskedAttributes((ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.Runtime); }
                set { impl_attributes = impl_attributes.SetMaskedAttributes((ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.Runtime, value); }
            }

            public bool IsUnmanaged
            {
                get { return impl_attributes.GetMaskedAttributes((ushort)MethodImplAttributes.ManagedMask, (ushort)MethodImplAttributes.Unmanaged); }
                set { impl_attributes = impl_attributes.SetMaskedAttributes((ushort)MethodImplAttributes.ManagedMask, (ushort)MethodImplAttributes.Unmanaged, value); }
            }

            public bool IsManaged
            {
                get { return impl_attributes.GetMaskedAttributes((ushort)MethodImplAttributes.ManagedMask, (ushort)MethodImplAttributes.Managed); }
                set { impl_attributes = impl_attributes.SetMaskedAttributes((ushort)MethodImplAttributes.ManagedMask, (ushort)MethodImplAttributes.Managed, value); }
            }

            public bool IsForwardRef
            {
                get { return impl_attributes.GetAttributes((ushort)MethodImplAttributes.ForwardRef); }
                set { impl_attributes = impl_attributes.SetAttributes((ushort)MethodImplAttributes.ForwardRef, value); }
            }

            public bool IsPreserveSig
            {
                get { return impl_attributes.GetAttributes((ushort)MethodImplAttributes.PreserveSig); }
                set { impl_attributes = impl_attributes.SetAttributes((ushort)MethodImplAttributes.PreserveSig, value); }
            }

            public bool IsInternalCall
            {
                get { return impl_attributes.GetAttributes((ushort)MethodImplAttributes.InternalCall); }
                set { impl_attributes = impl_attributes.SetAttributes((ushort)MethodImplAttributes.InternalCall, value); }
            }

            public bool IsSynchronized
            {
                get { return impl_attributes.GetAttributes((ushort)MethodImplAttributes.Synchronized); }
                set { impl_attributes = impl_attributes.SetAttributes((ushort)MethodImplAttributes.Synchronized, value); }
            }

            public bool NoInlining
            {
                get { return impl_attributes.GetAttributes((ushort)MethodImplAttributes.NoInlining); }
                set { impl_attributes = impl_attributes.SetAttributes((ushort)MethodImplAttributes.NoInlining, value); }
            }

            public bool NoOptimization
            {
                get { return impl_attributes.GetAttributes((ushort)MethodImplAttributes.NoOptimization); }
                set { impl_attributes = impl_attributes.SetAttributes((ushort)MethodImplAttributes.NoOptimization, value); }
            }

            public bool IsSetter
            {
                get { return this.GetSemantics(MethodSemanticsAttributes.Setter); }
                set { this.SetSemantics(MethodSemanticsAttributes.Setter, value); }
            }

            public bool IsGetter
            {
                get { return this.GetSemantics(MethodSemanticsAttributes.Getter); }
                set { this.SetSemantics(MethodSemanticsAttributes.Getter, value); }
            }

            public bool IsOther
            {
                get { return this.GetSemantics(MethodSemanticsAttributes.Other); }
                set { this.SetSemantics(MethodSemanticsAttributes.Other, value); }
            }

            public bool IsAddOn
            {
                get { return this.GetSemantics(MethodSemanticsAttributes.AddOn); }
                set { this.SetSemantics(MethodSemanticsAttributes.AddOn, value); }
            }

            public bool IsRemoveOn
            {
                get { return this.GetSemantics(MethodSemanticsAttributes.RemoveOn); }
                set { this.SetSemantics(MethodSemanticsAttributes.RemoveOn, value); }
            }

            public bool IsFire
            {
                get { return this.GetSemantics(MethodSemanticsAttributes.Fire); }
                set { this.SetSemantics(MethodSemanticsAttributes.Fire, value); }
            }

            public new TypeDefinition DeclaringType
            {
                get { return (TypeDefinition)base.DeclaringType; }
                set { base.DeclaringType = value; }
            }

            public bool IsConstructor
            {
                get
                {
                    return this.IsRuntimeSpecialName
                        && this.IsSpecialName
                        && (this.Name == ".cctor" || this.Name == ".ctor");
                }
            }

            public override bool IsDefinition
            {
                get { return true; }
            }

            internal MethodDefinition()
            {
                this.token = new MetadataToken(TokenType.Method);
            }

            public MethodDefinition(string name, MethodAttributes attributes, TypeReference returnType)
                : base(name, returnType)
            {
                this.attributes = (ushort)attributes;
                this.HasThis = !this.IsStatic;
                this.token = new MetadataToken(TokenType.Method);
            }

            public override MethodDefinition Resolve()
            {
                return this;
            }
        }

        static partial class Mixin
        {

            public static ParameterDefinition GetParameter(this MethodBody self, int index)
            {
                var method = self.method;

                if (method.HasThis)
                {
                    if (index == 0)
                        return self.ThisParameter;

                    index--;
                }

                var parameters = method.Parameters;

                if (index < 0 || index >= parameters.size)
                    return null;

                return parameters[index];
            }

            public static VariableDefinition GetVariable(this MethodBody self, int index)
            {
                var variables = self.Variables;

                if (index < 0 || index >= variables.size)
                    return null;

                return variables[index];
            }

            public static bool GetSemantics(this MethodDefinition self, MethodSemanticsAttributes semantics)
            {
                return (self.SemanticsAttributes & semantics) != 0;
            }

            public static void SetSemantics(this MethodDefinition self, MethodSemanticsAttributes semantics, bool value)
            {
                if (value)
                    self.SemanticsAttributes |= semantics;
                else
                    self.SemanticsAttributes &= ~semantics;
            }
        }



        [Flags]
        public enum MethodImplAttributes : ushort
        {
            CodeTypeMask = 0x0003,
            IL = 0x0000,
            Native = 0x0001,
            OPTIL = 0x0002,
            Runtime = 0x0003,
            ManagedMask = 0x0004,
            Unmanaged = 0x0004,
            Managed = 0x0000,
            ForwardRef = 0x0010,
            PreserveSig = 0x0080,
            InternalCall = 0x1000,
            Synchronized = 0x0020,
            NoOptimization = 0x0040,
            NoInlining = 0x0008,
            MaxMethodImplVal = 0xffff
        }


        public class MethodReference : MemberReference, IMethodSignature, IGenericParameterProvider, IGenericContext
        {

            internal ParameterDefinitionCollection parameters;
            MethodReturnType return_type;

            bool has_this;
            bool explicit_this;
            MethodCallingConvention calling_convention;
            internal Collection<GenericParameter> generic_parameters;

            public virtual bool HasThis
            {
                get { return has_this; }
                set { has_this = value; }
            }

            public virtual bool ExplicitThis
            {
                get { return explicit_this; }
                set { explicit_this = value; }
            }

            public virtual MethodCallingConvention CallingConvention
            {
                get { return calling_convention; }
                set { calling_convention = value; }
            }

            public virtual bool HasParameters
            {
                get { return !parameters.IsNullOrEmpty(); }
            }

            public virtual Collection<ParameterDefinition> Parameters
            {
                get
                {
                    if (parameters == null)
                        parameters = new ParameterDefinitionCollection(this);

                    return parameters;
                }
            }

            IGenericParameterProvider IGenericContext.Type
            {
                get
                {
                    var declaring_type = this.DeclaringType;
                    var instance = declaring_type as GenericInstanceType;
                    if (instance != null)
                        return instance.ElementType;

                    return declaring_type;
                }
            }

            IGenericParameterProvider IGenericContext.Method
            {
                get { return this; }
            }

            GenericParameterType IGenericParameterProvider.GenericParameterType
            {
                get { return GenericParameterType.Method; }
            }

            public virtual bool HasGenericParameters
            {
                get { return !generic_parameters.IsNullOrEmpty(); }
            }

            public virtual Collection<GenericParameter> GenericParameters
            {
                get
                {
                    if (generic_parameters != null)
                        return generic_parameters;

                    return generic_parameters = new Collection<GenericParameter>();
                }
            }

            public TypeReference ReturnType
            {
                get
                {
                    var return_type = MethodReturnType;
                    return return_type != null ? return_type.ReturnType : null;
                }
                set
                {
                    var return_type = MethodReturnType;
                    if (return_type != null)
                        return_type.ReturnType = value;
                }
            }

            public virtual MethodReturnType MethodReturnType
            {
                get { return return_type; }
                set { return_type = value; }
            }

            public override string FullName
            {
                get
                {
                    var builder = new StringBuilder();
                    builder.Append(ReturnType.FullName);
                    builder.Append("\t\t");
                    builder.Append(MemberFullName());
                    this.MethodSignatureFullName(builder);
                    return builder.ToString();
                }
            }

            public virtual bool IsGenericInstance
            {
                get { return false; }
            }

            internal override bool ContainsGenericParameter
            {
                get
                {
                    if (this.ReturnType.ContainsGenericParameter || base.ContainsGenericParameter)
                        return true;

                    var parameters = this.Parameters;

                    for (int i = 0; i < parameters.Count; i++)
                        if (parameters[i].ParameterType.ContainsGenericParameter)
                            return true;

                    return false;
                }
            }

            internal MethodReference()
            {
                this.return_type = new MethodReturnType(this);
                this.token = new MetadataToken(TokenType.MemberRef);
            }

            public MethodReference(string name, TypeReference returnType)
                : base(name)
            {
                if (returnType == null)
                    throw new ArgumentNullException("returnType");

                this.return_type = new MethodReturnType(this);
                this.return_type.ReturnType = returnType;
                this.token = new MetadataToken(TokenType.MemberRef);
            }

            public MethodReference(string name, TypeReference returnType, TypeReference declaringType)
                : this(name, returnType)
            {
                if (declaringType == null)
                    throw new ArgumentNullException("declaringType");

                this.DeclaringType = declaringType;
            }

            public virtual MethodReference GetElementMethod()
            {
                return this;
            }

            public virtual MethodDefinition Resolve()
            {
                var module = this.Module;
                if (module == null)
                    throw new NotSupportedException();

                return module.Resolve(this);
            }
        }

        static partial class Mixin
        {

            public static bool IsVarArg(this IMethodSignature self)
            {
                return (self.CallingConvention & MethodCallingConvention.VarArg) != 0;
            }

            public static int GetSentinelPosition(this IMethodSignature self)
            {
                if (!self.HasParameters)
                    return -1;

                var parameters = self.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                    if (parameters[i].ParameterType.IsSentinel)
                        return i;

                return -1;
            }
        }



        public sealed class MethodReturnType : IConstantProvider, ICustomAttributeProvider, IMarshalInfoProvider
        {

            internal IMethodSignature method;
            internal ParameterDefinition parameter;
            TypeReference return_type;

            public IMethodSignature Method
            {
                get { return method; }
            }

            public TypeReference ReturnType
            {
                get { return return_type; }
                set { return_type = value; }
            }

            internal ParameterDefinition Parameter
            {
                get { return parameter ?? (parameter = new ParameterDefinition(return_type)); }
                set { parameter = value; }
            }

            public MetadataToken MetadataToken
            {
                get { return Parameter.MetadataToken; }
                set { Parameter.MetadataToken = value; }
            }

            public bool HasCustomAttributes
            {
                get { return parameter != null && parameter.HasCustomAttributes; }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return Parameter.CustomAttributes; }
            }

            public bool HasDefault
            {
                get { return parameter != null && parameter.HasDefault; }
                set { Parameter.HasDefault = value; }
            }

            public bool HasConstant
            {
                get { return parameter != null && parameter.HasConstant; }
                set { Parameter.HasConstant = value; }
            }

            public object Constant
            {
                get { return Parameter.Constant; }
                set { Parameter.Constant = value; }
            }

            public bool HasFieldMarshal
            {
                get { return parameter != null && parameter.HasFieldMarshal; }
                set { Parameter.HasFieldMarshal = value; }
            }

            public bool HasMarshalInfo
            {
                get { return parameter != null && parameter.HasMarshalInfo; }
            }

            public MarshalInfo MarshalInfo
            {
                get { return Parameter.MarshalInfo; }
                set { Parameter.MarshalInfo = value; }
            }

            public MethodReturnType(IMethodSignature method)
            {
                this.method = method;
            }
        }



        [Flags]
        public enum MethodSemanticsAttributes : ushort
        {
            None = 0x0000,
            Setter = 0x0001,
            Getter = 0x0002,
            Other = 0x0004,
            AddOn = 0x0008,
            RemoveOn = 0x0010,
            Fire = 0x0020
        }


        public abstract class MethodSpecification : MethodReference
        {

            readonly MethodReference method;

            public MethodReference ElementMethod
            {
                get { return method; }
            }

            public override string Name
            {
                get { return method.Name; }
                set { throw new InvalidOperationException(); }
            }

            public override MethodCallingConvention CallingConvention
            {
                get { return method.CallingConvention; }
                set { throw new InvalidOperationException(); }
            }

            public override bool HasThis
            {
                get { return method.HasThis; }
                set { throw new InvalidOperationException(); }
            }

            public override bool ExplicitThis
            {
                get { return method.ExplicitThis; }
                set { throw new InvalidOperationException(); }
            }

            public override MethodReturnType MethodReturnType
            {
                get { return method.MethodReturnType; }
                set { throw new InvalidOperationException(); }
            }

            public override TypeReference DeclaringType
            {
                get { return method.DeclaringType; }
                set { throw new InvalidOperationException(); }
            }

            public override ModuleDefinition Module
            {
                get { return method.Module; }
            }

            public override bool HasParameters
            {
                get { return method.HasParameters; }
            }

            public override Collection<ParameterDefinition> Parameters
            {
                get { return method.Parameters; }
            }

            internal override bool ContainsGenericParameter
            {
                get { return method.ContainsGenericParameter; }
            }

            internal MethodSpecification(MethodReference method)
            {
                if (method == null)
                    throw new ArgumentNullException("method");

                this.method = method;
                this.token = new MetadataToken(TokenType.MethodSpec);
            }

            public sealed override MethodReference GetElementMethod()
            {
                return method.GetElementMethod();
            }
        }



        public interface IModifierType
        {
            TypeReference ModifierType { get; }
            TypeReference ElementType { get; }
        }

        public sealed class OptionalModifierType : TypeSpecification, IModifierType
        {

            TypeReference modifier_type;

            public TypeReference ModifierType
            {
                get { return modifier_type; }
                set { modifier_type = value; }
            }

            public override string Name
            {
                get { return base.Name + Suffix; }
            }

            public override string FullName
            {
                get { return base.FullName + Suffix; }
            }

            string Suffix
            {
                get { return " modopt(" + modifier_type + ")"; }
            }

            public override bool IsValueType
            {
                get { return false; }
                set { throw new InvalidOperationException(); }
            }

            public override bool IsOptionalModifier
            {
                get { return true; }
            }

            internal override bool ContainsGenericParameter
            {
                get { return modifier_type.ContainsGenericParameter || base.ContainsGenericParameter; }
            }

            public OptionalModifierType(TypeReference modifierType, TypeReference type)
                : base(type)
            {
                Mixin.CheckModifier(modifierType, type);
                this.modifier_type = modifierType;
                this.etype = MD.ElementType.CModOpt;
            }
        }

        public sealed class RequiredModifierType : TypeSpecification, IModifierType
        {

            TypeReference modifier_type;

            public TypeReference ModifierType
            {
                get { return modifier_type; }
                set { modifier_type = value; }
            }

            public override string Name
            {
                get { return base.Name + Suffix; }
            }

            public override string FullName
            {
                get { return base.FullName + Suffix; }
            }

            string Suffix
            {
                get { return " modreq(" + modifier_type + ")"; }
            }

            public override bool IsValueType
            {
                get { return false; }
                set { throw new InvalidOperationException(); }
            }

            public override bool IsRequiredModifier
            {
                get { return true; }
            }

            internal override bool ContainsGenericParameter
            {
                get { return modifier_type.ContainsGenericParameter || base.ContainsGenericParameter; }
            }

            public RequiredModifierType(TypeReference modifierType, TypeReference type)
                : base(type)
            {
                Mixin.CheckModifier(modifierType, type);
                this.modifier_type = modifierType;
                this.etype = MD.ElementType.CModReqD;
            }

        }

        static partial class Mixin
        {

            public static void CheckModifier(TypeReference modifierType, TypeReference type)
            {
                if (modifierType == null)
                    throw new ArgumentNullException("modifierType");
                if (type == null)
                    throw new ArgumentNullException("type");
            }
        }



        public enum ReadingMode
        {
            Immediate = 1,
            Deferred = 2,
        }

        public sealed class ReaderParameters
        {

            ReadingMode reading_mode;
            IAssemblyResolver assembly_resolver;
            Stream symbol_stream;
            ISymbolReaderProvider symbol_reader_provider;
            bool read_symbols;

            public ReadingMode ReadingMode
            {
                get { return reading_mode; }
                set { reading_mode = value; }
            }

            public IAssemblyResolver AssemblyResolver
            {
                get { return assembly_resolver; }
                set { assembly_resolver = value; }
            }

            public Stream SymbolStream
            {
                get { return symbol_stream; }
                set { symbol_stream = value; }
            }

            public ISymbolReaderProvider SymbolReaderProvider
            {
                get { return symbol_reader_provider; }
                set { symbol_reader_provider = value; }
            }

            public bool ReadSymbols
            {
                get { return read_symbols; }
                set { read_symbols = value; }
            }

            public ReaderParameters()
                : this(ReadingMode.Deferred)
            {
            }

            public ReaderParameters(ReadingMode readingMode)
            {
                this.reading_mode = readingMode;
            }
        }

        public sealed class ModuleDefinition : ModuleReference, ICustomAttributeProvider
        {

            internal Image Image;
            internal MetadataSystem MetadataSystem;
            internal ReadingMode ReadingMode;
            internal ISymbolReaderProvider SymbolReaderProvider;
            internal ISymbolReader SymbolReader;

            internal IAssemblyResolver assembly_resolver;
            internal TypeSystem type_system;

            readonly MetadataReader reader;
            readonly string fq_name;

            internal ModuleKind kind;
            TargetRuntime runtime;
            TargetArchitecture architecture;
            ModuleAttributes attributes;
            Guid mvid;

            internal AssemblyDefinition assembly;
            MethodDefinition entry_point;

            Collection<CustomAttribute> custom_attributes;
            Collection<AssemblyNameReference> references;
            Collection<ModuleReference> modules;
            Collection<Resource> resources;
            Collection<ExportedType> exported_types;
            TypeDefinitionCollection types;

            public bool IsMain
            {
                get { return kind != ModuleKind.NetModule; }
            }

            public ModuleKind Kind
            {
                get { return kind; }
                set { kind = value; }
            }

            public TargetRuntime Runtime
            {
                get { return runtime; }
                set { runtime = value; }
            }

            public TargetArchitecture Architecture
            {
                get { return architecture; }
                set { architecture = value; }
            }

            public ModuleAttributes Attributes
            {
                get { return attributes; }
                set { attributes = value; }
            }

            public string FullyQualifiedName
            {
                get { return fq_name; }
            }

            public Guid Mvid
            {
                get { return mvid; }
                set { mvid = value; }
            }

            internal bool HasImage
            {
                get { return Image != null; }
            }

            public bool HasSymbols
            {
                get { return SymbolReader != null; }
            }

            public override MetadataScopeType MetadataScopeType
            {
                get { return MetadataScopeType.ModuleDefinition; }
            }

            public AssemblyDefinition Assembly
            {
                get { return assembly; }
            }

            public IAssemblyResolver AssemblyResolver
            {
                get { return assembly_resolver; }
            }

            public TypeSystem TypeSystem
            {
                get { return type_system ?? (type_system = TypeSystem.CreateTypeSystem(this)); }
            }

            public bool HasAssemblyReferences
            {
                get
                {
                    if (references != null)
                        return references.Count > 0;

                    return HasImage && Image.HasTable(Table.AssemblyRef);
                }
            }

            public Collection<AssemblyNameReference> AssemblyReferences
            {
                get
                {
                    if (references != null)
                        return references;

                    if (HasImage)
                        return references = Read(this, (_, reader) => reader.ReadAssemblyReferences());

                    return references = new Collection<AssemblyNameReference>();
                }
            }

            public bool HasModuleReferences
            {
                get
                {
                    if (modules != null)
                        return modules.Count > 0;

                    return HasImage && Image.HasTable(Table.ModuleRef);
                }
            }

            public Collection<ModuleReference> ModuleReferences
            {
                get
                {
                    if (modules != null)
                        return modules;

                    if (HasImage)
                        return modules = Read(this, (_, reader) => reader.ReadModuleReferences());

                    return modules = new Collection<ModuleReference>();
                }
            }

            public bool HasResources
            {
                get
                {
                    if (resources != null)
                        return resources.Count > 0;

                    if (HasImage)
                        return Image.HasTable(Table.ManifestResource) || Read(this, (_, reader) => reader.HasFileResource());

                    return false;
                }
            }

            public Collection<Resource> Resources
            {
                get
                {
                    if (resources != null)
                        return resources;

                    if (HasImage)
                        return resources = Read(this, (_, reader) => reader.ReadResources());

                    return resources = new Collection<Resource>();
                }
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(this);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(this)); }
            }

            public bool HasTypes
            {
                get
                {
                    if (types != null)
                        return types.Count > 0;

                    return HasImage && Image.HasTable(Table.TypeDef);
                }
            }

            public Collection<TypeDefinition> Types
            {
                get
                {
                    if (types != null)
                        return types;

                    if (HasImage)
                        return types = Read(this, (_, reader) => reader.ReadTypes());

                    return types = new TypeDefinitionCollection(this);
                }
            }

            public bool HasExportedTypes
            {
                get
                {
                    if (exported_types != null)
                        return exported_types.Count > 0;

                    return HasImage && Image.HasTable(Table.ExportedType);
                }
            }

            public Collection<ExportedType> ExportedTypes
            {
                get
                {
                    if (exported_types != null)
                        return exported_types;

                    if (HasImage)
                        return exported_types = Read(this, (_, reader) => reader.ReadExportedTypes());

                    return exported_types = new Collection<ExportedType>();
                }
            }

            public MethodDefinition EntryPoint
            {
                get
                {
                    if (entry_point != null)
                        return entry_point;

                    if (HasImage)
                        return entry_point = Read(this, (_, reader) => reader.ReadEntryPoint());

                    return entry_point = null;
                }
                set { entry_point = value; }
            }

            internal ModuleDefinition()
            {
                this.MetadataSystem = new MetadataSystem();
                this.token = new MetadataToken(TokenType.Module, 1);
                this.assembly_resolver = GlobalAssemblyResolver.Instance;
            }

            internal ModuleDefinition(Image image)
                : this()
            {
                this.Image = image;
                this.kind = image.Kind;
                this.runtime = image.Runtime;
                this.architecture = image.Architecture;
                this.attributes = image.Attributes;
                this.fq_name = image.FileName;

                this.reader = new MetadataReader(this);
            }

            public bool HasTypeReference(string fullName)
            {
                return HasTypeReference(string.Empty, fullName);
            }

            public bool HasTypeReference(string scope, string fullName)
            {
                CheckFullName(fullName);

                if (!HasImage)
                    return false;

                return Read(this, (_, reader) => reader.GetTypeReference(scope, fullName) != null);
            }

            public bool TryGetTypeReference(string fullName, out TypeReference type)
            {
                return TryGetTypeReference(string.Empty, fullName, out type);
            }

            public bool TryGetTypeReference(string scope, string fullName, out TypeReference type)
            {
                CheckFullName(fullName);

                if (!HasImage)
                {
                    type = null;
                    return false;
                }

                return (type = Read(this, (_, reader) => reader.GetTypeReference(scope, fullName))) != null;
            }

            public IEnumerable<TypeReference> GetTypeReferences()
            {
                if (!HasImage)
                    return Empty<TypeReference>.Array;

                return Read(this, (_, reader) => reader.GetTypeReferences());
            }

            public IEnumerable<MemberReference> GetMemberReferences()
            {
                if (!HasImage)
                    return Empty<MemberReference>.Array;

                return Read(this, (_, reader) => reader.GetMemberReferences());
            }

            public TypeDefinition GetType(string fullName)
            {
                CheckFullName(fullName);

                var position = fullName.IndexOf('/');
                if (position > 0)
                    return GetNestedType(fullName);

                return ((TypeDefinitionCollection)this.Types).GetType(fullName);
            }

            public TypeDefinition GetType(string @namespace, string name)
            {
                Mixin.CheckName(name);

                return ((TypeDefinitionCollection)this.Types).GetType(@namespace ?? string.Empty, name);
            }

            static void CheckFullName(string fullName)
            {
                if (fullName == null)
                    throw new ArgumentNullException("fullName");
                if (fullName.Length == 0)
                    throw new ArgumentException();
            }

            TypeDefinition GetNestedType(string fullname)
            {
                var names = fullname.Split('/');
                var type = GetType(names[0]);

                if (type == null)
                    return null;

                for (int i = 1; i < names.Length; i++)
                {
                    var nested_type = type.GetNestedType(names[i]);
                    if (nested_type == null)
                        return null;

                    type = nested_type;
                }

                return type;
            }

            internal FieldDefinition Resolve(FieldReference field)
            {
                return MetadataResolver.Resolve(AssemblyResolver, field);
            }

            internal MethodDefinition Resolve(MethodReference method)
            {
                return MetadataResolver.Resolve(AssemblyResolver, method);
            }

            internal TypeDefinition Resolve(TypeReference type)
            {
                return MetadataResolver.Resolve(AssemblyResolver, type);
            }

            public IMetadataTokenProvider LookupToken(int token)
            {
                return LookupToken(new MetadataToken((uint)token));
            }

            public IMetadataTokenProvider LookupToken(MetadataToken token)
            {
                return Read(this, (_, reader) => reader.LookupToken(token));
            }

            internal TRet Read<TItem, TRet>(TItem item, Func<TItem, MetadataReader, TRet> read)
            {
                var position = reader.position;
                var context = reader.context;

                var ret = read(item, reader);

                reader.position = position;
                reader.context = context;

                return ret;
            }

            void ProcessDebugHeader()
            {
                if (Image == null || Image.Debug.IsZero)
                    return;

                byte[] header;
                var directory = Image.GetDebugHeader(out header);

                if (!SymbolReader.ProcessDebugHeader(directory, header))
                    throw new InvalidOperationException();
            }

            public void ReadSymbols()
            {
                if (string.IsNullOrEmpty(fq_name))
                    throw new InvalidOperationException();

                var provider = SymbolProvider.GetPlatformReaderProvider();

                SymbolReader = provider.GetSymbolReader(this, fq_name);

                ProcessDebugHeader();
            }

            public void ReadSymbols(ISymbolReader reader)
            {
                if (reader == null)
                    throw new ArgumentNullException("reader");

                SymbolReader = reader;

                ProcessDebugHeader();
            }

            public static ModuleDefinition ReadModule(string fileName)
            {
                return ReadModule(fileName, new ReaderParameters(ReadingMode.Deferred));
            }

            public static ModuleDefinition ReadModule(Stream stream)
            {
                return ReadModule(stream, new ReaderParameters(ReadingMode.Deferred));
            }

            public static ModuleDefinition ReadModule(string fileName, ReaderParameters parameters)
            {
                using (var stream = GetFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return ReadModule(stream, parameters);
                }
            }

            static void CheckStream(object stream)
            {
                if (stream == null)
                    throw new ArgumentNullException("stream");
            }

            public static ModuleDefinition ReadModule(Stream stream, ReaderParameters parameters)
            {
                CheckStream(stream);
                if (!stream.CanRead || !stream.CanSeek)
                    throw new ArgumentException();
                Mixin.CheckParameters(parameters);

                return ModuleReader.CreateModuleFrom(
                    ImageReader.ReadImageFrom(stream),
                    parameters);
            }

            static Stream GetFileStream(string fileName, FileMode mode, FileAccess access, FileShare share)
            {
                if (fileName == null)
                    throw new ArgumentNullException("fileName");
                if (fileName.Length == 0)
                    throw new ArgumentException();

                return new FileStream(fileName, mode, access, share);
            }
        }

        static partial class Mixin
        {

            public static void CheckParameters(object parameters)
            {
                if (parameters == null)
                    throw new ArgumentNullException("parameters");
            }

            public static bool HasImage(this ModuleDefinition self)
            {
                return self != null && self.HasImage;
            }

            public static string GetFullyQualifiedName(this Stream self)
            {
                var file_stream = self as FileStream;
                if (file_stream == null)
                    return string.Empty;

                return Path.GetFullPath(file_stream.Name);
            }

            public static TargetRuntime ParseRuntime(this string self)
            {
                switch (self[1])
                {
                    case '1':
                        return self[3] == '0'
                            ? TargetRuntime.Net_1_0
                            : TargetRuntime.Net_1_1;
                    case '2':
                        return TargetRuntime.Net_2_0;
                    case '4':
                    default:
                        return TargetRuntime.Net_4_0;
                }
            }
        }



        public enum ModuleKind
        {
            Dll,
            Console,
            Windows,
            NetModule,
        }

        public enum TargetArchitecture
        {
            I386,
            AMD64,
            IA64,
        }

        [Flags]
        public enum ModuleAttributes
        {
            ILOnly = 1,
            Required32Bit = 2,
            StrongNameSigned = 8,
        }


        public class ModuleReference : IMetadataScope
        {

            string name;

            internal MetadataToken token;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public virtual MetadataScopeType MetadataScopeType
            {
                get { return MetadataScopeType.ModuleReference; }
            }

            public MetadataToken MetadataToken
            {
                get { return token; }
                set { token = value; }
            }

            internal ModuleReference()
            {
                this.token = new MetadataToken(TokenType.ModuleRef);
            }

            public ModuleReference(string name)
                : this()
            {
                this.name = name;
            }

            public override string ToString()
            {
                return name;
            }
        }



        public enum NativeType
        {
            None = 0x66,

            Boolean = 0x02,
            I1 = 0x03,
            U1 = 0x04,
            I2 = 0x05,
            U2 = 0x06,
            I4 = 0x07,
            U4 = 0x08,
            I8 = 0x09,
            U8 = 0x0a,
            R4 = 0x0b,
            R8 = 0x0c,
            LPStr = 0x14,
            Int = 0x1f,
            UInt = 0x20,
            Func = 0x26,
            Array = 0x2a,
            Currency = 0x0f,
            BStr = 0x13,
            LPWStr = 0x15,
            LPTStr = 0x16,
            FixedSysString = 0x17,
            IUnknown = 0x19,
            IDispatch = 0x1a,
            Struct = 0x1b,
            IntF = 0x1c,
            SafeArray = 0x1d,
            FixedArray = 0x1e,
            ByValStr = 0x22,
            ANSIBStr = 0x23,
            TBStr = 0x24,
            VariantBool = 0x25,
            ASAny = 0x28,
            LPStruct = 0x2b,
            CustomMarshaler = 0x2c,
            Error = 0x2d,
            Max = 0x50
        }



        [Flags]
        public enum ParameterAttributes : ushort
        {
            None = 0x0000,
            In = 0x0001,
            Out = 0x0002,
            Lcid = 0x0004,
            Retval = 0x0008,
            Optional = 0x0010,
            HasDefault = 0x1000,
            HasFieldMarshal = 0x2000,
            Unused = 0xcfe0
        }


        public sealed class ParameterDefinition : ParameterReference, ICustomAttributeProvider, IConstantProvider, IMarshalInfoProvider
        {

            ushort attributes;

            internal IMethodSignature method;

            object constant = Mixin.NotResolved;
            Collection<CustomAttribute> custom_attributes;
            MarshalInfo marshal_info;

            public ParameterAttributes Attributes
            {
                get { return (ParameterAttributes)attributes; }
                set { attributes = (ushort)value; }
            }

            public IMethodSignature Method
            {
                get { return method; }
            }

            public bool HasConstant
            {
                get
                {
                    ResolveConstant();

                    return constant != Mixin.NoValue;
                }
                set { if (!value) constant = Mixin.NoValue; }
            }

            public object Constant
            {
                get { return HasConstant ? constant : null; }
                set { constant = value; }
            }

            void ResolveConstant()
            {
                if (constant != Mixin.NotResolved)
                    return;

                this.ResolveConstant(ref constant, parameter_type.Module);
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(parameter_type.Module);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(parameter_type.Module)); }
            }

            public bool HasMarshalInfo
            {
                get
                {
                    if (marshal_info != null)
                        return true;

                    return this.GetHasMarshalInfo(parameter_type.Module);
                }
            }

            public MarshalInfo MarshalInfo
            {
                get { return marshal_info ?? (marshal_info = this.GetMarshalInfo(parameter_type.Module)); }
                set { marshal_info = value; }
            }

            public bool IsIn
            {
                get { return attributes.GetAttributes((ushort)ParameterAttributes.In); }
                set { attributes = attributes.SetAttributes((ushort)ParameterAttributes.In, value); }
            }

            public bool IsOut
            {
                get { return attributes.GetAttributes((ushort)ParameterAttributes.Out); }
                set { attributes = attributes.SetAttributes((ushort)ParameterAttributes.Out, value); }
            }

            public bool IsLcid
            {
                get { return attributes.GetAttributes((ushort)ParameterAttributes.Lcid); }
                set { attributes = attributes.SetAttributes((ushort)ParameterAttributes.Lcid, value); }
            }

            public bool IsReturnValue
            {
                get { return attributes.GetAttributes((ushort)ParameterAttributes.Retval); }
                set { attributes = attributes.SetAttributes((ushort)ParameterAttributes.Retval, value); }
            }

            public bool IsOptional
            {
                get { return attributes.GetAttributes((ushort)ParameterAttributes.Optional); }
                set { attributes = attributes.SetAttributes((ushort)ParameterAttributes.Optional, value); }
            }

            public bool HasDefault
            {
                get { return attributes.GetAttributes((ushort)ParameterAttributes.HasDefault); }
                set { attributes = attributes.SetAttributes((ushort)ParameterAttributes.HasDefault, value); }
            }

            public bool HasFieldMarshal
            {
                get { return attributes.GetAttributes((ushort)ParameterAttributes.HasFieldMarshal); }
                set { attributes = attributes.SetAttributes((ushort)ParameterAttributes.HasFieldMarshal, value); }
            }

            public ParameterDefinition(TypeReference parameterType)
                : this(string.Empty, ParameterAttributes.None, parameterType)
            {
            }

            public ParameterDefinition(string name, ParameterAttributes attributes, TypeReference parameterType)
                : base(name, parameterType)
            {
                this.attributes = (ushort)attributes;
                this.token = new MetadataToken(TokenType.Param);
            }

            public override ParameterDefinition Resolve()
            {
                return this;
            }
        }



        sealed class ParameterDefinitionCollection : Collection<ParameterDefinition>
        {

            readonly IMethodSignature method;

            internal ParameterDefinitionCollection(IMethodSignature method)
            {
                this.method = method;
            }

            internal ParameterDefinitionCollection(IMethodSignature method, int capacity)
                : base(capacity)
            {
                this.method = method;
            }

            protected override void OnAdd(ParameterDefinition item, int index)
            {
                item.method = method;
                item.index = index;
            }

            protected override void OnInsert(ParameterDefinition item, int index)
            {
                item.method = method;
                item.index = index;

                for (int i = index; i < size; i++)
                    items[i].index = i + 1;
            }

            protected override void OnSet(ParameterDefinition item, int index)
            {
                item.method = method;
                item.index = index;
            }

            protected override void OnRemove(ParameterDefinition item, int index)
            {
                item.method = null;
                item.index = -1;

                for (int i = index + 1; i < size; i++)
                    items[i].index = i - 1;
            }
        }



        public abstract class ParameterReference : IMetadataTokenProvider
        {

            string name;
            internal int index = -1;
            protected TypeReference parameter_type;
            internal MetadataToken token;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public int Index
            {
                get { return index; }
            }

            public TypeReference ParameterType
            {
                get { return parameter_type; }
                set { parameter_type = value; }
            }

            public MetadataToken MetadataToken
            {
                get { return token; }
                set { token = value; }
            }

            internal ParameterReference(string name, TypeReference parameterType)
            {
                if (parameterType == null)
                    throw new ArgumentNullException("parameterType");

                this.name = name ?? string.Empty;
                this.parameter_type = parameterType;
            }

            public override string ToString()
            {
                return name;
            }

            public abstract ParameterDefinition Resolve();
        }



        public sealed class PinnedType : TypeSpecification
        {

            public override bool IsValueType
            {
                get { return false; }
                set { throw new InvalidOperationException(); }
            }

            public override bool IsPinned
            {
                get { return true; }
            }

            public PinnedType(TypeReference type)
                : base(type)
            {
                Mixin.CheckType(type);
                this.etype = MD.ElementType.Pinned;
            }
        }


        [Flags]
        public enum PInvokeAttributes : ushort
        {
            NoMangle = 0x0001,
            CharSetMask = 0x0006,
            CharSetNotSpec = 0x0000,
            CharSetAnsi = 0x0002,
            CharSetUnicode = 0x0004,
            CharSetAuto = 0x0006,
            SupportsLastError = 0x0040,
            CallConvMask = 0x0700,
            CallConvWinapi = 0x0100,
            CallConvCdecl = 0x0200,
            CallConvStdCall = 0x0300,
            CallConvThiscall = 0x0400,
            CallConvFastcall = 0x0500,
            BestFitMask = 0x0030,
            BestFitEnabled = 0x0010,
            BestFidDisabled = 0x0020,

            ThrowOnUnmappableCharMask = 0x3000,
            ThrowOnUnmappableCharEnabled = 0x1000,
            ThrowOnUnmappableCharDisabled = 0x2000,
        }


        public sealed class PInvokeInfo
        {

            ushort attributes;
            string entry_point;
            ModuleReference module;

            public PInvokeAttributes Attributes
            {
                get { return (PInvokeAttributes)attributes; }
                set { attributes = (ushort)value; }
            }

            public string EntryPoint
            {
                get { return entry_point; }
                set { entry_point = value; }
            }

            public ModuleReference Module
            {
                get { return module; }
                set { module = value; }
            }

            public bool IsNoMangle
            {
                get { return attributes.GetAttributes((ushort)PInvokeAttributes.NoMangle); }
                set { attributes = attributes.SetAttributes((ushort)PInvokeAttributes.NoMangle, value); }
            }

            public bool IsCharSetNotSpec
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CharSetMask, (ushort)PInvokeAttributes.CharSetNotSpec); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CharSetMask, (ushort)PInvokeAttributes.CharSetNotSpec, value); }
            }

            public bool IsCharSetAnsi
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CharSetMask, (ushort)PInvokeAttributes.CharSetAnsi); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CharSetMask, (ushort)PInvokeAttributes.CharSetAnsi, value); }
            }

            public bool IsCharSetUnicode
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CharSetMask, (ushort)PInvokeAttributes.CharSetUnicode); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CharSetMask, (ushort)PInvokeAttributes.CharSetUnicode, value); }
            }

            public bool IsCharSetAuto
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CharSetMask, (ushort)PInvokeAttributes.CharSetAuto); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CharSetMask, (ushort)PInvokeAttributes.CharSetAuto, value); }
            }

            public bool SupportsLastError
            {
                get { return attributes.GetAttributes((ushort)PInvokeAttributes.SupportsLastError); }
                set { attributes = attributes.SetAttributes((ushort)PInvokeAttributes.SupportsLastError, value); }
            }

            public bool IsCallConvWinapi
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvWinapi); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvWinapi, value); }
            }

            public bool IsCallConvCdecl
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvCdecl); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvCdecl, value); }
            }

            public bool IsCallConvStdCall
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvStdCall); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvStdCall, value); }
            }

            public bool IsCallConvThiscall
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvThiscall); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvThiscall, value); }
            }

            public bool IsCallConvFastcall
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvFastcall); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.CallConvMask, (ushort)PInvokeAttributes.CallConvFastcall, value); }
            }

            public bool IsBestFistEnabled
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.BestFitMask, (ushort)PInvokeAttributes.BestFitEnabled); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.BestFitMask, (ushort)PInvokeAttributes.BestFitEnabled, value); }
            }

            public bool IsBestFistDisabled
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.BestFitMask, (ushort)PInvokeAttributes.BestFidDisabled); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.BestFitMask, (ushort)PInvokeAttributes.BestFidDisabled, value); }
            }

            public bool IsThrowOnUnmappableCharEnabled
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort)PInvokeAttributes.ThrowOnUnmappableCharEnabled); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort)PInvokeAttributes.ThrowOnUnmappableCharEnabled, value); }
            }

            public bool IsThrowOnUnmappableCharDisabled
            {
                get { return attributes.GetMaskedAttributes((ushort)PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort)PInvokeAttributes.ThrowOnUnmappableCharDisabled); }
                set { attributes = attributes.SetMaskedAttributes((ushort)PInvokeAttributes.ThrowOnUnmappableCharMask, (ushort)PInvokeAttributes.ThrowOnUnmappableCharDisabled, value); }
            }

            public PInvokeInfo(PInvokeAttributes attributes, string entryPoint, ModuleReference module)
            {
                this.attributes = (ushort)attributes;
                this.entry_point = entryPoint;
                this.module = module;
            }
        }



        public sealed class PointerType : TypeSpecification
        {

            public override string Name
            {
                get { return base.Name + "*"; }
            }

            public override string FullName
            {
                get { return base.FullName + "*"; }
            }

            public override bool IsValueType
            {
                get { return false; }
                set { throw new InvalidOperationException(); }
            }

            public override bool IsPointer
            {
                get { return true; }
            }

            public PointerType(TypeReference type)
                : base(type)
            {
                Mixin.CheckType(type);
                this.etype = MD.ElementType.Ptr;
            }
        }


        [Flags]
        public enum PropertyAttributes : ushort
        {
            None = 0x0000,
            SpecialName = 0x0200,
            RTSpecialName = 0x0400,
            HasDefault = 0x1000,
            Unused = 0xe9ff
        }


        public sealed class PropertyDefinition : PropertyReference, IMemberDefinition, IConstantProvider
        {

            bool? has_this;
            ushort attributes;

            Collection<CustomAttribute> custom_attributes;

            internal MethodDefinition get_method;
            internal MethodDefinition set_method;
            internal Collection<MethodDefinition> other_methods;

            object constant = Mixin.NotResolved;

            public PropertyAttributes Attributes
            {
                get { return (PropertyAttributes)attributes; }
                set { attributes = (ushort)value; }
            }

            public bool HasThis
            {
                get
                {
                    if (has_this.HasValue)
                        return has_this.Value;

                    if (GetMethod != null)
                        return get_method.HasThis;

                    if (SetMethod != null)
                        return set_method.HasThis;

                    return false;
                }
                set { has_this = value; }
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(Module);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(Module)); }
            }

            public MethodDefinition GetMethod
            {
                get
                {
                    if (get_method != null)
                        return get_method;

                    InitializeMethods();
                    return get_method;
                }
                set { get_method = value; }
            }

            public MethodDefinition SetMethod
            {
                get
                {
                    if (set_method != null)
                        return set_method;

                    InitializeMethods();
                    return set_method;
                }
                set { set_method = value; }
            }

            public bool HasOtherMethods
            {
                get
                {
                    if (other_methods != null)
                        return other_methods.Count > 0;

                    InitializeMethods();
                    return !other_methods.IsNullOrEmpty();
                }
            }

            public Collection<MethodDefinition> OtherMethods
            {
                get
                {
                    if (other_methods != null)
                        return other_methods;

                    InitializeMethods();

                    if (other_methods != null)
                        return other_methods;

                    return other_methods = new Collection<MethodDefinition>();
                }
            }

            public bool HasParameters
            {
                get
                {
                    if (get_method != null)
                        return get_method.HasParameters;

                    if (set_method != null)
                        return set_method.HasParameters && set_method.Parameters.Count > 1;

                    return false;
                }
            }

            public override Collection<ParameterDefinition> Parameters
            {
                get
                {
                    InitializeMethods();

                    if (get_method != null)
                        return MirrorParameters(get_method, 0);

                    if (set_method != null)
                        return MirrorParameters(set_method, 1);

                    return new Collection<ParameterDefinition>();
                }
            }

            static Collection<ParameterDefinition> MirrorParameters(MethodDefinition method, int bound)
            {
                var parameters = new Collection<ParameterDefinition>();
                if (!method.HasParameters)
                    return parameters;

                var original_parameters = method.Parameters;
                var end = original_parameters.Count - bound;

                for (int i = 0; i < end; i++)
                    parameters.Add(original_parameters[i]);

                return parameters;
            }

            public bool HasConstant
            {
                get
                {
                    ResolveConstant();

                    return constant != Mixin.NoValue;
                }
                set { if (!value) constant = Mixin.NoValue; }
            }

            public object Constant
            {
                get { return HasConstant ? constant : null; }
                set { constant = value; }
            }

            void ResolveConstant()
            {
                if (constant != Mixin.NotResolved)
                    return;

                this.ResolveConstant(ref constant, Module);
            }

            public bool IsSpecialName
            {
                get { return attributes.GetAttributes((ushort)PropertyAttributes.SpecialName); }
                set { attributes = attributes.SetAttributes((ushort)PropertyAttributes.SpecialName, value); }
            }

            public bool IsRuntimeSpecialName
            {
                get { return attributes.GetAttributes((ushort)PropertyAttributes.RTSpecialName); }
                set { attributes = attributes.SetAttributes((ushort)PropertyAttributes.RTSpecialName, value); }
            }

            public bool HasDefault
            {
                get { return attributes.GetAttributes((ushort)PropertyAttributes.HasDefault); }
                set { attributes = attributes.SetAttributes((ushort)PropertyAttributes.HasDefault, value); }
            }

            public new TypeDefinition DeclaringType
            {
                get { return (TypeDefinition)base.DeclaringType; }
                set { base.DeclaringType = value; }
            }

            public override bool IsDefinition
            {
                get { return true; }
            }

            public override string FullName
            {
                get
                {
                    var builder = new StringBuilder();
                    builder.Append(PropertyType.ToString());
                    builder.Append(' ');
                    builder.Append(MemberFullName());
                    builder.Append('(');
                    if (HasParameters)
                    {
                        var parameters = Parameters;
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            if (i > 0)
                                builder.Append(',');
                            builder.Append(parameters[i].ParameterType.FullName);
                        }
                    }
                    builder.Append(')');
                    return builder.ToString();
                }
            }

            public PropertyDefinition(string name, PropertyAttributes attributes, TypeReference propertyType)
                : base(name, propertyType)
            {
                this.attributes = (ushort)attributes;
                this.token = new MetadataToken(TokenType.Property);
            }

            void InitializeMethods()
            {
                if (get_method != null || set_method != null)
                    return;

                var module = this.Module;
                if (!module.HasImage())
                    return;

                module.Read(this, (property, reader) => reader.ReadMethods(property));
            }
        }



        public abstract class PropertyReference : MemberReference
        {

            TypeReference property_type;

            public TypeReference PropertyType
            {
                get { return property_type; }
                set { property_type = value; }
            }

            public abstract Collection<ParameterDefinition> Parameters
            {
                get;
            }

            internal PropertyReference(string name, TypeReference propertyType)
                : base(name)
            {
                if (propertyType == null)
                    throw new ArgumentNullException("propertyType");

                property_type = propertyType;
            }
        }



        public sealed class ByReferenceType : TypeSpecification
        {

            public override string Name
            {
                get { return base.Name + "&"; }
            }

            public override string FullName
            {
                get { return base.FullName + "&"; }
            }

            public override bool IsValueType
            {
                get { return false; }
                set { throw new InvalidOperationException(); }
            }

            public override bool IsByReference
            {
                get { return true; }
            }

            public ByReferenceType(TypeReference type)
                : base(type)
            {
                Mixin.CheckType(type);
                this.etype = MD.ElementType.ByRef;
            }
        }


        public enum ResourceType
        {
            Linked,
            Embedded,
            AssemblyLinked,
        }

        public abstract class Resource
        {

            string name;
            uint attributes;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public ManifestResourceAttributes Attributes
            {
                get { return (ManifestResourceAttributes)attributes; }
                set { attributes = (uint)value; }
            }

            public abstract ResourceType ResourceType
            {
                get;
            }

            public bool IsPublic
            {
                get { return attributes.GetMaskedAttributes((uint)ManifestResourceAttributes.VisibilityMask, (uint)ManifestResourceAttributes.Public); }
                set { attributes = attributes.SetMaskedAttributes((uint)ManifestResourceAttributes.VisibilityMask, (uint)ManifestResourceAttributes.Public, value); }
            }

            public bool IsPrivate
            {
                get { return attributes.GetMaskedAttributes((uint)ManifestResourceAttributes.VisibilityMask, (uint)ManifestResourceAttributes.Private); }
                set { attributes = attributes.SetMaskedAttributes((uint)ManifestResourceAttributes.VisibilityMask, (uint)ManifestResourceAttributes.Private, value); }
            }

            internal Resource(string name, ManifestResourceAttributes attributes)
            {
                this.name = name;
                this.attributes = (uint)attributes;
            }
        }



        public enum SecurityAction : ushort
        {
            Request = 1,
            Demand = 2,
            Assert = 3,
            Deny = 4,
            PermitOnly = 5,
            LinkDemand = 6,
            InheritDemand = 7,
            RequestMinimum = 8,
            RequestOptional = 9,
            RequestRefuse = 10,
            PreJitGrant = 11,
            PreJitDeny = 12,
            NonCasDemand = 13,
            NonCasLinkDemand = 14,
            NonCasInheritance = 15
        }

        public interface ISecurityDeclarationProvider : IMetadataTokenProvider
        {

            bool HasSecurityDeclarations { get; }
            Collection<SecurityDeclaration> SecurityDeclarations { get; }
        }

        public sealed class SecurityAttribute : ICustomAttribute
        {

            TypeReference attribute_type;

            internal Collection<CustomAttributeNamedArgument> fields;
            internal Collection<CustomAttributeNamedArgument> properties;

            public TypeReference AttributeType
            {
                get { return attribute_type; }
                set { attribute_type = value; }
            }

            public bool HasFields
            {
                get { return !fields.IsNullOrEmpty(); }
            }

            public Collection<CustomAttributeNamedArgument> Fields
            {
                get { return fields ?? (fields = new Collection<CustomAttributeNamedArgument>()); }
            }

            public bool HasProperties
            {
                get { return !properties.IsNullOrEmpty(); }
            }

            public Collection<CustomAttributeNamedArgument> Properties
            {
                get { return properties ?? (properties = new Collection<CustomAttributeNamedArgument>()); }
            }

            public SecurityAttribute(TypeReference attributeType)
            {
                this.attribute_type = attributeType;
            }
        }

        public sealed class SecurityDeclaration
        {

            readonly internal uint signature;
            readonly ModuleDefinition module;

            internal bool resolved;
            SecurityAction action;
            internal Collection<SecurityAttribute> security_attributes;

            public SecurityAction Action
            {
                get { return action; }
                set { action = value; }
            }

            public bool HasSecurityAttributes
            {
                get
                {
                    Resolve();

                    return !security_attributes.IsNullOrEmpty();
                }
            }

            public Collection<SecurityAttribute> SecurityAttributes
            {
                get
                {
                    Resolve();

                    return security_attributes ?? (security_attributes = new Collection<SecurityAttribute>());
                }
            }

            internal bool HasImage
            {
                get { return module != null && module.HasImage; }
            }

            internal SecurityDeclaration(SecurityAction action, uint signature, ModuleDefinition module)
            {
                this.action = action;
                this.signature = signature;
                this.module = module;
            }

            public SecurityDeclaration(SecurityAction action)
            {
                this.action = action;
                this.resolved = true;
            }

            public byte[] GetBlob()
            {
                if (!HasImage || signature == 0)
                    throw new NotSupportedException();

                return module.Read(this, (declaration, reader) => reader.ReadSecurityDeclarationBlob(declaration.signature)); ;
            }

            void Resolve()
            {
                if (resolved || !HasImage)
                    return;

                module.Read(this, (declaration, reader) =>
                {
                    reader.ReadSecurityDeclarationSignature(declaration);
                    return this;
                });

                resolved = true;
            }
        }

        static partial class Mixin
        {

            public static bool GetHasSecurityDeclarations(
                this ISecurityDeclarationProvider self,
                ModuleDefinition module)
            {
                return module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.HasSecurityDeclarations(provider))
                    : false;
            }

            public static Collection<SecurityDeclaration> GetSecurityDeclarations(
                this ISecurityDeclarationProvider self,
                ModuleDefinition module)
            {
                return module.HasImage()
                    ? module.Read(self, (provider, reader) => reader.ReadSecurityDeclarations(provider))
                    : new Collection<SecurityDeclaration>();
            }
        }



        public sealed class SentinelType : TypeSpecification
        {

            public override bool IsValueType
            {
                get { return false; }
                set { throw new InvalidOperationException(); }
            }

            public override bool IsSentinel
            {
                get { return true; }
            }

            public SentinelType(TypeReference type)
                : base(type)
            {
                Mixin.CheckType(type);
                this.etype = MD.ElementType.Sentinel;
            }
        }


        public enum TargetRuntime
        {
            Net_1_0,
            Net_1_1,
            Net_2_0,
            Net_4_0,
        }


        public enum TokenType : uint
        {
            Module = 0x00000000,
            TypeRef = 0x01000000,
            TypeDef = 0x02000000,
            Field = 0x04000000,
            Method = 0x06000000,
            Param = 0x08000000,
            InterfaceImpl = 0x09000000,
            MemberRef = 0x0a000000,
            CustomAttribute = 0x0c000000,
            Permission = 0x0e000000,
            Signature = 0x11000000,
            Event = 0x14000000,
            Property = 0x17000000,
            ModuleRef = 0x1a000000,
            TypeSpec = 0x1b000000,
            Assembly = 0x20000000,
            AssemblyRef = 0x23000000,
            File = 0x26000000,
            ExportedType = 0x27000000,
            ManifestResource = 0x28000000,
            GenericParam = 0x2a000000,
            MethodSpec = 0x2b000000,
            String = 0x70000000,
        }


        [Flags]
        public enum TypeAttributes : uint
        {
            VisibilityMask = 0x00000007,
            NotPublic = 0x00000000,
            Public = 0x00000001,
            NestedPublic = 0x00000002,
            NestedPrivate = 0x00000003,
            NestedFamily = 0x00000004,
            NestedAssembly = 0x00000005,
            NestedFamANDAssem = 0x00000006,
            NestedFamORAssem = 0x00000007,
            LayoutMask = 0x00000018,
            AutoLayout = 0x00000000,
            SequentialLayout = 0x00000008,
            ExplicitLayout = 0x00000010,
            ClassSemanticMask = 0x00000020,
            Class = 0x00000000,
            Interface = 0x00000020,
            Abstract = 0x00000080,
            Sealed = 0x00000100,
            SpecialName = 0x00000400,
            Import = 0x00001000,
            Serializable = 0x00002000,
            StringFormatMask = 0x00030000,
            AnsiClass = 0x00000000,
            UnicodeClass = 0x00010000,
            AutoClass = 0x00020000,
            BeforeFieldInit = 0x00100000,
            RTSpecialName = 0x00000800,
            HasSecurity = 0x00040000,
            Forwarder = 0x00200000,
        }


        public sealed class TypeDefinition : TypeReference, IMemberDefinition, ISecurityDeclarationProvider
        {

            uint attributes;
            TypeReference base_type;
            internal Range fields_range;
            internal Range methods_range;

            short packing_size = Mixin.NotResolvedMarker;
            int class_size = Mixin.NotResolvedMarker;

            Collection<TypeReference> interfaces;
            Collection<TypeDefinition> nested_types;
            Collection<MethodDefinition> methods;
            Collection<FieldDefinition> fields;
            Collection<EventDefinition> events;
            Collection<PropertyDefinition> properties;
            Collection<CustomAttribute> custom_attributes;
            Collection<SecurityDeclaration> security_declarations;

            public TypeAttributes Attributes
            {
                get { return (TypeAttributes)attributes; }
                set { attributes = (uint)value; }
            }

            public TypeReference BaseType
            {
                get { return base_type; }
                set { base_type = value; }
            }

            void ResolveLayout()
            {
                if (packing_size != Mixin.NotResolvedMarker || class_size != Mixin.NotResolvedMarker)
                    return;

                if (!HasImage)
                {
                    packing_size = Mixin.NoDataMarker;
                    class_size = Mixin.NoDataMarker;
                    return;
                }

                var row = Module.Read(this, (type, reader) => reader.ReadTypeLayout(type));

                packing_size = row.Col1;
                class_size = row.Col2;
            }

            public bool HasLayoutInfo
            {
                get
                {
                    if (packing_size >= 0 || class_size >= 0)
                        return true;

                    ResolveLayout();

                    return packing_size >= 0 || class_size >= 0;
                }
            }

            public short PackingSize
            {
                get
                {
                    if (packing_size >= 0)
                        return packing_size;

                    ResolveLayout();

                    return packing_size >= 0 ? packing_size : (short)-1;
                }
                set { packing_size = value; }
            }

            public int ClassSize
            {
                get
                {
                    if (class_size >= 0)
                        return class_size;

                    ResolveLayout();

                    return class_size >= 0 ? class_size : -1;
                }
                set { class_size = value; }
            }

            public bool HasInterfaces
            {
                get
                {
                    if (interfaces != null)
                        return interfaces.Count > 0;

                    if (HasImage)
                        return Module.Read(this, (type, reader) => reader.HasInterfaces(type));

                    return false;
                }
            }

            public Collection<TypeReference> Interfaces
            {
                get
                {
                    if (interfaces != null)
                        return interfaces;

                    if (HasImage)
                        return interfaces = Module.Read(this, (type, reader) => reader.ReadInterfaces(type));

                    return interfaces = new Collection<TypeReference>();
                }
            }

            public bool HasNestedTypes
            {
                get
                {
                    if (nested_types != null)
                        return nested_types.Count > 0;

                    if (HasImage)
                        return Module.Read(this, (type, reader) => reader.HasNestedTypes(type));

                    return false;
                }
            }

            public Collection<TypeDefinition> NestedTypes
            {
                get
                {
                    if (nested_types != null)
                        return nested_types;

                    if (HasImage)
                        return nested_types = Module.Read(this, (type, reader) => reader.ReadNestedTypes(type));

                    return nested_types = new MemberDefinitionCollection<TypeDefinition>(this);
                }
            }

            internal new bool HasImage
            {
                get { return Module != null && Module.HasImage; }
            }

            public bool HasMethods
            {
                get
                {
                    if (methods != null)
                        return methods.Count > 0;

                    if (HasImage)
                        return methods_range.Length > 0;

                    return false;
                }
            }

            public Collection<MethodDefinition> Methods
            {
                get
                {
                    if (methods != null)
                        return methods;

                    if (HasImage)
                        return methods = Module.Read(this, (type, reader) => reader.ReadMethods(type));

                    return methods = new MemberDefinitionCollection<MethodDefinition>(this);
                }
            }

            public bool HasFields
            {
                get
                {
                    if (fields != null)
                        return fields.Count > 0;

                    if (HasImage)
                        return fields_range.Length > 0;

                    return false;
                }
            }

            public Collection<FieldDefinition> Fields
            {
                get
                {
                    if (fields != null)
                        return fields;

                    if (HasImage)
                        return fields = Module.Read(this, (type, reader) => reader.ReadFields(type));

                    return fields = new MemberDefinitionCollection<FieldDefinition>(this);
                }
            }

            public bool HasEvents
            {
                get
                {
                    if (events != null)
                        return events.Count > 0;

                    if (HasImage)
                        return Module.Read(this, (type, reader) => reader.HasEvents(type));

                    return false;
                }
            }

            public Collection<EventDefinition> Events
            {
                get
                {
                    if (events != null)
                        return events;

                    if (HasImage)
                        return events = Module.Read(this, (type, reader) => reader.ReadEvents(type));

                    return events = new MemberDefinitionCollection<EventDefinition>(this);
                }
            }

            public bool HasProperties
            {
                get
                {
                    if (properties != null)
                        return properties.Count > 0;

                    if (HasImage)
                        return Module.Read(this, (type, reader) => reader.HasProperties(type));

                    return false;
                }
            }

            public Collection<PropertyDefinition> Properties
            {
                get
                {
                    if (properties != null)
                        return properties;

                    if (HasImage)
                        return properties = Module.Read(this, (type, reader) => reader.ReadProperties(type));

                    return properties = new MemberDefinitionCollection<PropertyDefinition>(this);
                }
            }

            public bool HasSecurityDeclarations
            {
                get
                {
                    if (security_declarations != null)
                        return security_declarations.Count > 0;

                    return this.GetHasSecurityDeclarations(Module);
                }
            }

            public Collection<SecurityDeclaration> SecurityDeclarations
            {
                get { return security_declarations ?? (security_declarations = this.GetSecurityDeclarations(Module)); }
            }

            public bool HasCustomAttributes
            {
                get
                {
                    if (custom_attributes != null)
                        return custom_attributes.Count > 0;

                    return this.GetHasCustomAttributes(Module);
                }
            }

            public Collection<CustomAttribute> CustomAttributes
            {
                get { return custom_attributes ?? (custom_attributes = this.GetCustomAttributes(Module)); }
            }

            public override bool HasGenericParameters
            {
                get
                {
                    if (generic_parameters != null)
                        return generic_parameters.Count > 0;

                    return this.GetHasGenericParameters(Module);
                }
            }

            public override Collection<GenericParameter> GenericParameters
            {
                get { return generic_parameters ?? (generic_parameters = this.GetGenericParameters(Module)); }
            }

            public bool IsNotPublic
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NotPublic); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NotPublic, value); }
            }

            public bool IsPublic
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.Public); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.Public, value); }
            }

            public bool IsNestedPublic
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedPublic); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedPublic, value); }
            }

            public bool IsNestedPrivate
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedPrivate); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedPrivate, value); }
            }

            public bool IsNestedFamily
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamily); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamily, value); }
            }

            public bool IsNestedAssembly
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedAssembly); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedAssembly, value); }
            }

            public bool IsNestedFamilyAndAssembly
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamANDAssem); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamANDAssem, value); }
            }

            public bool IsNestedFamilyOrAssembly
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamORAssem); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.VisibilityMask, (uint)TypeAttributes.NestedFamORAssem, value); }
            }

            public bool IsAutoLayout
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.AutoLayout); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.AutoLayout, value); }
            }

            public bool IsSequentialLayout
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.SequentialLayout); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.SequentialLayout, value); }
            }

            public bool IsExplicitLayout
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.ExplicitLayout); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.LayoutMask, (uint)TypeAttributes.ExplicitLayout, value); }
            }

            public bool IsClass
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.ClassSemanticMask, (uint)TypeAttributes.Class); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.ClassSemanticMask, (uint)TypeAttributes.Class, value); }
            }

            public bool IsInterface
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.ClassSemanticMask, (uint)TypeAttributes.Interface); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.ClassSemanticMask, (uint)TypeAttributes.Interface, value); }
            }

            public bool IsAbstract
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Abstract); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Abstract, value); }
            }

            public bool IsSealed
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Sealed); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Sealed, value); }
            }

            public bool IsSpecialName
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.SpecialName); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.SpecialName, value); }
            }

            public bool IsImport
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Import); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Import, value); }
            }

            public bool IsSerializable
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.Serializable); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.Serializable, value); }
            }

            public bool IsAnsiClass
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AnsiClass); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AnsiClass, value); }
            }

            public bool IsUnicodeClass
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.UnicodeClass); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.UnicodeClass, value); }
            }

            public bool IsAutoClass
            {
                get { return attributes.GetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AutoClass); }
                set { attributes = attributes.SetMaskedAttributes((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AutoClass, value); }
            }

            public bool IsBeforeFieldInit
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.BeforeFieldInit); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.BeforeFieldInit, value); }
            }

            public bool IsRuntimeSpecialName
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.RTSpecialName); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.RTSpecialName, value); }
            }

            public bool HasSecurity
            {
                get { return attributes.GetAttributes((uint)TypeAttributes.HasSecurity); }
                set { attributes = attributes.SetAttributes((uint)TypeAttributes.HasSecurity, value); }
            }

            public bool IsEnum
            {
                get { return base_type != null && base_type.IsTypeOf("System", "Enum"); }
            }

            public override bool IsValueType
            {
                get
                {
                    if (base_type == null)
                        return false;

                    return base_type.IsTypeOf("System", "Enum") || (base_type.IsTypeOf("System", "ValueType") && !this.IsTypeOf("System", "Enum"));
                }
            }

            public override bool IsDefinition
            {
                get { return true; }
            }

            public new TypeDefinition DeclaringType
            {
                get { return (TypeDefinition)base.DeclaringType; }
                set { base.DeclaringType = value; }
            }

            public TypeDefinition(string @namespace, string name, TypeAttributes attributes)
                : base(@namespace, name)
            {
                this.attributes = (uint)attributes;
                this.token = new MetadataToken(TokenType.TypeDef);
            }

            public TypeDefinition(string @namespace, string name, TypeAttributes attributes, TypeReference baseType) :
                this(@namespace, name, attributes)
            {
                this.BaseType = baseType;
            }

            public override TypeDefinition Resolve()
            {
                return this;
            }
        }

        static partial class Mixin
        {

            public static TypeReference GetEnumUnderlyingType(this TypeDefinition self)
            {
                var fields = self.Fields;

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];
                    if (!field.IsStatic)
                        return field.FieldType;
                }

                throw new ArgumentException();
            }

            public static TypeDefinition GetNestedType(this TypeDefinition self, string name)
            {
                if (!self.HasNestedTypes)
                    return null;

                var nested_types = self.NestedTypes;

                for (int i = 0; i < nested_types.Count; i++)
                {
                    var nested_type = nested_types[i];
                    if (nested_type.Name == name)
                        return nested_type;
                }

                return null;
            }
        }

        sealed class TypeDefinitionCollection : Collection<TypeDefinition>
        {

            readonly ModuleDefinition container;
            readonly Dictionary<Slot, TypeDefinition> name_cache;

            internal TypeDefinitionCollection(ModuleDefinition container)
            {
                this.container = container;
                this.name_cache = new Dictionary<Slot, TypeDefinition>(new RowEqualityComparer());
            }

            internal TypeDefinitionCollection(ModuleDefinition container, int capacity)
                : base(capacity)
            {
                this.container = container;
                this.name_cache = new Dictionary<Slot, TypeDefinition>(capacity, new RowEqualityComparer());
            }

            protected override void OnAdd(TypeDefinition item, int index)
            {
                Attach(item);
            }

            protected override void OnSet(TypeDefinition item, int index)
            {
                Attach(item);
            }

            protected override void OnInsert(TypeDefinition item, int index)
            {
                Attach(item);
            }

            protected override void OnRemove(TypeDefinition item, int index)
            {
                Detach(item);
            }

            protected override void OnClear()
            {
                foreach (var type in this)
                    Detach(type);
            }

            void Attach(TypeDefinition type)
            {
                if (type.Module != null && type.Module != container)
                    throw new ArgumentException("Type already attached");

                type.module = container;
                type.scope = container;
                name_cache[new Slot(type.Namespace, type.Name)] = type;
            }

            void Detach(TypeDefinition type)
            {
                type.module = null;
                type.scope = null;
                name_cache.Remove(new Slot(type.Namespace, type.Name));
            }

            public TypeDefinition GetType(string fullname)
            {
                string @namespace, name;
                TypeParser.SplitFullName(fullname, out @namespace, out name);

                return GetType(@namespace, name);
            }

            public TypeDefinition GetType(string @namespace, string name)
            {
                TypeDefinition type;
                if (name_cache.TryGetValue(new Slot(@namespace, name), out type))
                    return type;

                return null;
            }
        }



        class TypeParser
        {

            class Type
            {
                public const int Ptr = -1;
                public const int ByRef = -2;
                public const int SzArray = -3;

                public string type_fullname;
                public string[] nested_names;
                public int arity;
                public int[] specs;
                public Type[] generic_arguments;
                public string assembly;
            }

            readonly string fullname;
            readonly int length;

            int position;

            TypeParser(string fullname)
            {
                this.fullname = fullname;
                this.length = fullname.Length;
            }

            Type ParseType(bool fq_name)
            {
                var type = new Type();
                type.type_fullname = ParsePart();

                type.nested_names = ParseNestedNames();

                if (TryGetArity(type))
                    type.generic_arguments = ParseGenericArguments(type.arity);

                type.specs = ParseSpecs();

                if (fq_name)
                    type.assembly = ParseAssemblyName();

                return type;
            }

            static bool TryGetArity(Type type)
            {
                int arity = 0;

                TryAddArity(type.type_fullname, ref arity);

                var nested_names = type.nested_names;
                if (!nested_names.IsNullOrEmpty())
                {
                    for (int i = 0; i < nested_names.Length; i++)
                        TryAddArity(nested_names[i], ref arity);
                }

                type.arity = arity;
                return arity > 0;
            }

            static bool TryGetArity(string name, out int arity)
            {
                arity = 0;
                var index = name.LastIndexOf('`');
                if (index == -1)
                    return false;

                return ParseInt32(name.Substring(index + 1), out arity);
            }

            static bool ParseInt32(string value, out int result)
            {
                return int.TryParse(value, out result);
            }

            static void TryAddArity(string name, ref int arity)
            {
                int type_arity;
                if (!TryGetArity(name, out type_arity))
                    return;

                arity += type_arity;
            }

            string ParsePart()
            {
                int start = position;
                while (position < length && !IsDelimiter(fullname[position]))
                    position++;

                return fullname.Substring(start, position - start);
            }

            static bool IsDelimiter(char chr)
            {
                return "+,[]*&".IndexOf(chr) != -1;
            }

            void TryParseWhiteSpace()
            {
                while (position < length && Char.IsWhiteSpace(fullname[position]))
                    position++;
            }

            string[] ParseNestedNames()
            {
                string[] nested_names = null;
                while (TryParse('+'))
                    Add(ref nested_names, ParsePart());

                return nested_names;
            }

            bool TryParse(char chr)
            {
                if (position < length && fullname[position] == chr)
                {
                    position++;
                    return true;
                }

                return false;
            }

            static void Add<T>(ref T[] array, T item)
            {
                if (array == null)
                {
                    array = new[] { item };
                    return;
                }
                Array.Resize(ref array, array.Length + 1);
                array[array.Length - 1] = item;
            }

            int[] ParseSpecs()
            {
                int[] specs = null;

                while (position < length)
                {
                    switch (fullname[position])
                    {
                        case '*':
                            position++;
                            Add(ref specs, Type.Ptr);
                            break;
                        case '&':
                            position++;
                            Add(ref specs, Type.ByRef);
                            break;
                        case '[':
                            position++;
                            switch (fullname[position])
                            {
                                case ']':
                                    position++;
                                    Add(ref specs, Type.SzArray);
                                    break;
                                case '*':
                                    position++;
                                    Add(ref specs, 1);
                                    break;
                                default:
                                    var rank = 1;
                                    while (TryParse(','))
                                        rank++;

                                    Add(ref specs, rank);

                                    TryParse(']');
                                    break;
                            }
                            break;
                        default:
                            return specs;
                    }
                }

                return specs;
            }

            Type[] ParseGenericArguments(int arity)
            {
                Type[] generic_arguments = null;

                if (position == length || fullname[position] != '[')
                    return generic_arguments;

                TryParse('[');

                for (int i = 0; i < arity; i++)
                {
                    var fq_argument = TryParse('[');
                    Add(ref generic_arguments, ParseType(fq_argument));
                    if (fq_argument)
                        TryParse(']');

                    TryParse(',');
                    TryParseWhiteSpace();
                }

                TryParse(']');

                return generic_arguments;
            }

            string ParseAssemblyName()
            {
                if (!TryParse(','))
                    return string.Empty;

                TryParseWhiteSpace();

                var start = position;
                while (position < length)
                {
                    var chr = fullname[position];
                    if (chr == '[' || chr == ']')
                        break;

                    position++;
                }

                return fullname.Substring(start, position - start);
            }

            public static TypeReference ParseType(ModuleDefinition module, string fullname)
            {
                if (fullname == null)
                    return null;

                var parser = new TypeParser(fullname);
                return GetTypeReference(module, parser.ParseType(true));
            }

            static TypeReference GetTypeReference(ModuleDefinition module, Type type_info)
            {
                TypeReference type;
                if (!TryGetDefinition(module, type_info, out type))
                    type = CreateReference(type_info, module, GetMetadataScope(module, type_info));

                return CreateSpecs(type, type_info);
            }

            static TypeReference CreateSpecs(TypeReference type, Type type_info)
            {
                type = TryCreateGenericInstanceType(type, type_info);

                var specs = type_info.specs;
                if (specs.IsNullOrEmpty())
                    return type;

                for (int i = 0; i < specs.Length; i++)
                {
                    switch (specs[i])
                    {
                        case Type.Ptr:
                            type = new PointerType(type);
                            break;
                        case Type.ByRef:
                            type = new ByReferenceType(type);
                            break;
                        case Type.SzArray:
                            type = new ArrayType(type);
                            break;
                        default:
                            var array = new ArrayType(type);
                            array.Dimensions.Clear();

                            for (int j = 0; j < specs[i]; j++)
                                array.Dimensions.Add(new ArrayDimension());

                            type = array;
                            break;
                    }
                }

                return type;
            }

            static TypeReference TryCreateGenericInstanceType(TypeReference type, Type type_info)
            {
                var generic_arguments = type_info.generic_arguments;
                if (generic_arguments.IsNullOrEmpty())
                    return type;

                var instance = new GenericInstanceType(type);
                var instance_arguments = instance.GenericArguments;

                for (int i = 0; i < generic_arguments.Length; i++)
                    instance_arguments.Add(GetTypeReference(type.Module, generic_arguments[i]));

                return instance;
            }

            public static void SplitFullName(string fullname, out string @namespace, out string name)
            {
                var last_dot = fullname.LastIndexOf('.');

                if (last_dot == -1)
                {
                    @namespace = string.Empty;
                    name = fullname;
                }
                else
                {
                    @namespace = fullname.Substring(0, last_dot);
                    name = fullname.Substring(last_dot + 1);
                }
            }

            static TypeReference CreateReference(Type type_info, ModuleDefinition module, IMetadataScope scope)
            {
                string @namespace, name;
                SplitFullName(type_info.type_fullname, out @namespace, out name);

                var type = new TypeReference(@namespace, name, module, scope);

                AdjustGenericParameters(type);

                var nested_names = type_info.nested_names;
                if (nested_names.IsNullOrEmpty())
                    return type;

                for (int i = 0; i < nested_names.Length; i++)
                {
                    type = new TypeReference(string.Empty, nested_names[i], module, null)
                    {
                        DeclaringType = type,
                    };

                    AdjustGenericParameters(type);
                }

                return type;
            }

            static void AdjustGenericParameters(TypeReference type)
            {
                int arity;
                if (!TryGetArity(type.Name, out arity))
                    return;

                for (int i = 0; i < arity; i++)
                    type.GenericParameters.Add(new GenericParameter(type));
            }

            static IMetadataScope GetMetadataScope(ModuleDefinition module, Type type_info)
            {
                if (string.IsNullOrEmpty(type_info.assembly))
                    return module.TypeSystem.Corlib;

                return MatchReference(module, AssemblyNameReference.Parse(type_info.assembly));
            }

            static AssemblyNameReference MatchReference(ModuleDefinition module, AssemblyNameReference pattern)
            {
                var references = module.AssemblyReferences;

                for (int i = 0; i < references.Count; i++)
                {
                    var reference = references[i];
                    if (reference.FullName == pattern.FullName)
                        return reference;
                }

                return pattern;
            }

            static bool TryGetDefinition(ModuleDefinition module, Type type_info, out TypeReference type)
            {
                type = null;
                if (!TryCurrentModule(module, type_info))
                    return false;

                var typedef = module.GetType(type_info.type_fullname);
                if (typedef == null)
                    return false;

                var nested_names = type_info.nested_names;
                if (!nested_names.IsNullOrEmpty())
                {
                    for (int i = 0; i < nested_names.Length; i++)
                        typedef = typedef.GetNestedType(nested_names[i]);
                }

                type = typedef;
                return true;
            }

            static bool TryCurrentModule(ModuleDefinition module, Type type_info)
            {
                if (string.IsNullOrEmpty(type_info.assembly))
                    return true;

                if (module.assembly != null && module.assembly.Name.FullName == type_info.assembly)
                    return true;

                return false;
            }

            public static string ToParseable(TypeReference type)
            {
                if (type == null)
                    return null;

                var name = new StringBuilder();
                AppendType(type, name, true, true);
                return name.ToString();
            }

            static void AppendType(TypeReference type, StringBuilder name, bool fq_name, bool top_level)
            {
                var declaring_type = type.DeclaringType;
                if (declaring_type != null)
                {
                    AppendType(declaring_type, name, false, top_level);
                    name.Append('+');
                }

                var @namespace = type.Namespace;
                if (!string.IsNullOrEmpty(@namespace))
                {
                    name.Append(@namespace);
                    name.Append('.');
                }

                name.Append(type.GetElementType().Name);

                if (!fq_name)
                    return;

                if (type.IsTypeSpecification())
                    AppendTypeSpecification((TypeSpecification)type, name);

                if (RequiresFullyQualifiedName(type, top_level))
                {
                    name.Append(", ");
                    name.Append(GetScopeFullName(type));
                }
            }

            static string GetScopeFullName(TypeReference type)
            {
                var scope = type.Scope;
                switch (scope.MetadataScopeType)
                {
                    case MetadataScopeType.AssemblyNameReference:
                        return ((AssemblyNameReference)scope).FullName;
                    case MetadataScopeType.ModuleDefinition:
                        return ((ModuleDefinition)scope).Assembly.Name.FullName;
                }

                throw new ArgumentException();
            }

            static void AppendTypeSpecification(TypeSpecification type, StringBuilder name)
            {
                if (type.ElementType.IsTypeSpecification())
                    AppendTypeSpecification((TypeSpecification)type.ElementType, name);

                switch (type.etype)
                {
                    case ElementType.Ptr:
                        name.Append('*');
                        break;
                    case ElementType.ByRef:
                        name.Append('&');
                        break;
                    case ElementType.SzArray:
                    case ElementType.Array:
                        var array = (ArrayType)type;
                        if (array.IsVector)
                        {
                            name.Append("[]");
                        }
                        else
                        {
                            name.Append('[');
                            for (int i = 1; i < array.Rank; i++)
                                name.Append(',');
                            name.Append(']');
                        }
                        break;
                    case ElementType.GenericInst:
                        var instance = (GenericInstanceType)type;
                        var arguments = instance.GenericArguments;

                        name.Append('[');

                        for (int i = 0; i < arguments.Count; i++)
                        {
                            if (i > 0)
                                name.Append(',');

                            var argument = arguments[i];
                            var requires_fqname = argument.Scope != argument.Module;

                            if (requires_fqname)
                                name.Append('[');

                            AppendType(argument, name, true, false);

                            if (requires_fqname)
                                name.Append(']');
                        }

                        name.Append(']');
                        break;
                    default:
                        return;
                }
            }

            static bool RequiresFullyQualifiedName(TypeReference type, bool top_level)
            {
                if (type.Scope == type.Module)
                    return false;

                if (type.Scope.Name == "mscorlib" && top_level)
                    return false;

                return true;
            }
        }



        public enum MetadataType : byte
        {
            Void = ElementType.Void,
            Boolean = ElementType.Boolean,
            Char = ElementType.Char,
            SByte = ElementType.I1,
            Byte = ElementType.U1,
            Int16 = ElementType.I2,
            UInt16 = ElementType.U2,
            Int32 = ElementType.I4,
            UInt32 = ElementType.U4,
            Int64 = ElementType.I8,
            UInt64 = ElementType.U8,
            Single = ElementType.R4,
            Double = ElementType.R8,
            String = ElementType.String,
            Pointer = ElementType.Ptr,
            ByReference = ElementType.ByRef,
            ValueType = ElementType.ValueType,
            Class = ElementType.Class,
            Var = ElementType.Var,
            Array = ElementType.Array,
            GenericInstance = ElementType.GenericInst,
            TypedByReference = ElementType.TypedByRef,
            IntPtr = ElementType.I,
            UIntPtr = ElementType.U,
            FunctionPointer = ElementType.FnPtr,
            Object = ElementType.Object,
            MVar = ElementType.MVar,
            RequiredModifier = ElementType.CModReqD,
            OptionalModifier = ElementType.CModOpt,
            Sentinel = ElementType.Sentinel,
            Pinned = ElementType.Pinned,
        }

        public class TypeReference : MemberReference, IGenericParameterProvider, IGenericContext
        {

            string @namespace;
            bool value_type;
            internal IMetadataScope scope;
            internal ModuleDefinition module;

            internal ElementType etype = ElementType.None;

            string fullname;

            protected Collection<GenericParameter> generic_parameters;

            public override string Name
            {
                get { return base.Name; }
                set
                {
                    base.Name = value;
                    fullname = null;
                }
            }

            public virtual string Namespace
            {
                get { return @namespace; }
                set
                {
                    @namespace = value;
                    fullname = null;
                }
            }

            public virtual bool IsValueType
            {
                get { return value_type; }
                set { value_type = value; }
            }

            public override ModuleDefinition Module
            {
                get
                {
                    if (module != null)
                        return module;

                    var declaring_type = this.DeclaringType;
                    if (declaring_type != null)
                        return declaring_type.Module;

                    return null;
                }
            }

            IGenericParameterProvider IGenericContext.Type
            {
                get { return this; }
            }

            IGenericParameterProvider IGenericContext.Method
            {
                get { return null; }
            }

            GenericParameterType IGenericParameterProvider.GenericParameterType
            {
                get { return GenericParameterType.Type; }
            }

            public virtual bool HasGenericParameters
            {
                get { return !generic_parameters.IsNullOrEmpty(); }
            }

            public virtual Collection<GenericParameter> GenericParameters
            {
                get
                {
                    if (generic_parameters != null)
                        return generic_parameters;

                    return generic_parameters = new Collection<GenericParameter>();
                }
            }

            public virtual IMetadataScope Scope
            {
                get
                {
                    var declaring_type = this.DeclaringType;
                    if (declaring_type != null)
                        return declaring_type.Scope;

                    return scope;
                }
            }

            public bool IsNested
            {
                get { return this.DeclaringType != null; }
            }

            public override TypeReference DeclaringType
            {
                get { return base.DeclaringType; }
                set
                {
                    base.DeclaringType = value;
                    fullname = null;
                }
            }

            public override string FullName
            {
                get
                {
                    if (fullname != null)
                        return fullname;

                    if (IsNested)
                        return fullname = DeclaringType.FullName + "/" + Name;

                    if (string.IsNullOrEmpty(@namespace))
                        return fullname = Name;

                    return fullname = @namespace + "." + Name;
                }
            }

            public virtual bool IsByReference
            {
                get { return false; }
            }

            public virtual bool IsPointer
            {
                get { return false; }
            }

            public virtual bool IsSentinel
            {
                get { return false; }
            }

            public virtual bool IsArray
            {
                get { return false; }
            }

            public virtual bool IsGenericParameter
            {
                get { return false; }
            }

            public virtual bool IsGenericInstance
            {
                get { return false; }
            }

            public virtual bool IsRequiredModifier
            {
                get { return false; }
            }

            public virtual bool IsOptionalModifier
            {
                get { return false; }
            }

            public virtual bool IsPinned
            {
                get { return false; }
            }

            public virtual bool IsFunctionPointer
            {
                get { return false; }
            }

            public bool IsPrimitive
            {
                get
                {
                    switch (etype)
                    {
                        case ElementType.Boolean:
                        case ElementType.Char:
                        case ElementType.I:
                        case ElementType.U:
                        case ElementType.I1:
                        case ElementType.U1:
                        case ElementType.I2:
                        case ElementType.U2:
                        case ElementType.I4:
                        case ElementType.U4:
                        case ElementType.I8:
                        case ElementType.U8:
                        case ElementType.R4:
                        case ElementType.R8:
                            return true;
                        default:
                            return false;
                    }
                }
            }

            public virtual MetadataType MetadataType
            {
                get
                {
                    switch (etype)
                    {
                        case ElementType.None:
                            return IsValueType ? MetadataType.ValueType : MetadataType.Class;
                        default:
                            return (MetadataType)etype;
                    }
                }
            }

            protected TypeReference(string @namespace, string name)
                : base(name)
            {
                this.@namespace = @namespace ?? string.Empty;
                this.token = new MetadataToken(TokenType.TypeRef, 0);
            }

            public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope)
                : this(@namespace, name)
            {
                this.module = module;
                this.scope = scope;
            }

            public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope, bool valueType) :
                this(@namespace, name, module, scope)
            {
                value_type = valueType;
            }

            public virtual TypeReference GetElementType()
            {
                return this;
            }

            public virtual TypeDefinition Resolve()
            {
                var module = this.Module;
                if (module == null)
                    throw new NotSupportedException();

                return module.Resolve(this);
            }
        }

        static partial class Mixin
        {

            public static bool IsTypeOf(this TypeReference self, string @namespace, string name)
            {
                return self.Name == name
                    && self.Namespace == @namespace;
            }

            public static bool IsTypeSpecification(this TypeReference type)
            {
                switch (type.etype)
                {
                    case ElementType.Array:
                    case ElementType.ByRef:
                    case ElementType.CModOpt:
                    case ElementType.CModReqD:
                    case ElementType.FnPtr:
                    case ElementType.GenericInst:
                    case ElementType.MVar:
                    case ElementType.Pinned:
                    case ElementType.Ptr:
                    case ElementType.SzArray:
                    case ElementType.Sentinel:
                    case ElementType.Var:
                        return true;
                }

                return false;
            }

            public static TypeDefinition CheckedResolve(this TypeReference self)
            {
                var type = self.Resolve();
                if (type == null)
                    throw new ResolutionException(self);

                return type;
            }
        }



        public abstract class TypeSpecification : TypeReference
        {

            readonly TypeReference element_type;

            public TypeReference ElementType
            {
                get { return element_type; }
            }

            public override string Name
            {
                get { return element_type.Name; }
                set { throw new NotSupportedException(); }
            }

            public override string Namespace
            {
                get { return element_type.Namespace; }
                set { throw new NotSupportedException(); }
            }

            public override IMetadataScope Scope
            {
                get { return element_type.Scope; }
            }

            public override ModuleDefinition Module
            {
                get { return element_type.Module; }
            }

            public override string FullName
            {
                get { return element_type.FullName; }
            }

            internal override bool ContainsGenericParameter
            {
                get { return element_type.ContainsGenericParameter; }
            }

            public override MetadataType MetadataType
            {
                get { return (MetadataType)etype; }
            }

            internal TypeSpecification(TypeReference type)
                : base(null, null)
            {
                this.element_type = type;
                this.token = new MetadataToken(TokenType.TypeSpec);
            }

            public override TypeReference GetElementType()
            {
                return element_type.GetElementType();
            }
        }

        static partial class Mixin
        {

            public static void CheckType(TypeReference type)
            {
                if (type == null)
                    throw new ArgumentNullException("type");
            }
        }



        public abstract class TypeSystem
        {

            sealed class CorlibTypeSystem : TypeSystem
            {

                public CorlibTypeSystem(ModuleDefinition module)
                    : base(module)
                {
                }

                internal override TypeReference LookupType(string @namespace, string name)
                {
                    var metadata = module.MetadataSystem;
                    if (metadata.Types == null)
                        Initialize(module.Types);

                    return module.Read(this, (_, reader) =>
                    {
                        var types = reader.metadata.Types;

                        for (int i = 0; i < types.Length; i++)
                        {
                            if (types[i] == null)
                                types[i] = reader.GetTypeDefinition((uint)i + 1);

                            var type = types[i];

                            if (type.Name == name && type.Namespace == @namespace)
                                return type;
                        }

                        return null;
                    });
                }

                static void Initialize(object obj)
                {
                }
            }

            sealed class CommonTypeSystem : TypeSystem
            {

                AssemblyNameReference corlib;

                public CommonTypeSystem(ModuleDefinition module)
                    : base(module)
                {
                }

                internal override TypeReference LookupType(string @namespace, string name)
                {
                    return CreateTypeReference(@namespace, name);
                }

                public AssemblyNameReference GetCorlibReference()
                {
                    if (corlib != null)
                        return corlib;

                    const string mscorlib = "mscorlib";

                    var references = module.AssemblyReferences;

                    for (int i = 0; i < references.Count; i++)
                    {
                        var reference = references[i];
                        if (reference.Name == mscorlib)
                            return corlib = reference;
                    }

                    corlib = new AssemblyNameReference
                    {
                        Name = mscorlib,
                        Version = GetCorlibVersion(),
                        PublicKeyToken = new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 },
                    };

                    references.Add(corlib);

                    return corlib;
                }

                Version GetCorlibVersion()
                {
                    switch (module.Runtime)
                    {
                        case TargetRuntime.Net_1_0:
                        case TargetRuntime.Net_1_1:
                            return new Version(1, 0, 0, 0);
                        case TargetRuntime.Net_2_0:
                            return new Version(2, 0, 0, 0);
                        case TargetRuntime.Net_4_0:
                            return new Version(4, 0, 0, 0);
                        default:
                            throw new NotSupportedException();
                    }
                }

                TypeReference CreateTypeReference(string @namespace, string name)
                {
                    return new TypeReference(@namespace, name, module, GetCorlibReference());
                }
            }

            readonly ModuleDefinition module;

            TypeReference type_object;
            TypeReference type_void;
            TypeReference type_bool;
            TypeReference type_char;
            TypeReference type_sbyte;
            TypeReference type_byte;
            TypeReference type_int16;
            TypeReference type_uint16;
            TypeReference type_int32;
            TypeReference type_uint32;
            TypeReference type_int64;
            TypeReference type_uint64;
            TypeReference type_single;
            TypeReference type_double;
            TypeReference type_intptr;
            TypeReference type_uintptr;
            TypeReference type_string;
            TypeReference type_typedref;

            TypeSystem(ModuleDefinition module)
            {
                this.module = module;
            }

            internal static TypeSystem CreateTypeSystem(ModuleDefinition module)
            {
                if (IsCorlib(module))
                    return new CorlibTypeSystem(module);

                return new CommonTypeSystem(module);
            }

            static bool IsCorlib(ModuleDefinition module)
            {
                if (module.Assembly == null)
                    return false;

                return module.Assembly.Name.Name == "mscorlib";
            }

            internal abstract TypeReference LookupType(string @namespace, string name);

            TypeReference LookupSystemType(string name, ElementType element_type)
            {
                var type = LookupType("System", name);
                type.etype = element_type;
                return type;
            }

            TypeReference LookupSystemValueType(string name, ElementType element_type)
            {
                var type = LookupSystemType(name, element_type);
                type.IsValueType = true;
                return type;
            }

            public IMetadataScope Corlib
            {
                get
                {
                    var common = this as CommonTypeSystem;
                    if (common == null)
                        return module;

                    return common.GetCorlibReference();
                }
            }

            public TypeReference Object
            {
                get { return type_object ?? (type_object = LookupSystemType("Object", ElementType.Object)); }
            }

            public TypeReference Void
            {
                get { return type_void ?? (type_void = LookupSystemType("Void", ElementType.Void)); }
            }

            public TypeReference Boolean
            {
                get { return type_bool ?? (type_bool = LookupSystemValueType("Boolean", ElementType.Boolean)); }
            }

            public TypeReference Char
            {
                get { return type_char ?? (type_char = LookupSystemValueType("Char", ElementType.Char)); }
            }

            public TypeReference SByte
            {
                get { return type_sbyte ?? (type_sbyte = LookupSystemValueType("SByte", ElementType.I1)); }
            }

            public TypeReference Byte
            {
                get { return type_byte ?? (type_byte = LookupSystemValueType("Byte", ElementType.U1)); }
            }

            public TypeReference Int16
            {
                get { return type_int16 ?? (type_int16 = LookupSystemValueType("Int16", ElementType.I2)); }
            }

            public TypeReference UInt16
            {
                get { return type_uint16 ?? (type_uint16 = LookupSystemValueType("UInt16", ElementType.U2)); }
            }

            public TypeReference Int32
            {
                get { return type_int32 ?? (type_int32 = LookupSystemValueType("Int32", ElementType.I4)); }
            }

            public TypeReference UInt32
            {
                get { return type_uint32 ?? (type_uint32 = LookupSystemValueType("UInt32", ElementType.U4)); }
            }

            public TypeReference Int64
            {
                get { return type_int64 ?? (type_int64 = LookupSystemValueType("Int64", ElementType.I8)); }
            }

            public TypeReference UInt64
            {
                get { return type_uint64 ?? (type_uint64 = LookupSystemValueType("UInt64", ElementType.U8)); }
            }

            public TypeReference Single
            {
                get { return type_single ?? (type_single = LookupSystemValueType("Single", ElementType.R4)); }
            }

            public TypeReference Double
            {
                get { return type_double ?? (type_double = LookupSystemValueType("Double", ElementType.R8)); }
            }

            public TypeReference IntPtr
            {
                get { return type_intptr ?? (type_intptr = LookupSystemValueType("IntPtr", ElementType.I)); }
            }

            public TypeReference UIntPtr
            {
                get { return type_uintptr ?? (type_uintptr = LookupSystemValueType("UIntPtr", ElementType.U)); }
            }

            public TypeReference String
            {
                get { return type_string ?? (type_string = LookupSystemType("String", ElementType.String)); }
            }

            public TypeReference TypedReference
            {
                get { return type_typedref ?? (type_typedref = LookupSystemValueType("TypedReference", ElementType.TypedByRef)); }
            }
        }



        static partial class Mixin
        {

            public static uint ReadCompressedUInt32(this byte[] data, ref int position)
            {
                uint integer;
                if ((data[position] & 0x80) == 0)
                {
                    integer = data[position];
                    position++;
                }
                else if ((data[position] & 0x40) == 0)
                {
                    integer = (uint)(data[position] & ~0x80) << 8;
                    integer |= data[position + 1];
                    position += 2;
                }
                else
                {
                    integer = (uint)(data[position] & ~0xc0) << 24;
                    integer |= (uint)data[position + 1] << 16;
                    integer |= (uint)data[position + 2] << 8;
                    integer |= (uint)data[position + 3];
                    position += 4;
                }
                return integer;
            }

            public static MetadataToken GetMetadataToken(this CodedIndex self, uint data)
            {
                uint rid;
                TokenType token_type;
                switch (self)
                {
                    case CodedIndex.TypeDefOrRef:
                        rid = data >> 2;
                        switch (data & 3)
                        {
                            case 0:
                                token_type = TokenType.TypeDef; goto ret;
                            case 1:
                                token_type = TokenType.TypeRef; goto ret;
                            case 2:
                                token_type = TokenType.TypeSpec; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.HasConstant:
                        rid = data >> 2;
                        switch (data & 3)
                        {
                            case 0:
                                token_type = TokenType.Field; goto ret;
                            case 1:
                                token_type = TokenType.Param; goto ret;
                            case 2:
                                token_type = TokenType.Property; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.HasCustomAttribute:
                        rid = data >> 5;
                        switch (data & 31)
                        {
                            case 0:
                                token_type = TokenType.Method; goto ret;
                            case 1:
                                token_type = TokenType.Field; goto ret;
                            case 2:
                                token_type = TokenType.TypeRef; goto ret;
                            case 3:
                                token_type = TokenType.TypeDef; goto ret;
                            case 4:
                                token_type = TokenType.Param; goto ret;
                            case 5:
                                token_type = TokenType.InterfaceImpl; goto ret;
                            case 6:
                                token_type = TokenType.MemberRef; goto ret;
                            case 7:
                                token_type = TokenType.Module; goto ret;
                            case 8:
                                token_type = TokenType.Permission; goto ret;
                            case 9:
                                token_type = TokenType.Property; goto ret;
                            case 10:
                                token_type = TokenType.Event; goto ret;
                            case 11:
                                token_type = TokenType.Signature; goto ret;
                            case 12:
                                token_type = TokenType.ModuleRef; goto ret;
                            case 13:
                                token_type = TokenType.TypeSpec; goto ret;
                            case 14:
                                token_type = TokenType.Assembly; goto ret;
                            case 15:
                                token_type = TokenType.AssemblyRef; goto ret;
                            case 16:
                                token_type = TokenType.File; goto ret;
                            case 17:
                                token_type = TokenType.ExportedType; goto ret;
                            case 18:
                                token_type = TokenType.ManifestResource; goto ret;
                            case 19:
                                token_type = TokenType.GenericParam; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.HasFieldMarshal:
                        rid = data >> 1;
                        switch (data & 1)
                        {
                            case 0:
                                token_type = TokenType.Field; goto ret;
                            case 1:
                                token_type = TokenType.Param; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.HasDeclSecurity:
                        rid = data >> 2;
                        switch (data & 3)
                        {
                            case 0:
                                token_type = TokenType.TypeDef; goto ret;
                            case 1:
                                token_type = TokenType.Method; goto ret;
                            case 2:
                                token_type = TokenType.Assembly; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.MemberRefParent:
                        rid = data >> 3;
                        switch (data & 7)
                        {
                            case 0:
                                token_type = TokenType.TypeDef; goto ret;
                            case 1:
                                token_type = TokenType.TypeRef; goto ret;
                            case 2:
                                token_type = TokenType.ModuleRef; goto ret;
                            case 3:
                                token_type = TokenType.Method; goto ret;
                            case 4:
                                token_type = TokenType.TypeSpec; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.HasSemantics:
                        rid = data >> 1;
                        switch (data & 1)
                        {
                            case 0:
                                token_type = TokenType.Event; goto ret;
                            case 1:
                                token_type = TokenType.Property; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.MethodDefOrRef:
                        rid = data >> 1;
                        switch (data & 1)
                        {
                            case 0:
                                token_type = TokenType.Method; goto ret;
                            case 1:
                                token_type = TokenType.MemberRef; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.MemberForwarded:
                        rid = data >> 1;
                        switch (data & 1)
                        {
                            case 0:
                                token_type = TokenType.Field; goto ret;
                            case 1:
                                token_type = TokenType.Method; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.Implementation:
                        rid = data >> 2;
                        switch (data & 3)
                        {
                            case 0:
                                token_type = TokenType.File; goto ret;
                            case 1:
                                token_type = TokenType.AssemblyRef; goto ret;
                            case 2:
                                token_type = TokenType.ExportedType; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.CustomAttributeType:
                        rid = data >> 3;
                        switch (data & 7)
                        {
                            case 2:
                                token_type = TokenType.Method; goto ret;
                            case 3:
                                token_type = TokenType.MemberRef; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.ResolutionScope:
                        rid = data >> 2;
                        switch (data & 3)
                        {
                            case 0:
                                token_type = TokenType.Module; goto ret;
                            case 1:
                                token_type = TokenType.ModuleRef; goto ret;
                            case 2:
                                token_type = TokenType.AssemblyRef; goto ret;
                            case 3:
                                token_type = TokenType.TypeRef; goto ret;
                            default:
                                goto exit;
                        }
                    case CodedIndex.TypeOrMethodDef:
                        rid = data >> 1;
                        switch (data & 1)
                        {
                            case 0:
                                token_type = TokenType.TypeDef; goto ret;
                            case 1:
                                token_type = TokenType.Method; goto ret;
                            default: goto exit;
                        }
                    default:
                        goto exit;
                }
            ret:
                return new MetadataToken(token_type, rid);
            exit:
                return MetadataToken.Zero;
            }

            public static int GetSize(this CodedIndex self, Func<Table, int> counter)
            {
                int bits;
                Table[] tables;

                switch (self)
                {
                    case CodedIndex.TypeDefOrRef:
                        bits = 2;
                        tables = new[] { Table.TypeDef, Table.TypeRef, Table.TypeSpec };
                        break;
                    case CodedIndex.HasConstant:
                        bits = 2;
                        tables = new[] { Table.Field, Table.Param, Table.Property };
                        break;
                    case CodedIndex.HasCustomAttribute:
                        bits = 5;
                        tables = new[] {
					Table.Method, Table.Field, Table.TypeRef, Table.TypeDef, Table.Param, Table.InterfaceImpl, Table.MemberRef,
					Table.Module, Table.DeclSecurity, Table.Property, Table.Event, Table.StandAloneSig, Table.ModuleRef,
					Table.TypeSpec, Table.Assembly, Table.AssemblyRef, Table.File, Table.ExportedType,
					Table.ManifestResource, Table.GenericParam
				};
                        break;
                    case CodedIndex.HasFieldMarshal:
                        bits = 1;
                        tables = new[] { Table.Field, Table.Param };
                        break;
                    case CodedIndex.HasDeclSecurity:
                        bits = 2;
                        tables = new[] { Table.TypeDef, Table.Method, Table.Assembly };
                        break;
                    case CodedIndex.MemberRefParent:
                        bits = 3;
                        tables = new[] { Table.TypeDef, Table.TypeRef, Table.ModuleRef, Table.Method, Table.TypeSpec };
                        break;
                    case CodedIndex.HasSemantics:
                        bits = 1;
                        tables = new[] { Table.Event, Table.Property };
                        break;
                    case CodedIndex.MethodDefOrRef:
                        bits = 1;
                        tables = new[] { Table.Method, Table.MemberRef };
                        break;
                    case CodedIndex.MemberForwarded:
                        bits = 1;
                        tables = new[] { Table.Field, Table.Method };
                        break;
                    case CodedIndex.Implementation:
                        bits = 2;
                        tables = new[] { Table.File, Table.AssemblyRef, Table.ExportedType };
                        break;
                    case CodedIndex.CustomAttributeType:
                        bits = 3;
                        tables = new[] { Table.Method, Table.MemberRef };
                        break;
                    case CodedIndex.ResolutionScope:
                        bits = 2;
                        tables = new[] { Table.Module, Table.ModuleRef, Table.AssemblyRef, Table.TypeRef };
                        break;
                    case CodedIndex.TypeOrMethodDef:
                        bits = 1;
                        tables = new[] { Table.TypeDef, Table.Method };
                        break;
                    default:
                        throw new ArgumentException();
                }

                int max = 0;

                for (int i = 0; i < tables.Length; i++)
                {
                    max = System.Math.Max(counter(tables[i]), max);
                }

                return max < (1 << (16 - bits)) ? 2 : 4;
            }
        }



        public enum VariantType
        {
            None = 0,
            I2 = 2,
            I4 = 3,
            R4 = 4,
            R8 = 5,
            CY = 6,
            Date = 7,
            BStr = 8,
            Dispatch = 9,
            Error = 10,
            Bool = 11,
            Variant = 12,
            Unknown = 13,
            Decimal = 14,
            I1 = 16,
            UI1 = 17,
            UI2 = 18,
            UI4 = 19,
            Int = 22,
            UInt = 23
        }
        #endregion

        #region Cil
        namespace Cil
        {
            public enum Code
            {
                Nop,
                Break,
                Ldarg_0,
                Ldarg_1,
                Ldarg_2,
                Ldarg_3,
                Ldloc_0,
                Ldloc_1,
                Ldloc_2,
                Ldloc_3,
                Stloc_0,
                Stloc_1,
                Stloc_2,
                Stloc_3,
                Ldarg_S,
                Ldarga_S,
                Starg_S,
                Ldloc_S,
                Ldloca_S,
                Stloc_S,
                Ldnull,
                Ldc_I4_M1,
                Ldc_I4_0,
                Ldc_I4_1,
                Ldc_I4_2,
                Ldc_I4_3,
                Ldc_I4_4,
                Ldc_I4_5,
                Ldc_I4_6,
                Ldc_I4_7,
                Ldc_I4_8,
                Ldc_I4_S,
                Ldc_I4,
                Ldc_I8,
                Ldc_R4,
                Ldc_R8,
                Dup,
                Pop,
                Jmp,
                Call,
                Calli,
                Ret,
                Br_S,
                Brfalse_S,
                Brtrue_S,
                Beq_S,
                Bge_S,
                Bgt_S,
                Ble_S,
                Blt_S,
                Bne_Un_S,
                Bge_Un_S,
                Bgt_Un_S,
                Ble_Un_S,
                Blt_Un_S,
                Br,
                Brfalse,
                Brtrue,
                Beq,
                Bge,
                Bgt,
                Ble,
                Blt,
                Bne_Un,
                Bge_Un,
                Bgt_Un,
                Ble_Un,
                Blt_Un,
                Switch,
                Ldind_I1,
                Ldind_U1,
                Ldind_I2,
                Ldind_U2,
                Ldind_I4,
                Ldind_U4,
                Ldind_I8,
                Ldind_I,
                Ldind_R4,
                Ldind_R8,
                Ldind_Ref,
                Stind_Ref,
                Stind_I1,
                Stind_I2,
                Stind_I4,
                Stind_I8,
                Stind_R4,
                Stind_R8,
                Add,
                Sub,
                Mul,
                Div,
                Div_Un,
                Rem,
                Rem_Un,
                And,
                Or,
                Xor,
                Shl,
                Shr,
                Shr_Un,
                Neg,
                Not,
                Conv_I1,
                Conv_I2,
                Conv_I4,
                Conv_I8,
                Conv_R4,
                Conv_R8,
                Conv_U4,
                Conv_U8,
                Callvirt,
                Cpobj,
                Ldobj,
                Ldstr,
                Newobj,
                Castclass,
                Isinst,
                Conv_R_Un,
                Unbox,
                Throw,
                Ldfld,
                Ldflda,
                Stfld,
                Ldsfld,
                Ldsflda,
                Stsfld,
                Stobj,
                Conv_Ovf_I1_Un,
                Conv_Ovf_I2_Un,
                Conv_Ovf_I4_Un,
                Conv_Ovf_I8_Un,
                Conv_Ovf_U1_Un,
                Conv_Ovf_U2_Un,
                Conv_Ovf_U4_Un,
                Conv_Ovf_U8_Un,
                Conv_Ovf_I_Un,
                Conv_Ovf_U_Un,
                Box,
                Newarr,
                Ldlen,
                Ldelema,
                Ldelem_I1,
                Ldelem_U1,
                Ldelem_I2,
                Ldelem_U2,
                Ldelem_I4,
                Ldelem_U4,
                Ldelem_I8,
                Ldelem_I,
                Ldelem_R4,
                Ldelem_R8,
                Ldelem_Ref,
                Stelem_I,
                Stelem_I1,
                Stelem_I2,
                Stelem_I4,
                Stelem_I8,
                Stelem_R4,
                Stelem_R8,
                Stelem_Ref,
                Ldelem_Any,
                Stelem_Any,
                Unbox_Any,
                Conv_Ovf_I1,
                Conv_Ovf_U1,
                Conv_Ovf_I2,
                Conv_Ovf_U2,
                Conv_Ovf_I4,
                Conv_Ovf_U4,
                Conv_Ovf_I8,
                Conv_Ovf_U8,
                Refanyval,
                Ckfinite,
                Mkrefany,
                Ldtoken,
                Conv_U2,
                Conv_U1,
                Conv_I,
                Conv_Ovf_I,
                Conv_Ovf_U,
                Add_Ovf,
                Add_Ovf_Un,
                Mul_Ovf,
                Mul_Ovf_Un,
                Sub_Ovf,
                Sub_Ovf_Un,
                Endfinally,
                Leave,
                Leave_S,
                Stind_I,
                Conv_U,
                Arglist,
                Ceq,
                Cgt,
                Cgt_Un,
                Clt,
                Clt_Un,
                Ldftn,
                Ldvirtftn,
                Ldarg,
                Ldarga,
                Starg,
                Ldloc,
                Ldloca,
                Stloc,
                Localloc,
                Endfilter,
                Unaligned,
                Volatile,
                Tail,
                Initobj,
                Constrained,
                Cpblk,
                Initblk,
                No,
                Rethrow,
                Sizeof,
                Refanytype,
                Readonly,
            }


            sealed class CodeReader : ByteBuffer
            {

                readonly internal MetadataReader reader;

                int start;
                Section code_section;

                MethodDefinition method;
                MethodBody body;

                int Offset
                {
                    get
                    {
                        return base.position - start;
                    }
                }

                CodeReader(Section section, MetadataReader reader)
                    : base(section.Data)
                {
                    this.code_section = section;
                    this.reader = reader;
                }

                public static CodeReader CreateCodeReader(MetadataReader metadata)
                {
                    return new CodeReader(metadata.image.MetadataSection, metadata);
                }

                public MethodBody ReadMethodBody(MethodDefinition method)
                {
                    this.method = method;
                    this.body = new MethodBody(method);

                    reader.context = method;

                    ReadMethodBody();

                    return this.body;
                }

                public void MoveTo(int rva)
                {
                    if (!IsInSection(rva))
                    {
                        code_section = reader.image.GetSectionAtVirtualAddress((uint)rva);
                        Reset(code_section.Data);
                    }

                    base.position = rva - (int)code_section.VirtualAddress;
                }

                bool IsInSection(int rva)
                {
                    return code_section.VirtualAddress <= rva && rva < code_section.VirtualAddress + code_section.SizeOfRawData;
                }

                void ReadMethodBody()
                {
                    MoveTo(method.RVA);

                    var flags = ReadByte();
                    switch (flags & 0x3)
                    {
                        case 0x2:
                            body.code_size = flags >> 2;
                            body.MaxStackSize = 8;
                            ReadCode();
                            break;
                        case 0x3:
                            base.position--;
                            ReadFatMethod();
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                    var symbol_reader = reader.module.SymbolReader;

                    if (symbol_reader != null)
                    {
                        var instructions = body.Instructions;
                        symbol_reader.Read(body, offset => GetInstruction(instructions, offset));
                    }
                }

                void ReadFatMethod()
                {
                    var flags = ReadUInt16();
                    body.max_stack_size = ReadUInt16();
                    body.code_size = (int)ReadUInt32();
                    body.local_var_token = new MetadataToken(ReadUInt32());
                    body.init_locals = (flags & 0x10) != 0;

                    if (body.local_var_token.RID != 0)
                        body.variables = ReadVariables(body.local_var_token);

                    ReadCode();

                    if ((flags & 0x8) != 0)
                        ReadSection();
                }

                public VariableDefinitionCollection ReadVariables(MetadataToken local_var_token)
                {
                    var position = reader.position;
                    var variables = reader.ReadVariables(local_var_token);
                    reader.position = position;

                    return variables;
                }

                void ReadCode()
                {
                    start = position;
                    var code_size = body.code_size;

                    if (code_size < 0 || buffer.Length <= (uint)(code_size + position))
                        code_size = 0;

                    var end = start + code_size;
                    var instructions = body.instructions = new InstructionCollection(code_size / 3);

                    while (position < end)
                    {
                        var offset = base.position - start;
                        var opcode = ReadOpCode();
                        var current = new Instruction(offset, opcode);

                        if (opcode.OperandType != OperandType.InlineNone)
                            current.operand = ReadOperand(current);

                        instructions.Add(current);
                    }

                    ResolveBranches(instructions);
                }

                OpCode ReadOpCode()
                {
                    var il_opcode = ReadByte();
                    return il_opcode != 0xfe
                        ? OpCodes.OneByteOpCode[il_opcode]
                        : OpCodes.TwoBytesOpCode[ReadByte()];
                }

                object ReadOperand(Instruction instruction)
                {
                    switch (instruction.opcode.OperandType)
                    {
                        case OperandType.InlineSwitch:
                            var length = ReadInt32();
                            var base_offset = Offset + (4 * length);
                            var branches = new int[length];
                            for (int i = 0; i < length; i++)
                                branches[i] = base_offset + ReadInt32();
                            return branches;
                        case OperandType.ShortInlineBrTarget:
                            return ReadSByte() + Offset;
                        case OperandType.InlineBrTarget:
                            return ReadInt32() + Offset;
                        case OperandType.ShortInlineI:
                            if (instruction.opcode == OpCodes.Ldc_I4_S)
                                return ReadSByte();

                            return ReadByte();
                        case OperandType.InlineI:
                            return ReadInt32();
                        case OperandType.ShortInlineR:
                            return ReadSingle();
                        case OperandType.InlineR:
                            return ReadDouble();
                        case OperandType.InlineI8:
                            return ReadInt64();
                        case OperandType.ShortInlineVar:
                            return GetVariable(ReadByte());
                        case OperandType.InlineVar:
                            return GetVariable(ReadUInt16());
                        case OperandType.ShortInlineArg:
                            return GetParameter(ReadByte());
                        case OperandType.InlineArg:
                            return GetParameter(ReadUInt16());
                        case OperandType.InlineSig:
                            return GetCallSite(ReadToken());
                        case OperandType.InlineString:
                            return GetString(ReadToken());
                        case OperandType.InlineTok:
                        case OperandType.InlineType:
                        case OperandType.InlineMethod:
                        case OperandType.InlineField:
                            return reader.LookupToken(ReadToken());
                        default:
                            throw new NotSupportedException();
                    }
                }

                public string GetString(MetadataToken token)
                {
                    return reader.image.UserStringHeap.Read(token.RID);
                }

                public ParameterDefinition GetParameter(int index)
                {
                    return body.GetParameter(index);
                }

                public VariableDefinition GetVariable(int index)
                {
                    return body.GetVariable(index);
                }

                public CallSite GetCallSite(MetadataToken token)
                {
                    return reader.ReadCallSite(token);
                }

                void ResolveBranches(Collection<Instruction> instructions)
                {
                    var items = instructions.items;
                    var size = instructions.size;

                    for (int i = 0; i < size; i++)
                    {
                        var instruction = items[i];
                        switch (instruction.opcode.OperandType)
                        {
                            case OperandType.ShortInlineBrTarget:
                            case OperandType.InlineBrTarget:
                                instruction.operand = GetInstruction((int)instruction.operand);
                                break;
                            case OperandType.InlineSwitch:
                                var offsets = (int[])instruction.operand;
                                var branches = new Instruction[offsets.Length];
                                for (int j = 0; j < offsets.Length; j++)
                                    branches[j] = GetInstruction(offsets[j]);

                                instruction.operand = branches;
                                break;
                        }
                    }
                }

                Instruction GetInstruction(int offset)
                {
                    return GetInstruction(body.Instructions, offset);
                }

                static Instruction GetInstruction(Collection<Instruction> instructions, int offset)
                {
                    var size = instructions.size;
                    var items = instructions.items;
                    if (offset < 0 || offset > items[size - 1].offset)
                        return null;

                    int min = 0;
                    int max = size - 1;
                    while (min <= max)
                    {
                        int mid = min + ((max - min) / 2);
                        var instruction = items[mid];
                        var instruction_offset = instruction.offset;

                        if (offset == instruction_offset)
                            return instruction;

                        if (offset < instruction_offset)
                            max = mid - 1;
                        else
                            min = mid + 1;
                    }

                    return null;
                }

                void ReadSection()
                {
                    Align(4);

                    const byte fat_format = 0x40;
                    const byte more_sects = 0x80;

                    var flags = ReadByte();
                    if ((flags & fat_format) == 0)
                        ReadSmallSection();
                    else
                        ReadFatSection();

                    if ((flags & more_sects) != 0)
                        ReadSection();
                }

                void ReadSmallSection()
                {
                    var count = ReadByte() / 12;
                    Advance(2);

                    ReadExceptionHandlers(
                        count,
                        () => (int)ReadUInt16(),
                        () => (int)ReadByte());
                }

                void ReadFatSection()
                {
                    position--;
                    var count = (ReadInt32() >> 8) / 24;

                    ReadExceptionHandlers(
                        count,
                        ReadInt32,
                        ReadInt32);
                }

                void ReadExceptionHandlers(int count, Func<int> read_entry, Func<int> read_length)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var handler = new ExceptionHandler(
                            (ExceptionHandlerType)(read_entry() & 0x7));

                        handler.TryStart = GetInstruction(read_entry());
                        handler.TryEnd = GetInstruction(handler.TryStart.Offset + read_length());

                        handler.HandlerStart = GetInstruction(read_entry());
                        handler.HandlerEnd = GetInstruction(handler.HandlerStart.Offset + read_length());

                        ReadExceptionHandlerSpecific(handler);

                        this.body.ExceptionHandlers.Add(handler);
                    }
                }

                void ReadExceptionHandlerSpecific(ExceptionHandler handler)
                {
                    switch (handler.HandlerType)
                    {
                        case ExceptionHandlerType.Catch:
                            handler.CatchType = (TypeReference)reader.LookupToken(ReadToken());
                            break;
                        case ExceptionHandlerType.Filter:
                            handler.FilterStart = GetInstruction(ReadInt32());
                            handler.FilterEnd = handler.HandlerStart.Previous;
                            break;
                        default:
                            Advance(4);
                            break;
                    }
                }

                void Align(int align)
                {
                    align--;
                    Advance(((position + align) & ~align) - position);
                }

                public MetadataToken ReadToken()
                {
                    return new MetadataToken(ReadUInt32());
                }
            }


            public enum DocumentType
            {
                Other,
                Text,
            }

            public enum DocumentHashAlgorithm
            {
                None,
                MD5,
                SHA1,
            }

            public enum DocumentLanguage
            {
                Other,
                C,
                Cpp,
                CSharp,
                Basic,
                Java,
                Cobol,
                Pascal,
                Cil,
                JScript,
                Smc,
                MCpp,
            }

            public enum DocumentLanguageVendor
            {
                Other,
                Microsoft,
            }

            public sealed class Document
            {

                string url;

                byte type;
                byte hash_algorithm;
                byte language;
                byte language_vendor;

                byte[] hash;

                public string Url
                {
                    get { return url; }
                    set { url = value; }
                }

                public DocumentType Type
                {
                    get { return (DocumentType)type; }
                    set { type = (byte)value; }
                }

                public DocumentHashAlgorithm HashAlgorithm
                {
                    get { return (DocumentHashAlgorithm)hash_algorithm; }
                    set { hash_algorithm = (byte)value; }
                }

                public DocumentLanguage Language
                {
                    get { return (DocumentLanguage)language; }
                    set { language = (byte)value; }
                }

                public DocumentLanguageVendor LanguageVendor
                {
                    get { return (DocumentLanguageVendor)language_vendor; }
                    set { language_vendor = (byte)value; }
                }

                public byte[] Hash
                {
                    get { return hash; }
                    set { hash = value; }
                }

                public Document(string url)
                {
                    this.url = url;
                    this.hash = Empty<byte>.Array;
                }
            }


            public enum ExceptionHandlerType
            {
                Catch = 0,
                Filter = 1,
                Finally = 2,
                Fault = 4,
            }

            public sealed class ExceptionHandler
            {

                Instruction try_start;
                Instruction try_end;
                Instruction filter_start;
                Instruction filter_end;
                Instruction handler_start;
                Instruction handler_end;

                TypeReference catch_type;
                ExceptionHandlerType handler_type;

                public Instruction TryStart
                {
                    get { return try_start; }
                    set { try_start = value; }
                }

                public Instruction TryEnd
                {
                    get { return try_end; }
                    set { try_end = value; }
                }

                public Instruction FilterStart
                {
                    get { return filter_start; }
                    set { filter_start = value; }
                }

                public Instruction FilterEnd
                {
                    get { return filter_end; }
                    set { filter_end = value; }
                }

                public Instruction HandlerStart
                {
                    get { return handler_start; }
                    set { handler_start = value; }
                }

                public Instruction HandlerEnd
                {
                    get { return handler_end; }
                    set { handler_end = value; }
                }

                public TypeReference CatchType
                {
                    get { return catch_type; }
                    set { catch_type = value; }
                }

                public ExceptionHandlerType HandlerType
                {
                    get { return handler_type; }
                    set { handler_type = value; }
                }

                public ExceptionHandler(ExceptionHandlerType handlerType)
                {
                    this.handler_type = handlerType;
                }
            }


            public sealed class ILProcessor
            {

                readonly MethodBody body;
                readonly Collection<Instruction> instructions;

                public MethodBody Body
                {
                    get { return body; }
                }

                internal ILProcessor(MethodBody body)
                {
                    this.body = body;
                    this.instructions = body.Instructions;
                }

                public Instruction Create(OpCode opcode)
                {
                    return Instruction.Create(opcode);
                }

                public Instruction Create(OpCode opcode, TypeReference type)
                {
                    return Instruction.Create(opcode, type);
                }

                public Instruction Create(OpCode opcode, CallSite site)
                {
                    return Instruction.Create(opcode, site);
                }

                public Instruction Create(OpCode opcode, MethodReference method)
                {
                    return Instruction.Create(opcode, method);
                }

                public Instruction Create(OpCode opcode, FieldReference field)
                {
                    return Instruction.Create(opcode, field);
                }

                public Instruction Create(OpCode opcode, string value)
                {
                    return Instruction.Create(opcode, value);
                }

                public Instruction Create(OpCode opcode, sbyte value)
                {
                    return Instruction.Create(opcode, value);
                }

                public Instruction Create(OpCode opcode, byte value)
                {
                    if (opcode.OperandType == OperandType.ShortInlineVar)
                        return Instruction.Create(opcode, body.Variables[value]);

                    if (opcode.OperandType == OperandType.ShortInlineArg)
                        return Instruction.Create(opcode, body.GetParameter(value));

                    return Instruction.Create(opcode, value);
                }

                public Instruction Create(OpCode opcode, int value)
                {
                    if (opcode.OperandType == OperandType.InlineVar)
                        return Instruction.Create(opcode, body.Variables[value]);

                    if (opcode.OperandType == OperandType.InlineArg)
                        return Instruction.Create(opcode, body.GetParameter(value));

                    return Instruction.Create(opcode, value);
                }

                public Instruction Create(OpCode opcode, long value)
                {
                    return Instruction.Create(opcode, value);
                }

                public Instruction Create(OpCode opcode, float value)
                {
                    return Instruction.Create(opcode, value);
                }

                public Instruction Create(OpCode opcode, double value)
                {
                    return Instruction.Create(opcode, value);
                }

                public Instruction Create(OpCode opcode, Instruction target)
                {
                    return Instruction.Create(opcode, target);
                }

                public Instruction Create(OpCode opcode, Instruction[] targets)
                {
                    return Instruction.Create(opcode, targets);
                }

                public Instruction Create(OpCode opcode, VariableDefinition variable)
                {
                    return Instruction.Create(opcode, variable);
                }

                public Instruction Create(OpCode opcode, ParameterDefinition parameter)
                {
                    return Instruction.Create(opcode, parameter);
                }

                public void Emit(OpCode opcode)
                {
                    Append(Create(opcode));
                }

                public void Emit(OpCode opcode, TypeReference type)
                {
                    Append(Create(opcode, type));
                }

                public void Emit(OpCode opcode, MethodReference method)
                {
                    Append(Create(opcode, method));
                }

                public void Emit(OpCode opcode, CallSite site)
                {
                    Append(Create(opcode, site));
                }

                public void Emit(OpCode opcode, FieldReference field)
                {
                    Append(Create(opcode, field));
                }

                public void Emit(OpCode opcode, string value)
                {
                    Append(Create(opcode, value));
                }

                public void Emit(OpCode opcode, byte value)
                {
                    Append(Create(opcode, value));
                }

                public void Emit(OpCode opcode, sbyte value)
                {
                    Append(Create(opcode, value));
                }

                public void Emit(OpCode opcode, int value)
                {
                    Append(Create(opcode, value));
                }

                public void Emit(OpCode opcode, long value)
                {
                    Append(Create(opcode, value));
                }

                public void Emit(OpCode opcode, float value)
                {
                    Append(Create(opcode, value));
                }

                public void Emit(OpCode opcode, double value)
                {
                    Append(Create(opcode, value));
                }

                public void Emit(OpCode opcode, Instruction target)
                {
                    Append(Create(opcode, target));
                }

                public void Emit(OpCode opcode, Instruction[] targets)
                {
                    Append(Create(opcode, targets));
                }

                public void Emit(OpCode opcode, VariableDefinition variable)
                {
                    Append(Create(opcode, variable));
                }

                public void Emit(OpCode opcode, ParameterDefinition parameter)
                {
                    Append(Create(opcode, parameter));
                }

                public void InsertBefore(Instruction target, Instruction instruction)
                {
                    if (target == null)
                        throw new ArgumentNullException("target");
                    if (instruction == null)
                        throw new ArgumentNullException("instruction");

                    var index = instructions.IndexOf(target);
                    if (index == -1)
                        throw new ArgumentOutOfRangeException("target");

                    instructions.Insert(index, instruction);
                }

                public void InsertAfter(Instruction target, Instruction instruction)
                {
                    if (target == null)
                        throw new ArgumentNullException("target");
                    if (instruction == null)
                        throw new ArgumentNullException("instruction");

                    var index = instructions.IndexOf(target);
                    if (index == -1)
                        throw new ArgumentOutOfRangeException("target");

                    instructions.Insert(index + 1, instruction);
                }

                public void Append(Instruction instruction)
                {
                    if (instruction == null)
                        throw new ArgumentNullException("instruction");

                    instructions.Add(instruction);
                }

                public void Replace(Instruction target, Instruction instruction)
                {
                    if (target == null)
                        throw new ArgumentNullException("target");
                    if (instruction == null)
                        throw new ArgumentNullException("instruction");

                    InsertAfter(target, instruction);
                    Remove(target);
                }

                public void Remove(Instruction instruction)
                {
                    if (instruction == null)
                        throw new ArgumentNullException("instruction");

                    if (!instructions.Remove(instruction))
                        throw new ArgumentOutOfRangeException("instruction");
                }
            }



            public sealed class Instruction
            {

                internal int offset;
                internal OpCode opcode;
                internal object operand;

                internal Instruction previous;
                internal Instruction next;

                SequencePoint sequence_point;

                public int Offset
                {
                    get { return offset; }
                    set { offset = value; }
                }

                public OpCode OpCode
                {
                    get { return opcode; }
                    set { opcode = value; }
                }

                public object Operand
                {
                    get { return operand; }
                    set { operand = value; }
                }

                public Instruction Previous
                {
                    get { return previous; }
                    set { previous = value; }
                }

                public Instruction Next
                {
                    get { return next; }
                    set { next = value; }
                }

                public SequencePoint SequencePoint
                {
                    get { return sequence_point; }
                    set { sequence_point = value; }
                }

                internal Instruction(int offset, OpCode opCode)
                {
                    this.offset = offset;
                    this.opcode = opCode;
                }

                internal Instruction(OpCode opcode, object operand)
                {
                    this.opcode = opcode;
                    this.operand = operand;
                }

                public int GetSize()
                {
                    int size = opcode.Size;

                    switch (opcode.OperandType)
                    {
                        case OperandType.InlineSwitch:
                            return size + (1 + ((Instruction[])operand).Length) * 4;
                        case OperandType.InlineI8:
                        case OperandType.InlineR:
                            return size + 8;
                        case OperandType.InlineBrTarget:
                        case OperandType.InlineField:
                        case OperandType.InlineI:
                        case OperandType.InlineMethod:
                        case OperandType.InlineString:
                        case OperandType.InlineTok:
                        case OperandType.InlineType:
                        case OperandType.ShortInlineR:
                        case OperandType.InlineSig:
                            return size + 4;
                        case OperandType.InlineArg:
                        case OperandType.InlineVar:
                            return size + 2;
                        case OperandType.ShortInlineBrTarget:
                        case OperandType.ShortInlineI:
                        case OperandType.ShortInlineArg:
                        case OperandType.ShortInlineVar:
                            return size + 1;
                        default:
                            return size;
                    }
                }

                public override string ToString()
                {
                    var instruction = new StringBuilder();

                    AppendLabel(instruction, this);
                    instruction.Append(':');
                    instruction.Append(' ');
                    instruction.Append(opcode.Name);

                    if (operand == null)
                        return instruction.ToString();

                    instruction.Append(' ');

                    switch (opcode.OperandType)
                    {
                        case OperandType.ShortInlineBrTarget:
                        case OperandType.InlineBrTarget:
                            AppendLabel(instruction, (Instruction)operand);
                            break;
                        case OperandType.InlineSwitch:
                            var labels = (Instruction[])operand;
                            for (int i = 0; i < labels.Length; i++)
                            {
                                if (i > 0)
                                    instruction.Append(',');

                                AppendLabel(instruction, labels[i]);
                            }
                            break;
                        case OperandType.InlineString:
                            instruction.Append('\"');
                            instruction.Append(operand);
                            instruction.Append('\"');
                            break;
                        default:
                            instruction.Append(operand);
                            break;
                    }

                    return instruction.ToString();
                }

                static void AppendLabel(StringBuilder builder, Instruction instruction)
                {
                    builder.Append("IL_");
                    builder.Append(instruction.offset.ToString("x4"));
                }

                public static Instruction Create(OpCode opcode)
                {
                    if (opcode.OperandType != OperandType.InlineNone)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, null);
                }

                public static Instruction Create(OpCode opcode, TypeReference type)
                {
                    if (type == null)
                        throw new ArgumentNullException("type");
                    if (opcode.OperandType != OperandType.InlineType &&
                        opcode.OperandType != OperandType.InlineTok)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, type);
                }

                public static Instruction Create(OpCode opcode, CallSite site)
                {
                    if (site == null)
                        throw new ArgumentNullException("site");
                    if (opcode.Code != Code.Calli)
                        throw new ArgumentException("code");

                    return new Instruction(opcode, site);
                }

                public static Instruction Create(OpCode opcode, MethodReference method)
                {
                    if (method == null)
                        throw new ArgumentNullException("method");
                    if (opcode.OperandType != OperandType.InlineMethod &&
                        opcode.OperandType != OperandType.InlineTok)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, method);
                }

                public static Instruction Create(OpCode opcode, FieldReference field)
                {
                    if (field == null)
                        throw new ArgumentNullException("field");
                    if (opcode.OperandType != OperandType.InlineField &&
                        opcode.OperandType != OperandType.InlineTok)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, field);
                }

                public static Instruction Create(OpCode opcode, string value)
                {
                    if (value == null)
                        throw new ArgumentNullException("value");
                    if (opcode.OperandType != OperandType.InlineString)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, value);
                }

                public static Instruction Create(OpCode opcode, sbyte value)
                {
                    if (opcode.OperandType != OperandType.ShortInlineI &&
                        opcode != OpCodes.Ldc_I4_S)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, value);
                }

                public static Instruction Create(OpCode opcode, byte value)
                {
                    if (opcode.OperandType != OperandType.ShortInlineI ||
                        opcode == OpCodes.Ldc_I4_S)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, value);
                }

                public static Instruction Create(OpCode opcode, int value)
                {
                    if (opcode.OperandType != OperandType.InlineI)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, value);
                }

                public static Instruction Create(OpCode opcode, long value)
                {
                    if (opcode.OperandType != OperandType.InlineI8)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, value);
                }

                public static Instruction Create(OpCode opcode, float value)
                {
                    if (opcode.OperandType != OperandType.ShortInlineR)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, value);
                }

                public static Instruction Create(OpCode opcode, double value)
                {
                    if (opcode.OperandType != OperandType.InlineR)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, value);
                }

                public static Instruction Create(OpCode opcode, Instruction target)
                {
                    if (target == null)
                        throw new ArgumentNullException("target");
                    if (opcode.OperandType != OperandType.InlineBrTarget &&
                        opcode.OperandType != OperandType.ShortInlineBrTarget)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, target);
                }

                public static Instruction Create(OpCode opcode, Instruction[] targets)
                {
                    if (targets == null)
                        throw new ArgumentNullException("targets");
                    if (opcode.OperandType != OperandType.InlineSwitch)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, targets);
                }

                public static Instruction Create(OpCode opcode, VariableDefinition variable)
                {
                    if (variable == null)
                        throw new ArgumentNullException("variable");
                    if (opcode.OperandType != OperandType.ShortInlineVar &&
                        opcode.OperandType != OperandType.InlineVar)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, variable);
                }

                public static Instruction Create(OpCode opcode, ParameterDefinition parameter)
                {
                    if (parameter == null)
                        throw new ArgumentNullException("parameter");
                    if (opcode.OperandType != OperandType.ShortInlineArg &&
                        opcode.OperandType != OperandType.InlineArg)
                        throw new ArgumentException("opcode");

                    return new Instruction(opcode, parameter);
                }
            }


            public sealed class MethodBody : IVariableDefinitionProvider
            {

                readonly internal MethodDefinition method;

                internal ParameterDefinition this_parameter;
                internal int max_stack_size;
                internal int code_size;
                internal bool init_locals;
                internal MetadataToken local_var_token;

                internal Collection<Instruction> instructions;
                internal Collection<ExceptionHandler> exceptions;
                internal Collection<VariableDefinition> variables;
                Scope scope;

                public MethodDefinition Method
                {
                    get { return method; }
                }

                public int MaxStackSize
                {
                    get { return max_stack_size; }
                    set { max_stack_size = value; }
                }

                public int CodeSize
                {
                    get { return code_size; }
                }

                public bool InitLocals
                {
                    get { return init_locals; }
                    set { init_locals = value; }
                }

                public MetadataToken LocalVarToken
                {
                    get { return local_var_token; }
                    set { local_var_token = value; }
                }

                public Collection<Instruction> Instructions
                {
                    get { return instructions ?? (instructions = new InstructionCollection()); }
                }

                public bool HasExceptionHandlers
                {
                    get { return !exceptions.IsNullOrEmpty(); }
                }

                public Collection<ExceptionHandler> ExceptionHandlers
                {
                    get { return exceptions ?? (exceptions = new Collection<ExceptionHandler>()); }
                }

                public bool HasVariables
                {
                    get { return !variables.IsNullOrEmpty(); }
                }

                public Collection<VariableDefinition> Variables
                {
                    get { return variables ?? (variables = new VariableDefinitionCollection()); }
                }

                public Scope Scope
                {
                    get { return scope; }
                    set { scope = value; }
                }

                public ParameterDefinition ThisParameter
                {
                    get
                    {
                        if (method == null || method.DeclaringType == null)
                            throw new NotSupportedException();

                        return this_parameter ?? (this_parameter = new ParameterDefinition("0", ParameterAttributes.None, method.DeclaringType));
                    }
                }

                public MethodBody(MethodDefinition method)
                {
                    this.method = method;
                }

                public ILProcessor GetILProcessor()
                {
                    return new ILProcessor(this);
                }
            }

            public interface IVariableDefinitionProvider
            {
                bool HasVariables { get; }
                Collection<VariableDefinition> Variables { get; }
            }

            class VariableDefinitionCollection : Collection<VariableDefinition>
            {

                internal VariableDefinitionCollection()
                {
                }

                internal VariableDefinitionCollection(int capacity)
                    : base(capacity)
                {
                }

                protected override void OnAdd(VariableDefinition item, int index)
                {
                    item.index = index;
                }

                protected override void OnInsert(VariableDefinition item, int index)
                {
                    item.index = index;

                    for (int i = index; i < size; i++)
                        items[i].index = i + 1;
                }

                protected override void OnSet(VariableDefinition item, int index)
                {
                    item.index = index;
                }

                protected override void OnRemove(VariableDefinition item, int index)
                {
                    item.index = -1;

                    for (int i = index + 1; i < size; i++)
                        items[i].index = i - 1;
                }
            }

            class InstructionCollection : Collection<Instruction>
            {

                internal InstructionCollection()
                {
                }

                internal InstructionCollection(int capacity)
                    : base(capacity)
                {
                }

                protected override void OnAdd(Instruction item, int index)
                {
                    if (index == 0)
                        return;

                    var previous = items[index - 1];
                    previous.next = item;
                    item.previous = previous;
                }

                protected override void OnInsert(Instruction item, int index)
                {
                    if (size == 0)
                        return;

                    var current = items[index];
                    if (current == null)
                    {
                        var last = items[index - 1];
                        last.next = item;
                        item.previous = last;
                        return;
                    }

                    var previous = current.previous;
                    if (previous != null)
                    {
                        previous.next = item;
                        item.previous = previous;
                    }

                    current.previous = item;
                    item.next = current;
                }

                protected override void OnSet(Instruction item, int index)
                {
                    var current = items[index];

                    item.previous = current.previous;
                    item.next = current.next;

                    current.previous = null;
                    current.next = null;
                }

                protected override void OnRemove(Instruction item, int index)
                {
                    var previous = item.previous;
                    if (previous != null)
                        previous.next = item.next;

                    var next = item.next;
                    if (next != null)
                        next.previous = item.previous;

                    item.previous = null;
                    item.next = null;
                }
            }


            public enum FlowControl
            {
                Branch,
                Break,
                Call,
                Cond_Branch,
                Meta,
                Next,
                Phi,
                Return,
                Throw,
            }

            public enum OpCodeType
            {
                Annotation,
                Macro,
                Nternal,
                Objmodel,
                Prefix,
                Primitive,
            }

            public enum OperandType
            {
                InlineBrTarget,
                InlineField,
                InlineI,
                InlineI8,
                InlineMethod,
                InlineNone,
                InlinePhi,
                InlineR,
                InlineSig,
                InlineString,
                InlineSwitch,
                InlineTok,
                InlineType,
                InlineVar,
                InlineArg,
                ShortInlineBrTarget,
                ShortInlineI,
                ShortInlineR,
                ShortInlineVar,
                ShortInlineArg,
            }

            public enum StackBehaviour
            {
                Pop0,
                Pop1,
                Pop1_pop1,
                Popi,
                Popi_pop1,
                Popi_popi,
                Popi_popi8,
                Popi_popi_popi,
                Popi_popr4,
                Popi_popr8,
                Popref,
                Popref_pop1,
                Popref_popi,
                Popref_popi_popi,
                Popref_popi_popi8,
                Popref_popi_popr4,
                Popref_popi_popr8,
                Popref_popi_popref,
                PopAll,
                Push0,
                Push1,
                Push1_push1,
                Pushi,
                Pushi8,
                Pushr4,
                Pushr8,
                Pushref,
                Varpop,
                Varpush,
            }

            public struct OpCode
            {

                readonly byte op1;
                readonly byte op2;
                readonly byte code;
                readonly byte flow_control;
                readonly byte opcode_type;
                readonly byte operand_type;
                readonly byte stack_behavior_pop;
                readonly byte stack_behavior_push;

                public string Name
                {
                    get { return OpCodeNames.names[op1 == 0xff ? op2 : op2 + 256]; }
                }

                public int Size
                {
                    get { return op1 == 0xff ? 1 : 2; }
                }

                public byte Op1
                {
                    get { return op1; }
                }

                public byte Op2
                {
                    get { return op2; }
                }

                public short Value
                {
                    get { return (short)((op1 << 8) | op2); }
                }

                public Code Code
                {
                    get { return (Code)code; }
                }

                public FlowControl FlowControl
                {
                    get { return (FlowControl)flow_control; }
                }

                public OpCodeType OpCodeType
                {
                    get { return (OpCodeType)opcode_type; }
                }

                public OperandType OperandType
                {
                    get { return (OperandType)operand_type; }
                }

                public StackBehaviour StackBehaviourPop
                {
                    get { return (StackBehaviour)stack_behavior_pop; }
                }

                public StackBehaviour StackBehaviourPush
                {
                    get { return (StackBehaviour)stack_behavior_push; }
                }

                internal OpCode(int x, int y)
                {
                    this.op1 = (byte)((x >> 0) & 0xff);
                    this.op2 = (byte)((x >> 8) & 0xff);
                    this.code = (byte)((x >> 16) & 0xff);
                    this.flow_control = (byte)((x >> 24) & 0xff);

                    this.opcode_type = (byte)((y >> 0) & 0xff);
                    this.operand_type = (byte)((y >> 8) & 0xff);
                    this.stack_behavior_pop = (byte)((y >> 16) & 0xff);
                    this.stack_behavior_push = (byte)((y >> 24) & 0xff);

                    if (op1 == 0xff)
                        OpCodes.OneByteOpCode[op2] = this;
                    else
                        OpCodes.TwoBytesOpCode[op2] = this;
                }

                public override int GetHashCode()
                {
                    return Value;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is OpCode))
                        return false;

                    var opcode = (OpCode)obj;
                    return op1 == opcode.op1 && op2 == opcode.op2;
                }

                public bool Equals(OpCode opcode)
                {
                    return op1 == opcode.op1 && op2 == opcode.op2;
                }

                public static bool operator ==(OpCode one, OpCode other)
                {
                    return one.op1 == other.op1 && one.op2 == other.op2;
                }

                public static bool operator !=(OpCode one, OpCode other)
                {
                    return one.op1 != other.op1 || one.op2 != other.op2;
                }

                public override string ToString()
                {
                    return Name;
                }
            }

            static class OpCodeNames
            {

                internal static readonly string[] names = {
			"nop",
			"break",
			"ldarg.0",
			"ldarg.1",
			"ldarg.2",
			"ldarg.3",
			"ldloc.0",
			"ldloc.1",
			"ldloc.2",
			"ldloc.3",
			"stloc.0",
			"stloc.1",
			"stloc.2",
			"stloc.3",
			"ldarg.s",
			"ldarga.s",
			"starg.s",
			"ldloc.s",
			"ldloca.s",
			"stloc.s",
			"ldnull",
			"ldc.i4.m1",
			"ldc.i4.0",
			"ldc.i4.1",
			"ldc.i4.2",
			"ldc.i4.3",
			"ldc.i4.4",
			"ldc.i4.5",
			"ldc.i4.6",
			"ldc.i4.7",
			"ldc.i4.8",
			"ldc.i4.s",
			"ldc.i4",
			"ldc.i8",
			"ldc.r4",
			"ldc.r8",
			null,
			"dup",
			"pop",
			"jmp",
			"call",
			"calli",
			"ret",
			"br.s",
			"brfalse.s",
			"brtrue.s",
			"beq.s",
			"bge.s",
			"bgt.s",
			"ble.s",
			"blt.s",
			"bne.un.s",
			"bge.un.s",
			"bgt.un.s",
			"ble.un.s",
			"blt.un.s",
			"br",
			"brfalse",
			"brtrue",
			"beq",
			"bge",
			"bgt",
			"ble",
			"blt",
			"bne.un",
			"bge.un",
			"bgt.un",
			"ble.un",
			"blt.un",
			"switch",
			"ldind.i1",
			"ldind.u1",
			"ldind.i2",
			"ldind.u2",
			"ldind.i4",
			"ldind.u4",
			"ldind.i8",
			"ldind.i",
			"ldind.r4",
			"ldind.r8",
			"ldind.ref",
			"stind.ref",
			"stind.i1",
			"stind.i2",
			"stind.i4",
			"stind.i8",
			"stind.r4",
			"stind.r8",
			"add",
			"sub",
			"mul",
			"div",
			"div.un",
			"rem",
			"rem.un",
			"and",
			"or",
			"xor",
			"shl",
			"shr",
			"shr.un",
			"neg",
			"not",
			"conv.i1",
			"conv.i2",
			"conv.i4",
			"conv.i8",
			"conv.r4",
			"conv.r8",
			"conv.u4",
			"conv.u8",
			"callvirt",
			"cpobj",
			"ldobj",
			"ldstr",
			"newobj",
			"castclass",
			"isinst",
			"conv.r.un",
			null,
			null,
			"unbox",
			"throw",
			"ldfld",
			"ldflda",
			"stfld",
			"ldsfld",
			"ldsflda",
			"stsfld",
			"stobj",
			"conv.ovf.i1.un",
			"conv.ovf.i2.un",
			"conv.ovf.i4.un",
			"conv.ovf.i8.un",
			"conv.ovf.u1.un",
			"conv.ovf.u2.un",
			"conv.ovf.u4.un",
			"conv.ovf.u8.un",
			"conv.ovf.i.un",
			"conv.ovf.u.un",
			"box",
			"newarr",
			"ldlen",
			"ldelema",
			"ldelem.i1",
			"ldelem.u1",
			"ldelem.i2",
			"ldelem.u2",
			"ldelem.i4",
			"ldelem.u4",
			"ldelem.i8",
			"ldelem.i",
			"ldelem.r4",
			"ldelem.r8",
			"ldelem.ref",
			"stelem.i",
			"stelem.i1",
			"stelem.i2",
			"stelem.i4",
			"stelem.i8",
			"stelem.r4",
			"stelem.r8",
			"stelem.ref",
			"ldelem.any",
			"stelem.any",
			"unbox.any",
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			"conv.ovf.i1",
			"conv.ovf.u1",
			"conv.ovf.i2",
			"conv.ovf.u2",
			"conv.ovf.i4",
			"conv.ovf.u4",
			"conv.ovf.i8",
			"conv.ovf.u8",
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			"refanyval",
			"ckfinite",
			null,
			null,
			"mkrefany",
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			"ldtoken",
			"conv.u2",
			"conv.u1",
			"conv.i",
			"conv.ovf.i",
			"conv.ovf.u",
			"add.ovf",
			"add.ovf.un",
			"mul.ovf",
			"mul.ovf.un",
			"sub.ovf",
			"sub.ovf.un",
			"endfinally",
			"leave",
			"leave.s",
			"stind.i",
			"conv.u",
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			"prefix7",
			"prefix6",
			"prefix5",
			"prefix4",
			"prefix3",
			"prefix2",
			"prefix1",
			"prefixref",
			"arglist",
			"ceq",
			"cgt",
			"cgt.un",
			"clt",
			"clt.un",
			"ldftn",
			"ldvirtftn",
			null,
			"ldarg",
			"ldarga",
			"starg",
			"ldloc",
			"ldloca",
			"stloc",
			"localloc",
			null,
			"endfilter",
			"unaligned.",
			"volatile.",
			"tail.",
			"initobj",
			"constrained.",
			"cpblk",
			"initblk",
			"no.",
			"rethrow",
			null,
			"sizeof",
			"refanytype",
			"readonly.",
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
		};
            }


            public static class OpCodes
            {

                internal static readonly OpCode[] OneByteOpCode = new OpCode[0xe0 + 1];
                internal static readonly OpCode[] TwoBytesOpCode = new OpCode[0x1e + 1];

                public static readonly OpCode Nop = new OpCode(
                    0xff << 0 | 0x00 << 8 | (byte)Code.Nop << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Break = new OpCode(
                    0xff << 0 | 0x01 << 8 | (byte)Code.Break << 16 | (byte)FlowControl.Break << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldarg_0 = new OpCode(
                    0xff << 0 | 0x02 << 8 | (byte)Code.Ldarg_0 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldarg_1 = new OpCode(
                    0xff << 0 | 0x03 << 8 | (byte)Code.Ldarg_1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldarg_2 = new OpCode(
                    0xff << 0 | 0x04 << 8 | (byte)Code.Ldarg_2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldarg_3 = new OpCode(
                    0xff << 0 | 0x05 << 8 | (byte)Code.Ldarg_3 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldloc_0 = new OpCode(
                    0xff << 0 | 0x06 << 8 | (byte)Code.Ldloc_0 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldloc_1 = new OpCode(
                    0xff << 0 | 0x07 << 8 | (byte)Code.Ldloc_1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldloc_2 = new OpCode(
                    0xff << 0 | 0x08 << 8 | (byte)Code.Ldloc_2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldloc_3 = new OpCode(
                    0xff << 0 | 0x09 << 8 | (byte)Code.Ldloc_3 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Stloc_0 = new OpCode(
                    0xff << 0 | 0x0a << 8 | (byte)Code.Stloc_0 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stloc_1 = new OpCode(
                    0xff << 0 | 0x0b << 8 | (byte)Code.Stloc_1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stloc_2 = new OpCode(
                    0xff << 0 | 0x0c << 8 | (byte)Code.Stloc_2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stloc_3 = new OpCode(
                    0xff << 0 | 0x0d << 8 | (byte)Code.Stloc_3 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldarg_S = new OpCode(
                    0xff << 0 | 0x0e << 8 | (byte)Code.Ldarg_S << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineArg << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldarga_S = new OpCode(
                    0xff << 0 | 0x0f << 8 | (byte)Code.Ldarga_S << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineArg << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Starg_S = new OpCode(
                    0xff << 0 | 0x10 << 8 | (byte)Code.Starg_S << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineArg << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldloc_S = new OpCode(
                    0xff << 0 | 0x11 << 8 | (byte)Code.Ldloc_S << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineVar << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldloca_S = new OpCode(
                    0xff << 0 | 0x12 << 8 | (byte)Code.Ldloca_S << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineVar << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Stloc_S = new OpCode(
                    0xff << 0 | 0x13 << 8 | (byte)Code.Stloc_S << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineVar << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldnull = new OpCode(
                    0xff << 0 | 0x14 << 8 | (byte)Code.Ldnull << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushref << 24);

                public static readonly OpCode Ldc_I4_M1 = new OpCode(
                    0xff << 0 | 0x15 << 8 | (byte)Code.Ldc_I4_M1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_0 = new OpCode(
                    0xff << 0 | 0x16 << 8 | (byte)Code.Ldc_I4_0 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_1 = new OpCode(
                    0xff << 0 | 0x17 << 8 | (byte)Code.Ldc_I4_1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_2 = new OpCode(
                    0xff << 0 | 0x18 << 8 | (byte)Code.Ldc_I4_2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_3 = new OpCode(
                    0xff << 0 | 0x19 << 8 | (byte)Code.Ldc_I4_3 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_4 = new OpCode(
                    0xff << 0 | 0x1a << 8 | (byte)Code.Ldc_I4_4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_5 = new OpCode(
                    0xff << 0 | 0x1b << 8 | (byte)Code.Ldc_I4_5 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_6 = new OpCode(
                    0xff << 0 | 0x1c << 8 | (byte)Code.Ldc_I4_6 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_7 = new OpCode(
                    0xff << 0 | 0x1d << 8 | (byte)Code.Ldc_I4_7 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_8 = new OpCode(
                    0xff << 0 | 0x1e << 8 | (byte)Code.Ldc_I4_8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4_S = new OpCode(
                    0xff << 0 | 0x1f << 8 | (byte)Code.Ldc_I4_S << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineI << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I4 = new OpCode(
                    0xff << 0 | 0x20 << 8 | (byte)Code.Ldc_I4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineI << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldc_I8 = new OpCode(
                    0xff << 0 | 0x21 << 8 | (byte)Code.Ldc_I8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineI8 << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Ldc_R4 = new OpCode(
                    0xff << 0 | 0x22 << 8 | (byte)Code.Ldc_R4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.ShortInlineR << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushr4 << 24);

                public static readonly OpCode Ldc_R8 = new OpCode(
                    0xff << 0 | 0x23 << 8 | (byte)Code.Ldc_R8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineR << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushr8 << 24);

                public static readonly OpCode Dup = new OpCode(
                    0xff << 0 | 0x25 << 8 | (byte)Code.Dup << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push1_push1 << 24);

                public static readonly OpCode Pop = new OpCode(
                    0xff << 0 | 0x26 << 8 | (byte)Code.Pop << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Jmp = new OpCode(
                    0xff << 0 | 0x27 << 8 | (byte)Code.Jmp << 16 | (byte)FlowControl.Call << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineMethod << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Call = new OpCode(
                    0xff << 0 | 0x28 << 8 | (byte)Code.Call << 16 | (byte)FlowControl.Call << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineMethod << 8 | (byte)StackBehaviour.Varpop << 16 | (byte)StackBehaviour.Varpush << 24);

                public static readonly OpCode Calli = new OpCode(
                    0xff << 0 | 0x29 << 8 | (byte)Code.Calli << 16 | (byte)FlowControl.Call << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineSig << 8 | (byte)StackBehaviour.Varpop << 16 | (byte)StackBehaviour.Varpush << 24);

                public static readonly OpCode Ret = new OpCode(
                    0xff << 0 | 0x2a << 8 | (byte)Code.Ret << 16 | (byte)FlowControl.Return << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Varpop << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Br_S = new OpCode(
                    0xff << 0 | 0x2b << 8 | (byte)Code.Br_S << 16 | (byte)FlowControl.Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Brfalse_S = new OpCode(
                    0xff << 0 | 0x2c << 8 | (byte)Code.Brfalse_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Brtrue_S = new OpCode(
                    0xff << 0 | 0x2d << 8 | (byte)Code.Brtrue_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Beq_S = new OpCode(
                    0xff << 0 | 0x2e << 8 | (byte)Code.Beq_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bge_S = new OpCode(
                    0xff << 0 | 0x2f << 8 | (byte)Code.Bge_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bgt_S = new OpCode(
                    0xff << 0 | 0x30 << 8 | (byte)Code.Bgt_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ble_S = new OpCode(
                    0xff << 0 | 0x31 << 8 | (byte)Code.Ble_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Blt_S = new OpCode(
                    0xff << 0 | 0x32 << 8 | (byte)Code.Blt_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bne_Un_S = new OpCode(
                    0xff << 0 | 0x33 << 8 | (byte)Code.Bne_Un_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bge_Un_S = new OpCode(
                    0xff << 0 | 0x34 << 8 | (byte)Code.Bge_Un_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bgt_Un_S = new OpCode(
                    0xff << 0 | 0x35 << 8 | (byte)Code.Bgt_Un_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ble_Un_S = new OpCode(
                    0xff << 0 | 0x36 << 8 | (byte)Code.Ble_Un_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Blt_Un_S = new OpCode(
                    0xff << 0 | 0x37 << 8 | (byte)Code.Blt_Un_S << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Br = new OpCode(
                    0xff << 0 | 0x38 << 8 | (byte)Code.Br << 16 | (byte)FlowControl.Branch << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Brfalse = new OpCode(
                    0xff << 0 | 0x39 << 8 | (byte)Code.Brfalse << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Brtrue = new OpCode(
                    0xff << 0 | 0x3a << 8 | (byte)Code.Brtrue << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Beq = new OpCode(
                    0xff << 0 | 0x3b << 8 | (byte)Code.Beq << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bge = new OpCode(
                    0xff << 0 | 0x3c << 8 | (byte)Code.Bge << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bgt = new OpCode(
                    0xff << 0 | 0x3d << 8 | (byte)Code.Bgt << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ble = new OpCode(
                    0xff << 0 | 0x3e << 8 | (byte)Code.Ble << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Blt = new OpCode(
                    0xff << 0 | 0x3f << 8 | (byte)Code.Blt << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bne_Un = new OpCode(
                    0xff << 0 | 0x40 << 8 | (byte)Code.Bne_Un << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bge_Un = new OpCode(
                    0xff << 0 | 0x41 << 8 | (byte)Code.Bge_Un << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Bgt_Un = new OpCode(
                    0xff << 0 | 0x42 << 8 | (byte)Code.Bgt_Un << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ble_Un = new OpCode(
                    0xff << 0 | 0x43 << 8 | (byte)Code.Ble_Un << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Blt_Un = new OpCode(
                    0xff << 0 | 0x44 << 8 | (byte)Code.Blt_Un << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Switch = new OpCode(
                    0xff << 0 | 0x45 << 8 | (byte)Code.Switch << 16 | (byte)FlowControl.Cond_Branch << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineSwitch << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldind_I1 = new OpCode(
                    0xff << 0 | 0x46 << 8 | (byte)Code.Ldind_I1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldind_U1 = new OpCode(
                    0xff << 0 | 0x47 << 8 | (byte)Code.Ldind_U1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldind_I2 = new OpCode(
                    0xff << 0 | 0x48 << 8 | (byte)Code.Ldind_I2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldind_U2 = new OpCode(
                    0xff << 0 | 0x49 << 8 | (byte)Code.Ldind_U2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldind_I4 = new OpCode(
                    0xff << 0 | 0x4a << 8 | (byte)Code.Ldind_I4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldind_U4 = new OpCode(
                    0xff << 0 | 0x4b << 8 | (byte)Code.Ldind_U4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldind_I8 = new OpCode(
                    0xff << 0 | 0x4c << 8 | (byte)Code.Ldind_I8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Ldind_I = new OpCode(
                    0xff << 0 | 0x4d << 8 | (byte)Code.Ldind_I << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldind_R4 = new OpCode(
                    0xff << 0 | 0x4e << 8 | (byte)Code.Ldind_R4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushr4 << 24);

                public static readonly OpCode Ldind_R8 = new OpCode(
                    0xff << 0 | 0x4f << 8 | (byte)Code.Ldind_R8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushr8 << 24);

                public static readonly OpCode Ldind_Ref = new OpCode(
                    0xff << 0 | 0x50 << 8 | (byte)Code.Ldind_Ref << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushref << 24);

                public static readonly OpCode Stind_Ref = new OpCode(
                    0xff << 0 | 0x51 << 8 | (byte)Code.Stind_Ref << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stind_I1 = new OpCode(
                    0xff << 0 | 0x52 << 8 | (byte)Code.Stind_I1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stind_I2 = new OpCode(
                    0xff << 0 | 0x53 << 8 | (byte)Code.Stind_I2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stind_I4 = new OpCode(
                    0xff << 0 | 0x54 << 8 | (byte)Code.Stind_I4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stind_I8 = new OpCode(
                    0xff << 0 | 0x55 << 8 | (byte)Code.Stind_I8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popi8 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stind_R4 = new OpCode(
                    0xff << 0 | 0x56 << 8 | (byte)Code.Stind_R4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popr4 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stind_R8 = new OpCode(
                    0xff << 0 | 0x57 << 8 | (byte)Code.Stind_R8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popr8 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Add = new OpCode(
                    0xff << 0 | 0x58 << 8 | (byte)Code.Add << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Sub = new OpCode(
                    0xff << 0 | 0x59 << 8 | (byte)Code.Sub << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Mul = new OpCode(
                    0xff << 0 | 0x5a << 8 | (byte)Code.Mul << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Div = new OpCode(
                    0xff << 0 | 0x5b << 8 | (byte)Code.Div << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Div_Un = new OpCode(
                    0xff << 0 | 0x5c << 8 | (byte)Code.Div_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Rem = new OpCode(
                    0xff << 0 | 0x5d << 8 | (byte)Code.Rem << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Rem_Un = new OpCode(
                    0xff << 0 | 0x5e << 8 | (byte)Code.Rem_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode And = new OpCode(
                    0xff << 0 | 0x5f << 8 | (byte)Code.And << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Or = new OpCode(
                    0xff << 0 | 0x60 << 8 | (byte)Code.Or << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Xor = new OpCode(
                    0xff << 0 | 0x61 << 8 | (byte)Code.Xor << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Shl = new OpCode(
                    0xff << 0 | 0x62 << 8 | (byte)Code.Shl << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Shr = new OpCode(
                    0xff << 0 | 0x63 << 8 | (byte)Code.Shr << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Shr_Un = new OpCode(
                    0xff << 0 | 0x64 << 8 | (byte)Code.Shr_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Neg = new OpCode(
                    0xff << 0 | 0x65 << 8 | (byte)Code.Neg << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Not = new OpCode(
                    0xff << 0 | 0x66 << 8 | (byte)Code.Not << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Conv_I1 = new OpCode(
                    0xff << 0 | 0x67 << 8 | (byte)Code.Conv_I1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_I2 = new OpCode(
                    0xff << 0 | 0x68 << 8 | (byte)Code.Conv_I2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_I4 = new OpCode(
                    0xff << 0 | 0x69 << 8 | (byte)Code.Conv_I4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_I8 = new OpCode(
                    0xff << 0 | 0x6a << 8 | (byte)Code.Conv_I8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Conv_R4 = new OpCode(
                    0xff << 0 | 0x6b << 8 | (byte)Code.Conv_R4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushr4 << 24);

                public static readonly OpCode Conv_R8 = new OpCode(
                    0xff << 0 | 0x6c << 8 | (byte)Code.Conv_R8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushr8 << 24);

                public static readonly OpCode Conv_U4 = new OpCode(
                    0xff << 0 | 0x6d << 8 | (byte)Code.Conv_U4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_U8 = new OpCode(
                    0xff << 0 | 0x6e << 8 | (byte)Code.Conv_U8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Callvirt = new OpCode(
                    0xff << 0 | 0x6f << 8 | (byte)Code.Callvirt << 16 | (byte)FlowControl.Call << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineMethod << 8 | (byte)StackBehaviour.Varpop << 16 | (byte)StackBehaviour.Varpush << 24);

                public static readonly OpCode Cpobj = new OpCode(
                    0xff << 0 | 0x70 << 8 | (byte)Code.Cpobj << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldobj = new OpCode(
                    0xff << 0 | 0x71 << 8 | (byte)Code.Ldobj << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldstr = new OpCode(
                    0xff << 0 | 0x72 << 8 | (byte)Code.Ldstr << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineString << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushref << 24);

                public static readonly OpCode Newobj = new OpCode(
                    0xff << 0 | 0x73 << 8 | (byte)Code.Newobj << 16 | (byte)FlowControl.Call << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineMethod << 8 | (byte)StackBehaviour.Varpop << 16 | (byte)StackBehaviour.Pushref << 24);

                public static readonly OpCode Castclass = new OpCode(
                    0xff << 0 | 0x74 << 8 | (byte)Code.Castclass << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Pushref << 24);

                public static readonly OpCode Isinst = new OpCode(
                    0xff << 0 | 0x75 << 8 | (byte)Code.Isinst << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_R_Un = new OpCode(
                    0xff << 0 | 0x76 << 8 | (byte)Code.Conv_R_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushr8 << 24);

                public static readonly OpCode Unbox = new OpCode(
                    0xff << 0 | 0x79 << 8 | (byte)Code.Unbox << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Throw = new OpCode(
                    0xff << 0 | 0x7a << 8 | (byte)Code.Throw << 16 | (byte)FlowControl.Throw << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldfld = new OpCode(
                    0xff << 0 | 0x7b << 8 | (byte)Code.Ldfld << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineField << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldflda = new OpCode(
                    0xff << 0 | 0x7c << 8 | (byte)Code.Ldflda << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineField << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Stfld = new OpCode(
                    0xff << 0 | 0x7d << 8 | (byte)Code.Stfld << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineField << 8 | (byte)StackBehaviour.Popref_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldsfld = new OpCode(
                    0xff << 0 | 0x7e << 8 | (byte)Code.Ldsfld << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineField << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldsflda = new OpCode(
                    0xff << 0 | 0x7f << 8 | (byte)Code.Ldsflda << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineField << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Stsfld = new OpCode(
                    0xff << 0 | 0x80 << 8 | (byte)Code.Stsfld << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineField << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stobj = new OpCode(
                    0xff << 0 | 0x81 << 8 | (byte)Code.Stobj << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popi_pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Conv_Ovf_I1_Un = new OpCode(
                    0xff << 0 | 0x82 << 8 | (byte)Code.Conv_Ovf_I1_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_I2_Un = new OpCode(
                    0xff << 0 | 0x83 << 8 | (byte)Code.Conv_Ovf_I2_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_I4_Un = new OpCode(
                    0xff << 0 | 0x84 << 8 | (byte)Code.Conv_Ovf_I4_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_I8_Un = new OpCode(
                    0xff << 0 | 0x85 << 8 | (byte)Code.Conv_Ovf_I8_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Conv_Ovf_U1_Un = new OpCode(
                    0xff << 0 | 0x86 << 8 | (byte)Code.Conv_Ovf_U1_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_U2_Un = new OpCode(
                    0xff << 0 | 0x87 << 8 | (byte)Code.Conv_Ovf_U2_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_U4_Un = new OpCode(
                    0xff << 0 | 0x88 << 8 | (byte)Code.Conv_Ovf_U4_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_U8_Un = new OpCode(
                    0xff << 0 | 0x89 << 8 | (byte)Code.Conv_Ovf_U8_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Conv_Ovf_I_Un = new OpCode(
                    0xff << 0 | 0x8a << 8 | (byte)Code.Conv_Ovf_I_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_U_Un = new OpCode(
                    0xff << 0 | 0x8b << 8 | (byte)Code.Conv_Ovf_U_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Box = new OpCode(
                    0xff << 0 | 0x8c << 8 | (byte)Code.Box << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushref << 24);

                public static readonly OpCode Newarr = new OpCode(
                    0xff << 0 | 0x8d << 8 | (byte)Code.Newarr << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushref << 24);

                public static readonly OpCode Ldlen = new OpCode(
                    0xff << 0 | 0x8e << 8 | (byte)Code.Ldlen << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelema = new OpCode(
                    0xff << 0 | 0x8f << 8 | (byte)Code.Ldelema << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelem_I1 = new OpCode(
                    0xff << 0 | 0x90 << 8 | (byte)Code.Ldelem_I1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelem_U1 = new OpCode(
                    0xff << 0 | 0x91 << 8 | (byte)Code.Ldelem_U1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelem_I2 = new OpCode(
                    0xff << 0 | 0x92 << 8 | (byte)Code.Ldelem_I2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelem_U2 = new OpCode(
                    0xff << 0 | 0x93 << 8 | (byte)Code.Ldelem_U2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelem_I4 = new OpCode(
                    0xff << 0 | 0x94 << 8 | (byte)Code.Ldelem_I4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelem_U4 = new OpCode(
                    0xff << 0 | 0x95 << 8 | (byte)Code.Ldelem_U4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelem_I8 = new OpCode(
                    0xff << 0 | 0x96 << 8 | (byte)Code.Ldelem_I8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Ldelem_I = new OpCode(
                    0xff << 0 | 0x97 << 8 | (byte)Code.Ldelem_I << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldelem_R4 = new OpCode(
                    0xff << 0 | 0x98 << 8 | (byte)Code.Ldelem_R4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushr4 << 24);

                public static readonly OpCode Ldelem_R8 = new OpCode(
                    0xff << 0 | 0x99 << 8 | (byte)Code.Ldelem_R8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushr8 << 24);

                public static readonly OpCode Ldelem_Ref = new OpCode(
                    0xff << 0 | 0x9a << 8 | (byte)Code.Ldelem_Ref << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Pushref << 24);

                public static readonly OpCode Stelem_I = new OpCode(
                    0xff << 0 | 0x9b << 8 | (byte)Code.Stelem_I << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stelem_I1 = new OpCode(
                    0xff << 0 | 0x9c << 8 | (byte)Code.Stelem_I1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stelem_I2 = new OpCode(
                    0xff << 0 | 0x9d << 8 | (byte)Code.Stelem_I2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stelem_I4 = new OpCode(
                    0xff << 0 | 0x9e << 8 | (byte)Code.Stelem_I4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stelem_I8 = new OpCode(
                    0xff << 0 | 0x9f << 8 | (byte)Code.Stelem_I8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi_popi8 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stelem_R4 = new OpCode(
                    0xff << 0 | 0xa0 << 8 | (byte)Code.Stelem_R4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi_popr4 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stelem_R8 = new OpCode(
                    0xff << 0 | 0xa1 << 8 | (byte)Code.Stelem_R8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi_popr8 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stelem_Ref = new OpCode(
                    0xff << 0 | 0xa2 << 8 | (byte)Code.Stelem_Ref << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popref_popi_popref << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldelem_Any = new OpCode(
                    0xff << 0 | 0xa3 << 8 | (byte)Code.Ldelem_Any << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popref_popi << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Stelem_Any = new OpCode(
                    0xff << 0 | 0xa4 << 8 | (byte)Code.Stelem_Any << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popref_popi_popref << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Unbox_Any = new OpCode(
                    0xff << 0 | 0xa5 << 8 | (byte)Code.Unbox_Any << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Conv_Ovf_I1 = new OpCode(
                    0xff << 0 | 0xb3 << 8 | (byte)Code.Conv_Ovf_I1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_U1 = new OpCode(
                    0xff << 0 | 0xb4 << 8 | (byte)Code.Conv_Ovf_U1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_I2 = new OpCode(
                    0xff << 0 | 0xb5 << 8 | (byte)Code.Conv_Ovf_I2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_U2 = new OpCode(
                    0xff << 0 | 0xb6 << 8 | (byte)Code.Conv_Ovf_U2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_I4 = new OpCode(
                    0xff << 0 | 0xb7 << 8 | (byte)Code.Conv_Ovf_I4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_U4 = new OpCode(
                    0xff << 0 | 0xb8 << 8 | (byte)Code.Conv_Ovf_U4 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_I8 = new OpCode(
                    0xff << 0 | 0xb9 << 8 | (byte)Code.Conv_Ovf_I8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Conv_Ovf_U8 = new OpCode(
                    0xff << 0 | 0xba << 8 | (byte)Code.Conv_Ovf_U8 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi8 << 24);

                public static readonly OpCode Refanyval = new OpCode(
                    0xff << 0 | 0xc2 << 8 | (byte)Code.Refanyval << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ckfinite = new OpCode(
                    0xff << 0 | 0xc3 << 8 | (byte)Code.Ckfinite << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushr8 << 24);

                public static readonly OpCode Mkrefany = new OpCode(
                    0xff << 0 | 0xc6 << 8 | (byte)Code.Mkrefany << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldtoken = new OpCode(
                    0xff << 0 | 0xd0 << 8 | (byte)Code.Ldtoken << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineTok << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_U2 = new OpCode(
                    0xff << 0 | 0xd1 << 8 | (byte)Code.Conv_U2 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_U1 = new OpCode(
                    0xff << 0 | 0xd2 << 8 | (byte)Code.Conv_U1 << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_I = new OpCode(
                    0xff << 0 | 0xd3 << 8 | (byte)Code.Conv_I << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_I = new OpCode(
                    0xff << 0 | 0xd4 << 8 | (byte)Code.Conv_Ovf_I << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Conv_Ovf_U = new OpCode(
                    0xff << 0 | 0xd5 << 8 | (byte)Code.Conv_Ovf_U << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Add_Ovf = new OpCode(
                    0xff << 0 | 0xd6 << 8 | (byte)Code.Add_Ovf << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Add_Ovf_Un = new OpCode(
                    0xff << 0 | 0xd7 << 8 | (byte)Code.Add_Ovf_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Mul_Ovf = new OpCode(
                    0xff << 0 | 0xd8 << 8 | (byte)Code.Mul_Ovf << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Mul_Ovf_Un = new OpCode(
                    0xff << 0 | 0xd9 << 8 | (byte)Code.Mul_Ovf_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Sub_Ovf = new OpCode(
                    0xff << 0 | 0xda << 8 | (byte)Code.Sub_Ovf << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Sub_Ovf_Un = new OpCode(
                    0xff << 0 | 0xdb << 8 | (byte)Code.Sub_Ovf_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Endfinally = new OpCode(
                    0xff << 0 | 0xdc << 8 | (byte)Code.Endfinally << 16 | (byte)FlowControl.Return << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Leave = new OpCode(
                    0xff << 0 | 0xdd << 8 | (byte)Code.Leave << 16 | (byte)FlowControl.Branch << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineBrTarget << 8 | (byte)StackBehaviour.PopAll << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Leave_S = new OpCode(
                    0xff << 0 | 0xde << 8 | (byte)Code.Leave_S << 16 | (byte)FlowControl.Branch << 24,
                    (byte)OpCodeType.Macro << 0 | (byte)OperandType.ShortInlineBrTarget << 8 | (byte)StackBehaviour.PopAll << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Stind_I = new OpCode(
                    0xff << 0 | 0xdf << 8 | (byte)Code.Stind_I << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Conv_U = new OpCode(
                    0xff << 0 | 0xe0 << 8 | (byte)Code.Conv_U << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Arglist = new OpCode(
                    0xfe << 0 | 0x00 << 8 | (byte)Code.Arglist << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ceq = new OpCode(
                    0xfe << 0 | 0x01 << 8 | (byte)Code.Ceq << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Cgt = new OpCode(
                    0xfe << 0 | 0x02 << 8 | (byte)Code.Cgt << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Cgt_Un = new OpCode(
                    0xfe << 0 | 0x03 << 8 | (byte)Code.Cgt_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Clt = new OpCode(
                    0xfe << 0 | 0x04 << 8 | (byte)Code.Clt << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Clt_Un = new OpCode(
                    0xfe << 0 | 0x05 << 8 | (byte)Code.Clt_Un << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1_pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldftn = new OpCode(
                    0xfe << 0 | 0x06 << 8 | (byte)Code.Ldftn << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineMethod << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldvirtftn = new OpCode(
                    0xfe << 0 | 0x07 << 8 | (byte)Code.Ldvirtftn << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineMethod << 8 | (byte)StackBehaviour.Popref << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Ldarg = new OpCode(
                    0xfe << 0 | 0x09 << 8 | (byte)Code.Ldarg << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineArg << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldarga = new OpCode(
                    0xfe << 0 | 0x0a << 8 | (byte)Code.Ldarga << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineArg << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Starg = new OpCode(
                    0xfe << 0 | 0x0b << 8 | (byte)Code.Starg << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineArg << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Ldloc = new OpCode(
                    0xfe << 0 | 0x0c << 8 | (byte)Code.Ldloc << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineVar << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push1 << 24);

                public static readonly OpCode Ldloca = new OpCode(
                    0xfe << 0 | 0x0d << 8 | (byte)Code.Ldloca << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineVar << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Stloc = new OpCode(
                    0xfe << 0 | 0x0e << 8 | (byte)Code.Stloc << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineVar << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Localloc = new OpCode(
                    0xfe << 0 | 0x0f << 8 | (byte)Code.Localloc << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Endfilter = new OpCode(
                    0xfe << 0 | 0x11 << 8 | (byte)Code.Endfilter << 16 | (byte)FlowControl.Return << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Unaligned = new OpCode(
                    0xfe << 0 | 0x12 << 8 | (byte)Code.Unaligned << 16 | (byte)FlowControl.Meta << 24,
                    (byte)OpCodeType.Prefix << 0 | (byte)OperandType.ShortInlineI << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Volatile = new OpCode(
                    0xfe << 0 | 0x13 << 8 | (byte)Code.Volatile << 16 | (byte)FlowControl.Meta << 24,
                    (byte)OpCodeType.Prefix << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Tail = new OpCode(
                    0xfe << 0 | 0x14 << 8 | (byte)Code.Tail << 16 | (byte)FlowControl.Meta << 24,
                    (byte)OpCodeType.Prefix << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Initobj = new OpCode(
                    0xfe << 0 | 0x15 << 8 | (byte)Code.Initobj << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Constrained = new OpCode(
                    0xfe << 0 | 0x16 << 8 | (byte)Code.Constrained << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Prefix << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Cpblk = new OpCode(
                    0xfe << 0 | 0x17 << 8 | (byte)Code.Cpblk << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Initblk = new OpCode(
                    0xfe << 0 | 0x18 << 8 | (byte)Code.Initblk << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Popi_popi_popi << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode No = new OpCode(
                    0xfe << 0 | 0x19 << 8 | (byte)Code.No << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Prefix << 0 | (byte)OperandType.ShortInlineI << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Rethrow = new OpCode(
                    0xfe << 0 | 0x1a << 8 | (byte)Code.Rethrow << 16 | (byte)FlowControl.Throw << 24,
                    (byte)OpCodeType.Objmodel << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);

                public static readonly OpCode Sizeof = new OpCode(
                    0xfe << 0 | 0x1c << 8 | (byte)Code.Sizeof << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineType << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Refanytype = new OpCode(
                    0xfe << 0 | 0x1d << 8 | (byte)Code.Refanytype << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Primitive << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop1 << 16 | (byte)StackBehaviour.Pushi << 24);

                public static readonly OpCode Readonly = new OpCode(
                    0xfe << 0 | 0x1e << 8 | (byte)Code.Readonly << 16 | (byte)FlowControl.Next << 24,
                    (byte)OpCodeType.Prefix << 0 | (byte)OperandType.InlineNone << 8 | (byte)StackBehaviour.Pop0 << 16 | (byte)StackBehaviour.Push0 << 24);
            }


            public sealed class SequencePoint
            {

                Document document;

                int start_line;
                int start_column;
                int end_line;
                int end_column;

                public int StartLine
                {
                    get { return start_line; }
                    set { start_line = value; }
                }

                public int StartColumn
                {
                    get { return start_column; }
                    set { start_column = value; }
                }

                public int EndLine
                {
                    get { return end_line; }
                    set { end_line = value; }
                }

                public int EndColumn
                {
                    get { return end_column; }
                    set { end_column = value; }
                }

                public Document Document
                {
                    get { return document; }
                    set { document = value; }
                }

                public SequencePoint(Document document)
                {
                    this.document = document;
                }
            }


            [StructLayout(LayoutKind.Sequential)]
            public struct ImageDebugDirectory
            {
                public int Characteristics;
                public int TimeDateStamp;
                public short MajorVersion;
                public short MinorVersion;
                public int Type;
                public int SizeOfData;
                public int AddressOfRawData;
                public int PointerToRawData;
            }

            public sealed class Scope : IVariableDefinitionProvider
            {

                Instruction start;
                Instruction end;

                Collection<Scope> scopes;
                Collection<VariableDefinition> variables;

                public Instruction Start
                {
                    get { return start; }
                    set { start = value; }
                }

                public Instruction End
                {
                    get { return end; }
                    set { end = value; }
                }

                public bool HasScopes
                {
                    get { return !scopes.IsNullOrEmpty(); }
                }

                public Collection<Scope> Scopes
                {
                    get
                    {
                        if (scopes == null)
                            scopes = new Collection<Scope>();

                        return scopes;
                    }
                }

                public bool HasVariables
                {
                    get { return !variables.IsNullOrEmpty(); }
                }

                public Collection<VariableDefinition> Variables
                {
                    get
                    {
                        if (variables == null)
                            variables = new Collection<VariableDefinition>();

                        return variables;
                    }
                }
            }

            public struct InstructionSymbol
            {

                public readonly int Offset;
                public readonly SequencePoint SequencePoint;

                public InstructionSymbol(int offset, SequencePoint sequencePoint)
                {
                    this.Offset = offset;
                    this.SequencePoint = sequencePoint;
                }
            }

            public sealed class MethodSymbols
            {

                internal int code_size;
                internal string method_name;
                internal MetadataToken method_token;
                internal MetadataToken local_var_token;
                internal Collection<VariableDefinition> variables;
                internal Collection<InstructionSymbol> instructions;

                public bool HasVariables
                {
                    get { return !variables.IsNullOrEmpty(); }
                }

                public Collection<VariableDefinition> Variables
                {
                    get
                    {
                        if (variables == null)
                            variables = new Collection<VariableDefinition>();

                        return variables;
                    }
                }

                public Collection<InstructionSymbol> Instructions
                {
                    get
                    {
                        if (instructions == null)
                            instructions = new Collection<InstructionSymbol>();

                        return instructions;
                    }
                }

                public int CodeSize
                {
                    get { return code_size; }
                }

                public string MethodName
                {
                    get { return method_name; }
                }

                public MetadataToken MethodToken
                {
                    get { return method_token; }
                }

                public MetadataToken LocalVarToken
                {
                    get { return local_var_token; }
                }

                public MethodSymbols(string methodName)
                {
                    this.method_name = methodName;
                }
            }

            public delegate Instruction InstructionMapper(int offset);

            public interface ISymbolReader : IDisposable
            {

                bool ProcessDebugHeader(ImageDebugDirectory directory, byte[] header);
                void Read(MethodBody body, InstructionMapper mapper);
                void Read(MethodSymbols symbols);
            }

            public interface ISymbolReaderProvider
            {

                ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName);
                ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream);
            }

            static class SymbolProvider
            {

                static readonly string symbol_kind = Type.GetType("Mono.Runtime") != null ? "Mdb" : "Pdb";

                static SR.AssemblyName GetPlatformSymbolAssemblyName()
                {
                    var cecil_name = typeof(SymbolProvider).Assembly.GetName();

                    var name = new SR.AssemblyName
                    {
                        Name = "Mono.Cecil." + symbol_kind,
                        Version = cecil_name.Version,
                    };

                    name.SetPublicKeyToken(cecil_name.GetPublicKeyToken());

                    return name;
                }

                static Type GetPlatformType(string fullname)
                {
                    var type = Type.GetType(fullname);
                    if (type != null)
                        return type;

                    var assembly_name = GetPlatformSymbolAssemblyName();

                    type = Type.GetType(fullname + ", " + assembly_name.FullName);
                    if (type != null)
                        return type;

                    try
                    {
                        var assembly = SR.Assembly.Load(assembly_name);
                        if (assembly != null)
                            return assembly.GetType(fullname);
                    }
                    catch (FileNotFoundException)
                    {

                    }
                    catch (FileLoadException)
                    {

                    }

                    return null;
                }

                static ISymbolReaderProvider reader_provider;

                public static ISymbolReaderProvider GetPlatformReaderProvider()
                {
                    if (reader_provider != null)
                        return reader_provider;

                    var type = GetPlatformType(GetProviderTypeName("ReaderProvider"));
                    if (type == null)
                        return null;

                    return reader_provider = (ISymbolReaderProvider)Activator.CreateInstance(type);
                }

                static string GetProviderTypeName(string name)
                {
                    return "Mono.Cecil." + symbol_kind + "." + symbol_kind + name;
                }
            }

            public sealed class VariableDefinition : VariableReference
            {

                public bool IsPinned
                {
                    get { return variable_type.IsPinned; }
                }

                public VariableDefinition(TypeReference variableType)
                    : base(variableType)
                {
                }

                public VariableDefinition(string name, TypeReference variableType)
                    : base(name, variableType)
                {
                }

                public override VariableDefinition Resolve()
                {
                    return this;
                }
            }


            public abstract class VariableReference
            {

                string name;
                internal int index = -1;
                protected TypeReference variable_type;

                public string Name
                {
                    get { return name; }
                    set { name = value; }
                }

                public TypeReference VariableType
                {
                    get { return variable_type; }
                    set { variable_type = value; }
                }

                public int Index
                {
                    get { return index; }
                }

                internal VariableReference(TypeReference variable_type)
                    : this(string.Empty, variable_type)
                {
                }

                internal VariableReference(string name, TypeReference variable_type)
                {
                    this.name = name;
                    this.variable_type = variable_type;
                }

                public abstract VariableDefinition Resolve();

                public override string ToString()
                {
                    if (!string.IsNullOrEmpty(name))
                        return name;

                    if (index >= 0)
                        return "V_" + index;

                    return string.Empty;
                }
            }
        }
        #endregion

        #region Metadata
        namespace Metadata
        {
            sealed class BlobHeap : Heap
            {
                public BlobHeap(Section section, uint start, uint size)
                    : base(section, start, size)
                {
                }

                public byte[] Read(uint index)
                {
                    if (index == 0 || index > Size - 1)
                        return Empty<byte>.Array;

                    var data = Section.Data;

                    int position = (int)(index + Offset);
                    int length = (int)data.ReadCompressedUInt32(ref position);

                    var buffer = new byte[length];

                    Buffer.BlockCopy(data, position, buffer, 0, length);

                    return buffer;
                }
            }

            enum CodedIndex
            {
                TypeDefOrRef,
                HasConstant,
                HasCustomAttribute,
                HasFieldMarshal,
                HasDeclSecurity,
                MemberRefParent,
                HasSemantics,
                MethodDefOrRef,
                MemberForwarded,
                Implementation,
                CustomAttributeType,
                ResolutionScope,
                TypeOrMethodDef
            }

            enum ElementType : byte
            {
                None = 0x00,
                Void = 0x01,
                Boolean = 0x02,
                Char = 0x03,
                I1 = 0x04,
                U1 = 0x05,
                I2 = 0x06,
                U2 = 0x07,
                I4 = 0x08,
                U4 = 0x09,
                I8 = 0x0a,
                U8 = 0x0b,
                R4 = 0x0c,
                R8 = 0x0d,
                String = 0x0e,
                Ptr = 0x0f,
                ByRef = 0x10,
                ValueType = 0x11,
                Class = 0x12,
                Var = 0x13,
                Array = 0x14,
                GenericInst = 0x15,
                TypedByRef = 0x16,
                I = 0x18,
                U = 0x19,
                FnPtr = 0x1b,
                Object = 0x1c,
                SzArray = 0x1d,
                MVar = 0x1e,
                CModReqD = 0x1f,
                CModOpt = 0x20,
                Internal = 0x21,
                Modifier = 0x40,
                Sentinel = 0x41,
                Pinned = 0x45,
                Type = 0x50,
                Boxed = 0x51,
                Enum = 0x55
            }

            sealed class GuidHeap : Heap
            {

                public GuidHeap(Section section, uint start, uint size)
                    : base(section, start, size)
                {
                }

                public Guid Read(uint index)
                {
                    if (index == 0)
                        return new Guid();

                    const int guid_size = 16;

                    var buffer = new byte[guid_size];

                    index--;

                    Buffer.BlockCopy(Section.Data, (int)(Offset + index), buffer, 0, guid_size);

                    return new Guid(buffer);

                }
            }

            abstract class Heap
            {
                public int IndexSize;
                public readonly Section Section;
                public readonly uint Offset;
                public readonly uint Size;

                protected Heap(Section section, uint offset, uint size)
                {
                    this.Section = section;
                    this.Offset = offset;
                    this.Size = size;
                }
            }


            struct Row<T1, T2>
            {
                internal T1 Col1;
                internal T2 Col2;

                public Row(T1 col1, T2 col2)
                {
                    Col1 = col1;
                    Col2 = col2;
                }
            }

            struct Row<T1, T2, T3>
            {
                internal T1 Col1;
                internal T2 Col2;
                internal T3 Col3;

                public Row(T1 col1, T2 col2, T3 col3)
                {
                    Col1 = col1;
                    Col2 = col2;
                    Col3 = col3;
                }
            }

            struct Row<T1, T2, T3, T4>
            {
                internal T1 Col1;
                internal T2 Col2;
                internal T3 Col3;
                internal T4 Col4;

                public Row(T1 col1, T2 col2, T3 col3, T4 col4)
                {
                    Col1 = col1;
                    Col2 = col2;
                    Col3 = col3;
                    Col4 = col4;
                }
            }

            struct Row<T1, T2, T3, T4, T5>
            {
                internal T1 Col1;
                internal T2 Col2;
                internal T3 Col3;
                internal T4 Col4;
                internal T5 Col5;

                public Row(T1 col1, T2 col2, T3 col3, T4 col4, T5 col5)
                {
                    Col1 = col1;
                    Col2 = col2;
                    Col3 = col3;
                    Col4 = col4;
                    Col5 = col5;
                }
            }

            struct Row<T1, T2, T3, T4, T5, T6>
            {
                internal T1 Col1;
                internal T2 Col2;
                internal T3 Col3;
                internal T4 Col4;
                internal T5 Col5;
                internal T6 Col6;

                public Row(T1 col1, T2 col2, T3 col3, T4 col4, T5 col5, T6 col6)
                {
                    Col1 = col1;
                    Col2 = col2;
                    Col3 = col3;
                    Col4 = col4;
                    Col5 = col5;
                    Col6 = col6;
                }
            }

            struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>
            {
                internal T1 Col1;
                internal T2 Col2;
                internal T3 Col3;
                internal T4 Col4;
                internal T5 Col5;
                internal T6 Col6;
                internal T7 Col7;
                internal T8 Col8;
                internal T9 Col9;

                public Row(T1 col1, T2 col2, T3 col3, T4 col4, T5 col5, T6 col6, T7 col7, T8 col8, T9 col9)
                {
                    Col1 = col1;
                    Col2 = col2;
                    Col3 = col3;
                    Col4 = col4;
                    Col5 = col5;
                    Col6 = col6;
                    Col7 = col7;
                    Col8 = col8;
                    Col9 = col9;
                }
            }

            sealed class RowEqualityComparer : IEqualityComparer<Row<string, string>>, IEqualityComparer<Row<uint, uint>>, IEqualityComparer<Row<uint, uint, uint>>
            {
                public bool Equals(Row<string, string> x, Row<string, string> y)
                {
                    return x.Col1 == y.Col1
                        && x.Col2 == y.Col2;
                }

                public int GetHashCode(Row<string, string> obj)
                {
                    string x = obj.Col1, y = obj.Col2;
                    return (x != null ? x.GetHashCode() : 0) ^ (y != null ? y.GetHashCode() : 0);
                }

                public bool Equals(Row<uint, uint> x, Row<uint, uint> y)
                {
                    return x.Col1 == y.Col1
                        && x.Col2 == y.Col2;
                }

                public int GetHashCode(Row<uint, uint> obj)
                {
                    return (int)(obj.Col1 ^ obj.Col2);
                }

                public bool Equals(Row<uint, uint, uint> x, Row<uint, uint, uint> y)
                {
                    return x.Col1 == y.Col1
                        && x.Col2 == y.Col2
                        && x.Col3 == y.Col3;
                }

                public int GetHashCode(Row<uint, uint, uint> obj)
                {
                    return (int)(obj.Col1 ^ obj.Col2 ^ obj.Col3);
                }
            }

            class StringHeap : Heap
            {
                readonly Dictionary<uint, string> strings = new Dictionary<uint, string>();

                public StringHeap(Section section, uint start, uint size)
                    : base(section, start, size)
                {
                }

                public string Read(uint index)
                {
                    if (index == 0)
                        return string.Empty;

                    string @string;
                    if (strings.TryGetValue(index, out @string))
                        return @string;

                    if (index > Size - 1)
                        return string.Empty;

                    @string = ReadStringAt(index);
                    if (@string.Length != 0)
                        strings.Add(index, @string);

                    return @string;
                }

                protected virtual string ReadStringAt(uint index)
                {
                    int length = 0;
                    byte[] data = Section.Data;
                    int start = (int)(index + Offset);

                    for (int i = start; ; i++)
                    {
                        if (data[i] == 0)
                            break;

                        length++;
                    }

                    return Encoding.UTF8.GetString(data, start, length);
                }
            }


            enum Table : byte
            {
                Module = 0x00,
                TypeRef = 0x01,
                TypeDef = 0x02,
                FieldPtr = 0x03,
                Field = 0x04,
                MethodPtr = 0x05,
                Method = 0x06,
                ParamPtr = 0x07,
                Param = 0x08,
                InterfaceImpl = 0x09,
                MemberRef = 0x0a,
                Constant = 0x0b,
                CustomAttribute = 0x0c,
                FieldMarshal = 0x0d,
                DeclSecurity = 0x0e,
                ClassLayout = 0x0f,
                FieldLayout = 0x10,
                StandAloneSig = 0x11,
                EventMap = 0x12,
                EventPtr = 0x13,
                Event = 0x14,
                PropertyMap = 0x15,
                PropertyPtr = 0x16,
                Property = 0x17,
                MethodSemantics = 0x18,
                MethodImpl = 0x19,
                ModuleRef = 0x1a,
                TypeSpec = 0x1b,
                ImplMap = 0x1c,
                FieldRVA = 0x1d,
                Assembly = 0x20,
                AssemblyProcessor = 0x21,
                AssemblyOS = 0x22,
                AssemblyRef = 0x23,
                AssemblyRefProcessor = 0x24,
                AssemblyRefOS = 0x25,
                File = 0x26,
                ExportedType = 0x27,
                ManifestResource = 0x28,
                NestedClass = 0x29,
                GenericParam = 0x2a,
                MethodSpec = 0x2b,
                GenericParamConstraint = 0x2c,
            }

            struct TableInformation
            {
                public uint Offset;
                public uint Length;
                public uint RowSize;
            }

            sealed class TableHeap : Heap
            {
                public long Valid;
                public long Sorted;

                public static readonly Table[] TableIdentifiers = new[] {
			Table.Module,
			Table.TypeRef,
			Table.TypeDef,
			Table.FieldPtr,
			Table.Field,
			Table.MethodPtr,
			Table.Method,
			Table.ParamPtr,
			Table.Param,
			Table.InterfaceImpl,
			Table.MemberRef,
			Table.Constant,
			Table.CustomAttribute,
			Table.FieldMarshal,
			Table.DeclSecurity,
			Table.ClassLayout,
			Table.FieldLayout,
			Table.StandAloneSig,
			Table.EventMap,
			Table.EventPtr,
			Table.Event,
			Table.PropertyMap,
			Table.PropertyPtr,
			Table.Property,
			Table.MethodSemantics,
			Table.MethodImpl,
			Table.ModuleRef,
			Table.TypeSpec,
			Table.ImplMap,
			Table.FieldRVA,
			Table.Assembly,
			Table.AssemblyProcessor,
			Table.AssemblyOS,
			Table.AssemblyRef,
			Table.AssemblyRefProcessor,
			Table.AssemblyRefOS,
			Table.File,
			Table.ExportedType,
			Table.ManifestResource,
			Table.NestedClass,
			Table.GenericParam,
			Table.MethodSpec,
			Table.GenericParamConstraint,
		};

                public readonly TableInformation[] Tables = new TableInformation[45];

                public TableInformation this[Table table]
                {
                    get
                    {
                        return Tables[(int)table];
                    }
                }

                public TableHeap(Section section, uint start, uint size)
                    : base(section, start, size)
                {
                }

                public bool HasTable(Table table)
                {
                    return (Valid & (1L << (int)table)) != 0;
                }
            }

            sealed class UserStringHeap : StringHeap
            {
                public UserStringHeap(Section section, uint start, uint size)
                    : base(section, start, size)
                {
                }

                protected override string ReadStringAt(uint index)
                {
                    byte[] data = Section.Data;
                    int start = (int)(index + Offset);

                    uint length = (uint)(data.ReadCompressedUInt32(ref start) & ~1);
                    if (length < 1)
                        return string.Empty;

                    var chars = new char[length / 2];

                    for (int i = start, j = 0; i < start + length; i += 2)
                        chars[j++] = (char)(data[i] | (data[i + 1] << 8));

                    return new string(chars);
                }
            }
        }
        #endregion

        #region PE
        namespace PE
        {
            class BinaryStreamReader : BinaryReader
            {

                public BinaryStreamReader(Stream stream)
                    : base(stream)
                {
                }

                protected void Advance(int bytes)
                {
                    BaseStream.Seek(bytes, SeekOrigin.Current);
                }

                protected DataDirectory ReadDataDirectory()
                {
                    return new DataDirectory(ReadUInt32(), ReadUInt32());
                }
            }


            class ByteBuffer
            {

                internal byte[] buffer;
                internal int length;
                internal int position;

                public ByteBuffer()
                {
                    this.buffer = Empty<byte>.Array;
                }

                public ByteBuffer(int length)
                {
                    this.buffer = new byte[length];
                }

                public ByteBuffer(byte[] buffer)
                {
                    this.buffer = buffer ?? Empty<byte>.Array;
                    this.length = this.buffer.Length;
                }

                public void Reset(byte[] buffer)
                {
                    this.buffer = buffer ?? Empty<byte>.Array;
                    this.length = this.buffer.Length;
                }

                public void Advance(int length)
                {
                    position += length;
                }

                public byte ReadByte()
                {
                    return buffer[position++];
                }

                public sbyte ReadSByte()
                {
                    return (sbyte)ReadByte();
                }

                public byte[] ReadBytes(int length)
                {
                    var bytes = new byte[length];
                    Buffer.BlockCopy(buffer, position, bytes, 0, length);
                    position += length;
                    return bytes;
                }

                public ushort ReadUInt16()
                {
                    ushort value = (ushort)(buffer[position]
                        | (buffer[position + 1] << 8));
                    position += 2;
                    return value;
                }

                public short ReadInt16()
                {
                    return (short)ReadUInt16();
                }

                public uint ReadUInt32()
                {
                    uint value = (uint)(buffer[position]
                        | (buffer[position + 1] << 8)
                        | (buffer[position + 2] << 16)
                        | (buffer[position + 3] << 24));
                    position += 4;
                    return value;
                }

                public int ReadInt32()
                {
                    return (int)ReadUInt32();
                }

                public ulong ReadUInt64()
                {
                    uint low = ReadUInt32();
                    uint high = ReadUInt32();

                    return (((ulong)high) << 32) | low;
                }

                public long ReadInt64()
                {
                    return (long)ReadUInt64();
                }

                public uint ReadCompressedUInt32()
                {
                    byte first = ReadByte();
                    if ((first & 0x80) == 0)
                        return first;

                    if ((first & 0x40) == 0)
                        return ((uint)(first & ~0x80) << 8)
                            | ReadByte();

                    return ((uint)(first & ~0xc0) << 24)
                        | (uint)ReadByte() << 16
                        | (uint)ReadByte() << 8
                        | ReadByte();
                }

                public int ReadCompressedInt32()
                {
                    var value = (int)ReadCompressedUInt32();

                    return (value & 1) != 0
                        ? -(value >> 1)
                        : value >> 1;
                }

                public float ReadSingle()
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        var bytes = ReadBytes(4);
                        Array.Reverse(bytes);
                        return BitConverter.ToSingle(bytes, 0);
                    }

                    float value = BitConverter.ToSingle(buffer, position);
                    position += 4;
                    return value;
                }

                public double ReadDouble()
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        var bytes = ReadBytes(8);
                        Array.Reverse(bytes);
                        return BitConverter.ToDouble(bytes, 0);
                    }

                    double value = BitConverter.ToDouble(buffer, position);
                    position += 8;
                    return value;
                }
            }


            sealed class ByteBufferEqualityComparer : IEqualityComparer<ByteBuffer>
            {

                public bool Equals(ByteBuffer x, ByteBuffer y)
                {
                    if (x.length != y.length)
                        return false;

                    var x_buffer = x.buffer;
                    var y_buffer = y.buffer;

                    for (int i = 0; i < x.length; i++)
                        if (x_buffer[i] != y_buffer[i])
                            return false;

                    return true;
                }

                public int GetHashCode(ByteBuffer buffer)
                {
                    var hash = 0;
                    var bytes = buffer.buffer;
                    for (int i = 0; i < buffer.length; i++)
                        hash = (hash * 37) ^ bytes[i];

                    return hash;
                }
            }


            struct DataDirectory
            {

                public readonly RVA VirtualAddress;
                public readonly uint Size;

                public bool IsZero
                {
                    get { return VirtualAddress == 0 && Size == 0; }
                }

                public DataDirectory(RVA rva, uint size)
                {
                    this.VirtualAddress = rva;
                    this.Size = size;
                }
            }


            sealed class Image
            {

                public ModuleKind Kind;
                public TargetRuntime Runtime;
                public TargetArchitecture Architecture;
                public string FileName;

                public Section[] Sections;

                public Section MetadataSection;

                public uint EntryPointToken;
                public ModuleAttributes Attributes;

                public DataDirectory Debug;
                public DataDirectory Resources;

                public StringHeap StringHeap;
                public BlobHeap BlobHeap;
                public UserStringHeap UserStringHeap;
                public GuidHeap GuidHeap;
                public TableHeap TableHeap;

                readonly int[] coded_index_sizes = new int[13];

                readonly Func<Table, int> counter;

                public Image()
                {
                    counter = GetTableLength;
                }

                public bool HasTable(Table table)
                {
                    return GetTableLength(table) > 0;
                }

                public int GetTableLength(Table table)
                {
                    return (int)TableHeap[table].Length;
                }

                public int GetTableIndexSize(Table table)
                {
                    return GetTableLength(table) < 65536 ? 2 : 4;
                }

                public int GetCodedIndexSize(CodedIndex coded_index)
                {
                    var index = (int)coded_index;
                    var size = coded_index_sizes[index];
                    if (size != 0)
                        return size;

                    return coded_index_sizes[index] = coded_index.GetSize(counter);
                }

                public uint ResolveVirtualAddress(RVA rva)
                {
                    var section = GetSectionAtVirtualAddress(rva);
                    if (section == null)
                        throw new ArgumentOutOfRangeException();

                    return ResolveVirtualAddressInSection(rva, section);
                }

                public uint ResolveVirtualAddressInSection(RVA rva, Section section)
                {
                    return rva + section.PointerToRawData - section.VirtualAddress;
                }

                public Section GetSection(string name)
                {
                    var sections = this.Sections;
                    for (int i = 0; i < sections.Length; i++)
                    {
                        var section = sections[i];
                        if (section.Name == name)
                            return section;
                    }

                    return null;
                }

                public Section GetSectionAtVirtualAddress(RVA rva)
                {
                    var sections = this.Sections;
                    for (int i = 0; i < sections.Length; i++)
                    {
                        var section = sections[i];
                        if (rva >= section.VirtualAddress && rva < section.VirtualAddress + section.SizeOfRawData)
                            return section;
                    }

                    return null;
                }

                public ImageDebugDirectory GetDebugHeader(out byte[] header)
                {
                    var section = GetSectionAtVirtualAddress(Debug.VirtualAddress);
                    var buffer = new ByteBuffer(section.Data);
                    buffer.position = (int)(Debug.VirtualAddress - section.VirtualAddress);

                    var directory = new ImageDebugDirectory
                    {
                        Characteristics = buffer.ReadInt32(),
                        TimeDateStamp = buffer.ReadInt32(),
                        MajorVersion = buffer.ReadInt16(),
                        MinorVersion = buffer.ReadInt16(),
                        Type = buffer.ReadInt32(),
                        SizeOfData = buffer.ReadInt32(),
                        AddressOfRawData = buffer.ReadInt32(),
                        PointerToRawData = buffer.ReadInt32(),
                    };

                    buffer.position = (int)(directory.PointerToRawData - section.PointerToRawData);

                    header = new byte[directory.SizeOfData];
                    Buffer.BlockCopy(buffer.buffer, buffer.position, header, 0, header.Length);

                    return directory;
                }
            }



            sealed class ImageReader : BinaryStreamReader
            {

                readonly Image image;

                DataDirectory cli;
                DataDirectory metadata;

                public ImageReader(Stream stream)
                    : base(stream)
                {
                    image = new Image();

                    image.FileName = stream.GetFullyQualifiedName();
                }

                void MoveTo(DataDirectory directory)
                {
                    BaseStream.Position = image.ResolveVirtualAddress(directory.VirtualAddress);
                }

                void MoveTo(uint position)
                {
                    BaseStream.Position = position;
                }

                void ReadImage()
                {
                    if (BaseStream.Length < 128)
                        throw new BadImageFormatException();
                    if (ReadUInt16() != 0x5a4d)
                        throw new BadImageFormatException();

                    Advance(58);
                    MoveTo(ReadUInt32());
                    if (ReadUInt32() != 0x00004550)
                        throw new BadImageFormatException();
                    image.Architecture = ReadArchitecture();
                    ushort sections = ReadUInt16();
                    Advance(14);
                    ushort characteristics = ReadUInt16();

                    ushort subsystem;
                    ReadOptionalHeaders(out subsystem);
                    ReadSections(sections);
                    ReadCLIHeader();
                    ReadMetadata();

                    image.Kind = GetModuleKind(characteristics, subsystem);
                }

                TargetArchitecture ReadArchitecture()
                {
                    var machine = ReadUInt16();
                    switch (machine)
                    {
                        case 0x014c:
                            return TargetArchitecture.I386;
                        case 0x8664:
                            return TargetArchitecture.AMD64;
                        case 0x0200:
                            return TargetArchitecture.IA64;
                    }

                    throw new NotSupportedException();
                }

                static ModuleKind GetModuleKind(ushort characteristics, ushort subsystem)
                {
                    if ((characteristics & 0x2000) != 0)
                        return ModuleKind.Dll;

                    if (subsystem == 0x2 || subsystem == 0x9)
                        return ModuleKind.Windows;

                    return ModuleKind.Console;
                }

                void ReadOptionalHeaders(out ushort subsystem)
                {
                    bool pe64 = ReadUInt16() == 0x20b;
                    Advance(66);
                    subsystem = ReadUInt16();
                    Advance(pe64 ? 90 : 74);
                    image.Debug = ReadDataDirectory();
                    Advance(56);
                    cli = ReadDataDirectory();

                    if (cli.IsZero)
                        throw new BadImageFormatException();

                    Advance(8);
                }

                string ReadAlignedString(int length)
                {
                    int read = 0;
                    var buffer = new char[length];
                    while (read < length)
                    {
                        var current = ReadByte();
                        if (current == 0)
                            break;

                        buffer[read++] = (char)current;
                    }

                    Advance(-1 + ((read + 4) & ~3) - read);

                    return new string(buffer, 0, read);
                }

                string ReadZeroTerminatedString(int length)
                {
                    int read = 0;
                    var buffer = new char[length];
                    var bytes = ReadBytes(length);
                    while (read < length)
                    {
                        var current = bytes[read];
                        if (current == 0)
                            break;

                        buffer[read++] = (char)current;
                    }

                    return new string(buffer, 0, read);
                }

                void ReadSections(ushort count)
                {
                    var sections = new Section[count];

                    for (int i = 0; i < count; i++)
                    {
                        var section = new Section();
                        section.Name = ReadZeroTerminatedString(8);
                        Advance(4);
                        section.VirtualAddress = ReadUInt32();
                        section.SizeOfRawData = ReadUInt32();
                        section.PointerToRawData = ReadUInt32();
                        Advance(16);

                        sections[i] = section;

                        if (section.Name == ".reloc")
                            continue;

                        ReadSectionData(section);
                    }

                    image.Sections = sections;
                }

                void ReadSectionData(Section section)
                {
                    var position = BaseStream.Position;

                    MoveTo(section.PointerToRawData);

                    var length = (int)section.SizeOfRawData;
                    var data = new byte[length];
                    int offset = 0, read;

                    while ((read = Read(data, offset, length - offset)) > 0)
                        offset += read;

                    section.Data = data;

                    BaseStream.Position = position;
                }

                void ReadCLIHeader()
                {
                    MoveTo(cli);
                    Advance(8);
                    metadata = ReadDataDirectory();
                    image.Attributes = (ModuleAttributes)ReadUInt32();
                    image.EntryPointToken = ReadUInt32();
                    image.Resources = ReadDataDirectory();
                }

                void ReadMetadata()
                {
                    MoveTo(metadata);

                    if (ReadUInt32() != 0x424a5342)
                        throw new BadImageFormatException();
                    Advance(8);

                    var version = ReadZeroTerminatedString(ReadInt32());
                    image.Runtime = version.ParseRuntime();
                    Advance(2);

                    var streams = ReadUInt16();

                    var section = image.GetSectionAtVirtualAddress(metadata.VirtualAddress);
                    if (section == null)
                        throw new BadImageFormatException();

                    image.MetadataSection = section;

                    for (int i = 0; i < streams; i++)
                        ReadMetadataStream(section);

                    if (image.TableHeap != null)
                        ReadTableHeap();
                }

                void ReadMetadataStream(Section section)
                {
                    uint start = metadata.VirtualAddress - section.VirtualAddress + ReadUInt32();
                    uint size = ReadUInt32();

                    var name = ReadAlignedString(16);
                    switch (name)
                    {
                        case "#~":
                        case "#-":
                            image.TableHeap = new TableHeap(section, start, size);
                            break;
                        case "#Strings":
                            image.StringHeap = new StringHeap(section, start, size);
                            break;
                        case "#Blob":
                            image.BlobHeap = new BlobHeap(section, start, size);
                            break;
                        case "#GUID":
                            image.GuidHeap = new GuidHeap(section, start, size);
                            break;
                        case "#US":
                            image.UserStringHeap = new UserStringHeap(section, start, size);
                            break;
                    }
                }

                void ReadTableHeap()
                {
                    var heap = image.TableHeap;

                    uint start = heap.Section.PointerToRawData;

                    MoveTo(heap.Offset + start);
                    Advance(6);
                    var sizes = ReadByte();
                    Advance(1);
                    heap.Valid = ReadInt64();
                    heap.Sorted = ReadInt64();

                    for (int i = 0; i < TableHeap.TableIdentifiers.Length; i++)
                    {
                        var table = TableHeap.TableIdentifiers[i];
                        if (!heap.HasTable(table))
                            continue;

                        heap.Tables[(int)table].Length = ReadUInt32();
                    }

                    SetIndexSize(image.StringHeap, sizes, 0x1);
                    SetIndexSize(image.GuidHeap, sizes, 0x2);
                    SetIndexSize(image.BlobHeap, sizes, 0x4);

                    ComputeTableInformations();
                }

                static void SetIndexSize(Heap heap, uint sizes, byte flag)
                {
                    if (heap == null)
                        return;

                    heap.IndexSize = (sizes & flag) > 0 ? 4 : 2;
                }

                int GetTableIndexSize(Table table)
                {
                    return image.GetTableIndexSize(table);
                }

                int GetCodedIndexSize(CodedIndex index)
                {
                    return image.GetCodedIndexSize(index);
                }

                void ComputeTableInformations()
                {
                    uint offset = (uint)BaseStream.Position - image.MetadataSection.PointerToRawData;

                    int stridx_size = image.StringHeap.IndexSize;
                    int blobidx_size = image.BlobHeap != null ? image.BlobHeap.IndexSize : 2;

                    var heap = image.TableHeap;
                    var tables = heap.Tables;

                    for (int i = 0; i < TableHeap.TableIdentifiers.Length; i++)
                    {
                        var table = TableHeap.TableIdentifiers[i];
                        if (!heap.HasTable(table))
                            continue;

                        int size;
                        switch (table)
                        {
                            case Table.Module:
                                size = 2 + stridx_size + (image.GuidHeap.IndexSize * 3);
                                break;
                            case Table.TypeRef:
                                size = GetCodedIndexSize(CodedIndex.ResolutionScope) + (stridx_size * 2);
                                break;
                            case Table.TypeDef:
                                size = 4 + (stridx_size * 2) + GetCodedIndexSize(CodedIndex.TypeDefOrRef) + GetTableIndexSize(Table.Field) + GetTableIndexSize(Table.Method);
                                break;
                            case Table.FieldPtr:
                                size = GetTableIndexSize(Table.Field);
                                break;
                            case Table.Field:
                                size = 2 + stridx_size + blobidx_size;
                                break;
                            case Table.MethodPtr:
                                size = GetTableIndexSize(Table.Method);
                                break;
                            case Table.Method:
                                size = 8 + stridx_size + blobidx_size + GetTableIndexSize(Table.Param);
                                break;
                            case Table.ParamPtr:
                                size = GetTableIndexSize(Table.Param);
                                break;
                            case Table.Param:
                                size = 4 + stridx_size;
                                break;
                            case Table.InterfaceImpl:
                                size = GetTableIndexSize(Table.TypeDef) + GetCodedIndexSize(CodedIndex.TypeDefOrRef);
                                break;
                            case Table.MemberRef:
                                size = GetCodedIndexSize(CodedIndex.MemberRefParent) + stridx_size + blobidx_size;
                                break;
                            case Table.Constant:
                                size = 2 + GetCodedIndexSize(CodedIndex.HasConstant) + blobidx_size;
                                break;
                            case Table.CustomAttribute:
                                size = GetCodedIndexSize(CodedIndex.HasCustomAttribute) + GetCodedIndexSize(CodedIndex.CustomAttributeType) + blobidx_size;
                                break;
                            case Table.FieldMarshal:
                                size = GetCodedIndexSize(CodedIndex.HasFieldMarshal) + blobidx_size;
                                break;
                            case Table.DeclSecurity:
                                size = 2 + GetCodedIndexSize(CodedIndex.HasDeclSecurity) + blobidx_size;
                                break;
                            case Table.ClassLayout:
                                size = 6 + GetTableIndexSize(Table.TypeDef);
                                break;
                            case Table.FieldLayout:
                                size = 4 + GetTableIndexSize(Table.Field);
                                break;
                            case Table.StandAloneSig:
                                size = blobidx_size;
                                break;
                            case Table.EventMap:
                                size = GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.Event);
                                break;
                            case Table.EventPtr:
                                size = GetTableIndexSize(Table.Event);
                                break;
                            case Table.Event:
                                size = 2 + stridx_size + GetCodedIndexSize(CodedIndex.TypeDefOrRef);
                                break;
                            case Table.PropertyMap:
                                size = GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.Property);
                                break;
                            case Table.PropertyPtr:
                                size = GetTableIndexSize(Table.Property);
                                break;
                            case Table.Property:
                                size = 2 + stridx_size + blobidx_size;
                                break;
                            case Table.MethodSemantics:
                                size = 2 + GetTableIndexSize(Table.Method) + GetCodedIndexSize(CodedIndex.HasSemantics);
                                break;
                            case Table.MethodImpl:
                                size = GetTableIndexSize(Table.TypeDef) + GetCodedIndexSize(CodedIndex.MethodDefOrRef) + GetCodedIndexSize(CodedIndex.MethodDefOrRef);
                                break;
                            case Table.ModuleRef:
                                size = stridx_size;
                                break;
                            case Table.TypeSpec:
                                size = blobidx_size;
                                break;
                            case Table.ImplMap:
                                size = 2 + GetCodedIndexSize(CodedIndex.MemberForwarded) + stridx_size + GetTableIndexSize(Table.ModuleRef);
                                break;
                            case Table.FieldRVA:
                                size = 4 + GetTableIndexSize(Table.Field);
                                break;
                            case Table.Assembly:
                                size = 16 + blobidx_size + (stridx_size * 2);
                                break;
                            case Table.AssemblyProcessor:
                                size = 4;
                                break;
                            case Table.AssemblyOS:
                                size = 12;
                                break;
                            case Table.AssemblyRef:
                                size = 12 + (blobidx_size * 2) + (stridx_size * 2);
                                break;
                            case Table.AssemblyRefProcessor:
                                size = 4 + GetTableIndexSize(Table.AssemblyRef);
                                break;
                            case Table.AssemblyRefOS:
                                size = 12 + GetTableIndexSize(Table.AssemblyRef);
                                break;
                            case Table.File:
                                size = 4 + stridx_size + blobidx_size;
                                break;
                            case Table.ExportedType:
                                size = 8 + (stridx_size * 2) + GetCodedIndexSize(CodedIndex.Implementation);
                                break;
                            case Table.ManifestResource:
                                size = 8 + stridx_size + GetCodedIndexSize(CodedIndex.Implementation);
                                break;
                            case Table.NestedClass:
                                size = GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.TypeDef);
                                break;
                            case Table.GenericParam:
                                size = 4 + GetCodedIndexSize(CodedIndex.TypeOrMethodDef) + stridx_size;
                                break;
                            case Table.MethodSpec:
                                size = GetCodedIndexSize(CodedIndex.MethodDefOrRef) + blobidx_size;
                                break;
                            case Table.GenericParamConstraint:
                                size = GetTableIndexSize(Table.GenericParam) + GetCodedIndexSize(CodedIndex.TypeDefOrRef);
                                break;
                            default:
                                throw new NotSupportedException();
                        }

                        int index = (int)table;

                        tables[index].RowSize = (uint)size;
                        tables[index].Offset = offset;

                        offset += (uint)size * tables[index].Length;
                    }
                }

                public static Image ReadImageFrom(Stream stream)
                {
                    try
                    {
                        var reader = new ImageReader(stream);
                        reader.ReadImage();
                        return reader.image;
                    }
                    catch (EndOfStreamException e)
                    {
                        throw new BadImageFormatException(stream.GetFullyQualifiedName(), e);
                    }
                }
            }

            sealed class Section
            {
                public string Name;
                public RVA VirtualAddress;
                public uint VirtualSize;
                public uint SizeOfRawData;
                public uint PointerToRawData;
                public byte[] Data;
            }
        }
        #endregion

    }
    #endregion

}

#pragma warning restore 649