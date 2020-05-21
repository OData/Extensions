//---------------------------------------------------------------------
// <copyright file="TestsServiceBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;

namespace Microsoft.OData.Extensions.Migration.Tests.Mock
{
    public class TestsServiceBase : IDisposable
    {
        private bool disposedValue = false;
        private IWebHost host;

        public TestsServiceBase(string baseAddress)
        {
            BaseAddress = baseAddress;

            host = WebHost.CreateDefaultBuilder()
                    .UseUrls("http://localhost.:8000")
                    .UseStartup<Startup>()
                    .UseDefaultServiceProvider(options =>
                    {
                        options.ValidateScopes = false;
                    })
                    .Build();
            host.Start();
        }

        public string BaseAddress { get; set; }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (host != null)
                    {
                        host.StopAsync();
                        host.WaitForShutdown();
                        host.Dispose();
                        host = null;
                    }
                }

                disposedValue = true;
            }
        }
    }

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
            services.AddOData();
            services.AddODataMigration(); // Allows this service to accept v3 request bodies and return v3 response bodies

            services.AddMvc()
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routeBuilder =>
            {
                //routeBuilder.MapODataServiceRoute("ODataRoutes", "odata", v4model);
                routeBuilder.MapODataServiceRoute("TestODataRoutes", "test", MockEdmModel.GetEdmModel());
                routeBuilder.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
