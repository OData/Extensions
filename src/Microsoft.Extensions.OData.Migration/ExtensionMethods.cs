﻿using Microsoft.OData.Edm;
using System;
using System.Globalization;

namespace Microsoft.Extensions.OData.Migration
{
    /// <summary>
    /// Contains extension methods for V3 IEdmType and System.Uri
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// (probably temporary until cleaner solution) method to concatenate a string to URI
        /// </summary>
        /// <param name="uri">base URI</param>
        /// <param name="extra">string to append</param>
        /// <returns>base URI with string appended</returns>
        public static Uri Append(this Uri uri, string extra)
        {
            return new Uri(uri.ToString() + extra);
        }

        /* 
        public static IEdmTypeReference GetReference (this IEdmType t)
        {
            if (t is EdmCollectionType) return new EdmCollectionTypeReference(((IEdmCollectionType)t));
            else if (t is EdmComplexType) return new EdmComplexTypeReference((IEdmComplexType)t, true);
            else if (t is EdmEntityReferenceType) return new EdmEntityReferenceTypeReference((EdmEntityReferenceType)t, true);
            else if (t is EdmEntityType) return new EdmEntityTypeReference((EdmEntityType)t, true);
            else if (t is EdmEnumType) return new EdmEnumTypeReference((EdmEnumType)t, true);
            else if (t is IEdmPrimitiveType) return GetPrimitiveTypeReference((IEdmPrimitiveType)t, true);
            //else if (t is EdmRowTypeReference) return new EdmRowTypeReference(); // row type equivalent in v4?
            else
            {
                throw new NotImplementedException();
            }

        }

        internal static IEdmPrimitiveTypeReference GetPrimitiveTypeReference(this IEdmPrimitiveType type, bool isNullable)
        {
            switch (type.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.Boolean:
                case EdmPrimitiveTypeKind.Byte:
                case EdmPrimitiveTypeKind.Date:
                case EdmPrimitiveTypeKind.Double:
                case EdmPrimitiveTypeKind.Guid:
                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                case EdmPrimitiveTypeKind.Int64:
                case EdmPrimitiveTypeKind.SByte:
                case EdmPrimitiveTypeKind.Single:
                case EdmPrimitiveTypeKind.Stream:
                case EdmPrimitiveTypeKind.PrimitiveType:
                    return new EdmPrimitiveTypeReference(type, isNullable);
                case EdmPrimitiveTypeKind.Binary:
                    return new EdmBinaryTypeReference(type, isNullable);
                case EdmPrimitiveTypeKind.String:
                    return new EdmStringTypeReference(type, isNullable);
                case EdmPrimitiveTypeKind.Decimal:
                    return new EdmDecimalTypeReference(type, isNullable);
                case EdmPrimitiveTypeKind.DateTimeOffset:
                case EdmPrimitiveTypeKind.Duration:
                case EdmPrimitiveTypeKind.TimeOfDay:
                    return new EdmTemporalTypeReference(type, isNullable);
                case EdmPrimitiveTypeKind.Geography:
                case EdmPrimitiveTypeKind.GeographyPoint:
                case EdmPrimitiveTypeKind.GeographyLineString:
                case EdmPrimitiveTypeKind.GeographyPolygon:
                case EdmPrimitiveTypeKind.GeographyCollection:
                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                case EdmPrimitiveTypeKind.Geometry:
                case EdmPrimitiveTypeKind.GeometryPoint:
                case EdmPrimitiveTypeKind.GeometryLineString:
                case EdmPrimitiveTypeKind.GeometryPolygon:
                case EdmPrimitiveTypeKind.GeometryCollection:
                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    return new EdmSpatialTypeReference(type, isNullable);
                default:
                    throw new NotImplementedException();
            }
        }*/


        /// <summary>
        /// Attempts multiple casts on Data.Edm.IEdmType to reach derived classes and extract full name.
        /// Implementation taken (copy-pasted and primitiveType case commented) from OData V4 ExtensionMethods.
        /// </summary>
        /// <param name="type">OData V3 type to extract name of</param>
        /// <returns>Full name of V3 type</returns>
        public static string FullTypeName(this Data.Edm.IEdmType type)
        {
            // Taken from OData v4 EdmConstants (internal)
            const string CollectionTypeFormat = "Collection" + "({0})";

            // No corresponding public class EdmCoreModelPrimitiveType in V3?
            // TODO: check with Sam Xu
            /*var primitiveType = type as Data.Edm.Library.EdmValidCoreModelPrimitiveType; // inaccessible because private.
            if (primitiveType != null)
            {
                return primitiveType.FullName;
            }*/

            var namedDefinition = type as Data.Edm.IEdmSchemaElement;
            var collectionType = type as Data.Edm.IEdmCollectionType;
            if (collectionType == null)
            {
                return namedDefinition != null ? FullName(namedDefinition) : null;
            }

            // Handle collection case.
            namedDefinition = collectionType.ElementType.Definition as Data.Edm.IEdmSchemaElement;

            return namedDefinition != null ? string.Format(CultureInfo.InvariantCulture, CollectionTypeFormat, FullName(namedDefinition)) : null;
        }

        /// <summary>
        /// Handles specific case of extracting a name from Data.Edm.IEdmSchemaElement.
        /// Implementation taken (copy-pasted and primitiveType case commented) from OData V4 ExtensionMethods.
        /// </summary>
        /// <param name="element">OData V3 IEdmSchemaElement to extract name of</param>
        /// <returns>Full name of V3 IEdmSchemaElement</returns>
        public static string FullName(this Data.Edm.IEdmSchemaElement element)
        {
            if (element.Name == null)
            {
                return string.Empty;
            }

            if (element.Namespace == null)
            {
                return element.Name;
            }

            return element.Namespace + "." + element.Name;
        }
    }
}
