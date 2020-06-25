//---------------------------------------------------------------------
// <copyright file="BasicUsageTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Extensions.Client.Tests.Netcore.Handlers;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.OData.Extensions.Client.Tests.Netcore.ScenarioTests
{
    public class BasicUsageTest
    {
        [Fact]
        public void TestHappyCase()
        {
            ServiceCollection sc = new ServiceCollection();
            var startup = new Startup();
            var sp = startup.ConfigureServices(sc);
            var controller = sp.GetRequiredService<VerificationController>();
            controller.TestHappyCase();
        }
    }
}
