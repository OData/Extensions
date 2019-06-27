// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Contains extension method for IApplicationBuilder to use pipeline provided by this project.
    /// </summary>
    public static class MigrationExtension
    {
        public static IApplicationBuilder UseMigrationMiddleware(this IApplicationBuilder builder, ODataMigrationOptions options)
        {
            return builder.UseMiddleware<TranslationMiddleware>(options);
        }
    }
}
