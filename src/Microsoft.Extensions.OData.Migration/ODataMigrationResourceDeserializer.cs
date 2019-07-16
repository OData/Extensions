// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    public class ODataMigrationResourceDeserializer : ODataResourceDeserializer
    {
        public ODataMigrationResourceDeserializer(ODataDeserializerProvider provider)
            : base(provider)
        {
        }


        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (readContext.Request.Headers.ContainsKey("DataServiceVersion") || readContext.Request.Headers.ContainsKey("MaxDataServiceVersion"))
            {
                return ReadAsV3(messageReader, type, readContext);
            }
            else
            {
                return base.Read(messageReader, type, readContext);
            }
        }
        
        private object ReadAsV3(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw new ArgumentNullException("messageReader");
            }

            if (readContext == null)
            {
                throw new ArgumentNullException("readContext");
            }

            IEdmTypeReference edmType = GetEdmType(readContext, type);

            if (!edmType.IsStructured())
            {
                throw new ArgumentException("type");
            }

            IEdmStructuredTypeReference structuredType = edmType.AsStructured();

            // Locate all Long types and change to String to properly type-check V3 request bodies.
            List<string> changedPropertyNames = new List<string>();
            IEdmStructuredType definition = structuredType.StructuredDefinition();
            foreach (IEdmStructuralProperty property in definition.StructuralProperties())
            {
                if (property.Type.TypeKind() == EdmTypeKind.Primitive &&
                    ((IEdmPrimitiveType)property.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.Int64)
                {
                    IEdmTypeReference stringType = EdmCoreModel.Instance.GetString(property.Type.IsNullable);

                    // Change type to String
                    FieldInfo field = property.GetType().BaseType.GetField("type", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    field.SetValue(property, stringType);

                    // Record for changing back afterward
                    changedPropertyNames.Add(property.Name);
                }
            }

            IEdmNavigationSource navigationSource = null;
            if (structuredType.IsEntity())
            {
                if (readContext.Path == null)
                {
                    throw new ArgumentException("readContext");
                }

                navigationSource = readContext.Path.NavigationSource;
                if (navigationSource == null)
                {
                    throw new SerializationException("Navigation source missing during deserialization");
                }
            }

            ODataReader odataReader = messageReader.CreateODataResourceReader(navigationSource, structuredType.StructuredDefinition());
            ODataResourceWrapper topLevelResource = ReadResourceOrResourceSet(odataReader) as ODataResourceWrapper;

            object result = ReadInline(topLevelResource, structuredType, readContext);

            // For safety, revert changed properties to original state
            foreach (IEdmStructuralProperty property in definition.StructuralProperties())
            {
                if (changedPropertyNames.Contains(property.Name) &&
                    property.Type.TypeKind() == EdmTypeKind.Primitive &&
                    ((IEdmPrimitiveType)property.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.String)
                {
                    IEdmTypeReference longType = EdmCoreModel.Instance.GetInt64(property.Type.IsNullable);

                    // Revert field type
                    FieldInfo field = property.GetType().BaseType.GetField("type", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    field.SetValue(property, longType);
                }
            }

            return result;
        }

        private ODataItemBase ReadResourceOrResourceSet(ODataReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            ODataItemBase topLevelItem = null;
            Stack<ODataItemBase> itemsStack = new Stack<ODataItemBase>();

            while (reader.Read())
            {
                switch (reader.State)
                {
                    case ODataReaderState.ResourceStart:
                        ODataResource resource = (ODataResource)reader.Item;
                        ODataResourceWrapper resourceWrapper = null;
                        if (resource != null)
                        {
                            resourceWrapper = new ODataResourceWrapper(resource);
                        }

                        if (itemsStack.Count == 0)
                        {
                            topLevelItem = resourceWrapper;
                        }
                        else
                        {
                            ODataItemBase parentItem = itemsStack.Peek();
                            ODataResourceSetWrapper parentResourceSet = parentItem as ODataResourceSetWrapper;
                            if (parentResourceSet != null)
                            {
                                parentResourceSet.Resources.Add(resourceWrapper);
                            }
                            else
                            {
                                ODataNestedResourceInfoWrapper parentNestedResource = (ODataNestedResourceInfoWrapper)parentItem;
                                parentNestedResource.NestedItems.Add(resourceWrapper);
                            }

                        }

                        itemsStack.Push(resourceWrapper);
                        break;

                    case ODataReaderState.ResourceEnd:
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.NestedResourceInfoStart:
                        ODataNestedResourceInfo nestedResourceInfo = (ODataNestedResourceInfo)reader.Item;

                        ODataNestedResourceInfoWrapper nestedResourceInfoWrapper = new ODataNestedResourceInfoWrapper(nestedResourceInfo);
                        {
                            ODataResourceWrapper parentResource = (ODataResourceWrapper)itemsStack.Peek();
                            parentResource.NestedResourceInfos.Add(nestedResourceInfoWrapper);
                        }

                        itemsStack.Push(nestedResourceInfoWrapper);
                        break;

                    case ODataReaderState.NestedResourceInfoEnd:
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.ResourceSetStart:
                        ODataResourceSet resourceSet = (ODataResourceSet)reader.Item;

                        ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(resourceSet);
                        if (itemsStack.Count > 0)
                        {
                            ODataNestedResourceInfoWrapper parentNestedResourceInfo = (ODataNestedResourceInfoWrapper)itemsStack.Peek();
                            parentNestedResourceInfo.NestedItems.Add(resourceSetWrapper);
                        }
                        else
                        {
                            topLevelItem = resourceSetWrapper;
                        }

                        itemsStack.Push(resourceSetWrapper);
                        break;

                    case ODataReaderState.ResourceSetEnd:
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.EntityReferenceLink:
                        ODataEntityReferenceLink entityReferenceLink = (ODataEntityReferenceLink)reader.Item;
                        ODataEntityReferenceLinkBase entityReferenceLinkWrapper = new ODataEntityReferenceLinkBase(entityReferenceLink);

                        {
                            ODataNestedResourceInfoWrapper parentNavigationLink = (ODataNestedResourceInfoWrapper)itemsStack.Peek();
                            parentNavigationLink.NestedItems.Add(entityReferenceLinkWrapper);
                        }

                        break;

                    default:
                        break;
                }
            }

            return topLevelItem;
        }

        private IEdmTypeReference GetEdmType (ODataDeserializerContext context, Type type)
        {
            if (context.ResourceEdmType != null)
            {
                return context.ResourceEdmType;
            }

            return EdmExtensions.GetExpectedPayloadType(type, context.Path, context.Model);
        }
    }
}
