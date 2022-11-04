//---------------------------------------------------------------------
// <copyright file="Startup.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using ODataVerificationService.Models;

namespace ODataVerificationService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddApplicationPart(this.GetType().Assembly);
            services.AddOData();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMvc(b =>
            {
                b.MapODataServiceRoute("moviesRoute", "movies", GetEdmModel());
            });
        }

        // Builds the EDM model for the OData service, including the OData action definitions.
        private static IEdmModel GetEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            var moviesEntitySet = modelBuilder.EntitySet<Movie>("Movies");

            // Now add actions.

            // CheckOut
            // URI: ~/odata/Movies(1)/ODataActionSample.Models.CheckOut
            ActionConfiguration checkOutAction = modelBuilder.EntityType<Movie>().Action("CheckOut");
            checkOutAction.ReturnsFromEntitySet<Movie>("Movies");

            // ReturnMovie
            // URI: ~/odata/Movies(1)/ODataActionSample.Models.Return
            // Binds to a single entity; no parameters.
            ActionConfiguration returnAction = modelBuilder.EntityType<Movie>().Action("Return");
            returnAction.ReturnsFromEntitySet<Movie>("Movies");

            // CheckOutMany action
            // URI: ~/odata/Movies/ODataActionSample.Models.CheckOutMany
            // Binds to a collection of entities.  This action accepts a collection of parameters.
            ActionConfiguration checkOutManyAction = modelBuilder.EntityType<Movie>().Collection.Action("CheckOutMany");
            checkOutManyAction.CollectionParameter<int>("MovieIDs");
            checkOutManyAction.ReturnsCollectionFromEntitySet<Movie>("Movies");

            // CreateMovie action
            // URI: ~/odata/CreateMovie
            // Unbound action. It is invoked from the service root.
            ActionConfiguration createMovieAction = modelBuilder.Action("CreateMovie");
            createMovieAction.Parameter<string>("Title");
            createMovieAction.ReturnsFromEntitySet<Movie>("Movies");

            modelBuilder.Namespace = typeof(Movie).Namespace;
            return modelBuilder.GetEdmModel();
        }
    }
}
