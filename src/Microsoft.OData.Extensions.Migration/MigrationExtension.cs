// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.OData.Extensions.Migration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Extensions.Migration.Filters;
    using Microsoft.OData.Extensions.Migration.Formatters.Deserialization;
    using Microsoft.OData.Extensions.Migration.Formatters.Serialization;
    using Microsoft.OData;

    /// <summary>
    /// Contains extension method for IApplicationBuilder to use translation middleware provided by this project.
    /// </summary>
    public static class MigrationExtension
    {
        /// <summary>
        /// Extension method to use V3 to V4 translation middleware.
        /// </summary>
        /// <param name="builder">IApplicationBuilder that will use translation middleware</param>
        /// <param name="v3Edmx">V3 edmx to send back when requested for metadata</param>
        /// <param name="v4Model">Required V4 model to validate translated URI</param>
        /// <returns>builder now using migration middleware</returns>
        public static IApplicationBuilder UseODataMigration(this IApplicationBuilder builder,
                                                                 string v3Edmx,
                                                                 Microsoft.OData.Edm.IEdmModel v4Model)
        {
            return builder
                    .UseMiddleware<ODataMigrationMiddleware>(v3Edmx, v4Model)
                    .UseRouter((new RouteBuilder(builder)).MapGet("$metadata", async (context) =>
                    {
                        await context.Response.WriteAsync(v3Edmx);
                    }).Build());
        }

        /// <summary>
        /// Extension method to use V3 to V4 translation middleware.
        /// </summary>
        /// <param name="builder">IApplicationBuilder that will use translation middleware</param>
        /// <param name="v3model">V3 model that represents edmx contract</param>
        /// <param name="v4Model">Required V4 model to validate translated URI</param>
        /// <returns>builder now using migration middleware</returns>
        public static IApplicationBuilder UseODataMigration(this IApplicationBuilder builder,
                                                                 Data.Edm.IEdmModel v3model,
                                                                 Microsoft.OData.Edm.IEdmModel v4Model)
        {
            // Convert v3 model to string to pass through metadata request.
            string v3Edmx;
            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    IEnumerable<Data.Edm.Validation.EdmError> errors = new List<Data.Edm.Validation.EdmError>();
                    Data.Edm.Csdl.EdmxWriter.TryWriteEdmx(v3model, xmlWriter, Data.Edm.Csdl.EdmxTarget.OData, out errors);
                }
                v3Edmx = stringWriter.ToString();
            }
            return builder.UseODataMigration(v3Edmx, v4Model);
        }

        /// <summary>
        /// Extension method to use filters, request body translation and response body translation
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddODataMigration (this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.AddODataMigrationFilters();
                options.AddODataMigrationInputFormatter();
                options.AddODataMigrationOutputFormatter();
            });
            return services;
        }

        /// <summary>
        /// Extension method to use exception and resource filters to handle V3 compatible requests and responses
        /// </summary>
        /// <param name="options">MvcOptions to add filters</param>
        /// <returns>MvcOptions</returns>
        public static MvcOptions AddODataMigrationFilters(this MvcOptions options)
        {
            options.Filters.Add(typeof(MigrationExceptionFilter));
            options.Filters.Add(typeof(MigrationResourceFilter));
            return options;
        }

        /// <summary>
        /// Extension method to use a V3 compatible OData V4 InputFormatter
        /// </summary>
        /// <param name="services">MvcOptions to add formatter</param>
        /// <returns>MvcOptions</returns>
        public static MvcOptions AddODataMigrationInputFormatter(this MvcOptions options)
        {
            options.InputFormatters.Insert(0, new ODataMigrationInputFormatter(
                new ODataPayloadKind[] 
                {
                    ODataPayloadKind.ResourceSet,
                    ODataPayloadKind.Resource,
                    ODataPayloadKind.Property,
                    ODataPayloadKind.EntityReferenceLink,
                    ODataPayloadKind.EntityReferenceLinks,
                    ODataPayloadKind.Collection,
                    ODataPayloadKind.ServiceDocument,
                    ODataPayloadKind.Error,
                    ODataPayloadKind.Parameter,
                    ODataPayloadKind.Delta
                }));
            return options;
        }

        /// <summary>
        /// Extension method to use a V3 compatible OData V4 OutputFormatter
        /// </summary>
        /// <param name="services">MvcOptions to add formatter</param>
        /// <returns>MvcOptions</returns>
        public static MvcOptions AddODataMigrationOutputFormatter(this MvcOptions options)
        {
            options.OutputFormatters.Insert(0, new ODataMigrationOutputFormatter(
                new ODataPayloadKind[] 
                {
                    ODataPayloadKind.ResourceSet,
                    ODataPayloadKind.Resource,
                    ODataPayloadKind.Property,
                    ODataPayloadKind.EntityReferenceLink,
                    ODataPayloadKind.EntityReferenceLinks,
                    ODataPayloadKind.Collection,
                    ODataPayloadKind.ServiceDocument,
                    ODataPayloadKind.Error,
                    ODataPayloadKind.Parameter,
                    ODataPayloadKind.Delta
                }));
            return options;
        }
    }
}
