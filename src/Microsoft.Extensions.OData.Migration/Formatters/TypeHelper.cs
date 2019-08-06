// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    internal static class TypeHelper
    {
        /// <summary>
        /// Return the memberInfo from a type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The memberInfo from a type.</returns>
        public static MemberInfo AsMemberInfo(Type clrType)
        {
            return clrType as MemberInfo;
        }

        /// <summary>
        /// Return the type from a MemberInfo.
        /// </summary>
        /// <param name="memberInfo">The MemberInfo to convert.</param>
        /// <returns>The type from a MemberInfo.</returns>
        public static Type AsType(MemberInfo memberInfo)
        {
            return memberInfo as Type;
        }

        /// <summary>
        /// Return the assembly from a type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The assembly from a type.</returns>
        public static Assembly GetAssembly(Type clrType)
        {
            return clrType.Assembly;
        }

        /// <summary>
        /// Return the base type from a type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The base type from a type.</returns>
        public static Type GetBaseType(Type clrType)
        {
            return clrType.BaseType;
        }

        /// <summary>
        /// Return the qualified name from a member info.
        /// </summary>
        /// <param name="memberInfo">The member info to convert.</param>
        /// <returns>The qualified name from a member info.</returns>
        public static string GetQualifiedName(MemberInfo memberInfo)
        {
            Contract.Assert(memberInfo != null);
            Type type = memberInfo as Type;
            return type != null ? (type.Namespace + "." + type.Name) : memberInfo.Name;
        }

        /// <summary>
        /// Return the reflected type from a member info.
        /// </summary>
        /// <param name="memberInfo">The member info to convert.</param>
        /// <returns>The reflected type from a member info.</returns>
        public static Type GetReflectedType(MemberInfo memberInfo)
        {
            return memberInfo.ReflectedType;
        }

        /// <summary>
        /// Determine if a type is abstract.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is abstract; false otherwise.</returns>
        public static bool IsAbstract(Type clrType)
        {
            return clrType.IsAbstract;
        }

        /// <summary>
        /// Determine if a type is a class.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a class; false otherwise.</returns>
        public static bool IsClass(Type clrType)
        {
            return clrType.IsClass;
        }

        /// <summary>
        /// Determine if a type is a generic type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a generic type; false otherwise.</returns>
        public static bool IsGenericType(this Type clrType)
        {
            return clrType.IsGenericType;
        }

        /// <summary>
        /// Determine if a type is a generic type definition.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a generic type definition; false otherwise.</returns>
        public static bool IsGenericTypeDefinition(this Type clrType)
        {
            return clrType.IsGenericTypeDefinition;
        }

        /// <summary>
        /// Determine if a type is an interface.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an interface; false otherwise.</returns>
        public static bool IsInterface(Type clrType)
        {
            return clrType.IsInterface;
        }

        /// <summary>
        /// Determine if a type is null-able.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is null-able; false otherwise.</returns>
        public static bool IsNullable(Type clrType)
        {
            if (TypeHelper.IsValueType(clrType))
            {
                // value types are only nullable if they are Nullable<T>
                return TypeHelper.IsGenericType(clrType) && clrType.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            else
            {
                // reference types are always nullable
                return true;
            }
        }

        /// <summary>
        /// Determine if a type is public.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is public; false otherwise.</returns>
        public static bool IsPublic(Type clrType)
        {
            return clrType.IsPublic;
        }

        /// <summary>
        /// Determine if a type is a primitive.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a primitive; false otherwise.</returns>
        public static bool IsPrimitive(Type clrType)
        {
            return clrType.IsPrimitive;
        }

        /// <summary>
        /// Determine if a type is assignable from another type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <param name="fromType">The type to assign from.</param>
        /// <returns>True if the type is assignable; false otherwise.</returns>
        public static bool IsTypeAssignableFrom(Type clrType, Type fromType)
        {
            return clrType.IsAssignableFrom(fromType);
        }

        // <summary>
        /// Determine if a type is a value type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a value type; false otherwise.</returns>
        public static bool IsValueType(Type clrType)
        {
            return clrType.IsValueType;
        }

        /// <summary>
        /// Determine if a type is visible.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is visible; false otherwise.</returns>
        public static bool IsVisible(Type clrType)
        {
            return clrType.IsVisible;
        }

        /// <summary>
        /// Return the type from a nullable type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The type from a nullable type.</returns>
        public static Type ToNullable(Type clrType)
        {
            if (TypeHelper.IsNullable(clrType))
            {
                return clrType;
            }
            else
            {
                return typeof(Nullable<>).MakeGenericType(clrType);
            }
        }

        // <summary>
        /// Return the collection element type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The collection element type from a type.</returns>
        public static Type GetInnerElementType(Type clrType)
        {
            Type elementType;
            TypeHelper.IsCollection(clrType, out elementType);
            Contract.Assert(elementType != null);

            return elementType;
        }

        internal static Type GetTaskInnerTypeOrSelf(Type type)
        {
            if (IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return type.GetGenericArguments().First();
            }

            return type;
        }


        /// <summary>
        /// Determine if a type is a collection.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsCollection(Type clrType)
        {
            Type elementType;
            return TypeHelper.IsCollection(clrType, out elementType);
        }

        /// <summary>
        /// Determine if a type is a collection.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <param name="elementType">out: the element type of the collection.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsCollection(Type clrType, out Type elementType)
        {
            if (clrType == null)
            {
                throw new ArgumentNullException(nameof(clrType));
            }

            elementType = clrType;

            // see if this type should be ignored.
            if (clrType == typeof(string))
            {
                return false;
            }

            Type collectionInterface
                = clrType.GetInterfaces()
                    .Union(new[] { clrType })
                    .FirstOrDefault(
                        t => TypeHelper.IsGenericType(t)
                             && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();
                return true;
            }

            return false;
        }

        public static Type GetUnderlyingTypeOrSelf(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// Determine if a type is an enumeration.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsEnum(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return underlyingTypeOrSelf.IsEnum;
        }

        /// <summary>
        /// Determine if a type is a DateTime.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a DateTime; false otherwise.</returns>
        public static bool IsDateTime(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return Type.GetTypeCode(underlyingTypeOrSelf) == TypeCode.DateTime;
        }

        /// <summary>
        /// Determine if a type is a TimeSpan.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a TimeSpan; false otherwise.</returns>
        public static bool IsTimeSpan(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return underlyingTypeOrSelf == typeof(TimeSpan);
        }
    }
}
