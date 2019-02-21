//---------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataCoreSample
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Workload.Errors.WebAPIODataCoreSample.ErrorHandling;
    using Microsoft.Workload.Errors.WebAPIODataCoreSample.Models;

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
            services.AddDbContext<DogContext>(opt => opt.UseInMemoryDatabase("DogList"));
            services.AddOData();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Dog>("Dogs");

            app.UseMiddleware(typeof(SampleExceptionMiddleware));
            app.UseMvc(builder =>
            {
                builder.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                builder.MapODataServiceRoute("ODataRoute", null, modelBuilder.GetEdmModel());
            });
        }
    }
}
