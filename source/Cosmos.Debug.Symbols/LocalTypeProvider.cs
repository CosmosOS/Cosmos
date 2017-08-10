using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Cosmos.Debug.Symbols.Metadata;

namespace Cosmos.Debug.Symbols
{
    public class LocalTypeProvider : ISignatureTypeProvider<Type, LocalTypeGenericContext>
    {
        private Module mModule;

        public LocalTypeProvider(Module aModule)
        {
            mModule = aModule;
        }

        public Type GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            switch (typeCode)
            {
                case PrimitiveTypeCode.Void:
                    return typeof(void);
                case PrimitiveTypeCode.Boolean:
                    return typeof(bool);
                case PrimitiveTypeCode.Char:
                    return typeof(char);
                case PrimitiveTypeCode.SByte:
                    return typeof(sbyte);
                case PrimitiveTypeCode.Byte:
                    return typeof(byte);
                case PrimitiveTypeCode.Int16:
                    return typeof(short);
                case PrimitiveTypeCode.UInt16:
                    return typeof(ushort);
                case PrimitiveTypeCode.Int32:
                    return typeof(int);
                case PrimitiveTypeCode.UInt32:
                    return typeof(uint);
                case PrimitiveTypeCode.Int64:
                    return typeof(long);
                case PrimitiveTypeCode.UInt64:
                    return typeof(ulong);
                case PrimitiveTypeCode.Single:
                    return typeof(float);
                case PrimitiveTypeCode.Double:
                    return typeof(double);
                case PrimitiveTypeCode.String:
                    return typeof(string);
                case PrimitiveTypeCode.TypedReference:
                    return typeof(Type);
                case PrimitiveTypeCode.IntPtr:
                    return typeof(IntPtr);
                case PrimitiveTypeCode.UIntPtr:
                    return typeof(UIntPtr);
                case PrimitiveTypeCode.Object:
                    return typeof(object);
                default:
                    return null;
            }
        }

        public Type GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            return MetadataHelper.GetTypeFromReference(reader, mModule, handle, rawTypeKind);
        }

        public Type GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            TypeDefinition xDefinition = reader.GetTypeDefinition(handle);

            string xName = xDefinition.Namespace.IsNil
                               ? reader.GetString(xDefinition.Name)
                               : reader.GetString(xDefinition.Namespace) + "." + reader.GetString(xDefinition.Name);

            if (xDefinition.Attributes.HasFlag(TypeAttributes.NestedPublic | TypeAttributes.NestedPrivate))
            {
                TypeDefinitionHandle xDeclaringTypeHandle;
                TypeDefinition xDeclaringTypeDefinition;

                do
                {
                    xDeclaringTypeHandle = xDefinition.GetDeclaringType();
                    xDeclaringTypeDefinition = reader.GetTypeDefinition(xDeclaringTypeHandle);

                    string xDeclaringTypeName = xDeclaringTypeDefinition.Namespace.IsNil
                                                ? reader.GetString(xDeclaringTypeDefinition.Name)
                                                : reader.GetString(xDeclaringTypeDefinition.Namespace) + "." + reader.GetString(xDeclaringTypeDefinition.Name);

                    xName = xDeclaringTypeName + "+" + xName;
                } while (xDeclaringTypeDefinition.Attributes.HasFlag(TypeAttributes.NestedPublic | TypeAttributes.NestedPrivate));
            }

            var xType = Type.GetType(xName);
            if (xType != null)
            {
                return xType;
            }

            try
            {
                xType = mModule.ResolveType(MetadataTokens.GetToken(handle), null, null);
                return xType;
            }
            catch
            {
                return null;
            }
        }

        public Type GetSZArrayType(Type elementType)
        {
            return elementType.MakeArrayType();
        }

        public Type GetGenericInstantiation(Type genericType, ImmutableArray<Type> typeArguments)
        {
            return genericType.MakeGenericType(typeArguments.ToArray());
        }

        public Type GetArrayType(Type elementType, ArrayShape shape)
        {
            return elementType.MakeArrayType();
        }

        public Type GetByReferenceType(Type elementType)
        {
            return elementType.MakeByRefType();
        }

        public Type GetPointerType(Type elementType)
        {
            return elementType.MakePointerType();
        }

        public Type GetFunctionPointerType(MethodSignature<Type> signature)
        {
            throw new NotImplementedException();
        }

        public Type GetPinnedType(Type elementType)
        {
            return elementType;
        }

        public Type GetTypeFromSpecification(MetadataReader reader, LocalTypeGenericContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        }

        public Type GetModifiedType(Type modifier, Type unmodifiedType, bool isRequired)
        {
            throw new NotImplementedException();
        }

        public Type GetGenericTypeParameter(LocalTypeGenericContext genericContext, int index)
        {
            if (index > -1 && genericContext.TypeParameters.Length > index)
            {
                return genericContext.TypeParameters[index];
            }
            return null;
        }

        public Type GetGenericMethodParameter(LocalTypeGenericContext genericContext, int index)
        {
            if (index > -1 && genericContext.MethodParameters.Length > index)
            {
                return genericContext.MethodParameters[index];
            }
            return null;
        }
    }

}
