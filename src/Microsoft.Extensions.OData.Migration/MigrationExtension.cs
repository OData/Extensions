// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNetCore.Builder;
    using System;

    /// <summary>
    /// Contains extension method for IApplicationBuilder to use translation middleware provided by this project.
    /// </summary>
    public static class MigrationExtension
    {
        /// <summary>
        /// Call this extension method to use V3 to V4 translation middleware.
        /// </summary>
        /// <param name="builder">IApplicationBuilder that will use translation middleware</param>
        /// <param name="v3Model">Required V3 model to use as a reference for translation</param>
        /// <param name="v4Model">Required V4 model to validate translated URI</param>
        /// <param name="options">Object containing V3/V4 models for use in the middleware</param>
        /// <returns></returns>
        public static IApplicationBuilder UseODataMigration(this IApplicationBuilder builder,
                                                                 Uri serviceRoot,
                                                                 Data.Edm.IEdmModel v3Model,
                                                                 Microsoft.OData.Edm.IEdmModel v4Model)
        {
            return builder.UseMiddleware<TranslationMiddleware>(serviceRoot, v3Model, v4Model);
        }
    }
}
