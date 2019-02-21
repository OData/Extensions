//---------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataSample
{
    using System.Web.Http;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.Workload.Errors.WebAPIODataSample.ErrorHandling;
    using Microsoft.Workload.Errors.WebAPIODataSample.Models;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Dog>("Dogs");
            config.MapODataServiceRoute(
                routeName: "ODataRoute",
                routePrefix: null,
                model: builder.GetEdmModel()
            );

            config.Filters.Add(new SampleExceptionFilterAttribute());
        }
    }
}
