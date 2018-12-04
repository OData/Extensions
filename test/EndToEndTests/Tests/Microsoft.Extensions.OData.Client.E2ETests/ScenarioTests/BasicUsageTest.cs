//---------------------------------------------------------------------
// <copyright file="BasicUsageTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ODataVerificationService;
using ODataVerificationService.Models;
using Xunit;

namespace Microsoft.Extensions.OData.Client.Tests.Netcore.ScenarioTests
{
    public class BasicUsageTest
    {
        [Fact]
        public async Task TestODataActionAsync()
        {
            var builder = Program.CreateWebHostBuilder(new string[] { });
            builder.ConfigureServices(sc => sc.AddLogging(b => ConfigureLogging(b)));
            var server = new TestServer(builder);
            var httpClient = server.CreateClient();
            var factory = ToODataClient(httpClient);

            // set the service root uri to the odata service
            var odataClient = factory.CreateClient<Container>(new Uri(httpClient.BaseAddress, "/movies"));

            var movies = (await odataClient.Movies.ExecuteAsync()).ToList();
            movies.Count.Should().Be(6, "6 movies exists initially from MoviesContext");

            var title = "Test Movie";
            var action = odataClient.CreateMovie(title);
            var movie = await action.GetValueAsync();
            movie.Title.Should().Be(title);

            movies = (await odataClient.Movies.ExecuteAsync()).ToList();
            movies.Count.Should().Be(7, "1 more movies added just now.");
        }

        private IODataClientFactory ToODataClient(HttpClient httpClient)
        {
            var sc = new ServiceCollection();
            sc.AddLogging(b => ConfigureLogging(b));
            sc.AddODataClient().AddHttpClient(httpClient);
            var factory = sc.BuildServiceProvider().GetRequiredService<IODataClientFactory>();
            return factory;
        }

        private ILoggingBuilder ConfigureLogging(ILoggingBuilder builder)
        {
            return builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug)
                .AddFilter("System", LogLevel.Debug)
                .AddFilter("Microsoft", LogLevel.Debug);
        }
    }
}
