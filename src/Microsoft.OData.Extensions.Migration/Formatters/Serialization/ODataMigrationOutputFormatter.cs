// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration.Formatters.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Formatter;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ODataPath = AspNet.OData.Routing.ODataPath;

    /// <summary>
    /// Output formatter that supports V3 conventions in response bodies
    /// </summary>
    public class ODataMigrationOutputFormatter : ODataOutputFormatter
    {
        private IServiceProvider customContainer;

        /// <summary>
        /// Initialize the ODataMigrationOutputFormatter and specify that it only accepts JSON UTF8/Unicode input
        /// </summary>
        /// <param name="payloadKinds">The types of payloads accepted by this output formatter.</param>
        public ODataMigrationOutputFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
            : base(payloadKinds)
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);

            // Use some of the injected services that are untouched by this extension, while leaving some out to override.
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ODataServiceDocumentSerializer, ODataServiceDocumentSerializer>(Microsoft.OData.ServiceLifetime.Scoped);
            builder.AddService<ODataEntityReferenceLinkSerializer, ODataEntityReferenceLinkSerializer > (Microsoft.OData.ServiceLifetime.Scoped);
            builder.AddService<ODataEntityReferenceLinksSerializer, ODataEntityReferenceLinksSerializer > (Microsoft.OData.ServiceLifetime.Scoped);
            builder.AddService<ODataErrorSerializer, ODataErrorSerializer>(Microsoft.OData.ServiceLifetime.Scoped);
            builder.AddService<ODataMetadataSerializer, ODataMetadataSerializer>(Microsoft.OData.ServiceLifetime.Scoped);
            builder.AddService<ODataRawValueSerializer, ODataRawValueSerializer>(Microsoft.OData.ServiceLifetime.Scoped);
            this.customContainer = builder.BuildContainer();
        }

        /// <summary>
        /// Check for the presence of OData v3 headers
        /// </summary>
        /// <param name="context">Context that contains HTTP request</param>
        /// <returns>True if specifically V3 OData request</returns>
        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            if (request.Headers.ContainsV3Headers())
            {
                return base.CanWriteResult(context);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Customized write method (derived from ODataOutputFormatter) which translates odata context
        /// and uses ODataMigration serializer provider
        /// </summary>
        /// <param name="context">OutputFormatterWriteContext</param>
        /// <param name="selectedEncoding">Encoding</param>
        /// <returns>Indication that writing is complete</returns>
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            Type type = context.ObjectType;
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            type = TypeHelper.GetTaskInnerTypeOrSelf(type);

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw new InvalidOperationException("Write to stream async must have request");
            }

            try
            {
                HttpResponse response = context.HttpContext.Response;
                Uri baseAddress = GetBaseAddressInternal(request);
                MediaTypeHeaderValue contentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());

                Func<ODataSerializerContext> getODataSerializerContext = () =>
                {
                    return new ODataSerializerContext()
                    {
                        Request = request,
                    };
                };

                
                ODataSerializerProvider serializerProvider = new ODataMigrationSerializerProvider(customContainer);

                WriteToStream(
                    type,
                    context.Object,
                    request.GetModel(),
                    baseAddress,
                    contentType,
                    request.GetUrlHelper(),
                    request,
                    request.Headers,
                    (services) => ODataMigrationMessageWrapper.Create(response.Body, response.Headers, null, services),
                    (edmType) => serializerProvider.GetEdmTypeSerializer(edmType),
                    (objectType) => serializerProvider.GetODataPayloadSerializer(objectType, request),
                    getODataSerializerContext);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        /// <summary>
        /// Read and serialize outgoing object to HTTP request stream.
        /// </summary>
        internal static void WriteToStream(
           Type type,
           object value,
           IEdmModel model,
           Uri baseAddress,
           MediaTypeHeaderValue contentType,
           IUrlHelper internalUrlHelper,
           HttpRequest internalRequest,
           IHeaderDictionary internalRequestHeaders,
           Func<IServiceProvider, ODataMigrationMessageWrapper> getODataMessageWrapper,
           Func<IEdmTypeReference, ODataSerializer> getEdmTypeSerializer,
           Func<Type, ODataSerializer> getODataPayloadSerializer,
           Func<ODataSerializerContext> getODataSerializerContext)
        {
            if (model == null)
            {
                throw new InvalidOperationException("Request must have model");
            }

            ODataSerializer serializer = GetSerializer(type, value, internalRequest, getEdmTypeSerializer, getODataPayloadSerializer);

            // special case: if the top level serializer is an ODataPrimitiveSerializer then swap it out for an ODataMigrationPrimitiveSerializer
            // This only applies to the top level because inline primitives are translated but top level primitives are not, unless we use a customized serializer.
            if (serializer is ODataPrimitiveSerializer)
            {
                serializer = new ODataMigrationPrimitiveSerializer();
            }

            ODataPath path = internalRequest.ODataFeature().Path;
            IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

            // serialize a response
            string preferHeader = GetRequestPreferHeader(internalRequestHeaders);
            string annotationFilter = null;
            if (!String.IsNullOrEmpty(preferHeader))
            {
                ODataMigrationMessageWrapper messageWrapper = getODataMessageWrapper(null);
                messageWrapper.SetHeader("Prefer", preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            ODataMigrationMessageWrapper responseMessageWrapper = getODataMessageWrapper(internalRequest.GetRequestContainer());
            IODataResponseMessage responseMessage = responseMessageWrapper;
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            ODataMessageWriterSettings writerSettings = internalRequest.GetWriterSettings();
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = ODataVersion.V4; // Todo how to specify v3?  Maybe don't because reading as v4
            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            string metadataLink = internalUrlHelper.CreateODataLink(MetadataSegment.Instance);

            if (metadataLink == null)
            {
                throw new SerializationException("Unable to determine metadata url");
            }

            writerSettings.ODataUri = new ODataUri
            {
                ServiceRoot = baseAddress,

                SelectAndExpand = internalRequest.ODataFeature()?.SelectExpandClause,
                Apply = internalRequest.ODataFeature().ApplyClause,
                Path = (path == null || IsOperationPath(path)) ? null : path.Path,
            };

            ODataMetadataLevel metadataLevel = ODataMetadataLevel.MinimalMetadata;
            if (contentType != null)
            {
                IEnumerable<KeyValuePair<string, string>> parameters =
                    contentType.Parameters.Select(val => new KeyValuePair<string, string>(val.Name.Value, val.Value.Value));
                metadataLevel = ODataMediaTypes.GetMetadataLevel(contentType.MediaType.Value, parameters);
            }

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = getODataSerializerContext();
                writeContext.NavigationSource = targetNavigationSource;
                writeContext.Model = model;
                writeContext.RootElementName = GetRootElementName(path) ?? "root";
                writeContext.SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet;
                writeContext.Path = path;
                writeContext.SelectExpandClause = internalRequest.ODataFeature()?.SelectExpandClause;
                writeContext.MetadataLevel = metadataLevel;

                // Substitute stream to swap @odata.context
                Stream substituteStream = new MemoryStream();
                Stream originalStream = messageWriter.SubstituteResponseStream(substituteStream);

                serializer.WriteObject(value, type, messageWriter, writeContext);
                StreamReader reader = new StreamReader(substituteStream);
                substituteStream.Seek(0, SeekOrigin.Begin);
                JToken responsePayload = JToken.Parse(reader.ReadToEnd());

                // If odata context is present, replace with odata metadata
                if (responsePayload["@odata.context"] != null)
                {
                    responsePayload["odata.metadata"] = responsePayload["@odata.context"].ToString().Replace("$entity", "@Element");
                    ((JObject)responsePayload).Property("@odata.context").Remove();
                }

                // Write to actual stream
                // We cannot dispose of the stream because this method does not own the stream (subsequent methods will close the streamwriter)
                StreamWriter streamWriter = new StreamWriter(originalStream);
                JsonTextWriter writer = new JsonTextWriter(streamWriter);
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(writer, responsePayload);
                writer.Flush();
                messageWriter.SubstituteResponseStream(originalStream);
            }
        }

        /// <summary>
        /// Internal method used for selecting the base address to be used with OData uris.
        /// If the consumer has provided a delegate for overriding our default implementation,
        /// we call that, otherwise we default to existing behavior below.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root; must terminate with a trailing '/'.</returns>
        private Uri GetBaseAddressInternal(HttpRequest request)
        {
            if (BaseAddressFactory != null)
            {
                return BaseAddressFactory(request);
            }
            else
            {
                return GetDefaultBaseAddress(request);
            }
        }

        private MediaTypeHeaderValue GetContentType(string contentTypeValue)
        {
            MediaTypeHeaderValue contentType = null;
            if (!string.IsNullOrEmpty(contentTypeValue))
            {
                MediaTypeHeaderValue.TryParse(contentTypeValue, out contentType);
            }

            return contentType;
        }

        // This function is used to determine whether an OData path includes operation (import) path segments.
        // We use this function to make sure the value of ODataUri.Path in ODataMessageWriterSettings is null
        // when any path segment is an operation. ODL will try to calculate the context URL if the ODataUri.Path
        // equals to null.
        private static bool IsOperationPath(ODataPath path)
        {
            if (path == null)
            {
                return false;
            }

            foreach (ODataPathSegment segment in path.Segments)
            {
                if (segment is OperationSegment ||
                    segment is OperationImportSegment)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetRootElementName(ODataPath path)
        {
            if (path != null)
            {
                ODataPathSegment lastSegment = path.Segments.LastOrDefault();
                if (lastSegment != null)
                {
                    OperationSegment actionSegment = lastSegment as OperationSegment;

                    if (actionSegment != null)
                    {
                        IEdmAction action = actionSegment.Operations.Single() as IEdmAction;
                        if (action != null)
                        {
                            return action.Name;
                        }
                    }

                    PropertySegment propertyAccessSegment = lastSegment as PropertySegment;
                    if (propertyAccessSegment != null)
                    {
                        return propertyAccessSegment.Property.Name;
                    }
                }
            }

            return null;
        }

        // Return what is preferred (not very relevant in v3)
        private static string GetRequestPreferHeader(IHeaderDictionary headers)
        {
            string PreferHeaderName = "Prefer";
            StringValues stringValues;
            bool found = headers.TryGetValue(PreferHeaderName, out stringValues);
            IEnumerable<string> values = stringValues.AsEnumerable();

            if (found)
            {
                // If there are many "Prefer" headers, pick up the first one.
                return values.FirstOrDefault();
            }

            return null;
        }

        // Analyzes incoming type, then uses Serializer Provider through functions to return appropriate serializer
        private static ODataSerializer GetSerializer(Type type, object value, HttpRequest internalRequest, Func<IEdmTypeReference, ODataSerializer> getEdmTypeSerializer, Func<Type, ODataSerializer> getODataPayloadSerializer)
        {
            ODataSerializer serializer;

            IEdmObject edmObject = value as IEdmObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new SerializationException("Edm type " + edmObject.GetType().FullName + " cannot be null");
                }

                serializer = getEdmTypeSerializer(edmType);
                if (serializer == null)
                {
                    string message = "Type " + edmType.ToTraceString() + " cannot be serialized";
                    throw new SerializationException(message);
                }
            }
            else
            {
                var applyClause = internalRequest.ODataFeature()?.ApplyClause;

                // get the most appropriate serializer given that we support inheritance.
                if (applyClause == null)
                {
                    type = value == null ? type : value.GetType();
                }

                serializer = getODataPayloadSerializer(type);
                if (serializer == null)
                {
                    string message = "Type " + type.Name + " cannot be serialized";
                    throw new SerializationException(message);
                }
            }

            return serializer;
        }
    }
}
